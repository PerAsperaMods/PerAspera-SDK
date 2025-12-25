using System;

namespace PerAspera.GameAPI.Native
{
    /// <summary>
    /// Native BaseGame type alias for Enhanced Events
    /// Represents the actual IL2CPP native BaseGame object
    /// Use PerAspera.GameAPI.Wrappers.BaseGameWrapper for SDK wrapper access
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

    

        public class HazardsManager
    {
        /// <summary>Native IL2CPP BaseGame instance</summary>
        public object NativeInstance { get; }

        public HazardsManager(object nativeInstance)
        {
            NativeInstance = nativeInstance ?? throw new ArgumentNullException(nameof(nativeInstance));
        }
    }
    public class IHandleable
    {
        /// <summary>Native IL2CPP BaseGame instance</summary>
        public object NativeInstance { get; }

        public IHandleable(object nativeInstance)
        {
            NativeInstance = nativeInstance ?? throw new ArgumentNullException(nameof(nativeInstance));
        }
    }
    public class InteractionManagerWrapper
    {
        /// <summary>Native IL2CPP BaseGame instance</summary>
        public object NativeInstance { get; }

        public InteractionManagerWrapper(object nativeInstance)
        {
            NativeInstance = nativeInstance ?? throw new ArgumentNullException(nameof(nativeInstance));
        }
    }
    
    public class Handle
    {
        /// <summary>Native IL2CPP BaseGame instance</summary>
        public object NativeInstance { get; }

        public Handle(object nativeInstance)
        {
            NativeInstance = nativeInstance ?? throw new ArgumentNullException(nameof(nativeInstance));
        }
    }


    public class ResourceType
    {
        /// <summary>Native IL2CPP BaseGame instance</summary>
        public object NativeInstance { get; }

        public ResourceType(object nativeInstance)
        {
            NativeInstance = nativeInstance ?? throw new ArgumentNullException(nameof(nativeInstance));
        }
    }
    public class Way
    {
        /// <summary>Native IL2CPP BaseGame instance</summary>
        public object NativeInstance { get; }

        public Way(object nativeInstance)
        {
            NativeInstance = nativeInstance ?? throw new ArgumentNullException(nameof(nativeInstance));
        }
    }
    public class Swarm
    {
        /// <summary>Native IL2CPP BaseGame instance</summary>
        public object NativeInstance { get; }

        public Swarm(object nativeInstance)
        {
            NativeInstance = nativeInstance ?? throw new ArgumentNullException(nameof(nativeInstance));
        }
    }
    

        public class TextAction
    {
        /// <summary>Native IL2CPP BaseGame instance</summary>
        public object NativeInstance { get; }

        public TextAction(object nativeInstance)
        {
            NativeInstance = nativeInstance ?? throw new ArgumentNullException(nameof(nativeInstance));
        }
    }
    public class FinishInjectionContext
    {
        /// <summary>Native IL2CPP BaseGame instance</summary>
        public object NativeInstance { get; }

        public FinishInjectionContext(object nativeInstance)
        {
            NativeInstance = nativeInstance ?? throw new ArgumentNullException(nameof(nativeInstance));
        }
    }
    public class KeeperMap
    {
        /// <summary>Native IL2CPP BaseGame instance</summary>
        public object NativeInstance { get; }

        public KeeperMap(object nativeInstance)
        {
            NativeInstance = nativeInstance ?? throw new ArgumentNullException(nameof(nativeInstance));
        }
    }
    public class GameEventBus
    {
        /// <summary>Native IL2CPP BaseGame instance</summary>
        public object NativeInstance { get; }

        public GameEventBus(object nativeInstance)
        {
            NativeInstance = nativeInstance ?? throw new ArgumentNullException(nameof(nativeInstance));
        }
    }

    public class BuildingType
    {
        /// <summary>Native IL2CPP BaseGame instance</summary>
        public object NativeInstance { get; }

        public BuildingType(object nativeInstance)
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
    public class Building
    {
        /// <summary>Native IL2CPP Universe instance</summary>
        public object NativeInstance { get; }

        public Building(object nativeInstance)
        {
            NativeInstance = nativeInstance ?? throw new ArgumentNullException(nameof(nativeInstance));
        }
    }


    public class Blackboard
    {
        /// <summary>Native IL2CPP Universe instance</summary>
        public object NativeInstance { get; }

        public Blackboard(object nativeInstance)
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
    public class Faction 
    {
        /// <summary>Native IL2CPP Planet instance</summary>
        public object NativeInstance { get; }

        public Faction(object nativeInstance)
        {
            NativeInstance = nativeInstance ?? throw new ArgumentNullException(nameof(nativeInstance));
        }

    }

    /// <summary>
    /// Native Console type alias for Enhanced Events
    /// Represents the actual IL2CPP native Console object
    /// Use PerAspera.GameAPI.Wrappers.ConsoleWrapper for SDK wrapper access
    /// </summary>
    public class Console
    {
        /// <summary>Native IL2CPP Console instance</summary>
        public object NativeInstance { get; }
        
        public Console(object nativeInstance)
        {
            NativeInstance = nativeInstance ?? throw new ArgumentNullException(nameof(nativeInstance));
        }
    }

    /// <summary>
    /// Native Building type alias for Enhanced Events
    /// Represents the actual IL2CPP native Building object
    /// Use PerAspera.GameAPI.Wrappers.BuildingWrapper for SDK wrapper access
    /// </summary>
    public class BuildingNative
    {
        /// <summary>Native IL2CPP Building instance</summary>
        public object NativeInstance { get; }

        public BuildingNative(object nativeInstance)
        {
            NativeInstance = nativeInstance ?? throw new ArgumentNullException(nameof(nativeInstance));
        }
    }

    /// <summary>
    /// Native Swarm type alias for Enhanced Events
    /// Represents the actual IL2CPP native Swarm object
    /// Use PerAspera.GameAPI.Wrappers.Swarm for SDK wrapper access
    /// </summary>
    public class SwarmNative
    {
        /// <summary>Native IL2CPP Swarm instance</summary>
        public object NativeInstance { get; }

        public SwarmNative(object nativeInstance)
        {
            NativeInstance = nativeInstance ?? throw new ArgumentNullException(nameof(nativeInstance));
        }
    }

    /// <summary>
    /// Native Way type alias for Enhanced Events
    /// Represents the actual IL2CPP native Way object
    /// Use PerAspera.GameAPI.Wrappers.Way for SDK wrapper access
    /// </summary>
    public class WayNative
    {
        /// <summary>Native IL2CPP Way instance</summary>
        public object NativeInstance { get; }

        public WayNative(object nativeInstance)
        {
            NativeInstance = nativeInstance ?? throw new ArgumentNullException(nameof(nativeInstance));
        }
    }
}