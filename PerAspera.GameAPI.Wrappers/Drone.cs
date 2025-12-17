#nullable enable
using System;
using PerAspera.GameAPI.Native;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Elegant wrapper for the native Drone class
    /// Transforms technical decompiled field names into beautiful, intuitive properties
    /// DOC REFERENCES: Drone.md - Decompiled drone class analysis
    /// </summary>
    public class Drone : WrapperBase
    {
        /// <summary>
        /// Initialize Drone wrapper with native drone object
        /// </summary>
        /// <param name="nativeDrone">Native drone instance from game</param>
        public Drone(object nativeDrone) : base(nativeDrone)
        {
        }
        
        /// <summary>
        /// Get drone from Keeper registry by handle
        /// </summary>
        /// <param name="handle">Native drone handle</param>
        /// <returns>Drone wrapper or null if not found</returns>
        public static Drone? GetByHandle(object handle)
        {
            // TODO: Implement via KeeperTypeRegistry when GetDrone method is available
            try
            {
                // Temporary implementation - will be replaced with proper registry lookup
                return handle != null ? new Drone(handle) : null;
            }
            catch
            {
                return null;
            }
        }
        
        // ==================== IDENTITY & POSITION ====================
        
        /// <summary>
        /// Unique drone identifier number
        /// Maps to: _number_k__BackingField
        /// </summary>
        public int DroneNumber
        {
            get => SafeInvoke<int?>("get_number") ?? 0;
        }
        
        /// <summary>
        /// Current 3D position on the planet
        /// Maps to: _position3D_k__BackingField
        /// </summary>
        public object? Position
        {
            get => SafeInvoke<object>("get_position3D");
        }
        
        /// <summary>
        /// Current movement direction and rotation
        /// Maps to: _directionRotation_k__BackingField
        /// </summary>
        public object? Direction
        {
            get => SafeInvoke<object>("get_directionRotation");
        }
        
        // ==================== CARGO & CAPACITY ====================
        
        /// <summary>
        /// Maximum cargo capacity of this drone
        /// Maps to: _cargoCapacity_k__BackingField
        /// </summary>
        public float MaxCargoCapacity
        {
            get => SafeInvoke<float?>("get_cargoCapacity") ?? 0f;
        }
        
        /// <summary>
        /// Whether drone is currently carrying resources
        /// Maps to: _hasResource_k__BackingField
        /// </summary>
        public bool IsCarryingCargo
        {
            get => SafeInvoke<bool?>("get_hasResource") ?? false;
        }
        
        /// <summary>
        /// Current cargo load (calculated property)
        /// Elegant wrapper combining cargo state checks
        /// </summary>
        public float CurrentCargoLoad
        {
            get
            {
                if (!IsCarryingCargo) return 0f;
                // Try to get actual cargo amount if available
                return SafeInvoke<float?>("GetCargoAmount") ?? 0f;
            }
        }
        
        /// <summary>
        /// Percentage of cargo capacity currently used
        /// Elegant computed property for cargo utilization
        /// </summary>
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
        
        /// <summary>
        /// Current operational state ID
        /// Maps to: _stateId_k__BackingField
        /// </summary>
        public int StateId
        {
            get => SafeInvoke<int?>("get_stateId") ?? 0;
        }
        
        /// <summary>
        /// Current navigation path being followed
        /// Maps to: _currentWay_k__BackingField
        /// </summary>
        public object? NavigationPath
        {
            get => SafeInvoke<object>("get_currentWay");
        }
        
        /// <summary>
        /// Whether drone is currently following a path
        /// Elegant wrapper around navigation state
        /// </summary>
        public bool IsNavigating
        {
            get => NavigationPath != null;
        }
        
        // ==================== HEALTH & STATUS ====================
        
        /// <summary>
        /// Whether drone is alive and operational
        /// Maps to: _alive_k__BackingField
        /// </summary>
        public bool IsAlive
        {
            get => SafeInvoke<bool?>("get_alive") ?? false;
        }
        
        /// <summary>
        /// Current health points
        /// Maps to: _health_k__BackingField
        /// </summary>
        public float Health
        {
            get => SafeInvoke<float?>("get_health") ?? 0f;
        }
        
        /// <summary>
        /// Whether drone is visible in the game world
        /// Maps to: _IsVisible_k__BackingField
        /// </summary>
        public bool IsVisible
        {
            get => SafeInvoke<bool?>("get_IsVisible") ?? true;
        }
        
        /// <summary>
        /// Operational status combining multiple state checks
        /// Elegant property for overall drone condition
        /// </summary>
        public DroneOperationalStatus OperationalStatus
        {
            get
            {
                if (!IsAlive) return DroneOperationalStatus.Destroyed;
                if (Health < 20f) return DroneOperationalStatus.Damaged;
                if (!IsVisible) return DroneOperationalStatus.Hidden;
                if (IsNavigating) return DroneOperationalStatus.Moving;
                if (IsCarryingCargo) return DroneOperationalStatus.Loaded;
                return DroneOperationalStatus.Idle;
            }
        }
        
        // ==================== SYSTEM REFERENCES ====================
        
        /// <summary>
        /// Reference to the universe this drone exists in
        /// Maps to: _universe_k__BackingField
        /// </summary>
        public object? Universe
        {
            get => SafeInvoke<object>("get_universe");
        }
        
        /// <summary>
        /// Reference to the planet this drone operates on
        /// Maps to: _planet_k__BackingField
        /// </summary>
        public object? Planet
        {
            get => SafeInvoke<object>("get_planet");
        }
        
        /// <summary>
        /// Drone handle for system operations
        /// Maps to: _handle_k__BackingField
        /// </summary>
        public object? Handle
        {
            get => SafeInvoke<object>("get_handle");
        }
        
        // ==================== UTILITY METHODS ====================
        
        /// <summary>
        /// Get detailed drone information for debugging
        /// Elegant summary combining multiple properties
        /// </summary>
        public override string ToString()
        {
            if (!ValidateNativeObject("ToString"))
                return "Drone: Invalid";
                
            return $"Drone #{DroneNumber}: {OperationalStatus}, " +
                   $"Cargo: {CargoUtilization:F1}% ({CurrentCargoLoad}/{MaxCargoCapacity}), " +
                   $"Health: {Health:F1}, " +
                   $"Navigation: {(IsNavigating ? "Active" : "Idle")}";
        }
    }
    
    /// <summary>
    /// Elegant enum for drone operational status
    /// Replaces complex state ID checking with clear status names
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