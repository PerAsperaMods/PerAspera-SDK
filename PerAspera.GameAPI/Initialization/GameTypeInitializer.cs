using System;
using System.Linq;
using System.Reflection;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI
{
    /// <summary>
    /// Initialization system for Per Aspera game types
    /// Provides safe access to game classes like BaseGame, Universe, Planet
    /// </summary>
    public static class GameTypeInitializer
    {
        private static readonly LogAspera _log = new LogAspera("GameAPI.TypeInitializer");
        private static bool _isInitialized = false;
        
        // Cached game types for performance
        private static System.Type? _baseGameType;
        private static System.Type? _universeType;
        private static System.Type? _planetType;
        private static System.Type? _factionType;
        private static System.Type? _resourceType;
        private static System.Type? _buildingType;
        private static System.Type? _technologyType;
        private static System.Type? _blackboardType;
        private static System.Type? _commandBusType;

        // Cached singleton instances
        private static object? _baseGameInstance;
        private static object? _universeInstance;

        /// <summary>
        /// Initialize game type discovery system
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized)
                return;

            try
            {
                _log.Info("🔍 Initializing game type discovery...");

                // Wait for game assemblies to be loaded
                WaitForGameAssemblies();

                // Try to discover core types
                DiscoverGameTypes();

                // Try to get singleton instances
                DiscoverSingletonInstances();

                _isInitialized = true;
                
                var stats = GetDiscoveryStats();
                _log.Info($"✅ Type discovery initialized: {stats}");
            }
            catch (Exception ex)
            {
                _log.Error($"❌ Failed to initialize type discovery: {ex.Message}");
            }
        }

        /// <summary>
        /// Wait for game assemblies to be loaded
        /// </summary>
        private static void WaitForGameAssemblies()
        {
            // Check for main game assembly
            var gameAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Assembly-CSharp");

            if (gameAssembly == null)
            {
                _log.Warning("⚠️ Assembly-CSharp not yet loaded, some types may not be available");
            }
            else
            {
                _log.Debug($"📦 Found game assembly: {gameAssembly.GetName().Name}");
            }
        }

        /// <summary>
        /// Discover singleton instances
        /// </summary>
        private static void DiscoverSingletonInstances()
        {
            try
            {
                // Try to get BaseGame singleton
                if (_baseGameType != null)
                {
                    _baseGameInstance = ReflectionHelpers.GetSingletonInstance(_baseGameType);
                    if (_baseGameInstance != null)
                    {
                        _log.Debug("🎯 Found BaseGame singleton instance");
                    }
                }

                // Try to get Universe singleton  
                if (_universeType != null)
                {
                    _universeInstance = ReflectionHelpers.GetSingletonInstance(_universeType);
                    if (_universeInstance != null)
                    {
                        _log.Debug("🌌 Found Universe singleton instance");
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warning($"Error discovering singleton instances: {ex.Message}");
            }
        }

        /// <summary>
        /// Get BaseGame singleton instance
        /// </summary>
        public static object? GetBaseGameInstance()
        {
            if (_baseGameInstance == null && _baseGameType != null)
            {
                _baseGameInstance = ReflectionHelpers.GetSingletonInstance(_baseGameType);
            }
            return _baseGameInstance;
        }

        /// <summary>
        /// Get Universe singleton instance
        /// </summary>
        public static object? GetUniverseInstance()
        {
            if (_universeInstance == null && _universeType != null)
            {
                _universeInstance = ReflectionHelpers.GetSingletonInstance(_universeType);
            }
            return _universeInstance;
        }

        /// <summary>
        /// Get BaseGame type
        /// </summary>
        public static System.Type? GetBaseGameType()
        {
            if (_baseGameType == null)
            {
                _baseGameType = FindGameType("BaseGame");
            }
            return _baseGameType;
        }

        /// <summary>
        /// Get Universe type
        /// </summary>
        public static System.Type? GetUniverseType()
        {
            if (_universeType == null)
            {
                _universeType = FindGameType("Universe");
            }
            return _universeType;
        }

        /// <summary>
        /// Get Planet type
        /// </summary>
        public static System.Type? GetPlanetType()
        {
            if (_planetType == null)
            {
                _planetType = FindGameType("Planet");
            }
            return _planetType;
        }

        /// <summary>
        /// Get Faction type
        /// </summary>
        public static System.Type? GetFactionType()
        {
            if (_factionType == null)
            {
                _factionType = FindGameType("Faction");
            }
            return _factionType;
        }

        /// <summary>
        /// Get Resource type
        /// </summary>
        public static System.Type? GetResourceType()
        {
            if (_resourceType == null)
            {
                _resourceType = FindGameType("ResourceType") ?? FindGameType("Resource");
            }
            return _resourceType;
        }

        /// <summary>
        /// Get Building type
        /// </summary>
        public static System.Type? GetBuildingType()
        {
            if (_buildingType == null)
            {
                _buildingType = FindGameType("Building") ?? FindGameType("BuildingType");
            }
            return _buildingType;
        }

        /// <summary>
        /// Get Technology type  
        /// </summary>
        public static System.Type? GetTechnologyType()
        {
            if (_technologyType == null)
            {
                _technologyType = FindGameType("Technology") ?? FindGameType("TechnologyType");
            }
            return _technologyType;
        }

        /// <summary>
        /// Get Blackboard type
        /// </summary>
        public static System.Type? GetBlackboardType()
        {
            if (_blackboardType == null)
            {
                _blackboardType = FindGameType("Blackboard");
            }
            return _blackboardType;
        }

        /// <summary>
        /// Get CommandBus type
        /// </summary>
        public static System.Type? GetCommandBusType()
        {
            if (_commandBusType == null)
            {
                _commandBusType = FindGameType("CommandBus") ?? FindGameType("GameEventBus");
            }
            return _commandBusType;
        }

        /// <summary>
        /// Get discovery statistics
        /// </summary>
        public static TypeDiscoveryStats GetDiscoveryStats()
        {
            return new TypeDiscoveryStats
            {
                HasBaseGame = GetBaseGameType() != null,
                HasUniverse = GetUniverseType() != null,
                HasPlanet = GetPlanetType() != null,
                HasBlackboard = GetBlackboardType() != null,
                HasFaction = GetFactionType() != null,
                HasResourceType = GetResourceType() != null,
                HasCommandBus = GetCommandBusType() != null,
                TotalTypesFound = CountFoundTypes(),
                IsInitialized = _isInitialized,
                Timestamp = DateTime.Now
            };
        }

        /// <summary>
        /// Find a game type by name from multiple assemblies
        /// </summary>
        /// <param name="typeName">Name of the type to find</param>
        /// <returns>Found type or null</returns>
        private static System.Type? FindGameType(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;

            try
            {
                var type = ReflectionHelpers.FindType(typeName);
                if (type != null)
                {
                    _log.Debug($"✅ Found game type: {typeName} -> {type.FullName}");
                }
                else
                {
                    _log.Debug($"❌ Game type not found: {typeName}");
                }

                return type;
            }
            catch (Exception ex)
            {
                _log.Error($"Error finding type {typeName}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Find type by name using ReflectionHelpers
        /// </summary>
        /// <param name="typeName">Name of type to find</param>
        /// <returns>Found type or null</returns>
        public static System.Type? FindType(string typeName)
        {
            return FindGameType(typeName);
        }

        /// <summary>
        /// Check if all core types are available
        /// </summary>
        /// <returns>True if all core types found</returns>
        public static bool AreAllCoreTypesAvailable()
        {
            EnsureInitialized();

            return GetBaseGameType() != null &&
                   GetUniverseType() != null &&
                   GetPlanetType() != null &&
                   GetFactionType() != null &&
                   GetResourceType() != null;
        }

        /// <summary>
        /// Discover all game types
        /// </summary>
        private static void DiscoverGameTypes()
        {
            try
            {
                _log.Debug("?? Discovering core game types...");

                // Pre-load all core types
                GetBaseGameType();
                GetUniverseType();
                GetPlanetType();
                GetBlackboardType();
                GetFactionType();
                GetResourceType();
                GetCommandBusType();

                var stats = GetDiscoveryStats();
                _log.Info($"?? Type discovery complete: {stats.TotalTypesFound}/7 types found");
            }
            catch (Exception ex)
            {
                _log.Error($"Error during type discovery: {ex.Message}");
            }
        }

        /// <summary>
        /// Count how many types were found
        /// </summary>
        private static int CountFoundTypes()
        {
            int count = 0;
            if (_baseGameType != null) count++;
            if (_universeType != null) count++;
            if (_planetType != null) count++;
            if (_blackboardType != null) count++;
            if (_factionType != null) count++;
            if (_resourceType != null) count++;
            if (_commandBusType != null) count++;
            return count;
        }

        /// <summary>
        /// Reset all cached types (for testing/development)
        /// </summary>
        public static void Reset()
        {
            _log.Debug("?? Resetting type cache...");
            _baseGameType = null;
            _universeType = null;
            _planetType = null;
            _factionType = null;
            _resourceType = null;
            _buildingType = null;
            _technologyType = null;
            _blackboardType = null;
            _commandBusType = null;
            _isInitialized = false;
        }

        /// <summary>
        /// Ensure initialization
        /// </summary>
        private static void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Initialize();
            }
        }

        // Additional type accessors for event patching services
        
        /// <summary>
        /// Get TimeManager type (fallback to Universe if not found)
        /// </summary>
        public static System.Type? GetTimeManagerType()
        {
            EnsureInitialized();
            
            // Try to find TimeManager type first
            var timeManagerType = FindType("TimeManager");
            if (timeManagerType != null)
                return timeManagerType;
            
            // Fallback to Universe type which often handles time
            return GetUniverseType();
        }

        /// <summary>
        /// Get ResourceManager type (fallback to Faction if not found)
        /// </summary>
        public static System.Type? GetResourceManagerType()
        {
            EnsureInitialized();
            
            // Try to find ResourceManager type first
            var resourceManagerType = FindType("ResourceManager");
            if (resourceManagerType != null)
                return resourceManagerType;
            
            // Fallback to Faction type which often handles resources
            return GetFactionType();
        }

        /// <summary>
        /// Get SaveManager type (fallback to BaseGame if not found)
        /// </summary>
        public static System.Type? GetSaveManagerType()
        {
            EnsureInitialized();
            
            // Try to find SaveManager type first
            var saveManagerType = FindType("SaveManager");
            if (saveManagerType != null)
                return saveManagerType;
            
            // Fallback to BaseGame type which often handles saves
            return GetBaseGameType();
        }

        /// <summary>
        /// Get SceneManager type (fallback to BaseGame if not found)
        /// </summary>
        public static System.Type? GetSceneManagerType()
        {
            EnsureInitialized();
            
            // Try to find SceneManager type first
            var sceneManagerType = FindType("SceneManager");
            if (sceneManagerType != null)
                return sceneManagerType;
            
            // Fallback to BaseGame type which often handles scenes
            return GetBaseGameType();
        }

        /// <summary>
        /// Get UIManager type (fallback to BaseGame if not found)
        /// </summary>
        public static System.Type? GetUIManagerType()
        {
            EnsureInitialized();
            
            // Try to find UIManager type first
            var uiManagerType = FindType("UIManager");
            if (uiManagerType != null)
                return uiManagerType;
            
            // Fallback to BaseGame type which often handles UI
            return GetBaseGameType();
        }

        /// <summary>
        /// Get BuildingManager type (fallback to Planet if not found)
        /// </summary>
        public static System.Type? GetBuildingManagerType()
        {
            EnsureInitialized();
            
            // Try to find BuildingManager type first
            var buildingManagerType = FindType("BuildingManager");
            if (buildingManagerType != null)
                return buildingManagerType;
            
            // Fallback to Planet type which often handles buildings
            return GetPlanetType();
        }

        /// <summary>
        /// Get Construction type (fallback to Building if not found)
        /// </summary>
        public static System.Type? GetConstructionType()
        {
            EnsureInitialized();
            
            // Try to find Construction type first
            var constructionType = FindType("Construction");
            if (constructionType != null)
                return constructionType;
            
            // Try alternative names
            var buildingConstructionType = FindType("BuildingConstruction");
            if (buildingConstructionType != null)
                return buildingConstructionType;
            
            // Fallback to Building type
            return GetBuildingType();
        }
    }

    /// <summary>
    /// Type discovery statistics
    /// </summary>
    public class TypeDiscoveryStats
    {
        public bool HasBaseGame { get; set; }
        public bool HasUniverse { get; set; }
        public bool HasPlanet { get; set; }
        public bool HasBlackboard { get; set; }
        public bool HasFaction { get; set; }
        public bool HasResourceType { get; set; }
        public bool HasCommandBus { get; set; }
        public int TotalTypesFound { get; set; }
        public bool IsInitialized { get; set; }
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return $"TypeDiscovery: {TotalTypesFound}/7 types found (BaseGame:{HasBaseGame}, Universe:{HasUniverse}, Planet:{HasPlanet})";
        }
    }
}