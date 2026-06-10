using System;
using PerAspera.Core;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for a single PointOfInterest instance.
    /// Uses direct IL2CPP cast — no reflection.
    ///
    /// Source: PointOfInterest.cs (IL2CPP dump)
    /// Public methods: GetName(), GetLatitude(), GetLongitude(), GetDiameter(), GetPOIType()
    /// Fields centerLatitude/centerLongitude are protected — accessed via the public methods above.
    ///
    /// <example>
    /// var w = new PointOfInterestWrapper(nativePoi);
    /// string name = w.Name;
    /// float lat   = w.CenterLatitude ?? 0f;  // -13.37 for Coprates Chasma
    /// float lon   = w.CenterLongitude ?? 0f; // -60.74 for Coprates Chasma
    /// </example>
    /// </summary>
    public class PointOfInterestWrapper : WrapperBase
    {
        private static readonly LogAspera Log = new LogAspera("PointOfInterestWrapper");
        private PointOfInterest? _native;

        public PointOfInterestWrapper(object native) : base(native)
        {
            _native = native as PointOfInterest;
        }

        public static PointOfInterestWrapper? FromNative(object? native)
            => native != null ? new PointOfInterestWrapper(native) : null;

        // ── Name / Type ───────────────────────────────────────────────────────

        /// <summary>POI display name</summary>
        public string? Name => _native?.GetName();

        /// <summary>POI type enum</summary>
        public PointOfInterest.POIType? PoiType => _native?.GetPOIType();

        /// <summary>POI type as display string (e.g. "Chasma", "Mons", "Crater")</summary>
        public string? GetPoiTypeAsString() => _native?.GetPOIType().ToString();

        // ── Coordinates ───────────────────────────────────────────────────────

        /// <summary>POI center latitude (°N positive, °S negative)</summary>
        public float? CenterLatitude => _native?.GetLatitude();

        /// <summary>POI center longitude (°E positive, °W negative)</summary>
        public float? CenterLongitude => _native?.GetLongitude();

        /// <summary>POI diameter in km</summary>
        public float? Diameter => _native?.GetDiameter();

        // ── Geometric helpers ─────────────────────────────────────────────────

        /// <summary>Check if a geographic point is inside this POI's bounds</summary>
        public bool IsPointInside(Planet planet, UnityEngine.Vector2 geoPos)
        {
            try { return _native?.IsPointInside(planet, geoPos) ?? false; }
            catch { return false; }
        }
    }
}
