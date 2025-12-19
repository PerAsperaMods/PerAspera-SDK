#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Native;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native TechnologyType class
    /// Provides safe access to technology type definitions and properties
    /// DOC: TechnologyType.md - Technology definitions loaded from YAML
    /// </summary>
    public class Technology : WrapperBase
    {
        /// <summary>
        /// Initialize Technology wrapper with native TechnologyType object
        /// </summary>
        /// <param name="nativeTechnology">Native TechnologyType instance from game</param>
        public Technology(object nativeTechnology) : base(nativeTechnology)
        {
        }
        
        /// <summary>
        /// Create wrapper from native TechnologyType object
        /// </summary>
        public static Technology? FromNative(object? nativeTechnology)
        {
            return nativeTechnology != null ? new Technology(nativeTechnology) : null;
        }
        
        // ==================== CORE IDENTIFICATION ====================
        
        /// <summary>
        /// Technology name/key identifier
        /// Maps to: name field (e.g., "tech_advanced_solar", "tech_water_extraction")
        /// </summary>
        public string Name
        {
            get => SafeInvoke<string>("get_name") ?? "unknown_tech";
        }
        
        /// <summary>
        /// Technology display name for UI
        /// Maps to: displayName or localizedName field
        /// </summary>
        public string DisplayName
        {
            get => SafeInvoke<string>("get_displayName") ?? 
                   SafeInvoke<string>("get_localizedName") ?? 
                   SafeInvoke<string>("get_title") ?? Name;
        }
        
        /// <summary>
        /// Technology description
        /// Maps to: description field
        /// </summary>
        public string Description
        {
            get => SafeInvoke<string>("get_description") ?? 
                   SafeInvoke<string>("get_content") ?? "No description available";
        }
        
        /// <summary>
        /// Technology index for efficient lookups
        /// Maps to: index field
        /// </summary>
        public int Index
        {
            get => SafeInvoke<int?>("get_index") ?? -1;
        }
        
        // ==================== RESEARCH TREE ====================
        
        /// <summary>
        /// Technology category/branch (Engineering, Biology, Space)
        /// Maps to: category or branch field
        /// </summary>
        public string Category
        {
            get => SafeInvoke<string>("get_category") ?? 
                   SafeInvoke<string>("get_branch") ?? 
                   SafeInvoke<string>("get_tree") ?? "General";
        }
        
        /// <summary>
        /// Technology tier/level (1, 2, 3, etc.)
        /// Maps to: tier or level field
        /// </summary>
        public int Tier
        {
            get => SafeInvoke<int?>("get_tier") ?? 
                   SafeInvoke<int?>("get_level") ?? 1;
        }
        
        /// <summary>
        /// Research cost in research points
        /// Maps to: cost or researchCost field
        /// </summary>
        public float ResearchCost
        {
            get => SafeInvoke<float?>("get_cost") ?? 
                   SafeInvoke<float?>("get_researchCost") ?? 0f;
        }
        
        /// <summary>
        /// Time required to research (in game time units)
        /// Maps to: researchTime or duration field
        /// </summary>
        public float ResearchTime
        {
            get => SafeInvoke<float?>("get_researchTime") ?? 
                   SafeInvoke<float?>("get_duration") ?? 0f;
        }
        
        // ==================== PREREQUISITES ====================
        
        /// <summary>
        /// Prerequisites needed to unlock this technology
        /// Maps to: prerequisites or requiredTechs array
        /// </summary>
        public List<Technology> GetPrerequisites()
        {
            try
            {
                var prerequisites = SafeInvoke<object>("get_prerequisites") ?? 
                                  SafeInvoke<object>("get_requiredTechs") ??
                                  SafeInvoke<object>("get_dependencies");
                
                var prereqList = new List<Technology>();
                
                if (prerequisites is System.Collections.IEnumerable enumerable)
                {
                    foreach (var prereq in enumerable)
                    {
                        if (prereq != null)
                        {
                            prereqList.Add(new Technology(prereq));
                        }
                    }
                }
                
                return prereqList;
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to get prerequisites for technology {Name}: {ex.Message}");
                return new List<Technology>();
            }
        }
        
        /// <summary>
        /// Check if all prerequisites are met for a faction
        /// </summary>
        /// <param name="faction">Faction to check prerequisites for</param>
        /// <returns>True if all prerequisites are researched</returns>
        public bool ArePrerequisitesMet(Faction faction)
        {
            if (!faction.IsValidWrapper) return false;
            
            var prerequisites = GetPrerequisites();
            return prerequisites.All(prereq => faction.HasTechnology(prereq.Name));
        }
        
        // ==================== UNLOCKS ====================
        
        /// <summary>
        /// Buildings unlocked by this technology
        /// Maps to: unlockedBuildings array
        /// </summary>
        public List<object> GetUnlockedBuildings()
        {
            try
            {
                var buildings = SafeInvoke<object>("get_unlockedBuildings") ?? 
                              SafeInvoke<object>("get_buildings") ??
                              SafeInvoke<object>("get_unlocks");
                
                var buildingList = new List<object>();
                
                if (buildings is System.Collections.IEnumerable enumerable)
                {
                    foreach (var building in enumerable)
                    {
                        if (building != null)
                        {
                            buildingList.Add(building);
                        }
                    }
                }
                
                return buildingList;
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to get unlocked buildings for technology {Name}: {ex.Message}");
                return new List<object>();
            }
        }
        
        /// <summary>
        /// Resources unlocked by this technology
        /// Maps to: unlockedResources array
        /// </summary>
        public List<ResourceType> GetUnlockedResources()
        {
            try
            {
                var resources = SafeInvoke<object>("get_unlockedResources") ?? 
                              SafeInvoke<object>("get_resources");
                
                var resourceList = new List<ResourceType>();
                
                if (resources is System.Collections.IEnumerable enumerable)
                {
                    foreach (var resource in enumerable)
                    {
                        if (resource != null)
                        {
                            resourceList.Add(new ResourceType(resource));
                        }
                    }
                }
                
                return resourceList;
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to get unlocked resources for technology {Name}: {ex.Message}");
                return new List<ResourceType>();
            }
        }
        
        // ==================== RESEARCH STATUS ====================
        
        /// <summary>
        /// Check if this technology is researched by a faction
        /// </summary>
        /// <param name="faction">Faction to check research status for</param>
        /// <returns>True if technology is researched</returns>
        public bool IsResearchedBy(Faction faction)
        {
            return faction.IsValidWrapper && faction.HasTechnology(Name);
        }
        
        /// <summary>
        /// Check if this technology can be researched by a faction
        /// (prerequisites met but not yet researched)
        /// </summary>
        /// <param name="faction">Faction to check availability for</param>
        /// <returns>True if technology can be researched</returns>
        public bool IsAvailableFor(Faction faction)
        {
            return faction.IsValidWrapper && 
                   !IsResearchedBy(faction) && 
                   ArePrerequisitesMet(faction);
        }
        
        /// <summary>
        /// Start research of this technology for a faction
        /// </summary>
        /// <param name="faction">Faction to research technology for</param>
        /// <returns>True if research was initiated successfully</returns>
        public bool StartResearch(Faction faction)
        {
            if (!faction.IsValidWrapper) return false;
            
            return faction.ResearchTechnology(Name);
        }
        
        // ==================== UTILITIES ====================
        
        /// <summary>
        /// Get technology icon path
        /// Maps to: iconPath or icon field
        /// </summary>
        public string IconPath
        {
            get => SafeInvoke<string>("get_iconPath") ?? 
                   SafeInvoke<string>("get_icon") ?? 
                   $"Technology Icons/{Name}";
        }
        
        /// <summary>
        /// Check if this is a key/milestone technology
        /// </summary>
        public bool IsMilestoneTechnology()
        {
            var milestones = new[] { "terraforming", "colonization", "space", "advanced", "expert" };
            return milestones.Any(milestone => Name.Contains(milestone, StringComparison.OrdinalIgnoreCase));
        }
        
        /// <summary>
        /// Get technology complexity rating (1-5)
        /// </summary>
        public int GetComplexityRating()
        {
            if (Tier <= 1) return 1;
            if (Tier <= 2) return 2;
            if (Tier <= 3) return 3;
            if (IsMilestoneTechnology()) return 5;
            return 4;
        }
        
        /// <summary>
        /// Get estimated research time in human-readable format
        /// </summary>
        public string GetEstimatedResearchTime()
        {
            var time = ResearchTime;
            if (time < 60) return $"{time:F0} minutes";
            if (time < 1440) return $"{time/60:F1} hours";
            return $"{time/1440:F1} days";
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
            return ToTitleCase(Name.Replace("tech_", "").Replace("technology_", "").Replace("_", " "));
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
            return $"Technology[{Name}] (Tier {Tier}, Category: {Category}, Valid: {IsValid})";
        }
        
        // ==================== STATIC UTILITIES ====================
        
        /// <summary>
        /// Common technology key constants for easy reference
        /// These are the YAML keys used in the game data files
        /// </summary>
        public static class CommonTechnologies
        {
            // Basic Technologies
            /// <summary>
            /// Basic engineering technology identifier
            /// </summary>
            public const string BasicEngineering = "tech_basic_engineering";
            /// <summary>
            /// Basic physics technology identifier
            /// </summary>
            public const string BasicPhysics = "tech_basic_physics";
            /// <summary>
            /// Basic chemistry technology identifier
            /// </summary>
            public const string BasicChemistry = "tech_basic_chemistry";
            
            // Solar Power
            /// <summary>
            /// Solar power tier 1 technology identifier
            /// </summary>
            public const string SolarPowerTier1 = "tech_solar_power_1";
            /// <summary>
            /// Solar power tier 2 technology identifier
            /// </summary>
            public const string SolarPowerTier2 = "tech_solar_power_2";
            /// <summary>
            /// Advanced solar power technology identifier
            /// </summary>
            public const string AdvancedSolar = "tech_advanced_solar";
            
            // Resource Extraction
            /// <summary>
            /// Water extraction tier 1 technology identifier
            /// </summary>
            public const string WaterExtractionTier1 = "tech_water_extraction_1";
            /// <summary>
            /// Mining tier 1 technology identifier
            /// </summary>
            public const string MiningTier1 = "tech_mining_1";
            /// <summary>
            /// Advanced mining technology identifier
            /// </summary>
            public const string AdvancedMining = "tech_advanced_mining";
            
            // Manufacturing
            /// <summary>
            /// Basic manufacturing technology identifier
            /// </summary>
            public const string BasicManufacturing = "tech_basic_manufacturing";
            /// <summary>
            /// Advanced manufacturing technology identifier
            /// </summary>
            public const string AdvancedManufacturing = "tech_advanced_manufacturing";
            /// <summary>
            /// Automation technology identifier
            /// </summary>
            public const string Automation = "tech_automation";
            
            // Terraforming
            /// <summary>
            /// Atmospheric processing technology identifier
            /// </summary>
            public const string AtmosphericProcessing = "tech_atmospheric_processing";
            /// <summary>
            /// Terraforming tier 1 technology identifier
            /// </summary>
            public const string TerraformingTier1 = "tech_terraforming_1";
            /// <summary>
            /// Advanced terraforming technology identifier
            /// </summary>
            public const string AdvancedTerraforming = "tech_advanced_terraforming";
        }
    }
}