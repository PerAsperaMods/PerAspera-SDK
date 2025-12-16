namespace PerAspera.GameAPI.Climate.Configuration
{
    /// <summary>
    /// Configuration parameters for climate simulation
    /// Based on Per Aspera's physical models and Mars terraforming constants
    /// </summary>
    public class ClimateConfig
    {
        // === SOLAR & ORBITAL ===
        
        /// <summary>Solar constant at Mars orbit (W/m²)</summary>
        public float SolarConstant { get; set; } = 590f; // Mars receives ~44% of Earth's solar
        
        /// <summary>Mars orbital period (Earth days)</summary>
        public float OrbitalPeriod { get; set; } = 687f;
        
        /// <summary>Mars axial tilt (degrees)</summary>
        public float AxialTilt { get; set; } = 25.2f;
        
        // === ALBEDO ===
        
        /// <summary>Base planetary albedo (reflectivity 0-1)</summary>
        public float BaseAlbedo { get; set; } = 0.25f; // Rocky Mars
        
        /// <summary>Ice albedo modifier</summary>
        public float IceAlbedo { get; set; } = 0.65f; // Polar caps
        
        /// <summary>Vegetation albedo modifier</summary>
        public float VegetationAlbedo { get; set; } = 0.18f; // Plant coverage
        
        // === GREENHOUSE EFFECT ===
        
        /// <summary>CO2 greenhouse efficiency (K per kPa)</summary>
        public float CO2GreenhouseEfficiency { get; set; } = 2.5f;
        
        /// <summary>Super-greenhouse gas efficiency multiplier</summary>
        public float GHGMultiplier { get; set; } = 100f; // 100x more potent than CO2
        
        /// <summary>Water vapor greenhouse efficiency (K per kPa)</summary>
        public float H2OGreenhouseEfficiency { get; set; } = 5.0f; // Strong feedback
        
        /// <summary>General greenhouse efficiency modifier (balancing)</summary>
        public float GreenhouseEfficiency { get; set; } = 1.0f;
        
        // === ATMOSPHERIC BUILDUP ===
        
        /// <summary>Pressure buildup rate from outgassing (atm/sol)</summary>
        public float OutgassingRate { get; set; } = 0.0001f;
        
        /// <summary>Water evaporation rate coefficient</summary>
        public float WaterEvaporationRate { get; set; } = 0.05f;
        
        /// <summary>Atmospheric escape rate (fraction per sol)</summary>
        public float AtmosphericEscapeRate { get; set; } = 0.00001f; // Slow without magnetosphere
        
        // === THERMAL ===
        
        /// <summary>Thermal inertia (how quickly temperature changes)</summary>
        public float ThermalInertia { get; set; } = 0.85f; // 0-1, lower = faster changes
        
        /// <summary>Heat capacity of atmosphere (J/K)</summary>
        public float HeatCapacity { get; set; } = 1000f;
        
        /// <summary>Blackbody radiation coefficient</summary>
        public float BlackbodyCoefficient { get; set; } = 5.67e-8f; // Stefan-Boltzmann
        
        // === GAME BALANCE ===
        
        /// <summary>Simulation speed multiplier</summary>
        public float SimulationSpeed { get; set; } = 1.0f;
        
        /// <summary>Enable realistic physics (vs game-balanced)</summary>
        public bool RealisticMode { get; set; } = false;
        
        /// <summary>Debug logging enabled</summary>
        public bool DebugLogging { get; set; } = false;
        
        public override string ToString() =>
            $"ClimateConfig: Solar={SolarConstant}W/m², GH_Efficiency={GreenhouseEfficiency}x, Realistic={RealisticMode}";
    }
}
