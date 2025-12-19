using System;
using PerAspera.GameAPI.Events.Core;
using PerAspera.GameAPI.Events.Native;
using PerAspera.GameAPI.Events.Constants;
using PerAspera.GameAPI.Events.SDK;
using PerAspera.GameAPI.Wrappers;
using PerAspera.Core;
using HarmonyLib;

namespace PerAspera.GameAPI.Events.Patches
{
    /// <summary>
    /// SDK-based game initialization detection (NO MORE HARMONY PATCHES)
    /// Uses existing SDK wrapper system instead of problematic IL2CPP patches
    /// </summary>
    public static class GameInitializationPatches // REMOVED [HarmonyPatch] - no patches needed!
    {
        private static readonly LogAspera _logger = new LogAspera("GameInitPatches");
        private static bool _gameHubInitialized = false;
        private static bool _gameFullyLoaded = false;

        /// <summary>
        /// Initialize game events using SDK wrapper detection instead of IL2CPP patches
        /// Called from EventsAutoStartPlugin after SDK wrappers are ready
        /// </summary>
        public static void InitializeSDKBasedEvents()
        {
            try
            {
                _logger.Info("🔧 Initializing SDK-based game initialization detection...");
                
                // Use existing SDK wrapper system to detect game state
                var baseGameWrapper = TryGetBaseGameWrapper();
                if (baseGameWrapper != null)
                {
                    TriggerGameHubInitialized(baseGameWrapper);
                    _logger.Info("✅ Game initialization detected via SDK wrappers");
                }
                else
                {
                    // Fallback: schedule periodic check using SDK
                    ScheduleSDKBasedCheck();
                    _logger.Info("⏰ Scheduled SDK-based game state monitoring");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"❌ Failed to initialize SDK-based events: {ex.Message}");
            }
        }

        /// <summary>
        /// Try to get BaseGame using existing SDK wrapper system
        /// </summary>
        private static GameAPI.Wrappers.BaseGame? TryGetBaseGameWrapper()
        {
            try
            {
                // Get BaseGame using BaseGame.GetCurrent()
                var baseGameInstance = GameAPI.Wrappers.BaseGame.GetCurrent();
                if (baseGameInstance != null)
                {
                    // Optionally verify Keeper is initialized within BaseGame
                    var keeper = baseGameInstance.GetKeeper();
                    if (keeper != null)
                    {
                        return baseGameInstance;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Debug($"BaseGame not yet available via SDK: {ex.Message}");
            }
            
            return null;
        }

        /// <summary>
        /// Trigger GameHubInitialized event using SDK wrapper
        /// </summary>
        private static void TriggerGameHubInitialized(GameAPI.Wrappers.BaseGame baseGameWrapper)
        {
            if (_gameHubInitialized) return;

            try
            {
                _gameHubInitialized = true;
                _logger.Info("🎮 Game initialization detected via SDK wrapper");

                var evt = new GameHubInitializedEvent(baseGameWrapper, isReady: true);
                EnhancedEventBus.Publish(SDKEventConstants.GameHubInitialized, evt);
                
                _logger.Info("📡 GameHubInitializedEvent published successfully");
            }
            catch (Exception ex)
            {
                _logger.Error($"❌ Failed to trigger GameHubInitialized: {ex.Message}");
            }
        }

        /// <summary>
        /// Schedule periodic SDK-based check for game initialization
        /// </summary>
        private static void ScheduleSDKBasedCheck()
        {
            // Use Unity coroutine or timer-based approach
            // This is cleaner than Harmony patches to private methods
            _logger.Info("⏰ SDK-based periodic checking not yet implemented");
            _logger.Info("💡 Game events will be available when SDK detects game state");
        }

        // ==============================================
        // DEPRECATED HARMONY PATCHES (kept for reference)
        // ==============================================
        // These patches caused "method null" errors because they target
        // private methods or non-existent types. Replaced by SDK-based approach.

        // ==============================================
        // DEPRECATED HARMONY PATCHES (kept for reference)
        // ==============================================
        // These patches caused "method null" errors because they target
        // private methods or non-existent types. Replaced by SDK-based approach.

        #if HARMONY_PATCHES_DISABLED // Disabled due to IL2CPP compatibility issues

        /// <summary>
        /// DISABLED: Patch GameHubManager.Awake() to trigger GameHubInitializedEvent
        /// PROBLEM: GameHubManager type not found in IL2CPP
        /// SOLUTION: Use SDK-based detection instead
        /// </summary>
        [HarmonyPatch("GameHubManager", "Awake")] // DISABLED - Type not found
        [HarmonyPostfix]
        public static void OnGameHubManagerAwake_DISABLED() // DISABLED
        {
            try
            {
                if (_gameHubInitialized) return;
                _gameHubInitialized = true;

                _logger.Info("🎮 GameHubManager.Awake() detected - triggering EarlyModsReady event");

                // Try to get BaseGame instance early
                var baseGameInstance = TryGetBaseGameInstance();
                
                // Always trigger EarlyModsReady event (even if BaseGame not available yet)
                var earlyEvent = new EarlyModsReadyEvent(baseGameInstance);
                EnhancedEventBus.Publish(SDKEventConstants.EarlyModsReady, earlyEvent);
                _logger.Info($"✅ EarlyModsReadyEvent published - BaseGame available: {earlyEvent.BaseGameAvailable}");
                
                // Also trigger GameHubInitialized for compatibility
                if (baseGameInstance != null)
                {
                    var evt = new GameHubInitializedEvent(baseGameInstance, isReady: true);
                    EnhancedEventBus.Publish(SDKEventConstants.GameHubInitialized, evt);
                    _logger.Info("✅ GameHubInitializedEvent published successfully");
                }
                else
                {
                    _logger.Warning("⚠️ BaseGame instance not available during GameHub initialization");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"❌ Error in GameHubManager.Awake patch: {ex.Message}");
            }
        }

        /// <summary>
        /// Target method resolution for BaseGame.OnFinishLoading patch
        /// Resolves the real IL2CPP BaseGame type dynamically
        /// </summary>
        [HarmonyTargetMethod]
        static System.Reflection.MethodBase TargetMethodBaseGame()
        {
            try
            {
                var baseGameType = AccessTools.TypeByName("BaseGame");
                if (baseGameType == null)
                {
                    _logger.Error("❌ BaseGame type not found in IL2CPP assemblies");
                    return null;
                }
                
                var onFinishMethod = AccessTools.Method(baseGameType, "OnFinishLoading");
                if (onFinishMethod == null)
                {
                    _logger.Warning("⚠️ BaseGame.OnFinishLoading() method not found, disabling patch");
                    return null;
                }
                
                _logger.Info($"✅ Successfully resolved BaseGame.OnFinishLoading() method");
                return onFinishMethod;
            }
            catch (Exception ex)
            {
                _logger.Error($"❌ Failed to resolve BaseGame.OnFinishLoading(): {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Patch BaseGame.OnFinishLoading() - public method for game initialization
        /// Triggers when the game finishes loading and is ready
        /// </summary>
        [HarmonyPatch]
        [HarmonyPostfix]
        public static void OnBaseGameAwake(object __instance)
        {
            try
            {
                _logger.Info("🎮 BaseGame.OnFinishLoading() detected - game is ready!");
                
                // If GameHub wasn't initialized yet but BaseGame is ready, trigger now
                if (!_gameHubInitialized)
                {
                    var baseGameInstance = TryGetBaseGameInstance();
                    if (baseGameInstance != null)
                    {
                        _gameHubInitialized = true;
                        
                        var evt = new GameHubInitializedEvent(baseGameInstance, isReady: true);
                        EnhancedEventBus.Publish(SDKEventConstants.GameHubInitialized, evt);
                        
                        _logger.Info("✅ GameHubInitializedEvent published from BaseGame.Awake");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"❌ Error in BaseGame.Awake patch: {ex.Message}");
            }
        }

        /// <summary>
        /// PATCH TEMPORAIREMENT DÉSACTIVÉ - Planet.Awake() method not found in IL2CPP
        /// Error: "Could not find method for type Planet and name Awake"
        /// TODO: Find correct Planet initialization method or use alternative event hook
        /// </summary>
        /*
        [HarmonyPatch("Planet", "Awake")]
        [HarmonyPostfix]
        public static void OnPlanetAwake()
        */
        public static void OnPlanetAwake_DISABLED() // Désactivé temporairement
        {
            try
            {
                if (_gameFullyLoaded) return;

                _logger.Info("🎮 Planet.Awake() detected - checking for complete system");

                // Try to get all main game instances
                var baseGame = TryGetBaseGameInstance();
                var universe = TryGetUniverseInstance();
                var planet = TryGetPlanetInstance();

                if (baseGame != null && universe != null && planet != null)
                {
                    _gameFullyLoaded = true;
                    
                    var evt = new GameFullyLoadedEvent(baseGame, universe, planet);
                    EnhancedEventBus.Publish(SDKEventConstants.GameFullyLoaded, evt);
                    
                    _logger.Info("✅ GameFullyLoadedEvent published successfully");
                }
                else
                {
                    _logger.Info($"⏳ System not fully loaded yet - BaseGame:{baseGame != null}, Universe:{universe != null}, Planet:{planet != null}");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"❌ Error in Planet.Awake patch: {ex.Message}");
            }
        }

        /// <summary>
        /// Try to get BaseGame instance using wrapper
        /// </summary>
        private static BaseGame? TryGetBaseGameInstance()
        {
            try
            {
                // Try different methods to get BaseGame instance
                
                // Method 1: BaseGame.Current (if available)
                var baseGameType = System.Type.GetType("BaseGame");
                if (baseGameType != null)
                {
                    var currentProperty = baseGameType.GetProperty("Current");
                    if (currentProperty != null)
                    {
                        var nativeInstance = currentProperty.GetValue(null);
                        if (nativeInstance != null)
                        {
                            return WrapperFactory.ConvertToWrapper<BaseGame>(nativeInstance);
                        }
                    }
                }

                // Method 2: Try finding BaseGame instance in scene
                var baseGameInstances = UnityEngine.Object.FindObjectsOfType<UnityEngine.MonoBehaviour>();
                foreach (var instance in baseGameInstances)
                {
                    if (instance.GetType().Name == "BaseGame")
                    {
                        return WrapperFactory.ConvertToWrapper<BaseGame>(instance);
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to get BaseGame instance: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Try to get Universe instance using wrapper
        /// </summary>
        private static Universe? TryGetUniverseInstance()
        {
            try
            {
                // Similar pattern to BaseGame
                var universeType = System.Type.GetType("Universe");
                if (universeType != null)
                {
                    var currentProperty = universeType.GetProperty("Current");
                    if (currentProperty != null)
                    {
                        var nativeInstance = currentProperty.GetValue(null);
                        if (nativeInstance != null)
                        {
                            return WrapperFactory.ConvertToWrapper<Universe>(nativeInstance);
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to get Universe instance: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Try to get Planet instance using wrapper
        /// </summary>
        private static Planet? TryGetPlanetInstance()
        {
            try
            {
                // Similar pattern to BaseGame/Universe
                var planetType = System.Type.GetType("Planet");
                if (planetType != null)
                {
                    var currentProperty = planetType.GetProperty("Current");
                    if (currentProperty != null)
                    {
                        var nativeInstance = currentProperty.GetValue(null);
                        if (nativeInstance != null)
                        {
                            return WrapperFactory.ConvertToWrapper<Planet>(nativeInstance);
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.Warning($"Failed to get Planet instance: {ex.Message}");
                return null;
            }
        }

        // ==================== BLACKBOARD INITIALIZATION PATCHES ====================

        /// <summary>
        /// Patch Universe.AddBlackboard() to trigger BlackboardInitializedEvent
        /// Method: AddBlackboard(Blackboard blackboard)
        /// </summary>
        [HarmonyPatch("Universe", "AddBlackboard")]
        [HarmonyPostfix]
        public static void OnUniverseAddBlackboard(object blackboard)
        {
            try
            {
                if (blackboard == null) return;

                _logger.Info("🗂️ Universe.AddBlackboard() detected - triggering BlackboardInitializedEvent");

                // Create enhanced event with BlackBoard wrapper
                var evt = new BlackboardInitializedEvent(blackboard);
                
                // Publish through Enhanced Event Bus
                EnhancedEventBus.Publish(SDKEventConstants.BlackboardInitialized, evt);
                
                var blackboardName = evt.BlackBoard?.Name ?? "Unknown";
                _logger.Info($"✅ BlackboardInitializedEvent published for '{blackboardName}'");
            }
            catch (Exception ex)
            {
                _logger.Error($"❌ Error in Universe.AddBlackboard patch: {ex.Message}");
            }
        }

        /// <summary>
        /// Patch Blackboard constructor to catch individual blackboard creation
        /// Constructor: Blackboard(string name, object parent)
        /// </summary>
        [HarmonyPatch("Blackboard", MethodType.Constructor)]
        [HarmonyPatch(new System.Type[] { typeof(string), typeof(object) })]
        [HarmonyPostfix]
        public static void OnBlackboardConstructor(object __instance, string name)
        {
            try
            {
                if (__instance == null || string.IsNullOrEmpty(name)) return;

                _logger.Info($"🗂️ Blackboard constructor detected for '{name}'");

                // Only trigger event for main blackboards to avoid spam
                if (name.Contains("main") || name.Contains("universe") || name.Contains("global"))
                {
                    var evt = new BlackboardInitializedEvent(__instance);
                    EnhancedEventBus.Publish(SDKEventConstants.BlackboardInitialized, evt);
                    
                    _logger.Info($"✅ BlackboardInitializedEvent published for main blackboard '{name}'");
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"❌ Error in Blackboard constructor patch: {ex.Message}");
            }
        #endif // HARMONY_PATCHES_DISABLED

    }
}