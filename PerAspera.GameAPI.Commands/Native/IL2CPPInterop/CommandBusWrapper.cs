using System;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using PerAspera.Core;

namespace PerAspera.GameAPI.Commands.Native.IL2CPPInterop
{
    /// <summary>
    /// IL2CPP wrapper for native CommandBus access with type safety
    /// Provides bridge to CommandBus.Dispatch<T>() system
    /// </summary>
    public class CommandBusWrapper
    {
        private readonly object _nativeCommandBus;
        private readonly Type _commandBusType;
        
        /// <summary>
        /// Initialize wrapper with native CommandBus instance
        /// Uses GameTypeInitializer for enhanced type discovery
        /// </summary>
        public CommandBusWrapper(object nativeCommandBus)
        {
            _nativeCommandBus = nativeCommandBus ?? throw new ArgumentNullException(nameof(nativeCommandBus));
            
            // First try to get type via GameTypeInitializer for better accuracy
            _commandBusType = GameTypeInitializer.GetCommandBusType() ?? nativeCommandBus.GetType();
            
            // Validate that the provided instance matches the expected type
            if (!_commandBusType.IsInstanceOfType(nativeCommandBus))
            {
                LogAspera.Warning($"CommandBus instance type mismatch. Expected: {_commandBusType.Name}, Actual: {nativeCommandBus.GetType().Name}");
                _commandBusType = nativeCommandBus.GetType(); // Fallback to runtime type
            }
            
            ValidateCommandBusType();
            LogAspera.Info($"CommandBusWrapper initialized for type: {_commandBusType.FullName} (via GameTypeInitializer: {GameTypeInitializer.GetCommandBusType() != null})");
        }
        
        /// <summary>
        /// Dispatch command via native CommandBus.Dispatch<T>(command)
        /// </summary>
        public bool DispatchCommand<T>(T command) where T : class
        {
            try
            {
                // Get generic Dispatch<T> method
                var dispatchMethod = GetDispatchMethod(typeof(T));
                if (dispatchMethod == null)
                {
                    LogAspera.Error($"Dispatch method not found for type {typeof(T).Name}");
                    return false;
                }
                
                // Invoke Dispatch<T>(command)
                var result = dispatchMethod.Invoke(_nativeCommandBus, new object[] { command });
                
                LogAspera.Debug($"Command dispatched: {typeof(T).Name}");
                return true;
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Failed to dispatch command {typeof(T).Name}: {ex.Message}");
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
                {
                    LogAspera.Error($"Command type not found: {commandTypeName}");
                    return false;
                }
                
                // Get dispatch method for this type
                var dispatchMethod = GetDispatchMethod(commandType);
                if (dispatchMethod == null)
                {
                    LogAspera.Error($"Dispatch method not found for command type: {commandTypeName}");
                    return false;
                }
                
                // Invoke dispatch
                var result = dispatchMethod.Invoke(_nativeCommandBus, new object[] { command });
                
                LogAspera.Debug($"Command dispatched by type: {commandTypeName}");
                return true;
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Failed to dispatch command by type {commandTypeName}: {ex.Message}");
                return false;
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
            {
                LogAspera.Error($"Failed to get supported command types: {ex.Message}");
                return new string[0];
            }
        }
        
        private void ValidateCommandBusType()
        {
            // Validate that we have a proper CommandBus type
            var hasDispatchMethod = _commandBusType.GetMethods()
                .Any(m => m.Name == "Dispatch" && m.IsGenericMethod);
                
            if (!hasDispatchMethod)
            {
                LogAspera.Warning($"CommandBus type {_commandBusType.Name} may not have expected Dispatch<T> method");
            }
        }
        
        private MethodInfo GetDispatchMethod(Type commandType)
        {
            try
            {
                // Look for generic Dispatch<T> method
                var methods = _commandBusType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.Name == "Dispatch" && m.IsGenericMethod)
                    .ToArray();
                    
                if (methods.Length == 0)
                {
                    LogAspera.Error("No Dispatch methods found on CommandBus");
                    return null;
                }
                
                // Get first generic Dispatch method and make it concrete for our command type
                var genericMethod = methods[0];
                return genericMethod.MakeGenericMethod(commandType);
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Failed to get dispatch method for {commandType.Name}: {ex.Message}");
                return null;
            }
        }
        
        private Type FindCommandType(string typeName)
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
            {
                LogAspera.Error($"Failed to find command type {typeName}: {ex.Message}");
                return null;
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
                {
                    LogAspera.Error("BaseGame type not found via GameTypeInitializer");
                    return null;
                }

                var baseGameInstance = baseGameType.GetProperty("Instance", 
                    BindingFlags.Public | BindingFlags.Static)?.GetValue(null);

                if (baseGameInstance == null)
                {
                    LogAspera.Error("BaseGame.Instance not found");
                    return null;
                }

                // Try to find CommandBus on BaseGame instance
                var commandBusProperty = baseGameType.GetProperty("CommandBus", 
                    BindingFlags.Public | BindingFlags.Instance);

                if (commandBusProperty != null)
                {
                    var commandBusInstance = commandBusProperty.GetValue(baseGameInstance);
                    if (commandBusInstance != null)
                    {
                        LogAspera.Info("CommandBus found via BaseGame.CommandBus property");
                        return new CommandBusWrapper(commandBusInstance);
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
                        {
                            LogAspera.Info($"CommandBus found via BaseGame field: {field.Name}");
                            return new CommandBusWrapper(commandBusInstance);
                        }
                    }
                }

                LogAspera.Error("CommandBus instance not found on BaseGame");
                return null;
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Failed to create CommandBusWrapper from game: {ex.Message}");
                return null;
            }
        }    }
}