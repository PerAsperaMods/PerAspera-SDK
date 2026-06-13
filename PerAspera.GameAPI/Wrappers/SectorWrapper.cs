using System;
using System.Collections.Generic;
using PerAspera.GameAPI.Sectors;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wraps a native Per Aspera Sector, exposing geometry, state, and the per-sector modifier system.
    /// Sectors are quadrilateral zones on the planet surface — each can be unlocked/locked
    /// and enhanced by buildings (DataCenter → building limit, CommTower → drone capacity, etc.)
    /// </summary>
    /// <example>
    /// <code>
    /// var faction = FactionWrapper.GetPlayerFaction();
    /// var sector  = faction?.GetSector(3);
    ///
    /// if (sector != null)
    /// {
    ///     bool hasSat   = sector.HasActiveSatellite;
    ///     int  maxBuild = sector.GetBuildingLimit(baseLimit: 150);
    ///     int  drones   = sector.GetDroneCapacity(baseCapacity: 4);
    ///     LogAspera.Info($"Sector {sector.Id}: buildings={maxBuild}, drones={drones}");
    /// }
    /// </code>
    /// </example>
    public class SectorWrapper : WrapperBase
    {
        /// <summary>The native Sector proxy. Use for direct IL2CPP operations.</summary>
        public Sector NativeSector { get; }

        /// <summary>Sector index (0-based, matches YAML argument for UnlockSector/LockSector).</summary>
        public int Id => NativeSector?.id ?? -1;

        /// <summary>
        /// Whether this sector is enabled — controls IsInsideEnabledSectors().
        /// Setting to false blocks all construction in the sector.
        /// </summary>
        public bool IsEnabled
        {
            get => NativeSector?.enabled ?? false;
            set
            {
                if (NativeSector != null)
                    NativeSector.enabled = value;
            }
        }

        /// <summary>True if a satellite is actively covering this sector.</summary>
        public bool HasActiveSatellite => SectorModifierRegistry.HasActiveSatellite(Id);

        // ─── Geometry ─────────────────────────────────────────────────────────

        /// <summary>Corner A of the sector quadrilateral (2D lat/lng).</summary>
        public UnityEngine.Vector2 CornerA => NativeSector?.a ?? default;

        /// <summary>Corner B of the sector quadrilateral (2D lat/lng).</summary>
        public UnityEngine.Vector2 CornerB => NativeSector?.b ?? default;

        /// <summary>Corner C of the sector quadrilateral (2D lat/lng).</summary>
        public UnityEngine.Vector2 CornerC => NativeSector?.c ?? default;

        /// <summary>Corner D of the sector quadrilateral (2D lat/lng).</summary>
        public UnityEngine.Vector2 CornerD => NativeSector?.d ?? default;

        /// <summary>Returns true if a 2D position is inside this sector's bounds.</summary>
        public bool Contains(UnityEngine.Vector2 position) => NativeSector?.Contains(position) ?? false;

        // ─── Per-Sector Modifiers ─────────────────────────────────────────────

        /// <summary>
        /// Get the total bonus for a modifier key from all buildings in this sector.
        /// Returns 0 if the sector has no active satellite.
        /// </summary>
        public float GetModifier(string key, float defaultValue = 0f)
            => SectorModifierRegistry.GetTotal(Id, key, defaultValue);

        /// <summary>
        /// Get effective building limit for this sector: base + bonus from DataCenters.
        /// </summary>
        /// <param name="baseLimit">The global faction building limit to add the bonus to</param>
        public int GetBuildingLimit(int baseLimit = 0)
            => baseLimit + (int)GetModifier(SectorModifierKeys.BuildingLimit);

        /// <summary>
        /// Get effective drone capacity for this sector: base + bonus from Communication Towers.
        /// </summary>
        /// <param name="baseCapacity">The default sector drone capacity</param>
        public int GetDroneCapacity(int baseCapacity = 0)
            => baseCapacity + (int)GetModifier(SectorModifierKeys.DroneCapacity);

        /// <summary>Get the routing quality bonus from buildings in this sector (0.0 to N).</summary>
        public float GetRoutingQuality() => GetModifier(SectorModifierKeys.RoutingQuality);

        /// <summary>Get the extra hop capacity bonus for drones in this sector.</summary>
        public int GetHopCapacity() => (int)GetModifier(SectorModifierKeys.HopCapacity);

        /// <summary>Get research speed multiplier bonus from AI buildings (0.0 = no bonus).</summary>
        public float GetResearchSpeedBonus() => GetModifier(SectorModifierKeys.ResearchSpeed);

        /// <summary>Get all active modifiers for this sector (for debug UI).</summary>
        public IEnumerable<(string key, float total)> GetAllModifiers()
            => SectorModifierRegistry.GetAllModifiers(Id);

        // ─── Satellite control ────────────────────────────────────────────────

        /// <summary>
        /// Activate satellite coverage for this sector.
        /// All building contributions become effective after this call.
        /// </summary>
        public void ActivateSatellite() => SectorModifierRegistry.ActivateSatellite(Id);

        /// <summary>
        /// Deactivate satellite coverage (e.g. satellite compromised).
        /// All modifier GetTotal() calls return 0 until reactivated.
        /// </summary>
        public void DeactivateSatellite() => SectorModifierRegistry.DeactivateSatellite(Id);

        // ─── Lock / Unlock ────────────────────────────────────────────────────

        /// <summary>
        /// Unlock this sector — enables construction within it.
        /// Prefer using UnlockSectorCommand from YAML for persistence.
        /// </summary>
        public void Unlock() => IsEnabled = true;

        /// <summary>
        /// Lock this sector — blocks all construction (same as LockSectorCommand).
        /// Use when satellite is compromised: LockSector + DeactivateSatellite.
        /// </summary>
        public void Lock()
        {
            IsEnabled = false;
            DeactivateSatellite();
        }

        // ─── Factory ─────────────────────────────────────────────────────────

        /// <summary>Wrap a native Sector proxy.</summary>
        public static SectorWrapper FromNative(Sector sector)
        {
            if (sector == null) throw new ArgumentNullException(nameof(sector));
            return new SectorWrapper(sector);
        }

        private SectorWrapper(Sector nativeSector) : base(nativeSector)
        {
            NativeSector = nativeSector;
        }
    }
}
