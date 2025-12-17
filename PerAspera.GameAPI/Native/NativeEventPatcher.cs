using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Native.Events;
// using PerAspera.GameAPI.Events; // TODO: Restore after Events project compiles

namespace PerAspera.GameAPI.Native
{
    /// <summary>
    /// Refactored native event patching system using modular services
    /// Provides comprehensive hooks for native game events through specialized event patching services
    /// </summary>
    public static class NativeEventPatcher
    {
        private static readonly LogAspera _log = new LogAspera("GameAPI.NativeEventPatcher");
        private static bool _isInitialized = false;
        private static Harmony? _harmony;

        // Modular event patching services
        private static ClimateEventPatchingService? _climateService;
        private static TimeEventPatchingService? _timeService;
        private static ResourceEventPatchingService? _resourceService;
        private static GameStateEventPatchingService? _gameStateService;
        private static BuildingEventPatchingService? _buildingService;

        /// <summary>
        /// Initialize the modular native event patching system
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized)
                return;

            try
            {
                _log.Info("🔧 Initializing modular native event patching system...");

                // Initialize Harmony with unique ID
                _harmony = new Harmony("PerAspera.GameAPI.NativeEvents.v3");

                // Initialize specialized event patching services
                _climateService = new ClimateEventPatchingService(_harmony);
                _timeService = new TimeEventPatchingService(_harmony);
                _resourceService = new ResourceEventPatchingService(_harmony);
                _gameStateService = new GameStateEventPatchingService(_harmony);
                _buildingService = new BuildingEventPatchingService(_harmony);

                // Setup event hooks through services
                var totalHooks = 0;
                totalHooks += _climateService.InitializeEventHooks();
                totalHooks += _timeService.InitializeEventHooks();
                totalHooks += _resourceService.InitializeEventHooks();
                totalHooks += _gameStateService.InitializeEventHooks();
                totalHooks += _buildingService.InitializeEventHooks();

                _isInitialized = true;
                _log.Info($"✅ Modular native event patching system initialized with {totalHooks} hooks");

                // Publish initialization event
                ModEventBus.Publish("NativeEventPatcherInitialized", new { 
                    HookCount = totalHooks,
                    Services = new[] { "Climate", "Time", "Resource", "GameState", "Building" },
                    Timestamp = DateTime.Now 
                });
            }
            catch (Exception ex)
            {
                _log.Error($"❌ Failed to initialize modular native event patcher: {ex.Message}");
            }
        }

        /// <summary>
        /// Get comprehensive diagnostic information from all event patching services
        /// </summary>
        /// <returns>Detailed diagnostic information</returns>
        public static string GetDiagnosticInfo()
        {
            if (!_isInitialized)
                return "Event patcher not initialized";

            var diagnostics = new System.Text.StringBuilder();
            diagnostics.AppendLine("=== Native Event Patcher Diagnostic Information ===");
            diagnostics.AppendLine($"Status: {(_isInitialized ? "✅ Initialized" : "❌ Not Initialized")}");
            diagnostics.AppendLine($"Harmony ID: {_harmony?.Id ?? "None"}");
            diagnostics.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            diagnostics.AppendLine();

            // Climate service diagnostics
            if (_climateService != null)
            {
                diagnostics.AppendLine(_climateService.GetDiagnosticInfo());
                diagnostics.AppendLine();
            }

            // Time service diagnostics
            if (_timeService != null)
            {
                diagnostics.AppendLine(_timeService.GetDiagnosticInfo());
                diagnostics.AppendLine();
            }

            // Resource service diagnostics
            if (_resourceService != null)
            {
                diagnostics.AppendLine(_resourceService.GetDiagnosticInfo());
                diagnostics.AppendLine();
            }

            // Game state service diagnostics
            if (_gameStateService != null)
            {
                diagnostics.AppendLine(_gameStateService.GetDiagnosticInfo());
                diagnostics.AppendLine();
            }

            // Building service diagnostics
            if (_buildingService != null)
            {
                diagnostics.AppendLine(_buildingService.GetDiagnosticInfo());
                diagnostics.AppendLine();
            }

            return diagnostics.ToString();
        }

        /// <summary>
        /// Get current Martian sol safely with fallbacks
        /// </summary>
        public static int GetCurrentMartianSol()
        {
            // TODO: Use Wrappers.Universe.GetCurrent() when available
            return 1; // Fallback
            /*
            try
            {
                // Try multiple sources
                return MirrorUniverse.GetCurrentMartianSol();
            }
            catch
            {
                try
                {
                    var universeInstance = MirrorUniverse.Shared?.Instance;
                    return universeInstance?.InvokeMethod<int>("GetCurrentSol") ?? 1;
                }
                catch
                {
                    return 1;
                }
            }
            */
        }

        /// <summary>
        /// Enhanced shutdown with proper cleanup of all services
        /// </summary>
        public static void Shutdown()
        {
            if (!_isInitialized)
                return;

            try
            {
                _log.Info("🛑 Shutting down modular native event patcher...");

                // Shutdown all services
                _climateService?.Dispose();
                _timeService?.Dispose();
                _resourceService?.Dispose();
                _gameStateService?.Dispose();
                _buildingService?.Dispose();

                // Unpatch all our patches
                _harmony?.UnpatchSelf();

                // Clear service references
                _climateService = null;
                _timeService = null;
                _resourceService = null;
                _gameStateService = null;
                _buildingService = null;

                _isInitialized = false;
                _log.Info("✅ Modular native event patcher shut down successfully");
            }
            catch (Exception ex)
            {
                _log.Error($"Error shutting down modular native event patcher: {ex.Message}");
            }
        }

        /// <summary>
        /// Enhanced statistics from all services
        /// </summary>
        public static Dictionary<string, object> GetStats()
        {
            var stats = new Dictionary<string, object>
            {
                ["IsInitialized"] = _isInitialized,
                ["HarmonyId"] = _harmony?.Id ?? "None",
                ["Timestamp"] = DateTime.Now
            };

            if (_isInitialized)
            {
                var serviceStats = new Dictionary<string, object>();
                
                if (_climateService != null)
                    serviceStats["Climate"] = new { EventType = _climateService.GetEventType(), Initialized = true };
                
                if (_timeService != null)
                    serviceStats["Time"] = new { EventType = _timeService.GetEventType(), Initialized = true };
                
                if (_resourceService != null)
                    serviceStats["Resource"] = new { EventType = _resourceService.GetEventType(), Initialized = true };
                
                if (_gameStateService != null)
                    serviceStats["GameState"] = new { EventType = _gameStateService.GetEventType(), Initialized = true };
                
                if (_buildingService != null)
                    serviceStats["Building"] = new { EventType = _buildingService.GetEventType(), Initialized = true };

                stats["Services"] = serviceStats;
            }

            return stats;
        }

        // Legacy compatibility methods
        // TODO: Remove these when GameAPI methods are updated

        public static void OnDayPassedPatch()
        {
            // TODO: Implement when GameAPI methods are available
            // GameAPI.TriggerDayPassed();
        }

        public static void OnResourceAddedPatch(string resource, float amount)
        {
            // TODO: Implement when GameAPI methods are available  
            // GameAPI.TriggerResourceAdded(resource, amount);
        }
    }
}