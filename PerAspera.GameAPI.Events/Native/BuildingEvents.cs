// DOC REFERENCES:
// - GameEvent.md: Native game event structure (Handle sender/target, GameEventType, GameEventPayload)
// - GameEventPayload.md: Payload fields (building, drone, faction, interactionAction, etc.)
// - BaseGame.md: OnEvent_* handlers (BuildingDespawned, BuildingUpgraded, WayDespawned, etc.)
// - Universe.md: Universe.gameEventBus (native event subscription system)
//
// ARCHITECTURE:
// - NativeGameEventBase: Base class for all native game event wrappers
// - Event classes wrap native GameEvent struct with strongly-typed data
// - GameEventPayload fields mapped to SDK-friendly properties
// - IL2CPP-safe: Uses System.Type, object references instead of dynamic

using PerAspera.GameAPI.Events.Core;

namespace PerAspera.GameAPI.Events.Native
{
    // ==================== BUILDING EVENTS ====================
    // DOC: BaseGame.md - OnEvent_BuildingDespawned, OnEvent_BuildingChangeBuildingTypeOrDeferredUpgradedTo, OnEvent_BuildingFinishScrapping
    
    /// <summary>
    /// Native event: Building spawned/created
    /// DOC: BaseGame.md - OnEvent_BuildingDespawned (Building sender, ref GameEvent evt)
    /// </summary>
    public class BuildingSpawnedNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingSpawned";
        
        /// <summary>Building instance that was spawned</summary>
        public object? Building { get; set; }
        
        /// <summary>Building type key (e.g., "SolarPanel", "Mine")</summary>
        public string BuildingTypeKey { get; set; } = string.Empty;
        
        /// <summary>Owner faction of the building</summary>
        public object? OwnerFaction { get; set; }
        
        /// <summary>Position X coordinate</summary>
        public float? PositionX { get; set; }
        
        /// <summary>Position Y coordinate</summary>
        public float? PositionY { get; set; }

        public override string ToString() => 
            $"BuildingSpawned: {BuildingTypeKey} at ({PositionX:F1}, {PositionY:F1}) - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Building destroyed/despawned
    /// DOC: BaseGame.md - OnEvent_BuildingDespawned (Building sender, ref GameEvent evt)
    /// </summary>
    public class BuildingDespawnedNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingDespawned";
        
        public object? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        public float? PositionX { get; set; }
        public float? PositionY { get; set; }

        public override string ToString() => 
            $"BuildingDespawned: {BuildingTypeKey} - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Building upgraded or type changed
    /// DOC: BaseGame.md - OnEvent_BuildingChangeBuildingTypeOrDeferredUpgradedTo (Building sender, ref GameEvent evt)
    /// </summary>
    public class BuildingUpgradedNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingUpgraded";
        
        public object? Building { get; set; }
        public string PreviousTypeKey { get; set; } = string.Empty;
        public string NewTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }

        public override string ToString() => 
            $"BuildingUpgraded: {PreviousTypeKey} → {NewTypeKey} - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Building finished scrapping
    /// DOC: BaseGame.md - OnEvent_BuildingFinishScrapping (Building sender, ref GameEvent evt)
    /// </summary>
    public class BuildingScrappedNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingScrapped";
        
        public object? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        
        /// <summary>Resources returned from scrapping (if any)</summary>
        public object? ResourcesReturned { get; set; }

        public override string ToString() => 
            $"BuildingScrapped: {BuildingTypeKey} - Sol {MartianSol}";
    }

    // ==================== UNIVERSE SPATIAL/LIFECYCLE BUILDING EVENTS ====================
    // DOC: Universe.md - OnEvent_BuildingPreRemove, OnEvent_BuildingSpawnedSpatialAdd
    
    /// <summary>
    /// Native event: Building pre-removal (before despawn)
    /// DOC: Universe.md - OnEvent_BuildingPreRemove (Building sender, ref GameEvent evt)
    /// Triggered BEFORE building is actually removed (cleanup phase)
    /// </summary>
    public class BuildingPreRemoveNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingPreRemove";
        
        public object? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        public float? PositionX { get; set; }
        public float? PositionY { get; set; }

        public override string ToString() => 
            $"BuildingPreRemove: {BuildingTypeKey} - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Building spawned spatial add (routing/pathfinding system)
    /// DOC: Universe.md - OnEvent_BuildingSpawnedSpatialAdd (Building sender, ref GameEvent evt)
    /// Triggered when building added to spatial grid (may differ from UI spawn)
    /// </summary>
    public class BuildingSpawnedSpatialAddNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingSpawnedSpatialAdd";
        
        public object? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        public float? PositionX { get; set; }
        public float? PositionY { get; set; }
        
        /// <summary>Added to spatial grid</summary>
        public bool AddedToSpatialGrid { get; set; }

        public override string ToString() => 
            $"BuildingSpatialAdd: {BuildingTypeKey} at ({PositionX:F1}, {PositionY:F1}) - Sol {MartianSol}";
    }
}
