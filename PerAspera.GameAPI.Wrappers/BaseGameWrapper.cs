using System;
using System.Collections.Generic;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for BaseGame singleton
    /// Provides type-safe access to main game systems
    /// 
    /// 📚 Vanilla Reference: F:\ModPeraspera\CleanedScriptAssemblyClass\BaseGame.md (349 fields, 145 methods)
    /// 🤖 Agent Expert: @per-aspera-sdk-coordinator
    /// 🌐 User Wiki: https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/sdk/
    /// 📝 Examples: F:\ModPeraspera\Individual-Mods\PerAspera-CommandsDemo\BaseGame usage
    /// </summary>
    public class BaseGameWrapper : WrapperBase
    {
        public BaseGameWrapper(object nativeBaseGame) : base(nativeBaseGame)
        {

        }

        

        /// <summary>
        /// Get current BaseGame singleton instance
        /// </summary>
        public static BaseGameWrapper? GetCurrent()
        {
            var instance = GameTypeInitializer.GetBaseGameInstance();
            
            return instance != null ? new BaseGameWrapper(instance) : null;
        }
        
        // ==================== CORE SYSTEMS ====================
        
        /// <summary>
        /// Get Keeper instance (entity registry and manager)
        /// Property: keeper { get; private set; }
        /// Backing field: _keeper_k__BackingField
        /// </summary>
        public KeeperWrapper? GetKeeper()
        {
            return  new KeeperWrapper( SafeInvoke<Keeper>("get_keeper"));
        }
        
        /// <summary>
        /// Get Universe wrapper instance (time, factions, planet)
        /// Property: universe { get; }
        /// Backing field: _universe_k__BackingField
        /// </summary>
        public UniverseWrapper? GetUniverse()
        {
            var nativeUniverse = SafeInvoke<object>("get_universe");
            return nativeUniverse != null ? new UniverseWrapper(nativeUniverse) : null;
        }
        
        /// <summary>
        /// Check if game is in quitting state
        /// </summary>
        public bool IsQuitting
        {
            get => SafeInvoke<bool?>("get_isQuitting") ?? false;
        }
        
        /// <summary>
        /// Check if game is ending (credits/end sequence)
        /// </summary>
        public bool IsEnding
        {
            get => SafeInvoke<bool?>("get_isEnding") ?? false;
        }
        
        // ==================== INITIALIZATION STATE ====================
        
        /// <summary>
        /// Check if main scene has finished initialization
        /// Field: MainSceneHasFinishedInit
        /// </summary>
        public bool MainSceneFinishedInit
        {
            get => NativeObject?.GetFieldValue<bool>("MainSceneHasFinishedInit") ?? false;
        }
        
        /// <summary>
        /// Check if game is currently loading
        /// Field: isLoading
        /// </summary>
        public bool IsLoading
        {
            get => NativeObject?.GetFieldValue<bool>("isLoading") ?? false;
        }
        
        /// <summary>
        /// Get loaded save file name
        /// Field: loadedSave
        /// </summary>
        public string? LoadedSave
        {
            get => NativeObject?.GetFieldValue<string>("loadedSave");
        }
        
        // ==================== GAME STATE ====================
        
        /// <summary>
        /// Check if game has cheats enabled
        /// Field: hasCheats
        /// </summary>
        public bool HasCheats
        {
            get => NativeObject?.GetFieldValue<bool>("hasCheats") ?? false;
        }
        
        /// <summary>
        /// Check if game has mods enabled
        /// Field: hasMods
        /// </summary>
        public bool HasMods
        {
            get => NativeObject?.GetFieldValue<bool>("hasMods") ?? false;
        }
        
        /// <summary>
        /// Get current difficulty level
        /// Field: difficulty
        /// </summary>
        public int Difficulty
        {
            get => NativeObject?.GetFieldValue<int>("difficulty") ?? 0;
        }
        
        // ==================== METHODS ====================
        
        /// <summary>
        /// Trigger OnFinishLoading callback
        /// Method: OnFinishLoading()
        /// </summary>
        public void OnFinishLoading()
        {
            SafeInvokeVoid("OnFinishLoading");
        }
        
        // ==================== SYSTEM REFERENCES ====================
        
        /// <summary>
        /// Get camera controller reference
        /// Maps to: cameraController field
        /// </summary>
        public object? CameraController
        {
            get => SafeInvoke<object>("get_cameraController");
        }
        
        /// <summary>
        /// Get mars terrain manager
        /// Maps to: marsManager field
        /// </summary>
        public object? MarsManager
        {
            get => SafeInvoke<object>("get_marsManager");
        }
        
        /// <summary>
        /// Get input raycaster for UI interactions
        /// Maps to: inputRaycaster field
        /// </summary>
        public object? InputRaycaster
        {
            get => SafeInvoke<object>("get_inputRaycaster");
        }
        
        

        
        /// <summary>
        /// Exit to main menu
        /// Method: ExitToMainMenu()
        /// </summary>
        public void ExitToMainMenu()
        {
            SafeInvokeVoid("ExitToMainMenu");
        }
        
        /// <summary>
        /// Force exit game
        /// Method: ForceExit()
        /// </summary>
        public void ForceExit()
        {
            SafeInvokeVoid("ForceExit");
        }
        
        // ==================== BLACKBOARD ACCESS ====================
        
        /// <summary>
        /// Get the main blackboard via Universe
        /// Convenience method: BaseGame -> Universe -> BlackBoard
        /// </summary>
        public BlackBoardWrapper? GetMainBlackBoard()
        {
            var universe = GetUniverse();
            return  new BlackBoardWrapper( universe?.GetMainBlackBoard());
        }
        
        /// <summary>
        /// Get a specific blackboard by name via Universe
        /// Convenience method: BaseGame -> Universe -> BlackBoard
        /// </summary>
        public BlackBoardWrapper? GetBlackBoard(string name)
        {
            var universe = GetUniverse();
            return universe?.GetBlackBoard(name);
        }
        
        /// <summary>
        /// Check if a blackboard exists via Universe
        /// Convenience method: BaseGame -> Universe -> BlackBoard
        /// </summary>
        public bool HasBlackBoard(string name)
        {
            var universe = GetUniverse();
            return universe?.HasBlackBoard(name) ?? false;
        }
        
        /// <summary>
        /// Get all blackboard names via Universe
        /// Convenience method: BaseGame -> Universe -> BlackBoard
        /// </summary>
        public IList<string>? GetBlackBoardNames()
        {
            var universe = GetUniverse();
            return universe?.GetBlackBoardNames();
        }
        
        public override string ToString()
        {
            var universe = GetUniverse();
            var blackboardCount = universe?.GetBlackBoardCount() ?? 0;
            return $"BaseGame [Loading:{IsLoading}, Quitting:{IsQuitting}, HasMods:{HasMods}, Blackboards:{blackboardCount}]";
        }
    }
}
