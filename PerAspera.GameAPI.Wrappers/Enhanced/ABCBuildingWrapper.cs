#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core.IL2CPP;
using UnityEngine;

namespace PerAspera.GameAPI.Wrappers.Enhanced
{
    /// <summary>
    /// Wrapper for the abstract ABCBuilding base class
    /// Provides safe access to building functionality and IHandleable implementation
    /// DOC: ABCBuilding is the base class for all buildings, implements IHandleable
    /// </summary>
    public class ABCBuildingWrapper : WrapperBase
    {
        private static readonly string LogPrefix = "[ABCBuildingWrapper]";
        
        /// <summary>
        /// Initialize ABCBuildingWrapper with native ABCBuilding instance
        /// </summary>
        /// <param name="nativeBuilding">Native ABCBuilding instance from game</param>
        public ABCBuildingWrapper(object nativeBuilding) : base(nativeBuilding)
        {
        }
        
        /// <summary>
        /// Create wrapper from native building object
        /// </summary>
        public static ABCBuildingWrapper? FromNative(object? nativeBuilding)
        {
            return nativeBuilding != null ? new ABCBuildingWrapper(nativeBuilding) : null;
        }
        
        // ==================== IHANDLEABLE IMPLEMENTATION ====================
        
        /// <summary>
        /// Get the Handle for this building (IHandleable.GetHandle())
        /// Essential for Keeper system integration
        /// </summary>
        /// <returns>Handle object for Keeper lookup</returns>
        public object? GetHandle()
        {
            try
            {
                return SafeInvoke<object>("GetHandle");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LogPrefix} Failed to get handle: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Check if this building is registered in the Keeper system
        /// </summary>
        /// <returns>True if building has a valid handle and is registered</returns>
        public bool IsRegistered()
        {
            var handle = GetHandle();
            if (handle == null) return false;
            
            // Use KeeperTypeRegistry to verify registration
            return KeeperTypeRegistry.GetByHandle(handle) != null;
        }
        
        // ==================== CORE BUILDING PROPERTIES ====================
        
        /// <summary>
        /// Get building type definition
        /// Maps to: buildingType property
        /// </summary>
        public BuildingType? GetBuildingType()
        {
            try
            {
                var nativeBuildingType = SafeInvoke<object>("get_buildingType");
                return nativeBuildingType != null ? new BuildingType(nativeBuildingType) : null;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LogPrefix} Failed to get building type: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get building position in world space
        /// Maps to: transform.position or position property
        /// </summary>
        public Vector3 GetPosition()
        {
            try
            {
                // Try position property first
                var position = SafeInvoke<Vector3>("get_position");
                if (position != Vector3.zero) return position;
                
                // Fallback to transform.position
                var transform = SafeInvoke<Transform>("get_transform");
                return transform?.position ?? Vector3.zero;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LogPrefix} Failed to get position: {ex.Message}");
                return Vector3.zero;
            }
        }
        
        /// <summary>
        /// Get building's energy production rate
        /// Maps to: energyProduction property
        /// </summary>
        public float GetEnergyProduction()
        {
            try
            {
                return SafeInvoke<float>("get_energyProduction");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LogPrefix} Failed to get energy production: {ex.Message}");
                return 0f;
            }
        }
        
        /// <summary>
        /// Get building's health/condition
        /// Maps to: health property
        /// </summary>
        public float GetHealth()
        {
            try
            {
                return SafeInvoke<float>("get_health");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LogPrefix} Failed to get health: {ex.Message}");
                return 0f;
            }
        }
        
        /// <summary>
        /// Check if building is operational/functioning
        /// Maps to: IsOperational() or operational property
        /// </summary>
        public bool IsOperational()
        {
            try
            {
                // Try method first
                var result = SafeInvoke<bool>("IsOperational");
                if (result) return true;
                
                // Fallback to property
                return SafeInvoke<bool>("get_operational");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LogPrefix} Failed to check operational status: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Check if building construction is complete
        /// Maps to: IsBuilt property or method
        /// </summary>
        public bool IsBuilt()
        {
            try
            {
                // Try property first
                var built = SafeInvoke<bool>("get_IsBuilt");
                if (built) return true;
                
                // Try method
                return SafeInvoke<bool>("IsBuilt");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LogPrefix} Failed to check built status: {ex.Message}");
                return false;
            }
        }
        
        // ==================== ADVANCED BUILDING OPERATIONS ====================
        
        /// <summary>
        /// Get building's work progress (0-100%)
        /// Maps to: workProgress property
        /// </summary>
        public float GetWorkProgress()
        {
            try
            {
                return SafeInvoke<float>("get_workProgress");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LogPrefix} Failed to get work progress: {ex.Message}");
                return 0f;
            }
        }
        
        /// <summary>
        /// Get building efficiency rating (0-1.0)
        /// Calculated from health, operational status, and environment factors
        /// </summary>
        public float GetEfficiency()
        {
            try
            {
                var efficiency = SafeInvoke<float>("get_efficiency");
                if (efficiency > 0) return efficiency;
                
                // Calculate basic efficiency from health and operational status
                if (!IsOperational() || !IsBuilt()) return 0f;
                
                var health = GetHealth();
                return Mathf.Clamp01(health / 100f); // Normalize health to 0-1
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LogPrefix} Failed to get efficiency: {ex.Message}");
                return 0f;
            }
        }
        
        /// <summary>
        /// Get the faction/owner of this building
        /// Maps to: faction or owner property
        /// </summary>
        public Faction? GetOwner()
        {
            try
            {
                var nativeFaction = SafeInvoke<object>("get_faction");
                if (nativeFaction == null)
                    nativeFaction = SafeInvoke<object>("get_owner");
                    
                return nativeFaction != null ? new Faction(nativeFaction) : null;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LogPrefix} Failed to get owner: {ex.Message}");
                return null;
            }
        }
        
        // ==================== UTILITY METHODS ====================
        
        /// <summary>
        /// Get comprehensive building information for debugging/monitoring
        /// </summary>
        public BuildingInfo GetBuildingInfo()
        {
            return new BuildingInfo
            {
                Handle = GetHandle(),
                BuildingType = GetBuildingType()?.Name ?? "Unknown",
                Position = GetPosition(),
                EnergyProduction = GetEnergyProduction(),
                Health = GetHealth(),
                IsOperational = IsOperational(),
                IsBuilt = IsBuilt(),
                WorkProgress = GetWorkProgress(),
                Efficiency = GetEfficiency(),
                Owner = GetOwner()?.Name ?? "Unknown",
                LastUpdated = DateTime.Now
            };
        }
        
        /// <summary>
        /// Get building name/identifier for display
        /// </summary>
        public string GetDisplayName()
        {
            var buildingType = GetBuildingType();
            return buildingType?.GetDisplayName() ?? GetType().Name;
        }
        
        /// <summary>
        /// Check if this building matches a specific building type
        /// </summary>
        /// <param name="buildingTypeName">Building type name to check</param>
        /// <returns>True if building matches the type</returns>
        public bool IsOfType(string buildingTypeName)
        {
            var buildingType = GetBuildingType();
            return buildingType?.Name?.Equals(buildingTypeName, StringComparison.OrdinalIgnoreCase) ?? false;
        }
        
        /// <summary>
        /// Perform validation checks on the building
        /// </summary>
        public BuildingValidation Validate()
        {
            var validation = new BuildingValidation();
            
            validation.IsValid = IsValid;
            validation.HasHandle = GetHandle() != null;
            validation.IsRegistered = IsRegistered();
            validation.IsBuilt = IsBuilt();
            validation.IsOperational = IsOperational();
            validation.HasValidType = GetBuildingType() != null;
            validation.HealthOK = GetHealth() > 10f; // Arbitrary threshold
            
            validation.OverallStatus = validation.IsValid && validation.HasHandle && 
                                     validation.IsRegistered && validation.IsBuilt;
                                     
            return validation;
        }
    }
    
    /// <summary>
    /// Comprehensive building information for monitoring and debugging
    /// </summary>
    public struct BuildingInfo
    {
        public object? Handle { get; set; }
        public string BuildingType { get; set; }
        public Vector3 Position { get; set; }
        public float EnergyProduction { get; set; }
        public float Health { get; set; }
        public bool IsOperational { get; set; }
        public bool IsBuilt { get; set; }
        public float WorkProgress { get; set; }
        public float Efficiency { get; set; }
        public string Owner { get; set; }
        public DateTime LastUpdated { get; set; }
        
        public override string ToString()
        {
            return $"{BuildingType} at {Position} (Health: {Health:F1}%, Efficiency: {Efficiency:P})";
        }
    }
    
    /// <summary>
    /// Building validation results for health checking
    /// </summary>
    public struct BuildingValidation
    {
        public bool IsValid { get; set; }
        public bool HasHandle { get; set; }
        public bool IsRegistered { get; set; }
        public bool IsBuilt { get; set; }
        public bool IsOperational { get; set; }
        public bool HasValidType { get; set; }
        public bool HealthOK { get; set; }
        public bool OverallStatus { get; set; }
        
        public override string ToString()
        {
            return $"Valid: {OverallStatus} (Handle: {HasHandle}, Registered: {IsRegistered}, Built: {IsBuilt}, Operational: {IsOperational})";
        }
    }
}