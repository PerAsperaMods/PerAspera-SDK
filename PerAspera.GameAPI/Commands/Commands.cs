using System;
using HarmonyLib;
using PerAspera.GameAPI.Commands.Builders;
using PerAspera.GameAPI.Commands.Constants;
using PerAspera.GameAPI.Commands.Core;
using PerAspera.GameAPI.Commands.ModActions;
using PerAspera.GameAPI.Commands.ModActions.BuiltinActions;
using PerAspera.GameAPI.Commands.Patches;
using PerAspera.GameAPI.Commands.Yaml;
using PerAspera.GameAPI.Events.SDK;

namespace PerAspera.GameAPI.Commands
{
    /// <summary>
    /// Main static entry point for Commands API with fluent builder pattern
    /// Provides convenient methods for creating and executing Per Aspera commands
    /// </summary>
    public static class Commands
    {
        // Static constructor: runs as soon as any plugin touches Commands (at Load() time,
        // before YAML validation). Registers built-ins and installs all Harmony patches
        // here — NOT from Initialize() — because PatchAll() fails inside DynamicInvoke contexts (IL2CPP).
        static Commands()
        {
            RegisterBuiltinActions();
            ApplyVerifyActionPatch();
            ApplyValidateConstraintsPatch();
            ApplyNativeInterceptPatch();
        }

        /// <summary>
        /// Initialize the Commands API with native game context from a GameCommandsReadyEvent.
        /// Must be called before using ExecuteFromYaml/ExecuteFromYamlFile with native commands.
        /// </summary>
        /// <example>
        /// <code>
        /// EnhancedEventBus.SubscribeToGameCommandsReady(evt => {
        ///     Commands.Initialize(evt);
        ///     Commands.ExecuteFromYamlFile(startupYaml);
        /// });
        /// </code>
        /// </example>
        public static void Initialize(GameCommandsReadyEvent evt)
        {
            YamlCommandsExecutor.Initialize(evt);
            ModTextActionRegistry.UpdateContext(evt);
            ApplyNativeInterceptPatch();
        }

        // Installs the Harmony prefix that routes custom commands from native DispatchAction
        // (idempotent — safe to call multiple times)
        private static bool _interceptPatchApplied;
        private static void ApplyNativeInterceptPatch()
        {
            if (_interceptPatchApplied) return;
            _interceptPatchApplied = true;
            var harmony = new Harmony("PerAspera.GameAPI.Commands.NativeIntercept");
            harmony.PatchAll(typeof(NativeDispatchInterceptPatch));
        }

        // Installs the Harmony prefix that bypasses load-time validation for registered custom commands
        // (idempotent — safe to call multiple times)
        private static bool _verifyPatchApplied;
        private static void ApplyVerifyActionPatch()
        {
            if (_verifyPatchApplied) return;
            _verifyPatchApplied = true;
            var harmony = new Harmony("PerAspera.GameAPI.Commands.VerifyAction");
            harmony.PatchAll(typeof(VerifyActionPatch));
        }

        // Installs the Harmony prefix on SpecialProjectType.ValidateConstraints that validates
        // custom SDK actions registered in CustomCommandRegistry (bypasses AOT-inlined VerifyAction)
        // (idempotent — safe to call multiple times)
        private static bool _validateConstraintsPatchApplied;
        private static void ApplyValidateConstraintsPatch()
        {
            if (_validateConstraintsPatchApplied) return;
            _validateConstraintsPatchApplied = true;
            var harmony = new Harmony("PerAspera.GameAPI.Commands.ValidateConstraints");
            harmony.PatchAll(typeof(ValidateConstraintsPatch));
        }

        /// <summary>
        /// Register a mod-defined TextAction by type.
        /// The type must implement <see cref="IModTextAction"/> and have a public parameterless constructor.
        /// Call this in your plugin Load() before game starts.
        /// </summary>
        /// <typeparam name="T">Your IModTextAction implementation</typeparam>
        /// <example>
        /// <code>
        /// Commands.RegisterAction&lt;SpawnMyUnit&gt;();
        /// </code>
        /// </example>
        public static void RegisterAction<T>() where T : IModTextAction, new()
        {
            ModTextActionRegistry.Register<T>();
        }

        /// <summary>
        /// Register a mod-defined TextAction instance.
        /// </summary>
        /// <param name="action">The action instance to register</param>
        public static void RegisterAction(IModTextAction action)
        {
            ModTextActionRegistry.Register(action);
        }

        // Registers SDK built-in actions (idempotent — safe to call multiple times)
        private static bool _builtinsRegistered;
        private static void RegisterBuiltinActions()
        {
            if (_builtinsRegistered) return;
            _builtinsRegistered = true;

            // Core actions
            ModTextActionRegistry.Register(new ShowMessageAction());
            ModTextActionRegistry.Register(new GiveSciencePointsAction());
            ModTextActionRegistry.Register(new ImportResourceAction());

            // Proper game API — no console required
            ModTextActionRegistry.Register(new FactionAddResourceAction());

            // Debug display commands
            ModTextActionRegistry.Register(new ShowSpaceportResourcesAction());

            // Console cheat commands exposed for YAML-only mods (no C# plugin required)
            ModTextActionRegistry.Register(new FactionAddResourceDistributedAction());
            ModTextActionRegistry.Register(new BuildingAddResourceAction());
            ModTextActionRegistry.Register(new BunchOfResourcesAction());
            ModTextActionRegistry.Register(new ClearStockpilesAction());
            ModTextActionRegistry.Register(new FinishConstructionsAction());
        }
        /// <summary>
        /// Create a new command builder for the specified command type
        /// </summary>
        /// <param name="commandType">Type of command to create (e.g., "ImportResource")</param>
        /// <returns>CommandBuilder for fluent configuration</returns>
        public static CommandBuilder Create(string commandType)
        {
            if (string.IsNullOrEmpty(commandType))
                throw new ArgumentException("Command type cannot be null or empty", nameof(commandType));
                
            return new CommandBuilder(commandType);
        }
        
        /// <summary>
        /// Create a faction-specific command builder for chaining faction commands
        /// </summary>
        /// <param name="faction">Faction to execute commands for</param>
        /// <returns>FactionCommandBuilder for faction-specific fluent API</returns>
        public static FactionCommandBuilder ForFaction(object faction)
        {
            if (faction == null)
                throw new ArgumentNullException(nameof(faction));
                
            return new FactionCommandBuilder(faction);
        }
        
        /// <summary>
        /// Create a batch command builder for executing multiple commands
        /// </summary>
        /// <returns>BatchCommandBuilder for batch execution</returns>
        public static BatchCommandBuilder CreateBatch()
        {
            return new BatchCommandBuilder();
        }
        
        // Static convenience methods for common commands
        
        /// <summary>
        /// Import resource for faction (convenience method)
        /// </summary>
        public static CommandResult ImportResource(object faction, object resource, int quantity)
        {
            return Create(NativeCommandTypes.ImportResource)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Resource, resource)
                .WithParameter(ParameterNames.Quantity, quantity)
                .Execute();
        }
        
        /// <summary>
        /// Unlock building for faction (convenience method)
        /// </summary>
        public static CommandResult UnlockBuilding(object faction, object building)
        {
            return Create(NativeCommandTypes.UnlockBuilding)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Building, building)
                .Execute();
        }
        
        /// <summary>
        /// Research technology for faction (convenience method)
        /// </summary>
        public static CommandResult ResearchTechnology(object faction, object technology)
        {
            return Create(NativeCommandTypes.ResearchTechnology)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Technology, technology)
                .Execute();
        }
        
        /// <summary>
        /// Unlock knowledge for faction (convenience method)
        /// </summary>
        public static CommandResult UnlockKnowledge(object faction, object knowledge)
        {
            return Create(NativeCommandTypes.UnlockKnowledge)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Knowledge, knowledge)
                .Execute();
        }
        
        /// <summary>
        /// Start dialogue (convenience method)
        /// </summary>
        public static CommandResult StartDialogue(object faction, object person, object dialogue)
        {
            return Create(NativeCommandTypes.StartDialogue)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Person, person)
                .WithParameter(ParameterNames.Dialogue, dialogue)
                .Execute();
        }
        
        /// <summary>
        /// Set override value (convenience method)
        /// </summary>
        public static CommandResult SetOverride(string key, object value)
        {
            return Create(NativeCommandTypes.SetOverride)
                .WithParameter(ParameterNames.Key, key)
                .WithParameter(ParameterNames.Value, value)
                .Execute();
        }
        
        /// <summary>
        /// Sabotage faction (convenience method)
        /// </summary>
        public static CommandResult Sabotage(object targetFaction)
        {
            return Create(NativeCommandTypes.Sabotage)
                .WithFaction(targetFaction)
                .Execute();
        }
        
        /// <summary>
        /// Spawn resource vein (convenience method)
        /// </summary>
        public static CommandResult SpawnResourceVein(object faction, object resource, float x, float y, float z)
        {
            return Create(NativeCommandTypes.SpawnResourceVein)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Resource, resource)
                .WithParameter(ParameterNames.X, x)
                .WithParameter(ParameterNames.Y, y)
                .WithParameter(ParameterNames.Z, z)
                .Execute();
        }
        
        /// <summary>
        /// Game over command (convenience method)
        /// </summary>
        public static CommandResult GameOver()
        {
            return Create(NativeCommandTypes.GameOver)
                .Execute();
        }
        
        /// <summary>
        /// Check if command type is supported
        /// </summary>
        public static bool IsCommandTypeSupported(string commandType)
        {
            return NativeCommandTypes.IsNativeCommandType(commandType);
        }
        
        /// <summary>
        /// Get all supported command types
        /// </summary>
        public static string[] GetSupportedCommandTypes()
        {
            return NativeCommandTypes.AllNativeTypes;
        }
        
        /// <summary>
        /// Subscribe to command execution events
        /// </summary>
        public static void OnCommandExecuted(Action<Events.CommandExecutedEvent> handler)
        {
            CommandDispatcher.Instance.SubscribeToExecutedEvents(handler);
        }
        
        /// <summary>
        /// Subscribe to command failure events
        /// </summary>
        public static void OnCommandFailed(Action<Events.CommandFailedEvent> handler)
        {
            CommandDispatcher.Instance.SubscribeToFailedEvents(handler);
        }
        
        /// <summary>
        /// Unsubscribe from command execution events
        /// </summary>
        public static void OffCommandExecuted(Action<Events.CommandExecutedEvent> handler)
        {
            CommandDispatcher.Instance.UnsubscribeFromExecutedEvents(handler);
        }
        
        /// <summary>
        /// Unsubscribe from command failure events
        /// </summary>
        public static void OffCommandFailed(Action<Events.CommandFailedEvent> handler)
        {
            CommandDispatcher.Instance.UnsubscribeFromFailedEvents(handler);
        }
        
        /// <summary>
        /// Get command execution statistics
        /// </summary>
        public static Events.CommandStatistics GetStatistics()
        {
            return CommandDispatcher.Instance.GetStatistics();
        }

        // ── YAML Actions ────────────────────────────────────────────────────────────

        /// <summary>
        /// Parse and execute all actions defined in a YAML string.
        /// Supports any console debug command (~) as well as native SDK commands.
        /// The YAML root must be an "actions:" key.
        /// </summary>
        /// <param name="yamlContent">YAML string with root "actions:" key</param>
        /// <returns>Number of actions executed successfully</returns>
        /// <example>
        /// <code>
        /// int count = Commands.ExecuteFromYaml(@"
        /// actions:
        ///   - command: FinishConstructions
        ///     arguments: []
        ///     daysDelay: 0.0
        ///     showInFrontend: false
        ///   - command: SetEngineTimescale
        ///     arguments: ['2.0']
        ///     daysDelay: 0.0
        ///     showInFrontend: false
        /// ");
        /// </code>
        /// </example>
        public static int ExecuteFromYaml(string yamlContent)
        {
            var actions = Yaml.YamlCommandsParser.ParseString(yamlContent);
            return Yaml.YamlCommandsExecutor.ExecuteActions(actions);
        }

        /// <summary>
        /// Parse and execute all actions from a YAML file on disk.
        /// Supports both "actions:" wrapped format and raw YAML list format.
        /// Works for any console debug command (~) and all native SDK commands.
        /// </summary>
        /// <param name="filePath">Absolute path to the YAML actions file</param>
        /// <returns>Number of actions executed successfully, -1 if file not found or parse failed</returns>
        /// <example>
        /// <code>
        /// // startup-actions.yaml lives in your mod folder
        /// string modPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        /// int count = Commands.ExecuteFromYamlFile(Path.Combine(modPath, "startup-actions.yaml"));
        /// </code>
        /// </example>
        public static int ExecuteFromYamlFile(string filePath)
        {
            return Yaml.YamlCommandsExecutor.ExecuteFile(filePath);
        }

        // ── Custom Command Registry ──────────────────────────────────────────────────

        /// <summary>
        /// Register a C# handler for a custom YAML command name.
        /// The handler will be invoked (with priority) whenever a YAML action with this name is executed.
        /// Case-insensitive. Replaces any previously registered handler for the same name.
        /// </summary>
        /// <param name="commandName">Command name as it appears in YAML (e.g. "ActivateSpaceport")</param>
        /// <param name="handler">Handler delegate: (commandName, arguments[]) → bool</param>
        /// <example>
        /// <code>
        /// // In your plugin Load():
        /// Commands.RegisterHandler("ActivateSpaceport", (cmd, args) =>
        /// {
        ///     string faction = args.Length > 0 ? args[0] : "player";
        ///     Log.LogInfo($"Spaceport activated for {faction}!");
        ///     return true;
        /// });
        ///
        /// // In your YAML:
        /// actions:
        ///   - command: ActivateSpaceport
        ///     arguments:
        ///       - player
        /// </code>
        /// </example>
        public static void RegisterHandler(string commandName, CustomCommandHandler handler)
            => CustomCommandRegistry.Register(commandName, handler);

        /// <summary>
        /// Unregister a previously registered custom command handler.
        /// </summary>
        /// <param name="commandName">Command name to unregister</param>
        public static void UnregisterHandler(string commandName)
            => CustomCommandRegistry.Unregister(commandName);

        /// <summary>
        /// Returns true if a custom C# handler is registered for this command name.
        /// </summary>
        public static bool IsCommandRegistered(string commandName)
            => CustomCommandRegistry.IsRegistered(commandName);
    }
}
