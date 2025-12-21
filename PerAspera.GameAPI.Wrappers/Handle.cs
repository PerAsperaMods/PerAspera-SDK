using PerAspera.GameAPI.Wrappers.Core;
using System;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for Handle struct with safe access to native handle properties
    /// </summary>
    public class HandleWrapper : WrapperBase
    {
        /// <summary>
        /// Initialize HandleWrapper with native handle object
        /// </summary>
        /// <param name="nativeHandle">Native Handle instance from game</param>
        public HandleWrapper(object nativeHandle) : base(nativeHandle)
        {
        }

        /// <summary>
        /// Get handle index value
        /// </summary>
        public int Index => GetNativeField<int>("index");

        /// <summary>
        /// Get handle version value  
        /// </summary>
        public int Version => GetNativeField<int>("version");

        /// <summary>
        /// Check if handle is valid (non-zero index)
        /// </summary>
        public bool IsValid => Index > 0;

        /// <summary>
        /// Create HandleWrapper from native handle object
        /// </summary>
        /// <param name="nativeHandle">Native handle object</param>
        /// <returns>HandleWrapper instance</returns>
        public static HandleWrapper FromNative(object nativeHandle)
        {
            return new HandleWrapper(nativeHandle);
        }

        /// <summary>
        /// String representation of handle
        /// </summary>
        /// <returns>HandleWrapper info string</returns>
        public override string ToString()
        {
            return $"HandleWrapper(index: {Index}, version: {Version}, valid: {IsValid})";
        }
    }
}
