#nullable enable

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native MarsManager class (planet visuals).
    /// MIGRATION 2026-06-10 — interop typé (marsRenderer/waterRenderer exposés par le proxy).
    /// </summary>
    public class MarsManagerWrapper : WrapperBase
    {
        /// <summary>Wraps an untyped native MarsManager (compat). Prefer the typed overload.</summary>
        public MarsManagerWrapper(object? nativeObject) : base(nativeObject) { }

        /// <summary>Wraps a typed interop MarsManager proxy.</summary>
        public MarsManagerWrapper(MarsManager nativeObject) : base(nativeObject) { }

        /// <summary>Typed interop proxy (null when the wrapper is invalid).</summary>
        public MarsManager? NativeMarsManager => GetNativeObject() as MarsManager;

        /// <summary>Water mesh renderer (typed read of MarsManager.waterRenderer).</summary>
        public MeshRendererWrapper? waterRenderer
        {
            get
            {
                var renderer = NativeMarsManager?.waterRenderer;
                return renderer != null ? new MeshRendererWrapper(renderer) : null;
            }
        }

        /// <summary>Mars surface mesh renderer (typed read of MarsManager.marsRenderer).</summary>
        public UnityEngine.MeshRenderer? marsRenderer => NativeMarsManager?.marsRenderer;
    }
}
