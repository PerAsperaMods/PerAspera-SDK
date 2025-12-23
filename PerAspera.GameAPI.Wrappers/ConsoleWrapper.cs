using BepInEx.Logging;
using PerAspera.Core;
using PerAspera.GameAPI.Wrappers.Core;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native Console class providing safe access to console functionality
    /// </summary>
    public class ConsoleWrapper : NativeWrapper<object>
    {
        private static readonly ManualLogSource Log = BepInEx.Logging.Logger.CreateLogSource("ConsoleWrapper");

        // to implement 	private Dictionary<string, MethodInfo> m_commands;
        //public static event Action<string, string, LogType> LogCallbackEvent
        // 	public void ExecuteCommandString(string cmd)
        //public void ExecuteFile(string path)
        //	private static void Commands(object[] args) Lists all available console commands, with an optional filter string

        /// <summary>
        /// Initialize Console wrapper with native console object
        /// </summary>
        /// <param name="nativeConsole">Native console instance from game</param>
        public ConsoleWrapper(object nativeConsole) : base(nativeConsole)
        {
        }

        /// <summary>
        /// Execute a command string through the console system
        /// </summary>
        /// <param name="command">Command string to execute (e.g., "import WATER 1000")</param>
        /// <returns>True if command executed successfully</returns>
        public bool ExecuteCommandString(string command)
        {
            try
            {
                CallNativeVoid("ExecuteCommandString", command);
                Log.LogInfo($"✅ Console command executed: {command}");
                return true;
            }
            catch (Exception ex)
            {
                Log.LogError($"❌ Failed to execute console command '{command}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Get list of all available console commands
        /// </summary>
        /// <returns>Array of command names, or null if unable to access</returns>
        public string[]? GetAvailableCommands()
        {
            try
            {
                // Access the private m_commands dictionary
                var commandsDict = GetNativeField<System.Collections.Generic.Dictionary<string, System.Reflection.MethodInfo>>("m_commands");
                if (commandsDict == null)
                {
                    Log.LogWarning("Unable to access m_commands dictionary");
                    return null;
                }

                var commandNames = commandsDict.Keys.ToArray();
                Log.LogInfo($"Found {commandNames.Length} available console commands");
                return commandNames;
            }
            catch (Exception ex)
            {
                Log.LogError($"❌ Failed to get available commands: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Log all available console commands to the console
        /// </summary>
        public void ListCommands()
        {
            var commands = GetAvailableCommands();
            if (commands == null || commands.Length == 0)
            {
                Log.LogWarning("No commands available or unable to access command list");
                return;
            }

            Log.LogInfo("=== AVAILABLE CONSOLE COMMANDS ===");
            foreach (var command in commands.OrderBy(c => c))
            {
                Log.LogInfo($"  {command}");
            }
            Log.LogInfo($"=== TOTAL: {commands.Length} commands ===");
        }

        /// <summary>
        /// Get the singleton Console instance
        /// </summary>
        /// <returns>ConsoleWrapper instance or null if not available</returns>
        public static ConsoleWrapper? GetInstance()
        {
            try
            {
                System.Type? consoleType = null;

                // Try ScriptsAssembly first (as seen in ILSpy)
                consoleType = Type.GetType("Console, ScriptsAssembly", false);
                if (consoleType == null)
                {
                    // Fallback: search through all loaded assemblies
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        consoleType = assembly.GetType("Console");
                        if (consoleType != null)
                        {
                            Log.LogInfo($"✅ Found Console type in assembly: {assembly.GetName().Name}");
                            break;
                        }
                    }
                }

                if (consoleType == null)
                {
                    Log.LogWarning("⚠️ Console type not found in any loaded assembly");
                    return null;
                }

                // Get the static instance property
                var instanceProperty = consoleType.GetProperty("instance", BindingFlags.Static | BindingFlags.Public);
                if (instanceProperty == null)
                {
                    Log.LogWarning("⚠️ Console.instance property not found");
                    return null;
                }

                var nativeInstance = instanceProperty.GetValue(null);
                if (nativeInstance == null)
                {
                    Log.LogWarning("⚠️ Console.instance is null");
                    return null;
                }

                return new ConsoleWrapper(nativeInstance);
            }
            catch (Exception ex)
            {
                Log.LogError($"❌ Failed to get Console instance: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create wrapper from native console object
        /// </summary>
        /// <param name="nativeConsole">Native console instance</param>
        /// <returns>ConsoleWrapper instance</returns>
        public static ConsoleWrapper? FromNative(object? nativeConsole)
        {
            if (nativeConsole == null)
            {
                Log.LogWarning("Cannot create ConsoleWrapper from null native object");
                return null;
            }

            return new ConsoleWrapper(nativeConsole);
        }
    }
}