using System;
using System.Collections.Generic;
using PerAspera.GameAPI.Wrappers;
using PerAspera.Core;

namespace PerAspera.GameAPI.Events.Core
{
    /// <summary>
    /// Factory for automatically converting native IL2CPP instances to SDK wrappers
    /// Provides type-safe access to game objects through SDK wrapper classes
    /// </summary>
    public static class WrapperFactory
    {
        private static readonly Dictionary<System.Type, Func<object, object>> _converters = new();
        private static readonly object _lock = new object();
        private static readonly LogAspera _logger = new LogAspera("WrapperFactory");

        static WrapperFactory()
        {
            RegisterConverters();
        }

        /// <summary>
        /// Convert a native IL2CPP instance to its corresponding SDK wrapper
        /// Returns null if conversion fails or input is null
        /// </summary>
        /// <typeparam name="T">Expected wrapper type</typeparam>
        /// <param name="nativeInstance">Native IL2CPP instance</param>
        /// <returns>SDK wrapper instance or null</returns>
        public static T? ConvertToWrapper<T>(object? nativeInstance) where T : class
        {
            if (nativeInstance == null) return null;

            try
            {
                var result = ConvertToWrapper(nativeInstance, typeof(T));
                return result as T;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to convert {nativeInstance.GetType().Name} to {typeof(T).Name}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Convert a native IL2CPP instance to SDK wrapper (type determined at runtime)
        /// </summary>
        /// <param name="nativeInstance">Native IL2CPP instance</param>
        /// <param name="targetWrapperType">Target wrapper type</param>
        /// <returns>SDK wrapper instance or null</returns>
        public static object? ConvertToWrapper(object? nativeInstance, System.Type? targetWrapperType = null)
        {
            if (nativeInstance == null) return null;

            var nativeType = nativeInstance.GetType();
            
            // Auto-detect target wrapper type if not specified
            if (targetWrapperType == null)
            {
                targetWrapperType = GetWrapperTypeForNative(nativeType);
                if (targetWrapperType == null)
                {
                    _logger.Warning($"No wrapper type found for native type: {nativeType.Name}");
                    return nativeInstance; // Return as-is if no wrapper available
                }
            }

            // Get converter function
            if (_converters.TryGetValue(targetWrapperType, out var converter))
            {
                return converter(nativeInstance);
            }

            // Try reflection-based conversion as fallback
            return TryReflectionConversion(nativeInstance, targetWrapperType);
        }

        /// <summary>
        /// Register all known native type → wrapper converters
        /// </summary>
        private static void RegisterConverters()
        {
            lock (_lock)
            {
                // Building wrapper
                _converters[typeof(Building)] = native => new PerAspera.GameAPI.Wrappers.Building(native);
                
                // Drone wrapper
                _converters[typeof(Drone)] = native => new PerAspera.GameAPI.Wrappers.Drone(native);
                
                // Universe wrapper  
                _converters[typeof(Universe)] = native => new PerAspera.GameAPI.Wrappers.Universe(native);
                
                // Planet wrapper
                _converters[typeof(Planet)] = native => new PerAspera.GameAPI.Wrappers.Planet(native);
                
                // BaseGame wrapper
                _converters[typeof(BaseGame)] = native => new PerAspera.GameAPI.Wrappers.BaseGame(native);
                
                // Faction wrapper
                _converters[typeof(Faction)] = native => new PerAspera.GameAPI.Wrappers.Faction(native);
                
                // Technology wrapper
                _converters[typeof(Technology)] = native => new PerAspera.GameAPI.Wrappers.Technology(native);
                
                // BuildingType wrapper
                _converters[typeof(BuildingType)] = native => new PerAspera.GameAPI.Wrappers.BuildingType(native);
                
                // ResourceType wrapper
                _converters[typeof(ResourceType)] = native => new PerAspera.GameAPI.Wrappers.ResourceType(native);
                
                // Knowledge wrapper
                _converters[typeof(Knowledge)] = native => new PerAspera.GameAPI.Wrappers.Knowledge(native);
                
                _logger.Info($"Registered {_converters.Count} wrapper converters");
            }
        }

        /// <summary>
        /// Attempt to detect wrapper type for a native type based on naming conventions
        /// </summary>
        /// <param name="nativeType">Native IL2CPP type</param>
        /// <returns>Corresponding wrapper type or null</returns>
        private static System.Type? GetWrapperTypeForNative(System.Type nativeType)
        {
            var nativeTypeName = nativeType.Name;
            
            // Try direct mapping (BuildingNative → Building)
            if (nativeTypeName.EndsWith("Native"))
            {
                nativeTypeName = nativeTypeName.Substring(0, nativeTypeName.Length - 6);
            }
            
            // Look for wrapper type in SDK assemblies
            var wrapperTypeName = $"PerAspera.GameAPI.Wrappers.{nativeTypeName}";
            var wrapperType = System.Type.GetType(wrapperTypeName);
            
            return wrapperType;
        }

        /// <summary>
        /// Fallback reflection-based converter for types not explicitly registered
        /// </summary>
        /// <param name="nativeInstance">Native instance</param>
        /// <param name="wrapperType">Target wrapper type</param>
        /// <returns>Wrapper instance or null</returns>
        private static object? TryReflectionConversion(object nativeInstance, System.Type wrapperType)
        {
            try
            {
                // Try constructor that takes native instance
                var constructor = wrapperType.GetConstructor(new[] { typeof(object) });
                if (constructor != null)
                {
                    return constructor.Invoke(new[] { nativeInstance });
                }

                _logger.Warning($"No suitable constructor found for {wrapperType.Name}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.Error($"Reflection conversion failed for {wrapperType.Name}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Check if a wrapper type is registered for conversion
        /// </summary>
        /// <param name="wrapperType">Wrapper type to check</param>
        /// <returns>True if converter is available</returns>
        public static bool IsWrapperSupported(System.Type wrapperType)
        {
            return _converters.ContainsKey(wrapperType);
        }

        /// <summary>
        /// Get all supported wrapper types
        /// </summary>
        /// <returns>Collection of supported wrapper types</returns>
        public static IReadOnlyCollection<System.Type> GetSupportedWrapperTypes()
        {
            return _converters.Keys;
        }

        /// <summary>
        /// Register a custom converter for a specific wrapper type
        /// </summary>
        /// <typeparam name="T">Wrapper type</typeparam>
        /// <param name="converter">Conversion function</param>
        public static void RegisterConverter<T>(Func<object, T> converter) where T : class
        {
            lock (_lock)
            {
                _converters[typeof(T)] = native => converter(native);
                _logger.Info($"Registered custom converter for {typeof(T).Name}");
            }
        }
    }
}