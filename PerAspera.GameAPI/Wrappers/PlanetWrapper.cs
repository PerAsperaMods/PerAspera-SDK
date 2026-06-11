#nullable enable
using System;
using System.Collections.Generic;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native Planet class.
    /// Provides safe access to planetary properties.
    /// For atmospheric data, use PerAspera.GameAPI.Climate.ClimateController directly.
    ///
    /// MIGRATION PILOTE (2026-06-10) — interop typé d'abord :
    /// les membres délèguent au proxy interop <see cref="global::Planet"/> (vérification à la
    /// compilation, IntelliSense, erreur de build si le jeu change) au lieu de la réflexion
    /// string-based SafeInvoke. La réflexion ne subsiste que pour les membres natifs
    /// réellement inaccessibles (ex: setter privé de waterStock).
    /// Les API qui n'ont JAMAIS existé côté jeu (GetResourceStock, AddResource,
    /// buildings sur Planet) sont marquées [Obsolete] avec un message honnête —
    /// elles retournaient silencieusement 0/vide depuis toujours.
    ///
    /// 📖 Enhanced Documentation: F:\ModPeraspera\docs\Planet-Enhanced.md
    /// 🤖 Agent Expert: @per-aspera-sdk-coordinator
    /// 🔧 Gap Analysis: F:\ModPeraspera\docs\Capabilities-Matrix.md
    /// </summary>
    public class PlanetWrapper : WrapperBase
    {
        /// <summary>Wraps an untyped native planet (compat). Prefer the typed overload.</summary>
        public PlanetWrapper(object nativePlanet) : base(nativePlanet) { }

        /// <summary>Wraps a typed interop Planet proxy.</summary>
        /// <example>var wrapper = new PlanetWrapper(myPlanetProxy);</example>
        public PlanetWrapper(Planet nativePlanet) : base(nativePlanet) { }

        /// <summary>
        /// Typed interop proxy of the wrapped planet.
        /// Null when the wrapper is invalid or wraps a non-Planet object.
        /// </summary>
        /// <example>var temp = wrapper.NativePlanet?.GetAverageTemperature();</example>
        public Planet? NativePlanet => GetNativeObject() as Planet;

        /// <summary>Gets a wrapper around the current planet, or null when unavailable.</summary>
        /// <example>var planet = PlanetWrapper.GetCurrent();</example>
        public static PlanetWrapper? GetCurrent()
        {
            var planet = KeeperTypeRegistry.GetPlanet();
            return planet != null ? new PlanetWrapper(planet) : null;
        }

        // ==================== SYSTEMS ====================

        /// <summary>Get HazardsManager for this planet (typed field access).</summary>
        /// <example>var hazards = planet.GetHazardsManager();</example>
        public HazardsManagerWrapper GetHazardsManager()
            => new HazardsManagerWrapper(NativePlanet?.HazardsManager);

        // ==================== COORDINATE CONVERSION ====================

        /// <summary>Convert geographic coordinates to the game's internal 2D position.</summary>
        /// <example>var pos = planet.GetGamePosition(137.4f, -4.6f);</example>
        public UnityEngine.Vector2 GetGamePosition(float longitude, float latitude)
            => NativePlanet?.GetPosition(longitude, latitude) ?? UnityEngine.Vector2.zero;

        /// <summary>Convert a game 2D world position to longitude (degrees).</summary>
        /// <example>float lon = planet.GetLongitude(pos);</example>
        public float GetLongitude(UnityEngine.Vector2 position)
            => NativePlanet?.GetLongitude(position) ?? 0f;

        /// <summary>Convert a game 2D world position to latitude (degrees).</summary>
        /// <example>float lat = planet.GetLatitude(pos);</example>
        public float GetLatitude(UnityEngine.Vector2 position)
            => NativePlanet?.GetLatitude(position) ?? 0f;

        // ==================== PLANET IDENTITY ====================

        /// <summary>Planet name. Hardcoded to "Mars" for Per Aspera.</summary>
        public string Name => "Mars";

        // ==================== TEMPERATURE ====================

        /// <summary>
        /// Global average surface temperature in Kelvin. 0°C = 273.15 K.
        /// Typed call to Planet.GetAverageTemperature().
        /// </summary>
        /// <example>float kelvin = planet.GetAverageTemperature();</example>
        public float GetAverageTemperature()
            => NativePlanet?.GetAverageTemperature() ?? 0f;

        /// <summary>
        /// Surface temperature at a specific geographic position in Kelvin.
        /// Typed call to Planet.GetTemperature(longitude, latitude).
        /// </summary>
        /// <example>float kelvin = planet.GetTemperatureAt(137.4f, -4.6f);</example>
        public float GetTemperatureAt(float longitude, float latitude)
            => NativePlanet?.GetTemperature(longitude, latitude) ?? 0f;

        // ==================== DEPRECATED CLIMATE ====================

        /// <summary>N'a jamais existé : Planet n'a pas de GetTemperature() sans paramètres.</summary>
        [Obsolete("Planet.GetTemperature() sans paramètres n'existe pas dans le jeu — retournait toujours 0. Utiliser GetAverageTemperature() ou GetTemperatureAt(lon, lat).", false)]
        public float GetTemperature() => 0f;

        /// <summary>N'a jamais existé sur Planet — utiliser ClimateController.</summary>
        [Obsolete("Planet.GetAtmosphericPressure() n'existe pas dans le jeu — retournait toujours 0. Utiliser PerAspera.GameAPI.Climate.ClimateController.", false)]
        public float GetAtmosphericPressure() => 0f;

        /// <summary>N'a jamais existé sur Planet — utiliser ClimateController.</summary>
        [Obsolete("Planet.GetOxygenLevel() n'existe pas dans le jeu — retournait toujours 0. Utiliser PerAspera.GameAPI.Climate.ClimateController.", false)]
        public float GetOxygenLevel() => 0f;

        /// <summary>Use PerAspera.GameAPI.Climate.ClimateController.Instance.Atmosphere directly.</summary>
        [Obsolete("Use PerAspera.GameAPI.Climate.ClimateController.Instance.Atmosphere directly", false)]
        public object? Atmosphere => null;

        // ==================== RESOURCES ====================

        /// <summary>
        /// Water stock on the planet.
        /// Typed read/write via the publicized interop proxy (BepInEx.AssemblyPublicizer).
        /// The native setter is private in-game but publicized at compile time — no reflection needed.
        /// </summary>
        /// <example>float water = planet.WaterStock; planet.WaterStock = 5000f;</example>
        public float WaterStock
        {
            get => NativePlanet?.waterStock ?? 0f;
            set { if (NativePlanet != null) NativePlanet.waterStock = value; }
        }

        /// <summary>
        /// Add water to planet stock. Positive = add, negative = remove.
        /// ⚠️ Same caveat as the WaterStock setter: best-effort, failure is logged.
        /// </summary>
        /// <example>planet.AddWaterStock(500f);</example>
        public void AddWaterStock(float amount)
            => WaterStock = WaterStock + amount;

        /// <summary>True when water stock exceeds 1000.</summary>
        public bool HasSufficientWater => WaterStock > 1000f;

        // ==================== DEPRECATED PHANTOM APIs ====================
        // Vérifié dans le dump décompilé (Planet.cs) : ces membres n'existent pas
        // côté jeu. L'ancien SafeInvoke échouait silencieusement et retournait 0/vide.

        /// <summary>N'a jamais fonctionné — Planet n'a pas de stocks de ressources.</summary>
        [Obsolete("Planet.GetResourceStock n'existe pas dans le jeu — retournait toujours 0. Les ressources sont par faction : voir FactionWrapper / ResourceCommandHelper.", false)]
        public float GetResourceStock(string resourceKey) => 0f;

        /// <summary>N'a jamais fonctionné — voir GetResourceStock.</summary>
        [Obsolete("Reposait sur Planet.GetResourceStock qui n'existe pas — retournait toujours 0. Voir FactionWrapper.", false)]
        public float SiliconStock => 0f;

        /// <summary>N'a jamais fonctionné — voir GetResourceStock.</summary>
        [Obsolete("Reposait sur Planet.GetResourceStock qui n'existe pas — retournait toujours 0. Voir FactionWrapper.", false)]
        public float IronStock => 0f;

        /// <summary>N'a jamais fonctionné — voir GetResourceStock.</summary>
        [Obsolete("Reposait sur Planet.GetResourceStock qui n'existe pas — retournait toujours 0. Voir FactionWrapper.", false)]
        public float CarbonStock => 0f;

        /// <summary>N'a jamais fonctionné — voir GetResourceStock.</summary>
        [Obsolete("Reposait sur Planet.GetResourceStock qui n'existe pas — retournait toujours 0. Voir FactionWrapper.", false)]
        public float CalciteStock => 0f;

        /// <summary>N'a jamais fonctionné — reposait sur des stocks fantômes.</summary>
        [Obsolete("Reposait sur Planet.GetResourceStock qui n'existe pas — retournait toujours false. Voir FactionWrapper.", false)]
        public bool HasBalancedResources => false;

        /// <summary>N'a jamais fonctionné — Planet n'a pas d'AddResource.</summary>
        [Obsolete("Planet.AddResource n'existe pas dans le jeu — n'a jamais rien ajouté. Utiliser ResourceCommandHelper ou les commandes faction.", false)]
        public bool AddResource(string resourceKey, float amount) => false;

        // ==================== TERRAIN & WATER ELEVATION ====================

        /// <summary>Raw terrain elevation at world position (metres, no water).</summary>
        /// <example>float alt = planet.GetAltitude(pos);</example>
        public float GetAltitude(UnityEngine.Vector2 position)
            => NativePlanet?.GetAltitude(position) ?? 0f;

        /// <summary>Elevation at world position, returning water surface when submerged.</summary>
        /// <example>float alt = planet.GetAltitudeWithWater(pos);</example>
        public float GetAltitudeWithWater(UnityEngine.Vector2 position)
            => NativePlanet?.GetAltitudeWithWater(position) ?? 0f;

        /// <summary>Global ocean surface elevation — rises as terraforming progresses.</summary>
        /// <example>float level = planet.GetWaterLevel();</example>
        public float GetWaterLevel()
            => NativePlanet?.GetWaterLevel() ?? 0f;

        // ==================== BUILDING MANAGEMENT ====================
        // Les bâtiments appartiennent à Faction (Faction.buildings), pas à Planet.

        /// <summary>N'a jamais fonctionné — les bâtiments sont sur Faction, pas Planet.</summary>
        [Obsolete("Planet n'a pas de liste buildings (elle est sur Faction) — retournait toujours une liste vide. Utiliser FactionWrapper.GetBuildings().", false)]
        public List<BuildingWrapper> GetBuildings() => new List<BuildingWrapper>();

        /// <summary>N'a jamais fonctionné — voir GetBuildings.</summary>
        [Obsolete("Planet n'a pas de liste buildings — retournait toujours un tableau vide. Utiliser FactionWrapper.GetBuildings().", false)]
        public BuildingWrapper[] GetBuildingsSafely() => Array.Empty<BuildingWrapper>();

        /// <summary>
        /// Get all buildings owned by a specific faction.
        /// Délègue à FactionWrapper.GetBuildings() (Faction.buildings existe côté jeu).
        /// </summary>
        /// <example>var buildings = planet.GetBuildingsByFaction(faction);</example>
        public List<BuildingWrapper> GetBuildingsByFaction(FactionWrapper faction)
            => faction.IsValidWrapper ? faction.GetBuildings() : new List<BuildingWrapper>();
    }
}
