#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Native;


#pragma warning disable CS1591
namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native BuildingType class
    /// Provides safe access to building type definitions and properties
    /// DOC: BuildingType.md - Building definitions and construction properties
    /// Implements IYamlTypeWrapper for unified game data access
    /// </summary>
    public class BuildingTypeWrapper : WrapperBase, IYamlTypeWrapper
    {
        /// <summary>
        /// Initialize BuildingType wrapper with native building type object
        /// </summary>
        /// <param name="nativeBuildingType">Native building type instance from game</param>
        public BuildingTypeWrapper(object nativeBuildingType) : base(nativeBuildingType)
        {
        }

        /// <summary>Wraps a typed interop BuildingType proxy.</summary>
        public BuildingTypeWrapper(BuildingType nativeBuildingType) : base(nativeBuildingType)
        {
        }

        /// <summary>Typed interop proxy (null when the wrapper is invalid).</summary>
        /// <example>int cap = bt.NativeBuildingType?.droneCapacity ?? 0;</example>
        public BuildingType? NativeBuildingType => GetNativeObject() as BuildingType;

        /// <summary>
        /// Create wrapper from native building type object
        /// </summary>
        public static BuildingTypeWrapper? FromNative(object? nativeBuildingType)
        {
            return nativeBuildingType != null ? new BuildingTypeWrapper(nativeBuildingType) : null;
        }

        // ==================== CORE IDENTIFICATION ====================

        /// <summary>
        /// Building type key identifier (typed read of StaticDataCollectionItem.key).
        /// E.g., "solar_panel_field", "water_mine".
        /// </summary>
        public string Name => NativeBuildingType?.key ?? "unknown_building";

        /// <summary>
        /// Building display name for UI (typed read of BuildingType.name).
        /// </summary>
        public string DisplayName => NativeBuildingType?.name ?? Name;

        /// <summary>
        /// Building description (typed read of BuildingType.description).
        /// </summary>
        public string Description => NativeBuildingType?.description ?? "No description available";

        /// <summary>N'a jamais existé — BuildingType n'a pas d'index.</summary>
        [Obsolete("BuildingType.index n'existe pas dans le jeu — retournait toujours -1.", false)]
        public int Index => -1;

        // ==================== BUILDING CATEGORY ====================

        /// <summary>
        /// Building category key (typed read of BuildingType.categoryType.key).
        /// (L'ancienne chaîne category/buildingCategory/type n'existait pas.)
        /// </summary>
        public string Category => NativeBuildingType?.categoryType?.key ?? "General";

        /// <summary>Typed category proxy (BuildingCategoryType).</summary>
        public BuildingCategoryType? CategoryType => NativeBuildingType?.categoryType;

        /// <summary>N'a jamais existé sur BuildingType.</summary>
        [Obsolete("BuildingType.subCategory/subType n'existent pas — retournait toujours \"\".", false)]
        public string SubCategory => "";
        
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
        
        /// <summary>N'a jamais existé — pas de coût scalaire ; voir GetConstructionResources().</summary>
        [Obsolete("BuildingType.constructionCost/cost n'existent pas — retournait toujours 0. Utiliser GetConstructionResources() (typé).", false)]
        public float ConstructionCost => 0f;

        /// <summary>N'a jamais existé sur BuildingType.</summary>
        [Obsolete("BuildingType.constructionTime/buildTime n'existent pas — retournait toujours 0.", false)]
        public float ConstructionTime => 0f;

        /// <summary>
        /// Required resources for construction, by resource key (units).
        /// Typed read of BuildingType.requiredConstructionResources.
        /// (L'ancienne chaîne requiredResources/constructionResources/materials n'existait pas.)
        /// </summary>
        /// <example>foreach (var (res, qty) in bt.GetConstructionResources()) { ... }</example>
        public Dictionary<string, float> GetConstructionResources()
        {
            var resourceDict = new Dictionary<string, float>();
            var resources = NativeBuildingType?.requiredConstructionResources;
            if (resources == null) return resourceDict;
            foreach (var kvp in resources)
                if (kvp.Key != null)
                    resourceDict[kvp.Key.key] = kvp.Value.ToFloat();
            return resourceDict;
        }

        /// <summary>N'a jamais existé — voir healthLossPerDay/maintenancePriority typés.</summary>
        [Obsolete("BuildingType.maintenanceCost/upkeepCost n'existent pas — retournait toujours 0. Voir HealthLossPerDay.", false)]
        public float MaintenanceCost => 0f;

        /// <summary>Daily health loss (typed read of BuildingType.healthLossPerDay).</summary>
        public float HealthLossPerDay => NativeBuildingType?.healthLossPerDay ?? 0f;

        /// <summary>Maximum building health (typed read of BuildingType.maxHealth).</summary>
        public float MaxHealth => NativeBuildingType?.maxHealth ?? 0f;
        
        // ==================== PRODUCTION PROPERTIES ====================
        
        /// <summary>
        /// Base energy production — the highest of the five typed production fields
        /// (solar/eolic/thermal/fission/fusion ; un bâtiment n'en utilise qu'un).
        /// (L'ancienne chaîne baseEnergyOutput/energyProduction/powerOutput n'existait pas.)
        /// </summary>
        public float BaseEnergyOutput
        {
            get
            {
                var bt = NativeBuildingType;
                if (bt == null) return 0f;
                return Math.Max(bt.maxSolarPowerProduction,
                       Math.Max(bt.maxEolicPowerProduction,
                       Math.Max(bt.maxThermalPowerProduction,
                       Math.Max(bt.maxFissionPowerProduction, bt.maxFusionPowerProduction))));
            }
        }

        /// <summary>Solar power production (typed).</summary>
        public float MaxSolarPowerProduction => NativeBuildingType?.maxSolarPowerProduction ?? 0f;

        /// <summary>Energy storage capacity (typed).</summary>
        public float EnergyStorageCapacity => NativeBuildingType?.energyStorageCapacity ?? 0f;

        /// <summary>
        /// Energy consumption (typed read of BuildingType.powerConsumption).
        /// </summary>
        public float EnergyConsumption => NativeBuildingType?.powerConsumption ?? 0f;

        /// <summary>Colonist capacity (typed).</summary>
        public int ColonistCapacity => NativeBuildingType?.colonistCapacity ?? 0;

        /// <summary>Drone capacity (typed).</summary>
        public int DroneCapacity => NativeBuildingType?.droneCapacity ?? 0;
        
        /// <summary>
        /// Resource production: output resource key → quantity per cycle.
        /// Typed read of BuildingType.outputResource/outputQuantity.
        /// (L'ancienne chaîne productionRates/outputs/produces n'existait pas.)
        /// </summary>
        public Dictionary<string, float> GetProductionRates()
        {
            var productionDict = new Dictionary<string, float>();
            var bt = NativeBuildingType;
            var output = bt?.outputResource;
            if (bt == null || output == null) return productionDict;
            productionDict[output.key] = bt.outputQuantity.ToFloat();
            return productionDict;
        }

        /// <summary>Output resource key, or null when the building produces nothing (typed).</summary>
        public string? OutputResourceKey => NativeBuildingType?.outputResource?.key;

        /// <summary>Production progress per day (typed read of BuildingType.progressPerDay).</summary>
        public float ProgressPerDay => NativeBuildingType?.progressPerDay ?? 0f;

        /// <summary>
        /// Resource consumption: input resource key → quantity.
        /// Typed read of BuildingType.inputResources.
        /// (L'ancienne chaîne consumptionRates/inputs/consumes n'existait pas.)
        /// </summary>
        public Dictionary<string, float> GetConsumptionRates()
        {
            var consumptionDict = new Dictionary<string, float>();
            var inputs = NativeBuildingType?.inputResources;
            if (inputs == null) return consumptionDict;
            foreach (var kvp in inputs)
                if (kvp.Key != null)
                    consumptionDict[kvp.Key.key] = kvp.Value.ToFloat();
            return consumptionDict;
        }
        
        // ==================== PHYSICAL PROPERTIES ====================
        
        /// <summary>N'a jamais existé — les bâtiments occupent un rayon, pas une grille.</summary>
        [Obsolete("BuildingType.size/dimensions n'existent pas — retournait toujours (1,1). Voir ReservedRadius.", false)]
        public (int Width, int Height) GetSize() => (1, 1);

        /// <summary>Reserved placement radius (typed read of BuildingType._reservedRadius).</summary>
        public float ReservedRadius => NativeBuildingType?._reservedRadius ?? 0f;

        /// <summary>
        /// Building prefab name for instantiation (typed read of BuildingType.prefabName).
        /// </summary>
        public string PrefabName => NativeBuildingType?.prefabName ?? Name;

        /// <summary>Icon sprite for UI display (typed).</summary>
        public UnityEngine.Sprite? Icon => NativeBuildingType?.iconName;

        /// <summary>
        /// Icon sprite name.
        /// ⚠️ « iconName » natif est un Sprite, pas une string — l'ancien binding string
        /// échouait et retournait toujours le fallback.
        /// </summary>
        public string IconName => NativeBuildingType?.iconName?.name ?? $"Building Icons/{Name}";
        
        // ==================== REQUIREMENTS ====================
        
        /// <summary>N'a jamais fonctionné — ces membres n'existent pas sur BuildingType.</summary>
        [Obsolete("requiredTechs/prerequisites/unlockRequirements n'existent pas sur BuildingType — retournait toujours vide. Le déblocage passe par BuildingType.availability et les actions des technologies (TechnologyWrapper.GetActions()).", false)]
        public List<string> GetRequiredTechnologies() => new List<string>();

        /// <summary>
        /// ⚠️ A toujours retourné TRUE : la chaîne IsUnlockedFor/IsAvailable n'existait pas,
        /// et le fallback testait All() sur une liste vide de prérequis (= true).
        /// </summary>
        [Obsolete("A toujours retourné true (bug : All() sur liste vide). Le déblocage passe par BuildingType.availability — pas encore exposé typé.", false)]
        public bool IsUnlockedFor(FactionWrapper faction) => true;
        
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
        /// Get construction complexity rating (1-5), based on the typed
        /// construction resources (total quantity + variety).
        /// </summary>
        public int GetConstructionComplexity()
        {
            var resources = GetConstructionResources();
            var totalQty = resources.Values.Sum();
            var resourceCount = resources.Count;

            if (totalQty > 1000 || resourceCount > 5) return 5;
            if (totalQty > 500 || resourceCount > 3) return 4;
            if (totalQty > 250 || resourceCount > 2) return 3;
            if (totalQty > 100 || resourceCount > 1) return 2;
            return 1;
        }
        
        /// <summary>
        /// Get localized display name from game data
        /// Uses native DisplayName property loaded from YAML
        /// </summary>
        /// <returns>Localized display name from game data</returns>
        public string GetDisplayName()
        {
            // Use native display name from YAML data
            var displayName = DisplayName;
            if (!string.IsNullOrEmpty(displayName) && displayName != Name)
            {
                return displayName;
            }
            
            // Fallback to formatted key name
            return ToTitleCase(Name.Replace("building_", "").Replace("power_", "").Replace("_", " "));
        }
        
        /// <summary>
        /// Convert string to title case
        /// </summary>
        private static string ToTitleCase(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            var words = text.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) + (words[i].Length > 1 ? words[i].Substring(1).ToLower() : "");
                }
            }
            return string.Join(" ", words);
        }
        
        /// <summary>
        /// String representation for debugging
        /// </summary>
        public override string ToString()
        {
            return $"BuildingType[{Name}] ({Category}, Valid: {IsValid})";
        }
        
        // ==================== STATIC UTILITIES ====================
        
        /// <summary>
        /// Common building type key constants for easy reference
        /// These are the YAML keys used in the game data files
        /// </summary>
        public static class CommonBuildings
        {
            // Power Buildings
            public const string SolarPanel = "building_solar_panel";
            /// <summary>
            /// Solar panel field building identifier
            /// </summary>
            public const string SolarPanelField = "power_solar_panel_field";
            /// <summary>
            /// Nuclear reactor building identifier
            /// </summary>
            public const string NuclearReactor = "power_nuclear_reactor";
            /// <summary>
            /// Geothermal plant building identifier
            /// </summary>
            public const string GeothermalPlant = "power_geothermal_plant";
            
            // Mining Buildings
            public const string WaterMine = "building_water_mine";
            /// <summary>
            /// Iron mine building identifier
            /// </summary>
            public const string IronMine = "building_iron_mine";
            /// <summary>
            /// Silicon mine building identifier
            /// </summary>
            public const string SiliconMine = "building_silicon_mine";
            /// <summary>
            /// Carbon mine building identifier
            /// </summary>
            public const string CarbonMine = "building_carbon_mine";
            /// <summary>
            /// Aluminum mine building identifier
            /// </summary>
            public const string AluminumMine = "building_aluminum_mine";
            
            // Manufacturing
            /// <summary>
            /// Steel factory building identifier
            /// </summary>
            public const string SteelFactory = "building_steel_factory";
            /// <summary>
            /// Glass factory building identifier
            /// </summary>
            public const string GlassFactory = "building_glass_factory";
            /// <summary>
            /// Electronics factory building identifier
            /// </summary>
            public const string ElectronicsFactory = "building_electronics_factory";
            /// <summary>
            /// Parts factory building identifier
            /// </summary>
            public const string PartsFactory = "building_parts_factory";
            /// <summary>
            /// Food factory building identifier
            /// </summary>
            public const string FoodFactory = "building_food_factory";
            
            // Infrastructure
            /// <summary>
            /// Basic colony building identifier
            /// </summary>
            public const string ColonyBasic = "building_colony_basic";
            /// <summary>
            /// Drone base building identifier
            /// </summary>
            public const string DroneBase = "building_drone_base";
            /// <summary>
            /// Research lab building identifier
            /// </summary>
            public const string ResearchLab = "building_research_lab";
            /// <summary>
            /// Spaceport building identifier
            /// </summary>
            public const string Spaceport = "building_spaceport";
            /// <summary>
            /// Maintenance facility building identifier
            /// </summary>
            public const string MaintenanceFacility = "building_maintenance_facility";
        }

        // ==================== IYamlTypeWrapper Implementation ====================

        /// <summary>
        /// Get the unique key for this building type
        /// Implements IYamlTypeWrapper.GetKey()
        /// </summary>
        /// <returns>The building key (e.g., "building_solar_panel")</returns>
        public string GetKey()
        {
            return Name;
        }

        /// <summary>
        /// Get the display name for this building type
        /// Implements IYamlTypeWrapper.GetName()
        /// </summary>
        /// <returns>The localized display name</returns>
        public string GetName()
        {
            return DisplayName;
        }

        /// <summary>
        /// Get the type name for this wrapper type
        /// Implements IYamlTypeWrapper.GetTypeName()
        /// </summary>
        /// <returns>"buildings" for building types</returns>
        public string GetTypeName()
        {
            return "buildings";
        }

        /// <summary>
        /// Get all keys for this type from the database
        /// Implements IYamlTypeWrapper.GetAllKeys()
        /// Note: BuildingTypeWrapper currently doesn't have a static database
        /// TODO: Implement static database similar to ResourceTypeWrapper
        /// </summary>
        /// <returns>List of all building keys (currently empty)</returns>
        public static List<string> GetAllKeys()
        {
            // TODO: Implement static database for buildings
            // For now, return empty list until database is populated
            return new List<string>();
        }

        // ==================== IYamlTypeWrapper IMPLEMENTATION ====================

        /// <summary>
        /// Unique key identifier for this building type
        /// Implements IYamlTypeWrapper.Key
        /// </summary>
        public string Key => Name;

        /// <summary>
        /// Check if this wrapper is valid and has data
        /// Implements IYamlTypeWrapper.IsValid
        /// </summary>
        public bool IsValid => IsValidWrapper;

        /// <summary>
        /// Get raw property value by name
        /// Implements IYamlTypeWrapper.GetProperty(string)
        /// </summary>
        /// <param name="propertyName">Name of the property to retrieve</param>
        /// <returns>Property value or null if not found</returns>
        public object? GetProperty(string propertyName)
        {
            return propertyName.ToLowerInvariant() switch
            {
                "name" => Name,
                "displayname" => DisplayName,
                "description" => Description,
                "category" => Category,
                "outputresource" => OutputResourceKey,
                "progressperday" => ProgressPerDay,
                "maxhealth" => MaxHealth,
                "colonistcapacity" => ColonistCapacity,
                "dronecapacity" => DroneCapacity,
                "baseenergyoutput" => BaseEnergyOutput,
                "energyconsumption" => EnergyConsumption,
                "isvalid" => IsValid,
                _ => null  // unknown property — add a case above if needed
            };
        }
    }
}
#pragma warning restore CS1591
