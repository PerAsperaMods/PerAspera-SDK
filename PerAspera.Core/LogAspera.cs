using BepInEx.Logging;
using System;
using System.IO;

namespace PerAspera.Core
{
    /// <summary>
    /// Centralized logging system for Per Aspera mods
    /// Provides both BepInEx console logging and file-based logging
    /// </summary>
    public class LogAspera
    {
        private readonly string _componentName;
        private readonly string _prefix;
        private readonly ManualLogSource _logSource;
        private readonly string _logFilePath;
        private static readonly object _fileLock = new object();

        // Static logging directory
        private static readonly string LogDirectory = Path.Combine(
            Environment.CurrentDirectory, 
            "BepInEx", 
            "logs", 
            "PerAspera"
        );

        /// <summary>
        /// Static constructor to ensure log directory exists
        /// </summary>
        static LogAspera()
        {
            try
            {
                if (!Directory.Exists(LogDirectory))
                {
                    Directory.CreateDirectory(LogDirectory);
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLogSource("LogAspera").LogWarning($"Failed to create logs directory: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a LogAspera instance with a specific component name
        /// </summary>
        /// <param name="componentName">Component name to identify log source</param>
        public LogAspera(string componentName)
        {
            _componentName = componentName ?? "Unknown";
            _prefix = $"[{_componentName}]";
            _logSource = Logger.CreateLogSource(_componentName);

            // Component-specific log file path
            _logFilePath = Path.Combine(LogDirectory, $"{_componentName}.log");

            // Write header on startup
            WriteToFile($"=== {_componentName} Log Started at {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");
        }

        /// <summary>
        /// Logs an information message
        /// </summary>
        public void Info(string message)
        {
            var formattedMessage = $"{_prefix} {message}";
            _logSource.LogInfo(formattedMessage);
            WriteToFile($"[INFO] {message}");
        }

        /// <summary>
        /// Logs a debug message
        /// </summary>
        public void Debug(string message)
        {
            var formattedMessage = $"{_prefix} {message}";
            _logSource.LogDebug(formattedMessage);
            WriteToFile($"[DEBUG] {message}");
        }

        /// <summary>
        /// Logs a warning message
        /// </summary>
        public void Warning(string message)
        {
            var formattedMessage = $"{_prefix} {message}";
            _logSource.LogWarning(formattedMessage);
            WriteToFile($"[WARNING] {message}");
        }

        /// <summary>
        /// Logs an error message
        /// </summary>
        public void Error(string message)
        {
            var formattedMessage = $"{_prefix} {message}";
            _logSource.LogError(formattedMessage);
            WriteToFile($"[ERROR] {message}");
        }

        /// <summary>
        /// Logs a fatal message
        /// </summary>
        public void Fatal(string message)
        {
            var formattedMessage = $"{_prefix} {message}";
            _logSource.LogFatal(formattedMessage);
            WriteToFile($"[FATAL] {message}");
        }

        /// <summary>
        /// Logs a message with custom level
        /// </summary>
        public void Log(LogLevel level, string message)
        {
            var formattedMessage = $"{_prefix} {message}";
            _logSource.Log(level, formattedMessage);
            WriteToFile($"[{level.ToString().ToUpper()}] {message}");
        }

        /// <summary>
        /// Writes message to component-specific log file
        /// </summary>
        private void WriteToFile(string message)
        {
            try
            {
                lock (_fileLock)
                {
                    var timestampedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}";
                    File.AppendAllText(_logFilePath, timestampedMessage + Environment.NewLine);
                }
            }
            catch
            {
                // Silent failure to avoid infinite logging loops
            }
        }

        /// <summary>
        /// Cleans old log files (call manually)
        /// </summary>
        /// <param name="daysToKeep">Number of days to keep logs</param>
        public static void CleanOldLogs(int daysToKeep = 7)
        {
            try
            {
                if (!Directory.Exists(LogDirectory)) 
                    return;

                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                var logFiles = Directory.GetFiles(LogDirectory, "*.log");

                foreach (var file in logFiles)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.LastWriteTime < cutoffDate)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLogSource("LogAspera").LogWarning($"Failed to clean old logs: {ex.Message}");
            }
        }

        //------------------------------------------------------
        // STATIC CONVENIENCE METHODS
        //------------------------------------------------------

        private static readonly LogAspera _defaultLogger = new LogAspera("PerAspera");

        /// <summary>
        /// Static convenience method for quick logging
        /// </summary>
        public static void LogInfo(string message) => _defaultLogger.Info(message);

        /// <summary>
        /// Static convenience method for debug logging
        /// </summary>
        public static void LogDebug(string message) => _defaultLogger.Debug(message);

        /// <summary>
        /// Static convenience method for warning logging
        /// </summary>
        public static void LogWarning(string message) => _defaultLogger.Warning(message);

        /// <summary>
        /// Static convenience method for error logging
        /// </summary>
        public static void LogError(string message) => _defaultLogger.Error(message);

        /// <summary>
        /// Static convenience method for fatal logging
        /// </summary>
        public static void LogFatal(string message) => _defaultLogger.Fatal(message);
    }
}
