using HarmonyLib;
using PerAspera.Core;
using System;
using System.Collections.Generic;
using PerAspera.GameAPI.Wrappers;

// Alias pour éviter le conflit
// TODO: Update for cellular atmosphere architecture
// using AtmosphereSDK = PerAspera.GameAPI.Wrappers.Atmosphere;

namespace PerAspera.GameAPI.Climate.Patches
{
    /// <summary>
    /// Harmony patches pour contrôle des effets de terraformation
    /// Permet d'ajouter/modifier/surcharger les effets de terraformation existants
    /// </summary>
    [HarmonyPatch]
    public static class TerraformingEffectsPatches
    {
        private static readonly LogAspera Log = new LogAspera("Climate.TerraformingEffects");
        
        // Stockage des effets personnalisés par planet native
        private static readonly Dictionary<object, TerraformingOverrides> _effectOverrides = new();
        
        /// <summary>
        /// Effets de terraformation overridés pour une planète donnée
        /// </summary>
        public class TerraformingOverrides
        {
            public bool IsActive { get; set; } = false;
            public Dictionary<string, float> CustomEffects { get; set; } = new();
            public Dictionary<string, float> VanillaOverrides { get; set; } = new();
        }
        
        /// <summary>
        /// Active le contrôle des effets de terraformation pour une planète
        /// </summary>
        public static void EnableTerraformingControl(object nativePlanet)
        {
            if (!_effectOverrides.ContainsKey(nativePlanet))
                _effectOverrides[nativePlanet] = new TerraformingOverrides();
                
            _effectOverrides[nativePlanet].IsActive = true;
            Log.Info("Terraforming effects control enabled for planet (Harmony patches active)");
        }
        
        /// <summary>
        /// Désactive le contrôle des effets de terraformation
        /// </summary>
        public static void DisableTerraformingControl(object nativePlanet)
        {
            if (_effectOverrides.ContainsKey(nativePlanet))
            {
                _effectOverrides[nativePlanet].IsActive = false;
                Log.Info("Terraforming effects control disabled - game takes over");
            }
        }
        
        /// <summary>
        /// Ajoute un effet de terraformation personnalisé
        /// </summary>
        public static void AddCustomTerraformingEffect(object nativePlanet, string effectName, float temperatureEffect)
        {
            if (!_effectOverrides.ContainsKey(nativePlanet))
                _effectOverrides[nativePlanet] = new TerraformingOverrides { IsActive = true };
                
            var overrides = _effectOverrides[nativePlanet];
            if (!overrides.IsActive) return;
            
            overrides.CustomEffects[effectName] = temperatureEffect;
            Log.Debug($"Custom terraforming effect added: {effectName} = {temperatureEffect:F2}K");
        }
        
        /// <summary>
        /// Surcharge un effet de terraformation vanilla existant
        /// </summary>
        public static void OverrideVanillaTerraformingEffect(object nativePlanet, string effectType, float temperatureEffect)
        {
            if (!_effectOverrides.ContainsKey(nativePlanet))
                _effectOverrides[nativePlanet] = new TerraformingOverrides { IsActive = true };
                
            var overrides = _effectOverrides[nativePlanet];
            if (!overrides.IsActive) return;
            
            overrides.VanillaOverrides[effectType] = temperatureEffect;
            Log.Debug($"Vanilla terraforming effect overridden: {effectType} = {temperatureEffect:F2}K");
        }
        
        /// <summary>
        /// Calcule l'effet total de terraformation (vanilla + custom)
        /// </summary>
        public static float GetTotalTerraformingEffect(object nativePlanet)
        {
            if (!_effectOverrides.TryGetValue(nativePlanet, out var overrides) || !overrides.IsActive)
                return 0f;
                
            float totalEffect = 0f;
            
            // Ajouter les effets personnalisés
            foreach (var effect in overrides.CustomEffects.Values)
            {
                totalEffect += effect;
            }
            
            Log.Debug($"Total custom terraforming effect: {totalEffect:F2}K");
            return totalEffect;
        }
        
        // ========== PATCHES HARMONY SUR LES EFFETS DE TERRAFORMATION ==========
        
        /// <summary>
        /// Patch Prefix sur get_polarTemperatureNukeEffect() 
        /// </summary>
        [HarmonyPatch(typeof(Planet), "get_polarTemperatureNukeEffect")]
        [HarmonyPrefix]
        public static bool GetPolarNukeEffect_Prefix(object __instance, ref float __result)
        {
            try
            {
                if (_effectOverrides.TryGetValue(__instance, out var overrides) && 
                    overrides.IsActive && overrides.VanillaOverrides.TryGetValue("PolarNuke", out var value))
                {
                    __result = value;
                    return false; // Skip original method - use override
                }
            }
            catch (Exception ex)
            {
                Log.Error($"GetPolarNukeEffect patch failed: {ex.Message}");
            }
            
            return true; // Run original method
        }
        
        /// <summary>
        /// Patch Prefix sur get_polarTemperatureDustEffect()
        /// </summary>
        [HarmonyPatch(typeof(Planet), "get_polarTemperatureDustEffect")]
        [HarmonyPrefix]
        public static bool GetPolarDustEffect_Prefix(object __instance, ref float __result)
        {
            try
            {
                if (_effectOverrides.TryGetValue(__instance, out var overrides) && 
                    overrides.IsActive && overrides.VanillaOverrides.TryGetValue("PolarDust", out var value))
                {
                    __result = value;
                    return false; // Skip original method - use override
                }
            }
            catch (Exception ex)
            {
                Log.Error($"GetPolarDustEffect patch failed: {ex.Message}");
            }
            
            return true;
        }
        
        /// <summary>
        /// Patch Prefix sur get_temperatureCometEffect()
        /// </summary>
        [HarmonyPatch(typeof(Planet), "get_temperatureCometEffect")]
        [HarmonyPrefix]
        public static bool GetCometEffect_Prefix(object __instance, ref float __result)
        {
            try
            {
                if (_effectOverrides.TryGetValue(__instance, out var overrides) && 
                    overrides.IsActive && overrides.VanillaOverrides.TryGetValue("Comet", out var value))
                {
                    __result = value;
                    return false; // Skip original method - use override
                }
            }
            catch (Exception ex)
            {
                Log.Error($"GetCometEffect patch failed: {ex.Message}");
            }
            
            return true;
        }
        
        /// <summary>
        /// Patch Prefix sur get_temperatureDeimosEffect()
        /// </summary>
        [HarmonyPatch(typeof(Planet), "get_temperatureDeimosEffect")]
        [HarmonyPrefix]
        public static bool GetDeimosEffect_Prefix(object __instance, ref float __result)
        {
            try
            {
                if (_effectOverrides.TryGetValue(__instance, out var overrides) && 
                    overrides.IsActive && overrides.VanillaOverrides.TryGetValue("Deimos", out var value))
                {
                    __result = value;
                    return false; // Skip original method - use override
                }
            }
            catch (Exception ex)
            {
                Log.Error($"GetDeimosEffect patch failed: {ex.Message}");
            }
            
            return true;
        }
        
        /// <summary>
        /// Patch Postfix sur GetAverageTemperature() pour ajouter les effets custom
        /// Ajoute automatiquement les effets personnalisés au calcul de température
        /// </summary>
        [HarmonyPatch(typeof(Planet), "GetAverageTemperature")]
        [HarmonyPostfix]
        public static void GetAverageTemperature_Postfix(object __instance, ref float __result)
        {
            try
            {
                if (_effectOverrides.TryGetValue(__instance, out var overrides) && overrides.IsActive)
                {
                    float customEffectTotal = 0f;
                    foreach (var effect in overrides.CustomEffects.Values)
                    {
                        customEffectTotal += effect;
                    }
                    
                    if (customEffectTotal != 0f)
                    {
                        __result += customEffectTotal;
                        Log.Debug($"Added custom terraforming effects: {customEffectTotal:F2}K to temperature");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"GetAverageTemperature postfix patch failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Récupère les overrides d'effets de terraformation pour une planète donnée
        /// </summary>
        public static TerraformingOverrides? GetTerraformingOverrides(object nativePlanet)
        {
            return _effectOverrides.TryGetValue(nativePlanet, out var overrides) ? overrides : null;
        }
    }
}