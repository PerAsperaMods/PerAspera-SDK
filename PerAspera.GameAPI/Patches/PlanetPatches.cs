using HarmonyLib;
using PerAspera.Core;
using PerAspera.GameAPI.Overrides.Patching;
using PerAspera.GameAPI.Overrides.Registry;

namespace PerAspera.GameAPI.Patches
{
    /// <summary>
    /// Harmony patches for Planet getter methods
    /// Uses new generic override system v2.0
    /// NOTE: Planet type must be available from Assembly-CSharp
    /// </summary>
    [AutoOverridePatch("Planet", "GetAtmosphericPressure", Category = "Climate")]
    [HarmonyPatch]
    public static class PlanetPatches
    {
        private static readonly LogAspera _logger = new LogAspera("GameAPI.Patches.Planet");

        // NOTE: These patches will be applied once Planet type is available
        // from Assembly-CSharp reference (imported via GameLibs.props)
        
        /*
        /// <summary>
        /// Patch for Planet.GetAtmosphericPressure method
        /// </summary>
        [HarmonyPatch(typeof(Planet), "GetAtmosphericPressure")]
        [HarmonyPostfix]
        public static void GetAtmosphericPressure_Postfix(ref float __result, Planet __instance)
        {
            try
            {
                var originalValue = __result;
                OverridePatchHelpers.ApplyOverride(ref __result, "Planet", "GetAtmosphericPressure", __instance);
                
                if (originalValue != __result)
                {
                    _logger.LogDebug($"Planet.GetAtmosphericPressure overridden: {originalValue:F2} -> {__result:F2}");
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error in GetAtmosphericPressure patch: {ex.Message}");
            }
        }
        */

        /// <summary>
        /// Initialize planet patches when Planet type is available
        /// </summary>
        public static void InitializePatches()
        {
            _logger.Info("Planet patches available for initialization");
            // Patches will be applied via OverridePatchSystem.ApplyPatches()
        }
    }
}