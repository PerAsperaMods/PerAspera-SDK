using System;
using PerAspera.GameAPI.Events.Data;

namespace PerAspera.GameAPI.Events.Helpers
{
    /// <summary>
    /// Helper utilities for working with climate events
    /// </summary>
    public static class ClimateHelpers
    {
        /// <summary>
        /// Check if climate value changed significantly (> threshold)
        /// </summary>
        public static bool IsSignificantChange(ClimateEventData climate, float threshold = 0.1f)
        {
            if (climate.PreviousValue == null) return true;
            
            float delta = Math.Abs(climate.CurrentValue - (float)climate.PreviousValue.Value);
            return delta >= threshold;
        }
        
        /// <summary>
        /// Get climate change delta (positive = increase, negative = decrease)
        /// </summary>
        public static float GetClimateDelta(ClimateEventData climate)
        {
            if (climate.PreviousValue == null) return 0f;
            return climate.CurrentValue - (float)climate.PreviousValue.Value;
        }
        
        /// <summary>
        /// Check if climate parameter increased
        /// </summary>
        public static bool IsClimateIncrease(ClimateEventData climate)
        {
            return GetClimateDelta(climate) > 0f;
        }
        
        /// <summary>
        /// Check if climate parameter decreased
        /// </summary>
        public static bool IsClimateDecrease(ClimateEventData climate)
        {
            return GetClimateDelta(climate) < 0f;
        }
        
        /// <summary>
        /// Create a filter function for climate events above threshold
        /// </summary>
        public static Func<object, bool> ClimateThresholdFilter(float threshold)
        {
            return (eventData) =>
            {
                if (EventHelpers.TryGetEventData<ClimateEventData>(eventData, out var climate))
                {
                    return IsSignificantChange(climate, threshold);
                }
                return false;
            };
        }
    }
}
