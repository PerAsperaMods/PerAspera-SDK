using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerAspera.GameAPI.Climate.Domain
{
    /// <summary>
    /// Represents the equatorial region with tropical climate characteristics
    /// Handles humidity, convection, and heat distribution in low latitudes
    /// </summary>
    public class EquatorialRegion
    {
        // Geographic properties
        public float Latitude { get; private set; } // Degrees from equator (typically 0)
        public float SurfaceArea { get; private set; } // km²

        // Temperature properties (in Kelvin)
        private float _surfaceTemperature;
        private float _atmosphericTemperature;

        // Humidity and moisture properties
        private float _relativeHumidity; // 0-1
        private float _absoluteHumidity; // kg/m³
        private float _dewPoint; // Kelvin

        // Atmospheric circulation
        private float _convectionStrength; // 0-1, affects heat transport
        private float _windSpeed; // m/s

        // Heat transfer coefficients
        private const float EQUATORIAL_HEAT_CAPACITY = 1.5f; // MJ/m³/K
        private const float HUMIDITY_LATENT_HEAT = 2.26f; // MJ/kg (water vaporization)
        private const float CONVECTION_COEFFICIENT = 0.02f; // Heat transfer rate

        public EquatorialRegion(float latitude, float surfaceAreaKm2)
        {
            Latitude = latitude;
            SurfaceArea = surfaceAreaKm2;

            // Initialize with typical equatorial conditions
            _surfaceTemperature = 300f; // 27°C
            _atmosphericTemperature = 295f; // 22°C
            _relativeHumidity = 0.7f; // 70%
            _absoluteHumidity = 0.015f; // 15 g/m³
            _dewPoint = 293f; // 20°C
            _convectionStrength = 0.8f; // Strong convection
            _windSpeed = 3.0f; // Light winds
        }

        /// <summary>
        /// Surface temperature in Kelvin
        /// </summary>
        public float SurfaceTemperature => _surfaceTemperature;

        /// <summary>
        /// Atmospheric temperature in Kelvin
        /// </summary>
        public float AtmosphericTemperature => _atmosphericTemperature;

        /// <summary>
        /// Relative humidity as a fraction (0-1)
        /// </summary>
        public float RelativeHumidity => _relativeHumidity;

        /// <summary>
        /// Absolute humidity in kg/m³
        /// </summary>
        public float AbsoluteHumidity => _absoluteHumidity;

        /// <summary>
        /// Dew point temperature in Kelvin
        /// </summary>
        public float DewPoint => _dewPoint;

        /// <summary>
        /// Convection strength affecting heat transport (0-1)
        /// </summary>
        public float ConvectionStrength => _convectionStrength;

        /// <summary>
        /// Wind speed in m/s
        /// </summary>
        public float WindSpeed => _windSpeed;

        /// <summary>
        /// Calculate insolation for equatorial region
        /// Equatorial regions receive consistent solar radiation throughout the year
        /// </summary>
        public float CalculateInsolation(float solarConstant, float dayOfYear, float timeOfDay)
        {
            // Equatorial regions have minimal seasonal variation
            float seasonalFactor = 1.0f - 0.1f * (float)Math.Cos(2 * Math.PI * dayOfYear / 668.6); // Mars year

            // Diurnal variation (stronger at equator)
            float diurnalFactor = 0.5f + 0.5f * (float)Math.Sin(2 * Math.PI * timeOfDay / 24.66); // Mars day

            // Atmospheric attenuation (less at equator due to thinner atmosphere)
            float atmosphericAttenuation = 0.85f;

            return solarConstant * seasonalFactor * diurnalFactor * atmosphericAttenuation;
        }

        /// <summary>
        /// Update equatorial temperatures based on insolation and atmospheric conditions
        /// </summary>
        public void UpdateTemperatures(float insolation, float greenhouseEffect, float timeStep)
        {
            // Heat balance equation for equatorial region
            float netRadiation = insolation * (1 - 0.1f) - greenhouseEffect; // Absorbed radiation minus emitted

            // Convection and latent heat effects
            float convectionHeat = _convectionStrength * (_atmosphericTemperature - _surfaceTemperature) * CONVECTION_COEFFICIENT;
            float latentHeat = CalculateLatentHeatFlux();

            // Temperature change
            float heatCapacity = EQUATORIAL_HEAT_CAPACITY * SurfaceArea * 1000000; // Convert km² to m²
            float deltaT = (netRadiation + convectionHeat + latentHeat) * timeStep / heatCapacity;

            _surfaceTemperature += deltaT;

            // Atmospheric temperature lags surface temperature
            float atmosphericLag = 0.1f; // 10% coupling
            _atmosphericTemperature += deltaT * atmosphericLag;

            // Ensure temperatures stay within reasonable bounds
            _surfaceTemperature = Math.Max(250f, Math.Min(350f, _surfaceTemperature));
            _atmosphericTemperature = Math.Max(240f, Math.Min(340f, _atmosphericTemperature));
        }

        /// <summary>
        /// Calculate latent heat flux from evaporation/condensation
        /// </summary>
        private float CalculateLatentHeatFlux()
        {
            // Simplified latent heat calculation based on humidity gradient
            float saturationHumidity = CalculateSaturationHumidity(_surfaceTemperature);
            float humidityDeficit = saturationHumidity - _absoluteHumidity;

            // Evaporation rate proportional to humidity deficit
            float evaporationRate = Math.Max(0, humidityDeficit * 0.001f); // kg/m²/s

            return evaporationRate * HUMIDITY_LATENT_HEAT * SurfaceArea * 1000000; // Convert to MJ
        }

        /// <summary>
        /// Calculate saturation humidity at given temperature
        /// Using Clausius-Clapeyron approximation
        /// </summary>
        private float CalculateSaturationHumidity(float temperatureK)
        {
            // Simplified Clausius-Clapeyron equation
            const float REFERENCE_TEMP = 273.15f; // 0°C in Kelvin
            const float REFERENCE_HUMIDITY = 0.0048f; // kg/m³ at 0°C
            const float LATENT_HEAT = 2.5e6f; // J/kg
            const float GAS_CONSTANT = 461f; // J/kg/K for water vapor

            float exponent = (LATENT_HEAT / GAS_CONSTANT) * (1f / REFERENCE_TEMP - 1f / temperatureK);
            return REFERENCE_HUMIDITY * (float)Math.Exp(exponent);
        }

        /// <summary>
        /// Update humidity based on temperature and atmospheric conditions
        /// </summary>
        public void UpdateHumidity(float co2Pressure, float timeStep)
        {
            // Humidity affected by temperature and CO2 pressure
            float saturationHumidity = CalculateSaturationHumidity(_surfaceTemperature);
            float targetHumidity = saturationHumidity * (1f - co2Pressure * 0.1f); // CO2 suppresses humidity

            // Exponential approach to target humidity
            float humidityTimeConstant = 3600f; // 1 hour time constant
            float humidityRate = (targetHumidity - _absoluteHumidity) / humidityTimeConstant;

            _absoluteHumidity += humidityRate * timeStep;

            // Update relative humidity
            _relativeHumidity = Math.Min(1.0f, _absoluteHumidity / saturationHumidity);

            // Update dew point
            _dewPoint = CalculateDewPoint(_absoluteHumidity);
        }

        /// <summary>
        /// Calculate dew point from absolute humidity
        /// </summary>
        private float CalculateDewPoint(float absoluteHumidity)
        {
            // Simplified dew point calculation
            const float A = 17.27f;
            const float B = 237.7f;

            float alpha = (float)Math.Log(absoluteHumidity / 0.0048);
            return B * alpha / (A - alpha) + 273.15f; // Convert to Kelvin
        }

        /// <summary>
        /// Apply seasonal and diurnal variations
        /// </summary>
        public void ApplySeasonalVariation(float seasonalOffset, float diurnalVariation)
        {
            // Equatorial regions have minimal seasonal variation but strong diurnal cycles
            float seasonalEffect = seasonalOffset * 0.1f; // Reduced seasonal effect
            float diurnalEffect = diurnalVariation * 0.3f; // Enhanced diurnal effect

            _surfaceTemperature += seasonalEffect + diurnalEffect;
            _atmosphericTemperature += seasonalEffect * 0.5f + diurnalEffect * 0.2f;
        }
    }
}