using System.Collections.Generic;
using HarmonyLib;
using PerAspera.Core;

namespace PerAspera.GameAPI.Commands.Patches
{
    /// <summary>
    /// Harmony prefix on <c>SpecialProjectType.ValidateConstraints()</c> that validates
    /// custom SDK actions registered in <see cref="CustomCommandRegistry"/>.
    ///
    /// Without this patch, any <c>launchAction</c> or <c>completeAction</c> that uses a
    /// registered custom command is rejected at YAML load time with
    /// "Invalid action command X", because the native <c>ValidateConstraints</c> uses an
    /// AOT-inlined validation path that bypasses <c>InteractionParser.VerifyAction</c>.
    ///
    /// This patch intercepts ONLY when the project contains custom SDK commands.
    /// When custom commands are present it validates ALL actions itself:
    /// <list type="bullet">
    ///   <item>Custom commands (in <see cref="CustomCommandRegistry"/>) are always valid.</item>
    ///   <item>Native commands are validated via <see cref="InteractionParser.VerifyAction"/>.</item>
    /// </list>
    /// When no custom commands are present the native method runs unchanged.
    /// </summary>
    /// <example>
    /// <code>
    /// # project.yaml — passes validation at load time with this patch
    /// launchActions:
    ///   - command: GiveSciencePoints
    ///     arguments: ['2000']
    ///     daysDelay: 0.0
    /// </code>
    /// </example>
    [HarmonyPatch(typeof(SpecialProjectType), "ValidateConstraints")]
    public static class ValidateConstraintsPatch
    {
        private static readonly LogAspera _log = new LogAspera("ValidateConstraintsPatch");

        /// <summary>
        /// Prefix: when the project has at least one custom SDK action, validate all
        /// actions ourselves and skip the native method (which would reject custom commands).
        /// </summary>
        /// <param name="__instance">The <see cref="SpecialProjectType"/> being validated.</param>
        /// <param name="__result">Return value injected by HarmonyX.</param>
        /// <returns>
        /// <c>false</c> (skip native) when custom commands are present;
        /// <c>true</c> (run native) otherwise.
        /// </returns>
        [HarmonyPrefix]
        public static bool Prefix(SpecialProjectType __instance, ref bool __result)
        {
            // Collect all actions from this project
            var allActions = CollectActions(__instance);
            if (allActions.Count == 0)
                return true; // nothing to check, let native run

            // Check if any action uses a registered custom command
            bool hasCustom = false;
            foreach (var action in allActions)
            {
                if (action == null || string.IsNullOrWhiteSpace(action.command))
                    continue;
                if (CustomCommandRegistry.IsRegistered(action.command))
                {
                    hasCustom = true;
                    break;
                }
            }

            if (!hasCustom)
                return true; // no custom commands — let native validate normally

            _log.Info($"[ValidateConstraints] Project '{__instance.key}' has custom SDK actions — validating ourselves.");

            // Validate each action: custom = always valid, native = via VerifyAction
            bool allValid = true;
            foreach (var action in allActions)
            {
                if (action == null || string.IsNullOrWhiteSpace(action.command))
                    continue;

                if (CustomCommandRegistry.IsRegistered(action.command))
                {
                    _log.Info($"[ValidateConstraints] ✅ Custom action '{action.command}' — registered, valid.");
                    continue;
                }

                // Native command — delegate to VerifyAction (silent=true to suppress duplicate errors)
                bool nativeValid = InteractionParser.VerifyAction(action, true);
                if (!nativeValid)
                {
                    _log.Error($"[ValidateConstraints] ❌ Native action '{action.command}' failed validation.");
                    allValid = false;
                }
                else
                {
                    _log.Info($"[ValidateConstraints] ✅ Native action '{action.command}' — valid.");
                }
            }

            __result = allValid;
            return false; // skip native — we did the validation
        }

        /// <summary>
        /// Collects all <see cref="TextAction"/> entries from <c>launchActions</c> and
        /// <c>completeActions</c> on the given <see cref="SpecialProjectType"/> instance.
        /// </summary>
        private static List<TextAction> CollectActions(SpecialProjectType instance)
        {
            var result = new List<TextAction>();
            AppendActions(result, instance.launchActions);
            AppendActions(result, instance.completeActions);
            return result;
        }

        private static void AppendActions(List<TextAction> target,
            Il2CppSystem.Collections.Generic.List<TextAction> source)
        {
            if (source == null) return;
            for (int i = 0; i < source.Count; i++)
            {
                var action = source[i];
                if (action != null)
                    target.Add(action);
            }
        }
    }
}
