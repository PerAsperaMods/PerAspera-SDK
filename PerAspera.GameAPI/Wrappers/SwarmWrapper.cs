#nullable enable

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native Swarm class (drone swarm).
    /// MIGRATION 2026-06-10 — interop typé (Swarm._keeper exposé par le proxy).
    /// </summary>
    public class Swarm : WrapperBase
    {
        /// <summary>Wraps an untyped native swarm (compat). Prefer the typed overload.</summary>
        public Swarm(object nativeSwarm) : base(nativeSwarm) { }

        /// <summary>Wraps a typed interop Swarm proxy.</summary>
        public Swarm(global::Swarm nativeSwarm) : base(nativeSwarm) { }

        /// <summary>Typed interop proxy (null when the wrapper is invalid).</summary>
        public global::Swarm? NativeSwarm => GetNativeObject() as global::Swarm;

        /// <summary>Factory — retourne null si l'objet natif est null.</summary>
        public static Swarm? FromNative(object? native)
            => native != null ? new Swarm(native) : null;

        /// <summary>Keeper this swarm belongs to (typed read of Swarm._keeper).</summary>
        public KeeperWrapper? GetKeeper()
        {
            var keeper = NativeSwarm?._keeper;
            return keeper != null ? new KeeperWrapper(keeper) : null;
        }
    }
}
