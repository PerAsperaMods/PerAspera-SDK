using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;
// using PerAspera.GameAPI.Events; // TODO: Restore after Events project compiles

namespace PerAspera.GameAPI.Native
{
    /// <summary>
    /// Enhanced native event patching system for the GameAPI
    /// Provides comprehensive hooks for native game events using Harmony IL2CPP patches
    /// </summary>
    public static class NativeEventPatcher
    {
        private static readonly LogAspera _log = new LogAspera("GameAPI.NativeEventPatcher");
        private static readonly Dictionary<string, string> _patchedMethods = new();
        private static bool _isInitialized = false;
        private static Harmony? _harmony;

        // Context for method patches
        private static readonly Dictionary<string, object> _patchContext = new();

        /// <summary>
        /// Initialize the native event patching system
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized)
                return;

            try
            {
                _log.Info("🔧 Initializing enhanced native event patching System..");

                // Initialize Harmony with unique ID
                _harmony = new Harmony("PerAspera.GameAPI.NativeEvents.v2");

                // Setup enhanced event hooks
                SetupClimateEventHooks();
                SetupTimeEventHooks();
                SetupResourceEventHooks();
                SetupGameStateEventHooks();
                SetupBuildingEventHooks();

                _isInitialized = true;
                _log.Info($"✅ Enhanced native event patching system initialized with {_patchedMethods.Count} patches");

                // Publish initialization event
                ModEventBus.Publish("NativeEventPatcherInitialized", new { 
                    PatchCount = _patchedMethods.Count, 
                    Timestamp = DateTime.Now 
                });
            }
            catch (Exception ex)
            {
                _log.Error($"❌ Failed to initialize enhanced native event patcher: {ex.Message}");
            }
        }

        /// <summary>
        /// Setup enhanced climate-related event hooks
        /// </summary>
        private static void SetupClimateEventHooks()
        {
            try
            {
                _log.Debug("🌡️ Setting up enhanced climate event hooks...");

                var planetType = GameTypeInitializer.GetPlanetType();
                if (planetType == null)
                {
                    _log.Warning("Planet type not found, skipping climate hooks");
                    return;
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
                    if (CreateClimateMethodHook(planetType, methodName, climateType))
                    {
                        hookedCount++;
                    }
                }

                _log.Info($"🌡️ Climate hooks setup complete: {hookedCount}/{climateHooks.Count} methods hooked");
            }
            catch (Exception ex)
            {
                _log.Error($"Error setting up climate hooks: {ex.Message}");
            }
        }

        /// <summary>
        /// Create an enhanced hook for a climate method
        /// </summary>
        private static bool CreateClimateMethodHook(System.Type planetType, string methodName, string climateType)
        {
            try
            {
                var methods = planetType.GetMethods()
                    .Where(m => m.Name == methodName && m.GetParameters().Length > 0)
                    .ToArray();

                if (!methods.Any())
                {
                    _log.Debug($"⚠️ Climate method {methodName} not found");
                    return false;
                }

                foreach (var method in methods)
                {
                    try
                    {
                        // Store context for the patch
                        var contextKey = $"{method.DeclaringType?.Name}.{method.Name}";
                        _patchContext[contextKey] = climateType;

                        // Create enhanced Harmony patches
                        var prefix = new HarmonyMethod(typeof(NativeEventPatcher), nameof(EnhancedClimatePrefix));
                        var postfix = new HarmonyMethod(typeof(NativeEventPatcher), nameof(EnhancedClimatePostfix));

                        _harmony?.Patch(method, prefix, postfix);
                        
                        var patchKey = $"{planetType.Name}.{methodName}";
                        _patchedMethods[patchKey] = climateType;
                        
                        _log.Debug($"✅ Enhanced climate patch created: {methodName} -> {climateType}");
                        return true;
                    }
                    catch (Exception ex)
                    {
                        _log.Warning($"Failed to patch method {methodName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Warning($"❌ Failed to hook climate method {methodName}: {ex.Message}");
            }

            return false;
        }

        /// <summary>
        /// Enhanced Harmony prefix for climate methods
        /// </summary>
        public static void EnhancedClimatePrefix(
            object __instance, 
            ref Dictionary<string, object> __state,
            System.Reflection.MethodBase __originalMethod)
        {
            try
            {
                var contextKey = $"{__originalMethod.DeclaringType?.Name}.{__originalMethod.Name}";
                var climateType = _patchContext.TryGetValue(contextKey, out var ct) ? ct.ToString() : "Unknown";

                __state = new Dictionary<string, object>
                {
                    ["Instance"] = __instance,
                    ["ClimateType"] = climateType,
                    ["MethodName"] = __originalMethod.Name,
                    ["Timestamp"] = DateTime.Now
                };

                // Get current value using enhanced detection
                var currentValue = GetCurrentClimateValue(__instance, climateType);
                if (currentValue.HasValue)
                {
                    __state["PreviousValue"] = currentValue.Value;
                }

                _log.Debug($"🔍 Climate method called: {__originalMethod.Name} ({climateType})");
            }
            catch (Exception ex)
            {
                _log.Debug($"Error in enhanced climate prefix: {ex.Message}");
            }
        }

        /// <summary>
        /// Enhanced Harmony postfix for climate methods
        /// </summary>
        public static void EnhancedClimatePostfix(
            object __instance, 
            Dictionary<string, object>? __state,
            System.Reflection.MethodBase __originalMethod,
            object[] __args)
        {
            try
            {
                if (__state == null)
                    return;

                var climateType = __state["ClimateType"].ToString();
                var methodName = __state["MethodName"].ToString();
                
                if (string.IsNullOrEmpty(climateType))
                    return;

                // Get new value from method arguments or instance
                var newValue = ExtractNewValue(__args);
                if (!newValue.HasValue)
                {
                    newValue = GetCurrentClimateValue(__instance, climateType);
                }

                if (!newValue.HasValue)
                {
                    _log.Debug($"Could not determine new value for {climateType}");
                    return;
                }

                // TODO: Create enhanced climate event (ClimateEventData needs refactoring)
                // var eventData = new ClimateEventData { ... };
                // BridgeClimateEvent(eventData, climateType);

                // _log.Debug($"🌡️ Climate change: {eventData}");
                _log.Debug($"🌡️ Climate change detected");
            }
            catch (Exception ex)
            {
                _log.Error($"Error in enhanced climate postfix: {ex.Message}");
            }
        }

        /// <summary>
        /// Get current climate value with enhanced detection
        /// </summary>
        private static float? GetCurrentClimateValue(object instance, string climateType)
        {
            try
            {
                return climateType.ToLower() switch
                {
                    "temperature" => instance.InvokeMethod<float>("GetAverageTemperature") ,
                    
                    "co2pressure" => instance.InvokeMethod<float>("GetCO2Pressure") ,
                    
                    "o2pressure" => instance.InvokeMethod<float>("GetO2Pressure"),
                    
                    "n2pressure" => instance.InvokeMethod<float>("GetN2Pressure"),
                    
                    "ghgpressure" => instance.InvokeMethod<float>("GetGHGPressure"),
                    
                    "waterstock" => instance.InvokeMethod<float>("GetWaterStock"),
                    
                    "totalpressure" => instance.InvokeMethod<float>("GetTotalPressure"),
                    
                    "argonpressure" => instance.InvokeMethod<float>("GetArgonPressure"),
                    
                    "dustconcentration" => instance.InvokeMethod<float>("GetDustConcentration"),
                    
                    _ => null
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Extract new value from method arguments
        /// </summary>
        private static float? ExtractNewValue(object[] args)
        {
            if (args == null || args.Length == 0)
                return null;

            foreach (var arg in args)
            {
                if (arg is float f)
                    return f;
                if (arg is double d)
                    return (float)d;
                if (arg is int i)
                    return i;
                
                // Try to parse string values
                if (arg is string s && float.TryParse(s, out var parsed))
                    return parsed;
            }

            return null;
        }

        /// <summary>
        /// Setup enhanced time/day progression hooks
        /// </summary>
        private static void SetupTimeEventHooks()
        {
            try
            {
                _log.Debug("🕒 Setting up enhanced time event hooks...");

                var universeType = GameTypeInitializer.GetUniverseType();
                if (universeType == null)
                {
                    _log.Warning("Universe type not found, skipping time hooks");
                    return;
                }

                // Multiple day progression method names
                var dayMethods = new[] { "AdvanceDay", "NextDay", "IncrementDay", "ProgressDay", "UpdateDay" };
                
                int hookedCount = 0;
                foreach (var methodName in dayMethods)
                {
                    var method = universeType.GetMethod(methodName);
                    if (method != null)
                    {
                        var postfix = new HarmonyMethod(typeof(NativeEventPatcher), nameof(EnhancedDayAdvancedPostfix));
                        _harmony?.Patch(method, postfix: postfix);
                        
                        _patchedMethods[$"Universe.{methodName}"] = "DayProgression";
                        hookedCount++;
                        _log.Debug($"✅ Day progression hook: {methodName}");
                    }
                }

                _log.Info($"🕒 Time hooks setup: {hookedCount} methods hooked");
            }
            catch (Exception ex)
            {
                _log.Error($"Error setting up time hooks: {ex.Message}");
            }
        }

        /// <summary>
        /// Enhanced day advancement postfix
        /// </summary>
        public static void EnhancedDayAdvancedPostfix(object __instance)
        {
            try
            {
                var currentSol = GetCurrentMartianSol();
                var previousSol = currentSol - 1;
                
                // Check if it's a new Martian year (approximately 687 days)
                var currentYear = currentSol / 687;
                var previousYear = previousSol / 687;
                var isNewYear = currentYear > previousYear;

                // TODO: Create MartianDayEventData when Events system is finalized
                /*
                var eventData = new MartianDayEventData
                {
                    DaysPassed = currentSol,
                    Planet = null, // MirrorUniverse.GetPlanet() obsolete
                    Timestamp = DateTime.Now
                };

                // Bridge to event system
                ModEventBus.Publish("MartianDayChanged", eventData);
                ModEventBus.Publish("Native.MartianDayChanged", eventData);
                */

                // Legacy compatibility - remove until GameAPI is updated
                // GameAPI.OnDayPassed(currentSol, previousSol);

                var yearMsg = isNewYear ? $" 🎉 NEW MARTIAN YEAR {currentYear + 1}!" : "";
                // _log.Debug($"🕒 Day advanced: {eventData}{yearMsg}");
                _log.Debug($"🕒 Day advanced: Sol {currentSol}{yearMsg}");
            }
            catch (Exception ex)
            {
                _log.Error($"Error in enhanced day advanced postfix: {ex.Message}");
            }
        }

        /// <summary>
        /// Setup enhanced resource event hooks
        /// </summary>
        private static void SetupResourceEventHooks()
        {
            try
            {
                _log.Debug("💎 Setting up enhanced resource event hooks...");

                var factionType = GameTypeInitializer.GetFactionType();
                if (factionType == null)
                {
                    _log.Warning("Faction type not found, skipping resource hooks");
                    return;
                }

                var resourceMethods = new[] { 
                    "AddResource", "ReceiveResource", "ConsumeResource", 
                    "UseResource", "SpendResource", "GainResource" 
                };
                
                int hookedCount = 0;
                foreach (var methodName in resourceMethods)
                {
                    var method = factionType.GetMethod(methodName);
                    if (method != null)
                    {
                        var prefix = new HarmonyMethod(typeof(NativeEventPatcher), nameof(EnhancedResourcePrefix));
                        var postfix = new HarmonyMethod(typeof(NativeEventPatcher), nameof(EnhancedResourcePostfix));
                        _harmony?.Patch(method, prefix, postfix);
                        
                        _patchedMethods[$"Faction.{methodName}"] = "ResourceChange";
                        hookedCount++;
                        _log.Debug($"✅ Resource hook: {methodName}");
                    }
                }

                _log.Info($"💎 Resource hooks setup: {hookedCount} methods hooked");
            }
            catch (Exception ex)
            {
                _log.Error($"Error setting up resource hooks: {ex.Message}");
            }
        }

        /// <summary>
        /// Enhanced resource prefix
        /// </summary>
        public static void EnhancedResourcePrefix(
            object __instance, 
            ref Dictionary<string, object> __state,
            object[] __args,
            System.Reflection.MethodBase __originalMethod)
        {
            try
            {
                __state = new Dictionary<string, object>
                {
                    ["Instance"] = __instance,
                    ["Operation"] = ExtractOperationFromMethodName(__originalMethod.Name),
                    ["Args"] = __args,
                    ["Timestamp"] = DateTime.Now
                };

                // Try to extract resource info from arguments
                if (__args != null && __args.Length > 0)
                {
                    var resourceInfo = ExtractResourceInfo(__args);
                    if (resourceInfo.HasValue)
                    {
                        __state["ResourceName"] = resourceInfo.Value.name;
                        __state["Amount"] = resourceInfo.Value.amount;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Debug($"Error in enhanced resource prefix: {ex.Message}");
            }
        }

        /// <summary>
        /// Enhanced resource postfix
        /// </summary>
        public static void EnhancedResourcePostfix(
            object __instance, 
            Dictionary<string, object>? __state)
        {
            try
            {
                if (__state == null)
                    return;

                var operation = __state.TryGetValue("Operation", out var op) ? op.ToString() : "Unknown";
                var resourceName = __state.TryGetValue("ResourceName", out var rn) ? rn.ToString() : "Unknown";
                var amount = __state.TryGetValue("Amount", out var amt) && amt is float a ? a : 0f;

                // TODO: Restore when Events project compiles
                /*
                var eventData = new ResourceChangedNativeEvent
                {
                    ResourceKey = resourceName ?? "Unknown",
                    Amount = amount,
                    Operation = operation ?? "Unknown",
                    SourceFaction = __instance,
                    MartianSol = GetCurrentMartianSol()
                };
                */

                // Bridge to event system - restore when Events compiles
                // ModEventBus.Publish("NativeResourceChanged", eventData);
                // ModEventBus.Publish($"Native.Resource.{operation}", eventData);

                // Legacy compatibility - remove until GameAPI is updated
                // GameAPI.OnResourceAdded(eventData.ResourceName, eventData.Amount);

                // _log.Debug($"💦 Resource change: {eventData}");
                _log.Debug($"💦 Resource change: {operation} - {resourceName}: {amount}");
            }
            catch (Exception ex)
            {
                _log.Error($"Error in enhanced resource postfix: {ex.Message}");
            }
        }

        /// <summary>
        /// Setup game state event hooks
        /// </summary>
        private static void SetupGameStateEventHooks()
        {
            try
            {
                _log.Debug("🎮 Setting up game state event hooks...");

                var baseGameType = GameTypeInitializer.GetBaseGameType();
                if (baseGameType == null)
                    return;

                var stateMethods = new[] { "SetGameSpeed", "SetPaused", "Pause", "Resume" };
                
                foreach (var methodName in stateMethods)
                {
                    var method = baseGameType.GetMethod(methodName);
                    if (method != null)
                    {
                        var postfix = new HarmonyMethod(typeof(NativeEventPatcher), nameof(GameStateChangedPostfix));
                        _harmony?.Patch(method, postfix: postfix);
                        _patchedMethods[$"BaseGame.{methodName}"] = "GameState";
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error setting up game state hooks: {ex.Message}");
            }
        }

        /// <summary>
        /// Setup building event hooks
        /// </summary>
        private static void SetupBuildingEventHooks()
        {
            try
            {
                _log.Debug("🏗️ Setting up building event hooks...");
                // TODO: Implement building hooks when building types are discovered
            }
            catch (Exception ex)
            {
                _log.Error($"Error setting up building hooks: {ex.Message}");
            }
        }

        /// <summary>
        /// Game state changed postfix
        /// </summary>
        public static void GameStateChangedPostfix(object __instance, object[] __args, System.Reflection.MethodBase __originalMethod)
        {
            try
            {
                var stateType = __originalMethod.Name switch
                {
                    "SetGameSpeed" => "Speed",
                    "SetPaused" or "Pause" or "Resume" => "Pause",
                    _ => "Unknown"
                };

                // TODO: Restore when Events project compiles
                /*
                var eventData = new GameStateChangedNativeEvent
                {
                    StateType = stateType,
                    CurrentValue = __args?.FirstOrDefault(),
                    MartianSol = GetCurrentMartianSol()
                };
                */

                // ModEventBus.Publish("NativeGameStateChanged", eventData);
                // _log.Debug($"🎮 Game state change: {eventData}");
                _log.Debug($"🎮 Game state change: {stateType}");
            }
            catch (Exception ex)
            {
                _log.Error($"Error in game state postfix: {ex.Message}");
            }
        }

        // Helper methods
        private static string ExtractOperationFromMethodName(string methodName) => methodName switch
        {
            var name when name.Contains("Add") || name.Contains("Receive") || name.Contains("Gain") => "Add",
            var name when name.Contains("Consume") || name.Contains("Use") || name.Contains("Spend") => "Remove",
            _ => "Unknown"
        };

        private static (string name, float amount)? ExtractResourceInfo(object[] args)
        {
            // Implementation depends on actual resource method signatures
            // This is a simplified version
            string? name = null;
            float amount = 0f;

            foreach (var arg in args)
            {
                if (arg is string s)
                    name = s;
                else if (arg is float f)
                    amount = f;
                else if (arg is int i)
                    amount = i;
            }

            return name != null ? (name, amount) : null;
        }

        /// <summary>
        /// Enhanced climate event bridging
        /// </summary>
        // TODO: Restore when ClimateEventData is refactored
        /*private static void BridgeClimateEvent(ClimateEventData eventData, string climateType)
        {
            try
            {
                // Publish to main event system
                ModEventBus.Publish(eventData.EventType, eventData);
                ModEventBus.Publish("NativeClimateChanged", eventData);

                // Specific compatibility events for ClimatAspera
                var analysisType = climateType.ToLower() switch  // ✅ Utiliser le paramètre
                {
                    "temperature" => "TemperatureAnalysis",
                    "o2pressure" => "O2Analysis", 
                    "waterstock" => "WaterAnalysis",
                    "co2pressure" => "CO2Analysis",
                    _ => $"{climateType}Analysis"
                };

                ModEventBus.Publish("ClimateAnalysisComplete", new ClimateAnalysisCompleteEvent(analysisType, eventData));
            }
            catch (Exception ex)
            {
                _log.Error($"Error bridging climate event: {ex.Message}");
            }
        }*/

        /// <summary>
        /// Get current Martian sol safely with fallbacks
        /// </summary>
        private static int GetCurrentMartianSol()
        {
            // TODO: Use Wrappers.Universe.GetCurrent() when available
            return 1; // Fallback
            /*
            try
            {
                // Try multiple sources
                return MirrorUniverse.GetCurrentMartianSol();
            }
            catch
            {
                try
                {
                    var universeInstance = MirrorUniverse.Shared?.Instance;
                    return universeInstance?.InvokeMethod<int>("GetCurrentSol") ?? 1;
                }
                catch
                {
                    return 1;
                }
            }
            */
        }

        /// <summary>
        /// Enhanced shutdown with proper cleanup
        /// </summary>
        public static void Shutdown()
        {
            if (!_isInitialized)
                return;

            try
            {
                _log.Info("🛑 Shutting down enhanced native event patcher...");

                // Unpatch all our patches
                _harmony?.UnpatchSelf();

                // Clear context
                _patchContext.Clear();
                _patchedMethods.Clear();

                _isInitialized = false;
                _log.Info($"✅ Enhanced native event patcher shut down ({_patchedMethods.Count} patches removed)");
            }
            catch (Exception ex)
            {
                _log.Error($"Error shutting down enhanced native event patcher: {ex.Message}");
            }
        }

        /// <summary>
        /// Enhanced statistics
        /// </summary>
        public static Dictionary<string, object> GetStats()
        {
            return new Dictionary<string, object>
            {
                ["IsInitialized"] = _isInitialized,
                ["PatchedMethodsCount"] = _patchedMethods.Count,
                ["PatchedMethods"] = _patchedMethods.Keys.ToList(),
                ["HarmonyId"] = _harmony?.Id ?? "None",
                ["ContextEntries"] = _patchContext.Count,
                ["NativeInstancesAvailable"] = 1,
                ["Timestamp"] = DateTime.Now
            };
        }

        public static void OnDayPassedPatch()
        {
            // TODO: Implement when GameAPI methods are available
            // GameAPI.TriggerDayPassed();
        }

        public static void OnResourceAddedPatch(string resource, float amount)
        {
            // TODO: Implement when GameAPI methods are available
            // GameAPI.TriggerResourceAdded(resource, amount);
        }
    }
}