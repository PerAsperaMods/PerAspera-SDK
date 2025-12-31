using PerAspera.GameAPI.Events.Core;
using System;

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
        public PerAspera.GameAPI.Wrappers.BaseGameWrapper? BaseGameWrapper { get; }
        
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
                BaseGameWrapper = new PerAspera.GameAPI.Wrappers.BaseGameWrapper(nativeBaseGame);
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
        public PerAspera.GameAPI.Wrappers.BaseGameWrapper BaseGameWrapper { get; }
        
        /// <summary>
        /// SDK wrapper for Universe (type-safe access)
        /// </summary>
        public PerAspera.GameAPI.Wrappers.UniverseWrapper UniverseWrapper { get; }

        public BaseGameDetectedEvent(object nativeBaseGame, object nativeUniverse)
        {
            BaseGameWrapper = new PerAspera.GameAPI.Wrappers.BaseGameWrapper(nativeBaseGame ?? throw new ArgumentNullException(nameof(nativeBaseGame)));
            UniverseWrapper = new PerAspera.GameAPI.Wrappers.UniverseWrapper(nativeUniverse ?? throw new ArgumentNullException(nameof(nativeUniverse)));
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
        public PerAspera.GameAPI.Wrappers.BaseGameWrapper BaseGameWrapper { get; }
        
        /// <summary>SDK wrapper for Universe (type-safe access)</summary>
        public PerAspera.GameAPI.Wrappers.UniverseWrapper UniverseWrapper { get; }
        
        /// <summary>SDK wrapper for Planet (type-safe access)</summary>
        public PerAspera.GameAPI.Wrappers.PlanetWrapper PlanetWrapper { get; }

        public GameFullyLoadedEvent(object nativeBaseGame, object nativeUniverse, object nativePlanet)
        {
            // Create SDK wrappers from native instances
            BaseGameWrapper = new PerAspera.GameAPI.Wrappers.BaseGameWrapper(nativeBaseGame ?? throw new ArgumentNullException(nameof(nativeBaseGame)));
            UniverseWrapper = new PerAspera.GameAPI.Wrappers.UniverseWrapper(nativeUniverse ?? throw new ArgumentNullException(nameof(nativeUniverse)));
            PlanetWrapper = new PerAspera.GameAPI.Wrappers.PlanetWrapper(nativePlanet ?? throw new ArgumentNullException(nameof(nativePlanet)));
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
        public PerAspera.GameAPI.Wrappers.BlackBoardWrapper BlackBoard { get; }

        public BlackboardInitializedEvent(object blackboard)
        {
            NativeBlackboard = blackboard ?? throw new ArgumentNullException(nameof(blackboard));
            BlackBoard = new PerAspera.GameAPI.Wrappers.BlackBoardWrapper(blackboard);
        }
    }

    /// <summary>
    /// Event triggered when the Mars planet is initialized
    /// </summary>
    public class PlanetInitializedEvent : SDKEventBase
    {
        public override string EventType => "PlanetInitialized";
        
        public PerAspera.GameAPI.Wrappers.PlanetWrapper Planet { get; }

        public PlanetInitializedEvent(object nativePlanet)
        {
            Planet = new PerAspera.GameAPI.Wrappers.PlanetWrapper(nativePlanet ?? throw new ArgumentNullException(nameof(nativePlanet)));
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
        public PerAspera.GameAPI.Wrappers.PlanetWrapper? Planet { get; }
        public float Temperature { get; }
        public float AtmosphericPressure { get; }

        public MartianDayChangedEvent(int sol, PerAspera.GameAPI.Wrappers.PlanetWrapper? planet = null)
        {
            Sol = sol;
            Planet = planet;
            // TODO: Adapt to cellular atmosphere API once AtmosphereGrid is complete
            // Temperature = planet?.Atmosphere?.Temperature ?? 210.0f;
            // AtmosphericPressure = planet?.Atmosphere?.TotalPressure ?? 6.77f;
            Temperature = 210.0f; // Default Martian temperature
            AtmosphericPressure = 6.77f; // Default Martian pressure (mbar)
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
        public PerAspera.GameAPI.Wrappers.BaseGameWrapper BaseGameWrapper { get; }
        
        /// <summary>Whether the GameHub is fully ready for mod interaction</summary>
        public bool IsReady { get; }
        
        /// <summary>Initialization timestamp</summary>
        public DateTime InitializedAt { get; }

        public GameHubInitializedEvent(object nativeBaseGame, bool isReady = true)
        {
            // Create SDK wrapper from native instance  
            BaseGameWrapper = new PerAspera.GameAPI.Wrappers.BaseGameWrapper(nativeBaseGame ?? throw new ArgumentNullException(nameof(nativeBaseGame)));
            
            IsReady = isReady;
            InitializedAt = DateTime.Now;
        }
    }

    /// <summary>
    /// Event triggered when GameHub/GameHubManager is first loaded
    /// ✅ This is the EARLIEST event - fires when GameHubManager.Awake() occurs
    /// Use this for mods that need immediate initialization before any game systems
    /// </summary>
    public class GameHubReadyEvent : SDKEventBase
    {
        public override string EventType => "GameHubReady";
        
        /// <summary>Whether GameHub scene is fully loaded</summary>
        public bool SceneLoaded { get; }
        
        /// <summary>Whether GameHubManager is available</summary>
        public bool ManagerReady { get; }
        
        /// <summary>Event timestamp</summary>
        public DateTime ReadyAt { get; }

        public GameHubReadyEvent(bool sceneLoaded = true, bool managerReady = true)
        {
            SceneLoaded = sceneLoaded;
            ManagerReady = managerReady;
            ReadyAt = DateTime.Now;
        }
    }

    /// <summary>
    /// Event triggered when BaseGame instance is created
    /// Fires when BaseGame.Awake() or initialization occurs
    /// </summary>
    public class BaseGameCreatedEvent : SDKEventBase
    {
        public override string EventType => "BaseGameCreated";
        
        /// <summary>SDK wrapper for BaseGame</summary>
        public PerAspera.GameAPI.Wrappers.BaseGameWrapper BaseGameWrapper { get; }
        
        /// <summary>Whether BaseGame is fully initialized</summary>
        public bool IsInitialized { get; }

        public BaseGameCreatedEvent(object nativeBaseGame, bool isInitialized = false)
        {
            BaseGameWrapper = new PerAspera.GameAPI.Wrappers.BaseGameWrapper(nativeBaseGame ?? throw new ArgumentNullException(nameof(nativeBaseGame)));
            IsInitialized = isInitialized;
        }
    }

    /// <summary>
    /// Event triggered when Universe instance is created
    /// Fires when Universe is instantiated and attached to BaseGame
    /// </summary>
    public class UniverseCreatedEvent : SDKEventBase
    {
        public override string EventType => "UniverseCreated";
        
        /// <summary>SDK wrapper for Universe</summary>
        public PerAspera.GameAPI.Wrappers.UniverseWrapper UniverseWrapper { get; }
        
        /// <summary>SDK wrapper for BaseGame (parent)</summary>
        public PerAspera.GameAPI.Wrappers.BaseGameWrapper? BaseGameWrapper { get; }

        public UniverseCreatedEvent(object nativeUniverse, object? nativeBaseGame)
        {
            UniverseWrapper = new PerAspera.GameAPI.Wrappers.UniverseWrapper(nativeUniverse ?? throw new ArgumentNullException(nameof(nativeUniverse)));
            
            if (nativeBaseGame != null)
            {
                BaseGameWrapper = new PerAspera.GameAPI.Wrappers.BaseGameWrapper(nativeBaseGame);
            }
        }
    }

    /// <summary>
    /// Event triggered when Planet instance is created
    /// Fires when Planet is instantiated and attached to Universe
    /// </summary>
    public class PlanetCreatedEvent : SDKEventBase
    {
        public override string EventType => "PlanetCreated";
        
        /// <summary>SDK wrapper for Planet</summary>
        public PerAspera.GameAPI.Wrappers.PlanetWrapper PlanetWrapper { get; }
        
        /// <summary>SDK wrapper for Universe (parent)</summary>
        public PerAspera.GameAPI.Wrappers.UniverseWrapper? UniverseWrapper { get; }

        public PlanetCreatedEvent(object nativePlanet, object? nativeUniverse)
        {
            PlanetWrapper = new PerAspera.GameAPI.Wrappers.PlanetWrapper(nativePlanet ?? throw new ArgumentNullException(nameof(nativePlanet)));
            
            if (nativeUniverse != null)
            {
                UniverseWrapper = new PerAspera.GameAPI.Wrappers.UniverseWrapper(nativeUniverse);
            }
        }
    }

    /// <summary>
    /// Event triggered when BaseGame.OnFinishLoading() completes
    /// Fires at the exact moment the game's loading process finishes
    /// ✅ Use this for mods that need to run immediately after game loading
    /// </summary>
    public class OnLoadFinishedEvent : SDKEventBase
    {
        public override string EventType => "OnLoadFinished";
        
        /// <summary>SDK wrapper for BaseGame (full access available)</summary>
        public PerAspera.GameAPI.Wrappers.BaseGameWrapper? BaseGameWrapper { get; }
        
        /// <summary>SDK wrapper for Universe (full access available)</summary>
        public PerAspera.GameAPI.Wrappers.UniverseWrapper? UniverseWrapper { get; }
        
        /// <summary>Whether BaseGame is available</summary>
        public bool BaseGameAvailable { get; }
        
        /// <summary>Whether Universe is available</summary>
        public bool UniverseAvailable { get; }
        
        /// <summary>Event timestamp</summary>
        public DateTime EventTime { get; }

        public OnLoadFinishedEvent(object? nativeBaseGame = null, object? nativeUniverse = null)
        {
            BaseGameAvailable = nativeBaseGame != null;
            UniverseAvailable = nativeUniverse != null;
            EventTime = DateTime.Now;
            
            // Create wrappers if available
            if (BaseGameAvailable && nativeBaseGame != null)
            {
                BaseGameWrapper = new PerAspera.GameAPI.Wrappers.BaseGameWrapper(nativeBaseGame);
            }
            
            if (UniverseAvailable && nativeUniverse != null)
            {
                UniverseWrapper = new PerAspera.GameAPI.Wrappers.UniverseWrapper(nativeUniverse);
            }
        }
    }
}
