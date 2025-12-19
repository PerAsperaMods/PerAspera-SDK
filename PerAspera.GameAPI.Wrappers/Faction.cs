#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Native;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native Faction class
    /// Provides safe access to faction properties and operations
    /// DOC: Faction.md - Player and AI faction management
    /// </summary>
    public class Faction : WrapperBase
    {
        /// <summary>
        /// Initialize Faction wrapper with native faction object
        /// </summary>
        /// <param name="nativeFaction">Native faction instance from game</param>
        public Faction(object nativeFaction) : base(nativeFaction)
        {
        }
        
        /// <summary>
        /// Create wrapper from native faction object
        /// </summary>
        public static Faction? FromNative(object? nativeFaction)
        {
            return nativeFaction != null ? new Faction(nativeFaction) : null;
        }
        
        // ==================== CORE IDENTIFICATION ====================
        
        /// <summary>
        /// Faction name identifier
        /// Maps to: name field
        /// </summary>
        public string Name
        {
            get => SafeInvoke<string>("get_name") ?? "Unknown";
        }
        
        /// <summary>
        /// Faction display name for UI
        /// Maps to: displayName field
        /// </summary>
        public string DisplayName
        {
            get => SafeInvoke<string>("get_displayName") ?? Name;
        }
        
        /// <summary>
        /// Faction type (Player, AI, etc.)
        /// Maps to: factionType field
        /// </summary>
        public object? FactionType
        {
            get => SafeInvoke<object>("get_factionType");
        }
        
        /// <summary>
        /// Is this the player faction?
        /// Maps to: isPlayerFaction property or comparison with playerFaction
        /// </summary>
        public bool IsPlayerFaction
        {
            get => SafeInvoke<bool?>("get_isPlayerFaction") ?? false;
        }
        
        // ==================== RESOURCES ====================
        
        /// <summary>
        /// Main faction stockpile for resources
        /// Maps to: mainStockpile field
        /// </summary>
        public object? MainStockpile
        {
            get => SafeInvoke<object>("get_mainStockpile");
        }
        
        /// <summary>
        /// Get resource stock amount safely
        /// Maps to: mainStockpile resource lookup
        /// </summary>
        /// <param name="resourceKey">Resource key (e.g., "resource_water")</param>
        /// <returns>Current stock amount or 0 if not found</returns>
        public float GetResourceStock(string resourceKey)
        {
            try
            {
                var stockpile = MainStockpile;
                if (stockpile == null) return 0f;
                
                // Try various methods to get resource stock
                var stockAmount = SafeInvoke<float?>("GetResourceStock", resourceKey) ??
                                SafeInvoke<float?>("GetStock", resourceKey) ??
                                SafeInvoke<float?>("GetResourceAmount", resourceKey);
                
                return stockAmount ?? 0f;
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to get resource stock for {resourceKey}: {ex.Message}");
                return 0f;
            }
        }
        
        /// <summary>
        /// Add resource to faction with safe error handling
        /// Maps to: AddResource method or mainStockpile.AddResource
        /// </summary>
        /// <param name="resourceKey">Resource key (e.g., "resource_water")</param>
        /// <param name="amount">Amount to add (can be negative to remove)</param>
        /// <returns>True if operation succeeded</returns>
        public bool AddResource(string resourceKey, float amount)
        {
            try
            {
                // Try direct faction AddResource first
                var result = SafeInvoke<bool?>("AddResource", resourceKey, amount);
                if (result.HasValue) return result.Value;
                
                // Try via stockpile
                var stockpile = MainStockpile;
                if (stockpile != null)
                {
                    var stockpileResult = SafeInvoke<bool?>("AddResource", stockpile, resourceKey, amount);
                    if (stockpileResult.HasValue) return stockpileResult.Value;
                }
                
                Log.Warning($"Could not add resource {resourceKey} to faction {Name}");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"Error adding resource {resourceKey} to faction {Name}: {ex.Message}");
                return false;
            }
        }
        
        // ==================== RELATIONS ====================
        
        /// <summary>
        /// Get relationship status with another faction
        /// Maps to: relations or diplomacy system
        /// </summary>
        /// <param name="otherFaction">Other faction to check relationship with</param>
        /// <returns>Relationship value (-100 to 100, or null if unknown)</returns>
        public float? GetRelationshipWith(Faction otherFaction)
        {
            if (!otherFaction.IsValid) return null;
            
            try
            {
                return SafeInvoke<float?>("GetRelationship", otherFaction.NativeObject) ??
                       SafeInvoke<float?>("GetDiplomacyStatus", otherFaction.NativeObject) ??
                       SafeInvoke<float?>("GetStanding", otherFaction.NativeObject);
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to get relationship between {Name} and {otherFaction.Name}: {ex.Message}");
                return null;
            }
        }
        
        // ==================== BUILDINGS ====================
        
        /// <summary>
        /// Get all buildings owned by this faction
        /// Maps to: buildings collection or planet filtering
        /// </summary>
        /// <returns>List of buildings owned by this faction</returns>
        public List<Building> GetBuildings()
        {
            try
            {
                var buildings = SafeInvoke<object>("get_buildings");
                if (buildings == null)
                {
                    // Use BaseGame architecture (corrected approach)
                    // DOC: BaseGame-Architecture-Corrections.md - Direct planet access
                    try
                    {
                        var planet = Planet.GetCurrent();
                        if (planet != null)
                        {
                            var planetBuildings = SafeInvoke<object>("get_buildings", planet.GetNativeObject());
                            if (planetBuildings is System.Collections.IEnumerable planetEnumerable)
                            {
                                var planetBuildingWrappers = new List<Building>();
                                foreach (var building in planetEnumerable)
                                {
                                    if (building != null)
                                    {
                                        // Filter by faction ownership if possible
                                        var buildingWrapper = new Building(building);
                                        if (buildingWrapper.IsValidWrapper)
                                        {
                                            planetBuildingWrappers.Add(buildingWrapper);
                                        }
                                    }
                                }
                                return planetBuildingWrappers;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Debug($"Failed to access buildings via BaseGame: {ex.Message}");
                    }
                    
                    return new List<Building>();
                }
                
                // Convert native buildings to wrappers
                var buildingWrappers = new List<Building>();
                var enumerable = buildings as System.Collections.IEnumerable;
                if (enumerable != null)
                {
                    foreach (var building in enumerable)
                    {
                        if (building != null)
                        {
                            buildingWrappers.Add(new Building(building));
                        }
                    }
                }
                
                return buildingWrappers;
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to get buildings for faction {Name}: {ex.Message}");
                return new List<Building>();
            }
        }
        
        // ==================== AI BEHAVIOR ====================
        
        /// <summary>
        /// AI difficulty level (for AI factions)
        /// Maps to: aiDifficulty or difficultyLevel field
        /// </summary>
        public int AIDifficulty
        {
            get => SafeInvoke<int?>("get_aiDifficulty") ?? 
                   SafeInvoke<int?>("get_difficultyLevel") ?? 0;
        }
        
        /// <summary>
        /// Is this faction controlled by AI?
        /// Maps to: isAI field or !isPlayerFaction
        /// </summary>
        public bool IsAI
        {
            get => SafeInvoke<bool?>("get_isAI") ?? !IsPlayerFaction;
        }
        
        /// <summary>
        /// AI personality type (aggressive, defensive, etc.)
        /// Maps to: aiPersonality or behaviorType field
        /// </summary>
        public string AIPersonality
        {
            get => SafeInvoke<string>("get_aiPersonality") ?? 
                   SafeInvoke<string>("get_behaviorType") ?? "default";
        }
        
        // ==================== TECHNOLOGY ====================
        
        /// <summary>
        /// Check if faction has researched a specific technology
        /// Maps to: researchedTechnologies or techTree system
        /// </summary>
        /// <param name="technologyKey">Technology key to check</param>
        /// <returns>True if technology is researched</returns>
        public bool HasTechnology(string technologyKey)
        {
            try
            {
                return SafeInvoke<bool?>("HasTechnology", technologyKey) ??
                       SafeInvoke<bool?>("IsTechResearched", technologyKey) ??
                       SafeInvoke<bool?>("HasResearched", technologyKey) ?? false;
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to check technology {technologyKey} for faction {Name}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Research a technology for this faction
        /// Maps to: ResearchTechnology method
        /// </summary>
        /// <param name="technologyKey">Technology key to research</param>
        /// <returns>True if research was initiated successfully</returns>
        public bool ResearchTechnology(string technologyKey)
        {
            try
            {
                var result = SafeInvoke<bool?>("ResearchTechnology", technologyKey);
                if (result.HasValue) return result.Value;
                
                Log.Warning($"Could not research technology {technologyKey} for faction {Name}");
                return false;
            }
            catch (Exception ex)
            {
                Log.Error($"Error researching technology {technologyKey} for faction {Name}: {ex.Message}");
                return false;
            }
        }
        
        // ==================== UTILITIES ====================
        
        /// <summary>
        /// Get faction color for UI display
        /// Maps to: color or factionColor field
        /// </summary>
        public System.Drawing.Color GetColor()
        {
            try
            {
                var color = SafeInvoke<object>("get_color") ?? SafeInvoke<object>("get_factionColor");
                if (color != null)
                {
                    // Convert Unity Color to System.Drawing.Color if needed
                    return ExtractColor(color);
                }
                return System.Drawing.Color.Gray; // Default color
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to get color for faction {Name}: {ex.Message}");
                return System.Drawing.Color.Gray;
            }
        }
        
        private System.Drawing.Color ExtractColor(object unityColor)
        {
            try
            {
                var r = SafeInvoke<float?>("get_r", unityColor) ?? 0.5f;
                var g = SafeInvoke<float?>("get_g", unityColor) ?? 0.5f;
                var b = SafeInvoke<float?>("get_b", unityColor) ?? 0.5f;
                var a = SafeInvoke<float?>("get_a", unityColor) ?? 1.0f;
                
                return System.Drawing.Color.FromArgb(
                    (int)(a * 255), (int)(r * 255), (int)(g * 255), (int)(b * 255));
            }
            catch
            {
                return System.Drawing.Color.Gray;
            }
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
            return $"Faction[{Name}] (Valid: {IsValid}, Player: {IsPlayerFaction}, AI: {IsAI})";
        }
    }
}