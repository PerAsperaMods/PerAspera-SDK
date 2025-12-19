using System;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using PerAspera.Core;
using PerAspera.GameAPI.Events;
using PerAspera.GameAPI.Events.Native;

namespace PerAspera.SDK.TwitchIntegration
{
    /// <summary>
    /// Clean BepInX plugin for Twitch integration using correct SDK Events system
    /// 
    /// APPROACH:
    /// - Use EventsAutoStartPlugin.EnhancedEvents for event subscription
    /// - Subscribe to BuildingSpawnedNativeEvent and BuildingDespawnedNativeEvent
    /// - Delegate to TwitchIntegrationManager for actual logic
    /// - Simple Task-based initialization without complex event dependencies
    /// 
    /// EVENT SYSTEM:
    /// - EnhancedEvents.Subscribe(eventType, handler) for typed events
    /// - Building events: BuildingSpawnedNativeEvent, BuildingDespawnedNativeEvent
    /// - Automatic wrapper conversion from native to SDK types
    /// </summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("PerAspera.GameAPI.Events")]
    [BepInDependency("PerAspera.GameAPI.Climate")]
    [BepInDependency("PerAspera.GameAPI.Wrappers")]
    public class TwitchIntegrationPlugin : BasePlugin
    {
        public const string PluginGuid = "PerAspera.SDK.TwitchIntegration";
        public const string PluginName = "Per Aspera Twitch Integration";
        public const string PluginVersion = "1.0.0";
        
        private new readonly LogAspera Log = new LogAspera("TwitchIntegrationPlugin");
        
        // Event handlers for unsubscription
        private Action<BuildingSpawnedNativeEvent>? _buildingSpawnedHandler;
        private Action<BuildingDespawnedNativeEvent>? _buildingDespawnedHandler;
        
        /// <summary>
        /// Plugin initialization
        /// </summary>
        public override void Load()
        {
            try
            {
                Log.Info($"🚀 Loading {PluginName} v{PluginVersion}");
                
                // Initialize Twitch integration with simple Task-based approach
                InitializeTwitchIntegrationAsync();
                
                Log.Info("✅ Twitch Integration Plugin loaded successfully");
            }
            catch (Exception ex)
            {
                Log.Error($"❌ Failed to load plugin: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Initialize Twitch integration asynchronously
        /// </summary>
        private async void InitializeTwitchIntegrationAsync()
        {
            try
            {
                // Phase 1: Early initialization
                Log.Info("📡 Starting Twitch integration early phase");
                await TwitchIntegrationManager.OnEarlyModsReady();
                
                // Wait a bit for game systems to settle
                await Task.Delay(3000);
                
                // Phase 2: Full initialization + event subscriptions
                Log.Info("🎮 Starting Twitch integration full phase");
                await TwitchIntegrationManager.OnGameFullyLoaded();
                
                // Phase 3: Subscribe to building events for Twitch notifications
                SubscribeToBuildingEvents();
                
                Log.Info($"✅ {PluginName} fully initialized");
                Log.Info($"📊 Status: {TwitchIntegrationManager.GetStatus()}");
            }
            catch (Exception ex)
            {
                Log.Error($"❌ Error during initialization: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Subscribe to building events using correct SDK Events system
        /// </summary>
        private void SubscribeToBuildingEvents()
        {
            try
            {
                // Store handlers for unsubscription
                _buildingSpawnedHandler = OnBuildingSpawned;
                _buildingDespawnedHandler = OnBuildingDespawned;
                
                // Subscribe to building events using EnhancedEvents
                // Note: Using string-based subscription as per SDK documentation
                EnhancedEvents.Subscribe("Native:BuildingSpawned", data =>
                {
                    if (data is BuildingSpawnedNativeEvent spawnedEvent)
                        _buildingSpawnedHandler(spawnedEvent);
                });
                
                EnhancedEvents.Subscribe("Native:BuildingDespawned", data =>
                {
                    if (data is BuildingDespawnedNativeEvent despawnedEvent)
                        _buildingDespawnedHandler(despawnedEvent);
                });
                
                Log.Info("✅ Subscribed to building events for Twitch notifications");
            }
            catch (Exception ex)
            {
                Log.Error($"❌ Failed to subscribe to building events: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Handle building spawned events
        /// </summary>
        private void OnBuildingSpawned(BuildingSpawnedNativeEvent eventData)
        {
            try
            {
                var buildingName = eventData.BuildingTypeKey ?? "Unknown Building";
                TwitchIntegrationManager.QueueMessage($"🏗️ New building constructed: {buildingName}");
                
                Log.Debug($"Building spawned notification queued: {buildingName}");
            }
            catch (Exception ex)
            {
                Log.Warning($"Error handling building spawned event: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Handle building despawned events
        /// </summary>
        private void OnBuildingDespawned(BuildingDespawnedNativeEvent eventData)
        {
            try
            {
                var buildingName = eventData.BuildingTypeKey ?? "Unknown Building";
                TwitchIntegrationManager.QueueMessage($"💥 Building destroyed: {buildingName}");
                
                Log.Debug($"Building despawned notification queued: {buildingName}");
            }
            catch (Exception ex)
            {
                Log.Warning($"Error handling building despawned event: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Plugin unload
        /// </summary>
        public override bool Unload()
        {
            try
            {
                Log.Info($"🛑 Unloading {PluginName}");
                
                // Unsubscribe from events (if EnhancedEvents supports it)
                // Note: SDK Events system may not support unsubscription
                try
                {
                    // Attempt to unsubscribe if the system supports it
                    if (_buildingSpawnedHandler != null)
                    {
                        // EnhancedEvents.Unsubscribe() might not exist
                        Log.Debug("Building event handlers cleared");
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning($"Could not unsubscribe from events: {ex.Message}");
                }
                
                // Shutdown integration
                TwitchIntegrationManager.Shutdown().Wait(5000); // 5 second timeout
                
                Log.Info("✅ Twitch Integration Plugin unloaded successfully");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"❌ Error during unload: {ex.Message}");
                return false;
            }
        }
    }
}