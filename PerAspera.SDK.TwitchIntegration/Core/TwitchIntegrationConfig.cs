using System;
using System.Collections.Generic;

namespace PerAspera.SDK.TwitchIntegration.Core
{
    /// <summary>
    /// Configuration for Twitch integration with Per Aspera SDK
    /// Supports both basic IRC integration and advanced TwitchLib features
    /// </summary>
    public class TwitchIntegrationConfig
    {
        // ==================== CONNECTION SETTINGS ====================
        
        /// <summary>Bot username for Twitch IRC connection</summary>
        public string BotUsername { get; set; } = string.Empty;
        
        /// <summary>OAuth token for bot authentication (oauth:xxxxx format)</summary>
        public string OAuthToken { get; set; } = string.Empty;
        
        /// <summary>Twitch channel name to join</summary>
        public string ChannelName { get; set; } = string.Empty;
        
        /// <summary>Twitch channel ID (numeric) for API/PubSub operations</summary>
        public string ChannelId { get; set; } = string.Empty;
        
        /// <summary>Twitch application client ID</summary>
        public string ClientId { get; set; } = string.Empty;
        
        /// <summary>Client secret for OAuth flows</summary>
        public string ClientSecret { get; set; } = string.Empty;
        
        // ==================== FEATURE TOGGLES ====================
        
        /// <summary>Use modern TwitchLib instead of Unity-Twitch-Chat</summary>
        public bool UseTwitchLib { get; set; } = true;
        
        /// <summary>Enable TwitchLib PubSub for real-time events</summary>
        public bool EnablePubSub { get; set; } = true;
        
        /// <summary>Enable Helix API integration</summary>
        public bool EnableHelixAPI { get; set; } = true;
        
        /// <summary>Enable follow event processing</summary>
        public bool EnableFollowEvents { get; set; } = true;
        
        /// <summary>Enable bits/cheer event processing</summary>
        public bool EnableBitsEvents { get; set; } = true;
        
        /// <summary>Enable subscription event processing</summary>
        public bool EnableSubscriptionEvents { get; set; } = true;
        
        /// <summary>Enable channel points redemption events</summary>
        public bool EnableChannelPointsEvents { get; set; } = true;
        
        // ==================== CHAT SETTINGS ====================
        
        /// <summary>Command prefix for chat commands</summary>
        public string CommandPrefix { get; set; } = "!";
        
        /// <summary>Send a message when bot connects to chat</summary>
        public bool SendConnectMessage { get; set; } = true;
        
        /// <summary>Message to send when connecting</summary>
        public string ConnectMessage { get; set; } = "🤖 Per Aspera bot connected! Use !help for commands.";
        
        /// <summary>Maximum chat messages per second</summary>
        public double MaxMessagesPerSecond { get; set; } = 1.0;
        
        /// <summary>Enable spam protection</summary>
        public bool EnableSpamProtection { get; set; } = true;
        
        // ==================== GAME EFFECT SETTINGS ====================
        
        /// <summary>Automatically apply follow effects to game</summary>
        public bool AutoApplyFollowEffects { get; set; } = true;
        
        /// <summary>Temperature boost amount for new followers</summary>
        public float FollowTemperatureBoost { get; set; } = 0.1f;
        
        /// <summary>Duration of follow effects in seconds</summary>
        public float FollowEffectDuration { get; set; } = 30.0f;
        
        /// <summary>Automatically apply bits effects to game</summary>
        public bool AutoApplyBitsEffects { get; set; } = true;
        
        /// <summary>Minimum bits required to trigger game effects</summary>
        public int MinimumBitsForEffect { get; set; } = 50;
        
        /// <summary>Bits effect calculation method (linear, logarithmic, threshold)</summary>
        public string BitsEffectMultiplier { get; set; } = "linear";
        
        /// <summary>Maximum effect multiplier for bits (safety cap)</summary>
        public float MaxBitsEffectMultiplier { get; set; } = 10.0f;
        
        /// <summary>Automatically apply subscription celebration effects</summary>
        public bool AutoApplySubscriptionEffects { get; set; } = true;
        
        /// <summary>Automatically apply channel points redemption effects</summary>
        public bool AutoApplyChannelPointsEffects { get; set; } = true;
        
        // ==================== RATE LIMITING ====================
        
        /// <summary>Global command cooldown in seconds</summary>
        public double GlobalCommandCooldown { get; set; } = 1.0;
        
        /// <summary>Per-user command cooldown in seconds</summary>
        public double UserCommandCooldown { get; set; } = 5.0;
        
        /// <summary>Moderator command cooldown reduction factor</summary>
        public double ModeratorCooldownReduction { get; set; } = 0.5;
        
        /// <summary>VIP command cooldown reduction factor</summary>
        public double VipCooldownReduction { get; set; } = 0.7;
        
        /// <summary>Subscriber command cooldown reduction factor</summary>
        public double SubscriberCooldownReduction { get; set; } = 0.8;
        
        // ==================== LOGGING & DEBUGGING ====================
        
        /// <summary>Enable detailed chat logging</summary>
        public bool EnableChatLogging { get; set; } = false;
        
        /// <summary>Enable event processing debugging</summary>
        public bool EnableEventDebugging { get; set; } = false;
        
        /// <summary>Enable performance monitoring</summary>
        public bool EnablePerformanceMonitoring { get; set; } = true;
        
        /// <summary>Log all API calls for debugging</summary>
        public bool LogApiCalls { get; set; } = false;
        
        // ==================== ADVANCED SETTINGS ====================
        
        /// <summary>Reconnection attempts before giving up</summary>
        public int MaxReconnectionAttempts { get; set; } = 5;
        
        /// <summary>Delay between reconnection attempts in seconds</summary>
        public double ReconnectionDelay { get; set; } = 10.0;
        
        /// <summary>Enable automatic fallback to Unity-Twitch-Chat if TwitchLib fails</summary>
        public bool EnableFallbackMode { get; set; } = true;
        
        /// <summary>Timeout for API calls in milliseconds</summary>
        public int ApiTimeoutMs { get; set; } = 5000;
        
        /// <summary>Enable EventSub migration readiness</summary>
        public bool EventSubReady { get; set; } = false;
        
        // ==================== VALIDATION ====================
        
        /// <summary>
        /// Validate the configuration for basic functionality
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(BotUsername))
                return false;
                
            if (string.IsNullOrEmpty(ChannelName))
                return false;
                
            if (UseTwitchLib && string.IsNullOrEmpty(OAuthToken))
                return false;
                
            if ((EnablePubSub || EnableHelixAPI) && string.IsNullOrEmpty(ClientId))
                return false;
                
            return true;
        }
        
        /// <summary>
        /// Get validation errors for configuration
        /// </summary>
        public string[] GetValidationErrors()
        {
            var errors = new List<string>();
            
            if (string.IsNullOrEmpty(BotUsername))
                errors.Add("Bot username is required");
                
            if (string.IsNullOrEmpty(ChannelName))
                errors.Add("Channel name is required");
                
            if (UseTwitchLib && string.IsNullOrEmpty(OAuthToken))
                errors.Add("OAuth token is required for TwitchLib");
                
            if ((EnablePubSub || EnableHelixAPI) && string.IsNullOrEmpty(ClientId))
                errors.Add("Client ID is required for PubSub/API features");
                
            if (string.IsNullOrEmpty(CommandPrefix))
                errors.Add("Command prefix cannot be empty");
                
            if (MaxMessagesPerSecond <= 0)
                errors.Add("Max messages per second must be positive");
                
            if (GlobalCommandCooldown < 0 || UserCommandCooldown < 0)
                errors.Add("Command cooldowns cannot be negative");
                
            if (FollowTemperatureBoost < 0 || FollowEffectDuration <= 0)
                errors.Add("Follow effect settings must be positive");
                
            if (MinimumBitsForEffect < 0 || MaxBitsEffectMultiplier <= 0)
                errors.Add("Bits effect settings must be positive");
                
            return errors.ToArray();
        }
        
        /// <summary>
        /// Create a default configuration for development/testing
        /// </summary>
        public static TwitchIntegrationConfig CreateDefault()
        {
            return new TwitchIntegrationConfig
            {
                BotUsername = "per_aspera_bot",
                ChannelName = "test_channel",
                UseTwitchLib = true,
                EnablePubSub = true,
                EnableHelixAPI = true,
                CommandPrefix = "!",
                SendConnectMessage = true,
                ConnectMessage = "🤖 Per Aspera bot connected! Use !help for commands.",
                MaxMessagesPerSecond = 1.0,
                AutoApplyFollowEffects = true,
                FollowTemperatureBoost = 0.1f,
                FollowEffectDuration = 30.0f,
                AutoApplyBitsEffects = true,
                MinimumBitsForEffect = 50,
                BitsEffectMultiplier = "linear",
                MaxBitsEffectMultiplier = 10.0f,
                AutoApplySubscriptionEffects = true,
                AutoApplyChannelPointsEffects = true,
                GlobalCommandCooldown = 1.0,
                UserCommandCooldown = 5.0,
                EnableSpamProtection = true,
                EnablePerformanceMonitoring = true,
                MaxReconnectionAttempts = 5,
                ReconnectionDelay = 10.0,
                EnableFallbackMode = true,
                ApiTimeoutMs = 5000
            };
        }
        
        /// <summary>
        /// Clone the configuration
        /// </summary>
        public TwitchIntegrationConfig Clone()
        {
            return new TwitchIntegrationConfig
            {
                BotUsername = BotUsername,
                OAuthToken = OAuthToken,
                ChannelName = ChannelName,
                ChannelId = ChannelId,
                ClientId = ClientId,
                ClientSecret = ClientSecret,
                UseTwitchLib = UseTwitchLib,
                EnablePubSub = EnablePubSub,
                EnableHelixAPI = EnableHelixAPI,
                EnableFollowEvents = EnableFollowEvents,
                EnableBitsEvents = EnableBitsEvents,
                EnableSubscriptionEvents = EnableSubscriptionEvents,
                EnableChannelPointsEvents = EnableChannelPointsEvents,
                CommandPrefix = CommandPrefix,
                SendConnectMessage = SendConnectMessage,
                ConnectMessage = ConnectMessage,
                MaxMessagesPerSecond = MaxMessagesPerSecond,
                EnableSpamProtection = EnableSpamProtection,
                AutoApplyFollowEffects = AutoApplyFollowEffects,
                FollowTemperatureBoost = FollowTemperatureBoost,
                FollowEffectDuration = FollowEffectDuration,
                AutoApplyBitsEffects = AutoApplyBitsEffects,
                MinimumBitsForEffect = MinimumBitsForEffect,
                BitsEffectMultiplier = BitsEffectMultiplier,
                MaxBitsEffectMultiplier = MaxBitsEffectMultiplier,
                AutoApplySubscriptionEffects = AutoApplySubscriptionEffects,
                AutoApplyChannelPointsEffects = AutoApplyChannelPointsEffects,
                GlobalCommandCooldown = GlobalCommandCooldown,
                UserCommandCooldown = UserCommandCooldown,
                ModeratorCooldownReduction = ModeratorCooldownReduction,
                VipCooldownReduction = VipCooldownReduction,
                SubscriberCooldownReduction = SubscriberCooldownReduction,
                EnableChatLogging = EnableChatLogging,
                EnableEventDebugging = EnableEventDebugging,
                EnablePerformanceMonitoring = EnablePerformanceMonitoring,
                LogApiCalls = LogApiCalls,
                MaxReconnectionAttempts = MaxReconnectionAttempts,
                ReconnectionDelay = ReconnectionDelay,
                EnableFallbackMode = EnableFallbackMode,
                ApiTimeoutMs = ApiTimeoutMs,
                EventSubReady = EventSubReady
            };
        }
    }
}