using UnityEngine;
using UnityEngine.SceneManagement;
using BepInEx.Logging;
using Il2CppInterop.Runtime.Injection;
using System;

namespace PerAspera.SDK.Utils
{
    /// <summary>
    /// Utility class for spawning MonoBehaviour components at the proper time in IL2CPP.
    /// Handles Unity scene loading timing issues and ensures MonoBehaviour creation 
    /// happens when the Unity scene system is ready.
    /// </summary>
    public static class DelayedMonoBehaviourSpawner
    {
        /// <summary>
        /// Spawn a MonoBehaviour component with proper timing for IL2CPP.
        /// This method handles Unity scene timing issues and retries if needed.
        /// </summary>
        /// <typeparam name="T">MonoBehaviour type to spawn</typeparam>
        /// <param name="gameObjectName">Name for the GameObject hosting the component</param>
        /// <param name="logger">Logger for status reporting</param>
        /// <param name="persistent">If true, GameObject survives scene changes</param>
        /// <returns>The spawned component, or null if spawn failed</returns>
        public static T SpawnWhenReady<T>(string gameObjectName, ManualLogSource logger, bool persistent = true) 
            where T : MonoBehaviour
        {
            try
            {
                // Step 1: Register type in Il2Cpp if not already registered
                try
                {
                    ClassInjector.RegisterTypeInIl2Cpp<T>();
                    logger?.LogDebug($"✅ Type {typeof(T).Name} registered in Il2Cpp");
                }
                catch (Exception ex) when (ex.Message.Contains("already registered"))
                {
                    // Type already registered - this is fine
                    logger?.LogDebug($"ℹ️ Type {typeof(T).Name} already registered");
                }
                
                // Step 2: Create GameObject
                var gameObject = new GameObject(gameObjectName);
                logger?.LogDebug($"✅ GameObject created: {gameObject.name}");
                
                // Step 3: Make persistent if requested
                if (persistent)
                {
                    GameObject.DontDestroyOnLoad(gameObject);
                    logger?.LogDebug("✅ GameObject set as persistent");
                }
                
                // Step 4: Add component
                var component = gameObject.AddComponent<T>();
                logger?.LogInfo($"✅ {typeof(T).Name} component spawned successfully");
                
                return component;
            }
            catch (Exception ex)
            {
                logger?.LogError($"❌ Failed to spawn {typeof(T).Name}: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Spawn a MonoBehaviour with automatic retry on scene loading.
        /// This method subscribes to scene loading events and retries spawn
        /// until successful or max attempts reached.
        /// </summary>
        /// <typeparam name="T">MonoBehaviour type to spawn</typeparam>
        /// <param name="gameObjectName">Name for the GameObject hosting the component</param>
        /// <param name="logger">Logger for status reporting</param>
        /// <param name="onSuccess">Callback when spawn succeeds</param>
        /// <param name="maxAttempts">Maximum spawn attempts (default: 5)</param>
        /// <param name="persistent">If true, GameObject survives scene changes</param>
        public static void SpawnWithRetry<T>(
            string gameObjectName, 
            ManualLogSource logger,
            Action<T> onSuccess = null,
            int maxAttempts = 5,
            bool persistent = true) 
            where T : MonoBehaviour
        {
            var spawner = new MonoBehaviourSpawnHelper<T>(
                gameObjectName, 
                logger, 
                onSuccess, 
                maxAttempts, 
                persistent
            );
            
            spawner.Start();
        }
        
        /// <summary>
        /// Helper class for managing MonoBehaviour spawn retries.
        /// Automatically unsubscribes from events when spawn succeeds or max attempts reached.
        /// </summary>
        private class MonoBehaviourSpawnHelper<T> where T : MonoBehaviour
        {
            private readonly string _gameObjectName;
            private readonly ManualLogSource _logger;
            private readonly Action<T> _onSuccess;
            private readonly int _maxAttempts;
            private readonly bool _persistent;
            private int _attemptCount = 0;
            private bool _spawned = false;
            
            public MonoBehaviourSpawnHelper(
                string gameObjectName, 
                ManualLogSource logger,
                Action<T> onSuccess,
                int maxAttempts,
                bool persistent)
            {
                _gameObjectName = gameObjectName;
                _logger = logger;
                _onSuccess = onSuccess;
                _maxAttempts = maxAttempts;
                _persistent = persistent;
            }
            
            public void Start()
            {
                // Try immediate spawn first
                TrySpawn("immediate");
                
                // Subscribe to scene events for retries
                if (!_spawned)
                {
                    SceneManager.sceneLoaded += OnSceneLoaded;
                    _logger?.LogInfo($"⏳ Subscribed to scene events for {typeof(T).Name} spawn retries");
                }
            }
            
            private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
            {
                if (_spawned) return;
                
                TrySpawn($"scene:{scene.name}");
            }
            
            private void TrySpawn(string trigger)
            {
                if (_spawned || _attemptCount >= _maxAttempts) return;
                
                _attemptCount++;
                _logger?.LogDebug($"🔄 Spawn attempt {_attemptCount}/{_maxAttempts} for {typeof(T).Name} (trigger: {trigger})");
                
                var component = SpawnWhenReady<T>(_gameObjectName, _logger, _persistent);
                
                if (component != null)
                {
                    _spawned = true;
                    SceneManager.sceneLoaded -= OnSceneLoaded;
                    
                    _logger?.LogInfo($"✅ {typeof(T).Name} spawned successfully on attempt {_attemptCount} (trigger: {trigger})");
                    _onSuccess?.Invoke(component);
                }
                else if (_attemptCount >= _maxAttempts)
                {
                    _logger?.LogError($"❌ Failed to spawn {typeof(T).Name} after {_maxAttempts} attempts");
                    SceneManager.sceneLoaded -= OnSceneLoaded;
                }
            }
        }
    }
}