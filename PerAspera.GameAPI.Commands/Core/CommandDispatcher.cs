using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core;
using PerAspera.GameAPI.Commands.Events;
using PerAspera.GameAPI.Events.Core;

namespace PerAspera.GameAPI.Commands.Core
{
    /// <summary>
    /// Main orchestrator for command system - provides high-level API and manages execution
    /// </summary>
    public class CommandDispatcher
    {
        private readonly CommandExecutor _executor;
        private readonly CommandEventBus _eventBus;
        private static CommandDispatcher _instance;
        
        /// <summary>
        /// Global instance for static API access
        /// </summary>
        public static CommandDispatcher Instance
        {
            get
            {
                if (_instance == null)
                    throw new InvalidOperationException("CommandDispatcher not initialized. Call Initialize() first.");
                return _instance;
            }
        }

        /// <summary>
        /// Initialize global dispatcher instance
        /// </summary>
        public static void Initialize(object commandBus, object keeper)
        {
            if (_instance != null)
                throw new InvalidOperationException("CommandDispatcher already initialized.");

            _instance = new CommandDispatcher(commandBus, keeper); // Logging disabled}
        }
        /// <summary>
        /// Reset global instance (for testing)
        /// </summary>
        internal static void Reset()
        {
            _instance = null;
        }
        
        /// <summary>
        /// Initialize dispatcher with executor and event bus
        /// </summary>
        private CommandDispatcher(object commandBus, object keeper)
        {
            _executor = new CommandExecutor(commandBus, keeper);
            _eventBus = new CommandEventBus();
        }
        
        /// <summary>
        /// Execute single command with event publishing
        /// </summary>
        public CommandResult Dispatch(IGameCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
                
            LoggingSystem.Debug($"Dispatching command: {command.GetDescription()}");
            
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
            { // Logging disabledvar failureResult = CommandResult.CreateFailure(command, ex.Message, 0);
                _eventBus.PublishCommandFailed(new CommandFailedEvent(command, ex.Message, 0));
                return failureResult;
            }
        }
        
        /// <summary>
        /// Execute multiple commands in sequence
        /// </summary>
        public BatchCommandResult DispatchBatch(IEnumerable<IGameCommand> commands)
        {
            var commandList = commands.ToList(); // Logging disabledvar results = new List<CommandResult>();
            
            foreach (var command in commandList)
            {
                var result = Dispatch(command);
                results.Add(result);
            }
            
            var batchResult = new BatchCommandResult(results); // Logging disabledreturn batchResult;
        }
        
        /// <summary>
        /// Execute commands until first failure
        /// </summary>
        public BatchCommandResult DispatchBatchUntilFailure(IEnumerable<IGameCommand> commands)
        {
            var commandList = commands.ToList();
            LoggingSystem.Info($"Dispatching batch (stop on failure) of {commandList.Count} commands");
            
            var results = new List<CommandResult>();
            
            foreach (var command in commandList)
            {
                var result = Dispatch(command);
                results.Add(result);
                
                if (!result.Success)
                { // Logging disabledbreak;
                }
            }
            
            var batchResult = new BatchCommandResult(results);
            LoggingSystem.Info($"Batch (stop on failure) complete: {batchResult.SuccessCount}/{batchResult.TotalCount} succeeded");
            
            return batchResult;
        }
        
        /// <summary>
        /// Subscribe to command execution events
        /// </summary>
        public void SubscribeToExecutedEvents(Action<CommandExecutedEvent> handler)
        {
            _eventBus.CommandExecuted += handler;
        }
        
        /// <summary>
        /// Subscribe to command failure events
        /// </summary>
        public void SubscribeToFailedEvents(Action<CommandFailedEvent> handler)
        {
            _eventBus.CommandFailed += handler;
        }
        
        /// <summary>
        /// Unsubscribe from command execution events
        /// </summary>
        public void UnsubscribeFromExecutedEvents(Action<CommandExecutedEvent> handler)
        {
            _eventBus.CommandExecuted -= handler;
        }
        
        /// <summary>
        /// Unsubscribe from command failure events
        /// </summary>
        public void UnsubscribeFromFailedEvents(Action<CommandFailedEvent> handler)
        {
            _eventBus.CommandFailed -= handler;
        }
        
        /// <summary>
        /// Get command execution statistics
        /// </summary>
        public CommandStatistics GetStatistics()
        {
            return _eventBus.GetStatistics();
        }
    }
}
