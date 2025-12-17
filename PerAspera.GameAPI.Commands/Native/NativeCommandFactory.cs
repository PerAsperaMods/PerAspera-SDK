using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using PerAspera.Core;
using PerAspera.GameAPI.Commands.Constants;
using PerAspera.GameAPI.Commands.Native.IL2CPPInterop;

namespace PerAspera.GameAPI.Commands.Native
{
    /// <summary>
    /// Factory for creating native Per Aspera command instances with type safety
    /// Uses reflection and IL2CPP interop to create proper native command objects
    /// </summary>
    public class NativeCommandFactory
    {
        private readonly Dictionary<string, Type> _commandTypes;
        private readonly Dictionary<string, ConstructorInfo> _constructors;
        private static NativeCommandFactory _instance;
        
        /// <summary>
        /// Singleton instance for global access
        /// </summary>
        public static NativeCommandFactory Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new NativeCommandFactory();
                return _instance;
            }
        }
        
        /// <summary>
        /// Initialize factory and scan for command types
        /// </summary>
        private NativeCommandFactory()
        {
            _commandTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
            _constructors = new Dictionary<string, ConstructorInfo>(StringComparer.OrdinalIgnoreCase);
            
            ScanForCommandTypes();
            LogAspera.Info($"NativeCommandFactory initialized with {_commandTypes.Count} command types");
        }
        
        /// <summary>
        /// Create native command instance by type name
        /// </summary>
        /// <param name="commandTypeName">Name of command type (e.g., "CmdImportResource" or "ImportResource")</param>
        /// <param name="parameters">Constructor parameters</param>
        /// <returns>Wrapped native command instance</returns>
        public CommandBaseWrapper CreateCommand(string commandTypeName, params object[] parameters)
        {
            try
            {
                // Normalize command type name
                var normalizedName = NormalizeCommandTypeName(commandTypeName);
                
                if (!_commandTypes.TryGetValue(normalizedName, out var commandType))
                {
                    LogAspera.Error($"Command type not found: {commandTypeName} (normalized: {normalizedName})");
                    return null;
                }
                
                // Create instance
                var nativeCommand = CreateNativeInstance(commandType, parameters);
                if (nativeCommand == null)
                {
                    LogAspera.Error($"Failed to create native instance of {commandType.Name}");
                    return null;
                }
                
                // Wrap and return
                var wrapper = new CommandBaseWrapper(nativeCommand);
                LogAspera.Debug($"Created command: {wrapper.CommandName}");
                return wrapper;
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Failed to create command {commandTypeName}: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Create typed native command instance
        /// </summary>
        /// <typeparam name="T">Native command type</typeparam>
        /// <param name="parameters">Constructor parameters</param>
        /// <returns>Wrapped native command instance</returns>
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
        /// Create command from SDK command type name
        /// </summary>
        /// <param name="sdkCommandType">SDK command type (from NativeCommandTypes)</param>
        /// <param name="parameters">Constructor parameters</param>
        /// <returns>Wrapped native command instance</returns>
        public CommandBaseWrapper CreateFromSDKType(string sdkCommandType, params object[] parameters)
        {
            var nativeTypeName = MapSDKToNativeType(sdkCommandType);
            return CreateCommand(nativeTypeName, parameters);
        }
        
        /// <summary>
        /// Check if command type is available
        /// </summary>
        public bool IsCommandTypeAvailable(string commandTypeName)
        {
            var normalizedName = NormalizeCommandTypeName(commandTypeName);
            return _commandTypes.ContainsKey(normalizedName);
        }
        
        /// <summary>
        /// Get all available command types
        /// </summary>
        public string[] GetAvailableCommandTypes()
        {
            return _commandTypes.Keys.ToArray();
        }
        
        /// <summary>
        /// Get native type for SDK command type
        /// </summary>
        public Type GetNativeType(string sdkCommandType)
        {
            var nativeTypeName = MapSDKToNativeType(sdkCommandType);
            var normalizedName = NormalizeCommandTypeName(nativeTypeName);
            _commandTypes.TryGetValue(normalizedName, out var type);
            return type;
        }
        
        /// <summary>
        /// Reset factory (for testing)
        /// </summary>
        internal static void Reset()
        {
            _instance = null;
        }
        
        private void ScanForCommandTypes()
        {
            try
            {
                LogAspera.Info("Scanning assemblies for command types...");
                
                // Initialize GameTypeInitializer to get access to game assemblies
                GameTypeInitializer.Initialize();
                
                // Get all loaded assemblies
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                
                // Priority scan: Focus on Assembly-CSharp (Per Aspera game assembly)
                var gameAssembly = assemblies.FirstOrDefault(a => a.GetName().Name == "Assembly-CSharp");
                if (gameAssembly != null)
                {
                    ScanAssemblyForCommands(gameAssembly, isPriorityAssembly: true);
                }
                
                // Scan other assemblies
                foreach (var assembly in assemblies)
                {
                    if (assembly == gameAssembly) continue; // Already scanned
                    
                    ScanAssemblyForCommands(assembly, isPriorityAssembly: false);
                }
                
                LogAspera.Info($"Command type scanning complete: {_commandTypes.Count} types found");
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Error during command type scanning: {ex.Message}");
            }
        }

        /// <summary>
        /// Scan a single assembly for command types
        /// </summary>
        private void ScanAssemblyForCommands(Assembly assembly, bool isPriorityAssembly)
        {
            try
            {
                // Look for types that look like commands
                var commandTypes = assembly.GetTypes()
                    .Where(t => IsCommandType(t))
                    .ToArray();
                    
                foreach (var type in commandTypes)
                {
                    RegisterCommandType(type);
                }
                
                if (commandTypes.Length > 0)
                {
                    LogAspera.Info($"Found {commandTypes.Length} command types in {assembly.GetName().Name}{(isPriorityAssembly ? " (game assembly)" : "")}");
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                LogAspera.Debug($"Could not load all types from {assembly.GetName().Name}: {ex.Message}");
            }
            catch (Exception ex)
            {
                LogAspera.Debug($"Error scanning assembly {assembly.GetName().Name}: {ex.Message}");
            }
                }
                
                LogAspera.Info($"Command type scan complete. Found {_commandTypes.Count} total command types");
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Failed to scan for command types: {ex.Message}");
            }
        }
        
        private bool IsCommandType(Type type)
        {
            // Check if type looks like a command
            return !type.IsAbstract &&
                   !type.IsInterface &&
                   (type.Name.StartsWith("Cmd") ||
                    type.Name.EndsWith("Command") ||
                    type.BaseType?.Name == "CommandBase" ||
                    type.GetInterfaces().Any(i => i.Name.Contains("Command")));
        }
        
        private void RegisterCommandType(Type commandType)
        {
            try
            {
                var normalizedName = NormalizeCommandTypeName(commandType.Name);
                _commandTypes[normalizedName] = commandType;
                
                // Cache constructor info
                var constructor = GetBestConstructor(commandType);
                if (constructor != null)
                {
                    _constructors[normalizedName] = constructor;
                }
                
                LogAspera.Debug($"Registered command type: {commandType.Name} -> {normalizedName}");
            }
            catch (Exception ex)
            {
                LogAspera.Warning($"Failed to register command type {commandType.Name}: {ex.Message}");
            }
        }
        
        private ConstructorInfo GetBestConstructor(Type commandType)
        {
            var constructors = commandType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            
            // Prefer parameterless constructor
            var parameterlessConstructor = constructors.FirstOrDefault(c => c.GetParameters().Length == 0);
            if (parameterlessConstructor != null)
                return parameterlessConstructor;
                
            // Otherwise use first available constructor
            return constructors.FirstOrDefault();
        }
        
        private object CreateNativeInstance(Type commandType, object[] parameters)
        {
            try
            {
                var normalizedName = NormalizeCommandTypeName(commandType.Name);
                
                // Try cached constructor first
                if (_constructors.TryGetValue(normalizedName, out var constructor))
                {
                    var paramCount = constructor.GetParameters().Length;
                    
                    if (paramCount == 0 && (parameters == null || parameters.Length == 0))
                    {
                        // Use parameterless constructor
                        return constructor.Invoke(new object[0]);
                    }
                    else if (paramCount == parameters?.Length)
                    {
                        // Use constructor with matching parameter count
                        return constructor.Invoke(parameters);
                    }
                }
                
                // Fallback to Activator.CreateInstance
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
        
        private string NormalizeCommandTypeName(string commandTypeName)
        {
            if (string.IsNullOrEmpty(commandTypeName))
                return commandTypeName;
                
            // Remove "Cmd" prefix if present, then add it back for consistency
            var normalized = commandTypeName.StartsWith("Cmd") 
                ? commandTypeName 
                : "Cmd" + commandTypeName;
                
            return normalized;
        }
        
        private string MapSDKToNativeType(string sdkCommandType)
        {
            // Map SDK command types to native command types
            // This mapping could be more sophisticated or data-driven
            return sdkCommandType switch
            {
                NativeCommandTypes.ImportResource => "CmdImportResource",
                NativeCommandTypes.SpawnResourceVein => "CmdSpawnResourceVein", 
                NativeCommandTypes.UnlockBuilding => "CmdUnlockBuilding",
                NativeCommandTypes.AdditionalBuilding => "CmdAdditionalBuilding",
                NativeCommandTypes.ResearchTechnology => "CmdResearchTechnology",
                NativeCommandTypes.UnlockKnowledge => "CmdUnlockKnowledge",
                NativeCommandTypes.StartDialogue => "CmdStartDialogue",
                NativeCommandTypes.NotifyDialogue => "CmdNotifyDialogue",
                NativeCommandTypes.Sabotage => "CmdSabotage",
                NativeCommandTypes.GameOver => "CmdGameOver",
                NativeCommandTypes.SetOverride => "CmdSetOverride",
                _ => sdkCommandType.StartsWith("Cmd") ? sdkCommandType : "Cmd" + sdkCommandType
            };
        }

        /// <summary>
        /// Create ImportResource command with MVP parameters
        /// Phase 1.2: Specialized factory method for testing
        /// </summary>
        public CommandBaseWrapper CreateImportResourceCommand(string resourceName, float amount)
        {
            try
            {
                LogAspera.Debug($"Creating ImportResource command: {resourceName} x {amount}");

                // Try to create CmdImportResource using discovered types
                var commandWrapper = CreateCommand("ImportResource", resourceName, amount);
                
                if (commandWrapper == null)
                {
                    // Fallback: try alternative command names
                    commandWrapper = CreateCommand("CmdImportResource", resourceName, amount);
                }

                if (commandWrapper == null)
                {
                    LogAspera.Error($"Failed to create ImportResource command for {resourceName}");
                }
                else
                {
                    LogAspera.Info($"Successfully created ImportResource command: {resourceName} x {amount}");
                }

                return commandWrapper;
            }
            catch (Exception ex)
            {
                LogAspera.Error($"Error creating ImportResource command: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Get diagnostic information about discovered command types
        /// Useful for debugging command discovery issues
        /// </summary>
        public string GetDiagnosticInfo()
        {
            try
            {
                var info = new System.Text.StringBuilder();
                info.AppendLine($"NativeCommandFactory Diagnostic Info:");
                info.AppendLine($"  Total Command Types: {_commandTypes.Count}");
                info.AppendLine($"  GameTypeInitializer Available: {GameTypeInitializer.GetBaseGameType() != null}");
                info.AppendLine($"  CommandBus Type Available: {GameTypeInitializer.GetCommandBusType() != null}");
                info.AppendLine();
                info.AppendLine("Discovered Command Types:");

                foreach (var kvp in _commandTypes.OrderBy(x => x.Key))
                {
                    info.AppendLine($"  - {kvp.Key} → {kvp.Value.FullName}");
                }

                return info.ToString();
            }
            catch (Exception ex)
            {
                return $"Error generating diagnostic info: {ex.Message}";
            }
        }
    }
}