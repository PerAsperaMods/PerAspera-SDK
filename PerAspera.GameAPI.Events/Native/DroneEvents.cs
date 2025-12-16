// DOC REFERENCES:
// - BaseGame.md: lambda handlers _SubscribeEventHandlers_b__198_1/2 (Drone sender, ref GameEvent evt)

using PerAspera.GameAPI.Events.Core;

namespace PerAspera.GameAPI.Events.Native
{
    // ==================== DRONE EVENTS ====================
    // DOC: BaseGame.md - lambda handlers _SubscribeEventHandlers_b__198_1/2 (Drone sender, ref GameEvent evt)
    
    /// <summary>
    /// Native event: Drone spawned/created
    /// DOC: BaseGame.md - Drone event handlers (lambda subscriptions)
    /// </summary>
    public class DroneSpawnedNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:DroneSpawned";
        
        public object? Drone { get; set; }
        public string DroneType { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }

        public override string ToString() => 
            $"DroneSpawned: {DroneType} - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Drone despawned/destroyed
    /// </summary>
    public class DroneDespawnedNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:DroneDespawned";
        
        public object? Drone { get; set; }
        public string DroneType { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }

        public override string ToString() => 
            $"DroneDespawned: {DroneType} - Sol {MartianSol}";
    }
}
