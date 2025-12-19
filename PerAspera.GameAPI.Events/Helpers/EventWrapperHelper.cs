#nullable enable
using System;
using System.Collections.Generic;
using PerAspera.GameAPI.Wrappers;
using PerAspera.Core;

namespace PerAspera.GameAPI.Events.Helpers
{
    /// <summary>
    /// Helper class for automatically converting native IL2CPP instances to SDK wrappers in events
    /// Provides type-safe wrapper creation with automatic fallback handling
    /// 
    /// USAGE: When an event receives a native object, use this helper to convert it to SDK wrapper:
    /// - eventData.Building = EventWrapperHelper.CreateBuildingWrapper(nativeBuildingInstance);
    /// - eventData.OwnerFaction = EventWrapperHelper.CreateFactionWrapper(nativeFactionInstance);
    /// 
    /// PRINCIPLE: Events should ALWAYS return wrappers, never native instances for modder safety
    /// </summary>
    public static class EventWrapperHelper
    {
        private static readonly LogAspera _logger = new LogAspera("EventWrapperHelper");

        // ==================== BUILDING WRAPPERS ====================
        
        /// <summary>
        /// Convert native Building instance to Building wrapper for events
        /// </summary>
        /// <param name="nativeBuilding">Native building instance from game</param>
        /// <returns>Building wrapper or null if conversion fails</returns>
        public static GameAPI.Wrappers.Building? CreateBuildingWrapper(object? nativeBuilding)
        {
            if (nativeBuilding == null) return null;
            
            try
            {
                return GameAPI.Wrappers.Building.FromNative(nativeBuilding);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to create Building wrapper: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Convert native BuildingType instance to BuildingType wrapper for events
        /// </summary>
        /// <param name="nativeBuildingType">Native building type instance</param>
        /// <returns>BuildingType wrapper or null if conversion fails</returns>
        public static GameAPI.Wrappers.BuildingType? CreateBuildingTypeWrapper(object? nativeBuildingType)
        {
            if (nativeBuildingType == null) return null;
            
            try
            {
                return GameAPI.Wrappers.BuildingType.FromNative(nativeBuildingType);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to create BuildingType wrapper: {ex.Message}");
                return null;
            }
        }

        // ==================== FACTION WRAPPERS ====================
        
        /// <summary>
        /// Convert native Faction instance to Faction wrapper for events
        /// </summary>
        /// <param name="nativeFaction">Native faction instance from game</param>
        /// <returns>Faction wrapper or null if conversion fails</returns>
        public static GameAPI.Wrappers.Faction? CreateFactionWrapper(object? nativeFaction)
        {
            if (nativeFaction == null) return null;
            
            try
            {
                return GameAPI.Wrappers.Faction.FromNative(nativeFaction);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to create Faction wrapper: {ex.Message}");
                return null;
            }
        }

        // ==================== RESOURCE WRAPPERS ====================
        
        /// <summary>
        /// Convert native ResourceType instance to ResourceType wrapper for events
        /// </summary>
        /// <param name="nativeResourceType">Native resource type instance</param>
        /// <returns>ResourceType wrapper or null if conversion fails</returns>
        public static GameAPI.Wrappers.ResourceType? CreateResourceTypeWrapper(object? nativeResourceType)
        {
            if (nativeResourceType == null) return null;
            
            try
            {
                return GameAPI.Wrappers.ResourceType.FromNative(nativeResourceType);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to create ResourceType wrapper: {ex.Message}");
                return null;
            }
        }

        // ==================== TECHNOLOGY WRAPPERS ====================
        
        /// <summary>
        /// Convert native Technology instance to Technology wrapper for events
        /// </summary>
        /// <param name="nativeTechnology">Native technology instance</param>
        /// <returns>Technology wrapper or null if conversion fails</returns>
        public static GameAPI.Wrappers.Technology? CreateTechnologyWrapper(object? nativeTechnology)
        {
            if (nativeTechnology == null) return null;
            
            try
            {
                return GameAPI.Wrappers.Technology.FromNative(nativeTechnology);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to create Technology wrapper: {ex.Message}");
                return null;
            }
        }

        // ==================== DRONE WRAPPERS ====================
        
        /// <summary>
        /// Convert native Drone instance to Drone wrapper for events
        /// </summary>
        /// <param name="nativeDrone">Native drone instance</param>
        /// <returns>Drone wrapper or null if conversion fails</returns>
        public static GameAPI.Wrappers.Drone? CreateDroneWrapper(object? nativeDrone)
        {
            if (nativeDrone == null) return null;
            
            try
            {
                return new GameAPI.Wrappers.Drone(nativeDrone);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to create Drone wrapper: {ex.Message}");
                return null;
            }
        }

        // ==================== UNIVERSAL WRAPPER CONVERTER ====================
        
        /// <summary>
        /// Universal wrapper converter using WrapperFactory
        /// Automatically detects the appropriate wrapper type and creates it
        /// </summary>
        /// <typeparam name="T">Expected wrapper type</typeparam>
        /// <param name="nativeInstance">Native IL2CPP instance</param>
        /// <returns>Wrapper instance of type T or null</returns>
        public static T? ConvertToWrapper<T>(object? nativeInstance) where T : class
        {
            if (nativeInstance == null) return null;
            
            try
            {
                return Core.WrapperFactory.ConvertToWrapper<T>(nativeInstance);
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to convert to {typeof(T).Name} wrapper: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Batch wrapper conversion for collections
        /// Converts a collection of native instances to wrapper collection
        /// </summary>
        /// <typeparam name="T">Wrapper type</typeparam>
        /// <param name="nativeInstances">Collection of native instances</param>
        /// <returns>Collection of wrapper instances (nulls filtered out)</returns>
        public static IEnumerable<T> ConvertCollectionToWrappers<T>(IEnumerable<object>? nativeInstances) where T : class
        {
            if (nativeInstances == null) yield break;
            
            foreach (var nativeInstance in nativeInstances)
            {
                var wrapper = ConvertToWrapper<T>(nativeInstance);
                if (wrapper != null)
                    yield return wrapper;
            }
        }

        // ==================== VALIDATION & DEBUGGING ====================
        
        /// <summary>
        /// Validate that a wrapper was created successfully
        /// </summary>
        /// <param name="wrapper">Wrapper to validate</param>
        /// <param name="contextName">Context for logging (e.g., "BuildingSpawned event")</param>
        /// <returns>True if wrapper is valid</returns>
        public static bool ValidateWrapper(object? wrapper, string contextName)
        {
            if (wrapper == null)
            {
                _logger.Warning($"Wrapper is null in {contextName}");
                return false;
            }

            if (wrapper is WrapperBase wrapperBase && !wrapperBase.IsValidWrapper)
            {
                _logger.Warning($"Wrapper is invalid in {contextName}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Log wrapper creation success/failure for debugging
        /// </summary>
        /// <param name="nativeType">Type of native instance</param>
        /// <param name="wrapperType">Type of wrapper created</param>
        /// <param name="success">Whether creation succeeded</param>
        /// <param name="contextName">Context for logging</param>
        public static void LogWrapperCreation(System.Type? nativeType, System.Type? wrapperType, bool success, string contextName)
        {
            if (success)
            {
                _logger.Debug($"Created {wrapperType?.Name} wrapper from {nativeType?.Name} in {contextName}");
            }
            else
            {
                _logger.Warning($"Failed to create {wrapperType?.Name} wrapper from {nativeType?.Name} in {contextName}");
            }
        }
    }
}