#nullable enable
using System;
using System.Collections.Generic;
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
                            if (icon?.ToString() != null)
                            {
                                iconList.Add(icon.ToString());
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
        /// Common resource type constants for easy reference
        /// </summary>
        public static class CommonResources
        {
            public const string Water = "resource_water";
            public const string Iron = "resource_iron";
            public const string Silicon = "resource_silicon";
            public const string Carbon = "resource_carbon";
            public const string Aluminum = "resource_aluminum";
            public const string Uranium = "resource_uranium";
            public const string Chemicals = "resource_chemicals";
            
            public const string Steel = "resource_steel";
            public const string Glass = "resource_glass";
            public const string Parts = "resource_parts";
            public const string Polymers = "resource_polymers";
            public const string Electronics = "resource_electronics";
            public const string Food = "resource_food";
            public const string Fuel = "resource_fuel";
            
            public const string Energy = "resource_energy";
            public const string Oxygen = "resource_oxygen";
        }
        
        /// <summary>
        /// Get display name for common resources
        /// </summary>
        public static string GetDisplayName(string resourceKey)
        {
            return resourceKey switch
            {
                CommonResources.Water => "Water",
                CommonResources.Iron => "Iron",
                CommonResources.Silicon => "Silicon",
                CommonResources.Carbon => "Carbon",
                CommonResources.Aluminum => "Aluminum",
                CommonResources.Uranium => "Uranium",
                CommonResources.Chemicals => "Chemicals",
                CommonResources.Steel => "Steel",
                CommonResources.Glass => "Glass",
                CommonResources.Parts => "Parts",
                CommonResources.Polymers => "Polymers",
                CommonResources.Electronics => "Electronics",
                CommonResources.Food => "Food",
                CommonResources.Fuel => "Fuel",
                CommonResources.Energy => "Energy",
                CommonResources.Oxygen => "Oxygen",
                _ => resourceKey.Replace("resource_", "").Replace("_", " ").ToTitleCase()
            };
        }
    }
}