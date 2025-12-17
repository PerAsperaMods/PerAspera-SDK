using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BepInEx.Logging;
using PerAspera.Core;
using PerAspera.GameAPI.Commands.Native;
using PerAspera.GameAPI.Commands.Native.IL2CPPInterop;

namespace PerAspera.GameAPI.Commands.Core
{
    /// <summary>
    /// Core executor for game commands - bridges SDK to native CommandBus
    /// </summary>
    public class CommandExecutor
    {
        private readonly object _commandBus;
        private readonly object _keeper;
        private static ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("ClassName");

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
                    return new CommandResult(command, "Command validation failed", stopwatch.ElapsedMilliseconds);
                }
                
                // Execute command via native CommandBus
                var success = ExecuteNativeCommand(command);
                
                stopwatch.Stop();
                
                if (success)
                {
                    _logger.LogInfo($"Command executed successfully: {command.GetDescription()}");
                    return CommandResult.CreateSuccess(command, stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    _logger.LogWarning($"Command execution failed: {command.GetDescription()}");
                    return CommandResult.CreateFailure(command, "Native execution failed", stopwatch.ElapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError($"Exception executing command {command.GetDescription()}: {ex.Message}");
                return CommandResult.CreateFailure(command, ex.Message, stopwatch.ElapsedMilliseconds);
            }
        }
        
        /// <summary>
        /// Execute a single command asynchronously
        /// </summary>
        public async System.Threading.Tasks.Task<CommandResult> ExecuteAsync(IGameCommand command)
        {
            return await System.Threading.Tasks.Task.Run(() => Execute(command));
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
                _logger.LogInfo($"Batch command {results.Count}: {(result.Success ? "SUCCESS" : "FAILED")} - {command.GetDescription()}");
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
            
            var results = await System.Threading.Tasks.Task.WhenAll(tasks);
            return new BatchCommandResult(results);
        }
        
        /// <summary>
        /// Execute native command through CommandBus.Dispatch() → Keeper.Register()
        /// This is the core bridge to the Per Aspera command system
        /// Phase 1.1: Now integrated with GameTypeInitializer
        /// </summary>
        private bool ExecuteNativeCommand(IGameCommand command)
        {
            try
            {
                // Initialize CommandBusAccessor if needed
                CommandBusAccessor.Initialize();

                if (!CommandBusAccessor.IsCommandBusAvailable())
                {
                    return false;
                }

                // Convert SDK command to native command via NativeCommandFactory
                var nativeCommand = ConvertToNativeCommand(command);
                if (nativeCommand == null)
                {
                    return false;
                }

                // Execute via CommandBusAccessor
                CommandBusAccessor.ExecuteCommand(nativeCommand);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Convert SDK command to native Per Aspera command instance
        /// Phase 1.2: MVP implementation for basic command types
        /// </summary>
        private CommandBaseWrapper ConvertToNativeCommand(IGameCommand command)
        {
            try
            {
                var factory = NativeCommandFactory.Instance;
                
                // Phase 1.2 MVP: Special handling for ImportResource
                if (command.CommandType == "ImportResource")
                {
                    // Try to extract resource and amount from command
                    if (TryExtractImportResourceParameters(command, out string resourceName, out float amount))
                    { // Logging disable
                      return factory.CreateImportResourceCommand(resourceName, amount);
                    }
                    else
                    { // Logging disabled}
                       throw new InvalidOperationException("Failed to extract parameters for ImportResource command");
                    }
                }
                
                // Fallback: Generic parameter extraction
                var parameters = ExtractCommandParameters(command);
                var nativeCommand = factory.CreateCommand(command.CommandType, parameters);

                    if (nativeCommand == null)
                    { // Logging disabled}

                        return nativeCommand;
                    }
            }
            catch (Exception ex)
            { // Logging disabledreturn null;
                throw new Exception($"Error converting command {command.CommandType} to native: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Try to extract ImportResource parameters from SDK command
        /// Phase 1.2: MVP parameter extraction for testing
        /// </summary>
        private bool TryExtractImportResourceParameters(IGameCommand command, out string resourceName, out float amount)
        {
            resourceName = null;
            amount = 0f;

            try
            {
                // Use reflection to find resource and amount properties
                var commandType = command.GetType();
                var properties = commandType.GetProperties();

                foreach (var prop in properties)
                {
                    if (prop.Name.Contains("Resource") && prop.PropertyType == typeof(string))
                    {
                        resourceName = (string)prop.GetValue(command);
                    }
                    else if ((prop.Name.Contains("Amount") || prop.Name.Contains("Quantity")) && 
                             (prop.PropertyType == typeof(float) || prop.PropertyType == typeof(double) || prop.PropertyType == typeof(int)))
                    {
                        var value = prop.GetValue(command);
                        amount = Convert.ToSingle(value);
                    }
                }

                var hasValidParams = !string.IsNullOrEmpty(resourceName) && amount > 0;
                
                if (hasValidParams)
                {
                    // Success case
                }
                else
                {
                    // Failure case
                }

                return hasValidParams;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Extract parameters from SDK command for native command creation
        /// Phase 1.2: Basic parameter extraction
        /// </summary>
        private object[] ExtractCommandParameters(IGameCommand command)
        {
            // TODO: Implement proper parameter extraction based on command type
            // This would analyze the command properties and extract native-compatible parameters
            
            // For now, return empty array - NativeCommandFactory should handle default construction
            return new object[0];
        }
    }
}

