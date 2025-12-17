using System;
using System.Reflection;
using BepInEx.Logging;
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
        private readonly KeeperWrapper _keeperWrapper;
        private readonly NativeCommandFactory _factory;
        private static CommandBusAccessor _instance;
        
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
            LogAspera.Info("CommandBusAccessor initialized successfully");
        }
        
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
        private CommandBusAccessor(object nativeCommandBus, object nativeKeeper)
        {
            _commandBusWrapper = new CommandBusWrapper(nativeCommandBus);
            _keeperWrapper = new KeeperWrapper(nativeKeeper);
            _factory = NativeCommandFactory.Instance;
            
            ValidateInitialization();
        }
        
        /// <summary>
        /// Execute command via native CommandBus.Dispatch<T>()
        /// </summary>
        public bool ExecuteCommand(CommandBaseWrapper command)
        {
            try
            {
                if (command == null)
                {
                    LogAspera.Warning("Cannot execute null command");
                    return false;
                }
                
                if (!IsAvailable())
                {
                    LogAspera.Error("CommandBus or Keeper not available");
                    return false;
                }
                
                // Validate command before execution
                if (!command.IsValid())
                {
                    LogAspera.Warning($"Command validation failed: {command.CommandName}");
                    return false;
                }
                
                // Execute via CommandBus
                var success = _commandBusWrapper.DispatchCommandByType(
                    command.CommandType.Name, 
                    command.NativeCommand);
                
                if (success)
                {
                    LogAspera.Debug($"Command executed successfully: {command.GetDescription()}");
                }
                else
                {
                    LogAspera.Warning($"Command execution failed: {command.GetDescription()}");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Exception executing command {command?.CommandName}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Create and execute command in one call
        /// </summary>
        public bool CreateAndExecuteCommand(string commandTypeName, params object[] parameters)
        {
            try
            {
                // Create command
                var command = _factory.CreateCommand(commandTypeName, parameters);
                if (command == null)
                {
                    LogAspera.Error($"Failed to create command: {commandTypeName}");
                    return false;
                }
                
                // Execute command
                return ExecuteCommand(command);
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Failed to create and execute command {commandTypeName}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Register object with Keeper
        /// </summary>
        public object RegisterWithKeeper(object handleableObject)
        {
            try
            {
                if (!IsAvailable())
                {
                    LogAspera.Error("Keeper not available for registration");
                    return null;
                }
                
                return _keeperWrapper.RegisterHandleable(handleableObject);
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Failed to register with Keeper: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Unregister object from Keeper
        /// </summary>
        public bool UnregisterFromKeeper(object handleableObject)
        {
            try
            {
                if (!IsAvailable())
                {
                    LogAspera.Error("Keeper not available for unregistration");
                    return false;
                }
                
                return _keeperWrapper.UnregisterHandleable(handleableObject);
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Failed to unregister from Keeper: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Check if CommandBus and Keeper are available
        /// </summary>
        public bool IsAvailable()
        {
            return _commandBusWrapper?.IsAvailable() == true && 
                   _keeperWrapper?.IsAvailable() == true;
        }
        
        /// <summary>
        /// Get supported command types from CommandBus
        /// </summary>
        public string[] GetSupportedCommandTypes()
        {
            try
            {
                return _commandBusWrapper?.GetSupportedCommandTypes() ?? new string[0];
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Failed to get supported command types: {ex.Message}");
                return new string[0];
            }
        }
        
        /// <summary>
        /// Get available command types from factory
        /// </summary>
        public string[] GetAvailableCommandTypes()
        {
            try
            {
                return _factory?.GetAvailableCommandTypes() ?? new string[0];
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Failed to get available command types: {ex.Message}");
                return new string[0];
            }
        }
        
        /// <summary>
        /// Access CommandBus wrapper for advanced scenarios
        /// </summary>
        public CommandBusWrapper GetCommandBusWrapper()
        {
            return _commandBusWrapper;
        }
        
        /// <summary>
        /// Access Keeper wrapper for advanced scenarios
        /// </summary>
        public KeeperWrapper GetKeeperWrapper()
        {
            return _keeperWrapper;
        }
        
        /// <summary>
        /// Access native command factory
        /// </summary>
        public NativeCommandFactory GetFactory()
        {
            return _factory;
        }
        
        /// <summary>
        /// Execute raw command object (advanced usage)
        /// </summary>
        public bool ExecuteRawCommand(object nativeCommand)
        {
            try
            {
                if (nativeCommand == null)
                {
                    LogAspera.Warning("Cannot execute null raw command");
                    return false;
                }
                
                var wrapper = new CommandBaseWrapper(nativeCommand);
                return ExecuteCommand(wrapper);
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Failed to execute raw command: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Get system information for debugging
        /// </summary>
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
        
        private void ValidateInitialization()
        {
            if (!IsAvailable())
            {
                LogAspera.Warning("CommandBusAccessor initialized but some components are not available");
            }
            
            var supportedCount = GetSupportedCommandTypes().Length;
            var availableCount = GetAvailableCommandTypes().Length;
            
            LogAspera.Info($"CommandBusAccessor ready: {supportedCount} supported, {availableCount} available command types");
            
            if (availableCount == 0)
            {
                LogAspera.Warning("No command types found - command execution may not work");
            }
        }
    }
}