using System;
using PerAspera.Core;

namespace PerAspera.GameAPI.Climate.Configuration
{
    /// <summary>
    /// Configuration for climate simulation parameters
    /// Contains Mars-specific physical constants and simulation settings
    /// </summary>
    public class ClimateConfig
    {
        // === MARS PHYSICAL CONSTANTS ===
        
        /// <summary>Solar constant for Mars (W/m²) - ~43% of Earth's due to distance</summary>
        public float SolarConstant { get; set; } = 589f;
        
        /// <summary>Mars surface gravity (m/s²)</summary>
        public float SurfaceGravity { get; set; } = 3.71f;
        
        /// <summary>Mars planetary radius (km)</summary>
        public float PlanetRadius { get; set; } = 3389.5f;
        
        // === ATMOSPHERIC COMPOSITION (Molar Masses) ===
        
        /// <summary>CO2 molar mass (g/mol)</summary>
        public float CO2MolarMass { get; set; } = 44.01f;
        
        /// <summary>O2 molar mass (g/mol)</summary>
        public float O2MolarMass { get; set; } = 32.00f;
        
        /// <summary>N2 molar mass (g/mol)</summary>
        public float N2MolarMass { get; set; } = 28.01f;
        
        /// <summary>H2O vapor molar mass (g/mol)</summary>
        public float H2OMolarMass { get; set; } = 18.02f;
        
        // === GREENHOUSE EFFECT PARAMETERS ===
        
        /// <summary>CO2 greenhouse efficiency factor</summary>
        public float CO2GreenhouseEfficiency { get; set; } = 1.0f;
        
        /// <summary>H2O vapor greenhouse efficiency factor (stronger than CO2)</summary>
        public float H2OGreenhouseEfficiency { get; set; } = 2.8f;
        
        /// <summary>Maximum greenhouse warming (K) - prevents runaway</summary>
        public float MaxGreenhouseWarming { get; set; } = 60f;
        
        // === FACTORY METHODS ===
        
        /// <summary>
        /// Realistic Mars physics configuration
        /// Based on actual Martian atmospheric constants
        /// </summary>
        public static ClimateConfig CreateRealistic()
        {
            return new ClimateConfig();
        }
        
        /// <summary>
        /// Game-balanced configuration for playability
        /// Faster terraforming with enhanced greenhouse effects
        /// </summary>
        public static ClimateConfig CreateGameBalanced()
        {
            return new ClimateConfig
            {
                CO2GreenhouseEfficiency = 1.5f,  // Enhanced for gameplay
                H2OGreenhouseEfficiency = 4.0f,  // Stronger water vapor effect
                MaxGreenhouseWarming = 80f       // Higher warming potential
            };
        }
        
        /// <summary>
        /// Quick terraforming for testing and debugging
        /// Extremely accelerated climate changes
        /// </summary>
        public static ClimateConfig CreateDebug()
        {
            return new ClimateConfig
            {
                CO2GreenhouseEfficiency = 5.0f,  // Massive greenhouse effect
                H2OGreenhouseEfficiency = 10.0f, // Extreme water vapor warming
                MaxGreenhouseWarming = 200f      // No practical limit
            };
        }
        
        /// <summary>
        /// Default configuration (same as realistic)
        /// </summary>
        public static ClimateConfig CreateDefault() => CreateRealistic();
    }
}





