using System;

namespace PerAspera.GameAPI.Events.Core
{
    /// <summary>
    /// Base interface for all game events (Native and SDK)
    /// </summary>
    public interface IGameEvent
    {
        /// <summary>
        /// Type of the event (used for routing and handling)
        /// </summary>
        string EventType { get; }
        
        /// <summary>
        /// When the event occurred
        /// </summary>
        DateTime Timestamp { get; set; }
    }
}
