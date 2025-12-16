namespace PerAspera.GameAPI.Models
{
    /// <summary>
    /// Terraforming progress status
    /// Tracks phase completion and habitability metrics
    /// </summary>
    public class TerraformingStatus
    {
        /// <summary>Current temperature phase</summary>
        public string TemperaturePhase { get; set; } = "Unknown";
        
        /// <summary>Current atmospheric pressure phase</summary>
        public string PressurePhase { get; set; } = "Unknown";
        
        /// <summary>Current oxygenation phase</summary>
        public string OxygenPhase { get; set; } = "Unknown";
        
        /// <summary>Overall habitability score (0-100%)</summary>
        public float Habitability { get; set; }
        
        /// <summary>Temperature score component (0-100%)</summary>
        public float TemperatureScore { get; set; }
        
        /// <summary>Pressure score component (0-100%)</summary>
        public float PressureScore { get; set; }
        
        /// <summary>Oxygen score component (0-100%)</summary>
        public float OxygenScore { get; set; }
        
        /// <summary>CO2 safety score component (0-100%)</summary>
        public float CO2SafetyScore { get; set; }
        
        /// <summary>Is planet currently habitable without life support?</summary>
        public bool IsHabitable => Habitability >= 80f;
        
        /// <summary>Recommended next action for terraforming</summary>
        public string RecommendedAction { get; set; } = "Continue monitoring";
        
        public override string ToString() =>
            $"Terraforming: {Habitability:F1}% habitable | " +
            $"Temp: {TemperaturePhase} | " +
            $"Pressure: {PressurePhase} | " +
            $"O2: {OxygenPhase}";
    }
}
