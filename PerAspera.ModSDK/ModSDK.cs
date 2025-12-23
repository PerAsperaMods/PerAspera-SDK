using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core;
using PerAspera.GameAPI;
namespace PerAspera.ModSDK
{
    /// <summary>
    /// PerAspera ModSDK - Your gateway to Per Aspera modding
    /// Refactored for better maintainability and separation of concerns
    /// </summary>
    public static class ModSDK
    {
        private static readonly LogAspera _log = new LogAspera(nameof(ModSDK));
        private static bool _isInitialized = false;
        private static string _modName = "Unknown";

        // TODO: Restore baseGame when Keeper/Wrappers reimplemented
        // private static MirrorBaseGame baseGame;

        // TODO: Subscribe to BaseGameDetected event



        /// <summary>
        /// Initialize the SDK for your mod
        /// </summary>
        public static void Initialize(string modName, string modVersion = "1.0.0")
        {
            if (_isInitialized)
            {
                                _log.Warning($"ModSDK already initialized for '{_modName}'. Skipping re-initialization for '{modName}'.");
                return;
            }

            _modName = modName ?? "Unknown";
            
            try
            {
                
                // Mark as initialized first
                _isInitialized = true;
                
                // GameAPI should auto-initialize, but we can verify it's ready
                // GameAPI.Initialize(); // This method doesn't exist
                
                // Initialize subsystems // Logging disabledEventSystem.Initialize();
                Systems.OverrideSystem.Initialize();
                
                _log.Info($"PerAspera ModSDK v{MyPluginInfo.PLUGIN_VERSION} initialized for mod '{modName}' v{modVersion}");
                _log.Info($"API Version: {MyPluginInfo.PLUGIN_VERSION}");
                _log.Info($"Override System: {Systems.OverrideSystem.GetStatistics()}");
            }
            catch (Exception ex)
            {
                // Reset if initialization fails
                _isInitialized = false;
                throw new ModSDKException($"Failed to initialize SDK for mod '{modName}': {ex.Message}", ex);
            }
        }




    }
}