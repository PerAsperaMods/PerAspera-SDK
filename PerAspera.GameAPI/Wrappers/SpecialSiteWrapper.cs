using System;
using PerAspera.Core;
using UnityEngine;


#pragma warning disable CS1591
namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper around a native SpecialSite static-data object.
    /// Exposes name, geo-position, and site class without touching StaticValues.
    /// </summary>
    public class SpecialSiteWrapper : WrapperBase
    {
        private static readonly LogAspera Log = new LogAspera("SpecialSiteWrapper");
        private SpecialSite? _native;

        public SpecialSiteWrapper(object native) : base(native)
        {
            try   { _native = (SpecialSite)native; }
            catch (Exception ex) { Log.Error($"Failed to cast to SpecialSite: {ex.Message}"); }
        }

        /// <summary>Localised name — researched version if available, unresearched fallback.</summary>
        public string Name => _native?.GetName(true) ?? _native?.GetName(false) ?? "";

        /// <summary>Site class enum as string: RUINS, CAVERN, ROVER, CRASH, LANDER, OTHER.</summary>
        public string Category => _native?.siteClass.ToString() ?? "UNKNOWN";

        /// <summary>Geographic position — x = latitude, y = longitude (degrees).</summary>
        public Vector2? GeoPosition => _native?.geoPosition;

        public float Latitude  => _native?.geoPosition.x ?? 0f;
        public float Longitude => _native?.geoPosition.y ?? 0f;
    }
}
#pragma warning restore CS1591
