using System;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using PerAspera.Core;

namespace PerAspera.GameAPI.Commands.Native.IL2CPPInterop
{
    /// <summary>
    /// IL2CPP wrapper for native CommandBase classes with type safety
    /// Provides bridge to CommandBase hierarchy (CmdImportResource, CmdUnlockBuilding, etc.)
    /// </summary>
    public class CommandBaseWrapper
    {
        private readonly object _nativeCommand;
        private readonly System.Type _commandType;
        private readonly MethodInfo _isValidMethod;
        private readonly MethodInfo _toTabbedStringMethod;
        
        /// <summary>
        /// Initialize wrapper with native Command instance
        /// </summary>
        public CommandBaseWrapper(object nativeCommand)
        {
            _nativeCommand = nativeCommand ?? throw new ArgumentNullException(nameof(nativeCommand));
            _commandType = nativeCommand.GetType();
            
            // Cache important methods for performance
            _isValidMethod = GetIsValidMethod();
            _toTabbedStringMethod = GetToTabbedStringMethod();
            
            ValidateCommandType(); // Logging disabled}
        
        /// <summary>
        /// Native Command instance (for direct IL2CPP access)
        /// </summary>
        public object NativeCommand => _nativeCommand;
        
        /// <summary>
        /// Type of the native command
        /// </summary>
        public Type CommandType => _commandType;
        
        /// <summary>
        /// Command name (derived from type)
        /// </summary>
        public string CommandName => _commandType.Name.StartsWith("Cmd") ? _commandType.Name.Substring(3) : _commandType.Name;
        
        /// <summary>
        /// Call native IsValid() method if available
        /// </summary>
        public bool IsValid()
        {
            try
            {
                if (_isValidMethod == null)
                { // Logging disabledreturn true; // Assume valid if method not available
                }
                
                var result = _isValidMethod.Invoke(_nativeCommand, new object[0]);
                return result is bool valid ? valid : true;
            }
            catch (Exception ex)
            { // Logging disabledreturn false;
            }
        }
        
        /// <summary>
        /// Get human-readable string representation via ToTabbedString()
        /// </summary>
        public string GetDescription()
        {
            try
            {
                if (_toTabbedStringMethod == null)
                {
                    return $"{CommandName}()";
                }
                
                var result = _toTabbedStringMethod.Invoke(_nativeCommand, new object[0]);
                return result?.ToString() ?? $"{CommandName}()";
            }
            catch (Exception ex)
            { // Logging disabledreturn $"{CommandName}(error)";
            }
        }
        
        /// <summary>
        /// Get Faction property if available
        /// </summary>
        public object GetFaction()
        {
            try
            {
                var factionProperty = _commandType.GetProperty("Faction", BindingFlags.Public | BindingFlags.Instance);
                if (factionProperty == null)
                {
                    return null;
                }
                
                return factionProperty.GetValue(_nativeCommand);
            }
            catch (Exception ex)
            { // Logging disabledreturn null;
            }
        }
        
        /// <summary>
        /// Set Faction property if available
        /// </summary>
        public bool SetFaction(object faction)
        {
            try
            {
                var factionProperty = _commandType.GetProperty("Faction", BindingFlags.Public | BindingFlags.Instance);
                if (factionProperty == null || !factionProperty.CanWrite)
                { // Logging disabledreturn false;
                }
                
                factionProperty.SetValue(_nativeCommand, faction);
                return true;
            }
            catch (Exception ex)
            { // Logging disabledreturn false;
            }
        }
        
        /// <summary>
        /// Get property value by name via reflection
        /// </summary>
        public object GetProperty(string propertyName)
        {
            try
            {
                var property = _commandType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property == null)
                {
                    return null;
                }
                
                return property.GetValue(_nativeCommand);
            }
            catch (Exception ex)
            { // Logging disabledreturn null;
            }
        }
        
        /// <summary>
        /// Set property value by name via reflection
        /// </summary>
        public bool SetProperty(string propertyName, object value)
        {
            try
            {
                var property = _commandType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property == null || !property.CanWrite)
                { // Logging disabledreturn false;
                }
                
                property.SetValue(_nativeCommand, value);
                return true;
            }
            catch (Exception ex)
            { // Logging disabledreturn false;
            }
        }
        
        /// <summary>
        /// Invoke method on native command via reflection
        /// </summary>
        public object InvokeMethod(string methodName, params object[] parameters)
        {
            try
            {
                var method = _commandType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                if (method == null)
                { // Logging disabledreturn null;
                }
                
                return method.Invoke(_nativeCommand, parameters);
            }
            catch (Exception ex)
            { // Logging disabledreturn null;
            }
        }
        
        /// <summary>
        /// Check if command is a specific type (e.g., "CmdImportResource")
        /// </summary>
        public bool IsCommandType(string commandTypeName)
        {
            return _commandType.Name.Equals(commandTypeName, StringComparison.OrdinalIgnoreCase);
        }
        
        private void ValidateCommandType()
        {
            // Check if this looks like a command type
            var isCommand = _commandType.Name.StartsWith("Cmd") || 
                           _commandType.Name.Contains("Command") ||
                           _commandType.BaseType?.Name == "CommandBase";
                           
            if (!isCommand)
            { // Logging disabled}
        }
        
        private MethodInfo GetIsValidMethod()
        {
            try
            {
                return _commandType.GetMethod("isValid", BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null) ??
                       _commandType.GetMethod("IsValid", BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);
            }
            catch (Exception ex)
            { // Logging disabledreturn null;
            }
        }
        
        private MethodInfo GetToTabbedStringMethod()
        {
            try
            {
                return _commandType.GetMethod("ToTabbedString", BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null);
            }
            catch (Exception ex)
            { // Logging disabledreturn null;
            }
        }
        
        public override string ToString()
        {
            return GetDescription();
        }
    }
}
