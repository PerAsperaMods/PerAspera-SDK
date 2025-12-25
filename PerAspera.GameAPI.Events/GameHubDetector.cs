using System;
using UnityEngine;
using Il2CppInterop.Runtime.Injection;
using PerAspera.GameAPI.Events.Core;
using PerAspera.GameAPI.Events.SDK;
using PerAspera.GameAPI.Events.Constants;
using PerAspera.GameAPI.Wrappers;
using PerAspera.Core;
using PerAspera.GameAPI.Events.Integration;
using BepInEx.Logging;

namespace PerAspera.GameAPI.Events.Detector
{
    /// <summary>
    /// MonoBehaviour that polls BaseGameWrapper.GetCurrent() until available
    /// Integrated into EventsAutoStartPlugin to avoid multiple plugins per DLL
    /// </summary>
    public class GameHubDetector : MonoBehaviour
    {
        public static ManualLogSource _logger;
        private int _frameCount = 0;
        private bool _eventEmitted = false;
        private bool _fullLoadMonitoringActive = false;
        private int _fullLoadCheckCount = 0;
        private const int MaxFrames = 3000; // ~50 seconds at 60fps
        private const int MaxFullLoadChecks = 600; // ~10 seconds at 60fps
        private BaseGameWrapper? _baseGameForFullLoad;
        
        /// <summary>
        /// Initialize the detector with a shared logger
        /// </summary>
        public static void Initialize(ManualLogSource logger)
        {
            _logger = logger;
        }

        private void Start()
        {
            if (_logger == null)
            {
                UnityEngine.Debug.LogError("[GameHubDetector] Logger not initialized! Component will not function.");
                Destroy(this.gameObject);
                return;
            }
            _logger.LogInfo("🔍 GameHubDetector.Start() called - monitoring BaseGameWrapper.GetCurrent()");
            _logger.LogInfo($"🎮 GameObject: {gameObject.name}, Active: {gameObject.activeInHierarchy}");
        }

        private void Update()
        {
            if (_logger == null) return;

            _frameCount++;
            
            // Log early frames to confirm Update is running
            if (_frameCount <= 5)
            {
                _logger.LogInfo($"🔄 GameHubDetector.Update() frame {_frameCount} - monitoring...");
            }

            // Check every 30 frames (~0.5 seconds at 60fps)
            if (_frameCount % 30 != 0) return;

            // Timeout after MaxFrames
            if (_frameCount > MaxFrames)
            {
                _logger.LogWarning("⚠️ GameHubDetector timed out after 50 seconds");
                SelfDestruct();
                return;
            }

            // If we haven't emitted the initial event yet, check for BaseGame
            if (!_eventEmitted)
            {
                // Log progress every 10 seconds
                if (_frameCount % 600 == 0)
                {
                    _logger.LogInfo($"⏳ Still monitoring BaseGame... (frame {_frameCount})");
                }

                try
                {
                    var baseGame = BaseGameWrapper.GetCurrent();
                    if (baseGame != null)
                    {
                        _logger.LogInfo($"🎯 BaseGame detected at frame {_frameCount}! Publishing events...");

                        // Emit EarlyModsReadyEvent first (for early mod initialization)
                        var earlyEvent = new EarlyModsReadyEvent(baseGame.GetNativeObject());
                        EnhancedEventBus.Publish(SDKEventConstants.EarlyModsReady, earlyEvent);
                        _logger.LogInfo("📡 EarlyModsReadyEvent published successfully!");

                        // Emit GameHubInitializedEvent for compatibility
                        var hubEvent = new GameHubInitializedEvent(baseGame.GetNativeObject(), isReady: true);
                        EnhancedEventBus.Publish(SDKEventConstants.GameHubInitialized, hubEvent);
                        _logger.LogInfo("📡 GameHubInitializedEvent published successfully!");

                        // Start monitoring for full game load (Universe + Planet)
                        StartFullGameLoadMonitoring(baseGame);

                        _eventEmitted = true;
                    }
                }
                catch (Exception ex)
                {
                    // Silent failure - BaseGame not ready yet
                    if (_frameCount % 600 == 0) // Log only every 10 seconds
                    {
                        _logger.LogDebug($"BaseGame not ready: {ex.Message}");
                    }
                }
            }
            // If initial event emitted but full load monitoring is active, check for full load
            else if (_fullLoadMonitoringActive)
            {
                _fullLoadCheckCount++;

                // Timeout for full load monitoring
                if (_fullLoadCheckCount > MaxFullLoadChecks)
                {
                    _logger.LogWarning("⚠️ Full game load monitoring timed out - Universe/Planet not detected");
                    SelfDestruct();
                    return;
                }

                try
                {
                    // Check if Universe and Planet are available
                    var universe = UniverseWrapper.GetCurrent();
                    var planet = PlanetWrapper.GetCurrent();

                    if (universe != null && planet != null && _baseGameForFullLoad != null)
                    {
                        _logger.LogInfo("🎯 Full game load detected! Publishing GameFullyLoadedEvent...");

                        // Emit GameFullyLoadedEvent
                        var fullLoadEvent = new GameFullyLoadedEvent(
                            _baseGameForFullLoad.GetNativeObject(),
                            universe.GetNativeObject(),
                            planet.GetNativeObject()
                        );
                        EnhancedEventBus.Publish(SDKEventConstants.GameFullyLoaded, fullLoadEvent);

                        _logger.LogInfo("📡 GameFullyLoadedEvent published successfully!");
                        SelfDestruct();
                        return;
                    }
                    else
                    {
                        if (_fullLoadCheckCount % 60 == 0) // Log every second
                        {
                            _logger.LogInfo($"⏳ Waiting for full load - Universe: {universe != null}, Planet: {planet != null}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (_fullLoadCheckCount % 60 == 0) // Log every second
                    {
                        _logger.LogDebug($"Full load check failed: {ex.Message}");
                    }
                }
            }
        }

        private void SelfDestruct()
        {
            if (_logger != null)
            {
                _logger.LogInfo("💥 GameHubDetector mission complete - self-destructing");
            }
            GameObject.Destroy(this.gameObject);
        }

        /// <summary>
        /// Start monitoring for full game load (Universe + Planet) to emit GameFullyLoadedEvent
        /// </summary>
        private void StartFullGameLoadMonitoring(BaseGameWrapper baseGame)
        {
            _logger.LogInfo("🔍 Starting full game load monitoring...");
            _baseGameForFullLoad = baseGame;
            _fullLoadMonitoringActive = true;
            _fullLoadCheckCount = 0;
        }
    }
}