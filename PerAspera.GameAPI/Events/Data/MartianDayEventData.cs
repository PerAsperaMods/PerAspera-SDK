using System;
using PerAspera.GameAPI.Events.Core;

namespace PerAspera.GameAPI.Events.Data
{
    /// <summary>
    /// Martian day change event data
    /// Fired when a new Martian sol begins (GevUniverseDayPassed)
    /// </summary>
    public class MartianDayEventData : GameEventBase
    {
        public override string EventType => "MartianDayPassed";

        // === SOL TRACKING ===
        public int CurrentSol { get; set; }
        public int MartianYear { get; set; }
        public int MartianSol { get; set; }
        public int PreviousSol { get; set; }
        public bool IsNewYear { get; set; }

        // === SEASON TRACKING ===
        public int Season { get; set; }
        public float DayLength { get; set; } = 24.6f; // Martian sol in Earth hours

        // === NATIVE INTEGRATION ===
        public object? Payload { get; set; }
        public object? Planet { get; set; }
        
        public MartianDayEventData()
        {
            Timestamp = DateTime.Now;
        }
        
        public MartianDayEventData(int currentSol, int martianYear)
        {
            CurrentSol = currentSol;
            MartianSol = currentSol;
            MartianYear = martianYear;
            PreviousSol = currentSol - 1;
            IsNewYear = (currentSol % 687 == 1);
            Season = (currentSol % 687) / (687 / 4); // 4 seasons per Martian year
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            return $"MartianDayPassed: Sol {MartianSol} of Year {MartianYear} (Season {Season}) {(IsNewYear ? "🎉 NEW YEAR" : "")}";
        }
    }
}
