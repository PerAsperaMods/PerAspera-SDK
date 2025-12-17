using System;
using System.Linq;
using System.Reflection;
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
        private readonly System.Type _commandType; // ? CORRECTION: Utiliser System.Type pour éviter l'ambiguïté
        private readonly MethodInfo? _isValidMethod;
        private readonly MethodInfo? _toTabbedStringMethod;
        private static readonly LogAspera _logger = new LogAspera("GameAPI.Commands.Wrapper"); // ? AJOUT

        /// <summary>
        /// Initialize wrapper with native Command instance
        /// </summary>
        /// <param name="nativeCommand">Native command instance to wrap</param>
        public CommandBaseWrapper(object nativeCommand)
        {
            _nativeCommand = nativeCommand ?? throw new ArgumentNullException(nameof(nativeCommand));
            _commandType = nativeCommand.GetType();

            // Cache important methods for performance
            _isValidMethod = GetIsValidMethod();
            _toTabbedStringMethod = GetToTabbedStringMethod();

            ValidateCommandType();
        } // ? CORRECTION: Accolade fermante manquante

        /// <summary>
        /// Native Command instance (for direct IL2CPP access)
        /// </summary>
        public object NativeCommand => _nativeCommand;
        
        /// <summary>
        /// Type of the native command
        /// </summary>
        public System.Type CommandType => _commandType; // ? CORRECTION: System.Type pour éviter l'ambiguïté
        
        /// <summary>
        /// Command name (derived from type)
        /// </summary>
        public string CommandName => _commandType.Name.StartsWith("Cmd") ? _commandType.Name.Substring(3) : _commandType.Name;
        
        /// <summary>
        /// Call native IsValid() method if available
        /// </summary>
        /// <returns>True if command is valid, false otherwise</returns>
        public bool IsValid()
        {
            try
            {
                if (_isValidMethod == null)
                {
                    _logger.LogDebug("IsValid method not available, assuming valid");
                    return true; // Assume valid if method not available
                }
                
                var result = _isValidMethod.Invoke(_nativeCommand, Array.Empty<object>());
                return result is bool valid ? valid : true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling IsValid: {ex.Message}");
                return false;
            }
        } // ? CORRECTION: Toutes les branches retournent une valeur
        
        /// <summary>
        /// Get human-readable string representation via ToTabbedString()
        /// </summary>
        /// <returns>Human-readable description of the command</returns>
        public string GetDescription()
        {
            try
            {
                if (_toTabbedStringMethod == null)
                {
                    return $"{CommandName}()";
                }
                
                var result = _toTabbedStringMethod.Invoke(_nativeCommand, Array.Empty<object>());
                return result?.ToString() ?? $"{CommandName}()";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting description: {ex.Message}");
                return $"{CommandName}(error)";
            }
        } // ? CORRECTION: Toutes les branches retournent une valeur
        
        /// <summary>
        /// Get Faction property if available
        /// </summary>
        /// <returns>Faction object or null if not available</returns>
        public object? GetFaction()
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
            {
                _logger.LogError($"Error getting faction: {ex.Message}");
                return null;
            }
        } // ? CORRECTION: Toutes les branches retournent une valeur
        
        /// <summary>
        /// Set Faction property if available
        /// </summary>
        /// <param name="faction">Faction object to set</param>
        /// <returns>True if faction was set successfully, false otherwise</returns>
        public bool SetFaction(object? faction)
        {
            try
            {
                var factionProperty = _commandType.GetProperty("Faction", BindingFlags.Public | BindingFlags.Instance);
                if (factionProperty == null || !factionProperty.CanWrite)
                {
                    _logger.LogWarning("Faction property not available or not writable");
                    return false;
                }
                
                factionProperty.SetValue(_nativeCommand, faction);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error setting faction: {ex.Message}");
                return false;
            }
        } // ? CORRECTION: Toutes les branches retournent une valeur
        
        /// <summary>
        /// Get property value by name via reflection
        /// </summary>
        /// <param name="propertyName">Name of the property to get</param>
        /// <returns>Property value or null if not found</returns>
        public object? GetProperty(string propertyName)
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
            {
                _logger.LogError($"Error getting property {propertyName}: {ex.Message}");
                return null;
            }
        } // ? CORRECTION: Toutes les branches retournent une valeur
        
        /// <summary>
        /// Set property value by name via reflection
        /// </summary>
        /// <param name="propertyName">Name of the property to set</param>
        /// <param name="value">Value to set</param>
        /// <returns>True if property was set successfully, false otherwise</returns>
        public bool SetProperty(string propertyName, object? value)
        {
            try
            {
                var property = _commandType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property == null || !property.CanWrite)
                {
                    _logger.LogWarning($"Property {propertyName} not available or not writable");
                    return false;
                }
                
                property.SetValue(_nativeCommand, value);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error setting property {propertyName}: {ex.Message}");
                return false;
            }
        } // ? CORRECTION: Toutes les branches retournent une valeur
        
        /// <summary>
        /// Invoke method on native command via reflection
        /// </summary>
        /// <param name="methodName">Name of the method to invoke</param>
        /// <param name="parameters">Method parameters</param>
        /// <returns>Method return value or null if method not found or failed</returns>
        public object? InvokeMethod(string methodName, params object[] parameters)
        {
            try
            {
                var method = _commandType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                if (method == null)
                {
                    _logger.LogWarning($"Method {methodName} not found");
                    return null;
                }
                
                return method.Invoke(_nativeCommand, parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error invoking method {methodName}: {ex.Message}");
                return null;
            }
        } // ? CORRECTION: Toutes les branches retournent une valeur
        
        /// <summary>
        /// Check if command is a specific type (e.g., "CmdImportResource")
        /// </summary>
        /// <param name="commandTypeName">Type name to check against</param>
        /// <returns>True if command matches the specified type name</returns>
        public bool IsCommandType(string commandTypeName)
        {
            return _commandType.Name.Equals(commandTypeName, StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Validate that the wrapped object is a command type
        /// </summary>
        private void ValidateCommandType()
        {
            // Check if this looks like a command type
            var isCommand = _commandType.Name.StartsWith("Cmd") || 
                           _commandType.Name.Contains("Command") ||
                           _commandType.BaseType?.Name == "CommandBase";
                           
            if (!isCommand)
            {
                _logger.LogWarning($"Type {_commandType.Name} does not appear to be a command type");
            }
        } // ? CORRECTION: Accolade fermante manquante
        
        /// <summary>
        /// Get IsValid method via reflection
        /// </summary>
        /// <returns>IsValid MethodInfo or null if not found</returns>
        private MethodInfo? GetIsValidMethod()
        {
            try
            {
                // ? CORRECTION: Utiliser System.Type[] et méthodes simplifiées
                return _commandType.GetMethod("isValid", BindingFlags.Public | BindingFlags.Instance) ??
                       _commandType.GetMethod("IsValid", BindingFlags.Public | BindingFlags.Instance);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting IsValid method: {ex.Message}");
                return null;
            }
        } // ? CORRECTION: Toutes les branches retournent une valeur
        
        /// <summary>
        /// Get ToTabbedString method via reflection
        /// </summary>
        /// <returns>ToTabbedString MethodInfo or null if not found</returns>
        private MethodInfo? GetToTabbedStringMethod()
        {
            try
            {
                // ? CORRECTION: Méthode simplifiée
                return _commandType.GetMethod("ToTabbedString", BindingFlags.Public | BindingFlags.Instance);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error getting ToTabbedString method: {ex.Message}");
                return null;
            }
        } // ? CORRECTION: Toutes les branches retournent une valeur
        
        /// <summary>
        /// Returns string representation of the command
        /// </summary>
        /// <returns>String representation</returns>
        public override string ToString()
        {
            return GetDescription();
        }
    }
}

