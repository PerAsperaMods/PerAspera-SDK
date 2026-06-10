#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using PerAspera.GameAPI.Database;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Unified interface for all YAML-based game data types (ResourceType, BuildingType, Technology, etc.)
    /// Provides consistent access patterns and caching for game data management
    /// </summary>
    public interface IYamlTypeWrapper
    {
        /// <summary>
        /// Unique key identifier for this type (e.g., "resource_water", "building_solar_panel")
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Display name for UI presentation
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Type category for organization
        /// </summary>
        string Category { get; }

        /// <summary>
        /// Check if this wrapper is valid and has data
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Get raw property value by name
        /// </summary>
        object? GetProperty(string propertyName);
    }

    /// <summary>
    /// Consolidated game data manager for all YAML types
    /// Provides unified access to ResourceType, BuildingType, Technology, and other game data
    /// </summary>
    public static class GameDataManager
    {
        private static readonly ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("GameDataManager");
        private static readonly Dictionary<string, IYamlTypeRegistry> _registries = new();

        /// <summary>
        /// Register a type registry for a specific YAML data type
        /// </summary>
        public static void RegisterRegistry(string typeName, IYamlTypeRegistry registry)
        {
            _registries[typeName] = registry;
            _logger.LogInfo($"✅ Registered {typeName} registry with {registry.Count} items");
        }

        /// <summary>
        /// Get all items of a specific type
        /// </summary>
        public static List<IYamlTypeWrapper> GetAll(string typeName)
        {
            if (_registries.TryGetValue(typeName, out var registry))
            {
                return registry.GetAll();
            }
            _logger.LogWarning($"Registry not found for type: {typeName}");
            return new List<IYamlTypeWrapper>();
        }

        /// <summary>
        /// Get item by key from specific type registry
        /// </summary>
        public static IYamlTypeWrapper? GetByKey(string typeName, string key)
        {
            if (_registries.TryGetValue(typeName, out var registry))
            {
                return registry.GetByKey(key);
            }
            _logger.LogWarning($"Registry not found for type: {typeName}");
            return null;
        }

        /// <summary>
        /// Get items by category from specific type registry
        /// </summary>
        public static List<IYamlTypeWrapper> GetByCategory(string typeName, string category)
        {
            if (_registries.TryGetValue(typeName, out var registry))
            {
                return registry.GetByCategory(category);
            }
            _logger.LogWarning($"Registry not found for type: {typeName}");
            return new List<IYamlTypeWrapper>();
        }

        /// <summary>
        /// Search across all registered types
        /// </summary>
        public static List<(string TypeName, IYamlTypeWrapper Item)> Search(string query, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            var results = new List<(string, IYamlTypeWrapper)>();

            foreach (var kvp in _registries)
            {
                var items = kvp.Value.GetAll()
                    .Where(item => item.Key.Contains(query, comparison) ||
                                  item.DisplayName.Contains(query, comparison))
                    .Select(item => (kvp.Key, item));

                results.AddRange(items);
            }

            return results;
        }

        /// <summary>
        /// Get statistics about registered data
        /// </summary>
        public static Dictionary<string, int> GetStatistics()
        {
            return _registries.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Count);
        }

        /// <summary>
        /// Initialize all registered registries with YAML data
        /// </summary>
        public static void InitializeFromYAML(string modId = "base", string checksum = null)
        {
            foreach (var kvp in _registries)
            {
                try
                {
                    kvp.Value.InitializeFromYAML(modId, checksum);
                    _logger.LogInfo($"✅ Initialized {kvp.Key} registry");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to initialize {kvp.Key} registry: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Interface for type-specific registries
    /// </summary>
    public interface IYamlTypeRegistry
    {
        /// <summary>
        /// Type name (e.g., "resources", "buildings", "technologies")
        /// </summary>
        string TypeName { get; }

        /// <summary>
        /// Number of registered items
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Get all items in this registry
        /// </summary>
        List<IYamlTypeWrapper> GetAll();

        /// <summary>
        /// Get item by key
        /// </summary>
        IYamlTypeWrapper? GetByKey(string key);

        /// <summary>
        /// Get items by category
        /// </summary>
        List<IYamlTypeWrapper> GetByCategory(string category);

        /// <summary>
        /// Initialize registry from YAML data
        /// </summary>
        void InitializeFromYAML(string modId = "base", string checksum = null);
    }

    /// <summary>
    /// Generic registry implementation for any YAML type
    /// </summary>
    public class YamlTypeRegistry<T> : IYamlTypeRegistry where T : IYamlTypeWrapper
    {
        private readonly Dictionary<string, T> _items = new();
        private readonly object _lock = new();
        private readonly ManualLogSource _logger;

        public string TypeName { get; }
        public int Count => _items.Count;

        public YamlTypeRegistry(string typeName)
        {
            TypeName = typeName;
            _logger = BepInEx.Logging.Logger.CreateLogSource($"YamlRegistry-{typeName}");
        }

        public List<IYamlTypeWrapper> GetAll()
        {
            lock (_lock)
            {
                return _items.Values.Cast<IYamlTypeWrapper>().ToList();
            }
        }

        public IYamlTypeWrapper? GetByKey(string key)
        {
            lock (_lock)
            {
                return _items.TryGetValue(key, out var item) ? item : null;
            }
        }

        public List<IYamlTypeWrapper> GetByCategory(string category)
        {
            lock (_lock)
            {
                return _items.Values
                    .Where(item => item.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                    .Cast<IYamlTypeWrapper>()
                    .ToList();
            }
        }

        public void Register(T item)
        {
            lock (_lock)
            {
                _items[item.Key] = item;
                _logger.LogDebug($"Registered {TypeName}: {item.Key}");
            }
        }

        public void RegisterRange(IEnumerable<T> items)
        {
            lock (_lock)
            {
                foreach (var item in items)
                {
                    _items[item.Key] = item;
                }
                _logger.LogInfo($"Registered {items.Count()} {TypeName}");
            }
        }

        public virtual void InitializeFromYAML(string modId = "base", string checksum = null)
        {
            try
            {
                // Load data from ModDatabase
                var yamlData = ModDatabase.Instance.RetrieveYAMLData(TypeName, modId);
                if (yamlData != null)
                {
                    // Convert YAML data to wrapper instances
                    // This would need to be implemented by specific registry subclasses
                    _logger.LogInfo($"Loaded {TypeName} data from database for mod '{modId}'");
                }
                else
                {
                    _logger.LogWarning($"No {TypeName} data found in database for mod '{modId}'");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize {TypeName} from YAML: {ex.Message}");
            }
        }
    }
}