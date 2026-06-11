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
    /// Wrapper for the native Knowledge class
    /// Provides safe access to knowledge entries and content
    /// DOC: Knowledge.md - Knowledge base entries and information
    /// </summary>
    public class Knowledge : WrapperBase
    {
        /// <summary>
        /// Initialize Knowledge wrapper with native knowledge object
        /// </summary>
        /// <param name="nativeKnowledge">Native knowledge instance from game</param>
        public Knowledge(object nativeKnowledge) : base(nativeKnowledge)
        {
        }

        /// <summary>Wraps a typed interop KnowledgeType proxy.</summary>
        public Knowledge(KnowledgeType nativeKnowledge) : base(nativeKnowledge)
        {
        }

        /// <summary>Typed interop proxy (null when the wrapper is invalid).</summary>
        /// <example>var title = knowledge.NativeKnowledgeType?.title;</example>
        public KnowledgeType? NativeKnowledgeType => GetNativeObject() as KnowledgeType;

        /// <summary>
        /// Create wrapper from native knowledge object
        /// </summary>
        public static Knowledge? FromNative(object? nativeKnowledge)
        {
            return nativeKnowledge != null ? new Knowledge(nativeKnowledge) : null;
        }

        // ==================== CORE IDENTIFICATION ====================

        /// <summary>
        /// Knowledge entry key identifier (typed read of StaticDataCollectionItem.key).
        /// </summary>
        public string Name => NativeKnowledgeType?.key ?? "unknown_knowledge";

        /// <summary>
        /// Knowledge title for display (typed read of KnowledgeType.title).
        /// </summary>
        public string Title => NativeKnowledgeType?.title ?? Name;

        /// <summary>
        /// Knowledge content/description (typed read of KnowledgeType.content).
        /// </summary>
        public string Content => NativeKnowledgeType?.content ?? "No content available";

        /// <summary>AMI variant of the content (typed read of KnowledgeType.contentAMI).</summary>
        public string ContentAMI => NativeKnowledgeType?.contentAMI ?? "";

        /// <summary>N'a jamais existé — KnowledgeType n'a pas d'index.</summary>
        [Obsolete("KnowledgeType.index n'existe pas dans le jeu — retournait toujours -1.", false)]
        public int Index => -1;

        // ==================== KNOWLEDGE ORGANIZATION ====================

        /// <summary>
        /// Knowledge path for hierarchical organization (typed read of KnowledgeType.path).
        /// E.g., "Buildings/Power/SolarPanel".
        /// </summary>
        public string Path => NativeKnowledgeType?.path ?? "Uncategorized";

        /// <summary>
        /// Localized path for UI navigation (typed read of KnowledgeType.localizedPath).
        /// </summary>
        public string LocalizedPath => NativeKnowledgeType?.localizedPath ?? Path;
        
        /// <summary>
        /// Knowledge category (Buildings, Resources, Technologies, etc.)
        /// </summary>
        public string Category
        {
            get
            {
                var path = Path;
                if (string.IsNullOrEmpty(path)) return "General";
                
                var parts = path.Split('/');
                return parts.Length > 0 ? parts[0] : "General";
            }
        }
        
        /// <summary>
        /// Knowledge subcategory
        /// </summary>
        public string SubCategory
        {
            get
            {
                var path = Path;
                if (string.IsNullOrEmpty(path)) return "";
                
                var parts = path.Split('/');
                return parts.Length > 1 ? parts[1] : "";
            }
        }
        
        // ==================== CONTENT DETAILS ====================
        
        /// <summary>
        /// Content table with structured information (typed read of KnowledgeType.contentTable).
        /// ⚠️ L'ancienne version invoquait get_field/get_text sur l'objet Knowledge au lieu
        /// de l'entrée (bug) — retournait des entrées vides.
        /// </summary>
        public List<KnowledgeTableEntry> GetContentTable()
        {
            var tableEntries = new List<KnowledgeTableEntry>();
            var contentTable = NativeKnowledgeType?.contentTable;
            if (contentTable == null) return tableEntries;
            foreach (var entry in contentTable)
                if (entry != null)
                    tableEntries.Add(new KnowledgeTableEntry(entry.field ?? "", entry.text ?? ""));
            return tableEntries;
        }

        /// <summary>Associated image sprite (typed read of KnowledgeType.image).</summary>
        public UnityEngine.Sprite? Image => NativeKnowledgeType?.image;

        /// <summary>
        /// Icon sprite name for UI display.
        /// ⚠️ « iconName » natif est un Sprite, pas une string — l'ancien binding string
        /// échouait et retournait toujours le fallback.
        /// </summary>
        public string IconName => NativeKnowledgeType?.iconName?.name ?? "KnowledgeBase/Unknown";

        /// <summary>Building associated to this knowledge entry, when any (typed).</summary>
        public BuildingTypeWrapper? GetContentBuilding()
        {
            var bt = NativeKnowledgeType?.contentBuilding;
            return bt != null ? new BuildingTypeWrapper(bt) : null;
        }
        
        // ==================== KNOWLEDGE TYPES ====================
        
        /// <summary>
        /// Check if this is building-related knowledge
        /// </summary>
        public bool IsBuildingKnowledge()
        {
            return Category.Equals("Buildings", StringComparison.OrdinalIgnoreCase) ||
                   Name.Contains("building", StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Check if this is resource-related knowledge
        /// </summary>
        public bool IsResourceKnowledge()
        {
            return Category.Equals("Resources", StringComparison.OrdinalIgnoreCase) ||
                   Name.Contains("resource", StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Check if this is technology-related knowledge
        /// </summary>
        public bool IsTechnologyKnowledge()
        {
            return Category.Equals("Technologies", StringComparison.OrdinalIgnoreCase) ||
                   Name.Contains("tech", StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Check if this is terraforming-related knowledge
        /// </summary>
        public bool IsTerraformingKnowledge()
        {
            return Path.Contains("Terraforming", StringComparison.OrdinalIgnoreCase) ||
                   Name.Contains("terraform", StringComparison.OrdinalIgnoreCase);
        }
        
        /// <summary>
        /// Check if this is gameplay mechanics knowledge
        /// </summary>
        public bool IsGameplayKnowledge()
        {
            var gameplayTerms = new[] { "gameplay", "mechanics", "controls", "tutorial" };
            return gameplayTerms.Any(term => Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                                           Path.Contains(term, StringComparison.OrdinalIgnoreCase));
        }
        
        // ==================== UNLOCK STATUS ====================
        
        /// <summary>N'a jamais fonctionné — aucun de ces noms n'existe sur KnowledgeType.</summary>
        [Obsolete("IsUnlockedFor/IsKnown/HasKnowledge n'existent pas sur KnowledgeType — retournait toujours false. Le déblocage de knowledge se gère côté Faction.", false)]
        public bool IsUnlockedFor(FactionWrapper faction) => false;

        /// <summary>N'a jamais fonctionné — aucun de ces noms n'existe sur KnowledgeType.</summary>
        [Obsolete("UnlockFor/Unlock/Grant n'existent pas sur KnowledgeType — n'a jamais rien débloqué. Le déblocage de knowledge se gère côté Faction.", false)]
        public bool UnlockFor(FactionWrapper faction) => false;
        
        // ==================== SEARCH & FILTERING ====================
        
        /// <summary>
        /// Check if knowledge matches search terms
        /// </summary>
        /// <param name="searchTerms">Terms to search for</param>
        /// <returns>True if any term matches title, content, or path</returns>
        public bool MatchesSearch(params string[] searchTerms)
        {
            if (searchTerms == null || searchTerms.Length == 0) return true;
            
            var searchText = $"{Title} {Content} {Path}".ToLowerInvariant();
            return searchTerms.Any(term => searchText.Contains(term.ToLowerInvariant()));
        }
        
        /// <summary>
        /// Get knowledge relevance score for search terms
        /// </summary>
        /// <param name="searchTerms">Terms to score against</param>
        /// <returns>Relevance score (0-100)</returns>
        public int GetSearchRelevance(params string[] searchTerms)
        {
            if (searchTerms == null || searchTerms.Length == 0) return 0;
            
            var score = 0;
            var title = Title.ToLowerInvariant();
            var content = Content.ToLowerInvariant();
            var path = Path.ToLowerInvariant();
            
            foreach (var term in searchTerms)
            {
                var lowerTerm = term.ToLowerInvariant();
                if (title.Contains(lowerTerm)) score += 30;
                if (path.Contains(lowerTerm)) score += 20;
                if (content.Contains(lowerTerm)) score += 10;
            }
            
            return Math.Min(100, score);
        }
        
        // ==================== UTILITIES ====================
        
        /// <summary>
        /// Get content summary (first 200 characters)
        /// </summary>
        public string GetContentSummary(int maxLength = 200)
        {
            var content = Content;
            if (content.Length <= maxLength) return content;
            
            var truncated = content.Substring(0, maxLength);
            var lastSpace = truncated.LastIndexOf(' ');
            if (lastSpace > 0) truncated = truncated.Substring(0, lastSpace);
            
            return truncated + "...";
        }
        
        /// <summary>
        /// Get localized display title from game data
        /// Uses native Title property loaded from YAML
        /// </summary>
        /// <returns>Localized title from game data</returns>
        public string GetDisplayTitle()
        {
            // Use native title from YAML data
            var title = Title;
            if (!string.IsNullOrEmpty(title) && title != Name)
            {
                return title;
            }
            
            // Fallback to formatted key name
            return ToTitleCase(Name.Replace("knowledge_", "").Replace("_", " "));
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
            return $"Knowledge[{Name}] ({Category}, Valid: {IsValid})";
        }
    }
    
    // ==================== SUPPORTING CLASSES ====================
    
    /// <summary>
    /// Represents a content table entry in knowledge
    /// </summary>
    public class KnowledgeTableEntry
    {
        /// <summary>
        /// Field name in the knowledge table
        /// </summary>
        public string Field { get; }
        /// <summary>
        /// Text content for the field
        /// </summary>
        public string Text { get; }
        
        /// <summary>
        /// Initialize knowledge table entry
        /// </summary>
        /// <param name="field">Field name</param>
        /// <param name="text">Text content</param>
        public KnowledgeTableEntry(string field, string text)
        {
            Field = field ?? "";
            Text = text ?? "";
        }
        
        public override string ToString()
        {
            return $"{Field}: {Text}";
        }
    }
}
#pragma warning restore CS1591
