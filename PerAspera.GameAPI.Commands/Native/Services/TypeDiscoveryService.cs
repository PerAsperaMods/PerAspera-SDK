using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using PerAspera.Core;
using PerAspera.GameAPI.Commands.Constants;

namespace PerAspera.GameAPI.Commands.Native.Services
{
    /// <summary>
    /// Service responsible for discovering command types from assemblies using IL2CPP-compatible reflection
    /// Provides thread-safe scanning and caching of command types with GameTypeInitializer integration
    /// </summary>
    public sealed class TypeDiscoveryService
    {
        private readonly ConcurrentDictionary<string, System.Type> _commandTypes;
        private volatile bool _isInitialized = false;

        public TypeDiscoveryService()
        {
            _commandTypes = new ConcurrentDictionary<string, System.Type>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Initialize command type discovery using GameTypeInitializer integration
        /// Follows BepInX 6 patterns for IL2CPP type discovery
        /// </summary>
        public void InitializeCommandTypes()
        {
            try
            {
                LogAspera.Info("Initializing command type discovery...");
                
                // Initialize GameTypeInitializer for enhanced type access
                GameTypeInitializer.Initialize();
                
                // Scan assemblies for command types
                ScanAssembliesForCommandTypes();
                
                LogAspera.Info($"Command type discovery complete: {_commandTypes.Count} types found");
                _isInitialized = true;
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Error during command type initialization: {ex.Message}");
                _isInitialized = false;
                throw;
            }
        }

        /// <summary>
        /// Scan all available assemblies for command types using enhanced IL2CPP patterns
        /// Prioritizes game assemblies and uses efficient reflection caching
        /// </summary>
        private void ScanAssembliesForCommandTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                .ToArray();

            LogAspera.Debug($"Scanning {assemblies.Length} assemblies for command types");

            // Priority scan: Focus on Assembly-CSharp (Per Aspera game assembly)
            var gameAssembly = assemblies.FirstOrDefault(a => a.GetName().Name == "Assembly-CSharp");
            if (gameAssembly != null)
            {
                LogAspera.Debug("Scanning priority assembly: Assembly-CSharp");
                ScanAssemblyForCommands(gameAssembly, isPriorityAssembly: true);
            }

            // Scan other relevant assemblies
            var relevantAssemblies = assemblies.Where(a => 
                a != gameAssembly && 
                (a.GetName().Name.Contains("PerAspera") || 
                 a.GetName().Name.Contains("Command") ||
                 a.GetName().Name == "Assembly-CSharp-firstpass"))
                .ToArray();

            foreach (var assembly in relevantAssemblies)
            {
                try
                {
                    LogAspera.Debug($"Scanning assembly: {assembly.GetName().Name}");
                    ScanAssemblyForCommands(assembly, isPriorityAssembly: false);
                }
                catch (Exception ex)
                {
                    LogAspera.Warning($"Failed to scan assembly {assembly.GetName().Name}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Scan individual assembly for command types with pattern matching and validation
        /// </summary>
        /// <param name="assembly">Assembly to scan</param>
        /// <param name="isPriorityAssembly">Whether this is a priority assembly for enhanced logging</param>
        private void ScanAssemblyForCommands(Assembly assembly, bool isPriorityAssembly)
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(IsCommandType)
                    .ToArray();

                if (isPriorityAssembly)
                {
                    LogAspera.Info($"Found {types.Length} command types in {assembly.GetName().Name}");
                }

                foreach (var type in types)
                {
                    RegisterCommandType(type);
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                LogAspera.Warning($"Partial type loading from assembly {assembly.GetName().Name}: {ex.LoaderExceptions.Length} exceptions");
                
                // Process types that loaded successfully
                var loadedTypes = ex.Types.Where(t => t != null && IsCommandType(t));
                foreach (var type in loadedTypes)
                {
                    RegisterCommandType(type);
                }
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Failed to scan assembly {assembly.GetName().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if a type is a command type based on naming patterns and inheritance
        /// Enhanced pattern matching for Per Aspera command detection
        /// </summary>
        /// <param name="type">Type to evaluate</param>
        /// <returns>True if the type appears to be a command type</returns>
        private static bool IsCommandType(System.Type type)
        {
            if (type == null || type.IsAbstract || type.IsInterface)
                return false;

            var typeName = type.Name;

            // Common Per Aspera command patterns
            if (typeName.StartsWith("Cmd") && !typeName.EndsWith("Base"))
                return true;

            if (typeName.EndsWith("Command") && !typeName.EndsWith("BaseCommand"))
                return true;

            // Check for command-related interfaces or base classes
            var interfaces = type.GetInterfaces().Select(i => i.Name);
            if (interfaces.Any(i => i.Contains("Command") || i.Contains("ICommand")))
                return true;

            // Check inheritance hierarchy for command base classes
            var baseType = type.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                if (baseType.Name.Contains("Command") && !baseType.Name.Contains("Base"))
                    return true;
                baseType = baseType.BaseType;
            }

            return false;
        }

        /// <summary>
        /// Register a command type with multiple naming variations for flexible lookup
        /// </summary>
        /// <param name="type">Command type to register</param>
        private void RegisterCommandType(System.Type type)
        {
            var typeName = type.Name;

            // Register with full type name
            _commandTypes.TryAdd(typeName, type);

            // Register with "Cmd" prefix removed if present
            if (typeName.StartsWith("Cmd"))
            {
                var shortName = typeName.Substring(3);
                _commandTypes.TryAdd(shortName, type);
            }

            // Register with "Command" suffix removed if present
            if (typeName.EndsWith("Command"))
            {
                var shortName = typeName.Substring(0, typeName.Length - 7);
                _commandTypes.TryAdd(shortName, type);
            }

            LogAspera.Debug($"Registered command type: {typeName} -> {type.FullName}");
        }

        /// <summary>
        /// Normalize command type name for consistent lookup
        /// </summary>
        /// <param name="typeName">Raw command type name</param>
        /// <returns>Normalized type name for lookup</returns>
        public string NormalizeCommandTypeName(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return string.Empty;

            // Remove common prefixes/suffixes for flexible matching
            var normalized = typeName.Trim();

            // Try exact match first
            if (_commandTypes.ContainsKey(normalized))
                return normalized;

            // Try with "Cmd" prefix
            var withCmdPrefix = "Cmd" + normalized;
            if (_commandTypes.ContainsKey(withCmdPrefix))
                return withCmdPrefix;

            // Try with "Command" suffix  
            var withCommandSuffix = normalized + "Command";
            if (_commandTypes.ContainsKey(withCommandSuffix))
                return withCommandSuffix;

            // Return original if no matches found
            return normalized;
        }

        /// <summary>
        /// Get available command types discovered during initialization
        /// </summary>
        /// <returns>Array of command type names</returns>
        public string[] GetAvailableCommandTypes()
        {
            if (!_isInitialized)
            {
                LogAspera.Warning("TypeDiscoveryService not initialized - returning empty array");
                return Array.Empty<string>();
            }

            return _commandTypes.Keys.ToArray();
        }

        /// <summary>
        /// Try to get a command type by name
        /// </summary>
        /// <param name="typeName">Command type name</param>
        /// <param name="commandType">Output command type if found</param>
        /// <returns>True if command type was found</returns>
        public bool TryGetCommandType(string typeName, out System.Type commandType)
        {
            var normalizedName = NormalizeCommandTypeName(typeName);
            return _commandTypes.TryGetValue(normalizedName, out commandType);
        }

        /// <summary>
        /// Get diagnostic information about discovered types
        /// </summary>
        /// <returns>Formatted diagnostic string</returns>
        public string GetDiagnosticInfo()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine("=== TypeDiscoveryService Diagnostics ===");
            info.AppendLine($"Initialized: {_isInitialized}");
            info.AppendLine($"Command Types Discovered: {_commandTypes.Count}");
            
            if (_commandTypes.Count > 0)
            {
                info.AppendLine("\nDiscovered Types:");
                foreach (var kvp in _commandTypes.OrderBy(x => x.Key))
                {
                    info.AppendLine($"  {kvp.Key} -> {kvp.Value.FullName}");
                }
            }

            return info.ToString();
        }

        /// <summary>
        /// Check if the service has been properly initialized
        /// </summary>
        public bool IsInitialized => _isInitialized && _commandTypes.Count > 0;
    }
}