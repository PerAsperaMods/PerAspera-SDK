using System;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Central registry for accessing Keeper-managed game entities
    /// Provides type-safe, stateless access to Planet, Universe, and entity handles
    /// DOC: See DOC/SDK/Core/Keeper-Instance-Access.md for architecture
    /// </summary>
    public static class KeeperTypeRegistry
    {
        private static readonly string LogPrefix = "[KeeperTypeRegistry]";
        
        // ==================== INITIALIZATION ====================
        
        /// <summary>
        /// Check if Keeper systems are available
        /// Returns true after BaseGame.Awake() completes
        /// </summary>
        public static bool IsInitialized()
        {
            var baseGame = BaseGame.GetCurrent();
            if (baseGame == null) return false;
            
            var keeper = baseGame.GetKeeper();
            return keeper != null;
        }
        
        /// <summary>
        /// Validate Keeper is ready (throws if not initialized)
        /// </summary>
        private static void ValidateInitialized()
        {
            if (!IsInitialized())
            {
                throw new InvalidOperationException(
                    $"{LogPrefix} Keeper not initialized. Call after BaseGame.Awake() completes.");
            }
        }
        
        // ==================== CORE SYSTEMS ====================
        
        /// <summary>
        /// Get Keeper instance (entity registry)
        /// Safe to call after BaseGame.Awake()
        /// </summary>
        public static Keeper ? GetKeeper()
        {
            
            return (Keeper)BaseGame.GetCurrent()?.GetKeeper();
        }
        
        /// <summary>
        /// Get Universe instance (time, factions, planet container)
        /// Safe to call after BaseGame.Awake()
        /// </summary>
        public static object? GetUniverse()
        {
            var baseGameWrapper = BaseGame.GetCurrent();
            if (!baseGameWrapper.IsValidWrapper) return null;
            return baseGameWrapper.GetUniverse()?.GetNativeObject(); // Get native object directly
        }
        
        /// <summary>
        /// Get Planet instance (climate, resources, buildings)
        /// Safe to call after BaseGame.Start() completes
        /// Returns null if called too early in lifecycle
        /// </summary>
        public static object? GetPlanet()
        {
            var nativeUniverse = GetUniverse(); // Now returns native object
            if (nativeUniverse == null)
            {
                UnityEngine.Debug.LogWarning($"{LogPrefix} Universe not available yet");
                return null;
            }
            
            // Universe.GetPlanet() via reflection on native object
            try
            {
                return nativeUniverse.InvokeMethod<object>("GetPlanet");
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LogPrefix} Failed to get Planet: {ex.Message}");
                return null;
            }
        }
        
        // ==================== STATIC DATA TYPES ====================
        
        /// <summary>
        /// Get ResourceType by key (e.g., "resource_water", "resource_silicon")
        /// Uses StaticDataCollectionItem<ResourceType>.Get(key) pattern
        /// Returns null if not found
        /// </summary>
        /// <param name="resourceKey">Resource key from YAML (e.g., "resource_water")</param>
        /// <returns>ResourceType instance or null</returns>
        public static object? GetResourceType(string resourceKey)
        {
            if (string.IsNullOrEmpty(resourceKey))
            {
                UnityEngine.Debug.LogWarning($"{LogPrefix} GetResourceType called with null/empty key");
                return null;
            }
            
            try
            {
                // Get ResourceType Type from GameTypeInitializer
                var resourceTypeClass = GameTypeInitializer.GetResourceType();
                if (resourceTypeClass == null)
                {
                    UnityEngine.Debug.LogError($"{LogPrefix} ResourceType class not found");
                    return null;
                }
                
                // Call static method: ResourceType.Get(key)
                var getMethod = resourceTypeClass.GetMethod("Get", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (getMethod == null)
                {
                    UnityEngine.Debug.LogError($"{LogPrefix} ResourceType.Get method not found");
                    return null;
                }
                
                var result = getMethod.Invoke(null, new object[] { resourceKey });
                if (result == null)
                {
                    UnityEngine.Debug.LogWarning($"{LogPrefix} ResourceType not found for key: {resourceKey}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LogPrefix} Failed to get ResourceType '{resourceKey}': {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get ResourceType wrapper by key (e.g., "resource_water", "resource_silicon")
        /// Returns wrapped ResourceType for type-safe access
        /// </summary>
        /// <param name="resourceKey">Resource key from YAML (e.g., "resource_water")</param>
        /// <returns>ResourceType wrapper or null if not found</returns>
        public static ResourceType? GetResourceTypeWrapper(string resourceKey)
        {
            var nativeResourceType = GetResourceType(resourceKey);
            return nativeResourceType != null ? new ResourceType(nativeResourceType) : null;
        }
        
        /// <summary>
        /// Get BuildingType by key (e.g., "building_solar_panel")
        /// Uses StaticDataCollectionItem<BuildingType>.Get(key) pattern
        /// Returns null if not found
        /// </summary>
        /// <param name="buildingKey">Building key from YAML</param>
        /// <returns>BuildingType instance or null</returns>
        public static object? GetBuildingType(string buildingKey)
        {
            if (string.IsNullOrEmpty(buildingKey))
            {
                UnityEngine.Debug.LogWarning($"{LogPrefix} GetBuildingType called with null/empty key");
                return null;
            }
            
            try
            {
                var buildingTypeClass = GameTypeInitializer.GetBuildingType();
                if (buildingTypeClass == null)
                {
                    UnityEngine.Debug.LogError($"{LogPrefix} BuildingType class not found");
                    return null;
                }
                
                var getMethod = buildingTypeClass.GetMethod("Get", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (getMethod == null)
                {
                    UnityEngine.Debug.LogError($"{LogPrefix} BuildingType.Get method not found");
                    return null;
                }
                
                var result = getMethod.Invoke(null, new object[] { buildingKey });
                if (result == null)
                {
                    UnityEngine.Debug.LogWarning($"{LogPrefix} BuildingType not found for key: {buildingKey}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LogPrefix} Failed to get BuildingType '{buildingKey}': {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get Person by key (e.g., "person_ami")
        /// Uses StaticDataCollectionItem<Person>.Get(key) pattern
        /// Returns null if not found
        /// </summary>
        /// <param name="personKey">Person key from YAML (e.g., "person_ami")</param>
        /// <returns>Person instance or null</returns>
        public static object? GetPerson(string personKey)
        {
            if (string.IsNullOrEmpty(personKey))
            {
                UnityEngine.Debug.LogWarning($"{LogPrefix} GetPerson called with null/empty key");
                return null;
            }
            
            try
            {
                var personTypeClass = GameTypeInitializer.GetPerson();
                if (personTypeClass == null)
                {
                    UnityEngine.Debug.LogError($"{LogPrefix} Person class not found");
                    return null;
                }
                
                var getMethod = personTypeClass.GetMethod("Get", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (getMethod == null)
                {
                    UnityEngine.Debug.LogError($"{LogPrefix} Person.Get method not found");
                    return null;
                }
                
                var result = getMethod.Invoke(null, new object[] { personKey });
                if (result == null)
                {
                    UnityEngine.Debug.LogWarning($"{LogPrefix} Person not found for key: {personKey}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LogPrefix} Failed to get Person '{personKey}': {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get TechnologyType by key (e.g., "tech_basic_chemistry")
        /// Uses StaticDataCollectionItem<TechnologyType>.Get(key) pattern
        /// Returns null if not found
        /// </summary>
        /// <param name="technologyKey">Technology key from YAML</param>
        /// <returns>TechnologyType instance or null</returns>
        public static object? GetTechnologyType(string technologyKey)
        {
            if (string.IsNullOrEmpty(technologyKey))
            {
                UnityEngine.Debug.LogWarning($"{LogPrefix} GetTechnologyType called with null/empty key");
                return null;
            }
            
            try
            {
                var technologyTypeClass = GameTypeInitializer.GetTechnologyType();
                if (technologyTypeClass == null)
                {
                    UnityEngine.Debug.LogError($"{LogPrefix} TechnologyType class not found");
                    return null;
                }
                
                var getMethod = technologyTypeClass.GetMethod("Get", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (getMethod == null)
                {
                    UnityEngine.Debug.LogError($"{LogPrefix} TechnologyType.Get method not found");
                    return null;
                }
                
                var result = getMethod.Invoke(null, new object[] { technologyKey });
                if (result == null)
                {
                    UnityEngine.Debug.LogWarning($"{LogPrefix} TechnologyType not found for key: {technologyKey}");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LogPrefix} Failed to get TechnologyType '{technologyKey}': {ex.Message}");
                return null;
            }
        }
        
        // ==================== ENTITY LOOKUP ====================
        
        /// <summary>
        /// Find entity by handle using KeeperMap
        /// Keeper.Find(handle) wrapper
        /// </summary>
        public static object? GetByHandle(object handle)
        {
            ValidateInitialized();
            
            var keeper = GetKeeper();
            if (keeper == null) return null;
            
            try
            {
                // Keeper.Find(handle)
                return keeper.InvokeMethod<object>("Find", handle);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LogPrefix} Failed to find handle: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Find entity by handle (type-safe with wrapper)
        /// </summary>
        public static T? GetByHandle<T>(object handle) where T : WrapperBase
        {
            var entity = GetByHandle(handle);
            if (entity == null) return null;
            
            return (T?)Activator.CreateInstance(typeof(T), entity);
        }
        
        // ==================== VALIDATION ====================
        
        /// <summary>
        /// Get initialization status report for debugging
        /// </summary>
        public static string GetStatus()
        {
            var baseGame = BaseGame.GetCurrent();
            if (baseGame == null) return "BaseGame: NOT FOUND";
            
            var keeper = baseGame.GetKeeper();
            var universe = baseGame.GetUniverse();
            var planet = GetPlanet();
            
            return $"BaseGame: ✓ | Keeper: {(keeper != null ? "✓" : "✗")} | " +
                   $"Universe: {(universe != null ? "✓" : "✗")} | Planet: {(planet != null ? "✓" : "✗")}";
        }
    }
}
