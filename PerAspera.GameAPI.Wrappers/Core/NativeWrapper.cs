using System;
using System.Reflection;
using PerAspera.Core.IL2CPP;
using BepInEx.Logging;

namespace PerAspera.GameAPI.Wrappers.Core
{
    /// <summary>
    /// Base class for all native object wrappers, providing unified method call patterns.
    /// Enables type-safe native method invocation with automatic error handling and logging.
    /// </summary>
    /// <typeparam name="T">The native object type being wrapped</typeparam>
    /// <example>
    /// <code>
    /// public class FactionWrapper : NativeWrapper&lt;object&gt;
    /// {
    ///     public string GetName() => CallNative&lt;string&gt;("get_Name") ?? "Unknown";
    ///     public void SetActive(bool active) => CallNativeVoid("SetActive", active);
    /// }
    /// </code>
    /// </example>
    public abstract class NativeWrapper<T> where T : class
    {
        protected static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("NativeWrapper");
        
        /// <summary>
        /// The wrapped native object instance
        /// </summary>
        protected T _nativeObject;
        
        /// <summary>
        /// Gets the underlying native object for direct access when needed
        /// </summary>
        /// <returns>The native object instance</returns>
        public T GetNativeObject() => _nativeObject;
        
        /// <summary>
        /// Initializes the wrapper with a native object instance
        /// </summary>
        /// <param name="nativeObject">The native object to wrap</param>
        protected NativeWrapper(T nativeObject)
        {
            _nativeObject = nativeObject ?? throw new ArgumentNullException(nameof(nativeObject));
        }
        
        /// <summary>
        /// Calls a native method with a return value using IL2CPP extensions
        /// </summary>
        /// <typeparam name="TResult">Expected return type</typeparam>
        /// <param name="methodName">Name of the method to invoke</param>
        /// <param name="parameters">Method parameters</param>
        /// <returns>Method result or default(TResult) on failure</returns>
        protected TResult? CallNative<TResult>(string methodName, params object[] parameters)
        {
            try
            {
                return _nativeObject.InvokeMethod<TResult>(methodName, parameters);
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to call native method {methodName}: {ex.Message}");
                return default(TResult);
            }
        }
        
        /// <summary>
        /// Calls a native void method using IL2CPP extensions
        /// </summary>
        /// <param name="methodName">Name of the method to invoke</param>
        /// <param name="parameters">Method parameters</param>
        protected void CallNativeVoid(string methodName, params object[] parameters)
        {
            try
            {
                _nativeObject.InvokeMethod(methodName, parameters);
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to call native void method {methodName}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Gets a field value from the native object using IL2CPP extensions
        /// </summary>
        /// <typeparam name="TField">Expected field type</typeparam>
        /// <param name="fieldName">Name of the field to access</param>
        /// <param name="bindingFlags">Optional binding flags for field access</param>
        /// <returns>Field value or default(TField) on failure</returns>
        protected TField? GetNativeField<TField>(string fieldName, BindingFlags? bindingFlags = null)
        {
            try
            {
                if (bindingFlags.HasValue)
                {
                    // Use reflection with specific binding flags for protected/private fields
                    var field = _nativeObject.GetType().GetField(fieldName, bindingFlags.Value);
                    return field != null ? (TField?)field.GetValue(_nativeObject) : default(TField);
                }
                else
                {
                    // Use IL2CPP extensions for standard field access
                    return _nativeObject.GetFieldValue<TField>(fieldName);
                }
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to get native field {fieldName}: {ex.Message}");
                return default(TField);
            }
        }
        
        /// <summary>
        /// Sets a field value on the native object using IL2CPP extensions
        /// </summary>
        /// <typeparam name="TField">Field type</typeparam>
        /// <param name="fieldName">Name of the field to set</param>
        /// <param name="value">Value to set</param>
        /// <param name="bindingFlags">Optional binding flags for field access</param>
        protected void SetNativeField<TField>(string fieldName, TField value, BindingFlags? bindingFlags = null)
        {
            try
            {
                if (bindingFlags.HasValue)
                {
                    // Use reflection with specific binding flags
                    var field = _nativeObject.GetType().GetField(fieldName, bindingFlags.Value);
                    field?.SetValue(_nativeObject, value);
                }
                else
                {
                    // Use IL2CPP extensions for standard field access
                    _nativeObject.SetFieldValue(fieldName, value);
                }
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to set native field {fieldName}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Gets a property value from the native object using IL2CPP extensions
        /// </summary>
        /// <typeparam name="TProp">Expected property type</typeparam>
        /// <param name="propertyName">Name of the property to access</param>
        /// <returns>Property value or default(TProp) on failure</returns>
        protected TProp? GetNativeProperty<TProp>(string propertyName)
        {
            try
            {
                // Properties are accessed via their getter methods
                return CallNative<TProp>($"get_{propertyName}");
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to get native property {propertyName}: {ex.Message}");
                return default(TProp);
            }
        }
        
        /// <summary>
        /// Sets a property value on the native object using IL2CPP extensions
        /// </summary>
        /// <typeparam name="TProp">Property type</typeparam>
        /// <param name="propertyName">Name of the property to set</param>
        /// <param name="value">Value to set</param>
        protected void SetNativeProperty<TProp>(string propertyName, TProp value)
        {
            try
            {
                // Properties are set via their setter methods
                CallNativeVoid($"set_{propertyName}", value);
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to set native property {propertyName}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Static method helper for calling static native methods
        /// </summary>
        /// <param name="nativeType">The native type containing the static method</param>
        /// <param name="methodName">Name of the static method</param>
        /// <param name="parameters">Method parameters</param>
        /// <returns>Method result or null on failure</returns>
        protected static object? CallStaticNative(System.Type nativeType, string methodName, params object[] parameters)
        {
            try
            {
                var method = nativeType.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
                return method?.Invoke(null, parameters);
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to call static native method {nativeType.Name}.{methodName}: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Static method helper for calling static native methods with typed return
        /// </summary>
        /// <typeparam name="TResult">Expected return type</typeparam>
        /// <param name="nativeType">The native type containing the static method</param>
        /// <param name="methodName">Name of the static method</param>
        /// <param name="parameters">Method parameters</param>
        /// <returns>Method result or default(TResult) on failure</returns>
        protected static TResult? CallStaticNative<TResult>(System.Type nativeType, string methodName, params object[] parameters)
        {
            var result = CallStaticNative(nativeType, methodName, parameters);
            return result is TResult typedResult ? typedResult : default(TResult);
        }
    }
}