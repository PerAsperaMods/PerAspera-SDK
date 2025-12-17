#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Direct wrapper around native KeeperMap class
    /// Provides raw Handle→Object mapping with IL2CPP safety
    /// DOC: F:\ModPeraspera\Internal_doc\ARCHITECTURE\Handle-System-Architecture.md
    /// </summary>
    public class KeeperMapWrapper : WrapperBase
    {
        private static readonly string LogPrefix = "[KeeperMapWrapper]";
        
        /// <summary>
        /// Initialize KeeperMapWrapper with native KeeperMap instance
        /// </summary>
        /// <param name="nativeKeeperMap">Native KeeperMap from BaseGame.keeper.map</param>
        public KeeperMapWrapper(object nativeKeeperMap) : base(nativeKeeperMap)
        {
        }
        
        /// <summary>
        /// Get current KeeperMap from BaseGame.keeper.map
        /// Factory method for wrapper creation
        /// </summary>
        public static KeeperMapWrapper? GetCurrent()
        {
            try
            {
                var baseGame = BaseGame.GetCurrent();
                if (baseGame == null) return null;
                
                var keeper = baseGame.GetKeeper();
                if (keeper == null) return null;
                
                var keeperMap = keeper.GetFieldValue<object>("map");
                if (keeperMap == null) return null;
                
                return new KeeperMapWrapper(keeperMap);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"{LogPrefix} GetCurrent failed: {ex.Message}");
                return null;
            }
        }
        
        // ==================== CORE KEEPERMAP METHODS ====================
        
        /// <summary>
        /// Direct native Find<T> method access
        /// Raw performance for specialized use cases
        /// Performance: O(1) Dictionary lookup
        /// </summary>
        /// <typeparam name="T">Expected entity type</typeparam>
        /// <param name="handle">Entity Handle</param>
        /// <returns>Typed entity instance or null if not found</returns>
        public T? Find<T>(object handle) where T : class
        {
            try
            {
                if (handle == null || NativeObject == null) return null;
                
                return SafeInvoke<T>("Find", handle);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"{LogPrefix} Find<{typeof(T).Name}> failed: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Direct native Contains method access
        /// Handle existence validation
        /// Performance: O(1) Dictionary.ContainsKey
        /// </summary>
        /// <param name="handle">Handle to check</param>
        /// <returns>True if Handle exists in KeeperMap</returns>
        public bool Contains(object handle)
        {
            try
            {
                if (handle == null || NativeObject == null) return false;
                
                return SafeInvoke<bool>("Contains", handle);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.Log($"{LogPrefix} Contains failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Non-generic Find method for base IHandleable access
        /// Returns base interface type for unknown entity types
        /// </summary>
        /// <param name="handle">Entity Handle</param>
        /// <returns>IHandleable instance or null</returns>
        public object? FindBase(object handle)
        {
            try
            {
                if (handle == null || NativeObject == null) return null;
                
                return SafeInvoke<object>("Find", handle);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"{LogPrefix} FindBase failed: {ex.Message}");
                return null;
            }
        }
        
        // ==================== ADVANCED OPERATIONS ====================
        
        /// <summary>
        /// Access to internal _objects Dictionary for advanced operations
        /// Use with caution - requires IL2CPP knowledge
        /// Returns: Dictionary<Handle, IHandleable> equivalent
        /// </summary>
        public object? GetObjectsDict()
        {
            try
            {
                if (NativeObject == null) return null;
                
                return NativeObject.GetFieldValue<object>("_objects");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"{LogPrefix} GetObjectsDict failed: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get Keeper instance that owns this KeeperMap
        /// Back-reference to parent Keeper
        /// </summary>
        public object? GetKeeper()
        {
            try
            {
                if (NativeObject == null) return null;
                
                return NativeObject.GetFieldValue<object>("_objects");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"{LogPrefix} GetKeeper failed: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get total entity count in KeeperMap
        /// Diagnostic method for monitoring
        /// </summary>
        public int GetEntityCount()
        {
            try
            {
                var objectsDict = GetObjectsDict();
                if (objectsDict == null) return 0;
                
                var count = objectsDict.GetPropertyValue<object>("Count");
                return count is int intCount ? intCount : 0;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"{LogPrefix} GetEntityCount failed: {ex.Message}");
                return 0;
            }
        }
        
        // ==================== BULK ENUMERATION ====================
        
        /// <summary>
        /// Enumerate all Handles in KeeperMap
        /// Performance: O(n) where n = total entities
        /// Use for bulk operations or complete system analysis
        /// </summary>
        public IEnumerable<object> EnumerateHandles()
        {
            var objectsDict = GetObjectsDict();
            if (objectsDict == null) yield break;
            
            try
            {
                // Get Dictionary.Keys collection
                var keys = objectsDict.InvokeMethod<object>("get_Keys");
                if (keys == null) yield break;
                
                var enumerator = keys.InvokeMethod<object>("GetEnumerator");
                if (enumerator == null) yield break;
                
                // Enumerate all Handles
                while (enumerator.InvokeMethod<bool>("MoveNext"))
                {
                    var current = enumerator.GetPropertyValue<object>("Current");
                    if (current != null)
                    {
                        yield return current;
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"{LogPrefix} EnumerateHandles failed: {ex.Message}");
                yield break;
            }
        }
        
        /// <summary>
        /// Enumerate all entities in KeeperMap
        /// Performance: O(n) where n = total entities
        /// Returns raw IHandleable objects
        /// </summary>
        public IEnumerable<object> EnumerateEntities()
        {
            var objectsDict = GetObjectsDict();
            if (objectsDict == null) yield break;
            
            try
            {
                // Get Dictionary.Values collection
                var values = objectsDict.InvokeMethod<object>("get_Values");
                if (values == null) yield break;
                
                var enumerator = values.InvokeMethod<object>("GetEnumerator");
                if (enumerator == null) yield break;
                
                // Enumerate all entities
                while (enumerator.InvokeMethod<bool>("MoveNext"))
                {
                    var current = enumerator.GetPropertyValue("Current");
                    if (current != null)
                    {
                        yield return current;
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"{LogPrefix} EnumerateEntities failed: {ex.Message}");
                yield break;
            }
        }
        
        /// <summary>
        /// Enumerate entities filtered by type predicate
        /// Performance: O(n) with type checking overhead
        /// Flexible filtering for specialized queries
        /// </summary>
        /// <param name="typePredicate">Function to test entity type</param>
        public IEnumerable<object> EnumerateEntitiesByType(Func<object, bool> typePredicate)
        {
            if (typePredicate == null) yield break;
            
            foreach (var entity in EnumerateEntities())
            {
                try
                {
                    if (typePredicate(entity))
                    {
                        yield return entity;
                    }
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.Log($"{LogPrefix} Type predicate failed for entity: {ex.Message}");
                    // Continue enumeration despite individual failures
                }
            }
        }
        
        // ==================== DIAGNOSTICS ====================
        
        /// <summary>
        /// Get detailed KeeperMap diagnostics
        /// Useful for debugging and performance monitoring
        /// </summary>
        public KeeperMapDiagnostics GetDiagnostics()
        {
            try
            {
                var diagnostics = new KeeperMapDiagnostics
                {
                    IsAvailable = NativeObject != null,
                    EntityCount = GetEntityCount(),
                    HasKeeper = GetKeeper() != null,
                    HasObjectsDict = GetObjectsDict() != null
                };
                
                if (diagnostics.IsAvailable && diagnostics.HasObjectsDict)
                {
                    // Count entities by type (sample analysis)
                    var typeGroups = EnumerateEntities()
                        .Take(100) // Limit sample for performance
                        .GroupBy(e => e?.GetType().Name ?? "Unknown")
                        .ToDictionary(g => g.Key, g => g.Count());
                        
                    diagnostics.SampleTypeCounts = typeGroups;
                }
                
                return diagnostics;
            }
            catch (Exception ex)
            {
                return new KeeperMapDiagnostics
                {
                    IsAvailable = false,
                    ErrorMessage = ex.Message
                };
            }
        }
        
        /// <summary>
        /// Test KeeperMap functionality with known Handle
        /// Validates that Find/Contains operations work correctly
        /// </summary>
        public bool TestFunctionality()
        {
            try
            {
                if (!GetDiagnostics().IsAvailable) return false;
                
                // Test with first available Handle
                var firstHandle = EnumerateHandles().FirstOrDefault();
                if (firstHandle == null) return false;
                
                // Test Contains operation
                if (!Contains(firstHandle)) return false;
                
                // Test Find operation
                var entity = FindBase(firstHandle);
                return entity != null;
            }
            catch
            {
                return false;
            }
        }
    }
    
    /// <summary>
    /// KeeperMap diagnostic information
    /// Used for monitoring and debugging Handle system health
    /// </summary>
    public struct KeeperMapDiagnostics
    {
        public bool IsAvailable { get; set; }
        public int EntityCount { get; set; }
        public bool HasKeeper { get; set; }
        public bool HasObjectsDict { get; set; }
        public Dictionary<string, int>? SampleTypeCounts { get; set; }
        public string? ErrorMessage { get; set; }
        
        public override string ToString()
        {
            if (!IsAvailable)
                return $"KeeperMap: UNAVAILABLE ({ErrorMessage})";
            
            var types = SampleTypeCounts?.Count ?? 0;
            return $"KeeperMap: {EntityCount} entities, {types} types sampled, " +
                   $"Keeper: {HasKeeper}, Dict: {HasObjectsDict}";
        }
    }
}