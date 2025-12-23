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

namespace PerAspera.GameAPI.Commands
{
    /// <summary>
    /// Resource command execution utilities that handle IHandleable casting internally
    /// to avoid exposing native types to mods.
    /// </summary>
    public static class ResourceCommandHelper
    {
        private static ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("ResourceCommandHelper");

        /// <summary>
        /// Executes a resource import command for the specified faction using the given resource type.
        /// Handles IHandleable casting internally to avoid exposing native types to mods.
        /// </summary>
        /// <param name="factionHandle">The faction handle wrapper to execute the command for</param>
        /// <param name="resourceType">The resource type (e.g., "WATER", "CHG", "ICE", "NITROGEN", "OXYGEN")</param>
        /// <param name="amount">The amount of resource to add (default: 1000)</param>
        /// <returns>True if the command executed successfully, false otherwise</returns>
        public static bool ExecuteResourceImportCommand(HandleWrapper factionHandle, string resourceType, float amount = 1000f)
        {
            if (factionHandle == null)
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
                // Get GameEventBus from player faction via SDK wrappers
                var baseGame = PerAspera.GameAPI.Wrappers.BaseGameWrapper.GetCurrent();
                if (baseGame == null)
                {
                    _logger.LogError("ResourceCommandHelper: Cannot get BaseGame instance");
                    return false;
                }

                var universe = baseGame.GetUniverse();
                if (universe == null)
                {
                    _logger.LogError("ResourceCommandHelper: Cannot get Universe instance");
                    return false;
                }

                var playerFaction = universe.GetPlayerFaction();
                if (playerFaction == null)
                {
                    _logger.LogError("ResourceCommandHelper: Cannot get player faction");
                    return false;
                }

                var gameEventBus = playerFaction.GetGameEventBus();
                if (gameEventBus == null)
                {
                    _logger.LogError("ResourceCommandHelper: Cannot get GameEventBus from player faction");
                    return false;
                }

                // Create TextAction using wrapper
                var textAction = PerAspera.GameAPI.Wrappers.TextActionWrapper.CreateAddResource(resourceType, (int)amount);
                if (textAction == null)
                {
                    _logger.LogError($"ResourceCommandHelper: Failed to create TextAction for resource {resourceType}");
                    return false;
                }
                // Execute the command using InteractionManagerWrapper
                return playerFaction.GetInteractionManager().DispatchAction(
                    factionHandle.GetNativeObject(), // Handle is IHandleable
                    gameEventBus,
                    textAction.GetNativeTextActionObject(),
                    $"ResourceImport_{resourceType}_{amount}"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"ResourceCommandHelper: Failed to execute resource import command: {ex.Message}");
                return false;
            }
        }
    }
}