using System;
using System.Linq;
using System.Reflection;

namespace PerAspera.Core.IL2CPP
{
    /// <summary>
    /// Reflection helpers for IL2CPP environments
    /// Centralizes type discovery functions for decompiled assemblies
    /// CORRECTED: Removed duplicated extension methods (use IL2CPP native extensions instead)
    /// </summary>
    public static class ReflectionHelpers
    {
        /// <summary>
        /// Finds a type by name across all loaded assemblies
        /// </summary>
        /// <param name="typeName">Full or simple type name</param>
        /// <returns>Type if found, null otherwise</returns>
        public static System.Type? FindType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    // Direct type lookup
                    var type = assembly.GetType(typeName);
                    if (type != null) 
                        return type;
                    
                    // Search in exported types
                    type = assembly.GetTypes()
                        .FirstOrDefault(t => t.Name == typeName || t.FullName == typeName);
                    if (type != null) 
                        return type;
                }
                catch
                {
                    // Ignore assemblies that cannot be examined
                    continue;
                }
            }
            
            return null;
        }

        /// <summary>
        /// Finds all types matching the given predicate
        /// </summary>
        /// <param name="predicate">Condition to match types</param>
        /// <returns>Array of matching types</returns>
        public static System.Type[] FindTypes(Func<System.Type, bool> predicate)
        {
            if (predicate == null)
                return System.Array.Empty<System.Type>();

            var results = new System.Collections.Generic.List<System.Type>();
            
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var matchingTypes = assembly.GetTypes().Where(predicate);
                    results.AddRange(matchingTypes);
                }
                catch
                {
                    // Ignore problematic assemblies
                    continue;
                }
            }
            
            return results.ToArray();
        }

        /// <summary>
        /// Checks if a type exists without throwing exceptions
        /// </summary>
        /// <param name="typeName">Type name to check</param>
        /// <returns>True if type exists</returns>
        public static bool TypeExists(string typeName)
        {
            return FindType(typeName) != null;
        }

        /// <summary>
        /// Gets all types in a specific namespace
        /// </summary>
        /// <param name="namespaceName">Namespace to search</param>
        /// <returns>Types in the namespace</returns>
        public static System.Type[] GetTypesInNamespace(string namespaceName)
        {
            if (string.IsNullOrEmpty(namespaceName))
                return System.Array.Empty<System.Type>();

            return FindTypes(t => t.Namespace == namespaceName);
        }

        /// <summary>
        /// Gets all types that inherit from a specific base type
        /// </summary>
        /// <param name="baseType">Base type to search for</param>
        /// <returns>Derived types</returns>
        public static System.Type[] GetDerivedTypes(System.Type baseType)
        {
            if (baseType == null)
                return System.Array.Empty<System.Type>();

            return FindTypes(t => t != baseType && baseType.IsAssignableFrom(t));
        }

        /// <summary>
        /// Safe method invocation with error handling
        /// </summary>
        /// <param name="instance">Target instance</param>
        /// <param name="methodName">Method name</param>
        /// <param name="parameters">Method parameters</param>
        /// <returns>Method result or null if failed</returns>
        public static object? SafeInvoke(object instance, string methodName, params object[] parameters)
        {
            if (instance == null || string.IsNullOrEmpty(methodName))
                return null;

            try
            {
                var method = instance.GetType().GetMethod(methodName);
                return method?.Invoke(instance, parameters);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Safe field/property access
        /// </summary>
        /// <param name="instance">Target instance</param>
        /// <param name="memberName">Field or property name</param>
        /// <returns>Value or null if failed</returns>
        public static object? SafeGetValue(object instance, string memberName)
        {
            if (instance == null || string.IsNullOrEmpty(memberName))
                return null;

            try
            {
                var type = instance.GetType();
                
                // Try property first
                var property = type.GetProperty(memberName);
                if (property?.CanRead == true)
                    return property.GetValue(instance);

                // Try field second
                var field = type.GetField(memberName);
                return field?.GetValue(instance);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get singleton instance of specified type
        /// </summary>
        /// <typeparam name="T">Type to get instance for</typeparam>
        /// <returns>Singleton instance if found</returns>
        public static T? GetSingletonInstance<T>() where T : class
        {
            try
            {
                var type = typeof(T);
                
                // Try static Instance property first
                var instanceProperty = type.GetProperty("Instance", 
                    BindingFlags.Public | BindingFlags.Static);
                
                if (instanceProperty != null)
                {
                    var result = instanceProperty.GetValue(null) as T;
                    if (result != null) return result;
                }
                
                // Try static Get() method
                var getMethod = type.GetMethod("Get", 
                    BindingFlags.Public | BindingFlags.Static, 
                    null, 
                    System.Type.EmptyTypes, 
                    null);
                    
                if (getMethod != null)
                {
                    var result = getMethod.Invoke(null, null) as T;
                    if (result != null) return result;
                }
                
                return null;
            }
            catch (Exception)
            {
                // Failed to get singleton instance - return null silently
                return null;
            }
        }

        /// <summary>
        /// Get singleton instance by type
        /// </summary>
        /// <param name="type">Type to get instance for</param>
        /// <returns>Singleton instance if found</returns>
        public static object? GetSingletonInstance(System.Type type)
        {
            try
            {
                // Try static Instance property first
                var instanceProperty = type.GetProperty("Instance", 
                    BindingFlags.Public | BindingFlags.Static);
                
                if (instanceProperty != null)
                {
                    var result = instanceProperty.GetValue(null);
                    if (result != null) return result;
                }
                
                // Try static Get() method
                var getMethod = type.GetMethod("Get", 
                    BindingFlags.Public | BindingFlags.Static, 
                    null, 
                    System.Type.EmptyTypes, 
                    null);
                    
                if (getMethod != null)
                {
                    var result = getMethod.Invoke(null, null);
                    if (result != null) return result;
                }
                
                return null;
            }
            catch (Exception)
            {
                // Failed to get singleton instance - return null silently
                return null;
            }
        }
    }
}
