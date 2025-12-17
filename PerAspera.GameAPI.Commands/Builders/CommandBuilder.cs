using System;
using System.Collections.Generic;
using BepInEx.Logging;
using PerAspera.Core;
using PerAspera.GameAPI.Commands.Constants;
using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.Builders
{
    /// <summary>
    /// Main fluent builder for creating and executing commands with method chaining
    /// Provides type-safe parameter validation and execution through CommandDispatcher
    /// </summary>
    public class CommandBuilder
    {
        private readonly string _commandType;
        private readonly Dictionary<string, object> _parameters;
        private readonly List<Func<IGameCommand, bool>> _validators;
        private TimeSpan? _timeout;
        private bool _validateBeforeExecution = true;
        
        /// <summary>
        /// Initialize builder with command type
        /// </summary>
        internal CommandBuilder(string commandType)
        {
            _commandType = commandType ?? throw new ArgumentNullException(nameof(commandType));
            _parameters = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _validators = new List<Func<IGameCommand, bool>>();
        }
        
        /// <summary>
        /// Set faction for this command
        /// </summary>
        public CommandBuilder WithFaction(object faction)
        {
            if (faction == null)
                throw new ArgumentNullException(nameof(faction));
                
            _parameters[ParameterNames.Faction] = faction;
            return this;
        }
        
        /// <summary>
        /// Add parameter with type-safe validation
        /// </summary>
        public CommandBuilder WithParameter<T>(string name, T value)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Parameter name cannot be null or empty", nameof(name));
                
            _parameters[name] = value;
            return this;
        }
        
        /// <summary>
        /// Add parameter without generic constraint
        /// </summary>
        public CommandBuilder WithParameter(string name, object value)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Parameter name cannot be null or empty", nameof(name));
                
            _parameters[name] = value; // Logging disabledreturn this;
        }
        
        /// <summary>
        /// Add multiple parameters from dictionary
        /// </summary>
        public CommandBuilder WithParameters(IDictionary<string, object> parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));
                
            foreach (var kvp in parameters)
            {
                _parameters[kvp.Key] = kvp.Value;
            } // Logging disabledreturn this;
        }
        
        /// <summary>
        /// Add custom validation rule for the command
        /// </summary>
        public CommandBuilder WithValidation(Func<IGameCommand, bool> validator)
        {
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));
                
            _validators.Add(validator); // Logging disabledreturn this;
        }
        
        /// <summary>
        /// Set execution timeout for this command
        /// </summary>
        public CommandBuilder WithTimeout(TimeSpan timeout)
        {
            if (timeout <= TimeSpan.Zero)
                throw new ArgumentException("Timeout must be positive", nameof(timeout));
                
            _timeout = timeout; // Logging disabledreturn this;
        }
        
        /// <summary>
        /// Skip validation before execution (use with caution)
        /// </summary>
        public CommandBuilder SkipValidation()
        {
            _validateBeforeExecution = false; // Logging disabledreturn this;
        }
        
        /// <summary>
        /// Validate parameters without executing (for testing)
        /// </summary>
        public CommandBuilder ValidateParameters()
        {
            var command = BuildCommand();
            
            // Run basic validation
            if (!command.IsValid())
            {
                throw new InvalidOperationException($"Command validation failed for {_commandType}");
            }
            
            // Run custom validators
            foreach (var validator in _validators)
            {
                if (!validator(command))
                {
                    throw new InvalidOperationException($"Custom validation failed for {_commandType}");
                }
            } // Logging disabledreturn this;
        }
        
        /// <summary>
        /// Execute the command and return result
        /// </summary>
        public CommandResult Execute()
        {
            try
            { // Logging disabled// Build command
                var command = BuildCommand();
                
                // Validate if enabled
                if (_validateBeforeExecution)
                {
                    ValidateCommand(command);
                }
                
                // Execute via dispatcher
                var dispatcher = CommandDispatcher.Instance;
                var result = dispatcher.Dispatch(command);
                
                LoggingSystem.Info($"CommandBuilder: {_commandType} execution {(result.Success ? "succeeded" : "failed")}");
                return result;
            }
            catch (Exception ex)
            { // Logging disabledvar errorCommand = BuildCommand();
                return CommandResult.CreateFailure(errorCommand, ex.Message, 0);
            }
        }
        
        /// <summary>
        /// Execute the command asynchronously
        /// </summary>
        public async System.Threading.Tasks.Task<CommandResult> ExecuteAsync()
        {
            return await System.Threading.Tasks.Task.Run(() => Execute());
        }
        
        /// <summary>
        /// Build the command without executing (for advanced scenarios)
        /// </summary>
        public IGameCommand BuildCommand()
        {
            return new BuiltCommand(_commandType, _parameters, _timeout);
        }
        
        /// <summary>
        /// Get copy of current parameters (for debugging)
        /// </summary>
        public Dictionary<string, object> GetParameters()
        {
            return new Dictionary<string, object>(_parameters);
        }
        
        /// <summary>
        /// Clear all parameters
        /// </summary>
        public CommandBuilder ClearParameters()
        {
            _parameters.Clear(); // Logging disabledreturn this;
        }
        
        /// <summary>
        /// Remove specific parameter
        /// </summary>
        public CommandBuilder RemoveParameter(string name)
        {
            if (_parameters.Remove(name))
            { // Logging disabled}
                return this;
            }
        }
        
        /// <summary>
        /// Check if parameter exists
        /// </summary>
        public bool HasParameter(string name)
        {
            return _parameters.ContainsKey(name);
        }
        
        /// <summary>
        /// Get parameter value
        /// </summary>
        public T GetParameter<T>(string name, T defaultValue = default)
        {
            if (!_parameters.TryGetValue(name, out var value))
                return defaultValue;
                
            if (value is T typedValue)
                return typedValue;
                
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }
        
        private void ValidateCommand(IGameCommand command)
        {
            // Run basic validation
            if (!command.IsValid())
            {
                throw new InvalidOperationException($"Command validation failed for {_commandType}");
            }
            
            // Run custom validators
            foreach (var validator in _validators)
            {
                if (!validator(command))
                {
                    throw new InvalidOperationException($"Custom validation failed for {_commandType}");
                }
            }
            
            // Check required faction parameter for most commands
            if (!HasParameter(ParameterNames.Faction) && RequiresFactionParameter(_commandType))
            {
                throw new InvalidOperationException($"Command {_commandType} requires a faction parameter");
            }
        }
        
        private bool RequiresFactionParameter(string commandType)
        {
            // Most commands require faction except for some universe-level commands
            return !commandType.Equals(NativeCommandTypes.SetOverride, StringComparison.OrdinalIgnoreCase) &&
                   !commandType.Equals(NativeCommandTypes.SPLoadPrefab, StringComparison.OrdinalIgnoreCase);
        }
        
        public override string ToString()
        {
            return $"CommandBuilder[{_commandType}] with {_parameters.Count} parameters";
        }
    }
    
    /// <summary>
    /// Internal command implementation built by CommandBuilder
    /// </summary>
    internal class BuiltCommand : GameCommandBase
    {
        private readonly object _faction;
        private readonly TimeSpan? _timeout;
        
        public override string CommandType { get; }
        public override object Faction => _faction;
        
        public BuiltCommand(string commandType, IDictionary<string, object> parameters, TimeSpan? timeout = null)
        {
            CommandType = commandType;
            _timeout = timeout;
            
            // Copy parameters
            foreach (var kvp in parameters)
            {
                AddParameter(kvp.Key, kvp.Value);
            }
            
            // Extract faction
            _faction = GetParameter<object>(ParameterNames.Faction);
        }
        
        public override bool IsValid()
        {
            // Basic validation - command type and faction
            if (string.IsNullOrEmpty(CommandType))
                return false;
                
            // Most commands need a faction
            if (RequiresFaction() && Faction == null)
                return false;
                
            return ValidateRequiredParameters(GetRequiredParameters());
        }
        
        private bool RequiresFaction()
        {
            // Some universe-level commands don't require faction
            return !CommandType.Equals(NativeCommandTypes.SetOverride, StringComparison.OrdinalIgnoreCase) &&
                   !CommandType.Equals(NativeCommandTypes.SPLoadPrefab, StringComparison.OrdinalIgnoreCase);
        }
        
        private string[] GetRequiredParameters()
        {
            // Return required parameters based on command type
            // This could be more sophisticated with attribute-based metadata
            return CommandType switch
            {
                NativeCommandTypes.ImportResource => new[] { ParameterNames.Resource, ParameterNames.Quantity },
                NativeCommandTypes.UnlockBuilding => new[] { ParameterNames.Building },
                NativeCommandTypes.ResearchTechnology => new[] { ParameterNames.Technology },
                NativeCommandTypes.StartDialogue => new[] { ParameterNames.Person, ParameterNames.Dialogue },
                NativeCommandTypes.SetOverride => new[] { ParameterNames.Key, ParameterNames.Value },
                _ => new string[0]
            };
        }
        
        public override string GetDescription()
        {
            var baseDescription = base.GetDescription();
            if (_timeout.HasValue)
            {
                baseDescription += $" [timeout: {_timeout.Value.TotalSeconds:F1}s]";
            }
            return baseDescription;
        }
    }
}
