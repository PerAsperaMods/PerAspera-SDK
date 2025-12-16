namespace PerAspera.GameAPI.Climate.Configuration
{
    /// <summary>
    /// Terraforming constants and milestone thresholds
    /// Defines game-specific targets for Mars habitability
    /// </summary>
    public static class TerraformingConstants
    {
        // === TEMPERATURE THRESHOLDS (Kelvin) ===
        
        /// <summary>Water freezing point</summary>
        public const float TEMP_WATER_FREEZE = 273.15f;
        
        /// <summary>Minimum comfortable human temperature</summary>
        public const float TEMP_MIN_HABITABLE = 273.15f; // 0°C
        
        /// <summary>Optimal human temperature</summary>
        public const float TEMP_OPTIMAL = 288.15f; // 15°C
        
        /// <summary>Maximum comfortable human temperature</summary>
        public const float TEMP_MAX_HABITABLE = 298.15f; // 25°C
        
        /// <summary>Current average Mars temperature</summary>
        public const float TEMP_MARS_CURRENT = 210f; // -63°C
        
        /// <summary>Mars greenhouse threshold (rapid warming starts)</summary>
        public const float TEMP_GREENHOUSE_THRESHOLD = 230f;
        
        // === PRESSURE THRESHOLDS (atmospheres) ===
        
        /// <summary>Current Mars atmospheric pressure</summary>
        public const float PRESSURE_MARS_CURRENT = 0.006f;
        
        /// <summary>Minimum for liquid water (Armstrong limit)</summary>
        public const float PRESSURE_LIQUID_WATER = 0.063f; // 6.3 kPa
        
        /// <summary>Minimum breathable pressure</summary>
        public const float PRESSURE_MIN_BREATHABLE = 0.5f; // 50 kPa
        
        /// <summary>Earth-like pressure</summary>
        public const float PRESSURE_EARTH = 1.0f; // 101.3 kPa
        
        /// <summary>Maximum safe pressure</summary>
        public const float PRESSURE_MAX_SAFE = 1.5f;
        
        // === GAS COMPOSITION THRESHOLDS (% of total atmosphere) ===
        
        /// <summary>Minimum O2 for breathing</summary>
        public const float O2_MIN_BREATHABLE = 16f; // 16%
        
        /// <summary>Optimal O2 percentage</summary>
        public const float O2_OPTIMAL = 21f; // Earth-like
        
        /// <summary>Maximum safe O2 (fire risk)</summary>
        public const float O2_MAX_SAFE = 25f;
        
        /// <summary>Maximum safe CO2 for long-term exposure</summary>
        public const float CO2_MAX_SAFE = 4f; // 4%
        
        /// <summary>Toxic CO2 threshold</summary>
        public const float CO2_TOXIC = 10f; // 10%
        
        // === WATER THRESHOLDS (Gigatons) ===
        
        /// <summary>Minimum water stock for basic life</summary>
        public const float WATER_MIN_LIFE = 100f;
        
        /// <summary>Minimum water stock for stable hydrosphere</summary>
        public const float WATER_MIN_HYDROSPHERE = 500f;
        
        /// <summary>Target water stock for Earth-like oceans</summary>
        public const float WATER_EARTH_EQUIVALENT = 1400f;
        
        // === HABITABILITY WEIGHTS ===
        
        /// <summary>Temperature contribution to habitability score</summary>
        public const float WEIGHT_TEMPERATURE = 0.30f;
        
        /// <summary>Pressure contribution to habitability score</summary>
        public const float WEIGHT_PRESSURE = 0.30f;
        
        /// <summary>Oxygen contribution to habitability score</summary>
        public const float WEIGHT_OXYGEN = 0.25f;
        
        /// <summary>CO2 safety contribution to habitability score</summary>
        public const float WEIGHT_CO2_SAFETY = 0.15f;
        
        // === TERRAFORMING PHASES ===
        
        public enum TerraformingPhase
        {
            PreTerraforming = 0,    // < 5% habitable
            EarlyWarming = 1,       // 5-20% habitable
            AtmosphereBuildup = 2,  // 20-40% habitable
            Oxygenation = 3,        // 40-70% habitable
            Stabilization = 4,      // 70-90% habitable
            Habitable = 5           // > 90% habitable
        }
        
        /// <summary>Get terraforming phase from habitability percentage</summary>
        public static TerraformingPhase GetPhase(float habitability)
        {
            if (habitability >= 90f) return TerraformingPhase.Habitable;
            if (habitability >= 70f) return TerraformingPhase.Stabilization;
            if (habitability >= 40f) return TerraformingPhase.Oxygenation;
            if (habitability >= 20f) return TerraformingPhase.AtmosphereBuildup;
            if (habitability >= 5f) return TerraformingPhase.EarlyWarming;
            return TerraformingPhase.PreTerraforming;
        }
        
        /// <summary>Get phase description</summary>
        public static string GetPhaseDescription(TerraformingPhase phase)
        {
            return phase switch
            {
                TerraformingPhase.PreTerraforming => "Initial warming required",
                TerraformingPhase.EarlyWarming => "Greenhouse effect building",
                TerraformingPhase.AtmosphereBuildup => "Atmospheric pressure increasing",
                TerraformingPhase.Oxygenation => "Oxygen generation in progress",
                TerraformingPhase.Stabilization => "Final adjustments for habitability",
                TerraformingPhase.Habitable => "Planet is habitable!",
                _ => "Unknown phase"
            };
        }
    }
}
