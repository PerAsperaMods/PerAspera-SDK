using HarmonyLib;
using PerAspera.Core;

namespace PerAspera.GameAPI.Commands.Patches
{
    /// <summary>
    /// Harmony prefix on <c>InteractionParser.VerifyAction</c> that bypasses the native
    /// validation for any <see cref="TextAction"/> whose command is registered in
    /// <see cref="CustomCommandRegistry"/>.
    ///
    /// Without this patch the game rejects custom commands at YAML load time
    /// (SpecialProjectType.ValidateConstraints → VerifyAction → "Invalid action command")
    /// even though the runtime dispatch would work correctly via
    /// <see cref="NativeDispatchInterceptPatch"/>.
    ///
    /// With this patch, load-time validation passes for all registered custom commands,
    /// allowing them to be used in launchActions, completeActions, unlockActions, etc.
    /// </summary>
    /// <example>
    /// <code>
    /// # project.yaml — now valid at load time AND dispatched at runtime
    /// launchActions:
    ///   - command: ShowMessage
    ///     arguments: ['coucou']
    ///     daysDelay: 0.0
    /// </code>
    /// </example>
    [HarmonyPatch(typeof(InteractionParser), nameof(InteractionParser.VerifyAction),
        typeof(TextAction), typeof(bool))]
    public static class VerifyActionPatch
    {
        private static readonly LogAspera _log = new LogAspera("VerifyActionPatch");

        /// <summary>
        /// Prefix: if the TextAction's command is a registered custom handler,
        /// set result to true and skip the native method (which would return false).
        /// </summary>
        /// <param name="action">The TextAction being validated.</param>
        /// <param name="__result">Output result injected by HarmonyX.</param>
        /// <returns>false = skip native (our result is used); true = let native run.</returns>
        [HarmonyPrefix]
        public static bool Prefix(TextAction action, ref bool __result)
        {
            if (action == null || string.IsNullOrWhiteSpace(action.command))
                return true;

            if (!CustomCommandRegistry.IsRegistered(action.command))
                return true;

            _log.Info($"[VerifyAction] Allowing custom command '{action.command}' (registered in CustomCommandRegistry)");
            __result = true;
            return false; // skip native validation
        }
    }
}
