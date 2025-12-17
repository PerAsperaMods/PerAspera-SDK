#nullable enable
using System;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Native;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native Building class
    /// Provides safe access to building properties and operations
    /// DOC: Building.md - Factory, infrastructure, and production buildings
    /// </summary>
    public class Building : WrapperBase
    {
        /// <summary>
        /// Initialize Building wrapper with native building object
        /// </summary>
        /// <param name="nativeBuilding">Native building instance from game</param>
        public Building(object nativeBuilding) : base(nativeBuilding)
        {
        }
        
        /// <summary>
        /// Create wrapper from native building object
        /// </summary>
        public static Building? FromNative(object? nativeBuilding)
        {
            return nativeBuilding != null ? new Building(nativeBuilding) : null;
        }
        
        // ==================== CORE IDENTIFICATION ====================
        
        /// <summary>
        /// Unique building identifier number
        /// Maps to: _number_k__BackingField
        /// </summary>
        public int Number
        {
            get => SafeInvoke<int?>("get_number") ?? 0;
        }
        
        /// <summary>
        /// Building type definition (factory, hab, etc.)
        /// Maps to: _buildingType field
        /// </summary>
        public object? BuildingType
        {
            get => SafeInvoke<object>("get_buildingType");
        }
        
        /// <summary>
        /// Building name from its type
        /// Elegant wrapper around BuildingType.name access
        /// </summary>
        public string BuildingTypeName
        {
            get
            {
                try
                {
                    return BuildingType?.GetFieldValue<string>("name") ?? "Unknown";
                }
                catch
                {
                    return "Unknown";
                }
            }
        }
        
        // ==================== BASIC PROPERTIES ====================
        
        /// <summary>
        /// Building type key (e.g., "SolarPanel", "Mine")
        /// </summary>
        public string TypeKey
        {
            get
            {
                var buildingType = SafeInvoke<object>("get_buildingType");
                return buildingType?.InvokeMethod<string>("get_key") ?? "Unknown";
            }
        }
        
        /// <summary>
        /// 2D position on the planet surface
        /// </summary>
        public (float x, float y) Position
        {
            get
            {
                var pos = SafeInvoke<object>("get_position");
                if (pos == null) return (0, 0);
                
                float x = pos.GetFieldValue<float>("x");
                float y = pos.GetFieldValue<float>("y");
                return (x, y);
            }
        }
        
        /// <summary>
        /// Is the building alive (not destroyed)
        /// </summary>
        public bool IsAlive => SafeGetField<bool>("_alive");
        
        /// <summary>
        /// Is the building operational
        /// </summary>
        public bool IsOperative => SafeGetField<bool>("_activated");
        
        /// <summary>
        /// Is the building fully built (not under construction)
        /// </summary>
        public bool IsBuilt => SafeGetField<bool>("_built");
        
        /// <summary>
        /// Is the building powered
        /// </summary>
        public bool IsPowered => SafeGetField<bool>("_powered");
        
        /// <summary>
        /// Is the building broken
        /// </summary>
        public bool IsBroken => SafeGetField<bool>("_broken");
        
        /// <summary>
        /// Current health (0.0 to 1.0)
        /// </summary>
        public float Health => SafeGetField<float>("_health");
        
        // ==================== FACTION ====================
        
        /// <summary>
        /// Get the faction that owns this building
        /// </summary>
        public object? GetFaction()
        {
            return SafeInvoke<object>("get_faction");
        }
        
        // ==================== STOCKPILE ====================
        
        /// <summary>
        /// Get the building's stockpile
        /// </summary>
        public object? GetStockpile()
        {
            return SafeInvoke<object>("GetStockpile");
        }
        
        
        /// <summary>
        /// Get resource quantity in stockpile
        /// </summary>
        /// <param name="resourceKey">Resource key (e.g., "resource_water")</param>
        /// <returns>Current quantity in stockpile or 0 if not found</returns>
        public float GetResourceStock(string resourceKey)
        {
            var stockpile = GetStockpile();
            if (stockpile == null) return 0f;
            
            // Get resource type
            var resourceType = KeeperTypeRegistry.GetResourceType(resourceKey);
            if (resourceType == null) return 0f;
            
            var cargoQty = stockpile.InvokeMethod<object>("get_Item", resourceType);
            return cargoQty?.GetFieldValue<float>("quantity") ?? 0f;
        }
        
        // ==================== ACTIONS ====================
        
        /// <summary>
        /// Toggle operative state
        /// </summary>
        public void ToggleOperative()
        {
            SafeInvokeVoid("ToggleOperative");
        }
        
        /// <summary>
        /// Start scrapping the building
        /// </summary>
        public void StartScrapping()
        {
            SafeInvokeVoid("StartScrapping");
        }
        
        /// <summary>
        /// Cancel scrapping
        /// </summary>
        public void CancelScrapping()
        {
            SafeInvokeVoid("CancelScrapping");
        }
        
        // ==================== INFO ====================
        
        /// <summary>
        /// Returns detailed building status as formatted string
        /// </summary>
        /// <returns>Building status with core properties</returns>
        public override string ToString()
        {
            try
            {
                var position = Position;
                var positionStr = $"({position.x:F1}, {position.y:F1})";
                
                return $"Building #{Number}: {TypeKey} at {positionStr} - " +
                       $"Alive: {IsAlive}, Built: {IsBuilt}, Operative: {IsOperative}";
            }
            catch
            {
                return $"Building #{Number}: {TypeKey} - Status unavailable";
            }
        }
    }
}
