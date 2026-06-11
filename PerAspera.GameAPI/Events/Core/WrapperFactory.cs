using System;
using System.Collections.Generic;
using PerAspera.Core;


#pragma warning disable CS1591
namespace PerAspera.GameAPI.Events.Core
{
    /// <summary>
    /// Identity factory: after Wrappers migration, native IL2CPP types are exposed directly.
    /// ConvertToWrapper now returns the native instance cast to the requested type.
    /// </summary>
    public static class WrapperFactory
    {
        private static readonly LogAspera _logger = new LogAspera("WrapperFactory");

        /// <summary>
        /// Return the native instance cast to T (identity � no wrapping anymore).
        /// </summary>
        public static T? ConvertToWrapper<T>(object? nativeInstance) where T : class
        {
            if (nativeInstance == null) return null;
            return nativeInstance as T;
        }

        /// <summary>
        /// Return native instance (no wrapping).
        /// </summary>
        public static object? ConvertToWrapper(object? nativeInstance, System.Type? targetType = null)
        {
            if (nativeInstance == null) return null;
            if (targetType == null) return nativeInstance;
            return targetType.IsInstanceOfType(nativeInstance) ? nativeInstance : null;
        }

        /// <summary>
        /// Returns the supported native types (previously "wrapper" types).
        /// </summary>
        public static IReadOnlyCollection<System.Type> GetSupportedWrapperTypes()
        {
            return new System.Type[]
            {
                typeof(Building), typeof(Drone), typeof(Universe), typeof(Planet),
                typeof(BaseGame), typeof(Faction), typeof(Technology),
                typeof(BuildingType), typeof(ResourceType), typeof(KnowledgeType)
            };
        }

        public static bool IsWrapperSupported(System.Type type)
        {
            foreach (var t in GetSupportedWrapperTypes())
                if (t == type) return true;
            return false;
        }

        /// <summary>
        /// Register a custom converter (no-op in identity mode, kept for API compatibility).
        /// </summary>
        public static void RegisterConverter<T>(Func<object, T> converter) where T : class
        {
            _logger.Warning($"RegisterConverter<{typeof(T).Name}>: ignored � factory is now identity-only");
        }
    }
}
#pragma warning restore CS1591
