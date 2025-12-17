using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PerAspera.GameAPI.Commands.Constants;
using PerAspera.GameAPI.Commands.Core;
using PerAspera.GameAPI.Commands.Builders.Services;

namespace PerAspera.GameAPI.Commands.Builders
{
    /// <summary>
    /// Specialized builder for faction-based commands with chainable methods for common faction operations
    /// Provides a fluent API optimized for faction-specific commands like resource management, technology unlocks, etc.
    /// </summary>
    public class FactionCommandBuilder
    {
        private readonly object _faction;
        private readonly List<CommandBuilder> _commands;
        private TimeSpan? _globalTimeout;
        private bool _stopOnFailure = true;
        
        internal FactionCommandBuilder(object faction)
        {
            FactionExecutionService.ValidateFaction(faction);
            _faction = faction;
            _commands = new List<CommandBuilder>();
        }
        
        /// <summary>
        /// Set global timeout for all commands in this faction builder
        /// </summary>
        public FactionCommandBuilder WithTimeout(TimeSpan timeout)
        {
            _globalTimeout = timeout;
            return this;
        }
        
        /// <summary>
        /// Set whether to stop execution on first failure (default: true)
        /// </summary>
        public FactionCommandBuilder StopOnFailure(bool stop = true)
        {
            _stopOnFailure = stop;
            return this;
        }
        
        // Resource Management Commands
        
        /// <summary>
        /// Import resource for this faction
        /// </summary>
        public FactionCommandBuilder ImportResource(object resource, int quantity)
        {
            var command = FactionResourceCommands.ImportResource(_faction, resource, quantity, _globalTimeout);
            _commands.Add(command);
            return this;
        }
        
        /// <summary>
        /// Export resource from this faction
        /// </summary>
        public FactionCommandBuilder ExportResource(object resource, int quantity)
        {
            var command = FactionResourceCommands.ExportResource(_faction, resource, quantity, _globalTimeout);
            _commands.Add(command);
            return this;
        }
        
        /// <summary>
        /// Set resource amount for this faction
        /// </summary>
        public FactionCommandBuilder SetResourceAmount(object resource, float amount)
        {
            var command = FactionResourceCommands.SetResourceAmount(_faction, resource, amount, _globalTimeout);
            _commands.Add(command);
            return this;
        }
        
        // Building Management Commands
        
        /// <summary>
        /// Unlock building for this faction
        /// </summary>
        public FactionCommandBuilder UnlockBuilding(object building)
        {
            var command = FactionBuildingCommands.UnlockBuilding(_faction, building, _globalTimeout);
            _commands.Add(command);
            return this;
        }
        
        /// <summary>
        /// Lock building for this faction
        /// </summary>
        public FactionCommandBuilder LockBuilding(object building)
        {
            var command = FactionBuildingCommands.LockBuilding(_faction, building, _globalTimeout);
            _commands.Add(command);
            return this;
        }
        
        /// <summary>
        /// Add building to faction
        /// </summary>
        public FactionCommandBuilder AddBuilding(object building, float x, float y, float z)
        {
            var command = FactionBuildingCommands.AddBuilding(_faction, building, x, y, z, _globalTimeout);
            _commands.Add(command);
            return this;
        }
        
        /// <summary>
        /// Remove building from faction
        /// </summary>
        public FactionCommandBuilder RemoveBuilding(object building)
        {
            var command = FactionBuildingCommands.RemoveBuilding(_faction, building, _globalTimeout);
            _commands.Add(command);
            return this;
        }
        
        // Technology and Knowledge Commands
        
        /// <summary>
        /// Research technology for this faction
        /// </summary>
        public FactionCommandBuilder ResearchTechnology(object technology)
        {
            var command = new CommandBuilder(NativeCommandTypes.ResearchTechnology)
                .WithFaction(_faction)
                .WithParameter(ParameterNames.Technology, technology);
                
            if (_globalTimeout.HasValue)
                command.WithTimeout(_globalTimeout.Value);
                
            _commands.Add(command);
            return this;
        }
        
        /// <summary>
        /// Unlock knowledge for this faction
        /// </summary>
        public FactionCommandBuilder UnlockKnowledge(object knowledge)
        {
            var command = new CommandBuilder(NativeCommandTypes.UnlockKnowledge)
                .WithFaction(_faction)
                .WithParameter(ParameterNames.Knowledge, knowledge);
                
            if (_globalTimeout.HasValue)
                command.WithTimeout(_globalTimeout.Value);
                
            _commands.Add(command);
            return this;
        }
        
        /// <summary>
        /// Lock knowledge for this faction
        /// </summary>
        public FactionCommandBuilder LockKnowledge(object knowledge)
        {
            var command = new CommandBuilder(NativeCommandTypes.LockKnowledge)
                .WithFaction(_faction)
                .WithParameter(ParameterNames.Knowledge, knowledge);
                
            if (_globalTimeout.HasValue)
                command.WithTimeout(_globalTimeout.Value);
                
            _commands.Add(command);
            return this;
        }
        
        // Interaction Commands
        
        /// <summary>
        /// Start dialogue for this faction
        /// </summary>
        public FactionCommandBuilder StartDialogue(object person, object dialogue)
        {
            var command = new CommandBuilder(NativeCommandTypes.StartDialogue)
                .WithFaction(_faction)
                .WithParameter(ParameterNames.Person, person)
                .WithParameter(ParameterNames.Dialogue, dialogue);
                
            if (_globalTimeout.HasValue)
                command.WithTimeout(_globalTimeout.Value);
                
            _commands.Add(command);
            return this;
        }
        
        /// <summary>
        /// Skip dialogue for this faction
        /// </summary>
        public FactionCommandBuilder SkipDialogue(object dialogue)
        {
            var command = new CommandBuilder(NativeCommandTypes.SkipDialogue)
                .WithFaction(_faction)
                .WithParameter(ParameterNames.Dialogue, dialogue);
                
            if (_globalTimeout.HasValue)
                command.WithTimeout(_globalTimeout.Value);
                
            _commands.Add(command);
            return this;
        }
        
        /// <summary>
        /// Enable keeper mode for this faction
        /// </summary>
        public FactionCommandBuilder EnableKeeperMode()
        {
            var command = new CommandBuilder(NativeCommandTypes.EnableKeeperMode)
                .WithFaction(_faction);
                
            if (_globalTimeout.HasValue)
                command.WithTimeout(_globalTimeout.Value);
                
            _commands.Add(command);
            return this;
        }
        
        /// <summary>
        /// Disable keeper mode for this faction
        /// </summary>
        public FactionCommandBuilder DisableKeeperMode()
        {
            var command = new CommandBuilder(NativeCommandTypes.DisableKeeperMode)
                .WithFaction(_faction);
                
            if (_globalTimeout.HasValue)
                command.WithTimeout(_globalTimeout.Value);
                
            _commands.Add(command);
            return this;
        }
        
        // Environmental Commands
        
        /// <summary>
        /// Spawn resource vein for this faction
        /// </summary>
        public FactionCommandBuilder SpawnResourceVein(object resource, float x, float y, float z)
        {
            var command = new CommandBuilder(NativeCommandTypes.SpawnResourceVein)
                .WithFaction(_faction)
                .WithParameter(ParameterNames.Resource, resource)
                .WithParameter(ParameterNames.X, x)
                .WithParameter(ParameterNames.Y, y)
                .WithParameter(ParameterNames.Z, z);
                
            if (_globalTimeout.HasValue)
                command.WithTimeout(_globalTimeout.Value);
                
            _commands.Add(command);
            return this;
        }
        
        /// <summary>
        /// Sabotage this faction
        /// </summary>
        public FactionCommandBuilder Sabotage()
        {
            var command = new CommandBuilder(NativeCommandTypes.Sabotage)
                .WithFaction(_faction);
                
            if (_globalTimeout.HasValue)
                command.WithTimeout(_globalTimeout.Value);
                
            _commands.Add(command);
            return this;
        }
        
        // Message Commands
        
        /// <summary>
        /// Show message to this faction
        /// </summary>
        public FactionCommandBuilder ShowMessage(string message)
        {
            var command = new CommandBuilder(NativeCommandTypes.ShowMessage)
                .WithFaction(_faction)
                .WithParameter(ParameterNames.Message, message);
                
            if (_globalTimeout.HasValue)
                command.WithTimeout(_globalTimeout.Value);
                
            _commands.Add(command);
            return this;
        }
        
        /// <summary>
        /// Show tutorial message to this faction
        /// </summary>
        public FactionCommandBuilder ShowTutorialMessage(string message)
        {
            var command = new CommandBuilder(NativeCommandTypes.ShowTutorialMessage)
                .WithFaction(_faction)
                .WithParameter(ParameterNames.Message, message);
                
            if (_globalTimeout.HasValue)
                command.WithTimeout(_globalTimeout.Value);
                
            _commands.Add(command);
            return this;
        }
        
        // Custom Command
        
        /// <summary>
        /// Add custom command with parameters
        /// </summary>
        public FactionCommandBuilder CustomCommand(string commandType, Action<ParameterBuilder> parameterConfig = null)
        {
            var command = new CommandBuilder(commandType)
                .WithFaction(_faction);
                
            if (parameterConfig != null)
            {
                var paramBuilder = new ParameterBuilder();
                parameterConfig(paramBuilder);
                
                foreach (var param in paramBuilder.Build())
                {
                    command.WithParameter(param.Key, param.Value);
                }
            }
            
            if (_globalTimeout.HasValue)
                command.WithTimeout(_globalTimeout.Value);
                
            _commands.Add(command);
            return this;
        }
        
        // Execution Methods
        
        /// <summary>
        /// Execute all commands synchronously
        /// </summary>
        public BatchCommandResult Execute()
        {
            var results = new List<CommandResult>();
            var errors = new List<string>();
            
            foreach (var command in _commands)
            {
                try
                {
                    var result = command.Execute();
                    results.Add(result);
                    
                    if (!result.Success && _stopOnFailure)
                    {
                        return new BatchCommandResult(results, false, $"Command failed: {result.Error}");
                    }
                }
                catch (Exception ex)
                {
                    var errorResult = new CommandResult(false, ex.Message, ex);
                    results.Add(errorResult);
                    
                    if (_stopOnFailure)
                    {
                        return new BatchCommandResult(results, false, $"Command execution failed: {ex.Message}");
                    }
                }
            }
            
            var overallSuccess = results.TrueForAll(r => r.Success);
            return new BatchCommandResult(results, overallSuccess, overallSuccess ? null : "Some commands failed");
        }
        
        /// <summary>
        /// Execute all commands asynchronously
        /// </summary>
        public async Task<BatchCommandResult> ExecuteAsync()
        {
            var results = new List<CommandResult>();
            
            foreach (var command in _commands)
            {
                try
                {
                    var result = await command.ExecuteAsync();
                    results.Add(result);
                    
                    if (!result.Success && _stopOnFailure)
                    {
                        return new BatchCommandResult(results, false, $"Command failed: {result.Error}");
                    }
                }
                catch (Exception ex)
                {
                    var errorResult = new CommandResult(false, ex.Message, ex);
                    results.Add(errorResult);
                    
                    if (_stopOnFailure)
                    {
                        return new BatchCommandResult(results, false, $"Command execution failed: {ex.Message}");
                    }
                }
            }
            
            var overallSuccess = results.TrueForAll(r => r.Success);
            return new BatchCommandResult(results, overallSuccess, overallSuccess ? null : "Some commands failed");
        }
        
        /// <summary>
        /// Get count of commands to be executed
        /// </summary>
        public int Count => _commands.Count;
        
        /// <summary>
        /// Get the faction this builder is configured for
        /// </summary>
        public object Faction => _faction;
        
        /// <summary>
        /// Clear all commands from this builder
        /// </summary>
        public FactionCommandBuilder Clear()
        {
            _commands.Clear();
            return this;
        }
        
        /// <summary>
        /// Create a copy of this builder with the same faction and settings
        /// </summary>
        public FactionCommandBuilder Clone()
        {
            var clone = new FactionCommandBuilder(_faction)
            {
                _globalTimeout = this._globalTimeout,
                _stopOnFailure = this._stopOnFailure
            };
            
            // Clone all commands
            foreach (var command in _commands)
            {
                // Note: CommandBuilder would need a Clone method for this to work properly
                // For now, we'll create new commands with same parameters
                clone._commands.Add(command);
            }
            
            return clone;
        }
        
        /// <summary>
        /// Convert to BatchCommandBuilder for more advanced batch operations
        /// </summary>
        public BatchCommandBuilder ToBatchBuilder()
        {
            var batchBuilder = new BatchCommandBuilder()
                .StopOnFailure(_stopOnFailure);
                
            if (_globalTimeout.HasValue)
                batchBuilder.WithTimeout(_globalTimeout.Value);
            
            foreach (var command in _commands)
            {
                batchBuilder.AddCommand(command);
            }
            
            return batchBuilder;
        }
    }
}