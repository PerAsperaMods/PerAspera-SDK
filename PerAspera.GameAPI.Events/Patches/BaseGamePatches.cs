using HarmonyLib;
using PerAspera.Core;
using PerAspera.GameAPI.Events.Constants;
using PerAspera.GameAPI.Events.Integration;
using PerAspera.GameAPI.Events.SDK;
using System;

namespace PerAspera.GameAPI.Events.Patches
{
    /// <summary>
    /// Harmony patches for BaseGame to trigger custom SDK events
    /// </summary>
    [HarmonyPatch]
    public static class BaseGamePatches
    {
        private static readonly LogAspera _logger = new LogAspera("BaseGamePatches");

        /// <summary>
        /// Patch for BaseGame.OnFinishLoading() to trigger OnLoadFinished event
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(BaseGame), "OnFinishLoading")]
        public static void OnFinishLoading_Postfix(BaseGame __instance)
        {
            try
            {
                _logger.Info("🎯 BaseGame.OnFinishLoading() completed, triggering OnLoadFinished event");

                // Get native instances for event creation
                var nativeBaseGame = __instance;
                object? nativeUniverse = ((BaseGame)nativeBaseGame).GetUniverse();
                object? nativePlanet = ((Universe)nativeUniverse).GetPlanet();


                // Create and dispatch the OnLoadFinished event
                var loadFinishedEvent = new OnLoadFinishedEvent(nativeBaseGame, nativeUniverse);
                EnhancedEventBus.Publish(SDKEventConstants.OnLoadFinished, loadFinishedEvent);
                _logger.Info("✅ OnLoadFinished event dispatched successfully");

                // EMIT EARLY MODS READY EVENT - This is what MasterGUI is waiting for!
                _logger.Info("🎯 Emitting EarlyModsReadyEvent for early mod initialization");
                var earlyModsReadyEvent = new EarlyModsReadyEvent(nativeBaseGame);
                EnhancedEventBus.Publish(SDKEventConstants.EarlyModsReady, earlyModsReadyEvent);
                _logger.Info("✅ EarlyModsReadyEvent dispatched successfully");

                // EMIT GAME FULLY LOADED EVENT if we have all components
                if (nativeUniverse != null && nativePlanet != null)
                {
                    _logger.Info("🎯 All game components available - emitting GameFullyLoadedEvent");
                    var gameFullyLoadedEvent = new GameFullyLoadedEvent(nativeBaseGame, nativeUniverse, nativePlanet);
                    EnhancedEventBus.Publish(SDKEventConstants.GameFullyLoaded, gameFullyLoadedEvent);
                    _logger.Info("✅ GameFullyLoadedEvent dispatched successfully");
                }
                else
                {
                    _logger.Info($"⏳ Game not fully loaded yet - Universe: {nativeUniverse != null}, Planet: {nativePlanet != null}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"❌ Failed to dispatch events: {ex.Message}");
                _logger.Error($"StackTrace: {ex.StackTrace}");
            }
        }
    }
}