using System;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using PerAspera.Core;
using PerAspera.GameAPI.Events;
using PerAspera.GameAPI.Events.Native;
using PerAspera.GameAPI.Wrappers;

namespace PerAspera.SDK.TwitchIntegration
{
    /// <summary>
    /// Clean BepInX plugin for Twitch integration using correct SDK Events system
    /// 
    /// APPROACH:
    /// - Subscribe to NativeEventHub events for building notifications
    /// - Delegate to TwitchIntegrationManager for actual logic
    /// - Simple Task-based initialization without complex event dependencies
    ///
    /// EVENT SYSTEM:
    /// - NativeEventHub.Subscribe(NativeGameEvent.*, handler) for native game events
    /// - Building events: BuildingBuilt + BuildingInternalRemove via NativeEventHub
    /// - NativeEventExtensions.ResolveBuilding(keeper) resolves Handle → Building
    /// </summary>
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    [BepInDependency("PerAspera.GameAPI.Events")]
    public class TwitchIntegrationPlugin : BasePlugin
    {
        public const string PluginGuid = "PerAspera.SDK.TwitchIntegration";
        public const string PluginName = "Per Aspera Twitch Integration";
        public const string PluginVersion = "1.0.0";
        
        private new readonly LogAspera Log = new LogAspera("TwitchIntegrationPlugin");
        
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
        /// Subscribe to building events via NativeEventHub for Twitch notifications.
        /// <para>Uses <c>BuildingBuilt</c> (construction complete) and <c>BuildingInternalRemove</c>
        /// (building removed from world). Resolves the sender Handle to a <see cref="Building"/>
        /// via <see cref="NativeEventExtensions.ResolveBuilding"/>.</para>
        /// </summary>
        private void SubscribeToBuildingEvents()
        {
            try
            {
                NativeEventHub.Subscribe(NativeGameEvent.BuildingBuilt, args =>
                {
                    try
                    {
                        var keeper = BaseGameWrapper.GetCurrent()?.NativeBaseGame?.keeper;
                        var building = args.ResolveBuilding(keeper);
                        var name = building?.buildingType?.name ?? "Unknown Building";
                        TwitchIntegrationManager.QueueMessage($"🏗️ New building constructed: {name}");
                    }
                    catch (Exception ex) { Log.Warning($"Building built notification error: {ex.Message}"); }
                });

                NativeEventHub.Subscribe(NativeGameEvent.BuildingInternalRemove, args =>
                {
                    try
                    {
                        var keeper = BaseGameWrapper.GetCurrent()?.NativeBaseGame?.keeper;
                        var building = args.ResolveBuilding(keeper);
                        var name = building?.buildingType?.name ?? "Unknown Building";
                        TwitchIntegrationManager.QueueMessage($"💥 Building destroyed: {name}");
                    }
                    catch (Exception ex) { Log.Warning($"Building removed notification error: {ex.Message}"); }
                });

                Log.Info("✅ Subscribed to building events via NativeEventHub");
            }
            catch (Exception ex)
            {
                Log.Error($"❌ Failed to subscribe to building events: {ex.Message}");
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
                        // NativeEventHub subscriptions are cleared on ClearAllSubscriptions() via EventSystemIntegration.Shutdown()
                    Log.Debug("Building event handlers will be cleared on SDK shutdown");
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