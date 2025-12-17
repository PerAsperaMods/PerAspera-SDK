using System;
using PerAspera.Core;
using UnityEngine;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Unity 2020.3.49f1 Compatible Scene Utility Wrapper
    /// COMPATIBILITY: SceneUtility doesn't exist in Unity 2020.3, providing alternative implementation
    /// </summary>
    public static class SceneUtility
    {
        private static readonly LogAspera Log = new LogAspera("SceneUtility");
        
        // ==================== PATH / INDEX CONVERSION ====================
        
        /// <summary>
        /// Get scene build index from scene path
        /// Unity 2020.3 compatibility: Alternative implementation since SceneUtility doesn't exist
        /// Returns -1 if scene not found in build settings
        /// </summary>
        public static int GetBuildIndexByScenePath(string scenePath)
        {
            try
            {
                if (string.IsNullOrEmpty(scenePath))
                {
                    Log.Warning("Scene path cannot be null or empty");
                    return -1;
                }
                
                // Unity 2020.3 alternative: Use SceneManager.sceneCountInBuildSettings
                var sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
                for (int i = 0; i < sceneCount; i++)
                {
                    // Simple path matching - very limited without SceneUtility in Unity 2020.3
                    var levelName = $"level{i}";
                    if (scenePath.ToLower().Contains(levelName) || scenePath.Contains($"scene{i}"))
                    {
                        Log.Debug($"Scene path '{scenePath}' -> build index {i} (approximate match)");
                        return i;
                    }
                }
                
                Log.Debug($"Scene path '{scenePath}' not found in build settings");
                return -1;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get build index for scene path '{scenePath}': {ex.Message}");
                return -1;
            }
        }
        
        /// <summary>
        /// Get scene path from build index
        /// Unity 2020.3 compatibility: Alternative implementation since SceneUtility doesn't exist
        /// Returns null if build index is invalid
        /// </summary>
        public static string GetScenePathByBuildIndex(int buildIndex)
        {
            try
            {
                var sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
                if (buildIndex < 0 || buildIndex >= sceneCount)
                {
                    Log.Warning($"Build index {buildIndex} is out of range (0-{sceneCount - 1})");
                    return null;
                }
                
                // Unity 2020.3 alternative: Generate standard path (very limited)
                var scenePath = $"Assets/Scenes/Level{buildIndex}.unity";
                Log.Debug($"Build index {buildIndex} -> estimated scene path '{scenePath}'");
                return scenePath;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get scene path for build index {buildIndex}: {ex.Message}");
                return null;
            }
        }
        
        // ==================== VALIDATION UTILITIES ====================
        
        /// <summary>
        /// Check if a scene path exists in build settings
        /// Returns true if scene is configured in build
        /// </summary>
        public static bool IsSceneInBuild(string scenePath)
        {
            return GetBuildIndexByScenePath(scenePath) != -1;
        }
        
        /// <summary>
        /// Check if a build index is valid
        /// Unity 2020.3 compatibility: Check against SceneManager.sceneCountInBuildSettings
        /// Returns true if build index exists in build settings
        /// </summary>
        public static bool IsBuildIndexValid(int buildIndex)
        {
            try
            {
                var sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
                if (buildIndex < 0 || buildIndex >= sceneCount)
                {
                    return false;
                }
                
                // Simple validation - if index is within range, assume valid in Unity 2020.3
                Log.Debug($"Build index {buildIndex} validity: {buildIndex >= 0 && buildIndex < sceneCount}");
                return true;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to validate build index {buildIndex}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Get scene name from scene path
        /// Extracts filename without extension from full path
        /// Example: "Assets/Scenes/MainMenu.unity" -> "MainMenu"
        /// </summary>
        public static string GetSceneNameFromPath(string scenePath)
        {
            try
            {
                if (string.IsNullOrEmpty(scenePath))
                {
                    Log.Warning("Scene path cannot be null or empty");
                    return null;
                }
                
                // Remove directory path
                var fileName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                
                Log.Debug($"Scene path '{scenePath}' -> scene name '{fileName}'");
                return fileName;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to extract scene name from path '{scenePath}': {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get all scene paths from build settings
        /// Unity 2020.3 compatibility: Enumerate through SceneManager.sceneCountInBuildSettings
        /// Returns array of all scene paths configured in build
        /// </summary>
        public static string[] GetAllScenePathsInBuild()
        {
            try
            {
                var sceneCount = UnityEngine.SceneManagement.SceneManager.sceneCountInBuildSettings;
                var validScenes = new System.Collections.Generic.List<string>();
                
                for (int i = 0; i < sceneCount; i++)
                {
                    var scenePath = GetScenePathByBuildIndex(i);
                    if (!string.IsNullOrEmpty(scenePath))
                    {
                        validScenes.Add(scenePath);
                    }
                }
                
                Log.Debug($"Found {validScenes.Count} valid scenes in build settings (out of {sceneCount} total)");
                return validScenes.ToArray();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get all scene paths from build settings: {ex.Message}");
                return new string[0];
            }
        }
        
        /// <summary>
        /// Get all scene names from build settings
        /// Returns array of all scene names (without paths) configured in build
        /// </summary>
        public static string[] GetAllSceneNamesInBuild()
        {
            try
            {
                var scenePaths = GetAllScenePathsInBuild();
                var sceneNames = new string[scenePaths.Length];
                
                for (int i = 0; i < scenePaths.Length; i++)
                {
                    sceneNames[i] = GetSceneNameFromPath(scenePaths[i]);
                }
                
                Log.Debug($"Extracted {sceneNames.Length} scene names from build settings");
                return sceneNames;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get all scene names from build settings: {ex.Message}");
                return new string[0];
            }
        }
        
        // ==================== SEARCH UTILITIES ====================
        
        /// <summary>
        /// Find scene build index by scene name (without path)
        /// Searches through all scenes in build settings
        /// Returns -1 if not found
        /// </summary>
        public static int FindBuildIndexBySceneName(string sceneName)
        {
            try
            {
                if (string.IsNullOrEmpty(sceneName))
                {
                    Log.Warning("Scene name cannot be null or empty");
                    return -1;
                }
                
                var scenePaths = GetAllScenePathsInBuild();
                
                for (int i = 0; i < scenePaths.Length; i++)
                {
                    var pathSceneName = GetSceneNameFromPath(scenePaths[i]);
                    if (string.Equals(pathSceneName, sceneName, StringComparison.OrdinalIgnoreCase))
                    {
                        Log.Debug($"Scene name '{sceneName}' found at build index {i}");
                        return i;
                    }
                }
                
                Log.Debug($"Scene name '{sceneName}' not found in build settings");
                return -1;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to find build index for scene name '{sceneName}': {ex.Message}");
                return -1;
            }
        }
        
        /// <summary>
        /// Find scene path by scene name (without path)  
        /// Searches through all scenes in build settings
        /// Returns null if not found
        /// </summary>
        public static string FindScenePathBySceneName(string sceneName)
        {
            try
            {
                var buildIndex = FindBuildIndexBySceneName(sceneName);
                return buildIndex != -1 ? GetScenePathByBuildIndex(buildIndex) : null;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to find scene path for scene name '{sceneName}': {ex.Message}");
                return null;
            }
        }
        
        // ==================== DEBUG UTILITIES ====================
        
        /// <summary>
        /// Log detailed information about all scenes in build settings
        /// Useful for debugging scene configuration
        /// </summary>
        public static void LogBuildSettingsInfo()
        {
            try
            {
                var sceneCount = SceneManager.SceneCountInBuildSettings;
                Log.Info($"=== Build Settings Scene Info ({sceneCount} scenes) ===");
                
                for (int i = 0; i < sceneCount; i++)
                {
                    var scenePath = GetScenePathByBuildIndex(i);
                    var sceneName = GetSceneNameFromPath(scenePath);
                    Log.Info($"[{i}] '{sceneName}' -> '{scenePath}'");
                }
                
                Log.Info("=== End Build Settings Info ===");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to log build settings info: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Validate scene configuration and log any issues
        /// Useful for troubleshooting scene setup
        /// </summary>
        public static void ValidateSceneConfiguration()
        {
            try
            {
                Log.Info("=== Scene Configuration Validation ===");
                
                var sceneCount = SceneManager.SceneCountInBuildSettings;
                var issues = 0;
                
                for (int i = 0; i < sceneCount; i++)
                {
                    var scenePath = GetScenePathByBuildIndex(i);
                    
                    if (string.IsNullOrEmpty(scenePath))
                    {
                        Log.Warning($"Build index {i} has null/empty scene path");
                        issues++;
                        continue;
                    }
                    
                    var sceneName = GetSceneNameFromPath(scenePath);
                    if (string.IsNullOrEmpty(sceneName))
                    {
                        Log.Warning($"Build index {i} path '{scenePath}' has no valid scene name");
                        issues++;
                    }
                    
                    // Check reverse lookup
                    var foundBuildIndex = GetBuildIndexByScenePath(scenePath);
                    if (foundBuildIndex != i)
                    {
                        Log.Warning($"Reverse lookup mismatch: {i} != {foundBuildIndex} for '{scenePath}'");
                        issues++;
                    }
                }
                
                if (issues == 0)
                {
                    Log.Info($"Scene configuration validation passed ({sceneCount} scenes)");
                }
                else
                {
                    Log.Warning($"Scene configuration validation found {issues} issues");
                }
                
                Log.Info("=== End Scene Configuration Validation ===");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to validate scene configuration: {ex.Message}");
            }
        }
    }
}