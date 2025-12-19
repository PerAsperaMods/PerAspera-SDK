using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using PerAspera.GameAPI;
using PerAspera.GameAPI.Wrappers;
using PerAspera.GameAPI.Events.SDK;
using PerAspera.GameAPI.Events.Constants;
using PerAspera.Core.IL2CPP;
using static PerAspera.GameAPI.Events.EventsAutoStartPlugin;

namespace PerAspera.SDK.TwitchIntegration
{
    /// <summary>
    /// Processes Twitch chat commands and generates game state responses
    /// Uses SDK wrappers for safe game data access after BaseGame initialization
    /// </summary>
    public class TwitchCommandProcessor
    {
        private static readonly LogAspera Log = LogAspera.Create("TwitchCommandProcessor");
        
        // Cached instances (populated after BaseGame loads)
        private static Universe? _universeCache;
        private static Planet? _planetCache;
        private static BaseGame? _baseGameCache;
        private static bool _isInitialized = false;
        
        /// <summary>
        /// Initialize the processor once BaseGame is loaded
        /// Called by event subscription in static constructor
        /// </summary>
        static TwitchCommandProcessor()
        {
            // Subscribe to real SDK events using EnhancedEvents
            EnhancedEvents.Subscribe<GameFullyLoadedEvent>(SDKEventConstants.GameFullyLoaded, OnGameFullyLoaded);
            EnhancedEvents.Subscribe<BaseGameDetectedEvent>(SDKEventConstants.BaseGameDetected, OnBaseGameDetected);
            
            Log.Info("TwitchCommandProcessor initialized - waiting for BaseGame load");
        }
        
        /// <summary>
        /// Handle BaseGame detection event
        /// </summary>
        private static void OnBaseGameDetected(BaseGameDetectedEvent eventArgs)
        {
            _baseGameCache = eventArgs.BaseGame;
            _universeCache = eventArgs.Universe;
            
            Log.Info("BaseGame and Universe detected");
            RefreshGameInstances();
        }
        
        /// <summary>
        /// Handle game fully loaded event
        /// </summary>
        private static void OnGameFullyLoaded(GameFullyLoadedEvent eventArgs)
        {
            _baseGameCache = eventArgs.BaseGameWrapper;
            _universeCache = eventArgs.UniverseWrapper;
            _planetCache = eventArgs.PlanetWrapper;
            
            Log.Info("Game fully loaded - all systems available");
            RefreshGameInstances();
        }
        
        /// <summary>
        /// Refresh cached game instances from current game state
        /// </summary>
        private static void RefreshGameInstances()
        {
            try
            {
                _baseGameCache = BaseGame.GetCurrent();
                _universeCache = Universe.GetCurrent();
                _planetCache = Planet.GetCurrent();
                
                _isInitialized = (_baseGameCache != null && _universeCache != null && _planetCache != null);
                
                if (_isInitialized)
                {
                    Log.Info("TwitchCommandProcessor fully initialized - game instances cached");
                }
                else
                {
                    Log.Warning("TwitchCommandProcessor partial initialization - some instances missing");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to refresh game instances: {ex.Message}");
                _isInitialized = false;
            }
        }
        
        /// <summary>
        /// Process a Twitch command and return response
        /// </summary>
        /// <param name="command">Command name (e.g., "!status", "!resources")</param>
        /// <param name="args">Command arguments</param>
        /// <returns>Response message for chat</returns>
        public static string ProcessCommand(string command, string[] args)
        {
            if (!_isInitialized)
            {
                return "Game not ready - please wait for initialization";
            }
            
            try
            {
                return command.ToLowerInvariant() switch
                {
                    "!status" => GetGameStatus(),
                    "!resources" => GetResourceStatus(), 
                    "!atmosphere" => GetAtmosphereStatus(),
                    "!time" => GetTimeStatus(),
                    "!help" => GetHelpText(),
                    _ => $"Unknown command: {command}. Type !help for available commands."
                };
            }
            catch (Exception ex)
            {
                Log.Error($"Command processing failed for '{command}': {ex.Message}");
                return $"Error processing command: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Get general game status using verified wrapper properties
        /// </summary>
        private static string GetGameStatus()
        {
            if (_universeCache == null) return "Universe not available";
            
            var sol = _universeCache.CurrentSol;
            var speed = _universeCache.GameSpeed;
            var paused = _universeCache.IsPaused;
            
            var status = paused ? "PAUSED" : $"Running at {speed}x speed";
            
            return $"Sol {sol} - {status}";
        }
        
        /// <summary>
        /// Get resource status using verified Planet wrapper properties
        /// </summary>
        private static string GetResourceStatus()
        {
            if (_planetCache == null) return "Planet not available";
            
            var resources = new List<string>();
            
            // Use verified Planet wrapper properties
            try
            {
                var water = _planetCache.WaterStock;
                resources.Add($"Water: {water:F1}");
            }
            catch { resources.Add("Water: N/A"); }
            
            try
            {
                var silicon = _planetCache.SiliconStock;
                resources.Add($"Silicon: {silicon:F1}");
            }
            catch { resources.Add("Silicon: N/A"); }
            
            try
            {
                var iron = _planetCache.IronStock;
                resources.Add($"Iron: {iron:F1}");
            }
            catch { resources.Add("Iron: N/A"); }
            
            return $"Resources: {string.Join(", ", resources)}";
        }
        
        /// <summary>
        /// Get atmosphere status using verified Atmosphere wrapper
        /// </summary>
        private static string GetAtmosphereStatus()
        {
            if (_planetCache == null) return "Planet not available";
            
            try
            {
                var atmosphere = _planetCache.Atmosphere;
                
                // Get basic atmosphere data
                var temp = atmosphere.Temperature;
                var pressure = atmosphere.TotalPressure;
                
                return $"Atmosphere: {temp:F1}K, {pressure:F2} kPa";
            }
            catch (Exception ex)
            {
                return $"Atmosphere data unavailable: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Get time-related status using verified Universe wrapper
        /// </summary>
        private static string GetTimeStatus()
        {
            if (_universeCache == null) return "Universe not available";
            
            var sol = _universeCache.CurrentSol;
            var speed = _universeCache.GameSpeed;
            var paused = _universeCache.IsPaused;
            
            return $"Sol {sol}, Speed: {speed}x, Paused: {paused}";
        }
        
        /// <summary>
        /// Get help text with available commands
        /// </summary>
        private static string GetHelpText()
        {
            return "Commands: !status (game state), !resources (resource levels), !atmosphere (climate), !time (sol/speed), !help";
        }
        
        /// <summary>
        /// Check if processor is ready for commands
        /// </summary>
        public static bool IsReady => _isInitialized;
        
        /// <summary>
        /// Get initialization status for debugging
        /// </summary>
        public static string GetInitializationStatus()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Initialized: {_isInitialized}");
            sb.AppendLine($"BaseGame: {(_baseGameCache != null ? "Available" : "Missing")}");
            sb.AppendLine($"Universe: {(_universeCache != null ? "Available" : "Missing")}");  
            sb.AppendLine($"Planet: {(_planetCache != null ? "Available" : "Missing")}");
            return sb.ToString();
        }
    }
}