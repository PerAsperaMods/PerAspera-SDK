#nullable enable
using System;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native Building class.
    /// Provides safe access to building properties and operations.
    ///
    /// 📚 Vanilla Reference: F:\ModPeraspera\CleanedScriptAssemblyClass\Building.md
    /// 🤖 Agent Expert: @per-aspera-sdk-coordinator
    /// </summary>
    public class BuildingWrapper : WrapperBase
    {
        public BuildingWrapper(object nativeBuilding) : base(nativeBuilding) { }

        /// <summary>Factory — retourne null si l'objet natif est null.</summary>
        public static BuildingWrapper? FromNative(object? native)
            => native != null ? new BuildingWrapper(native) : null;

        // ==================== CORE IDENTIFICATION ====================

        /// <summary>Unique building identifier number.</summary>
        public int Number => SafeInvoke<int?>("get_number") ?? 0;

        /// <summary>Building type definition (factory, hab, etc.)</summary>
        public BuildingTypeWrapper GetBuildingType()
            => new BuildingTypeWrapper(SafeInvoke<object>("get_buildingType"));

        /// <summary>Building type key (e.g., "SolarPanel", "Mine").</summary>
        public string TypeKey
        {
            get
            {
                var buildingType = SafeInvoke<object>("get_buildingType");
                return buildingType?.InvokeMethod<string>("get_key") ?? "Unknown";
            }
        }

        /// <summary>Building name from its type.</summary>
        public string BuildingTypeName
        {
            get
            {
                try { return GetBuildingType()?.GetFieldValue<string>("name") ?? "Unknown"; }
                catch { return "Unknown"; }
            }
        }

        // ==================== BASIC PROPERTIES ====================

        /// <summary>2D position on the planet surface.</summary>
        public (float x, float y) Position
        {
            get
            {
                var pos = SafeInvoke<object>("get_position");
                if (pos == null) return (0, 0);
                return (pos.GetFieldValue<float>("x"), pos.GetFieldValue<float>("y"));
            }
        }

        /// <summary>Is the building alive (not destroyed).</summary>
        public bool IsAlive => SafeGetField<bool>("_alive");

        /// <summary>Is the building operational.</summary>
        public bool IsOperative => SafeGetField<bool>("_activated");

        /// <summary>Is the building fully built (not under construction).</summary>
        public bool IsBuilt => SafeGetField<bool>("_built");

        /// <summary>Is the building powered.</summary>
        public bool IsPowered => SafeGetField<bool>("_powered");

        /// <summary>Is the building broken.</summary>
        public bool IsBroken => SafeGetField<bool>("_broken");

        /// <summary>Current health (0.0 to 1.0).</summary>
        public float Health => SafeGetField<float>("_health");

        /// <summary>Is the building active (alive, built, operational and not broken).</summary>
        public bool IsActive => IsAlive && IsBuilt && IsOperative && !IsBroken;

        // ==================== FACTION ====================

        /// <summary>Get the faction that owns this building.</summary>
        public FactionWrapper? GetFaction()
        {
            var native = SafeInvoke<object>("get_faction");
            return native != null ? new FactionWrapper(native) : null;
        }

        // ==================== STOCKPILE ====================

        /// <summary>Get the building's stockpile.</summary>
        public object? GetStockpile() => SafeInvoke<object>("GetStockpile");

        /// <summary>Get resource quantity in stockpile.</summary>
        /// <param name="resourceKey">Resource key (e.g., "resource_water")</param>
        public float GetResourceStock(string resourceKey)
        {
            var stockpile = GetStockpile();
            if (stockpile == null) return 0f;
            var resourceType = KeeperTypeRegistry.GetResourceType(resourceKey);
            if (resourceType == null) return 0f;
            var cargoQty = stockpile.InvokeMethod<object>("get_Item", resourceType);
            return cargoQty?.GetFieldValue<float>("quantity") ?? 0f;
        }

        // ==================== ACTIONS ====================

        /// <summary>Toggle operative state.</summary>
        public void ToggleOperative() => SafeInvokeVoid("ToggleOperative");

        /// <summary>Start scrapping the building.</summary>
        public void StartScrapping() => SafeInvokeVoid("StartScrapping");

        /// <summary>Cancel scrapping.</summary>
        public void CancelScrapping() => SafeInvokeVoid("CancelScrapping");

        // ==================== INFO ====================

        public override string ToString()
        {
            try
            {
                var pos = Position;
                return $"Building #{Number}: {TypeKey} at ({pos.x:F1}, {pos.y:F1}) — " +
                       $"Alive:{IsAlive} Built:{IsBuilt} Operative:{IsOperative}";
            }
            catch
            {
                return $"Building #{Number}: {TypeKey} — Status unavailable";
            }
        }
    }
}
