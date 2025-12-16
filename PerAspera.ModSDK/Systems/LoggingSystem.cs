using PerAspera.Core;

namespace PerAspera.ModSDK.Systems
{
    /// <summary>
    /// Logging utilities - Use this instead of Console.WriteLine
    /// </summary>
    public static class LoggingSystem
    {
        private static string _modName = "Unknown";

        /// <summary>
        /// Initialize the logging system with mod name
        /// </summary>
        internal static void Initialize(string modName)
        {
            _modName = modName ?? "Unknown";
        }

        /// <summary>
        /// Log info message
        /// </summary>
        public static void Info(string message) => LogAspera.LogInfo($"[{_modName}] {message}");

        /// <summary>
        /// Log warning message
        /// </summary>
        public static void Warning(string message) => LogAspera.LogWarning($"[{_modName}] {message}");

        /// <summary>
        /// Log error message
        /// </summary>
        public static void Error(string message) => LogAspera.LogError($"[{_modName}] {message}");

        /// <summary>
        /// Log debug message
        /// </summary>
        public static void Debug(string message) => LogAspera.LogDebug($"[{_modName}] {message}");

        /// <summary>
        /// Create a prefixed logger for a specific component
        /// </summary>
        public static ComponentLogger CreateComponentLogger(string componentName)
        {
            return new ComponentLogger(_modName, componentName);
        }
    }

    /// <summary>
    /// Component-specific logger
    /// </summary>
    public class ComponentLogger
    {
        private readonly string _prefix;

        internal ComponentLogger(string modName, string componentName)
        {
            _prefix = $"[{modName}.{componentName}]";
        }

        public void Info(string message) => LogAspera.LogInfo($"{_prefix} {message}");
        public void Warning(string message) => LogAspera.LogWarning($"{_prefix} {message}");
        public void Error(string message) => LogAspera.LogError($"{_prefix} {message}");
        public void Debug(string message) => LogAspera.LogDebug($"{_prefix} {message}");
    }
}