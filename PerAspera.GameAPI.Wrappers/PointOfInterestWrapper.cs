using System;
using System.Collections.Generic;
using System.Reflection;
using PerAspera.Core;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for accessing PointOfInterest static data collection
    /// Handles IL2CPP reflection to get POI from StaticValues
    /// </summary>
    public static class PointOfInterestWrapper
    {
        private static readonly LogAspera Log = new LogAspera("PointOfInterestWrapper");
        private static System.Type? _poiType;

        /// <summary>
        /// Get all POI items from the game
        /// BEST METHOD: Access by key directly using PointOfInterest.Get(key)
        /// Fallback: Try table field → StaticValues property
        /// </summary>
        public static IEnumerable<object> GetAllPOI()
        {
            var result = new List<object>();

            try
            {
                var poiType = GetPointOfInterestType();
                if (poiType == null)
                {
                    Log.Warning("PointOfInterest type not found");
                    return result;
                }

                Log.Info($"✅ Found PointOfInterest type: {poiType.FullName}");

                var bindFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy;

                // Strategy 1: Try 'table' field (Dictionary<string, PointOfInterest>)
                Log.Info("Trying 'table' field for direct enumeration...");
                var tableField = poiType.GetField("table", bindFlags);
                if (tableField != null)
                {
                    var tableValue = tableField.GetValue(null);
                    Log.Info($"  'table' field value type: {tableValue?.GetType().Name ?? "NULL"}");

                    if (tableValue is System.Collections.IDictionary table)
                    {
                        Log.Info($"✅ Found 'table' field with {table.Count} entries");

                        // Enumerate all table entries directly
                        foreach (var key in table.Keys)
                        {
                            try
                            {
                                var value = table[key];
                                if (value != null)
                                {
                                    result.Add(value);
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Warning($"Error accessing table[{key}]: {ex.Message}");
                            }
                        }

                        if (result.Count > 0)
                        {
                            Log.Info($"✅ Loaded {result.Count} POI from table enumeration");
                            return result;
                        }
                    }
                }

                // Strategy 2: Try StaticValues property (inherited from StaticDataCollectionItem<T>)
                Log.Info("Trying StaticValues property...");
                var staticValuesProp = poiType.GetProperty("StaticValues", bindFlags);
                if (staticValuesProp != null)
                {
                    var svValue = staticValuesProp.GetValue(null);
                    Log.Info($"StaticValues return type: {svValue?.GetType().Name ?? "NULL"}");

                    if (svValue != null && svValue is System.Collections.IEnumerable allPoi)
                    {
                        var count = 0;
                        var countProp = svValue.GetType().GetProperty("Count");
                        if (countProp != null)
                        {
                            var countValue = countProp.GetValue(svValue);
                            Log.Info($"  StaticValues.Count = {countValue}");
                        }

                        foreach (var poi in allPoi)
                        {
                            result.Add(poi);
                            count++;
                        }
                        Log.Info($"✅ Enumerated {count} items from StaticValues");
                        if (count > 0) return result;
                    }
                    else if (svValue == null)
                    {
                        Log.Warning("  StaticValues property returned NULL");
                    }
                }

                // Strategy 3: Try smallCraters field (fallback for direct access)
                Log.Info("Trying smallCraters field...");
                var smallCratersField = poiType.GetField("smallCraters", bindFlags);
                Log.Info($"smallCraters field: {(smallCratersField == null ? "NOT FOUND" : "FOUND")}");
                if (smallCratersField != null)
                {
                    var fieldValue = smallCratersField.GetValue(null);
                    Log.Info($"  smallCraters value: {(fieldValue == null ? "NULL" : fieldValue.GetType().Name)}");

                    if (fieldValue is System.Collections.IEnumerable craters)
                    {
                        var count = 0;
                        foreach (var crater in craters)
                        {
                            result.Add(crater);
                            count++;
                        }
                        Log.Info($"✅ Loaded {count} from smallCraters");
                        if (count > 0) return result;
                    }
                }

                Log.Warning($"❌ No data found from any strategy");
                Log.Warning($"   Possible causes:");
                Log.Warning($"   1. YAML mods haven't been loaded yet");
                Log.Warning($"   2. Collections are populated lazily");
                Log.Warning($"   3. Collections are empty in this game session");
                return result;
            }
            catch (Exception ex)
            {
                Log.Error($"Error loading PointOfInterest: {ex}");
                return result;
            }
        }

        /// <summary>
        /// Get POI by key from the static collection — DIRECT ACCESS
        /// This is the reliable way to access YAML mod POI data
        /// </summary>
        public static object? GetPOIByKey(string key)
        {
            try
            {
                var poiType = GetPointOfInterestType();
                if (poiType == null)
                {
                    Log.Warning($"Cannot get POI by key '{key}' - type not found");
                    return null;
                }

                var getMethod = poiType.GetMethod("Get",
                    BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

                if (getMethod != null)
                {
                    Log.Info($"Using Get('{key}')...");
                    var result = getMethod.Invoke(null, new object[] { key });

                    if (result != null)
                    {
                        Log.Info($"✅ Found POI by key '{key}': {result.GetType().Name}");
                    }
                    else
                    {
                        Log.Warning($"Get('{key}') returned NULL");
                    }

                    return result;
                }
                else
                {
                    Log.Warning("Get(string) method not found on PointOfInterest type");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error getting POI by key '{key}': {ex.Message}\n{ex.StackTrace}");
            }

            return null;
        }

        /// <summary>
        /// Get property value from POI object
        /// </summary>
        public static T? GetPOIProperty<T>(object poi, string propertyName)
        {
            try
            {
                if (poi == null)
                    return default;

                var poiType = GetPointOfInterestType();
                if (poiType == null)
                    return default;

                var prop = poiType.GetProperty(propertyName,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (prop != null)
                {
                    return (T?)prop.GetValue(poi);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error getting POI property {propertyName}: {ex}");
            }

            return default;
        }

        private static System.Type? GetPointOfInterestType()
        {
            if (_poiType != null)
                return _poiType;

            try
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    _poiType = assembly.GetType("PointOfInterest", false, true);
                    if (_poiType != null)
                        break;
                }

                if (_poiType == null)
                {
                    _poiType = System.Type.GetType("PointOfInterest", false, true);
                }

                if (_poiType == null)
                {
                    Log.Warning("PointOfInterest type not found");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error finding PointOfInterest type: {ex}");
            }

            return _poiType;
        }
    }
}
