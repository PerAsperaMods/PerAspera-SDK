using System;

namespace PerAspera.GameAPI.Commands.Core
{
    /// &lt;summary&gt;
    /// Base interface for all game commands (native and custom)
    /// &lt;/summary&gt;
    public interface IGameCommand
    {
        /// &lt;summary&gt;
        /// Type of the command (used for routing and handling)
        /// &lt;/summary&gt;
        string CommandType { get; }
        
        /// &lt;summary&gt;
        /// When the command was created
        /// &lt;/summary&gt;
        DateTime Timestamp { get; }
        
        /// &lt;summary&gt;
        /// Faction that will execute this command
        /// &lt;/summary&gt;
        object Faction { get; }
        
        /// &lt;summary&gt;
        /// Validate command parameters before execution
        /// &lt;/summary&gt;
        /// &lt;returns&gt;True if command is valid and ready for execution&lt;/returns&gt;
        bool IsValid();
        
        /// &lt;summary&gt;
        /// Get human-readable description of command for debugging
        /// &lt;/summary&gt;
        string GetDescription();
    }
}