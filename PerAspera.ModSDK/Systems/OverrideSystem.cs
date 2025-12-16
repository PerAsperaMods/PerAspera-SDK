using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PerAspera.Core;
using PerAspera.GameAPI.Overrides.Models;
using PerAspera.GameAPI.Overrides.Registry;
using PerAspera.GameAPI.Overrides.Patching;

namespace PerAspera.ModSDK.Systems
{
    /// <summary>
    /// Getter Override system - Modify game method return values dynamically
    /// </summary>
    public static class OverrideSystem
    {
        private static readonly LogAspera Log = new LogAspera(nameof(OverrideSystem));
        private static bool _initialized = false;

        /// <summary>
        /// Initialize the override system
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            try
            {
                // Initialize new v2.0 patch system
                OverridePatchSystem.Initialize("PerAspera.ModSDK.Overrides");
                
                // Auto-discover patches in GameAPI assembly
                var gameApiAssembly = typeof(PerAspera.GameAPI.Patches.PlanetPatches).Assembly;
                OverridePatchSystem.DiscoverAndApplyPatches(gameApiAssembly);
                
                _initialized = true;
                Log.Info("✅ Getter Override system v2.0 initialized");
            }
            catch (Exception ex)
            {
                throw new ModSDKException($"Failed to initialize Override system: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Shutdown the override system
        /// </summary>
        public static void Shutdown()
        {
            if (!_initialized) return;

            try
            {
                OverridePatchSystem.RemoveAllPatches();
                _initialized = false;
                Log.Info("✅ Getter Override system v2.0 shut down");
            }
            catch (Exception ex)
            {
                Log.Error($"Error shutting down Override system: {ex.Message}");
            }
        }

        /// <summary>
        /// Register a new getter override (float)
        /// </summary>
        public static GetterOverride<float> RegisterOverride(string className, string methodName, string displayName, 
                                            float defaultValue, float minValue = 0f, float maxValue = 10f, 
                                            string category = "Custom")
        {
            EnsureInitialized();
            try
            {
                var overrideConfig = new GetterOverride<float>(
                    className, methodName, displayName, defaultValue)
                {
                    Category = category,
                    Validator = v => v >= minValue && v <= maxValue
                };
                
                GetterOverrideRegistry.RegisterOverride(overrideConfig);
                
                Log.Info($"📝 Registered override: {className}.{methodName} -> {displayName}");
                return overrideConfig;
            }
            catch (Exception ex)
            {
                throw new ModSDKException($"Failed to register override {className}.{methodName}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Register a new getter override (generic)
        /// </summary>
        public static GetterOverride<T> RegisterOverride<T>(string className, string methodName, string displayName, 
                                            T defaultValue, string category = "Custom")
        {
            EnsureInitialized();
            try
            {
                var overrideConfig = new GetterOverride<T>(
                    className, methodName, displayName, defaultValue)
                {
                    Category = category
                };
                
                GetterOverrideRegistry.RegisterOverride(overrideConfig);
                
                Log.Info($"📝 Registered override: {className}.{methodName} -> {displayName} [{typeof(T).Name}]");
                return overrideConfig;
            }
            catch (Exception ex)
            {
                throw new ModSDKException($"Failed to register override {className}.{methodName}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Set override value
        /// </summary>
        public static void SetOverride(string className, string methodName, float value, bool enabled = true)
        {
            EnsureInitialized();
            try
            {
                var overrideConfig = GetterOverrideRegistry.GetOverride<float>(className, methodName);
                if (overrideConfig == null)
                {
                    throw new ModSDKException($"Override not found: {className}.{methodName}");
                }

                overrideConfig.SetValue(value);
                overrideConfig.SetEnabled(enabled);
                
                Log.Info($"🔧 Set override {className}.{methodName} = {value} ({(enabled ? "ON" : "OFF")})");
            }
            catch (Exception ex)
            {
                throw new ModSDKException($"Failed to set override {className}.{methodName}: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get override value
        /// </summary>
        public static float GetOverrideValue(string className, string methodName)
        {
            EnsureInitialized();
            try
            {
                var overrideConfig = GetterOverrideRegistry.GetOverride<float>(className, methodName);
                return overrideConfig?.EffectiveValue ?? 0f;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get override value {className}.{methodName}: {ex.Message}");
                return 0f;
            }
        }

        /// <summary>
        /// Check if override is active
        /// </summary>
        public static bool IsOverrideActive(string className, string methodName)
        {
            try
            {
                return GetterOverrideRegistry.IsOverrideActive(className, methodName);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get all overrides in a category
        /// </summary>
        public static IEnumerable<object> GetOverridesByCategory(string category)
        {
            try
            {
                return GetterOverrideRegistry.GetOverridesByCategory(category);
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get overrides by category {category}: {ex.Message}");
                return Enumerable.Empty<object>();
            }
        }

        /// <summary>
        /// Get all enabled overrides
        /// </summary>
        public static IEnumerable<string> GetActiveOverrides()
        {
            try
            {
                return GetterOverrideRegistry.GetAllKeys()
                    .Where(key => {
                        var parts = key.Split('.');
                        return parts.Length >= 2 && GetterOverrideRegistry.IsOverrideActive(parts[0], parts[1]);
                    });
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get active overrides: {ex.Message}");
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// Clear all registered overrides
        /// </summary>
        public static void ClearAllOverrides()
        {
            try
            {
                GetterOverrideRegistry.Clear();
                Log.Info("🗑️ Cleared all registered overrides");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to clear overrides: {ex.Message}");
            }
        }

        /// <summary>
        /// Get override system statistics
        /// </summary>
        public static string GetStatistics()
        {
            try
            {
                var stats = GetterOverrideRegistry.GetStatistics();
                return stats.ToString();
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get override statistics: {ex.Message}");
                return "Error getting statistics";
            }
        }

        /// <summary>
        /// Ensure override system is initialized
        /// </summary>
        private static void EnsureInitialized()
        {
            if (!_initialized)
            {
                throw new ModSDKException("Override system not initialized. Call OverrideSystem.Initialize() first.");
            }
        }
    }
}