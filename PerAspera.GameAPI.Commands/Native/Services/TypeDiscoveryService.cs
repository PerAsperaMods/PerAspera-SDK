using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.Native.Services
{
    /// <summary>
    /// Service for discovering and registering available command types across assemblies
    /// Provides type resolution and caching for the command system
    /// </summary>
    public class TypeDiscoveryService
    {
        private ConcurrentDictionary<string, System.Type> _commandTypes;
        private volatile bool _isInitialized;
        
        /// <summary>
        /// Gets whether the service is initialized
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Initialize a new TypeDiscoveryService instance
        /// </summary>
        public TypeDiscoveryService()
        {
            _commandTypes = new ConcurrentDictionary<string, System.Type>();
        }
        
        /// <summary>
        /// Initialize the service by discovering all available command types
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
                return;
                
            try
            {
                // Scan assemblies for command types
                ScanAssembliesForCommandTypes();
                _isInitialized = true;
            }
            catch (Exception)
            {
                _isInitialized = false;
                throw;
            }
        }
        
        /// <summary>
        /// Scan all loaded assemblies for GameCommandBase implementations
        /// </summary>
        private void ScanAssembliesForCommandTypes()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var commandTypes = new ConcurrentDictionary<string, System.Type>();
            
            // Priority scanning for key assemblies
            var gameAssembly = assemblies.FirstOrDefault(a => a.GetName().Name == "Assembly-CSharp");
            if (gameAssembly != null)
            {
                ScanAssemblyForCommands(gameAssembly, commandTypes);
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
                    ScanAssemblyForCommands(assembly, commandTypes);
                }
                catch (Exception)
                {
                    // Assembly scan failed - continue with next assembly
                }
            }
            
            _commandTypes = commandTypes;
        }

        /// <summary>
        /// Scan individual assembly for command types
        /// </summary>
        private void ScanAssemblyForCommands(Assembly assembly, ConcurrentDictionary<string, System.Type> commandTypes)
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(GameCommandBase).IsAssignableFrom(t))
                    .ToArray();
                
                foreach (var type in types)
                {
                    ProcessCommandType(commandTypes, type);
                }
            }
            catch (ReflectionTypeLoadException ex) when (ex.Types != null)
            {
                // Handle partial loading
                var loadedTypes = ex.Types.Where(t => t != null).ToArray();
                foreach (var type in loadedTypes)
                {
                    if (type.IsClass && !type.IsAbstract && typeof(GameCommandBase).IsAssignableFrom(type))
                    {
                        ProcessCommandType(commandTypes, type);
                    }
                }
            }
            catch (Exception)
            {
                // Assembly processing failed - continue
            }
        }
        
        /// <summary>
        /// Process and register a discovered command type
        /// </summary>
        private void ProcessCommandType(ConcurrentDictionary<string, System.Type> commandTypes, System.Type type)
        {
            try
            {
                var typeName = type.Name;
                
                // Remove "Command" suffix for cleaner lookups
                if (typeName.EndsWith("Command"))
                {
                    typeName = typeName.Substring(0, typeName.Length - 7);
                }
                
                // Register with multiple patterns for flexibility
                commandTypes.TryAdd(typeName, type);
                commandTypes.TryAdd(type.Name, type);
                commandTypes.TryAdd(type.FullName, type);
            }
            catch (Exception)
            {
                // Type registration failed - continue
            }
        }
        
        /// <summary>
        /// Get all discovered command types
        /// </summary>
        public System.Type[] GetDiscoveredTypes()
        {
            if (_commandTypes == null)
            {
                return Array.Empty<System.Type>();
            }
            
            return _commandTypes.Values.Distinct().ToArray();
        }
        
        /// <summary>
        /// Try to get a command type by name
        /// </summary>
        public bool TryGetCommandType(string typeName, out System.Type commandType)
        {
            commandType = FindCommandType(typeName);
            return commandType != null;
        }
        
        /// <summary>
        /// Get all available command type names
        /// </summary>
        public string[] GetAvailableCommandTypes()
        {
            if (_commandTypes == null)
                return Array.Empty<string>();
                
            return _commandTypes.Keys.ToArray();
        }
        
        /// <summary>
        /// Initialize command types (alias for Initialize)
        /// </summary>
        public void InitializeCommandTypes()
        {
            Initialize();
        }
        
        /// <summary>
        /// Find a command type by name with flexible matching
        /// </summary>
        public System.Type FindCommandType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName) || _commandTypes == null)
                return null;
                
            // Direct lookup
            if (_commandTypes.TryGetValue(typeName, out var type))
                return type;
                
            // Try with "Command" suffix
            var commandTypeName = typeName.EndsWith("Command") ? typeName : typeName + "Command";
            if (_commandTypes.TryGetValue(commandTypeName, out type))
                return type;
                
            // Case-insensitive lookup
            var match = _commandTypes.FirstOrDefault(kvp => 
                string.Equals(kvp.Key, typeName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(kvp.Key, commandTypeName, StringComparison.OrdinalIgnoreCase));
                
            return match.Value;
        }
        
        /// <summary>
        /// Check if a command type is registered
        /// </summary>
        public bool IsCommandTypeRegistered(string typeName)
        {
            return FindCommandType(typeName) != null;
        }
        
        /// <summary>
        /// Get count of registered command types
        /// </summary>
        public int GetRegisteredTypeCount()
        {
            return _commandTypes?.Count ?? 0;
        }
        
        /// <summary>
        /// Clear all registered types (for testing/reset scenarios)
        /// </summary>
        public void Clear()
        {
            _commandTypes?.Clear();
            _isInitialized = false;
        }
    }
}