using System;
using System.Collections.Generic;
using PerAspera.Core;
using PerAspera.GameAPI.Commands.Yaml;

namespace PerAspera.GameAPI.Commands
{
    /// <summary>
    /// Handler delegate for custom YAML commands registered by mods.
    /// </summary>
    /// <param name="commandName">The command name from YAML (e.g. "ActivateSpaceport")</param>
    /// <param name="arguments">Arguments array from YAML (may be empty)</param>
    /// <returns>True if the command succeeded, false if it failed</returns>
    public delegate bool CustomCommandHandler(string commandName, string[] arguments);

    /// <summary>
    /// Registry for custom YAML command handlers registered by mods.
    /// Checked first by YamlCommandsExecutor before native SDK commands and console fallback.
    /// </summary>
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
    public static class CustomCommandRegistry
    {
        private static readonly LogAspera _log = new LogAspera("CustomCommandRegistry");

        private static readonly Dictionary<string, CustomCommandHandler> _handlers =
            new Dictionary<string, CustomCommandHandler>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Register a C# handler for a custom YAML command name.
        /// If a handler already exists for this name, it is replaced.
        /// </summary>
        /// <param name="commandName">Command name as it appears in YAML (case-insensitive)</param>
        /// <param name="handler">Handler to invoke when the command is executed</param>
        public static void Register(string commandName, CustomCommandHandler handler)
        {
            if (string.IsNullOrWhiteSpace(commandName))
                throw new ArgumentNullException(nameof(commandName));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _handlers[commandName] = handler;
            _log.Info($"✅ Registered custom handler for command '{commandName}'");
        }

        /// <summary>
        /// Unregister a previously registered handler.
        /// </summary>
        /// <param name="commandName">Command name to unregister</param>
        public static void Unregister(string commandName)
        {
            if (_handlers.Remove(commandName))
                _log.Info($"Unregistered custom handler for command '{commandName}'");
        }

        /// <summary>
        /// Returns true if a handler is registered for this command name.
        /// </summary>
        public static bool IsRegistered(string commandName) =>
            !string.IsNullOrWhiteSpace(commandName) && _handlers.ContainsKey(commandName);

        /// <summary>
        /// All currently registered custom command names.
        /// </summary>
        public static IReadOnlyCollection<string> RegisteredCommands => _handlers.Keys;

        /// <summary>
        /// Returns the handler registered for the given command name.
        /// Throws KeyNotFoundException if not registered — check IsRegistered() first.
        /// Used by NativeDispatchInterceptPatch to invoke handlers from the native dispatch path.
        /// </summary>
        internal static CustomCommandHandler GetHandler(string commandName) => _handlers[commandName];

        /// <summary>
        /// Try to execute a registered handler for the given action.
        /// Returns true if a handler was found (regardless of its return value).
        /// </summary>
        /// <param name="action">The YAML action to execute</param>
        /// <param name="handlerResult">Result returned by the handler, or false if handler threw</param>
        internal static bool TryExecute(YamlActionDefinition action, out bool handlerResult)
        {
            handlerResult = false;

            if (action == null || !_handlers.TryGetValue(action.Command, out var handler))
                return false;

            try
            {
                var args = action.Arguments != null
                    ? action.Arguments.ToArray()
                    : Array.Empty<string>();

                handlerResult = handler(action.Command, args);
                return true;
            }
            catch (Exception ex)
            {
                _log.Error($"❌ Custom handler for '{action.Command}' threw: {ex.Message}");
                handlerResult = false;
                return true; // handler was found but failed — do not fall through to console
            }
        }
    }
}
