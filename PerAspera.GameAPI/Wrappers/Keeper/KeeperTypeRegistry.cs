using System;
using PerAspera.Core.IL2CPP;
using PerAspera.Core;

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
        private static readonly LogAspera Log = new LogAspera("Keeper");

        // ==================== INITIALIZATION ====================

        /// <summary>
        /// Check if Keeper systems are available
        /// Returns true after BaseGame.Awake() completes
        /// </summary>
        public static bool IsInitialized()
        {
            var baseGame = BaseGameWrapper.GetCurrent();
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
        public static KeeperWrapper? GetKeeper()
        {
            
            return (KeeperWrapper)BaseGameWrapper.GetCurrent()?.GetKeeper();
        }
        
        /// <summary>
        /// Get Universe instance (time, factions, planet container)
        /// Safe to call after BaseGame.Awake()
        /// </summary>
        public static object? GetUniverse()
        {
            var baseGameWrapper = BaseGameWrapper.GetCurrent();
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
            var nativeUniverse = GetUniverse();
            if (nativeUniverse == null) return null; // silent — universe not ready yet

            if (nativeUniverse is Universe u)
                return u.planet ?? u.GetPlanet();

            return null; // universe not typed — early lifecycle or bad state
        }
        
        // ==================== STATIC DATA TYPES ====================
        
        /// <summary>
        /// Get ResourceType by key (e.g., "resource_water", "resource_silicon")
        /// Uses StaticDataCollectionItem<ResourceType> table access
        /// Returns null if not found
        /// </summary>
        /// <param name="resourceKey">Resource key from YAML (e.g., "resource_water")</param>
        /// <returns>ResourceType instance or null</returns>
        public static object? GetResourceType(string resourceKey)
        {
            if (string.IsNullOrEmpty(resourceKey)) return null;
            try   { return ResourceType.Has(resourceKey) ? ResourceType.Get(resourceKey) : null; }
            catch (Exception ex) { Log.Warning($"{LogPrefix} GetResourceType failed for '{resourceKey}': {ex.Message}"); return null; }
        }
        
        /// <summary>
        /// Get ResourceType wrapper by key (e.g., "resource_water", "resource_silicon")
        /// Returns wrapped ResourceType for type-safe access
        /// </summary>
        /// <param name="resourceKey">Resource key from YAML (e.g., "resource_water")</param>
        /// <returns>ResourceType wrapper or null if not found</returns>
        public static ResourceTypeWrapper? GetResourceTypeWrapper(string resourceKey)
        {
            var nativeResourceType = GetResourceType(resourceKey);
            return nativeResourceType != null ? new ResourceTypeWrapper(nativeResourceType) : null;
        }
        
        /// <summary>
        /// Get BuildingType by key (e.g., "building_solar_panel")
        /// Uses StaticDataCollectionItem<BuildingType> table access
        /// Returns null if not found
        /// </summary>
        /// <param name="buildingKey">Building key from YAML</param>
        /// <returns>BuildingType instance or null</returns>
        public static object? GetBuildingType(string buildingKey)
        {
            if (string.IsNullOrEmpty(buildingKey)) return null;
            try   { return BuildingType.Has(buildingKey) ? BuildingType.Get(buildingKey) : null; }
            catch (Exception ex) { Log.Warning($"{LogPrefix} GetBuildingType failed for '{buildingKey}': {ex.Message}"); return null; }
        }
        
        /// <summary>
        /// Get Person by key (e.g., "person_ami")
        /// Uses StaticDataCollectionItem<Person> table access
        /// Returns null if not found
        /// </summary>
        /// <param name="personKey">Person key from YAML (e.g., "person_ami")</param>
        /// <returns>Person instance or null</returns>
        public static object? GetPerson(string personKey)
        {
            if (string.IsNullOrEmpty(personKey)) return null;
            try   { return Person.Has(personKey) ? Person.Get(personKey) : null; }
            catch (Exception ex) { Log.Warning($"{LogPrefix} GetPerson failed for '{personKey}': {ex.Message}"); return null; }
        }
        
        /// <summary>
        /// Get TechnologyType by key (e.g., "tech_basic_chemistry")
        /// Uses StaticDataCollectionItem<TechnologyType> table access
        /// Returns null if not found
        /// </summary>
        /// <param name="technologyKey">Technology key from YAML</param>
        /// <returns>TechnologyType instance or null</returns>
        public static object? GetTechnologyType(string technologyKey)
        {
            if (string.IsNullOrEmpty(technologyKey)) return null;
            try   { return TechnologyType.Has(technologyKey) ? TechnologyType.Get(technologyKey) : null; }
            catch (Exception ex) { Log.Warning($"{LogPrefix} GetTechnologyType failed for '{technologyKey}': {ex.Message}"); return null; }
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
            
            if (handle is not Handle h) return null;
            return keeper.GetKeeperMap()?.FindBase(h);
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
            var baseGame = BaseGameWrapper.GetCurrent();
            if (baseGame == null) return "BaseGame: NOT FOUND";
            
            var keeper = baseGame.GetKeeper();
            var universe = baseGame.GetUniverse();
            var planet = GetPlanet();
            
            return $"BaseGame: ✓ | Keeper: {(keeper != null ? "✓" : "✗")} | " +
                   $"Universe: {(universe != null ? "✓" : "✗")} | Planet: {(planet != null ? "✓" : "✗")}";
        }
    }
}
