using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core;
using PerAspera.GameAPI;

namespace PerAspera.SDK.TwitchIntegration
{
    /// <summary>
    /// Resource information for display and Twitch integration
    /// </summary>
    public class ResourceInfo
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsKnown { get; set; }
        public double CurrentAmount { get; set; }
        public double MaxStorage { get; set; }
        
        public override string ToString()
        {
            return $"{DisplayName} ({Category})";
        }
    }
    
    /// <summary>
    /// Helper class for accessing Faction data via SDK Keeper API
    /// </summary>
    public static class FactionHelper
    {
        private static readonly LogAspera Log = new LogAspera("FactionHelper");
        
        /// <summary>
        /// Get the player faction using Keeper API with caching
        /// </summary>
        public static Faction? GetPlayerFaction()
        {
            try
            {
                if (!Keeper.IsReady)
                {
                    Log.Warning("Keeper not ready for faction access");
                    return null;
                }
                
                // Try cache first (performance optimization)
                var cached = Keeper.Instances.GetCached<Faction>("player_faction");
                if (cached != null)
                {
                    Log.Debug("Retrieved player faction from cache");
                    return cached;
                }
                
                // Find via type registry
                Log.Debug("Searching for player faction via Keeper Type Registry");
                var allFactions = Keeper.Types.Get<Faction>();
                
                // Look for player faction (typically the first one or one with specific properties)
                var playerFaction = allFactions.FirstOrDefault(f => 
                {
                    // Try multiple approaches to identify player faction
                    try
                    {
                        // Method 1: Check if it's the main/first faction
                        return f != null && (f.factionID == 0 || f.IsPlayerFaction == true);
                    }
                    catch
                    {
                        // Method 2: Fallback - just use first valid faction
                        return f != null;
                    }
                });
                
                if (playerFaction != null)
                {
                    // Cache for next time (30-second timeout)
                    Keeper.Instances.Cache("player_faction", playerFaction);
                    Log.Info($"Found player faction: ID={playerFaction.factionID}");
                }
                else
                {
                    Log.Warning($"No player faction found among {allFactions.Count()} total factions");
                }
                
                return playerFaction;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get player faction: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get all known resource types for the player faction
        /// </summary>
        public static Dictionary<string, ResourceInfo> GetKnownResources()
        {
            try
            {
                var playerFaction = GetPlayerFaction();
                if (playerFaction?.knownResourceTypes == null)
                {
                    Log.Warning("Player faction or knownResourceTypes is null");
                    return new Dictionary<string, ResourceInfo>();
                }
                
                var resourceDict = new Dictionary<string, ResourceInfo>();
                
                Log.Info($"Processing {playerFaction.knownResourceTypes.Count} known resource types");
                
                foreach (var resourceType in playerFaction.knownResourceTypes)
                {
                    if (resourceType == null) continue;
                    
                    var resourceInfo = new ResourceInfo
                    {
                        Name = resourceType.name ?? "Unknown",
                        DisplayName = resourceType.displayName ?? resourceType.name ?? "Unknown",
                        Category = resourceType.category?.name ?? "Unknown",
                        Description = resourceType.description ?? "No description available",
                        IsKnown = true,
                        // Note: Current amounts would need additional access to Stock/ResourceManager
                        CurrentAmount = 0.0,
                        MaxStorage = 0.0
                    };
                    
                    resourceDict[resourceInfo.Name] = resourceInfo;
                }
                
                Log.Info($"Successfully processed {resourceDict.Count} resources");
                return resourceDict;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get known resources: {ex.Message}");
                return new Dictionary<string, ResourceInfo>();
            }
        }
        
        /// <summary>
        /// Check if a specific resource type is known by the player faction
        /// </summary>
        public static bool IsResourceKnown(string resourceName)
        {
            try
            {
                var playerFaction = GetPlayerFaction();
                if (playerFaction?.knownResourceTypes == null)
                    return false;
                
                return playerFaction.knownResourceTypes.Any(rt => 
                    rt?.name?.Equals(resourceName, StringComparison.OrdinalIgnoreCase) == true);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to check resource knowledge for {resourceName}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Get resource categories available to the player
        /// </summary>
        public static List<string> GetResourceCategories()
        {
            try
            {
                var knownResources = GetKnownResources();
                return knownResources.Values
                    .Select(r => r.Category)
                    .Where(c => !string.IsNullOrEmpty(c) && c != "Unknown")
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get resource categories: {ex.Message}");
                return new List<string>();
            }
        }
    }
}