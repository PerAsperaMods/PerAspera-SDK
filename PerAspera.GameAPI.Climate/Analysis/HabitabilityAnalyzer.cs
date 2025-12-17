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
    /// Analyzes atmospheric conditions to determine habitability for human life
    /// Considers multiple factors: pressure, temperature, oxygen levels, toxicity
    /// </summary>
    public class HabitabilityAnalyzer
    {
        private static readonly LogAspera Log = new LogAspera("Climate.HabitabilityAnalyzer");
        private readonly ClimateConfig _config;
        
        public HabitabilityAnalyzer(ClimateConfig? config = null)
        {
            _config = config ?? ClimateConfig.CreateRealistic();
        }
        
        /// <summary>
        /// Calculate overall habitability score (0-100%)
        /// Combines multiple environmental factors with weighted importance
        /// </summary>
        public float CalculateHabitabilityScore(PerAspera.GameAPI.Wrappers.Atmosphere PerAspera.GameAPI.Wrappers.Atmosphere)
        {
            if (PerAspera.GameAPI.Wrappers.Atmosphere?.Composition == null)
                return 0f;
                
            var breakdown = AnalyzeHabitabilityBreakdown(PerAspera.GameAPI.Wrappers.Atmosphere);
            
            // Weighted scoring: all factors must be reasonable for habitability
            var weights = new Dictionary<string, float>
            {
                ["oxygen"] = 0.35f,      // Critical for breathing
                ["pressure"] = 0.25f,    // Needed for liquid water and breathing
                ["temperature"] = 0.25f, // Human comfort zone
                ["toxicity"] = 0.15f     // CO2 poisoning, etc.
            };
            
            var score = 0f;
            foreach (var factor in breakdown)
            {
                if (weights.TryGetValue(factor.Key, out var weight))
                {
                    score += factor.Value * weight;
                }
            }
            
            return Math.Max(0f, Math.Min(100f, score));
        }
        
        /// <summary>
        /// Detailed breakdown of habitability factors
        /// Returns individual scores for analysis
        /// </summary>
        public Dictionary<string, float> AnalyzeHabitabilityBreakdown(PerAspera.GameAPI.Wrappers.Atmosphere PerAspera.GameAPI.Wrappers.Atmosphere)
        {
            var results = new Dictionary<string, float>();
            
            if (PerAspera.GameAPI.Wrappers.Atmosphere?.Composition == null)
            {
                return new Dictionary<string, float>
                {
                    ["oxygen"] = 0f,
                    ["pressure"] = 0f,
                    ["temperature"] = 0f,
                    ["toxicity"] = 0f
                };
            }
            
            // Oxygen score (0-100%)
            results["oxygen"] = CalculateOxygenScore(PerAspera.GameAPI.Wrappers.Atmosphere);
            
            // Pressure score (0-100%)
            results["pressure"] = CalculatePressureScore(PerAspera.GameAPI.Wrappers.Atmosphere);
            
            // Temperature score (0-100%)
            results["temperature"] = CalculateTemperatureScore(PerAspera.GameAPI.Wrappers.Atmosphere);
            
            // Toxicity score (0-100%, higher is better)
            results["toxicity"] = CalculateToxicityScore(PerAspera.GameAPI.Wrappers.Atmosphere);
            
            return results;
        }
        
        /// <summary>
        /// Calculate oxygen adequacy score
        /// Based on partial pressure of O2 vs. human requirements
        /// </summary>
        private float CalculateOxygenScore(PerAspera.GameAPI.Wrappers.Atmosphere PerAspera.GameAPI.Wrappers.Atmosphere)
        {
            var o2Pressure = PerAspera.GameAPI.Wrappers.Atmosphere.Composition["O2"]?.PartialPressure ?? 0f?.PartialPressure ?? 0f;
            
            if (o2Pressure <= 0f) return 0f;
            
            // Human needs: minimum 16 kPa, optimal 21 kPa (Earth level)
            var minBreathable = TerraformingConstants.O2_MIN_BREATHABLE;
            var earthLevel = TerraformingConstants.O2_EARTH_LEVEL;
            
            if (o2Pressure < minBreathable)
            {
                // Below breathable threshold
                return (o2Pressure / minBreathable) * 50f; // Max 50% score
            }
            else if (o2Pressure <= earthLevel)
            {
                // Between breathable and Earth-optimal
                var ratio = (o2Pressure - minBreathable) / (earthLevel - minBreathable);
                return 50f + (ratio * 50f); // 50-100% score
            }
            else
            {
                // Above Earth level - oxygen toxicity concerns
                var excess = o2Pressure - earthLevel;
                var toxicityPenalty = Math.Min(excess * 2f, 50f); // Max 50% penalty
                return Math.Max(50f, 100f - toxicityPenalty);
            }
        }
        
        /// <summary>
        /// Calculate atmospheric pressure adequacy score
        /// </summary>
        private float CalculatePressureScore(PerAspera.GameAPI.Wrappers.Atmosphere PerAspera.GameAPI.Wrappers.Atmosphere)
        {
            var totalPressure = PerAspera.GameAPI.Wrappers.Atmosphere.TotalPressure;
            
            var minPressure = TerraformingConstants.PRESSURE_LIQUID_WATER * 101.325f; // Convert atm to kPa
            var earthPressure = TerraformingConstants.PRESSURE_EARTH * 101.325f; // Convert atm to kPa
            
            if (totalPressure < minPressure)
            {
                // Below liquid water threshold
                return (totalPressure / minPressure) * 30f; // Max 30% score
            }
            else if (totalPressure < earthPressure * 0.5f)
            {
                // Low but livable pressure
                var ratio = totalPressure / (earthPressure * 0.5f);
                return 30f + (ratio * 50f); // 30-80% score
            }
            else if (totalPressure <= earthPressure * 1.5f)
            {
                // Good pressure range
                return 80f + ((1f - Math.Abs(totalPressure - earthPressure) / earthPressure) * 20f);
            }
            else
            {
                // High pressure - decompression sickness risks
                var excess = totalPressure - (earthPressure * 1.5f);
                var penalty = Math.Min(excess / 50f * 50f, 60f);
                return Math.Max(20f, 80f - penalty);
            }
        }
        
        /// <summary>
        /// Calculate temperature habitability score
        /// </summary>
        private float CalculateTemperatureScore(PerAspera.GameAPI.Wrappers.Atmosphere PerAspera.GameAPI.Wrappers.Atmosphere)
        {
            var tempK = PerAspera.GameAPI.Wrappers.Atmosphere.Temperature;
            var tempC = tempK - 273.15f;
            
            // Optimal human temperature range: 15-25°C (288-298K)
            var optimalMin = 288.15f;
            var optimalMax = 298.15f;
            var optimalRange = optimalMax - optimalMin;
            
            if (tempK >= optimalMin && tempK <= optimalMax)
            {
                // Perfect temperature range
                return 100f;
            }
            else if (tempK >= 273.15f && tempK <= 323.15f) // 0-50°C
            {
                // Livable but not optimal
                if (tempK < optimalMin)
                {
                    var coldOffset = optimalMin - tempK;
                    var maxCold = optimalMin - 273.15f; // 15°C range
                    return 70f + (1f - coldOffset / maxCold) * 30f;
                }
                else
                {
                    var hotOffset = tempK - optimalMax;
                    var maxHot = 323.15f - optimalMax; // 25°C range
                    return 70f + (1f - hotOffset / maxHot) * 30f;
                }
            }
            else if (tempK >= 253.15f && tempK <= 353.15f) // -20 to 80°C
            {
                // Survivable with protection
                var distance = Math.Min(Math.Abs(tempK - optimalMin), Math.Abs(tempK - optimalMax));
                var maxDistance = 80f; // Kelvin
                return Math.Max(20f, 70f - (distance / maxDistance) * 50f);
            }
            else
            {
                // Extreme temperatures
                return Math.Max(0f, 20f - Math.Abs(tempC) / 10f);
            }
        }
        
        /// <summary>
        /// Calculate toxicity score (atmospheric poisoning)
        /// </summary>
        private float CalculateToxicityScore(PerAspera.GameAPI.Wrappers.Atmosphere PerAspera.GameAPI.Wrappers.Atmosphere)
        {
            var co2Pressure = PerAspera.GameAPI.Wrappers.Atmosphere.Composition["CO2"]?.PartialPressure ?? 0f?.PartialPressure ?? 0f;
            
            // CO2 toxicity levels (kPa):
            // < 0.5: Safe
            // 0.5-4: Mild effects
            // 4-7: Drowsiness
            // > 7: Dangerous
            
            if (co2Pressure <= 0.5f)
            {
                return 100f; // No toxicity concerns
            }
            else if (co2Pressure <= 4f)
            {
                var ratio = (co2Pressure - 0.5f) / 3.5f;
                return 100f - (ratio * 30f); // 70-100% score
            }
            else if (co2Pressure <= 7f)
            {
                var ratio = (co2Pressure - 4f) / 3f;
                return 70f - (ratio * 50f); // 20-70% score
            }
            else
            {
                // Dangerous CO2 levels
                var excess = co2Pressure - 7f;
                var penalty = Math.Min(excess * 5f, 20f);
                return Math.Max(0f, 20f - penalty);
            }
        }
        
        /// <summary>
        /// Get human-readable habitability assessment
        /// </summary>
        public string GetHabitabilityAssessment(PerAspera.GameAPI.Wrappers.Atmosphere PerAspera.GameAPI.Wrappers.Atmosphere)
        {
            var score = CalculateHabitabilityScore(PerAspera.GameAPI.Wrappers.Atmosphere);
            var breakdown = AnalyzeHabitabilityBreakdown(PerAspera.GameAPI.Wrappers.Atmosphere);
            
            var rating = score switch
            {
                >= 90f => "Excellent - Earth-like conditions",
                >= 75f => "Good - Habitable with minimal protection", 
                >= 50f => "Moderate - Requires life support systems",
                >= 25f => "Poor - Survival possible with heavy protection",
                _ => "Hostile - Unsurvivable without full environmental suits"
            };
            
            var issues = new List<string>();
            if (breakdown["oxygen"] < 50f) issues.Add("insufficient oxygen");
            if (breakdown["pressure"] < 50f) issues.Add("low pressure");
            if (breakdown["temperature"] < 50f) issues.Add("extreme temperature");
            if (breakdown["toxicity"] < 50f) issues.Add("atmospheric toxicity");
            
            var issuesText = issues.Count > 0 ? $" (Issues: {string.Join(", ", issues)})" : "";
            
            return $"{rating} - Score: {score:F1}%{issuesText}";
        }
    }
}





