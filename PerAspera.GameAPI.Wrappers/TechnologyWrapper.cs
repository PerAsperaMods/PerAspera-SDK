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
    /// Implements IYamlTypeWrapper for unified game data access
    /// </summary>
    public class TechnologyWrapper : WrapperBase, IYamlTypeWrapper
    {
        /// <summary>
        /// Initialize Technology wrapper with native TechnologyType object
        /// </summary>
        /// <param name="nativeTechnology">Native TechnologyType instance from game</param>
        public TechnologyWrapper(object nativeTechnology) : base(nativeTechnology)
        {
        }

        /// <summary>Wraps a typed interop TechnologyType proxy.</summary>
        public TechnologyWrapper(TechnologyType nativeTechnology) : base(nativeTechnology)
        {
        }

        /// <summary>Typed interop proxy (null when the wrapper is invalid).</summary>
        /// <example>float pts = tech.NativeTechnologyType?.requiredResearchPoints ?? 0f;</example>
        public TechnologyType? NativeTechnologyType => GetNativeObject() as TechnologyType;

        /// <summary>
        /// Create wrapper from native TechnologyType object
        /// </summary>
        public static TechnologyWrapper? FromNative(object? nativeTechnology)
        {
            return nativeTechnology != null ? new TechnologyWrapper(nativeTechnology) : null;
        }

        // ==================== CORE IDENTIFICATION ====================

        /// <summary>
        /// Technology key identifier (typed read of StaticDataCollectionItem.key).
        /// E.g., "tech_advanced_solar".
        /// </summary>
        public string Name => NativeTechnologyType?.key ?? "unknown_tech";

        /// <summary>
        /// Technology display name for UI (typed read of TechnologyType.name).
        /// </summary>
        public string DisplayName => NativeTechnologyType?.name ?? Name;

        /// <summary>Short name for compact UI (typed read of TechnologyType.shortName).</summary>
        public string ShortName => NativeTechnologyType?.shortName ?? DisplayName;

        /// <summary>
        /// Technology description (typed read of TechnologyType.description).
        /// </summary>
        public string Description => NativeTechnologyType?.description ?? "No description available";

        /// <summary>N'a jamais existé — TechnologyType n'a pas d'index.</summary>
        [Obsolete("TechnologyType.index n'existe pas dans le jeu — retournait toujours -1.", false)]
        public int Index => -1;

        // ==================== RESEARCH TREE ====================

        /// <summary>
        /// Technology category (typed read of TechnologyType.categoryName).
        /// (L'ancienne chaîne category/branch/tree n'existait pas.)
        /// </summary>
        public string Category => NativeTechnologyType?.categoryName ?? "General";

        /// <summary>Position in the tech tree lane (typed read of TechnologyType.position).</summary>
        public int Position => NativeTechnologyType?.position ?? 0;

        /// <summary>True when this tech can be researched repeatedly (typed).</summary>
        public bool IsRepeatable => NativeTechnologyType?.isRepeatable ?? false;

        /// <summary>N'a jamais existé — tier/level n'existent pas sur TechnologyType.</summary>
        [Obsolete("TechnologyType.tier/level n'existent pas — retournait toujours 1. Voir Position (position dans la lane).", false)]
        public int Tier => 1;

        /// <summary>
        /// Research cost in research points (typed read of TechnologyType.requiredResearchPoints).
        /// (L'ancienne chaîne cost/researchCost n'existait pas — retournait toujours 0.)
        /// </summary>
        public float ResearchCost => NativeTechnologyType?.requiredResearchPoints ?? 0f;

        /// <summary>N'a jamais existé — pas de durée fixe, le temps dépend des points/jour.</summary>
        [Obsolete("TechnologyType.researchTime/duration n'existent pas — retournait toujours 0. Le temps = ResearchCost / points de recherche par jour de la faction.", false)]
        public float ResearchTime => 0f;
        
        // ==================== PREREQUISITES ====================
        
        /// <summary>
        /// Prerequisites needed to unlock this technology (typed read of TechnologyType.requirements).
        /// (L'ancienne chaîne prerequisites/requiredTechs/dependencies n'existait pas.)
        /// </summary>
        public List<TechnologyWrapper> GetPrerequisites()
        {
            var prereqList = new List<TechnologyWrapper>();
            var requirements = NativeTechnologyType?.requirements;
            if (requirements == null) return prereqList;
            foreach (var prereq in requirements)
                if (prereq != null) prereqList.Add(new TechnologyWrapper(prereq));
            return prereqList;
        }
        
        /// <summary>
        /// Check if all prerequisites are met for a faction
        /// </summary>
        /// <param name="faction">Faction to check prerequisites for</param>
        /// <returns>True if all prerequisites are researched</returns>
        public bool ArePrerequisitesMet(FactionWrapper faction)
        {
            if (!faction.IsValidWrapper) return false;
            
            var prerequisites = GetPrerequisites();
            return prerequisites.All(prereq => faction.HasTechnology(prereq.Name));
        }
        
        // ==================== UNLOCKS ====================
        
        /// <summary>
        /// Unlock actions of this technology (typed read of TechnologyType.actions).
        /// C'est le vrai mécanisme d'unlock : chaque TextAction décrit ce que la techno
        /// débloque (bâtiments, ressources, effets…).
        /// </summary>
        /// <example>foreach (var action in tech.GetActions()) { ... }</example>
        public List<TextActionWrapper> GetActions()
        {
            var result = new List<TextActionWrapper>();
            var actions = NativeTechnologyType?.actions;
            if (actions == null) return result;
            foreach (var action in actions)
                if (action != null) result.Add(new TextActionWrapper(action));
            return result;
        }

        /// <summary>N'a jamais fonctionné — ces membres n'existent pas.</summary>
        [Obsolete("unlockedBuildings/buildings/unlocks n'existent pas sur TechnologyType — retournait toujours vide. Les unlocks passent par TechnologyType.actions : voir GetActions().", false)]
        public List<object> GetUnlockedBuildings() => new List<object>();

        /// <summary>N'a jamais fonctionné — ces membres n'existent pas.</summary>
        [Obsolete("unlockedResources/resources n'existent pas sur TechnologyType — retournait toujours vide. Les unlocks passent par TechnologyType.actions : voir GetActions().", false)]
        public List<ResourceTypeWrapper> GetUnlockedResources() => new List<ResourceTypeWrapper>();
        
        // ==================== RESEARCH STATUS ====================
        
        /// <summary>
        /// Check if this technology is researched by a faction
        /// </summary>
        /// <param name="faction">Faction to check research status for</param>
        /// <returns>True if technology is researched</returns>
        public bool IsResearchedBy(FactionWrapper faction)
        {
            return faction.IsValidWrapper && faction.HasTechnology(Name);
        }
        
        /// <summary>
        /// Check if this technology can be researched by a faction
        /// (prerequisites met but not yet researched)
        /// </summary>
        /// <param name="faction">Faction to check availability for</param>
        /// <returns>True if technology can be researched</returns>
        public bool IsAvailableFor(FactionWrapper faction)
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
        public bool StartResearch(FactionWrapper faction)
        {
            if (!faction.IsValidWrapper) return false;
            
            return faction.ResearchTechnology(Name);
        }
        
        // ==================== UTILITIES ====================
        
        /// <summary>
        /// Icon sprite name (typed — « iconName » natif est un Sprite, pas une string ;
        /// l'ancien binding iconPath/icon n'existait pas).
        /// </summary>
        public string IconPath => NativeTechnologyType?.iconName?.name ?? $"Technology Icons/{Name}";

        /// <summary>Icon sprite for UI display (typed).</summary>
        public UnityEngine.Sprite? Icon => NativeTechnologyType?.iconName;

        /// <summary>
        /// Check if this is a key/milestone technology
        /// </summary>
        public bool IsMilestoneTechnology()
        {
            var milestones = new[] { "terraforming", "colonization", "space", "advanced", "expert" };
            return milestones.Any(milestone => Name.Contains(milestone, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>Reposait sur le Tier fantôme — retournait toujours 1.</summary>
        [Obsolete("Reposait sur Tier qui n'a jamais existé — retournait toujours 1. Utiliser ResearchCost ou Position pour estimer la complexité.", false)]
        public int GetComplexityRating() => 1;

        /// <summary>Reposait sur le ResearchTime fantôme — retournait toujours « 0 minutes ».</summary>
        [Obsolete("Reposait sur ResearchTime qui n'a jamais existé. Le temps = ResearchCost / points de recherche par jour de la faction.", false)]
        public string GetEstimatedResearchTime() => "unknown";
        
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
        /// String representation for debugging
        /// </summary>
        public override string ToString()
        {
            return $"Technology[{Name}] (Category: {Category}, Cost: {ResearchCost}, Valid: {IsValid})";
        }

        // ==================== IYamlTypeWrapper IMPLEMENTATION ====================

        /// <summary>
        /// Unique key identifier for this technology type
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
                "shortname" => ShortName,
                "description" => Description,
                "category" => Category,
                "position" => Position,
                "isrepeatable" => IsRepeatable,
                "researchcost" => ResearchCost,
                "isvalid" => IsValid,
                _ => SafeInvoke<object>(propertyName)
            };
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