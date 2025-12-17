using System;
using PerAspera.Core;
using PerAspera.GameAPI.Climate.Configuration;
using PerAspera.GameAPI.Wrappers;
using PerAspera.GameAPI.Wrappers.Atmosphere = PerAspera.GameAPI.Wrappers.PerAspera.GameAPI.Wrappers.Atmosphere;
using PerAspera.GameAPI.Wrappers.Planet = PerAspera.GameAPI.Wrappers.PerAspera.GameAPI.Wrappers.Planet;

namespace PerAspera.GameAPI.Climate.Simulation.Models
{
    /// <summary>
    /// Temperature equilibrium calculation model for Mars
    /// Balances solar input with radiative cooling and greenhouse warming
    /// </summary>
    public class TemperatureModel
    {
        private static readonly LogAspera Log = new LogAspera("Climate.TemperatureModel");
        private readonly ClimateConfig _config;
        
        public TemperatureModel(ClimateConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }
        
        /// <summary>
        /// Calculate equilibrium temperature with greenhouse effect
        /// T_eq = T_baseline + greenhouse_warming
        /// </summary>
        public float CalculateEquilibriumTemperature(PerAspera.GameAPI.Wrappers.Atmosphere PerAspera.GameAPI.Wrappers.Atmosphere, float greenhouseWarming)
        {
            if (PerAspera.GameAPI.Wrappers.Atmosphere == null)
            {
                Log.Warning("Cannot calculate temperature: null PerAspera.GameAPI.Wrappers.Atmosphere");
                return TerraformingConstants.TEMP_MARS_CURRENT;
            }
            
            try
            {
                // Base temperature from solar heating (simplified Stefan-Boltzmann)
                var baseTemperature = CalculateBaseSolarTemperature();
                
                // Apply greenhouse warming
                var equilibriumTemp = baseTemperature + greenhouseWarming;
                
                // Apply realistic bounds
                equilibriumTemp = Math.Max(equilibriumTemp, 150f); // Minimum ~-123°C
                equilibriumTemp = Math.Min(equilibriumTemp, 400f); // Maximum ~127°C
                
                Log.Debug($"Temperature calc: Base={baseTemperature:F1}K, +GH={greenhouseWarming:F1}K → {equilibriumTemp:F1}K");
                return equilibriumTemp;
            }
            catch (Exception ex)
            {
                Log.Error($"Temperature calculation failed: {ex.Message}");
                return PerAspera.GameAPI.Wrappers.Atmosphere.Temperature; // Return current as fallback
            }
        }
        
        /// <summary>
        /// Calculate base solar temperature without greenhouse effect
        /// Uses Mars solar constant and simple blackbody radiation
        /// </summary>
        private float CalculateBaseSolarTemperature()
        {
            // Stefan-Boltzmann: T = (S/4σ)^(1/4) where S = solar constant, σ = Stefan-Boltzmann constant
            var stefanBoltzmann = 5.67e-8; // W⋅m⁻²⋅K⁻⁴
            var effectiveSolar = _config.SolarConstant / 4.0; // Factor of 4 for spherical geometry
            
            var baseTemp = Math.Pow(effectiveSolar / stefanBoltzmann, 0.25);
            return (float)baseTemp;
        }
        
        /// <summary>
        /// Calculate thermal inertia effect for gradual temperature change
        /// Prevents instant temperature jumps - Mars has low thermal inertia
        /// </summary>
        public float ApplyThermalInertia(float currentTemp, float targetTemp, float deltaTime)
        {
            // Mars thermal inertia coefficient (lower = faster temperature change)
            var marsInertia = 0.15f; // Faster response than Earth due to thin PerAspera.GameAPI.Wrappers.Atmosphere
            var timeConstant = 24f; // Hours for significant change
            
            var maxChange = Math.Abs(targetTemp - currentTemp) * marsInertia * (deltaTime / timeConstant);
            var actualChange = Math.Sign(targetTemp - currentTemp) * Math.Min(maxChange, Math.Abs(targetTemp - currentTemp));
            
            return currentTemp + actualChange;
        }
        
        /// <summary>
        /// Calculate radiative cooling rate (W/m²)
        /// </summary>
        public float CalculateRadiativeCooling(float temperature)
        {
            var stefanBoltzmann = 5.67e-8f;
            var emissivity = 0.95f; // Mars surface emissivity
            
            return emissivity * stefanBoltzmann * (float)Math.Pow(temperature, 4);
        }
    }
}





