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
        private readonly KeeperWrapper? _keeperWrapper;
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
                
            _instance = new CommandBusAccessor(nativeCommandBus, nativeKeeper); // Logging disabled}

        /// <summary>
        /// Auto-initialize using GameTypeInitializer (Phase 1.1 integration)
        /// Attempts to find CommandBus and Keeper from BaseGame automatically
        /// </summary>
        public static bool TryAutoInitialize()
        {
            if (_instance != null)
            { // Logging disabledreturn true;
            }

            try
            { // Logging disabled// Initialize GameTypeInitializer first
                GameTypeInitializer.Initialize();

                // Get BaseGame instance
                var baseGameType = GameTypeInitializer.GetBaseGameType();
                if (baseGameType == null)
                { // Logging disabledreturn false;
                }

                var baseGameInstance = baseGameType.GetProperty("Instance", 
                    BindingFlags.Public | BindingFlags.Static)?.GetValue(null);

                if (baseGameInstance == null)
                { // Logging disabledreturn false;
                }

                // Find CommandBus on BaseGame
                object commandBusInstance = null;
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
                { // Logging disabledreturn false;
                }

                // Find Keeper (similar approach)
                object keeperInstance = null;
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
                LoggingSystem.Info($"CommandBusAccessor auto-initialized successfully (CommandBus: {commandBusInstance != null}, Keeper: {keeperInstance != null})");
                return true;
            }
            catch (Exception ex)
            { // Logging disabledreturn false;
            }
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
            
            // Only create KeeperWrapper if keeper instance is available
            if (nativeKeeper != null)
            {
                _keeperWrapper = new KeeperWrapper(nativeKeeper);
            }
            else
            {
                _keeperWrapper = null; // Logging disabled}
            
            _factory = NativeCommandFactory.Instance;
            
            ValidateInitialization();
        }
        
        /// <summary>
        /// Execute command via native CommandBus.Dispatch()
        /// </summary>
        public bool ExecuteCommand(CommandBaseWrapper command)
        {
            try
            {
                if (command == null)
                { // Logging disabledreturn false;
                }
                
                if (!IsAvailable())
                { // Logging disabledreturn false;
                }
                
                // Validate command before execution
                if (!command.IsValid())
                { // Logging disabledreturn false;
                }
                
                // Execute via CommandBus
                var success = _commandBusWrapper.DispatchCommandByType(
                    command.CommandType.Name, 
                    command.NativeCommand);
                
                if (success)
                {
                    LoggingSystem.Debug($"Command executed successfully: {command.GetDescription()}");
                }
                else
                {
                    LoggingSystem.Warning($"Command execution failed: {command.GetDescription()}");
                }
                
                return success;
            }
            catch (Exception ex)
            { // Logging disabledreturn false;
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
                { // Logging disabledreturn false;
                }
                
                // Execute command
                return ExecuteCommand(command);
            }
            catch (Exception ex)
            { // Logging disabledreturn false;
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
                { // Logging disabledreturn null;
                }
                
                if (_keeperWrapper == null)
                { // Logging disabledreturn null;
                }
                
                return _keeperWrapper.RegisterHandleable(handleableObject);
            }
            catch (Exception ex)
            { // Logging disabledreturn null;
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
                { // Logging disabledreturn false;
                }
                
                if (_keeperWrapper == null)
                { // Logging disabledreturn false;
                }
                
                return _keeperWrapper.UnregisterHandleable(handleableObject);
            }
            catch (Exception ex)
            { // Logging disabledreturn false;
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
            { // Logging disabledreturn new string[0];
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
            { // Logging disabledreturn new string[0];
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
        /// Access Keeper wrapper for advanced scenarios (may be null)
        /// </summary>
        public KeeperWrapper? GetKeeperWrapper()
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
                { // Logging disabledreturn false;
                }
                
                var wrapper = new CommandBaseWrapper(nativeCommand);
                return ExecuteCommand(wrapper);
            }
            catch (Exception ex)
            { // Logging disabledreturn false;
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
            { // Logging disabled}
            
            var supportedCount = GetSupportedCommandTypes().Length;
            var availableCount = GetAvailableCommandTypes().Length; // Logging disabledif (availableCount == 0)
            { // Logging disabled}
        }
    }
}
