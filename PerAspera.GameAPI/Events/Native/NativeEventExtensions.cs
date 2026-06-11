using PerAspera.Core;

namespace PerAspera.GameAPI.Events.Native
{
    /// <summary>
    /// Extension methods for resolving <see cref="NativeGameEventArgs"/> sender/target
    /// <c>Handle</c>s to concrete game objects.
    ///
    /// All methods use typed interop (zero reflection). Pass the game-owned
    /// <c>Keeper</c> (<c>universe.keeper</c> or <c>baseGame.keeper</c>) or
    /// <c>Universe</c> instance — both are available after <c>GameCommandsReadyEvent</c>.
    ///
    /// Sender semantics per event family:
    /// <list type="table">
    ///   <listheader><term>Family</term><description>Sender is…</description></listheader>
    ///   <item><term>Building* events</term><description>The Building that changed. Use <see cref="ResolveBuilding"/>.</description></item>
    ///   <item><term>Faction* events</term><description>The Faction involved. Use <see cref="ResolveFaction"/>.</description></item>
    ///   <item><term>Drone* events</term><description>The Drone. Use <see cref="ResolveDroneFromFaction"/> (search faction.drones).</description></item>
    ///   <item><term>Way* events</term><description>The Way. Use <see cref="ResolveWayFromFaction"/>.</description></item>
    ///   <item><term>Planet* events</term><description>The Planet (singleton). Access via <c>universe.planet</c> directly.</description></item>
    ///   <item><term>Universe* events</term><description>The Universe. Access via <c>baseGame.universe</c> directly.</description></item>
    /// </list>
    /// </summary>
    public static class NativeEventExtensions
    {
        private static readonly LogAspera _log = new LogAspera("GameAPI.Events.NativeEventExtensions");

        // ──────────────────────────────────────────────────────────────
        //  Building resolution via Keeper._HandleToBuildingSafe(Handle)
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Resolve the <c>sender</c> Handle to a <see cref="Building"/>.
        /// Uses <c>Keeper._HandleToBuildingSafe</c> — returns <c>null</c> if the
        /// handle is invalid or the building has already been despawned.
        /// </summary>
        /// <param name="args">The event args.</param>
        /// <param name="keeper"><c>universe.keeper</c> or <c>baseGame.keeper</c>.</param>
        /// <example>
        /// NativeEventHub.Subscribe(NativeGameEvent.BuildingBuilt, args => {
        ///     var building = args.ResolveBuilding(universe.keeper);
        ///     LogAspera.Info($"Built: {building?.buildingType?.id}");
        /// });
        /// </example>
        public static Building ResolveBuilding(this NativeGameEventArgs args, Keeper keeper)
        {
            if (keeper == null) return null;
            try
            {
                return keeper._HandleToBuildingSafe(args.SenderHandle) as Building;
            }
            catch (Exception ex)
            {
                _log.Warning($"ResolveBuilding failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Resolve the <c>target</c> Handle to a <see cref="Building"/>.
        /// Use for events that carry a secondary building context
        /// (e.g. <c>BuildingAfterChangeBuildingType</c> where target is the new type's building).
        /// </summary>
        /// <param name="args">The event args.</param>
        /// <param name="keeper"><c>universe.keeper</c> or <c>baseGame.keeper</c>.</param>
        /// <example>
        /// var oldBuilding = args.ResolveBuilding(keeper);
        /// var newBuilding = args.ResolveBuildingTarget(keeper);
        /// </example>
        public static Building ResolveBuildingTarget(this NativeGameEventArgs args, Keeper keeper)
        {
            if (keeper == null) return null;
            try
            {
                return keeper._HandleToBuildingSafe(args.TargetHandle) as Building;
            }
            catch (Exception ex)
            {
                _log.Warning($"ResolveBuildingTarget failed: {ex.Message}");
                return null;
            }
        }

        // ──────────────────────────────────────────────────────────────
        //  Faction resolution — iterate Universe.factions by Handle
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Resolve the <c>sender</c> Handle to a <see cref="Faction"/> by matching against
        /// <c>universe.factions</c> (O(n), typically 2–4 factions).
        /// Returns <c>null</c> if not found.
        /// </summary>
        /// <param name="args">The event args.</param>
        /// <param name="universe">The Universe instance (available after GameCommandsReady).</param>
        /// <example>
        /// NativeEventHub.Subscribe(NativeGameEvent.FactionShipArrived, args => {
        ///     var faction = args.ResolveFaction(universe);
        ///     LogAspera.Info($"Ship arrived for: {faction?.factionName}");
        /// });
        /// </example>
        public static Faction ResolveFaction(this NativeGameEventArgs args, Universe universe)
            => ResolveHandleAsFaction(args.SenderHandle, universe);

        /// <summary>
        /// Resolve the <c>target</c> Handle to a <see cref="Faction"/>.
        /// </summary>
        /// <param name="args">The event args.</param>
        /// <param name="universe">The Universe instance.</param>
        public static Faction ResolveFactionTarget(this NativeGameEventArgs args, Universe universe)
            => ResolveHandleAsFaction(args.TargetHandle, universe);

        // ──────────────────────────────────────────────────────────────
        //  Drone / Way — search via faction collections
        //  (no Keeper._HandleToDroneSafe / _HandleToWaySafe in interop)
        // ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Resolve the <c>sender</c> Handle to a <see cref="Drone"/> by searching
        /// <c>faction.drones</c> (<c>List&lt;Drone&gt;</c>) for the matching Handle.
        /// </summary>
        /// <param name="args">The event args.</param>
        /// <param name="faction">Faction to search — typically <c>universe.playerFaction</c>.</param>
        /// <example>
        /// NativeEventHub.Subscribe(NativeGameEvent.DroneSpawned, args => {
        ///     var drone = args.ResolveDroneFromFaction(universe.playerFaction);
        /// });
        /// </example>
        public static Drone ResolveDroneFromFaction(this NativeGameEventArgs args, Faction faction)
            => ResolveHandleAsDrone(args.SenderHandle, faction?.drones);

        /// <summary>
        /// Resolve the <c>sender</c> Handle to a <see cref="Way"/> by searching
        /// <c>faction.ways</c> (<c>List&lt;Way&gt;</c>) for the matching Handle.
        /// </summary>
        /// <param name="args">The event args.</param>
        /// <param name="faction">Faction to search — typically <c>universe.playerFaction</c>.</param>
        /// <example>
        /// NativeEventHub.Subscribe(NativeGameEvent.WayBuilt, args => {
        ///     var way = args.ResolveWayFromFaction(universe.playerFaction);
        /// });
        /// </example>
        public static Way ResolveWayFromFaction(this NativeGameEventArgs args, Faction faction)
            => ResolveHandleAsWay(args.SenderHandle, faction?.ways);

        // ──────────────────────────────────────────────────────────────
        //  Handle equality helper
        // ──────────────────────────────────────────────────────────────

        /// <summary>Returns true when two Handles refer to the same live object.</summary>
        public static bool HandleEquals(Handle a, Handle b)
            => a.index == b.index && a.version == b.version;

        // ──────────────────────────────────────────────────────────────
        //  Internal helpers (concrete types — no generic IHandleable
        //  constraint since IHandleable is a class in the interop)
        // ──────────────────────────────────────────────────────────────

        private static Faction ResolveHandleAsFaction(Handle handle, Universe universe)
        {
            if (universe?.factions == null) return null;
            try
            {
                var factions = universe.factions;
                for (int i = 0; i < factions.Count; i++)
                {
                    var f = factions[i];
                    if (f != null && HandleEquals(f.handle, handle))
                        return f;
                }
            }
            catch (Exception ex)
            {
                _log.Warning($"ResolveFaction failed: {ex.Message}");
            }
            return null;
        }

        private static Drone ResolveHandleAsDrone(Handle handle, Il2CppSystem.Collections.Generic.List<Drone> drones)
        {
            if (drones == null) return null;
            try
            {
                for (int i = 0; i < drones.Count; i++)
                {
                    var d = drones[i];
                    if (d != null && HandleEquals(d.handle, handle))
                        return d;
                }
            }
            catch (Exception ex)
            {
                _log.Warning($"ResolveDrone failed: {ex.Message}");
            }
            return null;
        }

        private static Way ResolveHandleAsWay(Handle handle, Il2CppSystem.Collections.Generic.List<Way> ways)
        {
            if (ways == null) return null;
            try
            {
                for (int i = 0; i < ways.Count; i++)
                {
                    var w = ways[i];
                    if (w != null && HandleEquals(w.handle, handle))
                        return w;
                }
            }
            catch (Exception ex)
            {
                _log.Warning($"ResolveWay failed: {ex.Message}");
            }
            return null;
        }
    }
}
