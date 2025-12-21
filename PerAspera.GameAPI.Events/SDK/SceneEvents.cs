using System;
using UnityEngine.SceneManagement;
using PerAspera.GameAPI.Events.Core;
using PerAspera.GameAPI.Wrappers;
using SceneWrapper = PerAspera.GameAPI.Wrappers.SceneWrapper;

namespace PerAspera.GameAPI.Events.SDK
{
    /// <summary>
    /// Event triggered when a scene is loaded
    /// Provides safe access to loaded scene information
    /// </summary>
    public class SceneLoadedEvent : SDKEventBase
    {
        public override string EventType => "SceneLoaded";
        
        /// <summary>
        /// The scene that was loaded
        /// </summary>
        public SceneWrapper Scene { get; }
        
        /// <summary>
        /// The mode used to load the scene (Single or Additive)
        /// </summary>
        public LoadSceneMode Mode { get; }

        public SceneLoadedEvent(SceneWrapper scene, LoadSceneMode mode)
        {
            Scene = scene ?? throw new ArgumentNullException(nameof(scene));
            Mode = mode;
        }

        public override string ToString()
        {
            return $"SceneLoadedEvent(Scene: '{Scene.Name}', Mode: {Mode}, BuildIndex: {Scene.BuildIndex})";
        }
    }

    /// <summary>
    /// Event triggered when a scene is unloaded
    /// Provides information about the unloaded scene
    /// </summary>
    public class SceneUnloadedEvent : SDKEventBase
    {
        public override string EventType => "SceneUnloaded";
        
        /// <summary>
        /// The scene that was unloaded
        /// </summary>
        public SceneWrapper Scene { get; }

        public SceneUnloadedEvent(SceneWrapper scene)
        {
            Scene = scene ?? throw new ArgumentNullException(nameof(scene));
        }

        public override string ToString()
        {
            return $"SceneUnloadedEvent(Scene: '{Scene.Name}', BuildIndex: {Scene.BuildIndex})";
        }
    }

    /// <summary>
    /// Event triggered when the active scene changes
    /// Provides both previous and new active scene information
    /// </summary>
    public class ActiveSceneChangedEvent : SDKEventBase
    {
        public override string EventType => "ActiveSceneChanged";
        
        /// <summary>
        /// The previously active scene (null if this is the first scene)
        /// </summary>
        public SceneWrapper PreviousScene { get; }
        
        /// <summary>
        /// The newly active scene
        /// </summary>
        public SceneWrapper NewScene { get; }

        public ActiveSceneChangedEvent(SceneWrapper previousScene, SceneWrapper newScene)
        {
            // PreviousScene can be null for first scene load
            PreviousScene = previousScene;
            NewScene = newScene ?? throw new ArgumentNullException(nameof(newScene));
        }

        public override string ToString()
        {
            var prevName = PreviousScene?.Name ?? "None";
            return $"ActiveSceneChangedEvent(Previous: '{prevName}', New: '{NewScene.Name}')";
        }
    }
}