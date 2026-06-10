using System;

namespace PerAspera.GameAPI.Events.Core
{
    /// <summary>
    /// Base class for SDK custom events (non-native)
    /// Used for mod-to-mod communication and SDK internal events
    /// </summary>
    public abstract class SDKEventBase : GameEventBase
    {
        /// <summary>
        /// Indicates if this event should be persisted/logged
        /// </summary>
        public bool IsPersistent { get; set; } = false;
        
        /// <summary>
        /// Priority for event processing (higher = processed first)
        /// </summary>
        public int Priority { get; set; } = 0;
        
        /// <summary>
        /// Optional mod identifier that created this event
        /// </summary>
        public string? ModId { get; set; }

        protected SDKEventBase() : base() { }
        protected SDKEventBase(object source) : base(source) { }
    }
}
