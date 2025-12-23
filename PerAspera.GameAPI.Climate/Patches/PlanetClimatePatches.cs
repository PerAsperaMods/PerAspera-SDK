using HarmonyLib;
using PerAspera.Core;
using System;
using System.Collections.Generic;
using PerAspera.GameAPI.Wrappers;

// Alias pour éviter le conflit Unity.Atmosphere vs PerAspera.GameAPI.Wrappers.Atmosphere
using AtmosphereSDK = PerAspera.GameAPI.Wrappers.Atmosphere;

namespace PerAspera.GameAPI.Climate.Patches
{
    /// <summary>
    /// Harmony patches pour contrôle bidirectionnel entre notre simulation climat et Planet native
    /// Intercepte les getters de Planet pour retourner nos valeurs simulées
    /// </summary>
    [HarmonyPatch]
    public static class PlanetClimatePatches
    {
        private static readonly LogAspera Log = new LogAspera("Climate.Patches");
        
        // Stockage des instances Atmosphere SDK par planet native
        private static readonly Dictionary<object, AtmosphereSDK> _atmosphereOverrides = new();
        private static readonly Dictionary<object, ClimateControlState> _controlStates = new();
        
        /// <summary>
        /// État du contrôle climatique pour une planet donnée
        /// </summary>
        public class ClimateControlState
        {
            public bool IsActive { get; set; } = false;
            public float? TemperatureOverride { get; set; }
            public float? CO2PressureOverride { get; set; }
            public float? O2PressureOverride { get; set; }
            public float? N2PressureOverride { get; set; }
            public float? WaterVaporPressureOverride { get; set; }
        }
        
        /// <summary>
        /// Active le contrôle climatique pour une planet
        /// </summary>
        public static void EnableClimateControl(object nativePlanet)
        {
            if (!_controlStates.ContainsKey(nativePlanet))
                _controlStates[nativePlanet] = new ClimateControlState();
            
            if (!_atmosphereOverrides.ContainsKey(nativePlanet))
                _atmosphereOverrides[nativePlanet] = new AtmosphereSDK(nativePlanet);
                
            _controlStates[nativePlanet].IsActive = true;
            Log.Info("Climate control enabled for planet (Harmony patches active, using SDK Atmosphere)");
        }
        
        /// <summary>
        /// Désactive le contrôle climatique 
        /// </summary>
        public static void DisableClimateControl(object nativePlanet)
        {
            if (_controlStates.ContainsKey(nativePlanet))
            {
                _controlStates[nativePlanet].IsActive = false;
                Log.Info("Climate control disabled - game takes over");
            }
        }
        
        /// <summary>
        /// Met à jour une valeur climatique overridée
        /// </summary>
        public static void SetClimateValue(object nativePlanet, string property, float value)
        {
            if (!_controlStates.ContainsKey(nativePlanet))
                _controlStates[nativePlanet] = new ClimateControlState { IsActive = true };
            
            if (!_atmosphereOverrides.ContainsKey(nativePlanet))
                _atmosphereOverrides[nativePlanet] = new AtmosphereSDK(nativePlanet);
                
            var state = _controlStates[nativePlanet];
            if (!state.IsActive) return;
            
            switch (property)
            {
                case "temperature": state.TemperatureOverride = value; break;
                case "CO2Pressure": state.CO2PressureOverride = value; break;
                case "O2Pressure": state.O2PressureOverride = value; break;
                case "N2Pressure": state.N2PressureOverride = value; break;
                case "waterVaporPressure": state.WaterVaporPressureOverride = value; break;
            }
            
            Log.Debug($"Climate override via SDK Atmosphere: {property} = {value}");
        }
        
        // ========== PATCHES HARMONY SUR LES GETTERS PLANET ==========
        
        /// <summary>
        /// Patch Prefix sur get_temperature() de Planet native
        /// Retourne notre valeur simulée si le contrôle climat est actif
        /// </summary>
        [HarmonyPatch(typeof(Planet), "get_temperature")]
        [HarmonyPrefix]
        public static bool GetTemperature_Prefix(object __instance, ref float __result)
        {
            try
            {
                if (_controlStates.TryGetValue(__instance, out var state) && 
                    state.IsActive && state.TemperatureOverride.HasValue)
                {
                    __result = state.TemperatureOverride.Value;
                    return false; // Skip original method - use SDK override
                }
            }
            catch (Exception ex)
            {
                Log.Error($"GetTemperature patch failed: {ex.Message}");
            }
            
            return true; // Run original method
        }
        
        /// <summary>
        /// Patch Prefix sur get_CO2Pressure() 
        /// </summary>
        [HarmonyPatch(typeof(Planet), "get_CO2Pressure")]
        [HarmonyPrefix]
        public static bool GetCO2Pressure_Prefix(object __instance, ref float __result)
        {
            try
            {
                if (_controlStates.TryGetValue(__instance, out var state) && 
                    state.IsActive && state.CO2PressureOverride.HasValue)
                {
                    __result = state.CO2PressureOverride.Value;
                    return false; // Skip original method - use SDK override
                }
            }
            catch (Exception ex)
            {
                Log.Error($"GetCO2Pressure patch failed: {ex.Message}");
            }
            
            return true;
        }
        
        /// <summary>
        /// Patch Prefix sur get_O2Pressure()
        /// </summary>
        [HarmonyPatch(typeof(Planet), "get_O2Pressure")]
        [HarmonyPrefix]
        public static bool GetO2Pressure_Prefix(object __instance, ref float __result)
        {
            try
            {
                if (_controlStates.TryGetValue(__instance, out var state) && 
                    state.IsActive && state.O2PressureOverride.HasValue)
                {
                    __result = state.O2PressureOverride.Value;
                    return false; // Skip original method - use SDK override
                }
            }
            catch (Exception ex)
            {
                Log.Error($"GetO2Pressure patch failed: {ex.Message}");
            }
            
            return true;
        }
        
        /// <summary>
        /// Patch Prefix sur get_N2Pressure()
        /// </summary>
        [HarmonyPatch(typeof(Planet), "get_N2Pressure")]
        [HarmonyPrefix]
        public static bool GetN2Pressure_Prefix(object __instance, ref float __result)
        {
            try
            {
                if (_controlStates.TryGetValue(__instance, out var state) && 
                    state.IsActive && state.N2PressureOverride.HasValue)
                {
                    __result = state.N2PressureOverride.Value;
                    return false; // Skip original method - use SDK override
                }
            }
            catch (Exception ex)
            {
                Log.Error($"GetN2Pressure patch failed: {ex.Message}");
            }
            
            return true;
        }
        
        /// <summary>
        /// Patch Prefix sur get_waterVaporPressure()
        /// </summary>
        [HarmonyPatch(typeof(Planet), "get_waterVaporPressure")]
        [HarmonyPrefix]
        public static bool GetWaterVaporPressure_Prefix(object __instance, ref float __result)
        {
            try
            {
                if (_controlStates.TryGetValue(__instance, out var state) && 
                    state.IsActive && state.WaterVaporPressureOverride.HasValue)
                {
                    __result = state.WaterVaporPressureOverride.Value;
                    return false; // Skip original method - use SDK override
                }
            }
            catch (Exception ex)
            {
                Log.Error($"GetWaterVaporPressure patch failed: {ex.Message}");
            }
            
            return true;
        }
        
        /// <summary>
        /// Récupère l'instance Atmosphere SDK pour une planète donnée
        /// Permet au ClimateController d'accéder directement à l'Atmosphere encapsulée
        /// </summary>
        public static AtmosphereSDK? GetSDKAtmosphere(object nativePlanet)
        {
            return _atmosphereOverrides.TryGetValue(nativePlanet, out var atmosphere) ? atmosphere : null;
        }
    }
}