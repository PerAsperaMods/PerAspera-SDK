using System;
using System.Reflection;
using BepInEx.Logging;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Commands
{
    /// <summary>
    /// Resource command execution utilities using pure reflection.
    /// Provides a type-safe facade over the game's resource import console commands.
    /// </summary>
    public static class ResourceCommandHelper
    {
        private static readonly ManualLogSource _logger = BepInEx.Logging.Logger.CreateLogSource("ResourceCommandHelper");
        private static System.Type? _consoleType;

        /// <summary>
        /// Executes a resource import command for the specified faction via the game console.
        /// Internally builds the "ImportResource &lt;faction&gt; &lt;resourceType&gt; &lt;amount&gt;" command string.
        /// </summary>
        /// <param name="factionName">The faction identifier (e.g. "PlayerFaction").</param>
        /// <param name="resourceType">The resource type key (e.g. "WATER", "CHG", "ICE", "NITROGEN", "OXYGEN").</param>
        /// <param name="amount">Amount of resource to add (default: 1000).</param>
        /// <returns>True if the command was dispatched without exception, false otherwise.</returns>
        /// <example>
        /// <code>
        /// ResourceCommandHelper.ExecuteResourceImportCommand("PlayerFaction", "WATER", 5000f);
        /// </code>
        /// </example>
        public static bool ExecuteResourceImportCommand(string factionName, string resourceType, float amount = 1000f)
        {
            if (string.IsNullOrEmpty(factionName))
            {
                _logger.LogError("ResourceCommandHelper: factionName cannot be null or empty");
                return false;
            }

            if (string.IsNullOrEmpty(resourceType))
            {
                _logger.LogError("ResourceCommandHelper: resourceType cannot be null or empty");
                return false;
            }

            try
            {
                _consoleType ??= ReflectionHelpers.FindType("Console");
                if (_consoleType == null)
                {
                    _logger.LogError("ResourceCommandHelper: Console type not found via reflection");
                    return false;
                }

                var instanceProp = _consoleType.GetProperty(
                    "instance",
                    BindingFlags.Static | BindingFlags.Public);
                var consoleInstance = instanceProp?.GetValue(null);
                if (consoleInstance == null)
                {
                    _logger.LogError("ResourceCommandHelper: Console.instance is null (game not ready?)");
                    return false;
                }

                var execMethod = _consoleType.GetMethod(
                    "ExecuteCommandString",
                    BindingFlags.Public | BindingFlags.Instance);
                if (execMethod == null)
                {
                    _logger.LogError("ResourceCommandHelper: Console.ExecuteCommandString method not found");
                    return false;
                }

                string cmd = $"ImportResource {factionName} {resourceType} {(int)amount}";
                _logger.LogInfo($"[ResourceCommandHelper] Executing: {cmd}");
                execMethod.Invoke(consoleInstance, new object[] { cmd });
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ResourceCommandHelper: Exception executing resource import: {ex.Message}");
                return false;
            }
        }
    }
}