using System;
using PerAspera.Core;
using PerAspera.GameAPI.Events.Data;
using PerAspera.GameAPI.Events.Native;

namespace PerAspera.GameAPI.Events.Helpers
{
    /// <summary>
    /// General helper utilities for working with game events
    /// </summary>
    public static class EventHelpers
    {
        private static readonly LogAspera Log = new LogAspera("EventHelpers");
        
        // ==================== TYPE CHECKING ====================
        
        /// <summary>
        /// Check if event data is of a specific type and cast it
        /// </summary>
        public static bool TryGetEventData<T>(object eventData, out T? result) where T : class
        {
            if (eventData is T typed)
            {
                result = typed;
                return true;
            }
            
            result = null;
            return false;
        }
        
        /// <summary>
        /// Safely cast event data to expected type
        /// </summary>
        public static T? AsEventData<T>(object eventData) where T : class
        {
            return eventData as T;
        }
        
        // ==================== RESOURCE EVENT HELPERS ====================
        
        /// <summary>
        /// Check if resource operation is an addition
        /// </summary>
        public static bool IsResourceAddition(string operation)
        {
            return operation.Equals("Add", StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Check if resource operation is a removal
        /// </summary>
        public static bool IsResourceRemoval(string operation)
        {
            return operation.Equals("Remove", StringComparison.OrdinalIgnoreCase);
        }
        
        // ==================== EVENT FILTERING ====================
        
        /// <summary>
        /// Create a filter function for specific building types
        /// </summary>
        public static Func<object, bool> BuildingTypeFilter(params string[] buildingTypes)
        {
            return (eventData) =>
            {
                if (TryGetEventData<BuildingSpawnedNativeEvent>(eventData, out var building))
                {
                    foreach (var type in buildingTypes)
                    {
                        if (building.BuildingTypeKey.Equals(type, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                }
                return false;
            };
        }
        
        // ==================== LOGGING HELPERS ====================
        
        /// <summary>
        /// Log climate event with formatted output
        /// </summary>
        public static void LogClimateEvent(ClimateEventData climate, string prefix = "")
        {
            string arrow = ClimateHelpers.IsClimateIncrease(climate) ? "↑" : 
                          ClimateHelpers.IsClimateDecrease(climate) ? "↓" : "=";
            float delta = ClimateHelpers.GetClimateDelta(climate);
            
            string message = string.IsNullOrEmpty(prefix) 
                ? $"{climate.EventType}: {climate.PreviousValue:F2} {arrow} {climate.CurrentValue:F2} (Δ{delta:+0.00;-0.00}) - Sol {climate.MartianSol}"
                : $"{prefix} - {climate.EventType}: {climate.CurrentValue:F2} (Δ{delta:+0.00;-0.00})";
            
            Log.Info(message);
        }
        
        /// <summary>
        /// Log Martian day event with year calculation
        /// </summary>
        public static void LogDayEvent(MartianDayEventData dayEvent, string prefix = "")
        {
            int year = TimeHelpers.GetMartianYear(dayEvent.MartianSol);
            int dayInYear = TimeHelpers.GetDayInYear(dayEvent.MartianSol);
            
            string message = string.IsNullOrEmpty(prefix)
                ? $"Martian Day: Sol {dayEvent.MartianSol} (Year {year}, Day {dayInYear})"
                : $"{prefix} - Sol {dayEvent.MartianSol} (Year {year})";
            
            Log.Info(message);
        }
    }
}
