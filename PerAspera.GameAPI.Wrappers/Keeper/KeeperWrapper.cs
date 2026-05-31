#nullable enable
using System;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Direct wrapper around native Keeper class
    /// Provides safe access to Keeper properties and operations
    /// DOC: F:\ModPeraspera\Internal_doc\ARCHITECTURE\Handle-System-Architecture.md
    /// </summary>
    public class KeeperWrapper : WrapperBase
    {
        private static readonly string LogPrefix = "[KeeperWrapper]";
        private Keeper? _nativeKeeper;

        /// <summary>
        /// Initialize KeeperWrapper with native Keeper instance
        /// </summary>
        /// <param name="nativeKeeper">Native Keeper from BaseGame.keeper</param>
        public KeeperWrapper(object nativeKeeper) : base(nativeKeeper)
        {
            // Try to cast to native type for direct access
            try
            {
                _nativeKeeper = (Keeper)nativeKeeper;
            }
            catch (Exception ex)
            {
                WrapperLog.Warning($"Failed to cast to Keeper, using reflection fallback: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Get current Keeper from BaseGame.keeper
        /// Factory method for wrapper creation
        /// </summary>
        public static KeeperWrapper? GetCurrent()
        {
            try
            {
                var baseGame = BaseGameWrapper.GetCurrent();
                if (baseGame == null) return null;
                
                var keeper = baseGame.GetKeeper();
                if (keeper == null) return null;
                
                return new KeeperWrapper(keeper);
            }
            catch (Exception ex)
            {
                Log.LogWarning($"{LogPrefix} GetCurrent failed: {ex.Message}");
                return null;
            }
        }
        
        // ==================== CORE KEEPER METHODS ====================
        
        /// <summary>
        /// Get KeeperMap wrapper for Handle→Object operations
        /// Performance: O(1) field access
        /// </summary>
        /// <returns>KeeperMapWrapper instance or null if unavailable</returns>
        public KeeperMapWrapper? GetKeeperMap()
        {
            try
            {
                if (NativeObject == null) return null;

                // Try direct access first
                if (_nativeKeeper != null)
                {
                    var keeperMap = _nativeKeeper.map;
                    if (keeperMap != null)
                    {
                        return new KeeperMapWrapper(keeperMap);
                    }
                }

                // Fallback to reflection
                var keeperMapRef = GetNativeField<object>("map");
                if (keeperMapRef == null) return null;
                
                return new KeeperMapWrapper(keeperMapRef);
            }
            catch (Exception ex)
            {
                Log.LogWarning($"{LogPrefix} GetKeeperMap failed: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Register an entity with the Keeper system
        /// Returns Handle for future lookups
        /// </summary>
        /// <param name="handleable">Entity to register (must implement IHandleable)</param>
        /// <returns>Handle for the registered entity</returns>
        public Handle Register(object handleable)
        {
            try
            {
                if (NativeObject == null || handleable == null) return default;

                // Try direct access first
                if (_nativeKeeper != null && handleable is IHandleable iHandleable)
                {
                    return _nativeKeeper.Register(iHandleable);
                }

                // Fallback to reflection
                return SafeInvoke<Handle>("Register", handleable);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"{LogPrefix} Register failed: {ex.Message}");
                return default;
            }
        }
        
        /// <summary>
        /// Unregister an entity from the Keeper system
        /// Removes the Handle→Object mapping
        /// </summary>
        /// <param name="handleable">Entity to unregister</param>
        public void Unregister(object handleable)
        {
            try
            {
                if (NativeObject == null || handleable == null) return;

                // Try direct access first
                if (_nativeKeeper != null && handleable is IHandleable iHandleable)
                {
                    _nativeKeeper.Unregister(iHandleable);
                    return;
                }

                // Fallback to reflection
                SafeInvoke<object>("Unregister", handleable);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning($"{LogPrefix} Unregister failed: {ex.Message}");
            }
        }
        
        // ==================== DIAGNOSTICS & DEBUGGING ====================
        
        /// <summary>
        /// Get HandleManager for low-level Handle operations
        /// Advanced usage only - prefer KeeperMapWrapper for normal operations
        /// </summary>
        /// <returns>Native HandleManager object or null</returns>
        public object? GetHandleManager()
        {
            try
            {
                if (NativeObject == null) return null;

                // Try direct access first
                if (_nativeKeeper != null)
                {
                    return _nativeKeeper.handleManager;
                }

                // Fallback to reflection
                return SafeInvoke<object>("get_handleManager");
            }
            catch (Exception ex)
            {
                Log.LogWarning($"{LogPrefix} GetHandleManager failed: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get ECS World instance for Entity-Component-System operations
        /// Advanced usage - requires Unity ECS knowledge
        /// </summary>
        /// <returns>Unity ECS World or null if not available</returns>
        public object? GetECSWorld()
        {
            try
            {
                if (NativeObject == null) return null;

                // Try direct access first
                if (_nativeKeeper != null)
                {
                    return _nativeKeeper.ecsWorld;
                }

                // Fallback to reflection
                return SafeInvoke<object>("get_ecsWorld");
            }
            catch (Exception ex)
            {
                Log.LogWarning($"{LogPrefix} GetECSWorld failed: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Get EntityManager for ECS entity operations
        /// Advanced usage - requires Unity ECS knowledge
        /// </summary>
        /// <returns>Unity EntityManager or null if not available</returns>
        public object? GetEntityManager()
        {
            try
            {
                if (NativeObject == null) return null;
                
                return SafeInvoke<object>("get_entityManager");
            }
            catch (Exception ex)
            {
                Log.LogWarning($"{LogPrefix} GetEntityManager failed: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Check if Keeper is properly initialized and ready for use
        /// Validates core components: map, handleManager, ecsWorld
        /// </summary>
        /// <returns>True if Keeper is fully functional</returns>
        public bool IsReady()
        {
            try
            {
                if (NativeObject == null) return false;
                
                var map = GetKeeperMap();
                var handleManager = GetHandleManager();
                
                return map != null && handleManager != null;
            }
            catch (Exception ex)
            {
                Log.LogWarning($"{LogPrefix} Unregister failed: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Get comprehensive Keeper diagnostics for debugging
        /// Useful for monitoring system health and performance
        /// </summary>
        /// <returns>Diagnostic information about Keeper state</returns>
        public KeeperDiagnostics GetDiagnostics()
        {
            try
            {
                var diagnostics = new KeeperDiagnostics
                {
                    IsInitialized = IsReady(),
                    HasKeeperMap = GetKeeperMap() != null,
                    HasHandleManager = GetHandleManager() != null,
                    HasECSWorld = GetECSWorld() != null,
                    HasEntityManager = GetEntityManager() != null
                };
                
                // Get entity count from KeeperMap if available
                var keeperMap = GetKeeperMap();
                if (keeperMap != null)
                {
                    diagnostics.EntityCount = keeperMap.GetEntityCount();
                }
                
                return diagnostics;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"{LogPrefix} GetDiagnostics failed: {ex.Message}");
                return new KeeperDiagnostics { IsInitialized = false };
            }
        }
    }
    
    /// <summary>
    /// Keeper diagnostic information for debugging and monitoring
    /// </summary>
    public struct KeeperDiagnostics
    {
        public bool IsInitialized { get; set; }
        public bool HasKeeperMap { get; set; }
        public bool HasHandleManager { get; set; }
        public bool HasECSWorld { get; set; }
        public bool HasEntityManager { get; set; }
        public int EntityCount { get; set; }
        
        public override string ToString()
        {
            return $"Keeper: Init={IsInitialized}, Map={HasKeeperMap}, Entities={EntityCount}, ECS={HasECSWorld}";
        }
    }
}