using System;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for BaseGame singleton
    /// Provides type-safe access to main game systems
    /// DOC: BaseGame.md - Main game controller and singleton
    /// </summary>
    public class BaseGame : WrapperBase
    {
        public BaseGame(object nativeBaseGame) : base(nativeBaseGame)
        {
        }
        
        /// <summary>
        /// Get current BaseGame singleton instance
        /// </summary>
        public static BaseGame? GetCurrent()
        {
            var instance = GameTypeInitializer.GetBaseGameInstance();
            
            return instance != null ? new BaseGame(instance) : null;
        }
        
        // ==================== CORE SYSTEMS ====================
        
        /// <summary>
        /// Get Keeper instance (entity registry and manager)
        /// Property: keeper { get; private set; }
        /// Backing field: _keeper_k__BackingField
        /// </summary>
        public object? GetKeeper()
        {
            return SafeInvoke<object>("get_keeper");
        }
        
        /// <summary>
        /// Get Universe instance (time, factions, planet)
        /// Property: universe { get; }
        /// Backing field: _universe_k__BackingField
        /// </summary>
        public object? GetUniverse()
        {
            return SafeInvoke<object>("get_universe");
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
        
        public override string ToString()
        {
            return $"BaseGame [Loading:{IsLoading}, Quitting:{IsQuitting}, HasMods:{HasMods}]";
        }
    }
}
