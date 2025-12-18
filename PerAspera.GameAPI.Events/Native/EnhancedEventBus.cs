using System;
using System.Collections.Generic;
using PerAspera.GameAPI.Events.Core;
using PerAspera.Core;

namespace PerAspera.GameAPI.Events.Native
{
    /// <summary>
    /// Enhanced event bus that automatically converts native IL2CPP instances to SDK wrappers
    /// Provides type-safe event handling with automatic wrapper conversion
    /// </summary>
    public static class EnhancedEventBus
    {
        private static readonly LogAspera _logger = new LogAspera("EnhancedEventBus");
        private static readonly Dictionary<string, List<Action<object>>> _subscribers = new();
        private static readonly object _lock = new object();
        private static bool _autoConversionEnabled = true;

        /// <summary>
        /// Subscribe to an event with automatic wrapper conversion
        /// </summary>
        /// <param name="eventType">Event type constant</param>
        /// <param name="handler">Event handler (will receive converted wrappers)</param>
        public static void Subscribe(string eventType, Action<object> handler)
        {
            if (string.IsNullOrEmpty(eventType) || handler == null)
                return;

            lock (_lock)
            {
                if (!_subscribers.ContainsKey(eventType))
                {
                    _subscribers[eventType] = new List<Action<object>>();
                }

                _subscribers[eventType].Add(handler);
                _logger.Debug($"Subscribed to {eventType}, total handlers: {_subscribers[eventType].Count}");
            }
        }

        /// <summary>
        /// Unsubscribe from an event
        /// </summary>
        /// <param name="eventType">Event type constant</param>
        /// <param name="handler">Event handler to remove</param>
        public static void Unsubscribe(string eventType, Action<object> handler)
        {
            if (string.IsNullOrEmpty(eventType) || handler == null)
                return;

            lock (_lock)
            {
                if (_subscribers.TryGetValue(eventType, out var handlers))
                {
                    handlers.Remove(handler);
                    if (handlers.Count == 0)
                    {
                        _subscribers.Remove(eventType);
                    }
                    _logger.Debug($"Unsubscribed from {eventType}");
                }
            }
        }

        /// <summary>
        /// Publish an event with automatic wrapper conversion
        /// </summary>
        /// <param name="eventType">Event type constant</param>
        /// <param name="eventData">Event data (will be converted)</param>
        public static void Publish(string eventType, object eventData)
        {
            if (string.IsNullOrEmpty(eventType) || eventData == null)
                return;

            List<Action<object>>? handlers;
            lock (_lock)
            {
                if (!_subscribers.TryGetValue(eventType, out handlers) || handlers.Count == 0)
                    return;

                // Create a copy to avoid modification during iteration
                handlers = new List<Action<object>>(handlers);
            }

            // Convert event data to use wrappers
            var convertedEventData = PrepareEventForDelivery(eventData);

            // Deliver to all subscribers
            var deliveredCount = 0;
            foreach (var handler in handlers)
            {
                try
                {
                    handler(convertedEventData);
                    deliveredCount++;
                }
                catch (Exception ex)
                {
                    _logger.Error($"Event handler failed for {eventType}: {ex.Message}");
                }
            }

            _logger.Debug($"Published {eventType} to {deliveredCount}/{handlers.Count} handlers");
        }

        /// <summary>
        /// Prepare event data for delivery by converting native instances to wrappers
        /// </summary>
        /// <param name="eventData">Raw event data</param>
        /// <returns>Event data with wrappers</returns>
        private static object PrepareEventForDelivery(object eventData)
        {
            if (!_autoConversionEnabled)
                return eventData;

            try
            {
                // Apply wrapper conversion to event properties
                NativeEventConverter.ConvertEventProperties(eventData);
                return eventData;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to convert event data: {ex.Message}");
                return eventData; // Return original if conversion fails
            }
        }

        /// <summary>
        /// Enable or disable automatic wrapper conversion
        /// </summary>
        /// <param name="enabled">True to enable conversion</param>
        public static void SetAutoConversion(bool enabled)
        {
            _autoConversionEnabled = enabled;
            _logger.Info($"Auto conversion {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Get current auto conversion status
        /// </summary>
        /// <returns>True if auto conversion is enabled</returns>
        public static bool IsAutoConversionEnabled()
        {
            return _autoConversionEnabled;
        }

        /// <summary>
        /// Get all subscribed event types
        /// </summary>
        /// <returns>Collection of event type names</returns>
        public static IReadOnlyCollection<string> GetSubscribedEventTypes()
        {
            lock (_lock)
            {
                return _subscribers.Keys;
            }
        }

        /// <summary>
        /// Get subscriber count for a specific event type
        /// </summary>
        /// <param name="eventType">Event type to check</param>
        /// <returns>Number of subscribers</returns>
        public static int GetSubscriberCount(string eventType)
        {
            lock (_lock)
            {
                return _subscribers.TryGetValue(eventType, out var handlers) ? handlers.Count : 0;
            }
        }

        /// <summary>
        /// Clear all subscriptions (for testing/cleanup)
        /// </summary>
        public static void ClearAllSubscriptions()
        {
            lock (_lock)
            {
                _subscribers.Clear();
                _logger.Info("Cleared all event subscriptions");
            }
        }

        /// <summary>
        /// Bridge method for legacy EventSystem compatibility
        /// Allows existing code to work through enhanced bus
        /// </summary>
        public static void PublishLegacyEvent(string eventKey, object eventData)
        {
            // Handle legacy event key formats
            var normalizedKey = NormalizeEventKey(eventKey);
            Publish(normalizedKey, eventData);
        }

        /// <summary>
        /// Normalize legacy event keys to new format
        /// </summary>
        /// <param name="eventKey">Original event key</param>
        /// <returns>Normalized event key</returns>
        private static string NormalizeEventKey(string eventKey)
        {
            // Convert legacy keys like "NativeBuildingSpawned" to standard format
            if (eventKey.StartsWith("Native"))
            {
                return $"Native:{eventKey.Substring(6)}"; // "Native:BuildingSpawned"
            }

            return eventKey;
        }

        /// <summary>
        /// Get statistics about event bus performance
        /// </summary>
        /// <returns>Performance statistics</returns>
        public static EventBusStats GetStats()
        {
            lock (_lock)
            {
                var totalHandlers = 0;
                foreach (var handlers in _subscribers.Values)
                {
                    totalHandlers += handlers.Count;
                }

                return new EventBusStats
                {
                    EventTypeCount = _subscribers.Count,
                    TotalHandlers = totalHandlers,
                    AutoConversionEnabled = _autoConversionEnabled
                };
            }
        }
    }

    /// <summary>
    /// Statistics about event bus performance and state
    /// </summary>
    public class EventBusStats
    {
        public int EventTypeCount { get; set; }
        public int TotalHandlers { get; set; }
        public bool AutoConversionEnabled { get; set; }

        public override string ToString()
        {
            return $"EventBus: {EventTypeCount} types, {TotalHandlers} handlers, AutoConversion={AutoConversionEnabled}";
        }
    }
}