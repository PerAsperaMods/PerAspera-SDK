using System;
using System.Collections.Generic;
using PerAspera.Core;
#pragma warning disable CS1591

namespace PerAspera.GameAPI.Sectors
{
    /// <summary>
    /// Declares which sector modifiers each building type contributes when placed.
    /// Register building types in your plugin's Load() — SectorBuildingTracker will
    /// automatically apply/remove contributions when buildings are constructed/destroyed.
    /// </summary>
    /// <example>
    /// <code>
    /// // In your plugin Load():
    /// SectorBuildingContributionSpec.Register("building_ai_data_center",
    ///     (SectorModifierKeys.BuildingLimit, 50f)
    /// );
    ///
    /// SectorBuildingContributionSpec.Register("building_Earth_Mars_Communication_Tower",
    ///     (SectorModifierKeys.DroneCapacity, 3f),
    ///     (SectorModifierKeys.RoutingQuality, 0.1f)
    /// );
    ///
    /// SectorBuildingContributionSpec.Register("building_ai_data_center_enhanced",
    ///     (SectorModifierKeys.BuildingLimit, 100f),
    ///     (SectorModifierKeys.ResearchSpeed, 0.15f)
    /// );
    /// </code>
    /// </example>
    public static class SectorBuildingContributionSpec
    {
        private static readonly LogAspera Log = new LogAspera("Sectors.ContribSpec");

        private static readonly Dictionary<string, List<(string key, float value)>> _specs
            = new Dictionary<string, List<(string key, float value)>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Register modifier contributions for a building type.
        /// Calling Register twice for the same buildingTypeId replaces the previous registration.
        /// </summary>
        /// <param name="buildingTypeId">Exact YAML building ID (e.g. "building_ai_data_center")</param>
        /// <param name="contributions">Modifier key-value pairs this building contributes per instance</param>
        public static void Register(string buildingTypeId, params (string key, float value)[] contributions)
        {
            if (string.IsNullOrEmpty(buildingTypeId))
                throw new ArgumentException("buildingTypeId cannot be null or empty");

            _specs[buildingTypeId] = new List<(string, float)>(contributions);
            Log.Debug($"Registered {contributions.Length} modifiers for '{buildingTypeId}'");
        }

        /// <summary>Remove the registration for a building type.</summary>
        public static void Unregister(string buildingTypeId) => _specs.Remove(buildingTypeId);

        /// <summary>Returns true if this building type contributes sector modifiers.</summary>
        public static bool IsRegistered(string buildingTypeId) =>
            !string.IsNullOrEmpty(buildingTypeId) && _specs.ContainsKey(buildingTypeId);

        /// <summary>Get the modifier contributions for a building type. Returns empty if not registered.</summary>
        public static IReadOnlyList<(string key, float value)> GetContributions(string buildingTypeId)
        {
            if (string.IsNullOrEmpty(buildingTypeId)) return Array.Empty<(string, float)>();
            return _specs.TryGetValue(buildingTypeId, out var list)
                ? list
                : Array.Empty<(string, float)>();
        }

        /// <summary>All registered building type IDs.</summary>
        public static IEnumerable<string> RegisteredTypes => _specs.Keys;

        /// <summary>Clear all registrations (e.g. on plugin unload).</summary>
        public static void ClearAll() => _specs.Clear();
    }
}
