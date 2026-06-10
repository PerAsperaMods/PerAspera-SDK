#nullable enable

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native GameEventBus class.
    /// MIGRATION 2026-06-10 — interop typé (GameEventBus._keeper exposé par le proxy).
    /// </summary>
    public class GameEventBusWrapper : WrapperBase
    {
        /// <summary>Wraps an untyped native event bus (compat). Prefer the typed overload.</summary>
        public GameEventBusWrapper(object nativeEventBus) : base(nativeEventBus) { }

        /// <summary>Wraps a typed interop GameEventBus proxy.</summary>
        public GameEventBusWrapper(GameEventBus nativeEventBus) : base(nativeEventBus) { }

        /// <summary>Typed interop proxy (null when the wrapper is invalid).</summary>
        public GameEventBus? NativeGameEventBus => GetNativeObject() as GameEventBus;

        /// <summary>Keeper of this event bus (typed read of GameEventBus._keeper).</summary>
        public KeeperWrapper? GetKeeper()
        {
            var keeper = NativeGameEventBus?._keeper;
            return keeper != null ? new KeeperWrapper(keeper) : null;
        }
    }
}
