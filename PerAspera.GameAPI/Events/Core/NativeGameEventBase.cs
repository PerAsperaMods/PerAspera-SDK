using System;

namespace PerAspera.GameAPI.Events.Core
{
    /// <summary>
    /// Base class for all native game events from Universe.gameEventBus
    /// Wraps native GameEvent struct with SDK-friendly interface
    /// DOC: GameEvent.md - struct with sender/target/type/payload
    /// </summary>
    public abstract class NativeGameEventBase : GameEventBase
    {
        /// <summary>Native GameEvent struct (sender, target, type, payload)</summary>
        public object? NativeGameEvent { get; set; }
        
        /// <summary>Sender Handle from native event</summary>
        public object? Sender { get; set; }
        
        /// <summary>Target Handle from native event</summary>
        public object? Target { get; set; }
        
        /// <summary>GameEventType from native event</summary>
        public object? NativeEventType { get; set; }
        
        /// <summary>GameEventPayload from native event</summary>
        public object? Payload { get; set; }
        
        /// <summary>Current Martian sol when event was triggered</summary>
        public int MartianSol { get; set; }

        protected NativeGameEventBase() : base() { }
        protected NativeGameEventBase(object source) : base(source) { }
    }
}
