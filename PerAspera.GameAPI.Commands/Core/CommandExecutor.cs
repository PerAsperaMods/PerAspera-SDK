using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BepInEx.Logging;
using PerAspera.Core;
using PerAspera.GameAPI.Commands.Native;

namespace PerAspera.GameAPI.Commands.Core
{
    /// <summary>
    /// Core executor for game commands - bridges SDK to native CommandBus
    /// </summary>
    public class CommandExecutor
    {
        private readonly object _commandBus;
        private readonly object _keeper;
        
        /// <summary>
        /// Initialize executor with access to native CommandBus and Keeper
        /// </summary>
        public CommandExecutor(object commandBus, object keeper)
        {
            _commandBus = commandBus ?? throw new ArgumentNullException(nameof(commandBus));
            _keeper = keeper ?? throw new ArgumentNullException(nameof(keeper));
        }
        
        /// <summary>
        /// Execute a single command synchronously
        /// </summary>
        public CommandResult Execute(IGameCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
                
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            try
            {
                // Validate command before execution
                if (!command.IsValid())
                {
                    stopwatch.Stop();
                    return CommandResult.CreateFailure(command, "Command validation failed", stopwatch.ElapsedMilliseconds);
                }
                
                // Execute command via native CommandBus
                var success = ExecuteNativeCommand(command);
                
                stopwatch.Stop();
                
                if (success)
                {
                    LogAspera.Info($"Command executed successfully: {command.GetDescription()}");
                    return CommandResult.CreateSuccess(command, stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    LogAspera.Warning($"Command execution failed: {command.GetDescription()}");
                    return CommandResult.CreateFailure(command, "Native execution failed", stopwatch.ElapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                LogAspera.Error($"Exception executing command {command.GetDescription()}: {ex.Message}");
                return CommandResult.CreateFailure(command, ex.Message, stopwatch.ElapsedMilliseconds);
            }
        }
        
        /// <summary>
        /// Execute a single command asynchronously
        /// </summary>
        public async Task<CommandResult> ExecuteAsync(IGameCommand command)
        {
            return await Task.Run(() => Execute(command));
        }
        
        /// <summary>
        /// Execute multiple commands in sequence
        /// </summary>
        public BatchCommandResult ExecuteBatch(IEnumerable<IGameCommand> commands)
        {
            var results = new List<CommandResult>();
            
            foreach (var command in commands)
            {
                var result = Execute(command);
                results.Add(result);
                
                // Log batch progress
                LogAspera.Info($"Batch command {results.Count}: {(result.Success ? "SUCCESS" : "FAILED")} - {command.GetDescription()}");
            }
            
            return new BatchCommandResult(results);
        }
        
        /// <summary>
        /// Execute commands in parallel (use with caution - may cause race conditions)
        /// </summary>
        public async Task<BatchCommandResult> ExecuteBatchParallelAsync(IEnumerable<IGameCommand> commands, int maxConcurrency = 4)
        {
            var semaphore = new SemaphoreSlim(maxConcurrency);
            var tasks = commands.Select(async command =>
            {
                await semaphore.WaitAsync();
                try
                {
                    return await ExecuteAsync(command);
                }
                finally
                {
                    semaphore.Release();
                }
            });
            
            var results = await Task.WhenAll(tasks);
            return new BatchCommandResult(results);
        }
        
        /// <summary>
        /// Execute native command through CommandBus.Dispatch<T>() → Keeper.Register()
        /// This is the core bridge to the Per Aspera command system
        /// </summary>
        private bool ExecuteNativeCommand(IGameCommand command)
        {
            try
            {
                // TODO: Implement actual native command execution
                // This will use reflection or IL2CPP interop to call:
                // CommandBus.Dispatch<CommandType>(nativeCommandInstance)
                // which then calls Keeper.Register() internally
                
                LogAspera.Debug($"Executing native command: {command.CommandType}");
                
                // Placeholder for native execution
                // In real implementation, this would:
                // 1. Convert SDK command to native Command class instance
                // 2. Call CommandBus.Dispatch<T>(nativeCommand)
                // 3. Return success/failure based on result
                
                return true; // Placeholder return
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Failed to execute native command {command.CommandType}: {ex.Message}");
                return false;
            }
        }
    }
}