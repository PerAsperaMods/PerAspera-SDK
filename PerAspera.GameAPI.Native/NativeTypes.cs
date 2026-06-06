// NativeTypes.cs � Thin native instance holders for Wrappers compatibility.
// Native IL2CPP types (BaseGame, Planet, Universe, etc.) come from GameLibs.Complete.
// These helpers are kept only for Wrapper internal usage.

namespace PerAspera.GameAPI.Native
{
    /// <summary>Thin holder for native Building instance (used by BuildingWrapper internals).</summary>
    public class BuildingNative
    {
        public object NativeInstance { get; }
        public BuildingNative(object native) => NativeInstance = native;
    }

    /// <summary>Thin holder for native Planet instance (used by PlanetWrapper internals).</summary>
    public class PlanetNative
    {
        public object NativeInstance { get; }
        public PlanetNative(object native) => NativeInstance = native;
    }

    /// <summary>Thin holder for native BuildingType instance (used by BuildingWrapper internals).</summary>
    public class BuildingTypeNative
    {
        public object NativeInstance { get; }
        public BuildingTypeNative(object native) => NativeInstance = native;
    }

    /// <summary>Thin holder for native Swarm instance.</summary>
    public class SwarmNative
    {
        public object NativeInstance { get; }
        public SwarmNative(object native) => NativeInstance = native;
    }

    /// <summary>Thin holder for native Way instance.</summary>
    public class WayNative
    {
        public object NativeInstance { get; }
        public WayNative(object native) => NativeInstance = native;
    }

    /// <summary>Thin holder for native VisualPointOfInterest instance (visual POI marker in game).</summary>
    public class VisualPointOfInterestNative
    {
        public object NativeInstance { get; }
        public VisualPointOfInterestNative(object native) => NativeInstance = native;
    }

    /// <summary>Thin holder for native PointOfInterest instance (POI static data).</summary>
    public class PointOfInterestNative
    {
        public object NativeInstance { get; }
        public PointOfInterestNative(object native) => NativeInstance = native;
    }

    /// <summary>Thin holder for native SpecialSite instance (special site data).</summary>
    public class SpecialSiteNative
    {
        public object NativeInstance { get; }
        public SpecialSiteNative(object native) => NativeInstance = native;
    }
}
