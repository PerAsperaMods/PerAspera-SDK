using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerAspera.GameAPI.Climate.Domain
{
    /// <summary>
    /// Represents a polar region with area-specific climate calculations
    /// Instead of average calculations, this provides regional climate modeling
    /// </summary>
    public class Pole
    {
        public enum PoleType
        {
            North,
            South
        }

        public PoleType Type { get; set; }

        // Geographic properties
        public float Latitude { get; private set; } // Degrees from equator
        public float SurfaceArea { get; private set; } // km²
        public float IceCapArea { get; private set; } // km² of ice/snow cover
        public float Albedo { get; private set; } // Reflectivity (0-1)

        // Temperature properties (in Kelvin)
        private float _surfaceTemperature;
        private float _iceTemperature;
        private float _atmosphericTemperature;

        // Heat transfer coefficients
        private const float ICE_HEAT_CAPACITY = 2.1f; // MJ/m³/K (ice)
        private const float SOIL_HEAT_CAPACITY = 1.3f; // MJ/m³/K (regolith)
        private const float ATMOSPHERE_HEAT_TRANSFER = 0.01f; // Heat transfer rate

        // Seasonal variation
        private float _seasonalOffset; // Degrees from solar equator
        private float _diurnalVariation; // Daily temperature swing

        public Pole(PoleType type, float latitude, float surfaceAreaKm2)
        {
            Type = type;
            Latitude = Math.Abs(latitude); // Always positive for calculations
            SurfaceArea = surfaceAreaKm2;
            IceCapArea = surfaceAreaKm2 * 0.3f; // Initial 30% ice coverage
            Albedo = 0.4f; // Mixed ice/soil albedo

            // Initial temperatures (Mars average ~210K)
            _surfaceTemperature = 210f;
            _iceTemperature = 200f;
            _atmosphericTemperature = 215f;

            _seasonalOffset = type == PoleType.North ? latitude : -latitude;
            _diurnalVariation = 20f; // 20K daily swing on Mars
        }

        #region Temperature Properties

        /// <summary>
        /// Surface temperature (soil/regolith) in Kelvin
        /// </summary>
        public float SurfaceTemperature
        {
            get => _surfaceTemperature;
            set => _surfaceTemperature = Math.Max(100f, Math.Min(350f, value)); // Clamp to reasonable Mars range
        }

        /// <summary>
        /// Ice cap temperature in Kelvin
        /// </summary>
        public float IceTemperature
        {
            get => _iceTemperature;
            set => _iceTemperature = Math.Max(100f, Math.Min(273f, value)); // Ice melts at 273K
        }

        /// <summary>
        /// Atmospheric temperature at pole in Kelvin
        /// </summary>
        public float AtmosphericTemperature
        {
            get => _atmosphericTemperature;
            set => _atmosphericTemperature = Math.Max(100f, Math.Min(350f, value));
        }

        /// <summary>
        /// Average temperature across the polar region (weighted by area)
        /// </summary>
        public float AverageTemperature
        {
            get
            {
                float iceWeight = IceCapArea / SurfaceArea;
                float soilWeight = 1f - iceWeight;
                return (_iceTemperature * iceWeight) + (_surfaceTemperature * soilWeight);
            }
        }

        #endregion

        #region Climate Calculations

        /// <summary>
        /// Calculate solar insolation at this latitude and season
        /// </summary>
        /// <param name="solarConstant">Solar constant (W/m²)</param>
        /// <param name="dayOfYear">Day of Martian year (0-668)</param>
        /// <returns>Insolation in W/m²</returns>
        public float CalculateInsolation(float solarConstant, float dayOfYear)
        {
            // Mars orbital parameters
            const float MARTIAN_YEAR_DAYS = 668.6f;
            const float OBLIQUITY = 25.19f; // Mars axial tilt

            // Calculate solar declination (simplified)
            float dayFraction = dayOfYear / MARTIAN_YEAR_DAYS;
            float declination = OBLIQUITY * (float)Math.Sin(2 * Math.PI * dayFraction);

            // Solar zenith angle
            float zenithAngle = Math.Abs(Latitude - declination);

            // Insolation with atmospheric attenuation
            float baseInsolation = solarConstant * (float)Math.Cos(zenithAngle * Math.PI / 180f);
            float atmosphericAttenuation = 0.7f; // Mars atmosphere blocks ~30% of sunlight

            return Math.Max(0f, baseInsolation * atmosphericAttenuation);
        }

        /// <summary>
        /// Calculate heat flux from surface to atmosphere
        /// </summary>
        /// <param name="atmosphericPressure">Local atmospheric pressure (kPa)</param>
        /// <param name="windSpeed">Wind speed (m/s)</param>
        /// <returns>Heat flux in W/m²</returns>
        public float CalculateHeatFlux(float atmosphericPressure, float windSpeed)
        {
            // Simplified heat transfer calculation
            float temperatureDifference = SurfaceTemperature - AtmosphericTemperature;
            float pressureFactor = Math.Max(0.1f, atmosphericPressure / 1f); // Pressure effect on heat transfer
            float windFactor = 1f + (windSpeed * 0.01f); // Wind enhances heat transfer

            return temperatureDifference * pressureFactor * windFactor * ATMOSPHERE_HEAT_TRANSFER;
        }

        /// <summary>
        /// Update polar temperatures based on energy balance
        /// </summary>
        /// <param name="insolation">Solar insolation (W/m²)</param>
        /// <param name="atmosphericPressure">Local pressure (kPa)</param>
        /// <param name="greenhouseEffect">Greenhouse warming (K)</param>
        /// <param name="deltaTime">Time step (seconds)</param>
        public void UpdateTemperatures(float insolation, float atmosphericPressure, float greenhouseEffect, float deltaTime)
        {
            // Energy balance equation: dT/dt = (Insolation - Emitted Radiation + Greenhouse) / HeatCapacity

            // Absorbed solar radiation (accounting for albedo)
            float absorbedRadiation = insolation * (1f - Albedo);

            // Emitted thermal radiation (Stefan-Boltzmann law)
            const float STEFAN_BOLTZMANN = 5.67e-8f; // W/m²/K⁴
            const float EMISSIVITY = 0.95f; // Mars surface emissivity
            float emittedRadiation = EMISSIVITY * STEFAN_BOLTZMANN * (float)Math.Pow(AverageTemperature, 4);

            // Greenhouse effect (additional warming)
            float greenhouseWarming = greenhouseEffect * 10f; // Convert K to W/m² equivalent

            // Net energy flux
            float netFlux = absorbedRadiation - emittedRadiation + greenhouseWarming;

            // Heat capacities (area-weighted)
            float iceHeatCapacity = IceCapArea * 1000f * ICE_HEAT_CAPACITY; // 1km thick ice
            float soilHeatCapacity = (SurfaceArea - IceCapArea) * 500f * SOIL_HEAT_CAPACITY; // 0.5km thick soil
            float totalHeatCapacity = iceHeatCapacity + soilHeatCapacity;

            // Temperature change
            float temperatureChange = (netFlux * deltaTime) / totalHeatCapacity;

            // Apply temperature changes
            SurfaceTemperature += temperatureChange * 0.7f; // Soil responds faster
            IceTemperature += temperatureChange * 0.3f; // Ice responds slower

            // Atmospheric coupling
            float heatFlux = CalculateHeatFlux(atmosphericPressure, 5f); // Assume 5 m/s wind
            AtmosphericTemperature += (heatFlux * deltaTime) / (SurfaceArea * 1000f); // Simple atmospheric heat capacity
        }

        /// <summary>
        /// Calculate ice sublimation/deposition rate
        /// </summary>
        /// <param name="atmosphericPressure">Local pressure (kPa)</param>
        /// <param name="humidity">Atmospheric humidity (0-1)</param>
        /// <returns>Sublimation rate in kg/m²/s (positive = sublimation, negative = deposition)</returns>
        public float CalculateIceSublimation(float atmosphericPressure, float humidity)
        {
            // Simplified Clausius-Clapeyron relation for CO2 ice on Mars
            const float SUBLIMATION_CONSTANT = 1e-6f; // kg/m²/s/kPa
            float vaporPressure = atmosphericPressure * humidity;

            // Temperature-dependent sublimation
            float tempFactor = (float)Math.Exp((IceTemperature - 148f) / 10f); // CO2 triple point ~148K

            return SUBLIMATION_CONSTANT * vaporPressure * tempFactor;
        }

        /// <summary>
        /// Update ice cap area based on sublimation and temperature
        /// </summary>
        /// <param name="sublimationRate">Sublimation rate from CalculateIceSublimation</param>
        /// <param name="deltaTime">Time step (seconds)</param>
        public void UpdateIceCap(float sublimationRate, float deltaTime)
        {
            // Ice density and latent heat
            const float ICE_DENSITY = 917f; // kg/m³
            const float ICE_LATENT_HEAT = 2.6e6f; // J/kg (sublimation enthalpy)

            // Mass change from sublimation
            float massChange = sublimationRate * IceCapArea * deltaTime;

            // Area change (assuming constant thickness)
            float thickness = 1000f; // 1km thick ice cap
            float volumeChange = massChange / ICE_DENSITY;
            float areaChange = volumeChange / thickness;

            IceCapArea = Math.Max(0f, IceCapArea + areaChange);

            // Update albedo based on ice coverage
            float iceFraction = IceCapArea / SurfaceArea;
            Albedo = 0.15f * (1f - iceFraction) + 0.65f * iceFraction; // Soil: 0.15, Ice: 0.65
        }

        #endregion

        #region Seasonal and Diurnal Effects

        /// <summary>
        /// Apply seasonal temperature variation
        /// </summary>
        /// <param name="seasonalAmplitude">Seasonal temperature swing (K)</param>
        /// <param name="dayOfYear">Day of year (0-668)</param>
        public void ApplySeasonalVariation(float seasonalAmplitude, float dayOfYear)
        {
            const float MARTIAN_YEAR_DAYS = 668.6f;
            float phase = (dayOfYear / MARTIAN_YEAR_DAYS) * 2f * (float)Math.PI;

            // Seasonal offset based on pole type
            float poleFactor = Type == PoleType.North ? 1f : -1f;
            float seasonalChange = seasonalAmplitude * (float)Math.Sin(phase) * poleFactor;

            SurfaceTemperature += seasonalChange * 0.01f; // Gradual change
            AtmosphericTemperature += seasonalChange * 0.005f; // Atmosphere lags behind
        }

        /// <summary>
        /// Apply diurnal (daily) temperature variation
        /// </summary>
        /// <param name="timeOfDay">Time of day (0-1, 0=midnight, 0.5=noon)</param>
        public void ApplyDiurnalVariation(float timeOfDay)
        {
            float diurnalOffset = _diurnalVariation * (float)Math.Sin(2 * Math.PI * timeOfDay);
            SurfaceTemperature += diurnalOffset * 0.1f; // Surface responds quickly
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Check if ice cap is stable (temperature below freezing)
        /// </summary>
        public bool IsIceStable => IceTemperature < 273f;

        /// <summary>
        /// Check if surface is habitable for liquid water
        /// </summary>
        public bool HasLiquidWater => SurfaceTemperature > 273f && IceCapArea < SurfaceArea * 0.1f;

        /// <summary>
        /// Get detailed status string for debugging
        /// </summary>
        public override string ToString()
        {
            return $"{Type} Pole: T_surf={SurfaceTemperature:F1}K, T_ice={IceTemperature:F1}K, " +
                   $"T_atm={AtmosphericTemperature:F1}K, Ice={IceCapArea:F0}km² ({IceCapArea/SurfaceArea:P1}), " +
                   $"Albedo={Albedo:F2}";
        }

        #endregion
    }
}