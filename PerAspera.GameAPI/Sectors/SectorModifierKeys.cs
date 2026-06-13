namespace PerAspera.GameAPI.Sectors
{
    /// <summary>
    /// Standard modifier keys for the per-sector modifier system.
    /// Use these constants when registering building contributions or reading values.
    /// </summary>
    public static class SectorModifierKeys
    {
        // ─── Construction ─────────────────────────────────────────────────────

        /// <summary>
        /// Additional buildings allowed in this sector beyond global limit.
        /// Default: 0. Each DataCenter adds +50.
        /// Patch: SectorBuildingLimitPatch reads this to increase allowed buildings.
        /// </summary>
        public const string BuildingLimit = "sector_building_limit";

        // ─── Drones & Routing ─────────────────────────────────────────────────

        /// <summary>
        /// Additional drone slots available in this sector.
        /// Default: 0. Each Communication Tower adds +3.
        /// Patch: SectorDroneCapacityPatch reads this per sector.
        /// </summary>
        public const string DroneCapacity = "sector_drone_capacity";

        /// <summary>
        /// Multiplier bonus on routing path quality (SPFA weight) in this sector.
        /// Default: 0. Higher = drones prefer this sector's routes.
        /// </summary>
        public const string RoutingQuality = "sector_routing_quality";

        /// <summary>
        /// Extra hop capacity for drones passing through this sector.
        /// Default: 0. Each relay/datacenter can add +1.
        /// </summary>
        public const string HopCapacity = "sector_hop_capacity";

        // ─── Research & Knowledge ─────────────────────────────────────────────

        /// <summary>
        /// Research speed multiplier bonus contributed by AI buildings in this sector.
        /// Default: 0.0. Each AI Research Lab adds +0.1 (10%).
        /// </summary>
        public const string ResearchSpeed = "sector_research_speed";

        // ─── Power & Resources ────────────────────────────────────────────────

        /// <summary>
        /// Power grid efficiency bonus in this sector.
        /// Default: 0.0. Reserved for future power buildings.
        /// </summary>
        public const string PowerEfficiency = "sector_power_efficiency";

        // ─── Satellite ────────────────────────────────────────────────────────

        /// <summary>Internal key — do not use in building contributions. Managed by SectorModifierRegistry.</summary>
        internal const string SatelliteActive = "_satellite_active";
    }
}
