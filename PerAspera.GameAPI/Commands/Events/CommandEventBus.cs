using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core;

namespace PerAspera.GameAPI.Commands.Events
{
    /// <summary>
    /// Statistics about command execution
    /// </summary>
    public class CommandStatistics
    {
        public int TotalCommandsExecuted { get; set; }
        public int SuccessfulCommands { get; set; }
        public int FailedCommands { get; set; }
        public long TotalExecutionTimeMs { get; set; }
        public double AverageExecutionTimeMs => TotalCommandsExecuted > 0 ? (double)TotalExecutionTimeMs / TotalCommandsExecuted : 0;
        public double SuccessRate => TotalCommandsExecuted > 0 ? (double)SuccessfulCommands / TotalCommandsExecuted : 0;
        
        public Dictionary<string, int> CommandTypeCounts { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, long> CommandTypeExecutionTimes { get; set; } = new Dictionary<string, long>();
        
        public override string ToString()
        {
            return $"CommandStats: {TotalCommandsExecuted} total, {SuccessfulCommands} success ({SuccessRate:P1}), {AverageExecutionTimeMs:F1}ms avg";
        }
    }
    
    /// <summary>
    /// Event bus for command system events
    /// </summary>
    public class CommandEventBus
    {
        private readonly CommandStatistics _statistics = new CommandStatistics();
        
        /// <summary>
        /// Event fired when a command is executed successfully
        /// </summary>
        public event Action<CommandExecutedEvent> CommandExecuted;
        
        /// <summary>
        /// Event fired when a command execution fails
        /// </summary>
        public event Action<CommandFailedEvent> CommandFailed;
        
        /// <summary>
        /// Publish command executed event
        /// </summary>
        public void PublishCommandExecuted(CommandExecutedEvent eventData)
        {
            try
            {
                // Update statistics
                UpdateStatistics(eventData.Command.CommandType, eventData.Result.ExecutionTimeMs, true);

                // Fire event
                CommandExecuted?.Invoke(eventData);
            }
            catch (Exception ex)
            {
                // Error handling
            }
        }

        /// <summary>
        /// Publish command failed event
        /// </summary>
        public void PublishCommandFailed(CommandFailedEvent eventData)
        {
            try
            {
                // Update statistics
                UpdateStatistics(eventData.Command.CommandType, eventData.ExecutionTimeMs, false);

                // Fire event
                CommandFailed?.Invoke(eventData);
            }
            catch (Exception ex)
            {
                // Error handling
            }
            { // Logging disabled}
            }
        }
        
        /// <summary>
        /// Get command execution statistics
        /// </summary>
        public CommandStatistics GetStatistics()
        {
            return new CommandStatistics
            {
                TotalCommandsExecuted = _statistics.TotalCommandsExecuted,
                SuccessfulCommands = _statistics.SuccessfulCommands,
                FailedCommands = _statistics.FailedCommands,
                TotalExecutionTimeMs = _statistics.TotalExecutionTimeMs,
                CommandTypeCounts = new Dictionary<string, int>(_statistics.CommandTypeCounts),
                CommandTypeExecutionTimes = new Dictionary<string, long>(_statistics.CommandTypeExecutionTimes)
            };
        }

        /// <summary>
        /// Reset statistics
        /// </summary>
        public void ResetStatistics()
        {
            _statistics.TotalCommandsExecuted = 0;
            _statistics.SuccessfulCommands = 0;
            _statistics.FailedCommands = 0;
            _statistics.TotalExecutionTimeMs = 0;
            _statistics.CommandTypeCounts.Clear();
            _statistics.CommandTypeExecutionTimes.Clear(); // Logging disabled}
        }
        private void UpdateStatistics(string commandType, long executionTimeMs, bool success)
        {
            _statistics.TotalCommandsExecuted++;
            _statistics.TotalExecutionTimeMs += executionTimeMs;
            
            if (success)
                _statistics.SuccessfulCommands++;
            else
                _statistics.FailedCommands++;
                
            // Update command type statistics
            _statistics.CommandTypeCounts[commandType] = _statistics.CommandTypeCounts.GetValueOrDefault(commandType, 0) + 1;
            _statistics.CommandTypeExecutionTimes[commandType] = _statistics.CommandTypeExecutionTimes.GetValueOrDefault(commandType, 0) + executionTimeMs;
        }
    }
}

