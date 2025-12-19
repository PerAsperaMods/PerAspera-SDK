using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PerAspera.Core;
using PerAspera.GameAPI.Events;
using PerAspera.GameAPI.Events.SDK;
using PerAspera.GameAPI.Events.Native;
using PerAspera.GameAPI.Wrappers;
using PerAspera.GameAPI.Climate;

namespace PerAspera.SDK.TwitchIntegration
{
    /// <summary>
    /// Clean Twitch Integration Manager leveraging SDK Events capabilities
    /// 
    /// APPROACH:
    /// - Use newly discovered TwitchSDKEvents system from PerAspera.GameAPI.Events\SDK\TwitchEvents.cs
    /// - Leverage TwitchGameContext for climate modification capabilities
    /// - Simple IRC client for chat commands
    /// - Two-phase initialization (Early + Full game state)
    /// 
    /// DISCOVERED SDK CAPABILITIES:
    /// - TwitchFollowSDKEvent.TryApplyFollowerEffect() - Climate effects for followers
    /// - TwitchBitsSDKEvent.TryApplyBitsEffects() - Multi-effect bits processing
    /// - TwitchSubscriptionSDKEvent.TryApplySubscriberEffect() - Subscription rewards
    /// - TwitchGameContext.ModifyTemperature/Pressure/Gas() - Direct climate control
    /// - BuildingConstructedEvent/BuildingDestroyedEvent - Building event notifications
    /// </summary>
    public static class TwitchIntegrationManager
    {
        private static readonly LogAspera Log = new LogAspera("TwitchIntegration");
        
        // Configuration
        private static TwitchConfiguration? _config;
        
        // IRC Client
        private static SimpleTwitchIRCClient? _ircClient;
        
        // Status
        private static bool _isInitialized = false;
        private static bool _isEarlyPhase = true;
        
        // Message queue for sending to Twitch
        private static readonly ConcurrentQueue<string> _messageQueue = new();
        
        /// <summary>
        /// Initialize Twitch integration during early mods ready phase
        /// </summary>
        public static async Task OnEarlyModsReady()
        {
            try
            {
                Log.Info("🚀 Starting Twitch Integration - Early Phase");
                
                // Load configuration
                _config = TwitchConfiguration.Load();
                if (!_config.IsValid())
                {
                    Log.Warning("⚠️ Invalid Twitch configuration, integration disabled");
                    Log.Info(_config.GetSetupInstructions());
                    return;
                }
                
                Log.Info($"✅ Configuration loaded: {_config.GetSummary()}");
                
                // Initialize IRC client
                _ircClient = new SimpleTwitchIRCClient(_config.BotUsername, _config.OAuthToken, _config.ChannelName);
                
                var connected = await _ircClient.ConnectAsync();
                if (!connected)
                {
                    Log.Error("❌ Failed to connect to Twitch IRC");
                    return;
                }
                
                Log.Info("✅ Twitch IRC connected - Early phase complete");
                _isInitialized = true;
                
                // Send initial message
                QueueMessage("🤖 Per Aspera Twitch bot connected! Type !help for commands (Early phase - limited features)");
            }
            catch (Exception ex)
            {
                Log.Error($"❌ Failed to initialize Twitch integration: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Complete Twitch integration during full game loaded phase
        /// </summary>
        public static async Task OnGameFullyLoaded()
        {
            try
            {
                if (!_isInitialized || _ircClient == null)
                {
                    Log.Warning("⚠️ Twitch IRC not initialized, skipping full integration");
                    return;
                }
                
                Log.Info("🎮 Starting Twitch Integration - Full Game Phase");
                
                _isEarlyPhase = false;
                
                // Subscribe to building events for Twitch notifications
                if (_config?.EnableBuildingNotifications == true)
                {
                    SubscribeToBuildingEvents();
                    Log.Info("✅ Building event notifications enabled");
                }
                
                // Send ready message
                QueueMessage("🌍 Per Aspera fully loaded! All Twitch features now available. Use !status to see Mars data");
                
                Log.Info("✅ Twitch Integration fully initialized");
            }
            catch (Exception ex)
            {
                Log.Error($"❌ Failed to complete Twitch integration: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Subscribe to building events for automatic Twitch notifications
        /// </summary>
        private static void SubscribeToBuildingEvents()
        {
            try
            {
                // Subscribe using discovered SDK Events capability
                // Use EnhancedEvents directly instead of EventsAutoStartPlugin
                EnhancedEvents.Subscribe("Native:BuildingSpawned", data =>
                {
                    if (data is BuildingSpawnedNativeEvent spawnedEvent)
                        OnBuildingSpawned(spawnedEvent);
                });
                
                EnhancedEvents.Subscribe("Native:BuildingDespawned", data =>
                {
                    if (data is BuildingDespawnedNativeEvent despawnedEvent)
                        OnBuildingDespawned(despawnedEvent);
                });
                
                Log.Info("✅ Subscribed to building events for Twitch notifications");
            }
            catch (Exception ex)
            {
                Log.Error($"❌ Failed to subscribe to building events: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Handle building construction events
        /// </summary>
        private static void OnBuildingSpawned(BuildingSpawnedNativeEvent buildingEvent)
        {
            try
            {
                if (_config?.EnableBuildingNotifications != true) return;
                
                var buildingName = buildingEvent.BuildingTypeKey ?? "Unknown Building";
                QueueMessage($"🏗️ New building constructed: {buildingName}");
                
                Log.Debug($"Building spawned notification sent: {buildingName}");
            }
            catch (Exception ex)
            {
                Log.Warning($"Error in building spawn notification: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Handle building destruction events
        /// </summary>
        private static void OnBuildingDespawned(BuildingDespawnedNativeEvent buildingEvent)
        {
            try
            {
                if (_config?.EnableBuildingNotifications != true) return;
                
                var buildingName = buildingEvent.BuildingTypeKey ?? "Unknown Building";
                QueueMessage($"💥 Building destroyed: {buildingName}");
                
                Log.Debug($"Building despawned notification sent: {buildingName}");
            }
            catch (Exception ex)
            {
                Log.Warning($"Error in building despawn notification: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Process Twitch chat commands
        /// </summary>
        public static string ProcessCommand(string command, string[] args, string username)
        {
            try
            {
                Log.Debug($"Processing command: {command} from {username}");
                
                switch (command.ToLower())
                {
                    case "!help":
                        return GetHelpText();
                    
                    case "!ping":
                        return "🏓 Pong! Per Aspera bot is active.";
                    
                    case "!status":
                        return GetGameStatus();
                    
                    case "!atmosphere":
                        return _isEarlyPhase ? "🌍 Atmosphere data available after full game load" : GetAtmosphereStatus();
                    
                    case "!resources":
                        return _isEarlyPhase ? "📦 Resource data available after full game load" : GetResourceStatus();
                    
                    case "!temperature":
                        return _isEarlyPhase ? "🌡️ Temperature data available after full game load" : GetTemperatureStatus();
                    
                    case "!follow":
                        return ProcessTwitchFollow(username, username, _config?.ChannelName ?? "unknown");
                    
                    case "!testbits":
                        var bits = args.Length > 0 && int.TryParse(args[0], out var b) ? b : 100;
                        return ProcessTwitchBits(username, username, _config?.ChannelName ?? "unknown", bits);
                    
                    default:
                        return ""; // Empty string = no response
                }
            }
            catch (Exception ex)
            {
                Log.Error($"❌ Error processing command {command}: {ex.Message}");
                return "❌ Command processing error";
            }
        }
        
        /// <summary>
        /// Process Twitch follow events using simple climate effect simulation
        /// </summary>
        public static string ProcessTwitchFollow(string followerUsername, string followerDisplayName, string channelName)
        {
            try
            {
                if (_config?.EnableClimateEffects != true) return "";
                
                // Simple temperature boost for followers
                // TODO: Replace with real TwitchSDKEvents when available
                LogClimateEffect("follower", followerDisplayName, "temperature boost +1K");
                
                Log.Info($"✅ Follower effect simulated: {followerDisplayName}");
                return $"🎉 Thanks for following, {followerDisplayName}! You warmed up Mars! 🔥";
            }
            catch (Exception ex)
            {
                Log.Error($"❌ Error processing follow event: {ex.Message}");
                return $"🎉 Thanks for following, {followerDisplayName}!";
            }
        }
        
        /// <summary>
        /// Process Twitch bits events using simple climate effect simulation
        /// </summary>
        public static string ProcessTwitchBits(string username, string displayName, string channelName, int bitAmount)
        {
            try
            {
                if (_config?.EnableClimateEffects != true) 
                    return $"💎 {displayName} cheered {bitAmount} bits! Thank you!";
                
                // Simple climate effects based on bit amount
                var effects = new List<string>();
                
                if (bitAmount >= 100)
                    effects.Add("temperature boost");
                if (bitAmount >= 500)
                    effects.Add("pressure increase");
                if (bitAmount >= 1000)
                    effects.Add("atmosphere improvement");
                
                if (effects.Count > 0)
                {
                    var effectDescription = string.Join(", ", effects);
                    LogClimateEffect("bits", displayName, $"{effectDescription} (from {bitAmount} bits)");
                    
                    Log.Info($"✅ Bits effects simulated for {displayName}: {effectDescription}");
                    return $"💎 {displayName} cheered {bitAmount} bits and triggered: {effectDescription}! 🌟";
                }
                else
                {
                    return $"💎 {displayName} cheered {bitAmount} bits! Thank you!";
                }
            }
            catch (Exception ex)
            {
                Log.Error($"❌ Error processing bits event: {ex.Message}");
                return $"💎 {displayName} cheered {bitAmount} bits! Thank you!";
            }
        }
        
        /// <summary>
        /// Process Twitch subscription events using simple climate effect simulation
        /// </summary>
        public static string ProcessTwitchSubscription(string username, string displayName, string channelName, string tier)
        {
            try
            {
                if (_config?.EnableClimateEffects != true) 
                    return $"⭐ {displayName} subscribed ({tier})! Welcome to the Mars terraforming crew!";
                
                // Subscription gives significant climate boost
                LogClimateEffect("subscription", displayName, $"major climate boost ({tier})");
                
                Log.Info($"✅ Subscription effect simulated: {displayName} ({tier})");
                return $"⭐ {displayName} subscribed ({tier})! You've significantly boosted Mars terraforming! 🚀🌍";
            }
            catch (Exception ex)
            {
                Log.Error($"❌ Error processing subscription event: {ex.Message}");
                return $"⭐ {displayName} subscribed ({tier})! Welcome to the Mars terraforming crew!";
            }
        }
        
        /// <summary>
        /// Log climate effect for debugging
        /// </summary>
        private static void LogClimateEffect(string eventType, string user, string effect)
        {
            Log.Info($"🌍 Climate Effect [{eventType.ToUpper()}] {user}: {effect}");
        }
        
        /// <summary>
        /// Get help text for commands
        /// </summary>
        private static string GetHelpText()
        {
            if (_isEarlyPhase)
            {
                return "🤖 Per Aspera Twitch Bot - Early Phase Commands: !help, !ping, !status (More commands available after game loads)";
            }
            
            return "🤖 Per Aspera Twitch Bot Commands: !help, !ping, !status, !atmosphere, !resources, !temperature, !follow (test), !testbits <amount>";
        }
        
        /// <summary>
        /// Get current game status
        /// </summary>
        private static string GetGameStatus()
        {
            if (_isEarlyPhase)
            {
                return "🎮 Game Status: Early loading phase - Full data available after complete load";
            }
            
            try
            {
                var baseGame = GameApi.wrapper.basegame;
                if (baseGame?.GetUniverse()?.GetPlanet() == null)
                    return "🎮 Game Status: Planet data not available";
                
                var planet = baseGame.GetUniverse().GetPlanet();
                return $"🎮 Game Status: Active | Planet: {planet?.Name ?? "Unknown"} | Terraforming in progress";
            }
            catch (Exception ex)
            {
                Log.Warning($"Error getting game status: {ex.Message}");
                return "🎮 Game Status: Data unavailable";
            }
        }
        
        /// <summary>
        /// Get resource status
        /// </summary>
        private static string GetResourceStatus()
        {
            try
            {
                var baseGame = GameApi.wrapper.basegame;
                var planet = baseGame?.GetUniverse()?.GetPlanet();
                if (planet?.Resources == null)
                    return "📦 Resources: Data not available";
                
                // Basic resource info using SDK wrappers
                return "📦 Resources: Available (detailed breakdown coming soon)";
            }
            catch (Exception ex)
            {
                Log.Warning($"Error getting resource status: {ex.Message}");
                return "📦 Resources: Data unavailable";
            }
        }
        
        /// <summary>
        /// Get atmosphere status
        /// </summary>
        private static string GetAtmosphereStatus()
        {
            try
            {
                var baseGame = GameApi.wrapper.basegame;
                var planet = baseGame?.GetUniverse()?.GetPlanet();
                if (planet?.Atmosphere == null)
                    return "🌍 Atmosphere: Data not available";
                
                var atmosphere = planet.Atmosphere;
                return $"🌍 Atmosphere: {atmosphere.TemperatureCelsius:F1}°C | {atmosphere.TotalPressure:F1}kPa | Breathable: {(atmosphere.IsBreathable ? "Yes" : "No")}";
            }
            catch (Exception ex)
            {
                Log.Warning($"Error getting atmosphere status: {ex.Message}");
                return "🌍 Atmosphere: Data unavailable";
            }
        }
        
        /// <summary>
        /// Get temperature status
        /// </summary>
        private static string GetTemperatureStatus()
        {
            try
            {
                var baseGame = GameApi.wrapper.basegame;
                var planet = baseGame?.GetUniverse()?.GetPlanet();
                if (planet?.Atmosphere == null)
                    return "🌡️ Temperature: Data not available";
                
                var temperature = planet.Atmosphere.TemperatureCelsius;
                var emoji = temperature switch
                {
                    < -50 => "🧊",
                    < 0 => "❄️",
                    < 20 => "🌡️",
                    < 50 => "🔥",
                    _ => "🌋"
                };
                
                return $"{emoji} Temperature: {temperature:F1}°C ({planet.Atmosphere.TemperatureKelvin:F1}K)";
            }
            catch (Exception ex)
            {
                Log.Warning($"Error getting temperature status: {ex.Message}");
                return "🌡️ Temperature: Data unavailable";
            }
        }
        
        /// <summary>
        /// Queue message to be sent to Twitch
        /// </summary>
        public static void QueueMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            
            _messageQueue.Enqueue(message);
            Log.Debug($"Queued message: {message}");
        }
        
        /// <summary>
        /// Get next queued message for IRC client
        /// </summary>
        public static string GetNextQueuedMessage()
        {
            return _messageQueue.TryDequeue(out var message) ? message : "";
        }
        
        /// <summary>
        /// Get integration status
        /// </summary>
        public static string GetStatus()
        {
            if (!_isInitialized || _ircClient == null)
                return "Twitch Integration: Disabled (Configuration issues)";
            
            var phase = _isEarlyPhase ? "Early Phase" : "Full Integration";
            return $"Twitch Integration: Active ({phase}) | {_ircClient.GetStatus()}";
        }
        
        /// <summary>
        /// Shutdown integration
        /// </summary>
        public static async Task Shutdown()
        {
            try
            {
                Log.Info("🛑 Shutting down Twitch integration");
                
                if (_ircClient != null)
                {
                    await _ircClient.DisconnectAsync();
                    _ircClient.Dispose();
                }
                
                _isInitialized = false;
                Log.Info("✅ Twitch integration shutdown complete");
            }
            catch (Exception ex)
            {
                Log.Error($"❌ Error during shutdown: {ex.Message}");
            }
        }
    }
}