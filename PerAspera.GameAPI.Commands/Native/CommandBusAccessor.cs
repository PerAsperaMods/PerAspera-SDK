using System;
using System.Reflection;
using PerAspera.Core;
using PerAspera.GameAPI.Commands.Native.IL2CPPInterop;

namespace PerAspera.GameAPI.Commands.Native
{
    /// <summary>
    /// Main accessor for native CommandBus system via IL2CPP interop
    /// Provides high-level API for command execution through native Per Aspera systems
    /// </summary>
    public class CommandBusAccessor
    {
        private readonly CommandBusWrapper _commandBusWrapper;
        private readonly KeeperWrapper? _keeperWrapper;
        private readonly NativeCommandFactory _factory;
        private static CommandBusAccessor? _instance;
        private static readonly LogAspera _logger = new LogAspera("GameAPI.Commands.BusAccessor"); // ✅ AJOUT
        
        /// <summary>
        /// Singleton instance for global access
        /// </summary>
        public static CommandBusAccessor Instance
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("CommandBusAccessor not initialized. Call Initialize() first.");
                return _instance;
            }
        }

        /// <summary>
        /// Initialize global accessor with native CommandBus and Keeper
        /// </summary>
        /// <param name="nativeCommandBus">Native CommandBus instance from game</param>
        /// <param name="nativeKeeper">Native Keeper instance from game</param>
        public static void Initialize(object nativeCommandBus, object nativeKeeper)
        {
            if (_instance != null)
                throw new InvalidOperationException("CommandBusAccessor already initialized.");

            _instance = new CommandBusAccessor(nativeCommandBus, nativeKeeper);
        } // ✅ CORRECTION: Accolade fermante manquante

        /// <summary>
        /// Auto-initialize using GameTypeInitializer (Phase 1.1 integration)
        /// Attempts to find CommandBus and Keeper from BaseGame automatically
        /// </summary>
        /// <returns>True if initialization succeeded, false otherwise</returns>
        public static bool TryAutoInitialize()
        {
            if (_instance != null)
            {
                _logger.Debug("CommandBusAccessor already initialized");
                return true;
            }

            try
            {
                _logger.Info("Attempting auto-initialization of CommandBusAccessor");
                
                // Initialize GameTypeInitializer first
                GameTypeInitializer.Initialize();

                // Get BaseGame instance
                var baseGameType = GameTypeInitializer.GetBaseGameType();
                if (baseGameType == null)
                {
                    _logger.Warning("BaseGame type not found");
                    return false;
                }

                var baseGameInstance = baseGameType.GetProperty("Instance", 
                    BindingFlags.Public | BindingFlags.Static)?.GetValue(null);

                if (baseGameInstance == null)
                {
                    _logger.Warning("BaseGame instance not found");
                    return false;
                }

                // Find CommandBus on BaseGame
                object? commandBusInstance = null;
                var commandBusProperty = baseGameType.GetProperty("CommandBus", BindingFlags.Public | BindingFlags.Instance);
                if (commandBusProperty != null)
                {
                    commandBusInstance = commandBusProperty.GetValue(baseGameInstance);
                }

                if (commandBusInstance == null)
                {
                    // Try field search
                    var commandBusFields = baseGameType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (var field in commandBusFields)
                    {
                        if (field.Name.Contains("CommandBus") || field.Name.Contains("commandBus"))
                        {
                            commandBusInstance = field.GetValue(baseGameInstance);
                            if (commandBusInstance != null) break;
                        }
                    }
                }

                if (commandBusInstance == null)
                {
                    _logger.Warning("CommandBus instance not found");
                    return false;
                }

                // Find Keeper (similar approach)
                object? keeperInstance = null;
                var keeperProperty = baseGameType.GetProperty("Keeper", BindingFlags.Public | BindingFlags.Instance);
                if (keeperProperty != null)
                {
                    keeperInstance = keeperProperty.GetValue(baseGameInstance);
                }

                if (keeperInstance == null)
                {
                    // Try field search for Keeper
                    var keeperFields = baseGameType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (var field in keeperFields)
                    {
                        if (field.Name.Contains("Keeper") || field.Name.Contains("keeper"))
                        {
                            keeperInstance = field.GetValue(baseGameInstance);
                            if (keeperInstance != null) break;
                        }
                    }
                }

                // Initialize with found instances (keeper can be null for now)
                _instance = new CommandBusAccessor(commandBusInstance, keeperInstance);
                _logger.Info($"CommandBusAccessor auto-initialized successfully (CommandBus: {commandBusInstance != null}, Keeper: {keeperInstance != null})");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"Auto-initialization failed: {ex.Message}");
                return false;
            }
        } // ✅ CORRECTION: Toutes les branches retournent une valeur
        
        /// <summary>
        /// Reset global instance (for testing)
        /// </summary>
        internal static void Reset()
        {
            _instance = null;
        }
        
        /// <summary>
        /// Initialize accessor with wrappers
        /// </summary>
        /// <param name="nativeCommandBus">Native CommandBus instance</param>
        /// <param name="nativeKeeper">Native Keeper instance (can be null)</param>
        private CommandBusAccessor(object nativeCommandBus, object? nativeKeeper)
        {
            _commandBusWrapper = new CommandBusWrapper(nativeCommandBus);

            // Only create KeeperWrapper if keeper instance is available
            if (nativeKeeper != null)
            {
                _keeperWrapper = new KeeperWrapper(nativeKeeper);
            }
            else
            {
                _keeperWrapper = null;
                _logger.Warning("Keeper instance not available");
            } // ✅ CORRECTION: Accolade fermante et else corrigé

            _factory = NativeCommandFactory.Instance;

            ValidateInitialization();
        } // ✅ CORRECTION: Accolade fermante manquante
        
        /// <summary>
        /// Execute command via native CommandBus.Dispatch()
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <returns>True if command executed successfully, false otherwise</returns>
        public bool ExecuteCommand(CommandBaseWrapper command)
        {
            try
            {
                if (command == null)
                {
                    _logger.Warning("Command is null");
                    return false;
                }
                
                if (!IsAvailable())
                {
                    _logger.Warning("CommandBus or Keeper not available");
                    return false;
                }
                
                // Validate command before execution
                if (!command.IsValid())
                {
                    _logger.Warning($"Command validation failed: {command.GetDescription()}");
                    return false;
                }
                
                // Execute via CommandBus
                var success = _commandBusWrapper.DispatchCommandByType(
                    command.CommandType.Name, 
                    command.NativeCommand);
                
                if (success)
                {
                    _logger.Debug($"Command executed successfully: {command.GetDescription()}");
                }
                else
                {
                    _logger.Warning($"Command execution failed: {command.GetDescription()}");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception executing command: {ex.Message}");
                return false;
            }
        } // ✅ CORRECTION: Toutes les branches retournent une valeur
        
        /// <summary>
        /// Create and execute command in one call
        /// </summary>
        /// <param name="commandTypeName">Type name of command to create</param>
        /// <param name="parameters">Command parameters</param>
        /// <returns>True if command was created and executed successfully, false otherwise</returns>
        public bool CreateAndExecuteCommand(string commandTypeName, params object[] parameters)
        {
            try
            {
                // Create command
                var command = _factory.CreateCommand(commandTypeName, parameters);
                if (command == null)
                {
                    _logger.Warning($"Failed to create command: {commandTypeName}");
                    return false;
                }
                
                // Execute command
                return ExecuteCommand(command);
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception creating/executing command {commandTypeName}: {ex.Message}");
                return false;
            }
        } // ✅ CORRECTION: Toutes les branches retournent une valeur
        
        /// <summary>
        /// Register object with Keeper
        /// </summary>
        /// <param name="handleableObject">Object to register</param>
        /// <returns>Registration result or null if failed</returns>
        public object? RegisterWithKeeper(object handleableObject)
        {
            try
            {
                if (!IsAvailable())
                {
                    _logger.Warning("System not available for keeper registration");
                    return null;
                }
                
                if (_keeperWrapper == null)
                {
                    _logger.Warning("Keeper wrapper not available");
                    return null;
                }
                
                return _keeperWrapper.RegisterHandleable(handleableObject);
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception registering with keeper: {ex.Message}");
                return null;
            }
        } // ✅ CORRECTION: Toutes les branches retournent une valeur
        
        /// <summary>
        /// Unregister object from Keeper
        /// </summary>
        /// <param name="handleableObject">Object to unregister</param>
        /// <returns>True if unregistered successfully, false otherwise</returns>
        public bool UnregisterFromKeeper(object handleableObject)
        {
            try
            {
                if (!IsAvailable())
                {
                    _logger.Warning("System not available for keeper unregistration");
                    return false;
                }
                
                if (_keeperWrapper == null)
                {
                    _logger.Warning("Keeper wrapper not available");
                    return false;
                }
                
                return _keeperWrapper.UnregisterHandleable(handleableObject);
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception unregistering from keeper: {ex.Message}");
                return false;
            }
        } // ✅ CORRECTION: Toutes les branches retournent une valeur
        
        /// <summary>
        /// Check if CommandBus and Keeper are available
        /// </summary>
        /// <returns>True if both systems are available, false otherwise</returns>
        public bool IsAvailable()
        {
            return _commandBusWrapper?.IsAvailable() == true && 
                   _keeperWrapper?.IsAvailable() == true;
        }
        
        /// <summary>
        /// Get supported command types from CommandBus
        /// </summary>
        /// <returns>Array of supported command type names</returns>
        public string[] GetSupportedCommandTypes()
        {
            try
            {
                return _commandBusWrapper?.GetSupportedCommandTypes() ?? Array.Empty<string>();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting supported command types: {ex.Message}");
                return Array.Empty<string>();
            }
        } // ✅ CORRECTION: Toutes les branches retournent une valeur
        
        /// <summary>
        /// Get available command types from factory
        /// </summary>
        /// <returns>Array of available command type names</returns>
        public string[] GetAvailableCommandTypes()
        {
            try
            {
                return _factory?.GetAvailableCommandTypes() ?? Array.Empty<string>();
            }
            catch (Exception ex)
            {
                _logger.Error($"Error getting available command types: {ex.Message}");
                return Array.Empty<string>();
            }
        } // ✅ CORRECTION: Toutes les branches retournent une valeur
        
        /// <summary>
        /// Access CommandBus wrapper for advanced scenarios
        /// </summary>
        /// <returns>CommandBus wrapper instance</returns>
        public CommandBusWrapper GetCommandBusWrapper()
        {
            return _commandBusWrapper;
        }
        
        /// <summary>
        /// Access Keeper wrapper for advanced scenarios (may be null)
        /// </summary>
        /// <returns>Keeper wrapper instance or null if not available</returns>
        public KeeperWrapper? GetKeeperWrapper()
        {
            return _keeperWrapper;
        }
        
        /// <summary>
        /// Access native command factory
        /// </summary>
        /// <returns>Native command factory instance</returns>
        public NativeCommandFactory GetFactory()
        {
            return _factory;
        }
        
        /// <summary>
        /// Execute raw command object (advanced usage)
        /// </summary>
        /// <param name="nativeCommand">Native command object to execute</param>
        /// <returns>True if command executed successfully, false otherwise</returns>
        public bool ExecuteRawCommand(object nativeCommand)
        {
            try
            {
                if (nativeCommand == null)
                {
                    _logger.Warning("Native command is null");
                    return false;
                }
                
                var wrapper = new CommandBaseWrapper(nativeCommand);
                return ExecuteCommand(wrapper);
            }
            catch (Exception ex)
            {
                _logger.Error($"Exception executing raw command: {ex.Message}");
                return false;
            }
        } // ✅ CORRECTION: Toutes les branches retournent une valeur
        
        /// <summary>
        /// Get system information for debugging
        /// </summary>
        /// <returns>Formatted system information string</returns>
        public string GetSystemInfo()
        {
            try
            {
                var commandBusAvailable = _commandBusWrapper?.IsAvailable() ?? false;
                var keeperAvailable = _keeperWrapper?.IsAvailable() ?? false;
                var supportedCommands = GetSupportedCommandTypes().Length;
                var availableCommands = GetAvailableCommandTypes().Length;
                
                return $"CommandBusAccessor Status:\n" +
                       $"- CommandBus Available: {commandBusAvailable}\n" +
                       $"- Keeper Available: {keeperAvailable}\n" +
                       $"- Supported Commands: {supportedCommands}\n" +
                       $"- Available Commands: {availableCommands}\n" +
                       $"- Overall Available: {IsAvailable()}";
            }
            catch (Exception ex)
            {
                return $"Failed to get system info: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Validate that initialization completed successfully
        /// </summary>
        private void ValidateInitialization()
        {
            if (!IsAvailable())
            {
                _logger.Warning("CommandBusAccessor initialization validation: Some systems not available");
            }
            
            var supportedCount = GetSupportedCommandTypes().Length;
            var availableCount = GetAvailableCommandTypes().Length;
            
            _logger.Debug($"CommandBusAccessor initialized: {supportedCount} supported, {availableCount} available commands");
            
            if (availableCount == 0)
            {
                _logger.Warning("No available commands detected - command factory may not be ready");
            }
        } // ✅ CORRECTION: Accolade fermante manquante
    }
}
