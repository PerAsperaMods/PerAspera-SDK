// DOC REFERENCES:
// - GameEventPayload.md: payload with resource data

using PerAspera.GameAPI.Events.Core;

namespace PerAspera.GameAPI.Events.Native
{
    // ==================== RESOURCE EVENTS ====================
    
    /// <summary>
    /// Native event: Resource amount changed (added/removed/set)
    /// DOC: GameEventPayload.md - payload with resource data
    /// </summary>
    public class ResourceChangedNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:ResourceChanged";
        
        public string ResourceKey { get; set; } = string.Empty;
        public float Amount { get; set; }
        public float? PreviousAmount { get; set; }
        public string Operation { get; set; } = string.Empty; // "Add", "Remove", "Set"
        public object? SourceFaction { get; set; }

        public override string ToString() => 
            $"ResourceChanged: {Operation} {Amount:F2} {ResourceKey} - Sol {MartianSol}";
    }
}
