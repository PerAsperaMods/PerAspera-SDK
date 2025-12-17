#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core.IL2CPP;
using UnityEngine;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Central API for Handle-based entity access in Per Aspera
    /// Provides type-safe, validated access to game entities via Handle system
    /// DOC: F:\ModPeraspera\Internal_doc\ARCHITECTURE\Handle-System-Architecture.md
    /// </summary>
    public static class HandleHelper
    {
        private static readonly string LogPrefix = "[HandleHelper]";
        
        // ==================== AVAILABILITY CHECK ====================
        
        /// <summary>
        /// Check if Handle system is available
        /// Returns true after BaseGame.Awake() and Keeper initialization
        /// </summary>
        public static bool IsAvailable()
        {
            try
            {
                var keeperMap = GetKeeperMapInternal();
                return keeperMap != null;
            }
            catch
            {
                return false;
            }
        }
        
        // ==================== CORE ACCESS ====================
        
        /// <summary>
        /// Find entity by Handle with type safety and null protection
        /// Performance: O(1) Dictionary lookup via KeeperMap
        /// Thread Safety: Safe - Handle is value type, validation before access
        /// </summary>
        /// <typeparam name="T">Expected entity type (Building, Faction, etc.)</typeparam>
        /// <param name="handle">Entity Handle (from game events, UI, etc.)</param>
        /// <returns>Typed entity instance or null if not found/invalid</returns>
        public static T? FindSafe<T>(object handle) where T : class
        {
            try
            {
                // 1. Get current KeeperMap
                var keeperMap = GetKeeperMapInternal();
                if (keeperMap == null)
                {
                    LogDebug($"KeeperMap not available");
                    return null;
                }
                
                // 2. Validate Handle exists (fast Dictionary.ContainsKey)
                bool contains = keeperMap.InvokeMethod<bool>("Contains", handle);
                if (!contains)
                {
                    LogDebug($"Handle not found in KeeperMap");
                    return null;
                }
                
                // 3. Perform typed lookup (generic Find<T> method)
                return keeperMap.InvokeMethod<T>("Find", handle);
            }
            catch (Exception ex)
            {
                LogWarning($"FindSafe<{typeof(T).Name}> failed: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Validate Handle exists and is accessible
        /// Fast check before attempting FindSafe operations
        /// Performance: O(1) Dictionary.ContainsKey operation
        /// </summary>
        /// <param name="handle">Handle to validate</param>
        /// <returns>True if Handle is valid and entity exists</returns>
        public static bool IsValid(object handle)
        {
            try
            {
                if (handle == null) return false;
                
                var keeperMap = GetKeeperMapInternal();
                if (keeperMap == null) return false;
                
                return keeperMap.InvokeMethod<bool>("Contains", handle);
            }
            catch (Exception ex)
            {
                LogDebug($"IsValid failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Attempt Handle resolution with try-pattern
        /// Combines validation and access in single call
        /// Recommended pattern for performance-critical code
        /// </summary>
        /// <typeparam name="T">Expected entity type</typeparam>
        /// <param name="handle">Entity Handle</param>
        /// <param name="result">Output entity instance</param>
        /// <returns>True if resolution successful</returns>
        public static bool TryFind<T>(object handle, out T? result) where T : class
        {
            result = FindSafe<T>(handle);
            return result != null;
        }
        
        // ==================== BULK OPERATIONS ====================
        
        /// <summary>
        /// Get all entities of specified type from KeeperMap
        /// Efficient enumeration of all registered entities by type
        /// Performance: O(n) where n = total entities in KeeperMap
        /// </summary>
        /// <typeparam name="T">Entity type to enumerate</typeparam>
        /// <returns>All entities of type T currently in game</returns>
        public static IEnumerable<T> FindAll<T>() where T : class
        {
            var keeperMap = GetKeeperMapInternal();
            if (keeperMap == null)
            {
                LogDebug("KeeperMap not available for FindAll");
                return Enumerable.Empty<T>();
            }
            
            try
            {
                // Access _objects Dictionary for bulk enumeration
                var objectsDict = keeperMap.GetFieldValue<object>("_objects");
                if (objectsDict == null)
                {
                    LogDebug("_objects Dictionary not accessible");
                    return Enumerable.Empty<T>();
                }
                
                // Use IL2CPP Dictionary enumeration
                var values = objectsDict.InvokeMethod<object>("get_Values");
                if (values == null) return Enumerable.Empty<T>();
                
                return EnumerateValues<T>(values);
            }
            catch (Exception ex)
            {
                LogWarning($"FindAll<{typeof(T).Name}> failed: {ex.Message}");
                return Enumerable.Empty<T>();
            }
        }
        
        private static IEnumerable<T> EnumerateValues<T>(object values) where T : class
        {
            var enumerator = values.InvokeMethod<object>("GetEnumerator");
            if (enumerator == null) yield break;
            
            // Enumerate and filter by type
            while (enumerator.InvokeMethod<bool>("MoveNext"))
            {
                var current = enumerator.GetPropertyValue<object>("Current");
                if (current is T typedEntity)
                {
                    yield return typedEntity;
                }
            }
        }
        
        /// <summary>
        /// Get entities by Handle list with batch validation
        /// Optimized for processing multiple Handles efficiently
        /// Skips invalid Handles rather than throwing exceptions
        /// </summary>
        /// <typeparam name="T">Expected entity type</typeparam>
        /// <param name="handles">Collection of Handles to resolve</param>
        /// <returns>Valid entities (skips invalid Handles)</returns>
        public static IEnumerable<T> FindMany<T>(IEnumerable<object> handles) where T : class
        {
            if (handles == null) 
                return Enumerable.Empty<T>();
            
            return EnumerateManyEntities<T>(handles);
        }
        
        private static IEnumerable<T> EnumerateManyEntities<T>(IEnumerable<object> handles) where T : class
        {
            var results = new List<T>();
            
            try
            {
                var keeperMap = GetKeeperMapInternal();
                if (keeperMap == null)
                {
                    LogDebug("KeeperMap not available for FindMany");
                    return results;
                }
                
                foreach (var handle in handles)
                {
                    if (handle != null && keeperMap.InvokeMethod<bool>("Contains", handle))
                    {
                        var entity = keeperMap.InvokeMethod<T>("Find", handle);
                        if (entity != null)
                        {
                            results.Add(entity);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogWarning($"FindMany<{typeof(T).Name}> failed: {ex.Message}");
            }
            
            return results;
        }
        
        // ==================== SPECIALIZED ACCESS ====================
        
        /// <summary>
        /// Find Buildings by type name (e.g., "SolarPanel", "WaterExtractor")
        /// Convenience method for common building queries
        /// Performance: O(n) where n = total buildings
        /// </summary>
        /// <param name="buildingTypeName">BuildingType.name from YAML</param>
        /// <returns>All buildings matching type name</returns>
        public static IEnumerable<object> FindBuildingsByType(string buildingTypeName)
        {
            if (string.IsNullOrEmpty(buildingTypeName)) 
                return Enumerable.Empty<object>();
            
            return EnumerateBuildingsByType(buildingTypeName);
        }
        
        private static IEnumerable<object> EnumerateBuildingsByType(string buildingTypeName)
        {
            var results = new List<object>();
            
            try
            {
                // Get all buildings via Handle system
                var allBuildings = FindAll<object>()
                    .Where(entity => IsBuilding(entity));
                
                foreach (var building in allBuildings)
                {
                    // Check buildingType.name property
                    var buildingType = building.GetPropertyValue<object>("buildingType");
                    if (buildingType != null)
                    {
                        var name = buildingType.GetPropertyValue<object>("name") as string;
                        if (string.Equals(name, buildingTypeName, StringComparison.OrdinalIgnoreCase))
                        {
                            results.Add(building);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogWarning($"FindBuildingsByType({buildingTypeName}) failed: {ex.Message}");
            }
            
            return results;
        }
        
        /// <summary>
        /// Find Buildings within radius of position
        /// Spatial query using Handle system + position filtering
        /// Performance: O(n) where n = total buildings
        /// </summary>
        /// <param name="center">Center position for search</param>
        /// <param name="radius">Search radius in game units</param>
        /// <returns>Buildings within specified area</returns>
        public static IEnumerable<object> FindBuildingsNear(Vector3 center, float radius)
        {
            if (radius <= 0) 
                return Enumerable.Empty<object>();
            
            return EnumerateBuildingsNear(center, radius);
        }
        
        private static IEnumerable<object> EnumerateBuildingsNear(Vector3 center, float radius)
        {
            var results = new List<object>();
            
            try
            {
                var radiusSquared = radius * radius;
                var allBuildings = FindAll<object>()
                    .Where(entity => IsBuilding(entity));
                
                foreach (var building in allBuildings)
                {
                    var position = GetBuildingPosition(building);
                    if (position.HasValue)
                    {
                        var distance = Vector3.SqrMagnitude(position.Value - center);
                        if (distance <= radiusSquared)
                        {
                            results.Add(building);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogWarning($"FindBuildingsNear failed: {ex.Message}");
            }
            
            return results;
        }
        
        // ==================== INTERNAL HELPERS ====================
        
        /// <summary>
        /// Get KeeperMap instance from BaseGame.Instance.keeper.map
        /// Internal method with error handling and validation
        /// </summary>
        private static object? GetKeeperMapInternal()
        {
            try
            {
                // Use existing BaseGame wrapper
                var baseGame = BaseGame.GetCurrent();
                if (baseGame == null)
                {
                    LogDebug("BaseGame.GetCurrent() returned null");
                    return null;
                }
                
                // Get Keeper from BaseGame
                var keeper = baseGame.GetKeeper();
                if (keeper == null)
                {
                    LogDebug("BaseGame.GetKeeper() returned null");
                    return null;
                }
                
                // Get KeeperMap from Keeper
                var keeperMap = keeper.GetFieldValue<object>("map");
                if (keeperMap == null)
                {
                    LogDebug("keeper.map field returned null");
                    return null;
                }
                
                return keeperMap;
            }
            catch (Exception ex)
            {
                LogDebug($"GetKeeperMapInternal failed: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Check if entity is a Building type
        /// Uses duck typing approach for IL2CPP compatibility
        /// </summary>
        private static bool IsBuilding(object entity)
        {
            if (entity == null) return false;
            
            try
            {
                // Check for building-specific properties
                var buildingType = entity.GetPropertyValue<object>("buildingType");
                var position = entity.GetPropertyValue<object>("position");
                return buildingType != null && position != null;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Extract position from building entity
        /// Safe accessor for building.position property
        /// </summary>
        private static Vector3? GetBuildingPosition(object building)
        {
            try
            {
                var position = building.GetPropertyValue<object>("position");
                if (position is Vector3 vec3)
                {
                    return vec3;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }
        
        // ==================== LOGGING ====================
        
        private static void LogDebug(string message)
        {
            UnityEngine.Debug.Log($"{LogPrefix} [DEBUG] {message}");
        }
        
        private static void LogWarning(string message)
        {
            UnityEngine.Debug.LogWarning($"{LogPrefix} [WARNING] {message}");
        }
        
        /// <summary>
        /// Get Handle system performance metrics
        /// Debug information for optimization and monitoring
        /// </summary>
        public static HandleSystemMetrics GetMetrics()
        {
            try
            {
                var keeperMap = GetKeeperMapInternal();
                if (keeperMap == null)
                {
                    return new HandleSystemMetrics
                    {
                        IsAvailable = false,
                        TotalEntities = 0,
                        ErrorMessage = "KeeperMap not available"
                    };
                }
                
                var objectsDict = keeperMap.GetFieldValue<object>("_objects");
                var count = objectsDict?.GetPropertyValue<object>("Count") as int? ?? 0;
                
                return new HandleSystemMetrics
                {
                    IsAvailable = true,
                    TotalEntities = count,
                    ErrorMessage = null
                };
            }
            catch (Exception ex)
            {
                return new HandleSystemMetrics
                {
                    IsAvailable = false,
                    TotalEntities = 0,
                    ErrorMessage = ex.Message
                };
            }
        }
    }
    
    /// <summary>
    /// Handle system performance and status metrics
    /// Used for debugging and monitoring Handle operations
    /// </summary>
    public struct HandleSystemMetrics
    {
        /// <summary>
        /// Gets or sets whether the Handle system is available and functioning
        /// </summary>
        public bool IsAvailable { get; set; }
        
        /// <summary>
        /// Gets or sets the total number of entities tracked in the Handle system
        /// </summary>
        public int TotalEntities { get; set; }
        
        /// <summary>
        /// Gets or sets the error message if Handle system access failed
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// Returns a string representation of the Handle system metrics
        /// </summary>
        /// <returns>Formatted metrics string</returns>
        public override string ToString()
        {
            if (!IsAvailable)
                return $"HandleSystem: UNAVAILABLE ({ErrorMessage})";
            
            return $"HandleSystem: Available, {TotalEntities} entities";
        }
    }
}