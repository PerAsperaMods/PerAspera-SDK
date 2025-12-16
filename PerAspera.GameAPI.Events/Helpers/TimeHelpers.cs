using System;
using PerAspera.GameAPI.Events.Data;

namespace PerAspera.GameAPI.Events.Helpers
{
    /// <summary>
    /// Helper utilities for working with time and Martian calendar events
    /// </summary>
    public static class TimeHelpers
    {
        /// <summary>
        /// Calculate Martian year from sol
        /// </summary>
        public static int GetMartianYear(int sol)
        {
            return sol / 687; // Martian year = 687 sols
        }
        
        /// <summary>
        /// Calculate day within Martian year (0-686)
        /// </summary>
        public static int GetDayInYear(int sol)
        {
            return sol % 687;
        }
        
        /// <summary>
        /// Check if sol marks start of new Martian year
        /// </summary>
        public static bool IsNewMartianYear(int sol)
        {
            return sol > 0 && sol % 687 == 0;
        }
        
        /// <summary>
        /// Check if sol marks a milestone (every N sols)
        /// </summary>
        public static bool IsMilestone(int sol, int interval = 100)
        {
            return sol > 0 && sol % interval == 0;
        }
        
        /// <summary>
        /// Create a filter function for milestone sols
        /// </summary>
        public static Func<object, bool> MilestoneFilter(int interval = 100)
        {
            return (eventData) =>
            {
                if (EventHelpers.TryGetEventData<MartianDayEventData>(eventData, out var dayEvent))
                {
                    return IsMilestone(dayEvent.MartianSol, interval);
                }
                return false;
            };
        }
    }
}
