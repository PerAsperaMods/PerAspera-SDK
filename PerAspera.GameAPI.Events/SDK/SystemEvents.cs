using PerAspera.GameAPI.Events.Core;
using PerAspera.GameAPI.Wrappers;

namespace PerAspera.GameAPI.Events.SDK
{
    // ==================== SYSTEM EVENTS ====================

    /// <summary>
    /// Event triggered when BaseGame and Universe are detected
    /// </summary>
    public class BaseGameDetectedEvent : SDKEventBase
    {
        public override string EventType => "BaseGameDetected";
        
        public object BaseGame { get; }
        public Universe Universe { get; }

        public BaseGameDetectedEvent(object baseGame, Universe universe)
        {
            BaseGame = baseGame;
            Universe = universe;
        }
    }

    /// <summary>
    /// Event triggered when game is fully loaded (BaseGame + Universe + Planet)
    /// Equivalent to legacy GameFullyLoadedEvent
    /// </summary>
    public class GameFullyLoadedEvent : SDKEventBase
    {
        public override string EventType => "GameFullyLoaded";
        
        public object BaseGameInstance { get; }
        public object UniverseInstance { get; }
        public object PlanetInstance { get; }

        public GameFullyLoadedEvent(object baseGame, object universe, object planet)
        {
            BaseGameInstance = baseGame;
            UniverseInstance = universe;
            PlanetInstance = planet;
        }
    }

    /// <summary>
    /// Event triggered when a game save is loaded
    /// </summary>
    public class GameLoadedEvent : SDKEventBase
    {
        public override string EventType => "GameLoaded";
        
        public string SaveGameName { get; }

        public GameLoadedEvent(string saveGameName)
        {
            SaveGameName = saveGameName ?? "Unknown";
        }
    }

    /// <summary>
    /// Event triggered when the main Blackboard is initialized
    /// </summary>
    public class BlackboardInitializedEvent : SDKEventBase
    {
        public override string EventType => "BlackboardInitialized";
        
        public object Blackboard { get; }

        public BlackboardInitializedEvent(object blackboard)
        {
            Blackboard = blackboard;
        }
    }

    /// <summary>
    /// Event triggered when the Mars planet is initialized
    /// </summary>
    public class PlanetInitializedEvent : SDKEventBase
    {
        public override string EventType => "PlanetInitialized";
        
        public Planet Planet { get; }

        public PlanetInitializedEvent(Planet planet)
        {
            Planet = planet;
        }
    }

    // ==================== GAMEPLAY EVENTS ====================

    /// <summary>
    /// Event triggered each in-game day
    /// </summary>
    public class DayPassedEvent : SDKEventBase
    {
        public override string EventType => "DayPassed";
        
        public int DayNumber { get; }
        public int MartianSol { get; set; }

        public DayPassedEvent(int dayNumber, int martianSol = -1)
        {
            DayNumber = dayNumber;
            MartianSol = martianSol >= 0 ? martianSol : dayNumber;
        }
    }

    /// <summary>
    /// Event triggered when resources are added
    /// </summary>
    public class ResourceAddedEvent : SDKEventBase
    {
        public override string EventType => "ResourceAdded";
        
        public string ResourceName { get; }
        public string ResourceKey { get; }
        public float Amount { get; }
        public object? Faction { get; }

        public ResourceAddedEvent(string resourceName, float amount, string resourceKey = "", object? faction = null)
        {
            ResourceName = resourceName;
            ResourceKey = resourceKey;
            Amount = amount;
            Faction = faction;
        }
    }

    // ==================== CLIMATE EVENTS (From ClimatAspera) ====================

    /// <summary>
    /// Event triggered when Martian day changes (climate-aware)
    /// </summary>
    public class MartianDayChangedEvent : SDKEventBase
    {
        public override string EventType => "MartianDayChanged";
        
        public int Sol { get; }
        public Wrappers.Planet? Planet { get; }
        public float Temperature { get; }
        public float AtmosphericPressure { get; }

        public MartianDayChangedEvent(int sol, Wrappers.Planet? planet = null)
        {
            Sol = sol;
            Planet = planet;
            Temperature = planet?.Atmosphere?.Temperature ?? 210.0f;
            AtmosphericPressure = planet?.Atmosphere?.TotalPressure ?? 6.77f;
        }
    }

    /// <summary>
    /// Event triggered when climate analysis is complete
    /// </summary>
    public class ClimateAnalysisCompleteEvent : SDKEventBase
    {
        public override string EventType => "ClimateAnalysisComplete";
        
        public string AnalysisType { get; }
        public object? AnalysisData { get; }
        public string[] Recommendations { get; }

        public ClimateAnalysisCompleteEvent(string analysisType, object? analysisData = null, string[]? recommendations = null)
        {
            AnalysisType = analysisType;
            AnalysisData = analysisData;
            Recommendations = recommendations ?? System.Array.Empty<string>();
        }
    }
}
