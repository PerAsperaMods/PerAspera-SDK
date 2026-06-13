using System;
using System.Collections.Generic;
using PerAspera.Core;

namespace PerAspera.GameAPI.Sectors
{
    /// <summary>
    /// Per-sector modifier storage. Tracks float values keyed by modifier name for each sector.
    /// Values are additive: multiple buildings in a sector each contribute their share.
    /// Satellite coverage controls whether modifiers are active (GetTotal returns 0 without satellite).
    /// </summary>
    /// <example>
    /// <code>
    /// // Read modifier total for sector 3
    /// float buildingBonus = SectorModifierRegistry.GetTotal(3, SectorModifierKeys.BuildingLimit);
    ///
    /// // Activate satellite for sector 3
    /// SectorModifierRegistry.ActivateSatellite(3);
    ///
    /// // Plugin — add contribution from a building (called by SectorBuildingTracker)
    /// SectorModifierRegistry.AddContribution(sectorId, buildingId, SectorModifierKeys.DroneCapacity, 3f);
    /// </code>
    /// </example>
    public static class SectorModifierRegistry
    {
        private static readonly LogAspera Log = new LogAspera("Sectors.Registry");

        // sectorId → modifierKey → entry (total + per-building breakdown)
        private static readonly Dictionary<int, Dictionary<string, SectorModifierEntry>> _registry
            = new Dictionary<int, Dictionary<string, SectorModifierEntry>>();

        // sectorId → has active satellite
        private static readonly HashSet<int> _activeSatellites = new HashSet<int>();

        // ─── Satellite state ──────────────────────────────────────────────────

        /// <summary>Mark a sector as covered by an active satellite. Enables all modifier contributions.</summary>
        public static void ActivateSatellite(int sectorId)
        {
            _activeSatellites.Add(sectorId);
            Log.Info($"Satellite activated for sector {sectorId}");
        }

        /// <summary>Mark a sector as satellite-lost. GetTotal returns 0 for all keys until reactivated.</summary>
        public static void DeactivateSatellite(int sectorId)
        {
            _activeSatellites.Remove(sectorId);
            Log.Info($"Satellite lost for sector {sectorId}");
        }

        /// <summary>Returns true if this sector has an active satellite.</summary>
        public static bool HasActiveSatellite(int sectorId) => _activeSatellites.Contains(sectorId);

        // ─── Contributions ────────────────────────────────────────────────────

        /// <summary>
        /// Add a modifier contribution from a building to a sector.
        /// Typically called by SectorBuildingTracker.HandleConstructed().
        /// </summary>
        /// <param name="sectorId">Sector where the building was placed</param>
        /// <param name="buildingId">Building.number — unique building instance ID</param>
        /// <param name="key">Modifier key (use SectorModifierKeys constants)</param>
        /// <param name="value">Value to add (positive delta)</param>
        public static void AddContribution(int sectorId, int buildingId, string key, float value)
        {
            if (!_registry.TryGetValue(sectorId, out var modifiers))
            {
                modifiers = new Dictionary<string, SectorModifierEntry>();
                _registry[sectorId] = modifiers;
            }

            if (!modifiers.TryGetValue(key, out var entry))
            {
                entry = new SectorModifierEntry();
                modifiers[key] = entry;
            }

            entry.AddContribution(buildingId, value);
            Log.Debug($"Sector {sectorId}: +{value} '{key}' from building #{buildingId} (total={entry.Total})");
        }

        /// <summary>
        /// Remove a building's contribution from a sector.
        /// Typically called by SectorBuildingTracker.HandleDestroyed().
        /// </summary>
        public static void RemoveContribution(int sectorId, int buildingId, string key)
        {
            if (!_registry.TryGetValue(sectorId, out var modifiers)) return;
            if (!modifiers.TryGetValue(key, out var entry)) return;

            float removed = entry.RemoveContribution(buildingId);
            Log.Debug($"Sector {sectorId}: -{removed} '{key}' building #{buildingId} removed (total={entry.Total})");
        }

        // ─── Reads ────────────────────────────────────────────────────────────

        /// <summary>
        /// Get the total modifier value for a key in a sector.
        /// Returns defaultValue if sector has no active satellite (satellite required for modifiers to be active).
        /// </summary>
        public static float GetTotal(int sectorId, string key, float defaultValue = 0f)
        {
            if (!_activeSatellites.Contains(sectorId)) return defaultValue;
            if (!_registry.TryGetValue(sectorId, out var modifiers)) return defaultValue;
            if (!modifiers.TryGetValue(key, out var entry)) return defaultValue;
            return entry.Total;
        }

        /// <summary>
        /// Get the total modifier regardless of satellite status.
        /// For display/info only — game logic should use GetTotal().
        /// </summary>
        public static float GetTotalRaw(int sectorId, string key, float defaultValue = 0f)
        {
            if (!_registry.TryGetValue(sectorId, out var modifiers)) return defaultValue;
            if (!modifiers.TryGetValue(key, out var entry)) return defaultValue;
            return entry.Total;
        }

        /// <summary>All sectors that have an active satellite.</summary>
        public static IReadOnlyCollection<int> ActiveSatelliteSectors => _activeSatellites;

        /// <summary>All modifier keys and totals for a sector (debug/display).</summary>
        public static IEnumerable<(string key, float total)> GetAllModifiers(int sectorId)
        {
            if (!_registry.TryGetValue(sectorId, out var modifiers)) yield break;
            foreach (var kv in modifiers)
                yield return (kv.Key, kv.Value.Total);
        }

        // ─── Lifecycle ────────────────────────────────────────────────────────

        /// <summary>Clear all data. Call on game unload / scene reset.</summary>
        public static void ClearAll()
        {
            _registry.Clear();
            _activeSatellites.Clear();
            Log.Info("Cleared all sector modifiers");
        }

        /// <summary>Clear modifiers for a single sector.</summary>
        public static void ClearSector(int sectorId)
        {
            _registry.Remove(sectorId);
            _activeSatellites.Remove(sectorId);
        }
    }

    /// <summary>
    /// Stores the total and per-building breakdown of a single modifier in a sector.
    /// </summary>
    internal sealed class SectorModifierEntry
    {
        private readonly Dictionary<int, float> _contributions = new Dictionary<int, float>();
        public float Total { get; private set; }

        public void AddContribution(int buildingId, float value)
        {
            if (_contributions.TryGetValue(buildingId, out float existing))
                Total -= existing;
            _contributions[buildingId] = value;
            Total += value;
        }

        public float RemoveContribution(int buildingId)
        {
            if (!_contributions.TryGetValue(buildingId, out float value)) return 0f;
            _contributions.Remove(buildingId);
            Total -= value;
            return value;
        }
    }
}
