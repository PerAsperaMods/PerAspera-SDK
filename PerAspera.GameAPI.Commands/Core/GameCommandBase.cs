using System;
using System.Collections.Generic;
using System.Linq;

namespace PerAspera.GameAPI.Commands.Core
{
    /// &lt;summary&gt;
    /// Abstract base class for all game commands with common functionality
    /// &lt;/summary&gt;
    public abstract class GameCommandBase : IGameCommand
    {
        /// &lt;inheritdoc/&gt;
        public abstract string CommandType { get; }
        
        /// &lt;inheritdoc/&gt;
        public DateTime Timestamp { get; }
        
        /// &lt;inheritdoc/&gt;
        public abstract object Faction { get; }
        
        /// &lt;summary&gt;
        /// Parameters for this command
        /// &lt;/summary&gt;
        protected Dictionary&lt;string, object&gt; Parameters { get; }
        
        /// &lt;summary&gt;
        /// Initialize base command with current timestamp
        /// &lt;/summary&gt;
        protected GameCommandBase()
        {
            Timestamp = DateTime.UtcNow;
            Parameters = new Dictionary&lt;string, object&gt;();
        }
        
        /// &lt;inheritdoc/&gt;
        public abstract bool IsValid();
        
        /// &lt;inheritdoc/&gt;
        public virtual string GetDescription()
        {
            var parametersList = string.Join(", ", Parameters.Select(kvp =&gt; $"{kvp.Key}={kvp.Value}"));
            return $"{CommandType}({parametersList})";
        }
        
        /// &lt;summary&gt;
        /// Add parameter to this command
        /// &lt;/summary&gt;
        protected void AddParameter(string key, object value)
        {
            Parameters[key] = value;
        }
        
        /// &lt;summary&gt;
        /// Get parameter value with type conversion
        /// &lt;/summary&gt;
        protected T GetParameter&lt;T&gt;(string key, T defaultValue = default)
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
        
        /// &lt;summary&gt;
        /// Check if parameter exists
        /// &lt;/summary&gt;
        protected bool HasParameter(string key)
        {
            return Parameters.ContainsKey(key);
        }
        
        /// &lt;summary&gt;
        /// Validate required parameters are present
        /// &lt;/summary&gt;
        protected bool ValidateRequiredParameters(params string[] requiredParameters)
        {
            return requiredParameters.All(param =&gt; Parameters.ContainsKey(param) && Parameters[param] != null);
        }
        
        public override string ToString()
        {
            return GetDescription();
        }
    }
}