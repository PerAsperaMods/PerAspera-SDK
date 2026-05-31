#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Resource type registry - consolidates ResourceTypeWrapper access
    /// </summary>
    public class ResourceTypeRegistry : YamlTypeRegistry<ResourceTypeWrapper>
    {
        private static ResourceTypeRegistry? _instance;

        public static ResourceTypeRegistry Instance => _instance ??= new ResourceTypeRegistry();

        private ResourceTypeRegistry() : base("resources")
        {
            // Register with GameDataManager
            GameDataManager.RegisterRegistry("resources", this);
        }

        /// <summary>
        /// Initialize from ResourceTypeWrapper's existing database
        /// </summary>
        public override void InitializeFromYAML(string modId = "base", string checksum = null)
        {
            base.InitializeFromYAML(modId, checksum);

            // Also sync with ResourceTypeWrapper's existing database
            var allResources = ResourceTypeWrapper.GetAllResources();
            RegisterRange(allResources);

            Log.LogInfo($"✅ ResourceTypeRegistry initialized with {allResources.Count} resources");
        }

        /// <summary>
        /// Get atmospheric gases specifically
        /// </summary>
        public List<ResourceTypeWrapper> GetAtmosphericGases()
        {
            return GetAll()
                .Cast<ResourceTypeWrapper>()
                .Where(r => r.IsAtmosphericGas())
                .ToList();
        }

        /// <summary>
        /// Get resources by material type
        /// </summary>
        public List<ResourceTypeWrapper> GetByMaterialType(string materialType)
        {
            return GetAll()
                .Cast<ResourceTypeWrapper>()
                .Where(r => r.MaterialType().Equals(materialType, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private ManualLogSource Log => BepInEx.Logging.Logger.CreateLogSource("ResourceTypeRegistry");
    }

    /// <summary>
    /// Building type registry - consolidates BuildingTypeWrapper access
    /// </summary>
    public class BuildingTypeRegistry : YamlTypeRegistry<BuildingTypeWrapper>
    {
        private static BuildingTypeRegistry? _instance;

        public static BuildingTypeRegistry Instance => _instance ??= new BuildingTypeRegistry();

        private BuildingTypeRegistry() : base("buildings")
        {
            // Register with GameDataManager
            GameDataManager.RegisterRegistry("buildings", this);
        }

        /// <summary>
        /// Get buildings by category
        /// </summary>
        public List<BuildingTypeWrapper> GetByBuildingCategory(string category)
        {
            return GetAll()
                .Cast<BuildingTypeWrapper>()
                .Where(b => b.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        /// <summary>
        /// Get power generation buildings
        /// </summary>
        public List<BuildingTypeWrapper> GetPowerBuildings()
        {
            return GetAll()
                .Cast<BuildingTypeWrapper>()
                .Where(b => b.IsPowerBuilding())
                .ToList();
        }

        /// <summary>
        /// Get mining buildings
        /// </summary>
        public List<BuildingTypeWrapper> GetMiningBuildings()
        {
            return GetAll()
                .Cast<BuildingTypeWrapper>()
                .Where(b => b.IsMiningBuilding())
                .ToList();
        }

        private ManualLogSource Log => BepInEx.Logging.Logger.CreateLogSource("BuildingTypeRegistry");
    }

    /// <summary>
    /// Technology registry for future use
    /// </summary>
    public class TechnologyRegistry : YamlTypeRegistry<TechnologyWrapper>
    {
        private static TechnologyRegistry? _instance;

        public static TechnologyRegistry Instance => _instance ??= new TechnologyRegistry();

        private TechnologyRegistry() : base("technologies")
        {
            // Register with GameDataManager
            GameDataManager.RegisterRegistry("technologies", this);
        }

        /// <summary>
        /// Get technologies by category
        /// </summary>
        public List<TechnologyWrapper> GetByTechCategory(string category)
        {
            return GetAll()
                .Cast<TechnologyWrapper>()
                .Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private ManualLogSource Log => BepInEx.Logging.Logger.CreateLogSource("TechnologyRegistry");
    }
}