using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PerAspera.Core.IL2CPP;
using BepInEx.Logging;

namespace PerAspera.Core.IL2CPP
{
    /// <summary>
    /// Consolidated console command execution system
    /// Centralizes all console command logic for mods
    /// Provides both console-based and native API execution
    /// </summary>
    public static class ConsoleCommandExecutor
    {
        private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("ConsoleCommandExecutor");

        /// <summary>
        /// Execute a console command string
        /// </summary>
        public static bool ExecuteConsoleCommand(string command)
        {
            try
            {
                // Use the same approach as PerAsperaExtensions
                var console = PerAsperaExtensions.GetConsole();
                return console?.InvokeMethod("ExecuteCommandString", command) ?? false;
            }
            catch (Exception ex)
            {
                Log.LogError($"❌ Console command execution failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Execute a faction resource addition command
        /// Supports both console and native execution modes
        /// </summary>
        public static bool ExecuteFactionAddResource(string resourceName, float amount, bool useNative = true)
        {
            if (useNative)
            {
                // Try native API first (standard command)
                return ExecuteFactionAddResourceNative(resourceName, amount);
            }
            else
            {
                // Fall back to console command
                return ExecuteFactionAddResourceConsole(resourceName, amount);
            }
        }

        /// <summary>
        /// Execute faction resource addition using native APIs (standard command)
        /// This is the preferred method for mods
        /// </summary>
        private static bool ExecuteFactionAddResourceNative(string resourceName, float amount)
        {
            try
            {
                // Get current faction
                var faction = PerAsperaExtensions.GetCurrentFaction();
                if (faction == null)
                {
                    Log.LogError("❌ Cannot get current faction for native resource addition");
                    return false;
                }

                // Use the simplified extension method
                return PerAsperaExtensions.AddResourcesToFaction(faction, resourceName, amount);
            }
            catch (Exception ex)
            {
                Log.LogError($"❌ Native resource addition failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Execute faction resource addition using console command (fallback)
        /// </summary>
        private static bool ExecuteFactionAddResourceConsole(string resourceName, float amount)
        {
            string command = $"factionaddresourcedistributed {resourceName} {amount}";
            return ExecuteConsoleCommand(command);
        }

        /// <summary>
        /// Execute multiple resource additions in batch
        /// </summary>
        public static bool ExecuteFactionAddResources(Dictionary<string, float> resources, bool useNative = true)
        {
            bool allSuccessful = true;

            foreach (var kvp in resources)
            {
                if (!ExecuteFactionAddResource(kvp.Key, kvp.Value, useNative))
                {
                    Log.LogError($"❌ Failed to add {kvp.Value} {kvp.Key}");
                    allSuccessful = false;
                }
            }

            return allSuccessful;
        }

        /// <summary>
        /// Execute research points addition
        /// </summary>
        public static bool ExecuteAddResearchPoints(float amount, bool useNative = true)
        {
            if (useNative)
            {
                return ExecuteAddResearchPointsNative(amount);
            }
            else
            {
                return ExecuteAddResearchPointsConsole(amount);
            }
        }

        /// <summary>
        /// Add research points using native API
        /// </summary>
        private static bool ExecuteAddResearchPointsNative(float amount)
        {
            try
            {
                var faction = PerAsperaExtensions.GetCurrentFaction();
                if (faction == null)
                {
                    Log.LogError("❌ Cannot get current faction for research points");
                    return false;
                }

                // Try to find and call AddResearchPoints method
                var method = faction.GetType().GetMethod("AddResearchPoints",
                    BindingFlags.Public | BindingFlags.Instance);

                if (method != null)
                {
                    method.Invoke(faction, new object[] { amount });
                    Log.LogInfo($"✅ Added {amount} research points natively");
                    return true;
                }
                else
                {
                    Log.LogWarning("⚠️ AddResearchPoints method not found, falling back to console");
                    return ExecuteAddResearchPointsConsole(amount);
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"❌ Native research points addition failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Add research points using console command
        /// </summary>
        private static bool ExecuteAddResearchPointsConsole(float amount)
        {
            string command = $"addresearchpoints {amount}";
            return ExecuteConsoleCommand(command);
        }

        /// <summary>
        /// Execute building unlock command
        /// </summary>
        public static bool ExecuteUnlockBuilding(string buildingName, bool useNative = true)
        {
            if (useNative)
            {
                return ExecuteUnlockBuildingNative(buildingName);
            }
            else
            {
                return ExecuteUnlockBuildingConsole(buildingName);
            }
        }

        /// <summary>
        /// Unlock building using native API
        /// </summary>
        private static bool ExecuteUnlockBuildingNative(string buildingName)
        {
            try
            {
                var faction = PerAsperaExtensions.GetCurrentFaction();
                if (faction == null)
                {
                    Log.LogError("❌ Cannot get current faction for building unlock");
                    return false;
                }

                // Try to find building unlock method
                var method = faction.GetType().GetMethod("UnlockBuilding",
                    BindingFlags.Public | BindingFlags.Instance);

                if (method != null)
                {
                    method.Invoke(faction, new object[] { buildingName });
                    Log.LogInfo($"✅ Unlocked building: {buildingName}");
                    return true;
                }
                else
                {
                    Log.LogWarning("⚠️ UnlockBuilding method not found, falling back to console");
                    return ExecuteUnlockBuildingConsole(buildingName);
                }
            }
            catch (Exception ex)
            {
                Log.LogError($"❌ Native building unlock failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Unlock building using console command
        /// </summary>
        private static bool ExecuteUnlockBuildingConsole(string buildingName)
        {
            string command = $"unlockbuilding {buildingName}";
            return ExecuteConsoleCommand(command);
        }

        /// <summary>
        /// Generic method to execute any console command with parameters
        /// </summary>
        public static bool ExecuteCommand(string commandName, params object[] parameters)
        {
            try
            {
                string paramString = string.Join(" ", parameters.Select(p => p.ToString()));
                string fullCommand = $"{commandName} {paramString}";
                return ExecuteConsoleCommand(fullCommand);
            }
            catch (Exception ex)
            {
                Log.LogError($"❌ Command execution failed: {ex.Message}");
                return false;
            }
        }
    }
}