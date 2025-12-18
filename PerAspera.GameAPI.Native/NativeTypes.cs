using System;

namespace PerAspera.GameAPI.Native
{
    /// <summary>
    /// Native BaseGame type alias for Enhanced Events
    /// Represents the actual IL2CPP native BaseGame object
    /// Use PerAspera.GameAPI.Wrappers.BaseGame for SDK wrapper access
    /// </summary>
    public class BaseGame
    {
        /// <summary>Native IL2CPP BaseGame instance</summary>
        public object NativeInstance { get; }
        
        public BaseGame(object nativeInstance)
        {
            NativeInstance = nativeInstance ?? throw new ArgumentNullException(nameof(nativeInstance));
        }
    }
    
    /// <summary>
    /// Native Universe type alias for Enhanced Events
    /// Represents the actual IL2CPP native Universe object
    /// Use PerAspera.GameAPI.Wrappers.Universe for SDK wrapper access
    /// </summary>
    public class Universe
    {
        /// <summary>Native IL2CPP Universe instance</summary>
        public object NativeInstance { get; }
        
        public Universe(object nativeInstance)
        {
            NativeInstance = nativeInstance ?? throw new ArgumentNullException(nameof(nativeInstance));
        }
    }
    
    /// <summary>
    /// Native Planet type alias for Enhanced Events
    /// Represents the actual IL2CPP native Planet object
    /// Use PerAspera.GameAPI.Wrappers.Planet for SDK wrapper access
    /// </summary>
    public class Planet
    {
        /// <summary>Native IL2CPP Planet instance</summary>
        public object NativeInstance { get; }
        
        public Planet(object nativeInstance)
        {
            NativeInstance = nativeInstance ?? throw new ArgumentNullException(nameof(nativeInstance));
        }
    }
}