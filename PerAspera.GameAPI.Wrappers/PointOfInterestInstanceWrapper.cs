using System;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Native;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for a single PointOfInterest instance from the game
    /// Provides safe, type-safe access to POI properties and methods
    ///
    /// Properties:
    /// - Name: POI display name (String)
    /// - PoiType: POI type (POIType enum/class)
    /// - Coordinates: Latitude, Longitude
    /// - Bounds: Northern/Southern/Eastern/Western latitudes/longitudes
    /// - Diameter: POI size
    ///
    /// Example:
    /// var poi = new PointOfInterestInstanceWrapper(nativePoi);
    /// var name = poi.Name;
    /// var type = poi.PoiType;
    /// </summary>
    public class PointOfInterestInstanceWrapper : WrapperBase
    {
        private static readonly LogAspera Log = new LogAspera("PointOfInterestInstanceWrapper");

        public PointOfInterestInstanceWrapper(object native) : base(native)
        {
        }

        /// <summary>
        /// Create wrapper from native PointOfInterest instance
        /// </summary>
        public static PointOfInterestInstanceWrapper? FromNative(object? native)
        {
            return native != null ? new PointOfInterestInstanceWrapper(native) : null;
        }

        // AUTO-GENERATED SHELL ABOVE - DO NOT EDIT ABOVE THIS LINE
        // MANUAL ADDITIONS BELOW THIS LINE WILL BE PRESERVED

        /// <summary>POI display name</summary>
        public string? Name => GetProperty<string>("name");

        /// <summary>POI type (POIType enum/class)</summary>
        public object? PoiType => GetProperty<object>("poiType");

        /// <summary>POI center latitude</summary>
        public float? CenterLatitude => GetProperty<float?>("centerLatitude");

        /// <summary>POI center longitude</summary>
        public float? CenterLongitude => GetProperty<float?>("centerLongitude");

        /// <summary>POI diameter</summary>
        public float? Diameter => GetProperty<float?>("diameter");

        /// <summary>Northern boundary latitude</summary>
        public float? NorthernLatitude => GetProperty<float?>("northernLatitude");

        /// <summary>Southern boundary latitude</summary>
        public float? SouthernLatitude => GetProperty<float?>("southernLatitude");

        /// <summary>Eastern boundary longitude</summary>
        public float? EasternLongitude => GetProperty<float?>("easternLongitude");

        /// <summary>Western boundary longitude</summary>
        public float? WesternLongitude => GetProperty<float?>("westernLongitude");

        /// <summary>
        /// Get POI type as string (converts POIType enum to string)
        /// </summary>
        public string? GetPoiTypeAsString()
        {
            var poiTypeObj = PoiType;
            return poiTypeObj?.ToString();
        }

        /// <summary>
        /// Get POI name via method call
        /// </summary>
        public string? GetNameViaMethod()
        {
            return InvokeMethod<string>("GetName");
        }

        /// <summary>
        /// Get POI latitude via method call
        /// </summary>
        public float? GetLatitude()
        {
            return InvokeMethod<float?>("GetLatitude");
        }

        /// <summary>
        /// Get POI longitude via method call
        /// </summary>
        public float? GetLongitude()
        {
            return InvokeMethod<float?>("GetLongitude");
        }

        /// <summary>
        /// Check if a point is inside this POI
        /// </summary>
        public bool IsPointInside(object? planet, float latitude, float longitude)
        {
            // Vector2 geoPos = new Vector2(longitude, latitude)
            // This requires Planet wrapper and Vector2 conversion
            // For now, just a placeholder
            return false;
        }
    }
}
