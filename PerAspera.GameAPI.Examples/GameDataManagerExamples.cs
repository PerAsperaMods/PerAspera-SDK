using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.GameAPI.Wrappers;
using PerAspera.GameAPI.Database;

namespace PerAspera.Examples
{
    /// <summary>
    /// Examples demonstrating the consolidated GameDataManager system
    /// Shows migration from individual wrapper access to unified data management
    /// </summary>
    public static class GameDataManagerExamples
    {
        /// <summary>
        /// Example 1: Basic resource access using consolidated system
        /// Shows how to replace ResourceTypeWrapper.GetByKey() calls
        /// </summary>
        public static void BasicResourceAccess()
        {
            LogAspera.Info("=== Basic Resource Access Example ===");

            // OLD WAY: Direct ResourceTypeWrapper access
            var waterResource = ResourceTypeWrapper.GetByKey("resource_water");
            if (waterResource != null)
            {
                LogAspera.Info($"Water resource: {waterResource.GetName()}");
            }

            // NEW WAY: Unified GameDataManager access
            var unifiedWaterResource = GameDataManager.GetByKey("resources", "resource_water");
            if (unifiedWaterResource != null)
            {
                LogAspera.Info($"Water resource (unified): {unifiedWaterResource.GetName()}");
            }

            // Get all resources using unified system
            var allResources = GameDataManager.GetAll("resources");
            LogAspera.Info($"Total resources available: {allResources.Count}");

            // Search for atmospheric gases
            var atmosphericGases = GameDataManager.Search("resources", "atmospheric");
            LogAspera.Info($"Atmospheric gases found: {atmosphericGases.Count}");
        }

        /// <summary>
        /// Example 2: Building access using consolidated system
        /// Shows how to access buildings through unified interface
        /// </summary>
        public static void BuildingAccess()
        {
            LogAspera.Info("=== Building Access Example ===");

            // NEW WAY: Unified access to buildings
            var solarPanel = GameDataManager.GetByKey("buildings", "building_solar_panel");
            if (solarPanel != null)
            {
                LogAspera.Info($"Solar panel: {solarPanel.GetName()}");
            }

            // Get all buildings
            var allBuildings = GameDataManager.GetAll("buildings");
            LogAspera.Info($"Total buildings available: {allBuildings.Count}");

            // Search for power-related buildings
            var powerBuildings = GameDataManager.Search("buildings", "power");
            LogAspera.Info($"Power buildings found: {powerBuildings.Count}");
        }

        /// <summary>
        /// Example 3: Cross-type queries and statistics
        /// Shows how to get statistics across multiple YAML types
        /// </summary>
        public static void CrossTypeQueries()
        {
            LogAspera.Info("=== Cross-Type Queries Example ===");

            // Get statistics for all types
            var resourceStats = GameDataManager.GetStatistics("resources");
            LogAspera.Info($"Resource statistics: {resourceStats}");

            var buildingStats = GameDataManager.GetStatistics("buildings");
            LogAspera.Info($"Building statistics: {buildingStats}");

            // Search across all types for "energy"
            var energyRelated = GameDataManager.SearchAllTypes("energy");
            LogAspera.Info($"Energy-related items across all types: {energyRelated.Count}");

            // Get all available types
            var availableTypes = GameDataManager.GetAvailableTypes();
            LogAspera.Info($"Available data types: {string.Join(", ", availableTypes)}");
        }

        /// <summary>
        /// Example 4: Atmosphere management integration
        /// Shows how the consolidated system supports atmosphere rework
        /// </summary>
        public static void AtmosphereManagementIntegration()
        {
            LogAspera.Info("=== Atmosphere Management Integration ===");

            // Get all atmospheric gases using unified system
            var atmosphericGases = GameDataManager.Search("resources", "atmospheric");
            LogAspera.Info($"Atmospheric gases for atmosphere management: {atmosphericGases.Count}");

            foreach (var gas in atmosphericGases)
            {
                LogAspera.Info($"  - {gas.GetKey()}: {gas.GetName()}");
            }

            // Get buildings that affect atmosphere
            var atmosphereBuildings = GameDataManager.Search("buildings", "atmosphere");
            LogAspera.Info($"Buildings affecting atmosphere: {atmosphereBuildings.Count}");

            // Use specific registry methods for atmosphere management
            var atmosphereRegistry = GameDataRegistries.ResourceTypeRegistry;
            var gasKeys = atmosphereRegistry.GetAtmosphericGasKeys();
            LogAspera.Info($"Atmospheric gas keys: {string.Join(", ", gasKeys)}");
        }

        /// <summary>
        /// Example 5: Migration helper for existing code
        /// Shows how to gradually migrate from old patterns to new unified system
        /// </summary>
        public static void MigrationHelper()
        {
            LogAspera.Info("=== Migration Helper Example ===");

            // Helper method to safely migrate resource access
            ResourceTypeWrapper? GetResourceSafely(string key)
            {
                // Try unified system first
                var unified = GameDataManager.GetByKey("resources", key) as ResourceTypeWrapper;
                if (unified != null) return unified;

                // Fallback to old system
                LogAspera.Warning($"Resource '{key}' not found in unified system, using legacy access");
                return ResourceTypeWrapper.GetByKey(key);
            }

            // Test migration
            var water = GetResourceSafely("resource_water");
            if (water != null)
            {
                LogAspera.Info($"Migrated access successful: {water.GetName()}");
            }
        }

        /// <summary>
        /// Run all examples to demonstrate the consolidated system
        /// </summary>
        public static void RunAllExamples()
        {
            try
            {
                LogAspera.Info("Starting GameDataManager Examples...");

                BasicResourceAccess();
                BuildingAccess();
                CrossTypeQueries();
                AtmosphereManagementIntegration();
                MigrationHelper();

                LogAspera.Info("All examples completed successfully!");
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Error running examples: {ex.Message}");
            }
        }
    }
}