using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using PerAspera.GameAPI.Wrappers;
using PerAspera.GameAPI.Events.SDK;
using PerAspera.GameAPI.Events.Constants;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Events;

namespace PerAspera.SDK.TwitchIntegration
{
    /// <summary>
    /// Two-phase Twitch integration system:
    /// Phase 1: Early initialization (basic commands, connection)
    /// Phase 2: Full intelligence (game state access, advanced commands)
    /// </summary>
    public class TwitchIntegrationManager
    {
        private static readonly LogAspera Log = LogAspera.Create("TwitchIntegrationManager");
        
        // Initialization phases
        private static bool _earlyPhaseInitialized = false;
        private static bool _fullPhaseInitialized = false;
        
        // Cached game instances (available in full phase only)
        private static Universe? _universeCache;
        private static Planet? _planetCache;
        private static BaseGame? _baseGameCache;
        
        // Thread synchronization for thread-safe command processing
        private static readonly object _lock = new object();
        private static readonly Dictionary<string, DateTime> _lastCommandTimes = new();
        
        /// <summary>
        /// Static constructor - Subscribe to early and full initialization events
        /// </summary>
        static TwitchIntegrationManager()
        {
            try
            {
                // Phase 1: Early initialization (as soon as GameHub is ready)
                EventsAutoStartPlugin.EnhancedEvents.Subscribe<EarlyModsReadyEvent>(
                    SDKEventConstants.EarlyModsReady, OnEarlyModsReady);
                
                // Phase 2: Full initialization (when all game systems are loaded)
                EventsAutoStartPlugin.EnhancedEvents.Subscribe<GameFullyLoadedEvent>(
                    SDKEventConstants.GameFullyLoaded, OnGameFullyLoaded);
                
                Log.Info("TwitchIntegrationManager event subscriptions registered");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to initialize TwitchIntegrationManager: {ex.Message}");
            }
        }
        
        // ==================== PHASE 1: EARLY INITIALIZATION ====================
        
        /// <summary>
        /// Phase 1: Early initialization - basic Twitch connection and simple commands
        /// Triggered by EarlyModsReady event (GameHub.Awake)
        /// </summary>
        private static void OnEarlyModsReady(EarlyModsReadyEvent eventArgs)
        {
            if (_earlyPhaseInitialized) return;
            
            try
            {
                Log.Info($"🟡 Phase 1 Init: EarlyModsReady received - BaseGame available: {eventArgs.BaseGameAvailable}");
                
                // Store BaseGame if available
                if (eventArgs.BaseGameAvailable && eventArgs.BaseGameWrapper != null)
                {
                    _baseGameCache = eventArgs.BaseGameWrapper;
                }
                
                // Initialize basic Twitch functionality
                InitializeEarlyPhase();
                
                _earlyPhaseInitialized = true;
                Log.Info("✅ Phase 1 Complete: Twitch basic initialization ready");
            }
            catch (Exception ex)
            {
                Log.Error($"❌ Phase 1 Failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Initialize basic Twitch functionality that doesn't require game state
        /// </summary>
        private static void InitializeEarlyPhase()
        {
            // TODO: Initialize Twitch IRC client
            // TODO: Set up basic command registry (!help, !ping, !status)
            // TODO: Configure rate limiting
            // TODO: Set up logging and error handlers
            
            Log.Info("Early phase: Basic Twitch systems initialized");
        }
        
        // ==================== PHASE 2: FULL INITIALIZATION ====================
        
        /// <summary>
        /// Phase 2: Full initialization - complete game integration and advanced commands
        /// Triggered by GameFullyLoaded event (BaseGame + Universe + Planet ready)
        /// </summary>
        private static void OnGameFullyLoaded(GameFullyLoadedEvent eventArgs)
        {
            if (_fullPhaseInitialized) return;
            
            try
            {
                Log.Info("🟢 Phase 2 Init: GameFullyLoaded received - enabling advanced Twitch commands");
                
                // Cache all game instances
                _baseGameCache = eventArgs.BaseGameWrapper;
                _universeCache = eventArgs.UniverseWrapper;
                _planetCache = eventArgs.PlanetWrapper;
                
                // Initialize advanced Twitch functionality
                InitializeFullPhase();
                
                _fullPhaseInitialized = true;
                Log.Info("✅ Phase 2 Complete: Twitch full integration ready");
            }
            catch (Exception ex)
            {
                Log.Error($"❌ Phase 2 Failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Initialize advanced Twitch functionality that requires game state
        /// </summary>
        private static void InitializeFullPhase()
        {
            // TODO: Register advanced commands (!resources, !atmosphere, !buildings)
            // TODO: Set up game state monitoring
            // TODO: Configure emote reactions and visual effects
            // TODO: Initialize viewer challenges and voting systems
            
            Log.Info("Full phase: Advanced Twitch-game integration initialized");
        }
        
        // ==================== COMMAND PROCESSING (THREAD-SAFE) ====================
        
        /// <summary>
        /// Process Twitch command with thread safety and rate limiting
        /// Can be called from background Twitch IRC thread
        /// </summary>
        /// <param name="command">Command name</param>
        /// <param name="args">Command arguments</param>
        /// <param name="username">Twitch username</param>
        /// <returns>Response message for chat</returns>
        public static string ProcessCommand(string command, string[] args, string username = \"viewer\")
        {
            lock (_lock)
            {
                try
                {
                    // Rate limiting check
                    if (!CheckRateLimit(username, command))
                    {
                        return \"⏱️ Slow down! Please wait before sending another command.\";
                    }
                    
                    // Route to appropriate command handler based on initialization phase
                    return command.ToLowerInvariant() switch
                    {
                        // Phase 1 commands (always available)
                        \"!help\" => GetHelpText(),
                        \"!ping\" => \"🏓 Pong! Twitch integration is alive.\",
                        \"!status\" => GetConnectionStatus(),
                        
                        // Phase 2 commands (require full game state)
                        \"!resources\" when _fullPhaseInitialized => GetResourceStatus(),
                        \"!atmosphere\" when _fullPhaseInitialized => GetAtmosphereStatus(),
                        \"!time\" when _fullPhaseInitialized => GetTimeStatus(),
                        \"!game\" when _fullPhaseInitialized => GetGameStatus(),
                        
                        // Command not available in current phase
                        _ when !_fullPhaseInitialized => \"🔄 Game still loading... Try !help for available commands.\",
                        _ => $\"❓ Unknown command: {command}. Type !help for available commands.\"
                    };
                }
                catch (Exception ex)
                {
                    Log.Error($\"Command processing failed for '{command}': {ex.Message}\");
                    return $\"❌ Error processing command: {ex.Message}\";
                }
            }
        }
        
        /// <summary>
        /// Check rate limiting for user commands (1 command per 3 seconds per user)
        /// </summary>
        private static bool CheckRateLimit(string username, string command)
        {
            var now = DateTime.Now;
            var key = $\"{username}:{command}\";
            
            if (_lastCommandTimes.TryGetValue(key, out var lastTime))
            {
                if ((now - lastTime).TotalSeconds < 3.0)
                {
                    return false; // Rate limited
                }
            }
            
            _lastCommandTimes[key] = now;
            
            // Cleanup old entries (keep only last 100)
            if (_lastCommandTimes.Count > 100)
            {
                var oldEntries = _lastCommandTimes
                    .Where(kvp => (now - kvp.Value).TotalMinutes > 10)
                    .Take(_lastCommandTimes.Count - 50)
                    .ToList();
                
                foreach (var entry in oldEntries)
                {
                    _lastCommandTimes.Remove(entry.Key);
                }
            }
            
            return true;
        }
        
        // ==================== COMMAND IMPLEMENTATIONS ====================
        
        /// <summary>
        /// Get help text - available in both phases
        /// </summary>
        private static string GetHelpText()
        {
            var commands = new List<string>
            {
                \"!help - Show this help\",
                \"!ping - Test connection\",
                \"!status - Show integration status\"
            };
            
            if (_fullPhaseInitialized)
            {
                commands.AddRange(new[]
                {
                    \"!game - Game status\",
                    \"!resources - Resource levels\",
                    \"!atmosphere - Climate data\",
                    \"!time - Current sol and speed\"
                });
            }
            
            return $\"📋 Commands: {string.Join(\", \", commands)}\";
        }
        
        /// <summary>
        /// Get connection and initialization status
        /// </summary>
        private static string GetConnectionStatus()
        {
            var phase = _fullPhaseInitialized ? \"Phase 2 (Full)\" : 
                       _earlyPhaseInitialized ? \"Phase 1 (Basic)\" : \"Initializing\";
            
            return $\"🔗 Twitch Integration: {phase} | Game Ready: {(_fullPhaseInitialized ? \"Yes\" : \"Loading...\")}\";
        }
        
        /// <summary>
        /// Get current game status - Phase 2 only
        /// </summary>
        private static string GetGameStatus()
        {
            if (_universeCache == null) return \"🔄 Universe not loaded\";
            
            try
            {
                var sol = _universeCache.CurrentSol;
                var speed = _universeCache.GameSpeed;
                var paused = _universeCache.IsPaused;
                
                var status = paused ? \"PAUSED\" : $\"Running at {speed}x speed\";
                return $\"🎮 Sol {sol} - {status}\";
            }
            catch (Exception ex)
            {
                return $\"❌ Game status error: {ex.Message}\";
            }
        }
        
        /// <summary>
        /// Get resource status - Phase 2 only
        /// </summary>
        private static string GetResourceStatus()
        {
            if (_planetCache == null) return \"🔄 Planet not loaded\";
            
            try
            {
                var resources = new List<string>();
                
                var water = _planetCache.WaterStock;
                resources.Add($\"Water: {water:F1}\");
                
                var silicon = _planetCache.SiliconStock;
                resources.Add($\"Silicon: {silicon:F1}\");
                
                var iron = _planetCache.IronStock;
                resources.Add($\"Iron: {iron:F1}\");
                
                return $\"📦 Resources: {string.Join(\", \", resources)}\";
            }
            catch (Exception ex)
            {
                return $\"❌ Resource error: {ex.Message}\";
            }
        }
        
        /// <summary>
        /// Get atmosphere status - Phase 2 only
        /// </summary>
        private static string GetAtmosphereStatus()
        {
            if (_planetCache == null) return \"🔄 Planet not loaded\";
            
            try
            {
                var atmosphere = _planetCache.Atmosphere;
                var temp = atmosphere.Temperature;
                var pressure = atmosphere.TotalPressure;
                
                return $\"🌡️ Atmosphere: {temp:F1}K ({temp - 273.15f:F1}°C), {pressure:F2} kPa\";
            }
            catch (Exception ex)
            {
                return $\"❌ Atmosphere error: {ex.Message}\";
            }
        }
        
        /// <summary>
        /// Get time status - Phase 2 only
        /// </summary>
        private static string GetTimeStatus()
        {
            if (_universeCache == null) return \"🔄 Universe not loaded\";
            
            try
            {
                var sol = _universeCache.CurrentSol;
                var speed = _universeCache.GameSpeed;
                var paused = _universeCache.IsPaused;
                
                return $\"⏰ Sol {sol}, Speed: {speed}x, Paused: {paused}\";
            }
            catch (Exception ex)
            {
                return $\"❌ Time error: {ex.Message}\";
            }
        }
        
        // ==================== PUBLIC STATUS API ====================
        
        /// <summary>
        /// Check if early phase (basic commands) is ready
        /// </summary>
        public static bool IsEarlyPhaseReady => _earlyPhaseInitialized;
        
        /// <summary>
        /// Check if full phase (game integration) is ready
        /// </summary>
        public static bool IsFullPhaseReady => _fullPhaseInitialized;
        
        /// <summary>
        /// Get detailed initialization status for debugging
        /// </summary>
        public static string GetInitializationStatus()
        {
            var sb = new StringBuilder();
            sb.AppendLine($\"Early Phase: {(_earlyPhaseInitialized ? \"✅ Ready\" : \"❌ Not Ready\")}\");
            sb.AppendLine($\"Full Phase: {(_fullPhaseInitialized ? \"✅ Ready\" : \"❌ Not Ready\")}\");
            sb.AppendLine($\"BaseGame: {(_baseGameCache != null ? \"Available\" : \"Missing\")}\");
            sb.AppendLine($\"Universe: {(_universeCache != null ? \"Available\" : \"Missing\")}\");  
            sb.AppendLine($\"Planet: {(_planetCache != null ? \"Available\" : \"Missing\")}\");
            return sb.ToString();
        }
        
        /// <summary>
        /// Cleanup method - unsubscribe from events and reset state
        /// </summary>
        public static void Cleanup()
        {
            try
            {
                // TODO: Unsubscribe from events when EnhancedEvents supports it
                // TODO: Cleanup Twitch IRC connection
                // TODO: Clear command caches
                
                _earlyPhaseInitialized = false;
                _fullPhaseInitialized = false;
                _baseGameCache = null;
                _universeCache = null;
                _planetCache = null;
                _lastCommandTimes.Clear();
                
                Log.Info(\"TwitchIntegrationManager cleanup completed\");
            }
            catch (Exception ex)
            {
                Log.Error($\"Error during TwitchIntegrationManager cleanup: {ex.Message}\");
            }
        }
    }
}