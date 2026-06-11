using HarmonyLib;
using PerAspera.Core;
using PerAspera.GameAPI.Events.Constants;
using PerAspera.GameAPI.Events.Integration;
using PerAspera.GameAPI.Events.SDK;
using System;
using System.Collections.Generic;


#pragma warning disable CS1591
namespace PerAspera.GameAPI.Events.Patches
{
    [HarmonyPatch]
    public static class BaseGameUpdatePatches
    {
        private static readonly LogAspera _logger = new LogAspera("BaseGameUpdatePatches");
        private static bool _commandsReadyFired = false;
        private static bool _uiReadyFired = false;

        // Throttle "not ready yet" log to once every 60 frames to avoid spam
        private static int _pollFrameCount = 0;
        private const int LOG_EVERY_N_FRAMES = 60;

        internal static bool GameLoadComplete = false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BaseGame), "Update")]
        public static void Update_Postfix(BaseGame __instance)
        {
            if (!GameLoadComplete) return;

            try
            {
                // ── Poll 1: InteractionManager → GameCommandsReadyEvent ──────
                if (!_commandsReadyFired)
                {
                    var universe = __instance.universe;
                    if (universe == null) { LogThrottled("⏳ universe is null — retry next frame"); goto checkUI; }

                    var playerFaction = universe.playerFaction;
                    if (playerFaction == null) { LogThrottled("⏳ playerFaction is null — retry next frame"); goto checkUI; }

                    if (playerFaction.interactionManager == null) { LogThrottled("⏳ interactionManager is null — retry next frame"); goto checkUI; }

                    _commandsReadyFired = true;
                    var planet = universe.planet;
                    _logger.Info("🎯 InteractionManager ready — emitting GameCommandsReadyEvent");
                    var evt = new GameCommandsReadyEvent(__instance, universe, planet, playerFaction);
                    EnhancedEventBus.Publish(SDKEventConstants.GameCommandsReady, evt);
                    _logger.Info("✅ GameCommandsReadyEvent dispatched");
                }

                checkUI:
                // ── Poll 2: notificationPresenter → GameUIReadyEvent ─────────
                // canvasRefs is populated after GameFullyLoaded — poll independently each frame.
                if (!_uiReadyFired && __instance.canvasRefs?.notificationPresenter != null)
                {
                    _uiReadyFired = true;
                    _logger.Info("🖥️ canvasRefs.notificationPresenter ready — emitting GameUIReadyEvent");
                    var uiEvt = new GameUIReadyEvent(__instance, __instance.canvasRefs);
                    EnhancedEventBus.Publish(SDKEventConstants.GameUIReady, uiEvt);
                    _logger.Info("✅ GameUIReadyEvent dispatched");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"❌ Update_Postfix failed: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private static void LogThrottled(string message)
        {
            _pollFrameCount++;
            if (_pollFrameCount % LOG_EVERY_N_FRAMES == 1)
                _logger.Info(message);
        }

        internal static void ResetForNewSession()
        {
            _commandsReadyFired = false;
            _uiReadyFired = false;
            _pollFrameCount = 0;
            GameLoadComplete = false;
            _logger.Info("🔄 Session flags reset (GameCommandsReady + GameUIReady)");
        }
    }
}
#pragma warning restore CS1591
