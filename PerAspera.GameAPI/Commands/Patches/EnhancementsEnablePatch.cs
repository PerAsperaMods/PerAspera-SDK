using System;
using System.Globalization;
using HarmonyLib;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Commands.Patches
{
    /// <summary>
    /// Harmony Postfix on <c>Enhancements.Enable(EnhancementType, bool)</c>.
    ///
    /// After the native Enable() runs (which stores all modifier values in the game's
    /// internal modifierValues dictionary), this postfix reads the raw modifier strings
    /// from the EnhancementType and invokes any registered handler in
    /// <see cref="CustomModifierRegistry"/> for each custom modifier key found.
    ///
    /// This mirrors <see cref="NativeDispatchInterceptPatch"/> for the Commands system:
    /// the game's own parsing runs untouched, then we piggyback on top for our custom keys.
    /// </summary>
    /// <example>
    /// <code>
    /// # enhancements.yaml
    /// enhancement_routing_optimization_1:
    ///   modifiers:
    ///     - "drone_hop_capacity: 1"   # custom key — native ignores it, our handler fires
    ///     - "extraction_time: -0.05"  # native key — native handles it, no custom handler needed
    ///
    /// // Plugin.cs
    /// Commands.RegisterModifier("drone_hop_capacity", (name, delta) => {
    ///     RoutingPatch.ExtraHopCapacity += (int)delta;
    /// });
    /// </code>
    /// </example>
    [HarmonyPatch(typeof(Enhancements), "Enable")]
    public static class EnhancementsEnablePatch
    {
        private static readonly LogAspera _log = new LogAspera("EnhancementsEnablePatch");

        /// <summary>
        /// Postfix: runs after native Enable(). Iterates modifier strings on the enabled
        /// EnhancementType and calls registered handlers for custom modifier keys.
        /// </summary>
        [HarmonyPostfix]
        public static void Postfix(object __instance, EnhancementType enhancement)
        {
            if (enhancement == null)
                return;

            try
            {
                // Get the modifiers list from the EnhancementType via reflection helper
                // (avoids hard dependency on IL2CPP interop List type; works with any BepInX 6 setup)
                var modifiersList = enhancement.GetMemberValue<object>("modifiers");
                if (modifiersList == null)
                    return;

                // Iterate IL2CPP List<string> or System.Collections.Generic.List<string>
                var count = modifiersList.InvokeMethod<int?>("get_Count") ?? 0;
                for (int i = 0; i < count; i++)
                {
                    var rawStr = modifiersList.InvokeMethod<object>("get_Item", i) as string;
                    if (string.IsNullOrWhiteSpace(rawStr))
                        continue;

                    if (!TryParseModifierString(rawStr, out string key, out float value))
                        continue;

                    if (!CustomModifierRegistry.IsRegistered(key))
                        continue;

                    _log.Info($"[EnhancementsEnable] Custom modifier '{key}' = {value} → invoking handler");
                    CustomModifierRegistry.TryInvoke(key, value);
                }
            }
            catch (Exception ex)
            {
                _log.Warning($"[EnhancementsEnablePatch] Error reading modifiers: {ex.Message}");
            }
        }

        /// <summary>
        /// Parse a YAML modifier string of the form "key: value" into its components.
        /// Handles both integer and float values. Examples:
        ///   "worker_speed: 0.05"   → ("worker_speed", 0.05f)
        ///   "building_limit: 50"   → ("building_limit", 50f)
        ///   "has_feature: true"    → ("has_feature", 1f)
        ///   "has_feature: false"   → ("has_feature", 0f)
        /// </summary>
        internal static bool TryParseModifierString(string raw, out string key, out float value)
        {
            key = string.Empty;
            value = 0f;

            if (string.IsNullOrWhiteSpace(raw))
                return false;

            int colonIdx = raw.IndexOf(':');
            if (colonIdx < 1)
                return false;

            key = raw.Substring(0, colonIdx).Trim();
            var valueStr = raw.Substring(colonIdx + 1).Trim();

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(valueStr))
                return false;

            // Handle bool values
            if (valueStr.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                value = 1f;
                return true;
            }
            if (valueStr.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                value = 0f;
                return true;
            }

            // Parse numeric value
            if (!float.TryParse(valueStr, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                return false;

            return true;
        }
    }
}
