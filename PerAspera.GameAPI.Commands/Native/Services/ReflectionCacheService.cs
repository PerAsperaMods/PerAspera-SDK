using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.Native.Services
{
    /// <summary>
    /// Service for caching reflection data to improve performance
    /// Provides fast access to constructors, methods, properties, and attributes
    /// </summary>
    public class ReflectionCacheService
    {
        private readonly ConcurrentDictionary<string, ConstructorInfo> _constructorCache;
        private readonly ConcurrentDictionary<System.Type, MethodInfo[]> _methodCache;
        private readonly ConcurrentDictionary<System.Type, PropertyInfo[]> _propertyCache;
        private readonly ConcurrentDictionary<System.Type, Attribute[]> _attributeCache;

        /// <summary>
        /// Initialize a new ReflectionCacheService instance
        /// </summary>
        public ReflectionCacheService()
        {
            _constructorCache = new ConcurrentDictionary<string, ConstructorInfo>();
            _methodCache = new ConcurrentDictionary<System.Type, MethodInfo[]>();
            _propertyCache = new ConcurrentDictionary<System.Type, PropertyInfo[]>();
            _attributeCache = new ConcurrentDictionary<System.Type, Attribute[]>();
        }

        /// <summary>
        /// Cache constructors for all provided command types
        /// </summary>
        public void CacheConstructors(ConcurrentDictionary<string, System.Type> commandTypes)
        {
            foreach (var kvp in commandTypes)
            {
                var type = kvp.Value;
                try
                {
                    CacheConstructorsForType(type);
                }
                catch (Exception)
                {
                    // Constructor caching failed - continue
                }
            }
        }

        /// <summary>
        /// Cache all constructors for a specific type
        /// </summary>
        private void CacheConstructorsForType(System.Type type)
        {
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var constructor in constructors)
            {
                var parameters = constructor.GetParameters();
                var key = $"{type.FullName}_{parameters.Length}";
                
                _constructorCache.TryAdd(key, constructor);
            }
        }

        /// <summary>
        /// Get cached constructor for a type with specific parameter count
        /// </summary>
        public ConstructorInfo GetCachedConstructor(System.Type type, int parameterCount)
        {
            var key = $"{type.FullName}_{parameterCount}";
            if (_constructorCache.TryGetValue(key, out var constructor))
                return constructor;
                
            // Cache miss - try to find and cache
            try
            {
                var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
                var found = constructors.FirstOrDefault(c => c.GetParameters().Length == parameterCount);
                
                if (found != null)
                {
                    _constructorCache.TryAdd(key, found);
                }
                
                return found;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Cache all methods for a type
        /// </summary>
        public void CacheMethodsForType(System.Type type)
        {
            try
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                _methodCache.TryAdd(type, methods);
            }
            catch (Exception)
            {
                // Method caching failed
            }
        }

        /// <summary>
        /// Get cached methods for a type
        /// </summary>
        public MethodInfo[] GetCachedMethods(System.Type type)
        {
            if (_methodCache.TryGetValue(type, out var methods))
                return methods;
            
            // Cache miss - try to cache and return
            try
            {
                CacheMethodsForType(type);
                if (_methodCache.TryGetValue(type, out var newMethods))
                    return newMethods;
            }
            catch (Exception)
            {
                // Method access failed
            }
            
            return Array.Empty<MethodInfo>();
        }

        /// <summary>
        /// Get a specific property from cache
        /// </summary>
        public PropertyInfo GetCachedProperty(System.Type type, string propertyName)
        {
            try
            {
                if (!_propertyCache.TryGetValue(type, out var properties))
                {
                    properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    _propertyCache.TryAdd(type, properties);
                }

                return properties.FirstOrDefault(p => p.Name == propertyName);
            }
            catch (Exception)
            {
                // Property access failed
                return null;
            }
        }

        /// <summary>
        /// Get cached attributes for a type
        /// </summary>
        public Attribute[] GetCachedAttributes(System.Type type)
        {
            if (_attributeCache.TryGetValue(type, out var attributes))
                return attributes;

            try
            {
                attributes = type.GetCustomAttributes().ToArray();
                _attributeCache.TryAdd(type, attributes);
                return attributes;
            }
            catch (Exception)
            {
                var emptyAttributes = Array.Empty<Attribute>();
                _attributeCache.TryAdd(type, emptyAttributes);
                return emptyAttributes;
            }
        }

        /// <summary>
        /// Create instance using cached reflection data
        /// </summary>
        public object CreateInstanceFast(System.Type type, object[] parameters)
        {
            try
            {
                var constructor = GetCachedConstructor(type, parameters?.Length ?? 0);
                if (constructor != null)
                {
                    return constructor.Invoke(parameters);
                }
                
                // Fallback to Activator
                return Activator.CreateInstance(type, parameters);
            }
            catch (Exception)
            {
                // Fast instance creation failed
                return null;
            }
        }

        /// <summary>
        /// Clear all caches
        /// </summary>
        public void ClearCaches()
        {
            _constructorCache.Clear();
            _methodCache.Clear();
            _propertyCache.Clear();
            _attributeCache.Clear();
        }

        /// <summary>
        /// Get cache statistics for diagnostics
        /// </summary>
        /// <returns>String with cache statistics information</returns>
        public string GetCacheStatistics()
        {
            return $"Cached Constructors: {_constructorCache.Count}\n" +
                   $"Cached Methods: {_methodCache.Count}\n" +
                   $"Cached Properties: {_propertyCache.Count}\n" +
                   $"Cached Attributes: {_attributeCache.Count}";
        }
    }
}
