using PerAspera.GameAPI.Events.Integration;
using PerAspera.GameAPI.Events.Core;
using PerAspera.GameAPI;
using PerAspera.Core;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using PerAspera.GameAPI.Events.Native;
using HarmonyLib;
using UnityEngine;
using Il2CppInterop.Runtime.Injection;
using EnhancedEventBus = PerAspera.GameAPI.Events.Integration.EnhancedEventBus;
using BepInEx.Logging;
using PerAspera.GameAPI.Events.SDK;
using PerAspera.GameAPI.Events.Constants;
namespace PerAspera.GameAPI.Events
{
    /// <summary>
    /// Automatic initialization for enhanced event system
    /// Provides seamless upgrade from legacy event system to wrapper-enabled events
    /// 
    /// 📋 Event Documentation: F:\ModPeraspera\SDK\PerAspera.GameAPI.Events\MODDER-GUIDE.md
    /// 🤖 Agent Expert: @per-aspera-sdk-coordinator (Events expertise)
    /// 📡 Usage Examples: F:\ModPeraspera\SDK\PerAspera.GameAPI.Events\USAGE-EXAMPLES.md
    /// 🌐 User Wiki: https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/tutorials/Events.md
    /// </summary>
    [BepInPlugin("PerAspera.GameAPI.Events", "PerAspera Enhanced Events", "1.0.0")]
    public class EventsAutoStartPlugin : BasePlugin
    {
        //public static LogAspera _logger = new LogAspera("EventsAutoStart");
        private ManualLogSource? _gameHubLogger;
        private static ManualLogSource? _staticLogger;

        public override void Load()
        {
            try
            {
                Log.LogInfo("Initializing Enhanced Event System...");
                
                // Initialize static logger for static methods
                _staticLogger = Log;

                // Initialize wrapper factory
                InitializeWrapperFactory();

                // Initialize event system integration
                EventSystemIntegration.Initialize();

                // ✅ Initialize SDK-based game initialization detection
                InitializeSDKBasedGameDetection();

                Log.LogInfo("✅ Enhanced Event System initialized successfully");
                Log.LogInfo("🎯 All native events now use SDK wrappers automatically");
                Log.LogInfo("🎮 Game initialization events (GameHubInitialized, GameFullyLoaded) ready");
                
                // Log system status
                LogSystemStatus();
            }
            catch (System.Exception ex)
            {
                Log.LogError($"❌ Failed to initialize Enhanced Event System: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Initialize SDK-based game state detection (replacing problematic Harmony patches)
        /// Uses existing SDK wrapper system for clean game initialization detection
        /// </summary>
        private void InitializeSDKBasedGameDetection()
        {
            try
            {
                Log.LogInfo("🔧 Initializing SDK-based game detection...");
                
                // Use existing SDK wrapper architecture instead of raw IL2CPP patches
                Patches.GameInitializationPatches.InitializeSDKBasedEvents();
                
                // Initialize GameHub Detector MonoBehaviour
                InitializeGameHubDetector();
                
                Log.LogInfo("✅ SDK-based game detection initialized successfully");
            }
            catch (System.Exception ex)
            {
                Log.LogError($"❌ Failed to initialize SDK-based game detection: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// DEPRECATED: Apply Harmony patches for game initialization detection
        /// Replaced by SDK-based detection for better reliability
        /// </summary>
        private void ApplyGameInitializationPatches()
        {
            try
            {
                Log.LogInfo("Applying game initialization patches...");
                
                var harmony = new HarmonyLib.Harmony("PerAspera.GameAPI.Events.GameInitialization");
                
                // Apply patches from GameInitializationPatches class
                harmony.PatchAll(typeof(Patches.GameInitializationPatches));
                
                Log.LogInfo("✅ Game initialization patches applied successfully");
            }
            catch (System.Exception ex)
            {
                Log.LogError($"❌ Failed to apply game initialization patches: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Initialize the wrapper factory with all available wrappers
        /// </summary>
        private void InitializeWrapperFactory()
        {
            Log.LogInfo("Initializing WrapperFactory...");
            
            var supportedWrappers = WrapperFactory.GetSupportedWrapperTypes();
            Log.LogInfo($"✅ WrapperFactory initialized with {supportedWrappers.Count} wrapper types");
            
            foreach (var wrapperType in supportedWrappers)
            {
                Log.LogDebug($"  - {wrapperType.Name}");
            }
        }

        /// <summary>
        /// Log current system status for debugging
        /// </summary>
        private void LogSystemStatus()
        {
            var stats = EnhancedEventBus.GetStats();
            Log.LogInfo($"Event Bus Status: {stats}");
            
            if (EventSystemIntegration.IsInitialized)
            {
                Log.LogInfo("✅ Integration with legacy EventSystem: Active");
            }
            else
            {
                Log.LogWarning("⚠️ Integration with legacy EventSystem: Not found - running standalone");
            }
        }

        /// <summary>
        /// Initialize native event subscriptions for GameHub detection
        /// Much more reliable than patches - listens to actual game events
        /// </summary>
        private void InitializeGameHubDetector()
        {
            try
            {
                Log.LogInfo("🔧 Setting up native event subscriptions for GameHub detection...");

                // Subscribe to native game start events
                EnhancedEventBus.Subscribe<PerAspera.GameAPI.Events.Native.UniverseNewGameStartedNativeEvent>(OnGameStarted);
                
                EnhancedEventBus.Subscribe<PerAspera.GameAPI.Events.Native.UniverseContinueEndedGameNativeEvent>(OnGameLoaded);

                Log.LogInfo("✅ Native event subscriptions setup successfully");
                Log.LogInfo("🎯 Will emit GameHubInitializedEvent when game starts/loads");
                Log.LogInfo("🎮 Game initialization events (GameHubInitialized, GameFullyLoaded) ready");
                
                // ALSO apply GameHub Harmony patches for immediate detection (works in menu)
                Log.LogInfo("🔧 Applying GameHub Harmony patches for immediate detection...");
                ApplyGameHubPatches();
                
                // Apply BaseGame patches for OnLoadFinished event
                Log.LogInfo("🔧 Applying BaseGame Harmony patches for OnLoadFinished event...");
                ApplyBaseGamePatches();
            }
            catch (System.Exception ex)
            {
                Log.LogError($"❌ Failed to setup GameHub native event detection: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply BaseGame Harmony patches for OnLoadFinished event
        /// </summary>
        private void ApplyBaseGamePatches()
        {
            try
            {
                var harmony = new HarmonyLib.Harmony("PerAspera.GameAPI.Events.BaseGame");
                
                // Apply BaseGame patches for OnLoadFinished + GameFullyLoaded events
                harmony.PatchAll(typeof(Patches.BaseGamePatches));
                
                // Apply BaseGame.Update one-shot patch for GameCommandsReadyEvent
                harmony.PatchAll(typeof(Patches.BaseGameUpdatePatches));
                
                Log.LogInfo("✅ BaseGame Harmony patches applied successfully (incl. GameCommandsReady)");
            }
            catch (System.Exception ex)
            {
                Log.LogError($"❌ Failed to apply BaseGame patches: {ex.Message}");
            }
        }

        /// <summary>
        /// Apply GameHub Harmony patches for immediate detection (works in menu)
        /// </summary>
        private void ApplyGameHubPatches()
        {
            try
            {
                var harmony = new HarmonyLib.Harmony("PerAspera.GameAPI.Events.GameHub");
                
                // Apply patches from GameHubManagerPatch class
                harmony.PatchAll(typeof(Patches.GameHubManagerPatch));
                
                Log.LogInfo("✅ GameHub Harmony patches applied successfully");
            }
            catch (System.Exception ex)
            {
                Log.LogError($"❌ Failed to apply GameHub patches: {ex.Message}");
            }
        }

        /// <summary>
        /// Handle new game started - BaseGame should be accessible now
        /// </summary>
        private static void OnGameStarted(PerAspera.GameAPI.Events.Native.UniverseNewGameStartedNativeEvent evt)
        {
            _staticLogger?.LogInfo($"🎮 New game started: {evt.GameMode} - attempting to emit GameHubInitializedEvent");
            Patches.BaseGameUpdatePatches.ResetForNewSession();
            EmitGameHubInitializedEvent("NewGameStarted");
        }

        /// <summary>
        /// Handle game loaded - BaseGame should be accessible now
        /// </summary>
        private static void OnGameLoaded(PerAspera.GameAPI.Events.Native.UniverseContinueEndedGameNativeEvent evt)
        {
            _staticLogger?.LogInfo($"🎮 Game loaded: {evt.SaveGameName} - attempting to emit GameHubInitializedEvent");
            Patches.BaseGameUpdatePatches.ResetForNewSession();
            EmitGameHubInitializedEvent("GameLoaded");
        }

        private static bool _gameHubInitialized = false;

        /// <summary>
        /// Emit GameHubInitializedEvent when BaseGame is confirmed accessible
        /// </summary>
        private static void EmitGameHubInitializedEvent(string triggerSource)
        {
            try
            {
                if (_gameHubInitialized)
                {
                    _staticLogger?.LogInfo($"⚠️ GameHubInitializedEvent already emitted, skipping {triggerSource}");
                    return;
                }

                _staticLogger?.LogInfo($"🎯 {triggerSource} triggered - checking BaseGame accessibility...");

                // Access BaseGame through SDK wrapper
                var baseGame = GameTypeInitializer.GetBaseGameInstance() as BaseGame;
                if (baseGame != null)
                {
                    _staticLogger?.LogInfo("🎮 BaseGame confirmed accessible - emitting all SDK events");
                    
                    // Create and emit GameHubInitializedEvent 
                    var gameHubEvent = new GameHubInitializedEvent(
                        (object)baseGame,
                        isReady: true
                    );
                    EnhancedEventBus.Publish(SDKEventConstants.GameHubInitialized, gameHubEvent);
                    _staticLogger?.LogInfo($"✅ GameHubInitializedEvent emitted via {triggerSource}");
                    
                    // Emit GameHubReadyEvent (what CommandsDemo expects)
                    var gameHubReadyEvent = new GameHubReadyEvent(
                        sceneLoaded: true,
                        managerReady: true
                    );
                    EnhancedEventBus.Publish(SDKEventConstants.GameHubReady, gameHubReadyEvent);
                    _staticLogger?.LogInfo($"✅ GameHubReadyEvent emitted via {triggerSource}");
                    
                    // Emit GameFullyLoadedEvent (backup for CommandsDemo)
                    var gameFullyLoadedEvent = new GameFullyLoadedEvent(
                        (object)baseGame,
                        (object)baseGame, // universe  baseGame.getUniverse() noramlement
                        null // planet might not be available yet
                    );
                    EnhancedEventBus.Publish(SDKEventConstants.GameFullyLoaded, gameFullyLoadedEvent);
                    _staticLogger?.LogInfo($"✅ GameFullyLoadedEvent emitted via {triggerSource}");
                    
                    _gameHubInitialized = true;
                    _staticLogger?.LogInfo($"🎯 All SDK events emitted successfully via {triggerSource}");
                }
                else
                {
                    _staticLogger?.LogWarning($"⚠️ BaseGame not yet accessible via {triggerSource}, will wait for next event");
                }
            }
            catch (System.Exception ex)
            {
                _staticLogger?.LogError($"❌ Error emitting GameHubInitializedEvent via {triggerSource}: {ex.Message}");
            }
        }
        /// <summary>
        /// Plugin shutdown
        /// </summary>
        public override bool Unload()
        {
            try
            {
                Log.LogInfo("Shutting down Enhanced Event System...");
                
                EventSystemIntegration.Shutdown();
                Log.LogInfo("✅ Enhanced Event System shut down successfully");
                return true;
            }
            catch (System.Exception ex)
            {
                Log.LogError($"❌ Error during shutdown: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Public API for mods to interact with the enhanced event system
    /// Provides a clean interface that automatically handles wrapper conversion
    /// </summary>
    public static class EnhancedEvents
    {
        public static LogAspera _logger = new LogAspera("EnhancedEvents");

        /// <summary>
        /// Subscribe to a native game event with automatic wrapper conversion
        /// </summary>
        /// <param name="eventType">Event type from NativeEventConstants</param>
        /// <param name="handler">Event handler (receives wrappers automatically)</param>
        public static void Subscribe(string eventType, System.Action<object> handler)
        {
            EnhancedEventBus.Subscribe(eventType, handler);
            _logger.Debug($"Subscribed to {eventType} with wrapper conversion");
        }

        /// <summary>
        /// Unsubscribe from a native game event
        /// </summary>
        /// <param name="eventType">Event type from NativeEventConstants</param>
        /// <param name="handler">Event handler to remove</param>
        public static void Unsubscribe(string eventType, System.Action<object> handler)
        {
            EnhancedEventBus.Unsubscribe(eventType, handler);
        }

        /// <summary>
        /// Subscribe to a native event with strong typing
        /// </summary>
        /// <typeparam name="T">Event type (e.g., BuildingSpawnedNativeEvent)</typeparam>
        /// <param name="eventType">Event type constant</param>
        /// <param name="handler">Typed event handler</param>
        public static void Subscribe<T>(string eventType, System.Action<T> handler) where T : class
        {
            EnhancedEventBus.Subscribe(eventType, data =>
            {
                if (data is T typedEvent)
                {
                    handler(typedEvent);
                }
                else
                {
                    _logger.Warning($"Received event {data.GetType().Name}, expected {typeof(T).Name}");
                }
            });
        }

        /// <summary>
        /// Publish a custom event through the enhanced system
        /// </summary>
        /// <param name="eventType">Event type</param>
        /// <param name="eventData">Event data (will be converted if needed)</param>
        public static void Publish(string eventType, object eventData)
        {
            EnhancedEventBus.Publish(eventType, eventData);
        }

        /// <summary>
        /// Check if the enhanced event system is available
        /// </summary>
        /// <returns>True if enhanced events are active</returns>
        public static bool IsAvailable()
        {
            return EventSystemIntegration.IsInitialized;
        }

        /// <summary>
        /// Get current event system statistics
        /// </summary>
        /// <returns>Event system statistics</returns>
        public static EventSystemStats GetStats()
        {
            return EnhancedEventBus.GetStats();
        }

        /// <summary>
        /// Enable or disable automatic wrapper conversion
        /// </summary>
        /// <param name="enabled">True to enable wrapper conversion</param>
        public static void SetWrapperConversion(bool enabled)
        {
            EnhancedEventBus.SetAutoConversion(enabled);
            _logger.Info($"Wrapper conversion {(enabled ? "enabled" : "disabled")}");
        }
    }
}