using PerAspera.Core;
using PerAspera.GameAPI.Events.SDK;

namespace PerAspera.GameAPI.Commands.ModActions.BuiltinActions
{
    /// <summary>
    /// Built-in SDK action: logs a message to the BepInEx console.
    /// Registered automatically by Commands.Initialize().
    ///
    /// YAML usage:
    /// <code>
    /// actions:
    ///   - command: ShowMessage
    ///     arguments:
    ///       - "Hello from my mod!"
    /// </code>
    ///
    /// Arguments:
    ///   [0] message  — the text to display (required)
    ///   [1] level    — optional: Info (default), Warning, Error
    /// </summary>
    public class ShowMessageAction : IModTextAction
    {
        private static readonly LogAspera _log = new LogAspera("ShowMessage");

        /// <summary>Gets the YAML command name for this action.</summary>
        public string CommandName => "ShowMessage";

        /// <summary>
        /// Logs the message at the requested level via BepInEx.
        /// </summary>
        /// <param name="args">args[0] = message text, args[1] = level (optional)</param>
        /// <param name="ctx">Game context (not required for this action)</param>
        /// <returns>True if a message was provided, false otherwise</returns>
        public bool Execute(string[] args, GameCommandsReadyEvent? ctx)
        {
            if (args == null || args.Length == 0)
            {
                _log.Warning("ShowMessage: no message argument provided");
                return false;
            }

            string message = args[0];
            string level = args.Length > 1 ? args[1].ToLowerInvariant() : "info";

            switch (level)
            {
                case "warning":
                case "warn":
                    _log.Warning($"[MOD MSG] {message}");
                    break;
                case "error":
                    _log.Error($"[MOD MSG] {message}");
                    break;
                default:
                    _log.Info($"[MOD MSG] {message}");
                    break;
            }

            return true;
        }
    }
}
