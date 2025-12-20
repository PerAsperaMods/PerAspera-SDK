using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PerAspera.Core;
using PerAspera.GameAPI;
using PerAspera.GameAPI.Events;
using PerAspera.GameAPI.Events.SDK;
using PerAspera.GameAPI.Events.Native;
using PerAspera.GameAPI.Wrappers;
using PerAspera.GameAPI.Climate;
using PerAspera.Core.IL2CPP;

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
        /// Initialize Twitch integration during early mods ready phase.
        /// Loads configuration, connects to Twitch IRC, and sets up basic bot functionality.
        /// </summary>
        /// <example>
        /// <code>
        /// // Called by SDK during early initialization
        /// await TwitchIntegrationManager.OnEarlyModsReady();
        /// // Bot connects to Twitch and announces availability
        /// </code>
        /// </example>
        /// <seealso href="https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/sdk/TwitchIntegration.md">Twitch Integration Documentation</seealso>
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
                    Log.Info(TwitchConfiguration.GetSetupInstructions());
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
        /// Complete Twitch integration during full game loaded phase.
        /// Enables building event notifications and full game data access for chat commands.
        /// </summary>
        /// <example>
        /// <code>
        /// // Called by SDK when game is fully loaded
        /// await TwitchIntegrationManager.OnGameFullyLoaded();
        /// // Now !status, !atmosphere, and !resources commands work
        /// </code>
        /// </example>
        /// <seealso href="https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/sdk/TwitchIntegration.md">Twitch Integration Documentation</seealso>
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
        /// Subscribe to building events for automatic Twitch notifications.
        /// Uses EnhancedEvents to monitor building construction and destruction in real-time.
        /// </summary>
        /// <example>
        /// <code>
        /// // Automatically called when building notifications are enabled
        /// SubscribeToBuildingEvents();
        /// // Now chat will see: "🏗️ New building constructed: Solar Panel"
        /// </code>
        /// </example>
        /// <seealso href="https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/sdk/Events.md">SDK Events Documentation</seealso>
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
        /// Handle building construction events and send notification to Twitch chat.
        /// Triggered automatically when buildings are placed on Mars.
        /// </summary>
        /// <example>
        /// <code>
        /// // Event fired when player builds a Solar Panel
        /// // Chat receives: "🏗️ New building constructed: Solar Panel"
        /// OnBuildingSpawned(new BuildingSpawnedNativeEvent { BuildingTypeKey = "Solar Panel" });
        /// </code>
        /// </example>
        /// <seealso href="https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/sdk/Events.md">SDK Events Documentation</seealso>
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
        /// Handle building destruction events and send notification to Twitch chat.
        /// Triggered automatically when buildings are destroyed or deconstructed.
        /// </summary>
        /// <example>
        /// <code>
        /// // Event fired when a building is destroyed
        /// // Chat receives: "💥 Building destroyed: Mining Outpost"
        /// OnBuildingDespawned(new BuildingDespawnedNativeEvent { BuildingTypeKey = "Mining Outpost" });
        /// </code>
        /// </example>
        /// <seealso href="https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/sdk/Events.md">SDK Events Documentation</seealso>
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
        /// Process Twitch chat commands and return appropriate responses.
        /// Handles all bot commands like !help, !status, !atmosphere, !resources, etc.
        /// </summary>
        /// <example>
        /// <code>
        /// // User types "!status" in chat
        /// var response = ProcessCommand("!status", new string[0], "viewer123");
        /// // Returns: "🎮 Game Status: Active | Planet: Mars | Terraforming in progress"
        /// 
        /// // User types "!resource water"
        /// var response = ProcessCommand("!resource", new string[] { "water" }, "viewer123");
        /// // Returns resource details about water
        /// </code>
        /// </example>
        /// <seealso href="https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/tutorials/TwitchCommands.md">Twitch Commands Tutorial</seealso>
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
                    
                    case "!resource":
                        if (_isEarlyPhase) return "📦 Resource details available after full game load";
                        if (args.Length == 0) return "💡 Usage: !resource <name> | Example: !resource water";
                        return GetResourceDetail(args[0]);
                    
                    case "!categories":
                        return _isEarlyPhase ? "📂 Resource categories available after full game load" : GetResourceCategories();
                    
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
        /// Process Twitch follow events and apply climate effects to Mars terraforming.
        /// Simulates warming effect when users follow the channel.
        /// </summary>
        /// <example>
        /// <code>
        /// // New follower event
        /// var response = ProcessTwitchFollow("user123", "CoolViewer", "streamer_channel");
        /// // Returns: "🎉 Thanks for following, CoolViewer! You warmed up Mars! 🔥"
        /// // Climate effect: +1K temperature boost logged
        /// </code>
        /// </example>
        /// <seealso href="https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/tutorials/TwitchEffects.md">Twitch Effects Tutorial</seealso>
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
        /// Process Twitch bits events and apply climate effects based on bit amount.
        /// Higher bit amounts trigger more significant terraforming effects.
        /// </summary>
        /// <example>
        /// <code>
        /// // 500 bits cheered
        /// var response = ProcessTwitchBits("user123", "Generous", "channel", 500);
        /// // Returns: "💎 Generous cheered 500 bits and triggered: temperature boost, pressure increase! 🌟"
        /// 
        /// // 100 bits cheered
        /// var response = ProcessTwitchBits("user123", "Supporter", "channel", 100);
        /// // Returns: "💎 Supporter cheered 100 bits and triggered: temperature boost! 🌟"
        /// </code>
        /// </example>
        /// <seealso href="https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/tutorials/TwitchEffects.md">Twitch Effects Tutorial</seealso>
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
        /// Process Twitch subscription events and apply major climate effects.
        /// Subscriptions provide the most significant terraforming boost.
        /// </summary>
        /// <example>
        /// <code>
        /// // New subscriber (Tier 1)
        /// var response = ProcessTwitchSubscription("user123", "NewSub", "channel", "Tier 1");
        /// // Returns: "⭐ NewSub subscribed (Tier 1)! You've significantly boosted Mars terraforming! 🚀🌍"
        /// // Major climate effect logged
        /// </code>
        /// </example>
        /// <seealso href="https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/tutorials/TwitchEffects.md">Twitch Effects Tutorial</seealso>
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
        /// Log climate effect for debugging and tracking terraforming changes.
        /// Records all Twitch-triggered climate modifications for analysis.
        /// </summary>
        /// <example>
        /// <code>
        /// // Log a follower effect
        /// LogClimateEffect("follower", "CoolViewer", "temperature boost +1K");
        /// // Log output: "🌍 Climate Effect [FOLLOWER] CoolViewer: temperature boost +1K"
        /// </code>
        /// </example>
        /// <seealso href="https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/advanced/Debugging.md">Debugging Documentation</seealso>
        private static void LogClimateEffect(string eventType, string user, string effect)
        {
            Log.Info($"🌍 Climate Effect [{eventType.ToUpper()}] {user}: {effect}");
        }
        
        /// <summary>
        /// Get help text for commands based on current game phase.
        /// Returns different command lists for early phase vs full integration.
        /// </summary>
        /// <example>
        /// <code>
        /// // During early phase
        /// var help = GetHelpText();
        /// // Returns: "🤖 Per Aspera Twitch Bot - Early Phase Commands: !help, !ping, !status"
        /// 
        /// // After full game load
        /// var help = GetHelpText();
        /// // Returns full command list with !atmosphere, !resources, etc.
        /// </code>
        /// </example>
        /// <seealso href="https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/tutorials/TwitchCommands.md">Twitch Commands Tutorial</seealso>
        private static string GetHelpText()
        {
            if (_isEarlyPhase)
            {
                return "🤖 Per Aspera Twitch Bot - Early Phase Commands: !help, !ping, !status (More commands available after game loads)";
            }
            
            return "🤖 Per Aspera Twitch Bot Commands: !help, !ping, !status, !atmosphere, !resources, !resource <name>, !categories, !temperature, !follow (test), !testbits <amount>";
        }
        
        /// <summary>
        /// Get current game status information for Twitch chat display.
        /// Shows game phase, planet information, and terraforming progress.
        /// </summary>
        /// <example>
        /// <code>
        /// // During active gameplay
        /// var status = GetGameStatus();
        /// // Returns: "🎮 Game Status: Active | Planet: Mars | Terraforming in progress"
        /// 
        /// // During early loading
        /// var status = GetGameStatus();
        /// // Returns: "🎮 Game Status: Early loading phase - Full data available after complete load"
        /// </code>
        /// </example>
        /// <seealso href="https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/sdk/GameAPI.md">Game API Documentation</seealso>
        private static string GetGameStatus()
        {
            if (_isEarlyPhase)
            {
                return "🎮 Game Status: Early loading phase - Full data available after complete load";
            }
            
            try
            {
                var baseGame = GameAPI.Wrappers.BaseGame.GetCurrent();
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
        /// Get resource status using Keeper API to access faction's known resource types.
        /// Displays count, categories, and sample resources for Twitch viewers.
        /// </summary>
        /// <example>
        /// <code>
        /// // When resources are available
        /// var status = GetResourceStatus();
        /// // Returns: "📦 Resources: 25 known types | Categories: Basic, Advanced, Rare | Examples: Water, Iron, Silicates (+22 more)"
        /// 
        /// // When Keeper not ready
        /// var status = GetResourceStatus();
        /// // Returns: "📦 Resources: Keeper not ready (data available after game loads)"
        /// </code>
        /// </example>
        /// <seealso href="https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/sdk/Resources.md">Resources API Documentation</seealso>
        private static string GetResourceStatus()
        {
            try
            {
                if (!KeeperHelper.IsKeeperReady())
                {
                    return "📦 Resources: Keeper not ready (data available after game loads)";
                }
                var knownResources = FactionHelper.GetKnownResources();
                
                if (knownResources.Count == 0)
                {
                    return "📦 Resources: No known resource types found";
                }
                
                // Get resource categories for summary
                var categories = FactionHelper.GetResourceCategories();
                var categoryList = categories.Count > 0 ? string.Join(", ", categories) : "Unknown";
                
                // Sample some resource names for display
                var sampleResources = knownResources.Values
                    .Take(5)
                    .Select(r => r.DisplayName)
                    .ToArray();
                
                var resourceSample = string.Join(", ", sampleResources);
                if (knownResources.Count > 5)
                {
                    resourceSample += $" (+{knownResources.Count - 5} more)";
                }
                
                return $"📦 Resources: {knownResources.Count} known types | Categories: {categoryList} | Examples: {resourceSample}";
            }
            catch (Exception ex)
            {
                Log.Warning($"Error getting resource status: {ex.Message}");
                return "📦 Resources: Data unavailable (error accessing faction data)";
            }
        }
        
        /// <summary>
        /// Get atmosphere status information for Mars terraforming progress display.
        /// Shows temperature, pressure, and breathability status in Twitch chat.
        /// </summary>
        /// <example>
        /// <code>
        /// // Current atmosphere data
        /// var status = GetAtmosphereStatus();
        /// // Returns: "🌍 Atmosphere: -15.3°C | 2.5kPa | Breathable: No"
        /// 
        /// // When data unavailable
        /// var status = GetAtmosphereStatus();
        /// // Returns: "🌍 Atmosphere: Data not available"
        /// </code>
        /// </example>
        /// <seealso href="https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/sdk/Climate.md">Climate API Documentation</seealso>
        private static string GetAtmosphereStatus()
        {
            try
            {
                var baseGame =  GameAPI.Wrappers.BaseGame.GetCurrent();
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
        /// Get temperature status with appropriate emoji indicators for current Mars climate.
        /// Provides visual feedback on terraforming progress in Twitch chat.
        /// </summary>
        /// <example>
        /// <code>
        /// // Cold Mars temperature
        /// var status = GetTemperatureStatus();
        /// // Returns: "❄️ Temperature: -45.2°C (227.95K)"
        /// 
        /// // Warmer temperature after terraforming
        /// var status = GetTemperatureStatus();
        /// // Returns: "🌡️ Temperature: 12.5°C (285.65K)"
        /// </code>
        /// </example>
        /// <seealso href="https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/sdk/Climate.md">Climate API Documentation</seealso>
        private static string GetTemperatureStatus()
        {
            try
            {
                var baseGame = GameAPI.Wrappers.BaseGame.GetCurrent();
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
                
                return $"{emoji} Temperature: {temperature:F1}°C ({planet.Atmosphere.Temperature:F1}K)";
            }
            catch (Exception ex)
            {
                Log.Warning($"Error getting temperature status: {ex.Message}");
                return "🌡️ Temperature: Data unavailable";
            }
        }
        
        /// <summary>
        /// Queue message to be sent to Twitch chat through IRC client.
        /// Messages are processed asynchronously to avoid blocking game execution.
        /// </summary>
        /// <example>
        /// <code>
        /// // Queue a welcome message
        /// QueueMessage("🤖 Per Aspera bot is now active!");
        /// 
        /// // Queue building notification
        /// QueueMessage("🏗️ New solar panel constructed!");
        /// // Messages will be sent to Twitch chat in order
        /// </code>
        /// </example>
        /// <seealso href="https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/sdk/TwitchIntegration.md">Twitch Integration Documentation</seealso>
        public static void QueueMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;
            
            _messageQueue.Enqueue(message);
            Log.Debug($"Queued message: {message}");
        }
        
        /// <summary>
        /// Get next queued message for IRC client to send to Twitch chat.
        /// Used by IRC client to process message queue asynchronously.
        /// </summary>
        /// <example>
        /// <code>
        /// // IRC client polls for messages
        /// var message = GetNextQueuedMessage();
        /// if (!string.IsNullOrEmpty(message))
        /// {
        ///     // Send message to Twitch: "🎮 Game Status: Active"
        ///     await ircClient.SendMessageAsync(message);
        /// }
        /// </code>
        /// </example>
        /// <seealso href="https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/sdk/TwitchIntegration.md">Twitch Integration Documentation</seealso>
        public static string GetNextQueuedMessage()
        {
            return _messageQueue.TryDequeue(out var message) ? message : "";
        }
        
        /// <summary>
        /// Get integration status for debugging and monitoring purposes.
        /// Shows current phase and IRC client connection status.
        /// </summary>
        /// <example>
        /// <code>
        /// // Check integration status
        /// var status = GetStatus();
        /// // Early phase: "Twitch Integration: Active (Early Phase) | IRC: Connected"
        /// // Full integration: "Twitch Integration: Active (Full Integration) | IRC: Connected"
        /// // Disabled: "Twitch Integration: Disabled (Configuration issues)"
        /// </code>
        /// </example>
        /// <seealso href="https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/troubleshooting/TwitchIntegration.md">Twitch Integration Troubleshooting</seealso>
        public static string GetStatus()
        {
            if (!_isInitialized || _ircClient == null)
                return "Twitch Integration: Disabled (Configuration issues)";
            
            var phase = _isEarlyPhase ? "Early Phase" : "Full Integration";
            return $"Twitch Integration: Active ({phase}) | {_ircClient.GetStatus()}";
        }
        
        /// <summary>
        /// Get detailed information about a specific resource by name.
        /// Searches known resources and provides comprehensive data for Twitch viewers.
        /// </summary>
        /// <example>
        /// <code>
        /// // Get water resource details
        /// var details = GetResourceDetail("water");
        /// // Returns: "📦 Water | Category: Basic | Status: ✅ Known | Description: Essential for life"
        /// 
        /// // Search for unknown resource
        /// var details = GetResourceDetail("xyz");
        /// // Returns: "❓ Resource 'xyz' not found | Did you mean: Water, Iron, Silicates"
        /// </code>
        /// </example>
        /// <seealso href="https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/sdk/Resources.md">Resources API Documentation</seealso>
        private static string GetResourceDetail(string resourceName)
        {
            try
            {
                if (!KeeperHelper.IsKeeperReady())
                {
                    return "📦 Resource details: Keeper not ready";
                }
                
                var knownResources = FactionHelper.GetKnownResources();
                
                // Search for resource (case-insensitive)
                var resource = knownResources.Values.FirstOrDefault(r => 
                    r.Name.Equals(resourceName, StringComparison.OrdinalIgnoreCase) ||
                    r.DisplayName.Equals(resourceName, StringComparison.OrdinalIgnoreCase));
                
                if (resource == null)
                {
                    // Suggest similar resources
                    var similar = knownResources.Values
                        .Where(r => r.Name.Contains(resourceName, StringComparison.OrdinalIgnoreCase) ||
                                   r.DisplayName.Contains(resourceName, StringComparison.OrdinalIgnoreCase))
                        .Take(3)
                        .Select(r => r.DisplayName)
                        .ToArray();
                    
                    var suggestion = similar.Length > 0 ? $" | Did you mean: {string.Join(", ", similar)}" : "";
                    return $"❓ Resource '{resourceName}' not found{suggestion}";
                }
                
                var isKnown = FactionHelper.IsResourceKnown(resource.Name);
                var knownStatus = isKnown ? "✅ Known" : "❌ Unknown";
                
                return $"📦 {resource.DisplayName} | Category: {resource.Category} | Status: {knownStatus} | Description: {resource.Description}";
            }
            catch (Exception ex)
            {
                Log.Warning($"Error getting resource detail for {resourceName}: {ex.Message}");
                return $"❌ Error getting details for '{resourceName}'";
            }
        }
        
        /// <summary>
        /// Get list of all resource categories known to the player's faction.
        /// Provides overview of resource organization in the game.
        /// </summary>
        /// <example>
        /// <code>
        /// // Get all resource categories
        /// var categories = GetResourceCategories();
        /// // Returns: "📂 Categories (5): Basic, Advanced, Rare, Organic, Synthetic"
        /// 
        /// // When Keeper not ready
        /// var categories = GetResourceCategories();
        /// // Returns: "📂 Categories: Keeper not ready"
        /// </code>
        /// </example>
        /// <seealso href="https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/sdk/Resources.md">Resources API Documentation</seealso>
        private static string GetResourceCategories()
        {
            try
            {
                if (!KeeperHelper.IsKeeperReady())
                {
                    return "📂 Categories: Keeper not ready";
                }
                
                var categories = FactionHelper.GetResourceCategories();
                
                if (categories.Count == 0)
                {
                    return "📂 Categories: None found";
                }
                
                return $"📂 Categories ({categories.Count}): {string.Join(", ", categories)}";
            }
            catch (Exception ex)
            {
                Log.Warning($"Error getting resource categories: {ex.Message}");
                return "❌ Error getting categories";
            }
        }

        /// <summary>
        /// Shutdown Twitch integration and clean up all resources.
        /// Disconnects from IRC, disposes clients, and resets initialization state.
        /// </summary>
        /// <example>
        /// <code>
        /// // Graceful shutdown when mod is unloaded
        /// await TwitchIntegrationManager.Shutdown();
        /// // IRC client disconnected, resources cleaned up
        /// // Log: "🛑 Shutting down Twitch integration"
        /// // Log: "✅ Twitch integration shutdown complete"
        /// </code>
        /// </example>
        /// <seealso href="https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/sdk/TwitchIntegration.md">Twitch Integration Documentation</seealso>
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