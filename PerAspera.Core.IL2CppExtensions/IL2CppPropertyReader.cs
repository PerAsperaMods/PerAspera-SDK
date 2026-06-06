using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Logging;

namespace PerAspera.Core.IL2CPP
{
    /// <summary>
    /// Robust IL2CPP property reader that handles multiple access strategies
    /// Works around IL2CPP reflection limitations by trying multiple approaches
    /// </summary>
    public static class IL2CppPropertyReader
    {
        private static readonly ManualLogSource Log = Logger.CreateLogSource("IL2CppPropertyReader");
        private static readonly Dictionary<(System.Type, string), PropertyAccessStrategy> _strategyCache = new();

        /// <summary>
        /// Read property from IL2CPP object using multiple strategies
        /// Returns value or default if all strategies fail
        /// </summary>
        public static T? ReadProperty<T>(object instance, string propertyName)
        {
            if (instance == null)
                return default;

            var objType = instance.GetType();
            var cacheKey = (objType, propertyName);

            // Try cached strategy first (if we know what works for this type+property)
            if (_strategyCache.TryGetValue(cacheKey, out var cachedStrategy))
            {
                return cachedStrategy.Read<T>(instance, propertyName);
            }

            // Try each strategy in order until one works
            var strategies = new PropertyAccessStrategy[]
            {
                new PublicPropertyStrategy(),
                new PublicFieldStrategy(),
                new NonPublicPropertyStrategy(),
                new NonPublicFieldStrategy(),
                new PrivateBackingFieldStrategy(),
                new IndexerStrategy()
            };

            foreach (var strategy in strategies)
            {
                try
                {
                    var result = strategy.Read<T>(instance, propertyName);

                    // Cache the successful strategy
                    _strategyCache[cacheKey] = strategy;

                    Log.LogDebug($"✅ {strategy.GetType().Name} succeeded for {objType.Name}.{propertyName}");
                    return result;
                }
                catch (Exception ex)
                {
                    // Strategy failed, try next one
                    Log.LogDebug($"  [{strategy.GetType().Name}] {ex.Message}");
                    continue;
                }
            }

            // All strategies failed - try to help debug
            Log.LogWarning($"❌ Could not read {objType.Name}.{propertyName} using any strategy");
            Log.LogInfo($"   TIP: Use IL2CppObjectInspector.DumpObject() or FindMembersByName() to debug");
            Log.LogInfo($"   Example: IL2CppObjectInspector.FindMembersByName(instance, \"{propertyName}\")");

            return default;
        }

        /// <summary>
        /// Strategy pattern for different property access methods
        /// </summary>
        private abstract class PropertyAccessStrategy
        {
            public abstract object? Read(object instance, string propertyName);

            // Helper to cast result to T
            public T? Read<T>(object instance, string propertyName)
            {
                var result = Read(instance, propertyName);
                return (T?)result;
            }
        }

        /// <summary>
        /// Strategy 1: Public property with getter
        /// </summary>
        private class PublicPropertyStrategy : PropertyAccessStrategy
        {
            public override object? Read(object instance, string propertyName)
            {
                var prop = instance.GetType().GetProperty(propertyName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (prop != null && prop.CanRead)
                {
                    return prop.GetValue(instance);
                }

                throw new InvalidOperationException("Public property not found");
            }
        }

        /// <summary>
        /// Strategy 2: Public field
        /// </summary>
        private class PublicFieldStrategy : PropertyAccessStrategy
        {
            public override object? Read(object instance, string propertyName)
            {
                var field = instance.GetType().GetField(propertyName,
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (field != null)
                {
                    return field.GetValue(instance);
                }

                throw new InvalidOperationException("Public field not found");
            }
        }

        /// <summary>
        /// Strategy 3: Non-public (protected/internal) property
        /// </summary>
        private class NonPublicPropertyStrategy : PropertyAccessStrategy
        {
            public override object? Read(object instance, string propertyName)
            {
                var prop = instance.GetType().GetProperty(propertyName,
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (prop != null && prop.CanRead)
                {
                    return prop.GetValue(instance);
                }

                throw new InvalidOperationException("Non-public property not found");
            }
        }

        /// <summary>
        /// Strategy 4: Non-public (private) field
        /// </summary>
        private class NonPublicFieldStrategy : PropertyAccessStrategy
        {
            public override object? Read(object instance, string propertyName)
            {
                var field = instance.GetType().GetField(propertyName,
                    BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);

                if (field != null)
                {
                    return field.GetValue(instance);
                }

                throw new InvalidOperationException("Non-public field not found");
            }
        }

        /// <summary>
        /// Strategy 5: Private backing field (e.g., _propertyName or _property_k__BackingField)
        /// </summary>
        private class PrivateBackingFieldStrategy : PropertyAccessStrategy
        {
            public override object? Read(object instance, string propertyName)
            {
                var type = instance.GetType();

                // Try common naming conventions for backing fields
                var fieldNames = new[]
                {
                    $"_{propertyName}",
                    $"_{propertyName[0].ToString().ToLower()}{propertyName.Substring(1)}",
                    $"_{propertyName}_k__BackingField",
                    $"<{propertyName}>k__BackingField"
                };

                foreach (var fieldName in fieldNames)
                {
                    var field = type.GetField(fieldName,
                        BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);

                    if (field != null)
                    {
                        return field.GetValue(instance);
                    }
                }

                throw new InvalidOperationException("Backing field not found");
            }
        }

        /// <summary>
        /// Strategy 6: Indexed accessor (for collection items or indexed properties)
        /// </summary>
        private class IndexerStrategy : PropertyAccessStrategy
        {
            public override object? Read(object instance, string propertyName)
            {
                // Try Item[index] for collections
                var indexerProp = instance.GetType().GetProperty("Item",
                    BindingFlags.Public | BindingFlags.Instance);

                if (indexerProp != null)
                {
                    // This would need an index parameter, so it's limited
                    // Mainly useful for collections with string keys
                    throw new InvalidOperationException("Indexer requires additional parameters");
                }

                throw new InvalidOperationException("No indexer found");
            }
        }

        /// <summary>
        /// Clear the strategy cache (useful for debugging or after IL2CPP changes)
        /// </summary>
        public static void ClearCache()
        {
            _strategyCache.Clear();
            Log.LogInfo("Strategy cache cleared");
        }
    }
}
