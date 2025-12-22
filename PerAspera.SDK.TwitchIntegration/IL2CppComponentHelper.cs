using System;
using UnityEngine;
using Il2CppInterop.Runtime.Injection;
using PerAspera.Core;

namespace PerAspera.SDK.TwitchIntegration
{
    /// <summary>
    /// IL2CPP-safe helper for component operations.
    /// Addresses the IL2CPP limitation where GameObject.AddComponent(Type) does not exist.
    /// Only the generic GameObject.AddComponent&lt;T&gt;() method is available in IL2CPP Unity.
    /// </summary>
    public static class IL2CppComponentHelper
    {
        private static readonly LogAspera _logger = new LogAspera("IL2CppComponentHelper");

        /// <summary>
        /// Safely adds a component to a GameObject in an IL2CPP environment.
        /// This method ensures proper type registration and uses the correct generic AddComponent method.
        /// </summary>
        /// <typeparam name="T">The MonoBehaviour type to add</typeparam>
        /// <param name="gameObject">The target GameObject</param>
        /// <param name="registerType">Whether to register the type in IL2CPP (default: true)</param>
        /// <returns>The added component, or null if the operation failed</returns>
        public static T? AddComponentSafe<T>(GameObject gameObject, bool registerType = true) where T : MonoBehaviour
        {
            if (gameObject == null)
            {
                _logger.Error("Cannot add component: GameObject is null");
                return null;
            }

            try
            {
                // Step 1: Register type in IL2CPP if requested
                if (registerType)
                {
                    try
                    {
                        ClassInjector.RegisterTypeInIl2Cpp<T>();
                        _logger.Debug($"Registered type {typeof(T).Name} in IL2CPP");
                    }
                    catch (Exception ex) when (ex.Message.Contains("already registered"))
                    {
                        // Type already registered - this is fine
                        _logger.Debug($"Type {typeof(T).Name} already registered");
                    }
                }

                // Step 2: Add component using the generic method (IL2CPP-safe)
                // IMPORTANT: Never use gameObject.AddComponent(typeof(T)) - it doesn't exist in IL2CPP
                var component = gameObject.AddComponent<T>();
                
                if (component != null)
                {
                    _logger.Debug($"Successfully added component {typeof(T).Name} to {gameObject.name}");
                    return component;
                }
                else
                {
                    _logger.Warning($"AddComponent returned null for {typeof(T).Name}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to add component {typeof(T).Name}: {ex.Message}");
                _logger.Error($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Creates a new GameObject with a component attached, properly registered for IL2CPP.
        /// </summary>
        /// <typeparam name="T">The MonoBehaviour type to attach</typeparam>
        /// <param name="gameObjectName">Name for the new GameObject</param>
        /// <param name="dontDestroyOnLoad">Whether the GameObject should persist across scenes</param>
        /// <returns>The component attached to the new GameObject, or null if creation failed</returns>
        public static T? CreateGameObjectWithComponent<T>(
            string gameObjectName, 
            bool dontDestroyOnLoad = true) where T : MonoBehaviour
        {
            try
            {
                // Create GameObject
                var gameObject = new GameObject(gameObjectName);
                _logger.Debug($"Created GameObject: {gameObjectName}");

                // Make persistent if requested
                if (dontDestroyOnLoad)
                {
                    GameObject.DontDestroyOnLoad(gameObject);
                    _logger.Debug($"GameObject {gameObjectName} set to persist across scenes");
                }

                // Add component
                var component = AddComponentSafe<T>(gameObject, registerType: true);
                
                if (component == null)
                {
                    // Cleanup on failure
                    GameObject.Destroy(gameObject);
                    _logger.Error($"Failed to create GameObject with component {typeof(T).Name}");
                    return null;
                }

                return component;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to create GameObject with component {typeof(T).Name}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets or adds a component to a GameObject in an IL2CPP-safe manner.
        /// </summary>
        /// <typeparam name="T">The MonoBehaviour type</typeparam>
        /// <param name="gameObject">The target GameObject</param>
        /// <param name="registerType">Whether to register the type if adding (default: true)</param>
        /// <returns>The component (existing or newly added), or null if the operation failed</returns>
        public static T? GetOrAddComponent<T>(GameObject gameObject, bool registerType = true) where T : MonoBehaviour
        {
            if (gameObject == null)
            {
                _logger.Error("Cannot get or add component: GameObject is null");
                return null;
            }

            try
            {
                // Try to get existing component first
                var existingComponent = gameObject.GetComponent<T>();
                if (existingComponent != null)
                {
                    _logger.Debug($"Found existing component {typeof(T).Name} on {gameObject.name}");
                    return existingComponent;
                }

                // Component doesn't exist, add it
                _logger.Debug($"Component {typeof(T).Name} not found, adding to {gameObject.name}");
                return AddComponentSafe<T>(gameObject, registerType);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to get or add component {typeof(T).Name}: {ex.Message}");
                return null;
            }
        }
    }
}
