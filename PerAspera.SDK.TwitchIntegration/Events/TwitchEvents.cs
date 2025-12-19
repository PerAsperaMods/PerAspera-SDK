using System;
using PerAspera.GameAPI.Events.SDK;

namespace PerAspera.SDK.TwitchIntegration.Events
{
    /// <summary>
    /// Base class for all Twitch-related events in Per Aspera
    /// </summary>
    public abstract class TwitchEventBase : ISDKEvent
    {
        public string EventId { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string ChannelId { get; set; } = string.Empty;
        public abstract string EventType { get; }
        
        // ISDKEvent implementation
        public virtual object? GetEventData() => this;
    }
    
    /// <summary>
    /// Twitch follow event for Per Aspera game integration
    /// Triggers positive climate effects and notifications
    /// </summary>
    public class TwitchFollowEvent : TwitchEventBase
    {
        public override string EventType => "TwitchFollow";
        
        public string FollowerName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public float TemperatureBoost { get; set; } = 0.1f; // Default small temperature boost
        public string EffectType { get; set; } = "TemperatureBoost";
        public float Duration { get; set; } = 30.0f; // Effect duration in seconds
        
        public override string ToString() => 
            $"[TwitchFollow] {DisplayName} followed - {EffectType}: +{TemperatureBoost:F1}°C for {Duration}s";
    }
    
    /// <summary>
    /// Twitch bits/cheer event for Per Aspera game integration
    /// Scales effects based on bits amount
    /// </summary>
    public class TwitchBitsEvent : TwitchEventBase
    {
        public override string EventType => "TwitchBits";
        
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public int BitsAmount { get; set; }
        public string Message { get; set; } = string.Empty;
        public float EffectMultiplier { get; set; } = 1.0f;
        public string EffectType { get; set; } = "MultipleBoosts";
        
        /// <summary>
        /// Calculate scaled temperature effect based on bits
        /// </summary>
        public float GetTemperatureEffect() => Math.Min(BitsAmount / 100.0f, 5.0f);
        
        /// <summary>
        /// Calculate scaled pressure effect based on bits
        /// </summary>
        public float GetPressureEffect() => Math.Min(BitsAmount / 200.0f, 2.5f);
        
        /// <summary>
        /// Calculate scaled oxygen effect based on bits
        /// </summary>
        public float GetOxygenEffect() => Math.Min(BitsAmount / 150.0f, 3.0f);
        
        /// <summary>
        /// Calculate effect duration based on bits amount
        /// </summary>
        public float GetEffectDuration() => Math.Min(30.0f + (BitsAmount / 50.0f), 300.0f); // 30s to 5min max
        
        public override string ToString() => 
            $"[TwitchBits] {DisplayName} cheered {BitsAmount} bits - Effects: {EffectMultiplier:F1}x for {GetEffectDuration():F0}s";
    }
    
    /// <summary>
    /// Twitch subscription event for Per Aspera game integration
    /// Provides permanent bonuses based on subscription tier
    /// </summary>
    public class TwitchSubscriptionEvent : TwitchEventBase
    {
        public override string EventType => "TwitchSubscription";
        
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string SubscriptionPlan { get; set; } = string.Empty;
        public bool IsGift { get; set; }
        public float PermanentBonus { get; set; }
        public string BonusType { get; set; } = "GlobalEfficiency";
        
        /// <summary>
        /// Get building efficiency bonus based on subscription tier
        /// </summary>
        public float GetBuildingEfficiencyBonus()
        {
            return SubscriptionPlan.ToLower() switch
            {
                "tier 3" or "prime" => 0.15f, // 15% efficiency bonus
                "tier 2" => 0.10f, // 10% efficiency bonus
                "tier 1" or "1000" => 0.05f, // 5% efficiency bonus
                _ => 0.02f // 2% default bonus
            };
        }
        
        /// <summary>
        /// Get resource production bonus based on subscription tier
        /// </summary>
        public float GetResourceProductionBonus()
        {
            return SubscriptionPlan.ToLower() switch
            {
                "tier 3" or "prime" => 0.20f, // 20% production bonus
                "tier 2" => 0.15f, // 15% production bonus
                "tier 1" or "1000" => 0.10f, // 10% production bonus
                _ => 0.05f // 5% default bonus
            };
        }
        
        public override string ToString() => 
            $"[TwitchSubscription] {DisplayName} subscribed ({SubscriptionPlan}, Gift: {IsGift}) - Permanent bonus: +{PermanentBonus:P0}";
    }
    
    /// <summary>
    /// Twitch channel points redemption event for Per Aspera game integration
    /// Enables custom viewer interactions with game systems
    /// </summary>
    public class TwitchChannelPointsEvent : TwitchEventBase
    {
        public override string EventType => "TwitchChannelPoints";
        
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string RewardTitle { get; set; } = string.Empty;
        public int RewardCost { get; set; }
        public string UserInput { get; set; } = string.Empty;
        public string GameAction { get; set; } = string.Empty;
        public float ActionIntensity { get; set; } = 1.0f;
        
        /// <summary>
        /// Parse common reward types and map to game actions
        /// </summary>
        public void ParseRewardAction()
        {
            var rewardLower = RewardTitle.ToLower();
            
            GameAction = rewardLower switch
            {
                var r when r.Contains("temperature") => "ModifyTemperature",
                var r when r.Contains("pressure") => "ModifyPressure",
                var r when r.Contains("oxygen") => "ModifyOxygen",
                var r when r.Contains("disaster") => "TriggerDisaster",
                var r when r.Contains("building") => "BuildingEvent",
                var r when r.Contains("research") => "ResearchBoost",
                var r when r.Contains("resource") => "ResourceBonus",
                _ => "CustomAction"
            };
            
            // Calculate intensity based on cost
            ActionIntensity = Math.Min(RewardCost / 1000.0f, 10.0f); // Scale by thousands, cap at 10x
        }
        
        /// <summary>
        /// Get temperature modification amount based on reward
        /// </summary>
        public float GetTemperatureModification()
        {
            ParseRewardAction();
            return GameAction == "ModifyTemperature" ? ActionIntensity * 0.5f : 0.0f;
        }
        
        /// <summary>
        /// Get pressure modification amount based on reward
        /// </summary>
        public float GetPressureModification()
        {
            ParseRewardAction();
            return GameAction == "ModifyPressure" ? ActionIntensity * 0.3f : 0.0f;
        }
        
        /// <summary>
        /// Check if reward should trigger a random disaster
        /// </summary>
        public bool ShouldTriggerDisaster()
        {
            ParseRewardAction();
            return GameAction == "TriggerDisaster" && ActionIntensity > 0.5f;
        }
        
        public override string ToString() => 
            $"[TwitchChannelPoints] {DisplayName} redeemed '{RewardTitle}' ({RewardCost} points) - Action: {GameAction} ({ActionIntensity:F1}x)";
    }
    
    /// <summary>
    /// Twitch raid event for Per Aspera game integration
    /// Triggers massive positive effects based on raid size
    /// </summary>
    public class TwitchRaidEvent : TwitchEventBase
    {
        public override string EventType => "TwitchRaid";
        
        public string RaiderUsername { get; set; } = string.Empty;
        public string RaiderDisplayName { get; set; } = string.Empty;
        public int ViewerCount { get; set; }
        public float MassiveEffectMultiplier { get; set; }
        
        /// <summary>
        /// Calculate effect multiplier based on raid size
        /// </summary>
        public void CalculateRaidEffects()
        {
            MassiveEffectMultiplier = ViewerCount switch
            {
                >= 1000 => 20.0f, // Massive raid
                >= 500 => 15.0f,  // Large raid
                >= 100 => 10.0f,  // Medium raid
                >= 50 => 5.0f,    // Small raid
                >= 10 => 3.0f,    // Mini raid
                _ => 1.0f         // Tiny raid
            };
        }
        
        /// <summary>
        /// Get terraforming boost from raid
        /// </summary>
        public float GetTerraformingBoost()
        {
            CalculateRaidEffects();
            return MassiveEffectMultiplier * 2.0f; // Double multiplier for terraforming
        }
        
        /// <summary>
        /// Get resource production boost from raid
        /// </summary>
        public float GetResourceProductionBoost()
        {
            CalculateRaidEffects();
            return MassiveEffectMultiplier * 1.5f; // 1.5x multiplier for resources
        }
        
        /// <summary>
        /// Get effect duration from raid (longer for bigger raids)
        /// </summary>
        public float GetEffectDuration()
        {
            CalculateRaidEffects();
            return Math.Min(60.0f + (ViewerCount * 0.5f), 600.0f); // 1-10 minutes based on size
        }
        
        public override string ToString() => 
            $"[TwitchRaid] {RaiderDisplayName} raided with {ViewerCount} viewers - Effects: {MassiveEffectMultiplier:F1}x for {GetEffectDuration():F0}s";
    }
    
    /// <summary>
    /// Twitch host event for Per Aspera game integration
    /// Provides sustained bonuses during host period
    /// </summary>
    public class TwitchHostEvent : TwitchEventBase
    {
        public override string EventType => "TwitchHost";
        
        public string HostUsername { get; set; } = string.Empty;
        public string HostDisplayName { get; set; } = string.Empty;
        public int HostViewerCount { get; set; }
        public bool IsAutoHost { get; set; }
        public float SustainedBonus { get; set; }
        
        /// <summary>
        /// Calculate sustained bonus based on host viewer count
        /// </summary>
        public void CalculateHostBonus()
        {
            SustainedBonus = Math.Min(HostViewerCount / 100.0f, 5.0f); // Scale by hundreds, cap at 5x
            
            if (IsAutoHost)
            {
                SustainedBonus *= 0.5f; // Auto-hosts get reduced bonus
            }
        }
        
        /// <summary>
        /// Get building efficiency bonus during host
        /// </summary>
        public float GetBuildingEfficiencyBonus()
        {
            CalculateHostBonus();
            return SustainedBonus * 0.1f; // 10% per bonus point
        }
        
        public override string ToString() => 
            $"[TwitchHost] {HostDisplayName} hosting with {HostViewerCount} viewers (Auto: {IsAutoHost}) - Bonus: {SustainedBonus:F1}x";
    }
}