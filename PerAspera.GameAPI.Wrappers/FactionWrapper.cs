#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PerAspera.Commands;
using PerAspera.Core.IL2CPP;
using PerAspera.GameAPI.Native;

namespace PerAspera.GameAPI.Wrappers
{
    /// <summary>
    /// Wrapper for the native Faction class.
    /// Provides safe access to faction properties and operations.
    /// </summary>
    public class FactionWrapper : WrapperBase
    {
        public FactionWrapper(object nativeFaction) : base(nativeFaction) { }

        public static FactionWrapper? FromNative(object? nativeFaction)
            => nativeFaction != null ? new FactionWrapper(nativeFaction) : null;

        // ==================== CONSOLE COMMANDS ====================

        /// <summary>Add resources distributed across faction buildings via console command.</summary>
        public bool FactionAddResourceDistributed(string resourceString, string amountString)
        {
            var consoleWrapper = ConsoleWrapper.GetInstance();
            if (consoleWrapper == null)
            {
                WrapperLog.Warning("Console wrapper not available for FactionAddResourceDistributed");
                return false;
            }
            try
            {
                return consoleWrapper.ExecuteCommandString(
                    $"factionaddresourcedistributed {resourceString} {amountString}");
            }
            catch (Exception ex)
            {
                WrapperLog.Error($"FactionAddResourceDistributed failed: {ex.Message}");
                return false;
            }
        }

        public static void ListAvailableConsoleCommands()
        {
            ConsoleWrapper.GetInstance()?.ListCommands();
        }

        // ==================== NATIVE METHODS ====================

        /// <summary>Add research points to this faction.</summary>
        public bool AddResearchPoints(float amount)
        {
            try
            {
                ((Faction)NativeObject).AddResearchPoints(amount);
                return true;
            }
            catch (Exception ex)
            {
                WrapperLog.Error($"AddResearchPoints failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>Direct native call for resource addition (test/debug).</summary>
        public bool TestAddResource(string resourceType, float amount)
            => NativeObject.InvokeMethod("FactionAddResourceDistributed", resourceType, amount.ToString());

        /// <summary>
        /// Sets a bool variable on the faction blackboard.
        /// Used by YAML rules/criteria: <c>$key == true</c>.
        /// </summary>
        public bool SetBlackboardBool(string key, bool value)
        {
            try
            {
                ((Faction)NativeObject).blackboardFaction?.SetValue(key, value);
                return true;
            }
            catch (Exception ex)
            {
                WrapperLog.Error($"SetBlackboardBool({key}) failed: {ex.Message}");
                return false;
            }
        }

        // ==================== HANDLE / INTERACTION ====================

        public InteractionManagerWrapper GetInteractionManager()
            => new InteractionManagerWrapper(NativeObject.GetMemberValue<InteractionManager>("interactionManager"));

        public IHandleable? GetAsIHandleable()
        {
            try { return (IHandleable)GetNativeObject(); }
            catch (InvalidCastException)
            {
                WrapperLog.Warning("Cannot cast Faction to IHandleable in IL2CPP context");
                return null;
            }
        }

        public HandleWrapper? GetHandle()
        {
            try
            {
                var handleObj = SafeInvoke<Handle>("get_handle");
                if (handleObj != null) return HandleWrapper.FromNative(handleObj);

                string[] names = { "<Handle>k__BackingField", "handle", "_handle", "m_handle" };
                foreach (var fieldName in names)
                {
                    try
                    {
                        var h = GetNativeField<Handle>(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
                        if (h != null) return HandleWrapper.FromNative(h);
                    }
                    catch { }
                }
                WrapperLog.Warning($"[GetHandle] No handle field found on {GetNativeObject()?.GetType().Name}");
                return null;
            }
            catch (Exception ex)
            {
                WrapperLog.Error($"[GetHandle] {ex.Message}");
                return null;
            }
        }

        public object? GetRawHandle()
        {
            try { return GetNativeField<object>("handle"); }
            catch (Exception ex)
            {
                WrapperLog.Error($"[GetRawHandle] {ex.Message}");
                return null;
            }
        }

        // ==================== CORE IDENTIFICATION ====================

        public string Name        => GetNativePropertySafe<string>("name") ?? "Unknown";
        public string DisplayName => GetNativePropertySafe<string>("displayName") ?? Name;
        public object? FactionType => GetNativePropertySafe<object>("factionType");
        public bool IsPlayerFaction => GetNativePropertySafe<bool?>("isPlayerFaction") ?? false;

        // ==================== RESOURCES ====================

        public object? MainStockpile => GetNativePropertySafe<object>("mainStockpile");

        /// <summary>Get resource stock amount from faction stockpile.</summary>
        public float GetResourceStock(string resourceKey)
        {
            try
            {
                if (MainStockpile == null) return 0f;
                return CallNative<float?>("GetResourceStock", resourceKey) ??
                       CallNative<float?>("GetStock", resourceKey) ??
                       CallNative<float?>("GetResourceAmount", resourceKey) ?? 0f;
            }
            catch (Exception ex)
            {
                WrapperLog.Warning($"GetResourceStock failed for {resourceKey}: {ex.Message}");
                return 0f;
            }
        }

        /// <summary>Add resource to faction. Use negative amount to remove.</summary>
        public bool AddResource(string resourceKey, float amount)
        {
            try
            {
                var result = CallNative<bool?>("AddResource", resourceKey, amount);
                if (result.HasValue) return result.Value;

                var stockpile = MainStockpile;
                if (stockpile != null)
                {
                    var r2 = CallNative<bool?>("AddResource", stockpile, resourceKey, amount);
                    if (r2.HasValue) return r2.Value;
                }
                WrapperLog.Warning($"Could not add {resourceKey} to faction {Name}");
                return false;
            }
            catch (Exception ex)
            {
                WrapperLog.Error($"AddResource {resourceKey} failed: {ex.Message}");
                return false;
            }
        }

        // ==================== RELATIONS ====================

        public float? GetRelationshipWith(FactionWrapper otherFaction)
        {
            if (!otherFaction.IsValidWrapper) return null;
            try
            {
                return CallNative<float?>("GetRelationship", otherFaction.GetNativeObject()) ??
                       CallNative<float?>("GetDiplomacyStatus", otherFaction.GetNativeObject()) ??
                       CallNative<float?>("GetStanding", otherFaction.GetNativeObject());
            }
            catch (Exception ex)
            {
                WrapperLog.Warning($"GetRelationshipWith failed: {ex.Message}");
                return null;
            }
        }

        // ==================== BUILDINGS ====================

        public List<BuildingWrapper> GetBuildings()
        {
            try
            {
                var buildings = CallNative<object>("get_buildings");
                if (buildings == null)
                {
                    var planet = PlanetWrapper.GetCurrent();
                    if (planet != null)
                    {
                        var planetBuildings = CallNative<object>("get_buildings", planet.GetNativeObject());
                        if (planetBuildings is System.Collections.IEnumerable pe)
                        {
                            return pe.Cast<object?>()
                                     .Where(b => b != null)
                                     .Select(b => new BuildingWrapper(b!))
                                     .Where(bw => bw.IsValidWrapper)
                                     .ToList();
                        }
                    }
                    return new List<BuildingWrapper>();
                }

                if (buildings is System.Collections.IEnumerable enumerable)
                {
                    return enumerable.Cast<object?>()
                                     .Where(b => b != null)
                                     .Select(b => new BuildingWrapper(b!))
                                     .ToList();
                }
                return new List<BuildingWrapper>();
            }
            catch (Exception ex)
            {
                WrapperLog.Warning($"GetBuildings failed: {ex.Message}");
                return new List<BuildingWrapper>();
            }
        }

        // ==================== AI BEHAVIOR ====================

        public int AIDifficulty => GetNativeProperty<int?>("aiDifficulty") ??
                                   GetNativeProperty<int?>("difficultyLevel") ?? 0;

        public bool IsAI => GetNativeProperty<bool?>("isAI") ?? !IsPlayerFaction;

        public string AIPersonality => GetNativeProperty<string>("aiPersonality") ??
                                       GetNativeProperty<string>("behaviorType") ?? "default";

        // ==================== TECHNOLOGY ====================

        public bool HasTechnology(string technologyKey)
        {
            try
            {
                return CallNative<bool?>("HasTechnology", technologyKey) ??
                       CallNative<bool?>("IsTechResearched", technologyKey) ??
                       CallNative<bool?>("HasResearched", technologyKey) ?? false;
            }
            catch (Exception ex)
            {
                WrapperLog.Warning($"HasTechnology {technologyKey} failed: {ex.Message}");
                return false;
            }
        }

        public bool ResearchTechnology(string technologyKey)
        {
            try
            {
                return CallNative<bool?>("ResearchTechnology", technologyKey) ?? false;
            }
            catch (Exception ex)
            {
                WrapperLog.Error($"ResearchTechnology {technologyKey} failed: {ex.Message}");
                return false;
            }
        }

        // ==================== UTILITIES ====================

        public System.Drawing.Color GetColor()
        {
            try
            {
                var color = GetNativeProperty<object>("color") ?? GetNativeProperty<object>("factionColor");
                return color != null ? ExtractColor(color) : System.Drawing.Color.Gray;
            }
            catch { return System.Drawing.Color.Gray; }
        }

        private static System.Drawing.Color ExtractColor(object unityColor)
        {
            try
            {
                var r = unityColor.GetFieldValue<float?>("r") ?? 0.5f;
                var g = unityColor.GetFieldValue<float?>("g") ?? 0.5f;
                var b = unityColor.GetFieldValue<float?>("b") ?? 0.5f;
                var a = unityColor.GetFieldValue<float?>("a") ?? 1.0f;
                return System.Drawing.Color.FromArgb((int)(a * 255), (int)(r * 255), (int)(g * 255), (int)(b * 255));
            }
            catch { return System.Drawing.Color.Gray; }
        }

        // ==================== GAME EVENT BUS ====================

        public object? GetGameEventBus()
        {
            var bus = SafeInvoke<object>("get__gameEventBus");
            if (bus != null) return bus;

            bus = GetNativeField<object>("_gameEventBus", BindingFlags.NonPublic | BindingFlags.Instance) ??
                  GetNativeField<object>("gameEventBus",  BindingFlags.Public    | BindingFlags.Instance) ??
                  GetNativeField<object>("m_gameEventBus", BindingFlags.NonPublic | BindingFlags.Instance) ??
                  GetNativeField<object>("_gameEventBus", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            return bus;
        }

        // ==================== COMMAND EXECUTION ====================

        public bool ExecuteResourceImportCommand(string resourceType, float amount = 1000f)
        {
            var mgr = GetInteractionManager();
            var action = ResourceCommandHelper.CreateNativeTextAction(resourceType, amount);
            return mgr.DispatchAction(NativeObject, GetGameEventBus(), action, "ExecuteResourceImportCommand");
        }

        public bool ExecuteCustomCommand(string commandType, Dictionary<string, object>? parameters = null)
        {
            try
            {
                var handleable = GetAsIHandleable();
                if (handleable == null)
                {
                    WrapperLog.Error($"Cannot get IHandleable for faction {Name}");
                    return false;
                }
                float amount = parameters?.ContainsKey("amount") == true
                    ? Convert.ToSingle(parameters["amount"]) : 1000f;
                return ResourceCommandHelper.ExecuteResourceImportCommand(handleable, commandType, amount);
            }
            catch (Exception ex)
            {
                WrapperLog.Error($"ExecuteCustomCommand {commandType} failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Generic command dispatch via CommandBus. Avoids creating a wrapper per command type.
        /// </summary>
        /// <param name="commandTypeName">Full type name (e.g., "PerAspera.Commands.CmdFactionResourceAllocation")</param>
        /// <param name="constructorArgs">Constructor arguments for the command.</param>
        public bool DispatchCommand(string commandTypeName, params object[] constructorArgs)
        {
            try
            {
                var scriptsAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .FirstOrDefault(a => a.GetName().Name == "ScriptsAssembly");
                var cmdType = scriptsAssembly?.GetType(commandTypeName);
                if (cmdType == null)
                {
                    WrapperLog.Error($"Command type not found: {commandTypeName}");
                    return false;
                }

                var constructor = cmdType.GetConstructor(constructorArgs.Select(a => a.GetType()).ToArray());
                if (constructor == null)
                {
                    WrapperLog.Error($"Constructor not found for command: {commandTypeName}");
                    return false;
                }

                var cmdInstance = constructor.Invoke(constructorArgs);
                var commandBus = UniverseWrapper.GetCurrent()?.GetCommandBus();
                if (commandBus == null)
                {
                    WrapperLog.Error("CommandBus not available");
                    return false;
                }

                var dispatchMethod = commandBus.GetType()
                    .GetMethod("Dispatch", BindingFlags.Instance | BindingFlags.Public);
                if (dispatchMethod?.IsGenericMethod == true)
                {
                    dispatchMethod.MakeGenericMethod(cmdType).Invoke(commandBus, new object[] { cmdInstance });
                    return true;
                }

                WrapperLog.Error("Generic Dispatch not available on CommandBus");
                return false;
            }
            catch (Exception ex)
            {
                WrapperLog.Error($"DispatchCommand {commandTypeName} failed: {ex.Message}");
                return false;
            }
        }

        public string GetCommandDebugInfo()
        {
            var handle = GetHandle();
            return $"Faction Command Debug:\n" +
                   $"  Name: {Name}\n" +
                   $"  Handle: {(handle != null ? "Available" : "Not Available")}\n" +
                   $"  IsPlayer: {IsPlayerFaction}  IsValid: {IsValidWrapper}";
        }

        // ==================== INTERNAL ====================

        private T? GetNativePropertySafe<T>(string propertyName)
        {
            string[] names = {
                $"_{propertyName}_k__BackingField",
                propertyName,
                $"_{propertyName}",
                $"m_{propertyName}"
            };
            foreach (var name in names)
            {
                try
                {
                    var v = GetNativeProperty<T>(name);
                    if (v != null) return v;
                }
                catch { }
            }
            return default;
        }

        public override string ToString()
            => $"Faction[{Name}] (Valid:{IsValidWrapper} Player:{IsPlayerFaction} AI:{IsAI})";
    }
}
