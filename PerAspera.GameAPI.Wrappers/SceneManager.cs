using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using PerAspera.Core;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Type-safe wrapper for Unity SceneManager static class
    /// Provides safe access to scene management operations
    /// DOC: SceneManager wrapper for Unity SceneManagement system
    /// </summary>
    public static class SceneManager
    {
        private static readonly LogAspera Log = new LogAspera("SceneManager");
        
        // ==================== SCENE QUERIES ====================
        
        /// <summary>
        /// Get currently active scene
        /// Static Method: SceneManager.GetActiveScene() -> Scene
        /// </summary>
        public static SceneWrapper GetActiveScene()
        {
            try
            {
                var nativeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                return new SceneWrapper(nativeScene);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get active scene: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get scene by name
        /// Static Method: SceneManager.GetSceneByName(string) -> Scene
        /// </summary>
        public static SceneWrapper GetSceneByName(string name)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    Log.Warning("Scene name cannot be null or empty");
                    return null;
                }
                
                var nativeScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(name);
                return nativeScene.IsValid() ? new SceneWrapper(nativeScene) : null;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get scene by name '{name}': {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get scene by build index
        /// Static Method: SceneManager.GetSceneAt(int) -> Scene
        /// </summary>
        public static SceneWrapper GetSceneAt(int index)
        {
            try
            {
                if (index < 0 || index >= SceneCount)
                {
                    Log.Warning($"Scene index {index} is out of range (0-{SceneCount-1})");
                    return null;
                }
                
                var nativeScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(index);
                return new SceneWrapper(nativeScene);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get scene at index {index}: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get scene by build index in build settings
        /// Unity 2020.3 compatibility: Use GetSceneAt with validation
        /// </summary>
        public static SceneWrapper GetSceneByBuildIndex(int buildIndex)
        {
            try
            {
                // Unity 2020.3 doesn't have GetSceneByBuildIndex, use alternative
                var sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCount;
                if (buildIndex >= 0 && buildIndex < sceneCount)
                {
                    var nativeScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(buildIndex);
                    return nativeScene.IsValid() ? new SceneWrapper(nativeScene) : null;
                }
                
                Log.Warning($"Build index {buildIndex} is out of range (0-{sceneCount - 1})");
                return null;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get scene by build index {buildIndex}: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get all currently loaded scenes
        /// Returns array of all loaded Scene wrappers
        /// NOTE: Unity GetAllScenes() throws NotSupportedException in IL2CPP,
        ///       so we enumerate manually via GetSceneAt()
        /// </summary>
        public static SceneWrapper[] GetAllScenes()
        {
            try
            {
                var sceneCount = SceneCount;
                var scenes = new SceneWrapper[sceneCount];
                
                for (int i = 0; i < sceneCount; i++)
                {
                    scenes[i] = GetSceneAt(i);
                }
                
                return scenes.Where(s => s != null).ToArray();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get all scenes: {ex.Message}");
                return new SceneWrapper[0];
            }
        }
        
        // ==================== SCENE COUNTS ====================
        
        /// <summary>
        /// Number of currently loaded scenes
        /// Static Property: SceneManager.sceneCount { get; }
        /// </summary>
        public static int SceneCount
        {
            get
            {
                try
                {
                    return UnityEngine.SceneManagement.SceneManager.sceneCount;
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to get scene count: {ex.Message}");
                    return 0;
                }
            }
        }
        
        /// <summary>
        /// Number of scenes in build settings
        /// Static Property: SceneManager.sceneCountInBuildSettings { get; }
        /// </summary>
        public static int SceneCountInBuildSettings
        {
            get
            {
                try
                {
                    return UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed to get scene count in build settings: {ex.Message}");
                    return 0;
                }
            }
        }
        
        // ==================== SCENE OPERATIONS ====================
        
        /// <summary>
        /// Set active scene
        /// Static Method: SceneManager.SetActiveScene(Scene) -> bool
        /// </summary>
        public static bool SetActiveScene(SceneWrapper scene)
        {
            try
            {
                if (scene == null)
                {
                    Log.Warning("Cannot set null scene as active");
                    return false;
                }
                
                if (!scene.IsLoaded)
                {
                    Log.Warning($"Cannot set unloaded scene '{scene.Name}' as active");
                    return false;
                }
                
                return UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene.NativeScene);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to set active scene '{scene?.Name}': {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Create new empty scene
        /// Static Method: SceneManager.CreateScene(string) -> Scene
        /// </summary>
        public static SceneWrapper CreateScene(string sceneName)
        {
            try
            {
                if (string.IsNullOrEmpty(sceneName))
                {
                    Log.Warning("Scene name cannot be null or empty");
                    return null;
                }
                
                var nativeScene = UnityEngine.SceneManagement.SceneManager.CreateScene(sceneName);
                return new SceneWrapper(nativeScene);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to create scene '{sceneName}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Move GameObject to scene
        /// Unity 2020.3 compatibility: Alternative approach using Transform
        /// </summary>
        public static void MoveGameObjectToScene(GameObject go, SceneWrapper scene)
        {
            if (go == null || scene == null)
            {
                Log.Warning("Cannot move null GameObject or to null Scene");
                return;
            }
            
            try
            {
                if (!scene.IsLoaded)
                {
                    Log.Warning($"Cannot move GameObject to unloaded scene '{scene.Name}'");
                    return;
                }
                
                // Unity 2020.3 alternative: Use native scene directly with reflection
                var moveMethod = typeof(UnityEngine.SceneManagement.SceneManager)
                    .GetMethod("MoveGameObjectToScene", new[] { typeof(GameObject), typeof(UnityEngine.SceneManagement.Scene) });
                
                if (moveMethod != null)
                {
                    moveMethod.Invoke(null, new object[] { go, scene.NativeScene });
                    Log.Debug($"Moved GameObject '{go.name}' to scene '{scene.Name}'");
                }
                else
                {
                    Log.Warning("MoveGameObjectToScene not available in this Unity version");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to move GameObject '{go?.name}' to scene '{scene?.Name}': {ex.Message}");
            }
        }
        
        // ==================== ASYNC OPERATIONS ====================
        
        /// <summary>
        /// Load scene asynchronously by name
        /// Static Method: SceneManager.LoadSceneAsync(string) -> AsyncOperation
        /// </summary>
        public static AsyncOperation LoadSceneAsync(string sceneName)
        {
            return LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }
        
        /// <summary>
        /// Load scene asynchronously by name with mode
        /// Static Method: SceneManager.LoadSceneAsync(string, LoadSceneMode) -> AsyncOperation
        /// </summary>
        public static AsyncOperation LoadSceneAsync(string sceneName, LoadSceneMode mode)
        {
            try
            {
                if (string.IsNullOrEmpty(sceneName))
                {
                    Log.Warning("Scene name cannot be null or empty");
                    return null;
                }
                
                Log.Info($"Loading scene '{sceneName}' with mode {mode}");
                return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName, mode);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load scene '{sceneName}' asynchronously: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Load scene asynchronously by build index
        /// Static Method: SceneManager.LoadSceneAsync(int) -> AsyncOperation
        /// </summary>
        public static AsyncOperation LoadSceneAsync(int sceneBuildIndex)
        {
            return LoadSceneAsync(sceneBuildIndex, LoadSceneMode.Single);
        }
        
        /// <summary>
        /// Load scene asynchronously by build index with mode
        /// Static Method: SceneManager.LoadSceneAsync(int, LoadSceneMode) -> AsyncOperation
        /// </summary>
        public static AsyncOperation LoadSceneAsync(int sceneBuildIndex, LoadSceneMode mode)
        {
            try
            {
                Log.Info($"Loading scene at build index {sceneBuildIndex} with mode {mode}");
                return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneBuildIndex.ToString(), mode);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load scene at build index {sceneBuildIndex} asynchronously: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Unload scene asynchronously
        /// Static Method: SceneManager.UnloadSceneAsync(Scene) -> AsyncOperation
        /// </summary>
        public static AsyncOperation UnloadSceneAsync(SceneWrapper scene)
        {
            try
            {
                if (scene == null)
                {
                    Log.Warning("Cannot unload null scene");
                    return null;
                }
                
                if (!scene.IsLoaded)
                {
                    Log.Warning($"Scene '{scene.Name}' is already unloaded");
                    return null;
                }
                
                Log.Info($"Unloading scene '{scene.Name}'");
                return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scene.NativeScene);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to unload scene '{scene?.Name}' asynchronously: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Unload scene asynchronously by name
        /// Static Method: SceneManager.UnloadSceneAsync(string) -> AsyncOperation
        /// </summary>
        public static AsyncOperation UnloadSceneAsync(string sceneName)
        {
            try
            {
                if (string.IsNullOrEmpty(sceneName))
                {
                    Log.Warning("Scene name cannot be null or empty");
                    return null;
                }
                
                Log.Info($"Unloading scene '{sceneName}'");
                var targetScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName(sceneName);
                return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(targetScene);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to unload scene '{sceneName}' asynchronously: {ex.Message}");
                return null;
            }
        }
        
        // ==================== SYNCHRONOUS OPERATIONS ====================
        
        /// <summary>
        /// Load scene synchronously by name
        /// Static Method: SceneManager.LoadScene(string)
        /// WARNING: This blocks the main thread
        /// </summary>
        public static void LoadScene(string sceneName)
        {
            LoadScene(sceneName, LoadSceneMode.Single);
        }
        
        /// <summary>
        /// Load scene synchronously by name with mode
        /// Static Method: SceneManager.LoadScene(string, LoadSceneMode)
        /// WARNING: This blocks the main thread
        /// </summary>
        public static void LoadScene(string sceneName, LoadSceneMode mode)
        {
            try
            {
                if (string.IsNullOrEmpty(sceneName))
                {
                    Log.Warning("Scene name cannot be null or empty");
                    return;
                }
                
                Log.Warning($"Loading scene '{sceneName}' synchronously - this will block main thread");
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName, mode);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load scene '{sceneName}' synchronously: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Load scene synchronously by build index
        /// Static Method: SceneManager.LoadScene(int)
        /// WARNING: This blocks the main thread
        /// </summary>
        public static void LoadScene(int sceneBuildIndex)
        {
            LoadScene(sceneBuildIndex, LoadSceneMode.Single);
        }
        
        /// <summary>
        /// Load scene synchronously by build index with mode
        /// Static Method: SceneManager.LoadScene(int, LoadSceneMode)
        /// WARNING: This blocks the main thread
        /// </summary>
        public static void LoadScene(int sceneBuildIndex, LoadSceneMode mode)
        {
            try
            {
                Log.Warning($"Loading scene at build index {sceneBuildIndex} synchronously - this will block main thread");
                UnityEngine.SceneManagement.SceneManager.LoadScene(sceneBuildIndex, mode);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load scene at build index {sceneBuildIndex} synchronously: {ex.Message}");
            }
        }
    }
}