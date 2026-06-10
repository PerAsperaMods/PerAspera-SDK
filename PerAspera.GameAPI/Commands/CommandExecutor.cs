using System;
using System.Collections.Generic;
using PerAspera.Core;

namespace PerAspera.GameAPI.Commands
{
    /// <summary>
    /// Handles command execution with validation, error handling, and logging
    /// </summary>
    public class CommandExecutor
    {
        private static readonly LogAspera _log = new LogAspera("CommandExecutor");
        private readonly List<ICommand> _commandQueue = new List<ICommand>();
        private bool _isExecuting = false;

        /// <summary>
        /// Execute a command immediately
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <returns>Result of command execution</returns>
        public object? ExecuteCommand(ICommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            try
            {
                _log.Debug($"Executing command: {command.CommandName}");

                if (!command.CanExecute())
                {
                    _log.Warning($"Command '{command.CommandName}' cannot be executed");
                    return null;
                }

                var result = command.Execute();
                _log.Debug($"Command '{command.CommandName}' completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _log.Error($"Command '{command.CommandName}' failed: {ex.Message}");
                throw new CommandExecutionException($"Failed to execute command '{command.CommandName}'", ex);
            }
        }

        /// <summary>
        /// Try to execute a command without throwing exceptions
        /// </summary>
        /// <param name="command">Command to execute</param>
        /// <param name="result">Result of command execution</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool TryExecuteCommand(ICommand command, out object? result)
        {
            result = null;

            try
            {
                result = ExecuteCommand(command);
                return true;
            }
            catch (Exception ex)
            {
                _log.Error($"Command execution failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Queue a command for later execution
        /// </summary>
        /// <param name="command">Command to queue</param>
        public void QueueCommand(ICommand command)
        {
            if (command == null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            lock (_commandQueue)
            {
                _commandQueue.Add(command);
                _log.Debug($"Command '{command.CommandName}' queued ({_commandQueue.Count} in queue)");
            }
        }

        /// <summary>
        /// Execute all queued commands
        /// </summary>
        /// <returns>Number of commands executed</returns>
        public int ExecuteQueuedCommands()
        {
            if (_isExecuting)
            {
                _log.Warning("Already executing queued commands");
                return 0;
            }

            _isExecuting = true;
            int executedCount = 0;

            try
            {
                List<ICommand> commandsToExecute;
                lock (_commandQueue)
                {
                    commandsToExecute = new List<ICommand>(_commandQueue);
                    _commandQueue.Clear();
                }

                _log.Info($"Executing {commandsToExecute.Count} queued commands");

                foreach (var command in commandsToExecute)
                {
                    try
                    {
                        ExecuteCommand(command);
                        executedCount++;
                    }
                    catch (Exception ex)
                    {
                        _log.Error($"Error executing queued command '{command.CommandName}': {ex.Message}");
                        // Continue with next command
                    }
                }

                _log.Info($"Executed {executedCount}/{commandsToExecute.Count} queued commands");
                return executedCount;
            }
            finally
            {
                _isExecuting = false;
            }
        }

        /// <summary>
        /// Get the number of queued commands
        /// </summary>
        public int QueuedCommandCount
        {
            get
            {
                lock (_commandQueue)
                {
                    return _commandQueue.Count;
                }
            }
        }

        /// <summary>
        /// Clear all queued commands
        /// </summary>
        public void ClearQueue()
        {
            lock (_commandQueue)
            {
                var count = _commandQueue.Count;
                _commandQueue.Clear();
                _log.Debug($"Cleared {count} queued commands");
            }
        }
    }

    /// <summary>
    /// Exception thrown when command execution fails
    /// </summary>
    public class CommandExecutionException : Exception
    {
        public CommandExecutionException(string message) : base(message)
        {
        }

        public CommandExecutionException(string message, Exception innerException) 
            : base(message, innerException)
        {
        }
    }
}

