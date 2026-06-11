// Mirror enum généré depuis le dump — les 121 noms sont auto-descriptifs,
// la doc est portée par le type (CS1591 désactivé pour les membres).
#pragma warning disable CS1591

namespace PerAspera.GameAPI.Events.Native
{
    /// <summary>
    /// Managed mirror of every native <c>GameEventType</c> exposed by the game
    /// (the static <c>Gev*</c> properties on Building, Faction, Universe, Planet…).
    /// Generated from Tools/InteropDump (2026-06) — 121 events, 11 owner classes.
    /// Use with <see cref="NativeEventHub.Subscribe"/>.
    /// </summary>
    /// <example>
    /// NativeEventHub.Subscribe(NativeGameEvent.UniverseDayPassed, args =>
    ///     LogAspera.Info("Un sol martien est passé !"));
    /// </example>
    public enum NativeGameEvent
    {
        // ── Building (owner: global::Building) ──────────────────────────
        BuildingAfterChangeBuildingType,
        BuildingAttacked,
        BuildingAudioRelevantPropertyChanged,
        BuildingBeforeChangeBuildingType,
        BuildingBuilt,
        BuildingCanceledScrapping,
        BuildingCitizenBorn,
        BuildingCitizenDied,
        BuildingCitizenStarving,
        BuildingDamagedByAsteroid,
        BuildingDestroyedByDamage,
        BuildingDistrictChangedActive,
        BuildingExtendsClusterRangeChanged,
        BuildingFinishedScrapping,
        BuildingInternalAdd,
        BuildingInternalAddNew,
        BuildingInternalLoad,
        BuildingInternalPreRemove,
        BuildingInternalRemove,
        BuildingOperativeChanged,
        BuildingOutOfPower,
        BuildingSelfDespawned,
        BuildingSpawned,
        BuildingStartedRebuild,
        BuildingStartedScrapping,
        BuildingToggledScrapping,
        BuildingUpgradeCanceled,
        BuildingUpgradeStarted,
        BuildingUpgradeToggled,
        BuildingUpgradedTo,
        FactoryProducedResource,

        // ── Drone (owner: global::Drone) ─────────────────────────────────
        DroneDespawned,
        DroneInternalAdd,
        DroneInternalLoad,
        DroneInternalRemove,
        DroneSpawned,
        DroneStartWorking,
        DroneStopWorking,

        // ── Faction (owner: global::Faction) ─────────────────────────────
        FactionAIWaveEnded,
        FactionAIWaveStarted,
        FactionBestDronesReassigned,
        FactionBuildingTypeRemoved,
        FactionBuildingTypeUnlocked,
        FactionCloseAllWindows,
        FactionColonistsDeparted,
        FactionDefeated,
        FactionDialogueFinished,
        FactionDialoguePulse,
        FactionDialogueStarted,
        FactionElectricityClusteringChanged,
        FactionKnowledgeRead,
        FactionKnowledgeUnlocked,
        FactionMaintenanceClusteringChanged,
        FactionMilitaryDroneKilled,
        FactionOrbitalBuildingChanged,
        FactionPipesClusteringChanged,
        FactionQuestAborted,
        FactionQuestCompleted,
        FactionQuestUnlocked,
        FactionResourceVeinRevealed,
        FactionRivalInitialized,
        FactionScannerTileChanged,
        FactionScannerTileRevealed,
        FactionSectorUnlocked,
        FactionSectorsCreated,
        FactionShipArrived,
        FactionShipMissed,
        FactionSpecialProjectAdded,
        FactionSpecialProjectCompleted,
        FactionSpecialProjectLaunchPerformed,
        FactionSpecialProjectPortAdded,
        FactionSpecialProjectPortRemoved,
        FactionSpecialProjectStageStarted,
        FactionSpecialSiteBuildingSpawned,
        FactionSpecialSiteResearched,
        FactionSpecialSiteRevealed,
        FactionSwarmDetected,
        FactionTechnologyResearchFinished,
        FactionTechnologyResearchStarted,
        FactionWayTypeUnlocked,
        NotificationsListChanged,

        // ── MaintenanceDrone (owner: global::MaintenanceDrone) ───────────
        MaintenanceDroneStartWorking,
        MaintenanceDroneStopWorking,

        // ── MilitaryDrone (owner: global::MilitaryDrone) ─────────────────
        MilitaryInflictDamage,

        // ── Planet (owner: global::Planet) ───────────────────────────────
        HazardDespawned,
        HazardSpawned,
        PlanetO2PressureChanged,
        PlanetPressureCO2LevelChanged,
        PlanetPressureChanged,
        PlanetPressureO2LevelChanged,
        PlanetTemperatureChanged,
        PlanetWaterStockChanged,

        // ── RailedCarrier (owner: global::RailedCarrier) ─────────────────
        RailedCarrierDespawned,
        RailedCarrierSpawned,

        // ── Swarm (owner: global::Swarm) ─────────────────────────────────
        SwarmDespawned,
        SwarmInternalAdd,
        SwarmInternalLoad,
        SwarmInternalRemove,
        SwarmSpawned,

        // ── Universe (owner: global::Universe) ───────────────────────────
        UniverseContinueEndedGame,
        UniverseDayPassed,
        UniverseExplosion,
        UniverseGameOver,
        UniverseGameSpeedChanged,
        UniverseHideVein,
        UniverseNewGameStarted,
        UniverseStatsUpdated,
        UniverseSwapFaction,

        // ── Way (owner: global::Way) ─────────────────────────────────────
        WayChanged,
        WayDespawned,
        WayInternalAdd,
        WayInternalLoad,
        WayInternalRemove,
        WayOperativeChanged,
        WaySpawned,
        WayUpgradeCancel,
        WayUpgradeStart,
        WayUpgradeToggleOn,
        WayUpgraded,

        // ── Zeppelin (owner: global::Zeppelin) ───────────────────────────
        ZeppelinDespawned,
        ZeppelinSpawned,
    }
}
