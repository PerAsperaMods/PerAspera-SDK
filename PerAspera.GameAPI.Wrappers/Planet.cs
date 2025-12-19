#nullable enable
using System;
using System.Collections.Generic;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Native;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native Planet class
    /// Provides safe access to planetary properties and climate data
    /// </summary>
    public class Planet : WrapperBase
    {
        private Atmosphere? _atmosphere;
        
        /// <summary>
        /// Initialize Planet wrapper with native planet object
        /// </summary>
        /// <param name="nativePlanet">Native planet instance from game</param>
        public Planet(object nativePlanet) : base(nativePlanet)
        {
        }
        
        /// <summary>
        /// Get the current planet instance
        /// </summary>
        public static Planet? GetCurrent()
        {
            var planet = KeeperTypeRegistry.GetPlanet();
            return planet != null ? new Planet(planet) : null;
        }
        
        // ==================== PLANET IDENTITY ====================
        
        /// <summary>
        /// Planet name - Hardcoded to "Mars" for Per Aspera (future extensibility for multi-planet mods)
        /// </summary>
        public string Name => "Mars";
        
        // ==================== ATMOSPHERE ====================
        
        /// <summary>
        /// Planet atmosphere (composition, temperature, pressure, effects)
        /// Access via: Planet.Atmosphere.Composition["CO2"].PartialPressure
        /// </summary>
        public Atmosphere Atmosphere
        {
            get
            {
                if (_atmosphere == null && NativeObject != null)
                    _atmosphere = new Atmosphere(NativeObject);
                return _atmosphere ?? throw new InvalidOperationException("Planet not initialized");
            }
        }
        


        // ==================== RESOURCES ====================
        
        /// <summary>
        /// Water stock on the planet (direct access for convenience)
        /// Maps to: GetWaterStock() / SetWaterStock() methods
        /// </summary>
        public float WaterStock
        {
            get => SafeInvoke<float?>("GetWaterStock") ?? 0f;
            set => SafeInvokeVoid("SetWaterStock", value);
        }
        
        /// <summary>
        /// Silicon reserves on the planet
        /// Maps to: GetResourceStock("resource_silicon")
        /// </summary>
        public float SiliconStock
        {
            get => GetResourceStock("resource_silicon");
        }
        
        /// <summary>
        /// Iron ore reserves on the planet
        /// Maps to: GetResourceStock("resource_iron")
        /// </summary>
        public float IronStock
        {
            get => GetResourceStock("resource_iron");
        }
        
        /// <summary>
        /// Carbon reserves on the planet
        /// Maps to: GetResourceStock("resource_carbon")
        /// </summary>
        public float CarbonStock
        {
            get => GetResourceStock("resource_carbon");
        }
        
        /// <summary>
        /// Calcite (limestone) reserves on the planet
        /// Maps to: GetResourceStock("resource_calcite")
        /// </summary>
        public float CalciteStock
        {
            get => GetResourceStock("resource_calcite");
        }
        
        /// <summary>
        /// Check if planet has sufficient water for operations
        /// Elegant wrapper around water stock checking
        /// </summary>
        public bool HasSufficientWater => WaterStock > 1000f;
        
        /// <summary>
        /// Check if planet has balanced resource reserves
        /// Combines multiple resource checks in elegant property
        /// </summary>
        public bool HasBalancedResources
        {
            get
            {
                return WaterStock > 500f &&
                       SiliconStock > 100f &&
                       IronStock > 100f &&
                       CarbonStock > 50f;
            }
        }
        
        
        // ==================== RESOURCE MANAGEMENT ====================
        
        /// <summary>
        /// Get stock of a specific resource with elegant error handling
        /// Maps to: Complex resource lookup via KeeperTypeRegistry
        /// </summary>
        /// <param name="resourceKey">Resource key (e.g., "resource_water", "resource_silicon")</param>
        /// <returns>Current stock amount or 0 if resource not found</returns>
        public float GetResourceStock(string resourceKey)
        {
            try
            {
                var resourceType = KeeperTypeRegistry.GetResourceType(resourceKey);
                if (resourceType == null) 
                {
                    Log.Warning($"Resource type not found: {resourceKey}");
                    return 0f;
                }
                
                return SafeInvoke<float?>("GetResourceStock", resourceType) ?? 0f;
            }
            catch (System.Exception ex)
            {
                Log.Warning($"Failed to get resource stock for {resourceKey}: {ex.Message}");
                return 0f;
            }
        }
        
        /// <summary>
        /// Add resource to planet stock with elegant validation
        /// Maps to: Complex resource addition via KeeperTypeRegistry
        /// </summary>
        /// <param name="resourceKey">Resource key (e.g., "resource_water", "resource_silicon")</param>
        /// <param name="amount">Amount to add (can be negative to remove)</param>
        /// <returns>True if operation succeeded</returns>
        public bool AddResource(string resourceKey, float amount)
        {
            try
            {
                var resourceType = KeeperTypeRegistry.GetResourceType(resourceKey);
                if (resourceType == null)
                {
                    Log.Warning($"Cannot add resource - type not found: {resourceKey}");
                    return false;
                }
                
                SafeInvokeVoid("AddResource", resourceType, amount);
                Log.Debug($"Added {amount} of {resourceKey} to planet");
                return true;
            }
            catch (System.Exception ex)
            {
                Log.Error($"Failed to add resource {resourceKey}: {ex.Message}");
                return false;
            }
        }
        
        // ==================== INFO ====================
        
        // ==================== BUILDING MANAGEMENT ====================
        
        /// <summary>
        /// Get all buildings on this planet
        /// Maps to: buildings field or GetBuildings() method
        /// </summary>
        /// <returns>List of all buildings on the planet</returns>
        public List<Building> GetBuildings()
        {
            try
            {
                var nativeBuildings = SafeInvoke<object>("get_buildings") ?? 
                                    SafeInvoke<object>("GetBuildings");
                
                if (nativeBuildings == null) return new List<Building>();
                
                var buildingWrappers = new List<Building>();
                var enumerable = nativeBuildings as System.Collections.IEnumerable;
                if (enumerable != null)
                {
                    foreach (var building in enumerable)
                    {
                        if (building != null)
                        {
                            buildingWrappers.Add(new Building(building));
                        }
                    }
                }
                
                return buildingWrappers;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get buildings for planet: {ex.Message}");
                return new List<Building>();
            }
        }
        
        /// <summary>
        /// Get all buildings owned by a specific faction on this planet
        /// </summary>
        /// <param name="faction">Faction to filter by</param>
        /// <returns>List of buildings owned by the faction</returns>
        public List<Building> GetBuildingsByFaction(Faction faction)
        {
            if (!faction.IsValid) return new List<Building>();
            
            try
            {
                // Get all buildings and filter by faction
                var allBuildings = GetBuildings();
                var factionBuildings = new List<Building>();
                
                foreach (var building in allBuildings)
                {
                    try
                    {
                        var buildingFaction = building.GetFaction();
                        if (buildingFaction != null && AreSameFaction(buildingFaction, faction.GetNativeObject()))
                        {
                            factionBuildings.Add(building);
                        }
                    }
                    catch
                    {
                        // Skip buildings that can't be checked
                        continue;
                    }
                }
                
                return factionBuildings;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get buildings for faction {faction.Name}: {ex.Message}");
                return new List<Building>();
            }
        }
        
        /// <summary>
        /// Helper method to compare if two faction objects are the same
        /// </summary>
        private bool AreSameFaction(object faction1, object? faction2)
        {
            if (faction1 == null || faction2 == null) return false;
            
            try
            {
                // Try comparing by reference first
                if (ReferenceEquals(faction1, faction2)) return true;
                
                // Try comparing by faction name/id
                var name1 = faction1.GetFieldValue<string>("name") ?? 
                           faction1.InvokeMethod<string>("get_name");
                var name2 = faction2.GetFieldValue<string>("name") ?? 
                           faction2.InvokeMethod<string>("get_name");
                
                return !string.IsNullOrEmpty(name1) && name1 == name2;
            }
            catch
            {
                return false;
            }
        }
        
        // ==================== INFO ====================
        
        /// <summary>
        /// Get the native game object (for Harmony patches)
        /// </summary>
        /// <returns>Native planet object or null</returns>
        public object? GetNativeObject() => NativeObject;
        
        /// <summary>
        /// Returns detailed planet status as formatted string
        /// </summary>
        /// <returns>Planet status with atmosphere and resource information</returns>
        public override string ToString()
        {
            if (NativeObject == null)
                return "Planet: Not initialized";
                
            try
            {
                var atmo = Atmosphere;
                var co2 = atmo?.Composition?["CO2"]?.PartialPressure ?? 0f;
                var o2 = atmo?.Composition?["O2"]?.PartialPressure ?? 0f;
                var n2 = atmo?.Composition?["N2"]?.PartialPressure ?? 0f;
                
                return $"Planet: Temp={atmo?.Temperature:F1}K, Pressure={atmo?.TotalPressure:F2}kPa " +
                       $"(CO2:{co2:F2}, O2:{o2:F2}, N2:{n2:F2}), " +
                       $"Water:{WaterStock:F1}, Breathable:{atmo?.IsBreathable}";
            }
            catch
            {
                return "Planet: Atmosphere data unavailable";
            }
        }
    }
}
