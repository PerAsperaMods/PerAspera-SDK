using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using PerAspera.Core;
using PerAspera.GameAPI.Wrappers.Enhanced.Registration;
using BepInEx.Unity.IL2CPP;

namespace PerAspera.GameAPI.Wrappers.Enhanced.Events
{
    /// <summary>
    /// Feature 2: Event System Enhancement with Generic Type Safety
    /// SCOPE: Typed event system + Game state events + Integration with Feature 1
    /// PERFORMANCE TARGET: <0.1ms per event emission, batch processing support
    /// DOC REFERENCES: F:\ModPeraspera\Internal_doc\IMPLEMENTATION\Feature2-Events-Implementation.cs
    /// </summary>
    [BepInPlugin("peraspera.sdk.keeper.events", "Keeper Event System Enhancement", "1.0.0")]
    public class EventSystemEnhancementPlugin : BasePlugin
    {
        private static ManualLogSource? logger;
        private static readonly Harmony harmony = new Harmony("peraspera.sdk.keeper.events");

        public override void Load()
        {
            logger = Log;
            logger.LogInfo("📡 Initializing Keeper Event System Enhancement...");
            
            try
            {
                // Initialize event system
                KeeperEventSystem.Initialize();
                
                // Apply HarmonyX patches for game state events
                harmony.PatchAll(typeof(EventPatches));
                
                // Integrate with Feature 1 if available
                IntegrateWithRegistrationSystem();
                
                logger.LogInfo("✅ Event System Enhancement loaded successfully");
            }
            catch (Exception ex)
            {
                logger.LogError($"❌ Failed to load Event System Enhancement: {ex.Message}");
                throw;
            }
        }
        
        private void IntegrateWithRegistrationSystem()
        {
            try
            {
                // Subscribe to Feature 1 events for automatic event emission
                KeeperRegistrationSDK.SubscribeToRegistrationEvents(
                    OnEntityRegistered,
                    OnEntityUnregistered
                );
                
                logger.LogInfo("✅ Integrated with Registration System (Feature 1)");
            }
            catch (Exception ex)
            {
                logger.LogWarning($"⚠️ Feature 1 not available for integration: {ex.Message}");
            }
        }
        
        private static void OnEntityRegistered(object entity, int handle)
        {
            KeeperEventSystem.EmitRegistrationEvent(entity, handle, KeeperEventType.Registered);
        }
        
        private static void OnEntityUnregistered(object entity, int handle)
        {
            KeeperEventSystem.EmitRegistrationEvent(entity, handle, KeeperEventType.Unregistered);
        }
        
        public override bool Unload()
        {
            logger?.LogInfo("🛑 Unloading Event System Enhancement");
            KeeperEventSystem.Cleanup();
            harmony.UnpatchSelf();
            return true;
        }
    }

    /// <summary>
    /// Typed Event System with Generic Type Safety
    /// </summary>
    public static class KeeperEventSystem
    {
        private static readonly LogAspera _log = new LogAspera("EventSystem");
        private static readonly ConcurrentDictionary<System.Type, List<object>> _subscribers = new();
        private static readonly ConcurrentDictionary<System.Type, Queue<object>> _eventQueue = new();
        
        // Performance metrics
        private static long _totalEventsEmitted = 0;
        private static long _totalEventsProcessed = 0;
        private static bool _batchProcessing = false;
        
        public static void Initialize()
        {
            _log.Info("📡 Initializing Event System");
        }
        
        /// <summary>
        /// Subscribe to events of type T with type safety
        /// </summary>
        public static void Subscribe<T>(Action<T> handler) where T : KeeperEvent
        {
            var eventType = typeof(T);
            
            if (!_subscribers.ContainsKey(eventType))
            {
                _subscribers[eventType] = new List<object>();
            }
            
            _subscribers[eventType].Add(handler);
            _log.Debug($"📝 Subscribed to {eventType.Name}");
        }
        
        /// <summary>
        /// Unsubscribe from events of type T
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : KeeperEvent
        {
            var eventType = typeof(T);
            
            if (_subscribers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);
                _log.Debug($"🗑️ Unsubscribed from {eventType.Name}");
            }
        }
        
        /// <summary>
        /// Emit event with type safety and error isolation
        /// </summary>
        public static void EmitEvent<T>(T keeperEvent) where T : KeeperEvent
        {
            try
            {
                var eventType = typeof(T);
                System.Threading.Interlocked.Increment(ref _totalEventsEmitted);
                
                if (_batchProcessing)
                {
                    // Queue for batch processing
                    if (!_eventQueue.ContainsKey(eventType))
                    {
                        _eventQueue[eventType] = new Queue<object>();
                    }
                    _eventQueue[eventType].Enqueue(keeperEvent);
                }
                else
                {
                    // Immediate processing
                    ProcessEvent(eventType, keeperEvent);
                }
                
                _log.Debug($"📤 Emitted {eventType.Name}");
            }
            catch (Exception ex)
            {
                _log.Error($"❌ Failed to emit event: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Process individual event with subscriber notification
        /// </summary>
        private static void ProcessEvent(System.Type eventType, object eventInstance)
        {
            if (!_subscribers.TryGetValue(eventType, out var handlers))
                return;
                
            foreach (var handler in handlers.ToList()) // Copy to avoid collection modification
            {
                try
                {
                    if (handler is Delegate del)
                    {
                        del.DynamicInvoke(eventInstance);
                        System.Threading.Interlocked.Increment(ref _totalEventsProcessed);
                    }
                }
                catch (Exception ex)
                {
                    _log.Error($"❌ Event handler error for {eventType.Name}: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// Integration with Feature 1 Registration Events
        /// </summary>
        public static void EmitRegistrationEvent(object entity, int handle, KeeperEventType eventType)
        {
            var entityTypeName = entity.GetType().Name;
            
            // Create typed event based on entity type
            if (entityTypeName.Contains("Building"))
            {
                EmitEvent(new BuildingKeeperEvent
                {
                    Entity = entity,
                    Handle = handle,
                    EventType = eventType,
                    Timestamp = DateTime.Now
                });
            }
            else if (entityTypeName.Contains("Drone"))
            {
                EmitEvent(new DroneKeeperEvent
                {
                    Entity = entity,
                    Handle = handle,
                    EventType = eventType,
                    Timestamp = DateTime.Now
                });
            }
            else
            {
                // Generic entity event
                EmitEvent(new EntityKeeperEvent
                {
                    Entity = entity,
                    Handle = handle,
                    EventType = eventType,
                    EntityType = entity.GetType(),
                    Timestamp = DateTime.Now
                });
            }
        }
        
        /// <summary>
        /// Enable batch processing for high-frequency scenarios
        /// </summary>
        public static void EnableBatchProcessing()
        {
            _batchProcessing = true;
            _log.Info("📦 Batch processing enabled");
        }
        
        /// <summary>
        /// Process all queued events in batch
        /// </summary>
        public static void ProcessBatchedEvents()
        {
            if (!_batchProcessing) return;
            
            var processedCount = 0;
            
            foreach (var kvp in _eventQueue)
            {
                var eventType = kvp.Key;
                var queue = kvp.Value;
                
                while (queue.Count > 0)
                {
                    var eventInstance = queue.Dequeue();
                    ProcessEvent(eventType, eventInstance);
                    processedCount++;
                }
            }
            
            if (processedCount > 0)
            {
                _log.Debug($"📦 Processed {processedCount} batched events");
            }
        }
        
        /// <summary>
        /// Get event system performance statistics
        /// </summary>
        public static EventSystemStats GetStats()
        {
            return new EventSystemStats
            {
                TotalEventsEmitted = _totalEventsEmitted,
                TotalEventsProcessed = _totalEventsProcessed,
                ActiveSubscriptions = _subscribers.Sum(kvp => kvp.Value.Count),
                QueuedEvents = _eventQueue.Sum(kvp => kvp.Value.Count),
                BatchProcessing = _batchProcessing
            };
        }
        
        public static void Cleanup()
        {
            _subscribers.Clear();
            _eventQueue.Clear();
            _totalEventsEmitted = 0;
            _totalEventsProcessed = 0;
            _batchProcessing = false;
        }
    }

    /// <summary>
    /// Base class for all Keeper events
    /// </summary>
    public abstract class KeeperEvent
    {
        public object Entity { get; set; } = null!;
        public int Handle { get; set; }
        public KeeperEventType EventType { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    /// <summary>
    /// Event types for Keeper system
    /// </summary>
    public enum KeeperEventType
    {
        Registered,
        Unregistered,
        StateChanged,
        PropertyUpdated,
        Error
    }
    
    /// <summary>
    /// Building-specific Keeper event
    /// </summary>
    public class BuildingKeeperEvent : KeeperEvent
    {
        public string? BuildingType { get; set; }
        public Vector3? Position { get; set; }
        public float? EnergyProduction { get; set; }
    }
    
    /// <summary>
    /// Drone-specific Keeper event
    /// </summary>
    public class DroneKeeperEvent : KeeperEvent
    {
        public string? DroneType { get; set; }
        public string? CurrentState { get; set; }
        public Vector3? Position { get; set; }
    }
    
    /// <summary>
    /// Generic entity Keeper event
    /// </summary>
    public class EntityKeeperEvent : KeeperEvent
    {
        public System.Type EntityType { get; set; } = null!;
        public Dictionary<string, object> Properties { get; set; } = new();
    }
    
    /// <summary>
    /// Event system performance statistics
    /// </summary>
    public struct EventSystemStats
    {
        public long TotalEventsEmitted { get; set; }
        public long TotalEventsProcessed { get; set; }
        public int ActiveSubscriptions { get; set; }
        public int QueuedEvents { get; set; }
        public bool BatchProcessing { get; set; }
        
        public override string ToString()
        {
            return $"Events: {TotalEventsProcessed}/{TotalEventsEmitted}, Subs: {ActiveSubscriptions}, Queue: {QueuedEvents}";
        }
    }

    /// <summary>
    /// HarmonyX patches for game state events
    /// </summary>
    [HarmonyPatch]
    public static class EventPatches
    {
        private static readonly LogAspera _log = new LogAspera("EventPatches");
        
        /// <summary>
        /// Patch BaseGame.Update for periodic event processing
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BaseGame), "Update")]
        public static void BaseGameUpdatePostfix(BaseGame __instance)
        {
            try
            {
                // Process batched events every frame in BaseGame
                KeeperEventSystem.ProcessBatchedEvents();
            }
            catch (Exception ex)
            {
                _log.Error($"❌ BaseGame Update patch error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// SDK Public API for Feature 2
    /// </summary>
    public static class KeeperEventsSDK
    {
        /// <summary>
        /// Subscribe to typed Keeper events
        /// </summary>
        public static void Subscribe<T>(Action<T> handler) where T : KeeperEvent
        {
            KeeperEventSystem.Subscribe(handler);
        }
        
        /// <summary>
        /// Unsubscribe from typed Keeper events
        /// </summary>
        public static void Unsubscribe<T>(Action<T> handler) where T : KeeperEvent
        {
            KeeperEventSystem.Unsubscribe(handler);
        }
        
        /// <summary>
        /// Emit custom Keeper event
        /// </summary>
        public static void EmitEvent<T>(T keeperEvent) where T : KeeperEvent
        {
            KeeperEventSystem.EmitEvent(keeperEvent);
        }
        
        /// <summary>
        /// Enable batch processing for high-frequency scenarios
        /// </summary>
        public static void EnableBatchProcessing()
        {
            KeeperEventSystem.EnableBatchProcessing();
        }
        
        /// <summary>
        /// Get event system statistics
        /// </summary>
        public static EventSystemStats GetEventStats()
        {
            return KeeperEventSystem.GetStats();
        }
    }
}