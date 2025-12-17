using System;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using PerAspera.Core;

namespace PerAspera.GameAPI.Commands.Native.IL2CPPInterop
{
    /// <summary>
    /// IL2CPP wrapper for native CommandBus access with type safety
    /// Provides bridge to CommandBus.Dispatch() system
    /// </summary>
    public class CommandBusWrapper
    {
        private readonly object _nativeCommandBus;
        private readonly System.Type _commandBusType;
        private static ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("ClassName");

        /// <summary>
        /// Initialize wrapper with native CommandBus instance
        /// Uses GameTypeInitializer for enhanced type discovery
        /// </summary>
        /// </summary>
        public CommandBusWrapper(object nativeCommandBus)
        {
            _nativeCommandBus = nativeCommandBus ?? throw new ArgumentNullException(nameof(nativeCommandBus));
            
            // First try to get type via GameTypeInitializer for better accuracy
            _commandBusType = GameTypeInitializer.GetCommandBusType() ?? nativeCommandBus.GetType();
            
            // Validate that the provided instance matches the expected type
            if (!_commandBusType.IsInstanceOfType(nativeCommandBus))
            {
                _logger.LogWarning($"CommandBus instance type mismatch. Expected: {_commandBusType.Name}, Actual: {nativeCommandBus.GetType().Name}");
                _commandBusType = nativeCommandBus.GetType(); // Fallback to runtime type
            }
            
            ValidateCommandBusType();
            _logger.LogInfo($"CommandBusWrapper initialized for type: {_commandBusType.FullName} (via GameTypeInitializer: {GameTypeInitializer.GetCommandBusType() != null})");
        }
        
        /// <summary>
        /// Dispatch command via native CommandBus.Dispatch(command)
        /// </summary>
        public bool DispatchCommand<T>(T command) where T : class
        {
            try
            {
                // Get generic Dispatch<T> method
                var dispatchMethod = GetDispatchMethod(typeof(T));
                if (dispatchMethod == null)
                {
                    _logger.LogError($"Dispatch method not found for type {typeof(T).Name}");
                    return false;
                }
                
                // Invoke Dispatch<T>(command)
                var result = dispatchMethod.Invoke(_nativeCommandBus, new object[] { command });
                
                _logger.LogDebug($"Command dispatched: {typeof(T).Name}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to dispatch command {typeof(T).Name}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Dispatch command by type name (for dynamic scenarios)
        /// </summary>
        public bool DispatchCommandByType(string commandTypeName, object command)
        {
            try
            {
                // Find command type
                var commandType = FindCommandType(commandTypeName);
                if (commandType == null)
                { // Logging disabledreturn false;
                }
                
                // Get dispatch method for this type
                var dispatchMethod = GetDispatchMethod(commandType);
                if (dispatchMethod == null)
                { // Logging disabledreturn false;
                }
                
                // Invoke dispatch
                var result = dispatchMethod.Invoke(_nativeCommandBus, new object[] { command }); // Logging disabledreturn true;
            }
            catch (Exception ex)
            { // Logging disabledreturn false;
            }
        }
        
        /// <summary>
        /// Check if CommandBus is available and functional
        /// </summary>
        public bool IsAvailable()
        {
            try
            {
                return _nativeCommandBus != null && _commandBusType != null;
            }
            catch
            {
                return false;
            }
        }
        
        /// <summary>
        /// Get information about available command types
        /// </summary>
        public string[] GetSupportedCommandTypes()
        {
            try
            {
                // This would scan assemblies for Cmd* classes
                // For now return known command types
                return new string[]
                {
                    "CmdImportResource",
                    "CmdSpawnResourceVein", 
                    "CmdUnlockBuilding",
                    "CmdAdditionalBuilding",
                    "CmdResearchTechnology",
                    "CmdUnlockKnowledge",
                    "CmdStartDialogue",
                    "CmdNotifyDialogue",
                    "CmdSabotage",
                    "CmdGameOver",
                    "CmdSetOverride"
                    // TODO: Complete list of 55 commands
                };
            }
            catch (Exception ex)
            { // Logging disabledreturn new string[0];
            }
        }
        
        private void ValidateCommandBusType()
        {
            // Validate that we have a proper CommandBus type
            var hasDispatchMethod = _commandBusType.GetMethods()
                .Any(m => m.Name == "Dispatch" && m.IsGenericMethod);

            if (!hasDispatchMethod)
            { // Logging disabled}
                
            }
        }
        
        private MethodInfo GetDispatchMethod(System.Type commandType)
        {
            try
            {
                // Look for generic Dispatch<T> method
                var methods = _commandBusType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.Name == "Dispatch" && m.IsGenericMethod)
                    .ToArray();
                    
                if (methods.Length == 0)
                { // Logging disabledreturn null;
                }
                
                // Get first generic Dispatch method and make it concrete for our command type
                var genericMethod = methods[0];
                return genericMethod.MakeGenericMethod(commandType);
            }
            catch (Exception ex)
            { // Logging disabledreturn null;
            }
        }
        
        private System.Type FindCommandType(string typeName)
        {
            try
            {
                // Search in current assembly first
                var currentAssembly = Assembly.GetExecutingAssembly();
                var type = currentAssembly.GetType(typeName);
                if (type != null) return type;
                
                // Search in all loaded assemblies
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = assembly.GetType(typeName);
                    if (type != null) return type;
                    
                    // Also try to find types with this name (case insensitive)
                    var matchingTypes = assembly.GetTypes()
                        .Where(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase))
                        .ToArray();
                        
                    if (matchingTypes.Length > 0)
                        return matchingTypes[0];
                }
                
                return null;
            }
            catch (Exception ex)
            { // Logging disabledreturn null;
            }
        }
        /// <summary>
        /// Create CommandBusWrapper using GameTypeInitializer for automatic CommandBus discovery
        /// </summary>
        /// <returns>CommandBusWrapper instance or null if CommandBus not found</returns>
        public static CommandBusWrapper CreateFromGame()
        {
            try
            {
                // Initialize GameTypeInitializer if needed
                GameTypeInitializer.Initialize();

                // Get BaseGame instance
                var baseGameType = GameTypeInitializer.GetBaseGameType();
                if (baseGameType == null)
                { // Logging disabledreturn null;
                }

                var baseGameInstance = baseGameType.GetProperty("Instance", 
                    BindingFlags.Public | BindingFlags.Static)?.GetValue(null);

                if (baseGameInstance == null)
                { // Logging disabledreturn null;
                }

                // Try to find CommandBus on BaseGame instance
                var commandBusProperty = baseGameType.GetProperty("CommandBus", 
                    BindingFlags.Public | BindingFlags.Instance);

                if (commandBusProperty != null)
                {
                    var commandBusInstance = commandBusProperty.GetValue(baseGameInstance);
                    if (commandBusInstance != null)
                    { // Logging disabledreturn new CommandBusWrapper(commandBusInstance);
                    }
                }

                // Try alternative field names
                var commandBusFields = baseGameType.GetFields(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (var field in commandBusFields)
                {
                    if (field.Name.Contains("CommandBus") || field.Name.Contains("commandBus"))
                    {
                        var commandBusInstance = field.GetValue(baseGameInstance);
                        if (commandBusInstance != null)
                        { // Logging disabledreturn new CommandBusWrapper(commandBusInstance);
                        }
                    }
                } // Logging disabledreturn null;
            }
            catch (Exception ex)
            { // Logging disabledreturn null;
            }
        }    }
}

