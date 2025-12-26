using System;
using System.Collections.Generic;
using PerAspera.Core;
using PerAspera.GameAPI.Climate.Configuration;
using PerAspera.GameAPI.Climate.Domain;
using PerAspera.GameAPI.Wrappers;

namespace PerAspera.GameAPI.Climate.Simulation
{
    /// <summary>
    /// Advanced climate simulation engine with area-specific calculations
    /// Implements custom formulas instead of relying on game averages
    /// </summary>
    public class ClimateSimulator
    {
        private static readonly LogAspera Log = new LogAspera("Climate.Simulator");

        private readonly ClimateConfig _config;
        private readonly Pole _northPole;
        private readonly Pole _southPole;
        private readonly EquatorialRegion _equatorialRegion;

        // Simulation state
        private float _currentTime;
        private float _dayOfYear;
        private float _timeOfDay;

        // Mars orbital parameters
        private const float MARTIAN_YEAR_DAYS = 668.6f;
        private const float MARTIAN_DAY_HOURS = 24.66f;
        private const float MARTIAN_SOLAR_DAY_SECONDS = MARTIAN_DAY_HOURS * 3600f;

        public ClimateSimulator(ClimateConfig config)
        {
            _config = config;

            // Initialize regional climate zones
            _northPole = new Pole(Pole.PoleType.North, 85f, 500000f); // 85°N, 500,000 km²
            _southPole = new Pole(Pole.PoleType.South, 85f, 500000f); // 85°S, 500,000 km²
            _equatorialRegion = new EquatorialRegion(0f, 2000000f); // Equator, 2,000,000 km²

            _currentTime = 0f;
            _dayOfYear = 0f;
            _timeOfDay = 0f;

            Log.Info("Advanced ClimateSimulator initialized with regional calculations");
        }

        /// <summary>
        /// Simulate one climate step with area-specific calculations
        /// </summary>
        public void SimulateStep(PerAspera.GameAPI.Wrappers.Atmosphere atmosphere, float deltaTime)
        {
            try
            {
                _currentTime += deltaTime;
                UpdateOrbitalParameters(deltaTime);

                // Get current atmospheric state
                float totalPressure = atmosphere.TotalPressure;
                float co2Pressure = atmosphere.GetGasQuantity("resource_carbon_dioxide_release");
                float o2Pressure = atmosphere.GetGasQuantity("resource_oxygen_release");
                float n2Pressure = atmosphere.GetGasQuantity("resource_nitrogen_release");
                float ghgPressure = atmosphere.GetGasQuantity("resource_ghg_release");

                // Calculate greenhouse effect
                float greenhouseEffect = CalculateGreenhouseEffect(co2Pressure, o2Pressure, ghgPressure);

                // Update each region
                UpdatePolarRegion(_northPole, atmosphere, greenhouseEffect, deltaTime);
                UpdatePolarRegion(_southPole, atmosphere, greenhouseEffect, deltaTime);
                UpdateEquatorialRegion(_equatorialRegion, atmosphere, greenhouseEffect, deltaTime);

                // Calculate global averages from regional data
                float globalTemperature = CalculateGlobalAverageTemperature();
                float globalPressure = totalPressure;

                // Apply changes back to game atmosphere (if needed)
                // This would be done through Harmony patches or direct API calls

                Log.Debug($"Climate step: T_global={globalTemperature:F1}K, P={globalPressure:F2}kPa, " +
                         $"GHG={greenhouseEffect:F1}K, Day={_dayOfYear:F0}, Time={_timeOfDay:F2}");
            }
            catch (Exception ex)
            {
                Log.Error($"Climate simulation error: {ex.Message}");
            }
        }

        /// <summary>
        /// Update orbital time parameters
        /// </summary>
        private void UpdateOrbitalParameters(float deltaTime)
        {
            // Update day of year
            _dayOfYear += deltaTime / MARTIAN_SOLAR_DAY_SECONDS;
            if (_dayOfYear >= MARTIAN_YEAR_DAYS)
                _dayOfYear -= MARTIAN_YEAR_DAYS;

            // Update time of day
            _timeOfDay += deltaTime / MARTIAN_SOLAR_DAY_SECONDS;
            if (_timeOfDay >= 1f)
                _timeOfDay -= 1f;
        }

        /// <summary>
        /// Update a polar region's climate
        /// </summary>
        private void UpdatePolarRegion(Pole pole, PerAspera.GameAPI.Wrappers.Atmosphere atmosphere,
                                     float greenhouseEffect, float deltaTime)
        {
            // Update pole temperatures using the new integrated method
            pole.UpdateTemperatures(_config.SolarConstant, atmosphere.TotalPressure,
                                  greenhouseEffect, _dayOfYear, _timeOfDay, deltaTime);
        }

        /// <summary>
        /// Update equatorial region's climate
        /// </summary>
        private void UpdateEquatorialRegion(EquatorialRegion equator, PerAspera.GameAPI.Wrappers.Atmosphere atmosphere,
                                          float greenhouseEffect, float deltaTime)
        {
            equator.UpdateTemperatures(_config.SolarConstant, atmosphere.TotalPressure,
                                    greenhouseEffect, _dayOfYear, _timeOfDay, deltaTime);
        }

        /// <summary>
        /// Calculate greenhouse effect based on atmospheric composition
        /// Custom formula implementation
        /// </summary>
        private float CalculateGreenhouseEffect(float co2Pressure, float o2Pressure, float ghgPressure)
        {
            // CO2 greenhouse effect (logarithmic relationship)
            float co2Effect = _config.CO2GreenhouseEfficiency *
                            (float)Math.Log(1f + co2Pressure * 100f) * 5f; // Scale factor

            // H2O vapor effect (simplified - would need actual water vapor data)
            float h2oEffect = _config.H2OGreenhouseEfficiency * 0.01f; // Placeholder

            // Other greenhouse gases
            float ghgEffect = ghgPressure * 0.1f; // Simplified

            // Total greenhouse warming
            float totalEffect = co2Effect + h2oEffect + ghgEffect;

            // Apply maximum limit
            return Math.Min(totalEffect, _config.MaxGreenhouseWarming);
        }

        /// <summary>
        /// Calculate regional humidity based on pole conditions
        /// </summary>
        private float CalculateRegionalHumidity(Pole pole, PerAspera.GameAPI.Wrappers.Atmosphere atmosphere)
        {
            // Simplified humidity calculation based on temperature and ice coverage
            float baseHumidity = 0.1f; // Base atmospheric humidity

            // Cold poles have lower humidity due to ice sequestration
            float temperatureFactor = Math.Max(0f, 1f - (pole.IceTemperature - 150f) / 100f);
            float iceFactor = 1f - (pole.IceCapArea / pole.SurfaceArea);

            return baseHumidity * temperatureFactor * iceFactor;
        }

        /// <summary>
        /// Calculate global average temperature from regional data
        /// Area-weighted average
        /// </summary>
        private float CalculateGlobalAverageTemperature()
        {
            float totalArea = _northPole.SurfaceArea + _southPole.SurfaceArea + _equatorialRegion.SurfaceArea;
            float weightedTemp = (_northPole.AverageTemperature * _northPole.SurfaceArea) +
                               (_southPole.AverageTemperature * _southPole.SurfaceArea) +
                               (_equatorialRegion.AverageTemperature * _equatorialRegion.SurfaceArea);

            return weightedTemp / totalArea;
        }

        /// <summary>
        /// Get detailed climate status with regional information
        /// </summary>
        public string GetClimateStatus(PerAspera.GameAPI.Wrappers.Atmosphere atmosphere)
        {
            float globalTemp = CalculateGlobalAverageTemperature();
            float greenhouse = CalculateGreenhouseEffect(
                atmosphere.GetGasQuantity("resource_carbon_dioxide_release"),
                atmosphere.GetGasQuantity("resource_oxygen_release"),
                atmosphere.GetGasQuantity("resource_ghg_release"));

            return $"Global: {globalTemp:F1}K, GHG: {greenhouse:F1}K\n" +
                   $"{_northPole}\n" +
                   $"{_southPole}\n" +
                   $"{_equatorialRegion}";
        }

        /// <summary>
        /// Get regional temperature data for UI display
        /// </summary>
        public (float northTemp, float southTemp, float equatorTemp, float globalTemp) GetRegionalTemperatures()
        {
            return (_northPole.AverageTemperature,
                   _southPole.AverageTemperature,
                   _equatorialRegion.AverageTemperature,
                   CalculateGlobalAverageTemperature());
        }

        /// <summary>
        /// Get ice cap status for monitoring
        /// </summary>
        public (float northIceArea, float southIceArea, bool northStable, bool southStable) GetIceCapStatus()
        {
            return (_northPole.IceCapArea,
                   _southPole.IceCapArea,
                   _northPole.IsIceStable,
                   _southPole.IsIceStable);
        }

        /// <summary>
        /// Get comprehensive regional climate data
        /// </summary>
        public ClimateRegionData GetRegionalData()
        {
            var regionalData = new ClimateRegionData(_northPole, _southPole, _equatorialRegion);

            // Calculate global averages
            float totalArea = _northPole.SurfaceArea + _southPole.SurfaceArea + _equatorialRegion.SurfaceArea;

            regionalData.GlobalAverages.SurfaceTemperature =
                (_northPole.SurfaceTemperature * _northPole.SurfaceArea +
                 _southPole.SurfaceTemperature * _southPole.SurfaceArea +
                 _equatorialRegion.SurfaceTemperature * _equatorialRegion.SurfaceArea) / totalArea;

            regionalData.GlobalAverages.AtmosphericTemperature =
                (_northPole.AtmosphericTemperature * _northPole.SurfaceArea +
                 _southPole.AtmosphericTemperature * _southPole.SurfaceArea +
                 _equatorialRegion.AtmosphericTemperature * _equatorialRegion.SurfaceArea) / totalArea;

            regionalData.GlobalAverages.IceTemperature =
                (_northPole.IceTemperature + _southPole.IceTemperature) / 2f;

            regionalData.GlobalAverages.AverageAlbedo =
                (_northPole.Albedo * _northPole.SurfaceArea +
                 _southPole.Albedo * _southPole.SurfaceArea +
                 0.2f * _equatorialRegion.SurfaceArea) / totalArea; // Equatorial albedo ~0.2

            regionalData.GlobalAverages.TotalIceArea = _northPole.IceCapArea + _southPole.IceCapArea;
            regionalData.GlobalAverages.TotalSurfaceArea = totalArea;

            regionalData.GlobalAverages.AverageHumidity = _equatorialRegion.RelativeHumidity;
            regionalData.GlobalAverages.AverageWindSpeed = _equatorialRegion.WindSpeed;

            return regionalData;
        }
    }
}