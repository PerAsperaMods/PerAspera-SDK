using System;
using System.Collections.Generic;
using UnityEngine;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.UI
{
    /// <summary>
    /// Helper to find and cache UI panel instances in the game scene.
    /// Centralizes discovery of native UI elements.
    /// </summary>
    public static class UISceneHelper
    {
        private static Dictionary<string, object?> _panelCache = new();
        private static bool _scanComplete = false;

        /// <summary>Find a panel instance by type name.</summary>
        public static PerAspera.GameAPI.Wrappers.UI.UIPanelWrapper? FindPanelByType(string typeName)
        {
            try
            {
                // Check cache first
                if (_panelCache.TryGetValue(typeName, out var cached))
                {
                    if (cached != null)
                        return new PerAspera.GameAPI.Wrappers.UI.UIPanelWrapper(cached);
                    return null;
                }

                // Find type
                var panelType = ReflectionHelpers.FindType(typeName);
                if (panelType == null)
                {
                    _panelCache[typeName] = null;
                    return null;
                }

                // Search in scene via Behaviour objects
                var behaviours = UnityEngine.Object.FindObjectsOfType<UnityEngine.Behaviour>();
                foreach (var behaviour in behaviours)
                {
                    if (behaviour != null && behaviour.GetType() == panelType)
                    {
                        _panelCache[typeName] = behaviour;
                        return new PerAspera.GameAPI.Wrappers.UI.UIPanelWrapper(behaviour);
                    }
                }

                _panelCache[typeName] = null;
                return null;
            }
            catch { return null; }
        }

        /// <summary>Find BuildingScreenPanel specifically.</summary>
        public static PerAspera.GameAPI.Wrappers.UI.UIPanelWrapper? FindBuildingScreenPanel()
        {
            return FindPanelByType("BuildingScreenPanel");
        }

        /// <summary>Find all panel types in the scene.</summary>
        public static List<string> ScanAvailablePanels()
        {
            var found = new List<string>();

            try
            {
                var commonPanelTypes = new[]
                {
                    "BuildingScreenPanel",
                    "PlanetStatsPanel",
                    "ResourcesPanel",
                    "CommandPanel",
                    "TechTreePanel"
                };

                foreach (var typeName in commonPanelTypes)
                {
                    var wrapper = FindPanelByType(typeName);
                    if (wrapper != null)
                        found.Add(typeName);
                }

                _scanComplete = true;
            }
            catch { }

            return found;
        }

        /// <summary>Clear the panel cache (useful after scene loads).</summary>
        public static void ClearCache()
        {
            _panelCache.Clear();
            _scanComplete = false;
        }

        /// <summary>Get cache statistics.</summary>
        public static string GetCacheStats()
        {
            return $"Cached panels: {_panelCache.Count}, Scan complete: {_scanComplete}";
        }
    }
}
