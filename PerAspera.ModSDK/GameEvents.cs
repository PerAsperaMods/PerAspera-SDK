namespace PerAspera.ModSDK
{
    /// <summary>
    /// Game event constants mapped to actual Per Aspera GameEventType values
    /// Based on Events-Raw-List.md documentation
    /// </summary>
    public static class GameEvents
    {
        // === PLANET CLIMATE EVENTS (Real game events) ===
        public const string PlanetTemperatureChanged = "GevPlanetTemperatureChanged";
        public const string PlanetO2PressureChanged = "GevPlanetO2PressureChanged";
        public const string PlanetPressureChanged = "GevPlanetPressureChanged";
        public const string PlanetPressureCO2LevelChanged = "GevPlanetPressureCO2LevelChanged";
        public const string PlanetPressureO2LevelChanged = "GevPlanetPressureO2LevelChanged";

        // === UNIVERSE TIME EVENTS (Real game events) ===
        public const string UniverseDayPassed = "GevUniverseDayPassed";
        public const string UniverseGameSpeedChanged = "GevUniverseGameSpeedChanged";
        public const string UniverseStatsUpdated = "GevUniverseStatsUpdated";

        // === BUILDING EVENTS (Real game events) ===
        public const string BuildingBuilt = "GevBuildingBuilt";
        public const string BuildingSpawned = "GevBuildingSpawned";
        public const string BuildingFinishedScrapping = "GevBuildingFinishedScrapping";
        public const string BuildingStartedScrapping = "GevBuildingStartedScrapping";
        public const string BuildingOperativeChanged = "GevBuildingOperativeChanged";
        public const string BuildingOutOfPower = "GevBuildingOutOfPower";

        // === PRODUCTION EVENTS (Real game events) ===
        public const string FactoryProducedResource = "GevFactoryProducedResource";

        // === TECHNOLOGY EVENTS (Real game events) ===
        public const string TechnologyResearchStarted = "GevFactionTechnologyResearchStarted";
        public const string TechnologyResearchFinished = "GevFactionTechnologyResearchFinished";
        public const string KnowledgeUnlocked = "GevFactionKnowledgeUnlocked";

        // === DRONE EVENTS (Real game events) ===
        public const string DroneSpawned = "GevDroneSpawned";
        public const string DroneDespawned = "GevDroneDespawned";
        public const string DroneStartWorking = "GevDroneStartWorking";
        public const string DroneStopWorking = "GevDroneStopWorking";

        // === Legacy/Compatibility Events (For existing mods) ===
        public const string MartianDayPassed = UniverseDayPassed; // Alias
        public const string TemperatureChanged = PlanetTemperatureChanged; // Alias
        public const string AtmosphereChanged = PlanetPressureChanged; // Alias
        public const string BuildingConstructed = BuildingBuilt; // Alias
        public const string ProductionCompleted = FactoryProducedResource; // Alias

        // === System Events (Custom for SDK) ===
        public const string ModSystemInitialized = "Events.System.ModInitialized";
        public const string ModSystemShutdown = "Events.System.ModShutdown";
        public const string CommandSystemReady = "Events.System.CommandReady";
    }

    /// <summary>
    /// ModSDK-specific exception
    /// </summary>
    public class ModSDKException : System.Exception
    {
        public ModSDKException(string message) : base(message) { }
        public ModSDKException(string message, System.Exception innerException) : base(message, innerException) { }
    }
}