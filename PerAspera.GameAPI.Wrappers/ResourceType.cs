#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Native;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native ResourceType class
    /// Provides safe access to resource type definitions and properties
    /// DOC: Resource.md - Resource definitions and properties
    /// </summary>
    public class ResourceType : WrapperBase
    {
        /// <summary>
        /// Initialize ResourceType wrapper with native resource type object
        /// </summary>
        /// <param name="nativeResourceType">Native resource type instance from game</param>
        public ResourceType(object nativeResourceType) : base(nativeResourceType)
        {
        }
        
        /// <summary>
        /// Create wrapper from native resource type object
        /// </summary>
        public static ResourceType? FromNative(object? nativeResourceType)
        {
            return nativeResourceType != null ? new ResourceType(nativeResourceType) : null;
        }
        
        // ==================== CORE IDENTIFICATION ====================
        
        /// <summary>
        /// Resource type name/key identifier
        /// Maps to: name field (e.g., "resource_water", "resource_iron")
        /// </summary>
        public string Name
        {
            get => SafeInvoke<string>("get_name") ?? "unknown_resource";
        }
        
        /// <summary>
        /// Resource display name for UI
        /// Maps to: displayName or localizedName field
        /// </summary>
        public string DisplayName
        {
            get => SafeInvoke<string>("get_displayName") ?? 
                   SafeInvoke<string>("get_localizedName") ?? Name;
        }
        
        /// <summary>
        /// Resource index for efficient lookups
        /// Maps to: index field
        /// </summary>
        public int Index
        {
            get => SafeInvoke<int?>("get_index") ?? -1;
        }
        
        /// <summary>
        /// Resource color for UI display
        /// Maps to: color field
        /// </summary>
        public string ColorHex
        {
            get => SafeInvoke<string>("get_color") ?? "FFFFFF";
        }
        
        // ==================== RESOURCE PROPERTIES ====================
        
        /// <summary>
        /// Material type category (Mined, Manufactured, etc.)
        /// Maps to: materialType field
        /// </summary>
        public string MaterialType
        {
            get => SafeInvoke<string>("get_materialType") ?? "Unknown";
        }
        
        /// <summary>
        /// Is this a mined/extracted resource?
        /// </summary>
        public bool IsMined
        {
            get => MaterialType.Equals("Mined", StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Is this a manufactured/processed resource?
        /// </summary>
        public bool IsManufactured
        {
            get => MaterialType.Equals("Manufactured", StringComparison.OrdinalIgnoreCase);
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
                    Log.Warning($"Failed to get vein icons for resource {Name}: {ex.Message}");
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
                Log.Warning($"Failed to parse color '{ColorHex}' for resource {Name}: {ex.Message}");
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
        /// Common resource type key constants for easy reference
        /// These are the YAML keys used in the game data files
        /// </summary>
        public static class CommonResources
        {
            /// <summary>
            /// Water resource identifier
            /// </summary>
            public const string Water = "resource_water";
            /// <summary>
            /// Iron ore resource identifier
            /// </summary>
            public const string Iron = "resource_iron";
            /// <summary>
            /// Silicon resource identifier
            /// </summary>
            public const string Silicon = "resource_silicon";
            /// <summary>
            /// Carbon resource identifier
            /// </summary>
            public const string Carbon = "resource_carbon";
            /// <summary>
            /// Aluminum resource identifier
            /// </summary>
            public const string Aluminum = "resource_aluminum";
            /// <summary>
            /// Uranium resource identifier
            /// </summary>
            public const string Uranium = "resource_uranium";
            /// <summary>
            /// Chemicals resource identifier
            /// </summary>
            public const string Chemicals = "resource_chemicals";
            
            /// <summary>
            /// Steel manufactured resource identifier
            /// </summary>
            public const string Steel = "resource_steel";
            /// <summary>
            /// Glass manufactured resource identifier
            /// </summary>
            public const string Glass = "resource_glass";
            /// <summary>
            /// Parts manufactured resource identifier
            /// </summary>
            public const string Parts = "resource_parts";
            /// <summary>
            /// Polymers manufactured resource identifier
            /// </summary>
            public const string Polymers = "resource_polymers";
            /// <summary>
            /// Electronics manufactured resource identifier
            /// </summary>
            public const string Electronics = "resource_electronics";
            /// <summary>
            /// Food resource identifier
            /// </summary>
            public const string Food = "resource_food";
            /// <summary>
            /// Fuel resource identifier
            /// </summary>
            public const string Fuel = "resource_fuel";
            
            /// <summary>
            /// Energy resource identifier
            /// </summary>
            public const string Energy = "resource_energy";
            /// <summary>
            /// Oxygen atmospheric resource identifier
            /// </summary>
            public const string Oxygen = "resource_oxygen";
        }
        
        /// <summary>
        /// Get display name using native DisplayName property or fallback to formatted name
        /// This is dynamic and uses the actual game data loaded from YAML
        /// </summary>
        /// <param name="resourceWrapper">ResourceType wrapper instance</param>
        /// <returns>Localized display name from game data</returns>
        public static string GetDynamicDisplayName(ResourceType resourceWrapper)
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
    }
}