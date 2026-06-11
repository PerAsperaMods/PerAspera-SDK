namespace PerAspera.GameAPI.Events.Constants
{
    /// <summary>
    /// Event name constants for all native game events.
    /// Use these instead of magic strings when subscribing to events.
    /// </summary>
    public static class NativeEventConstants
    {
        // ==================== BUILDING EVENTS ====================
        
        /// <summary>Event: Building spawned/created</summary>
        public const string BuildingSpawned = "NativeBuildingSpawned";
        
        /// <summary>Event: Building destroyed/despawned</summary>
        public const string BuildingDespawned = "NativeBuildingDespawned";
        
        /// <summary>Event: Building upgraded or type changed</summary>
        public const string BuildingUpgraded = "NativeBuildingUpgraded";
        
        /// <summary>Event: Building finished scrapping</summary>
        public const string BuildingScrapped = "NativeBuildingScrapped";
        
        /// <summary>Event: Building state changed (operative/broken/powered)</summary>
        public const string BuildingStateChanged = "NativeBuildingStateChanged";

        // ==================== EXTENDED BUILDING EVENTS ====================
        
        // Internal lifecycle events
        /// <summary>Event: Building internal add (core system)</summary>
        public const string BuildingInternalAdd = "NativeBuildingInternalAdd";
        
        /// <summary>Event: Building internal add new (fresh creation)</summary>
        public const string BuildingInternalAddNew = "NativeBuildingInternalAddNew";
        
        /// <summary>Event: Building internal load (from save)</summary>
        public const string BuildingInternalLoad = "NativeBuildingInternalLoad";
        
        /// <summary>Event: Building internal remove</summary>
        public const string BuildingInternalRemove = "NativeBuildingInternalRemove";
        
        /// <summary>Event: Building pre-removal (before despawn)</summary>
        public const string BuildingPreRemove = "NativeBuildingPreRemove";
        
        /// <summary>Event: Building spawned spatial add</summary>
        public const string BuildingSpawnedSpatialAdd = "NativeBuildingSpawnedSpatialAdd";

        // Completion & lifecycle
        /// <summary>Event: Building completed construction</summary>
        public const string BuildingBuilt = "NativeBuildingBuilt";

        // Citizen events
        /// <summary>Event: Citizen born in building</summary>
        public const string BuildingCitizenBorn = "NativeBuildingCitizenBorn";
        
        /// <summary>Event: Citizen starving in building</summary>
        public const string BuildingCitizenStarving = "NativeBuildingCitizenStarving";
        
        /// <summary>Event: Citizen died in building</summary>
        public const string BuildingCitizenDied = "NativeBuildingCitizenDied";

        // Production events
        /// <summary>Event: Factory/building produced resource</summary>
        public const string BuildingFactoryProducedResource = "NativeBuildingFactoryProducedResource";

        // Building type changes
        /// <summary>Event: Building before changing type</summary>
        public const string BuildingBeforeChangeBuildingType = "NativeBuildingBeforeChangeBuildingType";

        // Damage & combat
        /// <summary>Event: Building was attacked</summary>
        public const string BuildingAttacked = "NativeBuildingAttacked";
        
        /// <summary>Event: Building destroyed by damage</summary>
        public const string BuildingDestroyedByDamage = "NativeBuildingDestroyedByDamage";
        
        /// <summary>Event: Building damaged by asteroid</summary>
        public const string BuildingDamagedByAsteroid = "NativeBuildingDamagedByAsteroid";

        // Power & operational state
        /// <summary>Event: Building ran out of power</summary>
        public const string BuildingOutOfPower = "NativeBuildingOutOfPower";
        
        /// <summary>Event: Building operational state changed</summary>
        public const string BuildingOperativeChanged = "NativeBuildingOperativeChanged";
        
        /// <summary>Event: Building audio property changed</summary>
        public const string BuildingAudioRelevantPropertyChanged = "NativeBuildingAudioRelevantPropertyChanged";

        // Cluster & spatial
        /// <summary>Event: Building cluster range changed</summary>
        public const string BuildingExtendsClusterRangeChanged = "NativeBuildingExtendsClusterRangeChanged";

        // Scrapping workflow
        /// <summary>Event: Building scrapping toggled</summary>
        public const string BuildingToggledScrapping = "NativeBuildingToggledScrapping";
        
        /// <summary>Event: Building scrapping cancelled</summary>
        public const string BuildingCanceledScrapping = "NativeBuildingCanceledScrapping";
        
        /// <summary>Event: Building started scrapping</summary>
        public const string BuildingStartedScrapping = "NativeBuildingStartedScrapping";

        // Rebuild & upgrade workflow
        /// <summary>Event: Building started rebuild</summary>
        public const string BuildingStartedRebuild = "NativeBuildingStartedRebuild";
        
        /// <summary>Event: Building upgrade cancelled</summary>
        public const string BuildingUpgradeCanceled = "NativeBuildingUpgradeCanceled";
        
        /// <summary>Event: Building upgrade started</summary>
        public const string BuildingUpgradeStarted = "NativeBuildingUpgradeStarted";
        
        /// <summary>Event: Building upgrade toggled</summary>
        public const string BuildingUpgradeToggled = "NativeBuildingUpgradeToggled";

        // District & zone
        /// <summary>Event: Building district changed active</summary>
        public const string BuildingDistrictChangedActive = "NativeBuildingDistrictChangedActive";
        
        // ==================== DRONE EVENTS ====================
        
        /// <summary>Event: Drone spawned/created</summary>
        public const string DroneSpawned = "NativeDroneSpawned";
        
        /// <summary>Event: Drone destroyed/despawned</summary>
        public const string DroneDespawned = "NativeDroneDespawned";
        
        /// <summary>Event: Drone started working on task</summary>
        public const string DroneStartedWork = "NativeDroneStartedWork";
        
        /// <summary>Event: Drone finished working on task</summary>
        public const string DroneFinishedWork = "NativeDroneFinishedWork";
        
        // ==================== CLIMATE EVENTS ====================
        
        /// <summary>Event: Any climate parameter changed (temperature, pressure, water)</summary>
        public const string ClimateChanged = "NativeClimateChanged";
        
        /// <summary>Event: Temperature changed</summary>
        public const string TemperatureChanged = "NativeTemperatureChanged";
        
        /// <summary>Event: CO2 pressure changed</summary>
        public const string CO2PressureChanged = "NativeCO2PressureChanged";
        
        /// <summary>Event: O2 pressure changed</summary>
        public const string O2PressureChanged = "NativeO2PressureChanged";
        
        /// <summary>Event: N2 pressure changed</summary>
        public const string N2PressureChanged = "NativeN2PressureChanged";
        
        /// <summary>Event: Total atmospheric pressure changed</summary>
        public const string TotalPressureChanged = "NativeTotalPressureChanged";
        
        /// <summary>Event: Water stock changed</summary>
        public const string WaterStockChanged = "NativeWaterStockChanged";
        
        /// <summary>Event: Greenhouse gas pressure changed</summary>
        public const string GHGPressureChanged = "NativeGHGPressureChanged";
        
        /// <summary>Event: Argon pressure changed</summary>
        public const string ArgonPressureChanged = "NativeArgonPressureChanged";
        
        // ==================== TIME EVENTS ====================
        
        /// <summary>Event: Martian day advanced (sol incremented)</summary>
        public const string MartianDayChanged = "MartianDayChanged";
        
        /// <summary>Event: Day progressed (time advanced)</summary>
        public const string DayProgressed = "NativeDayProgressed";
        
        // ==================== RESOURCE EVENTS ====================
        
        /// <summary>Event: Resource added to faction stockpile</summary>
        public const string ResourceAdded = "NativeResourceAdded";
        
        /// <summary>Event: Resource consumed from faction stockpile</summary>
        public const string ResourceConsumed = "NativeResourceConsumed";
        
        /// <summary>Event: Any resource change (add/remove)</summary>
        public const string ResourceChanged = "NativeResourceChanged";
        
        // ==================== GAME STATE EVENTS ====================
        
        /// <summary>Event: Game speed changed</summary>
        public const string GameSpeedChanged = "NativeGameSpeedChanged";
        
        /// <summary>Event: Game paused or unpaused</summary>
        public const string GamePauseChanged = "NativeGamePauseChanged";
        
        /// <summary>Event: Any game state changed</summary>
        public const string GameStateChanged = "NativeGameStateChanged";
        
        // ==================== FACTION EVENTS ====================
        
        /// <summary>Event: Faction created</summary>
        public const string FactionCreated = "NativeFactionCreated";
        
        /// <summary>Event: Faction destroyed</summary>
        public const string FactionDestroyed = "NativeFactionDestroyed";
        
        /// <summary>Event: Faction relation changed</summary>
        public const string FactionRelationChanged = "NativeFactionRelationChanged";
        
        // ==================== TECHNOLOGY EVENTS ====================
        
        /// <summary>Event: Technology researched</summary>
        public const string TechnologyResearched = "NativeTechnologyResearched";
        
        /// <summary>Event: Technology research started</summary>
        public const string TechnologyResearchStarted = "NativeTechnologyResearchStarted";
        
        // ==================== POI EVENTS ====================
        
        /// <summary>Event: Point of Interest discovered</summary>
        public const string POIDiscovered = "NativePOIDiscovered";
        
        /// <summary>Event: Point of Interest explored</summary>
        public const string POIExplored = "NativePOIExplored";
    }
}
