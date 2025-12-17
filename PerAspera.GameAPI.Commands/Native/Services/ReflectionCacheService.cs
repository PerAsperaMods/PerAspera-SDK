using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Logging;

namespace PerAspera.GameAPI.Commands.Native.Services
{
    /// <summary>
    /// High-performance reflection caching service optimized for IL2CPP command creation
    /// Provides thread-safe caching of constructors, methods, and reflection metadata
    /// Follows BepInX 6 best practices for minimal allocation and optimal performance
    /// </summary>
    public sealed class ReflectionCacheService
    {
        private readonly ConcurrentDictionary<string, ConstructorInfo> _constructorCache;
        private readonly ConcurrentDictionary<System.Type, MethodInfo[]> _methodCache;
        private readonly ConcurrentDictionary<string, PropertyInfo> _propertyCache;
        private readonly ConcurrentDictionary<System.Type, object[]> _attributeCache;

        public ReflectionCacheService()
        {
            _constructorCache = new ConcurrentDictionary<string, ConstructorInfo>(StringComparer.Ordinal);
            _methodCache = new ConcurrentDictionary<System.Type, MethodInfo[]>();
            _propertyCache = new ConcurrentDictionary<string, PropertyInfo>(StringComparer.Ordinal);
            _attributeCache = new ConcurrentDictionary<System.Type, object[]>();
        }

        /// <summary>
        /// Build constructor cache for performance optimization
        /// Pre-caches commonly used constructors for faster instantiation
        /// </summary>
        /// <param name="commandTypes">Dictionary of discovered command types</param>
        public void CacheConstructors(ConcurrentDictionary<string, System.Type> commandTypes)
        { // Logging disabledforeach (var kvp in commandTypes)
            {
                var type = kvp.Value;
                try
                {
                    CacheConstructorsForType(type);
                }
                catch (Exception ex)
                { // Logging disabled}
            }

        /// <summary>
        /// Cache all constructors for a specific type with different parameter patterns
        /// </summary>
        /// <param name="type">Type to cache constructors for</param>
        private void CacheConstructorsForType(System.Type type)
        {
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var constructor in constructors)
            {
                var key = GenerateConstructorKey(type, constructor);
                _constructorCache.TryAdd(key, constructor); // Logging disabled}
        }

        /// <summary>
        /// Generate a unique key for constructor caching based on type and parameter signature
        /// </summary>
        /// <param name="type">Type containing the constructor</param>
        /// <param name="constructor">Constructor to generate key for</param>
        /// <returns>Unique cache key</returns>
        private static string GenerateConstructorKey(System.Type type, ConstructorInfo constructor)
        {
            var paramTypes = constructor.GetParameters().Select(p => p.ParameterType.Name);
            var paramSignature = string.Join(",", paramTypes);
            return $"{type.FullName}({paramSignature})";
        }

        /// <summary>
        /// Get optimal constructor for a type with given parameter types
        /// Uses cached constructors for maximum performance
        /// </summary>
        /// <param name="type">Type to create constructor for</param>
        /// <param name="parameterTypes">Parameter types for constructor matching</param>
        /// <returns>Best matching constructor or null if not found</returns>
        public ConstructorInfo GetOptimalConstructor(System.Type type, System.Type[] parameterTypes)
        {
            // Try exact match from cache first
            var paramSignature = string.Join(",", parameterTypes.Select(t => t.Name));
            var exactKey = $"{type.FullName}({paramSignature})";
            
            if (_constructorCache.TryGetValue(exactKey, out var exactMatch))
            {
                return exactMatch;
            }

            // Fallback: Find best matching constructor
            var constructors = GetCachedConstructors(type);
            
            // Find exact parameter match
            var exactConstructor = constructors.FirstOrDefault(c => 
                ParameterTypesMatch(c.GetParameters(), parameterTypes));
            
            if (exactConstructor != null)
            {
                // Cache this combination for future use
                _constructorCache.TryAdd(exactKey, exactConstructor);
                return exactConstructor;
            }

            // Find compatible constructor (with parameter type compatibility)
            var compatibleConstructor = constructors
                .Where(c => c.GetParameters().Length == parameterTypes.Length)
                .FirstOrDefault(c => ParameterTypesCompatible(c.GetParameters(), parameterTypes));

            if (compatibleConstructor != null)
            {
                _constructorCache.TryAdd(exactKey, compatibleConstructor);
            }

            return compatibleConstructor;
        }

        /// <summary>
        /// Get cached constructors for a type
        /// </summary>
        /// <param name="type">Type to get constructors for</param>
        /// <returns>Array of constructors for the type</returns>
        private ConstructorInfo[] GetCachedConstructors(System.Type type)
        {
            return _constructorCache.Values
                .Where(c => c.DeclaringType == type)
                .ToArray();
        }

        /// <summary>
        /// Check if parameter types match exactly
        /// </summary>
        /// <param name="constructorParams">Constructor parameters</param>
        /// <param name="providedTypes">Provided parameter types</param>
        /// <returns>True if types match exactly</returns>
        private static bool ParameterTypesMatch(ParameterInfo[] constructorParams, System.Type[] providedTypes)
        {
            if (constructorParams.Length != providedTypes.Length)
                return false;

            for (int i = 0; i < constructorParams.Length; i++)
            {
                if (constructorParams[i].ParameterType != providedTypes[i])
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Check if parameter types are compatible (includes inheritance and conversion)
        /// </summary>
        /// <param name="constructorParams">Constructor parameters</param>
        /// <param name="providedTypes">Provided parameter types</param>
        /// <returns>True if types are compatible</returns>
        private static bool ParameterTypesCompatible(ParameterInfo[] constructorParams, System.Type[] providedTypes)
        {
            if (constructorParams.Length != providedTypes.Length)
                return false;

            for (int i = 0; i < constructorParams.Length; i++)
            {
                var paramType = constructorParams[i].ParameterType;
                var providedType = providedTypes[i];

                // Exact match
                if (paramType == providedType)
                    continue;

                // Assignable (inheritance/interface)
                if (paramType.IsAssignableFrom(providedType))
                    continue;

                // IL2CPP object compatibility
                if (paramType == typeof(object))
                    continue;

                // Not compatible
                return false;
            }

            return true;
        }

        /// <summary>
        /// Cache methods for a type for reflection optimization
        /// </summary>
        /// <param name="type">Type to cache methods for</param>
        public void CacheMethodsForType(System.Type type)
        {
            if (_methodCache.ContainsKey(type))
                return;

            try
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                    .Where(m => !m.IsGenericMethod && !m.IsAbstract)
                    .ToArray();

                _methodCache.TryAdd(type, methods); // Logging disabled}
            catch (Exception ex)
            { // Logging disabled_methodCache.TryAdd(type, Array.Empty<MethodInfo>());
            }
        }

        /// <summary>
        /// Get cached methods for a type
        /// </summary>
        /// <param name="type">Type to get methods for</param>
        /// <returns>Cached methods array</returns>
        public MethodInfo[] GetCachedMethods(System.Type type)
        {
            if (_methodCache.TryGetValue(type, out var methods))
            {
                return methods;
            }

            // Cache methods if not already cached
            CacheMethodsForType(type);
            return _methodCache.GetValueOrDefault(type, Array.Empty<MethodInfo>());
        }

        /// <summary>
        /// Cache property for fast property access
        /// </summary>
        /// <param name="type">Type containing the property</param>
        /// <param name="propertyName">Name of the property</param>
        /// <returns>Cached PropertyInfo or null if not found</returns>
        public PropertyInfo GetCachedProperty(System.Type type, string propertyName)
        {
            var key = $"{type.FullName}.{propertyName}";
            
            if (_propertyCache.TryGetValue(key, out var property))
            {
                return property;
            }

            try
            {
                property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    _propertyCache.TryAdd(key, property);
                }
                return property;
            }
            catch (Exception ex)
            { // Logging disabledreturn null;
            }
        }

        /// <summary>
        /// Get cached attributes for a type
        /// </summary>
        /// <param name="type">Type to get attributes for</param>
        /// <returns>Cached attributes array</returns>
        public object[] GetCachedAttributes(System.Type type)
        {
            if (_attributeCache.TryGetValue(type, out var attributes))
            {
                return attributes;
            }

            try
            {
                attributes = type.GetCustomAttributes(false);
                _attributeCache.TryAdd(type, attributes);
                return attributes;
            }
            catch (Exception ex)
            { // Logging disabledvar emptyAttributes = Array.Empty<object>();
                _attributeCache.TryAdd(type, emptyAttributes);
                return emptyAttributes;
            }
        }

        /// <summary>
        /// Create instance using cached reflection data
        /// Optimized path for maximum performance with IL2CPP
        /// </summary>
        /// <param name="type">Type to create instance of</param>
        /// <param name="parameters">Constructor parameters</param>
        /// <returns>Created instance or null on failure</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object CreateInstanceFast(System.Type type, object[] parameters)
        {
            try
            {
                var paramTypes = parameters?.Select(p => p?.GetType() ?? typeof(object)).ToArray() ?? Array.Empty<System.Type>();
                var constructor = GetOptimalConstructor(type, paramTypes);
                
                if (constructor == null)
                {
                    // Fallback to Activator for simple cases
                    return Activator.CreateInstance(type, parameters);
                }

                return constructor.Invoke(parameters);
            }
            catch (Exception ex)
            { // Logging disabledreturn null;
            }
        }

        /// <summary>
        /// Get diagnostic information about cache performance
        /// </summary>
        /// <returns>Formatted cache statistics</returns>
        public string GetCacheStatistics()
        {
            var stats = new System.Text.StringBuilder();
            stats.AppendLine("=== ReflectionCacheService Statistics ===");
            stats.AppendLine($"Cached Constructors: {_constructorCache.Count}");
            stats.AppendLine($"Cached Method Groups: {_methodCache.Count}");
            stats.AppendLine($"Total Cached Methods: {_methodCache.Values.Sum(m => m.Length)}");
            stats.AppendLine($"Cached Properties: {_propertyCache.Count}");
            stats.AppendLine($"Cached Attribute Groups: {_attributeCache.Count}");

            return stats.ToString();
        }

        /// <summary>
        /// Clear all caches (for memory management or testing)
        /// </summary>
        public void ClearCaches()
        {
            _constructorCache.Clear();
            _methodCache.Clear();
            _propertyCache.Clear();
            _attributeCache.Clear(); // Logging disabled}
    }
}
