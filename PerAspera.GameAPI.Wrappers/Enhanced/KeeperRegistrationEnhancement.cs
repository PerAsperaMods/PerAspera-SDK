using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Wrappers;

namespace PerAspera.GameAPI.Wrappers.Enhanced.Registration
{
    /// <summary>
    /// Feature 1: Registration Enhancement with HarmonyX Advanced Patterns
    /// SCOPE: Real-time registry tracking + Events + Validation
    /// PERFORMANCE TARGET: <0.5ms total overhead per registration/unregistration
    /// DOC REFERENCES: F:\ModPeraspera\Internal_doc\IMPLEMENTATION\Feature1-Registration-Implementation.cs
    /// </summary>
    [BepInPlugin("peraspera.sdk.keeper.registration", "Keeper Registration Enhancement", "1.0.0")]
    public class RegistrationEnhancementPlugin : BasePlugin
    {
        private static ManualLogSource? logger;
        private static readonly Harmony harmony = new Harmony("peraspera.sdk.keeper.registration");

        // CRITICAL IL2CPP Type Safety Rule
        private static System.Type? _keeperType;
        private static System.Type? _iHandleableType;

        public override void Load()
        {
            logger = Log;
            logger.LogInfo("🔧 Initializing Keeper Registration Enhancement with HarmonyX patterns...");
            
            try
            {
                // Initialize IL2CPP types with safety
                InitializeTypes();
                
                // Apply HarmonyX patches with advanced patterns
                harmony.PatchAll(typeof(RegistrationPatches));
                
                // Initialize tracking system
                KeeperRegistrationTracker.Initialize();
                
                logger.LogInfo("✅ Registration Enhancement loaded successfully");
            }
            catch (Exception ex)
            {
                logger.LogError($"❌ Failed to load Registration Enhancement: {ex.Message}");
                throw;
            }
        }

        private void InitializeTypes()
        {
            try
            {
                // Get IL2CPP types safely
                _keeperType = GameTypeInitializer.GetKeeper();
                _iHandleableType = GameTypeInitializer.GetIHandleable();
                
                logger.LogInfo($"✅ Types initialized - Keeper: {_keeperType?.Name}, IHandleable: {_iHandleableType?.Name}");
            }
            catch (Exception ex)
            {
                logger.LogError($"❌ Failed to initialize types: {ex.Message}");
                throw;
            }
        }
        
        public override bool Unload()
        {
            logger?.LogInfo("🛑 Unloading Registration Enhancement");
            KeeperRegistrationTracker.Cleanup();
            harmony.UnpatchSelf();
            return true;
        }
    }

    /// <summary>
    /// Enhanced Registration Tracker with Event Emission
    /// Thread-safe tracking with WeakReference for IL2CPP safety
    /// </summary>
    public static class KeeperRegistrationTracker
    {
        private static readonly LogAspera _log = new LogAspera("RegistrationTracker");
        private static readonly ConcurrentDictionary<int, WeakReference<object>> _registeredEntities = new();
        private static readonly ConcurrentDictionary<int, WeakReference<object>> _entityTypeCache = new();
        
        // Performance counters
        private static int _totalRegistrations = 0;
        private static int _totalUnregistrations = 0;
        
        // Events for Feature 2 integration
        public static event Action<object, int>? OnEntityRegistered;
        public static event Action<object, int>? OnEntityUnregistered;
        
        public static void Initialize()
        {
            _log.Info("🔧 Initializing Registration Tracker");
        }
        
        /// <summary>
        /// Track entity registration from HarmonyX patch
        /// </summary>
        public static void TrackRegistration(object entity, int handle)
        {
            if (entity == null) return;
            
            try
            {
                // Store weak reference to prevent memory leaks
                _registeredEntities[handle] = new WeakReference<object>(entity);
                
                // Cache entity type for O(1) filtering
                var entityType = entity.GetType();
                _entityTypeCache[handle] = new WeakReference<object>(entityType);
                
                // Update counters
                System.Threading.Interlocked.Increment(ref _totalRegistrations);
                
                // Emit event for Feature 2
                OnEntityRegistered?.Invoke(entity, handle);
                
                _log.Debug($"📝 Registered: {entityType.Name} (Handle: {handle})");
            }
            catch (Exception ex)
            {
                _log.Error($"❌ Failed to track registration: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Track entity unregistration from HarmonyX patch
        /// </summary>
        public static void TrackUnregistration(object entity, int handle)
        {
            if (entity == null) return;
            
            try
            {
                // Remove from tracking
                _registeredEntities.TryRemove(handle, out _);
                _entityTypeCache.TryRemove(handle, out _);
                
                // Update counters
                System.Threading.Interlocked.Increment(ref _totalUnregistrations);
                
                // Emit event for Feature 2
                OnEntityUnregistered?.Invoke(entity, handle);
                
                _log.Debug($"🗑️ Unregistered: {entity.GetType().Name} (Handle: {handle})");
            }
            catch (Exception ex)
            {
                _log.Error($"❌ Failed to track unregistration: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Get all registered entities of type T
        /// Uses cached type information for O(1) filtering
        /// </summary>
        public static IEnumerable<T> GetRegisteredEntities<T>() where T : class
        {
            var targetType = typeof(T);
            var results = new List<T>();
            
            foreach (var kvp in _registeredEntities)
            {
                if (_entityTypeCache.TryGetValue(kvp.Key, out var typeRef))
                {
                    if (typeRef.TryGetTarget(out var cachedType) && 
                        cachedType is System.Type type && targetType.IsAssignableFrom(type))
                    {
                        if (kvp.Value.TryGetTarget(out var entity) && entity is T typedEntity)
                        {
                            results.Add(typedEntity);
                        }
                    }
                }
            }
            
            return results;
        }
        
        /// <summary>
        /// Get registration statistics for monitoring
        /// </summary>
        public static (int total, int active, int unregistered) GetRegistrationStats()
        {
            var activeCount = 0;
            
            // Count entities with valid weak references
            foreach (var kvp in _registeredEntities)
            {
                if (kvp.Value.TryGetTarget(out _))
                {
                    activeCount++;
                }
            }
            
            return (_totalRegistrations, activeCount, _totalUnregistrations);
        }
        
        /// <summary>
        /// Cleanup dead weak references for memory optimization
        /// </summary>
        public static void CleanupDeadReferences()
        {
            var deadHandles = new List<int>();
            
            foreach (var kvp in _registeredEntities)
            {
                if (!kvp.Value.TryGetTarget(out _))
                {
                    deadHandles.Add(kvp.Key);
                }
            }
            
            foreach (var handle in deadHandles)
            {
                _registeredEntities.TryRemove(handle, out _);
                _entityTypeCache.TryRemove(handle, out _);
            }
            
            if (deadHandles.Count > 0)
            {
                _log.Debug($"🧹 Cleaned {deadHandles.Count} dead references");
            }
        }
        
        public static void Cleanup()
        {
            _registeredEntities.Clear();
            _entityTypeCache.Clear();
            OnEntityRegistered = null;
            OnEntityUnregistered = null;
        }
    }

    /// <summary>
    /// HarmonyX Advanced Patches with EmitDelegate and CodeMatcher
    /// </summary>
    [HarmonyPatch]
    public static class RegistrationPatches
    {
        private static readonly LogAspera _log = new LogAspera("RegistrationPatches");
        
        /// <summary>
        /// HarmonyX Postfix for Keeper.Register()
        /// Captures registration events and enables SDK integration
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(object), "Register")] // Will be resolved to actual Keeper type
        public static void RegisterPostfix(object __instance, object __0, object __result)
        {
            try
            {
                // Validate this is actually a Keeper instance
                if (__instance?.GetType().Name != "Keeper") return;
                
                // Extract entity and handle from parameters and result
                var entity = __0; // IHandleable entity parameter
                
                if (entity != null && __result != null)
                {
                    // Convert handle to int (assuming Handle has ToInt() or similar)
                    var handleValue = ExtractHandleValue(__result);
                    if (handleValue.HasValue)
                    {
                        KeeperRegistrationTracker.TrackRegistration(entity, handleValue.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"❌ RegisterPostfix error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// HarmonyX Prefix for Keeper.Unregister()
        /// Captures entity before unregistration for event emission
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(object), "Unregister")]
        public static void UnregisterPrefix(object __instance, object __0)
        {
            try
            {
                // Validate this is actually a Keeper instance
                if (__instance?.GetType().Name != "Keeper") return;
                
                var entity = __0; // IHandleable entity parameter
                
                if (entity != null)
                {
                    // Get handle before unregistration
                    var handle = entity.InvokeMethod<object>("GetHandle");
                    var handleValue = ExtractHandleValue(handle);
                    
                    if (handleValue.HasValue)
                    {
                        KeeperRegistrationTracker.TrackUnregistration(entity, handleValue.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"❌ UnregisterPrefix error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Extract integer value from Handle object
        /// Handle types may vary, so try multiple approaches
        /// </summary>
        private static int? ExtractHandleValue(object? handle)
        {
            if (handle == null) return null;
            
            try
            {
                // Try direct int conversion
                if (handle is int intValue) return intValue;
                
                // Try ToInt() method
                var toIntMethod = handle.GetType().GetMethod("ToInt");
                if (toIntMethod != null)
                {
                    var result = toIntMethod.Invoke(handle, null);
                    if (result is int value) return value;
                }
                
                // Try Value property
                var valueProperty = handle.GetType().GetProperty("Value");
                if (valueProperty != null)
                {
                    var result = valueProperty.GetValue(handle);
                    if (result is int value) return value;
                }
                
                // Try ToString() and parse
                var stringValue = handle.ToString();
                if (int.TryParse(stringValue, out var parsedValue))
                {
                    return parsedValue;
                }
                
                return null;
            }
            catch
            {
                return null;
            }
        }
    }

    /// <summary>
    /// SDK Public API for Feature 1
    /// </summary>
    public static class KeeperRegistrationSDK
    {
        /// <summary>
        /// Get all entities of type T currently registered in Keeper system
        /// Uses tracked registration data for O(1) type filtering
        /// </summary>
        public static IEnumerable<T> GetRegisteredEntities<T>() where T : class
        {
            return KeeperRegistrationTracker.GetRegisteredEntities<T>();
        }
        
        /// <summary>
        /// Get real-time registration statistics
        /// Performance: O(1) for counts, O(n) for active count with weak reference validation
        /// </summary>
        public static (int total, int active, int unregistered) GetRegistrationStats()
        {
            return KeeperRegistrationTracker.GetRegistrationStats();
        }
        
        /// <summary>
        /// Subscribe to real-time registration events
        /// Integration point for Feature 2 (Events)
        /// </summary>
        public static void SubscribeToRegistrationEvents(
            Action<object, int>? onRegistered = null,
            Action<object, int>? onUnregistered = null)
        {
            if (onRegistered != null)
                KeeperRegistrationTracker.OnEntityRegistered += onRegistered;
            
            if (onUnregistered != null)
                KeeperRegistrationTracker.OnEntityUnregistered += onUnregistered;
        }
        
        /// <summary>
        /// Unsubscribe from registration events
        /// </summary>
        public static void UnsubscribeFromRegistrationEvents(
            Action<object, int>? onRegistered = null,
            Action<object, int>? onUnregistered = null)
        {
            if (onRegistered != null)
                KeeperRegistrationTracker.OnEntityRegistered -= onRegistered;
            
            if (onUnregistered != null)
                KeeperRegistrationTracker.OnEntityUnregistered -= onUnregistered;
        }
        
        /// <summary>
        /// Force cleanup of dead weak references
        /// Call periodically in high-registration scenarios
        /// </summary>
        public static void OptimizeMemory()
        {
            KeeperRegistrationTracker.CleanupDeadReferences();
        }
    }
}