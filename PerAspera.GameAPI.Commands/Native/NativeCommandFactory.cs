using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Logging;
using PerAspera.Core;
using PerAspera.GameAPI.Commands.Constants;
using PerAspera.GameAPI.Commands.Native.IL2CPPInterop;
using PerAspera.GameAPI.Commands.Native.Services;

namespace PerAspera.GameAPI.Commands.Native
{
    /// <summary>
    /// Factory for creating native Per Aspera command instances with type safety
    /// Refactored into modular services for better maintainability and testability
    /// Uses dependency injection pattern with specialized services for each responsibility
    /// </summary>
    public sealed class NativeCommandFactory
    {
        #region Fields and Properties

        private readonly TypeDiscoveryService _typeDiscovery;
        private readonly ReflectionCacheService _reflectionCache;
        private readonly CommandInstanceFactory _instanceFactory;
        private readonly NativeCommandDiagnostics _diagnostics;
        
        private static readonly object _lockObject = new object();
        private static volatile NativeCommandFactory _instance;
        private volatile bool _isInitialized = false;
        
        /// <summary>
        /// Thread-safe singleton instance with double-checked locking pattern
        /// </summary>
        public static NativeCommandFactory Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                            _instance = new NativeCommandFactory();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Check if factory has been initialized and has command types available
        /// </summary>
        public bool IsInitialized => _isInitialized && _typeDiscovery.IsInitialized;

        #endregion

        #region Constructor and Initialization

        /// <summary>
        /// Private constructor implementing singleton pattern
        /// Initializes all service dependencies in correct order
        /// </summary>
        private NativeCommandFactory()
        {
            try
            {
                // Initialize services in dependency order
                _typeDiscovery = new TypeDiscoveryService();
                _reflectionCache = new ReflectionCacheService();
                _instanceFactory = new CommandInstanceFactory(_reflectionCache);
                _diagnostics = new NativeCommandDiagnostics(_typeDiscovery, _reflectionCache, _instanceFactory);
                
                // Initialize the discovery and caching systems
                InitializeServices();
                
                LogAspera.Info($"NativeCommandFactory initialized with modular services");
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Failed to initialize NativeCommandFactory: {ex.Message}");
                _isInitialized = false;
            }
        }

        /// <summary>
        /// Initialize all services in correct dependency order
        /// </summary>
        private void InitializeServices()
        {
            _diagnostics.RecordEvent(DiagnosticLevel.Info, "Initialization", "Starting service initialization");
            
            // 1. Initialize type discovery first
            _typeDiscovery.InitializeCommandTypes();
            
            // 2. Build reflection caches using discovered types
            var commandTypes = new ConcurrentDictionary<string, System.Type>(StringComparer.OrdinalIgnoreCase);
            foreach (var typeName in _typeDiscovery.GetAvailableCommandTypes())
            {
                if (_typeDiscovery.TryGetCommandType(typeName, out var type))
                {
                    commandTypes.TryAdd(typeName, type);
                }
            }
            
            _reflectionCache.CacheConstructors(commandTypes);
            
            _diagnostics.RecordEvent(DiagnosticLevel.Info, "Initialization", 
                $"Services initialized successfully: {commandTypes.Count} types discovered");
        }

        #endregion

        #region Public Factory Methods

        /// <summary>
        /// Create native command instance by type name with enhanced error handling
        /// Uses the modular service architecture for optimal performance
        /// </summary>
        /// <param name="commandTypeName">Command type name (e.g., "CmdImportResource" or "ImportResource")</param>
        /// <param name="parameters">Constructor parameters</param>
        /// <returns>Wrapped native command instance or null on failure</returns>
        public CommandBaseWrapper CreateCommand(string commandTypeName, params object[] parameters)
        {
            if (string.IsNullOrWhiteSpace(commandTypeName))
            {
                _diagnostics.RecordEvent(DiagnosticLevel.Warning, "CreateCommand", 
                    "Cannot create command with null or empty type name");
                return null;
            }

            try
            {
                _diagnostics.RecordEvent(DiagnosticLevel.Debug, "CreateCommand", 
                    $"Creating command: {commandTypeName} with {parameters?.Length ?? 0} parameters");

                // Use type discovery service to find the command type
                if (!_typeDiscovery.TryGetCommandType(commandTypeName, out var commandType))
                {
                    _diagnostics.RecordEvent(DiagnosticLevel.Warning, "CreateCommand", 
                        $"Command type not found: '{commandTypeName}'");
                    return _instanceFactory.CreateFallbackCommand(commandTypeName, parameters);
                }
                
                // Use instance factory to create the command
                var nativeInstance = _instanceFactory.CreateNativeInstance(commandType, parameters);
                if (nativeInstance == null)
                {
                    _diagnostics.RecordEvent(DiagnosticLevel.Error, "CreateCommand", 
                        $"Failed to create native instance of {commandType.Name}");
                    return null;
                }
                
                // Wrap and validate the created command
                var wrapper = new CommandBaseWrapper(nativeInstance);
                if (_instanceFactory.ValidateCommand(wrapper))
                {
                    _diagnostics.RecordEvent(DiagnosticLevel.Debug, "CreateCommand", 
                        $"Successfully created command: {wrapper.CommandName}");
                    return wrapper;
                }
                
                _diagnostics.RecordEvent(DiagnosticLevel.Warning, "CreateCommand", 
                    $"Command validation failed for {wrapper.CommandName}");
                return null;
            }
            catch (Exception ex)
            {
                _diagnostics.RecordEvent(DiagnosticLevel.Error, "CreateCommand", 
                    $"Failed to create command '{commandTypeName}': {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create typed native command instance with compile-time type safety
        /// </summary>
        /// <typeparam name="T">Native command type</typeparam>
        /// <param name="parameters">Constructor parameters</param>
        /// <returns>Wrapped native command instance or null on failure</returns>
        public CommandBaseWrapper CreateCommand<T>(params object[] parameters) where T : class
        {
            try
            {
                var commandType = typeof(T);
                var nativeInstance = _instanceFactory.CreateNativeInstance(commandType, parameters);
                
                if (nativeInstance == null)
                {
                    _diagnostics.RecordEvent(DiagnosticLevel.Error, "CreateCommand<T>", 
                        $"Failed to create native instance of {commandType.Name}");
                    return null;
                }
                
                var wrapper = new CommandBaseWrapper(nativeInstance);
                _diagnostics.RecordEvent(DiagnosticLevel.Debug, "CreateCommand<T>", 
                    $"Created typed command: {wrapper.CommandName}");
                return wrapper;
            }
            catch (Exception ex)
            {
                _diagnostics.RecordEvent(DiagnosticLevel.Error, "CreateCommand<T>", 
                    $"Failed to create typed command {typeof(T).Name}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create ImportResource command with MVP parameters (specialized factory method)
        /// Enhanced version with parameter validation and type checking
        /// </summary>
        /// <param name="resourceName">Resource name (e.g., "water", "carbon")</param>
        /// <param name="amount">Amount to import (must be > 0)</param>
        /// <returns>Configured CommandWrapper or null on failure</returns>
        public CommandBaseWrapper CreateImportResourceCommand(string resourceName, float amount)
        {
            try
            {
                _diagnostics.RecordEvent(DiagnosticLevel.Info, "ImportResource", 
                    $"Creating ImportResource command: {resourceName} x {amount}");

                return _instanceFactory.CreateImportResourceCommand(resourceName, amount);
            }
            catch (Exception ex)
            {
                _diagnostics.RecordEvent(DiagnosticLevel.Error, "ImportResource", 
                    $"Error creating ImportResource command: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Command Type Management

        /// <summary>
        /// Check if command type is available in the factory
        /// Uses the type discovery service for accurate lookup
        /// </summary>
        /// <param name="commandTypeName">Command type name</param>
        /// <returns>True if command type is available</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCommandTypeAvailable(string commandTypeName)
        {
            if (string.IsNullOrWhiteSpace(commandTypeName))
                return false;
                
            return _typeDiscovery.TryGetCommandType(commandTypeName, out _);
        }

        /// <summary>
        /// Get all available command type names from the discovery service
        /// </summary>
        /// <returns>Array of command type names</returns>
        public string[] GetAvailableCommandTypes()
        {
            return _typeDiscovery.GetAvailableCommandTypes();
        }

        /// <summary>
        /// Get native type for command type name using the discovery service
        /// </summary>
        /// <param name="commandTypeName">Command type name</param>
        /// <returns>Native Type or null if not found</returns>
        public System.Type GetNativeType(string commandTypeName)
        {
            if (string.IsNullOrWhiteSpace(commandTypeName))
                return null;
                
            _typeDiscovery.TryGetCommandType(commandTypeName, out var type);
            return type;
        }

        #endregion

        #region Diagnostics and Health Monitoring

        /// <summary>
        /// Get comprehensive diagnostic information about the factory state
        /// Uses the dedicated diagnostics service for detailed reporting
        /// </summary>
        /// <returns>Detailed diagnostic information</returns>
        public string GetDiagnosticInfo()
        {
            try
            {
                return _diagnostics.GenerateDiagnosticReport();
            }
            catch (Exception ex)
            {
                return $"Error generating diagnostic info: {ex.Message}";
            }
        }

        /// <summary>
        /// Perform comprehensive system health check
        /// Returns detailed health status for monitoring and debugging
        /// </summary>
        /// <returns>System health result with component details</returns>
        public SystemHealthResult PerformHealthCheck()
        {
            return _diagnostics.PerformHealthCheck();
        }

        /// <summary>
        /// Get recent error events for monitoring and debugging
        /// </summary>
        /// <param name="timeWindow">Time window to search within</param>
        /// <returns>List of recent error entries</returns>
        public List<DiagnosticEntry> GetRecentErrors(TimeSpan timeWindow)
        {
            return _diagnostics.GetRecentErrors(timeWindow);
        }

        #endregion

        #region Service Access (for advanced users)

        /// <summary>
        /// Get access to the type discovery service for advanced operations
        /// </summary>
        /// <returns>Type discovery service instance</returns>
        public TypeDiscoveryService GetTypeDiscoveryService()
        {
            return _typeDiscovery;
        }

        /// <summary>
        /// Get access to the reflection cache service for performance monitoring
        /// </summary>
        /// <returns>Reflection cache service instance</returns>
        public ReflectionCacheService GetReflectionCacheService()
        {
            return _reflectionCache;
        }

        /// <summary>
        /// Get access to the instance factory for advanced creation patterns
        /// </summary>
        /// <returns>Command instance factory</returns>
        public CommandInstanceFactory GetInstanceFactory()
        {
            return _instanceFactory;
        }

        /// <summary>
        /// Get access to the diagnostics service for monitoring and logging
        /// </summary>
        /// <returns>Diagnostics service instance</returns>
        public NativeCommandDiagnostics GetDiagnosticsService()
        {
            return _diagnostics;
        }

        #endregion

        #region Testing and Reset

        /// <summary>
        /// Reset factory state (for testing purposes only)
        /// Clears all cached data and reinitializes services
        /// </summary>
        internal static void Reset()
        {
            lock (_lockObject)
            {
                if (_instance != null)
                {
                    try
                    {
                        _instance._diagnostics?.Reset();
                        _instance._reflectionCache?.ClearCaches();
                        _instance._diagnostics?.RecordEvent(DiagnosticLevel.Info, "Reset", "Factory state reset");
                    }
                    catch (Exception ex)
                    {
                        LogAspera.Warning($"Error during factory reset: {ex.Message}");
                    }
                    finally
                    {
                        _instance = null;
                    }
                }
            }
        }

        /// <summary>
        /// Reinitialize services without recreating the singleton
        /// Useful for recovering from errors or updating configurations
        /// </summary>
        public void Reinitialize()
        {
            try
            {
                _diagnostics.RecordEvent(DiagnosticLevel.Info, "Reinitialize", "Starting factory reinitialization");
                
                _isInitialized = false;
                InitializeServices();
                _isInitialized = true;
                
                _diagnostics.RecordEvent(DiagnosticLevel.Info, "Reinitialize", "Factory reinitialized successfully");
            }
            catch (Exception ex)
            {
                _diagnostics.RecordEvent(DiagnosticLevel.Error, "Reinitialize", $"Reinitialization failed: {ex.Message}");
                _isInitialized = false;
                throw;
            }
        }

        #endregion
    }
}