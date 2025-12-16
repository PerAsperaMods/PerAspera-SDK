#nullable enable
using System;
using System.Collections.Generic;
using PerAspera.Core;

namespace PerAspera.GameAPI.Native
{
    /// <summary>
    /// ModEventBus - Proxy for event publishing from native event patches
    /// GameAPI doesn't depend on ModSDK, so this provides a callback-based system
    /// ModSDK will hook into these callbacks to integrate with its EventSystem
    /// </summary>
    public static class ModEventBus
    {
        private static readonly LogAspera _log = new LogAspera("ModEventBus");
        
        // Callback that ModSDK will register during initialization
        public static Action<string, object>? OnEventPublish { get; set; }

        /// <summary>
        /// Publish an event - called by NativeEventPatcher patches
        /// ModSDK will hook this to forward to its EventSystem
        /// </summary>
        public static void Publish(string eventName, object eventData)
        {
            if (string.IsNullOrEmpty(eventName))
                return;

            try
            {
                // Invoke the callback if registered (ModSDK will set this)
                OnEventPublish?.Invoke(eventName, eventData);
            }
            catch (Exception ex)
            {
                _log.Warning($"Error publishing event '{eventName}': {ex.Message}");
            }
        }
    }
}
