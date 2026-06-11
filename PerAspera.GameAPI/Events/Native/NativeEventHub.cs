using System.Collections.Generic;
using HarmonyLib;
using PerAspera.Core;

namespace PerAspera.GameAPI.Events.Native
{
    /// <summary>
    /// Arguments passed to <see cref="NativeEventHub"/> handlers.
    /// Wraps the raw native <c>GameEvent</c> struct captured at dispatch time.
    ///
    /// Layout of <c>GameEvent</c> (72 bytes, blittable):
    /// <list type="bullet">
    ///   <item><description><c>sender</c> — Handle of the object that fired the event (typically the Building/Faction/Drone etc.).</description></item>
    ///   <item><description><c>target</c> — Optional second entity (context-dependent, e.g. target building for an upgrade).</description></item>
    ///   <item><description><c>payload</c> — Union of event-specific data (<c>building</c>, <c>drone</c>, <c>faction</c>, <c>key</c>…).</description></item>
    /// </list>
    /// Use the extension methods in <c>NativeEventExtensions</c> to resolve
    /// <c>Handle → Building / Faction</c> without reflection.
    /// </summary>
    /// <example>
    /// NativeEventHub.Subscribe(NativeGameEvent.BuildingBuilt, args => {
    ///     var building = args.ResolveBuilding(universe.keeper);
    ///     Log.Info($"Built: {building?.buildingType?.id}");
    /// });
    /// </example>
    public readonly struct NativeGameEventArgs
    {
        /// <summary>The managed event identifier (resolved from the native eventUid).</summary>
        public NativeGameEvent EventType { get; }

        /// <summary>
        /// Raw native event struct — <c>sender</c>/<c>target</c> handles and the
        /// <c>payload</c> union are accessible as typed fields (PerAspera.Events.GameEvent).
        /// </summary>
        public PerAspera.Events.GameEvent RawEvent { get; }

        /// <summary>
        /// The Handle of the object that fired this event (Building, Faction, Drone, etc.).
        /// Pass to <see cref="NativeEventExtensions.ResolveBuilding"/> or
        /// <see cref="NativeEventExtensions.ResolveFaction"/> to get the actual object.
        /// </summary>
        public Handle SenderHandle => RawEvent.sender;

        /// <summary>
        /// Optional secondary Handle (target context, e.g. new type in a type change event).
        /// Null/<c>Handle.Null</c> when not used by the event.
        /// </summary>
        public Handle TargetHandle => RawEvent.target;

        /// <summary>
        /// Union payload — event-specific data (attack damage, key string, faction sub-data, etc.).
        /// Access via typed fields: <c>Payload.building</c>, <c>Payload.faction</c>, <c>Payload.key</c>.
        /// </summary>
        public PerAspera.Events.GameEventPayload Payload => RawEvent.payload;

        internal NativeGameEventArgs(NativeGameEvent eventType, PerAspera.Events.GameEvent rawEvent)
        {
            EventType = eventType;
            RawEvent = rawEvent;
        }
    }

    /// <summary>
    /// Typed bridge to the game's native <c>GameEventBus</c>.
    ///
    /// A single Harmony Postfix on <c>GameEventBus.DispatchInternal</c> — the choke point
    /// every native event flows through (immediate AND deferred) — dispatches to managed
    /// handlers registered per <see cref="NativeGameEvent"/>. Event UIDs are resolved
    /// lazily via the typed static <c>Gev*</c> interop properties (zero reflection).
    ///
    /// PROTOTYPE STATUS (2026-06): the struct-by-value patch parameter must be validated
    /// in game once — enable <see cref="TraceAllEvents"/> and check the BepInEx log.
    /// </summary>
    /// <example>
    /// // In plugin Load():
    /// var harmony = new Harmony("com.mymod.id");
    /// NativeEventHub.Apply(harmony);
    ///
    /// // Remplace le polling de BaseGame.alreadyWokeUp :
    /// NativeEventHub.Subscribe(NativeGameEvent.UniverseNewGameStarted,
    ///     _ => LogAspera.Info("Partie démarrée !"));
    ///
    /// NativeEventHub.Subscribe(NativeGameEvent.FactionShipArrived,
    ///     args => LogAspera.Info("Cargo arrivé !"));
    /// </example>
    public static class NativeEventHub
    {
        private static readonly LogAspera _log = new LogAspera("GameAPI.Events.NativeEventHub");
        private static readonly object _lock = new();
        private static readonly Dictionary<int, List<Action<NativeGameEventArgs>>> _handlers = new();
        private static readonly Dictionary<NativeGameEvent, int> _uidCache = new();
        private static readonly Dictionary<int, NativeGameEvent> _uidReverse = new();
        private static bool _patched;
        private static bool _firstDispatchLogged;

        /// <summary>
        /// When true, every native event passing through the bus is logged
        /// (name + uid). Diagnostic / prototype-validation tool — leave off in production.
        /// </summary>
        public static bool TraceAllEvents { get; set; }

        /// <summary>
        /// Apply the Harmony patch on <c>GameEventBus.DispatchInternal</c>.
        /// Call once from your plugin's Load() — safe to call multiple times.
        /// </summary>
        /// <example>NativeEventHub.Apply(new Harmony("com.mymod.id"));</example>
        public static void Apply(Harmony harmony)
        {
            if (_patched) return;
            try
            {
                var original = AccessTools.Method(typeof(global::GameEventBus), "DispatchInternal");
                if (original == null)
                {
                    _log.Error("GameEventBus.DispatchInternal not found — native event bridge unavailable");
                    return;
                }
                var postfix = new HarmonyMethod(typeof(NativeEventHub), nameof(DispatchInternalPostfix));
                harmony.Patch(original, postfix: postfix);
                _patched = true;
                _log.Info("✅ NativeEventHub patched GameEventBus.DispatchInternal");
            }
            catch (Exception ex)
            {
                _log.Error($"❌ NativeEventHub.Apply failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Register a handler for a native game event.
        /// </summary>
        /// <param name="gameEvent">Event to listen to.</param>
        /// <param name="handler">Called synchronously after the game's own listeners ran.</param>
        /// <returns>True if registered, false if the native event UID could not be resolved.</returns>
        /// <example>
        /// NativeEventHub.Subscribe(NativeGameEvent.BuildingBuilt,
        ///     args => LogAspera.Info("Bâtiment construit !"));
        /// </example>
        public static bool Subscribe(NativeGameEvent gameEvent, Action<NativeGameEventArgs> handler)
        {
            if (handler == null) return false;

            int uid;
            try { uid = ResolveUid(gameEvent); }
            catch (Exception ex)
            {
                _log.Error($"Subscribe({gameEvent}): cannot resolve native eventUid — {ex.Message}");
                return false;
            }

            lock (_lock)
            {
                if (!_handlers.TryGetValue(uid, out var list))
                {
                    list = new List<Action<NativeGameEventArgs>>();
                    _handlers[uid] = list;
                }
                list.Add(handler);
            }
            _log.Info($"Subscribed to {gameEvent} (uid={uid})");
            return true;
        }

        /// <summary>Remove a previously registered handler.</summary>
        public static void Unsubscribe(NativeGameEvent gameEvent, Action<NativeGameEventArgs> handler)
        {
            if (handler == null) return;
            if (!_uidCache.TryGetValue(gameEvent, out int uid)) return;
            lock (_lock)
            {
                if (_handlers.TryGetValue(uid, out var list))
                    list.Remove(handler);
            }
        }

        /// <summary>Remove all handlers (plugin teardown / tests).</summary>
        public static void Clear()
        {
            lock (_lock) { _handlers.Clear(); }
        }

        // ==================== HARMONY PATCH ====================

        /// <summary>
        /// Postfix on <c>GameEventBus.DispatchInternal(GameEvent evt, IHandleable senderObjectHint)</c>.
        /// Parameter name MUST be <c>evt</c> (HarmonyX maps IL2CPP parameters by exact name).
        /// </summary>
        [HarmonyPostfix]
        public static void DispatchInternalPostfix(PerAspera.Events.GameEvent evt)
        {
            try
            {
                if (!_firstDispatchLogged)
                {
                    _firstDispatchLogged = true;
                    _log.Info($"🎯 First native event intercepted (uid={evt.type.eventUid}) — struct marshaling works!");
                }

                int uid = evt.type.eventUid;

                if (TraceAllEvents)
                {
                    string name = _uidReverse.TryGetValue(uid, out var known)
                        ? known.ToString()
                        : $"uid:{uid}";
                    _log.Info($"[trace] {name}");
                }

                List<Action<NativeGameEventArgs>>? list = null;
                lock (_lock)
                {
                    if (_handlers.TryGetValue(uid, out var registered) && registered.Count > 0)
                        list = new List<Action<NativeGameEventArgs>>(registered);
                }
                if (list == null) return;

                var eventType = _uidReverse.TryGetValue(uid, out var resolved)
                    ? resolved
                    : default;
                var args = new NativeGameEventArgs(eventType, evt);

                foreach (var handler in list)
                {
                    try { handler(args); }
                    catch (Exception ex)
                    {
                        _log.Error($"Handler for {eventType} threw: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"DispatchInternalPostfix failed: {ex.Message}");
            }
        }

        // ==================== UID RESOLUTION (typed interop, zero reflection) ====================

        /// <summary>
        /// Resolve the runtime eventUid of a <see cref="NativeGameEvent"/> via the typed
        /// static <c>Gev*</c> interop property of its owner class. Cached after first call.
        /// </summary>
        private static int ResolveUid(NativeGameEvent e)
        {
            lock (_lock)
            {
                if (_uidCache.TryGetValue(e, out int cached)) return cached;
            }

            int uid = e switch
            {
                // ── Building ──
                NativeGameEvent.BuildingAfterChangeBuildingType => global::Building.GevBuildingAfterChangeBuildingType.eventUid,
                NativeGameEvent.BuildingAttacked => global::Building.GevBuildingAttacked.eventUid,
                NativeGameEvent.BuildingAudioRelevantPropertyChanged => global::Building.GevBuildingAudioRelevantPropertyChanged.eventUid,
                NativeGameEvent.BuildingBeforeChangeBuildingType => global::Building.GevBuildingBeforeChangeBuildingType.eventUid,
                NativeGameEvent.BuildingBuilt => global::Building.GevBuildingBuilt.eventUid,
                NativeGameEvent.BuildingCanceledScrapping => global::Building.GevBuildingCanceledScrapping.eventUid,
                NativeGameEvent.BuildingCitizenBorn => global::Building.GevBuildingCitizenBorn.eventUid,
                NativeGameEvent.BuildingCitizenDied => global::Building.GevBuildingCitizenDied.eventUid,
                NativeGameEvent.BuildingCitizenStarving => global::Building.GevBuildingCitizenStarving.eventUid,
                NativeGameEvent.BuildingDamagedByAsteroid => global::Building.GevBuildingDamagedByAsteroid.eventUid,
                NativeGameEvent.BuildingDestroyedByDamage => global::Building.GevBuildingDestroyedByDamage.eventUid,
                NativeGameEvent.BuildingDistrictChangedActive => global::Building.GevBuildingDistrictChangedActive.eventUid,
                NativeGameEvent.BuildingExtendsClusterRangeChanged => global::Building.GevBuildingExtendsClusterRangeChanged.eventUid,
                NativeGameEvent.BuildingFinishedScrapping => global::Building.GevBuildingFinishedScrapping.eventUid,
                NativeGameEvent.BuildingInternalAdd => global::Building.GevBuildingInternalAdd.eventUid,
                NativeGameEvent.BuildingInternalAddNew => global::Building.GevBuildingInternalAddNew.eventUid,
                NativeGameEvent.BuildingInternalLoad => global::Building.GevBuildingInternalLoad.eventUid,
                NativeGameEvent.BuildingInternalPreRemove => global::Building.GevBuildingInternalPreRemove.eventUid,
                NativeGameEvent.BuildingInternalRemove => global::Building.GevBuildingInternalRemove.eventUid,
                NativeGameEvent.BuildingOperativeChanged => global::Building.GevBuildingOperativeChanged.eventUid,
                NativeGameEvent.BuildingOutOfPower => global::Building.GevBuildingOutOfPower.eventUid,
                NativeGameEvent.BuildingSelfDespawned => global::Building.GevBuildingSelfDespawned.eventUid,
                NativeGameEvent.BuildingSpawned => global::Building.GevBuildingSpawned.eventUid,
                NativeGameEvent.BuildingStartedRebuild => global::Building.GevBuildingStartedRebuild.eventUid,
                NativeGameEvent.BuildingStartedScrapping => global::Building.GevBuildingStartedScrapping.eventUid,
                NativeGameEvent.BuildingToggledScrapping => global::Building.GevBuildingToggledScrapping.eventUid,
                NativeGameEvent.BuildingUpgradeCanceled => global::Building.GevBuildingUpgradeCanceled.eventUid,
                NativeGameEvent.BuildingUpgradeStarted => global::Building.GevBuildingUpgradeStarted.eventUid,
                NativeGameEvent.BuildingUpgradeToggled => global::Building.GevBuildingUpgradeToggled.eventUid,
                NativeGameEvent.BuildingUpgradedTo => global::Building.GevBuildingUpgradedTo.eventUid,
                NativeGameEvent.FactoryProducedResource => global::Building.GevFactoryProducedResource.eventUid,

                // ── Drone ──
                NativeGameEvent.DroneDespawned => global::Drone.GevDroneDespawned.eventUid,
                NativeGameEvent.DroneInternalAdd => global::Drone.GevDroneInternalAdd.eventUid,
                NativeGameEvent.DroneInternalLoad => global::Drone.GevDroneInternalLoad.eventUid,
                NativeGameEvent.DroneInternalRemove => global::Drone.GevDroneInternalRemove.eventUid,
                NativeGameEvent.DroneSpawned => global::Drone.GevDroneSpawned.eventUid,
                NativeGameEvent.DroneStartWorking => global::Drone.GevDroneStartWorking.eventUid,
                NativeGameEvent.DroneStopWorking => global::Drone.GevDroneStopWorking.eventUid,

                // ── Faction ──
                NativeGameEvent.FactionAIWaveEnded => global::Faction.GevFactionAIWaveEnded.eventUid,
                NativeGameEvent.FactionAIWaveStarted => global::Faction.GevFactionAIWaveStarted.eventUid,
                NativeGameEvent.FactionBestDronesReassigned => global::Faction.GevFactionBestDronesReassigned.eventUid,
                NativeGameEvent.FactionBuildingTypeRemoved => global::Faction.GevFactionBuildingTypeRemoved.eventUid,
                NativeGameEvent.FactionBuildingTypeUnlocked => global::Faction.GevFactionBuildingTypeUnlocked.eventUid,
                NativeGameEvent.FactionCloseAllWindows => global::Faction.GevFactionCloseAllWindows.eventUid,
                NativeGameEvent.FactionColonistsDeparted => global::Faction.GevFactionColonistsDeparted.eventUid,
                NativeGameEvent.FactionDefeated => global::Faction.GevFactionDefeated.eventUid,
                NativeGameEvent.FactionDialogueFinished => global::Faction.GevFactionDialogueFinished.eventUid,
                NativeGameEvent.FactionDialoguePulse => global::Faction.GevFactionDialoguePulse.eventUid,
                NativeGameEvent.FactionDialogueStarted => global::Faction.GevFactionDialogueStarted.eventUid,
                NativeGameEvent.FactionElectricityClusteringChanged => global::Faction.GevFactionElectricityClusteringChanged.eventUid,
                NativeGameEvent.FactionKnowledgeRead => global::Faction.GevFactionKnowledgeRead.eventUid,
                NativeGameEvent.FactionKnowledgeUnlocked => global::Faction.GevFactionKnowledgeUnlocked.eventUid,
                NativeGameEvent.FactionMaintenanceClusteringChanged => global::Faction.GevFactionMaintenanceClusteringChanged.eventUid,
                NativeGameEvent.FactionMilitaryDroneKilled => global::Faction.GevFactionMilitaryDroneKilled.eventUid,
                NativeGameEvent.FactionOrbitalBuildingChanged => global::Faction.GevFactionOrbitalBuildingChanged.eventUid,
                NativeGameEvent.FactionPipesClusteringChanged => global::Faction.GevFactionPipesClusteringChanged.eventUid,
                NativeGameEvent.FactionQuestAborted => global::Faction.GevFactionQuestAborted.eventUid,
                NativeGameEvent.FactionQuestCompleted => global::Faction.GevFactionQuestCompleted.eventUid,
                NativeGameEvent.FactionQuestUnlocked => global::Faction.GevFactionQuestUnlocked.eventUid,
                NativeGameEvent.FactionResourceVeinRevealed => global::Faction.GevFactionResourceVeinRevealed.eventUid,
                NativeGameEvent.FactionRivalInitialized => global::Faction.GevFactionRivalInitialized.eventUid,
                NativeGameEvent.FactionScannerTileChanged => global::Faction.GevFactionScannerTileChanged.eventUid,
                NativeGameEvent.FactionScannerTileRevealed => global::Faction.GevFactionScannerTileRevealed.eventUid,
                NativeGameEvent.FactionSectorUnlocked => global::Faction.GevFactionSectorUnlocked.eventUid,
                NativeGameEvent.FactionSectorsCreated => global::Faction.GevFactionSectorsCreated.eventUid,
                NativeGameEvent.FactionShipArrived => global::Faction.GevFactionShipArrived.eventUid,
                NativeGameEvent.FactionShipMissed => global::Faction.GevFactionShipMissed.eventUid,
                NativeGameEvent.FactionSpecialProjectAdded => global::Faction.GevFactionSpecialProjectAdded.eventUid,
                NativeGameEvent.FactionSpecialProjectCompleted => global::Faction.GevFactionSpecialProjectCompleted.eventUid,
                NativeGameEvent.FactionSpecialProjectLaunchPerformed => global::Faction.GevFactionSpecialProjectLaunchPerformed.eventUid,
                NativeGameEvent.FactionSpecialProjectPortAdded => global::Faction.GevFactionSpecialProjectPortAdded.eventUid,
                NativeGameEvent.FactionSpecialProjectPortRemoved => global::Faction.GevFactionSpecialProjectPortRemoved.eventUid,
                NativeGameEvent.FactionSpecialProjectStageStarted => global::Faction.GevFactionSpecialProjectStageStarted.eventUid,
                NativeGameEvent.FactionSpecialSiteBuildingSpawned => global::Faction.GevFactionSpecialSiteBuildingSpawned.eventUid,
                NativeGameEvent.FactionSpecialSiteResearched => global::Faction.GevFactionSpecialSiteResearched.eventUid,
                NativeGameEvent.FactionSpecialSiteRevealed => global::Faction.GevFactionSpecialSiteRevealed.eventUid,
                NativeGameEvent.FactionSwarmDetected => global::Faction.GevFactionSwarmDetected.eventUid,
                NativeGameEvent.FactionTechnologyResearchFinished => global::Faction.GevFactionTechnologyResearchFinished.eventUid,
                NativeGameEvent.FactionTechnologyResearchStarted => global::Faction.GevFactionTechnologyResearchStarted.eventUid,
                NativeGameEvent.FactionWayTypeUnlocked => global::Faction.GevFactionWayTypeUnlocked.eventUid,
                NativeGameEvent.NotificationsListChanged => global::Faction.GevNotificationsListChanged.eventUid,

                // ── MaintenanceDrone / MilitaryDrone ──
                NativeGameEvent.MaintenanceDroneStartWorking => global::MaintenanceDrone.GevMaintenanceDroneStartWorking.eventUid,
                NativeGameEvent.MaintenanceDroneStopWorking => global::MaintenanceDrone.GevMaintenanceDroneStopWorking.eventUid,
                NativeGameEvent.MilitaryInflictDamage => global::MilitaryDrone.GevMilitaryInflictDamage.eventUid,

                // ── Planet ──
                NativeGameEvent.HazardDespawned => global::Planet.GevHazardDespawned.eventUid,
                NativeGameEvent.HazardSpawned => global::Planet.GevHazardSpawned.eventUid,
                NativeGameEvent.PlanetO2PressureChanged => global::Planet.GevPlanetO2PressureChanged.eventUid,
                NativeGameEvent.PlanetPressureCO2LevelChanged => global::Planet.GevPlanetPressureCO2LevelChanged.eventUid,
                NativeGameEvent.PlanetPressureChanged => global::Planet.GevPlanetPressureChanged.eventUid,
                NativeGameEvent.PlanetPressureO2LevelChanged => global::Planet.GevPlanetPressureO2LevelChanged.eventUid,
                NativeGameEvent.PlanetTemperatureChanged => global::Planet.GevPlanetTemperatureChanged.eventUid,
                NativeGameEvent.PlanetWaterStockChanged => global::Planet.GevPlanetWaterStockChanged.eventUid,

                // ── RailedCarrier / Swarm ──
                NativeGameEvent.RailedCarrierDespawned => global::RailedCarrier.GevRailedCarrierDespawned.eventUid,
                NativeGameEvent.RailedCarrierSpawned => global::RailedCarrier.GevRailedCarrierSpawned.eventUid,
                NativeGameEvent.SwarmDespawned => global::Swarm.GevSwarmDespawned.eventUid,
                NativeGameEvent.SwarmInternalAdd => global::Swarm.GevSwarmInternalAdd.eventUid,
                NativeGameEvent.SwarmInternalLoad => global::Swarm.GevSwarmInternalLoad.eventUid,
                NativeGameEvent.SwarmInternalRemove => global::Swarm.GevSwarmInternalRemove.eventUid,
                NativeGameEvent.SwarmSpawned => global::Swarm.GevSwarmSpawned.eventUid,

                // ── Universe ──
                NativeGameEvent.UniverseContinueEndedGame => global::Universe.GevUniverseContinueEndedGame.eventUid,
                NativeGameEvent.UniverseDayPassed => global::Universe.GevUniverseDayPassed.eventUid,
                NativeGameEvent.UniverseExplosion => global::Universe.GevUniverseExplosion.eventUid,
                NativeGameEvent.UniverseGameOver => global::Universe.GevUniverseGameOver.eventUid,
                NativeGameEvent.UniverseGameSpeedChanged => global::Universe.GevUniverseGameSpeedChanged.eventUid,
                NativeGameEvent.UniverseHideVein => global::Universe.GevUniverseHideVein.eventUid,
                NativeGameEvent.UniverseNewGameStarted => global::Universe.GevUniverseNewGameStarted.eventUid,
                NativeGameEvent.UniverseStatsUpdated => global::Universe.GevUniverseStatsUpdated.eventUid,
                NativeGameEvent.UniverseSwapFaction => global::Universe.GevUniverseSwapFaction.eventUid,

                // ── Way ──
                NativeGameEvent.WayChanged => global::Way.GevWayChanged.eventUid,
                NativeGameEvent.WayDespawned => global::Way.GevWayDespawned.eventUid,
                NativeGameEvent.WayInternalAdd => global::Way.GevWayInternalAdd.eventUid,
                NativeGameEvent.WayInternalLoad => global::Way.GevWayInternalLoad.eventUid,
                NativeGameEvent.WayInternalRemove => global::Way.GevWayInternalRemove.eventUid,
                NativeGameEvent.WayOperativeChanged => global::Way.GevWayOperativeChanged.eventUid,
                NativeGameEvent.WaySpawned => global::Way.GevWaySpawned.eventUid,
                NativeGameEvent.WayUpgradeCancel => global::Way.GevWayUpgradeCancel.eventUid,
                NativeGameEvent.WayUpgradeStart => global::Way.GevWayUpgradeStart.eventUid,
                NativeGameEvent.WayUpgradeToggleOn => global::Way.GevWayUpgradeToggleOn.eventUid,
                NativeGameEvent.WayUpgraded => global::Way.GevWayUpgraded.eventUid,

                // ── Zeppelin ──
                NativeGameEvent.ZeppelinDespawned => global::Zeppelin.GevZeppelinDespawned.eventUid,
                NativeGameEvent.ZeppelinSpawned => global::Zeppelin.GevZeppelinSpawned.eventUid,

                _ => throw new ArgumentOutOfRangeException(nameof(e), e, "Unknown NativeGameEvent"),
            };

            lock (_lock)
            {
                _uidCache[e] = uid;
                _uidReverse[uid] = e;
            }
            return uid;
        }

        /// <summary>
        /// Resolve ALL event UIDs eagerly — fills the reverse map so that
        /// <see cref="TraceAllEvents"/> shows names instead of raw uids.
        /// Call after the game is loaded (UIDs are registered during native static init).
        /// </summary>
        /// <returns>Number of events successfully resolved.</returns>
        /// <example>
        /// NativeEventHub.ResolveAll();
        /// NativeEventHub.TraceAllEvents = true;
        /// </example>
        public static int ResolveAll()
        {
            int ok = 0;
            foreach (NativeGameEvent e in Enum.GetValues(typeof(NativeGameEvent)))
            {
                try { ResolveUid(e); ok++; }
                catch (Exception ex)
                {
                    _log.Warning($"ResolveAll: {e} failed — {ex.Message}");
                }
            }
            _log.Info($"ResolveAll: {ok}/{Enum.GetValues(typeof(NativeGameEvent)).Length} native event UIDs resolved");
            return ok;
        }
    }
}
