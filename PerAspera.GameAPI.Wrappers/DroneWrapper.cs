#nullable enable
using System;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native Drone class (typed interop access).
    ///
    /// MIGRATION 2026-06-10 — interop typé d'abord : délégation au proxy <see cref="global::Drone"/>.
    /// Vérifié contre Tools\InteropDump\ScriptsAssembly\Drone.cs. Fantômes corrigés :
    /// hasResource/currentWay/IsVisible/GetCargoAmount n'existent pas — l'état cargo réel
    /// vient de Drone.cargo (Cargo : resource + quantity).
    ///
    /// 🤖 Agent Expert: @per-aspera-sdk-coordinator — Skill: /per-aspera-drone-routing
    /// </summary>
    public class DroneWrapper : WrapperBase
    {
        /// <summary>Wraps an untyped native drone (compat). Prefer the typed overload.</summary>
        public DroneWrapper(object nativeDrone) : base(nativeDrone) { }

        /// <summary>Wraps a typed interop Drone proxy.</summary>
        public DroneWrapper(Drone nativeDrone) : base(nativeDrone) { }

        /// <summary>Typed interop proxy (null when the wrapper is invalid).</summary>
        /// <example>var cargo = drone.NativeDrone?.cargo;</example>
        public Drone? NativeDrone => GetNativeObject() as Drone;

        /// <summary>Factory — retourne null si l'objet natif est null.</summary>
        public static DroneWrapper? FromNative(object? native)
            => native != null ? new DroneWrapper(native) : null;

        /// <summary>Get drone wrapper from a native drone object.</summary>
        public static DroneWrapper? GetByHandle(object handle)
            => FromNative(handle);

        // ==================== IDENTITY & POSITION ====================

        /// <summary>Unique drone identifier number (typed read of Drone.number).</summary>
        public int DroneNumber => NativeDrone?.number ?? 0;

        /// <summary>Current 3D position on the planet (typed read of Drone.position3D).</summary>
        public UnityEngine.Vector3 Position => NativeDrone?.position3D ?? UnityEngine.Vector3.zero;

        /// <summary>Current movement rotation (typed read of Drone.directionRotation).</summary>
        public UnityEngine.Quaternion Direction => NativeDrone?.directionRotation ?? UnityEngine.Quaternion.identity;

        // ==================== CARGO & CAPACITY ====================

        /// <summary>
        /// Maximum cargo capacity in units (typed read of Drone.cargoCapacity).
        /// ⚠️ cargoCapacity natif est un CargoQuantity, pas un float — l'ancien binding
        /// échouait silencieusement.
        /// </summary>
        public float MaxCargoCapacity => NativeDrone?.cargoCapacity.ToFloat() ?? 0f;

        /// <summary>Current cargo (typed Cargo proxy, null when empty).</summary>
        public Cargo? Cargo => NativeDrone?.cargo;

        /// <summary>
        /// Whether drone is currently carrying resources.
        /// (« hasResource » n'existe pas — l'état réel est Drone.cargo != null.)
        /// </summary>
        public bool IsCarryingCargo => NativeDrone?.cargo != null;

        /// <summary>
        /// Current cargo load in units (typed read of Drone.cargo.quantity).
        /// (« GetCargoAmount » n'existait pas — retournait toujours 0.)
        /// </summary>
        public float CurrentCargoLoad => NativeDrone?.cargo?.quantity.ToFloat() ?? 0f;

        /// <summary>Resource key currently carried, or null (typed).</summary>
        public string? CargoResourceKey => NativeDrone?.cargo?.resource?.key;

        /// <summary>Percentage of cargo capacity currently used.</summary>
        public float CargoUtilization
        {
            get
            {
                var maxCapacity = MaxCargoCapacity;
                if (maxCapacity <= 0) return 0f;
                return (CurrentCargoLoad / maxCapacity) * 100f;
            }
        }

        // ==================== STATE & NAVIGATION ====================

        /// <summary>Current operational state ID (typed read of Drone.stateId).</summary>
        public int StateId => (int)(NativeDrone?.stateId ?? 0);

        /// <summary>N'a jamais existé — Drone n'a pas de currentWay exposé.</summary>
        [Obsolete("Drone.currentWay n'existe pas — retournait toujours null. Voir l'état FSM via StateId et /per-aspera-drone-routing.", false)]
        public object? NavigationPath => null;

        /// <summary>
        /// Whether drone is moving (FSM state check via stateId).
        /// (L'ancienne implémentation testait le NavigationPath fantôme — toujours false.)
        /// </summary>
        public bool IsNavigating => NativeDrone?._currentState != null &&
                                    NativeDrone._currentState.Pointer == NativeDrone._stateMoving?.Pointer;

        // ==================== HEALTH & STATUS ====================

        /// <summary>Whether drone is alive and operational (typed read of Drone.alive).</summary>
        public bool IsAlive => NativeDrone?.alive ?? false;

        /// <summary>Current health points (typed read of Drone.health).</summary>
        public float Health => NativeDrone?.health ?? 0f;

        /// <summary>N'a jamais existé sur Drone.</summary>
        [Obsolete("Drone.IsVisible n'existe pas — retournait toujours true.", false)]
        public bool IsVisible => true;

        /// <summary>
        /// Operational status combining multiple state checks.
        /// </summary>
        public DroneOperationalStatus OperationalStatus
        {
            get
            {
                if (!IsAlive) return DroneOperationalStatus.Destroyed;
                if (Health < 20f) return DroneOperationalStatus.Damaged;
                if (IsNavigating) return DroneOperationalStatus.Moving;
                if (IsCarryingCargo) return DroneOperationalStatus.Loaded;
                return DroneOperationalStatus.Idle;
            }
        }

        // ==================== SYSTEM REFERENCES ====================

        /// <summary>Universe this drone exists in (typed).</summary>
        public Universe? Universe => NativeDrone?.universe;

        /// <summary>Planet this drone operates on (typed).</summary>
        public Planet? Planet => NativeDrone?.planet;

        /// <summary>Drone handle for system operations (typed).</summary>
        public Handle? Handle => NativeDrone?.handle;

        // ==================== UTILITY METHODS ====================

        /// <summary>Detailed drone information for debugging.</summary>
        public override string ToString()
        {
            if (!IsValidWrapper) return "Drone: Invalid";
            return $"Drone #{DroneNumber}: {OperationalStatus}, " +
                   $"Cargo: {CargoUtilization:F1}% ({CurrentCargoLoad}/{MaxCargoCapacity}), " +
                   $"Health: {Health:F1}";
        }
    }

    /// <summary>
    /// Drone operational status — clear names instead of raw state IDs.
    /// </summary>
    public enum DroneOperationalStatus
    {
        /// <summary>Drone has been destroyed and is no longer functional</summary>
        Destroyed,
        /// <summary>Drone is damaged but potentially repairable</summary>
        Damaged,
        /// <summary>Drone exists but is not visible in the game world</summary>
        Hidden,
        /// <summary>Drone is idle and waiting for tasks</summary>
        Idle,
        /// <summary>Drone is currently moving to a destination</summary>
        Moving,
        /// <summary>Drone is carrying cargo</summary>
        Loaded,
        /// <summary>Drone is actively working on a task</summary>
        Working
    }
}
