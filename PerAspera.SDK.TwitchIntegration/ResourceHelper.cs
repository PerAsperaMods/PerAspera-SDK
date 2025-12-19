using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;
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
        /// Get the player faction using proper Keeper/Handle system
        /// </summary>
        public static GameAPI.Wrappers.Faction? GetPlayerFaction()
        {
            try
            {
                Log.Debug("Getting player faction via Universe wrapper");
                
                // 1. Get Universe via BaseGame wrapper
                var baseGame = GameAPI.Wrappers.BaseGame.GetCurrent();
                if (baseGame == null)
                {
                    Log.Warning("BaseGame not available");
                    return null;
                }
                
                var universe = baseGame.GetUniverse();
                if (universe == null)
                {
                    Log.Warning("Universe not available");
                    return null;
                }
                
                // 2. Get player faction via Universe wrapper
                var playerFaction = universe.GetPlayerFaction();
                if (playerFaction == null)
                {
                    Log.Warning("Player faction not available via Universe");
                    return null;
                }
                
                Log.Info($"Successfully retrieved player faction via wrapper");
                return playerFaction;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get player faction: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get all known resource types using proper Keeper Handle system
        /// </summary>
        public static Dictionary<string, ResourceInfo> GetKnownResources()
        {
            try
            {
                Log.Debug("Getting known resources via Faction wrapper to native");
                
                // 1. Get player faction wrapper
                var playerFactionWrapper = GetPlayerFaction();
                if (playerFactionWrapper == null)
                {
                    Log.Warning("Player faction wrapper not available");
                    return new Dictionary<string, ResourceInfo>();
                }
                
                // 2. Get native faction object via Handle system
                var nativeFaction = GetNativeFactionFromWrapper(playerFactionWrapper);
                if (nativeFaction == null)
                {
                    Log.Warning("Could not get native faction object");
                    return new Dictionary<string, ResourceInfo>();
                }
                
                // 3. Access knownResourceTypes field from native faction
                var knownResourceTypes = nativeFaction.GetFieldValue<object>("knownResourceTypes");
                if (knownResourceTypes == null)
                {
                    Log.Warning("knownResourceTypes field not accessible");
                    return new Dictionary<string, ResourceInfo>();
                }
                
                Log.Info("Successfully accessed knownResourceTypes, processing...");
                
                // 4. Process resource types
                var resourceDict = new Dictionary<string, ResourceInfo>();
                int processedCount = 0;
                
                foreach (var resourceType in EnumerateArraySet(knownResourceTypes))
                {
                    if (resourceType == null) continue;
                    
                    try
                    {
                        var resourceInfo = new ResourceInfo
                        {
                            Name = resourceType.GetFieldValue<string>("name") ?? "Unknown",
                            DisplayName = resourceType.GetFieldValue<string>("displayName") ?? "Unknown",
                            Category = GetResourceCategory(resourceType),
                            Description = resourceType.GetFieldValue<string>("description") ?? "No description",
                            IsKnown = true,
                            CurrentAmount = 0.0, // TODO: Get from Stock system if needed
                            MaxStorage = 0.0     // TODO: Get from Stock system if needed
                        };
                        
                        if (!string.IsNullOrEmpty(resourceInfo.Name) && resourceInfo.Name != "Unknown")
                        {
                            resourceDict[resourceInfo.Name] = resourceInfo;
                            processedCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning($"Failed to process individual resource type: {ex.Message}");
                    }
                }
                
                Log.Info($"Successfully processed {processedCount} resource types");
                return resourceDict;
            }
            catch (Exception ex)
            {
                Log.Error($"GetKnownResources failed: {ex.Message}");
                return new Dictionary<string, ResourceInfo>();
            }
        }
        
        /// <summary>
        /// Get native faction object from wrapper using KeeperMapWrapper
        /// </summary>
        private static object? GetNativeFactionFromWrapper(GameAPI.Wrappers.Faction factionWrapper)
        {
            try
            {
                // 1. Get Handle from wrapper
                var handle = factionWrapper.GetHandle();
                if (handle.Equals(default(Handle)))
                {
                    Log.Warning("Faction wrapper has invalid handle");
                    return null;
                }
                
                // 2. Get KeeperMapWrapper directly (much cleaner!)
                var keeperMapWrapper = GameAPI.Wrappers.KeeperMapWrapper.GetCurrent();
                if (keeperMapWrapper == null)
                {
                    Log.Warning("KeeperMapWrapper not available for handle lookup");
                    return null;
                }
                
                // 3. Use KeeperMapWrapper.Find() with Handle - type-safe!
                var nativeFaction = keeperMapWrapper.Find<object>(handle);
                if (nativeFaction == null)
                {
                    Log.Warning($"Handle {handle} not found in KeeperMap");
                    return null;
                }
                
                Log.Debug($"Successfully retrieved native faction via KeeperMapWrapper with Handle {handle}");
                return nativeFaction;
            }
            catch (Exception ex)
            {
                Log.Error($"GetNativeFactionFromWrapper failed: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Enumerate an ArraySet collection safely
        /// </summary>
        private static IEnumerable<object> EnumerateArraySet(object arraySet)
        {
            if (arraySet == null) yield break;
            
            var results = new List<object>();
            
            try
            {
                // Try to get enumerator first
                var enumerator = arraySet.InvokeMethod<object>("GetEnumerator");
                if (enumerator != null)
                {
                    while (enumerator.InvokeMethod<bool>("MoveNext"))
                    {
                        var current = enumerator.GetPropertyValue<object>("Current");
                        if (current != null) results.Add(current);
                    }
                }
                else
                {
                    // Fallback: try to access _items directly
                    var items = arraySet.GetFieldValue<object>("_items");
                    if (items != null)
                    {
                        var count = items.GetPropertyValue<int>("Count");
                        for (int i = 0; i < count; i++)
                        {
                            var item = items.InvokeMethod<object>("get_Item", i);
                            if (item != null) results.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning($"EnumerateArraySet failed: {ex.Message}");
            }
            
            foreach (var result in results)
                yield return result;
        }
        
        /// <summary>
        /// Get resource category name safely
        /// </summary>
        private static string GetResourceCategory(object resourceType)
        {
            try
            {
                var category = resourceType.GetFieldValue<object>("category");
                if (category == null) return "Unknown";
                
                var categoryName = category.GetFieldValue<string>("name");
                return categoryName ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }
        
        /// <summary>
        /// Check if a specific resource type is known by the player faction
        /// </summary>
        public static bool IsResourceKnown(string resourceName)
        {
            try
            {
                var knownResources = GetKnownResources();
                return knownResources.ContainsKey(resourceName) ||
                       knownResources.Values.Any(r => 
                           r.DisplayName.Equals(resourceName, StringComparison.OrdinalIgnoreCase));
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