using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core;

namespace PerAspera.GameAPI.Commands.Events
{
    /// &lt;summary&gt;
    /// Statistics about command execution
    /// &lt;/summary&gt;
    public class CommandStatistics
    {
        public int TotalCommandsExecuted { get; set; }
        public int SuccessfulCommands { get; set; }
        public int FailedCommands { get; set; }
        public long TotalExecutionTimeMs { get; set; }
        public double AverageExecutionTimeMs =&gt; TotalCommandsExecuted &gt; 0 ? (double)TotalExecutionTimeMs / TotalCommandsExecuted : 0;
        public double SuccessRate =&gt; TotalCommandsExecuted &gt; 0 ? (double)SuccessfulCommands / TotalCommandsExecuted : 0;
        
        public Dictionary&lt;string, int&gt; CommandTypeCounts { get; set; } = new Dictionary&lt;string, int&gt;();
        public Dictionary&lt;string, long&gt; CommandTypeExecutionTimes { get; set; } = new Dictionary&lt;string, long&gt;();
        
        public override string ToString()
        {
            return $"CommandStats: {TotalCommandsExecuted} total, {SuccessfulCommands} success ({SuccessRate:P1}), {AverageExecutionTimeMs:F1}ms avg";
        }
    }
    
    /// &lt;summary&gt;
    /// Event bus for command system events
    /// &lt;/summary&gt;
    public class CommandEventBus
    {
        private readonly CommandStatistics _statistics = new CommandStatistics();
        
        /// &lt;summary&gt;
        /// Event fired when a command is executed successfully
        /// &lt;/summary&gt;
        public event Action&lt;CommandExecutedEvent&gt; CommandExecuted;
        
        /// &lt;summary&gt;
        /// Event fired when a command execution fails
        /// &lt;/summary&gt;
        public event Action&lt;CommandFailedEvent&gt; CommandFailed;
        
        /// &lt;summary&gt;
        /// Publish command executed event
        /// &lt;/summary&gt;
        public void PublishCommandExecuted(CommandExecutedEvent eventData)
        {
            try
            {
                // Update statistics
                UpdateStatistics(eventData.Command.CommandType, eventData.Result.ExecutionTimeMs, true);
                
                // Fire event
                CommandExecuted?.Invoke(eventData);
                
                LogAspera.Debug($"Published CommandExecutedEvent: {eventData}");
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Error publishing CommandExecutedEvent: {ex.Message}");
            }
        }
        
        /// &lt;summary&gt;
        /// Publish command failed event
        /// &lt;/summary&gt;
        public void PublishCommandFailed(CommandFailedEvent eventData)
        {
            try
            {
                // Update statistics
                UpdateStatistics(eventData.Command.CommandType, eventData.ExecutionTimeMs, false);
                
                // Fire event
                CommandFailed?.Invoke(eventData);
                
                LogAspera.Debug($"Published CommandFailedEvent: {eventData}");
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Error publishing CommandFailedEvent: {ex.Message}");
            }
        }
        
        /// &lt;summary&gt;
        /// Get command execution statistics
        /// &lt;/summary&gt;
        public CommandStatistics GetStatistics()
        {
            return new CommandStatistics
            {
                TotalCommandsExecuted = _statistics.TotalCommandsExecuted,
                SuccessfulCommands = _statistics.SuccessfulCommands,
                FailedCommands = _statistics.FailedCommands,
                TotalExecutionTimeMs = _statistics.TotalExecutionTimeMs,
                CommandTypeCounts = new Dictionary&lt;string, int&gt;(_statistics.CommandTypeCounts),
                CommandTypeExecutionTimes = new Dictionary&lt;string, long&gt;(_statistics.CommandTypeExecutionTimes)
            };
        }
        
        /// &lt;summary&gt;
        /// Reset statistics
        /// &lt;/summary&gt;
        public void ResetStatistics()
        {
            _statistics.TotalCommandsExecuted = 0;
            _statistics.SuccessfulCommands = 0;
            _statistics.FailedCommands = 0;
            _statistics.TotalExecutionTimeMs = 0;
            _statistics.CommandTypeCounts.Clear();
            _statistics.CommandTypeExecutionTimes.Clear();
            
            LogAspera.Info("Command statistics reset");
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