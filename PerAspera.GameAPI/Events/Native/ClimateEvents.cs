using System;
using PerAspera.GameAPI.Events.Core;
using PerAspera.GameAPI.Events.Data;

namespace PerAspera.GameAPI.Events.Native
{
    /// <summary>
    /// Native climate events from Planet class
    /// Triggered by NativeEventPatcher when Planet setter methods are called
    /// DOC: Planet.md - climate fields and setter methods
    /// </summary>
    public static class ClimateEvents
    {
        /// <summary>Event key for temperature changes (SetAverageTemperature, SetTemperature)</summary>
        public const string TemperatureChanged = "NativeTemperatureChanged";
        
        /// <summary>Event key for CO2 pressure changes (SetCO2Pressure)</summary>
        public const string CO2PressureChanged = "NativeCO2PressureChanged";
        
        /// <summary>Event key for O2 pressure changes (SetO2Pressure, SetOxygenPressure)</summary>
        public const string O2PressureChanged = "NativeO2PressureChanged";
        
        /// <summary>Event key for N2 pressure changes (SetN2Pressure, SetNitrogenPressure)</summary>
        public const string N2PressureChanged = "NativeN2PressureChanged";
        
        /// <summary>Event key for GHG pressure changes (SetGHGPressure, SetGreenhouseGasPressure)</summary>
        public const string GHGPressureChanged = "NativeGHGPressureChanged";
        
        /// <summary>Event key for total pressure changes (SetTotalPressure, SetPressure)</summary>
        public const string TotalPressureChanged = "NativeTotalPressureChanged";
        
        /// <summary>Event key for water stock changes (SetWaterStock, SetWater)</summary>
        public const string WaterStockChanged = "NativeWaterStockChanged";
        
        /// <summary>Event key for argon pressure changes (SetArgonPressure)</summary>
        public const string ArgonPressureChanged = "NativeArgonPressureChanged";
        
        /// <summary>
        /// Create climate event data for temperature change
        /// </summary>
        public static ClimateEventData CreateTemperatureEvent(float oldValue, float newValue, int sol)
        {
            return new ClimateEventData("Temperature")
            {
                PreviousValue = oldValue,
                CurrentValue = newValue,
                Delta = newValue - oldValue,
                Temperature = newValue,
                Units = "K",
                MartianSol = sol,
                Timestamp = DateTime.UtcNow
            };
        }
        
        /// <summary>
        /// Create climate event data for CO2 pressure change
        /// </summary>
        public static ClimateEventData CreateCO2Event(float oldValue, float newValue, int sol)
        {
            return new ClimateEventData("CO2Pressure")
            {
                PreviousValue = oldValue,
                CurrentValue = newValue,
                Delta = newValue - oldValue,
                CO2Pressure = newValue,
                Units = "kPa",
                MartianSol = sol,
                Timestamp = DateTime.UtcNow
            };
        }
        
        /// <summary>
        /// Create climate event data for O2 pressure change
        /// </summary>
        public static ClimateEventData CreateO2Event(float oldValue, float newValue, int sol)
        {
            return new ClimateEventData("O2Pressure")
            {
                PreviousValue = oldValue,
                CurrentValue = newValue,
                Delta = newValue - oldValue,
                O2Pressure = newValue,
                Units = "kPa",
                MartianSol = sol,
                Timestamp = DateTime.UtcNow
            };
        }
        
        /// <summary>
        /// Create climate event data for N2 pressure change
        /// </summary>
        public static ClimateEventData CreateN2Event(float oldValue, float newValue, int sol)
        {
            return new ClimateEventData("N2Pressure")
            {
                PreviousValue = oldValue,
                CurrentValue = newValue,
                Delta = newValue - oldValue,
                N2Pressure = newValue,
                Units = "kPa",
                MartianSol = sol,
                Timestamp = DateTime.UtcNow
            };
        }
        
        /// <summary>
        /// Create climate event data for GHG pressure change
        /// </summary>
        public static ClimateEventData CreateGHGEvent(float oldValue, float newValue, int sol)
        {
            return new ClimateEventData("GHGPressure")
            {
                PreviousValue = oldValue,
                CurrentValue = newValue,
                Delta = newValue - oldValue,
                GHGPressure = newValue,
                Units = "kPa",
                MartianSol = sol,
                Timestamp = DateTime.UtcNow
            };
        }
        
        /// <summary>
        /// Create climate event data for water stock change
        /// </summary>
        public static ClimateEventData CreateWaterStockEvent(float oldValue, float newValue, int sol)
        {
            return new ClimateEventData("WaterStock")
            {
                PreviousValue = oldValue,
                CurrentValue = newValue,
                Delta = newValue - oldValue,
                WaterStock = newValue,
                Units = "Gt",
                MartianSol = sol,
                Timestamp = DateTime.UtcNow
            };
        }
        
        /// <summary>
        /// Create climate event data for total pressure change
        /// </summary>
        public static ClimateEventData CreateTotalPressureEvent(float oldValue, float newValue, int sol)
        {
            return new ClimateEventData("TotalPressure")
            {
                PreviousValue = oldValue,
                CurrentValue = newValue,
                Delta = newValue - oldValue,
                TotalPressure = newValue,
                Units = "atm",
                MartianSol = sol,
                Timestamp = DateTime.UtcNow
            };
        }
    }
}
