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
