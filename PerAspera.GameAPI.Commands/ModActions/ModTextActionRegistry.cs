using System;
using System.Collections.Generic;
using PerAspera.Core;
using PerAspera.GameAPI.Events.SDK;

namespace PerAspera.GameAPI.Commands.ModActions
{
    /// <summary>
    /// Registry for mod-defined <see cref="IModTextAction"/> implementations.
    /// Actions registered here are automatically bridged into the <see cref="CustomCommandRegistry"/>
    /// and become executable from YAML action files.
    ///
    /// The game context (<see cref="GameCommandsReadyEvent"/>) is injected at dispatch time
    /// so actions always receive fresh Universe/Planet/Faction references.
    /// </summary>
    /// <example>
    /// <code>
    /// // Register by type (instance created automatically):
    /// ModTextActionRegistry.Register&lt;SpawnMyUnit&gt;();
    ///
    /// // Register an existing instance:
    /// ModTextActionRegistry.Register(new SpawnMyUnit());
    ///
    /// // Check registration:
    /// bool ok = ModTextActionRegistry.IsRegistered("SpawnMyUnit");
    ///
    /// // List all actions:
    /// foreach (var name in ModTextActionRegistry.RegisteredActions)
    ///     Log.LogInfo(name);
    /// </code>
    /// </example>
    public static class ModTextActionRegistry
    {
        private static readonly LogAspera _log = new LogAspera("ModTextActionRegistry");

        // Stores action instances keyed by CommandName (case-insensitive)
        private static readonly Dictionary<string, IModTextAction> _actions =
            new Dictionary<string, IModTextAction>(StringComparer.OrdinalIgnoreCase);

        // Current game context — set by UpdateContext(), called from Commands.Initialize()
        private static GameCommandsReadyEvent? _ctx;

        /// <summary>
        /// Register a mod action by type. The type must have a public parameterless constructor.
        /// </summary>
        /// <typeparam name="T">IModTextAction implementation</typeparam>
        public static void Register<T>() where T : IModTextAction, new()
        {
            Register(new T());
        }

        /// <summary>
        /// Register a mod action instance.
        /// </summary>
        /// <param name="action">The action instance to register</param>
        public static void Register(IModTextAction action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (string.IsNullOrWhiteSpace(action.CommandName))
                throw new ArgumentException($"IModTextAction.CommandName cannot be null or empty on {action.GetType().Name}");

            _actions[action.CommandName] = action;

            // Bridge into CustomCommandRegistry so YamlCommandsExecutor picks it up
            CustomCommandRegistry.Register(action.CommandName, (cmd, args) =>
            {
                try
                {
                    return action.Execute(args, _ctx);
                }
                catch (Exception ex)
                {
                    _log.Error($"❌ ModTextAction '{action.CommandName}' threw: {ex.Message}");
                    return false;
                }
            });

            _log.Info($"✅ ModTextAction registered: '{action.CommandName}' ({action.GetType().Name})");
        }

        /// <summary>
        /// Unregister a mod action by command name.
        /// </summary>
        public static void Unregister(string commandName)
        {
            if (_actions.Remove(commandName))
            {
                CustomCommandRegistry.Unregister(commandName);
                _log.Info($"Unregistered ModTextAction '{commandName}'");
            }
        }

        /// <summary>
        /// Returns true if an action is registered for this command name.
        /// </summary>
        public static bool IsRegistered(string commandName) =>
            !string.IsNullOrWhiteSpace(commandName) && _actions.ContainsKey(commandName);

        /// <summary>
        /// All currently registered mod action command names.
        /// </summary>
        public static IReadOnlyCollection<string> RegisteredActions => _actions.Keys;

        /// <summary>
        /// Update the game context injected into all actions at execution time.
        /// Called automatically by Commands.Initialize().
        /// </summary>
        internal static void UpdateContext(GameCommandsReadyEvent ctx)
        {
            _ctx = ctx;
        }
    }
}
