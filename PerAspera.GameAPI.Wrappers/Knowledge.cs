#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Native;

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
        
        /// <summary>
        /// Create wrapper from native knowledge object
        /// </summary>
        public static Knowledge? FromNative(object? nativeKnowledge)
        {
            return nativeKnowledge != null ? new Knowledge(nativeKnowledge) : null;
        }
        
        // ==================== CORE IDENTIFICATION ====================
        
        /// <summary>
        /// Knowledge entry name/key identifier
        /// Maps to: name field (e.g., "knowledge_water", "knowledge_solar_panel")
        /// </summary>
        public string Name
        {
            get => SafeInvoke<string>("get_name") ?? "unknown_knowledge";
        }
        
        /// <summary>
        /// Knowledge title for display
        /// Maps to: title field
        /// </summary>
        public string Title
        {
            get => SafeInvoke<string>("get_title") ?? 
                   SafeInvoke<string>("get_displayName") ?? Name;
        }
        
        /// <summary>
        /// Knowledge content/description
        /// Maps to: content field
        /// </summary>
        public string Content
        {
            get => SafeInvoke<string>("get_content") ?? 
                   SafeInvoke<string>("get_description") ?? "No content available";
        }
        
        /// <summary>
        /// Knowledge index for efficient lookups
        /// Maps to: index field
        /// </summary>
        public int Index
        {
            get => SafeInvoke<int?>("get_index") ?? -1;
        }
        
        // ==================== KNOWLEDGE ORGANIZATION ====================
        
        /// <summary>
        /// Knowledge path for hierarchical organization
        /// Maps to: path field (e.g., "Buildings/Power/SolarPanel")
        /// </summary>
        public string Path
        {
            get => SafeInvoke<string>("get_path") ?? "Uncategorized";
        }
        
        /// <summary>
        /// Localized path for UI navigation
        /// Maps to: localizedPath field
        /// </summary>
        public string LocalizedPath
        {
            get => SafeInvoke<string>("get_localizedPath") ?? Path;
        }
        
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
        /// Content table with structured information
        /// Maps to: contentTable array
        /// </summary>
        public List<KnowledgeTableEntry> GetContentTable()
        {
            try
            {
                var contentTable = SafeInvoke<object>("get_contentTable");
                var tableEntries = new List<KnowledgeTableEntry>();
                
                if (contentTable is System.Collections.IEnumerable enumerable)
                {
                    foreach (var entry in enumerable)
                    {
                        if (entry != null)
                        {
                            var field = SafeInvoke<string>("get_field", entry) ?? "";
                            var text = SafeInvoke<string>("get_text", entry) ?? "";
                            tableEntries.Add(new KnowledgeTableEntry(field, text));
                        }
                    }
                }
                
                return tableEntries;
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to get content table for knowledge {Name}: {ex.Message}");
                return new List<KnowledgeTableEntry>();
            }
        }
        
        /// <summary>
        /// Associated image for knowledge entry
        /// Maps to: image field
        /// </summary>
        public object? Image
        {
            get => SafeInvoke<object>("get_image");
        }
        
        /// <summary>
        /// Icon name for UI display
        /// Maps to: iconName field
        /// </summary>
        public string IconName
        {
            get => SafeInvoke<string>("get_iconName") ?? "KnowledgeBase/Unknown";
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
        
        /// <summary>
        /// Check if this knowledge is unlocked for a faction
        /// </summary>
        /// <param name="faction">Faction to check unlock status for</param>
        /// <returns>True if knowledge is unlocked</returns>
        public bool IsUnlockedFor(Faction faction)
        {
            if (!faction.IsValidWrapper) return false;
            
            try
            {
                return SafeInvoke<bool?>("IsUnlockedFor", faction.GetNativeObject()) ??
                       SafeInvoke<bool?>("IsKnown", faction.GetNativeObject()) ??
                       SafeInvoke<bool?>("HasKnowledge", faction.GetNativeObject()) ?? false;
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failed to check unlock status of knowledge {Name} for faction {faction.Name}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Unlock this knowledge for a faction
        /// </summary>
        /// <param name="faction">Faction to unlock knowledge for</param>
        /// <returns>True if knowledge was unlocked successfully</returns>
        public bool UnlockFor(Faction faction)
        {
            if (!faction.IsValidWrapper) return false;
            
            try
            {
                var result = SafeInvoke<bool?>("UnlockFor", faction.GetNativeObject()) ??
                           SafeInvoke<bool?>("Unlock", faction.GetNativeObject()) ??
                           SafeInvoke<bool?>("Grant", faction.GetNativeObject());
                
                if (result.HasValue) return result.Value;
                
                Log.LogWarning($"Could not unlock knowledge {Name} for faction {faction.Name}");
                return false;
            }
            catch (Exception ex)
            {
                Log.LogError($"Error unlocking knowledge {Name} for faction {faction.Name}: {ex.Message}");
                return false;
            }
        }
        
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
            return $"Knowledge[{Name}] ({Category}, Index: {Index}, Valid: {IsValid})";
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