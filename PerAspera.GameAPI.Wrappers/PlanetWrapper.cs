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
    /// 
    /// 📖 Enhanced Documentation: F:\ModPeraspera\SDK-Enhanced-Classes\Planet-Enhanced.md
    /// 🤖 Agent Expert: @per-aspera-sdk-coordinator 
    /// 🌐 User Wiki: https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/sdk/
    /// 🔧 Gap Analysis: F:\ModPeraspera\SDK-Enhanced-Classes\Capabilities-Matrix.md
    /// 🎯 Examples: F:\ModPeraspera\Individual-Mods\MasterGui2\PlanetWrapper usage
    /// </summary>
    public class PlanetWrapper : WrapperBase
    {
        private Atmosphere? _atmosphere;
        private Native.Planet? _nativePlanet;

        /// <summary>
        /// Initialize Planet wrapper with native planet object
        /// </summary>
        /// <param name="nativePlanet">Native planet instance from game</param>
        /// 

        public PlanetWrapper(object nativePlanet) : base(nativePlanet)
        {
            // Initialize native planet reference for direct access
            try
            {
                _nativePlanet = new Native.Planet(nativePlanet);
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to initialize native planet reference: {ex.Message}");
            }
        }

        public HazardsManagerWrapper GetHazardsManager()
        {
            return new HazardsManagerWrapper(GetNativeObject().GetMemberValue<object>("HazardsManager"));
        }

        

        /// <summary>
        /// Get the current planet instance
        /// </summary>
        public static PlanetWrapper? GetCurrent()
        {
            var planet = KeeperTypeRegistry.GetPlanet();
            return planet != null ? new PlanetWrapper(planet) : null;
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
            get
            {
                try
                {
                    // Try direct access first (100x faster)
                    if (_nativePlanet?.NativeInstance != null)
                    {
                        return (float)_nativePlanet.NativeInstance.GetMemberValue("waterStock");
                    }
                }
                catch (Exception ex)
                {
                    Log.LogDebug($"Direct waterStock access failed, falling back to reflection: {ex.Message}");
                }
                
                // Fallback to reflection
                return SafeInvoke<float?>("GetWaterStock") ?? 0f;
            }
            set
            {
                try
                {
                    // Try direct access first
                    if (_nativePlanet?.NativeInstance != null)
                    {
                        _nativePlanet.NativeInstance.SetMemberValue("waterStock", value);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Log.LogDebug($"Direct waterStock set failed, falling back to reflection: {ex.Message}");
                }
                
                // Fallback to reflection
                SafeInvokeVoid("SetWaterStock", value);
            }
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
                    Log.LogWarning($"Resource type not found: {resourceKey}");
                    return 0f;
                }
                
                try
                {
                    // Try direct access first
                    if (_nativePlanet?.NativeInstance != null)
                    {
                        // For methods that return values, we need to use reflection
                        // Direct IL2CPP access for return values is complex, so we use SafeInvoke for now
                        // TODO: Optimize this with direct IL2CPP method calls when possible
                        return SafeInvoke<float?>("GetResourceStock", resourceType) ?? 0f;
                    }
                }
                catch (Exception ex)
                {
                    Log.LogDebug($"Direct GetResourceStock access failed, falling back to reflection: {ex.Message}");
                }
                
                // Fallback to reflection
                return SafeInvoke<float?>("GetResourceStock", resourceType) ?? 0f;
            }
            catch (System.Exception ex)
            {
                Log.LogWarning($"Failed to get resource stock for {resourceKey}: {ex.Message}");
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
                    Log.LogWarning($"Cannot add resource - type not found: {resourceKey}");
                    return false;
                }
                
                try
                {
                    // Try direct access first
                    if (_nativePlanet?.NativeInstance != null)
                    {
                        // For methods that return values, we need to use reflection
                        // Direct IL2CPP access for return values is complex, so we use SafeInvoke for now
                        // TODO: Optimize this with direct IL2CPP method calls when possible
                        SafeInvokeVoid("AddResource", resourceType, amount);
                        Log.LogDebug($"Added {amount} of {resourceKey} to planet");
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Log.LogDebug($"Direct AddResource access failed, falling back to reflection: {ex.Message}");
                }
                
                // Fallback to reflection
                SafeInvokeVoid("AddResource", resourceType, amount);
                Log.LogDebug($"Added {amount} of {resourceKey} to planet");
                return true;
            }
            catch (System.Exception ex)
            {
                Log.LogError($"Failed to add resource {resourceKey}: {ex.Message}");
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
        public List<BuildingWrapper> GetBuildings()
        {
            try
            {
                object? nativeBuildings = null;
                
                try
                {
                    // Try direct access first - check for buildings field
                    if (_nativePlanet?.NativeInstance != null)
                    {
                        nativeBuildings = _nativePlanet.NativeInstance.GetMemberValue("buildings");
                        if (nativeBuildings == null)
                        {
                            // Try GetBuildings method
                            nativeBuildings = _nativePlanet.NativeInstance.InvokeMethod("GetBuildings");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.LogDebug($"Direct buildings access failed, falling back to reflection: {ex.Message}");
                }
                
                // Fallback to reflection
                if (nativeBuildings == null)
                {
                    nativeBuildings = SafeInvoke<object>("get_buildings") ?? 
                                    SafeInvoke<object>("GetBuildings");
                }
                
                if (nativeBuildings == null) return new List<BuildingWrapper>();
                
                var buildingWrappers = new List<BuildingWrapper>();
                var enumerable = nativeBuildings as System.Collections.IEnumerable;
                if (enumerable != null)
                {
                    foreach (var building in enumerable)
                    {
                        if (building != null)
                        {
                            buildingWrappers.Add(new BuildingWrapper(building));
                        }
                    }
                }
                
                return buildingWrappers;
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get buildings for planet: {ex.Message}");
                return new List<BuildingWrapper>();
            }
        }
        
        /// <summary>
        /// Get all buildings owned by a specific faction on this planet
        /// </summary>
        /// <param name="faction">Faction to filter by</param>
        /// <returns>List of buildings owned by the faction</returns>
        public List<BuildingWrapper> GetBuildingsByFaction(FactionWrapper faction)
        {
            if (!faction.IsValidWrapper) return new List<BuildingWrapper>();
            
            try
            {
                // Get all buildings and filter by faction
                var allBuildings = GetBuildings();
                var factionBuildings = new List<BuildingWrapper>();
                
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
                Log.LogError($"Failed to get buildings for faction {faction.Name}: {ex.Message}");
                return new List<BuildingWrapper>();
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
