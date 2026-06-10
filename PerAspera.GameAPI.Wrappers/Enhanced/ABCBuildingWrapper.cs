#nullable enable
using System;
using UnityEngine;

namespace PerAspera.GameAPI.Wrappers.Enhanced
{
    /// <summary>
    /// Wrapper for the abstract ABCBuilding base class.
    ///
    /// MIGRATION 2026-06-10 — interop typé d'abord. ABCBuilding natif n'expose QUE
    /// handle/GetName/Dispose ; tous les membres riches (buildingType, health, faction…)
    /// vivent sur la classe dérivée Building — l'ancien SafeInvoke ne marchait que parce
    /// que la réflexion résolvait sur le type runtime. Le wrapper expose désormais les
    /// deux niveaux typés : <see cref="NativeABCBuilding"/> et <see cref="AsBuilding"/>.
    /// Fantômes corrigés : energyProduction/efficiency/get_transform n'existent pas
    /// (GetPosition retournait TOUJOURS Vector3.zero).
    /// </summary>
    public class ABCBuildingWrapper : WrapperBase
    {
        /// <summary>Wraps an untyped native building (compat). Prefer the typed overload.</summary>
        public ABCBuildingWrapper(object nativeBuilding) : base(nativeBuilding) { }

        /// <summary>Wraps a typed interop ABCBuilding proxy.</summary>
        public ABCBuildingWrapper(ABCBuilding nativeBuilding) : base(nativeBuilding) { }

        /// <summary>Typed ABCBuilding proxy (null when the wrapper is invalid).</summary>
        public ABCBuilding? NativeABCBuilding => GetNativeObject() as ABCBuilding;

        /// <summary>
        /// Typed Building proxy when the wrapped object is a concrete Building
        /// (null pour les autres dérivés d'ABCBuilding).
        /// </summary>
        /// <example>float hp = abc.AsBuilding?._health ?? 0f;</example>
        public Building? AsBuilding => GetNativeObject() as Building;

        /// <summary>Create wrapper from native building object.</summary>
        public static ABCBuildingWrapper? FromNative(object? nativeBuilding)
            => nativeBuilding != null ? new ABCBuildingWrapper(nativeBuilding) : null;

        // ==================== IHANDLEABLE IMPLEMENTATION ====================

        /// <summary>
        /// Get the Handle for this building (typed read of ABCBuilding.handle).
        /// Essential for Keeper system integration.
        /// </summary>
        public Handle? GetHandle() => NativeABCBuilding?.handle;

        /// <summary>Check if this building is registered in the Keeper system.</summary>
        public bool IsRegistered()
        {
            var handle = GetHandle();
            if (handle == null) return false;
            return KeeperTypeRegistry.GetByHandle(handle) != null;
        }

        /// <summary>Native display name (typed call to ABCBuilding.GetName()).</summary>
        public string GetName() => NativeABCBuilding?.GetName() ?? "Unknown";

        // ==================== CORE BUILDING PROPERTIES ====================

        /// <summary>Building type definition (typed via Building.buildingType).</summary>
        public BuildingTypeWrapper? GetBuildingType()
            => BuildingTypeWrapper.FromNative(AsBuilding?.buildingType);

        /// <summary>
        /// Building position in world space (typed via Building.position, Vector2 → Vector3).
        /// (L'ancienne implémentation visait get_position en Vector3 puis get_transform —
        /// les deux échouaient : retournait TOUJOURS Vector3.zero.)
        /// </summary>
        public Vector3 GetPosition()
        {
            var b = AsBuilding;
            if (b == null) return Vector3.zero;
            var pos = b.position;
            return new Vector3(pos.x, pos.y, 0f);
        }

        /// <summary>N'a jamais existé — Building n'a pas d'energyProduction scalaire.</summary>
        [Obsolete("Building.energyProduction n'existe pas — retournait toujours 0. La production max vient du BuildingType (BaseEnergyOutput) ; l'état runtime via Building.powerEfficiency (ProductivityBuffer).", false)]
        public float GetEnergyProduction() => 0f;

        /// <summary>Building health (typed read of Building._health).</summary>
        public float GetHealth() => AsBuilding?._health ?? 0f;

        /// <summary>Building operational state (typed read of Building._activated).</summary>
        public bool IsOperational() => AsBuilding?._activated ?? false;

        /// <summary>Construction complete (typed read of Building._built).</summary>
        public bool IsBuilt() => AsBuilding?._built ?? false;

        // ==================== ADVANCED BUILDING OPERATIONS ====================

        /// <summary>Work progress (typed read of Building.workProgress).</summary>
        public float GetWorkProgress() => AsBuilding?.workProgress ?? 0f;

        /// <summary>
        /// Building efficiency estimate (0-1.0), from health and operational state.
        /// (« efficiency » n'existe pas en float natif — calcul wrapper assumé.)
        /// </summary>
        public float GetEfficiency()
        {
            if (!IsOperational() || !IsBuilt()) return 0f;
            return Mathf.Clamp01(GetHealth());
        }

        /// <summary>Faction owning this building (typed via Building.faction).</summary>
        public FactionWrapper? GetOwner()
            => FactionWrapper.FromNative(AsBuilding?.faction);

        // ==================== UTILITY METHODS ====================

        /// <summary>Comprehensive building information for debugging/monitoring.</summary>
        public BuildingInfo GetBuildingInfo()
        {
            return new BuildingInfo
            {
                Handle = GetHandle(),
                BuildingType = GetBuildingType()?.Name ?? "Unknown",
                Position = GetPosition(),
                Health = GetHealth(),
                IsOperational = IsOperational(),
                IsBuilt = IsBuilt(),
                WorkProgress = GetWorkProgress(),
                Efficiency = GetEfficiency(),
                Owner = GetOwner()?.Name ?? "Unknown",
                LastUpdated = DateTime.Now
            };
        }

        /// <summary>Building name/identifier for display.</summary>
        public string GetDisplayName()
        {
            var buildingType = GetBuildingType();
            return buildingType?.GetDisplayName() ?? GetName();
        }

        /// <summary>Check if this building matches a specific building type key.</summary>
        public bool IsOfType(string buildingTypeName)
        {
            var buildingType = GetBuildingType();
            return buildingType?.Name?.Equals(buildingTypeName, StringComparison.OrdinalIgnoreCase) ?? false;
        }

        /// <summary>Perform validation checks on the building.</summary>
        public BuildingValidation Validate()
        {
            var validation = new BuildingValidation();

            validation.IsValid = IsValid;
            validation.HasHandle = GetHandle() != null;
            validation.IsRegistered = IsRegistered();
            validation.IsBuilt = IsBuilt();
            validation.IsOperational = IsOperational();
            validation.HasValidType = GetBuildingType() != null;
            validation.HealthOK = GetHealth() > 0.1f;

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
        /// <summary>Native Keeper handle.</summary>
        public object? Handle { get; set; }
        /// <summary>Building type key.</summary>
        public string BuildingType { get; set; }
        /// <summary>World position.</summary>
        public Vector3 Position { get; set; }
        /// <summary>Health (0-1).</summary>
        public float Health { get; set; }
        /// <summary>Operational state.</summary>
        public bool IsOperational { get; set; }
        /// <summary>Construction complete.</summary>
        public bool IsBuilt { get; set; }
        /// <summary>Work progress.</summary>
        public float WorkProgress { get; set; }
        /// <summary>Efficiency estimate (0-1).</summary>
        public float Efficiency { get; set; }
        /// <summary>Owning faction name.</summary>
        public string Owner { get; set; }
        /// <summary>Snapshot timestamp.</summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>Human-readable summary.</summary>
        public override string ToString()
        {
            return $"{BuildingType} at {Position} (Health: {Health:F2}, Efficiency: {Efficiency:P})";
        }
    }

    /// <summary>
    /// Building validation results for health checking
    /// </summary>
    public struct BuildingValidation
    {
        /// <summary>Wrapper validity.</summary>
        public bool IsValid { get; set; }
        /// <summary>Has a Keeper handle.</summary>
        public bool HasHandle { get; set; }
        /// <summary>Registered in Keeper.</summary>
        public bool IsRegistered { get; set; }
        /// <summary>Construction complete.</summary>
        public bool IsBuilt { get; set; }
        /// <summary>Operational state.</summary>
        public bool IsOperational { get; set; }
        /// <summary>Building type resolved.</summary>
        public bool HasValidType { get; set; }
        /// <summary>Health above threshold.</summary>
        public bool HealthOK { get; set; }
        /// <summary>Overall validation status.</summary>
        public bool OverallStatus { get; set; }

        /// <summary>Human-readable summary.</summary>
        public override string ToString()
        {
            return $"Valid: {OverallStatus} (Handle: {HasHandle}, Registered: {IsRegistered}, Built: {IsBuilt}, Operational: {IsOperational})";
        }
    }
}
