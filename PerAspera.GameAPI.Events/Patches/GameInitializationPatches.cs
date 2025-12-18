using System;
using PerAspera.GameAPI.Events.Core;
using PerAspera.GameAPI.Events.Native;
using PerAspera.GameAPI.Events.Constants;
using PerAspera.GameAPI.Events.SDK;
using PerAspera.GameAPI.Wrappers;
using PerAspera.Core;
using HarmonyLib;

namespace PerAspera.GameAPI.Events.Patches
{
    /// <summary>
    /// Harmony patches for game initialization events
    /// Triggers Enhanced Events when specific game systems initialize
    /// </summary>
    [HarmonyPatch]
    public static class GameInitializationPatches
    {
        private static readonly LogAspera _logger = new LogAspera("GameInitPatches");
        private static bool _gameHubInitialized = false;
        private static bool _gameFullyLoaded = false;

        /// <summary>
        /// Patch GameHubManager.Awake() to trigger GameHubInitializedEvent
        /// Based on log pattern: "GameHubManager:Awake() (at :0)"
        /// </summary>
        [HarmonyPatch("GameHubManager", "Awake")]
        [HarmonyPostfix]
        public static void OnGameHubManagerAwake()
        {
            try
            {
                if (_gameHubInitialized) return;
                _gameHubInitialized = true;

                _logger.Info("🎮 GameHubManager.Awake() detected - triggering GameHubInitializedEvent");

                // Try to get BaseGame instance early
                var baseGameInstance = TryGetBaseGameInstance();
                if (baseGameInstance != null)
                {
                    // Create enhanced event with wrapper
                    var evt = new GameHubInitializedEvent(baseGameInstance, isReady: true);
                    
                    // Publish through Enhanced Event Bus
                    EnhancedEventBus.Publish(SDKEventConstants.GameHubInitialized, evt);
                    
                    _logger.Info("✅ GameHubInitializedEvent published successfully");
                }
                else
                {
                    _logger.Warning("⚠️ BaseGame instance not available during GameHub initialization");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"❌ Error in GameHubManager.Awake patch: {ex.Message}");
            }
        }

        /// <summary>
        /// Patch BaseGame.Awake() or similar to ensure we have BaseGame access
        /// </summary>
        [HarmonyPatch("BaseGame", "Awake")]
        [HarmonyPostfix]
        public static void OnBaseGameAwake()
        {
            try
            {
                _logger.Info("🎮 BaseGame.Awake() detected");
                
                // If GameHub wasn't initialized yet but BaseGame is ready, trigger now
                if (!_gameHubInitialized)
                {
                    var baseGameInstance = TryGetBaseGameInstance();
                    if (baseGameInstance != null)
                    {
                        _gameHubInitialized = true;
                        
                        var evt = new GameHubInitializedEvent(baseGameInstance, isReady: true);
                        EnhancedEventBus.Publish(SDKEventConstants.GameHubInitialized, evt);
                        
                        _logger.Info("✅ GameHubInitializedEvent published from BaseGame.Awake");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"❌ Error in BaseGame.Awake patch: {ex.Message}");
            }
        }

        /// <summary>
        /// Patch Planet.Awake() or scene load completion to trigger GameFullyLoadedEvent
        /// </summary>
        [HarmonyPatch("Planet", "Awake")]
        [HarmonyPostfix]
        public static void OnPlanetAwake()
        {
            try
            {
                if (_gameFullyLoaded) return;

                _logger.Info("🎮 Planet.Awake() detected - checking for complete system");

                // Try to get all main game instances
                var baseGame = TryGetBaseGameInstance();
                var universe = TryGetUniverseInstance();
                var planet = TryGetPlanetInstance();

                if (baseGame != null && universe != null && planet != null)
                {
                    _gameFullyLoaded = true;
                    
                    var evt = new GameFullyLoadedEvent(baseGame, universe, planet);
                    EnhancedEventBus.Publish(SDKEventConstants.GameFullyLoaded, evt);
                    
                    _logger.Info("✅ GameFullyLoadedEvent published successfully");
                }
                else
                {
                    _logger.Info($"⏳ System not fully loaded yet - BaseGame:{baseGame != null}, Universe:{universe != null}, Planet:{planet != null}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"❌ Error in Planet.Awake patch: {ex.Message}");
            }
        }

        /// <summary>
        /// Try to get BaseGame instance using wrapper
        /// </summary>
        private static BaseGame? TryGetBaseGameInstance()
        {
            try
            {
                // Try different methods to get BaseGame instance
                
                // Method 1: BaseGame.Current (if available)
                var baseGameType = System.Type.GetType("BaseGame");
                if (baseGameType != null)
                {
                    var currentProperty = baseGameType.GetProperty("Current");
                    if (currentProperty != null)
                    {
                        var nativeInstance = currentProperty.GetValue(null);
                        if (nativeInstance != null)
                        {
                            return WrapperFactory.ConvertToWrapper<BaseGame>(nativeInstance);
                        }
                    }
                }

                // Method 2: Try finding BaseGame instance in scene
                var baseGameInstances = UnityEngine.Object.FindObjectsOfType<UnityEngine.MonoBehaviour>();
                foreach (var instance in baseGameInstances)
                {
                    if (instance.GetType().Name == "BaseGame")
                    {
                        return WrapperFactory.ConvertToWrapper<BaseGame>(instance);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to get BaseGame instance: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Try to get Universe instance using wrapper
        /// </summary>
        private static Universe? TryGetUniverseInstance()
        {
            try
            {
                // Similar pattern to BaseGame
                var universeType = System.Type.GetType("Universe");
                if (universeType != null)
                {
                    var currentProperty = universeType.GetProperty("Current");
                    if (currentProperty != null)
                    {
                        var nativeInstance = currentProperty.GetValue(null);
                        if (nativeInstance != null)
                        {
                            return WrapperFactory.ConvertToWrapper<Universe>(nativeInstance);
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to get Universe instance: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Try to get Planet instance using wrapper
        /// </summary>
        private static Planet? TryGetPlanetInstance()
        {
            try
            {
                // Similar pattern to BaseGame/Universe
                var planetType = System.Type.GetType("Planet");
                if (planetType != null)
                {
                    var currentProperty = planetType.GetProperty("Current");
                    if (currentProperty != null)
                    {
                        var nativeInstance = currentProperty.GetValue(null);
                        if (nativeInstance != null)
                        {
                            return WrapperFactory.ConvertToWrapper<Planet>(nativeInstance);
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to get Planet instance: {ex.Message}");
                return null;
            }
        }
    }
}