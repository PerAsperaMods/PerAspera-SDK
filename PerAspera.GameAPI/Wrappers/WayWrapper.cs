#nullable enable

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native Way class (routing path segment).
    /// MIGRATION 2026-06-10 — interop typé (Way._keeper exposé par le proxy).
    /// </summary>
    public class Way : WrapperBase
    {
        /// <summary>Wraps an untyped native way (compat). Prefer the typed overload.</summary>
        public Way(object nativeWay) : base(nativeWay) { }

        /// <summary>Wraps a typed interop Way proxy.</summary>
        public Way(global::Way nativeWay) : base(nativeWay) { }

        /// <summary>Typed interop proxy (null when the wrapper is invalid).</summary>
        public global::Way? NativeWay => GetNativeObject() as global::Way;

        /// <summary>Factory — retourne null si l'objet natif est null.</summary>
        public static Way? FromNative(object? native)
            => native != null ? new Way(native) : null;

        /// <summary>Keeper this way belongs to (typed read of Way._keeper).</summary>
        public KeeperWrapper? GetKeeper()
        {
            var keeper = NativeWay?._keeper;
            return keeper != null ? new KeeperWrapper(keeper) : null;
        }
    }
}
