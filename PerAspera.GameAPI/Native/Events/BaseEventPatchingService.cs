using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Native.Events
{
    /// <summary>
    /// Base class for all event patching services
    /// Provides common functionality and patterns for Harmony IL2CPP patching
    /// </summary>
    public abstract class BaseEventPatchingService
    {
        /// <summary>
        /// Logger instance for event patching operations
        /// </summary>
        protected readonly LogAspera _log;
        
        /// <summary>
        /// Dictionary tracking patched methods and their event types
        /// </summary>
        protected readonly Dictionary<string, string> _patchedMethods;
        
        /// <summary>
        /// Context information for patch operations
        /// </summary>
        protected readonly Dictionary<string, object> _patchContext;
        
        /// <summary>
        /// Harmony instance for IL2CPP patching
        /// </summary>
        protected readonly Harmony _harmony;

        /// <summary>
        /// Initialize base event patching service with logging and Harmony instance
        /// </summary>
        /// <param name="serviceName">Name of the service for logging purposes</param>
        /// <param name="harmony">Harmony instance for patching</param>
        protected BaseEventPatchingService(string serviceName, Harmony harmony)
        {
            _log = new LogAspera($"GameAPI.Events.{serviceName}");
            _patchedMethods = new Dictionary<string, string>();
            _patchContext = new Dictionary<string, object>();
            _harmony = harmony ?? throw new ArgumentNullException(nameof(harmony));
        }

        /// <summary>
        /// Initialize event hooks for this service
        /// </summary>
        /// <returns>Number of successfully patched methods</returns>
        public abstract int InitializeEventHooks();

        /// <summary>
        /// Get event type name for this service
        /// </summary>
        /// <returns>Event type identifier</returns>
        public abstract string GetEventType();

        /// <summary>
        /// Get statistics about patched methods for this service
        /// </summary>
        /// <returns>Dictionary of method name to event type mappings</returns>
        public Dictionary<string, string> GetPatchedMethods()
        {
            return new Dictionary<string, string>(_patchedMethods);
        }

        /// <summary>
        /// Create safe method patch with error handling and validation
        /// </summary>
        /// <param name="targetType">Type containing the method to patch</param>
        /// <param name="methodName">Name of the method to patch</param>
        /// <param name="eventType">Type of event for tracking</param>
        /// <param name="prefix">Optional prefix method</param>
        /// <param name="postfix">Optional postfix method</param>
        /// <returns>True if patch was successfully applied</returns>
        protected bool CreateSafeMethodPatch(System.Type targetType, string methodName, string eventType, 
            MethodInfo prefix = null, MethodInfo postfix = null)
        {
            try
            {
                var method = targetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                if (method == null)
                {
                    _log.Debug($"Method {methodName} not found on {targetType.Name}");
                    return false;
                }

                var harmonyPrefix = prefix != null ? new HarmonyMethod(prefix) : null;
                var harmonyPostfix = postfix != null ? new HarmonyMethod(postfix) : null;

                _harmony.Patch(method, prefix: harmonyPrefix, postfix: harmonyPostfix);
                
                var patchKey = $"{targetType.Name}.{methodName}";
                _patchedMethods[patchKey] = eventType;
                
                _log.Debug($"✓ Patched {patchKey} for {eventType} events");
                return true;
            }
            catch (Exception ex)
            {
                _log.Warning($"Failed to patch {targetType.Name}.{methodName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Extract value from method arguments based on parameter patterns
        /// </summary>
        /// <param name="args">Method arguments</param>
        /// <returns>Extracted value or null</returns>
        protected object ExtractValueFromArgs(object[] args)
        {
            if (args == null || args.Length == 0)
                return null;

            // Common patterns for Per Aspera method arguments
            // First argument is often the new value
            return args[0];
        }

        /// <summary>
        /// Publish event through the ModEventBus
        /// </summary>
        /// <param name="eventName">Name of the event</param>
        /// <param name="eventData">Event data object</param>
        protected void PublishEvent(string eventName, object eventData)
        {
            try
            {
                ModEventBus.Publish(eventName, eventData);
                _log.Debug($"Published {eventName} event");
            }
            catch (Exception ex)
            {
                _log.Warning($"Failed to publish {eventName} event: {ex.Message}");
            }
        }

        /// <summary>
        /// Store context data for method patches
        /// </summary>
        /// <param name="key">Context key</param>
        /// <param name="value">Context value</param>
        protected void StoreContext(string key, object value)
        {
            _patchContext[key] = value;
        }

        /// <summary>
        /// Retrieve context data from method patches
        /// </summary>
        /// <param name="key">Context key</param>
        /// <returns>Context value or null if not found</returns>
        protected object GetContext(string key)
        {
            return _patchContext.GetValueOrDefault(key);
        }

        /// <summary>
        /// Clear context data (useful for cleanup)
        /// </summary>
        protected void ClearContext()
        {
            _patchContext.Clear();
        }

        /// <summary>
        /// Generate event data object for common event patterns
        /// </summary>
        /// <param name="instance">Object instance where change occurred</param>
        /// <param name="oldValue">Previous value</param>
        /// <param name="newValue">New value</param>
        /// <param name="methodName">Method that triggered the change</param>
        /// <returns>Formatted event data object</returns>
        protected object CreateEventData(object instance, object oldValue, object newValue, string methodName)
        {
            return new
            {
                Instance = instance,
                OldValue = oldValue,
                NewValue = newValue,
                Method = methodName,
                Timestamp = DateTime.UtcNow,
                EventType = GetEventType()
            };
        }

        /// <summary>
        /// Validate that a type has the expected method for patching
        /// </summary>
        /// <param name="type">Type to validate</param>
        /// <param name="methodName">Method name to check</param>
        /// <returns>True if method exists and is patchable</returns>
        protected bool ValidateMethodForPatching(System.Type type, string methodName)
        {
            if (type == null)
                return false;

            try
            {
                var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                if (method == null)
                    return false;

                // Check if method is virtual or can be patched
                if (method.IsAbstract)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                _log.Debug($"Method validation failed for {type.Name}.{methodName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get friendly type name for logging
        /// </summary>
        /// <param name="type">Type to get name for</param>
        /// <returns>Friendly type name</returns>
        protected string GetFriendlyTypeName(System.Type type)
        {
            if (type == null)
                return "Unknown";

            return type.Name.Replace("IL2CPP", "").Replace("Wrapper", "");
        }
    }
}

