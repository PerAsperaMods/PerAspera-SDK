using Iced.Intel;
using PerAspera.Core;
using PerAspera.GameAPI.Events.Core;
using PerAspera.GameAPI.Events.SDK;
using PerAspera.GameAPI.Wrappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PerAspera.GameAPI.Events.SDK
{
    // ==================== TWITCH INTEGRATION EVENTS ====================

    /// <summary>
    /// Base class for all Twitch-related events in the Per Aspera SDK event system
    /// Integrates seamlessly with EnhancedEventBus and provides game context
    /// </summary>
    public abstract class TwitchSDKEventBase : SDKEventBase
    {
        /// <summary>Twitch channel ID where the event occurred</summary>
        public string ChannelId { get; }
        
        /// <summary>Twitch username (login name)</summary>
        public string Username { get; }
        
        /// <summary>Twitch display name (formatted name)</summary>
        public string DisplayName { get; }
        
        /// <summary>Event timestamp</summary>
        public DateTime EventTimestamp { get; }
        
        /// <summary>Game context at the time of the event</summary>
        public TwitchGameContext? GameContext { get; }
        
        protected TwitchSDKEventBase(string channelId, string username, string displayName)
        {
            ChannelId = channelId ?? throw new ArgumentNullException(nameof(channelId));
            Username = username ?? throw new ArgumentNullException(nameof(username));
            DisplayName = displayName ?? username;
            EventTimestamp = DateTime.UtcNow;
            GameContext = TryCreateGameContext();
        }
        
        /// <summary>
        /// Safely create game context if available
        /// </summary>
        private TwitchGameContext? TryCreateGameContext()
        {
            try
            {
                var baseGame = PerAspera.GameAPI.Wrappers.BaseGameWrapper.GetCurrent();
                if (baseGame?.GetUniverse()?.GetPlanet() != null)
                {
                    return new TwitchGameContext(baseGame);
                }
            }
            catch (Exception ex)
            {
                var logger = new PerAspera.Core.LogAspera("TwitchEvents");
                logger.Warning($"Failed to create game context for Twitch event: {ex.Message}");
            }
            return null;
        }
    }

    /// <summary>
    /// Game context snapshot for Twitch events
    /// Provides safe access to current game state
    /// </summary>
    public class TwitchGameContext
    {
        public bool IsGameLoaded { get; }
        public bool HasActivePlanet { get; }
        public string PlanetName { get; }
        public float Temperature { get; }
        public float Pressure { get; }
        public float Oxygen { get; }
        public int TotalBuildings { get; }
        public int ActiveBuildings { get; }
        public DateTime SnapshotTime { get; }
        
        public TwitchGameContext(PerAspera.GameAPI.Wrappers.BaseGameWrapper baseGame)
        {
            SnapshotTime = DateTime.UtcNow;
            IsGameLoaded = baseGame != null;
            BaseGameWrapper _b= baseGame;
            GameAPI.Wrappers.UniverseWrapper _u= _b.GetUniverse();
            GameAPI.Wrappers.PlanetWrapper _p= _u.GetPlanet();
            if (_u != null)
            {
                HasActivePlanet = true;
                PlanetName = _p.Name ?? "Unknown Planet";
                
                try
                {
                    // TODO: Adapt to cellular atmosphere API once AtmosphereGrid is complete
                    // Safely get climate data
                    // Temperature = _p.Atmosphere.Temperature;
                    // Pressure = _p.Atmosphere.TotalPressure;
                    // Oxygen = _p.Atmosphere.Composition["O2"].PartialPressure;
                    Temperature = 210.0f; // Default Martian temperature
                    Pressure = 6.77f; // Default Martian pressure
                    Oxygen = 0.13f; // Default Martian oxygen partial pressure

                    // Safely get building counts
                    var faction = _u.GetPlayerFaction();
                    var buildingsList = faction.GetBuildings();
                    TotalBuildings = buildingsList?.Count ?? 0;
                    if (buildingsList != null)
                    {
                        ActiveBuildings = buildingsList.Where(b => b != null && b.IsActive).Count();
                    }
                    else
                    {
                        ActiveBuildings = 0;
                    }
                }
                catch (Exception ex)
                {
                    PerAspera.Core.LogAspera.LogError($"Failed to capture complete game context: {ex.Message}");
                }
            }
            else
            {
                HasActivePlanet = false;
                PlanetName = "No Active Planet";
            }
        }
    }

    // ==================== SPECIFIC TWITCH EVENTS ====================

    /// <summary>
    /// Twitch follower event integrated with Per Aspera SDK event system
    /// Triggered when someone follows the channel
    /// </summary>
    public class TwitchFollowSDKEvent : TwitchSDKEventBase
    {
        public override string EventType => "TwitchFollow";
        
        /// <summary>Suggested climate boost for new follower</summary>
        public float SuggestedTemperatureBoost { get; }
        
        /// <summary>Effect duration in seconds</summary>
        public float EffectDuration { get; }
        
        /// <summary>Whether this event should trigger immediate game effects</summary>
        public bool ShouldTriggerEffects { get; }
        
        public TwitchFollowSDKEvent(
            string channelId, 
            string username, 
            string displayName,
            float temperatureBoost = 0.1f,
            float duration = 30.0f) : base(channelId, username, displayName)
        {
            SuggestedTemperatureBoost = temperatureBoost;
            EffectDuration = duration;
            ShouldTriggerEffects = GameContext?.HasActivePlanet == true;
        }
        
        /// <summary>
        /// Apply the follower effect to the game if possible
        /// </summary>
        public bool TryApplyFollowerEffect()
        {
            if (!ShouldTriggerEffects || GameContext == null) return false;
            
            try
            {
                // TODO: Adapt to cellular atmosphere API once AtmosphereGrid is complete
                // Apply climate boost via Climate wrapper
                // GameAPI.Wrappers.PlanetWrapper _p= PerAspera.GameAPI.Wrappers.BaseGameWrapper.GetCurrent().GetUniverse().GetPlanet();
                // var climateWrapper = _p;
                // climateWrapper?.Atmosphere.ModifyTemperature(SuggestedTemperatureBoost, EffectDuration, $"Twitch follower: {DisplayName}");
                
                LogAspera.LogInfo($"Follower effect queued for {DisplayName}: +{SuggestedTemperatureBoost:F1}°C for {EffectDuration}s (cellular API pending)");
                return true;
            }
            catch (Exception ex)
            {
                PerAspera.Core.LogAspera.LogError($"Failed to apply follower effect: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Twitch bits/cheer event integrated with Per Aspera SDK event system
    /// Triggered when someone cheers bits in chat
    /// </summary>
    public class TwitchBitsSDKEvent : TwitchSDKEventBase
    {
        public override string EventType => "TwitchBits";
        
        /// <summary>Number of bits cheered</summary>
        public int BitsAmount { get; }
        
        /// <summary>Chat message with the bits</summary>
        public string ChatMessage { get; }
        
        /// <summary>Calculated effect multiplier based on bits amount</summary>
        public float EffectMultiplier { get; }
        
        /// <summary>Suggested climate effects</summary>
        public TwitchClimateEffects SuggestedEffects { get; }
        
        public TwitchBitsSDKEvent(
            string channelId,
            string username,
            string displayName,
            int bitsAmount,
            string chatMessage = "") : base(channelId, username, displayName)
        {
            BitsAmount = bitsAmount;
            ChatMessage = chatMessage;
            EffectMultiplier = CalculateEffectMultiplier(bitsAmount);
            SuggestedEffects = CalculateClimateEffects(bitsAmount);
        }
        
        private float CalculateEffectMultiplier(int bits)
        {
            return Math.Min(bits / 100.0f, 10.0f); // Linear scaling, capped at 10x
        }
        
        private TwitchClimateEffects CalculateClimateEffects(int bits)
        {
            var baseMultiplier = CalculateEffectMultiplier(bits);
            
            return new TwitchClimateEffects
            {
                TemperatureChange = baseMultiplier * 0.5f,
                PressureChange = baseMultiplier * 0.3f,
                OxygenChange = baseMultiplier * 0.4f,
                Duration = Math.Min(30.0f + (bits / 50.0f), 300.0f), // 30s to 5min
                Source = $"Bits from {DisplayName} ({bits} bits)"
            };
        }
        
        /// <summary>
        /// Apply bits effects to the game if possible
        /// </summary>
        public bool TryApplyBitsEffects()
        {
            if (GameContext?.HasActivePlanet != true) return false;
            
            try
            {
                // TODO: Adapt to cellular atmosphere API once AtmosphereGrid is complete
                // GameAPI.Wrappers.PlanetWrapper _p= PerAspera.GameAPI.Wrappers.BaseGameWrapper.GetCurrent().GetUniverse().GetPlanet();
                // var climateWrapper = _p.Atmosphere;
                // if (climateWrapper == null) return false;
                
                // // Apply multiple climate effects
                // climateWrapper.ModifyTemperature(SuggestedEffects.TemperatureChange, SuggestedEffects.Duration, SuggestedEffects.Source);
                // climateWrapper.ModifyPressure(SuggestedEffects.PressureChange, SuggestedEffects.Duration, SuggestedEffects.Source);
                // climateWrapper.ModifyGas("O2",SuggestedEffects.OxygenChange, SuggestedEffects.Duration, SuggestedEffects.Source);

                LogAspera.LogInfo($"Bits effects queued for {DisplayName} ({BitsAmount} bits): " +
                             $"Temp +{SuggestedEffects.TemperatureChange:F1}, " +
                             $"Pressure +{SuggestedEffects.PressureChange:F1}, " +
                             $"O2 +{SuggestedEffects.OxygenChange:F1} for {SuggestedEffects.Duration:F0}s (cellular API pending)");
                return true;
            }
            catch (Exception ex)
            {
                PerAspera.Core.LogAspera.LogError($"Failed to apply bits effects: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Twitch subscription event integrated with Per Aspera SDK event system
    /// Triggered when someone subscribes to the channel
    /// </summary>
    public class TwitchSubscriptionSDKEvent : TwitchSDKEventBase
    {
        public override string EventType => "TwitchSubscription";
        
        /// <summary>Subscription tier (Tier 1, Tier 2, Tier 3, Prime)</summary>
        public string SubscriptionTier { get; }
        
        /// <summary>Whether this is a gift subscription</summary>
        public bool IsGift { get; }
        
        /// <summary>Calculated permanent bonus percentage</summary>
        public float PermanentBonusPercentage { get; }
        
        /// <summary>Suggested building efficiency boost</summary>
        public float BuildingEfficiencyBoost { get; }
        
        public TwitchSubscriptionSDKEvent(
            string channelId,
            string username,
            string displayName,
            string subscriptionTier,
            bool isGift = false) : base(channelId, username, displayName)
        {
            SubscriptionTier = subscriptionTier ?? "Tier 1";
            IsGift = isGift;
            PermanentBonusPercentage = CalculatePermanentBonus(subscriptionTier);
            BuildingEfficiencyBoost = PermanentBonusPercentage * 0.5f; // Half for building efficiency
        }
        
        private float CalculatePermanentBonus(string tier)
        {
            return tier?.ToLower() switch
            {
                "tier 3" or "prime" => 0.15f, // 15% bonus
                "tier 2" => 0.10f, // 10% bonus
                "tier 1" or "1000" => 0.05f, // 5% bonus
                _ => 0.02f // 2% default
            };
        }
        
        /// <summary>
        /// Apply subscription bonus effects to the game
        /// Note: Permanent bonuses require special handling in mod implementation
        /// </summary>
        public bool TryApplySubscriptionBonus()
        {
            if (GameContext?.HasActivePlanet != true) return false;
            
            try
            {
                // TODO: Adapt to cellular atmosphere API once AtmosphereGrid is complete
                // For immediate effect, apply a temporary massive boost
                // GameAPI.Wrappers.PlanetWrapper _p= PerAspera.GameAPI.Wrappers.BaseGameWrapper.GetCurrent().GetUniverse().GetPlanet();
                // var climateWrapper = _p.Atmosphere;
                // if (climateWrapper != null)
                // {
                //     var immediateBoost = PermanentBonusPercentage * 10.0f; // 10x for immediate effect
                //     climateWrapper.ModifyTemperature(immediateBoost, 60.0f, $"Subscription celebration: {DisplayName}");
                // }
                
                LogAspera.LogInfo($"Subscription bonus queued for {DisplayName} ({SubscriptionTier}, Gift: {IsGift}): " +
                             $"{PermanentBonusPercentage:P0} permanent bonus, immediate celebration effect (cellular API pending)");
                return true;
            }
            catch (Exception ex)
            {
                PerAspera.Core.LogAspera.LogError($"Failed to apply subscription bonus: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Twitch channel points redemption event integrated with Per Aspera SDK event system
    /// </summary>
    public class TwitchChannelPointsSDKEvent : TwitchSDKEventBase
    {
        public override string EventType => "TwitchChannelPoints";
        
        /// <summary>Name of the redeemed reward</summary>
        public string RewardTitle { get; }
        
        /// <summary>Cost of the reward in channel points</summary>
        public int RewardCost { get; }
        
        /// <summary>User input for the redemption</summary>
        public string UserInput { get; }
        
        /// <summary>Parsed game action from reward title</summary>
        public string ParsedGameAction { get; }
        
        /// <summary>Calculated effect intensity</summary>
        public float EffectIntensity { get; }
        
        public TwitchChannelPointsSDKEvent(
            string channelId,
            string username,
            string displayName,
            string rewardTitle,
            int rewardCost,
            string userInput = "") : base(channelId, username, displayName)
        {
            RewardTitle = rewardTitle ?? "Unknown Reward";
            RewardCost = rewardCost;
            UserInput = userInput;
            ParsedGameAction = ParseGameAction(rewardTitle);
            EffectIntensity = Math.Min(rewardCost / 1000.0f, 10.0f); // Scale by thousands
        }
        
        private string ParseGameAction(string rewardTitle)
        {
            var titleLower = rewardTitle.ToLower();
            
            return titleLower switch
            {
                var t when t.Contains("temperature") => "ModifyTemperature",
                var t when t.Contains("pressure") => "ModifyPressure",
                var t when t.Contains("oxygen") => "ModifyOxygen",
                var t when t.Contains("disaster") => "TriggerDisaster",
                var t when t.Contains("building") => "BuildingBoost",
                var t when t.Contains("research") => "ResearchBoost",
                _ => "CustomAction"
            };
        }
        
        /// <summary>
        /// Apply channel points effect based on parsed action
        /// </summary>
        public bool TryApplyChannelPointsEffect()
        {
            if (GameContext?.HasActivePlanet != true) return false;
            
            try
            {
                // TODO: Adapt to cellular atmosphere API once AtmosphereGrid is complete
                // var climateWrapper = BaseGameWrapper.GetCurrent().GetUniverse().GetPlanet().Atmosphere;
                // if (climateWrapper == null) return false;
                
                var effectDuration = Math.Min(60.0f + (RewardCost / 100.0f), 300.0f);
                var source = $"Channel Points: {RewardTitle} by {DisplayName}";
                
                switch (ParsedGameAction)
                {
                    case "ModifyTemperature":
                        // climateWrapper.ModifyTemperature(EffectIntensity, effectDuration, source);
                        LogAspera.LogInfo($"Temperature modification queued: {EffectIntensity:F1}°C for {effectDuration:F0}s");
                        break;
                    case "ModifyPressure":
                        // climateWrapper.ModifyPressure(EffectIntensity * 0.7f, effectDuration, source);
                        LogAspera.LogInfo($"Pressure modification queued: {EffectIntensity * 0.7f:F1} mbar for {effectDuration:F0}s");
                        break;
                    case "ModifyOxygen":
                        // climateWrapper.ModifyGas("O2",EffectIntensity * 0.8f, effectDuration, source);
                        LogAspera.LogInfo($"Oxygen modification queued: {EffectIntensity * 0.8f:F1} for {effectDuration:F0}s");
                        break;
                    case "TriggerDisaster":
                        // Note: Disaster triggering would require additional implementation
                        LogAspera.LogInfo($"Disaster trigger requested by {DisplayName} - requires custom implementation");
                        return false;
                    default:
                        // Apply general positive effect
                        // climateWrapper.ModifyTemperature(EffectIntensity * 0.3f, effectDuration, source);
                        LogAspera.LogInfo($"General effect queued: {EffectIntensity * 0.3f:F1}°C for {effectDuration:F0}s");
                        break;
                }
                
                LogAspera.LogInfo($"Channel points effect queued: {ParsedGameAction} ({EffectIntensity:F1}x) for {effectDuration:F0}s (cellular API pending)");
                return true;
            }
            catch (Exception ex)
            {
                PerAspera.Core.LogAspera.LogError($"Failed to apply channel points effect: {ex.Message}");
                return false;
            }
        }
    }

    // ==================== HELPER CLASSES ====================

    /// <summary>
    /// Climate effects structure for Twitch events
    /// </summary>
    public class TwitchClimateEffects
    {
        public float TemperatureChange { get; set; }
        public float PressureChange { get; set; }
        public float OxygenChange { get; set; }
        public float Duration { get; set; }
        public string Source { get; set; } = string.Empty;
    }
}