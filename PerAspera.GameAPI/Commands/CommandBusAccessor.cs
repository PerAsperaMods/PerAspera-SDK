using System;
using System.Reflection;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI;
using PerAspera.GameAPI.Events.SDK;

namespace PerAspera.GameAPI.Commands
{
    /// <summary>
    /// Provides access to the game's command bus system
    /// Allows mods to execute commands and interact with the game's command infrastructure
    /// </summary>
    public static class CommandBusAccessor
    {
        private static readonly LogAspera _log = new LogAspera("CommandBusAccessor");
        private static object? _commandBusInstance = null;
        private static System.Type? _commandBusType = null;
        private static bool _isInitialized = false;

        /// <summary>
        /// Initialize the command bus accessor
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized)
            {
                _log.Debug("CommandBusAccessor already initialized");
                return;
            }

            try
            {
                _log.Info("🎮 Initializing CommandBusAccessor...");

                // Get command bus type from GameTypeInitializer
                _commandBusType = GameTypeInitializer.GetCommandBusType();

                if (_commandBusType == null)
                {
                    _log.Warning("⚠️ CommandBus type not found - command system may not be available");
                    _isInitialized = true;
                    return;
                }

                _log.Info($"✅ Found CommandBus type: {_commandBusType.FullName}");

                // Try to get command bus instance
                _commandBusInstance = GetCommandBusInstance();

                _isInitialized = true;

                if (_commandBusInstance != null)
                {
                    _log.Info("✅ CommandBusAccessor initialized successfully with active command bus");
                    
                    // Publish command system ready event
                    PublishCommandSystemReadyEvent();
                }
                else
                {
                    _log.Info("✅ CommandBusAccessor initialized (command bus not yet active)");
                }
            }
            catch (Exception ex)
            {
                _log.Error($"❌ Failed to initialize CommandBusAccessor: {ex.Message}");
                _isInitialized = true; // Mark as initialized to prevent retry loops
            }
        }

        /// <summary>
        /// Get the command bus instance from the game
        /// </summary>
        /// <returns>Command bus instance, or null if not available</returns>
        public static object? GetCommandBus()
        {
            EnsureInitialized();

            if (_commandBusInstance != null)
            {
                return _commandBusInstance;
            }

            // Try to get instance if not cached
            _commandBusInstance = GetCommandBusInstance();
            return _commandBusInstance;
        }

        /// <summary>
        /// Check if the command bus is available
        /// </summary>
        /// <returns>True if command bus is available, false otherwise</returns>
        public static bool IsCommandBusAvailable()
        {
            EnsureInitialized();
            return GetCommandBus() != null;
        }

        /// <summary>
        /// Execute a command on the command bus
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <exception cref="InvalidOperationException">Thrown if command bus is not available</exception>
        public static void ExecuteCommand(ICommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            EnsureInitialized();

            var commandBus = GetCommandBus();
            if (commandBus == null)
            {
                throw new InvalidOperationException("Command bus is not available");
            }

            try
            {
                _log.Debug($"Executing command '{command.CommandName}' on command bus");

                // Check if command can be executed
                if (!command.CanExecute())
                {
                    _log.Warning($"Command '{command.CommandName}' cannot be executed in current state");
                    return;
                }

                // Execute the command
                command.Execute();

                _log.Debug($"Command '{command.CommandName}' executed successfully");
            }
            catch (Exception ex)
            {
                _log.Error($"Error executing command '{command.CommandName}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Try to execute a command, returning false on failure instead of throwing
        /// </summary>
        /// <param name="command">The command to execute</param>
        /// <returns>True if command was executed, false otherwise</returns>
        public static bool TryExecuteCommand(ICommand command)
        {
            try
            {
                ExecuteCommand(command);
                return true;
            }
            catch (Exception ex)
            {
                _log.Error($"Failed to execute command '{command?.CommandName ?? "unknown"}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get command bus type
        /// </summary>
        /// <returns>Command bus type, or null if not available</returns>
        public static System.Type? GetCommandBusType()
        {
            EnsureInitialized();
            return _commandBusType;
        }

        /// <summary>
        /// Reset the command bus accessor (for testing)
        /// </summary>
        public static void Reset()
        {
            _log.Debug("Resetting CommandBusAccessor");
            _commandBusInstance = null;
            _commandBusType = null;
            _isInitialized = false;
        }

        /// <summary>
        /// Get command bus instance using reflection
        /// </summary>
        private static object? GetCommandBusInstance()
        {
            try
            {
                // BaseGame.self + GetMemberValue \u2014 RS0030-exempt (typed + Core)
                var bg = BaseGame.self;
                return bg?.GetMemberValue<object>("commandBus")
                    ?? bg?.GetMemberValue<object>("CommandBus");
            }
            catch (Exception ex)
            {
                _log.Warning($"Error getting command bus instance: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Ensure the accessor is initialized
        /// </summary>
        private static void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Publish command system ready event
        /// </summary>
        private static void PublishCommandSystemReadyEvent()
        {
            try
            {
                // Note: Event publishing requires the event bus to be initialized
                // which may not be available at this point in the initialization sequence.
                // Event integration should be handled at the ModSDK level.
                _log.Info("📢 Command system is ready");
            }
            catch (Exception ex)
            {
                _log.Warning($"Could not publish command system ready event: {ex.Message}");
            }
        }
    }
}

