using System;
using System.Collections.Generic;
using System.Reflection;
using PerAspera.Core;

namespace PerAspera.GameAPI.Commands.Helpers
{
    /// <summary>
    /// Helper to discover native game commands from InteractionParser
    /// Calls InteractionParser.GetCommands() to retrieve valid TextAction command names
    /// </summary>
    public static class CommandDiscovery
    {
        private static readonly LogAspera _log = new LogAspera("CommandDiscovery");

        /// <summary>
        /// Get all valid TextAction command names by calling InteractionParser.GetCommands()
        /// Returns empty array if not available
        /// </summary>
        public static string[] GetValidTextActionCommands()
        {
            try
            {
                _log.Debug("Attempting to call InteractionParser.GetCommands()...");

                // Find InteractionParser in Assembly-CSharp
                var assembly = System.Reflection.Assembly.Load("Assembly-CSharp");
                if (assembly == null)
                {
                    _log.Warning("Assembly-CSharp not loaded");
                    return Array.Empty<string>();
                }

                var parserType = assembly.GetType("PerAspera.InteractionParser");
                if (parserType == null)
                {
                    _log.Warning("PerAspera.InteractionParser type not found");
                    return Array.Empty<string>();
                }

                _log.Debug($"Found InteractionParser type");

                // Get GetCommands() method
                var getCommandsMethod = parserType.GetMethod(
                    "GetCommands",
                    BindingFlags.Static | BindingFlags.Public,
                    null,
                    Array.Empty<Type>(),
                    null);

                if (getCommandsMethod == null)
                {
                    _log.Warning("GetCommands() method not found on InteractionParser");
                    return Array.Empty<string>();
                }

                _log.Debug($"Found GetCommands() method, return type: {getCommandsMethod.ReturnType.Name}");

                // Invoke GetCommands()
                object commandsArray = null;
                try
                {
                    commandsArray = getCommandsMethod.Invoke(null, null);
                }
                catch (Exception ex)
                {
                    _log.Error($"Exception invoking GetCommands(): {ex.GetType().Name}");
                    _log.Error($"  Message: {ex.Message}");
                    if (ex.InnerException != null)
                        _log.Error($"  Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                    return Array.Empty<string>();
                }

                if (commandsArray == null)
                {
                    _log.Warning("GetCommands() returned null");
                    return Array.Empty<string>();
                }

                _log.Debug($"GetCommands() returned type: {commandsArray.GetType().Name}");

                // Convert to string array
                var result = new List<string>();

                try
                {
                    if (commandsArray is System.Collections.IEnumerable enumerable)
                    {
                        foreach (var item in enumerable)
                        {
                            if (item != null)
                            {
                                var str = item.ToString();
                                if (!string.IsNullOrEmpty(str))
                                    result.Add(str);
                            }
                        }
                    }
                    else
                    {
                        _log.Warning($"GetCommands() result is not enumerable: {commandsArray.GetType().Name}");
                    }
                }
                catch (Exception ex)
                {
                    _log.Error($"Error enumerating commands: {ex.GetType().Name}: {ex.Message}");
                    return Array.Empty<string>();
                }

                _log.Info($"✅ Discovered {result.Count} valid TextAction commands from GetCommands()");
                return result.ToArray();
            }
            catch (Exception ex)
            {
                _log.Error($"CRITICAL - Failed to discover commands: {ex.GetType().Name}: {ex.Message}");
                return Array.Empty<string>();
            }
        }
    }
}
