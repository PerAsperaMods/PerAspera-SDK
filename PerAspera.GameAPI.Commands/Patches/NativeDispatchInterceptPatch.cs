using System;
using System.Linq;
using HarmonyLib;
using PerAspera.Core;

namespace PerAspera.GameAPI.Commands.Patches
{
    /// <summary>
    /// Harmony prefix on InteractionManager.DispatchAction that intercepts any TextAction
    /// whose command name is registered in <see cref="CustomCommandRegistry"/>.
    ///
    /// This means custom SDK actions (e.g. ShowMessage) registered via
    /// <c>Commands.RegisterAction&lt;T&gt;()</c> or <c>Commands.RegisterHandler()</c>
    /// can be used directly in ANY native YAML context:
    ///   - SpecialProjectType.launchActions / completeActions
    ///   - rule-patch.yaml actions
    ///   - technology unlockActions
    ///   - anywhere the game calls InteractionManager.DispatchAction
    /// </summary>
    /// <example>
    /// <code>
    /// # project.yaml — works after Commands.RegisterAction&lt;ShowMessageAction&gt;()
    /// launchActions:
    ///   - command: ShowMessage
    ///     arguments: ['coucou']
    ///     daysDelay: 0.0
    /// </code>
    /// </example>
    [HarmonyPatch(typeof(InteractionManager), "DispatchAction",
        typeof(IHandleable), typeof(GameEventBus), typeof(TextAction), typeof(string))]
    public static class NativeDispatchInterceptPatch
    {
        private static readonly LogAspera _log = new LogAspera("NativeDispatchIntercept");

        /// <summary>
        /// The IHandleable sender of the current dispatch call.
        /// Set for the duration of handler execution so actions can access the live sender.
        /// Typically the Faction for SpecialProject launch actions.
        /// </summary>
        internal static IHandleable? CurrentSender;

        /// <summary>
        /// Prefix: if the command matches a registered custom handler, execute it and
        /// return false (skip native dispatch). Otherwise return true (let native run).
        /// </summary>
        [HarmonyPrefix]
        public static bool Prefix(IHandleable asSender, GameEventBus bus, TextAction textAction, string context)
        {
            if (textAction == null || string.IsNullOrWhiteSpace(textAction.command))
                return true; // let native handle it

            if (!CustomCommandRegistry.IsRegistered(textAction.command))
                return true; // not a custom command — let native handle it

            try
            {
                var args = textAction.arguments != null
                    ? textAction.arguments.ToArray()
                    : Array.Empty<string>();

                _log.Info($"[NativeIntercept] Routing '{textAction.command}' → CustomCommandRegistry (context={context})");

                var handler = CustomCommandRegistry.GetHandler(textAction.command);
                CurrentSender = asSender;
                bool result;
                try { result = handler(textAction.command, args); }
                finally { CurrentSender = null; }

                if (result)
                    _log.Info($"[NativeIntercept] ✅ '{textAction.command}' executed");
                else
                    _log.Warning($"[NativeIntercept] ⚠ '{textAction.command}' returned false");

                return false; // skip native DispatchAction
            }
            catch (Exception ex)
            {
                _log.Warning($"[NativeIntercept] '{textAction.command}' threw: {ex.Message}");
                return false; // still skip native — command was recognized
            }
        }
    }
}
