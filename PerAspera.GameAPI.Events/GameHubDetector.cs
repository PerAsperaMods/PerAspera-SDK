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
        private const int MaxFrames = 3000; // ~50 seconds at 60fps
        
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
            if (_logger == null || _eventEmitted) return;

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
                    _logger.LogInfo($"🎯 BaseGame detected at frame {_frameCount}! Publishing GameHubInitializedEvent...");
                    
                    // Emit the event
                    var evt = new GameHubInitializedEvent(baseGame.GetNativeObject(), isReady: true);
                    EnhancedEventBus.Publish(SDKEventConstants.GameHubInitialized, evt);
                    
                    _logger.LogInfo("📡 GameHubInitializedEvent published successfully!");
                    _eventEmitted = true;
                    
                    // Self-destruct after successful emission
                    SelfDestruct();
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

        private void SelfDestruct()
        {
            if (_logger != null)
            {
                _logger.LogInfo("💥 GameHubDetector mission complete - self-destructing");
            }
            GameObject.Destroy(this.gameObject);
        }
    }
}