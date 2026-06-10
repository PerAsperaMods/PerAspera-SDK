using System;
using PerAspera.GameAPI.Events.Core;
using PerAspera.GameAPI.Events.Native;
using PerAspera.GameAPI.Events.Constants;
using PerAspera.GameAPI.Events.SDK;
using PerAspera.GameAPI;
using PerAspera.Core;
using HarmonyLib;

namespace PerAspera.GameAPI.Events.Patches
{
    /// <summary>
    /// SDK-based game initialization detection (NO MORE HARMONY PATCHES)
    /// Uses existing SDK wrapper system instead of problematic IL2CPP patches
    /// </summary>
    public static class GameInitializationPatches // REMOVED [HarmonyPatch] - no patches needed!
    {
        private static readonly LogAspera _logger = new LogAspera("GameInitPatches");
        private static bool _gameHubInitialized = false;
        private static bool _gameFullyLoaded = false;

        /// <summary>
        /// Initialize game events using SDK wrapper detection instead of IL2CPP patches
        /// Called from EventsAutoStartPlugin after SDK wrappers are ready
        /// </summary>
        public static void InitializeSDKBasedEvents()
        {
            try
            {
                _logger.Info("🔧 Initializing SDK-based game initialization detection...");
                
                // Use existing SDK wrapper system to detect game state
                var baseGame = TryGetBaseGame();
                if (baseGame != null)
                {
                    TriggerGameHubInitialized(baseGame);
                    _logger.Info("✅ Game initialization detected via SDK wrappers");
                }
                else
                {
                    // Fallback: schedule periodic check using SDK
                    ScheduleSDKBasedCheck();
                    _logger.Info("⏰ Scheduled SDK-based game state monitoring");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"❌ Failed to initialize SDK-based events: {ex.Message}");
            }
        }

        /// <summary>
        /// Try to get BaseGame using existing SDK wrapper system
        /// </summary>
        private static BaseGame? TryGetBaseGame()
        {
            try
            {
                // Get BaseGame using GameTypeInitializer.GetBaseGameInstance() as BaseGame
                var baseGameInstance = GameTypeInitializer.GetBaseGameInstance() as BaseGame;
                if (baseGameInstance != null)
                {
                    return baseGameInstance;
                }
            }
            catch (Exception ex)
            {
                _logger.Debug($"BaseGame not yet available via SDK: {ex.Message}");
            }
            
            return null;
        }

        /// <summary>
        /// Trigger GameHubInitialized event using SDK wrapper
        /// </summary>
        private static void TriggerGameHubInitialized(BaseGame? baseGame)
        {
            if (_gameHubInitialized) return;

            try
            {
                _gameHubInitialized = true;
                _logger.Info("🎮 Game initialization detected via SDK wrapper");

                var evt = new GameHubInitializedEvent((object?)baseGame, isReady: true);
                EnhancedEventBus.Publish(SDKEventConstants.GameHubInitialized, evt);
                
                _logger.Info("📡 GameHubInitializedEvent published successfully");
            }
            catch (Exception ex)
            {
                _logger.Error($"❌ Failed to trigger GameHubInitialized: {ex.Message}");
            }
        }

        /// <summary>
        /// Schedule periodic SDK-based check for game initialization
        /// </summary>
        private static void ScheduleSDKBasedCheck()
        {
            // Deprecated: Now handled by dedicated GameHubDetectorPlugin
            _logger.Info("⏳ BaseGame monitoring delegated to GameHubDetectorPlugin");
            _logger.Info("💡 GameHubDetectorPlugin will emit GameHubInitializedEvent when ready");
        }

    }
}