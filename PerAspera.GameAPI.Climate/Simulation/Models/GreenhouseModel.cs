using System;
using PerAspera.Core;
using PerAspera.GameAPI.Climate.Configuration;
using PerAspera.GameAPI.Wrappers;
using PerAspera.GameAPI.Wrappers.Atmosphere = PerAspera.GameAPI.Wrappers.PerAspera.GameAPI.Wrappers.Atmosphere;
using PerAspera.GameAPI.Wrappers.Planet = PerAspera.GameAPI.Wrappers.PerAspera.GameAPI.Wrappers.Planet;

namespace PerAspera.GameAPI.Climate.Simulation.Models
{
    /// <summary>
    /// Greenhouse effect calculation model for Mars atmospheric warming
    /// Implements realistic radiative forcing from CO2 and H2O
    /// </summary>
    public class GreenhouseModel
    {
        private static readonly LogAspera Log = new LogAspera("Climate.GreenhouseModel");
        private readonly ClimateConfig _config;
        
        public GreenhouseModel(ClimateConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }
        
        /// <summary>
        /// Calculate total greenhouse warming from atmospheric composition
        /// Uses logarithmic CO2 response and exponential H2O response
        /// </summary>
        public float CalculateGreenhouseWarming(PerAspera.GameAPI.Wrappers.Atmosphere PerAspera.GameAPI.Wrappers.Atmosphere)
        {
            if (PerAspera.GameAPI.Wrappers.Atmosphere?.Composition == null)
            {
                Log.Warning("Cannot calculate greenhouse effect: null PerAspera.GameAPI.Wrappers.Atmosphere");
                return 0f;
            }
            
            try
            {
                // Get gas partial pressures
                var co2Pressure = PerAspera.GameAPI.Wrappers.Atmosphere.Composition["CO2"]?.PartialPressure ?? 0f?.PartialPressure ?? 0f;
                var h2oPressure = PerAspera.GameAPI.Wrappers.Atmosphere.Composition["H2O"]?.PartialPressure ?? 0f?.PartialPressure ?? 0f;
                
                // CO2 greenhouse effect (logarithmic response - realistic)
                var co2Warming = CalculateCO2Warming(co2Pressure);
                
                // H2O greenhouse effect (exponential response - strong feedback)
                var h2oWarming = CalculateH2OWarming(h2oPressure);
                
                // Total warming with saturation limit
                var totalWarming = co2Warming + h2oWarming;
                totalWarming = Math.Min(totalWarming, _config.MaxGreenhouseWarming);
                
                Log.Debug($"Greenhouse: CO2={co2Warming:F1}K, H2O={h2oWarming:F1}K, Total={totalWarming:F1}K");
                return totalWarming;
            }
            catch (Exception ex)
            {
                Log.Error($"Greenhouse calculation failed: {ex.Message}");
                return 0f;
            }
        }
        
        /// <summary>
        /// CO2 greenhouse warming using logarithmic relationship
        /// ΔT = efficiency * ln(P_CO2 / P_baseline)
        /// </summary>
        private float CalculateCO2Warming(float co2Pressure)
        {
            var marsBaselineCO2 = 0.6f; // Mars baseline CO2 pressure in kPa
            if (co2Pressure <= marsBaselineCO2) return 0f;
            
            var ratio = co2Pressure / marsBaselineCO2;
            var warming = _config.CO2GreenhouseEfficiency * 5.35f * (float)Math.Log(ratio);
            
            return Math.Max(0f, warming); // No cooling below baseline
        }
        
        /// <summary>
        /// H2O vapor greenhouse warming using exponential relationship
        /// Water vapor provides strong positive feedback
        /// </summary>
        private float CalculateH2OWarming(float h2oPressure)
        {
            if (h2oPressure <= 0f) return 0f;
            
            // H2O has exponential greenhouse effect (feedback mechanism)
            var warming = _config.H2OGreenhouseEfficiency * h2oPressure * 2.8f;
            
            return Math.Max(0f, warming);
        }
        
        /// <summary>
        /// Calculate radiative forcing (W/m²) for diagnostic purposes
        /// </summary>
        public float CalculateRadiativeForcing(PerAspera.GameAPI.Wrappers.Atmosphere PerAspera.GameAPI.Wrappers.Atmosphere)
        {
            if (PerAspera.GameAPI.Wrappers.Atmosphere?.Composition == null) return 0f;
            
            var co2Pressure = PerAspera.GameAPI.Wrappers.Atmosphere.Composition["CO2"]?.PartialPressure ?? 0f?.PartialPressure ?? 0f;
            var h2oPressure = PerAspera.GameAPI.Wrappers.Atmosphere.Composition["H2O"]?.PartialPressure ?? 0f?.PartialPressure ?? 0f;
            
            // Convert greenhouse warming to radiative forcing (Stefan-Boltzmann)
            var greenhouse = CalculateGreenhouseWarming(PerAspera.GameAPI.Wrappers.Atmosphere);
            var forcing = 4 * 5.67e-8f * (float)Math.Pow(PerAspera.GameAPI.Wrappers.Atmosphere.Temperature, 3) * greenhouse;
            
            return forcing; // W/m²
        }
    }
}





