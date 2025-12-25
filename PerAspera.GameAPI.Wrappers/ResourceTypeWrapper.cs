using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Native;
using PerAspera.GameAPI.Database;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native ResourceType class
    /// Provides safe access to resource type definitions and properties
    /// DOC: Resource.md - Resource definitions and properties
    /// </summary>
    public class ResourceTypeWrapper : WrapperBase
    {
        // ==================== STATIC RESOURCE DATABASE ====================
        
        /// <summary>
        /// Static database of all resources parsed from YAML data
        /// Key: resource name (e.g., "resource_water")
        /// Value: ResourceTypeWrapper instance
        /// </summary>
        private static readonly Dictionary<string, ResourceTypeWrapper> _resourceDatabase = new();
        
        /// <summary>
        /// Lock for thread-safe access to resource database
        /// </summary>
        private static readonly object _databaseLock = new();
        
        /// <summary>
        /// Get all resources from the static database
        /// Returns resources parsed from YAML data during game initialization
        /// </summary>
        /// <returns>List of all available ResourceTypeWrapper instances</returns>
        public static List<ResourceTypeWrapper> GetAllResources()
        {
            lock (_databaseLock)
            {
                return _resourceDatabase.Values.ToList();
            }
        }
        
        /// <summary>
        /// Populate the static resource database from parsed YAML data
        /// Called by YAMLDataInterceptorPlugin during game initialization
        /// Now uses SQLite for persistent storage
        /// </summary>
        /// <param name="parsedResources">Dictionary of parsed resource data</param>
        /// <param name="modId">Mod identifier</param>
        /// <param name="checksum">Checksum of the source YAML</param>
        public static void PopulateDatabase(Dictionary<string, object> parsedResources, string modId = "base", string checksum = null)
        {
            lock (_databaseLock)
            {
                try
                {
                    // Store in SQLite database for persistence
                    ModDatabase.Instance.StoreYAMLData("resources", modId, parsedResources, checksum);

                    // Also maintain in-memory cache for fast access
                    _resourceDatabase.Clear();

                    foreach (var kvp in parsedResources)
                    {
                        try
                        {
                            // Create a lightweight wrapper for YAML data
                            var yamlResource = new YamlResourceWrapper(kvp.Key, kvp.Value);
                            _resourceDatabase[kvp.Key] = yamlResource;
                        }
                        catch (Exception ex)
                        {
                            Log.LogWarning($"Failed to create ResourceTypeWrapper for {kvp.Key}: {ex.Message}");
                        }
                    }

                    Log.LogInfo($"✅ Resource database populated with {_resourceDatabase.Count} resources (stored in SQLite for mod '{modId}')");
                }
                catch (Exception ex)
                {
                    Log.LogError($"Failed to populate resource database: {ex.Message}");

                    // Fallback: populate in-memory only
                    _resourceDatabase.Clear();
                    foreach (var kvp in parsedResources)
                    {
                        try
                        {
                            var yamlResource = new YamlResourceWrapper(kvp.Key, kvp.Value);
                            _resourceDatabase[kvp.Key] = yamlResource;
                        }
                        catch (Exception ex2)
                        {
                            Log.LogWarning($"Failed to create ResourceTypeWrapper for {kvp.Key}: {ex2.Message}");
                        }
                    }

                    Log.LogWarning("⚠️ Using in-memory database only (SQLite failed)");
                }
            }
        }
        
        /// <summary>
        /// Lightweight wrapper for resources parsed from YAML data
        /// Used when native ResourceType objects are not available
        /// </summary>
        private class YamlResourceWrapper : ResourceTypeWrapper
        {
            private readonly string _name;
            private readonly Dictionary<string, object> _yamlData;
            
            public YamlResourceWrapper(string name, object yamlData) : base(null)
            {
                _name = name;
                _yamlData = yamlData as Dictionary<string, object> ?? new Dictionary<string, object>();
            }
            
            // Override base properties to provide YAML-based data
            public override string Name => _name;
            
            public override string DisplayName => 
                _yamlData.TryGetValue("displayName", out var displayName) ? displayName?.ToString() ?? _name : _name;
                
            public override int Index => 
                _yamlData.TryGetValue("index", out var index) && int.TryParse(index?.ToString(), out var i) ? i : -1;
                
            public override string ColorHex => 
                _yamlData.TryGetValue("color", out var color) ? color?.ToString() ?? "FFFFFF" : "FFFFFF";
        }
        /// <summary>
        /// Initialize ResourceType wrapper with native resource type object
        /// </summary>
        /// <param name="nativeResourceType">Native resource type instance from game</param>
        public ResourceTypeWrapper(object nativeResourceType) : base(nativeResourceType)
        {
        }
        
        /// <summary>
        /// Create wrapper from native resource type object
        /// </summary>
        public static ResourceTypeWrapper? FromNative(object? nativeResourceType)
        {
            return nativeResourceType != null ? new ResourceTypeWrapper(nativeResourceType) : null;
        }
        
        /// <summary>
        /// Get ResourceType wrapper by key (e.g., "resource_water", "resource_silicon")
        /// Uses KeeperTypeRegistry to access native ResourceType collection
        /// </summary>
        /// <param name="resourceKey">Resource key from YAML definitions</param>
        /// <returns>ResourceType wrapper or null if not found</returns>
        /// <example>
        /// var water = ResourceType.GetByKey("resource_water");
        /// var silicon = ResourceType.GetByKey("resource_silicon");
        /// if (water != null) {
        ///     Console.WriteLine($"Water: {water.DisplayName}");
        /// }
        /// </example>
        public static ResourceTypeWrapper? GetByKey(string resourceKey)
        {
            if (string.IsNullOrEmpty(resourceKey))
            {
                Log.LogWarning("GetByKey called with null/empty resource key");
                return null;
            }
            
            try
            {
                var nativeResourceType = KeeperTypeRegistry.GetResourceType(resourceKey);
                return FromNative(nativeResourceType);
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get ResourceType for key '{resourceKey}': {ex.Message}");
                return null;
            }
        }
        
        // ==================== CORE IDENTIFICATION ====================
        
        /// <summary>
        /// Resource type name/key identifier
        /// Maps to: name field (e.g., "resource_water", "resource_iron")
        /// </summary>
        public virtual string Name
        {
            get => SafeInvoke<string>("get_name") ?? "unknown_resource";
        }
        
        /// <summary>
        /// Resource display name for UI
        /// Maps to: displayName or localizedName field
        /// </summary>
        public virtual string DisplayName
        {
            get => SafeInvoke<string>("get_displayName") ?? 
                   SafeInvoke<string>("get_localizedName") ?? Name;
        }
        
        /// <summary>
        /// Resource index for efficient lookups
        /// Maps to: index field
        /// </summary>
        public virtual int Index
        {
            get => SafeInvoke<int?>("get_index") ?? -1;
        }
        
        /// <summary>
        /// Resource color for UI display
        /// Maps to: color field
        /// </summary>
        public virtual string ColorHex
        {
            get => SafeInvoke<string>("get_color") ?? "FFFFFF";
        }
        
        // ==================== RESOURCE PROPERTIES ====================
        
        /// <summary>
        /// Material type category (Mined, Manufactured, etc.)
        /// Maps to: materialType field
        /// </summary>
        public string MaterialType()
        {
            return ((ResourceType)NativeObject).materialType.ToString(); 
        }
        
        /// <summary>
        /// Is this a mined/extracted resource?
        /// </summary>
        public bool IsMined
        {
            get => MaterialType().Equals("Mined", StringComparison.OrdinalIgnoreCase);
        }
        public bool isGas
        {
            get =>  MaterialType().Equals("Released", StringComparison.OrdinalIgnoreCase);

        }
        /// <summary>
        /// Is this a manufactured/processed resource?
        /// </summary>
        public bool IsManufactured
        {
            get => MaterialType().Equals("Manufactured", StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Resource prefab name for instantiation
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
            get => SafeInvoke<string>("get_iconName") ?? "Resource Icons/Unknown";
        }
        
        // ==================== KNOWLEDGE INTEGRATION ====================
        
        /// <summary>
        /// Associated knowledge entry for this resource
        /// Maps to: knowledge field
        /// </summary>
        public object? Knowledge
        {
            get => SafeInvoke<object>("get_knowledge");
        }
        
        /// <summary>
        /// Get knowledge wrapper for this resource
        /// </summary>
        public Knowledge? GetKnowledge()
        {
            var knowledge = Knowledge;
            return knowledge != null ? new Knowledge(knowledge) : null;
        }
        
        // ==================== RESOURCE VEINS ====================
        
        /// <summary>
        /// Vein icon names for resource deposits
        /// Maps to: veinIconsName array
        /// </summary>
        public List<string> VeinIconNames
        {
            get
            {
                try
                {
                    var veinIcons = SafeInvoke<object>("get_veinIconsName");
                    var iconList = new List<string>();
                    
                    if (veinIcons is System.Collections.IEnumerable enumerable)
                    {
                        foreach (var icon in enumerable)
                        {
                            var iconString = icon?.ToString();
                            if (!string.IsNullOrEmpty(iconString))
                            {
                                iconList.Add(iconString);
                            }
                        }
                    }
                    
                    return iconList;
                }
                catch (Exception ex)
                {
                    Log.LogWarning($"Failed to get vein icons for resource {Name}: {ex.Message}");
                    return new List<string>();
                }
            }
        }
        
        /// <summary>
        /// Cube material for resource display
        /// Maps to: cubeMaterial field
        /// </summary>
        public string CubeMaterial
        {
            get => SafeInvoke<string>("get_cubeMaterial") ?? "ResourceCube";
        }
        
        // ==================== UTILITIES ====================
        
        /// <summary>
        /// Get resource color as System.Drawing.Color
        /// </summary>
        public System.Drawing.Color GetColor()
        {
            try
            {
                var colorHex = ColorHex;
                if (colorHex.Length == 6) // RRGGBB format
                {
                    var r = Convert.ToInt32(colorHex.Substring(0, 2), 16);
                    var g = Convert.ToInt32(colorHex.Substring(2, 2), 16);
                    var b = Convert.ToInt32(colorHex.Substring(4, 2), 16);
                    return System.Drawing.Color.FromArgb(255, r, g, b);
                }
                return System.Drawing.Color.Gray;
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to parse color '{ColorHex}' for resource {Name}: {ex.Message}");
                return System.Drawing.Color.Gray;
            }
        }
        
        /// <summary>
        /// Check if this is a primary/base resource
        /// </summary>
        public bool IsPrimaryResource()
        {
            var primaryResources = new[] { "water", "iron", "silicon", "carbon", "aluminum", "uranium", "chemicals" };
            return primaryResources.Any(primary => Name.Contains(primary, StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// Check if this is an energy-related resource
        /// </summary>
        public bool IsEnergyResource()
        {
            var energyResources = new[] { "energy", "fuel", "uranium" };
            return energyResources.Any(energy => Name.Contains(energy, StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// Check if this is a construction material
        /// </summary>
        public bool IsConstructionMaterial()
        {
            var constructionMaterials = new[] { "steel", "glass", "parts", "polymers", "electronics" };
            return constructionMaterials.Any(material => Name.Contains(material, StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// Get resource category for organization
        /// </summary>
        public string GetCategory()
        {
            if (IsPrimaryResource()) return "Primary";
            if (IsEnergyResource()) return "Energy";
            if (IsConstructionMaterial()) return "Construction";
            if (IsManufactured) return "Manufactured";
            if (IsMined) return "Mined";
            return "Other";
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
            return $"ResourceType[{Name}] ({MaterialType}, Index: {Index}, Valid: {IsValid})";
        }
        
        // ==================== STATIC UTILITIES ====================
        
        /// <summary>
        /// SDK-Friendly resource discovery utilities for modable games
        /// ⚠️ NOTE: Per Aspera is fully modable - mods can add any resource keys via YAML
        /// Always use ResourceType.GetByKey("resource_name") for dynamic discovery
        /// These constants are for convenience only, not an exhaustive list
        /// </summary>
        public static class VanillaResources
        {
            /// <summary>
            /// Core vanilla mined resources (verified in game YAML)
            /// </summary>
            public static readonly string[] Mined = 
            {
                "resource_water", "resource_iron", "resource_carbon", "resource_silicon", 
                "resource_aluminum", "resource_chemicals", "resource_uranium"
            };
            
            /// <summary>
            /// Core vanilla manufactured resources (verified in game YAML)
            /// </summary>
            public static readonly string[] Manufactured = 
            {
                "resource_steel", "resource_glass", "resource_parts", 
                "resource_polymers", "resource_electronics", "resource_food"
            };
            
            /// <summary>
            /// Other vanilla resources (fuel, placement, etc.)
            /// </summary>
            public static readonly string[] Other = 
            {
                "resource_fuel", "resource_crater", "resource_heat", "resource_special_site"
            };
            
            /// <summary>
            /// Get all vanilla resource keys (non-exhaustive - mods can add more!)
            /// </summary>
            public static string[] GetAllVanilla() => 
                Mined.Concat(Manufactured).Concat(Other).ToArray();
        }
        
        /// <summary>
        /// Discover all available resources dynamically from the loaded game
        /// This respects mod additions and YAML modifications
        /// </summary>
        /// <returns>List of all resource keys currently available in game</returns>
        public static List<string> DiscoverAllResourceKeys()
        {
            var availableKeys = new List<string>();
            
            // Try vanilla resources first
            foreach (var key in VanillaResources.GetAllVanilla())
            {
                if (GetByKey(key) != null)
                {
                    availableKeys.Add(key);
                }
            }
            
            // TODO: Add reflection-based discovery of all ResourceType instances
            // This would find mod-added resources automatically
            
            return availableKeys;
        }
        
        /// <summary>
        /// <summary>
        /// Get display name using native DisplayName property or fallback to formatted name
        /// This is dynamic and uses the actual game data loaded from YAML
        /// </summary>
        /// <param name="resourceWrapper">ResourceType wrapper instance</param>
        /// <returns>Localized display name from game data</returns>
        public static string GetDynamicDisplayName(ResourceTypeWrapper resourceWrapper)
        {
            if (resourceWrapper?.IsValid == true)
            {
                // Use native display name from YAML data
                var displayName = resourceWrapper.DisplayName;
                if (!string.IsNullOrEmpty(displayName) && displayName != resourceWrapper.Name)
                {
                    return displayName;
                }
            }
            
            // Fallback to formatted key name
            var resourceKey = resourceWrapper?.Name ?? "unknown";
            return ToTitleCase(resourceKey.Replace("resource_", "").Replace("_", " "));
        }
        
        /// <summary>
        /// Get display name using instance method (preferred)
        /// Uses native DisplayName property loaded from YAML
        /// </summary>
        /// <returns>Localized display name from game data</returns>
        public string GetDisplayName()
        {
            return GetDynamicDisplayName(this);
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
        
        // ==================== ATMOSPHERIC GAS DETECTION ====================
        
        /// <summary>
        /// Registry of mod-registered atmospheric gases
        /// Allows mods to register their custom atmospheric gases
        /// </summary>
        private static readonly HashSet<string> _registeredAtmosphericGases = new HashSet<string>
        {
            // Native atmospheric gases (base game)
            "resource_oxygen_release",
            "resource_carbon_dioxide_release", 
            "resource_nitrogen_release",
            "resource_ghg_release"
        };
        
        /// <summary>
        /// Check if this resource is a native atmospheric gas
        /// Uses multiple criteria: MaterialType, key pattern, and known gases list
        /// </summary>
        /// <returns>True if this is a native atmospheric gas</returns>
        public bool IsNativeAtmosphericGas()
        {
            if (string.IsNullOrEmpty(Name)) return false;
            
            // Must be MaterialType.Released (atmospheric)
            if (!isGas) return false;
            
            // Must follow atmospheric gas naming pattern
            if (!Name.EndsWith("_release")) return false;
            
            // Must be in the known native gases list
            return _registeredAtmosphericGases.Contains(Name);
        }
        
        /// <summary>
        /// Check if this resource is any atmospheric gas (native or mod-registered)
        /// Includes both native gases and mod-added atmospheric gases
        /// </summary>
        /// <returns>True if this is an atmospheric gas</returns>
        public bool IsAtmosphericGas()
        {
            if (string.IsNullOrEmpty(Name)) return false;
            
            // Must be MaterialType.Released (atmospheric)
            if (!isGas) return false;
            
            // Must follow atmospheric gas naming pattern OR be registered
            return Name.EndsWith("_release") || _registeredAtmosphericGases.Contains(Name);
        }
        
        /// <summary>
        /// Register a custom atmospheric gas for mod use
        /// Allows mods to add their own atmospheric gases to the system
        /// </summary>
        /// <param name="resourceKey">Resource key (e.g., "resource_custom_gas_release")</param>
        /// <returns>True if successfully registered</returns>
        /// <example>
        /// ResourceTypeWrapper.RegisterAtmosphericGas("resource_methane_release");
        /// ResourceTypeWrapper.RegisterAtmosphericGas("resource_argon_release");
        /// </example>
        public static bool RegisterAtmosphericGas(string resourceKey)
        {
            if (string.IsNullOrEmpty(resourceKey))
            {
                Log.LogWarning("Cannot register atmospheric gas: resource key is null or empty");
                return false;
            }
            
            if (!resourceKey.EndsWith("_release"))
            {
                Log.LogWarning($"Cannot register atmospheric gas '{resourceKey}': must end with '_release'");
                return false;
            }
            
            if (_registeredAtmosphericGases.Contains(resourceKey))
            {
                Log.LogInfo($"Atmospheric gas '{resourceKey}' already registered");
                return true; // Already registered, not an error
            }
            
            _registeredAtmosphericGases.Add(resourceKey);
            Log.LogInfo($"Registered new atmospheric gas: {resourceKey}");
            return true;
        }
        
        /// <summary>
        /// Get all registered atmospheric gas resource keys
        /// Returns both native and mod-registered atmospheric gases
        /// </summary>
        /// <returns>List of all atmospheric gas resource keys</returns>
        public static List<string> GetAllAtmosphericGasKeys()
        {
            return _registeredAtmosphericGases.ToList();
        }
        
        /// <summary>
        /// Get native atmospheric gas resource keys only
        /// Returns only the base game atmospheric gases
        /// </summary>
        /// <returns>List of native atmospheric gas resource keys</returns>
        public static List<string> GetNativeAtmosphericGasKeys()
        {
            return new List<string>
            {
                "resource_oxygen_release",
                "resource_carbon_dioxide_release",
                "resource_nitrogen_release", 
                "resource_ghg_release"
            };
        }
        
        // ==================== RESOURCE ENUMERATION ====================
        
        /// <summary>
        /// Get all available ResourceType instances from the ResourcesPanel cache
        /// Includes both vanilla resources and mod-added resources
        /// This provides complete enumeration of all resources known to the game
        /// </summary>
        /// <returns>List of all ResourceTypeWrapper instances, or empty list if unavailable</returns>
        /// <example>
        /// <code>
        /// var allResources = ResourceTypeWrapper.GetAllResourcesFromCache();
        /// foreach (var resource in allResources)
        /// {
        ///     Console.WriteLine($"{resource.Name}: {resource.DisplayName}");
        /// }
        /// </code>
        /// </example>
        public static List<ResourceTypeWrapper> GetAllResourcesFromCache()
        {
            try
            {
                var resourcesPanel = ResourcesPanelWrapper.GetCurrent();
                if (resourcesPanel == null)
                {
                    Log.LogWarning("ResourcesPanel not available, cannot enumerate resources");
                    return new List<ResourceTypeWrapper>();
                }
                
                var cachedTypes = resourcesPanel.resourceTypesCached;
                if (cachedTypes == null || cachedTypes.Count == 0)
                {
                    Log.LogWarning("Resource types cache is empty or null");
                    return new List<ResourceTypeWrapper>();
                }
                
                var wrappers = new List<ResourceTypeWrapper>();
                foreach (var nativeResourceType in cachedTypes)
                {
                    if (nativeResourceType != null)
                    {
                        var wrapper = FromNative(nativeResourceType);
                        if (wrapper != null)
                        {
                            wrappers.Add(wrapper);
                        }
                    }
                }
                
                Log.LogInfo($"Successfully enumerated {wrappers.Count} resources from cache");
                return wrappers;
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to get all resources: {ex.Message}");
                return new List<ResourceTypeWrapper>();
            }
        }
    }
}