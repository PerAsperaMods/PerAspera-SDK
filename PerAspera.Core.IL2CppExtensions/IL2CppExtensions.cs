using System;
using System.Reflection;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Common;
using BepInEx.Logging;

namespace PerAspera.Core.IL2CPP
{
    /// <summary>
    /// Extension methods for safe IL2CPP object manipulation
    /// Uses Il2CppInterop for proper IL2CPP support in BepInEx 6.x
    /// </summary>
    public static class IL2CppExtensions
    {
        private static readonly ManualLogSource _log = Logger.CreateLogSource("IL2CppExtensions");

        /// <summary>
        /// Safely get a member value from an IL2CPP object
        /// </summary>
        public static TValue? GetMemberValue<TValue>(this object instance, string memberName)
        {
            if (instance == null)
                return default;

            try
            {
                var type = instance.GetIl2CppType();
                
                // Try property first
                var property = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);
                if (property != null && property.CanRead)
                {
                    var value = property.GetValue(instance);
                    return ConvertValue<TValue>(value);
                }

                // Try field
                var field = type.GetField(memberName, BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    var value = field.GetValue(instance);
                    return ConvertValue<TValue>(value);
                }

                _log.LogWarning($"Member '{memberName}' not found on type {type.Name}");
                return default;
            }
            catch (Exception ex)
            {
                _log.LogError($"Error getting member '{memberName}': {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// Safely set a member value on an IL2CPP object
        /// </summary>
        public static void SetMemberValue(this object instance, string memberName, object value)
        {
            if (instance == null)
                return;

            try
            {
                var type = instance.GetIl2CppType();

                // Try property first
                var property = type.GetProperty(memberName, BindingFlags.Public | BindingFlags.Instance);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(instance, value);
                    return;
                }

                // Try field
                var field = type.GetField(memberName, BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(instance, value);
                    return;
                }

                _log.LogWarning($"Writable member '{memberName}' not found on type {type.Name}");
            }
            catch (Exception ex)
            {
                _log.LogError($"Error setting member '{memberName}': {ex.Message}");
            }
        }

        /// <summary>
        /// Safely invoke a void method on an IL2CPP object
        /// </summary>
        /// <returns>True if method was found and invoked successfully, false otherwise</returns>
        public static bool InvokeMethod(this object instance, string methodName, params object[] parameters)
        {
            if (instance == null)
                return false;

            try
            {
                var type = instance.GetIl2CppType();
                var paramTypes = parameters?.Length > 0 
                    ? System.Array.ConvertAll(parameters, p => p.GetType()) 
                    : System.Type.EmptyTypes;

                var method = type.GetMethod(methodName, paramTypes);
                if (method != null)
                {
                    method.Invoke(instance, parameters);
                    return true;
                }

                // Try without exact parameter matching
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (var m in methods)
                {
                    if (m.Name == methodName && m.GetParameters().Length == (parameters?.Length ?? 0))
                    {
                        m.Invoke(instance, parameters);
                        return true;
                    }
                }

                _log.LogWarning($"Method '{methodName}' not found on type {type.Name}");
                return false;
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed to invoke method '{methodName}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Safely invoke a method on an IL2CPP object with return value
        /// </summary>
        public static TResult? InvokeMethod<TResult>(this object instance, string methodName, params object[] parameters)
        {
            if (instance == null)
                return default;

            try
            {
                var type = instance.GetIl2CppType();
                var paramTypes = parameters?.Length > 0 
                    ? System.Array.ConvertAll(parameters, p => p.GetType()) 
                    : System.Type.EmptyTypes;

                var method = type.GetMethod(methodName, paramTypes);
                if (method != null)
                {
                    var result = method.Invoke(instance, parameters);
                    return ConvertValue<TResult>(result);
                }

                // Try without exact parameter matching
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (var m in methods)
                {
                    if (m.Name == methodName && m.GetParameters().Length == (parameters?.Length ?? 0))
                    {
                        var result = m.Invoke(instance, parameters);
                        return ConvertValue<TResult>(result);
                    }
                }

                _log.LogWarning($"Method '{methodName}' not found on type {type.Name}");
                return default;
            }
            catch (Exception ex)
            {
                _log.LogError($"Error invoking method '{methodName}': {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// Safe invoke with fallback for IL2CPP objects
        /// </summary>
        public static object? SafeInvoke(this object instance, string methodName, params object[] parameters)
        {
            return InvokeMethod<object>(instance, methodName, parameters);
        }

        /// <summary>
        /// Get the IL2CPP type of an object
        /// </summary>
        public static System.Type GetIl2CppType(this object instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            // Handle IL2CPP objects properly
            if (instance is Il2CppObjectBase il2cppObj)
            {
                return il2cppObj.GetType();
            }

            return instance.GetType();
        }

        /// <summary>
        /// Convert values safely between IL2CPP and managed types
        /// </summary>
        private static TValue? ConvertValue<TValue>(object? value)
        {
            if (value == null)
                return default;

            try
            {
                // Direct assignment if types match
                if (value is TValue directValue)
                    return directValue;

                // Handle IL2CPP to managed conversion
                if (typeof(TValue) == typeof(string) && value != null)
                    return (TValue)(object)value.ToString();

                // Handle numeric conversions
                if (typeof(TValue).IsPrimitive)
                    return (TValue)Convert.ChangeType(value, typeof(TValue));

                // Default conversion attempt
                return (TValue)value;
            }
            catch (Exception ex)
            {
                var log = Logger.CreateLogSource("IL2CppExtensions.ConvertValue");
                log.LogWarning($"Failed to convert {value?.GetType().Name} to {typeof(TValue).Name}: {ex.Message}");
                return default;
            }
        }

        // ==================== CONVENIENCE ALIASES ====================

        /// <summary>
        /// Alias for GetMemberValue - specifically for field access
        /// </summary>
        public static TValue? GetFieldValue<TValue>(this object instance, string fieldName)
        {
            return GetMemberValue<TValue>(instance, fieldName);
        }

        /// <summary>
        /// Alias for GetMemberValue - specifically for property access
        /// </summary>
        public static TValue? GetPropertyValue<TValue>(this object instance, string propertyName)
        {
            return GetMemberValue<TValue>(instance, propertyName);
        }

        /// <summary>
        /// Alias for SetMemberValue - specifically for field access
        /// </summary>
        public static void SetFieldValue(this object instance, string fieldName, object value)
        {
            SetMemberValue(instance, fieldName, value);
        }

        /// <summary>
        /// Alias for SetMemberValue - specifically for property access
        /// </summary>
        public static void SetPropertyValue(this object instance, string propertyName, object value)
        {
            SetMemberValue(instance, propertyName, value);
        }
    }
}
