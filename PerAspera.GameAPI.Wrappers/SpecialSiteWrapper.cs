using System;
using System.Collections.Generic;
using System.Reflection;
using PerAspera.Core;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for accessing SpecialSite static data collection
    /// SpecialSite inherits from StaticDataCollectionItem<SpecialSite>
    /// </summary>
    public static class SpecialSiteWrapper
    {
        private static readonly LogAspera Log = new LogAspera("SpecialSiteWrapper");
        private static System.Type? _siteType;

        /// <summary>
        /// Get all special sites from the static collection
        /// </summary>
        public static IEnumerable<object> GetAllSites()
        {
            var result = new List<object>();

            try
            {
                var siteType = GetSpecialSiteType();
                if (siteType == null)
                    return result;

                // Access StaticValues property (inherited from StaticDataCollectionItem<SpecialSite>)
                var staticValuesProp = siteType.GetProperty("StaticValues",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                if (staticValuesProp?.GetValue(null) is System.Collections.IEnumerable sites)
                {
                    foreach (var site in sites)
                    {
                        result.Add(site);
                    }
                }

                Log.Info($"Loaded {result.Count} SpecialSite items from StaticValues");
                return result;
            }
            catch (Exception ex)
            {
                Log.Error($"Error loading SpecialSite: {ex}");
                return result;
            }
        }

        /// <summary>
        /// Get site by key from the static collection
        /// </summary>
        public static object? GetSiteByKey(string key)
        {
            try
            {
                var siteType = GetSpecialSiteType();
                if (siteType == null)
                    return null;

                var getMethod = siteType.GetMethod("Get",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                if (getMethod != null)
                {
                    return getMethod.Invoke(null, new object[] { key });
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error getting site by key: {ex}");
            }

            return null;
        }

        /// <summary>
        /// Get property value from site object
        /// </summary>
        public static T? GetSiteProperty<T>(object site, string propertyName)
        {
            try
            {
                if (site == null)
                    return default;

                var siteType = GetSpecialSiteType();
                if (siteType == null)
                    return default;

                var prop = siteType.GetProperty(propertyName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop != null)
                {
                    return (T?)prop.GetValue(site);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error getting site property {propertyName}: {ex}");
            }

            return default;
        }

        private static System.Type? GetSpecialSiteType()
        {
            if (_siteType != null)
                return _siteType;

            try
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    _siteType = assembly.GetType("SpecialSite", false, true);
                    if (_siteType != null)
                        break;
                }

                if (_siteType == null)
                {
                    _siteType = System.Type.GetType("SpecialSite", false, true);
                }

                if (_siteType == null)
                {
                    Log.Warning("SpecialSite type not found");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error finding SpecialSite type: {ex}");
            }

            return _siteType;
        }
    }
}
