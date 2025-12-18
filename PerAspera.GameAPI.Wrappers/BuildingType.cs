#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Native;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native BuildingType class
    /// Provides safe access to building type definitions and properties
    /// DOC: BuildingType.md - Building definitions and construction properties
    /// </summary>
    public class BuildingType : WrapperBase
    {
        /// <summary>
        /// Initialize BuildingType wrapper with native building type object
        /// </summary>
        /// <param name="nativeBuildingType">Native building type instance from game</param>
        public BuildingType(object nativeBuildingType) : base(nativeBuildingType)
        {
        }
        
        /// <summary>
        /// Create wrapper from native building type object
        /// </summary>
        public static BuildingType? FromNative(object? nativeBuildingType)
        {
            return nativeBuildingType != null ? new BuildingType(nativeBuildingType) : null;
        }
        
        // ==================== CORE IDENTIFICATION ====================
        
        /// <summary>
        /// Building type name/key identifier
        /// Maps to: name field (e.g., "building_solar_panel", "building_water_mine")
        /// </summary>
        public string Name
        {
            get => SafeInvoke<string>("get_name") ?? "unknown_building";
        }
        
        /// <summary>
        /// Building display name for UI
        /// Maps to: displayName or localizedName field
        /// </summary>
        public string DisplayName
        {
            get => SafeInvoke<string>("get_displayName") ?? 
                   SafeInvoke<string>("get_localizedName") ?? 
                   SafeInvoke<string>("get_title") ?? Name;
        }
        
        /// <summary>
        /// Building description
        /// Maps to: description field
        /// </summary>
        public string Description
        {
            get => SafeInvoke<string>("get_description") ?? 
                   SafeInvoke<string>("get_content") ?? "No description available";
        }
        
        /// <summary>
        /// Building index for efficient lookups
        /// Maps to: index field
        /// </summary>
        public int Index
        {
            get => SafeInvoke<int?>("get_index") ?? -1;
        }
        
        // ==================== BUILDING CATEGORY ====================
        
        /// <summary>
        /// Building category (Power, Mining, Manufacturing, etc.)
        /// Maps to: category or buildingCategory field
        /// </summary>
        public string Category
        {
            get => SafeInvoke<string>("get_category") ?? 
                   SafeInvoke<string>("get_buildingCategory") ?? 
                   SafeInvoke<string>("get_type") ?? "General";
        }
        
        /// <summary>
        /// Building subcategory for detailed organization
        /// Maps to: subCategory field
        /// </summary>
        public string SubCategory
        {
            get => SafeInvoke<string>("get_subCategory") ?? 
                   SafeInvoke<string>("get_subType") ?? "";
        }
        
        /// <summary>
        /// Check if this is a power generation building
        /// </summary>
        public bool IsPowerBuilding()
        {
            return Category.Contains("Power", StringComparison.OrdinalIgnoreCase) ||
                   Name.Contains("power", StringComparison.OrdinalIgnoreCase) ||
                   Name.Contains("solar", StringComparison.OrdinalIgnoreCase) ||
                   Name.Contains("nuclear", StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Check if this is a resource extraction building
        /// </summary>
        public bool IsMiningBuilding()
        {
            return Category.Contains("Mining", StringComparison.OrdinalIgnoreCase) ||
                   Name.Contains("mine", StringComparison.OrdinalIgnoreCase) ||
                   Name.Contains("extractor", StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Check if this is a manufacturing building
        /// </summary>
        public bool IsManufacturingBuilding()
        {
            return Category.Contains("Manufacturing", StringComparison.OrdinalIgnoreCase) ||
                   Name.Contains("factory", StringComparison.OrdinalIgnoreCase) ||
                   Name.Contains("plant", StringComparison.OrdinalIgnoreCase);
        }
        
        // ==================== CONSTRUCTION PROPERTIES ====================
        
        /// <summary>
        /// Construction cost in resources
        /// Maps to: constructionCost or cost field
        /// </summary>
        public float ConstructionCost
        {
            get => SafeInvoke<float?>("get_constructionCost") ?? 
                   SafeInvoke<float?>("get_cost") ?? 0f;
        }
        
        /// <summary>
        /// Construction time in game time units
        /// Maps to: constructionTime or buildTime field
        /// </summary>
        public float ConstructionTime
        {
            get => SafeInvoke<float?>("get_constructionTime") ?? 
                   SafeInvoke<float?>("get_buildTime") ?? 0f;
        }
        
        /// <summary>
        /// Required resources for construction
        /// Maps to: requiredResources array
        /// </summary>
        public Dictionary<string, float> GetConstructionResources()
        {
            try
            {
                var resources = SafeInvoke<object>("get_requiredResources") ?? 
                              SafeInvoke<object>("get_constructionResources") ??
                              SafeInvoke<object>("get_materials");
                
                var resourceDict = new Dictionary<string, float>();
                
                if (resources is System.Collections.IDictionary dictionary)
                {
                    foreach (var key in dictionary.Keys)
                    {
                        var value = dictionary[key];
                        if (key != null && value != null)
                        {
                            resourceDict[key.ToString()] = Convert.ToSingle(value);
                        }
                    }
                }
                else if (resources is System.Collections.IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        if (item != null)
                        {
                            var resourceType = SafeInvoke<string>("get_resourceType", item) ?? 
                                             SafeInvoke<string>("get_name", item) ?? "unknown";
                            var amount = SafeInvoke<float?>("get_amount", item) ?? 
                                       SafeInvoke<float?>("get_quantity", item) ?? 0f;
                            resourceDict[resourceType] = amount;
                        }
                    }
                }
                
                return resourceDict;
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to get construction resources for building {Name}: {ex.Message}");
                return new Dictionary<string, float>();
            }
        }
        
        /// <summary>
        /// Maintenance cost per time unit
        /// Maps to: maintenanceCost field
        /// </summary>
        public float MaintenanceCost
        {
            get => SafeInvoke<float?>("get_maintenanceCost") ?? 
                   SafeInvoke<float?>("get_upkeepCost") ?? 0f;
        }
        
        // ==================== PRODUCTION PROPERTIES ====================
        
        /// <summary>
        /// Base energy production (for power buildings)
        /// Maps to: baseEnergyOutput or energyProduction field
        /// </summary>
        public float BaseEnergyOutput
        {
            get => SafeInvoke<float?>("get_baseEnergyOutput") ?? 
                   SafeInvoke<float?>("get_energyProduction") ?? 
                   SafeInvoke<float?>("get_powerOutput") ?? 0f;
        }
        
        /// <summary>
        /// Energy consumption (for non-power buildings)
        /// Maps to: energyConsumption or powerConsumption field
        /// </summary>
        public float EnergyConsumption
        {
            get => SafeInvoke<float?>("get_energyConsumption") ?? 
                   SafeInvoke<float?>("get_powerConsumption") ?? 
                   SafeInvoke<float?>("get_energyRequired") ?? 0f;
        }
        
        /// <summary>
        /// Base resource production rates
        /// Maps to: productionRates or outputs field
        /// </summary>
        public Dictionary<string, float> GetProductionRates()
        {
            try
            {
                var production = SafeInvoke<object>("get_productionRates") ?? 
                               SafeInvoke<object>("get_outputs") ??
                               SafeInvoke<object>("get_produces");
                
                var productionDict = new Dictionary<string, float>();
                
                if (production is System.Collections.IDictionary dictionary)
                {
                    foreach (var key in dictionary.Keys)
                    {
                        var value = dictionary[key];
                        if (key != null && value != null)
                        {
                            productionDict[key.ToString()] = Convert.ToSingle(value);
                        }
                    }
                }
                else if (production is System.Collections.IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        if (item != null)
                        {
                            var resourceType = SafeInvoke<string>("get_resourceType", item) ?? 
                                             SafeInvoke<string>("get_name", item) ?? "unknown";
                            var rate = SafeInvoke<float?>("get_rate", item) ?? 
                                     SafeInvoke<float?>("get_amount", item) ?? 0f;
                            productionDict[resourceType] = rate;
                        }
                    }
                }
                
                return productionDict;
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to get production rates for building {Name}: {ex.Message}");
                return new Dictionary<string, float>();
            }
        }
        
        /// <summary>
        /// Resource consumption rates
        /// Maps to: consumptionRates or inputs field
        /// </summary>
        public Dictionary<string, float> GetConsumptionRates()
        {
            try
            {
                var consumption = SafeInvoke<object>("get_consumptionRates") ?? 
                                SafeInvoke<object>("get_inputs") ??
                                SafeInvoke<object>("get_consumes");
                
                var consumptionDict = new Dictionary<string, float>();
                
                if (consumption is System.Collections.IDictionary dictionary)
                {
                    foreach (var key in dictionary.Keys)
                    {
                        var value = dictionary[key];
                        if (key != null && value != null)
                        {
                            consumptionDict[key.ToString()] = Convert.ToSingle(value);
                        }
                    }
                }
                else if (consumption is System.Collections.IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        if (item != null)
                        {
                            var resourceType = SafeInvoke<string>("get_resourceType", item) ?? 
                                             SafeInvoke<string>("get_name", item) ?? "unknown";
                            var rate = SafeInvoke<float?>("get_rate", item) ?? 
                                     SafeInvoke<float?>("get_amount", item) ?? 0f;
                            consumptionDict[resourceType] = rate;
                        }
                    }
                }
                
                return consumptionDict;
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to get consumption rates for building {Name}: {ex.Message}");
                return new Dictionary<string, float>();
            }
        }
        
        // ==================== PHYSICAL PROPERTIES ====================
        
        /// <summary>
        /// Building size (width, height)
        /// Maps to: size or dimensions field
        /// </summary>
        public (int Width, int Height) GetSize()
        {
            try
            {
                var size = SafeInvoke<object>("get_size") ?? SafeInvoke<object>("get_dimensions");
                if (size != null)
                {
                    var width = SafeInvoke<int?>("get_width", size) ?? SafeInvoke<int?>("get_x", size) ?? 1;
                    var height = SafeInvoke<int?>("get_height", size) ?? SafeInvoke<int?>("get_y", size) ?? 1;
                    return (width, height);
                }
                
                // Fallback to individual properties
                var w = SafeInvoke<int?>("get_width") ?? 1;
                var h = SafeInvoke<int?>("get_height") ?? 1;
                return (w, h);
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to get size for building {Name}: {ex.Message}");
                return (1, 1);
            }
        }
        
        /// <summary>
        /// Building prefab name for instantiation
        /// Maps to: prefabName field
        /// </summary>
        public string PrefabName
        {
            get => SafeInvoke<string>("get_prefabName") ?? Name;
        }
        
        /// <summary>
        /// Icon name for UI display
        /// Maps to: iconName field
        /// </summary>
        public string IconName
        {
            get => SafeInvoke<string>("get_iconName") ?? $"Building Icons/{Name}";
        }
        
        // ==================== REQUIREMENTS ====================
        
        /// <summary>
        /// Required technologies to unlock this building
        /// Maps to: requiredTechs or prerequisites field
        /// </summary>
        public List<string> GetRequiredTechnologies()
        {
            try
            {
                var techs = SafeInvoke<object>("get_requiredTechs") ?? 
                          SafeInvoke<object>("get_prerequisites") ??
                          SafeInvoke<object>("get_unlockRequirements");
                
                var techList = new List<string>();
                
                if (techs is System.Collections.IEnumerable enumerable)
                {
                    foreach (var tech in enumerable)
                    {
                        if (tech != null)
                        {
                            var techName = SafeInvoke<string>("get_name", tech) ?? tech.ToString();
                            if (!string.IsNullOrEmpty(techName))
                            {
                                techList.Add(techName);
                            }
                        }
                    }
                }
                
                return techList;
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to get required technologies for building {Name}: {ex.Message}");
                return new List<string>();
            }
        }
        
        /// <summary>
        /// Check if building is unlocked for a faction
        /// </summary>
        /// <param name="faction">Faction to check unlock status for</param>
        /// <returns>True if building is unlocked</returns>
        public bool IsUnlockedFor(Faction faction)
        {
            if (!faction.IsValid) return false;
            
            try
            {
                var result = SafeInvoke<bool?>("IsUnlockedFor", faction.NativeObject) ??
                           SafeInvoke<bool?>("IsAvailable", faction.NativeObject);
                           
                if (result.HasValue) return result.Value;
                
                // Check if all required technologies are researched
                var requiredTechs = GetRequiredTechnologies();
                return requiredTechs.All(tech => faction.HasTechnology(tech));
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to check unlock status of building {Name} for faction {faction.Name}: {ex.Message}");
                return false;
            }
        }
        
        // ==================== UTILITIES ====================
        
        /// <summary>
        /// Get building efficiency rating (1-5)
        /// </summary>
        public int GetEfficiencyRating()
        {
            var energyOutput = BaseEnergyOutput;
            var productionRates = GetProductionRates();
            var totalProduction = productionRates.Values.Sum();
            
            if (energyOutput > 50 || totalProduction > 100) return 5;
            if (energyOutput > 30 || totalProduction > 50) return 4;
            if (energyOutput > 15 || totalProduction > 25) return 3;
            if (energyOutput > 5 || totalProduction > 10) return 2;
            return 1;
        }
        
        /// <summary>
        /// Get construction complexity rating (1-5)
        /// </summary>
        public int GetConstructionComplexity()
        {
            var cost = ConstructionCost;
            var resourceCount = GetConstructionResources().Count;
            
            if (cost > 1000 || resourceCount > 5) return 5;
            if (cost > 500 || resourceCount > 3) return 4;
            if (cost > 250 || resourceCount > 2) return 3;
            if (cost > 100 || resourceCount > 1) return 2;
            return 1;
        }
        
        /// <summary>
        /// Get the native game object (for Harmony patches)
        /// </summary>
        public object? GetNativeObject()
        {
            return NativeObject;
        }
        
        /// <summary>
        /// String representation for debugging
        /// </summary>
        public override string ToString()
        {
            return $"BuildingType[{Name}] ({Category}, Index: {Index}, Valid: {IsValid})";
        }
        
        // ==================== STATIC UTILITIES ====================
        
        /// <summary>
        /// Common building type constants for easy reference
        /// </summary>
        public static class CommonBuildings
        {
            // Power Buildings
            public const string SolarPanel = "building_solar_panel";
            public const string SolarPanelField = "power_solar_panel_field";
            public const string NuclearReactor = "power_nuclear_reactor";
            public const string GeothermalPlant = "power_geothermal_plant";
            
            // Mining Buildings
            public const string WaterMine = "building_water_mine";
            public const string IronMine = "building_iron_mine";
            public const string SiliconMine = "building_silicon_mine";
            public const string CarbonMine = "building_carbon_mine";
            public const string AluminumMine = "building_aluminum_mine";
            
            // Manufacturing
            public const string SteelFactory = "building_steel_factory";
            public const string GlassFactory = "building_glass_factory";
            public const string ElectronicsFactory = "building_electronics_factory";
            public const string PartsFactory = "building_parts_factory";
            public const string FoodFactory = "building_food_factory";
            
            // Infrastructure
            public const string ColonyBasic = "building_colony_basic";
            public const string DroneBase = "building_drone_base";
            public const string ResearchLab = "building_research_lab";
            public const string Spaceport = "building_spaceport";
            public const string MaintenanceFacility = "building_maintenance_facility";
        }
    }
}