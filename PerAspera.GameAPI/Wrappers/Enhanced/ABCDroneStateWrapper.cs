#nullable enable
using System;

namespace PerAspera.GameAPI.Wrappers.Enhanced
{
    /// <summary>
    /// Wrapper for the abstract ABCDroneState class (drone FSM state).
    /// MIGRATION 2026-06-10 — interop typé : Enter/Exit/OnTick/OnFeed/DebugDetailedInfo
    /// délèguent au proxy. OnFeed est désormais correctement typé
    /// (Drone.InputEvent → Drone.StateID) au lieu d'object→object.
    ///
    /// Skill: /per-aspera-drone-routing (FSM, transitions, StateID).
    /// </summary>
    public class ABCDroneStateWrapper : WrapperBase
    {
        /// <summary>Wraps an untyped native drone state (compat). Prefer the typed overload.</summary>
        public ABCDroneStateWrapper(object nativeDroneState) : base(nativeDroneState) { }

        /// <summary>Wraps a typed interop ABCDroneState proxy.</summary>
        public ABCDroneStateWrapper(ABCDroneState nativeDroneState) : base(nativeDroneState) { }

        /// <summary>Typed interop proxy (null when the wrapper is invalid).</summary>
        public ABCDroneState? NativeDroneState => GetNativeObject() as ABCDroneState;

        /// <summary>Create wrapper from native drone state object.</summary>
        public static ABCDroneStateWrapper? FromNative(object? nativeDroneState)
            => nativeDroneState != null ? new ABCDroneStateWrapper(nativeDroneState) : null;

        // ==================== STATE MACHINE METHODS ====================

        /// <summary>Enter the drone state (typed call to ABCDroneState.Enter()).</summary>
        public void Enter() => NativeDroneState?.Enter();

        /// <summary>Exit the drone state (typed call to ABCDroneState.Exit()).</summary>
        public void Exit() => NativeDroneState?.Exit();

        /// <summary>Tick the drone state (typed call to ABCDroneState.OnTick(deltaDays)).</summary>
        /// <param name="deltaDays">Time delta in game days</param>
        public void OnTick(float deltaDays) => NativeDroneState?.OnTick(deltaDays);

        /// <summary>
        /// Feed input to the drone state machine (typed call to ABCDroneState.OnFeed).
        /// </summary>
        /// <param name="input">Input event for the FSM</param>
        /// <returns>StateID for the next state transition, or null when invalid</returns>
        /// <example>var next = state.OnFeed(Drone.InputEvent.ARRIVED);</example>
        public Drone.StateID? OnFeed(Drone.InputEvent input)
            => NativeDroneState?.OnFeed(input);

        /// <summary>Detailed debug information (typed call to DebugDetailedInfo()).</summary>
        public string GetDebugInfo()
            => NativeDroneState?.DebugDetailedInfo() ?? "No debug info available";

        // ==================== UTILITY METHODS ====================

        /// <summary>Type name of this drone state (e.g., DroneStateMoving).</summary>
        public string GetStateTypeName()
            => GetNativeObject()?.GetType().Name ?? "Unknown";

        /// <summary>Check if this is a specific state type.</summary>
        /// <param name="expectedTypeName">Expected state type name</param>
        public bool IsStateType(string expectedTypeName)
            => GetStateTypeName().Equals(expectedTypeName, StringComparison.OrdinalIgnoreCase);

        /// <summary>Runtime information about the state.</summary>
        public StateInfo GetStateInfo()
        {
            return new StateInfo
            {
                TypeName = GetStateTypeName(),
                IsValid = IsValid,
                DebugInfo = GetDebugInfo(),
                LastUpdated = DateTime.Now
            };
        }
    }

    /// <summary>
    /// Information about a drone state for diagnostics
    /// </summary>
    public struct StateInfo
    {
        /// <summary>State type name.</summary>
        public string TypeName { get; set; }
        /// <summary>Wrapper validity.</summary>
        public bool IsValid { get; set; }
        /// <summary>Native debug details.</summary>
        public string DebugInfo { get; set; }
        /// <summary>Snapshot timestamp.</summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>Human-readable summary.</summary>
        public override string ToString()
        {
            return $"{TypeName} (Valid: {IsValid}, Updated: {LastUpdated:HH:mm:ss})";
        }
    }
}
