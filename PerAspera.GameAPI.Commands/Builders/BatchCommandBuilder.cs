using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.Builders
{
    /// <summary>
    /// Builder for executing multiple commands in batch with various execution strategies
    /// Supports sequential, parallel, conditional execution with failure handling
    /// </summary>
    public class BatchCommandBuilder
    {
        private readonly List<CommandBuilder> _commands;
        private readonly List<Func<bool>> _conditions;
        private bool _stopOnFailure = true;
        private bool _executeInParallel = false;
        private TimeSpan? _globalTimeout;
        private int? _maxParallelism;
        
        public BatchCommandBuilder()
        {
            _commands = new List<CommandBuilder>();
            _conditions = new List<Func<bool>>();
        }
        
        /// <summary>
        /// Add a command to the batch
        /// </summary>
        public BatchCommandBuilder AddCommand(CommandBuilder command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
                
            _commands.Add(command);
            _conditions.Add(() => true); // Default: always execute
            return this;
        }
        
        /// <summary>
        /// Add a command with a condition
        /// </summary>
        public BatchCommandBuilder AddCommand(CommandBuilder command, Func<bool> condition)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
                
            _commands.Add(command);
            _conditions.Add(condition);
            return this;
        }
        
        /// <summary>
        /// Add multiple commands to the batch
        /// </summary>
        public BatchCommandBuilder AddCommands(params CommandBuilder[] commands)
        {
            if (commands == null)
                throw new ArgumentNullException(nameof(commands));
                
            foreach (var command in commands)
            {
                AddCommand(command);
            }
            
            return this;
        }
        
        /// <summary>
        /// Add multiple commands with conditions
        /// </summary>
        public BatchCommandBuilder AddCommands(IEnumerable<(CommandBuilder command, Func<bool> condition)> commandsWithConditions)
        {
            if (commandsWithConditions == null)
                throw new ArgumentNullException(nameof(commandsWithConditions));
                
            foreach (var (command, condition) in commandsWithConditions)
            {
                AddCommand(command, condition);
            }
            
            return this;
        }
        
        /// <summary>
        /// Set whether to stop execution on first failure (default: true)
        /// </summary>
        public BatchCommandBuilder StopOnFailure(bool stop = true)
        {
            _stopOnFailure = stop;
            return this;
        }
        
        /// <summary>
        /// Set whether to execute commands in parallel (default: false)
        /// </summary>
        public BatchCommandBuilder ExecuteInParallel(bool parallel = true)
        {
            _executeInParallel = parallel;
            return this;
        }
        
        /// <summary>
        /// Set maximum number of parallel operations (only used when ExecuteInParallel is true)
        /// </summary>
        public BatchCommandBuilder WithMaxParallelism(int maxParallelism)
        {
            if (maxParallelism <= 0)
                throw new ArgumentException("Max parallelism must be positive", nameof(maxParallelism));
                
            _maxParallelism = maxParallelism;
            return this;
        }
        
        /// <summary>
        /// Set global timeout for the entire batch operation
        /// </summary>
        public BatchCommandBuilder WithTimeout(TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero)
                throw new ArgumentException("Timeout must be positive", nameof(timeout));
                
            _globalTimeout = timeout;
            return this;
        }
        
        /// <summary>
        /// Add a conditional block of commands
        /// </summary>
        public BatchCommandBuilder AddConditionalBlock(Func<bool> condition, Action<BatchCommandBuilder> blockBuilder)
        {
            if (condition == null)
                throw new ArgumentNullException(nameof(condition));
            if (blockBuilder == null)
                throw new ArgumentNullException(nameof(blockBuilder));
                
            var subBuilder = new BatchCommandBuilder();
            blockBuilder(subBuilder);
            
            foreach (var command in subBuilder._commands.Zip(subBuilder._conditions, (cmd, cond) => new { Command = cmd, Condition = cond }))
            {
                // Combine the block condition with individual command conditions
                AddCommand(command.Command, () => condition() && command.Condition());
            }
            
            return this;
        }
        
        /// <summary>
        /// Add commands from a FactionCommandBuilder
        /// </summary>
        public BatchCommandBuilder AddFromFactionBuilder(FactionCommandBuilder factionBuilder)
        {
            if (factionBuilder == null)
                throw new ArgumentNullException(nameof(factionBuilder));
                
            // Convert faction builder to batch builder and merge commands
            var batchBuilder = factionBuilder.ToBatchBuilder();
            foreach (var command in batchBuilder._commands)
            {
                AddCommand(command);
            }
            
            return this;
        }
        
        /// <summary>
        /// Execute all commands according to the configured strategy
        /// </summary>
        public BatchCommandResult Execute()
        {
            if (_commands.Count == 0)
                return new BatchCommandResult(new List<CommandResult>(), true, null);
                
            if (_executeInParallel)
                return ExecuteInParallelInternal();
            else
                return ExecuteSequentialInternal();
        }
        
        /// <summary>
        /// Execute all commands asynchronously
        /// </summary>
        public async Task<BatchCommandResult> ExecuteAsync()
        {
            if (_commands.Count == 0)
                return new BatchCommandResult(new List<CommandResult>(), true, null);
                
            if (_executeInParallel)
                return await ExecuteInParallelInternalAsync();
            else
                return await ExecuteSequentialInternalAsync();
        }
        
        private BatchCommandResult ExecuteSequentialInternal()
        {
            var results = new List<CommandResult>();
            var startTime = DateTime.UtcNow;
            
            for (int i = 0; i < _commands.Count; i++)
            {
                // Check global timeout
                if (_globalTimeout.HasValue && DateTime.UtcNow - startTime > _globalTimeout.Value)
                {
                    return new BatchCommandResult(results, false, "Batch execution timed out");
                }
                
                // Check condition
                try
                {
                    if (!_conditions[i]())
                    {
                        // Condition not met, skip this command
                        results.Add(new CommandResult(true, "Skipped due to condition", null));
                        continue;
                    }
                }
                catch (Exception conditionEx)
                {
                    var conditionErrorResult = new CommandResult(false, $"Condition evaluation failed: {conditionEx.Message}", conditionEx);
                    results.Add(conditionErrorResult);
                    
                    if (_stopOnFailure)
                    {
                        return new BatchCommandResult(results, false, $"Condition evaluation failed: {conditionEx.Message}");
                    }
                    continue;
                }
                
                // Execute command
                try
                {
                    var result = _commands[i].Execute();
                    results.Add(result);
                    
                    if (!result.Success && _stopOnFailure)
                    {
                        return new BatchCommandResult(results, false, $"Command {i + 1} failed: {result.Error}");
                    }
                }
                catch (Exception ex)
                {
                    var errorResult = new CommandResult(false, ex.Message, ex);
                    results.Add(errorResult);
                    
                    if (_stopOnFailure)
                    {
                        return new BatchCommandResult(results, false, $"Command {i + 1} execution failed: {ex.Message}");
                    }
                }
            }
            
            var overallSuccess = results.TrueForAll(r => r.Success);
            return new BatchCommandResult(results, overallSuccess, overallSuccess ? null : "Some commands failed");
        }
        
        private async Task<BatchCommandResult> ExecuteSequentialInternalAsync()
        {
            var results = new List<CommandResult>();
            var startTime = DateTime.UtcNow;
            
            for (int i = 0; i < _commands.Count; i++)
            {
                // Check global timeout
                if (_globalTimeout.HasValue && DateTime.UtcNow - startTime > _globalTimeout.Value)
                {
                    return new BatchCommandResult(results, false, "Batch execution timed out");
                }
                
                // Check condition
                try
                {
                    if (!_conditions[i]())
                    {
                        results.Add(new CommandResult(true, "Skipped due to condition", null));
                        continue;
                    }
                }
                catch (Exception conditionEx)
                {
                    var conditionErrorResult = new CommandResult(false, $"Condition evaluation failed: {conditionEx.Message}", conditionEx);
                    results.Add(conditionErrorResult);
                    
                    if (_stopOnFailure)
                    {
                        return new BatchCommandResult(results, false, $"Condition evaluation failed: {conditionEx.Message}");
                    }
                    continue;
                }
                
                // Execute command
                try
                {
                    var result = await _commands[i].ExecuteAsync();
                    results.Add(result);
                    
                    if (!result.Success && _stopOnFailure)
                    {
                        return new BatchCommandResult(results, false, $"Command {i + 1} failed: {result.Error}");
                    }
                }
                catch (Exception ex)
                {
                    var errorResult = new CommandResult(false, ex.Message, ex);
                    results.Add(errorResult);
                    
                    if (_stopOnFailure)
                    {
                        return new BatchCommandResult(results, false, $"Command {i + 1} execution failed: {ex.Message}");
                    }
                }
            }
            
            var overallSuccess = results.TrueForAll(r => r.Success);
            return new BatchCommandResult(results, overallSuccess, overallSuccess ? null : "Some commands failed");
        }
        
        private BatchCommandResult ExecuteInParallelInternal()
        {
            var results = new CommandResult[_commands.Count];
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = _maxParallelism ?? Environment.ProcessorCount
            };
            
            if (_globalTimeout.HasValue)
            {
                parallelOptions.CancellationToken = new System.Threading.CancellationTokenSource(_globalTimeout.Value).Token;
            }
            
            try
            {
                Parallel.For(0, _commands.Count, parallelOptions, i =>
                {
                    // Check condition
                    try
                    {
                        if (!_conditions[i]())
                        {
                            results[i] = new CommandResult(true, "Skipped due to condition", null);
                            return;
                        }
                    }
                    catch (Exception conditionEx)
                    {
                        results[i] = new CommandResult(false, $"Condition evaluation failed: {conditionEx.Message}", conditionEx);
                        return;
                    }
                    
                    // Execute command
                    try
                    {
                        results[i] = _commands[i].Execute();
                    }
                    catch (Exception ex)
                    {
                        results[i] = new CommandResult(false, ex.Message, ex);
                    }
                });
            }
            catch (OperationCanceledException)
            {
                return new BatchCommandResult(results.Where(r => r != null).ToList(), false, "Batch execution timed out");
            }
            
            var resultList = results.ToList();
            var overallSuccess = resultList.TrueForAll(r => r.Success);
            
            if (_stopOnFailure && !overallSuccess)
            {
                var firstFailure = resultList.FirstOrDefault(r => !r.Success);
                return new BatchCommandResult(resultList, false, $"Parallel execution failed: {firstFailure?.Error}");
            }
            
            return new BatchCommandResult(resultList, overallSuccess, overallSuccess ? null : "Some commands failed");
        }
        
        private async Task<BatchCommandResult> ExecuteInParallelInternalAsync()
        {
            var tasks = new List<Task<(int index, CommandResult result)>>();
            
            for (int i = 0; i < _commands.Count; i++)
            {
                var index = i; // Capture loop variable
                
                var task = Task.Run(async () =>
                {
                    // Check condition
                    try
                    {
                        if (!_conditions[index]())
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
                        var result = await _commands[index].ExecuteAsync();
                        return (index, result);
                    }
                    catch (Exception ex)
                    {
                        return (index, new CommandResult(false, ex.Message, ex));
                    }
                });
                
                tasks.Add(task);
            }
            
            CommandResult[] results;
            
            if (_globalTimeout.HasValue)
            {
                var completedTasks = await Task.WhenAll(tasks).WaitAsync(_globalTimeout.Value);
                results = new CommandResult[_commands.Count];
                foreach (var (index, result) in completedTasks)
                {
                    results[index] = result;
                }
            }
            else
            {
                var completedTasks = await Task.WhenAll(tasks);
                results = new CommandResult[_commands.Count];
                foreach (var (index, result) in completedTasks)
                {
                    results[index] = result;
                }
            }
            
            var resultList = results.ToList();
            var overallSuccess = resultList.TrueForAll(r => r.Success);
            
            if (_stopOnFailure && !overallSuccess)
            {
                var firstFailure = resultList.FirstOrDefault(r => !r.Success);
                return new BatchCommandResult(resultList, false, $"Parallel execution failed: {firstFailure?.Error}");
            }
            
            return new BatchCommandResult(resultList, overallSuccess, overallSuccess ? null : "Some commands failed");
        }
        
        /// <summary>
        /// Get count of commands in the batch
        /// </summary>
        public int Count => _commands.Count;
        
        /// <summary>
        /// Clear all commands from the batch
        /// </summary>
        public BatchCommandBuilder Clear()
        {
            _commands.Clear();
            _conditions.Clear();
            return this;
        }
        
        /// <summary>
        /// Remove command at specified index
        /// </summary>
        public BatchCommandBuilder RemoveAt(int index)
        {
            if (index < 0 || index >= _commands.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
                
            _commands.RemoveAt(index);
            _conditions.RemoveAt(index);
            return this;
        }
        
        /// <summary>
        /// Insert command at specified index
        /// </summary>
        public BatchCommandBuilder InsertAt(int index, CommandBuilder command, Func<bool> condition = null)
        {
            if (index < 0 || index > _commands.Count)
                throw new ArgumentOutOfRangeException(nameof(index));
            if (command == null)
                throw new ArgumentNullException(nameof(command));
                
            _commands.Insert(index, command);
            _conditions.Insert(index, condition ?? (() => true));
            return this;
        }
        
        /// <summary>
        /// Create a copy of this builder
        /// </summary>
        public BatchCommandBuilder Clone()
        {
            var clone = new BatchCommandBuilder()
            {
                _stopOnFailure = this._stopOnFailure,
                _executeInParallel = this._executeInParallel,
                _globalTimeout = this._globalTimeout,
                _maxParallelism = this._maxParallelism
            };
            
            for (int i = 0; i < _commands.Count; i++)
            {
                clone.AddCommand(_commands[i], _conditions[i]);
            }
            
            return clone;
        }
    }
}