using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Native.Events
{
    /// <summary>
    /// Climate event patching service for Per Aspera
    /// Handles all climate-related events including temperature, pressure, and atmospheric composition changes
    /// Uses static individual patches for IL2CPP HarmonyX compatibility
    /// </summary>
    public sealed class ClimateEventPatchingService : BaseEventPatchingService
    {
        private static System.Type _planetType;
        private static readonly Dictionary<object, Dictionary<string, object>> _climateStateCache = new();

        /// <summary>
        /// Initialize climate event patching service
        /// </summary>
        /// <param name="harmony">Harmony instance for IL2CPP patching</param>
        public ClimateEventPatchingService(Harmony harmony)
            : base("Climate", harmony)
        {
        }

        /// <summary>
        /// Get the event type identifier for this service
        /// </summary>
        /// <returns>Event type string</returns>
        public override string GetEventType() => "Climate";

        /// <summary>
        /// Initialize all climate-related event hooks with static patches
        /// </summary>
        /// <returns>Number of successfully hooked methods</returns>
        public override int InitializeEventHooks()
        {
            _log.Debug("🌡️ Setting up climate event hooks with IL2CPP-compatible patches...");

            _planetType = GameTypeInitializer.GetPlanetType();
            if (_planetType == null)
            {
                _log.Warning("Planet type not found, skipping climate hooks");
                return 0;
            }

            int hookedCount = 0;

            // Hook all climate modifier methods with postfix (capture changes)
            var modifiers = new[] {
                "IncreaseCO2", "DecreaseCO2",
                "IncreaseO2", "DecreaseO2",
                "IncreaseN2", "DecreaseN2",
                "IncreaseGHG", "DecreaseGHG",
                "IncreaseWater", "DecreaseWater",
                "ConvertCO2IntoO2", "ConvertO2IntoCO2"
            };

            foreach (var methodName in modifiers)
            {
                if (PatchClimateModifier(methodName))
                    hookedCount++;
            }

            _log.Info($"✅ Climate hooks initialized: {hookedCount}/{modifiers.Length} modifier methods hooked");
            return hookedCount;
        }

        /// <summary>
        /// Patch a climate modifier method (IncreaseCO2, DecreaseCO2, etc.)
        /// Uses postfix to capture result after method execution
        /// </summary>
        private bool PatchClimateModifier(string methodName)
        {
            try
            {
                var method = AccessTools.Method(_planetType, methodName);
                if (method == null)
                {
                    _log.Warning($"Method {methodName} not found on Planet");
                    return false;
                }

                var postfixMethod = AccessTools.Method(
                    typeof(ClimateEventPatchingService),
                    $"{methodName}_Postfix");

                if (postfixMethod == null)
                {
                    _log.Warning($"Postfix method {methodName}_Postfix not found");
                    return false;
                }

                _harmony.Patch(method, postfix: new HarmonyMethod(postfixMethod));
                var patchKey = $"Planet.{methodName}";
                _patchedMethods[patchKey] = "ClimateModifier";

                _log.Debug($"✓ Hooked {patchKey}");
                return true;
            }
            catch (Exception ex)
            {
                _log.Warning($"Failed to patch {methodName}: {ex.Message}");
                return false;
            }
        }

        // ===== STATIC POSTFIX PATCHES FOR CLIMATE MODIFIERS =====
        // Each patch follows IL2CPP HarmonyX convention: __instance, method params, __result

        [HarmonyPostfix]
        private static void IncreaseCO2_Postfix(object __instance, float pressure, float __result)
        {
            PublishClimateEvent(__instance, "CO2Pressure", pressure, __result);
        }

        [HarmonyPostfix]
        private static void DecreaseCO2_Postfix(object __instance, float pressure, float __result)
        {
            PublishClimateEvent(__instance, "CO2Pressure", -pressure, __result);
        }

        [HarmonyPostfix]
        private static void IncreaseO2_Postfix(object __instance, float pressure, float __result)
        {
            PublishClimateEvent(__instance, "O2Pressure", pressure, __result);
        }

        [HarmonyPostfix]
        private static void DecreaseO2_Postfix(object __instance, float pressure, float __result)
        {
            PublishClimateEvent(__instance, "O2Pressure", -pressure, __result);
        }

        [HarmonyPostfix]
        private static void IncreaseN2_Postfix(object __instance, float pressure)
        {
            PublishClimateEvent(__instance, "N2Pressure", pressure, null);
        }

        [HarmonyPostfix]
        private static void DecreaseN2_Postfix(object __instance, float pressure)
        {
            PublishClimateEvent(__instance, "N2Pressure", -pressure, null);
        }

        [HarmonyPostfix]
        private static void IncreaseGHG_Postfix(object __instance, float pressure)
        {
            PublishClimateEvent(__instance, "GHGPressure", pressure, null);
        }

        [HarmonyPostfix]
        private static void DecreaseGHG_Postfix(object __instance, float pressure)
        {
            PublishClimateEvent(__instance, "GHGPressure", -pressure, null);
        }

        [HarmonyPostfix]
        private static void IncreaseWater_Postfix(object __instance, float pressure)
        {
            PublishClimateEvent(__instance, "WaterStock", pressure, null);
        }

        [HarmonyPostfix]
        private static void DecreaseWater_Postfix(object __instance, float pressure)
        {
            PublishClimateEvent(__instance, "WaterStock", -pressure, null);
        }

        [HarmonyPostfix]
        private static void ConvertCO2IntoO2_Postfix(object __instance, float __result)
        {
            PublishClimateEvent(__instance, "Conversion", __result, null);
        }

        [HarmonyPostfix]
        private static void ConvertO2IntoCO2_Postfix(object __instance, float __result)
        {
            PublishClimateEvent(__instance, "Conversion", __result, null);
        }

        /// <summary>
        /// Publish climate change event with proper data
        /// </summary>
        private static void PublishClimateEvent(object instance, string climateType, object delta, object newValue)
        {
            try
            {
                var eventData = new
                {
                    Planet = instance,
                    ClimateType = climateType,
                    Delta = delta,
                    NewValue = newValue,
                    Timestamp = DateTime.UtcNow
                };

                ModEventBus.Publish($"Climate{climateType}Changed", eventData);
                ModEventBus.Publish("ClimateChanged", eventData);
            }
            catch (Exception ex)
            {
                // Fail silently to avoid disrupting game flow
            }
        }

        /// <summary>
        /// Get diagnostic information about climate event hooks
        /// </summary>
        /// <returns>Diagnostic information string</returns>
        public string GetDiagnosticInfo()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine("=== Climate Event Patching Service ===");
            info.AppendLine($"Planet Type: {GetFriendlyTypeName(_planetType)}");
            info.AppendLine($"Hooked Methods: {_patchedMethods.Count}");
            info.AppendLine();

            foreach (var patch in _patchedMethods)
            {
                info.AppendLine($"  ✓ {patch.Key} → {patch.Value}");
            }

            return info.ToString();
        }
    }
}

