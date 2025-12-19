#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// High-level helper class for common Keeper operations
    /// Provides convenient methods for entity access and management
    /// Built on top of KeeperWrapper and KeeperMapWrapper for optimal performance
    /// </summary>
    public static class KeeperHelper
    {
        private static readonly LogAspera Log = new LogAspera("KeeperHelper");
        
        // ==================== AVAILABILITY & STATUS ====================
        
        /// <summary>
        /// Check if Keeper system is available and ready
        /// Combines BaseGame availability + Keeper initialization checks
        /// </summary>
        /// <returns>True if Keeper system is fully operational</returns>
        public static bool IsKeeperReady()
        {
            try
            {
                var keeper = KeeperWrapper.GetCurrent();
                return keeper?.IsReady() ?? false;
            }
            catch (Exception ex)
            {
                Log.Warning($"IsKeeperReady check failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Get comprehensive Keeper system status for debugging
        /// Includes BaseGame, Keeper, and KeeperMap availability
        /// </summary>
        /// <returns>Detailed system status information</returns>
        public static KeeperSystemStatus GetSystemStatus()
        {
            var status = new KeeperSystemStatus();
            
            try
            {
                // Check BaseGame availability
                var baseGame = BaseGame.GetCurrent();
                status.BaseGameAvailable = baseGame != null;
                
                if (!status.BaseGameAvailable) return status;
                
                // Check Keeper availability
                var keeper = KeeperWrapper.GetCurrent();
                status.KeeperAvailable = keeper != null;
                
                if (keeper != null)
                {
                    status.KeeperDiagnostics = keeper.GetDiagnostics();
                }
                
                // Check KeeperMap availability
                var keeperMap = KeeperMapWrapper.GetCurrent();
                status.KeeperMapAvailable = keeperMap != null;
                
                if (keeperMap != null)
                {
                    status.EntityCount = keeperMap.GetEntityCount();
                }
                
                status.IsFullyOperational = status.BaseGameAvailable && 
                                           status.KeeperAvailable && 
                                           status.KeeperMapAvailable &&
                                           status.KeeperDiagnostics.IsInitialized;
                
                return status;
            }
            catch (Exception ex)
            {
                Log.Error($"GetSystemStatus failed: {ex.Message}");
                status.ErrorMessage = ex.Message;
                return status;
            }
        }
        
        // ==================== ENTITY ACCESS ====================
        
        /// <summary>
        /// Find entity by Handle with automatic error handling and logging
        /// Preferred method for Handle-based entity lookup
        /// </summary>
        /// <typeparam name="T">Expected entity type</typeparam>
        /// <param name="handle">Entity Handle</param>
        /// <param name="logErrors">Whether to log errors (default: true)</param>
        /// <returns>Typed entity instance or null if not found</returns>
        public static T? FindEntity<T>(Handle handle, bool logErrors = true) where T : class
        {
            try
            {
                var keeperMap = KeeperMapWrapper.GetCurrent();
                if (keeperMap == null)
                {
                    if (logErrors) Log.Warning("KeeperMap not available for entity lookup");
                    return null;
                }
                
                return keeperMap.Find<T>(handle);
            }
            catch (Exception ex)
            {
                if (logErrors) Log.Error($"FindEntity<{typeof(T).Name}> failed: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Check if entity exists by Handle (fast Contains check)
        /// More efficient than FindEntity when you only need existence verification
        /// </summary>
        /// <param name="handle">Handle to check</param>
        /// <returns>True if entity exists in KeeperMap</returns>
        public static bool EntityExists(Handle handle)
        {
            try
            {
                var keeperMap = KeeperMapWrapper.GetCurrent();
                return keeperMap?.Contains(handle) ?? false;
            }
            catch (Exception ex)
            {
                Log.Warning($"EntityExists check failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Find multiple entities by Handle list with batch optimization
        /// More efficient than multiple FindEntity calls
        /// </summary>
        /// <typeparam name="T">Expected entity type</typeparam>
        /// <param name="handles">Collection of Handles to lookup</param>
        /// <returns>Dictionary mapping Handle to entity (null if not found)</returns>
        public static Dictionary<Handle, T?> FindEntities<T>(IEnumerable<Handle> handles) where T : class
        {
            var results = new Dictionary<Handle, T?>();
            
            try
            {
                var keeperMap = KeeperMapWrapper.GetCurrent();
                if (keeperMap == null)
                {
                    Log.Warning("KeeperMap not available for batch entity lookup");
                    return results;
                }
                
                foreach (var handle in handles)
                {
                    results[handle] = keeperMap.Find<T>(handle);
                }
                
                return results;
            }
            catch (Exception ex)
            {
                Log.Error($"FindEntities<{typeof(T).Name}> failed: {ex.Message}");
                return results;
            }
        }
        
        // ==================== ENTITY ENUMERATION ====================
        
        /// <summary>
        /// Get all entities of a specific type from KeeperMap
        /// Performance: O(n) where n = total entities - use sparingly
        /// </summary>
        /// <typeparam name="T">Entity type to enumerate</typeparam>
        /// <returns>All entities of type T currently registered</returns>
        public static IEnumerable<T> GetAllEntities<T>() where T : class
        {
            try
            {
                var keeperMap = KeeperMapWrapper.GetCurrent();
                if (keeperMap == null)
                {
                    Log.Warning($"KeeperMap not available for GetAllEntities<{typeof(T).Name}>");
                    return Enumerable.Empty<T>();
                }
                
                return keeperMap.EnumerateEntitiesByType(entity => entity is T).Cast<T>();
            }
            catch (Exception ex)
            {
                Log.Error($"GetAllEntities<{typeof(T).Name}> failed: {ex.Message}");
                return Enumerable.Empty<T>();
            }
        }
        
        /// <summary>
        /// Count entities of a specific type without enumeration
        /// More efficient than GetAllEntities().Count() for large datasets
        /// </summary>
        /// <typeparam name="T">Entity type to count</typeparam>
        /// <returns>Number of entities of type T</returns>
        public static int CountEntities<T>() where T : class
        {
            try
            {
                return GetAllEntities<T>().Count();
            }
            catch (Exception ex)
            {
                Log.Error($"CountEntities<{typeof(T).Name}> failed: {ex.Message}");
                return 0;
            }
        }
        
        // ==================== HANDLE OPERATIONS ====================
        
        /// <summary>
        /// Validate multiple Handles efficiently
        /// More efficient than multiple EntityExists calls
        /// </summary>
        /// <param name="handles">Handles to validate</param>
        /// <returns>Dictionary mapping Handle to existence status</returns>
        public static Dictionary<Handle, bool> ValidateHandles(IEnumerable<Handle> handles)
        {
            var results = new Dictionary<Handle, bool>();
            
            try
            {
                var keeperMap = KeeperMapWrapper.GetCurrent();
                if (keeperMap == null)
                {
                    Log.Warning("KeeperMap not available for Handle validation");
                    return results;
                }
                
                foreach (var handle in handles)
                {
                    results[handle] = keeperMap.Contains(handle);
                }
                
                return results;
            }
            catch (Exception ex)
            {
                Log.Error($"ValidateHandles failed: {ex.Message}");
                return results;
            }
        }
        
        // ==================== DIAGNOSTICS & MONITORING ====================
        
        /// <summary>
        /// Print comprehensive Keeper system diagnostics to log
        /// Useful for debugging and system monitoring
        /// </summary>
        public static void LogSystemDiagnostics()
        {
            try
            {
                var status = GetSystemStatus();
                
                Log.Info("=== KEEPER SYSTEM DIAGNOSTICS ===");
                Log.Info($"Fully Operational: {status.IsFullyOperational}");
                Log.Info($"BaseGame Available: {status.BaseGameAvailable}");
                Log.Info($"Keeper Available: {status.KeeperAvailable}");
                Log.Info($"KeeperMap Available: {status.KeeperMapAvailable}");
                Log.Info($"Entity Count: {status.EntityCount}");
                
                if (status.KeeperAvailable)
                {
                    var diag = status.KeeperDiagnostics;
                    Log.Info($"Keeper Details: {diag}");
                }
                
                if (!string.IsNullOrEmpty(status.ErrorMessage))
                {
                    Log.Error($"System Error: {status.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"LogSystemDiagnostics failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Get entity type distribution for performance monitoring
        /// Helps identify memory usage patterns and hotspots
        /// </summary>
        /// <returns>Dictionary mapping type name to entity count</returns>
        public static Dictionary<string, int> GetEntityTypeDistribution()
        {
            var distribution = new Dictionary<string, int>();
            
            try
            {
                var keeperMap = KeeperMapWrapper.GetCurrent();
                if (keeperMap == null)
                {
                    Log.Warning("KeeperMap not available for type distribution analysis");
                    return distribution;
                }
                
                foreach (var entity in keeperMap.EnumerateEntities())
                {
                    if (entity == null) continue;
                    
                    var typeName = entity.GetType().Name;
                    distribution[typeName] = distribution.GetValueOrDefault(typeName, 0) + 1;
                }
                
                return distribution;
            }
            catch (Exception ex)
            {
                Log.Error($"GetEntityTypeDistribution failed: {ex.Message}");
                return distribution;
            }
        }
    }
    
    /// <summary>
    /// Comprehensive status information for Keeper system diagnostics
    /// </summary>
    public struct KeeperSystemStatus
    {
        public bool BaseGameAvailable { get; set; }
        public bool KeeperAvailable { get; set; }
        public bool KeeperMapAvailable { get; set; }
        public bool IsFullyOperational { get; set; }
        public int EntityCount { get; set; }
        public KeeperDiagnostics KeeperDiagnostics { get; set; }
        public string? ErrorMessage { get; set; }
        
        public override string ToString()
        {
            var status = IsFullyOperational ? "READY" : "NOT_READY";
            return $"KeeperSystem: {status} (Entities: {EntityCount})";
        }
    }
}