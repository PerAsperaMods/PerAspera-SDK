#nullable enable

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native BaseGameReferences class.
    /// MIGRATION 2026-06-10 — interop typé (BaseGameReferences.mars exposé par le proxy).
    /// </summary>
    public class BaseGameReferencesWrapper : WrapperBase
    {
        /// <summary>Wraps an untyped native references object (compat). Prefer the typed overload.</summary>
        public BaseGameReferencesWrapper(object? nativeObject) : base(nativeObject) { }

        /// <summary>Wraps a typed interop BaseGameReferences proxy.</summary>
        public BaseGameReferencesWrapper(BaseGameReferences nativeObject) : base(nativeObject) { }

        /// <summary>Typed interop proxy (null when the wrapper is invalid).</summary>
        public BaseGameReferences? NativeBaseGameReferences => GetNativeObject() as BaseGameReferences;

        /// <summary>Mars manager (typed read of BaseGameReferences.mars).</summary>
        public MarsManagerWrapper? Mars
        {
            get
            {
                var mars = NativeBaseGameReferences?.mars;
                return mars != null ? new MarsManagerWrapper(mars) : null;
            }
        }
    }
}
