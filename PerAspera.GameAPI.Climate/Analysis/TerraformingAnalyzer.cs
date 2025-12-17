using System;
using PerAspera.Core;
using System.Collections.Generic;
using PerAspera.GameAPI.Climate.Configuration;
using PerAspera.GameAPI.Wrappers;
using PerAspera.GameAPI.Wrappers.Atmosphere = PerAspera.GameAPI.Wrappers.PerAspera.GameAPI.Wrappers.Atmosphere;
using PerAspera.GameAPI.Wrappers.Planet = PerAspera.GameAPI.Wrappers.PerAspera.GameAPI.Wrappers.Planet;

namespace PerAspera.GameAPI.Climate.Analysis
{
    /// <summary>
    /// Analyzes terraforming progress towards Earth-like conditions
    /// Tracks atmospheric transformation from current Mars state to habitable environment
    /// </summary>
    public class TerraformingAnalyzer
    {
        private static readonly LogAspera Log = new LogAspera("Climate.TerraformingAnalyzer");
        private readonly ClimateConfig _config;
        
        public TerraformingAnalyzer(ClimateConfig? config = null)
        {
            _config = config ?? ClimateConfig.CreateRealistic();
        }
        
        /// <summary>
        /// Calculate overall terraforming progress (0-100%)
        /// Weighted average of temperature, pressure, and atmospheric composition goals
        /// </summary>
        public float CalculateTerraformingProgress(PerAspera.GameAPI.Wrappers.Atmosphere PerAspera.GameAPI.Wrappers.Atmosphere)
        {
            if (PerAspera.GameAPI.Wrappers.Atmosphere?.Composition == null)
                return 0f;
            
            var breakdown = AnalyzeTerraformingBreakdown(PerAspera.GameAPI.Wrappers.Atmosphere);
            
            // Terraforming weights: all factors important but pressure is critical foundation
            var weights = new Dictionary<string, float>
            {
                ["pressure"] = 0.30f,     // Foundation for liquid water
                ["oxygen"] = 0.25f,       // Essential for life
                ["temperature"] = 0.25f,  // Climate stability
                ["nitrogen"] = 0.20f      // Atmospheric bulk and stability
            };
            
            var progress = 0f;
            foreach (var factor in breakdown)
            {
                if (weights.TryGetValue(factor.Key, out var weight))
                {
                    progress += factor.Value * weight;
                }
            }
            
            return Math.Max(0f, Math.Min(100f, progress));
        }
        
        /// <summary>
        /// Detailed breakdown of terraforming progress by category
        /// Returns percentage completion for each terraforming goal
        /// </summary>
        public Dictionary<string, float> AnalyzeTerraformingBreakdown(PerAspera.GameAPI.Wrappers.Atmosphere PerAspera.GameAPI.Wrappers.Atmosphere)
        {
            var results = new Dictionary<string, float>();
            
            if (PerAspera.GameAPI.Wrappers.Atmosphere?.Composition == null)
            {
                return new Dictionary<string, float>
                {
                    ["pressure"] = 0f,
                    ["oxygen"] = 0f,
                    ["temperature"] = 0f,
                    ["nitrogen"] = 0f
                };
            }
            
            // Pressure progress (Mars → Earth)
            results["pressure"] = CalculatePressureProgress(PerAspera.GameAPI.Wrappers.Atmosphere);
            
            // Oxygen progress (None → Earth levels)
            results["oxygen"] = CalculateOxygenProgress(PerAspera.GameAPI.Wrappers.Atmosphere);
            
            // Temperature progress (Cold Mars → Temperate Earth)
            results["temperature"] = CalculateTemperatureProgress(PerAspera.GameAPI.Wrappers.Atmosphere);
            
            // Nitrogen progress (None → Earth levels)
            results["nitrogen"] = CalculateNitrogenProgress(PerAspera.GameAPI.Wrappers.Atmosphere);
            
            return results;
        }
        
        /// <summary>
        /// Calculate atmospheric pressure terraforming progress
        /// Target: Mars baseline (0.636 kPa) → Earth level (101.325 kPa)
        /// </summary>
        private float CalculatePressureProgress(PerAspera.GameAPI.Wrappers.Atmosphere PerAspera.GameAPI.Wrappers.Atmosphere)
        {
            var currentPressure = PerAspera.GameAPI.Wrappers.Atmosphere.TotalPressure;
            var marsBaseline = TerraformingConstants.PRESSURE_MARS_CURRENT * 101.325f; // Convert atm to kPa
            var earthTarget = TerraformingConstants.PRESSURE_EARTH * 101.325f; // Convert atm to kPa
            
            if (currentPressure <= marsBaseline)
                return 0f;
            
            if (currentPressure >= earthTarget)
                return 100f;
                
            var progress = (currentPressure - marsBaseline) / (earthTarget - marsBaseline);
            return progress * 100f;
        }
        
        /// <summary>
        /// Calculate oxygen terraforming progress
        /// Target: ~0 kPa → 21.28 kPa (Earth level)
        /// </summary>
        private float CalculateOxygenProgress(PerAspera.GameAPI.Wrappers.Atmosphere PerAspera.GameAPI.Wrappers.Atmosphere)
        {
            var currentO2 = PerAspera.GameAPI.Wrappers.Atmosphere.Composition["O2"]?.PartialPressure ?? 0f?.PartialPressure ?? 0f;
            var marsBaseline = 0.0001f; // Mars has virtually no O2
            var earthTarget = TerraformingConstants.O2_EARTH_LEVEL;
            
            if (currentO2 <= marsBaseline)
                return 0f;
                
            if (currentO2 >= earthTarget)
                return 100f;
                
            var progress = (currentO2 - marsBaseline) / (earthTarget - marsBaseline);
            return progress * 100f;
        }
        
        /// <summary>
        /// Calculate temperature terraforming progress
        /// Target: Mars cold (210K) → Earth temperate (288K)
        /// </summary>
        private float CalculateTemperatureProgress(PerAspera.GameAPI.Wrappers.Atmosphere PerAspera.GameAPI.Wrappers.Atmosphere)
        {
            var currentTemp = PerAspera.GameAPI.Wrappers.Atmosphere.Temperature;
            var marsBaseline = TerraformingConstants.TEMP_MARS_CURRENT;
            var earthTarget = TerraformingConstants.TEMP_OPTIMAL;
            
            if (currentTemp <= marsBaseline)
                return 0f;
                
            if (currentTemp >= earthTarget)
                return 100f;
                
            var progress = (currentTemp - marsBaseline) / (earthTarget - marsBaseline);
            return progress * 100f;
        }
        
        /// <summary>
        /// Calculate nitrogen terraforming progress
        /// Target: ~0 kPa → 79.03 kPa (Earth level)
        /// </summary>
        private float CalculateNitrogenProgress(PerAspera.GameAPI.Wrappers.Atmosphere PerAspera.GameAPI.Wrappers.Atmosphere)
        {
            var currentN2 = PerAspera.GameAPI.Wrappers.Atmosphere.Composition["N2"]?.PartialPressure ?? 0f?.PartialPressure ?? 0f;
            var marsBaseline = 0f; // Mars has virtually no nitrogen
            var earthTarget = 78.0f; // Earth N2 percentage
            
            if (currentN2 <= marsBaseline)
                return 0f;
                
            if (currentN2 >= earthTarget)
                return 100f;
                
            var progress = currentN2 / earthTarget;
            return progress * 100f;
        }
        
        /// <summary>
        /// Get human-readable terraforming status summary
        /// </summary>
        public string GetTerraformingStatusSummary(PerAspera.GameAPI.Wrappers.Atmosphere PerAspera.GameAPI.Wrappers.Atmosphere)
        {
            var progress = CalculateTerraformingProgress(PerAspera.GameAPI.Wrappers.Atmosphere);
            var breakdown = AnalyzeTerraformingBreakdown(PerAspera.GameAPI.Wrappers.Atmosphere);
            
            var phase = progress switch
            {
                < 10f => "Early Stage - Basic infrastructure development",
                < 30f => "Foundation Phase - Atmospheric thickening in progress",
                < 50f => "Development Phase - Significant atmospheric changes",
                < 75f => "Advanced Phase - Approaching habitable conditions",
                < 90f => "Final Phase - Fine-tuning for Earth-like conditions",
                _ => "Complete - Fully terraformed environment achieved"
            };
            
            var weakestArea = "";
            var lowestProgress = 100f;
            foreach (var kvp in breakdown)
            {
                if (kvp.Value < lowestProgress)
                {
                    lowestProgress = kvp.Value;
                    weakestArea = kvp.Key;
                }
            }
            
            var bottleneck = lowestProgress < 50f ? $" (bottleneck: {weakestArea})" : "";
            
            return $"{phase} - Overall Progress: {progress:F1}%{bottleneck}";
        }
    }
}










