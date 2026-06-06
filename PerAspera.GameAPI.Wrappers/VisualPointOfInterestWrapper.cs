using System;
using System.Collections.Generic;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the list of VisualPointOfInterest instances in the game.
    /// VisualPointOfInterest = the in-game visual marker for POI on the map.
    ///
    /// Example:
    /// var baseGame = BaseGameWrapper.GetCurrent();
    /// var visualPois = baseGame.VisualPOIs;
    /// foreach (var vPoi in visualPois)
    /// {
    ///     var poi = vPoi.GetPOI();
    ///     var name = poi.GetProperty("name");
    /// }
    /// </summary>
    public class VisualPointOfInterestListWrapper
    {
        private static readonly LogAspera Log = new LogAspera("VisualPointOfInterestListWrapper");
        private readonly object _nativeList;
        private IList<object>? _cachedList;

        public VisualPointOfInterestListWrapper(object nativeList)
        {
            _nativeList = nativeList;
        }

        /// <summary>
        /// Get all visual POI as managed list
        /// Caches the conversion for performance
        /// </summary>
        public IList<object> GetAll()
        {
            if (_cachedList != null)
                return _cachedList;

            _cachedList = _nativeList.ConvertIl2CppList<object>() ?? new List<object>();
            Log.Info($"Loaded {_cachedList.Count} VisualPointOfInterest instances");
            return _cachedList;
        }

        /// <summary>
        /// Get POI from a VisualPointOfInterest wrapper
        /// </summary>
        public static object? GetPOIFromVisualPOI(object visualPoi)
        {
            if (visualPoi == null)
                return null;

            return IL2CppPropertyReader.ReadProperty<object>(visualPoi, "poi");
        }

        /// <summary>
        /// Get POI name from a VisualPointOfInterest
        /// </summary>
        public static string? GetNameFromVisualPOI(object visualPoi)
        {
            var poi = GetPOIFromVisualPOI(visualPoi);
            if (poi == null)
                return null;

            return IL2CppPropertyReader.ReadProperty<string>(poi, "name");
        }

        /// <summary>
        /// Count of visual POI
        /// </summary>
        public int Count
        {
            get
            {
                var list = GetAll();
                return list?.Count ?? 0;
            }
        }
    }
}
