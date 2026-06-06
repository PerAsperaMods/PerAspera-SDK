using System;
using System.Collections.Generic;
using System.Reflection;
using PerAspera.Core;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Static wrapper for accessing SpecialSite collection from BaseGame
    /// Handles IL2CPP reflection to get special sites data
    /// </summary>
    public static class SpecialSiteWrapper
    {
        private static readonly LogAspera Log = new LogAspera("SpecialSiteWrapper");

        /// <summary>
        /// Get all special sites from BaseGame.visualSites collection
        /// </summary>
        public static IEnumerable<object> GetAllSpecialSites()
        {
            var result = new List<object>();

            try
            {
                // Get BaseGame instance
                var baseGame = BaseGameWrapper.GetCurrent();
                if (baseGame == null)
                {
                    Log.Warning("BaseGame not found");
                    return result;
                }

                var nativeBaseGame = baseGame.GetNativeObject();
                if (nativeBaseGame == null)
                {
                    Log.Warning("BaseGame native object is null");
                    return result;
                }

                // Try to get visualSites property
                var baseGameType = nativeBaseGame.GetType();
                var visualSitesProp = baseGameType.GetProperty("visualSites",
                    BindingFlags.Public | BindingFlags.Instance);

                if (visualSitesProp != null)
                {
                    var visualSitesValue = visualSitesProp.GetValue(nativeBaseGame);
                    Log.Info($"visualSites type: {visualSitesValue?.GetType().Name ?? "NULL"}");

                    if (visualSitesValue is System.Collections.IDictionary dict)
                    {
                        Log.Info($"Found {dict.Count} visual sites");

                        foreach (var key in dict.Keys)
                        {
                            try
                            {
                                var siteValue = dict[key];
                                if (siteValue != null)
                                {
                                    // The value in the dictionary is the SpecialSite instance
                                    result.Add(siteValue);
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Warning($"Error accessing site: {ex.Message}");
                            }
                        }

                        if (result.Count > 0)
                        {
                            Log.Info($"Successfully loaded {result.Count} special sites from visualSites");
                            return result;
                        }
                    }
                }
                else
                {
                    Log.Warning("visualSites property not found on BaseGame");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error loading SpecialSites: {ex}");
            }

            return result;
        }
    }
}
