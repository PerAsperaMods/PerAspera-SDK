using System;
using PerAspera.Core;
using PerAspera.GameAPI.Climate.Patches;
using PerAspera.GameAPI.Wrappers;

namespace PerAspera.GameAPI.Climate
{
    /// <summary>
    /// Contrôleur pour les effets de terraformation personnalisés
    /// Permet d'ajouter/modifier des effets de température sur la planète
    /// </summary>
    public class TerraformingEffectsController
    {
        private static readonly LogAspera Log = new LogAspera("Climate.TerraformingController");
        
        private readonly object _nativePlanet;
        private bool _isActive = false;
        
        public TerraformingEffectsController(GameAPI.Wrappers.Planet planet)
        {
            _nativePlanet = planet.GetNativeObject() ?? throw new ArgumentNullException(nameof(planet));
        }
        
        /// <summary>
        /// Active le contrôle des effets de terraformation
        /// </summary>
        public void EnableControl()
        {
            _isActive = true;
            TerraformingEffectsPatches.EnableTerraformingControl(_nativePlanet);
            Log.Info("Terraforming effects control activated");
        }
        
        /// <summary>
        /// Désactive le contrôle des effets de terraformation
        /// </summary>
        public void DisableControl()
        {
            _isActive = false;
            TerraformingEffectsPatches.DisableTerraformingControl(_nativePlanet);
            Log.Info("Terraforming effects control deactivated");
        }
        
        /// <summary>
        /// Ajoute un effet de terraformation temporaire (ex: événement Twitch)
        /// </summary>
        /// <param name="effectName">Nom de l'effet (ex: "TwitchHeatWave")</param>
        /// <param name="temperatureChange">Changement de température en Kelvin</param>
        /// <param name="source">Source de l'effet (ex: "Twitch follower: Username")</param>
        public void AddTempEffect(string effectName, float temperatureChange, string source = "Unknown")
        {
            if (!_isActive)
            {
                Log.Warning("Cannot add terraforming effect: control not active");
                return;
            }
            
            try
            {
                TerraformingEffectsPatches.AddCustomTerraformingEffect(_nativePlanet, effectName, temperatureChange);
                Log.Info($"Added terraforming effect '{effectName}': {temperatureChange:+0.00;-0.00;0}K from {source}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to add terraforming effect '{effectName}': {ex.Message}");
            }
        }
        
        /// <summary>
        /// Supprime un effet de terraformation personnalisé
        /// </summary>
        /// <param name="effectName">Nom de l'effet à supprimer</param>
        public void RemoveEffect(string effectName)
        {
            if (!_isActive)
            {
                Log.Warning("Cannot remove terraforming effect: control not active");
                return;
            }
            
            try
            {
                var overrides = TerraformingEffectsPatches.GetTerraformingOverrides(_nativePlanet);
                if (overrides != null && overrides.CustomEffects.ContainsKey(effectName))
                {
                    var oldValue = overrides.CustomEffects[effectName];
                    overrides.CustomEffects.Remove(effectName);
                    Log.Info($"Removed terraforming effect '{effectName}' ({oldValue:+0.00;-0.00;0}K)");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to remove terraforming effect '{effectName}': {ex.Message}");
            }
        }
        
        /// <summary>
        /// Surcharge un effet vanilla existant (polar nukes, dust, comet, deimos)
        /// </summary>
        /// <param name="vanillaEffect">Type d'effet vanilla ("PolarNuke", "PolarDust", "Comet", "Deimos")</param>
        /// <param name="newValue">Nouvelle valeur d'effet en Kelvin</param>
        /// <param name="source">Source de la modification</param>
        public void OverrideVanillaEffect(string vanillaEffect, float newValue, string source = "Custom")
        {
            if (!_isActive)
            {
                Log.Warning("Cannot override vanilla effect: control not active");
                return;
            }
            
            try
            {
                TerraformingEffectsPatches.OverrideVanillaTerraformingEffect(_nativePlanet, vanillaEffect, newValue);
                Log.Info($"Overridden vanilla effect '{vanillaEffect}': {newValue:+0.00;-0.00;0}K from {source}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to override vanilla effect '{vanillaEffect}': {ex.Message}");
            }
        }
        
        /// <summary>
        /// Obtient l'effet total des terraformations personnalisées
        /// </summary>
        public float GetTotalCustomEffect()
        {
            if (!_isActive) return 0f;
            
            try
            {
                return TerraformingEffectsPatches.GetTotalTerraformingEffect(_nativePlanet);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get total terraforming effect: {ex.Message}");
                return 0f;
            }
        }
        
        /// <summary>
        /// Crée un effet de "vague de chaleur" temporaire (bon pour Twitch)
        /// </summary>
        /// <param name="intensity">Intensité (1.0 = +5K, 2.0 = +10K, etc.)</param>
        /// <param name="source">Source de l'événement</param>
        public void CreateHeatWave(float intensity = 1.0f, string source = "Heat Wave Event")
        {
            float temperatureBoost = 5.0f * intensity; // 5K par unité d'intensité
            AddTempEffect("HeatWave", temperatureBoost, source);
        }
        
        /// <summary>
        /// Crée un effet de "refroidissement" temporaire (bon pour Twitch)
        /// </summary>
        /// <param name="intensity">Intensité (1.0 = -5K, 2.0 = -10K, etc.)</param>
        /// <param name="source">Source de l'événement</param>
        public void CreateColdSnap(float intensity = 1.0f, string source = "Cold Snap Event")
        {
            float temperatureReduction = -5.0f * intensity; // -5K par unité d'intensité
            AddTempEffect("ColdSnap", temperatureReduction, source);
        }
        
        /// <summary>
        /// Crée un effet de "tempête solaire" (boost temporaire de température)
        /// </summary>
        /// <param name="intensity">Intensité de la tempête</param>
        /// <param name="source">Source de l'événement</param>
        public void CreateSolarStorm(float intensity = 1.0f, string source = "Solar Storm Event")
        {
            float temperatureBoost = 8.0f * intensity; // 8K par unité d'intensité
            AddTempEffect("SolarStorm", temperatureBoost, source);
        }
        
        /// <summary>
        /// Simule un impact de météorite (changement de température aléatoire)
        /// </summary>
        /// <param name="source">Source de l'événement</param>
        public void CreateMeteorImpact(string source = "Meteor Impact Event")
        {
            var random = new Random();
            float temperatureChange = (float)(random.NextDouble() * 20.0 - 10.0); // Entre -10K et +10K
            AddTempEffect("MeteorImpact", temperatureChange, source);
            Log.Info($"Meteor impact: {temperatureChange:+0.00;-0.00;0}K temperature change");
        }
        
        /// <summary>
        /// Obtient le statut des effets actifs
        /// </summary>
        public string GetStatus()
        {
            if (!_isActive) return "Terraforming Effects: INACTIVE";
            
            try
            {
                var overrides = TerraformingEffectsPatches.GetTerraformingOverrides(_nativePlanet);
                if (overrides == null) return "Terraforming Effects: ACTIVE (no effects)";
                
                var customCount = overrides.CustomEffects.Count;
                var vanillaCount = overrides.VanillaOverrides.Count;
                var totalEffect = GetTotalCustomEffect();
                
                return $"Terraforming Effects: ACTIVE | Custom: {customCount} effects | Vanilla overrides: {vanillaCount} | Total effect: {totalEffect:+0.00;-0.00;0}K";
            }
            catch (Exception ex)
            {
                return $"Terraforming Effects: ERROR - {ex.Message}";
            }
        }
    }
}