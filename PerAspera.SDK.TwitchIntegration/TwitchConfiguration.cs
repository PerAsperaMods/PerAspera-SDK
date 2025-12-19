using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
using PerAspera.Core;

namespace PerAspera.SDK.TwitchIntegration
{
    /// <summary>
    /// Configuration for Twitch integration
    /// 
    /// SETUP INSTRUCTIONS:
    /// 1. Create twitch_config.json in BepInEx/plugins/ folder
    /// 2. Get OAuth token from https://twitchapps.com/tmi/
    /// 3. Configure bot username and channel
    /// 
    /// SECURITY:
    /// - Configuration file should be in .gitignore
    /// - OAuth token should be kept private
    /// </summary>
    public class TwitchConfiguration
    {
        private static readonly LogAspera Log = new LogAspera("TwitchConfig");
        
        // Default configuration file path
        private static readonly string ConfigPath = Path.Combine(BepInEx.Paths.PluginPath, "twitch_config.json");
        
        /// <summary>
        /// Bot username (your Twitch bot account name)
        /// </summary>
        public string BotUsername { get; set; } = "your_bot_username";
        
        /// <summary>
        /// OAuth token from https://twitchapps.com/tmi/
        /// Should start with "oauth:"
        /// </summary>
        public string OAuthToken { get; set; } = "oauth:your_oauth_token_here";
        
        /// <summary>
        /// Channel to monitor (streamer channel name)
        /// </summary>
        public string ChannelName { get; set; } = "your_channel_name";
        
        /// <summary>
        /// Enable climate effects from Twitch events
        /// </summary>
        public bool EnableClimateEffects { get; set; } = true;
        
        /// <summary>
        /// Enable building event notifications to Twitch
        /// </summary>
        public bool EnableBuildingNotifications { get; set; } = true;
        
        /// <summary>
        /// Command prefix for chat commands
        /// </summary>
        public string CommandPrefix { get; set; } = "!";
        
        /// <summary>
        /// Follower effect intensity (0.1 to 5.0)
        /// </summary>
        public float FollowerEffectIntensity { get; set; } = 1.0f;
        
        /// <summary>
        /// Bits effect multiplier (bits * multiplier = climate effect)
        /// </summary>
        public float BitsEffectMultiplier { get; set; } = 0.01f;
        
        /// <summary>
        /// Maximum temperature change per effect (in Kelvin)
        /// </summary>
        public float MaxTemperatureChangeK { get; set; } = 10.0f;
        
        /// <summary>
        /// Load configuration from file
        /// </summary>
        public static TwitchConfiguration Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    var json = File.ReadAllText(ConfigPath);
                    var config = JsonConvert.DeserializeObject<TwitchConfiguration>(json);
                    
                    if (config != null)
                    {
                        Log.Info($"✅ Loaded Twitch configuration: {config.BotUsername} → #{config.ChannelName}");
                        return config;
                    }
                }
                
                // Create default configuration
                Log.Warning("⚠️ No Twitch configuration found, creating default config");
                var defaultConfig = new TwitchConfiguration();
                defaultConfig.Save();
                
                Log.Info($"📄 Created default Twitch config at: {ConfigPath}");
                Log.Info("🔧 Please edit the configuration file with your Twitch credentials");
                
                return defaultConfig;
            }
            catch (Exception ex)
            {
                Log.Error($"❌ Failed to load Twitch configuration: {ex.Message}");
                return new TwitchConfiguration();
            }
        }
        
        /// <summary>
        /// Save configuration to file
        /// </summary>
        public void Save()
        {
            try
            {
                var json = JsonConvert.SerializeObject(this, Formatting.Indented);
                
                // Ensure directory exists
                var directory = Path.GetDirectoryName(ConfigPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(ConfigPath, json);
                Log.Info($"💾 Saved Twitch configuration to: {ConfigPath}");
            }
            catch (Exception ex)
            {
                Log.Error($"❌ Failed to save Twitch configuration: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Validate configuration
        /// </summary>
        public bool IsValid()
        {
            var errors = new List<string>();
            
            if (string.IsNullOrWhiteSpace(BotUsername) || BotUsername == "your_bot_username")
                errors.Add("BotUsername not configured");
            
            if (string.IsNullOrWhiteSpace(OAuthToken) || OAuthToken == "oauth:your_oauth_token_here")
                errors.Add("OAuthToken not configured");
            
            if (string.IsNullOrWhiteSpace(ChannelName) || ChannelName == "your_channel_name")
                errors.Add("ChannelName not configured");
            
            if (!OAuthToken.StartsWith("oauth:"))
                errors.Add("OAuthToken must start with 'oauth:'");
            
            if (FollowerEffectIntensity < 0.1f || FollowerEffectIntensity > 5.0f)
                errors.Add("FollowerEffectIntensity must be between 0.1 and 5.0");
            
            if (errors.Count > 0)
            {
                Log.Warning($"⚠️ Twitch configuration validation failed:");
                foreach (var error in errors)
                {
                    Log.Warning($"   - {error}");
                }
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Get setup instructions
        /// </summary>
        public static string GetSetupInstructions()
        {
            return $@"
🔧 TWITCH INTEGRATION SETUP:

1. Get OAuth Token:
   - Go to: https://twitchapps.com/tmi/
   - Login with your bot Twitch account
   - Copy the oauth token (starts with 'oauth:')

2. Edit Configuration:
   - File: {ConfigPath}
   - Set BotUsername to your bot's username
   - Set OAuthToken to the token from step 1
   - Set ChannelName to the channel to monitor

3. Enable Effects:
   - EnableClimateEffects: Twitch events affect Mars climate
   - EnableBuildingNotifications: Building events sent to Twitch chat

4. Restart Per Aspera

Example configuration:
{{
  ""BotUsername"": ""my_peraspera_bot"",
  ""OAuthToken"": ""oauth:abcd1234567890..."",
  ""ChannelName"": ""streamer_name"",
  ""EnableClimateEffects"": true,
  ""EnableBuildingNotifications"": true,
  ""CommandPrefix"": ""!"",
  ""FollowerEffectIntensity"": 1.0,
  ""BitsEffectMultiplier"": 0.01,
  ""MaxTemperatureChangeK"": 10.0
}}
";
        }
        
        /// <summary>
        /// Get configuration summary for logging
        /// </summary>
        public string GetSummary()
        {
            return $"Bot: {BotUsername} | Channel: #{ChannelName} | " +
                   $"Climate: {(EnableClimateEffects ? "ON" : "OFF")} | " +
                   $"Building Notifications: {(EnableBuildingNotifications ? "ON" : "OFF")} | " +
                   $"Follower Intensity: {FollowerEffectIntensity:F1}x | " +
                   $"Bits Multiplier: {BitsEffectMultiplier:F3}x";
        }
    }
}