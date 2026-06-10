using System;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using PerAspera.Core;

namespace PerAspera.GameAPI.Commands.Native.IL2CPPInterop
{
    /// <summary>
    /// IL2CPP wrapper for native Keeper access with Handle management
    /// Provides bridge to Keeper.Register() and Keeper.Unregister() system
    /// </summary>
    public class KeeperWrapper
    {
        private readonly object _nativeKeeper;
        private readonly System.Type _keeperType;
        private readonly MethodInfo _registerMethod;
        private readonly MethodInfo _unregisterMethod;
        private static ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("ClassName");

        /// <summary>
        /// Initialize wrapper with native Keeper instance
        /// </summary>
        public KeeperWrapper(object nativeKeeper)
        {
            _nativeKeeper = nativeKeeper ?? throw new ArgumentNullException(nameof(nativeKeeper));
            _keeperType = nativeKeeper.GetType();

            // Cache important methods for performance
            _registerMethod = GetRegisterMethod();
            _unregisterMethod = GetUnregisterMethod();

            ValidateKeeperType(); // Logging disabled}
        }
        /// <summary>
        /// Register an IHandleable object via native Keeper.Register()
        /// Returns Handle for the registered object
        /// </summary>
        public object RegisterHandleable(object handleableObject)
        {
            try
            {
                if (handleableObject == null)
                {
                    // Logging disabled
                    return null;
                }
                
                if (_registerMethod == null)
                {
                    // Logging disabled
                    return null;
                }
                
                // Call Keeper.Register(IHandleable) -> returns Handle
                var handle = _registerMethod.Invoke(_nativeKeeper, new object[] { handleableObject });
                
                _logger.LogDebug($"Registered handleable object: {handleableObject.GetType().Name}");
                return handle;
            }
            catch (Exception ex)
            { // Logging disabled
                return null;
            }
        }
        
        /// <summary>
        /// Unregister an IHandleable object via native Keeper.Unregister()
        /// </summary>
        public bool UnregisterHandleable(object handleableObject)
        {
            try
            {
                if (handleableObject == null)
                {
                    // Logging disabled
                    return false;
                }
                
                if (_unregisterMethod == null)
                {
                    // Logging disabled
                    return false;
                }
                
                // Call Keeper.Unregister(IHandleable)
                _unregisterMethod.Invoke(_nativeKeeper, new object[] { handleableObject });
                
                _logger.LogDebug($"Unregistered handleable object: {handleableObject.GetType().Name}");
                return true;
            }
            catch (Exception ex)
            {
                // Logging disabled
                return false;
            }
        }
        
        /// <summary>
        /// Check if Keeper is available and functional
        /// </summary>
        public bool IsAvailable()
        {
            try
            {
                return _nativeKeeper != null && 
                       _keeperType != null && 
                       _registerMethod != null && 
                       _unregisterMethod != null;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Get native Keeper instance for advanced scenarios
        /// </summary>
        public object GetNativeKeeper()
        {
            return _nativeKeeper;
        }
        
        /// <summary>
        /// Access Keeper via reflection for custom operations
        /// </summary>
        public object InvokeKeeperMethod(string methodName, params object[] parameters)
        {
            try
            {
                var method = _keeperType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                if (method == null)
                {
                    // Logging disabled
                    return null;
                }
                
                return method.Invoke(_nativeKeeper, parameters);
            }
            catch (Exception ex)
            { // Logging disabled
                return null;
            }
        }
        
        /// <summary>
        /// Get property value from Keeper via reflection
        /// </summary>
        public object GetKeeperProperty(string propertyName)
        {
            try
            {
                var property = _keeperType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property == null)
                { // Logging disabled
                return null;
                }
                
                return property.GetValue(_nativeKeeper);
            }
            catch (Exception ex)
            { // Logging disabled
                return null;
            }
        }
        
        private void ValidateKeeperType()
        {
            var hasRegisterMethod = _keeperType.GetMethods()
                .Any(m => m.Name == "Register" && m.GetParameters().Length == 1);
                
            var hasUnregisterMethod = _keeperType.GetMethods()
                .Any(m => m.Name == "Unregister" && m.GetParameters().Length == 1);

            if (!hasRegisterMethod)
            { // Logging disabled}
            }
                if (!hasUnregisterMethod)
                { // Logging disabled}
                }
        }
        
        private MethodInfo GetRegisterMethod()
        {
            try
            {
                // Look for Register(IHandleable) method
                var methods = _keeperType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.Name == "Register" && m.GetParameters().Length == 1)
                    .ToArray();
                    
                if (methods.Length == 0)
                { // Logging disabled
                return null;
                }
                
                // Return first Register method found
                return methods[0];
            }
            catch (Exception ex)
            { // Logging disabled
                return null;
            }
        }
        
        private MethodInfo GetUnregisterMethod()
        {
            try
            {
                // Look for Unregister(IHandleable) method
                var methods = _keeperType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.Name == "Unregister" && m.GetParameters().Length == 1)
                    .ToArray();
                    
                if (methods.Length == 0)
                { // Logging disabled
                return null;
                }
                
                // Return first Unregister method found
                return methods[0];
            }
            catch (Exception ex)
            { // Logging disabled
                return null;
            }
        }
    }
}

