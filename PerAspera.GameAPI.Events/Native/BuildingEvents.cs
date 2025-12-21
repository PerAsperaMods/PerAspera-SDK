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
using PerAspera.GameAPI.Wrappers;
using PerAspera.GameAPI.Events.Helpers;

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
        
        /// <summary>Building instance that was spawned (SDK wrapper)</summary>
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        
        /// <summary>Building type key (e.g., "SolarPanel", "Mine")</summary>
        public string BuildingTypeKey { get; set; } = string.Empty;
        
        /// <summary>Owner faction of the building (SDK wrapper)</summary>
        public GameAPI.Wrappers.FactionWrapper? OwnerFaction { get; set; }
        
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
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public GameAPI.Wrappers.FactionWrapper? OwnerFaction { get; set; }
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
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string PreviousTypeKey { get; set; } = string.Empty;
        public string NewTypeKey { get; set; } = string.Empty;
        public GameAPI.Wrappers.FactionWrapper? OwnerFaction { get; set; }

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
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public GameAPI.Wrappers.FactionWrapper? OwnerFaction { get; set; }
        
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
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
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
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        public float? PositionX { get; set; }
        public float? PositionY { get; set; }
        
        /// <summary>Added to spatial grid</summary>
        public bool AddedToSpatialGrid { get; set; }

        public override string ToString() => 
            $"BuildingSpatialAdd: {BuildingTypeKey} at ({PositionX:F1}, {PositionY:F1}) - Sol {MartianSol}";
    }

    // ==================== INTERNAL LIFECYCLE BUILDING EVENTS ====================
    // DOC: GameEventType.cs - GevBuildingInternalAdd, GevBuildingInternalAddNew, GevBuildingInternalLoad, GevBuildingInternalRemove
    
    /// <summary>
    /// Native event: Building internal add (core game system)
    /// Maps to: GevBuildingInternalAdd
    /// </summary>
    public class BuildingInternalAddNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingInternalAdd";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        public float? PositionX { get; set; }
        public float? PositionY { get; set; }

        public override string ToString() => 
            $"BuildingInternalAdd: {BuildingTypeKey} - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Building internal add new (fresh building creation)
    /// Maps to: GevBuildingInternalAddNew
    /// </summary>
    public class BuildingInternalAddNewNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingInternalAddNew";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public GameAPI.Wrappers.FactionWrapper? OwnerFaction { get; set; }
        public float? PositionX { get; set; }
        public float? PositionY { get; set; }

        public override string ToString() => 
            $"BuildingInternalAddNew: {BuildingTypeKey} - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Building internal load (loading from save)
    /// Maps to: GevBuildingInternalLoad
    /// </summary>
    public class BuildingInternalLoadNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingInternalLoad";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        public float? PositionX { get; set; }
        public float? PositionY { get; set; }

        public override string ToString() => 
            $"BuildingInternalLoad: {BuildingTypeKey} - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Building internal remove (core game system)
    /// Maps to: GevBuildingInternalRemove
    /// </summary>
    public class BuildingInternalRemoveNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingInternalRemove";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        public float? PositionX { get; set; }
        public float? PositionY { get; set; }

        public override string ToString() => 
            $"BuildingInternalRemove: {BuildingTypeKey} - Sol {MartianSol}";
    }

    // ==================== BUILDING COMPLETION & LIFECYCLE EVENTS ====================
    
    /// <summary>
    /// Native event: Building completed construction (built and ready)
    /// Maps to: GevBuildingBuilt
    /// Different from Spawned - this is when construction finishes
    /// </summary>
    public class BuildingBuiltNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingBuilt";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        public float? PositionX { get; set; }
        public float? PositionY { get; set; }
        
        /// <summary>Construction time in game ticks</summary>
        public long? ConstructionTime { get; set; }

        public override string ToString() => 
            $"BuildingBuilt: {BuildingTypeKey} at ({PositionX:F1}, {PositionY:F1}) - Sol {MartianSol}";
    }

    // ==================== CITIZEN LIFECYCLE EVENTS ====================
    // DOC: GameEventType.cs - GevBuildingCitizenBorn, GevBuildingCitizenStarving, GevBuildingCitizenDied
    
    /// <summary>
    /// Native event: Citizen born in a building
    /// Maps to: GevBuildingCitizenBorn
    /// </summary>
    public class BuildingCitizenBornNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingCitizenBorn";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        
        /// <summary>Citizen information (wrapped if possible)</summary>
        public object? CitizenInstance { get; set; }
        
        /// <summary>Population count after birth</summary>
        public int? PopulationCount { get; set; }

        public override string ToString() => 
            $"BuildingCitizenBorn: {BuildingTypeKey} (Pop: {PopulationCount}) - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Citizen starving in a building
    /// Maps to: GevBuildingCitizenStarving
    /// </summary>
    public class BuildingCitizenStarvingNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingCitizenStarving";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        
        public object? CitizenInstance { get; set; }
        
        /// <summary>Missing resource causing starvation</summary>
        public string? MissingResource { get; set; }
        
        /// <summary>Starvation duration in ticks</summary>
        public long? StarvationTime { get; set; }

        public override string ToString() => 
            $"BuildingCitizenStarving: {BuildingTypeKey} (Missing: {MissingResource}) - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Citizen died in a building
    /// Maps to: GevBuildingCitizenDied
    /// </summary>
    public class BuildingCitizenDiedNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingCitizenDied";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        
        public object? CitizenInstance { get; set; }
        
        /// <summary>Cause of death</summary>
        public string? CauseOfDeath { get; set; }
        
        /// <summary>Population count after death</summary>
        public int? PopulationCount { get; set; }

        public override string ToString() => 
            $"BuildingCitizenDied: {BuildingTypeKey} ({CauseOfDeath}, Pop: {PopulationCount}) - Sol {MartianSol}";
    }

    // ==================== PRODUCTION & RESOURCE EVENTS ====================
    
    /// <summary>
    /// Native event: Factory/building produced a resource
    /// Maps to: GevFactoryProducedResource
    /// </summary>
    public class BuildingFactoryProducedResourceNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingFactoryProducedResource";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public GameAPI.Wrappers.FactionWrapper? OwnerFaction { get; set; }
        
        /// <summary>Resource type that was produced (SDK ResourceType wrapper)</summary>
        public GameAPI.Wrappers.ResourceTypeWrapper? ProducedResourceType { get; set; }
        
        /// <summary>Resource type key (e.g., "water", "silicon")</summary>
        public string ProducedResourceKey { get; set; } = string.Empty;
        
        /// <summary>Amount produced</summary>
        public float? AmountProduced { get; set; }
        
        /// <summary>Production efficiency at time of production</summary>
        public float? ProductionEfficiency { get; set; }

        public override string ToString() => 
            $"BuildingFactoryProducedResource: {BuildingTypeKey} produced {AmountProduced:F2} {ProducedResourceKey} - Sol {MartianSol}";
    }

    // ==================== BUILDING TYPE CHANGE EVENTS ====================
    
    /// <summary>
    /// Native event: Building before changing building type
    /// Maps to: GevBuildingBeforeChangeBuildingType
    /// Triggered BEFORE the type change happens
    /// </summary>
    public class BuildingBeforeChangeBuildingTypeNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingBeforeChangeBuildingType";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string CurrentBuildingTypeKey { get; set; } = string.Empty;
        public string TargetBuildingTypeKey { get; set; } = string.Empty;
        public GameAPI.Wrappers.FactionWrapper? OwnerFaction { get; set; }

        public override string ToString() => 
            $"BuildingBeforeChangeType: {CurrentBuildingTypeKey} → {TargetBuildingTypeKey} - Sol {MartianSol}";
    }

    // ==================== DAMAGE & COMBAT EVENTS ====================
    
    /// <summary>
    /// Native event: Building was attacked
    /// Maps to: GevBuildingAttacked
    /// </summary>
    public class BuildingAttackedNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingAttacked";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public GameAPI.Wrappers.FactionWrapper? OwnerFaction { get; set; }
        
        /// <summary>Attacking faction/entity (could be Faction or other entity)</summary>
        public GameAPI.Wrappers.FactionWrapper? Attacker { get; set; }
        
        /// <summary>Damage amount dealt</summary>
        public float? DamageAmount { get; set; }
        
        /// <summary>Building health after attack</summary>
        public float? HealthAfterAttack { get; set; }

        public override string ToString() => 
            $"BuildingAttacked: {BuildingTypeKey} took {DamageAmount:F1} damage - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Building destroyed by damage
    /// Maps to: GevBuildingDestroyedByDamage
    /// </summary>
    public class BuildingDestroyedByDamageNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingDestroyedByDamage";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        
        /// <summary>Source of damage that destroyed building</summary>
        public string? DamageSource { get; set; }
        
        /// <summary>Final damage amount that caused destruction</summary>
        public float? FinalDamageAmount { get; set; }

        public override string ToString() => 
            $"BuildingDestroyedByDamage: {BuildingTypeKey} destroyed by {DamageSource} - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Building damaged by asteroid
    /// Maps to: GevBuildingDamagedByAsteroid
    /// </summary>
    public class BuildingDamagedByAsteroidNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingDamagedByAsteroid";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        
        /// <summary>Asteroid impact information</summary>
        public object? AsteroidInstance { get; set; }
        
        /// <summary>Damage dealt by asteroid</summary>
        public float? AsteroidDamage { get; set; }
        
        /// <summary>Impact coordinates</summary>
        public float? ImpactX { get; set; }
        public float? ImpactY { get; set; }

        public override string ToString() => 
            $"BuildingDamagedByAsteroid: {BuildingTypeKey} hit by asteroid ({AsteroidDamage:F1} damage) - Sol {MartianSol}";
    }

    // ==================== POWER & OPERATIONAL STATE EVENTS ====================
    
    /// <summary>
    /// Native event: Building ran out of power
    /// Maps to: GevBuildingOutOfPower
    /// </summary>
    public class BuildingOutOfPowerNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingOutOfPower";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        
        /// <summary>Power demand when outage occurred</summary>
        public float? PowerDemand { get; set; }
        
        /// <summary>Available power when outage occurred</summary>
        public float? AvailablePower { get; set; }

        public override string ToString() => 
            $"BuildingOutOfPower: {BuildingTypeKey} (Need: {PowerDemand:F1}, Available: {AvailablePower:F1}) - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Building operational state changed
    /// Maps to: GevBuildingOperativeChanged
    /// </summary>
    public class BuildingOperativeChangedNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingOperativeChanged";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        
        /// <summary>Previous operational state</summary>
        public bool? WasOperational { get; set; }
        
        /// <summary>Current operational state</summary>
        public bool? IsOperational { get; set; }
        
        /// <summary>Reason for state change</summary>
        public string? StateChangeReason { get; set; }

        public override string ToString() => 
            $"BuildingOperativeChanged: {BuildingTypeKey} {(IsOperational == true ? "ONLINE" : "OFFLINE")} ({StateChangeReason}) - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Building audio-relevant property changed
    /// Maps to: GevBuildingAudioRelevantPropertyChanged
    /// </summary>
    public class BuildingAudioRelevantPropertyChangedNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingAudioRelevantPropertyChanged";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        
        /// <summary>Property that changed (for audio system)</summary>
        public string? ChangedProperty { get; set; }
        
        /// <summary>Previous value</summary>
        public object? PreviousValue { get; set; }
        
        /// <summary>New value</summary>
        public object? NewValue { get; set; }

        public override string ToString() => 
            $"BuildingAudioPropertyChanged: {BuildingTypeKey} {ChangedProperty} changed - Sol {MartianSol}";
    }

    // ==================== CLUSTER RANGE & SPATIAL EVENTS ====================
    
    /// <summary>
    /// Native event: Building cluster range extension changed
    /// Maps to: GevBuildingExtendsClusterRangeChanged
    /// </summary>
    public class BuildingExtendsClusterRangeChangedNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingExtendsClusterRangeChanged";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        
        /// <summary>Previous cluster range</summary>
        public float? PreviousRange { get; set; }
        
        /// <summary>New cluster range</summary>
        public float? NewRange { get; set; }
        
        /// <summary>Affected cluster</summary>
        public object? AffectedCluster { get; set; }

        public override string ToString() => 
            $"BuildingClusterRangeChanged: {BuildingTypeKey} range {PreviousRange:F1} → {NewRange:F1} - Sol {MartianSol}";
    }

    // ==================== SCRAPPING WORKFLOW EVENTS ====================
    
    /// <summary>
    /// Native event: Building scrapping toggled
    /// Maps to: GevBuildingToggledScrapping
    /// </summary>
    public class BuildingToggledScrappingNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingToggledScrapping";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        
        /// <summary>Whether scrapping was enabled or disabled</summary>
        public bool? ScrappingEnabled { get; set; }

        public override string ToString() => 
            $"BuildingToggledScrapping: {BuildingTypeKey} scrapping {(ScrappingEnabled == true ? "ENABLED" : "DISABLED")} - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Building scrapping cancelled
    /// Maps to: GevBuildingCanceledScrapping
    /// </summary>
    public class BuildingCanceledScrappingNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingCanceledScrapping";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        
        /// <summary>Reason for cancellation</summary>
        public string? CancellationReason { get; set; }

        public override string ToString() => 
            $"BuildingCanceledScrapping: {BuildingTypeKey} ({CancellationReason}) - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Building started scrapping
    /// Maps to: GevBuildingStartedScrapping
    /// </summary>
    public class BuildingStartedScrappingNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingStartedScrapping";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        
        /// <summary>Expected scrapping time</summary>
        public long? EstimatedScrappingTime { get; set; }

        public override string ToString() => 
            $"BuildingStartedScrapping: {BuildingTypeKey} (Est. {EstimatedScrappingTime} ticks) - Sol {MartianSol}";
    }

    // ==================== REBUILD & UPGRADE WORKFLOW EVENTS ====================
    
    /// <summary>
    /// Native event: Building started rebuild process
    /// Maps to: GevBuildingStartedRebuild
    /// </summary>
    public class BuildingStartedRebuildNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingStartedRebuild";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public string TargetBuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        
        /// <summary>Expected rebuild time</summary>
        public long? EstimatedRebuildTime { get; set; }

        public override string ToString() => 
            $"BuildingStartedRebuild: {BuildingTypeKey} → {TargetBuildingTypeKey} - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Building upgrade cancelled
    /// Maps to: GevBuildingUpgradeCanceled
    /// </summary>
    public class BuildingUpgradeCanceledNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingUpgradeCanceled";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public string TargetBuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        
        /// <summary>Reason for upgrade cancellation</summary>
        public string? CancellationReason { get; set; }

        public override string ToString() => 
            $"BuildingUpgradeCanceled: {BuildingTypeKey} → {TargetBuildingTypeKey} ({CancellationReason}) - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Building upgrade started
    /// Maps to: GevBuildingUpgradeStarted
    /// </summary>
    public class BuildingUpgradeStartedNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingUpgradeStarted";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public string TargetBuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        
        /// <summary>Expected upgrade time</summary>
        public long? EstimatedUpgradeTime { get; set; }
        
        /// <summary>Resources required for upgrade</summary>
        public object? RequiredResources { get; set; }

        public override string ToString() => 
            $"BuildingUpgradeStarted: {BuildingTypeKey} → {TargetBuildingTypeKey} - Sol {MartianSol}";
    }
    
    /// <summary>
    /// Native event: Building upgrade toggled
    /// Maps to: GevBuildingUpgradeToggled
    /// </summary>
    public class BuildingUpgradeToggledNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingUpgradeToggled";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        
        /// <summary>Whether upgrade queue was enabled or disabled</summary>
        public bool? UpgradeEnabled { get; set; }
        
        /// <summary>Target upgrade type</summary>
        public string? TargetUpgradeType { get; set; }

        public override string ToString() => 
            $"BuildingUpgradeToggled: {BuildingTypeKey} upgrade {(UpgradeEnabled == true ? "ENABLED" : "DISABLED")} - Sol {MartianSol}";
    }

    // ==================== DISTRICT & ZONE EVENTS ====================
    
    /// <summary>
    /// Native event: Building district changed active state
    /// Maps to: GevBuildingDistrictChangedActive
    /// </summary>
    public class BuildingDistrictChangedActiveNativeEvent : NativeGameEventBase
    {
        public override string EventType => "Native:BuildingDistrictChangedActive";
        
        public GameAPI.Wrappers.BuildingWrapper? Building { get; set; }
        public string BuildingTypeKey { get; set; } = string.Empty;
        public object? OwnerFaction { get; set; }
        
        /// <summary>District that changed</summary>
        public object? District { get; set; }
        
        /// <summary>Previous active state</summary>
        public bool? WasActive { get; set; }
        
        /// <summary>Current active state</summary>
        public bool? IsActive { get; set; }

        public override string ToString() => 
            $"BuildingDistrictChangedActive: {BuildingTypeKey} district {(IsActive == true ? "ACTIVATED" : "DEACTIVATED")} - Sol {MartianSol}";
    }
}
