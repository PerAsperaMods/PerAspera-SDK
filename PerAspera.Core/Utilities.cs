using System;
using System.Linq;
using System.Reflection;

namespace PerAspera.Core
{
    /// <summary>
    /// General utility functions for Per Aspera modding
    /// Includes reflection helpers, type utilities, and common operations
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Thread-safe logging helper
        /// </summary>
        private static void Log(Action<string>? logger, string message)
        {
            try
            {
                logger?.Invoke(message);
            }
            catch
            {
                // Ignore logging failures
            }
        }

        //------------------------------------------------------
        // REFLECTION UTILITIES
        //------------------------------------------------------

        /// <summary>
        /// Sets a field or property value using reflection
        /// </summary>
        /// <param name="obj">Target object</param>
        /// <param name="fieldName">Field or property name</param>
        /// <param name="value">Value to set</param>
        public static void SetFieldOrProp(object obj, string fieldName, object value)
        {
            if (obj == null || string.IsNullOrEmpty(fieldName)) 
                return;

            var type = obj.GetType();

            // Try field first
            var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (field != null) 
            { 
                field.SetValue(obj, value); 
                return; 
            }

            // Try property second
            var prop = type.GetProperty(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (prop != null && prop.CanWrite) 
            {
                prop.SetValue(obj, value);
            }
        }

        /// <summary>
        /// Gets a float value from field or property, with default fallback
        /// </summary>
        /// <param name="obj">Target object</param>
        /// <param name="fieldName">Field or property name</param>
        /// <param name="defaultValue">Default value if not found</param>
        /// <returns>Float value or default</returns>
        public static float GetFloatFieldOrProp(object obj, string fieldName, float defaultValue = 0f)
        {
            if (obj == null || string.IsNullOrEmpty(fieldName)) 
                return defaultValue;

            var type = obj.GetType();

            // Try field first
            var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (field != null && field.GetValue(obj) is float fieldValue) 
                return fieldValue;

            // Try property second
            var prop = type.GetProperty(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            if (prop != null && prop.GetValue(obj) is float propValue) 
                return propValue;

            return defaultValue;
        }

        //------------------------------------------------------
        // MULTI-NAME GETTER (FIELD / PROPERTY / METHOD)
        //------------------------------------------------------

        /// <summary>
        /// Gets member value by trying multiple names (field/property/method)
        /// </summary>
        /// <typeparam name="T">Expected return type</typeparam>
        /// <param name="instance">Target object instance</param>
        /// <param name="names">Array of possible member names to try</param>
        /// <param name="log">Optional logging callback</param>
        /// <returns>Value cast to T or null</returns>
        public static T? GetMemberValue<T>(object instance, string[] names, Action<string>? log = null) where T : class
        {
            var val = GetMemberValue(instance, names, log);
            return val as T;
        }

        /// <summary>
        /// Gets member value by trying multiple names (field/property/method)
        /// </summary>
        /// <param name="instance">Target object instance</param>
        /// <param name="names">Array of possible member names to try</param>
        /// <param name="log">Optional logging callback</param>
        /// <returns>Member value or null</returns>
        public static object? GetMemberValue(object instance, string[] names, Action<string>? log = null)
        {
            if (instance == null)
            {
                Log(log, "GetMemberValue: instance is NULL");
                return null;
            }

            var type = instance.GetType();

            foreach (var name in names)
            {
                if (string.IsNullOrEmpty(name))
                    continue;

                // Try method first (no parameters)
                var method = type.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (method != null && method.GetParameters().Length == 0)
                {
                    try 
                    { 
                        return method.Invoke(instance, null); 
                    }
                    catch (Exception ex) 
                    { 
                        Log(log, $"Exception calling method {name}: {ex.Message}"); 
                    }
                }

                // Try property second
                var prop = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (prop != null && prop.CanRead)
                {
                    try 
                    { 
                        return prop.GetValue(instance); 
                    }
                    catch (Exception ex) 
                    { 
                        Log(log, $"Exception accessing property {name}: {ex.Message}"); 
                    }
                }

                // Try field last
                var field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                if (field != null)
                {
                    try 
                    { 
                        return field.GetValue(instance); 
                    }
                    catch (Exception ex) 
                    { 
                        Log(log, $"Exception accessing field {name}: {ex.Message}"); 
                    }
                }
            }

            Log(log, $"No accessible member found: {string.Join(", ", names)}");
            return null;
        }

        //------------------------------------------------------
        // TYPE LOOKUP UTILITIES
        //------------------------------------------------------

        /// <summary>
        /// Finds a type by name across all loaded assemblies
        /// </summary>
        /// <param name="name">Type name (full or simple)</param>
        /// <param name="log">Optional logging callback</param>
        /// <returns>Type or null if not found</returns>
        public static System.Type? FindTypeStatic(string name, Action<string>? log = null)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            try
            {
                // First try direct type lookup
                var directType = System.Type.GetType(name);
                if (directType != null)
                    return directType;

                // Search through all loaded assemblies
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        var foundType = assembly.GetTypes()
                            .FirstOrDefault(t => t.FullName == name || t.Name == name);
                        
                        if (foundType != null)
                            return foundType;
                    }
                    catch (Exception ex)
                    {
                        Log(log, $"Exception searching assembly {assembly.FullName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log(log, $"Exception in FindTypeStatic for {name}: {ex.Message}");
            }

            Log(log, $"Type not found: {name}");
            return null;
        }

        //------------------------------------------------------
        // COMMON UTILITY METHODS
        //------------------------------------------------------

        /// <summary>
        /// Safe conversion to float with fallback
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default if conversion fails</param>
        /// <returns>Float value or default</returns>
        public static float ToFloat(object? value, float defaultValue = 0f)
        {
            if (value == null)
                return defaultValue;

            try
            {
                return Convert.ToSingle(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Safe conversion to int with fallback
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default if conversion fails</param>
        /// <returns>Int value or default</returns>
        public static int ToInt(object? value, int defaultValue = 0)
        {
            if (value == null)
                return defaultValue;

            try
            {
                return Convert.ToInt32(value);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Safe string conversion
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="defaultValue">Default if null</param>
        /// <returns>String representation or default</returns>
        public static string ToString(object? value, string defaultValue = "")
        {
            return value?.ToString() ?? defaultValue;
        }
    }
}
