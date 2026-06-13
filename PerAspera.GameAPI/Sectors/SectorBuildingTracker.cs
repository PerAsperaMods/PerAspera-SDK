using HarmonyLib;
using PerAspera.Core;
#pragma warning disable CS1591

namespace PerAspera.GameAPI.Sectors
{
    /// <summary>
    /// Hooks into Building.FinishConstruction and Building.RemoveBuilding to automatically apply
    /// per-sector modifier contributions declared via SectorBuildingContributionSpec.
    ///
    /// Call Apply(harmony) once in your plugin Load(), AFTER registering building specs.
    /// </summary>
    /// <example>
    /// <code>
    /// // In plugin Load():
    /// SectorBuildingContributionSpec.Register("building_ai_data_center",
    ///     (SectorModifierKeys.BuildingLimit, 50f)
    /// );
    /// SectorBuildingContributionSpec.Register("building_Earth_Mars_Communication_Tower",
    ///     (SectorModifierKeys.DroneCapacity, 3f),
    ///     (SectorModifierKeys.RoutingQuality, 0.1f)
    /// );
    /// SectorBuildingTracker.Apply(harmony);
    /// </code>
    /// </example>
    public static class SectorBuildingTracker
    {
        private static readonly LogAspera Log = new LogAspera("Sectors.BuildingTracker");
        private static bool _applied;

        /// <summary>
        /// Apply HarmonyX patches to track building construction/destruction.
        /// Safe to call multiple times (deduplicates).
        /// </summary>
        public static void Apply(Harmony harmony)
        {
            if (_applied) return;
            _applied = true;

            harmony.PatchAll(typeof(FinishConstructionPatch));
            harmony.PatchAll(typeof(RemoveBuildingPatch));

            Log.Info("Patches applied — tracking FinishConstruction + RemoveBuilding");
        }

        // ─── Internal handlers ────────────────────────────────────────────────

        internal static void HandleConstructed(Building building)
        {
            if (building == null) return;

            string? typeKey = building.buildingType?.key;
            if (string.IsNullOrEmpty(typeKey)) return;
            if (!SectorBuildingContributionSpec.IsRegistered(typeKey)) return;

            var faction = building.faction;
            if (faction == null) return;

            int sectorId = faction.SectorByGeographic(building.position);
            int buildingId = building.number;

            foreach (var (key, value) in SectorBuildingContributionSpec.GetContributions(typeKey))
                SectorModifierRegistry.AddContribution(sectorId, buildingId, key, value);

            Log.Debug($"Built '{typeKey}' (#{buildingId}) → sector {sectorId}");
        }

        internal static void HandleDestroyed(Building building)
        {
            if (building == null) return;

            string? typeKey = building.buildingType?.key;
            if (string.IsNullOrEmpty(typeKey)) return;
            if (!SectorBuildingContributionSpec.IsRegistered(typeKey)) return;

            var faction = building.faction;
            if (faction == null) return;

            int sectorId = faction.SectorByGeographic(building.position);
            int buildingId = building.number;

            foreach (var (key, _) in SectorBuildingContributionSpec.GetContributions(typeKey))
                SectorModifierRegistry.RemoveContribution(sectorId, buildingId, key);

            Log.Debug($"Destroyed '{typeKey}' (#{buildingId}) from sector {sectorId}");
        }
    }

    // ─── Harmony Patches ──────────────────────────────────────────────────────

    [HarmonyPatch(typeof(Building), nameof(Building.FinishConstruction))]
    internal static class FinishConstructionPatch
    {
        static void Postfix(Building __instance) => SectorBuildingTracker.HandleConstructed(__instance);
    }

    [HarmonyPatch(typeof(Building), nameof(Building.RemoveBuilding))]
    internal static class RemoveBuildingPatch
    {
        static void Prefix(Building __instance) => SectorBuildingTracker.HandleDestroyed(__instance);
    }
}
