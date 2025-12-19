using System;
using System.Collections.Generic;
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