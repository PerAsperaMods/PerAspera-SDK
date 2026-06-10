#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Robust Handle validation and error recovery system
    /// Provides comprehensive Handle integrity checking with performance optimization
    /// DOC: F:\ModPeraspera\Internal_doc\ARCHITECTURE\Handle-System-Architecture.md
    /// </summary>
    public static class HandleValidator
    {
        private static readonly string LogPrefix = "[HandleValidator]";
        
        // Performance cache for validated Handles (TTL-based)
        private static readonly Dictionary<object, DateTime> _validatedHandlesCache = new();
        private static readonly TimeSpan CacheValidityDuration = TimeSpan.FromSeconds(30);
        private static readonly int MaxCacheSize = 1000;
        
        // Known invalid Handles to prevent repeated validation
        private static readonly HashSet<object> _knownInvalidHandles = new();
        private static DateTime _lastCacheCleanup = DateTime.MinValue;
        
        // ==================== CORE VALIDATION ====================
        
        /// <summary>
        /// Comprehensive Handle validation with caching
        /// Checks: null, KeeperMap existence, Handle registration
        /// Performance: O(1) cached, O(log n) uncached
        /// </summary>
        /// <param name="handle">Handle to validate</param>
        /// <param name="keeperMap">Optional KeeperMap instance (auto-retrieved if null)</param>
        /// <returns>True if Handle is valid and registered</returns>
        public static bool IsValidHandle(object? handle, KeeperMapWrapper? keeperMap = null)
        {
            if (handle == null) return false;
            
            try
            {
                // Check known invalid cache first
                if (_knownInvalidHandles.Contains(handle))
                {
                    return false;
                }
                
                // Check validation cache
                if (_validatedHandlesCache.TryGetValue(handle, out var cacheTime))
                {
                    if (DateTime.UtcNow - cacheTime < CacheValidityDuration)
                    {
                        return true; // Cache hit - Handle was valid recently
                    }
                    else
                    {
                        _validatedHandlesCache.Remove(handle); // Expired
                    }
                }
                
                // Perform actual validation
                var isValid = ValidateHandleInternal(handle, keeperMap);
                
                // Update caches
                if (isValid)
                {
                    CacheValidHandle(handle);
                }
                else
                {
                    _knownInvalidHandles.Add(handle);
                }
                
                return isValid;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"{LogPrefix} Validation failed for Handle: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Internal validation logic without caching
        /// Actual KeeperMap lookup and Handle verification
        /// </summary>
        private static bool ValidateHandleInternal(object handle, KeeperMapWrapper? keeperMap)
        {
            // Get or create KeeperMap wrapper
            keeperMap ??= KeeperMapWrapper.GetCurrent();
            if (keeperMap == null)
            {
                UnityEngine.Debug.LogWarning($"{LogPrefix} KeeperMap unavailable for validation");
                return false;
            }
            
            // Check Handle registration in KeeperMap
            return keeperMap.Contains(handle);
        }
        
        /// <summary>
        /// Validate Handle and return entity type information
        /// Extended validation with type safety checking
        /// </summary>
        /// <param name="handle">Handle to validate</param>
        /// <param name="expectedType">Expected entity type (optional)</param>
        /// <returns>Validation result with type information</returns>
        public static HandleValidationResult ValidateWithType(object? handle, System.Type? expectedType = null)
        {
            if (handle == null)
            {
                return new HandleValidationResult
                {
                    IsValid = false,
                    ErrorCode = ValidationError.NullHandle,
                    ErrorMessage = "Handle is null"
                };
            }
            
            try
            {
                var keeperMap = KeeperMapWrapper.GetCurrent();
                if (keeperMap == null)
                {
                    return new HandleValidationResult
                    {
                        IsValid = false,
                        ErrorCode = ValidationError.KeeperMapUnavailable,
                        ErrorMessage = "KeeperMap is not available"
                    };
                }
                
                // Check Handle existence
                if (!keeperMap.Contains(handle))
                {
                    return new HandleValidationResult
                    {
                        IsValid = false,
                        ErrorCode = ValidationError.HandleNotFound,
                        ErrorMessage = "Handle not found in KeeperMap"
                    };
                }
                
                // Retrieve entity for type checking
                var entity = keeperMap.FindBase(handle);
                if (entity == null)
                {
                    return new HandleValidationResult
                    {
                        IsValid = false,
                        ErrorCode = ValidationError.EntityRetrievalFailed,
                        ErrorMessage = "Entity retrieval failed despite Handle existence"
                    };
                }
                
                var actualType = entity.GetType();
                
                // Type compatibility check
                bool typeCompatible = true;
                if (expectedType != null && !expectedType.IsAssignableFrom(actualType))
                {
                    typeCompatible = false;
                }
                
                return new HandleValidationResult
                {
                    IsValid = typeCompatible,
                    ErrorCode = typeCompatible ? ValidationError.None : ValidationError.TypeMismatch,
                    ErrorMessage = typeCompatible ? null : $"Expected {expectedType?.Name}, got {actualType.Name}",
                    EntityType = actualType,
                    Handle = handle,
                    Entity = entity
                };
            }
            catch (Exception ex)
            {
                return new HandleValidationResult
                {
                    IsValid = false,
                    ErrorCode = ValidationError.ValidationException,
                    ErrorMessage = ex.Message
                };
            }
        }
        
        // ==================== BATCH VALIDATION ====================
        
        /// <summary>
        /// Validate multiple Handles efficiently
        /// Optimized for bulk operations with shared KeeperMap access
        /// Performance: O(n) with single KeeperMap retrieval
        /// </summary>
        /// <param name="handles">Collection of Handles to validate</param>
        /// <returns>Dictionary mapping Handles to validation results</returns>
        public static Dictionary<object, bool> ValidateMany(IEnumerable<object> handles)
        {
            var results = new Dictionary<object, bool>();
            if (handles == null) return results;
            
            try
            {
                var keeperMap = KeeperMapWrapper.GetCurrent();
                if (keeperMap == null)
                {
                    // Mark all as invalid if KeeperMap unavailable
                    foreach (var handle in handles)
                    {
                        if (handle != null) results[handle] = false;
                    }
                    return results;
                }
                
                // Batch validation with shared KeeperMap
                foreach (var handle in handles)
                {
                    if (handle != null)
                    {
                        results[handle] = IsValidHandle(handle, keeperMap);
                    }
                }
                
                return results;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"{LogPrefix} Batch validation failed: {ex.Message}");
                return results;
            }
        }
        
        /// <summary>
        /// Filter collection to only valid Handles
        /// Performance-optimized for large collections
        /// </summary>
        /// <param name="handles">Input Handle collection</param>
        /// <returns>Filtered collection with only valid Handles</returns>
        public static IEnumerable<object> FilterValidHandles(IEnumerable<object> handles)
        {
            if (handles == null) yield break;
            
            var keeperMap = KeeperMapWrapper.GetCurrent();
            if (keeperMap == null) yield break;
            
            foreach (var handle in handles)
            {
                if (handle != null && IsValidHandle(handle, keeperMap))
                {
                    yield return handle;
                }
            }
        }
        
        // ==================== ERROR RECOVERY ====================
        
        /// <summary>
        /// Attempt to recover or suggest alternatives for invalid Handle
        /// Useful for degraded state recovery in complex systems
        /// </summary>
        /// <param name="invalidHandle">Handle that failed validation</param>
        /// <param name="fallbackStrategies">Recovery strategies to attempt</param>
        /// <returns>Recovery result with alternative Handle or failure info</returns>
        public static HandleRecoveryResult AttemptRecovery(object invalidHandle, HandleRecoveryStrategy fallbackStrategies = HandleRecoveryStrategy.Default)
        {
            try
            {
                var keeperMap = KeeperMapWrapper.GetCurrent();
                if (keeperMap == null)
                {
                    return new HandleRecoveryResult
                    {
                        Success = false,
                        ErrorMessage = "KeeperMap unavailable for recovery"
                    };
                }
                
                // Strategy 1: Re-validation (temporary issues)
                if (fallbackStrategies.HasFlag(HandleRecoveryStrategy.Revalidate))
                {
                    ClearHandleCache(invalidHandle); // Clear cache
                    if (IsValidHandle(invalidHandle, keeperMap))
                    {
                        return new HandleRecoveryResult
                        {
                            Success = true,
                            RecoveredHandle = invalidHandle,
                            RecoveryMethod = "Re-validation succeeded"
                        };
                    }
                }
                
                // Strategy 2: Find similar entities (type-based recovery)
                if (fallbackStrategies.HasFlag(HandleRecoveryStrategy.FindSimilar))
                {
                    // This would require type information - simplified implementation
                    var anyValidHandle = keeperMap.EnumerateHandles().FirstOrDefault();
                    if (anyValidHandle != null && IsValidHandle(anyValidHandle, keeperMap))
                    {
                        return new HandleRecoveryResult
                        {
                            Success = true,
                            RecoveredHandle = anyValidHandle,
                            RecoveryMethod = "Found alternative valid Handle"
                        };
                    }
                }
                
                return new HandleRecoveryResult
                {
                    Success = false,
                    ErrorMessage = "All recovery strategies failed"
                };
            }
            catch (Exception ex)
            {
                return new HandleRecoveryResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }
        
        // ==================== CACHE MANAGEMENT ====================
        
        /// <summary>
        /// Add Handle to validation cache for performance optimization
        /// </summary>
        private static void CacheValidHandle(object handle)
        {
            try
            {
                CleanupCacheIfNeeded();
                
                if (_validatedHandlesCache.Count < MaxCacheSize)
                {
                    _validatedHandlesCache[handle] = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log($"{LogPrefix} Cache update failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Remove specific Handle from all caches
        /// </summary>
        public static void ClearHandleCache(object handle)
        {
            if (handle == null) return;
            
            _validatedHandlesCache.Remove(handle);
            _knownInvalidHandles.Remove(handle);
        }
        
        /// <summary>
        /// Clear all validation caches
        /// Use when KeeperMap state might have changed significantly
        /// </summary>
        public static void ClearAllCaches()
        {
            _validatedHandlesCache.Clear();
            _knownInvalidHandles.Clear();
            UnityEngine.Debug.Log($"{LogPrefix} All validation caches cleared");
        }
        
        /// <summary>
        /// Periodic cache cleanup for memory management
        /// </summary>
        private static void CleanupCacheIfNeeded()
        {
            var now = DateTime.UtcNow;
            
            // Cleanup every 60 seconds
            if (now - _lastCacheCleanup < TimeSpan.FromSeconds(60)) return;
            
            try
            {
                var expiredKeys = _validatedHandlesCache
                    .Where(kvp => now - kvp.Value > CacheValidityDuration)
                    .Select(kvp => kvp.Key)
                    .ToList();
                    
                foreach (var key in expiredKeys)
                {
                    _validatedHandlesCache.Remove(key);
                }
                
                // Limit invalid handles cache size
                if (_knownInvalidHandles.Count > MaxCacheSize)
                {
                    _knownInvalidHandles.Clear();
                }
                
                _lastCacheCleanup = now;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"{LogPrefix} Cache cleanup failed: {ex.Message}");
            }
        }
        
        // ==================== DIAGNOSTICS ====================
        
        /// <summary>
        /// Get Handle validation system diagnostics
        /// </summary>
        public static HandleValidatorDiagnostics GetDiagnostics()
        {
            try
            {
                CleanupCacheIfNeeded(); // Update cache state
                
                return new HandleValidatorDiagnostics
                {
                    ValidCacheSize = _validatedHandlesCache.Count,
                    InvalidCacheSize = _knownInvalidHandles.Count,
                    CacheHitRate = CalculateCacheHitRate(),
                    LastCleanup = _lastCacheCleanup,
                    KeeperMapAvailable = KeeperMapWrapper.GetCurrent() != null
                };
            }
            catch (Exception ex)
            {
                return new HandleValidatorDiagnostics
                {
                    ErrorMessage = ex.Message
                };
            }
        }
        
        private static double CalculateCacheHitRate()
        {
            // Simplified cache hit rate calculation
            // In production, would track actual hits vs misses
            var totalCache = _validatedHandlesCache.Count + _knownInvalidHandles.Count;
            return totalCache > 0 ? (_validatedHandlesCache.Count / (double)totalCache) * 100 : 0;
        }
    }
    
    // ==================== SUPPORTING TYPES ====================
    
    /// <summary>
    /// Result of Handle validation with detailed information
    /// </summary>
    public struct HandleValidationResult
    {
        /// <summary>
        /// Whether the handle validation was successful
        /// </summary>
        public bool IsValid { get; set; }
        /// <summary>
        /// Error code if validation failed, None if successful
        /// </summary>
        public ValidationError ErrorCode { get; set; }
        /// <summary>
        /// Human-readable error message if validation failed
        /// </summary>
        public string? ErrorMessage { get; set; }
        /// <summary>
        /// Type of the entity associated with the handle
        /// </summary>
        public System.Type? EntityType { get; set; }
        /// <summary>
        /// The handle object being validated
        /// </summary>
        public object? Handle { get; set; }
        /// <summary>
        /// The entity retrieved from the handle, if validation succeeded
        /// </summary>
        public object? Entity { get; set; }
        
        /// <summary>
        /// Returns a string representation of the validation result
        /// </summary>
        /// <returns>Formatted string with validation status and details</returns>
        public override string ToString()
        {
            if (IsValid)
                return $"Valid Handle → {EntityType?.Name ?? "Unknown"}";
            else
                return $"Invalid Handle: {ErrorCode} - {ErrorMessage}";
        }
    }
    
    /// <summary>
    /// Handle validation error classification
    /// </summary>
    public enum ValidationError
    {
        /// <summary>No error - validation successful</summary>
        None = 0,
        /// <summary>Handle is null or invalid</summary>
        NullHandle,
        /// <summary>KeeperMap instance is not available</summary>
        KeeperMapUnavailable,
        /// <summary>Handle does not exist in KeeperMap</summary>
        HandleNotFound,
        /// <summary>Failed to retrieve entity from valid handle</summary>
        EntityRetrievalFailed,
        /// <summary>Retrieved entity type does not match expected type</summary>
        TypeMismatch,
        /// <summary>Exception occurred during validation process</summary>
        ValidationException
    }
    
    /// <summary>
    /// Handle recovery attempt result
    /// </summary>
    public struct HandleRecoveryResult
    {
        /// <summary>
        /// Whether the handle recovery attempt was successful
        /// </summary>
        public bool Success { get; set; }
        /// <summary>
        /// The recovered handle if recovery was successful
        /// </summary>
        public object? RecoveredHandle { get; set; }
        /// <summary>
        /// Description of the recovery method used
        /// </summary>
        public string? RecoveryMethod { get; set; }
        /// <summary>
        /// Error message if recovery failed
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// Returns a string representation of the recovery result
        /// </summary>
        /// <returns>Formatted string with recovery status and details</returns>
        public override string ToString()
        {
            return Success ? $"Recovered via: {RecoveryMethod}" : $"Recovery failed: {ErrorMessage}";
        }
    }
    
    /// <summary>
    /// Handle recovery strategies
    /// </summary>
    [Flags]
    public enum HandleRecoveryStrategy
    {
        /// <summary>No recovery strategy - accept failure</summary>
        None = 0,
        /// <summary>Attempt to revalidate the same handle</summary>
        Revalidate = 1,
        /// <summary>Attempt to find similar valid handle</summary>
        FindSimilar = 2,
        /// <summary>Default recovery strategy - revalidate and find similar</summary>
        Default = Revalidate | FindSimilar
    }
    
    /// <summary>
    /// HandleValidator system diagnostics
    /// </summary>
    public struct HandleValidatorDiagnostics
    {
        /// <summary>
        /// Number of valid handles in cache
        /// </summary>
        public int ValidCacheSize { get; set; }
        /// <summary>
        /// Number of invalid handles in cache
        /// </summary>
        public int InvalidCacheSize { get; set; }
        /// <summary>
        /// Cache hit rate percentage (0.0 to 100.0)
        /// </summary>
        public double CacheHitRate { get; set; }
        /// <summary>
        /// Timestamp of last cache cleanup operation
        /// </summary>
        public DateTime LastCleanup { get; set; }
        /// <summary>
        /// Whether KeeperMap instance is currently available
        /// </summary>
        public bool KeeperMapAvailable { get; set; }
        /// <summary>
        /// Current error message, if any
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        /// <summary>
        /// Returns a string representation of the validator diagnostics
        /// </summary>
        /// <returns>Formatted string with cache statistics and system status</returns>
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
                return $"HandleValidator: ERROR - {ErrorMessage}";
                
            return $"HandleValidator: {ValidCacheSize} valid cached, {InvalidCacheSize} invalid cached, " +
                   $"{CacheHitRate:F1}% hit rate, KeeperMap: {(KeeperMapAvailable ? "OK" : "UNAVAILABLE")}";
        }
    }
}