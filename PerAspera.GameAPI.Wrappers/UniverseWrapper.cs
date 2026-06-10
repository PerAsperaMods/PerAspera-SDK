#nullable enable
using System;
using System.Collections.Generic;
using PerAspera.Commands;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native Universe class.
    /// Provides safe access to time, game state and universe-level properties.
    ///
    /// MIGRATION 2026-06-10 — interop typé d'abord : délégation au proxy <see cref="global::Universe"/>.
    /// Vérifié contre Tools\InteropDump\ScriptsAssembly\Universe.cs. Corrections au passage :
    /// « SetPaused » n'existe pas (vrais noms : SetGamePaused/ToggleGamePaused — Pause/Unpause
    /// n'avaient JAMAIS fonctionné), « GetBaseGame » n'existe pas sur Universe, et
    /// AddBlackBoard passait le wrapper au lieu de l'objet natif.
    ///
    /// 🤖 Agent Expert: @per-aspera-sdk-coordinator
    /// </summary>
    public class UniverseWrapper : WrapperBase
    {
        /// <summary>Wraps an untyped native Universe (compat). Prefer the typed overload.</summary>
        public UniverseWrapper(object nativeUniverse) : base(nativeUniverse) { }

        /// <summary>Wraps a typed interop Universe proxy.</summary>
        public UniverseWrapper(Universe nativeUniverse) : base(nativeUniverse) { }

        /// <summary>Typed interop proxy (null when the wrapper is invalid).</summary>
        /// <example>int sol = universe.NativeUniverse?.GetMartianSol() ?? 0;</example>
        public Universe? NativeUniverse => GetNativeObject() as Universe;

        /// <summary>Get the current universe instance.</summary>
        /// <example>var universe = UniverseWrapper.GetCurrent();</example>
        public static UniverseWrapper? GetCurrent()
        {
            var universe = KeeperTypeRegistry.GetUniverse();
            return universe != null ? new UniverseWrapper(universe) : null;
        }

        // ==================== CORE SYSTEMS ====================

        /// <summary>Get the SliceMaster (planet slicing system).</summary>
        public SliceMasterWrapper GetSliceMaster()
            => new SliceMasterWrapper(NativeUniverse?.sliceMaster);

        /// <summary>Get the native CommandBus (typed).</summary>
        public CommandBus? GetCommandBus() => NativeUniverse?.commandBus;

        /// <summary>Get Keeper instance for Universe entities (factions, planets, resources).</summary>
        public KeeperWrapper? GetKeeper()
        {
            var keeper = NativeUniverse?.keeper;
            return keeper != null ? new KeeperWrapper(keeper) : null;
        }

        /// <summary>Get the player faction.</summary>
        /// <example>var player = universe.GetPlayerFaction();</example>
        public FactionWrapper? GetPlayerFaction()
            => FactionWrapper.FromNative(NativeUniverse?.GetPlayerFaction());

        // ==================== TIME PROPERTIES ====================

        /// <summary>Current Martian sol — days passed (typed Universe.GetMartianSol()).</summary>
        public int CurrentSol => NativeUniverse?.GetMartianSol() ?? 0;

        /// <summary>Current game speed multiplier (typed Get/SetGameSpeed).</summary>
        public float GameSpeed
        {
            get => NativeUniverse?.GetGameSpeed() ?? 1.0f;
            set => NativeUniverse?.SetGameSpeed(value);
        }

        /// <summary>
        /// Number of simulation ticks per in-game day (static native field Universe.TICKS_PER_DAY).
        /// Higher values = faster simulation.
        /// </summary>
        public int TicksPerDay
        {
            get => Universe.TICKS_PER_DAY;
            set => Universe.TICKS_PER_DAY = value;
        }

        /// <summary>Check if game is paused (typed Universe.GetGamePaused()).</summary>
        public bool IsPaused => NativeUniverse?.GetGamePaused() ?? false;

        // ==================== GAME STATE ====================

        /// <summary>Get the planet wrapper instance.</summary>
        /// <example>var planet = universe.GetPlanet();</example>
        public PlanetWrapper? GetPlanet()
        {
            var planet = NativeUniverse?.GetPlanet();
            return planet != null ? new PlanetWrapper(planet) : null;
        }

        /// <summary>Get the current planet instance (alias for GetPlanet).</summary>
        public PlanetWrapper? CurrentPlanet => GetPlanet();

        /// <summary>
        /// Get the base game wrapper instance.
        /// (« GetBaseGame » n'existe pas sur Universe — délègue à BaseGameWrapper.GetCurrent().)
        /// </summary>
        public BaseGameWrapper? GetBaseGame() => BaseGameWrapper.GetCurrent();

        /// <summary>Get all factions in the universe (typed read of Universe.factions).</summary>
        /// <example>foreach (var f in universe.GetFactions()) { ... }</example>
        public List<FactionWrapper> GetFactions()
        {
            var result = new List<FactionWrapper>();
            var factions = NativeUniverse?.factions;
            if (factions == null) return result;
            foreach (var faction in factions)
                if (faction != null) result.Add(new FactionWrapper(faction));
            return result;
        }

        // ==================== ACTIONS ====================

        /// <summary>
        /// Pause the game (typed Universe.SetGamePaused(true)).
        /// ⚠️ L'ancienne implémentation visait « SetPaused » qui n'existe pas — elle
        /// n'a jamais rien fait. Celle-ci fonctionne réellement.
        /// </summary>
        public void Pause() => NativeUniverse?.SetGamePaused(true);

        /// <summary>Unpause the game (typed Universe.SetGamePaused(false)).</summary>
        public void Unpause() => NativeUniverse?.SetGamePaused(false);

        /// <summary>Toggle pause state (typed Universe.ToggleGamePaused()).</summary>
        public void TogglePause() => NativeUniverse?.ToggleGamePaused();

        // ==================== BLACKBOARD SYSTEM ====================
        // blackboardMain et le dictionnaire blackboards sont exposés typés par le proxy.

        /// <summary>Get the main blackboard instance (lu par les règles YAML MISSION, scope « main. »).</summary>
        /// <example>universe.GetMainBlackBoard()?.SetValue("mon_flag", true);</example>
        public BlackBoardWrapper? GetMainBlackBoard()
        {
            var bb = NativeUniverse?.blackboardMain;
            return bb != null ? new BlackBoardWrapper(bb) : null;
        }

        /// <summary>Get a specific blackboard by name (typed dictionary access).</summary>
        public BlackBoardWrapper? GetBlackBoard(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            var dict = NativeUniverse?.blackboards;
            if (dict == null || !dict.ContainsKey(name)) return null;
            var bb = dict[name];
            return bb != null ? new BlackBoardWrapper(bb) : null;
        }

        /// <summary>Get all blackboard names.</summary>
        public IList<string> GetBlackBoardNames()
        {
            var result = new List<string>();
            var dict = NativeUniverse?.blackboards;
            if (dict == null) return result;
            foreach (var kvp in dict) result.Add(kvp.Key);
            return result;
        }

        /// <summary>Check if a blackboard with the given name exists.</summary>
        public bool HasBlackBoard(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            return NativeUniverse?.blackboards?.ContainsKey(name) ?? false;
        }

        /// <summary>
        /// Add a new blackboard to the universe (typed Universe.AddBlackboard).
        /// ⚠️ L'ancienne implémentation passait le WRAPPER à l'appel natif (bug) —
        /// l'objet natif est maintenant déballé correctement.
        /// </summary>
        public void AddBlackBoard(BlackBoardWrapper blackboard)
        {
            var native = blackboard?.GetNativeObject() as Blackboard;
            if (native == null)
            {
                WrapperLog.Warning("Cannot add null/invalid blackboard to Universe");
                return;
            }
            NativeUniverse?.AddBlackboard(native);
        }

        /// <summary>Get count of blackboards in the universe.</summary>
        public int GetBlackBoardCount() => NativeUniverse?.blackboards?.Count ?? 0;

        // ==================== INFO ====================

        /// <summary>Human-readable universe state summary.</summary>
        public override string ToString()
        {
            var blackboardCount = GetBlackBoardCount();
            var mainBlackboardName = GetMainBlackBoard()?.Name ?? "None";
            return $"Universe: Sol {CurrentSol}, Speed={GameSpeed}x, Paused={IsPaused}, Blackboards={blackboardCount}, MainBB={mainBlackboardName}";
        }
    }
}
