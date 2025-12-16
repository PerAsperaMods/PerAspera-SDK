// DOC REFERENCES:
// - BaseGame.md: OnEvent_WayDespawned, OnEvent_WayUpgraded, OnEvent_WayOperativeChanged

using PerAspera.GameAPI.Events.Core;

namespace PerAspera.GameAPI.Events.Native
{
    // ==================== WAY/ROAD EVENTS ====================
    // DOC: BaseGame.md - OnEvent_WayDespawned, OnEvent_WayUpgraded, OnEvent_WayOperativeChanged
    
    /// <summary>
    /// Native event: Way (road/path) despawned
    /// DOC: BaseGame.md - OnEvent_WayDespawned (Way sender, ref GameEvent evt)
    /// </summary>
    public class WayDespawnedNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:WayDespawned";
        
        public object? Way { get; set; }
        public string WayType { get; set; } = string.Empty;

        public override string ToString() => 
            $"WayDespawned: {WayType} - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Way upgraded
    /// DOC: BaseGame.md - OnEvent_WayUpgraded (Way sender, ref GameEvent evt)
    /// </summary>
    public class WayUpgradedNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:WayUpgraded";
        
        public object? Way { get; set; }
        public string PreviousType { get; set; } = string.Empty;
        public string NewType { get; set; } = string.Empty;

        public override string ToString() => 
            $"WayUpgraded: {PreviousType} → {NewType} - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Way operative status changed (active/inactive)
    /// DOC: BaseGame.md - OnEvent_WayOperativeChanged (Way sender, ref GameEvent evt)
    /// </summary>
    public class WayOperativeChangedNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:WayOperativeChanged";
        
        public object? Way { get; set; }
        public bool IsOperative { get; set; }

        public override string ToString() => 
            $"WayOperativeChanged: {(IsOperative ? "Active" : "Inactive")} - Sol {MartianSol}";
    }
}
