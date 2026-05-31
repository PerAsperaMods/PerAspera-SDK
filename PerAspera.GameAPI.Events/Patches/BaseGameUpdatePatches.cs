using HarmonyLib;
using PerAspera.Core;
using PerAspera.GameAPI.Events.Constants;
using PerAspera.GameAPI.Events.Integration;
using PerAspera.GameAPI.Events.SDK;
using System;

namespace PerAspera.GameAPI.Events.Patches
{
    [HarmonyPatch]
    public static class BaseGameUpdatePatches
    {
        private static readonly LogAspera _logger = new LogAspera("BaseGameUpdatePatches");
        private static bool _commandsReadyFired = false;

        // Throttle "not ready yet" log to once every 60 frames to avoid spam
        private static int _pollFrameCount = 0;
        private const int LOG_EVERY_N_FRAMES = 60;

        internal static bool GameLoadComplete = false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BaseGame), "Update")]
        public static void Update_Postfix(BaseGame __instance)
        {
            if (_commandsReadyFired || !GameLoadComplete)
                return;

            try
            {
                // ── Direct interop access — zero reflection ─────────────────
                // Path: BaseGame → universe → playerFaction → interactionManager
                // All types are from PerAspera.GameLibs.Complete (interop DLLs).
                var universe = __instance.universe;
                if (universe == null)
                {
                    LogThrottled("⏳ universe is null — retry next frame");
                    return;
                }

                var playerFaction = universe.playerFaction;
                if (playerFaction == null)
                {
                    LogThrottled("⏳ playerFaction is null — retry next frame");
                    return;
                }

                // interactionManager is a plain public field on Faction (not a singleton)
                if (playerFaction.interactionManager == null)
                {
                    LogThrottled("⏳ interactionManager is null — retry next frame");
                    return;
                }

                // All systems go — fire once
                _commandsReadyFired = true;

                // universe.planet is public property — available after WakeUp (SKILL: universe.planet ✅)
                // Defensive null check in case of edge case (early load, save reload, etc.)
                var planet = universe.planet;

                _logger.Info("🎯 InteractionManager ready — emitting GameCommandsReadyEvent");
                _logger.Info($"   BaseGame={__instance.GetType().Name}, Universe={universe.GetType().Name}, " +
                             $"Planet={(planet != null ? planet.GetType().Name : "null (edge case)")}, " +
                             $"Faction={playerFaction.GetType().Name}");

                var evt = new GameCommandsReadyEvent(__instance, universe, planet, playerFaction);
                EnhancedEventBus.Publish(SDKEventConstants.GameCommandsReady, evt);
                _logger.Info("✅ GameCommandsReadyEvent dispatched — game commands are now executable");
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
            _pollFrameCount = 0;
            GameLoadComplete = false;
            _logger.Info("🔄 GameCommandsReady flag reset for new game session");
        }
    }
}
