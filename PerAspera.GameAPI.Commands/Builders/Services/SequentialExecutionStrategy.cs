using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.Builders.Services
{
    /// <summary>
    /// Execution strategy for sequential command processing
    /// Handles command conditions, error management, and timeout control
    /// </summary>
    public class SequentialExecutionStrategy
    {
        private readonly bool _stopOnFailure;
        private readonly TimeSpan? _globalTimeout;

        public SequentialExecutionStrategy(bool stopOnFailure, TimeSpan? globalTimeout)
        {
            _stopOnFailure = stopOnFailure;
            _globalTimeout = globalTimeout;
        }

        /// <summary>
        /// Execute commands sequentially with condition checking
        /// </summary>
        public BatchCommandResult Execute(IReadOnlyList<CommandBuilder> commands, IReadOnlyList<Func<bool>> conditions)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (conditions == null) throw new ArgumentNullException(nameof(conditions));
            if (commands.Count != conditions.Count) throw new ArgumentException("Commands and conditions count mismatch");

            var results = new List<CommandResult>();
            var startTime = DateTime.UtcNow;

            for (int i = 0; i < commands.Count; i++)
            {
                // Check global timeout
                if (IsTimedOut(startTime))
                {
                    return new BatchCommandResult(results, false, "Batch execution timed out");
                }

                // Execute command with condition check
                var result = ExecuteCommandAtIndex(commands, conditions, i);
                results.Add(result);

                // Handle failure based on strategy
                if (!result.Success && _stopOnFailure)
                {
                    return new BatchCommandResult(results, false, $"Command {i + 1} failed: {result.Error}");
                }
            }

            return CreateFinalResult(results);
        }

        /// <summary>
        /// Execute commands sequentially asynchronously
        /// </summary>
        public async Task<BatchCommandResult> ExecuteAsync(IReadOnlyList<CommandBuilder> commands, IReadOnlyList<Func<bool>> conditions)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (conditions == null) throw new ArgumentNullException(nameof(conditions));
            if (commands.Count != conditions.Count) throw new ArgumentException("Commands and conditions count mismatch");

            var results = new List<CommandResult>();
            var startTime = DateTime.UtcNow;

            for (int i = 0; i < commands.Count; i++)
            {
                // Check global timeout
                if (IsTimedOut(startTime))
                {
                    return new BatchCommandResult(results, false, "Batch execution timed out");
                }

                // Execute command asynchronously with condition check
                var result = await ExecuteCommandAtIndexAsync(commands, conditions, i);
                results.Add(result);

                // Handle failure based on strategy
                if (!result.Success && _stopOnFailure)
                {
                    return new BatchCommandResult(results, false, $"Command {i + 1} failed: {result.Error}");
                }
            }

            return CreateFinalResult(results);
        }

        private CommandResult ExecuteCommandAtIndex(IReadOnlyList<CommandBuilder> commands, IReadOnlyList<Func<bool>> conditions, int index)
        {
            // Check condition
            try
            {
                if (!conditions[index]())
                {
                    return new CommandResult(true, "Skipped due to condition", null);
                }
            }
            catch (Exception conditionEx)
            {
                return new CommandResult(false, $"Condition evaluation failed: {conditionEx.Message}", conditionEx);
            }

            // Execute command
            try
            {
                return commands[index].Execute();
            }
            catch (Exception ex)
            {
                return new CommandResult(false, ex.Message, ex);
            }
        }

        private async Task<CommandResult> ExecuteCommandAtIndexAsync(IReadOnlyList<CommandBuilder> commands, IReadOnlyList<Func<bool>> conditions, int index)
        {
            // Check condition
            try
            {
                if (!conditions[index]())
                {
                    return new CommandResult(true, "Skipped due to condition", null);
                }
            }
            catch (Exception conditionEx)
            {
                return new CommandResult(false, $"Condition evaluation failed: {conditionEx.Message}", conditionEx);
            }

            // Execute command
            try
            {
                return await commands[index].ExecuteAsync();
            }
            catch (Exception ex)
            {
                return CommandResult(errorCommand, ex.Message, 0);
            }
        }

        private bool IsTimedOut(DateTime startTime)
        {
            return _globalTimeout.HasValue && DateTime.UtcNow - startTime > _globalTimeout.Value;
        }

        private static BatchCommandResult CreateFinalResult(List<CommandResult> results)
        {
            return new BatchCommandResult(results);
        }
    }
}