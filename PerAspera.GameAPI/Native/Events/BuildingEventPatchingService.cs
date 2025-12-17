using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Native.Events
{
    /// <summary>
    /// Building event patching service for Per Aspera
    /// Handles all building-related events including construction, destruction, upgrades, and operational state changes
    /// </summary>
    public sealed class BuildingEventPatchingService : BaseEventPatchingService
    {
        private System.Type _buildingType;
        private System.Type _buildingManagerType;
        private System.Type _constructionType;
        private System.Type _planetType;

        /// <summary>
        /// Initialize building event patching service
        /// </summary>
        /// <param name="harmony">Harmony instance for IL2CPP patching</param>
        public BuildingEventPatchingService(Harmony harmony) 
            : base("Building", harmony)
        {
        }

        /// <summary>
        /// Get the event type identifier for this service
        /// </summary>
        /// <returns>Event type string</returns>
        public override string GetEventType() => "Building";

        /// <summary>
        /// Initialize all building-related event hooks
        /// </summary>
        /// <returns>Number of successfully hooked methods</returns>
        public override int InitializeEventHooks()
        {
            _log.Debug("🏗️ Setting up enhanced building event hooks...");

            _buildingType = GameTypeInitializer.GetBuildingType();
            _buildingManagerType = GameTypeInitializer.GetBuildingManagerType();
            _constructionType = GameTypeInitializer.GetConstructionType();
            _planetType = GameTypeInitializer.GetPlanetType();

            if (_buildingType == null && _buildingManagerType == null)
            {
                _log.Warning("Building-related types not found, skipping building hooks");
                return 0;
            }

            // Enhanced building methods with comprehensive coverage
            var buildingHooks = new Dictionary<string, (System.Type type, string eventType)>();

            // Building instance hooks
            if (_buildingType != null)
            {
                AddBuildingInstanceHooks(buildingHooks);
            }

            // Building manager hooks
            if (_buildingManagerType != null)
            {
                AddBuildingManagerHooks(buildingHooks);
            }

            // Construction hooks
            if (_constructionType != null)
            {
                AddConstructionHooks(buildingHooks);
            }

            // Planet building hooks
            if (_planetType != null)
            {
                AddPlanetBuildingHooks(buildingHooks);
            }

            int hookedCount = 0;
            foreach (var (methodName, (type, eventType)) in buildingHooks)
            {
                if (CreateBuildingMethodHook(type, methodName, eventType))
                {
                    hookedCount++;
                }
            }

            _log.Info($"✅ Building hooks initialized: {hookedCount}/{buildingHooks.Count} methods hooked");
            return hookedCount;
        }

        /// <summary>
        /// Add Building instance-specific hooks
        /// </summary>
        /// <param name="hooks">Hook dictionary to populate</param>
        private void AddBuildingInstanceHooks(Dictionary<string, (System.Type type, string eventType)> hooks)
        {
            var buildingHooks = new Dictionary<string, string>
            {
                { "Construct", "BuildingConstruct" },
                { "Build", "BuildingBuild" },
                { "Destroy", "BuildingDestroy" },
                { "Demolish", "BuildingDemolish" },
                { "Remove", "BuildingRemove" },
                { "Upgrade", "BuildingUpgrade" },
                { "Downgrade", "BuildingDowngrade" },
                { "Repair", "BuildingRepair" },
                { "Damage", "BuildingDamage" },
                { "Activate", "BuildingActivate" },
                { "Deactivate", "BuildingDeactivate" },
                { "Enable", "BuildingEnable" },
                { "Disable", "BuildingDisable" },
                { "StartProduction", "ProductionStart" },
                { "StopProduction", "ProductionStop" },
                { "PauseProduction", "ProductionPause" },
                { "ResumeProduction", "ProductionResume" },
                { "SetProduction", "ProductionSet" },
                { "UpdateProduction", "ProductionUpdate" },
                { "ProcessProduction", "ProductionProcess" },
                { "CompleteConstruction", "ConstructionComplete" },
                { "StartConstruction", "ConstructionStart" },
                { "CancelConstruction", "ConstructionCancel" }
            };

            foreach (var (method, eventType) in buildingHooks)
            {
                hooks[method] = (_buildingType, eventType);
            }
        }

        /// <summary>
        /// Add BuildingManager-specific hooks
        /// </summary>
        /// <param name="hooks">Hook dictionary to populate</param>
        private void AddBuildingManagerHooks(Dictionary<string, (System.Type type, string eventType)> hooks)
        {
            var managerHooks = new Dictionary<string, string>
            {
                { "CreateBuilding", "BuildingCreate" },
                { "PlaceBuilding", "BuildingPlace" },
                { "RemoveBuilding", "BuildingRemove" },
                { "DestroyBuilding", "BuildingDestroy" },
                { "AddBuilding", "BuildingAdd" },
                { "RegisterBuilding", "BuildingRegister" },
                { "UnregisterBuilding", "BuildingUnregister" },
                { "ValidatePlacement", "PlacementValidate" },
                { "CanPlaceBuilding", "PlacementCheck" },
                { "UpdateBuildings", "BuildingsUpdate" },
                { "ProcessBuildings", "BuildingsProcess" },
                { "RefreshBuildings", "BuildingsRefresh" }
            };

            foreach (var (method, eventType) in managerHooks)
            {
                hooks[method] = (_buildingManagerType, eventType);
            }
        }

        /// <summary>
        /// Add Construction-specific hooks
        /// </summary>
        /// <param name="hooks">Hook dictionary to populate</param>
        private void AddConstructionHooks(Dictionary<string, (System.Type type, string eventType)> hooks)
        {
            var constructionHooks = new Dictionary<string, string>
            {
                { "StartConstruction", "ConstructionStart" },
                { "CompleteConstruction", "ConstructionComplete" },
                { "CancelConstruction", "ConstructionCancel" },
                { "PauseConstruction", "ConstructionPause" },
                { "ResumeConstruction", "ConstructionResume" },
                { "UpdateProgress", "ConstructionProgress" },
                { "AddMaterials", "ConstructionMaterials" },
                { "ConsumeResources", "ConstructionResources" },
                { "ValidateResources", "ConstructionValidation" }
            };

            foreach (var (method, eventType) in constructionHooks)
            {
                hooks[method] = (_constructionType, eventType);
            }
        }

        /// <summary>
        /// Add Planet building-specific hooks
        /// </summary>
        /// <param name="hooks">Hook dictionary to populate</param>
        private void AddPlanetBuildingHooks(Dictionary<string, (System.Type type, string eventType)> hooks)
        {
            var planetHooks = new Dictionary<string, string>
            {
                { "AddBuilding", "PlanetBuildingAdd" },
                { "RemoveBuilding", "PlanetBuildingRemove" },
                { "UpdateBuildings", "PlanetBuildingsUpdate" },
                { "RecalculateBuildings", "PlanetBuildingsRecalculate" },
                { "ValidateBuildings", "PlanetBuildingsValidate" },
                { "OptimizeBuildings", "PlanetBuildingsOptimize" }
            };

            foreach (var (method, eventType) in planetHooks)
            {
                hooks[method] = (_planetType, eventType);
            }
        }

        /// <summary>
        /// Create a building-specific method hook with prefix and postfix handling
        /// </summary>
        /// <param name="targetType">Type containing the method</param>
        /// <param name="methodName">Method name to hook</param>
        /// <param name="eventType">Type of building event</param>
        /// <returns>True if hook was successfully created</returns>
        private bool CreateBuildingMethodHook(System.Type targetType, string methodName, string eventType)
        {
            if (!ValidateMethodForPatching(targetType, methodName))
            {
                return false;
            }

            try
            {
                var method = targetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                
                // Create harmony patches with event type context
                var prefix = new HarmonyMethod(typeof(BuildingEventPatchingService), nameof(BuildingPrefix));
                var postfix = new HarmonyMethod(typeof(BuildingEventPatchingService), nameof(BuildingPostfix));

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
        /// Enhanced Harmony prefix for building methods
        /// Captures the old building state before method execution
        /// </summary>
        [HarmonyPrefix]
        public static void BuildingPrefix(object __instance, Dictionary<string, object> __state, MethodBase __originalMethod)
        {
            try
            {
                if (__state == null)
                    __state = new Dictionary<string, object>();

                var methodName = __originalMethod.Name;
                var buildingType = ExtractBuildingTypeFromMethodName(methodName);
                
                // Capture current building state before change
                var currentBuildingState = GetCurrentBuildingState(__instance, methodName);
                __state["OldState"] = currentBuildingState;
                __state["BuildingType"] = buildingType;
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
        /// Enhanced Harmony postfix for building methods
        /// Publishes building change events with before/after states
        /// </summary>
        [HarmonyPostfix]
        public static void BuildingPostfix(object __instance, Dictionary<string, object> __state, 
            MethodBase __originalMethod, object[] __args)
        {
            try
            {
                if (__state == null || !__state.ContainsKey("OldState"))
                    return;

                var buildingType = (string)__state["BuildingType"];
                var methodName = (string)__state["MethodName"];
                var oldState = __state["OldState"];
                var timestamp = (DateTime)__state["Timestamp"];

                // Capture new building state after change
                var newState = GetCurrentBuildingState(__instance, methodName);

                // Extract building-specific information from arguments
                var buildingInfo = ExtractBuildingInformation(__args, methodName);

                var eventData = new
                {
                    Instance = __instance,
                    BuildingType = buildingType,
                    OldState = oldState,
                    NewState = newState,
                    MethodName = methodName,
                    BuildingInfo = buildingInfo,
                    Arguments = ExtractMethodArguments(__args),
                    Timestamp = timestamp,
                    Duration = DateTime.UtcNow - timestamp,
                    InstanceType = __instance.GetType().Name
                };

                // Publish specific building event
                ModEventBus.Publish($"Building{buildingType}", eventData);
                
                // Publish generic building event
                ModEventBus.Publish("BuildingChanged", eventData);

                // Special handling for critical building events
                PublishSpecialBuildingEvents(buildingType, eventData);
            }
            catch (Exception)
            {
                // Fail silently to avoid disrupting game flow
            }
        }

        /// <summary>
        /// Get current building state with comprehensive information
        /// </summary>
        /// <param name="instance">Building instance</param>
        /// <param name="methodName">Method being called</param>
        /// <returns>Current building state object</returns>
        private static object GetCurrentBuildingState(object instance, string methodName)
        {
            try
            {
                var buildingState = new Dictionary<string, object>();

                // Try to extract basic building information
                var basicInfo = ExtractBasicBuildingInfo(instance);
                if (basicInfo != null)
                {
                    buildingState["Basic"] = basicInfo;
                }

                // Try to extract production information
                if (methodName.Contains("Production"))
                {
                    var productionInfo = ExtractProductionInfo(instance);
                    if (productionInfo != null)
                    {
                        buildingState["Production"] = productionInfo;
                    }
                }

                // Try to extract construction information
                if (methodName.Contains("Construction") || methodName.Contains("Build"))
                {
                    var constructionInfo = ExtractConstructionInfo(instance);
                    if (constructionInfo != null)
                    {
                        buildingState["Construction"] = constructionInfo;
                    }
                }

                // Try to extract operational state
                var operationalInfo = ExtractOperationalInfo(instance);
                if (operationalInfo != null)
                {
                    buildingState["Operational"] = operationalInfo;
                }

                return buildingState.Count > 0 ? buildingState : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Extract basic building information
        /// </summary>
        /// <param name="instance">Building instance</param>
        /// <returns>Basic building info or null</returns>
        private static object ExtractBasicBuildingInfo(object instance)
        {
            try
            {
                var basicInfo = new Dictionary<string, object>();

                var basicProperties = new[]
                {
                    "buildingType", "BuildingType", "type", "Type",
                    "id", "Id", "ID", "buildingId", "BuildingId",
                    "name", "Name", "displayName", "DisplayName",
                    "position", "Position", "location", "Location",
                    "level", "Level", "tier", "Tier",
                    "health", "Health", "condition", "Condition"
                };
                
                foreach (var propName in basicProperties)
                {
                    var value = GetPropertyValue(instance, propName);
                    if (value != null)
                    {
                        basicInfo[propName] = value;
                    }
                }

                return basicInfo.Count > 0 ? basicInfo : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Extract production information
        /// </summary>
        /// <param name="instance">Building instance</param>
        /// <returns>Production info or null</returns>
        private static object ExtractProductionInfo(object instance)
        {
            try
            {
                var productionInfo = new Dictionary<string, object>();

                var productionProperties = new[]
                {
                    "production", "Production", "productionRate", "ProductionRate",
                    "efficiency", "Efficiency", "productivity", "Productivity",
                    "isProducing", "IsProducing", "producing", "Producing",
                    "output", "Output", "input", "Input",
                    "energyProduction", "EnergyProduction", "energyConsumption", "EnergyConsumption"
                };
                
                foreach (var propName in productionProperties)
                {
                    var value = GetPropertyValue(instance, propName);
                    if (value != null)
                    {
                        productionInfo[propName] = value;
                    }
                }

                return productionInfo.Count > 0 ? productionInfo : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Extract construction information
        /// </summary>
        /// <param name="instance">Building instance</param>
        /// <returns>Construction info or null</returns>
        private static object ExtractConstructionInfo(object instance)
        {
            try
            {
                var constructionInfo = new Dictionary<string, object>();

                var constructionProperties = new[]
                {
                    "isUnderConstruction", "IsUnderConstruction", "constructing", "Constructing",
                    "constructionProgress", "ConstructionProgress", "progress", "Progress",
                    "constructionTime", "ConstructionTime", "buildTime", "BuildTime",
                    "requiredResources", "RequiredResources", "materials", "Materials",
                    "constructionCost", "ConstructionCost", "cost", "Cost"
                };
                
                foreach (var propName in constructionProperties)
                {
                    var value = GetPropertyValue(instance, propName);
                    if (value != null)
                    {
                        constructionInfo[propName] = value;
                    }
                }

                return constructionInfo.Count > 0 ? constructionInfo : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Extract operational information
        /// </summary>
        /// <param name="instance">Building instance</param>
        /// <returns>Operational info or null</returns>
        private static object ExtractOperationalInfo(object instance)
        {
            try
            {
                var operationalInfo = new Dictionary<string, object>();

                var operationalProperties = new[]
                {
                    "isActive", "IsActive", "active", "Active",
                    "isEnabled", "IsEnabled", "enabled", "Enabled",
                    "isOperational", "IsOperational", "operational", "Operational",
                    "isPowered", "IsPowered", "powered", "Powered",
                    "isConnected", "IsConnected", "connected", "Connected",
                    "status", "Status", "state", "State"
                };
                
                foreach (var propName in operationalProperties)
                {
                    var value = GetPropertyValue(instance, propName);
                    if (value != null)
                    {
                        operationalInfo[propName] = value;
                    }
                }

                return operationalInfo.Count > 0 ? operationalInfo : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Helper method to get property value safely
        /// </summary>
        /// <param name="instance">Object instance</param>
        /// <param name="propertyName">Property name</param>
        /// <returns>Property value or null</returns>
        private static object GetPropertyValue(object instance, string propertyName)
        {
            try
            {
                var instanceType = instance.GetType();

                var property = instanceType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (property != null && property.CanRead)
                {
                    return property.GetValue(instance);
                }

                var field = instanceType.GetField(propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    return field.GetValue(instance);
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Extract building type from method name
        /// </summary>
        /// <param name="methodName">Method name</param>
        /// <returns>Building type identifier</returns>
        private static string ExtractBuildingTypeFromMethodName(string methodName)
        {
            return methodName switch
            {
                var name when name.Contains("Construct") || name.Contains("Build") => "Construction",
                var name when name.Contains("Destroy") || name.Contains("Demolish") || name.Contains("Remove") => "Destruction",
                var name when name.Contains("Upgrade") => "Upgrade",
                var name when name.Contains("Downgrade") => "Downgrade",
                var name when name.Contains("Repair") => "Repair",
                var name when name.Contains("Damage") => "Damage",
                var name when name.Contains("Activate") || name.Contains("Enable") => "Activate",
                var name when name.Contains("Deactivate") || name.Contains("Disable") => "Deactivate",
                var name when name.Contains("Production") => "Production",
                var name when name.Contains("Place") => "Placement",
                var name when name.Contains("Register") => "Registration",
                var name when name.Contains("Update") => "Update",
                var name when name.Contains("Process") => "Process",
                var name when name.Contains("Validate") => "Validation",
                _ => "Generic"
            };
        }

        /// <summary>
        /// Extract building-specific information from method arguments
        /// </summary>
        /// <param name="args">Method arguments</param>
        /// <param name="methodName">Method name for context</param>
        /// <returns>Building information object</returns>
        private static object ExtractBuildingInformation(object[] args, string methodName)
        {
            try
            {
                if (args == null || args.Length == 0)
                    return null;

                var buildingInfo = new Dictionary<string, object>();

                // For building methods, arguments often contain building types, positions, or configurations
                if (args.Length > 0)
                {
                    buildingInfo["PrimaryArgument"] = args[0];
                }

                if (args.Length > 1)
                {
                    buildingInfo["SecondaryArgument"] = args[1];
                }

                // Look for specific patterns based on method name
                if (methodName.Contains("Place") && args.Length >= 2)
                {
                    buildingInfo["BuildingType"] = args[0];
                    buildingInfo["Position"] = args[1];
                }

                return buildingInfo;
            }
            catch (Exception)
            {
                return null;
            }
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

            if (args.Length == 1)
                return args[0];

            var argDict = new Dictionary<string, object>();
            for (int i = 0; i < args.Length; i++)
            {
                argDict[$"Arg{i}"] = args[i];
            }

            return argDict;
        }

        /// <summary>
        /// Publish special building events for critical building state changes
        /// </summary>
        /// <param name="buildingType">Type of building event</param>
        /// <param name="eventData">Event data</param>
        private static void PublishSpecialBuildingEvents(string buildingType, object eventData)
        {
            try
            {
                switch (buildingType)
                {
                    case "Construction":
                        ModEventBus.Publish("BuildingConstructed", eventData);
                        break;

                    case "Destruction":
                        ModEventBus.Publish("BuildingDestroyed", eventData);
                        break;

                    case "Production":
                        ModEventBus.Publish("BuildingProductionChanged", eventData);
                        break;

                    case "Placement":
                        ModEventBus.Publish("BuildingPlaced", eventData);
                        break;

                    case "Upgrade":
                        ModEventBus.Publish("BuildingUpgraded", eventData);
                        break;

                    case "Activate":
                        ModEventBus.Publish("BuildingActivated", eventData);
                        break;

                    case "Deactivate":
                        ModEventBus.Publish("BuildingDeactivated", eventData);
                        break;
                }
            }
            catch (Exception)
            {
                // Fail silently
            }
        }

        /// <summary>
        /// Get diagnostic information about building event hooks
        /// </summary>
        /// <returns>Diagnostic information string</returns>
        public string GetDiagnosticInfo()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine("=== Building Event Patching Service ===");
            info.AppendLine($"Building Type: {GetFriendlyTypeName(_buildingType)}");
            info.AppendLine($"BuildingManager Type: {GetFriendlyTypeName(_buildingManagerType)}");
            info.AppendLine($"Construction Type: {GetFriendlyTypeName(_constructionType)}");
            info.AppendLine($"Planet Type: {GetFriendlyTypeName(_planetType)}");
            info.AppendLine($"Hooked Methods: {_patchedMethods.Count}");
            info.AppendLine();

            var categoryGroups = new Dictionary<string, List<string>>();
            foreach (var patch in _patchedMethods)
            {
                var category = patch.Value.Contains("Construction") ? "Construction" :
                              patch.Value.Contains("Production") ? "Production" :
                              patch.Value.Contains("Planet") ? "Planet" :
                              patch.Value.Contains("Building") ? "Building" : "General";
                
                if (!categoryGroups.ContainsKey(category))
                    categoryGroups[category] = new List<string>();
                
                categoryGroups[category].Add($"{patch.Key} → {patch.Value}");
            }

            foreach (var group in categoryGroups)
            {
                info.AppendLine($"  {group.Key}:");
                foreach (var item in group.Value)
                {
                    info.AppendLine($"    ✓ {item}");
                }
                info.AppendLine();
            }

            return info.ToString();
        }
    }
}

