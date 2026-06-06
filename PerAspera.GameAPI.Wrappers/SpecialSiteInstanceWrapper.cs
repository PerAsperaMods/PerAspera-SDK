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

        /// <summary>Site display name</summary>
        public string? Name => SafeInvoke<string>("get_name");

        /// <summary>Site center latitude</summary>
        public float? CenterLatitude => SafeInvoke<float?>("get_centerLatitude");

        /// <summary>Site center longitude</summary>
        public float? CenterLongitude => SafeInvoke<float?>("get_centerLongitude");

        /// <summary>
        /// Get site name via method call
        /// </summary>
        public string? GetNameViaMethod()
        {
            return SafeInvoke<string>("GetName");
        }

        /// <summary>
        /// Get site latitude via method call
        /// </summary>
        public float? GetLatitude()
        {
            return SafeInvoke<float?>("GetLatitude");
        }

        /// <summary>
        /// Get site longitude via method call
        /// </summary>
        public float? GetLongitude()
        {
            return SafeInvoke<float?>("GetLongitude");
        }
    }
}
