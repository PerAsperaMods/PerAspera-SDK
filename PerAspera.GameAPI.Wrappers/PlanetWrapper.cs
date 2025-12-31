#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
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
        //private Atmosphere? _atmosphere;
        //private PerAspera.GameAPI.Climate.Atmosphere? atmosphereGrid;
        private Native.Planet? _nativePlanet;

        /// <summary>
        /// Initialize Planet wrapper with native planet object
        /// </summary>
        /// <param name="nativePlanet">Native planet instance from game</param>
        /// 

        //
        //AREA



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
        
        // ==================== CLIMATE DATA ====================
        
        /// <summary>
        /// DEPRECATED: For atmospheric data, use PerAspera.GameAPI.Climate directly.
        /// Example: ClimateController.Instance?.Atmosphere?.AverageTemperature ?? GetTemperatureFallback()
        /// </summary>
        [Obsolete("Use PerAspera.GameAPI.Climate.ClimateController for atmospheric data. This method will be removed in v2.0", false)]
        public float GetTemperature()
        {
            // Fallback to native planet data only
            try
            {
                if (_nativePlanet != null)
                {
                    return (float)_nativePlanet.NativeInstance.GetMemberValue("temperature");
                }
                return SafeInvoke<float>("GetTemperature");
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to get temperature: {ex.Message}");
                return 0.0f;
            }
        }
        
        /// <summary>
        /// DEPRECATED: For atmospheric data, use PerAspera.GameAPI.Climate directly.
        /// Example: ClimateController.Instance?.Atmosphere?.TotalPressure ?? GetAtmosphericPressureFallback()
        /// </summary>
        [Obsolete("Use PerAspera.GameAPI.Climate.ClimateController for atmospheric data. This method will be removed in v2.0", false)]
        public float GetAtmosphericPressure()
        {
            // Fallback to native planet data only
            try
            {
                if (_nativePlanet != null)
                {
                    return (float)_nativePlanet.NativeInstance.GetMemberValue("atmosphericPressure");
                }
                return SafeInvoke<float>("GetAtmosphericPressure");
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to get atmospheric pressure: {ex.Message}");
                return 0.0f;
            }
        }
        
        /// <summary>
        /// DEPRECATED: For atmospheric data, use PerAspera.GameAPI.Climate directly.
        /// Example: ClimateController.Instance?.Atmosphere?.OxygenLevel ?? GetOxygenLevelFallback()
        /// </summary>
        [Obsolete("Use PerAspera.GameAPI.Climate.ClimateController for atmospheric data. This method will be removed in v2.0", false)]
        public float GetOxygenLevel()
        {
            // Fallback to native planet data only
            try
            {
                if (_nativePlanet != null)
                {
                    return (float)_nativePlanet.NativeInstance.GetMemberValue("oxygenLevel");
                }
                return SafeInvoke<float>("GetOxygenLevel");
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to get oxygen level: {ex.Message}");
                return 0.0f;
            }
        }
        
        // ==================== BUILDINGS ====================
        
        /// <summary>
        /// Get all buildings on the planet safely (null-checked)
        /// </summary>
        public BuildingWrapper[] GetBuildingsSafely()
        {
            try
            {
                var buildingsList = GetBuildings();
                return buildingsList?.ToArray() ?? new BuildingWrapper[0];
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to get buildings safely: {ex.Message}");
                return new BuildingWrapper[0];
            }
        }

        // ==================== ENHANCED ATMOSPHERE API (SDK CLIMATE INTEGRATION) ====================
        
        /// <summary>
        /// For atmospheric data, use PerAspera.GameAPI.Climate directly:
        /// var climate = ClimateController.Instance?.Atmosphere;
        /// This avoids circular dependencies and improves performance.
        /// </summary>
        [Obsolete("Use PerAspera.GameAPI.Climate.ClimateController.Instance.Atmosphere directly for better performance", false)]
        public object? Atmosphere => null;
        
        // ==================== PLANET IDENTITY ====================
        
        /// <summary>
        /// Planet name - Hardcoded to "Mars" for Per Aspera (future extensibility for multi-planet mods)
        /// </summary>
        public string Name => "Mars";
        
        // ==================== ATMOSPHERE ====================
        
        /// <summary>
        /// Planet atmosphere (composition, temperature, pressure, effects)
        /// Access via: Planet.Atmosphere.Composition["CO2"].PartialPressure

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
        
        // ==================== ATMOSPHERIC GAS MANAGEMENT ====================
        
        /// <summary>
        /// Map resource key to gas ID used by Atmosphere.ModifyGas
        /// </summary>
        /// <param name="resourceKey">Resource key (e.g., "resource_nitrogen_release")</param>
        /// <returns>Gas ID (e.g., "N2") or null if not mappable</returns>
        private string? MapResourceKeyToGasId(string resourceKey)
        {
            return resourceKey switch
            {
                "resource_nitrogen_release" => "N2",
                "resource_oxygen_release" => "O2", 
                "resource_carbon_dioxide_release" => "CO2",
                "resource_ghg_release" => "GHG",
                // Add more mappings as needed for other atmospheric resources
                _ => null
            };
        }
        
        /// <summary>
        /// Increase nitrogen (N2) atmospheric pressure
        /// Wrapper for atmosphere gas modification with proper resource key mapping
        /// </summary>
        /// <param name="pressure">Pressure increase in kPa</param>
        /*public void IncreaseN2(float pressure)
        {
            IncreaseGasPressure("resource_nitrogen_release", pressure);
        }* /
        
        /// <summary>
        /// Increase oxygen (O2) atmospheric pressure
        /// Wrapper for atmosphere gas modification with proper resource key mapping
        /// </summary>
        /// <param name="pressure">Pressure increase in kPa</param>
        /*public void IncreaseO2(float pressure)
        {
            IncreaseGasPressure("resource_oxygen_release", pressure);
        }* /
        
        /// <summary>
        /// Increase carbon dioxide (CO2) atmospheric pressure
        /// Wrapper for atmosphere gas modification with proper resource key mapping
        /// </summary>
        /// <param name="pressure">Pressure increase in kPa</param>
        /*public void IncreaseCO2(float pressure)
        {
            IncreaseGasPressure("resource_carbon_dioxide_release", pressure);
        }* /
        
        /// <summary>
        /// Increase greenhouse gases (GHG) atmospheric pressure
        /// Wrapper for atmosphere gas modification with proper resource key mapping
        /// </summary>
        /// <param name="pressure">Pressure increase in kPa</param>
        /*public void IncreaseGHG(float pressure)
        {
            IncreaseGasPressure("resource_ghg_release", pressure);
        }* /
        
        /// <summary>
        /// Generic method to increase atmospheric gas pressure
        /// This is the core implementation called by all specific gas methods
        /// </summary>
        /// <param name="resourceKey">Resource key (e.g., "resource_nitrogen_release")</param>
        /// <param name="pressure">Pressure increase in kPa</param>
        /*public void IncreaseGasPressure(string resourceKey, float pressure)
        {
            try
            {
                var atmosphere = Atmosphere;
                if (atmosphere == null)
                {
                    Log.LogWarning($"Cannot increase {resourceKey} pressure: atmosphere not available");
                    return;
                }
                
                // Map resource key to gas ID used by Atmosphere.ModifyGas
                var gasId = MapResourceKeyToGasId(resourceKey);
                if (gasId == null)
                {
                    Log.LogWarning($"Cannot map resource key {resourceKey} to gas ID");
                    return;
                }
                
                // Use the atmosphere's ModifyGas method
                atmosphere.ModifyGas(gasId, pressure);
                
                // Log the change with climate properties if available
                var resource = ResourceTypeWrapper.GetByKey(resourceKey);
                var climateProps = resource?.GetClimateProperties();
                if (climateProps != null)
                {
                    Log.LogInfo($"Increased {climateProps.GasSymbol} pressure by {pressure:F2}kPa " +
                               $"(GWP: {climateProps.GreenhousePotential:F1})");
                }
                else
                {
                    Log.LogInfo($"Increased {resourceKey} pressure by {pressure:F2}kPa");
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to increase {resourceKey} pressure: {ex.Message}");
            }
        }* /
        
        /// <summary>
        /// Decrease atmospheric gas pressure
        /// </summary>
        /// <param name="resourceKey">Resource key</param>
        /// <param name="pressure">Pressure decrease in kPa</param>
        /*public void DecreaseGasPressure(string resourceKey, float pressure)
        {
            try
            {
                var atmosphere = Atmosphere;
                if (atmosphere == null)
                {
                    Log.LogWarning($"Cannot decrease {resourceKey} pressure: atmosphere not available");
                    return;
                }
                
                // Map resource key to gas ID used by Atmosphere.ModifyGas
                var gasId = MapResourceKeyToGasId(resourceKey);
                if (gasId == null)
                {
                    Log.LogWarning($"Cannot map resource key {resourceKey} to gas ID");
                    return;
                }
                
                // Use negative amount for decrease
                atmosphere.ModifyGas(gasId, -pressure);
                
                var resource = ResourceTypeWrapper.GetByKey(resourceKey);
                var climateProps = resource?.GetClimateProperties();
                if (climateProps != null)
                {
                    Log.LogInfo($"Decreased {climateProps.GasSymbol} pressure by {pressure:F2}kPa");
                }
                else
                {
                    Log.LogInfo($"Decreased {resourceKey} pressure by {pressure:F2}kPa");
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to decrease {resourceKey} pressure: {ex.Message}");
            }
        }* /
        
        /// <summary>
        /// Get current pressure of a specific atmospheric gas
        /// </summary>
        /// <param name="resourceKey">Resource key</param>
        /// <returns>Current pressure in kPa, or 0 if not found</returns>
        /*public float GetGasPressure(string resourceKey)
        {
            try
            {
                return Atmosphere?.GetGasQuantity(resourceKey) ?? 0f;
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to get {resourceKey} pressure: {ex.Message}");
                return 0f;
            }
        }* /
        
        /// <summary>
        /// Calculate greenhouse effect based on current atmospheric composition
        /// Uses climate properties from loaded configuration
        /// </summary>
        /// <returns>Greenhouse effect multiplier (1.0 = no effect)</returns>
        public float CalculateGreenhouseEffect()
        {
            try
            {
                float totalEffect = 1.0f; // Base level (no greenhouse effect)
                
                // Get all atmospheric gases and calculate their contribution
                var atmosphere = Atmosphere;
                if (atmosphere != null)
                {
                    // Known atmospheric gas resource keys
                    var atmosphericGasKeys = new[]
                    {
                        "resource_carbon_dioxide_release",
                        "resource_oxygen_release", 
                        "resource_nitrogen_release",
                        "resource_ghg_release"
                    };
                    
                    foreach (var gasKey in atmosphericGasKeys)
                    {
                        float pressure = atmosphere.GetGasQuantity(gasKey);
                        if (pressure > 0)
                        {
                            var resource = ResourceTypeWrapper.GetByKey(gasKey);
                            var climateProps = resource?.GetClimateProperties();
                            
                            if (climateProps != null && climateProps.GreenhousePotential > 0)
                            {
                                // Effect = pressure * GWP (simplified model)
                                float effect = pressure * (float)climateProps.GreenhousePotential * 0.001f;
                                totalEffect += effect;
                            }
                        }
                    }
                }
                
                return totalEffect;
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to calculate greenhouse effect: {ex.Message}");
                return 1.0f;
            }
        }
        
        /// <summary>
        /// Check if atmosphere is breathable based on climate configuration
        /// </summary>
        /// <returns>True if atmosphere meets breathability criteria</returns>
        /*public bool IsAtmosphereBreathable()
        {
            try
            {
                var atmosphere = Atmosphere;
                if (atmosphere == null) return false;
                
                // Check basic requirements
                if (atmosphere.TotalPressure < 90 || atmosphere.TotalPressure > 110) return false;
                if (atmosphere.TemperatureCelsius < -10 || atmosphere.TemperatureCelsius > 45) return false;
                
                // Check gas composition using climate config
                float o2Pressure = GetGasPressure("resource_oxygen_release");
                float co2Pressure = GetGasPressure("resource_carbon_dioxide_release");
                
                // Oxygen between 19.5% and 23.5% of atmosphere
                float o2Percentage = (o2Pressure / atmosphere.TotalPressure) * 100;
                if (o2Percentage < 19.5f || o2Percentage > 23.5f) return false;
                
                // CO2 less than 0.5% of atmosphere
                float co2Percentage = (co2Pressure / atmosphere.TotalPressure) * 100;
                if (co2Percentage > 0.5f) return false;
                
                return true;
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to check atmosphere breathability: {ex.Message}");
                return false;
            }
        } */
        
        /// <summary>
        /// Returns detailed planet status as formatted string
        /// </summary>
        /// <returns>Planet status with atmosphere and resource information</returns>
        /*public override string ToString()
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
        } */

        /// <summary>
        /// Simple atmosphere wrapper for backward compatibility
        /// Provides basic atmospheric data access
        /// </summary>
    }
}
