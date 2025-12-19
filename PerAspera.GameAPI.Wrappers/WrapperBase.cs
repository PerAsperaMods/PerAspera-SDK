using System;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Base class for all game object wrappers
    /// Provides common functionality for wrapping native IL2CPP game objects
    /// </summary>
    public abstract class WrapperBase
    {
        protected static readonly LogAspera Log = new LogAspera("Wrappers");
        
        /// <summary>
        /// The native IL2CPP game object being wrapped
        /// </summary>
        protected object? NativeObject { get; set; }
        

        /// <summary>
        /// Check if the native object is valid
        /// </summary>
        protected bool IsValid => NativeObject != null;
        
        /// <summary>
        /// Public accessor for IsValid - allows cross-wrapper validation
        /// </summary>
        public bool IsValidWrapper => IsValid;
        
        /// <summary>
        /// Get the Type of the native IL2CPP object (for debugging/inspection)
        /// Returns null if no native object is wrapped
        /// </summary>
        public System.Type? GetNativeType()
        {
            return NativeObject?.GetType();
        }
        
        /// <summary>
        /// Get the native object for use in other wrapper methods
        /// This allows wrappers to pass native objects to other wrapper method calls
        /// </summary>
        public object? GetNativeObject()
        {
            return NativeObject;
        }
        
        /// <summary>
        /// Constructor with native object
        /// </summary>
        protected WrapperBase(object? nativeObject)
        {
            NativeObject = nativeObject;
        }
        
        /// <summary>
        /// Validate that the native object exists
        /// </summary>
        protected bool ValidateNativeObject(string operationName = "")
        {
            if (NativeObject == null)
            {
                Log.Warning($"Native object is null for {GetType().Name}" + 
                           (string.IsNullOrEmpty(operationName) ? "" : $" during {operationName}"));
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// Safely invoke a method on the native object
        /// </summary>
        protected T? SafeInvoke<T>(string methodName, params object[] args)
        {
            if (!ValidateNativeObject(methodName))
                return default;
                
            try
            {
                return NativeObject.InvokeMethod<T>(methodName, args);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to invoke {methodName} on {GetType().Name}: {ex.Message}");
                return default;
            }
        }
        
        /// <summary>
        /// Safely invoke a void method on the native object
        /// </summary>
        protected void SafeInvokeVoid(string methodName, params object[] args)
        {
            if (!ValidateNativeObject(methodName))
                return;
                
            try
            {
                NativeObject.InvokeMethod<object>(methodName, args);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to invoke {methodName} on {GetType().Name}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Safely get a field value from the native object
        /// </summary>
        protected T? SafeGetField<T>(string fieldName)
        {
            if (!ValidateNativeObject($"get field {fieldName}"))
                return default;
                
            try
            {
                return NativeObject.GetFieldValue<T>(fieldName);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get field {fieldName} on {GetType().Name}: {ex.Message}");
                return default;
            }
        }
        
        /// <summary>
        /// Safely set a field value on the native object
        /// </summary>
        protected void SafeSetField<T>(string fieldName, T value)
        {
            if (!ValidateNativeObject($"set field {fieldName}"))
                return;
                
            try
            {
                NativeObject.SetFieldValue(fieldName, value);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to set field {fieldName} on {GetType().Name}: {ex.Message}");
            }
        }
    }
}
