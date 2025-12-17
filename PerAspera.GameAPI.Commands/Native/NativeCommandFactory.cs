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

namespace PerAspera.GameAPI.Commands.Native
{
    /// <summary>
    /// Factory for creating native Per Aspera command instances with type safety
    /// Uses reflection and IL2CPP interop to create proper native command objects
    /// Implements performance optimizations and caching following BepInX 6 best practices
    /// </summary>
    public sealed class NativeCommandFactory
    {
        #region Fields and Properties

        private readonly ConcurrentDictionary<string, System.Type> _commandTypes;
        private readonly ConcurrentDictionary<string, ConstructorInfo> _constructorCache;
        private readonly ConcurrentDictionary<System.Type, MethodInfo[]> _methodCache;
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
        public bool IsInitialized => _isInitialized && _commandTypes.Count > 0;

        #endregion

        #region Constructor and Initialization

        /// <summary>
        /// Private constructor implementing singleton pattern
        /// Initializes thread-safe collections and performs type discovery
        /// </summary>
        private NativeCommandFactory()
        {
            // Use concurrent collections for thread safety
            _commandTypes = new ConcurrentDictionary<string, System.Type>(StringComparer.OrdinalIgnoreCase);
            _constructorCache = new ConcurrentDictionary<string, ConstructorInfo>(StringComparer.OrdinalIgnoreCase);
            _methodCache = new ConcurrentDictionary<System.Type, MethodInfo[]>();
            
            try
            {
                InitializeCommandTypes();
                LogAspera.Info($"NativeCommandFactory initialized with {_commandTypes.Count} command types");
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Failed to initialize NativeCommandFactory: {ex.Message}");
                _isInitialized = false;
            }
        }

        /// <summary>
        /// Initialize command type discovery using GameTypeInitializer integration
        /// Follows BepInX 6 patterns for IL2CPP type discovery
        /// </summary>
        private void InitializeCommandTypes()
        {
            try
            {
                LogAspera.Info("Initializing command type discovery...");
                
                // Initialize GameTypeInitializer for enhanced type access
                GameTypeInitializer.Initialize();
                
                // Scan assemblies for command types
                ScanAssembliesForCommandTypes();
                
                // Build constructor cache for performance
                CacheConstructors();
                
                LogAspera.Info($"Command type discovery complete: {_commandTypes.Count} types found");
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Error during command type initialization: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Public Factory Methods

        /// <summary>
        /// Create native command instance by type name with enhanced error handling
        /// </summary>
        /// <param name="commandTypeName">Command type name (e.g., "CmdImportResource" or "ImportResource")</param>
        /// <param name="parameters">Constructor parameters</param>
        /// <returns>Wrapped native command instance or null on failure</returns>
        public CommandBaseWrapper CreateCommand(string commandTypeName, params object[] parameters)
        {
            if (string.IsNullOrWhiteSpace(commandTypeName))
            {
                LogAspera.Warning("Cannot create command with null or empty type name");
                return null;
            }

            try
            {
                // Normalize command type name for consistent lookup
                var normalizedName = NormalizeCommandTypeName(commandTypeName);
                
                if (!_commandTypes.TryGetValue(normalizedName, out var commandType))
                {
                    LogAspera.Warning($"Command type not found: '{commandTypeName}' (normalized: '{normalizedName}')");
                    return CreateFallbackCommand(commandTypeName, parameters);
                }
                
                // Create native instance using cached constructors when possible
                var nativeCommand = CreateNativeInstance(commandType, parameters);
                if (nativeCommand == null)
                {
                    LogAspera.Error($"Failed to create native instance of {commandType.Name}");
                    return null;
                }
                
                // Wrap and validate the created command
                var wrapper = new CommandBaseWrapper(nativeCommand);
                LogAspera.Debug($"Created command: {wrapper.CommandName}");
                return wrapper;
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Failed to create command '{commandTypeName}': {ex.Message}");
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
                var nativeCommand = CreateNativeInstance(commandType, parameters);
                
                if (nativeCommand == null)
                {
                    LogAspera.Error($"Failed to create native instance of {commandType.Name}");
                    return null;
                }
                
                var wrapper = new CommandBaseWrapper(nativeCommand);
                LogAspera.Debug($"Created typed command: {wrapper.CommandName}");
                return wrapper;
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Failed to create typed command {typeof(T).Name}: {ex.Message}");
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
            if (string.IsNullOrWhiteSpace(resourceName))
            {
                LogAspera.Warning("Cannot create ImportResource command with null or empty resource name");
                return null;
            }

            if (amount <= 0)
            {
                LogAspera.Warning($"Cannot create ImportResource command with invalid amount: {amount}");
                return null;
            }

            try
            {
                LogAspera.Debug($"Creating ImportResource command: {resourceName} x {amount}");

                // Try multiple naming patterns for ImportResource command
                var commandTypes = new[] { "ImportResource", "CmdImportResource", "ImportResourceCommand" };
                
                foreach (var cmdType in commandTypes)
                {
                    var commandWrapper = CreateCommand(cmdType, resourceName, amount);
                    if (commandWrapper != null)
                    {
                        LogAspera.Info($"Successfully created ImportResource command: {resourceName} x {amount}");
                        return commandWrapper;
                    }
                }

                LogAspera.Error($"Failed to create ImportResource command for {resourceName} - no matching command type found");
                return null;
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Error creating ImportResource command: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Command Type Management

        /// <summary>
        /// Check if command type is available in the factory
        /// </summary>
        /// <param name="commandTypeName">Command type name</param>
        /// <returns>True if command type is available</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsCommandTypeAvailable(string commandTypeName)
        {
            if (string.IsNullOrWhiteSpace(commandTypeName))
                return false;
                
            var normalizedName = NormalizeCommandTypeName(commandTypeName);
            return _commandTypes.ContainsKey(normalizedName);
        }

        /// <summary>
        /// Get all available command type names
        /// </summary>
        /// <returns>Array of command type names</returns>
        public string[] GetAvailableCommandTypes()
        {
            return _commandTypes.Keys.ToArray();
        }

        /// <summary>
        /// Get native type for command type name
        /// </summary>
        /// <param name="commandTypeName">Command type name</param>
        /// <returns>Native Type or null if not found</returns>
        public System.Type GetNativeType(string commandTypeName)
        {
            if (string.IsNullOrWhiteSpace(commandTypeName))
                return null;
                
            var normalizedName = NormalizeCommandTypeName(commandTypeName);
            _commandTypes.TryGetValue(normalizedName, out var type);
            return type;
        }

        #endregion

        #region Type Discovery and Scanning

        /// <summary>
        /// Scan assemblies for command types with priority handling
        /// Uses GameTypeInitializer for enhanced Assembly-CSharp discovery
        /// </summary>
        private void ScanAssembliesForCommandTypes()
        {
            try
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                
                // Priority scan: Assembly-CSharp (Per Aspera game assembly)
                var gameAssembly = assemblies.FirstOrDefault(a => 
                    a.GetName().Name.Equals("Assembly-CSharp", StringComparison.OrdinalIgnoreCase));
                
                if (gameAssembly != null)
                {
                    LogAspera.Info("Scanning game assembly (Assembly-CSharp) for command types...");
                    ScanAssemblyForCommandTypes(gameAssembly, isPriorityAssembly: true);
                }
                
                // Scan other relevant assemblies
                foreach (var assembly in assemblies)
                {
                    if (assembly == gameAssembly) continue;
                    
                    // Skip system assemblies for performance
                    var assemblyName = assembly.GetName().Name;
                    if (IsSystemAssembly(assemblyName)) continue;
                    
                    ScanAssemblyForCommandTypes(assembly, isPriorityAssembly: false);
                }
                
                LogAspera.Info($"Assembly scanning complete: {_commandTypes.Count} command types discovered");
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Error during assembly scanning: {ex.Message}");
            }
        }

        /// <summary>
        /// Scan a single assembly for command types with error handling
        /// </summary>
        /// <param name="assembly">Assembly to scan</param>
        /// <param name="isPriorityAssembly">Whether this is a high-priority assembly</param>
        private void ScanAssemblyForCommandTypes(Assembly assembly, bool isPriorityAssembly)
        {
            try
            {
                var commandTypes = assembly.GetTypes()
                    .Where(IsCommandType)
                    .ToArray();
                    
                foreach (var type in commandTypes)
                {
                    RegisterCommandType(type);
                }
                
                if (commandTypes.Length > 0)
                {
                    var priority = isPriorityAssembly ? " (priority)" : "";
                    LogAspera.Debug($"Found {commandTypes.Length} command types in {assembly.GetName().Name}{priority}");
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                LogAspera.Debug($"Could not load all types from {assembly.GetName().Name}: {ex.Message}");
                
                // Process successfully loaded types
                var loadedTypes = ex.Types?.Where(t => t != null) ?? Enumerable.Empty<System.Type>();
                foreach (var type in loadedTypes.Where(IsCommandType))
                {
                    RegisterCommandType(type);
                }
            }
            catch (Exception ex)
            {
                LogAspera.Debug($"Error scanning assembly {assembly.GetName().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Determine if a type represents a command class
        /// Enhanced pattern matching for Per Aspera command types
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns>True if type appears to be a command</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsCommandType(System.Type type)
        {
            if (type == null || type.IsAbstract || type.IsInterface || type.IsGenericTypeDefinition)
                return false;

            var typeName = type.Name;
            
            // Check naming patterns
            if (typeName.StartsWith("Cmd", StringComparison.OrdinalIgnoreCase) ||
                typeName.EndsWith("Command", StringComparison.OrdinalIgnoreCase))
                return true;
                
            // Check inheritance
            var baseType = type.BaseType;
            while (baseType != null)
            {
                if (baseType.Name.Contains("Command") || baseType.Name.Contains("Cmd"))
                    return true;
                baseType = baseType.BaseType;
            }
            
            // Check interfaces
            return type.GetInterfaces().Any(i => i.Name.Contains("Command"));
        }

        /// <summary>
        /// Register a command type in the factory with thread safety
        /// </summary>
        /// <param name="commandType">Command type to register</param>
        private void RegisterCommandType(System.Type commandType)
        {
            try
            {
                var normalizedName = NormalizeCommandTypeName(commandType.Name);
                
                // Use TryAdd for thread safety
                if (_commandTypes.TryAdd(normalizedName, commandType))
                {
                    LogAspera.Debug($"Registered command type: {normalizedName} -> {commandType.FullName}");
                }
            }
            catch (Exception ex)
            {
                LogAspera.Warning($"Failed to register command type {commandType.Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if assembly name represents a system assembly
        /// </summary>
        /// <param name="assemblyName">Assembly name to check</param>
        /// <returns>True if it's a system assembly</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsSystemAssembly(string assemblyName)
        {
            return assemblyName.StartsWith("System.") ||
                   assemblyName.StartsWith("Microsoft.") ||
                   assemblyName.StartsWith("mscorlib") ||
                   assemblyName.StartsWith("netstandard") ||
                   assemblyName.StartsWith("Unity.");
        }

        #endregion

        #region Constructor Caching and Instance Creation

        /// <summary>
        /// Cache constructors for all registered command types for performance
        /// </summary>
        private void CacheConstructors()
        {
            try
            {
                foreach (var kvp in _commandTypes)
                {
                    var commandType = kvp.Value;
                    var constructor = GetBestConstructor(commandType);
                    
                    if (constructor != null)
                    {
                        _constructorCache.TryAdd(kvp.Key, constructor);
                    }
                }
                
                LogAspera.Debug($"Constructor caching complete: {_constructorCache.Count} constructors cached");
            }
            catch (Exception ex)
            {
                LogAspera.Warning($"Error during constructor caching: {ex.Message}");
            }
        }

        /// <summary>
        /// Create native command instance using reflection with performance optimizations
        /// </summary>
        /// <param name="commandType">Command type to instantiate</param>
        /// <param name="parameters">Constructor parameters</param>
        /// <returns>Native command instance or null</returns>
        private object CreateNativeInstance(System.Type commandType, object[] parameters)
        {
            try
            {
                // Try cached constructor first
                var normalizedName = NormalizeCommandTypeName(commandType.Name);
                
                if (_constructorCache.TryGetValue(normalizedName, out var cachedConstructor))
                {
                    return CreateInstanceWithConstructor(cachedConstructor, parameters);
                }
                
                // Fallback to reflection
                var constructor = GetBestConstructor(commandType);
                if (constructor != null)
                {
                    // Cache for future use
                    _constructorCache.TryAdd(normalizedName, constructor);
                    return CreateInstanceWithConstructor(constructor, parameters);
                }
                
                // Last resort: Activator.CreateInstance
                return parameters == null || parameters.Length == 0 
                    ? Activator.CreateInstance(commandType)
                    : Activator.CreateInstance(commandType, parameters);
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Failed to create instance of {commandType.Name}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create instance using a specific constructor with parameter validation
        /// </summary>
        /// <param name="constructor">Constructor to use</param>
        /// <param name="parameters">Parameters to pass</param>
        /// <returns>Created instance or null</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private object CreateInstanceWithConstructor(ConstructorInfo constructor, object[] parameters)
        {
            try
            {
                var paramTypes = constructor.GetParameters();
                
                // Validate parameter count
                if (parameters?.Length != paramTypes.Length)
                {
                    // Try to adapt parameters
                    parameters = AdaptParameters(parameters, paramTypes);
                }
                
                return constructor.Invoke(parameters ?? new object[0]);
            }
            catch (Exception ex)
            {
                LogAspera.Debug($"Constructor invocation failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Find the best constructor for a command type
        /// Prioritizes constructors with matching parameter counts
        /// </summary>
        /// <param name="commandType">Command type</param>
        /// <returns>Best constructor or null</returns>
        private static ConstructorInfo GetBestConstructor(System.Type commandType)
        {
            try
            {
                var constructors = commandType.GetConstructors()
                    .OrderBy(c => c.GetParameters().Length)
                    .ToArray();
                
                // Prefer public constructors
                return constructors.FirstOrDefault(c => c.IsPublic) ?? constructors.FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Adapt parameters to match constructor requirements
        /// </summary>
        /// <param name="provided">Provided parameters</param>
        /// <param name="required">Required parameter types</param>
        /// <returns>Adapted parameters</returns>
        private static object[] AdaptParameters(object[] provided, ParameterInfo[] required)
        {
            if (required.Length == 0)
                return new object[0];
                
            var adapted = new object[required.Length];
            
            for (int i = 0; i < required.Length; i++)
            {
                if (provided != null && i < provided.Length)
                {
                    adapted[i] = provided[i];
                }
                else
                {
                    // Use default value or null
                    adapted[i] = required[i].HasDefaultValue ? required[i].DefaultValue : null;
                }
            }
            
            return adapted;
        }

        #endregion

        #region Fallback and Utility Methods

        /// <summary>
        /// Attempt to create command using alternative discovery methods
        /// </summary>
        /// <param name="commandTypeName">Original command type name</param>
        /// <param name="parameters">Constructor parameters</param>
        /// <returns>Command wrapper or null</returns>
        private CommandBaseWrapper CreateFallbackCommand(string commandTypeName, object[] parameters)
        {
            try
            {
                // Try different naming variations
                var variations = new[]
                {
                    commandTypeName,
                    $"Cmd{commandTypeName}",
                    $"{commandTypeName}Command",
                    commandTypeName.Replace("Command", "").Replace("Cmd", "")
                };
                
                foreach (var variation in variations.Distinct())
                {
                    if (TryCreateByFullSearch(variation, parameters, out var wrapper))
                    {
                        LogAspera.Info($"Created command using fallback method: {variation}");
                        return wrapper;
                    }
                }
                
                LogAspera.Debug($"All fallback methods failed for command type: {commandTypeName}");
                return null;
            }
            catch (Exception ex)
            {
                LogAspera.Warning($"Error in fallback command creation: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Try to create command by searching all assemblies
        /// </summary>
        /// <param name="typeName">Type name to search for</param>
        /// <param name="parameters">Constructor parameters</param>
        /// <param name="wrapper">Output wrapper</param>
        /// <returns>True if successful</returns>
        private bool TryCreateByFullSearch(string typeName, object[] parameters, out CommandBaseWrapper wrapper)
        {
            wrapper = null;
            
            try
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var type = assembly.GetTypes()
                        .FirstOrDefault(t => t.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
                    
                    if (type != null && IsCommandType(type))
                    {
                        var instance = CreateNativeInstance(type, parameters);
                        if (instance != null)
                        {
                            wrapper = new CommandBaseWrapper(instance);
                            
                            // Register for future use
                            RegisterCommandType(type);
                            return true;
                        }
                    }
                }
                
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Normalize command type name for consistent lookup
        /// Enhanced normalization with multiple pattern support
        /// </summary>
        /// <param name="commandTypeName">Raw command type name</param>
        /// <returns>Normalized name</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static string NormalizeCommandTypeName(string commandTypeName)
        {
            if (string.IsNullOrWhiteSpace(commandTypeName))
                return commandTypeName ?? string.Empty;
            
            // Remove common prefixes/suffixes and normalize to CmdXxx format
            var normalized = commandTypeName;
            
            if (normalized.EndsWith("Command", StringComparison.OrdinalIgnoreCase))
                normalized = normalized.Substring(0, normalized.Length - 7);
                
            if (!normalized.StartsWith("Cmd", StringComparison.OrdinalIgnoreCase))
                normalized = "Cmd" + normalized;
                
            return normalized;
        }

        #endregion

        #region Diagnostics and Debugging

        /// <summary>
        /// Get comprehensive diagnostic information about the factory state
        /// Enhanced diagnostics for debugging and monitoring
        /// </summary>
        /// <returns>Detailed diagnostic information</returns>
        public string GetDiagnosticInfo()
        {
            try
            {
                var info = new System.Text.StringBuilder();
                
                // Header and status
                info.AppendLine("=== NativeCommandFactory Diagnostics ===");
                info.AppendLine($"Initialized: {_isInitialized}");
                info.AppendLine($"Command Types: {_commandTypes.Count}");
                info.AppendLine($"Cached Constructors: {_constructorCache.Count}");
                info.AppendLine($"Method Cache: {_methodCache.Count}");
                info.AppendLine();
                
                // GameTypeInitializer integration status
                info.AppendLine("GameTypeInitializer Integration:");
                info.AppendLine($"  BaseGame Available: {GameTypeInitializer.GetBaseGameType() != null}");
                info.AppendLine($"  CommandBus Available: {GameTypeInitializer.GetCommandBusType() != null}");
                info.AppendLine();
                
                // Command type details
                info.AppendLine("Discovered Command Types:");
                var sortedTypes = _commandTypes.OrderBy(kvp => kvp.Key);
                foreach (var kvp in sortedTypes)
                {
                    var hasCachedConstructor = _constructorCache.ContainsKey(kvp.Key);
                    var constructorMark = hasCachedConstructor ? "✓" : "✗";
                    info.AppendLine($"  {constructorMark} {kvp.Key} → {kvp.Value.FullName}");
                }
                
                // Performance statistics
                info.AppendLine();
                info.AppendLine("Performance Statistics:");
                info.AppendLine($"  Cache Hit Rate: {CalculateCacheHitRate():P1}");
                info.AppendLine($"  Memory Usage: ~{EstimateMemoryUsage():N0} bytes");
                
                return info.ToString();
            }
            catch (Exception ex)
            {
                return $"Error generating diagnostic info: {ex.Message}";
            }
        }

        /// <summary>
        /// Calculate cache hit rate for performance monitoring
        /// </summary>
        /// <returns>Cache hit rate as decimal</returns>
        private double CalculateCacheHitRate()
        {
            try
            {
                var totalTypes = _commandTypes.Count;
                var cachedTypes = _constructorCache.Count;
                return totalTypes > 0 ? (double)cachedTypes / totalTypes : 0.0;
            }
            catch
            {
                return 0.0;
            }
        }

        /// <summary>
        /// Estimate memory usage for monitoring
        /// </summary>
        /// <returns>Estimated memory usage in bytes</returns>
        private long EstimateMemoryUsage()
        {
            try
            {
                // Rough estimation based on collection sizes
                const int averageStringSize = 50;
                const int objectReferenceSize = 8;
                
                var stringMemory = _commandTypes.Count * averageStringSize * 2; // Key + Type name
                var referenceMemory = (_commandTypes.Count + _constructorCache.Count + _methodCache.Count) * objectReferenceSize;
                
                return stringMemory + referenceMemory;
            }
            catch
            {
                return 0;
            }
        }

        #endregion

        #region Testing and Reset

        /// <summary>
        /// Reset factory state (for testing purposes only)
        /// </summary>
        internal static void Reset()
        {
            lock (_lockObject)
            {
                _instance = null;
            }
        }

        #endregion
    }
}