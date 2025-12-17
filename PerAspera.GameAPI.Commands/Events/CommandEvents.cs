using System;

namespace PerAspera.GameAPI.Commands.Events
{
    /// <summary>
    /// Event published when a command is executed successfully
    /// </summary>
    public class CommandExecutedEvent
    {
        /// <summary>
        /// Command that was executed
        /// </summary>
        public Core.IGameCommand Command { get; }
        
        /// <summary>
        /// Result of the execution
        /// </summary>
        public Core.CommandResult Result { get; }
        
        /// <summary>
        /// When the event occurred
        /// </summary>
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
    
    /// <summary>
    /// Event published when a command execution fails
    /// </summary>
    public class CommandFailedEvent
    {
        /// <summary>
        /// Command that failed
        /// </summary>
        public Core.IGameCommand Command { get; }
        
        /// <summary>
        /// Error message
        /// </summary>
        public string Error { get; }
        
        /// <summary>
        /// Execution time before failure
        /// </summary>
        public long ExecutionTimeMs { get; }
        
        /// <summary>
        /// When the event occurred
        /// </summary>
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
