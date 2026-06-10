#nullable enable

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native SliceMaster class (game timing and tick management).
    /// MIGRATION 2026-06-10 — interop typé : tous les membres délèguent au proxy.
    ///
    /// 🤖 Agent Expert: @per-aspera-sdk-coordinator
    /// </summary>
    public class SliceMasterWrapper : WrapperBase
    {
        /// <summary>Wraps an untyped native SliceMaster (compat). Prefer the typed overload.</summary>
        public SliceMasterWrapper(object? sliceMaster) : base(sliceMaster) { }

        /// <summary>Wraps a typed interop SliceMaster proxy.</summary>
        public SliceMasterWrapper(SliceMaster sliceMaster) : base(sliceMaster) { }

        /// <summary>Typed interop proxy (null when the wrapper is invalid).</summary>
        public SliceMaster? NativeSliceMaster => GetNativeObject() as SliceMaster;

        /// <summary>
        /// Tick time multiplier for game speed control (typed).
        /// Values between 0.001 and 10.0 are typically supported.
        /// </summary>
        /// <example>universe.GetSliceMaster().tickTimeMultiplier = 2.0f; // 2x speed</example>
        public float tickTimeMultiplier
        {
            get => NativeSliceMaster?.tickTimeMultiplier ?? 1f;
            set { var sm = NativeSliceMaster; if (sm != null) sm.tickTimeMultiplier = value; }
        }

        /// <summary>
        /// Base tick time used by the game engine, before multipliers (typed).
        /// </summary>
        /// <example>var currentTick = sliceMaster.tickTime;</example>
        public float tickTime
        {
            get => NativeSliceMaster?.tickTime ?? 0f;
            set { var sm = NativeSliceMaster; if (sm != null) sm.tickTime = value; }
        }

        /// <summary>Real frame-to-frame delta time used by the timing system (typed).</summary>
        /// <example>var deltaTime = sliceMaster.realDeltaTime;</example>
        public float realDeltaTime
        {
            get => NativeSliceMaster?.realDeltaTime ?? 0f;
            set { var sm = NativeSliceMaster; if (sm != null) sm.realDeltaTime = value; }
        }

        /// <summary>Timer accumulator for tick timing management (typed).</summary>
        public float timerAcc
        {
            get => NativeSliceMaster?.timerAcc ?? 0f;
            set { var sm = NativeSliceMaster; if (sm != null) sm.timerAcc = value; }
        }

        /// <summary>Number of frames between tick processing (typed).</summary>
        public int framesBetween
        {
            get => NativeSliceMaster?.framesBetween ?? 0;
            set { var sm = NativeSliceMaster; if (sm != null) sm.framesBetween = value; }
        }
    }
}
