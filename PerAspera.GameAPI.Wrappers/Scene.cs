using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using PerAspera.Core;
using Il2CppInterop.Runtime.InteropTypes.Arrays;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Type-safe wrapper for Unity Scene struct
    /// Provides safe access to Unity scene data via IL2CPP
    /// DOC: Scene wrapper for Unity SceneManagement system
    /// </summary>
    public class Scene : WrapperBase
    {
        private UnityEngine.SceneManagement.Scene _nativeScene;
        
        /// <summary>
        /// Create wrapper from native Unity Scene
        /// </summary>
        public Scene(UnityEngine.SceneManagement.Scene nativeScene) : base(null)
        {
            _nativeScene = nativeScene;
            // Scene is struct, so no object validation needed
        }
        
        /// <summary>
        /// Get native Unity Scene struct (for internal SDK use)
        /// </summary>
        internal UnityEngine.SceneManagement.Scene NativeScene => _nativeScene;
        
        // ==================== CORE PROPERTIES ====================
        
        /// <summary>
        /// Scene name (filename without extension)
        /// Property: name { get; }
        /// </summary>
        public string Name => _nativeScene.name ?? "";
        
        /// <summary>
        /// Scene file path
        /// Property: path { get; }  
        /// </summary>
        public string Path => _nativeScene.path ?? "";
        
        /// <summary>
        /// Scene build index (-1 if not in build settings)
        /// Property: buildIndex { get; }
        /// </summary>
        public int BuildIndex => _nativeScene.buildIndex;
        
        /// <summary>
        /// Scene handle (internal Unity identifier)
        /// Property: handle { get; }
        /// </summary>
        public int Handle => _nativeScene.handle;
        
        /// <summary>
        /// Check if scene is currently loaded
        /// Property: isLoaded { get; }
        /// </summary>
        public bool IsLoaded => _nativeScene.isLoaded;
        
        /// <summary>
        /// Current scene loading state
        /// Unity 2020.3 compatibility: Scene struct doesn't have loadingState property
        /// </summary>
        public string LoadingState 
        { 
            get 
            {
                try 
                {
                    // Unity 2020.3 alternative: derive state from isLoaded
                    if (_nativeScene.IsValid())
                    {
                        return _nativeScene.isLoaded ? "Loaded" : "Loading";
                    }
                    return "Invalid";
                } 
                catch 
                {
                    return "Unknown";
                }
            }
        }
        
        /// <summary>
        /// Number of root GameObjects in scene
        /// Property: rootCount { get; }
        /// </summary>
        public int RootCount => _nativeScene.rootCount;
        
        /// <summary>
        /// Scene GUID (unique identifier) - Unity 2020.3 compatibility
        /// Property: Alternative to guid property
        /// </summary>
        public string Guid 
        {
            get
            {
                try
                {
                    // Unity 2020.3 compatibility: Scene struct doesn't have guid or path properties
                    // Use scene name and build index as unique identifier
                    return $"{_nativeScene.name}_{_nativeScene.buildIndex}_{_nativeScene.GetHashCode()}";
                }
                catch
                {
                    return _nativeScene.GetHashCode().ToString();
                }
            }
        }
        
        // ==================== VALIDATION ====================
        
        /// <summary>
        /// Check if scene is valid (has valid handle)
        /// Method: IsValid()
        /// </summary>
        public bool IsValid()
        {
            try
            {
                return _nativeScene.IsValid();
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Scene.IsValid() failed: {ex.Message}");
                return false;
            }
        }
        
        // ==================== GAMEOBJECT MANAGEMENT ====================
        
        /// <summary>
        /// Get all root GameObjects in this scene
        /// Method: GetRootGameObjects() -> GameObject[]
        /// Safe IL2CPP conversion with error handling
        /// </summary>
        public GameObject[] GetRootGameObjects()
        {
            try
            {
                if (!IsLoaded)
                {
                    Log.LogWarning($"Scene '{Name}' is not loaded, cannot get root GameObjects");
                    return new GameObject[0];
                }
                
                var nativeArray = _nativeScene.GetRootGameObjects();
                return nativeArray?.ToArray() ?? new GameObject[0];
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get root GameObjects for scene '{Name}': {ex.Message}");
                return new GameObject[0];
            }
        }
        
        /// <summary>
        /// Fill list with root GameObjects (allocation-free)
        /// Method: GetRootGameObjects(List<GameObject> rootObjects)
        /// </summary>
        public void GetRootGameObjects(System.Collections.Generic.List<GameObject> rootObjects)
        {
            try
            {
                if (!IsLoaded)
                {
                    Log.LogWarning($"Scene '{Name}' is not loaded, cannot get root GameObjects");
                    return;
                }
                
                _nativeScene.GetRootGameObjects(rootObjects);
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to fill root GameObjects list for scene '{Name}': {ex.Message}");
            }
        }
        
        // ==================== EQUALITY & COMPARISON ====================
        
        /// <summary>
        /// Check if this scene equals another
        /// </summary>
        public bool Equals(Scene other)
        {
            if (other == null) return false;
            return _nativeScene.handle == other._nativeScene.handle;
        }
        
        public override bool Equals(object obj)
        {
            return obj is Scene other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return _nativeScene.handle.GetHashCode();
        }
        
        public static bool operator ==(Scene left, Scene right)
        {
            if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
            return left.Equals(right);
        }
        
        public static bool operator !=(Scene left, Scene right)
        {
            return !(left == right);
        }
        
        // ==================== DISPLAY ====================
        
        public override string ToString()
        {
            return $"Scene(Name:'{Name}', BuildIndex:{BuildIndex}, IsLoaded:{IsLoaded}, Handle:{Handle})";
        }
    }
}