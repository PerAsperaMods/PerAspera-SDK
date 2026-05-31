using System.Globalization;
using PerAspera.Core;
using PerAspera.GameAPI.Events.SDK;

namespace PerAspera.GameAPI.Commands.ModActions.BuiltinActions
{
    /// <summary>
    /// Static helpers shared by all built-in SDK actions.
    /// Provides safe argument parsing and faction access so individual actions
    /// stay short and focus on their own logic.
    ///
    /// Usage pattern for a new IModTextAction:
    /// <code>
    /// public bool Execute(string[] args, GameCommandsReadyEvent? ctx)
    /// {
    ///     if (!ActionContextHelper.TryGetFloat(args, 0, out float amount, _log, CommandName))
    ///         return false;
    ///     if (!ActionContextHelper.TryGetFaction(ctx, out var faction, _log, CommandName))
    ///         return false;
    ///
    ///     faction.AddResearchPoints(amount);
    ///     return true;
    /// }
    /// </code>
    /// </summary>
    public static class ActionContextHelper
    {
        // ─── Argument helpers ─────────────────────────────────────────────────

        /// <summary>
        /// Parse a required float argument.
        /// Logs a warning and sets amount=0 on failure.
        /// </summary>
        /// <param name="args">The full arguments array</param>
        /// <param name="index">Zero-based index of the argument to parse</param>
        /// <param name="amount">Parsed value, or 0 if parsing fails</param>
        /// <param name="log">Logger for the calling action</param>
        /// <param name="actionName">Name used in the warning message</param>
        /// <returns>True if parsing succeeded</returns>
        public static bool TryGetFloat(string[] args, int index, out float amount, LogAspera log, string actionName)
        {
            amount = 0f;
            if (args == null || args.Length <= index)
            {
                log.Warning($"{actionName}: missing argument at index {index}");
                return false;
            }

            if (!float.TryParse(args[index], NumberStyles.Float, CultureInfo.InvariantCulture, out amount))
            {
                log.Warning($"{actionName}: argument[{index}] '{args[index]}' is not a valid number");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parse a required positive float argument (> 0).
        /// Logs a warning if the value is zero or negative.
        /// </summary>
        public static bool TryGetPositiveFloat(string[] args, int index, out float amount, LogAspera log, string actionName)
        {
            if (!TryGetFloat(args, index, out amount, log, actionName))
                return false;

            if (amount <= 0f)
            {
                log.Warning($"{actionName}: argument[{index}] must be positive (got {amount})");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Parse a required integer argument.
        /// </summary>
        public static bool TryGetInt(string[] args, int index, out int value, LogAspera log, string actionName)
        {
            value = 0;
            if (args == null || args.Length <= index)
            {
                log.Warning($"{actionName}: missing argument at index {index}");
                return false;
            }

            if (!int.TryParse(args[index], out value))
            {
                log.Warning($"{actionName}: argument[{index}] '{args[index]}' is not a valid integer");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the string argument at the given index, or null if absent.
        /// Never throws — safe to call with any args array.
        /// </summary>
        public static string? GetString(string[] args, int index)
        {
            if (args == null || args.Length <= index) return null;
            return args[index];
        }

        // ─── Faction helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Get the NativePlayerFaction from context.
        /// Logs a warning and returns null if context or faction is unavailable.
        /// </summary>
        /// <example>
        /// <code>
        /// if (!ActionContextHelper.TryGetFaction(ctx, out var faction, _log, CommandName))
        ///     return false;
        /// faction.AddResearchPoints(500f);
        /// </code>
        /// </example>
        public static bool TryGetFaction(GameCommandsReadyEvent? ctx, out Faction? faction, LogAspera log, string actionName)
        {
            faction = ctx?.NativePlayerFaction;
            if (faction == null)
            {
                log.Warning($"{actionName}: player faction not available in context (ctx={(ctx == null ? "null" : "present")})");
                return false;
            }

            return true;
        }

        // ─── Context helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Returns true if a valid context with a non-null faction is present.
        /// Useful for optional context-dependent behaviour.
        /// </summary>
        public static bool HasFaction(GameCommandsReadyEvent? ctx)
            => ctx?.NativePlayerFaction != null;
    }
}
