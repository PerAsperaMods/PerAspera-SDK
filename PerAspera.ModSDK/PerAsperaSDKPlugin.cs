using BepInEx;
using PerAspera.ModSDK;
using PerAspera.GameAPI;
using PerAspera.GameAPI.Events;
using BepInEx.Unity.IL2CPP;
using PerAspera.Core;

namespace PerAspera.ModSDK
{
    /// <summary>
    /// Plugin principal PerAspera SDK - Fournit l'infrastructure de modding complète
    /// Uses new event architecture (PerAspera.GameAPI.Events v2)
    /// </summary>
    [BepInPlugin("peraaspera.modsdk", "PerAspera ModSDK", "1.0.0")]
    [BepInProcess("Per Aspera.exe")]
    public class PerAsperaSDKPlugin : BasePlugin
    {
        // ✅ CORRECTION: Utiliser LogAspera au lieu du Logger BepInX
        private static readonly LogAspera _logger = new LogAspera("SDK.Plugin");

        public override void Load()
        {
            try
            {
                _logger.Info("🚀 Initializing PerAspera ModSDK...");
                
                // ✅ 1. Initialize ModSDK core
                ModSDK.Initialize("PerAspera.ModSDK", "1.0.0");
                
                // ✅ 2. Initialize native event system
                InitializeNativeEventSystem();
                
                _logger.Info("✅ PerAspera ModSDK ready - mods can now subscribe to events!");
            }
            catch (System.Exception ex)
            {
                _logger.Fatal($"❌ CRITICAL: Failed to initialize PerAspera ModSDK: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Initialise le système d'interception des événements natifs du jeu
        /// Uses new PerAspera.GameAPI.Events architecture v2
        /// </summary>
        private void InitializeNativeEventSystem()
        {
            try
            {
                _logger.Info("🎮 Initializing native game event system (v2)...");
                
                // NOTE: New event system (PerAspera.GameAPI.Events) is automatically initialized
                // via its own native event patches. No manual initialization needed.
                // Game instances accessed via Planet.GetCurrent(), Universe.GetCurrent() from Wrappers
                
                // Forward native events to ModSDK EventSystem
                ModEventBus.OnEventPublish += (eventName, eventData) =>
                {
                    try
                    {
                        Systems.EventSystem.Publish(eventName, eventData);
                    }
                    catch (System.Exception ex)
                    {
                        _logger.Warning($"⚠️ Failed to publish event {eventName}: {ex.Message}");
                    }
                };
                
                _logger.Info("✅ Native event system ready - events are being captured and forwarded");
            }
            catch (System.Exception ex)
            {
                _logger.Error($"Failed to initialize native event system: {ex.Message}");
                throw;
            }
        }




        private void OnDestroy()
        {
            try
            {
                _logger.Info("🔄 Shutting down PerAspera ModSDK...");
                
                _logger.Info("✅ PerAspera ModSDK shut down cleanly");
            }
            catch (System.Exception ex)
            {
                _logger.Error($"Error during SDK shutdown: {ex.Message}");
            }
        }


    }
}