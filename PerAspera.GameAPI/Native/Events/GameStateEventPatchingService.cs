using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Native.Events
{
    /// <summary>
    /// Game state event patching service for Per Aspera
    /// Handles all game state events including save/load, scene transitions, game mode changes, and UI state
    /// </summary>
    public sealed class GameStateEventPatchingService : BaseEventPatchingService
    {
        private System.Type _baseGameType;
        private System.Type _saveManagerType;
        private System.Type _sceneManagerType;
        private System.Type _uiManagerType;

        public GameStateEventPatchingService(Harmony harmony) 
            : base("GameState", harmony)
        {
        }

        public override string GetEventType() => "GameState";

        public override int InitializeEventHooks()
        {
            _log.Debug("🎮 Setting up enhanced game state event hooks...");

            _baseGameType = GameTypeInitializer.GetBaseGameType();
            _saveManagerType = GameTypeInitializer.GetSaveManagerType();
            _sceneManagerType = GameTypeInitializer.GetSceneManagerType();
            _uiManagerType = GameTypeInitializer.GetUIManagerType();

            if (_baseGameType == null)
            {
                _log.Warning("BaseGame type not found, skipping game state hooks");
                return 0;
            }

            // Enhanced game state methods with comprehensive coverage
            var gameStateHooks = new Dictionary<string, (System.Type type, string eventType)>();

            // Base game state hooks
            AddBaseGameHooks(gameStateHooks);

            // Save/Load hooks
            if (_saveManagerType != null)
            {
                AddSaveManagerHooks(gameStateHooks);
            }

            // Scene management hooks
            if (_sceneManagerType != null)
            {
                AddSceneManagerHooks(gameStateHooks);
            }

            // UI state hooks
            if (_uiManagerType != null)
            {
                AddUIManagerHooks(gameStateHooks);
            }

            int hookedCount = 0;
            foreach (var (methodName, (type, eventType)) in gameStateHooks)
            {
                if (CreateGameStateMethodHook(type, methodName, eventType))
                {
                    hookedCount++;
                }
            }

            _log.Info($"✅ Game state hooks initialized: {hookedCount}/{gameStateHooks.Count} methods hooked");
            return hookedCount;
        }

        /// <summary>
        /// Add BaseGame-specific hooks
        /// </summary>
        /// <param name="hooks">Hook dictionary to populate</param>
        private void AddBaseGameHooks(Dictionary<string, (System.Type type, string eventType)> hooks)
        {
            var baseGameHooks = new Dictionary<string, string>
            {
                { "StartNewGame", "GameStart" },
                { "LoadGame", "GameLoad" },
                { "SaveGame", "GameSave" },
                { "ExitGame", "GameExit" },
                { "QuitGame", "GameExit" },
                { "RestartGame", "GameRestart" },
                { "Initialize", "GameInitialize" },
                { "OnFinishLoading", "GameLoadComplete" },
                { "SetGameMode", "GameModeChange" },
                { "ChangeGameMode", "GameModeChange" },
                { "EnterMainMenu", "MainMenuEnter" },
                { "ExitMainMenu", "MainMenuExit" },
                { "ShowPauseMenu", "PauseMenuShow" },
                { "HidePauseMenu", "PauseMenuHide" }
            };

            foreach (var (method, eventType) in baseGameHooks)
            {
                hooks[method] = (_baseGameType, eventType);
            }
        }

        /// <summary>
        /// Add SaveManager-specific hooks
        /// </summary>
        /// <param name="hooks">Hook dictionary to populate</param>
        private void AddSaveManagerHooks(Dictionary<string, (System.Type type, string eventType)> hooks)
        {
            var saveHooks = new Dictionary<string, string>
            {
                { "Save", "SaveGame" },
                { "Load", "LoadGame" },
                { "QuickSave", "QuickSave" },
                { "QuickLoad", "QuickLoad" },
                { "AutoSave", "AutoSave" },
                { "DeleteSave", "SaveDelete" },
                { "ValidateSave", "SaveValidate" },
                { "BackupSave", "SaveBackup" },
                { "RestoreSave", "SaveRestore" }
            };

            foreach (var (method, eventType) in saveHooks)
            {
                hooks[method] = (_saveManagerType, eventType);
            }
        }

        /// <summary>
        /// Add SceneManager-specific hooks
        /// </summary>
        /// <param name="hooks">Hook dictionary to populate</param>
        private void AddSceneManagerHooks(Dictionary<string, (System.Type type, string eventType)> hooks)
        {
            var sceneHooks = new Dictionary<string, string>
            {
                { "LoadScene", "SceneLoad" },
                { "UnloadScene", "SceneUnload" },
                { "ChangeScene", "SceneChange" },
                { "SwitchScene", "SceneSwitch" },
                { "TransitionToScene", "SceneTransition" },
                { "OnSceneLoaded", "SceneLoadComplete" },
                { "OnSceneUnloaded", "SceneUnloadComplete" }
            };

            foreach (var (method, eventType) in sceneHooks)
            {
                hooks[method] = (_sceneManagerType, eventType);
            }
        }

        /// <summary>
        /// Add UIManager-specific hooks
        /// </summary>
        /// <param name="hooks">Hook dictionary to populate</param>
        private void AddUIManagerHooks(Dictionary<string, (System.Type type, string eventType)> hooks)
        {
            var uiHooks = new Dictionary<string, string>
            {
                { "ShowUI", "UIShow" },
                { "HideUI", "UIHide" },
                { "ToggleUI", "UIToggle" },
                { "UpdateUI", "UIUpdate" },
                { "RefreshUI", "UIRefresh" },
                { "OpenMenu", "MenuOpen" },
                { "CloseMenu", "MenuClose" },
                { "ShowDialog", "DialogShow" },
                { "HideDialog", "DialogHide" },
                { "ShowNotification", "NotificationShow" },
                { "HideNotification", "NotificationHide" }
            };

            foreach (var (method, eventType) in uiHooks)
            {
                hooks[method] = (_uiManagerType, eventType);
            }
        }

        /// <summary>
        /// Create a game state-specific method hook with prefix and postfix handling
        /// </summary>
        /// <param name="targetType">Type containing the method</param>
        /// <param name="methodName">Method name to hook</param>
        /// <param name="eventType">Type of game state event</param>
        /// <returns>True if hook was successfully created</returns>
        private bool CreateGameStateMethodHook(System.Type targetType, string methodName, string eventType)
        {
            if (!ValidateMethodForPatching(targetType, methodName))
            {
                return false;
            }

            try
            {
                var method = targetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                
                // Create harmony patches with event type context
                var prefix = new HarmonyMethod(typeof(GameStateEventPatchingService), nameof(GameStatePrefix));
                var postfix = new HarmonyMethod(typeof(GameStateEventPatchingService), nameof(GameStatePostfix));

                _harmony.Patch(method, prefix: prefix, postfix: postfix);

                var patchKey = $"{targetType.Name}.{methodName}";
                _patchedMethods[patchKey] = eventType;
                
                _log.Debug($"✓ Hooked {patchKey} for {eventType} events");
                return true;
            }
            catch (Exception ex)
            {
                _log.Warning($"Failed to hook {methodName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Enhanced Harmony prefix for game state methods
        /// Captures the old game state before method execution
        /// </summary>
        [HarmonyPrefix]
        public static void GameStatePrefix(object __instance, Dictionary<string, object> __state, MethodBase __originalMethod)
        {
            try
            {
                if (__state == null)
                    __state = new Dictionary<string, object>();

                var methodName = __originalMethod.Name;
                var stateType = ExtractStateTypeFromMethodName(methodName);
                
                // Capture current game state before change
                var currentGameState = GetCurrentGameState(__instance, methodName);
                __state["OldState"] = currentGameState;
                __state["StateType"] = stateType;
                __state["MethodName"] = methodName;
                __state["Instance"] = __instance;
                __state["Timestamp"] = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                // Fail silently to avoid disrupting game flow
            }
        }

        /// <summary>
        /// Enhanced Harmony postfix for game state methods
        /// Publishes game state change events with before/after states
        /// </summary>
        [HarmonyPostfix]
        public static void GameStatePostfix(object __instance, Dictionary<string, object> __state, 
            MethodBase __originalMethod, object[] __args)
        {
            try
            {
                if (__state == null || !__state.ContainsKey("OldState"))
                    return;

                var stateType = (string)__state["StateType"];
                var methodName = (string)__state["MethodName"];
                var oldState = __state["OldState"];
                var timestamp = (DateTime)__state["Timestamp"];

                // Capture new game state after change
                var newState = GetCurrentGameState(__instance, methodName);

                var eventData = new
                {
                    Instance = __instance,
                    StateType = stateType,
                    OldState = oldState,
                    NewState = newState,
                    MethodName = methodName,
                    Arguments = ExtractMethodArguments(__args),
                    Timestamp = timestamp,
                    Duration = DateTime.UtcNow - timestamp,
                    InstanceType = __instance.GetType().Name
                };

                // Publish specific game state event
                ModEventBus.Publish($"GameState{stateType}", eventData);
                
                // Publish generic game state event
                ModEventBus.Publish("GameStateChanged", eventData);

                // Special handling for critical game state events
                PublishSpecialGameStateEvents(stateType, eventData);
            }
            catch (Exception ex)
            {
                // Fail silently to avoid disrupting game flow
            }
        }

        /// <summary>
        /// Get current game state with comprehensive information
        /// </summary>
        /// <param name="instance">Game instance</param>
        /// <param name="methodName">Method being called</param>
        /// <returns>Current game state object</returns>
        private static object GetCurrentGameState(object instance, string methodName)
        {
            try
            {
                var gameState = new Dictionary<string, object>();

                // Try to extract basic game state
                var basicState = ExtractBasicGameState(instance);
                if (basicState != null)
                {
                    gameState["Basic"] = basicState;
                }

                // Try to extract save-related state
                if (methodName.Contains("Save") || methodName.Contains("Load"))
                {
                    var saveState = ExtractSaveState(instance);
                    if (saveState != null)
                    {
                        gameState["Save"] = saveState;
                    }
                }

                // Try to extract UI state
                if (methodName.Contains("UI") || methodName.Contains("Menu") || methodName.Contains("Dialog"))
                {
                    var uiState = ExtractUIState(instance);
                    if (uiState != null)
                    {
                        gameState["UI"] = uiState;
                    }
                }

                // Try to extract scene state
                if (methodName.Contains("Scene"))
                {
                    var sceneState = ExtractSceneState(instance);
                    if (sceneState != null)
                    {
                        gameState["Scene"] = sceneState;
                    }
                }

                return gameState.Count > 0 ? gameState : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Extract basic game state information
        /// </summary>
        /// <param name="instance">Game instance</param>
        /// <returns>Basic game state or null</returns>
        private static object ExtractBasicGameState(object instance)
        {
            try
            {
                var basicState = new Dictionary<string, object>();

                var basicProperties = new[]
                {
                    "isPaused", "IsPaused", "paused", "Paused",
                    "isLoading", "IsLoading", "loading", "Loading",
                    "gameMode", "GameMode", "mode", "Mode",
                    "currentScene", "CurrentScene", "scene", "Scene",
                    "isInitialized", "IsInitialized", "initialized", "Initialized"
                };
                
                foreach (var propName in basicProperties)
                {
                    var value = GetPropertyValue(instance, propName);
                    if (value != null)
                    {
                        basicState[propName] = value;
                    }
                }

                return basicState.Count > 0 ? basicState : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Extract save-related state information
        /// </summary>
        /// <param name="instance">Game instance</param>
        /// <returns>Save state or null</returns>
        private static object ExtractSaveState(object instance)
        {
            try
            {
                var saveState = new Dictionary<string, object>();

                var saveProperties = new[]
                {
                    "currentSave", "CurrentSave", "saveName", "SaveName",
                    "saveVersion", "SaveVersion", "saveDate", "SaveDate",
                    "isSaving", "IsSaving", "isLoading", "IsLoading",
                    "lastSave", "LastSave", "autoSave", "AutoSave"
                };
                
                foreach (var propName in saveProperties)
                {
                    var value = GetPropertyValue(instance, propName);
                    if (value != null)
                    {
                        saveState[propName] = value;
                    }
                }

                return saveState.Count > 0 ? saveState : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Extract UI state information
        /// </summary>
        /// <param name="instance">Game instance</param>
        /// <returns>UI state or null</returns>
        private static object ExtractUIState(object instance)
        {
            try
            {
                var uiState = new Dictionary<string, object>();

                var uiProperties = new[]
                {
                    "activeMenu", "ActiveMenu", "currentMenu", "CurrentMenu",
                    "uiVisible", "UIVisible", "isUIVisible", "IsUIVisible",
                    "activeDialog", "ActiveDialog", "currentDialog", "CurrentDialog",
                    "menuStack", "MenuStack", "dialogStack", "DialogStack"
                };
                
                foreach (var propName in uiProperties)
                {
                    var value = GetPropertyValue(instance, propName);
                    if (value != null)
                    {
                        uiState[propName] = value;
                    }
                }

                return uiState.Count > 0 ? uiState : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Extract scene state information
        /// </summary>
        /// <param name="instance">Game instance</param>
        /// <returns>Scene state or null</returns>
        private static object ExtractSceneState(object instance)
        {
            try
            {
                var sceneState = new Dictionary<string, object>();

                var sceneProperties = new[]
                {
                    "currentScene", "CurrentScene", "scene", "Scene",
                    "targetScene", "TargetScene", "nextScene", "NextScene",
                    "sceneLoading", "SceneLoading", "isTransitioning", "IsTransitioning",
                    "sceneName", "SceneName", "sceneIndex", "SceneIndex"
                };
                
                foreach (var propName in sceneProperties)
                {
                    var value = GetPropertyValue(instance, propName);
                    if (value != null)
                    {
                        sceneState[propName] = value;
                    }
                }

                return sceneState.Count > 0 ? sceneState : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Helper method to get property value safely
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>Property value or null</returns>
        private static object GetPropertyValue(object instance, string propertyName)
        {
            try
            {
                var instanceType = instance.GetType();

                var property = instanceType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property != null && property.CanRead)
                {
                    return property.GetValue(instance);
                }

                var field = instanceType.GetField(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    return field.GetValue(instance);
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Extract state type from method name
        /// </summary>
        /// <param name="methodName">Method name</param>
        /// <returns>State type identifier</returns>
        private static string ExtractStateTypeFromMethodName(string methodName)
        {
            return methodName switch
            {
                var name when name.Contains("Save") => "Save",
                var name when name.Contains("Load") => "Load",
                var name when name.Contains("Scene") => "Scene",
                var name when name.Contains("UI") => "UI",
                var name when name.Contains("Menu") => "Menu",
                var name when name.Contains("Dialog") => "Dialog",
                var name when name.Contains("Notification") => "Notification",
                var name when name.Contains("Game") => "Game",
                var name when name.Contains("Initialize") => "Initialize",
                var name when name.Contains("Exit") || name.Contains("Quit") => "Exit",
                var name when name.Contains("Start") => "Start",
                var name when name.Contains("Restart") => "Restart",
                var name when name.Contains("Pause") => "Pause",
                var name when name.Contains("Mode") => "Mode",
                _ => "Generic"
            };
        }

        /// <summary>
        /// Extract meaningful arguments from method call
        /// </summary>
        /// <param name="args">Method arguments</param>
        /// <returns>Processed arguments object</returns>
        private static object ExtractMethodArguments(object[] args)
        {
            if (args == null || args.Length == 0)
                return null;

            if (args.Length == 1)
                return args[0];

            var argDict = new Dictionary<string, object>();
            for (int i = 0; i < args.Length; i++)
            {
                argDict[$"Arg{i}"] = args[i];
            }

            return argDict;
        }

        /// <summary>
        /// Publish special game state events for critical game state changes
        /// </summary>
        /// <param name="stateType">Type of game state event</param>
        /// <param name="eventData">Event data</param>
        private static void PublishSpecialGameStateEvents(string stateType, object eventData)
        {
            try
            {
                switch (stateType)
                {
                    case "Save":
                        ModEventBus.Publish("GameSaved", eventData);
                        break;

                    case "Load":
                        ModEventBus.Publish("GameLoaded", eventData);
                        break;

                    case "Scene":
                        ModEventBus.Publish("SceneChanged", eventData);
                        break;

                    case "Game":
                        ModEventBus.Publish("GameStateTransition", eventData);
                        break;

                    case "Initialize":
                        ModEventBus.Publish("GameInitialized", eventData);
                        break;

                    case "Exit":
                        ModEventBus.Publish("GameExited", eventData);
                        break;
                }
            }
            catch (Exception)
            {
                // Fail silently
            }
        }

        /// <summary>
        /// Get diagnostic information about game state event hooks
        /// </summary>
        /// <returns>Diagnostic information string</returns>
        public string GetDiagnosticInfo()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine("=== Game State Event Patching Service ===");
            info.AppendLine($"BaseGame Type: {GetFriendlyTypeName(_baseGameType)}");
            info.AppendLine($"SaveManager Type: {GetFriendlyTypeName(_saveManagerType)}");
            info.AppendLine($"SceneManager Type: {GetFriendlyTypeName(_sceneManagerType)}");
            info.AppendLine($"UIManager Type: {GetFriendlyTypeName(_uiManagerType)}");
            info.AppendLine($"Hooked Methods: {_patchedMethods.Count}");
            info.AppendLine();

            var categoryGroups = new Dictionary<string, List<string>>();
            foreach (var patch in _patchedMethods)
            {
                var category = patch.Value.Contains("Save") || patch.Value.Contains("Load") ? "Save/Load" :
                              patch.Value.Contains("Scene") ? "Scene" :
                              patch.Value.Contains("UI") || patch.Value.Contains("Menu") ? "UI" :
                              patch.Value.Contains("Game") ? "Game" : "General";
                
                if (!categoryGroups.ContainsKey(category))
                    categoryGroups[category] = new List<string>();
                
                categoryGroups[category].Add($"{patch.Key} → {patch.Value}");
            }

            foreach (var group in categoryGroups)
            {
                info.AppendLine($"  {group.Key}:");
                foreach (var item in group.Value)
                {
                    info.AppendLine($"    ✓ {item}");
                }
                info.AppendLine();
            }

            return info.ToString();
        }
    }
}