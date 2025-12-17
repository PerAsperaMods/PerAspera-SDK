using System;
using PerAspera.Core;
using PerAspera.GameAPI.Climate.Configuration;
using PerAspera.GameAPI.Wrappers;
using PerAspera.GameAPI.Wrappers.Atmosphere = PerAspera.GameAPI.Wrappers.PerAspera.GameAPI.Wrappers.Atmosphere;
using PerAspera.GameAPI.Wrappers.Planet = PerAspera.GameAPI.Wrappers.PerAspera.GameAPI.Wrappers.Planet;

namespace PerAspera.GameAPI.Climate.Simulation.Models
{
    /// <summary>
    /// Atmospheric pressure dynamics model for Mars
    /// Handles pressure changes from gas additions/removals
    /// </summary>
    public class PressureModel
    {
        private static readonly LogAspera Log = new LogAspera("Climate.PressureModel");
        private readonly ClimateConfig _config;
        
        public PressureModel(ClimateConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }
        
        /// <summary>
        /// Calculate pressure change over time
        /// Currently returns 0 - pressure changes are primarily driven by external factors
        /// </summary>
        public float CalculatePressureChange(PerAspera.GameAPI.Wrappers.Atmosphere PerAspera.GameAPI.Wrappers.Atmosphere, float deltaTime)
        {
            if (PerAspera.GameAPI.Wrappers.Atmosphere?.Composition == null)
            {
                Log.Warning("Cannot calculate pressure change: null PerAspera.GameAPI.Wrappers.Atmosphere");
                return 0f;
            }
            
            try
            {
                // For now, pressure changes are primarily external (buildings adding/removing gases)
                // This could be extended to include:
                // - Seasonal CO2 sublimation/freezing
                // - Water vapor condensation/evaporation
                // - Gas escape to space
                
                Log.Debug("Pressure change calculation (currently passive)");
                return 0f; // No autonomous pressure changes
            }
            catch (Exception ex)
            {
                Log.Error($"Pressure calculation failed: {ex.Message}");
                return 0f;
            }
        }
        
        /// <summary>
        /// Calculate new pressure when adding gas to PerAspera.GameAPI.Wrappers.Atmosphere
        /// Uses ideal gas law: PV = nRT
        /// </summary>
        public float CalculatePressureAfterGasAddition(PerAspera.GameAPI.Wrappers.Atmosphere PerAspera.GameAPI.Wrappers.Atmosphere, string gasType, float amountKg)
        {
            if (PerAspera.GameAPI.Wrappers.Atmosphere?.Composition == null || amountKg <= 0f) return PerAspera.GameAPI.Wrappers.Atmosphere?.TotalPressure ?? 0f;
            
            try
            {
                var molarMass = GetMolarMass(gasType);
                var gasConstant = 8.314f; // J/(mol⋅K)
                var temperature = PerAspera.GameAPI.Wrappers.Atmosphere.Temperature;
                var planetSurface = 4f * (float)Math.PI * Math.Pow(_config.PlanetRadius * 1000, 2); // m²
                
                // Convert mass to moles
                var moles = amountKg * 1000f / molarMass; // kg → g → mol
                
                // Calculate pressure change: ΔP = (nRT)/(A⋅h) where h = scale height
                var scaleHeight = CalculateScaleHeight(temperature, molarMass);
                var pressureChange = (moles * gasConstant * temperature) / (planetSurface * scaleHeight);
                
                // Convert Pa to kPa
                pressureChange /= 1000f;
                
                Log.Debug($"Adding {amountKg}kg {gasType}: +{pressureChange:F4}kPa");
                return PerAspera.GameAPI.Wrappers.Atmosphere.TotalPressure + pressureChange;
            }
            catch (Exception ex)
            {
                Log.Error($"Gas addition calculation failed: {ex.Message}");
                return PerAspera.GameAPI.Wrappers.Atmosphere.TotalPressure;
            }
        }
        
        /// <summary>
        /// Calculate atmospheric scale height for gas distribution
        /// H = RT/(mg) where g = surface gravity
        /// </summary>
        private float CalculateScaleHeight(float temperature, float molarMass)
        {
            var gasConstant = 8.314f; // J/(mol⋅K)
            var gravity = _config.SurfaceGravity; // m/s²
            
            var scaleHeight = (gasConstant * temperature) / (molarMass * 0.001f * gravity); // mol mass in kg
            return scaleHeight; // meters
        }
        
        /// <summary>
        /// Get molar mass for atmospheric gases
        /// </summary>
        private float GetMolarMass(string gasType)
        {
            return gasType.ToUpper() switch
            {
                "CO2" => _config.CO2MolarMass,
                "O2" => _config.O2MolarMass,
                "N2" => _config.N2MolarMass,
                "H2O" => _config.H2OMolarMass,
                _ => 28.97f // Average air molar mass as fallback
            };
        }
        
        /// <summary>
        /// Calculate pressure at altitude using barometric formula
        /// P(h) = P₀ * exp(-h/H) where H = scale height
        /// </summary>
        public float CalculatePressureAtAltitude(PerAspera.GameAPI.Wrappers.Atmosphere PerAspera.GameAPI.Wrappers.Atmosphere, float altitudeKm)
        {
            if (PerAspera.GameAPI.Wrappers.Atmosphere == null) return 0f;
            
            var averageMolarMass = 44f; // Mostly CO2 for Mars
            var scaleHeight = CalculateScaleHeight(PerAspera.GameAPI.Wrappers.Atmosphere.Temperature, averageMolarMass) / 1000f; // km
            
            var pressureRatio = Math.Exp(-altitudeKm / scaleHeight);
            return PerAspera.GameAPI.Wrappers.Atmosphere.TotalPressure * (float)pressureRatio;
        }
    }
}





