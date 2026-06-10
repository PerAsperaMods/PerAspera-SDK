#nullable enable
using System;
using System.Collections.Generic;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for BaseGame singleton.
    /// Provides type-safe access to main game systems.
    ///
    /// MIGRATION 2026-06-10 — interop typé d'abord : délégation au proxy <see cref="global::BaseGame"/>.
    /// Vérifié contre Tools\InteropDump\ScriptsAssembly\BaseGame.cs. Les anciens doubles
    /// chemins « typé + fallback réflexion » ont été supprimés (le fallback ne servait à
    /// rien : si le membre typé compile, il existe). Corrections au passage :
    /// loadedSave/hasCheats/hasMods/difficulty sont STATIQUES (les lectures d'instance de
    /// l'ancien code échouaient silencieusement), et currentDay n'existe pas sur BaseGame
    /// (le sol courant vient de Universe.GetMartianSol()).
    ///
    /// 🤖 Agent Expert: @per-aspera-sdk-coordinator
    /// </summary>
    public class BaseGameWrapper : WrapperBase
    {
        /// <summary>Wraps an untyped native BaseGame (compat). Prefer the typed overload.</summary>
        public BaseGameWrapper(object nativeBaseGame) : base(nativeBaseGame) { }

        /// <summary>Wraps a typed interop BaseGame proxy.</summary>
        public BaseGameWrapper(BaseGame nativeBaseGame) : base(nativeBaseGame) { }

        /// <summary>Typed interop proxy (null when the wrapper is invalid).</summary>
        /// <example>var keeper = baseGame.NativeBaseGame?.keeper;</example>
        public BaseGame? NativeBaseGame => GetNativeObject() as BaseGame;

        /// <summary>Get current BaseGame singleton instance.</summary>
        /// <example>var baseGame = BaseGameWrapper.GetCurrent();</example>
        public static BaseGameWrapper? GetCurrent()
        {
            var instance = GameTypeInitializer.GetBaseGameInstance();
            return instance != null ? new BaseGameWrapper(instance) : null;
        }

        // ==================== GAME STATE ====================

        /// <summary>Check if the game is paused (delegates to Universe).</summary>
        public bool IsPaused() => GetUniverse()?.IsPaused ?? false;

        /// <summary>
        /// Current martian sol (game day). Typed call to Universe.GetMartianSol().
        /// (L'ancien binding « currentDay » n'existait pas sur BaseGame — retournait null.)
        /// </summary>
        /// <example>int? sol = baseGame.GetCurrentDay();</example>
        public int? GetCurrentDay() => NativeBaseGame?.universe?.GetMartianSol();

        // ==================== CORE SYSTEMS ====================

        /// <summary>Get Keeper instance (entity registry and manager).</summary>
        /// <example>var keeper = baseGame.GetKeeper();</example>
        public KeeperWrapper? GetKeeper()
        {
            var keeper = NativeBaseGame?.keeper;
            return keeper != null ? new KeeperWrapper(keeper) : null;
        }

        /// <summary>Get Universe wrapper instance (time, factions, planet).</summary>
        /// <example>var universe = baseGame.GetUniverse();</example>
        public UniverseWrapper? GetUniverse()
        {
            var universe = NativeBaseGame?.universe;
            return universe != null ? new UniverseWrapper(universe) : null;
        }

        /// <summary>Check if game is in quitting state (static native property).</summary>
        public bool IsQuitting => BaseGame.isQuitting;

        /// <summary>Check if game is ending — credits/end sequence (static native property).</summary>
        public bool IsEnding => BaseGame.isEnding;

        // ==================== INITIALIZATION STATE ====================

        /// <summary>Check if main scene has finished initialization.</summary>
        public bool MainSceneFinishedInit => NativeBaseGame?.MainSceneHasFinishedInit ?? false;

        /// <summary>Check if game is currently loading.</summary>
        public bool IsLoading => NativeBaseGame?.isLoading ?? false;

        /// <summary>
        /// Loaded save file name (static native field, tuple name+header).
        /// (L'ancien code lisait « loadedSave » comme champ d'instance string — toujours null.)
        /// </summary>
        public string? LoadedSave => BaseGame.loadedSave?.Item1;

        // ==================== GAME STATE FLAGS ====================

        /// <summary>Check if game has cheats enabled (static native field).</summary>
        public bool HasCheats => BaseGame.hasCheats;

        /// <summary>Check if game has mods enabled (static native field).</summary>
        public bool HasMods => BaseGame.hasMods;

        /// <summary>Current difficulty (native BaseGame.Difficulty enum, static field).</summary>
        /// <example>var diff = baseGame.GameDifficulty;</example>
        public BaseGame.Difficulty GameDifficulty => BaseGame.difficulty;

        /// <summary>N'a jamais fonctionné — « difficulty » est un champ STATIQUE de type Difficulty.</summary>
        [Obsolete("Lecture d'instance int de « difficulty » — toujours 0 (champ statique de type Difficulty). Utiliser GameDifficulty.", false)]
        public int Difficulty => 0;

        /// <summary>List of enabled mod names (static native field).</summary>
        public IList<string> GetModList()
        {
            var result = new List<string>();
            var mods = BaseGame.modList;
            if (mods == null) return result;
            foreach (var m in mods) result.Add(m);
            return result;
        }

        // ==================== SYSTEM REFERENCES ====================

        /// <summary>Canvas references for UI components (typed).</summary>
        public GameCanvasReferences? CanvasRefs => NativeBaseGame?.canvasRefs;

        /// <summary>Camera controller (typed OrbitingCamera).</summary>
        public OrbitingCamera? CameraController => NativeBaseGame?.cameraController;

        /// <summary>Mars terrain manager (typed).</summary>
        public MarsManager? MarsManager => NativeBaseGame?.marsManager;

        /// <summary>Input raycaster for UI interactions (typed).</summary>
        public InputRaycaster? InputRaycaster => NativeBaseGame?.inputRaycaster;

        /// <summary>Base game references wrapper.</summary>
        public BaseGameReferencesWrapper? baseGameReferences
        {
            get
            {
                var refs = NativeBaseGame?.baseGameReferences;
                return refs != null ? new BaseGameReferencesWrapper(refs) : null;
            }
        }

        // ==================== METHODS ====================

        /// <summary>Trigger OnFinishLoading callback.</summary>
        public void OnFinishLoading() => NativeBaseGame?.OnFinishLoading();

        /// <summary>Exit to main menu.</summary>
        public void ExitToMainMenu() => NativeBaseGame?.ExitToMainMenu();

        /// <summary>Force exit game.</summary>
        public void ForceExit() => NativeBaseGame?.ForceExit();

        // ==================== BLACKBOARD ACCESS ====================

        /// <summary>Get the main blackboard via Universe.</summary>
        public BlackBoardWrapper? GetMainBlackBoard()
        {
            var universe = GetUniverse();
            return new BlackBoardWrapper(universe?.GetMainBlackBoard());
        }

        /// <summary>Get a specific blackboard by name via Universe.</summary>
        public BlackBoardWrapper? GetBlackBoard(string name)
            => GetUniverse()?.GetBlackBoard(name);

        /// <summary>Check if a blackboard exists via Universe.</summary>
        public bool HasBlackBoard(string name)
            => GetUniverse()?.HasBlackBoard(name) ?? false;

        /// <summary>Get all blackboard names via Universe.</summary>
        public IList<string>? GetBlackBoardNames()
            => GetUniverse()?.GetBlackBoardNames();

        // ==================== VISUAL POI ACCESS ====================

        /// <summary>
        /// Get wrapped list of all visual POI instances in the game
        /// (vanilla + YAML mods). Typed read of BaseGame.pois.
        /// </summary>
        /// <example>foreach (var poi in baseGame.GetPois()!) { ... }</example>
        public IList<VisualPointOfInterestWrapper> GetPois()
        {
            var result = new List<VisualPointOfInterestWrapper>();
            var nativePois = NativeBaseGame?.pois;
            if (nativePois == null) return result;
            foreach (var visPoi in nativePois)
                if (visPoi != null) result.Add(new VisualPointOfInterestWrapper(visPoi));
            return result;
        }

        /// <summary>Visual special sites with their resource veins (typed read of BaseGame.visualSites).</summary>
        public Dictionary<SpecialSiteWrapper, VisualResourceVeinWrapper>? GetVisualSites()
        {
            var sites = NativeBaseGame?.visualSites;
            if (sites == null) return null;

            var result = new Dictionary<SpecialSiteWrapper, VisualResourceVeinWrapper>();
            foreach (var kvp in sites)
                result[new SpecialSiteWrapper(kvp.Key)] = new VisualResourceVeinWrapper(kvp.Value);
            return result;
        }

        /// <summary>Human-readable game state summary.</summary>
        public override string ToString()
        {
            var universe = GetUniverse();
            var blackboardCount = universe?.GetBlackBoardCount() ?? 0;
            return $"BaseGame [Loading:{IsLoading}, Quitting:{IsQuitting}, HasMods:{HasMods}, Blackboards:{blackboardCount}]";
        }
    }
}
