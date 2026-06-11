#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core.IL2CPP;


#pragma warning disable CS1591
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
        /// <summary>Get the Keeper that owns this map.</summary>
        public KeeperWrapper? GetKeeper()
        {
            if (NativeObject is not KeeperMap map) return null;
            var keeper = map._keeper;
            return keeper != null ? new KeeperWrapper(keeper) : null;
        }
        /// <summary>
        /// Get current KeeperMap from BaseGame.keeper.map
        /// Factory method for wrapper creation
        /// </summary>
        public static KeeperMapWrapper? GetCurrent()
        {
            try
            {
                var baseGame = BaseGameWrapper.GetCurrent();
                if (baseGame == null) return null;
                
                var keeper = baseGame.GetKeeper();
                if (keeper == null) return null;
                
                var keeperMap = keeper.GetKeeperMap();
                if (keeperMap == null) return null;
                
                return keeperMap;
            }
            catch (Exception ex)
            {
                Log.LogWarning($"{LogPrefix} GetCurrent failed: {ex.Message}");
                return null;
            }
        }
        
        // ==================== CORE KEEPERMAP METHODS ====================
        
        /// <summary>
        /// Direct native Find&lt;T&gt; method access.
        /// Performance: O(1) Dictionary lookup.
        /// </summary>
        /// <typeparam name="T">Expected entity type</typeparam>
        /// <param name="handle">Entity Handle (accepts boxed Handle struct or Handle directly)</param>
        /// <returns>Typed entity instance or null if not found</returns>
        public T? Find<T>(object handle) where T : class
        {
            if (NativeObject is not KeeperMap map || handle is not Handle h) return null;
            var result = map.Find<T>(h);
            return result;
        }
        
        /// <summary>
        /// Direct native Contains method access.
        /// Performance: O(1) Dictionary.ContainsKey.
        /// </summary>
        /// <param name="handle">Handle to check (accepts boxed Handle struct)</param>
        /// <returns>True if Handle exists in KeeperMap</returns>
        public bool Contains(object handle)
        {
            if (NativeObject is not KeeperMap map || handle is not Handle h) return false;
            return map.Contains(h);
        }
        
        /// <summary>
        /// Find an entity by Handle via the typed interop proxy.
        /// Returns null if the handle is not in the map or the wrapper is invalid.
        /// </summary>
        /// <param name="handle">Entity Handle (accepts Handle or object)</param>
        public IHandleable? FindBase(object? handle)
        {
            if (handle is not Handle h || NativeObject is not KeeperMap map) return null;
            return map.Find(h);
        }
        
        // ==================== ADVANCED OPERATIONS ====================
        
        /// <summary>
        /// Access to internal _objects dictionary via the publicized interop proxy.
        /// Returns the native Il2Cpp Dictionary&lt;Handle, IHandleable&gt; or null.
        /// </summary>
        public Il2CppSystem.Collections.Generic.Dictionary<Handle, IHandleable>? GetObjectsDict()
            => NativeObject is KeeperMap map ? map._objects : null;
        
        
        /// <summary>
        /// Get total entity count in KeeperMap (typed _objects.Count).
        /// </summary>
        public int GetEntityCount() => GetObjectsDict()?.Count ?? 0;
        
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

            // Itération typée du dictionnaire IL2CPP (clé = Handle, struct)
            foreach (var kvp in objectsDict)
                yield return kvp.Key;
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

            // Itération typée du dictionnaire IL2CPP (valeur = IHandleable)
            foreach (var kvp in objectsDict)
                if (kvp.Value != null) yield return kvp.Value;
        }
        
        /// <summary>
        /// Enumerate entities filtered by type predicate
        /// Performance: O(n) with type checking overhead
        /// Flexible filtering for specialized queries
        /// </summary>
        /// <param name="typePredicate">Function to test entity type</param>
        public IEnumerable<object> EnumerateEntitiesByType(Func<object, bool> typePredicate)
        {
            if (typePredicate == null) 
                return Enumerable.Empty<object>();
            
            var results = new List<object>();
            
            foreach (var entity in EnumerateEntities())
            {
                try
                {
                    if (typePredicate(entity))
                    {
                        results.Add(entity);
                    }
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.Log($"{LogPrefix} Type predicate failed for entity: {ex.Message}");
                    // Continue enumeration despite individual failures
                }
            }
            
            return results;
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
#pragma warning restore CS1591
