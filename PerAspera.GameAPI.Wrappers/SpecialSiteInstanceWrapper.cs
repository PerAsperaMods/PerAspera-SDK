using System;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Native;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for a single SpecialSite instance from the game
    /// Provides safe, type-safe access to SpecialSite properties
    ///
    /// Properties:
    /// - Name: Site display name (String)
    /// - Coordinates: Latitude, Longitude
    ///
    /// Example:
    /// var site = new SpecialSiteInstanceWrapper(nativeSite);
    /// var name = site.Name;
    /// var lat = site.CenterLatitude;
    /// </summary>
    public class SpecialSiteInstanceWrapper : WrapperBase
    {
        private static readonly LogAspera Log = new LogAspera("SpecialSiteInstanceWrapper");

        public SpecialSiteInstanceWrapper(object native) : base(native)
        {
        }

        /// <summary>
        /// Create wrapper from native SpecialSite instance
        /// </summary>
        public static SpecialSiteInstanceWrapper? FromNative(object? native)
        {
            return native != null ? new SpecialSiteInstanceWrapper(native) : null;
        }

        // AUTO-GENERATED SHELL ABOVE - DO NOT EDIT ABOVE THIS LINE
        // MANUAL ADDITIONS BELOW THIS LINE WILL BE PRESERVED

        /// <summary>Site display name via GetName(bool researched)</summary>
        public string? Name => SafeInvoke<string>("GetName", true);

        /// <summary>
        /// geoPosition.x = longitude, geoPosition.y = latitude (from YAML: x=longitude, y=latitude)
        /// </summary>
        private UnityEngine.Vector2? GetGeoPosition()
        {
            try { return SafeInvoke<UnityEngine.Vector2>("get_geoPosition"); }
            catch { return null; }
        }

        /// <summary>Site center latitude (geoPosition.y)</summary>
        public float? CenterLatitude => GetGeoPosition()?.y;

        /// <summary>Site center longitude (geoPosition.x)</summary>
        public float? CenterLongitude => GetGeoPosition()?.x;
    }
}
