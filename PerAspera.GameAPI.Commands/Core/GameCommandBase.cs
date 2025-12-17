using System;
using System.Collections.Generic;
using System.Linq;

namespace PerAspera.GameAPI.Commands.Core
{
    /// <summary>
    /// Abstract base class for all game commands with common functionality
    /// </summary>
    public abstract class GameCommandBase : IGameCommand
    {
        /// <inheritdoc/>
        public virtual string CommandType => _commandType ?? GetType().Name;
        
        /// <inheritdoc/>
        public DateTime Timestamp { get; }
        
        /// <inheritdoc/>
        public abstract object Faction { get; }
        
        /// <summary>
        /// Parameters for this command
        /// </summary>
        protected Dictionary<string, object> Parameters { get; }
        
        /// <summary>
        /// Initialize base command with current timestamp
        /// </summary>
        protected GameCommandBase()
        {
            Timestamp = DateTime.UtcNow;
            Parameters = new Dictionary<string, object>();
        }
        
        /// <summary>
        /// Initialize base command with command type and current timestamp
        /// </summary>
        protected GameCommandBase(string commandType) : this()
        {
            _commandType = commandType;
        }
        
        private readonly string _commandType;
        
        /// <inheritdoc/>
        public abstract bool IsValid();
        
        /// <inheritdoc/>
        public virtual string GetDescription()
        {
            var parametersList = string.Join(", ", Parameters.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            return $"{CommandType}({parametersList})";
        }
        
        /// <summary>
        /// Add parameter to this command
        /// </summary>
        protected void AddParameter(string key, object value)
        {
            Parameters[key] = value;
        }
        
        /// <summary>
        /// Get parameter value with type conversion
        /// </summary>
        protected T GetParameter<T>(string key, T defaultValue = default)
        {
            if (!Parameters.TryGetValue(key, out var value))
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
        
        /// <summary>
        /// Check if parameter exists
        /// </summary>
        protected bool HasParameter(string key)
        {
            return Parameters.ContainsKey(key);
        }
        
        /// <summary>
        /// Validate required parameters are present
        /// </summary>
        protected bool ValidateRequiredParameters(params string[] requiredParameters)
        {
            return requiredParameters.All(param => Parameters.ContainsKey(param) && Parameters[param] != null);
        }
        
        public override string ToString()
        {
            return GetDescription();
        }
    }
}