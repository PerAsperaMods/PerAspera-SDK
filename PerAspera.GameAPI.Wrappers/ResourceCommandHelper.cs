using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Wrappers;
using PerAspera.GameAPI.Wrappers.Core;
using PerAspera.GameAPI.Native;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Resource command execution utilities that handle IHandleable casting internally
    /// to avoid exposing native types to mods.
    /// </summary>
    public static class ResourceCommandHelper
    {
        private static System.Type? _iHandleableType;
        private static System.Type? _interactionManagerType;
        private static System.Type? _textActionType;
        private static System.Type? _gameEventBusType;
        private static MethodInfo? _dispatchActionMethod;
        private static ConstructorInfo? _textActionConstructor;
        private static ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("ResourceCommandHelper");

        static ResourceCommandHelper()
        {
            InitializeTypes();
        }

        private static void InitializeTypes()
        {
            try
            {
                // Get assemblies
                var scriptsAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "ScriptsAssembly");

                if (scriptsAssembly != null)
                {
                    _iHandleableType = scriptsAssembly.GetType("IHandleable");
                    _interactionManagerType = scriptsAssembly.GetType("InteractionManager");
                    _textActionType = scriptsAssembly.GetType("TextAction");
                    _gameEventBusType = scriptsAssembly.GetType("GameEventBus");

                    // Cache constructor for TextAction
                    if (_textActionType != null)
                    {
                        _textActionConstructor = _textActionType.GetConstructor(new[] { typeof(string), typeof(string[]) });
                    }

                    // Cache DispatchAction method
                    if (_interactionManagerType != null && _iHandleableType != null && _gameEventBusType != null && _textActionType != null)
                    {
                        _dispatchActionMethod = _interactionManagerType.GetMethod("DispatchAction",
                            BindingFlags.Public | BindingFlags.Static,
                            null,
                            new System.Type[] { _iHandleableType, _gameEventBusType, _textActionType, typeof(string) },
                            null);
                    }
                }

                _logger.LogInfo($"ResourceCommandHelper initialized: IHandleable={_iHandleableType != null}, InteractionManager={_interactionManagerType != null}, TextAction={_textActionType != null}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to initialize ResourceCommandHelper types: {ex.Message}");
            }
        }

        /// <summary>
        /// Executes a resource import command for the specified faction using the given resource type.
        /// Handles IHandleable casting internally to avoid exposing native types to mods.
        /// </summary>
        /// <param name="factionHandle">The faction handle wrapper to execute the command for</param>
        /// <param name="resourceType">The resource type (e.g., "WATER", "CHG", "ICE", "NITROGEN", "OXYGEN")</param>
        /// <param name="amount">The amount of resource to add (default: 1000)</param>
        /// <returns>True if the command executed successfully, false otherwise</returns>
        public static bool ExecuteResourceImportCommand(IHandleable handleable, string resourceType, float amount = 1000f)
        {
            if (handleable == null)
            {
                _logger.LogError("ResourceCommandHelper: Faction handle cannot be null");
                return false;
            }

            if (string.IsNullOrEmpty(resourceType))
            {
                _logger.LogError("ResourceCommandHelper: Resource type cannot be null or empty");
                return false;
            }

            try
            {
                // il faut rechercehr le REsoruceTYPE dans le  KeeperMap/keerper

                // Get native objects directly from IL2CPP
                HandleWrapper _h = new HandleWrapper(handleable.handle);
                
                TextAction? textAction = CreateNativeTextAction(resourceType, amount) as TextAction;


                // Execute the command using cached method
                return DispatchActionInternal(handleable, handleable.GetMemberValue<GameEventBus>("_gameEventBus"), textAction, $"ResourceImport_{resourceType}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"ResourceCommandHelper: Failed to execute resource import command: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get native faction object from handle using KeeperMap
        /// </summary>
        private static object? GetNativeFactionFromHandle(HandleWrapper factionHandle)
        {
            try
            {
                // Access BaseGame.Instance directly (IL2CPP singleton)
                var baseGameType = ReflectionHelpers.FindType("BaseGame");
                if (baseGameType == null) return null;

                var instanceProperty = baseGameType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                if (instanceProperty == null) return null;

                var baseGameInstance = instanceProperty.GetValue(null);
                if (baseGameInstance == null) return null;

                // Get universe
                var universeProperty = baseGameType.GetProperty("universe");
                if (universeProperty == null) return null;

                var universe = universeProperty.GetValue(baseGameInstance);
                if (universe == null) return null;

                // Get keeper from universe
                var keeperField = universe.GetType().GetField("_keeper", BindingFlags.NonPublic | BindingFlags.Instance) ??
                                 universe.GetType().GetField("keeper", BindingFlags.Public | BindingFlags.Instance);
                if (keeperField == null) return null;

                var keeper = keeperField.GetValue(universe);
                if (keeper == null) return null;

                // Get keeperMap from keeper
                var keeperMapField = keeper.GetType().GetField("_keeperMap", BindingFlags.NonPublic | BindingFlags.Instance) ??
                                    keeper.GetType().GetField("keeperMap", BindingFlags.Public | BindingFlags.Instance);
                if (keeperMapField == null) return null;

                var keeperMap = keeperMapField.GetValue(keeper);
                if (keeperMap == null) return null;

                // Find faction using handle
                var findMethod = keeperMap.GetType().GetMethod("Find", new[] { factionHandle.GetNativeObject().GetType() });
                if (findMethod == null) return null;

                return findMethod.Invoke(keeperMap, new[] { factionHandle.GetNativeObject() });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get native faction from handle: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get GameEventBus from native faction object
        /// </summary>
        private static object? GetGameEventBusFromFaction(object nativeFaction)
        {
            try
            {
                // Try different field names for GameEventBus
                var gameEventBusField = nativeFaction.GetType().GetField("_gameEventBus", BindingFlags.NonPublic | BindingFlags.Instance) ??
                                       nativeFaction.GetType().GetField("gameEventBus", BindingFlags.Public | BindingFlags.Instance) ??
                                       nativeFaction.GetType().GetField("m_gameEventBus", BindingFlags.NonPublic | BindingFlags.Instance);

                if (gameEventBusField != null)
                {
                    return gameEventBusField.GetValue(nativeFaction);
                }

                // Try property access
                var gameEventBusProperty = nativeFaction.GetType().GetProperty("GameEventBus", BindingFlags.Public | BindingFlags.Instance);
                if (gameEventBusProperty != null)
                {
                    return gameEventBusProperty.GetValue(nativeFaction);
                }

                _logger.LogError("Cannot find GameEventBus field or property on faction");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get GameEventBus from faction: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create native TextAction object
        /// </summary>
        public static object? CreateNativeTextAction(string resourceType, float amount)
        {
            try
            {
                if (_textActionConstructor == null)
                {
                    _logger.LogError("TextAction constructor not cached");
                    return null;
                }

                // Normalize resource type
                var normalizedType = resourceType.ToUpperInvariant() switch
                {
                    "WATER" or "H2O" => "WATER",
                    "ICE" => "Ice",
                    "CHG" or "METAL" or "IRON" or "METHANE" or "CARBON" => "CHG",
                    "NITROGEN" => "NITROGEN",
                    "OXYGEN" => "OXYGEN",
                    _ => resourceType.ToUpperInvariant()
                };

                // Create command string
                var commandString = $"FactionAddResourceDistributed\t{normalizedType}\t{amount}";
                var arguments = new string[0];

                // Create TextAction instance
                return _textActionConstructor.Invoke(new object[] { commandString, arguments });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to create TextAction: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Execute DispatchAction using cached method
        /// </summary>
        private static bool DispatchActionInternal(object handleable, object gameEventBus, object textAction, string context)
        {
            try
            {
                if (_dispatchActionMethod == null)
                {
                    _logger.LogError("DispatchAction method not cached");
                    return false;
                }

                _logger.LogInfo($"🚀 Dispatching resource command: {context}");

                // Invoke the cached method
                _dispatchActionMethod.Invoke(null, new object[] { handleable, gameEventBus, textAction, context });

                _logger.LogInfo($"✅ Resource command dispatched successfully: {context}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to dispatch action: {ex.Message}");
                return false;
            }
        }
    }
}