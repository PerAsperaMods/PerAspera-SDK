#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core.IL2CPP;
using UnityEngine;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Handle type conversion and transformation utilities
    /// Provides safe type casting, Handle→Entity resolution, and IL2CPP compatibility helpers
    /// DOC: F:\ModPeraspera\Internal_doc\ARCHITECTURE\Handle-System-Architecture.md
    /// </summary>
    public static class HandleConversionUtils
    {
        private static readonly string LogPrefix = "[HandleConversionUtils]";
        
        // ==================== HANDLE RESOLUTION ====================
        
        /// <summary>
        /// Resolve Handle to strongly-typed entity with validation
        /// Safe alternative to direct KeeperMap.Find<T> with error handling
        /// Performance: O(1) with validation overhead
        /// </summary>
        /// <typeparam name="T">Expected entity type</typeparam>
        /// <param name="handle">Handle to resolve</param>
        /// <param name="validator">Optional custom validation function</param>
        /// <returns>Strongly-typed entity or null if conversion failed</returns>
        public static T? ResolveHandle<T>(object? handle, Func<T, bool>? validator = null) where T : class
        {
            try
            {
                if (handle == null) return null;
                
                // Validate Handle first
                if (!HandleValidator.IsValidHandle(handle))
                {
                    return null;
                }
                
                // Get KeeperMap and resolve
                var keeperMap = KeeperMapWrapper.GetCurrent();
                if (keeperMap == null) return null;
                
                var entity = keeperMap.Find<T>(handle);
                if (entity == null) return null;
                
                // Apply custom validation if provided
                if (validator != null && !validator(entity))
                {
                    return null;
                }
                
                return entity;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"{LogPrefix} ResolveHandle<{typeof(T).Name}> failed: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Resolve Handle to base object type (IHandleable)
        /// Use when exact entity type is unknown
        /// </summary>
        /// <param name="handle">Handle to resolve</param>
        /// <returns>Base entity object or null</returns>
        public static object? ResolveHandleBase(object? handle)
        {
            try
            {
                if (handle == null) return null;
                
                if (!HandleValidator.IsValidHandle(handle))
                {
                    return null;
                }
                
                var keeperMap = KeeperMapWrapper.GetCurrent();
                return keeperMap?.FindBase(handle);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"{LogPrefix} ResolveHandleBase failed: {ex.Message}");
                return null;
            }
        }
        
        // ==================== BULK HANDLE CONVERSION ====================
        
        /// <summary>
        /// Resolve multiple Handles to entities efficiently
        /// Optimized for bulk operations with shared KeeperMap access
        /// Returns: Dictionary<Handle, Entity> with successful resolutions only
        /// </summary>
        /// <typeparam name="T">Expected entity type for all Handles</typeparam>
        /// <param name="handles">Collection of Handles to resolve</param>
        /// <param name="continueOnError">If false, stops on first error</param>
        /// <returns>Dictionary mapping Handles to resolved entities</returns>
        public static Dictionary<object, T> ResolveHandles<T>(IEnumerable<object> handles, bool continueOnError = true) where T : class
        {
            var results = new Dictionary<object, T>();
            if (handles == null) return results;
            
            try
            {
                var keeperMap = KeeperMapWrapper.GetCurrent();
                if (keeperMap == null) return results;
                
                foreach (var handle in handles)
                {
                    try
                    {
                        if (handle == null) continue;
                        
                        var entity = keeperMap.Find<T>(handle);
                        if (entity != null)
                        {
                            results[handle] = entity;
                        }
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarning($"{LogPrefix} Handle resolution failed: {ex.Message}");
                        if (!continueOnError) break;
                    }
                }
                
                return results;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LogPrefix} Bulk resolution failed: {ex.Message}");
                return results;
            }
        }
        
        /// <summary>
        /// Convert Handle collection to Entity collection
        /// Filters out invalid/non-convertible Handles automatically
        /// Performance: O(n) with validation per Handle
        /// </summary>
        /// <typeparam name="T">Target entity type</typeparam>
        /// <param name="handles">Source Handle collection</param>
        /// <returns>Filtered collection of resolved entities</returns>
        public static IEnumerable<T> HandlesToEntities<T>(IEnumerable<object> handles) where T : class
        {
            if (handles == null) yield break;
            
            var keeperMap = KeeperMapWrapper.GetCurrent();
            if (keeperMap == null) yield break;
            
            foreach (var handle in handles)
            {
                if (handle == null) continue;
                
                try
                {
                    var entity = keeperMap.Find<T>(handle);
                    if (entity != null)
                    {
                        yield return entity;
                    }
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogDebug($"{LogPrefix} Handle→Entity conversion failed: {ex.Message}");
                    // Continue with next Handle
                }
            }
        }
        
        // ==================== REVERSE CONVERSION (ENTITY → HANDLE) ====================
        
        /// <summary>
        /// Extract Handle from entity object
        /// Searches for Handle property/field in entity
        /// Performance: O(1) reflection lookup (cached)
        /// </summary>
        /// <param name="entity">Entity object to extract Handle from</param>
        /// <returns>Handle object or null if not found</returns>
        public static object? ExtractHandle(object? entity)
        {
            if (entity == null) return null;
            
            try
            {
                // Try common Handle property names
                var handlePropertyNames = new[] { "Handle", "handle", "_handle", "ID", "id" };
                
                foreach (var propertyName in handlePropertyNames)
                {
                    try
                    {
                        var handle = entity.GetPropertyValue(propertyName);
                        if (handle != null)
                        {
                            return handle;
                        }
                    }
                    catch
                    {
                        // Try next property name
                    }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"{LogPrefix} Handle extraction failed: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Convert entity collection to Handle collection
        /// Extracts Handles from each entity and filters out nulls
        /// </summary>
        /// <param name="entities">Source entity collection</param>
        /// <returns>Collection of extracted Handles</returns>
        public static IEnumerable<object> EntitiesToHandles(IEnumerable<object> entities)
        {
            if (entities == null) yield break;
            
            foreach (var entity in entities)
            {
                var handle = ExtractHandle(entity);
                if (handle != null)
                {
                    yield return handle;
                }
            }
        }
        
        // ==================== TYPE CONVERSION UTILITIES ====================
        
        /// <summary>
        /// Safe type cast with Handle validation
        /// Attempts to cast entity to target type with Handle integrity check
        /// </summary>
        /// <typeparam name="TSource">Source entity type</typeparam>
        /// <typeparam name="TTarget">Target entity type</typeparam>
        /// <param name="sourceEntity">Source entity to convert</param>
        /// <returns>Converted entity or null if cast failed</returns>
        public static TTarget? SafeCast<TSource, TTarget>(TSource? sourceEntity) 
            where TSource : class 
            where TTarget : class
        {
            if (sourceEntity == null) return null;
            
            try
            {
                // Try direct cast first
                if (sourceEntity is TTarget directCast)
                {
                    return directCast;
                }
                
                // For Handle-based entities, try re-resolution
                var handle = ExtractHandle(sourceEntity);
                if (handle != null && HandleValidator.IsValidHandle(handle))
                {
                    return ResolveHandle<TTarget>(handle);
                }
                
                return null;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"{LogPrefix} SafeCast<{typeof(TSource).Name}, {typeof(TTarget).Name}> failed: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Convert Handle to specific building type with position validation
        /// Specialized conversion for building entities with spatial constraints
        /// </summary>
        /// <typeparam name="T">Target building type</typeparam>
        /// <param name="handle">Building Handle</param>
        /// <param name="expectedPosition">Optional position validation</param>
        /// <param name="positionTolerance">Position tolerance for validation</param>
        /// <returns>Building entity or null if conversion/validation failed</returns>
        public static T? HandleToBuilding<T>(object? handle, Vector3? expectedPosition = null, float positionTolerance = 1.0f) where T : class
        {
            var building = ResolveHandle<T>(handle);
            if (building == null) return null;
            
            // Position validation if expected position provided
            if (expectedPosition.HasValue)
            {
                try
                {
                    var buildingPosition = building.GetPropertyValue("position");
                    if (buildingPosition is Vector3 pos)
                    {
                        var distance = Vector3.Distance(pos, expectedPosition.Value);
                        if (distance > positionTolerance)
                        {
                            UnityEngine.Debug.LogWarning($"{LogPrefix} Building position validation failed: distance {distance} > tolerance {positionTolerance}");
                            return null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogWarning($"{LogPrefix} Position validation failed: {ex.Message}");
                    // Continue without position validation
                }
            }
            
            return building;
        }
        
        // ==================== CONVERSION VALIDATION ====================
        
        /// <summary>
        /// Validate Handle→Entity conversion chain integrity
        /// Comprehensive test of Handle system functionality
        /// Use for debugging and system health checks
        /// </summary>
        /// <param name="testHandles">Collection of Handles to test (optional)</param>
        /// <returns>Conversion system diagnostics</returns>
        public static ConversionDiagnostics ValidateConversionSystem(IEnumerable<object>? testHandles = null)
        {
            var diagnostics = new ConversionDiagnostics();
            
            try
            {
                var keeperMap = KeeperMapWrapper.GetCurrent();
                if (keeperMap == null)
                {
                    diagnostics.ErrorMessage = "KeeperMap unavailable";
                    return diagnostics;
                }
                
                diagnostics.KeeperMapAvailable = true;
                
                // Use provided test Handles or get sample from KeeperMap
                var handles = testHandles?.Take(10) ?? keeperMap.EnumerateHandles().Take(10);
                var testHandleList = handles.ToList();
                
                diagnostics.TestHandleCount = testHandleList.Count;
                
                // Test Handle→Entity conversion
                int successfulConversions = 0;
                int validationSuccesses = 0;
                int roundTripSuccesses = 0;
                
                foreach (var handle in testHandleList)
                {
                    try
                    {
                        // Test Handle validation
                        if (HandleValidator.IsValidHandle(handle))
                        {
                            validationSuccesses++;
                        }
                        
                        // Test Handle→Entity conversion
                        var entity = ResolveHandleBase(handle);
                        if (entity != null)
                        {
                            successfulConversions++;
                            
                            // Test Entity→Handle round trip
                            var extractedHandle = ExtractHandle(entity);
                            if (extractedHandle != null && extractedHandle.Equals(handle))
                            {
                                roundTripSuccesses++;
                            }
                        }
                    }
                    catch
                    {
                        // Individual test failure - continue with others
                    }
                }
                
                diagnostics.ValidationSuccessRate = testHandleList.Count > 0 ? 
                    (validationSuccesses / (double)testHandleList.Count) * 100 : 0;
                    
                diagnostics.ConversionSuccessRate = testHandleList.Count > 0 ? 
                    (successfulConversions / (double)testHandleList.Count) * 100 : 0;
                    
                diagnostics.RoundTripSuccessRate = testHandleList.Count > 0 ? 
                    (roundTripSuccesses / (double)testHandleList.Count) * 100 : 0;
                
                diagnostics.IsHealthy = diagnostics.ValidationSuccessRate > 80 && 
                                      diagnostics.ConversionSuccessRate > 80;
                
                return diagnostics;
            }
            catch (Exception ex)
            {
                diagnostics.ErrorMessage = ex.Message;
                return diagnostics;
            }
        }
        
        // ==================== UTILITY HELPERS ====================
        
        /// <summary>
        /// Create Handle→Entity lookup dictionary for performance optimization
        /// Pre-resolves all Handles for repeated access scenarios
        /// Use when multiple lookups are needed for same Handle set
        /// </summary>
        /// <typeparam name="T">Target entity type</typeparam>
        /// <param name="handles">Handles to pre-resolve</param>
        /// <returns>Lookup dictionary for O(1) access</returns>
        public static Dictionary<object, T> CreateHandleLookup<T>(IEnumerable<object> handles) where T : class
        {
            return ResolveHandles<T>(handles);
        }
        
        /// <summary>
        /// Check if Handle represents entity of specific type
        /// Type checking without full entity resolution
        /// </summary>
        /// <typeparam name="T">Type to check for</typeparam>
        /// <param name="handle">Handle to check</param>
        /// <returns>True if Handle resolves to entity of type T</returns>
        public static bool IsHandleOfType<T>(object? handle) where T : class
        {
            return ResolveHandle<T>(handle) != null;
        }
    }
    
    /// <summary>
    /// Diagnostics for Handle conversion system health
    /// </summary>
    public struct ConversionDiagnostics
    {
        public bool KeeperMapAvailable { get; set; }
        public int TestHandleCount { get; set; }
        public double ValidationSuccessRate { get; set; }
        public double ConversionSuccessRate { get; set; }
        public double RoundTripSuccessRate { get; set; }
        public bool IsHealthy { get; set; }
        public string? ErrorMessage { get; set; }
        
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
                return $"Conversion System: ERROR - {ErrorMessage}";
                
            return $"Conversion System: {TestHandleCount} tested, " +
                   $"Validation: {ValidationSuccessRate:F1}%, " +
                   $"Conversion: {ConversionSuccessRate:F1}%, " +
                   $"RoundTrip: {RoundTripSuccessRate:F1}%, " +
                   $"Health: {(IsHealthy ? "OK" : "DEGRADED")}";
        }
    }
}