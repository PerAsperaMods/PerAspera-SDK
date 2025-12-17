using System;
using PerAspera.Core;

namespace PerAspera.GameAPI.Commands
{
    /// <summary>
    /// Abstract base class for command implementations
    /// Provides common functionality like logging
    /// </summary>
    public abstract class CommandBase : ICommand
    {
        /// <summary>
        /// Logger instance for this command
        /// </summary>
        protected readonly LogAspera Logger;

        /// <summary>
        /// Initialize the command with logging
        /// </summary>
        protected CommandBase()
        {
            Logger = new LogAspera("Command");
        }
        
        /// <summary>
        /// Initialize the command with a specific logger name
        /// </summary>
        protected CommandBase(string loggerName)
        {
            Logger = new LogAspera(loggerName);
        }

        /// <summary>
        /// The name/identifier of the command
        /// </summary>
        public abstract string CommandName { get; }

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <returns>Result of command execution, or null if no result</returns>
        public abstract object? Execute();

        /// <summary>
        /// Check if the command can be executed in the current state
        /// Default implementation returns true
        /// </summary>
        /// <returns>True if command can be executed, false otherwise</returns>
        public virtual bool CanExecute()
        {
            return true;
        }

        /// <summary>
        /// Validate command before execution
        /// </summary>
        /// <returns>True if validation passed</returns>
        protected virtual bool Validate()
        {
            return true;
        }

        /// <summary>
        /// Execute command with validation and error handling
        /// </summary>
        /// <returns>Result of command execution</returns>
        public object? ExecuteSafe()
        {
            try
            {
                if (!CanExecute())
                {
                    Logger.Warning($"Command '{CommandName}' cannot be executed in current state");
                    return null;
                }

                if (!Validate())
                {
                    Logger.Warning($"Command '{CommandName}' validation failed");
                    return null;
                }

                Logger.Debug($"Executing command '{CommandName}'");
                var result = Execute();
                Logger.Debug($"Command '{CommandName}' executed successfully");
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error executing command '{CommandName}': {ex.Message}");
                throw;
            }
        }
    }
}
