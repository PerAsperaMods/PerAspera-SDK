using System;

namespace PerAspera.GameAPI.Climate
{
    /// <summary>
    /// Polar region with ice dynamics and extreme temperatures
    /// </summary>
    public class Pole
    {
        public enum PoleType { North, South }

        public PoleType Type { get; }
        public float Latitude { get; }
        public float SurfaceArea { get; }

        private float _surfaceTemperature;
        private float _atmosphericTemperature;
        private float _iceTemperature;
        private float _iceCapArea;
        private float _albedo;

        public Pole(PoleType type, float latitude, float surfaceAreaKm2)
        {
            Type = type;
            Latitude = type == PoleType.North ? latitude : -latitude;
            SurfaceArea = surfaceAreaKm2;
            _surfaceTemperature = 180f; // Very cold
            _atmosphericTemperature = 185f;
            _iceTemperature = 170f;
            _iceCapArea = surfaceAreaKm2 * 0.8f; // 80% ice coverage initially
            _albedo = 0.6f; // High albedo for ice
        }

        public float SurfaceTemperature => _surfaceTemperature;
        public float AtmosphericTemperature => _atmosphericTemperature;
        public float IceTemperature => _iceTemperature;
        public float IceCapArea => _iceCapArea;
        public float Albedo => _albedo;
        public float AverageTemperature => (_surfaceTemperature + _atmosphericTemperature) / 2f;
        public bool IsIceStable => _iceTemperature < 273f; // Below freezing

        public void UpdateTemperatures(float solarConstant, float atmosphericPressure,
                                     float greenhouseEffect, float dayOfYear, float timeOfDay, float deltaTime)
        {
            // Polar insolation (very low due to latitude and season)
            float baseInsolation = solarConstant * 0.1f; // Much less than equator

            // Seasonal variation (extreme at poles)
            float seasonalFactor = (float)Math.Sin(2 * Math.PI * (dayOfYear / 668.6f + (Type == PoleType.North ? 0 : 0.5f)));
            float insolation = baseInsolation * Math.Max(0f, seasonalFactor); // No sunlight in winter

            // Diurnal variation (minimal at poles)
            float diurnalFactor = (float)Math.Sin(2 * Math.PI * timeOfDay) * 0.2f + 0.8f;
            insolation *= diurnalFactor;

            // Energy balance
            const float STEFAN_BOLTZMANN = 5.67e-8f;
            float emitted = 0.95f * STEFAN_BOLTZMANN * (float)Math.Pow(_surfaceTemperature, 4);
            float netFlux = insolation - emitted + greenhouseEffect * 2f; // Reduced greenhouse effect at poles

            // Temperature change (polar regions are more stable)
            float heatCapacity = SurfaceArea * 1000f * 2.0f; // Higher heat capacity due to ice
            float tempChange = (netFlux * deltaTime) / heatCapacity;

            _surfaceTemperature += tempChange;
            _atmosphericTemperature += tempChange * 0.9f; // Atmosphere closely follows surface

            // Ice temperature (slightly lower than surface)
            _iceTemperature = _surfaceTemperature - 10f;

            // Clamp to reasonable ranges
            _surfaceTemperature = Math.Max(100f, Math.Min(280f, _surfaceTemperature));
            _atmosphericTemperature = Math.Max(100f, Math.Min(280f, _atmosphericTemperature));
            _iceTemperature = Math.Max(100f, Math.Min(273f, _iceTemperature));

            // Update ice dynamics
            UpdateIceDynamics(deltaTime);
        }

        private void UpdateIceDynamics(float deltaTime)
        {
            // Ice sublimation (simplified Clausius-Clapeyron)
            if (_iceTemperature < 273f && _iceTemperature > 150f)
            {
                float sublimationRate = (float)Math.Exp((273f - _iceTemperature) / 10f) * 0.001f;
                _iceCapArea -= sublimationRate * deltaTime;
                _iceCapArea = Math.Max(0f, _iceCapArea);
            }

            // Albedo changes with ice coverage
            _albedo = 0.2f + 0.4f * (_iceCapArea / SurfaceArea); // Bare ground: 0.2, full ice: 0.6
        }

        public override string ToString()
        {
            string poleName = Type == PoleType.North ? "North" : "South";
            return $"{poleName} Pole: T_surf={_surfaceTemperature:F1}K, T_ice={_iceTemperature:F1}K, " +
                   $"Ice={_iceCapArea:F0}km² ({_iceCapArea/SurfaceArea:P0}), Albedo={_albedo:F2}";
        }
    }

    /// <summary>
    /// Equatorial region with different climate dynamics
    /// </summary>
    public class EquatorialRegion
    {
        public float Latitude { get; }
        public float SurfaceArea { get; }

        private float _surfaceTemperature;
        private float _atmosphericTemperature;
        private float _soilMoisture;
        private float _relativeHumidity;
        private float _windSpeed;

        public EquatorialRegion(float latitude, float surfaceAreaKm2)
        {
            Latitude = latitude;
            SurfaceArea = surfaceAreaKm2;
            _surfaceTemperature = 250f; // Warmer than poles
            _atmosphericTemperature = 255f;
            _soilMoisture = 0.05f; // Low moisture on Mars
            _relativeHumidity = 0.15f; // Low humidity on Mars
            _windSpeed = 5.0f; // Moderate winds
        }

        public float AverageTemperature => (_surfaceTemperature + _atmosphericTemperature) / 2f;
        public float SurfaceTemperature => _surfaceTemperature;
        public float AtmosphericTemperature => _atmosphericTemperature;
        public float RelativeHumidity => _relativeHumidity;
        public float WindSpeed => _windSpeed;

        public void UpdateTemperatures(float solarConstant, float atmosphericPressure,
                                     float greenhouseEffect, float dayOfYear, float timeOfDay, float deltaTime)
        {
            // Equatorial insolation (higher than poles)
            float insolation = solarConstant * 0.8f; // Account for atmospheric attenuation

            // Diurnal variation
            float diurnalFactor = (float)Math.Sin(2 * Math.PI * timeOfDay);
            insolation *= (0.5f + 0.5f * diurnalFactor); // Day/night cycle

            // Energy balance
            const float STEFAN_BOLTZMANN = 5.67e-8f;
            float emitted = 0.95f * STEFAN_BOLTZMANN * (float)Math.Pow(_surfaceTemperature, 4);
            float netFlux = insolation - emitted + greenhouseEffect * 5f; // Greenhouse effect

            // Temperature change (equatorial regions respond faster)
            float heatCapacity = SurfaceArea * 1000f * 1.3f; // MJ/K
            float tempChange = (netFlux * deltaTime) / heatCapacity;

            _surfaceTemperature += tempChange;
            _atmosphericTemperature += tempChange * 0.8f; // Atmosphere follows surface

            // Clamp to reasonable ranges
            _surfaceTemperature = Math.Max(150f, Math.Min(350f, _surfaceTemperature));
            _atmosphericTemperature = Math.Max(150f, Math.Min(350f, _atmosphericTemperature));

            // Update humidity based on temperature and moisture
            UpdateHumidity(deltaTime);

            // Update wind speed based on temperature gradients
            UpdateWindSpeed(deltaTime);
        }

        private void UpdateHumidity(float deltaTime)
        {
            // Humidity increases with temperature and moisture
            float targetHumidity = Math.Min(0.8f, _soilMoisture * 2f + (_surfaceTemperature - 200f) / 200f);
            _relativeHumidity += (targetHumidity - _relativeHumidity) * deltaTime * 0.1f;
            _relativeHumidity = Math.Max(0.01f, Math.Min(1.0f, _relativeHumidity));
        }

        private void UpdateWindSpeed(float deltaTime)
        {
            // Wind speed varies with temperature gradients
            float tempGradient = Math.Abs(_surfaceTemperature - _atmosphericTemperature);
            float targetWindSpeed = 2f + tempGradient * 0.1f; // Base wind + gradient effect
            _windSpeed += (targetWindSpeed - _windSpeed) * deltaTime * 0.05f;
            _windSpeed = Math.Max(0.5f, Math.Min(20f, _windSpeed));
        }

        public override string ToString()
        {
            return $"Equator: T_surf={_surfaceTemperature:F1}K, T_atm={_atmosphericTemperature:F1}K, " +
                   $"Humidity={_relativeHumidity:P1}, Wind={_windSpeed:F1}m/s, Moisture={_soilMoisture:P1}";
        }
    }

    /// <summary>
    /// Data structure containing regional climate information
    /// </summary>
    public class ClimateRegionData
    {
        public Pole NorthPole { get; set; }
        public Pole SouthPole { get; set; }
        public EquatorialRegion EquatorialRegion { get; set; }
        public GlobalClimateAverages GlobalAverages { get; set; }

        public ClimateRegionData(Pole northPole, Pole southPole, EquatorialRegion equatorialRegion)
        {
            NorthPole = northPole;
            SouthPole = southPole;
            EquatorialRegion = equatorialRegion;
            GlobalAverages = new GlobalClimateAverages();
        }
    }

    /// <summary>
    /// Global climate averages calculated from regional data
    /// </summary>
    public class GlobalClimateAverages
    {
        public float SurfaceTemperature { get; set; }
        public float AtmosphericTemperature { get; set; }
        public float IceTemperature { get; set; }
        public float AverageAlbedo { get; set; }
        public float AverageHumidity { get; set; }
        public float AverageWindSpeed { get; set; }
        public float TotalIceArea { get; set; }
        public float TotalSurfaceArea { get; set; }

        public GlobalClimateAverages()
        {
            // Initialize with defaults
            SurfaceTemperature = 0f;
            AtmosphericTemperature = 0f;
            IceTemperature = 0f;
            AverageAlbedo = 0f;
            AverageHumidity = 0f;
            AverageWindSpeed = 0f;
            TotalIceArea = 0f;
            TotalSurfaceArea = 0f;
        }
    }
}