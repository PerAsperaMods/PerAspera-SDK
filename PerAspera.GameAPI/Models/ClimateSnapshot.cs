using System;

namespace PerAspera.GameAPI.Models
{
    /// <summary>
    /// Immutable snapshot of planetary climate at specific moment
    /// Captures all atmospheric and environmental parameters from Planet native data
    /// </summary>
    public record ClimateSnapshot
    {
        public DateTime Timestamp { get; init; }
        public int Sol { get; init; }
        
        // === TEMPERATURE (Kelvin) ===
        public float Temperature { get; init; }
        public float MinTemperature { get; init; }
        public float MaxTemperature { get; init; }
        
        // === ATMOSPHERIC PRESSURE ===
        /// <summary>Total atmospheric pressure (atm)</summary>
        public float TotalPressure { get; init; }
        /// <summary>CO2 partial pressure (kPa)</summary>
        public float CO2Pressure { get; init; }
        /// <summary>O2 partial pressure (kPa)</summary>
        public float O2Pressure { get; init; }
        /// <summary>N2 partial pressure (kPa)</summary>
        public float N2Pressure { get; init; }
        /// <summary>Greenhouse gas partial pressure (kPa)</summary>
        public float GHGPressure { get; init; }
        /// <summary>Argon partial pressure (kPa)</summary>
        public float ArgonPressure { get; init; }
        /// <summary>Water vapor partial pressure (kPa)</summary>
        public float H2OPressure { get; init; }
        
        // === WATER (Gigatons & Pressure) ===
        public float WaterStock { get; init; }
        public float PermafrostDeposits { get; init; }
        /// <summary>Water vapor partial pressure (kPa)</summary>
        public float WaterVaporPressure { get; init; }
        
        // === PLANETARY EFFECTS ===
        /// <summary>Greenhouse effect coefficient (multiplier on temperature)</summary>
        public float GreenhouseEffect { get; init; }
        /// <summary>Planetary albedo - reflectivity (0-1)</summary>
        public float Albedo { get; init; }
        
        // === SPECIAL EFFECTS (Temperature modifiers) ===
        public float PolarNukeEffect { get; init; }
        public float PolarDustEffect { get; init; }
        public float CometEffect { get; init; }
        public float DeimosEffect { get; init; }
        
        // === CALCULATED PROPERTIES ===
        
        /// <summary>Get O2 percentage in atmosphere (0-100)</summary>
        public float O2Percentage => TotalPressure > 0 ? (O2Pressure / TotalPressure) * 100f : 0f;
        
        /// <summary>Get CO2 percentage in atmosphere (0-100)</summary>
        public float CO2Percentage => TotalPressure > 0 ? (CO2Pressure / TotalPressure) * 100f : 0f;
        
        /// <summary>Get N2 percentage in atmosphere (0-100)</summary>
        public float N2Percentage => TotalPressure > 0 ? (N2Pressure / TotalPressure) * 100f : 0f;
        
        /// <summary>Is atmosphere breathable for humans?</summary>
        public bool IsBreathable => 
            Temperature >= 273f && Temperature <= 298f &&
            TotalPressure >= 0.5f &&
            O2Percentage >= 16f &&
            CO2Percentage <= 4f;
        
        /// <summary>Temperature in Celsius</summary>
        public float TemperatureCelsius => Temperature - 273.15f;
        
        public override string ToString() =>
            $"Climate[Sol {Sol}]: {Temperature:F1}K ({TemperatureCelsius:F1}°C), " +
            $"P={TotalPressure:F3}atm, O2={O2Percentage:F1}%, CO2={CO2Percentage:F1}%, " +
            $"Breathable={IsBreathable}";
    }
}
