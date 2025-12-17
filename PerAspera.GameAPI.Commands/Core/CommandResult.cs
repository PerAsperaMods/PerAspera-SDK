using System;
using System.Collections.Generic;
using System.Linq;

namespace PerAspera.GameAPI.Commands.Core
{
    /// &lt;summary&gt;
    /// Result of command execution with success/failure status and metadata
    /// &lt;/summary&gt;
    public class CommandResult
    {
        /// &lt;summary&gt;
        /// Whether the command executed successfully
        /// &lt;/summary&gt;
        public bool Success { get; }
        
        /// &lt;summary&gt;
        /// Error message if command failed
        /// &lt;/summary&gt;
        public string Error { get; }
        
        /// &lt;summary&gt;
        /// Command that was executed
        /// &lt;/summary&gt;
        public IGameCommand Command { get; }
        
        /// &lt;summary&gt;
        /// When the command execution completed
        /// &lt;/summary&gt;
        public DateTime ExecutedAt { get; }
        
        /// &lt;summary&gt;
        /// Execution time in milliseconds
        /// &lt;/summary&gt;
        public long ExecutionTimeMs { get; }
        
        /// &lt;summary&gt;
        /// Additional metadata from command execution
        /// &lt;/summary&gt;
        public Dictionary&lt;string, object&gt; Metadata { get; }
        
        /// &lt;summary&gt;
        /// Create successful command result
        /// &lt;/summary&gt;
        public CommandResult(IGameCommand command, long executionTimeMs, Dictionary&lt;string, object&gt; metadata = null)
        {
            Success = true;
            Command = command;
            ExecutedAt = DateTime.UtcNow;
            ExecutionTimeMs = executionTimeMs;
            Metadata = metadata ?? new Dictionary&lt;string, object&gt;();
        }
        
        /// &lt;summary&gt;
        /// Create failed command result
        /// &lt;/summary&gt;
        public CommandResult(IGameCommand command, string error, long executionTimeMs, Dictionary&lt;string, object&gt; metadata = null)
        {
            Success = false;
            Error = error;
            Command = command;
            ExecutedAt = DateTime.UtcNow;
            ExecutionTimeMs = executionTimeMs;
            Metadata = metadata ?? new Dictionary&lt;string, object&gt;();
        }
        
        /// &lt;summary&gt;
        /// Create successful result from existing command
        /// &lt;/summary&gt;
        public static CommandResult CreateSuccess(IGameCommand command, long executionTimeMs, Dictionary&lt;string, object&gt; metadata = null)
        {
            return new CommandResult(command, executionTimeMs, metadata);
        }
        
        /// &lt;summary&gt;
        /// Create failed result from existing command
        /// &lt;/summary&gt;
        public static CommandResult CreateFailure(IGameCommand command, string error, long executionTimeMs, Dictionary&lt;string, object&gt; metadata = null)
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
    
    /// &lt;summary&gt;
    /// Result of multiple command execution with aggregate status
    /// &lt;/summary&gt;
    public class BatchCommandResult
    {
        /// &lt;summary&gt;
        /// Individual command results
        /// &lt;/summary&gt;
        public IReadOnlyList&lt;CommandResult&gt; Results { get; }
        
        /// &lt;summary&gt;
        /// Number of successfully executed commands
        /// &lt;/summary&gt;
        public int SuccessCount =&gt; Results.Count(r =&gt; r.Success);
        
        /// &lt;summary&gt;
        /// Number of failed commands
        /// &lt;/summary&gt;
        public int FailureCount =&gt; Results.Count(r =&gt; !r.Success);
        
        /// &lt;summary&gt;
        /// Total number of commands executed
        /// &lt;/summary&gt;
        public int TotalCount =&gt; Results.Count;
        
        /// &lt;summary&gt;
        /// Whether all commands succeeded
        /// &lt;/summary&gt;
        public bool AllSucceeded =&gt; FailureCount == 0;
        
        /// &lt;summary&gt;
        /// Whether any commands succeeded
        /// &lt;/summary&gt;
        public bool AnySucceeded =&gt; SuccessCount &gt; 0;
        
        /// &lt;summary&gt;
        /// Total execution time for all commands
        /// &lt;/summary&gt;
        public long TotalExecutionTimeMs =&gt; Results.Sum(r =&gt; r.ExecutionTimeMs);
        
        /// &lt;summary&gt;
        /// Get all failed results
        /// &lt;/summary&gt;
        public IEnumerable&lt;CommandResult&gt; Failures =&gt; Results.Where(r =&gt; !r.Success);
        
        /// &lt;summary&gt;
        /// Get all successful results
        /// &lt;/summary&gt;
        public IEnumerable&lt;CommandResult&gt; Successes =&gt; Results.Where(r =&gt; r.Success);
        
        public BatchCommandResult(IEnumerable&lt;CommandResult&gt; results)
        {
            Results = results.ToList().AsReadOnly();
        }
        
        public override string ToString()
        {
            return $"BatchCommandResult: {SuccessCount}/{TotalCount} succeeded - {TotalExecutionTimeMs}ms total";
        }
    }
}