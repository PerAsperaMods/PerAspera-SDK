using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core;
using PerAspera.GameAPI.Events.Core;
using PerAspera.GameAPI.Events.SDK;

namespace PerAspera.GameAPI.Events.Integration
{
    /// <summary>
    /// Enhanced Event Bus providing static subscription methods for SDK events
    /// Provides type-safe event subscription with automatic wrapper conversion
    /// </summary>
    public static class EnhancedEventBus
    {
        private static readonly LogAspera _logger = new LogAspera("EnhancedEventBus");
        private static readonly Dictionary<System.Type, List<Delegate>> _eventHandlers = new();
        private static bool _autoConversionEnabled = true;

        // ==================== SYSTEM EVENT SUBSCRIPTIONS ====================

        /// <summary>
        /// Subscribe to GameHub initialization event (earliest possible mod loading)
        /// Fires when GameHub scene is loaded and manager is available
        /// ✅ Use this for mods that need immediate initialization (UI, Twitch, logging, etc.)
        /// </summary>
        public static void SubscribeToGameHubReady(Action onGameHubReady)
        {
            if (onGameHubReady == null)
                throw new ArgumentNullException(nameof(onGameHubReady));

            _logger.Info("Subscribed to GameHub ready event");
            RegisterHandler<Action>(typeof(GameHubReadyEvent), onGameHubReady);
        }

        /// <summary>
        /// Subscribe to early mods ready event
        /// Fires after GameHub initialization, before full game load
        /// ✅ Use this for mods that need immediate initialization but require BaseGame access
        /// </summary>
        public static void SubscribeToEarlyModsReady(Action<EarlyModsReadyEvent> onEarlyModsReady)
        {
            if (onEarlyModsReady == null)
                throw new ArgumentNullException(nameof(onEarlyModsReady));

            _logger.Info("Subscribed to EarlyModsReady event");
            RegisterHandler(typeof(EarlyModsReadyEvent), onEarlyModsReady);
        }

        /// <summary>
        /// Subscribe to BaseGame detection event
        /// Fires when BaseGame and Universe instances are available
        /// ✅ Use this for mods that need game state but not full planet access
        /// </summary>
        public static void SubscribeToBaseGameDetected(Action<BaseGameDetectedEvent> onBaseGameDetected)
        {
            if (onBaseGameDetected == null)
                throw new ArgumentNullException(nameof(onBaseGameDetected));

            _logger.Info("Subscribed to BaseGameDetected event");
            RegisterHandler(typeof(BaseGameDetectedEvent), onBaseGameDetected);
        }

        /// <summary>
        /// Subscribe to game fully loaded event
        /// Fires when BaseGame + Universe + Planet are all available
        /// ✅ Use this for mods that need full game state access
        /// </summary>
        public static void SubscribeToGameFullyLoaded(Action<GameFullyLoadedEvent> onGameFullyLoaded)
        {
            if (onGameFullyLoaded == null)
                throw new ArgumentNullException(nameof(onGameFullyLoaded));

            _logger.Info("Subscribed to GameFullyLoaded event");
            RegisterHandler(typeof(GameFullyLoadedEvent), onGameFullyLoaded);
        }

        /// <summary>
        /// Subscribe to BaseGame creation event
        /// Fires when BaseGame instance is first created
        /// ✅ Use this for early game initialization hooks
        /// </summary>
        public static void SubscribeToBaseGameCreated(Action<BaseGameCreatedEvent> onBaseGameCreated)
        {
            if (onBaseGameCreated == null)
                throw new ArgumentNullException(nameof(onBaseGameCreated));

            _logger.Info("Subscribed to BaseGameCreated event");
            RegisterHandler(typeof(BaseGameCreatedEvent), onBaseGameCreated);
        }

        /// <summary>
        /// Subscribe to Universe creation event
        /// Fires when Universe instance is first created
        /// ✅ Use this for universe-level initialization hooks
        /// </summary>
        public static void SubscribeToUniverseCreated(Action<UniverseCreatedEvent> onUniverseCreated)
        {
            if (onUniverseCreated == null)
                throw new ArgumentNullException(nameof(onUniverseCreated));

            _logger.Info("Subscribed to UniverseCreated event");
            RegisterHandler(typeof(UniverseCreatedEvent), onUniverseCreated);
        }

        /// <summary>
        /// Subscribe to Planet creation event  
        /// Fires when Planet instance is first created
        /// ✅ Use this for planet-level initialization hooks
        /// </summary>
        public static void SubscribeToPlanetCreated(Action<PlanetCreatedEvent> onPlanetCreated)
        {
            if (onPlanetCreated == null)
                throw new ArgumentNullException(nameof(onPlanetCreated));

            _logger.Info("Subscribed to PlanetCreated event");
            RegisterHandler(typeof(PlanetCreatedEvent), onPlanetCreated);
        }

        // ==================== TWITCH INTEGRATION SUBSCRIPTIONS ====================

        /// <summary>
        /// Subscribe to Twitch follower events
        /// Fires when someone follows the Twitch channel
        /// ✅ Use this for immediate reactions to new followers (climate boosts, notifications)
        /// </summary>
        public static void SubscribeToTwitchFollow(Action<TwitchFollowSDKEvent> onTwitchFollow)
        {
            if (onTwitchFollow == null)
                throw new ArgumentNullException(nameof(onTwitchFollow));

            _logger.Info("Subscribed to Twitch follow events");
            RegisterHandler(typeof(TwitchFollowSDKEvent), onTwitchFollow);
        }

        /// <summary>
        /// Subscribe to Twitch bits events
        /// Fires when someone cheers bits in chat
        /// ✅ Use this for scaled effects based on bits amount (major bits = major effects)
        /// </summary>
        public static void SubscribeToTwitchBits(Action<TwitchBitsSDKEvent> onTwitchBits)
        {
            if (onTwitchBits == null)
                throw new ArgumentNullException(nameof(onTwitchBits));

            _logger.Info("Subscribed to Twitch bits events");
            RegisterHandler(typeof(TwitchBitsSDKEvent), onTwitchBits);
        }

        /// <summary>
        /// Subscribe to Twitch subscription events
        /// Fires when someone subscribes to the channel
        /// ✅ Use this for permanent bonuses and celebration effects
        /// </summary>
        public static void SubscribeToTwitchSubscription(Action<TwitchSubscriptionSDKEvent> onTwitchSubscription)
        {
            if (onTwitchSubscription == null)
                throw new ArgumentNullException(nameof(onTwitchSubscription));

            _logger.Info("Subscribed to Twitch subscription events");
            RegisterHandler(typeof(TwitchSubscriptionSDKEvent), onTwitchSubscription);
        }

        /// <summary>
        /// Subscribe to Twitch channel points redemption events
        /// Fires when someone redeems a custom reward with channel points
        /// ✅ Use this for custom viewer interactions with game systems
        /// </summary>
        public static void SubscribeToTwitchChannelPoints(Action<TwitchChannelPointsSDKEvent> onTwitchChannelPoints)
        {
            if (onTwitchChannelPoints == null)
                throw new ArgumentNullException(nameof(onTwitchChannelPoints));

            _logger.Info("Subscribed to Twitch channel points events");
            RegisterHandler(typeof(TwitchChannelPointsSDKEvent), onTwitchChannelPoints);
        }

        /// <summary>
        /// Subscribe to all Twitch events with a unified handler
        /// ✅ Use this for comprehensive Twitch integration or analytics
        /// </summary>
        public static void SubscribeToAllTwitchEvents(Action<TwitchSDKEventBase> onTwitchEvent)
        {
            if (onTwitchEvent == null)
                throw new ArgumentNullException(nameof(onTwitchEvent));

            _logger.Info("Subscribed to all Twitch events");
            
            // Subscribe to each specific event type and forward to unified handler
            SubscribeToTwitchFollow(evt => onTwitchEvent(evt));
            SubscribeToTwitchBits(evt => onTwitchEvent(evt));
            SubscribeToTwitchSubscription(evt => onTwitchEvent(evt));
            SubscribeToTwitchChannelPoints(evt => onTwitchEvent(evt));
        }

        // ==================== GENERIC EVENT SUBSCRIPTIONS ====================

        /// <summary>
        /// Generic event subscription method
        /// Supports any event type with automatic wrapper conversion
        /// </summary>
        public static void Subscribe<T>(Action<T> handler) where T : class, IGameEvent
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _logger.Info($"Subscribed to {typeof(T).Name}");
            RegisterHandler(typeof(T), handler);
        }

        /// <summary>
        /// Subscribe to a named event with object-based handler
        /// For compatibility with string-based event subscriptions
        /// </summary>
        public static void Subscribe(string eventType, Action<object> handler)
        {
            if (string.IsNullOrEmpty(eventType))
                throw new ArgumentException("Event type cannot be null or empty", nameof(eventType));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _logger.Info($"Subscribed to event type: {eventType}");
            
            // Store handler in a special registry for string-based events
            var stringEventType = eventType.GetType();
            if (!_eventHandlers.ContainsKey(stringEventType))
            {
                _eventHandlers[stringEventType] = new List<Delegate>();
            }
            _eventHandlers[stringEventType].Add(handler);
        }

        /// <summary>
        /// Unsubscribe from a named event
        /// </summary>
        public static void Unsubscribe(string eventType, Action<object> handler)
        {
            if (string.IsNullOrEmpty(eventType))
                throw new ArgumentException("Event type cannot be null or empty", nameof(eventType));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var stringEventType = eventType.GetType();
            if (_eventHandlers.TryGetValue(stringEventType, out var handlers))
            {
                handlers.Remove(handler);
                _logger.Info($"Unsubscribed from event type: {eventType}");
            }
        }

        // ==================== EVENT TRIGGERING ====================

        /// <summary>
        /// Trigger an SDK event for all subscribers
        /// Internal method used by game hooks and other SDK components
        /// </summary>
        public static void TriggerEvent<T>(T eventData) where T : class, IGameEvent
        {
            if (eventData == null)
                return;

            var eventType = typeof(T);
            
            try
            {
                // Apply auto-conversion if enabled
                if (_autoConversionEnabled)
                {
                    NativeEventConverter.ConvertEventProperties(eventData);
                }

                // Execute all handlers for this event type
                if (_eventHandlers.TryGetValue(eventType, out var handlers))
                {
                    _logger.Debug($"Triggering {eventType.Name} for {handlers.Count} handlers");
                    
                    foreach (var handler in handlers)
                    {
                        try
                        {
                            if (handler is Action<T> typedHandler)
                            {
                                typedHandler(eventData);
                            }
                            else if (handler is Action simpleHandler)
                            {
                                simpleHandler();
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"Handler failed for {eventType.Name}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to trigger event {eventType.Name}: {ex.Message}");
            }
        }

        // ==================== CONFIGURATION ====================

        /// <summary>
        /// Enable or disable automatic wrapper conversion
        /// </summary>
        public static void SetAutoConversion(bool enabled)
        {
            _autoConversionEnabled = enabled;
            _logger.Info($"Auto-conversion {(enabled ? "enabled" : "disabled")}");
        }

        /// <summary>
        /// Get event system statistics
        /// </summary>
        public static EventSystemStats GetStats()
        {
            var totalHandlers = 0;
            foreach (var handlers in _eventHandlers.Values)
            {
                totalHandlers += handlers.Count;
            }

            return new EventSystemStats
            {
                EventTypeCount = _eventHandlers.Count,
                TotalHandlers = totalHandlers,
                AutoConversionEnabled = _autoConversionEnabled
            };
        }

        /// <summary>
        /// Publish an event to all registered handlers
        /// </summary>
        /// <param name="eventType">Type of the event</param>
        /// <param name="eventData">Event data to publish</param>
        public static void Publish(string eventType, object eventData)
        {
            try
            {
                if (eventData == null) return;

                var dataType = eventData.GetType();
                if (_eventHandlers.TryGetValue(dataType, out var handlers))
                {
                    foreach (var handler in handlers)
                    {
                        try
                        {
                            handler.DynamicInvoke(eventData);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error($"Error invoking event handler for {eventType}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Error publishing event {eventType}: {ex.Message}");
            }
        }

        /// <summary>
        /// Clear all event subscriptions
        /// </summary>
        public static void ClearAllSubscriptions()
        {
            try
            {
                var totalHandlers = _eventHandlers.Values.Sum(handlers => handlers.Count);
                _eventHandlers.Clear();
                _logger.Info($"Cleared {totalHandlers} event handlers from {_eventHandlers.Count} event types");
            }
            catch (Exception ex)
            {
                _logger.Error($"Error clearing subscriptions: {ex.Message}");
            }
        }

        // ==================== INTERNAL METHODS ====================

        private static void RegisterHandler<T>(System.Type eventType, T handler) where T : Delegate
        {
            if (!_eventHandlers.ContainsKey(eventType))
            {
                _eventHandlers[eventType] = new List<Delegate>();
            }
            
            _eventHandlers[eventType].Add(handler);
        }
    }

    /// <summary>
    /// Event system statistics
    /// </summary>
    public class EventSystemStats
    {
        public int EventTypeCount { get; set; }
        public int TotalHandlers { get; set; }
        public bool AutoConversionEnabled { get; set; }
        public int EventsProcessed { get; set; }
        public int ConversionErrors { get; set; }
        public double AverageConversionTimeMs { get; set; }
    }
}