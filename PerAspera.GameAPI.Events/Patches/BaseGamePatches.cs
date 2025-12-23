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
                object? nativeUniverse = null;

                // Try to get Universe if available
                try
                {
                    var universeProperty = typeof(BaseGame).GetProperty("universe");
                    if (universeProperty != null)
                    {
                        nativeUniverse = universeProperty.GetValue(__instance);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warning($"Could not get Universe from BaseGame: {ex.Message}");
                }

                // Create and dispatch the event
                var loadFinishedEvent = new OnLoadFinishedEvent(nativeBaseGame, nativeUniverse);
                EnhancedEventBus.Publish(SDKEventConstants.OnLoadFinished, loadFinishedEvent);

                _logger.Info("✅ OnLoadFinished event dispatched successfully");
            }
            catch (Exception ex)
            {
                _logger.Error($"❌ Failed to dispatch OnLoadFinished event: {ex.Message}");
                _logger.Error($"StackTrace: {ex.StackTrace}");
            }
        }
    }
}