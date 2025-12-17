using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Native.Events
{
    /// <summary>
    /// Climate event patching service for Per Aspera
    /// Handles all climate-related events including temperature, pressure, and atmospheric composition changes
    /// </summary>
    public sealed class ClimateEventPatchingService : BaseEventPatchingService
    {
        private System.Type _planetType;

        /// <summary>
        /// Initialize climate event patching service
        /// </summary>
        /// <param name="harmony">Harmony instance for IL2CPP patching</param>
        public ClimateEventPatchingService(Harmony harmony) 
            : base("Climate", harmony)
        {
        }

        /// <summary>
        /// Get the event type identifier for this service
        /// </summary>
        /// <returns>Event type string</returns>
        public override string GetEventType() => "Climate";

        /// <summary>
        /// Initialize all climate-related event hooks
        /// </summary>
        /// <returns>Number of successfully hooked methods</returns>
        public override int InitializeEventHooks()
        {
            _log.Debug("🌡️ Setting up enhanced climate event hooks...");

            _planetType = GameTypeInitializer.GetPlanetType();
            if (_planetType == null)
            {
                _log.Warning("Planet type not found, skipping climate hooks");
                return 0;
            }

            // Enhanced climate methods with better detection
            var climateHooks = new Dictionary<string, string>
            {
                { "SetAverageTemperature", "Temperature" },
                { "SetTemperature", "Temperature" },
                { "SetCO2Pressure", "CO2Pressure" },
                { "SetO2Pressure", "O2Pressure" },
                { "SetOxygenPressure", "O2Pressure" },
                { "SetN2Pressure", "N2Pressure" },
                { "SetNitrogenPressure", "N2Pressure" },
                { "SetGHGPressure", "GHGPressure" },
                { "SetGreenhouseGasPressure", "GHGPressure" },
                { "SetWaterStock", "WaterStock" },
                { "SetWater", "WaterStock" },
                { "SetTotalPressure", "TotalPressure" },
                { "SetPressure", "TotalPressure" },
                { "SetArgonPressure", "ArgonPressure" }
            };

            int hookedCount = 0;
            foreach (var (methodName, climateType) in climateHooks)
            {
                if (CreateClimateMethodHook(methodName, climateType))
                {
                    hookedCount++;
                }
            }

            _log.Info($"✅ Climate hooks initialized: {hookedCount}/{climateHooks.Count} methods hooked");
            return hookedCount;
        }

        /// <summary>
        /// Create a climate-specific method hook with prefix and postfix handling
        /// </summary>
        /// <param name="methodName">Method name to hook</param>
        /// <param name="climateType">Type of climate parameter</param>
        /// <returns>True if hook was successfully created</returns>
        private bool CreateClimateMethodHook(string methodName, string climateType)
        {
            if (!ValidateMethodForPatching(_planetType, methodName))
            {
                return false;
            }

            try
            {
                var method = _planetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                
                // Create harmony patches
                var prefix = new HarmonyMethod(typeof(ClimateEventPatchingService), nameof(ClimatePrefix));
                var postfix = new HarmonyMethod(typeof(ClimateEventPatchingService), nameof(ClimatePostfix));

                _harmony.Patch(method, prefix: prefix, postfix: postfix);

                var patchKey = $"Planet.{methodName}";
                _patchedMethods[patchKey] = climateType;
                
                _log.Debug($"✓ Hooked {patchKey} for {climateType} events");
                return true;
            }
            catch (Exception ex)
            {
                _log.Warning($"Failed to hook {methodName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Enhanced Harmony prefix for climate methods
        /// Captures the old value before method execution
        /// </summary>
        [HarmonyPrefix]
        public static void ClimatePrefix(object __instance, Dictionary<string, object> __state, MethodBase __originalMethod)
        {
            try
            {
                if (__state == null)
                    __state = new Dictionary<string, object>();

                var methodName = __originalMethod.Name;
                var climateType = ExtractClimateTypeFromMethodName(methodName);
                
                // Capture current value before change
                var currentValue = GetCurrentClimateValue(__instance, climateType);
                __state["OldValue"] = currentValue;
                __state["ClimateType"] = climateType;
                __state["MethodName"] = methodName;
            }
            catch (Exception)
            {
                // Fail silently to avoid disrupting game flow
            }
        }

        /// <summary>
        /// Enhanced Harmony postfix for climate methods
        /// Publishes climate change events with before/after values
        /// </summary>
        [HarmonyPostfix]
        public static void ClimatePostfix(object __instance, Dictionary<string, object> __state, 
            MethodBase __originalMethod, object[] __args)
        {
            try
            {
                if (__state == null || !__state.ContainsKey("OldValue"))
                    return;

                var climateType = (string)__state["ClimateType"];
                var methodName = (string)__state["MethodName"];
                var oldValue = __state["OldValue"];
                var newValue = ExtractNewValue(__args);

                // Only publish if value actually changed
                if (!ValuesEqual(oldValue, newValue))
                {
                    var eventData = new
                    {
                        Planet = __instance,
                        ClimateType = climateType,
                        OldValue = oldValue,
                        NewValue = newValue,
                        MethodName = methodName,
                        Timestamp = DateTime.UtcNow
                    };

                    // Publish specific climate event
                    ModEventBus.Publish($"Climate{climateType}Changed", eventData);
                    
                    // Publish generic climate event
                    ModEventBus.Publish("ClimateChanged", eventData);
                }
            }
            catch (Exception)
            {
                // Fail silently to avoid disrupting game flow
            }
        }

        /// <summary>
        /// Get current climate value with enhanced detection
        /// </summary>
        /// <param name="instance">Planet instance</param>
        /// <param name="climateType">Type of climate parameter</param>
        /// <returns>Current climate value or null if not found</returns>
        private static object GetCurrentClimateValue(object instance, string climateType)
        {
            try
            {
                var planetType = instance.GetType();

                // Try common property naming patterns
                var propertyNames = GeneratePropertyNames(climateType);
                
                foreach (var propertyName in propertyNames)
                {
                    var property = planetType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                    if (property != null && property.CanRead)
                    {
                        return property.GetValue(instance);
                    }
                }

                // Try field access as fallback
                foreach (var propertyName in propertyNames)
                {
                    var field = planetType.GetField(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field != null)
                    {
                        return field.GetValue(instance);
                    }
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Generate possible property names for a climate type
        /// </summary>
        /// <param name="climateType">Climate type identifier</param>
        /// <returns>Array of possible property names</returns>
        private static string[] GeneratePropertyNames(string climateType)
        {
            return climateType switch
            {
                "Temperature" => new[] { "averageTemperature", "temperature", "temp", "AverageTemperature", "Temperature" },
                "CO2Pressure" => new[] { "co2Pressure", "CO2Pressure", "carbonDioxidePressure" },
                "O2Pressure" => new[] { "o2Pressure", "O2Pressure", "oxygenPressure", "oxygen" },
                "N2Pressure" => new[] { "n2Pressure", "N2Pressure", "nitrogenPressure", "nitrogen" },
                "GHGPressure" => new[] { "ghgPressure", "GHGPressure", "greenhouseGasPressure" },
                "WaterStock" => new[] { "waterStock", "water", "WaterStock", "Water" },
                "TotalPressure" => new[] { "totalPressure", "pressure", "TotalPressure", "Pressure" },
                "ArgonPressure" => new[] { "argonPressure", "argon", "ArgonPressure", "Argon" },
                _ => new[] { climateType.ToLowerInvariant(), climateType }
            };
        }

        /// <summary>
        /// Extract climate type from method name
        /// </summary>
        /// <param name="methodName">Method name</param>
        /// <returns>Climate type identifier</returns>
        private static string ExtractClimateTypeFromMethodName(string methodName)
        {
            return methodName switch
            {
                "SetAverageTemperature" or "SetTemperature" => "Temperature",
                "SetCO2Pressure" => "CO2Pressure",
                "SetO2Pressure" or "SetOxygenPressure" => "O2Pressure",
                "SetN2Pressure" or "SetNitrogenPressure" => "N2Pressure",
                "SetGHGPressure" or "SetGreenhouseGasPressure" => "GHGPressure",
                "SetWaterStock" or "SetWater" => "WaterStock",
                "SetTotalPressure" or "SetPressure" => "TotalPressure",
                "SetArgonPressure" => "ArgonPressure",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Extract new value from method arguments
        /// </summary>
        /// <param name="args">Method arguments</param>
        /// <returns>New value or null</returns>
        private static object ExtractNewValue(object[] args)
        {
            if (args == null || args.Length == 0)
                return null;

            // First argument is typically the new value in setter methods
            return args[0];
        }

        /// <summary>
        /// Compare values for equality handling floating point precision
        /// </summary>
        /// <param name="oldValue">Old value</param>
        /// <param name="newValue">New value</param>
        /// <returns>True if values are considered equal</returns>
        private static bool ValuesEqual(object oldValue, object newValue)
        {
            if (oldValue == null && newValue == null)
                return true;
            
            if (oldValue == null || newValue == null)
                return false;

            // Handle floating point comparison with tolerance
            if (oldValue is float oldFloat && newValue is float newFloat)
            {
                return Math.Abs(oldFloat - newFloat) < 0.0001f;
            }

            if (oldValue is double oldDouble && newValue is double newDouble)
            {
                return Math.Abs(oldDouble - newDouble) < 0.0001;
            }

            return oldValue.Equals(newValue);
        }

        /// <summary>
        /// Get diagnostic information about climate event hooks
        /// </summary>
        /// <returns>Diagnostic information string</returns>
        public string GetDiagnosticInfo()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine("=== Climate Event Patching Service ===");
            info.AppendLine($"Planet Type: {GetFriendlyTypeName(_planetType)}");
            info.AppendLine($"Hooked Methods: {_patchedMethods.Count}");
            info.AppendLine();

            foreach (var patch in _patchedMethods)
            {
                info.AppendLine($"  ✓ {patch.Key} → {patch.Value}");
            }

            return info.ToString();
        }
    }
}