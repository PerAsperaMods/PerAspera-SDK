using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Events.Integration;
using PerAspera.GameAPI.Events.SDK;
using PerAspera.GameAPI.Events.Constants;
using PerAspera.SDK.TwitchIntegration.Core;
using PerAspera.SDK.TwitchIntegration.Events;

namespace PerAspera.SDK.TwitchIntegration.Integration
{
    /// <summary>
    /// Bridge between TwitchLib events and Per Aspera SDK event system
    /// Converts TwitchLib events to SDK events and publishes them through EnhancedEventBus
    /// </summary>
    public class TwitchSDKEventBridge : ITwitchGameEventBridge
    {
        private static readonly LogAspera Log = LogAspera.Create("TwitchSDKEventBridge");
        
        private readonly ILogger<TwitchSDKEventBridge> _logger;
        private readonly TwitchIntegrationConfig _config;
        
        /// <summary>
        /// Initialize the Twitch-SDK event bridge
        /// </summary>
        public TwitchSDKEventBridge(ILogger<TwitchSDKEventBridge> logger, TwitchIntegrationConfig config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            
            Log.Info("TwitchSDKEventBridge initialized - ready to convert TwitchLib events to SDK events");
        }
        
        #region ITwitchGameEventBridge Implementation
        
        /// <summary>
        /// Process a Twitch follow event and convert to SDK event
        /// </summary>
        public async Task ProcessFollowEventAsync(TwitchFollowEvent followEvent)
        {
            try
            {
                _logger.LogInformation($"Processing follow event for {followEvent.DisplayName}");
                
                // Convert to SDK event
                var sdkEvent = new TwitchFollowSDKEvent(
                    channelId: followEvent.ChannelId,
                    username: followEvent.FollowerName,
                    displayName: followEvent.DisplayName,
                    temperatureBoost: _config.FollowTemperatureBoost,
                    duration: _config.FollowEffectDuration);
                
                // Publish through SDK event system
                await PublishSDKEventAsync(sdkEvent);
                
                // Auto-apply effect if enabled
                if (_config.AutoApplyFollowEffects && sdkEvent.ShouldTriggerEffects)
                {
                    var applied = sdkEvent.TryApplyFollowerEffect();
                    Log.Info($"Follow effect auto-applied for {followEvent.DisplayName}: {applied}");
                }
                
                Log.Info($"Follow event processed: {followEvent.DisplayName} -> SDK event published");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to process follow event: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Process a Twitch bits event and convert to SDK event
        /// </summary>
        public async Task ProcessBitsEventAsync(TwitchBitsEvent bitsEvent)
        {
            try
            {
                _logger.LogInformation($"Processing bits event: {bitsEvent.DisplayName} cheered {bitsEvent.BitsAmount} bits");
                
                // Convert to SDK event
                var sdkEvent = new TwitchBitsSDKEvent(
                    channelId: bitsEvent.ChannelId,
                    username: bitsEvent.Username,
                    displayName: bitsEvent.DisplayName,
                    bitsAmount: bitsEvent.BitsAmount,
                    chatMessage: bitsEvent.Message);
                
                // Publish through SDK event system
                await PublishSDKEventAsync(sdkEvent);
                
                // Auto-apply effects if enabled and significant bits
                if (_config.AutoApplyBitsEffects && bitsEvent.BitsAmount >= _config.MinimumBitsForEffect && sdkEvent.GameContext?.HasActivePlanet == true)
                {
                    var applied = sdkEvent.TryApplyBitsEffects();
                    Log.Info($"Bits effects auto-applied for {bitsEvent.DisplayName} ({bitsEvent.BitsAmount} bits): {applied}");
                }
                
                Log.Info($"Bits event processed: {bitsEvent.DisplayName} ({bitsEvent.BitsAmount} bits) -> SDK event published");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to process bits event: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Process a Twitch subscription event and convert to SDK event
        /// </summary>
        public async Task ProcessSubscriptionEventAsync(TwitchSubscriptionEvent subscriptionEvent)
        {
            try
            {
                _logger.LogInformation($"Processing subscription event: {subscriptionEvent.DisplayName} ({subscriptionEvent.SubscriptionPlan}, Gift: {subscriptionEvent.IsGift})");
                
                // Convert to SDK event
                var sdkEvent = new TwitchSubscriptionSDKEvent(
                    channelId: subscriptionEvent.ChannelId,
                    username: subscriptionEvent.Username,
                    displayName: subscriptionEvent.DisplayName,
                    subscriptionTier: subscriptionEvent.SubscriptionPlan,
                    isGift: subscriptionEvent.IsGift);
                
                // Publish through SDK event system
                await PublishSDKEventAsync(sdkEvent);
                
                // Auto-apply celebration effect if enabled
                if (_config.AutoApplySubscriptionEffects && sdkEvent.GameContext?.HasActivePlanet == true)
                {
                    var applied = sdkEvent.TryApplySubscriptionBonus();
                    Log.Info($"Subscription effects auto-applied for {subscriptionEvent.DisplayName}: {applied}");
                }
                
                Log.Info($"Subscription event processed: {subscriptionEvent.DisplayName} ({subscriptionEvent.SubscriptionPlan}) -> SDK event published");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to process subscription event: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Process a Twitch channel points event and convert to SDK event
        /// </summary>
        public async Task ProcessChannelPointsEventAsync(TwitchChannelPointsEvent channelPointsEvent)
        {
            try
            {
                _logger.LogInformation($"Processing channel points event: {channelPointsEvent.DisplayName} redeemed '{channelPointsEvent.RewardTitle}' ({channelPointsEvent.RewardCost} points)");
                
                // Convert to SDK event
                var sdkEvent = new TwitchChannelPointsSDKEvent(
                    channelId: channelPointsEvent.ChannelId,
                    username: channelPointsEvent.Username,
                    displayName: channelPointsEvent.DisplayName,
                    rewardTitle: channelPointsEvent.RewardTitle,
                    rewardCost: channelPointsEvent.RewardCost,
                    userInput: channelPointsEvent.UserInput);
                
                // Publish through SDK event system
                await PublishSDKEventAsync(sdkEvent);
                
                // Auto-apply effect if enabled and valid action
                if (_config.AutoApplyChannelPointsEffects && sdkEvent.ParsedGameAction != "CustomAction" && sdkEvent.GameContext?.HasActivePlanet == true)
                {
                    var applied = sdkEvent.TryApplyChannelPointsEffect();
                    Log.Info($"Channel points effects auto-applied for {channelPointsEvent.DisplayName} ({channelPointsEvent.RewardTitle}): {applied}");
                }
                
                Log.Info($"Channel points event processed: {channelPointsEvent.DisplayName} ({channelPointsEvent.RewardTitle}) -> SDK event published");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to process channel points event: {ex.Message}", ex);
            }
        }
        
        #endregion
        
        #region SDK Event Publishing
        
        /// <summary>
        /// Publish a Twitch SDK event through the EnhancedEventBus
        /// </summary>
        private async Task PublishSDKEventAsync<T>(T sdkEvent) where T : TwitchSDKEventBase
        {
            try
            {
                // Publish through the enhanced event bus
                EnhancedEventBus.TriggerEvent(sdkEvent);
                
                // Also publish through the legacy event system for compatibility
                EventsAutoStartPlugin.EnhancedEvents.Publish(sdkEvent);
                
                _logger.LogDebug($"SDK event published: {sdkEvent.EventType} for {sdkEvent.DisplayName}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to publish SDK event: {ex.Message}", ex);
                throw;
            }
            
            await Task.CompletedTask;
        }
        
        #endregion
        
        #region Analytics & Monitoring
        
        /// <summary>
        /// Get analytics summary for Twitch events processed
        /// </summary>
        public TwitchEventAnalytics GetAnalytics()
        {
            // This would typically track processed events over time
            // For now, return a basic structure
            return new TwitchEventAnalytics
            {
                EventsProcessedToday = 0, // TODO: Implement tracking
                MostActiveUser = "N/A",
                TotalBitsProcessed = 0,
                TotalEffectsApplied = 0,
                LastEventTimestamp = DateTime.UtcNow
            };
        }
        
        #endregion
    }
    
    /// <summary>
    /// Interface for Twitch game event bridge
    /// </summary>
    public interface ITwitchGameEventBridge
    {
        Task ProcessFollowEventAsync(TwitchFollowEvent followEvent);
        Task ProcessBitsEventAsync(TwitchBitsEvent bitsEvent);
        Task ProcessSubscriptionEventAsync(TwitchSubscriptionEvent subscriptionEvent);
        Task ProcessChannelPointsEventAsync(TwitchChannelPointsEvent channelPointsEvent);
    }
    
    /// <summary>
    /// Analytics data for Twitch event processing
    /// </summary>
    public class TwitchEventAnalytics
    {
        public int EventsProcessedToday { get; set; }
        public string MostActiveUser { get; set; } = string.Empty;
        public int TotalBitsProcessed { get; set; }
        public int TotalEffectsApplied { get; set; }
        public DateTime LastEventTimestamp { get; set; }
    }
}