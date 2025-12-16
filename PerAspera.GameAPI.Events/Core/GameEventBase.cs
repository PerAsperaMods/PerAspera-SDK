using System;
using System.Collections.Generic;

namespace PerAspera.GameAPI.Events.Core
{
    /// <summary>
    /// Base class for all game events
    /// Provides common properties and functionality
    /// Supports both SDK events and native game events
    /// </summary>
    public abstract class GameEventBase : IGameEvent
    {
        /// <summary>
        /// Type of the event (must be implemented by derived classes)
        /// </summary>
        public abstract string EventType { get; }
        
        /// <summary>
        /// When the event occurred
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.Now;

        /// <summary>
        /// Source object that triggered the event (optional)
        /// </summary>
        public object? Source { get; set; }

        /// <summary>
        /// Additional metadata for the event (optional)
        /// </summary>
        public Dictionary<string, object>? Metadata { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        protected GameEventBase()
        {
        }

        /// <summary>
        /// Constructor with source
        /// </summary>
        protected GameEventBase(object source)
        {
            Source = source;
        }

        /// <summary>
        /// Add metadata to the event
        /// </summary>
        public void AddMetadata(string key, object value)
        {
            Metadata ??= new Dictionary<string, object>();
            Metadata[key] = value;
        }

        /// <summary>
        /// Get metadata from the event
        /// </summary>
        public T? GetMetadata<T>(string key)
        {
            if (Metadata?.TryGetValue(key, out var value) == true && value is T result)
                return result;
            return default;
        }

        /// <summary>
        /// Default string representation
        /// </summary>
        public override string ToString()
        {
            return $"{EventType} at {Timestamp:HH:mm:ss}";
        }
    }
}
