using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Native.Events
{
    /// <summary>
    /// Time event patching service for Per Aspera
    /// Handles all time-related events including day progression, time speed, and temporal state changes
    /// </summary>
    public sealed class TimeEventPatchingService : BaseEventPatchingService
    {
        private System.Type _baseGameType;
        private System.Type _timeManagerType;

        /// <summary>
        /// Initialize time event patching service
        /// </summary>
        /// <param name="harmony">Harmony instance for IL2CPP patching</param>
        public TimeEventPatchingService(Harmony harmony) 
            : base("Time", harmony)
        {
        }

        /// <summary>
        /// Get the event type identifier for this service
        /// </summary>
        /// <returns>Event type string</returns>
        public override string GetEventType() => "Time";

        /// <summary>
        /// Initialize all time-related event hooks
        /// </summary>
        /// <returns>Number of successfully hooked methods</returns>
        public override int InitializeEventHooks()
        {
            _log.Debug("⏰ Setting up enhanced time event hooks...");

            _baseGameType = GameTypeInitializer.GetBaseGameType();
            _timeManagerType = GameTypeInitializer.GetTimeManagerType();

            if (_baseGameType == null && _timeManagerType == null)
            {
                _log.Warning("Time-related types not found, skipping time hooks");
                return 0;
            }

            // Enhanced time methods with comprehensive coverage
            var timeHooks = new Dictionary<string, (System.Type type, string eventType)>
            {
                // Day progression events
                { "AdvanceDay", (_timeManagerType, "DayAdvance") },
                { "NextDay", (_timeManagerType, "DayAdvance") },
                { "StartNewDay", (_timeManagerType, "DayAdvance") },
                
                // Time speed events
                { "SetTimeSpeed", (_baseGameType, "TimeSpeed") },
                { "ChangeTimeSpeed", (_baseGameType, "TimeSpeed") },
                { "SetSpeed", (_baseGameType, "TimeSpeed") },
                
                // Pause/Resume events
                { "Pause", (_baseGameType, "GamePause") },
                { "Resume", (_baseGameType, "GameResume") },
                { "PauseGame", (_baseGameType, "GamePause") },
                { "ResumeGame", (_baseGameType, "GameResume") },
                { "SetPaused", (_baseGameType, "GamePause") },
                
                // Time progression events
                { "UpdateTime", (_timeManagerType, "TimeUpdate") },
                { "TickTime", (_timeManagerType, "TimeTick") },
                { "ProcessTimeStep", (_timeManagerType, "TimeStep") }
            };

            int hookedCount = 0;
            foreach (var (methodName, (type, eventType)) in timeHooks)
            {
                if (type != null && CreateTimeMethodHook(type, methodName, eventType))
                {
                    hookedCount++;
                }
            }

            _log.Info($"✅ Time hooks initialized: {hookedCount}/{timeHooks.Count} methods hooked");
            return hookedCount;
        }

        /// <summary>
        /// Create a time-specific method hook with prefix and postfix handling
        /// </summary>
        /// <param name="targetType">Type containing the method</param>
        /// <param name="methodName">Method name to hook</param>
        /// <param name="eventType">Type of time event</param>
        /// <returns>True if hook was successfully created</returns>
        private bool CreateTimeMethodHook(System.Type targetType, string methodName, string eventType)
        {
            if (!ValidateMethodForPatching(targetType, methodName))
            {
                return false;
            }

            try
            {
                var method = targetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                
                // Create harmony patches with event type context
                var prefix = new HarmonyMethod(typeof(TimeEventPatchingService), nameof(TimePrefix));
                var postfix = new HarmonyMethod(typeof(TimeEventPatchingService), nameof(TimePostfix));

                _harmony.Patch(method, prefix: prefix, postfix: postfix);

                var patchKey = $"{targetType.Name}.{methodName}";
                _patchedMethods[patchKey] = eventType;
                
                _log.Debug($"✓ Hooked {patchKey} for {eventType} events");
                return true;
            }
            catch (Exception ex)
            {
                _log.Warning($"Failed to hook {methodName}: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Enhanced Harmony prefix for time methods
        /// Captures the old time state before method execution
        /// </summary>
        [HarmonyPrefix]
        public static void TimePrefix(object __instance, Dictionary<string, object> __state, MethodBase __originalMethod)
        {
            try
            {
                if (__state == null)
                    __state = new Dictionary<string, object>();

                var methodName = __originalMethod.Name;
                var timeType = ExtractTimeTypeFromMethodName(methodName);
                
                // Capture current time state before change
                var currentTimeState = GetCurrentTimeState(__instance);
                __state["OldState"] = currentTimeState;
                __state["TimeType"] = timeType;
                __state["MethodName"] = methodName;
                __state["Instance"] = __instance;
                __state["Timestamp"] = DateTime.UtcNow;
            }
            catch (Exception)
            {
                // Fail silently to avoid disrupting game flow
            }
        }

        /// <summary>
        /// Enhanced Harmony postfix for time methods
        /// Publishes time change events with before/after states
        /// </summary>
        [HarmonyPostfix]
        public static void TimePostfix(object __instance, Dictionary<string, object> __state, 
            MethodBase __originalMethod, object[] __args)
        {
            try
            {
                if (__state == null || !__state.ContainsKey("OldState"))
                    return;

                var timeType = (string)__state["TimeType"];
                var methodName = (string)__state["MethodName"];
                var oldState = __state["OldState"];
                var timestamp = (DateTime)__state["Timestamp"];

                // Capture new time state after change
                var newState = GetCurrentTimeState(__instance);

                // Always publish time events (even if state unchanged, as timing matters)
                var eventData = new
                {
                    Instance = __instance,
                    TimeType = timeType,
                    OldState = oldState,
                    NewState = newState,
                    MethodName = methodName,
                    Arguments = ExtractMethodArguments(__args),
                    Timestamp = timestamp,
                    Duration = DateTime.UtcNow - timestamp
                };

                // Publish specific time event
                ModEventBus.Publish($"Time{timeType}", eventData);
                
                // Publish generic time event
                ModEventBus.Publish("TimeChanged", eventData);

                // Special handling for critical time events
                PublishSpecialTimeEvents(timeType, eventData);
            }
            catch (Exception)
            {
                // Fail silently to avoid disrupting game flow
            }
        }

        /// <summary>
        /// Get current time state with comprehensive information
        /// </summary>
        /// <param name="instance">Game instance</param>
        /// <returns>Current time state object</returns>
        private static object GetCurrentTimeState(object instance)
        {
            try
            {
                var timeState = new Dictionary<string, object>();

                // Try to extract day information
                var dayInfo = ExtractDayInformation(instance);
                if (dayInfo != null)
                {
                    timeState["Day"] = dayInfo;
                }

                // Try to extract speed information
                var speedInfo = ExtractSpeedInformation(instance);
                if (speedInfo != null)
                {
                    timeState["Speed"] = speedInfo;
                }

                // Try to extract pause state
                var pauseState = ExtractPauseState(instance);
                if (pauseState != null)
                {
                    timeState["IsPaused"] = pauseState;
                }

                return timeState.Count > 0 ? timeState : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Extract day information from game instance
        /// </summary>
        /// <param name="instance">Game instance</param>
        /// <returns>Day information or null</returns>
        private static object ExtractDayInformation(object instance)
        {
            try
            {
                var instanceType = instance.GetType();

                // Try common day property names
                var dayProperties = new[] { "currentDay", "day", "CurrentDay", "Day", "dayNumber", "DayNumber" };
                
                foreach (var propName in dayProperties)
                {
                    var property = instanceType.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                    if (property != null && property.CanRead)
                    {
                        return property.GetValue(instance);
                    }

                    var field = instanceType.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
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
        /// Extract speed information from game instance
        /// </summary>
        /// <param name="instance">Game instance</param>
        /// <returns>Speed information or null</returns>
        private static object ExtractSpeedInformation(object instance)
        {
            try
            {
                var instanceType = instance.GetType();

                // Try common speed property names
                var speedProperties = new[] { "timeSpeed", "speed", "TimeSpeed", "Speed", "gameSpeed", "GameSpeed" };
                
                foreach (var propName in speedProperties)
                {
                    var property = instanceType.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                    if (property != null && property.CanRead)
                    {
                        return property.GetValue(instance);
                    }

                    var field = instanceType.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
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
        /// Extract pause state from game instance
        /// </summary>
        /// <param name="instance">Game instance</param>
        /// <returns>Pause state or null</returns>
        private static object ExtractPauseState(object instance)
        {
            try
            {
                var instanceType = instance.GetType();

                // Try common pause property names
                var pauseProperties = new[] { "isPaused", "paused", "IsPaused", "Paused", "gamePaused", "GamePaused" };
                
                foreach (var propName in pauseProperties)
                {
                    var property = instanceType.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);
                    if (property != null && property.CanRead)
                    {
                        return property.GetValue(instance);
                    }

                    var field = instanceType.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
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
        /// Extract time type from method name
        /// </summary>
        /// <param name="methodName">Method name</param>
        /// <returns>Time type identifier</returns>
        private static string ExtractTimeTypeFromMethodName(string methodName)
        {
            return methodName switch
            {
                "AdvanceDay" or "NextDay" or "StartNewDay" => "DayAdvance",
                "SetTimeSpeed" or "ChangeTimeSpeed" or "SetSpeed" => "TimeSpeed",
                "Pause" or "PauseGame" or "SetPaused" => "GamePause",
                "Resume" or "ResumeGame" => "GameResume",
                "UpdateTime" => "TimeUpdate",
                "TickTime" => "TimeTick",
                "ProcessTimeStep" => "TimeStep",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Extract meaningful arguments from method call
        /// </summary>
        /// <param name="args">Method arguments</param>
        /// <returns>Processed arguments object</returns>
        private static object ExtractMethodArguments(object[] args)
        {
            if (args == null || args.Length == 0)
                return null;

            // For single argument, return directly
            if (args.Length == 1)
                return args[0];

            // For multiple arguments, return as dictionary
            var argDict = new Dictionary<string, object>();
            for (int i = 0; i < args.Length; i++)
            {
                argDict[$"Arg{i}"] = args[i];
            }

            return argDict;
        }

        /// <summary>
        /// Publish special time events for critical game state changes
        /// </summary>
        /// <param name="timeType">Type of time event</param>
        /// <param name="eventData">Event data</param>
        private static void PublishSpecialTimeEvents(string timeType, object eventData)
        {
            try
            {
                switch (timeType)
                {
                    case "DayAdvance":
                        ModEventBus.Publish("NewDayStarted", eventData);
                        break;

                    case "GamePause":
                        ModEventBus.Publish("GamePaused", eventData);
                        break;

                    case "GameResume":
                        ModEventBus.Publish("GameResumed", eventData);
                        break;

                    case "TimeSpeed":
                        ModEventBus.Publish("GameSpeedChanged", eventData);
                        break;
                }
            }
            catch (Exception)
            {
                // Fail silently
            }
        }

        /// <summary>
        /// Get diagnostic information about time event hooks
        /// </summary>
        /// <returns>Diagnostic information string</returns>
        public string GetDiagnosticInfo()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine("=== Time Event Patching Service ===");
            info.AppendLine($"BaseGame Type: {GetFriendlyTypeName(_baseGameType)}");
            info.AppendLine($"TimeManager Type: {GetFriendlyTypeName(_timeManagerType)}");
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