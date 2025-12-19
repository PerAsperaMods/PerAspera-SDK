using PerAspera.GameAPI.Events.Core;
using PerAspera.GameAPI.Wrappers;
using System;

// Enhanced Events Native type aliases for IL2CPP objects
using NativeBaseGame = PerAspera.GameAPI.Native.BaseGame;
using NativeUniverse = PerAspera.GameAPI.Native.Universe;
using NativePlanet = PerAspera.GameAPI.Native.Planet;

namespace PerAspera.GameAPI.Events.SDK
{
    // ==================== SYSTEM EVENTS ====================

    /// <summary>
    /// Event triggered when early mods can start loading
    /// Fires immediately after GameHub initialization, before full game load
    /// ✅ Use this for mods that need immediate initialization (UI, Twitch, logging, etc.)
    /// </summary>
    public class EarlyModsReadyEvent : SDKEventBase
    {
        public override string EventType => "EarlyModsReady";
        
        /// <summary>SDK wrapper for BaseGame (basic access available)</summary>
        public PerAspera.GameAPI.Wrappers.BaseGame? BaseGameWrapper { get; }
        
        /// <summary>Whether BaseGame is available for wrapper creation</summary>
        public bool BaseGameAvailable { get; }
        
        /// <summary>Whether this is safe for early mod initialization</summary>
        public bool SafeForEarlyInit { get; }
        
        /// <summary>Event timestamp</summary>
        public DateTime EventTime { get; }

        public EarlyModsReadyEvent(object? nativeBaseGame = null)
        {
            BaseGameAvailable = nativeBaseGame != null;
            SafeForEarlyInit = true;
            EventTime = DateTime.Now;
            
            // Create wrapper only if BaseGame is available
            if (BaseGameAvailable && nativeBaseGame != null)
            {
                BaseGameWrapper = new PerAspera.GameAPI.Wrappers.BaseGame(nativeBaseGame);
            }
        }
    }

    /// <summary>
    /// Event triggered when BaseGame and Universe are detected
    /// Uses SDK wrapper classes for type-safe access
    /// </summary>
    public class BaseGameDetectedEvent : SDKEventBase
    {
        public override string EventType => "BaseGameDetected";
        
        /// <summary>
        /// SDK wrapper for BaseGame (type-safe access)
        /// </summary>
        public BaseGame BaseGame { get; }
        
        /// <summary>
        /// SDK wrapper for Universe (type-safe access)
        /// </summary>
        public Universe Universe { get; }

        public BaseGameDetectedEvent(BaseGame baseGame, Universe universe)
        {
            BaseGame = baseGame ?? throw new ArgumentNullException(nameof(baseGame));
            Universe = universe ?? throw new ArgumentNullException(nameof(universe));
        }
    }

    /// <summary>
    /// Event triggered when game is fully loaded (BaseGame + Universe + Planet)
    /// ✅ Enhanced: Uses SDK wrappers for type-safe access
    /// </summary>
    public class GameFullyLoadedEvent : SDKEventBase
    {
        public override string EventType => "GameFullyLoaded";
        
        /// <summary>SDK wrapper for BaseGame (type-safe access)</summary>
        public PerAspera.GameAPI.Wrappers.BaseGame BaseGameWrapper { get; }
        
        /// <summary>SDK wrapper for Universe (type-safe access)</summary>
        public PerAspera.GameAPI.Wrappers.Universe UniverseWrapper { get; }
        
        /// <summary>SDK wrapper for Planet (type-safe access)</summary>
        public PerAspera.GameAPI.Wrappers.Planet PlanetWrapper { get; }
        
        /// <summary>Native IL2CPP BaseGame instance</summary>
        public NativeBaseGame NativeBaseGame { get; }
        
        /// <summary>Native IL2CPP Universe instance</summary>
        public NativeUniverse NativeUniverse { get; }
        
        /// <summary>Native IL2CPP Planet instance</summary>
        public NativePlanet NativePlanet { get; }

        public GameFullyLoadedEvent(object nativeBaseGame, object nativeUniverse, object nativePlanet)
        {
            // Store native instances using explicit native type aliases
            NativeBaseGame = new NativeBaseGame(nativeBaseGame ?? throw new ArgumentNullException(nameof(nativeBaseGame)));
            NativeUniverse = new NativeUniverse(nativeUniverse ?? throw new ArgumentNullException(nameof(nativeUniverse)));
            NativePlanet = new NativePlanet(nativePlanet ?? throw new ArgumentNullException(nameof(nativePlanet)));
            
            // Create SDK wrappers from native instances
            BaseGameWrapper = new PerAspera.GameAPI.Wrappers.BaseGame(nativeBaseGame);
            UniverseWrapper = new PerAspera.GameAPI.Wrappers.Universe(nativeUniverse);
            PlanetWrapper = new PerAspera.GameAPI.Wrappers.Planet(nativePlanet);
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
    /// ✅ Enhanced: Uses BlackBoard wrapper for type-safe access
    /// </summary>
    public class BlackboardInitializedEvent : SDKEventBase
    {
        public override string EventType => "BlackboardInitialized";
        
        /// <summary>
        /// The native blackboard object (for compatibility)
        /// </summary>
        public object NativeBlackboard { get; }
        
        /// <summary>
        /// Type-safe BlackBoard wrapper instance
        /// </summary>
        public BlackBoard BlackBoard { get; }

        public BlackboardInitializedEvent(object blackboard)
        {
            NativeBlackboard = blackboard ?? throw new ArgumentNullException(nameof(blackboard));
            BlackBoard = new BlackBoard(blackboard);
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

    /// <summary>
    /// Event triggered when GameHub/GameHubManager is initialized
    /// ✅ Enhanced: Early initialization event for mods that need immediate setup
    /// This fires before GameFullyLoadedEvent and provides BaseGame access
    /// </summary>
    public class GameHubInitializedEvent : SDKEventBase
    {
        public override string EventType => "GameHubInitialized";
        
        /// <summary>SDK wrapper for BaseGame (available early in initialization)</summary>
        public PerAspera.GameAPI.Wrappers.BaseGame BaseGameWrapper { get; }
        
        /// <summary>Native IL2CPP BaseGame instance</summary>
        public NativeBaseGame NativeBaseGame { get; }
        
        /// <summary>Whether the GameHub is fully ready for mod interaction</summary>
        public bool IsReady { get; }
        
        /// <summary>Initialization timestamp</summary>
        public DateTime InitializedAt { get; }

        public GameHubInitializedEvent(object nativeBaseGame, bool isReady = true)
        {
            // Store native instance using explicit native type alias
            NativeBaseGame = new NativeBaseGame(nativeBaseGame ?? throw new ArgumentNullException(nameof(nativeBaseGame)));
            
            // Create SDK wrapper from native instance  
            BaseGameWrapper = new PerAspera.GameAPI.Wrappers.BaseGame(nativeBaseGame);
            
            IsReady = isReady;
            InitializedAt = DateTime.Now;
        }
    }
}
