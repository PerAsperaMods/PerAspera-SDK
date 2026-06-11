#nullable enable
using PerAspera.Core;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for a single SpecialSite instance from the game.
    /// MIGRATION 2026-06-10 — interop typé (GetName/geoPosition exposés par le proxy).
    ///
    /// geoPosition.x = longitude, geoPosition.y = latitude (convention YAML).
    /// </summary>
    /// <example>
    /// var site = new SpecialSiteInstanceWrapper(nativeSite);
    /// var name = site.Name;
    /// var lat = site.CenterLatitude;
    /// </example>
    public class SpecialSiteInstanceWrapper : WrapperBase
    {
        private static readonly LogAspera Log = new LogAspera("SpecialSiteInstanceWrapper");

        /// <summary>Wraps an untyped native site (compat). Prefer the typed overload.</summary>
        public SpecialSiteInstanceWrapper(object native) : base(native) { }

        /// <summary>Wraps a typed interop SpecialSite proxy.</summary>
        public SpecialSiteInstanceWrapper(SpecialSite native) : base(native) { }

        /// <summary>Typed interop proxy (null when the wrapper is invalid).</summary>
        public SpecialSite? NativeSpecialSite => GetNativeObject() as SpecialSite;

        /// <summary>Create wrapper from native SpecialSite instance.</summary>
        public static SpecialSiteInstanceWrapper? FromNative(object? native)
            => native != null ? new SpecialSiteInstanceWrapper(native) : null;

        /// <summary>Site display name (typed call to SpecialSite.GetName(researched: true)).</summary>
        public string? Name => NativeSpecialSite?.GetName(true);

        /// <summary>Site center latitude (typed read of geoPosition.y).</summary>
        public float? CenterLatitude => NativeSpecialSite?.geoPosition.y;

        /// <summary>Site center longitude (typed read of geoPosition.x).</summary>
        public float? CenterLongitude => NativeSpecialSite?.geoPosition.x;
    }
}
