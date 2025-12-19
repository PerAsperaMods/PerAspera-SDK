using PerAspera.GameAPI.Events.Integration;
using PerAspera.GameAPI.Events.Core;
using PerAspera.Core;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using PerAspera.GameAPI.Events.Native;
using HarmonyLib;
using EnhancedEventBus = PerAspera.GameAPI.Events.Integration.EnhancedEventBus;

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
        public static LogAspera _logger = new LogAspera("EventsAutoStart");

        public override void Load()
        {
            try
            {
                _logger.Info("Initializing Enhanced Event System...");

                // Initialize wrapper factory
                InitializeWrapperFactory();

                // Initialize event system integration
                EventSystemIntegration.Initialize();

                // ✅ Initialize SDK-based game initialization detection
                InitializeSDKBasedGameDetection();

                _logger.Info("✅ Enhanced Event System initialized successfully");
                _logger.Info("🎯 All native events now use SDK wrappers automatically");
                _logger.Info("🎮 Game initialization events (GameHubInitialized, GameFullyLoaded) ready");
                
                // Log system status
                LogSystemStatus();
            }
            catch (System.Exception ex)
            {
                _logger.Error($"❌ Failed to initialize Enhanced Event System: {ex.Message}");
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
                _logger.Info("🔧 Initializing SDK-based game detection...");
                
                // Use existing SDK wrapper architecture instead of raw IL2CPP patches
                Patches.GameInitializationPatches.InitializeSDKBasedEvents();
                
                _logger.Info("✅ SDK-based game detection initialized successfully");
            }
            catch (System.Exception ex)
            {
                _logger.Error($"❌ Failed to initialize SDK-based game detection: {ex.Message}");
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
                _logger.Info("Applying game initialization patches...");
                
                var harmony = new HarmonyLib.Harmony("PerAspera.GameAPI.Events.GameInitialization");
                
                // Apply patches from GameInitializationPatches class
                harmony.PatchAll(typeof(Patches.GameInitializationPatches));
                
                _logger.Info("✅ Game initialization patches applied successfully");
            }
            catch (System.Exception ex)
            {
                _logger.Error($"❌ Failed to apply game initialization patches: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Initialize the wrapper factory with all available wrappers
        /// </summary>
        private void InitializeWrapperFactory()
        {
            _logger.Info("Initializing WrapperFactory...");
            
            var supportedWrappers = WrapperFactory.GetSupportedWrapperTypes();
            _logger.Info($"✅ WrapperFactory initialized with {supportedWrappers.Count} wrapper types");
            
            foreach (var wrapperType in supportedWrappers)
            {
                _logger.Debug($"  - {wrapperType.Name}");
            }
        }

        /// <summary>
        /// Log current system status for debugging
        /// </summary>
        private void LogSystemStatus()
        {
            var stats = EnhancedEventBus.GetStats();
            _logger.Info($"Event Bus Status: {stats}");
            
            if (EventSystemIntegration.IsInitialized)
            {
                _logger.Info("✅ Integration with legacy EventSystem: Active");
            }
            else
            {
                _logger.Warning("⚠️ Integration with legacy EventSystem: Not found - running standalone");
            }
        }

        /// <summary>
        /// Plugin shutdown
        /// </summary>
        public override bool Unload()
        {
            try
            {
                _logger.Info("Shutting down Enhanced Event System...");
                EventSystemIntegration.Shutdown();
                _logger.Info("✅ Enhanced Event System shut down successfully");
                return true;
            }
            catch (System.Exception ex)
            {
                _logger.Error($"❌ Error during shutdown: {ex.Message}");
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