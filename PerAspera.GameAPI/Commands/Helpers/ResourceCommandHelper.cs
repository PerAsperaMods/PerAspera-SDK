using System;
using System.Linq;
using System.Reflection;
using PerAspera.Core;

namespace PerAspera.GameAPI.Commands.Helpers
{
    /// <summary>
    /// Helper for executing game console cheat commands via reflection.
    /// Wraps the IL2CPP Console singleton's ExecuteCommandString method.
    /// </summary>
    public static class ResourceCommandHelper
    {
        private static readonly LogAspera _log = new LogAspera("ConsoleCommandHelper");

        // Cache the type and method — they never change once the game loads.
        // The instance is fetched fresh each call (Console.instance may not be ready at startup).
        private static System.Type? _consoleType;
        private static MethodInfo? _executeMethod;
        private static bool _typeSearched;

        // Finds the GAME's Console class (global namespace, FullName=="Console").
        // FindTypeStatic("Console") matches System.Console via t.Name=="Console" first —
        // using FullName exact match ensures we get the game's class, not System.Console.
        private static System.Type? FindGameConsoleType()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var t = assembly.GetTypes().FirstOrDefault(x => x.FullName == "Console");
                    if (t != null) return t;
                }
                catch { /* skip assemblies that can't enumerate types */ }
            }
            return null;
        }

        /// <summary>
        /// Execute any game console command string via Console.instance.ExecuteCommandString.
        /// </summary>
        /// <param name="cmd">Full command string, e.g. "BunchOfResources" or "FactionAddResourceDistributed water 500"</param>
        /// <returns>true if the command was dispatched; false if Console not ready</returns>
        public static bool ExecuteConsoleCommand(string cmd)
        {
            try
            {
                // Console.instance / ExecuteCommandString \u2014 typed, InteropDump lignes 1279 + 1659
                var console = Console.instance;
                if (console == null)
                {
                    _log.Warning("[ConsoleCmd] Console.instance is null \u2014 game not ready?");
                    return false;
                }
                console.ExecuteCommandString(cmd);
                return true;
            }
            catch (Exception ex)
            {
                _log.Warning($"[ConsoleCmd] Failed to execute '{cmd}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Import a resource for a faction via console command.
        /// </summary>
        public static bool ExecuteResourceImportCommand(string factionName, string resourceKey, float amount)
            => ExecuteConsoleCommand($"ImportResource {factionName} {resourceKey} {(int)amount}");

        /// <summary>
        /// Distribute a resource across all faction stockpiles.
        /// </summary>
        public static bool ExecuteResourceDistributedCommand(string resourceKey, float amount)
            => ExecuteConsoleCommand($"FactionAddResourceDistributed {resourceKey} {(int)amount}");

        /// <summary>
        /// Add a resource directly to a building's storage.
        /// </summary>
        public static bool ExecuteBuildingAddResourceCommand(string buildingId, string resourceKey, float amount)
            => ExecuteConsoleCommand($"BuildingAddResource {buildingId} {resourceKey} {(int)amount}");
    }
}
