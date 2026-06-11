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
        public BaseGame? BaseGameWrapper { get; }
        
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
                BaseGameWrapper = (nativeBaseGame as BaseGame)!;
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
        public BaseGame BaseGameWrapper { get; }
        
        /// <summary>
        /// SDK wrapper for Universe (type-safe access)
        /// </summary>
        public Universe UniverseWrapper { get; }

        public BaseGameDetectedEvent(object nativeBaseGame, object nativeUniverse)
        {
            BaseGameWrapper = (BaseGame)nativeBaseGame;
            UniverseWrapper = (Universe)nativeUniverse;
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
        public BaseGame BaseGameWrapper { get; }
        
        /// <summary>SDK wrapper for Universe (type-safe access)</summary>
        public Universe UniverseWrapper { get; }
        
        /// <summary>SDK wrapper for Planet (type-safe access)</summary>
        public Planet PlanetWrapper { get; }

        public GameFullyLoadedEvent(object nativeBaseGame, object nativeUniverse, object nativePlanet)
        {
            // Create SDK wrappers from native instances
            BaseGameWrapper = (BaseGame)nativeBaseGame;
            UniverseWrapper = (Universe)nativeUniverse;
            PlanetWrapper = (Planet)nativePlanet;
        }
    }

    /// <summary>
    /// Event triggered once InteractionManager is available (first Update() after player clicks Wake Up).
    /// ✅ Use this instead of GameFullyLoaded when you need to dispatch game commands.
    /// ✅ All SDK objects (BaseGame, Universe, Planet, PlayerFaction) are ready — no reflection needed.
    /// </summary>
    public class GameCommandsReadyEvent : SDKEventBase
    {
        public override string EventType => "GameCommandsReady";

        /// <summary>Direct interop access to BaseGame — no reflection needed.</summary>
        public BaseGame NativeBaseGame { get; }

        /// <summary>Direct interop access to Universe — no reflection needed.</summary>
        public Universe NativeUniverse { get; }

        /// <summary>
        /// Direct interop access to Planet. Available after WakeUp (always non-null when this event fires).
        /// </summary>
        public Planet? NativePlanet { get; }

        /// <summary>Direct interop access to the player Faction — interactionManager available on it.</summary>
        public Faction NativePlayerFaction { get; }

        /// <summary>
        /// True when InteractionManager is confirmed non-null on the player faction.
        /// Always true when this event is received.
        /// </summary>
        public bool InteractionManagerReady => NativePlayerFaction?.interactionManager != null;

        public GameCommandsReadyEvent(BaseGame baseGame, Universe universe, Planet? planet, Faction playerFaction)
        {
            NativeBaseGame    = baseGame;
            NativeUniverse    = universe;
            NativePlanet      = planet;
            NativePlayerFaction = playerFaction;
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
        public Blackboard BlackBoard { get; }

        public BlackboardInitializedEvent(object blackboard)
        {
            NativeBlackboard = blackboard ?? throw new ArgumentNullException(nameof(blackboard));
            BlackBoard = (blackboard as Blackboard)!;
        }
    }

    /// <summary>
    /// Event triggered when the Mars planet is initialized
    /// </summary>
    public class PlanetInitializedEvent : SDKEventBase
    {
        public override string EventType => "PlanetInitialized";
        
        public Planet Planet { get; }

        public PlanetInitializedEvent(object nativePlanet)
        {
            Planet = (Planet)nativePlanet;
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
        public Planet? Planet { get; }
        public float Temperature { get; }
        public float AtmosphericPressure { get; }

        public MartianDayChangedEvent(int sol, Planet? planet = null)
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
        public BaseGame BaseGameWrapper { get; }
        
        /// <summary>Whether the GameHub is fully ready for mod interaction</summary>
        public bool IsReady { get; }
        
        /// <summary>Initialization timestamp</summary>
        public DateTime InitializedAt { get; }

        public GameHubInitializedEvent(object nativeBaseGame, bool isReady = true)
        {
            // Create SDK wrapper from native instance  
            BaseGameWrapper = (BaseGame)nativeBaseGame;
            
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
        public BaseGame BaseGameWrapper { get; }
        
        /// <summary>Whether BaseGame is fully initialized</summary>
        public bool IsInitialized { get; }

        public BaseGameCreatedEvent(object nativeBaseGame, bool isInitialized = false)
        {
            BaseGameWrapper = (BaseGame)nativeBaseGame;
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
        public Universe UniverseWrapper { get; }
        
        /// <summary>SDK wrapper for BaseGame (parent)</summary>
        public BaseGame? BaseGameWrapper { get; }

        public UniverseCreatedEvent(object nativeUniverse, object? nativeBaseGame)
        {
            UniverseWrapper = (Universe)nativeUniverse;
            
            if (nativeBaseGame != null)
            {
                BaseGameWrapper = (nativeBaseGame as BaseGame)!;
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
        public Planet PlanetWrapper { get; }
        
        /// <summary>SDK wrapper for Universe (parent)</summary>
        public Universe? UniverseWrapper { get; }

        public PlanetCreatedEvent(object nativePlanet, object? nativeUniverse)
        {
            PlanetWrapper = (Planet)nativePlanet;
            
            if (nativeUniverse != null)
            {
                UniverseWrapper = (nativeUniverse as Universe)!;
            }
        }
    }

    /// <summary>
    /// Fires once per session when a game session begins — new game or loaded save.
    /// Anchored on <c>GevUniverseNewGameStarted</c> / <c>GevUniverseContinueEndedGame</c> via NativeEventHub.
    /// Replaces polling <c>BaseGame.alreadyWokeUp</c> and fixes session reset on reload.
    /// ✅ Use to reset per-session state; subscribe via <c>EnhancedEventBus.SubscribeToGameSessionStarted</c>.
    /// </summary>
    /// <example>
    /// EnhancedEventBus.SubscribeToGameSessionStarted(evt =>
    ///     LogAspera.Info(evt.IsNewGame ? "Nouvelle partie !" : "Partie chargée !"));
    /// </example>
    public class GameSessionStartedEvent : SDKEventBase
    {
        public override string EventType => "GameSessionStarted";

        /// <summary>Direct interop access to BaseGame.</summary>
        public BaseGame NativeBaseGame { get; }

        /// <summary>Direct interop access to Universe (available when the native event fires).</summary>
        public Universe? NativeUniverse { get; }

        /// <summary>True for a new game, false for a loaded save (<c>GevUniverseContinueEndedGame</c>).</summary>
        public bool IsNewGame { get; }

        public GameSessionStartedEvent(BaseGame baseGame, Universe? universe, bool isNewGame)
        {
            NativeBaseGame = baseGame;
            NativeUniverse = universe;
            IsNewGame = isNewGame;
        }
    }

    /// <summary>
    /// Fires once per session on the first frame where <c>canvasRefs.notificationPresenter</c>
    /// is non-null — the moment the native UI hub is ready for notifications.
    /// Fixes the class of timing bugs where <c>GameFullyLoadedEvent</c> fires before the UI hub.
    /// ✅ Use this instead of <c>GameFullyLoadedEvent</c> whenever you need to show native notifications.
    /// </summary>
    /// <example>
    /// EnhancedEventBus.SubscribeToGameUIReady(evt =>
    ///     GameUI.ShowNotification("UI ready!", NotificationUrgency.Info));
    /// </example>
    public class GameUIReadyEvent : SDKEventBase
    {
        public override string EventType => "GameUIReady";

        /// <summary>Direct interop access to BaseGame.</summary>
        public BaseGame NativeBaseGame { get; }

        /// <summary>The native canvas refs — <c>notificationPresenter</c> is guaranteed non-null when this fires.</summary>
        public GameCanvasReferences CanvasRefs { get; }

        public GameUIReadyEvent(BaseGame baseGame, GameCanvasReferences canvasRefs)
        {
            NativeBaseGame = baseGame;
            CanvasRefs = canvasRefs;
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
        public BaseGame? BaseGameWrapper { get; }
        
        /// <summary>SDK wrapper for Universe (full access available)</summary>
        public Universe? UniverseWrapper { get; }
        
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
                BaseGameWrapper = (nativeBaseGame as BaseGame)!;
            }
            
            if (UniverseAvailable && nativeUniverse != null)
            {
                UniverseWrapper = (nativeUniverse as Universe)!;
            }
        }
    }
}
