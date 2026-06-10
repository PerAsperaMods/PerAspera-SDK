// DOC REFERENCES:
// - Universe.md: Game state management methods

using PerAspera.GameAPI.Events.Core;

namespace PerAspera.GameAPI.Events.Native
{
    // ==================== GAME STATE EVENTS ====================
    
    /// <summary>
    /// Native event: Game state changed (pause/speed/load/etc.)
    /// DOC: Universe.md - Game state management methods
    /// </summary>
    public class GameStateChangedNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:GameStateChanged";
        
        public string StateType { get; set; } = string.Empty; // "Pause", "Speed", "Load", "Save"
        public object? PreviousValue { get; set; }
        public object? CurrentValue { get; set; }

        public override string ToString() => 
            $"GameStateChanged: {StateType} {PreviousValue} → {CurrentValue} - Sol {MartianSol}";
    }
}
