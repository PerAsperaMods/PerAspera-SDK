using HarmonyLib;
using PerAspera.Core;
using System;
using System.Collections.Generic;
using PerAspera.GameAPI.Wrappers;

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
        
        // Stockage des valeurs overridées par notre simulation
        private static readonly Dictionary<object, ClimateOverrides> _overrides = new();
        
        /// <summary>
        /// Valeurs climatiques overridées pour un planet donné
        /// </summary>
        public class ClimateOverrides
        {
            public float? Temperature { get; set; }
            public float? CO2Pressure { get; set; }
            public float? O2Pressure { get; set; }
            public float? N2Pressure { get; set; }
            public float? WaterVaporPressure { get; set; }
            public bool IsActive { get; set; } = false;
        }
        
        /// <summary>
        /// Active le contrôle climatique pour une planet
        /// </summary>
        public static void EnableClimateControl(object nativePlanet)
        {
            if (!_overrides.ContainsKey(nativePlanet))
                _overrides[nativePlanet] = new ClimateOverrides();
                
            _overrides[nativePlanet].IsActive = true;
            Log.Info("Climate control enabled for planet (Harmony patches active)");
        }
        
        /// <summary>
        /// Désactive le contrôle climatique 
        /// </summary>
        public static void DisableClimateControl(object nativePlanet)
        {
            if (_overrides.ContainsKey(nativePlanet))
            {
                _overrides[nativePlanet].IsActive = false;
                Log.Info("Climate control disabled - game takes over");
            }
        }
        
        /// <summary>
        /// Met à jour une valeur climatique overridée
        /// </summary>
        public static void SetClimateValue(object nativePlanet, string property, float value)
        {
            if (!_overrides.ContainsKey(nativePlanet))
                _overrides[nativePlanet] = new ClimateOverrides { IsActive = true };
                
            var overrides = _overrides[nativePlanet];
            if (!overrides.IsActive) return;
            
            switch (property)
            {
                case "temperature": overrides.Temperature = value; break;
                case "CO2Pressure": overrides.CO2Pressure = value; break;
                case "O2Pressure": overrides.O2Pressure = value; break;
                case "N2Pressure": overrides.N2Pressure = value; break;
                case "waterVaporPressure": overrides.WaterVaporPressure = value; break;
            }
            
            Log.Debug($"Climate override: {property} = {value}");
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
                if (_overrides.TryGetValue(__instance, out var overrides) && 
                    overrides.IsActive && overrides.Temperature.HasValue)
                {
                    __result = overrides.Temperature.Value;
                    return false; // Skip original method
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
                if (_overrides.TryGetValue(__instance, out var overrides) && 
                    overrides.IsActive && overrides.CO2Pressure.HasValue)
                {
                    __result = overrides.CO2Pressure.Value;
                    return false;
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
                if (_overrides.TryGetValue(__instance, out var overrides) && 
                    overrides.IsActive && overrides.O2Pressure.HasValue)
                {
                    __result = overrides.O2Pressure.Value;
                    return false;
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
                if (_overrides.TryGetValue(__instance, out var overrides) && 
                    overrides.IsActive && overrides.N2Pressure.HasValue)
                {
                    __result = overrides.N2Pressure.Value;
                    return false;
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
                if (_overrides.TryGetValue(__instance, out var overrides) && 
                    overrides.IsActive && overrides.WaterVaporPressure.HasValue)
                {
                    __result = overrides.WaterVaporPressure.Value;
                    return false;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"GetWaterVaporPressure patch failed: {ex.Message}");
            }
            
            return true;
        }
    }
}