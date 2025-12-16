using PerAspera.GameAPI.Overrides.Registry;

namespace PerAspera.GameAPI.Overrides.Patching
{
    /// <summary>
    /// Helper methods to reduce boilerplate in Harmony patches
    /// Provides common patterns for applying overrides in Postfix patches
    /// </summary>
    public static class OverridePatchHelpers
    {
        /// <summary>
        /// Apply override to a method result (Postfix pattern)
        /// Usage in Harmony patch: OverridePatchHelpers.ApplyOverride(ref __result, "Planet", "GetTemperature", __instance);
        /// </summary>
        public static void ApplyOverride<T>(ref T result, string className, string methodName, object? instance = null)
        {
            result = GetterOverrideRegistry.ApplyOverride(result, className, methodName, instance);
        }

        /// <summary>
        /// Check if override should be applied (before patching)
        /// </summary>
        public static bool ShouldApplyOverride(string className, string methodName)
        {
            return GetterOverrideRegistry.IsOverrideActive(className, methodName);
        }

        /// <summary>
        /// Get override value directly (without original value)
        /// Use when you want to completely replace the original logic
        /// </summary>
        public static T? GetOverrideValue<T>(string className, string methodName)
        {
            var overrideConfig = GetterOverrideRegistry.GetOverride<T>(className, methodName);
            return overrideConfig != null && overrideConfig.IsEnabled
                ? overrideConfig.CurrentValue
                : default;
        }

        /// <summary>
        /// Try to apply override, returns false if not active
        /// </summary>
        public static bool TryApplyOverride<T>(ref T result, string className, string methodName, object? instance = null)
        {
            if (!ShouldApplyOverride(className, methodName))
                return false;

            result = GetterOverrideRegistry.ApplyOverride(result, className, methodName, instance);
            return true;
        }
    }
}
