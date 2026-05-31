using System;
using UnityEngine.SceneManagement;
using PerAspera.GameAPI.Events.Core;

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
        public UnityEngine.SceneManagement.Scene Scene { get; }
        
        /// <summary>
        /// The mode used to load the scene (Single or Additive)
        /// </summary>
        public LoadSceneMode Mode { get; }

        public SceneLoadedEvent(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            Scene = scene;
            Mode = mode;
        }

        public override string ToString()
        {
            return $"SceneLoadedEvent(Scene: '{Scene.name}', Mode: {Mode}, BuildIndex: {Scene.buildIndex})";
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
        public UnityEngine.SceneManagement.Scene Scene { get; }

        public SceneUnloadedEvent(UnityEngine.SceneManagement.Scene scene)
        {
            Scene = scene;
        }

        public override string ToString()
        {
            return $"SceneUnloadedEvent(Scene: '{Scene.name}', BuildIndex: {Scene.buildIndex})";
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
        public UnityEngine.SceneManagement.Scene PreviousScene { get; }
        
        /// <summary>
        /// The newly active scene
        /// </summary>
        public UnityEngine.SceneManagement.Scene NewScene { get; }

        public ActiveSceneChangedEvent(UnityEngine.SceneManagement.Scene previousScene, UnityEngine.SceneManagement.Scene newScene)
        {
            // PreviousScene can be null for first scene load
            PreviousScene = previousScene;
            NewScene = newScene;
        }

        public override string ToString()
        {
            var prevName = PreviousScene.name;
            return $"ActiveSceneChangedEvent(Previous: '{prevName}', New: '{NewScene.name}')";
        }
    }
}