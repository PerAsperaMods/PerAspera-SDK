using PerAspera.GameAPI.Commands.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Profiling.Memory.Experimental;

namespace PerAspera.GameAPI.Commands.Builders.Services
{
    /// <summary>
    /// Service for faction command execution with shared logic
    /// Handles synchronous and asynchronous execution patterns for faction command sequences
    /// </summary>
    public static class FactionExecutionService
    {
        /// <summary>
        /// Execute commands sequentially with stop-on-failure logic
        /// </summary>
        public static BatchCommandResult ExecuteCommands(IReadOnlyList<CommandBuilder> commands, bool stopOnFailure)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));

            var results = new List<CommandResult>();

            foreach (var command in commands)
            {
                try
                {
                    var result = command.Execute();
                    results.Add(result);

                    if (!result.Success && stopOnFailure)
                    {
                        // ? CORRECTION: Utilise seulement les results
                        return new BatchCommandResult(results);
                    }
                }
                catch (Exception ex)
                {
                    var errorResult = CommandResult.CreateFailure(command.BuildCommand(), ex.Message, 0);
                    results.Add(errorResult);

                    if (stopOnFailure)
                    {
                        // ? CORRECTION: Utilise seulement les results
                        return new BatchCommandResult(results);
                    }
                }
            }

            // ? CORRECTION: À la fin aussi
            return new  (results);
        }

        /// <summary>
        /// Execute commands sequentially asynchronously with stop-on-failure logic
        /// </summary>
        public static async Task<BatchCommandResult> ExecuteCommandsAsync(IReadOnlyList<CommandBuilder> commands, bool stopOnFailure)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            
            var results = new List<CommandResult>();
            
            foreach (var command in commands)
            {
                try
                {
                    var result = await command.ExecuteAsync();
                    results.Add(result);
                    
                    if (!result.Success && stopOnFailure)
                    {
                        return new BatchCommandResult(results);
                    }
                }
                catch (Exception ex)
                {
                    var errorResult = CommandResult.CreateFailure(command.BuildCommand(), ex.Message, 0);
                    results.Add(errorResult);
                    
                    if (stopOnFailure)
                    {
                        return new BatchCommandResult(results);
                    }
                }
            }
            
            var overallSuccess = results.TrueForAll(r => r.Success);
            return new BatchCommandResult(results);
        }
        
        /// <summary>
        /// Create custom command with parameters for faction
        /// </summary>
        public static CommandBuilder CreateCustomCommand(object faction, string commandType, TimeSpan? timeout, Action<ParameterBuilder> parameterConfig = null)
        {
            var command = new CommandBuilder(commandType)
                .WithFaction(faction);
                
            if (parameterConfig != null)
            {
                var paramBuilder = new ParameterBuilder();
                parameterConfig(paramBuilder);
                
                foreach (var param in paramBuilder.Build())
                {
                    command.WithParameter(param.Key, param.Value);
                }
            }
            
            if (timeout.HasValue)
                command.WithTimeout(timeout.Value);
                
            return command;
        }
        
        /// <summary>
        /// Clone command list for faction builder copying
        /// </summary>
        public static List<CommandBuilder> CloneCommandList(IReadOnlyList<CommandBuilder> sourceCommands)
        {
            // Note: For proper cloning, CommandBuilder would need a Clone method
            // For now, returning a new list with same references
            // This might need improvement based on CommandBuilder implementation
            return new List<CommandBuilder>(sourceCommands);
        }
        
        /// <summary>
        /// Validate faction object for command operations
        /// </summary>
        public static void ValidateFaction(object faction)
        {
            if (faction == null)
                throw new ArgumentNullException(nameof(faction), "Faction cannot be null");
        }
        
        /// <summary>
        /// Get command count safely
        /// </summary>
        public static int GetCommandCount(IReadOnlyList<CommandBuilder> commands)
        {
            return commands?.Count ?? 0;
        }
        
        /// <summary>
        /// Check if command list is empty
        /// </summary>
        public static bool IsEmpty(IReadOnlyList<CommandBuilder> commands)
        {
            return commands == null || commands.Count == 0;
        }
    }
}
