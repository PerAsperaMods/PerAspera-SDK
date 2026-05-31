using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Native;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native SliceMaster class
    /// Provides safe access to game timing and tick management
    /// 
    /// 📚 Based on: SliceMaster (ScriptsAssembly) with tickTime, tickTimeMultiplier, realDeltaTime properties
    /// 🤖 Agent Expert: @per-aspera-sdk-coordinator
    /// 🌐 User Wiki: https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki/sdk/
    /// ⏰ Timing System: Game speed, tick timing, real-time management
    /// </summary>
    public class SliceMasterWrapper : WrapperBase
    {
        public SliceMasterWrapper(object sliceMaster) : base(sliceMaster)
        {
        }

        /// <summary>
        /// Gets or sets the tick time multiplier for game speed control.
        /// Values between 0.001 and 10.0 are typically supported.
        /// </summary>
        /// <example>
        /// <code>
        /// var sliceMaster = universe.GetSliceMaster();
        /// sliceMaster.tickTimeMultiplier = 2.0f; // 2x speed
        /// sliceMaster.tickTimeMultiplier = 0.5f; // Half speed
        /// </code>
        /// </example>
        public float tickTimeMultiplier 
        { 
            get { return (float)NativeObject.GetMemberValue("tickTimeMultiplier"); } 
            set { NativeObject.SetMemberValue("tickTimeMultiplier", value); } 
        }

        /// <summary>
        /// Gets or sets the actual tick time used by the game engine.
        /// This is the base timing value before multipliers are applied.
        /// </summary>
        /// <example>
        /// <code>
        /// var sliceMaster = universe.GetSliceMaster();
        /// var currentTick = sliceMaster.tickTime;
        /// sliceMaster.tickTime = 0.05f; // Direct tick time control
        /// </code>
        /// </example>
        public float tickTime 
        { 
            get { return (float)NativeObject.GetMemberValue("tickTime"); } 
            set { NativeObject.SetMemberValue("tickTime", value); } 
        }

        /// <summary>
        /// Gets or sets the real delta time used by the timing system.
        /// This represents the actual frame-to-frame time progression.
        /// </summary>
        /// <example>
        /// <code>
        /// var sliceMaster = universe.GetSliceMaster();
        /// var deltaTime = sliceMaster.realDeltaTime;
        /// </code>
        /// </example>
        public float realDeltaTime 
        { 
            get { return (float)NativeObject.GetMemberValue("realDeltaTime"); } 
            set { NativeObject.SetMemberValue("realDeltaTime", value); } 
        }

        /// <summary>
        /// Gets or sets the timer accumulator for tick timing management.
        /// </summary>
        public float timerAcc 
        { 
            get { return (float)NativeObject.GetMemberValue("timerAcc"); } 
            set { NativeObject.SetMemberValue("timerAcc", value); } 
        }

        /// <summary>
        /// Gets or sets the number of frames between tick processing.
        /// </summary>
        public int framesBetween 
        { 
            get { return (int)NativeObject.GetMemberValue("framesBetween"); } 
            set { NativeObject.SetMemberValue("framesBetween", value); } 
        }
    }
}