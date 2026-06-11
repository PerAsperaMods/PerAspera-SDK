using System;
using PerAspera.GameAPI.Events.Core;
using PerAspera.GameAPI;
using PerAspera.Core;
using EnhancedEventBus = PerAspera.GameAPI.Events.Integration.EnhancedEventBus;

namespace PerAspera.GameAPI.Events.Integration
{
    /// <summary>
    /// Lightweight integration bridge for the SDK event system.
    /// Performs one-time post-load initialization (instance registry refresh).
    /// All native event routing is now handled by <see cref="Native.NativeEventHub"/>.
    /// </summary>
    public static class EventSystemIntegration
    {
        private static readonly LogAspera _logger = new LogAspera("EventSystemIntegration");
        private static bool _isInitialized = false;

        /// <summary>
        /// Initialize the event system integration.
        /// Subscribes to <see cref="SDK.GameFullyLoadedEvent"/> to refresh the instance registry.
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized)
                return;

            try
            {
                EnhancedEventBus.SetAutoConversion(true);

                // Refresh InstanceManager after game load — instances aren't available during plugin Load()
                EnhancedEventBus.SubscribeToGameFullyLoaded(_ =>
                {
                    try { GameTypeInitializer.RefreshInstanceRegistry(); }
                    catch (Exception ex) { _logger.Error($"Failed to refresh instance registry: {ex.Message}"); }
                });

                _isInitialized = true;
                _logger.Info("Event system integration initialized");
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to initialize event system integration: {ex.Message}");
                throw;
            }
        }

        /// <summary>Shutdown the event system integration.</summary>
        public static void Shutdown()
        {
            if (!_isInitialized)
                return;

            EnhancedEventBus.ClearAllSubscriptions();
            _isInitialized = false;
            _logger.Info("Event system integration shut down");
        }

        /// <summary>True if integration is active.</summary>
        public static bool IsInitialized => _isInitialized;
    }
}
