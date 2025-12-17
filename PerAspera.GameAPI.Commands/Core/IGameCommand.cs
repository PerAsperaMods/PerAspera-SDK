using System;

namespace PerAspera.GameAPI.Commands.Core
{
    /// <summary>
    /// Base interface for all game commands (native and custom)
    /// </summary>
    public interface IGameCommand
    {
        /// <summary>
        /// Type of the command (used for routing and handling)
        /// </summary>
        string CommandType { get; }
        
        /// <summary>
        /// When the command was created
        /// </summary>
        DateTime Timestamp { get; }
        
        /// <summary>
        /// Faction that will execute this command
        /// </summary>
        object Faction { get; }
        
        /// <summary>
        /// Validate command parameters before execution
        /// </summary>
        /// <returns>True if command is valid and ready for execution</returns>
        bool IsValid();
        
        /// <summary>
        /// Get human-readable description of command for debugging
        /// </summary>
        string GetDescription();
    }
}
