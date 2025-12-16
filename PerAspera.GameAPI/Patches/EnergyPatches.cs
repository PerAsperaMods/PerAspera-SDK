using HarmonyLib;
using PerAspera.Core;
using PerAspera.GameAPI.Overrides.Patching;
using PerAspera.GameAPI.Overrides.Registry;

namespace PerAspera.GameAPI.Patches
{
    /// <summary>
    /// Harmony patches for Energy system getter methods
    /// Uses new generic override system v2.0
    /// NOTE: SolarPanel type must be available from Assembly-CSharp
    /// </summary>
    [AutoOverridePatch("SolarPanel", "GetEnergyProduction", Category = "Energy")]
    [HarmonyPatch]
    public static class EnergyPatches
    {
        private static readonly LogAspera _logger = new LogAspera("GameAPI.Patches.Energy");

        // NOTE: These patches will be applied once SolarPanel type is available
        // from Assembly-CSharp reference (imported via GameLibs.props)
        
        /*
        /// <summary>
        /// Patch for SolarPanel.GetEnergyProduction method
        /// </summary>
        [HarmonyPatch(typeof(SolarPanel), "GetEnergyProduction")]
        [HarmonyPostfix]
        public static void GetEnergyProduction_Postfix(ref float __result, SolarPanel __instance)
        {
            try
            {
                var originalValue = __result;
                OverridePatchHelpers.ApplyOverride(ref __result, "SolarPanel", "GetEnergyProduction", __instance);
                
                if (originalValue != __result)
                {
                    _logger.LogDebug($"SolarPanel.GetEnergyProduction overridden: {originalValue:F2} -> {__result:F2}");
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Error in GetEnergyProduction patch: {ex.Message}");
            }
        }
        */

        /// <summary>
        /// Initialize energy patches when SolarPanel type is available
        /// </summary>
        public static void InitializePatches()
        {
            _logger.Info("Energy patches available for initialization");
            // Patches will be applied via OverridePatchSystem.ApplyPatches()
        }
    }
}