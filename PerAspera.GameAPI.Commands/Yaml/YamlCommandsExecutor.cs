using System;
using System.Collections.Generic;
using System.Reflection;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Commands.Constants;
using PerAspera.GameAPI.Events.SDK;

namespace PerAspera.GameAPI.Commands.Yaml
{
    /// <summary>
    /// Executes YAML-defined actions using a hybrid strategy:
    ///   1. If the command is a known native SDK command → dispatches via InteractionManager (TextAction)
    ///   2. Otherwise → falls back to ConsoleWrapper.ExecuteCommandString (all console commands)
    ///
    /// This allows every debug console command (~) to be driven from YAML.
    /// </summary>
    /// <example>
    /// <code>
    /// var actions = YamlCommandsParser.ParseFile("startup-actions.yaml");
    /// int executed = YamlCommandsExecutor.ExecuteActions(actions);
    /// Log.Info($"Executed {executed} / {actions.Count} actions");
    /// </code>
    /// </example>
    public static class YamlCommandsExecutor
    {
        private static readonly LogAspera _log = new LogAspera("YamlCommandsExecutor");

        // Cached reflected types (resolved once at first call)
        private static System.Type? _textActionType;
        private static System.Type? _consoleType;
        private static System.Type? _interactionManagerType;

        // Native game context — set via Initialize(GameCommandsReadyEvent)
        private static object? _playerFaction;   // native Faction (IHandleable)
        private static object? _gameEventBus;    // native GameEventBus

        /// <summary>
        /// Initializes the native game context from a GameCommandsReadyEvent.
        /// Must be called before native command dispatch (UnlockKnowledge, UnlockBuilding, etc.).
        /// </summary>
        /// <example>
        /// <code>
        ///     YamlCommandsExecutor.Initialize(evt);
        /// </code>
        /// </example>
        public static void Initialize(GameCommandsReadyEvent evt)
        {
            if (evt == null) { _log.Warning("Initialize: event is null"); return; }
            _playerFaction = evt.NativePlayerFaction;
            var universe = evt.NativeUniverse;
            _gameEventBus = universe?.GetType()
                .GetProperty("gameEventBus", BindingFlags.Public | BindingFlags.Instance)
                ?.GetValue(universe);
            _log.Info($"\u2705 GameContext initialized \u2014 faction={_playerFaction?.GetType().Name}, universe={universe?.GetType().Name}");
        }

        /// <summary>
        /// Execute a single YAML action definition.
        /// Uses native TextAction dispatch for known SDK commands, console fallback for all others.
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <returns>True if the action was dispatched without error</returns>
        public static bool ExecuteAction(YamlActionDefinition action)
        {
            if (action == null)
            {
                _log.Warning("ExecuteAction: action is null, skipping");
                return false;
            }

            if (string.IsNullOrWhiteSpace(action.Command))
            {
                _log.Warning("ExecuteAction: command name is empty, skipping");
                return false;
            }

            var label = string.IsNullOrEmpty(action.Label) ? action.Command : action.Label;
            _log.Info($"▶ Executing YAML action: {action}");

            // Strategy 0: custom mod-registered handler (checked first — allows mods to define any command)
            if (CustomCommandRegistry.TryExecute(action, out bool customResult))
            {
                if (customResult)
                    _log.Info($"✅ [Custom] '{action.Command}' executed");
                else
                    _log.Warning($"⚠️ [Custom] '{action.Command}' handler returned false");
                return customResult;
            }

            // Strategy 1: native SDK command via TextAction (InteractionManager)
            if (NativeCommandTypes.IsNativeCommandType(action.Command))
            {
                return ExecuteViaNativeTextAction(action);
            }

            // Strategy 2: console command fallback
            return ExecuteViaConsole(action);
        }

        /// <summary>
        /// Execute all actions in the list, in order.
        /// </summary>
        /// <param name="actions">List of actions to execute</param>
        /// <returns>Number of actions that executed without error</returns>
        public static int ExecuteActions(List<YamlActionDefinition> actions)
        {
            if (actions == null || actions.Count == 0)
            {
                _log.Debug("ExecuteActions: empty action list");
                return 0;
            }

            int successCount = 0;
            for (int i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
                try
                {
                    if (ExecuteAction(action))
                        successCount++;
                    else
                        _log.Warning($"⚠️ Action [{i}] '{action.Command}' returned false");
                }
                catch (Exception ex)
                {
                    _log.Error($"❌ Action [{i}] '{action.Command}' threw: {ex.Message}");
                }
            }

            _log.Info($"✅ ExecuteActions: {successCount}/{actions.Count} succeeded");
            return successCount;
        }

        /// <summary>
        /// Parse and execute all actions from a YAML file.
        /// </summary>
        /// <param name="filePath">Absolute path to the YAML file</param>
        /// <returns>Number of actions that executed without error, -1 if parse failed</returns>
        public static int ExecuteFile(string filePath)
        {
            var actions = YamlCommandsParser.ParseFile(filePath);
            if (actions.Count == 0)
            {
                _log.Warning($"ExecuteFile: no actions parsed from '{filePath}'");
                return -1;
            }

            _log.Info($"ExecuteFile: executing {actions.Count} action(s) from '{System.IO.Path.GetFileName(filePath)}'");
            return ExecuteActions(actions);
        }

        // ── Private helpers ──────────────────────────────────────────────────────────

        /// <summary>
        /// Dispatch via native TextAction through InteractionManager using pure reflection.
        /// This is the same path the game uses for technology.yaml actions.
        /// </summary>
        private static bool ExecuteViaNativeTextAction(YamlActionDefinition action)
        {
            try
            {
                string[] args = action.Arguments != null
                    ? action.Arguments.ToArray()
                    : Array.Empty<string>();

                // Resolve TextAction type once
                _textActionType ??= ReflectionHelpers.FindType("TextAction");
                if (_textActionType == null)
                {
                    _log.Warning($"⚠️ TextAction type not found for '{action.Command}', falling back to console");
                    return ExecuteViaConsole(action);
                }

                // Create native TextAction(string command, string[] arguments)
                object? nativeTextAction = Activator.CreateInstance(_textActionType, action.Command, args);
                if (nativeTextAction == null)
                {
                    _log.Warning($"⚠️ TextAction instantiation failed for '{action.Command}', falling back to console");
                    return ExecuteViaConsole(action);
                }

                // Set optional fields via reflection
                if (action.DaysDelay > 0f)
                    _textActionType.GetField("daysDelay")?.SetValue(nativeTextAction, action.DaysDelay);
                _textActionType.GetField("showInFrontend")?.SetValue(nativeTextAction, action.ShowInFrontend);

                // Dispatch via InteractionManager
                if (DispatchNativeTextAction(nativeTextAction))
                {
                    _log.Info($"✅ [Native] '{action.Command}' dispatched");
                    return true;
                }

                _log.Warning($"⚠️ [Native] DispatchAction failed for '{action.Command}', falling back to console");
                return ExecuteViaConsole(action);
            }
            catch (Exception ex)
            {
                _log.Warning($"⚠️ [Native] '{action.Command}' threw ({ex.Message}), falling back to console");
                return ExecuteViaConsole(action);
            }
        }

        /// <summary>
        /// Execute via the game debug console using pure reflection.
        /// Works for ALL console commands including those not in the native SDK.
        /// </summary>
        private static bool ExecuteViaConsole(YamlActionDefinition action)
        {
            try
            {
                // Resolve Console type once
                _consoleType ??= ReflectionHelpers.FindType("Console");
                if (_consoleType == null)
                {
                    _log.Error($"❌ [Console] Console type not found for '{action.Command}'");
                    return false;
                }

                // Get singleton instance
                var instanceProp = _consoleType.GetProperty(
                    "instance",
                    BindingFlags.Static | BindingFlags.Public);
                var consoleInstance = instanceProp?.GetValue(null);
                if (consoleInstance == null)
                {
                    _log.Error($"❌ [Console] Console.instance is null for '{action.Command}'");
                    return false;
                }

                // Get ExecuteCommandString method
                var execMethod = _consoleType.GetMethod(
                    "ExecuteCommandString",
                    BindingFlags.Public | BindingFlags.Instance);
                if (execMethod == null)
                {
                    _log.Error($"❌ [Console] ExecuteCommandString method not found");
                    return false;
                }

                string commandStr = BuildConsoleCommandString(action);
                _log.Debug($"[Console] Sending: {commandStr}");
                execMethod.Invoke(consoleInstance, new object[] { commandStr });
                _log.Info($"✅ [Console] '{commandStr}' executed");
                return true;
            }
            catch (Exception ex)
            {
                _log.Error($"❌ [Console] '{action.Command}' threw: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Build "CommandName arg1 arg2 ..." string for console execution.
        /// Arguments are space-separated; arguments containing spaces are quoted.
        /// </summary>
        private static string BuildConsoleCommandString(YamlActionDefinition action)
        {
            if (action.Arguments == null || action.Arguments.Count == 0)
                return action.Command;

            var parts = new System.Text.StringBuilder(action.Command);
            foreach (var arg in action.Arguments)
            {
                parts.Append(' ');
                // Quote arguments that contain spaces
                if (arg.Contains(' '))
                    parts.Append('"').Append(arg).Append('"');
                else
                    parts.Append(arg);
            }
            return parts.ToString();
        }

        /// <summary>
        /// Dispatch a native TextAction through the game's static InteractionManager.DispatchAction.
        /// Uses stored player faction + gameEventBus — set via Initialize().
        /// Selects the 4-param static overload to avoid AmbiguousMatchException.
        /// </summary>
        private static bool DispatchNativeTextAction(object nativeTextAction)
        {
            if (_playerFaction == null || _gameEventBus == null)
            {
                _log.Warning("\u26a0\ufe0f DispatchNativeTextAction: game context not initialized (call Initialize() first)");
                return false;
            }

            try
            {
                _interactionManagerType ??= ReflectionHelpers.FindType("InteractionManager");
                if (_interactionManagerType == null)
                {
                    _log.Warning("InteractionManager type not found");
                    return false;
                }

                // IL2CPP interface cast: Faction → IHandleable via native pointer.
                // Reflection.Invoke rejects passing Faction where IHandleable is expected,
                // so we reconstruct IHandleable from the Faction's Il2Cpp pointer.
                var iHandleableType = ReflectionHelpers.FindType("IHandleable");
                if (iHandleableType == null)
                {
                    _log.Warning("IHandleable type not found");
                    return false;
                }
                var factionBase = _playerFaction as Il2CppObjectBase;
                if (factionBase == null)
                {
                    _log.Warning("_playerFaction is not an Il2CppObjectBase");
                    return false;
                }
                var iHandleableInstance = Activator.CreateInstance(iHandleableType, factionBase.Pointer);

                // Use GetMethods() + param-count filter to avoid AmbiguousMatchException.
                // Target: static DispatchAction(IHandleable, GameEventBus, TextAction, string) — 4 params.
                MethodInfo? dispatchMethod = null;
                foreach (var m in _interactionManagerType.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    if (m.Name == "DispatchAction" && m.GetParameters().Length == 4)
                    {
                        dispatchMethod = m;
                        break;
                    }
                }

                if (dispatchMethod == null)
                {
                    _log.Warning("InteractionManager.DispatchAction(4-param static) not found");
                    return false;
                }

                dispatchMethod.Invoke(null, new object[] { iHandleableInstance, _gameEventBus, nativeTextAction, "mod" });
                return true;
            }
            catch (System.Reflection.TargetInvocationException)
            {
                // IL2CPP marshaling exception on return path — the dispatch already happened.
                // Unity logs "Dispatch Action ..." before this exception, confirming success.
                return true;
            }
            catch (Exception ex)
            {
                _log.Warning($"DispatchNativeTextAction failed: {ex.Message}");
                return false;
            }
        }
    }
}
