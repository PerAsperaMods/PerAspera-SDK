using HarmonyLib;
using BepInEx.Logging;
using PerAspera.GameAPI.Events.Constants;
using PerAspera.GameAPI.Events.Data;
using PerAspera.GameAPI.Events.Integration;
using PerAspera.GameAPI.Events.SDK;

namespace PerAspera.GameAPI.Events.Patches
{
    /// <summary>
    /// Harmony patches for GameHubManager methods to detect game initialization
    /// Much more reliable than Unity SceneManager events in IL2CPP environment
    /// </summary>
    public static class GameHubManagerPatch
    {
        private static ManualLogSource Log => BepInEx.Logging.Logger.CreateLogSource("GameHubPatch");
        private static bool _eventEmitted = false; // Ensure we only emit once

        /// <summary>
        /// Postfix patch on GameHubManager.StartPing() - called when GameHub is fully ready
        /// </summary>
        [HarmonyPatch(typeof(GameHubManager), "StartPing")]
        [HarmonyPostfix]
        public static void OnStartPing()
        {
            if (_eventEmitted)
                return;

            EmitGameHubInitializedEvent("StartPing");
        }
        
        /// <summary>
        /// Postfix patch on Hub_InitializationComplete - alternative detection point
        /// Called earlier in the initialization process
        /// </summary>
        [HarmonyPatch(typeof(GameHubManager), "Hub_InitializationComplete")]
        [HarmonyPostfix]
        public static void OnHubInitializationComplete()
        {
            if (_eventEmitted)
                return;

            EmitGameHubInitializedEvent("Hub_InitializationComplete");
        }
        
        /// <summary>
        /// Common method to emit GameHubInitializedEvent
        /// </summary>
        private static void EmitGameHubInitializedEvent(string triggerMethod)
        {
            try
            {
                Log.LogInfo($"🎯 GameHubManager.{triggerMethod}() called - GameHub is ready!");
                
                // GameHub is ready - emit GameHubReady event immediately
                // This is the EARLIEST event, doesn't need BaseGame/Universe/Planet
                Log.LogInfo("🎮 GameHub ready - emitting GameHubReady event");
                var gameHubReadyEvent = new GameHubReadyEvent(
                    sceneLoaded: true,
                    managerReady: true
                );
                EnhancedEventBus.Publish(SDKEventConstants.GameHubReady, gameHubReadyEvent);
                Log.LogInfo($"✅ GameHubReady event emitted via {triggerMethod}");
                
                // Do NOT emit other events here - they have their own detection systems
                // BaseGameDetected, EarlyModsReady, GameFullyLoaded will be emitted by other systems
                
                _eventEmitted = true;
            }
            catch (System.Exception ex)
            {
                Log.LogError($"❌ Error in GameHubManager.{triggerMethod} patch: {ex.Message}");
            }
        }
    }
}