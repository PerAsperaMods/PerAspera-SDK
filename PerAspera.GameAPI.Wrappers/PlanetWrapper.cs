#nullable enable
using System;
using System.Collections.Generic;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native Planet class.
    /// Provides safe access to planetary properties and building management.
    /// For atmospheric data, use PerAspera.GameAPI.Climate.ClimateController directly.
    ///
    /// 📖 Enhanced Documentation: F:\ModPeraspera\SDK-Enhanced-Classes\Planet-Enhanced.md
    /// 🤖 Agent Expert: @per-aspera-sdk-coordinator
    /// 🔧 Gap Analysis: F:\ModPeraspera\SDK-Enhanced-Classes\Capabilities-Matrix.md
    /// </summary>
    public class PlanetWrapper : WrapperBase
    {
        public PlanetWrapper(object nativePlanet) : base(nativePlanet) { }

        public static PlanetWrapper? GetCurrent()
        {
            var planet = KeeperTypeRegistry.GetPlanet();
            return planet != null ? new PlanetWrapper(planet) : null;
        }

        // ==================== SYSTEMS ====================

        /// <summary>Get HazardsManager for this planet.</summary>
        public HazardsManagerWrapper GetHazardsManager()
            => new HazardsManagerWrapper(SafeInvoke<object>("get_HazardsManager"));

        // ==================== COORDINATE CONVERSION ====================

        /// <summary>Convert geographic coordinates to the game's internal 2D position.</summary>
        public UnityEngine.Vector2 GetGamePosition(float longitude, float latitude)
            => SafeInvoke<UnityEngine.Vector2>("GetPosition", longitude, latitude);

        /// <summary>Convert a game 2D world position to longitude (degrees).</summary>
        public float GetLongitude(UnityEngine.Vector2 position)
            => SafeInvoke<float>("GetLongitude", position);

        /// <summary>Convert a game 2D world position to latitude (degrees).</summary>
        public float GetLatitude(UnityEngine.Vector2 position)
            => SafeInvoke<float>("GetLatitude", position);

        // ==================== PLANET IDENTITY ====================

        /// <summary>Planet name. Hardcoded to "Mars" for Per Aspera.</summary>
        public string Name => "Mars";

        // ==================== TEMPERATURE ====================

        /// <summary>
        /// Global average surface temperature in Kelvin. 0°C = 273.15 K.
        /// Corresponds to Planet.GetAverageTemperature() in native code.
        /// </summary>
        public float GetAverageTemperature()
            => SafeInvoke<float>("GetAverageTemperature");

        /// <summary>
        /// Surface temperature at a specific geographic position in Kelvin.
        /// Corresponds to Planet.GetTemperature(longitude, latitude) in native code.
        /// </summary>
        public float GetTemperatureAt(float longitude, float latitude)
            => SafeInvoke<float>("GetTemperature", longitude, latitude);

        // ==================== DEPRECATED CLIMATE ====================

        /// <summary>Use PerAspera.GameAPI.Climate.ClimateController for atmospheric data.</summary>
        [Obsolete("Use PerAspera.GameAPI.Climate.ClimateController for atmospheric data. Will be removed in v2.0", false)]
        public float GetTemperature() => SafeInvoke<float>("GetTemperature");

        /// <summary>Use PerAspera.GameAPI.Climate.ClimateController for atmospheric data.</summary>
        [Obsolete("Use PerAspera.GameAPI.Climate.ClimateController for atmospheric data. Will be removed in v2.0", false)]
        public float GetAtmosphericPressure() => SafeInvoke<float>("GetAtmosphericPressure");

        /// <summary>Use PerAspera.GameAPI.Climate.ClimateController for atmospheric data.</summary>
        [Obsolete("Use PerAspera.GameAPI.Climate.ClimateController for atmospheric data. Will be removed in v2.0", false)]
        public float GetOxygenLevel() => SafeInvoke<float>("GetOxygenLevel");

        /// <summary>Use PerAspera.GameAPI.Climate.ClimateController.Instance.Atmosphere directly.</summary>
        [Obsolete("Use PerAspera.GameAPI.Climate.ClimateController.Instance.Atmosphere directly", false)]
        public object? Atmosphere => null;

        // ==================== RESOURCES ====================

        /// <summary>Water stock on the planet.</summary>
        public float WaterStock
        {
            get => SafeInvoke<float?>("GetWaterStock") ?? SafeGetField<float>("waterStock");
            set
            {
                // Property setter when available, otherwise direct field write.
                // SafeInvokeVoid never throws, so the old try/catch fallback was dead code.
                if (!TryInvokeVoid("set_waterStock", value))
                    TrySetField("waterStock", value);
            }
        }

        /// <summary>
        /// Add water to planet stock. Positive = add, negative = remove.
        /// Safe: reads current stock before writing.
        /// </summary>
        public void AddWaterStock(float amount)
        {
            try { WaterStock = WaterStock + amount; }
            catch (Exception ex) { WrapperLog.Warning($"AddWaterStock({amount}) failed: {ex.Message}"); }
        }

        public float SiliconStock => GetResourceStock("resource_silicon");
        public float IronStock    => GetResourceStock("resource_iron");
        public float CarbonStock  => GetResourceStock("resource_carbon");
        public float CalciteStock => GetResourceStock("resource_calcite");

        public bool HasSufficientWater   => WaterStock > 1000f;
        public bool HasBalancedResources => WaterStock > 500f && SiliconStock > 100f &&
                                           IronStock > 100f  && CarbonStock > 50f;

        // ==================== RESOURCE MANAGEMENT ====================

        /// <summary>Get stock of a specific resource.</summary>
        /// <param name="resourceKey">Resource key (e.g., "resource_water", "resource_silicon")</param>
        public float GetResourceStock(string resourceKey)
        {
            try
            {
                var resourceType = KeeperTypeRegistry.GetResourceType(resourceKey);
                if (resourceType == null) return 0f;
                return SafeInvoke<float?>("GetResourceStock", resourceType) ?? 0f;
            }
            catch (Exception ex)
            {
                WrapperLog.Warning($"GetResourceStock failed for {resourceKey}: {ex.Message}");
                return 0f;
            }
        }

        /// <summary>Add resource to planet stock. Use negative amount to remove.</summary>
        public bool AddResource(string resourceKey, float amount)
        {
            try
            {
                var resourceType = KeeperTypeRegistry.GetResourceType(resourceKey);
                if (resourceType == null) return false;
                SafeInvokeVoid("AddResource", resourceType, amount);
                return true;
            }
            catch (Exception ex)
            {
                WrapperLog.Error($"AddResource failed for {resourceKey}: {ex.Message}");
                return false;
            }
        }

        // ==================== TERRAIN & WATER ELEVATION ====================

        /// <summary>Raw terrain elevation at world position (metres, no water).</summary>
        public float GetAltitude(UnityEngine.Vector2 position)
            => SafeInvoke<float>("GetAltitude", position);

        /// <summary>Elevation at world position, returning water surface when submerged.</summary>
        public float GetAltitudeWithWater(UnityEngine.Vector2 position)
            => SafeInvoke<float>("GetAltitudeWithWater", position);

        /// <summary>Global ocean surface elevation — rises as terraforming progresses.</summary>
        public float GetWaterLevel()
            => SafeInvoke<float>("GetWaterLevel");

        // ==================== BUILDING MANAGEMENT ====================

        /// <summary>Get all buildings on this planet.</summary>
        public List<BuildingWrapper> GetBuildings()
        {
            try
            {
                var nativeBuildings = SafeInvoke<object>("get_buildings") ??
                                     SafeInvoke<object>("GetBuildings");
                if (nativeBuildings == null) return new List<BuildingWrapper>();

                var result = new List<BuildingWrapper>();
                if (nativeBuildings is System.Collections.IEnumerable enumerable)
                {
                    foreach (var b in enumerable)
                        if (b != null) result.Add(new BuildingWrapper(b));
                }
                return result;
            }
            catch (Exception ex)
            {
                WrapperLog.Error($"GetBuildings failed: {ex.Message}");
                return new List<BuildingWrapper>();
            }
        }

        /// <summary>Get all buildings on the planet (array version).</summary>
        public BuildingWrapper[] GetBuildingsSafely()
        {
            try { return GetBuildings().ToArray(); }
            catch { return Array.Empty<BuildingWrapper>(); }
        }

        /// <summary>Get all buildings owned by a specific faction.</summary>
        public List<BuildingWrapper> GetBuildingsByFaction(FactionWrapper faction)
        {
            if (!faction.IsValidWrapper) return new List<BuildingWrapper>();

            var result = new List<BuildingWrapper>();
            foreach (var building in GetBuildings())
            {
                try
                {
                    var f = building.GetFaction();
                    if (f?.Name == faction.Name)
                        result.Add(building);
                }
                catch { continue; }
            }
            return result;
        }
    }
}
