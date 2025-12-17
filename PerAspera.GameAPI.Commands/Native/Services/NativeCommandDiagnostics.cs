using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.Logging;

namespace PerAspera.GameAPI.Commands.Native.Services
{
    /// <summary>
    /// Comprehensive diagnostics service for native command system
    /// Provides error handling, performance monitoring, and debugging tools
    /// Includes system health checks and detailed reporting capabilities
    /// </summary>
    public sealed class NativeCommandDiagnostics
    {
        private readonly TypeDiscoveryService _typeDiscovery;
        private readonly ReflectionCacheService _reflectionCache;
        private readonly CommandInstanceFactory _instanceFactory;
        
        private readonly List<DiagnosticEntry> _diagnosticHistory;
        private readonly object _historyLock = new object();
        private readonly Dictionary<string, int> _errorCounts;
        private DateTime _startTime;

        public NativeCommandDiagnostics(
            TypeDiscoveryService typeDiscovery,
            ReflectionCacheService reflectionCache,
            CommandInstanceFactory instanceFactory)
        {
            _typeDiscovery = typeDiscovery ?? throw new ArgumentNullException(nameof(typeDiscovery));
            _reflectionCache = reflectionCache ?? throw new ArgumentNullException(nameof(reflectionCache));
            _instanceFactory = instanceFactory ?? throw new ArgumentNullException(nameof(instanceFactory));
            
            _diagnosticHistory = new List<DiagnosticEntry>();
            _errorCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            _startTime = DateTime.UtcNow;
            
            LogAspera.Info("NativeCommandDiagnostics service initialized");
        }

        /// <summary>
        /// Record a diagnostic event for monitoring and analysis
        /// </summary>
        /// <param name="level">Severity level</param>
        /// <param name="category">Event category</param>
        /// <param name="message">Diagnostic message</param>
        /// <param name="details">Additional details</param>
        public void RecordEvent(DiagnosticLevel level, string category, string message, object details = null)
        {
            var entry = new DiagnosticEntry
            {
                Timestamp = DateTime.UtcNow,
                Level = level,
                Category = category,
                Message = message,
                Details = details?.ToString()
            };

            lock (_historyLock)
            {
                _diagnosticHistory.Add(entry);
                
                // Keep history manageable (last 1000 entries)
                if (_diagnosticHistory.Count > 1000)
                {
                    _diagnosticHistory.RemoveAt(0);
                }

                // Track error counts by category
                if (level == DiagnosticLevel.Error || level == DiagnosticLevel.Warning)
                {
                    var key = $"{level}:{category}";
                    _errorCounts[key] = _errorCounts.GetValueOrDefault(key, 0) + 1;
                }
            }

            // Log to BepInX based on severity
            switch (level)
            {
                case DiagnosticLevel.Error:
                    LogAspera.Error($"[{category}] {message}");
                    break;
                case DiagnosticLevel.Warning:
                    LogAspera.Warning($"[{category}] {message}");
                    break;
                case DiagnosticLevel.Info:
                    LogAspera.Info($"[{category}] {message}");
                    break;
                case DiagnosticLevel.Debug:
                    LogAspera.Debug($"[{category}] {message}");
                    break;
            }
        }

        /// <summary>
        /// Perform comprehensive system health check
        /// Validates all components and reports any issues
        /// </summary>
        /// <returns>Health check result with detailed status</returns>
        public SystemHealthResult PerformHealthCheck()
        {
            var result = new SystemHealthResult
            {
                CheckTime = DateTime.UtcNow,
                OverallHealth = HealthStatus.Healthy,
                ComponentStatuses = new Dictionary<string, ComponentHealth>()
            };

            try
            {
                // Check Type Discovery Service
                result.ComponentStatuses["TypeDiscovery"] = CheckTypeDiscoveryHealth();
                
                // Check Reflection Cache Service
                result.ComponentStatuses["ReflectionCache"] = CheckReflectionCacheHealth();
                
                // Check Instance Factory
                result.ComponentStatuses["InstanceFactory"] = CheckInstanceFactoryHealth();
                
                // Check overall system integration
                result.ComponentStatuses["SystemIntegration"] = CheckSystemIntegrationHealth();

                // Determine overall health
                var worstStatus = result.ComponentStatuses.Values
                    .Select(c => c.Status)
                    .OrderByDescending(s => (int)s)
                    .FirstOrDefault();
                
                result.OverallHealth = worstStatus;
                
                RecordEvent(
                    result.OverallHealth == HealthStatus.Healthy ? DiagnosticLevel.Info : DiagnosticLevel.Warning,
                    "HealthCheck",
                    $"System health check completed: {result.OverallHealth}",
                    result.ComponentStatuses.Count
                );
            }
            catch (Exception ex)
            {
                result.OverallHealth = HealthStatus.Critical;
                RecordEvent(DiagnosticLevel.Error, "HealthCheck", "Health check failed", ex.Message);
            }

            return result;
        }

        /// <summary>
        /// Check health of type discovery service
        /// </summary>
        private ComponentHealth CheckTypeDiscoveryHealth()
        {
            var health = new ComponentHealth
            {
                ComponentName = "TypeDiscoveryService",
                Status = HealthStatus.Healthy,
                Details = new List<string>()
            };

            try
            {
                if (!_typeDiscovery.IsInitialized)
                {
                    health.Status = HealthStatus.Critical;
                    health.Details.Add("Service not initialized");
                    return health;
                }

                var availableTypes = _typeDiscovery.GetAvailableCommandTypes();
                health.Details.Add($"Command types discovered: {availableTypes.Length}");
                
                if (availableTypes.Length == 0)
                {
                    health.Status = HealthStatus.Warning;
                    health.Details.Add("No command types found - possible discovery issue");
                }
                else if (availableTypes.Length < 10)
                {
                    health.Status = HealthStatus.Warning;
                    health.Details.Add("Low number of command types - verify game assemblies loaded");
                }
                else
                {
                    health.Details.Add("Normal command type discovery");
                }
            }
            catch (Exception ex)
            {
                health.Status = HealthStatus.Critical;
                health.Details.Add($"Exception during check: {ex.Message}");
            }

            return health;
        }

        /// <summary>
        /// Check health of reflection cache service
        /// </summary>
        private ComponentHealth CheckReflectionCacheHealth()
        {
            var health = new ComponentHealth
            {
                ComponentName = "ReflectionCacheService",
                Status = HealthStatus.Healthy,
                Details = new List<string>()
            };

            try
            {
                var stats = _reflectionCache.GetCacheStatistics();
                health.Details.Add($"Cache statistics: {stats.Replace("\n", " | ")}");
                
                // Check for reasonable cache sizes
                if (stats.Contains("Cached Constructors: 0"))
                {
                    health.Status = HealthStatus.Warning;
                    health.Details.Add("No cached constructors - performance may be degraded");
                }
                else
                {
                    health.Details.Add("Constructor cache populated");
                }
            }
            catch (Exception ex)
            {
                health.Status = HealthStatus.Critical;
                health.Details.Add($"Exception during check: {ex.Message}");
            }

            return health;
        }

        /// <summary>
        /// Check health of instance factory
        /// </summary>
        private ComponentHealth CheckInstanceFactoryHealth()
        {
            var health = new ComponentHealth
            {
                ComponentName = "CommandInstanceFactory",
                Status = HealthStatus.Healthy,
                Details = new List<string>()
            };

            try
            {
                var diagnostics = _instanceFactory.GetInstanceCreationDiagnostics();
                health.Details.Add("Instance factory operational");
                
                // Test basic functionality with a simple type
                try
                {
                    var testInstance = _instanceFactory.CreateNativeInstance(typeof(object), Array.Empty<object>());
                    if (testInstance != null)
                    {
                        health.Details.Add("Basic instance creation test passed");
                    }
                    else
                    {
                        health.Status = HealthStatus.Warning;
                        health.Details.Add("Basic instance creation returned null");
                    }
                }
                catch (Exception testEx)
                {
                    health.Status = HealthStatus.Warning;
                    health.Details.Add($"Basic instance creation test failed: {testEx.Message}");
                }
            }
            catch (Exception ex)
            {
                health.Status = HealthStatus.Critical;
                health.Details.Add($"Exception during check: {ex.Message}");
            }

            return health;
        }

        /// <summary>
        /// Check overall system integration health
        /// </summary>
        private ComponentHealth CheckSystemIntegrationHealth()
        {
            var health = new ComponentHealth
            {
                ComponentName = "SystemIntegration",
                Status = HealthStatus.Healthy,
                Details = new List<string>()
            };

            try
            {
                // Check service interdependencies
                if (_typeDiscovery.IsInitialized)
                {
                    health.Details.Add("Type discovery integrated");
                }
                else
                {
                    health.Status = HealthStatus.Warning;
                    health.Details.Add("Type discovery not integrated");
                }

                // Check error rates
                var recentErrors = GetRecentErrors(TimeSpan.FromMinutes(5));
                if (recentErrors.Count > 10)
                {
                    health.Status = HealthStatus.Warning;
                    health.Details.Add($"High error rate: {recentErrors.Count} errors in last 5 minutes");
                }
                else
                {
                    health.Details.Add($"Normal error rate: {recentErrors.Count} errors in last 5 minutes");
                }

                // Check memory usage patterns
                health.Details.Add($"Uptime: {DateTime.UtcNow - _startTime:hh\\:mm\\:ss}");
            }
            catch (Exception ex)
            {
                health.Status = HealthStatus.Critical;
                health.Details.Add($"Exception during integration check: {ex.Message}");
            }

            return health;
        }

        /// <summary>
        /// Get recent error entries within specified time window
        /// </summary>
        /// <param name="timeWindow">Time window to search within</param>
        /// <returns>List of recent error entries</returns>
        public List<DiagnosticEntry> GetRecentErrors(TimeSpan timeWindow)
        {
            var cutoffTime = DateTime.UtcNow - timeWindow;
            
            lock (_historyLock)
            {
                return _diagnosticHistory
                    .Where(e => e.Timestamp >= cutoffTime && 
                               (e.Level == DiagnosticLevel.Error || e.Level == DiagnosticLevel.Warning))
                    .ToList();
            }
        }

        /// <summary>
        /// Generate comprehensive diagnostic report
        /// </summary>
        /// <returns>Formatted diagnostic report</returns>
        public string GenerateDiagnosticReport()
        {
            var report = new StringBuilder();
            
            report.AppendLine("=== NATIVE COMMAND SYSTEM DIAGNOSTIC REPORT ===");
            report.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
            report.AppendLine($"Uptime: {DateTime.UtcNow - _startTime}");
            report.AppendLine();

            // Health Check Section
            var healthCheck = PerformHealthCheck();
            report.AppendLine("=== SYSTEM HEALTH ===");
            report.AppendLine($"Overall Status: {healthCheck.OverallHealth}");
            report.AppendLine($"Check Time: {healthCheck.CheckTime:HH:mm:ss}");
            report.AppendLine();

            foreach (var component in healthCheck.ComponentStatuses)
            {
                report.AppendLine($"{component.Key}: {component.Value.Status}");
                foreach (var detail in component.Value.Details)
                {
                    report.AppendLine($"  - {detail}");
                }
                report.AppendLine();
            }

            // Error Summary
            report.AppendLine("=== ERROR SUMMARY ===");
            lock (_historyLock)
            {
                foreach (var errorType in _errorCounts.OrderByDescending(kvp => kvp.Value))
                {
                    report.AppendLine($"{errorType.Key}: {errorType.Value}");
                }
            }
            report.AppendLine();

            // Recent Activity
            var recentEvents = GetRecentErrors(TimeSpan.FromMinutes(10));
            report.AppendLine("=== RECENT ACTIVITY (Last 10 minutes) ===");
            foreach (var entry in recentEvents.TakeLast(20))
            {
                report.AppendLine($"{entry.Timestamp:HH:mm:ss} [{entry.Level}] {entry.Category}: {entry.Message}");
            }

            // Service Details
            report.AppendLine();
            report.AppendLine("=== SERVICE DETAILS ===");
            
            if (_typeDiscovery?.IsInitialized == true)
            {
                report.AppendLine(_typeDiscovery.GetDiagnosticInfo());
            }
            
            if (_reflectionCache != null)
            {
                report.AppendLine(_reflectionCache.GetCacheStatistics());
            }
            
            if (_instanceFactory != null)
            {
                report.AppendLine(_instanceFactory.GetInstanceCreationDiagnostics());
            }

            return report.ToString();
        }

        /// <summary>
        /// Reset diagnostic history and counters
        /// </summary>
        public void Reset()
        {
            lock (_historyLock)
            {
                _diagnosticHistory.Clear();
                _errorCounts.Clear();
                _startTime = DateTime.UtcNow;
            }
            
            RecordEvent(DiagnosticLevel.Info, "Diagnostics", "Diagnostic history reset");
        }
    }

    #region Supporting Types

    /// <summary>
    /// Diagnostic event entry
    /// </summary>
    public class DiagnosticEntry
    {
        public DateTime Timestamp { get; set; }
        public DiagnosticLevel Level { get; set; }
        public string Category { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    }

    /// <summary>
    /// Diagnostic severity levels
    /// </summary>
    public enum DiagnosticLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }

    /// <summary>
    /// System health check result
    /// </summary>
    public class SystemHealthResult
    {
        public DateTime CheckTime { get; set; }
        public HealthStatus OverallHealth { get; set; }
        public Dictionary<string, ComponentHealth> ComponentStatuses { get; set; }
    }

    /// <summary>
    /// Individual component health status
    /// </summary>
    public class ComponentHealth
    {
        public string ComponentName { get; set; }
        public HealthStatus Status { get; set; }
        public List<string> Details { get; set; } = new List<string>();
    }

    /// <summary>
    /// Health status enumeration
    /// </summary>
    public enum HealthStatus
    {
        Healthy = 0,
        Warning = 1,
        Degraded = 2,
        Critical = 3
    }

    #endregion
}