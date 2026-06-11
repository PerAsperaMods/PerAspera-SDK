#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime.InteropTypes;
using UnityEngine;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Central API for Handle-based entity access in Per Aspera.
    /// Provides type-safe, validated access to game entities via the Keeper Handle system.
    ///
    /// MIGRATION 2026-06 — interop typé : délégation directe à Keeper.map (KeeperMap typé,
    /// Contains/Find/_objects). Le downcast IL2CPP passe par le ctor IntPtr des proxies
    /// (équivalent TryCast, compatible avec la contrainte « where T : class » publique).
    /// Bug corrigé : FindBuildingsNear ne matchait JAMAIS (position attendue en Vector3
    /// alors que Building.position est un Vector2).
    /// </summary>
    public static class HandleHelper
    {
        private static readonly string LogPrefix = "[HandleHelper]";

        // ==================== AVAILABILITY CHECK ====================

        /// <summary>
        /// Check if Handle system is available
        /// (true after BaseGame.Awake() and Keeper initialization).
        /// </summary>
        public static bool IsAvailable() => GetKeeperMapInternal() != null;

        // ==================== CORE ACCESS ====================

        /// <summary>
        /// Find entity by Handle with type safety and null protection.
        /// Performance: O(1) dictionary lookup via KeeperMap (typed).
        /// </summary>
        /// <typeparam name="T">Expected entity type (Building, Faction, etc.)</typeparam>
        /// <param name="handle">Entity Handle (from game events, UI, etc.)</param>
        /// <returns>Typed entity instance or null if not found/invalid</returns>
        /// <example>var building = HandleHelper.FindSafe&lt;Building&gt;(handle);</example>
        public static T? FindSafe<T>(object handle) where T : class
        {
            var map = GetKeeperMapInternal();
            if (map == null || handle is not Handle h) return null;

            try
            {
                if (!map.Contains(h)) return null;
                return CastIl2Cpp<T>(map.Find(h));
            }
            catch (Exception ex)
            {
                LogWarning($"FindSafe<{typeof(T).Name}> failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Validate Handle exists and is accessible (typed KeeperMap.Contains).
        /// </summary>
        public static bool IsValid(object handle)
        {
            var map = GetKeeperMapInternal();
            if (map == null || handle is not Handle h) return false;
            try { return map.Contains(h); }
            catch { return false; }
        }

        /// <summary>
        /// Attempt Handle resolution with try-pattern.
        /// </summary>
        public static bool TryFind<T>(object handle, out T? result) where T : class
        {
            result = FindSafe<T>(handle);
            return result != null;
        }

        // ==================== BULK OPERATIONS ====================

        /// <summary>
        /// Get all entities of specified type from KeeperMap (typed _objects enumeration).
        /// Performance: O(n) where n = total entities in KeeperMap.
        /// </summary>
        /// <example>foreach (var b in HandleHelper.FindAll&lt;Building&gt;()) { ... }</example>
        public static IEnumerable<T> FindAll<T>() where T : class
        {
            var objects = GetKeeperMapInternal()?._objects;
            if (objects == null)
            {
                LogDebug("KeeperMap._objects not available for FindAll");
                yield break;
            }

            foreach (var kvp in objects)
            {
                var typed = CastIl2Cpp<T>(kvp.Value);
                if (typed != null) yield return typed;
            }
        }

        /// <summary>
        /// Get entities by Handle list with batch validation (skips invalid Handles).
        /// </summary>
        public static IEnumerable<T> FindMany<T>(IEnumerable<object> handles) where T : class
        {
            if (handles == null) return Enumerable.Empty<T>();

            var results = new List<T>();
            var map = GetKeeperMapInternal();
            if (map == null) return results;

            foreach (var handle in handles)
            {
                if (handle is not Handle h) continue;
                try
                {
                    if (!map.Contains(h)) continue;
                    var entity = CastIl2Cpp<T>(map.Find(h));
                    if (entity != null) results.Add(entity);
                }
                catch (Exception ex)
                {
                    LogDebug($"FindMany: handle skipped ({ex.Message})");
                }
            }
            return results;
        }

        // ==================== SPECIALIZED ACCESS ====================

        /// <summary>
        /// Find Buildings by type key or display name (typed Building.buildingType).
        /// Performance: O(n) where n = total entities.
        /// </summary>
        /// <param name="buildingTypeName">BuildingType key (e.g. "water_mine") or name</param>
        /// <example>var mines = HandleHelper.FindBuildingsByType("water_mine");</example>
        public static IEnumerable<Building> FindBuildingsByType(string buildingTypeName)
        {
            if (string.IsNullOrEmpty(buildingTypeName)) yield break;

            foreach (var building in FindAll<Building>())
            {
                var bt = building.buildingType;
                if (bt == null) continue;
                if (string.Equals(bt.key, buildingTypeName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(bt.name, buildingTypeName, StringComparison.OrdinalIgnoreCase))
                {
                    yield return building;
                }
            }
        }

        /// <summary>
        /// Find Buildings within radius of a 2D position (typed Building.position).
        /// ⚠️ Building.position est un Vector2 (monde 2D) — l'ancienne version comparait
        /// un Vector3 inexistant et ne retournait JAMAIS rien.
        /// </summary>
        /// <param name="center">Center position (x/y du monde 2D)</param>
        /// <param name="radius">Search radius in game units</param>
        /// <example>var nearby = HandleHelper.FindBuildingsNear(pos, 50f);</example>
        public static IEnumerable<Building> FindBuildingsNear(Vector2 center, float radius)
        {
            if (radius <= 0) yield break;
            var radiusSquared = radius * radius;

            foreach (var building in FindAll<Building>())
            {
                var pos = building.position;
                if (Vector2.SqrMagnitude(pos - center) <= radiusSquared)
                    yield return building;
            }
        }

        // ==================== INTERNAL HELPERS ====================

        /// <summary>
        /// Get KeeperMap instance from BaseGame.keeper.map (typed chain).
        /// </summary>
        private static KeeperMap? GetKeeperMapInternal()
        {
            try
            {
                return BaseGameWrapper.GetCurrent()?.NativeBaseGame?.keeper?.map;
            }
            catch (Exception ex)
            {
                LogDebug($"GetKeeperMapInternal failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Downcast IL2CPP : un objet rendu sous son type déclaré (ex: IHandleable)
        /// est réinstancié dans le proxy T via son ctor IntPtr — équivalent TryCast
        /// compatible avec la contrainte publique « where T : class ».
        /// </summary>
        private static T? CastIl2Cpp<T>(object? entity) where T : class
        {
            if (entity == null) return null;
            if (entity is T direct) return direct;
            if (entity is Il2CppObjectBase il2cpp)
            {
                try
                {
                    var il2cppType = Il2CppInterop.Runtime.Il2CppType.Of<T>();
                    var objType = il2cpp.TryCast<Il2CppSystem.Object>()?.GetIl2CppType();
                    if (objType != null && !il2cppType.IsAssignableFrom(objType)) return null;
                    return (T?)Activator.CreateInstance(typeof(T), il2cpp.Pointer);
                }
                catch { return null; }
            }
            return null;
        }

        // ==================== LOGGING ====================

        private static void LogDebug(string message)
            => UnityEngine.Debug.Log($"{LogPrefix} [DEBUG] {message}");

        private static void LogWarning(string message)
            => UnityEngine.Debug.LogWarning($"{LogPrefix} [WARNING] {message}");

        /// <summary>
        /// Get Handle system performance metrics (typed _objects.Count).
        /// </summary>
        public static HandleSystemMetrics GetMetrics()
        {
            try
            {
                var objects = GetKeeperMapInternal()?._objects;
                if (objects == null)
                {
                    return new HandleSystemMetrics
                    {
                        IsAvailable = false,
                        TotalEntities = 0,
                        ErrorMessage = "KeeperMap not available"
                    };
                }

                return new HandleSystemMetrics
                {
                    IsAvailable = true,
                    TotalEntities = objects.Count,
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
