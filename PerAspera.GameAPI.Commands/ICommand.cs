using System;

namespace PerAspera.GameAPI.Commands
{
    /// <summary>
    /// Base interface for all commands in the PerAspera command system
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// The name/identifier of the command
        /// </summary>
        string CommandName { get; }

        /// <summary>
        /// Execute the command
        /// </summary>
        /// <returns>Result of command execution, or null if no result</returns>
        object? Execute();

        /// <summary>
        /// Check if the command can be executed in the current state
        /// </summary>
        /// <returns>True if command can be executed, false otherwise</returns>
        bool CanExecute();
    }
}
