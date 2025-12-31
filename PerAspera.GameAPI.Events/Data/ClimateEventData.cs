using System;
using PerAspera.GameAPI.Events.Core;
using PerAspera.GameAPI.Wrappers;

namespace PerAspera.GameAPI.Events.Data
{
    /// <summary>
    /// Climate change event data
    /// Uses the Atmosphere wrapper for current atmospheric state
    /// </summary>
    public class ClimateEventData : GameEventBase
    {
        public override string EventType => "ClimateEvent";

        // === ATMOSPHERIC STATE ===
        /// <summary>
        /// Current atmospheric state (from Wrappers.Atmosphere)
        /// COMMENTED OUT: Using cellular atmosphere architecture instead
        /// </summary>
        //public Wrappers.Atmosphere? AtmosphericState { get; set; }

        // === CHANGE TRACKING ===
        public float? PreviousValue { get; set; }
        public float CurrentValue { get; set; }
        
        // Delta can be calculated or manually set for legacy compatibility
        private float? _delta;
        public float? Delta 
        { 
            get => _delta ?? (CurrentValue - PreviousValue);
            set => _delta = value;
        }

        // === METADATA ===
        public string ClimateType { get; set; } = string.Empty;
        public string Units { get; set; } = string.Empty;
        public int MartianSol { get; set; }

        // === NATIVE INTEGRATION ===
        public object? Payload { get; set; }
        public object? NativeEventType { get; set; }
        
        // === CONVENIENCE ACCESSORS (manual override only - cellular architecture) ===
        private float? _temperature;
        public float Temperature 
        { 
            get => _temperature ?? CurrentValue;
            set => _temperature = value;
        }
        
        private float? _totalPressure;
        public float TotalPressure 
        { 
            get => _totalPressure ?? 0f;
            set => _totalPressure = value;
        }
        
        public float CO2Pressure 
        { 
            get => _co2Pressure ?? 0f;
            set => _co2Pressure = value;
        }
        private float? _co2Pressure;
        
        public float O2Pressure 
        { 
            get => _o2Pressure ?? 0f;
            set => _o2Pressure = value;
        }
        private float? _o2Pressure;
        
        public float N2Pressure 
        { 
            get => _n2Pressure ?? 0f;
            set => _n2Pressure = value;
        }
        private float? _n2Pressure;
        
        public float GHGPressure 
        { 
            get => _ghgPressure ?? 0f;
            set => _ghgPressure = value;
        }
        private float? _ghgPressure;
        
        private float? _waterStock;
        public float WaterStock 
        { 
            get => _waterStock ?? 0f;
            set => _waterStock = value;
        }
        
        public ClimateEventData() { }
        
        public ClimateEventData(string climateType)
        {
            ClimateType = climateType;
            Timestamp = DateTime.Now;
        }
        
        public ClimateEventData(string climateType, float currentValue, float? previousValue = null)
        {
            ClimateType = climateType;
            CurrentValue = currentValue;
            PreviousValue = previousValue;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"ClimateEvent: {ClimateType} - Current: {CurrentValue:F2} (Δ {Delta:F2}) [{Units}]";
        }
    }
}
