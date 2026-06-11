using System;
using System.Reflection;

namespace PerAspera.Core.IL2CPP
{
    /// <summary>
    /// Extension methods to simplify reflection operations
    /// Provides safe member access and invocation
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Safely invokes a method by name with error handling
        /// </summary>
        /// <param name="obj">Target object</param>
        /// <param name="methodName">Method name to invoke</param>
        /// <param name="args">Method arguments</param>
        /// <returns>Method result or null if failed</returns>
        public static object SafeInvoke(this object obj, string methodName, params object[] args)
        {
            if (obj == null || string.IsNullOrEmpty(methodName))
                return null;

            try
            {
                var method = obj.GetType().GetMethod(methodName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                return method?.Invoke(obj, args);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a property or field value by name
        /// </summary>
        /// <param name="obj">Target object</param>
        /// <param name="memberName">Property or field name</param>
        /// <returns>Member value or null if not found</returns>
        public static object GetMemberValue(this object obj, string memberName)
        {
            if (obj == null || string.IsNullOrEmpty(memberName))
                return null;

            var type = obj.GetType();
            
            try
            {
                // Try properties first
                var property = type.GetProperty(memberName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (property?.CanRead == true)
                    return property.GetValue(obj);
                
                // Then try fields
                var field = type.GetField(memberName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                    return field.GetValue(obj);
            }
            catch
            {
                // Return null on any reflection errors
            }
            
            return null;
        }


        /// <summary>
        /// Checks if a member (property or field) exists on the object
        /// </summary>
        /// <param name="obj">Target object</param>
        /// <param name="memberName">Member name to check</param>
        /// <returns>True if member exists</returns>
        public static bool HasMember(this object obj, string memberName)
        {
            if (obj == null || string.IsNullOrEmpty(memberName))
                return false;

            var type = obj.GetType();

            // Check properties
            var property = type.GetProperty(memberName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (property != null)
                return true;

            // Check fields
            var field = type.GetField(memberName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            return field != null;
        }

        /// <summary>
        /// Gets strongly typed member value with fallback
        /// </summary>
        /// <typeparam name="T">Expected type</typeparam>
        /// <param name="obj">Target object</param>
        /// <param name="memberName">Member name</param>
        /// <param name="defaultValue">Default value if not found or wrong type</param>
        /// <returns>Typed value or default</returns>
        public static T GetMemberValue<T>(this object obj, string memberName, T defaultValue = default(T))
        {
            var value = obj.GetMemberValue(memberName);
            
            if (value == null)
                return defaultValue;

            try
            {
                if (value is T typedValue)
                    return typedValue;

                // Try conversion
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Checks if object has a method with specific name
        /// </summary>
        /// <param name="obj">Target object</param>
        /// <param name="methodName">Method name to check</param>
        /// <returns>True if method exists</returns>
        public static bool HasMethod(this object obj, string methodName)
        {
            if (obj == null || string.IsNullOrEmpty(methodName))
                return false;

            var method = obj.GetType().GetMethod(methodName,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            
            return method != null;
        }

        /// <summary>
        /// Safe type casting with error handling
        /// </summary>
        /// <typeparam name="T">Target type</typeparam>
        /// <param name="obj">Object to cast</param>
        /// <returns>Casted object or default(T)</returns>
        public static T SafeCast<T>(this object obj) where T : class
        {
            try
            {
                return obj as T;
            }
            catch
            {
                return default(T);
            }
        }
    }
}
