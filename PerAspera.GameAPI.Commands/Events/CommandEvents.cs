using System;

namespace PerAspera.GameAPI.Commands.Events
{
    /// &lt;summary&gt;
    /// Event published when a command is executed successfully
    /// &lt;/summary&gt;
    public class CommandExecutedEvent
    {
        /// &lt;summary&gt;
        /// Command that was executed
        /// &lt;/summary&gt;
        public Core.IGameCommand Command { get; }
        
        /// &lt;summary&gt;
        /// Result of the execution
        /// &lt;/summary&gt;
        public Core.CommandResult Result { get; }
        
        /// &lt;summary&gt;
        /// When the event occurred
        /// &lt;/summary&gt;
        public DateTime Timestamp { get; }
        
        public CommandExecutedEvent(Core.IGameCommand command, Core.CommandResult result)
        {
            Command = command;
            Result = result;
            Timestamp = DateTime.UtcNow;
        }
        
        public override string ToString()
        {
            return $"CommandExecuted: {Command.CommandType} in {Result.ExecutionTimeMs}ms";
        }
    }
    
    /// &lt;summary&gt;
    /// Event published when a command execution fails
    /// &lt;/summary&gt;
    public class CommandFailedEvent
    {
        /// &lt;summary&gt;
        /// Command that failed
        /// &lt;/summary&gt;
        public Core.IGameCommand Command { get; }
        
        /// &lt;summary&gt;
        /// Error message
        /// &lt;/summary&gt;
        public string Error { get; }
        
        /// &lt;summary&gt;
        /// Execution time before failure
        /// &lt;/summary&gt;
        public long ExecutionTimeMs { get; }
        
        /// &lt;summary&gt;
        /// When the event occurred
        /// &lt;/summary&gt;
        public DateTime Timestamp { get; }
        
        public CommandFailedEvent(Core.IGameCommand command, string error, long executionTimeMs)
        {
            Command = command;
            Error = error;
            ExecutionTimeMs = executionTimeMs;
            Timestamp = DateTime.UtcNow;
        }
        
        public override string ToString()
        {
            return $"CommandFailed: {Command.CommandType} - {Error} ({ExecutionTimeMs}ms)";
        }
    }
}