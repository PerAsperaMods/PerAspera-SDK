using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core;
using PerAspera.GameAPI.Commands.Events;
using PerAspera.GameAPI.Events.Core;

namespace PerAspera.GameAPI.Commands.Core
{
    /// &lt;summary&gt;
    /// Main orchestrator for command system - provides high-level API and manages execution
    /// &lt;/summary&gt;
    public class CommandDispatcher
    {
        private readonly CommandExecutor _executor;
        private readonly CommandEventBus _eventBus;
        private static CommandDispatcher _instance;
        
        /// &lt;summary&gt;
        /// Global instance for static API access
        /// &lt;/summary&gt;
        public static CommandDispatcher Instance
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("CommandDispatcher not initialized. Call Initialize() first.");
                return _instance;
            }
        }
        
        /// &lt;summary&gt;
        /// Initialize global dispatcher instance
        /// &lt;/summary&gt;
        public static void Initialize(object commandBus, object keeper)
        {
            if (_instance != null)
                throw new InvalidOperationException("CommandDispatcher already initialized.");
                
            _instance = new CommandDispatcher(commandBus, keeper);
            LogAspera.Info("CommandDispatcher initialized successfully");
        }
        
        /// &lt;summary&gt;
        /// Reset global instance (for testing)
        /// &lt;/summary&gt;
        internal static void Reset()
        {
            _instance = null;
        }
        
        /// &lt;summary&gt;
        /// Initialize dispatcher with executor and event bus
        /// &lt;/summary&gt;
        private CommandDispatcher(object commandBus, object keeper)
        {
            _executor = new CommandExecutor(commandBus, keeper);
            _eventBus = new CommandEventBus();
        }
        
        /// &lt;summary&gt;
        /// Execute single command with event publishing
        /// &lt;/summary&gt;
        public CommandResult Dispatch(IGameCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
                
            LogAspera.Debug($"Dispatching command: {command.GetDescription()}");
            
            try
            {
                // Execute command
                var result = _executor.Execute(command);
                
                // Publish events
                if (result.Success)
                {
                    _eventBus.PublishCommandExecuted(new CommandExecutedEvent(command, result));
                }
                else
                {
                    _eventBus.PublishCommandFailed(new CommandFailedEvent(command, result.Error, result.ExecutionTimeMs));
                }
                
                return result;
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Exception in command dispatch: {ex.Message}");
                var failureResult = CommandResult.CreateFailure(command, ex.Message, 0);
                _eventBus.PublishCommandFailed(new CommandFailedEvent(command, ex.Message, 0));
                return failureResult;
            }
        }
        
        /// &lt;summary&gt;
        /// Execute multiple commands in sequence
        /// &lt;/summary&gt;
        public BatchCommandResult DispatchBatch(IEnumerable&lt;IGameCommand&gt; commands)
        {
            var commandList = commands.ToList();
            LogAspera.Info($"Dispatching batch of {commandList.Count} commands");
            
            var results = new List&lt;CommandResult&gt;();
            
            foreach (var command in commandList)
            {
                var result = Dispatch(command);
                results.Add(result);
            }
            
            var batchResult = new BatchCommandResult(results);
            LogAspera.Info($"Batch complete: {batchResult.SuccessCount}/{batchResult.TotalCount} succeeded");
            
            return batchResult;
        }
        
        /// &lt;summary&gt;
        /// Execute commands until first failure
        /// &lt;/summary&gt;
        public BatchCommandResult DispatchBatchUntilFailure(IEnumerable&lt;IGameCommand&gt; commands)
        {
            var commandList = commands.ToList();
            LogAspera.Info($"Dispatching batch (stop on failure) of {commandList.Count} commands");
            
            var results = new List&lt;CommandResult&gt;();
            
            foreach (var command in commandList)
            {
                var result = Dispatch(command);
                results.Add(result);
                
                if (!result.Success)
                {
                    LogAspera.Warning($"Batch stopped on failure: {result.Error}");
                    break;
                }
            }
            
            var batchResult = new BatchCommandResult(results);
            LogAspera.Info($"Batch (stop on failure) complete: {batchResult.SuccessCount}/{batchResult.TotalCount} succeeded");
            
            return batchResult;
        }
        
        /// &lt;summary&gt;
        /// Subscribe to command execution events
        /// &lt;/summary&gt;
        public void SubscribeToExecutedEvents(Action&lt;CommandExecutedEvent&gt; handler)
        {
            _eventBus.CommandExecuted += handler;
        }
        
        /// &lt;summary&gt;
        /// Subscribe to command failure events
        /// &lt;/summary&gt;
        public void SubscribeToFailedEvents(Action&lt;CommandFailedEvent&gt; handler)
        {
            _eventBus.CommandFailed += handler;
        }
        
        /// &lt;summary&gt;
        /// Unsubscribe from command execution events
        /// &lt;/summary&gt;
        public void UnsubscribeFromExecutedEvents(Action&lt;CommandExecutedEvent&gt; handler)
        {
            _eventBus.CommandExecuted -= handler;
        }
        
        /// &lt;summary&gt;
        /// Unsubscribe from command failure events
        /// &lt;/summary&gt;
        public void UnsubscribeFromFailedEvents(Action&lt;CommandFailedEvent&gt; handler)
        {
            _eventBus.CommandFailed -= handler;
        }
        
        /// &lt;summary&gt;
        /// Get command execution statistics
        /// &lt;/summary&gt;
        public CommandStatistics GetStatistics()
        {
            return _eventBus.GetStatistics();
        }
    }
}