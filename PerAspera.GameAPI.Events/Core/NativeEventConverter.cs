using System;
using System.Reflection;
using System.Collections.Generic;
using PerAspera.Core;

namespace PerAspera.GameAPI.Events.Core
{
    /// <summary>
    /// Automatically converts native event data properties to SDK wrappers
    /// Processes event objects and replaces native IL2CPP instances with type-safe wrappers
    /// </summary>
    public static class NativeEventConverter
    {
        private static readonly LogAspera _logger = new LogAspera("NativeEventConverter");
        private static readonly HashSet<string> _excludedProperties = new() { "EventType", "Timestamp", "Source", "Metadata" };

        /// <summary>
        /// Convert all compatible properties in an event from native instances to SDK wrappers
        /// Modifies the event object in-place
        /// </summary>
        /// <param name="eventObject">Event object to convert</param>
        /// <returns>True if any conversions were performed</returns>
        public static bool ConvertEventProperties(object eventObject)
        {
            if (eventObject == null)
                return false;

            var converted = false;
            var eventType = eventObject.GetType();
            var properties = eventType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (ShouldSkipProperty(property))
                    continue;

                try
                {
                    var currentValue = property.GetValue(eventObject);
                    if (currentValue == null)
                        continue;

                    var convertedValue = ConvertPropertyValue(currentValue, property);
                    if (convertedValue != currentValue)
                    {
                        // Only set if we actually converted something
                        if (property.CanWrite)
                        {
                            property.SetValue(eventObject, convertedValue);
                            converted = true;
                            _logger.Debug($"Converted {eventType.Name}.{property.Name} to wrapper");
                        }
                        else
                        {
                            _logger.Warning($"Property {eventType.Name}.{property.Name} is read-only, cannot set wrapper");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error($"Failed to convert property {eventType.Name}.{property.Name}: {ex.Message}");
                }
            }

            return converted;
        }

        /// <summary>
        /// Convert a specific property value from native to wrapper if possible
        /// </summary>
        /// <param name="value">Current property value</param>
        /// <param name="property">Property info for context</param>
        /// <returns>Converted value (wrapper) or original value</returns>
        private static object ConvertPropertyValue(object value, PropertyInfo property)
        {
            if (value == null)
                return value;

            var valueType = value.GetType();
            var propertyType = property.PropertyType;

            // Skip if already a wrapper (type is in SDK namespace)
            if (IsWrapperType(valueType))
                return value;

            // Skip primitive types and strings
            if (valueType.IsPrimitive || valueType == typeof(string) || valueType == typeof(DateTime))
                return value;

            // Try to convert using WrapperFactory
            var converted = WrapperFactory.ConvertToWrapper(value, propertyType);
            
            // If conversion succeeded and types are compatible, use converted value
            if (converted != null && propertyType.IsAssignableFrom(converted.GetType()))
            {
                return converted;
            }

            // If target is object type, try auto-detection
            if (propertyType == typeof(object))
            {
                var autoConverted = WrapperFactory.ConvertToWrapper(value);
                if (autoConverted != null && autoConverted != value)
                {
                    return autoConverted;
                }
            }

            return value; // Return original if no conversion possible
        }

        /// <summary>
        /// Determine if a property should be skipped during conversion
        /// </summary>
        /// <param name="property">Property to check</param>
        /// <returns>True if property should be skipped</returns>
        private static bool ShouldSkipProperty(PropertyInfo property)
        {
            // Skip excluded base properties
            if (_excludedProperties.Contains(property.Name))
                return true;

            // Skip if property has custom attribute indicating no conversion
            if (property.GetCustomAttribute<NoWrapperConversionAttribute>() != null)
                return true;

            // Skip indexers
            if (property.GetIndexParameters().Length > 0)
                return true;

            return false;
        }

        /// <summary>
        /// Check if a type is likely a wrapper type (in SDK namespace)
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>True if type appears to be a wrapper</returns>
        private static bool IsWrapperType(System.Type type)
        {
            var typeName = type.FullName ?? type.Name;
            
            return typeName.StartsWith("PerAspera.GameAPI.Wrappers.") ||
                   typeName.StartsWith("PerAspera.GameAPI.Events.") ||
                   type.BaseType?.Name == "WrapperBase";
        }

        /// <summary>
        /// Batch convert multiple event objects
        /// </summary>
        /// <param name="events">Collection of events to convert</param>
        /// <returns>Number of events that had conversions applied</returns>
        public static int ConvertEventsBatch(IEnumerable<object> events)
        {
            var convertedCount = 0;
            
            foreach (var eventObj in events)
            {
                if (ConvertEventProperties(eventObj))
                {
                    convertedCount++;
                }
            }

            return convertedCount;
        }

        /// <summary>
        /// Create a copy of an event object with all properties converted to wrappers
        /// Useful when original event should remain unchanged
        /// </summary>
        /// <typeparam name="T">Event type</typeparam>
        /// <param name="originalEvent">Original event</param>
        /// <returns>New event instance with converted properties</returns>
        public static T? ConvertEventCopy<T>(T originalEvent) where T : class
        {
            if (originalEvent == null)
                return null;

            try
            {
                // Create a copy using reflection (basic implementation)
                var eventType = typeof(T);
                var copy = Activator.CreateInstance(eventType) as T;
                
                if (copy == null)
                    return null;

                // Copy all properties
                var properties = eventType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                foreach (var property in properties)
                {
                    if (!property.CanRead || !property.CanWrite)
                        continue;

                    var value = property.GetValue(originalEvent);
                    property.SetValue(copy, value);
                }

                // Convert the copy
                ConvertEventProperties(copy);
                return copy;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to create converted copy of {typeof(T).Name}: {ex.Message}");
                return null;
            }
        }
    }

    /// <summary>
    /// Attribute to mark properties that should not be converted to wrappers
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class NoWrapperConversionAttribute : Attribute
    {
    }
}