using System;
using System.Collections.Generic;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;


#pragma warning disable CS1591
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
    public class VisualPointOfInterestWrapper : WrapperBase
    {
        private static readonly LogAspera Log = new LogAspera("VisualPointOfInterestWrapper");
        private VisualPointOfInterest? _nativeVisualPointOfInterest;

        public VisualPointOfInterestWrapper(object nativeVisualPointOfInterest) : base(nativeVisualPointOfInterest)
        {
            try
            {
                _nativeVisualPointOfInterest = nativeVisualPointOfInterest as VisualPointOfInterest;
            }
            catch (Exception ex)
            {
                Log.Warning($"Failed to cast to VisualPointOfInterest: {ex.Message}");
            }
        }
        public PointOfInterestWrapper? get_poi()
        {
            var poi = _nativeVisualPointOfInterest?.poi;
            return poi != null ? new PointOfInterestWrapper(poi) : null;
        }

    }
}
#pragma warning restore CS1591
