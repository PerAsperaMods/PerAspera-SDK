using System;
using System.Collections.Generic;
using System.Linq;

namespace PerAspera.GameAPI.Commands.Core
{
    /// <summary>
    /// Result of command execution with success/failure status and metadata
    /// </summary>
    public class CommandResult
    {
        /// <summary>
        /// Whether the command executed successfully
        /// </summary>
        public bool Success { get; }
        
        /// <summary>
        /// Error message if command failed
        /// </summary>
        public string Error { get; }
        
        /// <summary>
        /// Result message (success or error description)
        /// </summary>
        public string Message => Success ? "Success" : Error ?? "Unknown error";
        
        /// <summary>
        /// Return value from command execution
        /// </summary>
        public object Value { get; set; }
        
        /// <summary>
        /// Command that was executed
        /// </summary>
        public IGameCommand Command { get; }
        
        /// <summary>
        /// When the command execution completed
        /// </summary>
        public DateTime ExecutedAt { get; }
        
        /// <summary>
        /// Execution time in milliseconds
        /// </summary>
        public long ExecutionTimeMs { get; }
        
        /// <summary>
        /// Additional metadata from command execution
        /// </summary>
        public Dictionary<string, object> Metadata { get; }
        
        /// <summary>
        /// Create successful command result
        /// </summary>
        public CommandResult(IGameCommand command, long executionTimeMs, Dictionary<string, object> metadata = null)
        {
            Success = true;
            Command = command;
            ExecutedAt = DateTime.UtcNow;
            ExecutionTimeMs = executionTimeMs;
            Metadata = metadata ?? new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Create failed command result
        /// </summary>
        public CommandResult(IGameCommand command, string error, long executionTimeMs, Dictionary<string, object> metadata = null)
        {
            Success = false;
            Error = error;
            Command = command;
            ExecutedAt = DateTime.UtcNow;
            ExecutionTimeMs = executionTimeMs;
            Metadata = metadata ?? new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Create successful result from existing command
        /// </summary>
        public static CommandResult CreateSuccess(IGameCommand command, long executionTimeMs, Dictionary<string, object> metadata = null)
        {
            return new CommandResult(command, executionTimeMs, metadata);
        }
        
        /// <summary>
        /// Create failed result from existing command
        /// </summary>
        public static CommandResult CreateFailure(IGameCommand command, string error, long executionTimeMs, Dictionary<string, object> metadata = null)
        {
            return new CommandResult(command, error, executionTimeMs, metadata);
        }
        
        public override string ToString()
        {
            return $"CommandResult: {(Success ? "SUCCESS" : "FAILED")} - {Command.CommandType}" +
                   (Success ? "" : $" - {Error}") +
                   $" - {ExecutionTimeMs}ms";
        }
    }
    
    /// <summary>
    /// Result of multiple command execution with aggregate status
    /// </summary>
    public class BatchCommandResult
    {
        /// <summary>
        /// Individual command results
        /// </summary>
        public IReadOnlyList<CommandResult> Results { get; }
        
        /// <summary>
        /// Number of successfully executed commands
        /// </summary>
        public int SuccessCount => Results.Count(r => r.Success);
        
        /// <summary>
        /// Number of failed commands
        /// </summary>
        public int FailureCount => Results.Count(r => !r.Success);
        
        /// <summary>
        /// Total number of commands executed
        /// </summary>
        public int TotalCount => Results.Count;
        
        /// <summary>
        /// Whether all commands succeeded
        /// </summary>
        public bool AllSucceeded => FailureCount == 0;
        
        /// <summary>
        /// Whether any commands succeeded
        /// </summary>
        public bool AnySucceeded => SuccessCount > 0;
        
        /// <summary>
        /// Total execution time for all commands
        /// </summary>
        public long TotalExecutionTimeMs => Results.Sum(r => r.ExecutionTimeMs);
        
        /// <summary>
        /// Get all failed results
        /// </summary>
        public IEnumerable<CommandResult> Failures => Results.Where(r => !r.Success);
        
        /// <summary>
        /// Get all successful results
        /// </summary>
        public IEnumerable<CommandResult> Successes => Results.Where(r => r.Success);
        
        public BatchCommandResult(IEnumerable<CommandResult> results)
        {
            Results = results.ToList().AsReadOnly();
        }
        
        public override string ToString()
        {
            return $"BatchCommandResult: {SuccessCount}/{TotalCount} succeeded - {TotalExecutionTimeMs}ms total";
        }
    }
}