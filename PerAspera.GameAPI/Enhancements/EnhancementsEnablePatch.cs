using System.Globalization;
using HarmonyLib;
using PerAspera.Core;

namespace PerAspera.GameAPI.Enhancements
{
    /// <summary>
    /// Harmony Postfix on <c>Enhancements.Enable(EnhancementType, bool)</c>.
    /// After the native system applies its known modifiers, this patch iterates
    /// <c>EnhancementType.modifiers</c> (typed IL2CPP field — <c>List&lt;string&gt;</c>)
    /// and dispatches unknown keys to <see cref="CustomModifierRegistry"/>.
    ///
    /// Native keys (handled automatically by the game) are silently skipped — only keys
    /// with a registered handler in <see cref="CustomModifierRegistry"/> produce a callback.
    /// </summary>
    /// <example>
    /// // In your plugin Awake() / Load():
    /// EnhancementsEnablePatch.Apply(harmony);
    ///
    /// // Then register handlers:
    /// CustomModifierRegistry.Register("drone_hop_bonus", delta =>
    ///     RoutingPatch.ExtraHopCapacity += (int)delta);
    /// </example>
    [HarmonyPatch(typeof(global::Enhancements), nameof(global::Enhancements.Enable))]
    public static class EnhancementsEnablePatch
    {
        private static readonly LogAspera _log = new LogAspera("GameAPI.Enhancements.EnablePatch");

        /// <summary>
        /// Apply this patch manually against a Harmony instance.
        /// Call once from your plugin's initialization — safe to call multiple times
        /// (subsequent calls are no-ops because Harmony deduplicates patches).
        /// </summary>
        /// <example>
        /// var harmony = new Harmony("com.mymod.id");
        /// EnhancementsEnablePatch.Apply(harmony);
        /// </example>
        public static void Apply(Harmony harmony)
        {
            var original = AccessTools.Method(typeof(global::Enhancements), nameof(global::Enhancements.Enable));
            var postfix  = new HarmonyMethod(typeof(EnhancementsEnablePatch), nameof(Postfix));
            harmony.Patch(original, postfix: postfix);
            _log.Info("EnhancementsEnablePatch applied");
        }

        /// <summary>
        /// Postfix — runs after the native Enable() has finished.
        /// Reads the typed <c>EnhancementType.modifiers</c> field and dispatches
        /// any modifier key registered in <see cref="CustomModifierRegistry"/>.
        /// </summary>
        [HarmonyPostfix]
        public static void Postfix(EnhancementType enhancement)
        {
            if (enhancement == null) return;

            // Typed IL2CPP field — confirmed in InteropDump/ScriptsAssembly/EnhancementType.cs:728
            var mods = enhancement.modifiers;
            if (mods == null || mods.Count == 0) return;

            for (int i = 0; i < mods.Count; i++)
            {
                string? raw = mods[i];
                if (raw == null) continue;

                // YAML format: "key: value"
                int colon = raw.IndexOf(':');
                if (colon < 1) continue;

                string key   = raw[..colon].Trim();
                string valStr = raw[(colon + 1)..].Trim();

                if (!CustomModifierRegistry.IsRegistered(key)) continue;

                // Parse as float — InvariantCulture required for "−0.05" etc.
                if (!float.TryParse(valStr, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                {
                    // Boolean shorthand: "true"/"false"
                    if (valStr.Equals("true",  StringComparison.OrdinalIgnoreCase)) value = 1f;
                    else if (valStr.Equals("false", StringComparison.OrdinalIgnoreCase)) value = 0f;
                    else
                    {
                        _log.Warning($"Custom modifier '{key}': cannot parse value '{valStr}' as float — skipped");
                        continue;
                    }
                }

                CustomModifierRegistry.TryInvoke(key, value);
            }
        }
    }
}
