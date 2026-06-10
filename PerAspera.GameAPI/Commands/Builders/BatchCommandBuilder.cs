using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PerAspera.GameAPI.Commands.Core;
using PerAspera.GameAPI.Commands.Builders.Services;

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
            BatchCommandUtilities.AddCommand(_commands, _conditions, command);
            return this;
        }
        
        /// <summary>
        /// Add a command with a condition
        /// </summary>
        public BatchCommandBuilder AddCommand(CommandBuilder command, Func<bool> condition)
        {
            BatchCommandUtilities.AddCommand(_commands, _conditions, command, condition);
            return this;
        }
        
        /// <summary>
        /// Add multiple commands to the batch
        /// </summary>
        public BatchCommandBuilder AddCommands(params CommandBuilder[] commands)
        {
            BatchCommandUtilities.AddCommands(_commands, _conditions, commands);
            return this;
        }
        
        /// <summary>
        /// Add multiple commands with conditions
        /// </summary>
        public BatchCommandBuilder AddCommands(IEnumerable<(CommandBuilder command, Func<bool> condition)> commandsWithConditions)
        {
            BatchCommandUtilities.AddCommandsWithConditions(_commands, _conditions, commandsWithConditions);
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
                
            BatchCommandUtilities.AddConditionalBlock(_commands, _conditions, condition, (commands, conditions) =>
            {
                var subBuilder = new BatchCommandBuilder();
                blockBuilder(subBuilder);
                
                // Transfer commands from sub-builder
                commands.AddRange(subBuilder._commands);
                conditions.AddRange(subBuilder._conditions);
            });
            
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
                return new BatchCommandResult(new List<CommandResult>());
                
            if (_executeInParallel)
            {
                var parallelStrategy = new ParallelExecutionStrategy(_stopOnFailure, _globalTimeout, _maxParallelism);
                return parallelStrategy.Execute(_commands, _conditions);
            }
            else
            {
                var sequentialStrategy = new SequentialExecutionStrategy(_stopOnFailure, _globalTimeout);
                return sequentialStrategy.Execute(_commands, _conditions);
            }
        }
        
        /// <summary>
        /// Execute all commands asynchronously
        /// </summary>
        public async Task<BatchCommandResult> ExecuteAsync()
        {
            if (_commands.Count == 0)
                return new BatchCommandResult(new List<CommandResult>());
                
            if (_executeInParallel)
            {
                var parallelStrategy = new ParallelExecutionStrategy(_stopOnFailure, _globalTimeout, _maxParallelism);
                return await parallelStrategy.ExecuteAsync(_commands, _conditions);
            }
            else
            {
                var sequentialStrategy = new SequentialExecutionStrategy(_stopOnFailure, _globalTimeout);
                return await sequentialStrategy.ExecuteAsync(_commands, _conditions);
            }
        }
        
        // Removed: Sequential execution logic moved to SequentialExecutionStrategy
        
        // Removed: Async sequential execution logic moved to SequentialExecutionStrategy
        
        // Removed: Parallel execution logic moved to ParallelExecutionStrategy
        
        // Removed: Async parallel execution logic moved to ParallelExecutionStrategy
        
        /// <summary>
        /// Get count of commands in the batch
        /// </summary>
        public int Count => _commands.Count;
        
        /// <summary>
        /// Clear all commands from the batch
        /// </summary>
        public BatchCommandBuilder Clear()
        {
            BatchCommandUtilities.Clear(_commands, _conditions);
            return this;
        }
        
        /// <summary>
        /// Remove command at specified index
        /// </summary>
        public BatchCommandBuilder RemoveAt(int index)
        {
            BatchCommandUtilities.RemoveAt(_commands, _conditions, index);
            return this;
        }
        
        /// <summary>
        /// Insert command at specified index
        /// </summary>
        public BatchCommandBuilder InsertAt(int index, CommandBuilder command, Func<bool> condition = null)
        {
            BatchCommandUtilities.InsertAt(_commands, _conditions, index, command, condition);
            return this;
        }
        
        /// <summary>
        /// Create a copy of this builder
        /// </summary>
        public BatchCommandBuilder Clone()
        {
            var (clonedCommands, clonedConditions) = BatchCommandUtilities.CloneCommandLists(_commands, _conditions);
            
            var clone = new BatchCommandBuilder
            {
                _stopOnFailure = this._stopOnFailure,
                _executeInParallel = this._executeInParallel,
                _globalTimeout = this._globalTimeout,
                _maxParallelism = this._maxParallelism
            };
            
            clone._commands.AddRange(clonedCommands);
            clone._conditions.AddRange(clonedConditions);
            
            return clone;
        }
    }
}
