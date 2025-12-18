using System;
using UnityEngine.SceneManagement;
using PerAspera.GameAPI.Events.Core;
using PerAspera.GameAPI.Wrappers;
using Scene = PerAspera.GameAPI.Wrappers.Scene;

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
        public Scene Scene { get; }
        
        /// <summary>
        /// The mode used to load the scene (Single or Additive)
        /// </summary>
        public LoadSceneMode Mode { get; }

        public SceneLoadedEvent(Scene scene, LoadSceneMode mode)
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
        public Scene Scene { get; }

        public SceneUnloadedEvent(Scene scene)
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
        public Scene PreviousScene { get; }
        
        /// <summary>
        /// The newly active scene
        /// </summary>
        public Scene NewScene { get; }

        public ActiveSceneChangedEvent(Scene previousScene, Scene newScene)
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