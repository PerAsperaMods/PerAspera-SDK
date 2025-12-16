using System;
using System.Collections.Generic;
using PerAspera.Core;

namespace PerAspera.ModSDK.Systems
{
    /// <summary>
    /// Event system - Custom mod event publishing and subscription
    /// </summary>
    public static class EventSystem
    {
        private static readonly Dictionary<string, List<Action<object>>> _subscriptions = new();
        private static bool _eventSystemInitialized = false;
        private static readonly LogAspera Log = new LogAspera(nameof(EventSystem));

        /// <summary>
        /// Initialize the event system
        /// </summary>
        internal static void Initialize()
        {
            if (_eventSystemInitialized) return;
            
            _eventSystemInitialized = true;
            Log.Info("ModSDK Event System initialized");
        }

        /// <summary>
        /// Subscribe to a game event
        /// </summary>
        /// <param name="eventName">Event name (use GameEvents.* constants)</param>
        /// <param name="handler">Your event handler</param>
        public static void Subscribe(string eventName, Action<object> handler)
        {
            EnsureInitialized();
            try
            {
                if (!_subscriptions.ContainsKey(eventName))
                {
                    _subscriptions[eventName] = new List<Action<object>>();
                }
                
                _subscriptions[eventName].Add(handler);
                Log.Debug($"Subscribed to event: {eventName}");
            }
            catch (Exception ex)
            {
                throw new ModSDKException($"Failed to subscribe to event '{eventName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Unsubscribe from a game event
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="handler">Your event handler</param>
        public static void Unsubscribe(string eventName, Action<object> handler)
        {
            if (!_eventSystemInitialized) return;

            try
            {
                if (_subscriptions.ContainsKey(eventName))
                {
                    _subscriptions[eventName].Remove(handler);
                    Log.Debug($"Unsubscribed from event: {eventName}");
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to unsubscribe from event '{eventName}': {ex.Message}");
            }
        }

        /// <summary>
        /// Publish a custom mod event
        /// </summary>
        /// <param name="eventName">Your custom event name (prefix with your mod name)</param>
        /// <param name="eventData">Event data</param>
        public static void Publish(string eventName, object eventData)
        {
            EnsureInitialized();

            try
            {
                if (_subscriptions.ContainsKey(eventName))
                {
                    foreach (var handler in _subscriptions[eventName])
                    {
                        try
                        {
                            handler(eventData);
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Error in event handler for '{eventName}': {ex.Message}");
                        }
                    }
                }
                
                Log.Debug($"Published event: {eventName}");
            }
            catch (Exception ex)
            {
                throw new ModSDKException($"Failed to publish event '{eventName}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get event subscription statistics
        /// </summary>
        public static Dictionary<string, int> GetStats()
        {
            var stats = new Dictionary<string, int>();
            
            foreach (var kvp in _subscriptions)
            {
                stats[kvp.Key] = kvp.Value.Count;
            }

            return stats;
        }

        /// <summary>
        /// Unsubscribe from all events (internal cleanup)
        /// </summary>
        internal static void UnsubscribeAll()
        {
            try
            {
                _subscriptions.Clear();
            }
            catch (Exception ex)
            {
                Log.Error($"Error unsubscribing from all events: {ex.Message}");
            }
        }

        /// <summary>
        /// Ensure the event system is initialized
        /// </summary>
        private static void EnsureInitialized()
        {
            if (!_eventSystemInitialized)
            {
                throw new ModSDKException("Event system not initialized. Call ModSDK.Initialize() first.");
            }
        }
    }
}