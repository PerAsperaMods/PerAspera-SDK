using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using PerAspera.Core;
using PerAspera.GameAPI.Overrides.Models;

namespace PerAspera.GameAPI.Overrides.Registry
{
    /// <summary>
    /// Thread-safe registry for managing getter overrides
    /// Supports multiple types and provides discovery/lifecycle management
    /// </summary>
    public static class GetterOverrideRegistry
    {
        private static readonly LogAspera Log = new LogAspera("Overrides.Registry");
        
        // Thread-safe storage for overrides (key = "ClassName.MethodName", value = GetterOverride<T>)
        private static readonly ConcurrentDictionary<string, object> _overrides = new();

        // Event subscriptions
        public static event EventHandler<OverrideRegisteredEventArgs>? OverrideRegistered;
        public static event EventHandler<OverrideUnregisteredEventArgs>? OverrideUnregistered;
        public static event EventHandler<OverrideValueChangedEventArgs>? OverrideValueChanged;

        /// <summary>
        /// Register a new override (generic version)
        /// </summary>
        public static void RegisterOverride<T>(GetterOverride<T> overrideConfig)
        {
            if (overrideConfig == null)
                throw new ArgumentNullException(nameof(overrideConfig));

            var key = overrideConfig.Key;

            if (_overrides.ContainsKey(key))
            {
                Log.Warning($"Override already registered: {key} - Replacing");
            }

            // Subscribe to value changes for event forwarding
            overrideConfig.ValueChanged += (sender, args) =>
            {
                OverrideValueChanged?.Invoke(sender, new OverrideValueChangedEventArgs(
                    key, args.OldValue?.ToString() ?? "null", args.NewValue?.ToString() ?? "null"));
            };

            _overrides[key] = overrideConfig;
            OverrideRegistered?.Invoke(null, new OverrideRegisteredEventArgs(key, typeof(T).Name));

            Log.Info($"✅ Registered override: {key} [{typeof(T).Name}] = {overrideConfig.DefaultValue}");
        }

        /// <summary>
        /// Unregister an override
        /// </summary>
        public static bool UnregisterOverride(string className, string methodName)
        {
            var key = $"{className}.{methodName}";

            if (_overrides.TryRemove(key, out var removed))
            {
                OverrideUnregistered?.Invoke(null, new OverrideUnregisteredEventArgs(key));
                Log.Info($"❌ Unregistered override: {key}");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get override configuration (generic version)
        /// </summary>
        public static GetterOverride<T>? GetOverride<T>(string className, string methodName)
        {
            var key = $"{className}.{methodName}";

            if (_overrides.TryGetValue(key, out var obj))
            {
                if (obj is GetterOverride<T> typed)
                    return typed;

                Log.Warning($"Type mismatch for override {key}: expected {typeof(T).Name}, got {obj.GetType().Name}");
            }

            return null;
        }

        /// <summary>
        /// Check if override exists and is active
        /// </summary>
        public static bool IsOverrideActive(string className, string methodName)
        {
            var key = $"{className}.{methodName}";

            if (!_overrides.TryGetValue(key, out var obj))
                return false;

            // Use reflection to access IsEnabled property (works for any GetterOverride<T>)
            var isEnabledProp = obj.GetType().GetProperty("IsEnabled");
            return isEnabledProp?.GetValue(obj) as bool? ?? false;
        }

        /// <summary>
        /// Check if override exists
        /// </summary>
        public static bool HasOverride(string className, string methodName)
        {
            var key = $"{className}.{methodName}";
            return _overrides.ContainsKey(key);
        }

        /// <summary>
        /// Get all overrides in a category
        /// </summary>
        public static IEnumerable<object> GetOverridesByCategory(string category)
        {
            return _overrides.Values.Where(o =>
            {
                var categoryProp = o.GetType().GetProperty("Category");
                return categoryProp?.GetValue(o) as string == category;
            });
        }

        /// <summary>
        /// Get all override keys
        /// </summary>
        public static IEnumerable<string> GetAllKeys()
        {
            return _overrides.Keys;
        }

        /// <summary>
        /// Get count of registered overrides
        /// </summary>
        public static int Count => _overrides.Count;

        /// <summary>
        /// Clear all overrides (use with caution)
        /// </summary>
        public static void Clear()
        {
            var count = _overrides.Count;
            _overrides.Clear();
            Log.Warning($"🗑️ Cleared all overrides ({count} removed)");
        }

        /// <summary>
        /// Apply override to a value (helper method for patches)
        /// </summary>
        public static T ApplyOverride<T>(T originalValue, string className, string methodName, object? instance = null)
        {
            var overrideConfig = GetOverride<T>(className, methodName);

            if (overrideConfig == null || !overrideConfig.IsEnabled)
                return originalValue;

            return overrideConfig.ApplyStrategy(originalValue, instance);
        }

        /// <summary>
        /// Get statistics about registered overrides
        /// </summary>
        public static RegistryStatistics GetStatistics()
        {
            var stats = new RegistryStatistics
            {
                TotalOverrides = _overrides.Count,
                ActiveOverrides = _overrides.Values.Count(o =>
                {
                    var isEnabledProp = o.GetType().GetProperty("IsEnabled");
                    return isEnabledProp?.GetValue(o) as bool? ?? false;
                })
            };

            // Count by type
            foreach (var kvp in _overrides)
            {
                var type = kvp.Value.GetType().GetGenericArguments().FirstOrDefault()?.Name ?? "Unknown";
                stats.OverridesByType[type] = stats.OverridesByType.GetValueOrDefault(type) + 1;
            }

            // Count by category
            foreach (var o in _overrides.Values)
            {
                var categoryProp = o.GetType().GetProperty("Category");
                var category = categoryProp?.GetValue(o) as string ?? "Unknown";
                stats.OverridesByCategory[category] = stats.OverridesByCategory.GetValueOrDefault(category) + 1;
            }

            return stats;
        }
    }

    /// <summary>
    /// Statistics about the override registry
    /// </summary>
    public class RegistryStatistics
    {
        public int TotalOverrides { get; set; }
        public int ActiveOverrides { get; set; }
        public Dictionary<string, int> OverridesByType { get; } = new();
        public Dictionary<string, int> OverridesByCategory { get; } = new();

        public override string ToString()
        {
            var types = string.Join(", ", OverridesByType.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            var categories = string.Join(", ", OverridesByCategory.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            return $"Overrides: {ActiveOverrides}/{TotalOverrides} active | Types: [{types}] | Categories: [{categories}]";
        }
    }

    #region Event Args

    public class OverrideRegisteredEventArgs
    {
        public string Key { get; }
        public string TypeName { get; }

        public OverrideRegisteredEventArgs(string key, string typeName)
        {
            Key = key;
            TypeName = typeName;
        }
    }

    public class OverrideUnregisteredEventArgs
    {
        public string Key { get; }

        public OverrideUnregisteredEventArgs(string key)
        {
            Key = key;
        }
    }

    public class OverrideValueChangedEventArgs
    {
        public string Key { get; }
        public string OldValue { get; }
        public string NewValue { get; }

        public OverrideValueChangedEventArgs(string key, string oldValue, string newValue)
        {
            Key = key;
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    #endregion
}
