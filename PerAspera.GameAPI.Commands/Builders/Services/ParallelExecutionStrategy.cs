using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.Builders.Services
{
    /// <summary>
    /// Execution strategy for parallel command processing
    /// Handles parallel execution with degree control, timeout management, and error aggregation
    /// </summary>
    public class ParallelExecutionStrategy
    {
        private readonly bool _stopOnFailure;
        private readonly TimeSpan? _globalTimeout;
        private readonly int? _maxParallelism;

        public ParallelExecutionStrategy(bool stopOnFailure, TimeSpan? globalTimeout, int? maxParallelism)
        {
            _stopOnFailure = stopOnFailure;
            _globalTimeout = globalTimeout;
            _maxParallelism = maxParallelism;
        }

        /// <summary>
        /// Execute commands in parallel using Parallel.For
        /// </summary>
        public BatchCommandResult Execute(IReadOnlyList<CommandBuilder> commands, IReadOnlyList<Func<bool>> conditions)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (conditions == null) throw new ArgumentNullException(nameof(conditions));
            if (commands.Count != conditions.Count) throw new ArgumentException("Commands and conditions count mismatch");

            var results = new CommandResult[commands.Count];
            var parallelOptions = CreateParallelOptions();

            try
            {
                Parallel.For(0, commands.Count, parallelOptions, i =>
                {
                    results[i] = ExecuteCommandAtIndex(commands, conditions, i);
                });
            }
            catch (OperationCanceledException)
            {
                return new BatchCommandResult(
                    results.Where(r => r != null).ToList()
                );
            }

            return ProcessParallelResults(results.ToList());
        }

        /// <summary>
        /// Execute commands in parallel asynchronously using Task.Run
        /// </summary>
        public async Task<BatchCommandResult> ExecuteAsync(IReadOnlyList<CommandBuilder> commands, IReadOnlyList<Func<bool>> conditions)
        {
            if (commands == null) throw new ArgumentNullException(nameof(commands));
            if (conditions == null) throw new ArgumentNullException(nameof(conditions));
            if (commands.Count != conditions.Count) throw new ArgumentException("Commands and conditions count mismatch");

            var tasks = CreateExecutionTasks(commands, conditions);
            var results = await WaitForTasksWithTimeout(tasks);

            return ProcessParallelResults(results.ToList());
        }

        private ParallelOptions CreateParallelOptions()
        {
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = _maxParallelism ?? Environment.ProcessorCount
            };

            if (_globalTimeout.HasValue)
            {
                var cancellationTokenSource = new System.Threading.CancellationTokenSource(_globalTimeout.Value);
                parallelOptions.CancellationToken = cancellationTokenSource.Token;
            }

            return parallelOptions;
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

        private List<Task<(int index, CommandResult result)>> CreateExecutionTasks(
            IReadOnlyList<CommandBuilder> commands, 
            IReadOnlyList<Func<bool>> conditions)
        {
            var tasks = new List<Task<(int index, CommandResult result)>>();

            for (int i = 0; i < commands.Count; i++)
            {
                var index = i; // Capture loop variable

                var task = Task.Run(async () =>
                {
                    // Check condition
                    try
                    {
                        if (!conditions[index]())
                        {
                            return (index, new CommandResult(true, "Skipped due to condition", null));
                        }
                    }
                    catch (Exception conditionEx)
                    {
                        return (index, new CommandResult(false, $"Condition evaluation failed: {conditionEx.Message}", conditionEx));
                    }

                    // Execute command
                    try
                    {
                        var result = await commands[index].ExecuteAsync();
                        return (index, result);
                    }
                    catch (Exception ex)
                    {
                        return (index, new CommandResult(false, ex.Message, ex));
                    }
                });

                tasks.Add(task);
            }

            return tasks;
        }

        private async Task<CommandResult[]> WaitForTasksWithTimeout(List<Task<(int index, CommandResult result)>> tasks)
        {
            CommandResult[] results;

            if (_globalTimeout.HasValue)
            {
                try
                {
                    var completedTasks = await Task.WhenAll(tasks).WaitAsync(_globalTimeout.Value);
                    results = new CommandResult[tasks.Count];
                    foreach (var (index, result) in completedTasks)
                    {
                        results[index] = result;
                    }
                }
                catch (TimeoutException)
                {
                    // Handle partial results from timeout
                    results = new CommandResult[tasks.Count];
                    for (int i = 0; i < tasks.Count; i++)
                    {
                        if (tasks[i].IsCompleted && !tasks[i].IsFaulted)
                        {
                            var (index, result) = tasks[i].Result;
                            results[index] = result;
                        }
                        else
                        {
                            results[i] = new CommandResult(false, "Task timed out or faulted", null);
                        }
                    }
                }
            }
            else
            {
                var completedTasks = await Task.WhenAll(tasks);
                results = new CommandResult[tasks.Count];
                foreach (var (index, result) in completedTasks)
                {
                    results[index] = result;
                }
            }

            return results;
        }

        private BatchCommandResult ProcessParallelResults(List<CommandResult> results)
        {
            return new BatchCommandResult(results);
        }
    }
}
