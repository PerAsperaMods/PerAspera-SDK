using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using PerAspera.Core;
using PerAspera.Core.IL2CPP;

namespace PerAspera.GameAPI.Native.Events
{
    /// <summary>
    /// Resource event patching service for Per Aspera
    /// Handles all resource-related events including production, consumption, transfer, and storage changes
    /// </summary>
    public sealed class ResourceEventPatchingService : BaseEventPatchingService
    {
        private System.Type _resourceManagerType;
        private System.Type _buildingType;
        private System.Type _planetType;

        /// <summary>
        /// Initialize resource event patching service
        /// </summary>
        /// <param name="harmony">Harmony instance for IL2CPP patching</param>
        public ResourceEventPatchingService(Harmony harmony) 
            : base("Resource", harmony)
        {
        }

        /// <summary>
        /// Get the event type identifier for this service
        /// </summary>
        /// <returns>Event type string</returns>
        public override string GetEventType() => "Resource";

        /// <summary>
        /// Initialize all resource-related event hooks
        /// </summary>
        /// <returns>Number of successfully hooked methods</returns>
        public override int InitializeEventHooks()
        {
            _log.Debug("🔋 Setting up enhanced resource event hooks...");

            _resourceManagerType = GameTypeInitializer.GetResourceManagerType();
            _buildingType = GameTypeInitializer.GetBuildingType();
            _planetType = GameTypeInitializer.GetPlanetType();

            if (_resourceManagerType == null && _buildingType == null && _planetType == null)
            {
                _log.Warning("Resource-related types not found, skipping resource hooks");
                return 0;
            }

            // Enhanced resource methods with comprehensive coverage
            var resourceHooks = new Dictionary<string, (System.Type type, string eventType)>();

            // Resource Manager hooks
            if (_resourceManagerType != null)
            {
                AddResourceManagerHooks(resourceHooks);
            }

            // Building resource hooks
            if (_buildingType != null)
            {
                AddBuildingResourceHooks(resourceHooks);
            }

            // Planet resource hooks
            if (_planetType != null)
            {
                AddPlanetResourceHooks(resourceHooks);
            }

            int hookedCount = 0;
            foreach (var (methodName, (type, eventType)) in resourceHooks)
            {
                if (CreateResourceMethodHook(type, methodName, eventType))
                {
                    hookedCount++;
                }
            }

            _log.Info($"✅ Resource hooks initialized: {hookedCount}/{resourceHooks.Count} methods hooked");
            return hookedCount;
        }

        /// <summary>
        /// Add ResourceManager-specific hooks
        /// </summary>
        /// <param name="hooks">Hook dictionary to populate</param>
        private void AddResourceManagerHooks(Dictionary<string, (System.Type type, string eventType)> hooks)
        {
            var managerHooks = new Dictionary<string, string>
            {
                { "AddResource", "ResourceAdd" },
                { "RemoveResource", "ResourceRemove" },
                { "ConsumeResource", "ResourceConsume" },
                { "ProduceResource", "ResourceProduce" },
                { "TransferResource", "ResourceTransfer" },
                { "UpdateResourceStock", "ResourceStockUpdate" },
                { "SetResourceAmount", "ResourceAmountSet" },
                { "ChangeResourceAmount", "ResourceAmountChange" }
            };

            foreach (var (method, eventType) in managerHooks)
            {
                hooks[method] = (_resourceManagerType, eventType);
            }
        }

        /// <summary>
        /// Add Building-specific resource hooks
        /// </summary>
        /// <param name="hooks">Hook dictionary to populate</param>
        private void AddBuildingResourceHooks(Dictionary<string, (System.Type type, string eventType)> hooks)
        {
            var buildingHooks = new Dictionary<string, string>
            {
                { "ProduceEnergy", "EnergyProduce" },
                { "ConsumeEnergy", "EnergyConsume" },
                { "SetEnergyProduction", "EnergyProductionSet" },
                { "SetEnergyConsumption", "EnergyConsumptionSet" },
                { "UpdateProduction", "BuildingProductionUpdate" },
                { "UpdateConsumption", "BuildingConsumptionUpdate" },
                { "ProcessResources", "BuildingResourceProcess" },
                { "TransferToStorage", "BuildingStorageTransfer" },
                { "ExtractResource", "ResourceExtract" },
                { "DepositResource", "ResourceDeposit" }
            };

            foreach (var (method, eventType) in buildingHooks)
            {
                hooks[method] = (_buildingType, eventType);
            }
        }

        /// <summary>
        /// Add Planet-specific resource hooks
        /// </summary>
        /// <param name="hooks">Hook dictionary to populate</param>
        private void AddPlanetResourceHooks(Dictionary<string, (System.Type type, string eventType)> hooks)
        {
            var planetHooks = new Dictionary<string, string>
            {
                { "UpdateGlobalResources", "GlobalResourceUpdate" },
                { "UpdateResourcePools", "ResourcePoolUpdate" },
                { "ProcessResourceChains", "ResourceChainProcess" },
                { "BalanceResources", "ResourceBalance" },
                { "SyncResources", "ResourceSync" },
                { "RecalculateResources", "ResourceRecalculate" }
            };

            foreach (var (method, eventType) in planetHooks)
            {
                hooks[method] = (_planetType, eventType);
            }
        }

        /// <summary>
        /// Create a resource-specific method hook with prefix and postfix handling
        /// </summary>
        /// <param name="targetType">Type containing the method</param>
        /// <param name="methodName">Method name to hook</param>
        /// <param name="eventType">Type of resource event</param>
        /// <returns>True if hook was successfully created</returns>
        private bool CreateResourceMethodHook(System.Type targetType, string methodName, string eventType)
        {
            if (!ValidateMethodForPatching(targetType, methodName))
            {
                return false;
            }

            try
            {
                var method = targetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                
                // Create harmony patches with event type context
                var prefix = new HarmonyMethod(typeof(ResourceEventPatchingService), nameof(ResourcePrefix));
                var postfix = new HarmonyMethod(typeof(ResourceEventPatchingService), nameof(ResourcePostfix));

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
        /// Enhanced Harmony prefix for resource methods
        /// Captures the old resource state before method execution
        /// </summary>
        [HarmonyPrefix]
        public static void ResourcePrefix(object __instance, Dictionary<string, object> __state, MethodBase __originalMethod)
        {
            try
            {
                if (__state == null)
                    __state = new Dictionary<string, object>();

                var methodName = __originalMethod.Name;
                var resourceType = ExtractResourceTypeFromMethodName(methodName);
                
                // Capture current resource state before change
                var currentResourceState = GetCurrentResourceState(__instance, methodName);
                __state["OldState"] = currentResourceState;
                __state["ResourceType"] = resourceType;
                __state["MethodName"] = methodName;
                __state["Instance"] = __instance;
                __state["Timestamp"] = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                // Fail silently to avoid disrupting game flow
            }
        }

        /// <summary>
        /// Enhanced Harmony postfix for resource methods
        /// Publishes resource change events with before/after states
        /// </summary>
        [HarmonyPostfix]
        public static void ResourcePostfix(object __instance, Dictionary<string, object> __state, 
            MethodBase __originalMethod, object[] __args)
        {
            try
            {
                if (__state == null || !__state.ContainsKey("OldState"))
                    return;

                var resourceType = (string)__state["ResourceType"];
                var methodName = (string)__state["MethodName"];
                var oldState = __state["OldState"];
                var timestamp = (DateTime)__state["Timestamp"];

                // Capture new resource state after change
                var newState = GetCurrentResourceState(__instance, methodName);

                // Extract resource-specific information from arguments
                var resourceInfo = ExtractResourceInformation(__args, methodName);

                var eventData = new
                {
                    Instance = __instance,
                    ResourceType = resourceType,
                    OldState = oldState,
                    NewState = newState,
                    MethodName = methodName,
                    ResourceInfo = resourceInfo,
                    Arguments = ExtractMethodArguments(__args),
                    Timestamp = timestamp,
                    Duration = DateTime.UtcNow - timestamp
                };

                // Publish specific resource event
                ModEventBus.Publish($"Resource{resourceType}", eventData);
                
                // Publish generic resource event
                ModEventBus.Publish("ResourceChanged", eventData);

                // Special handling for critical resource events
                PublishSpecialResourceEvents(resourceType, eventData);
            }
            catch (Exception ex)
            {
                // Fail silently to avoid disrupting game flow
            }
        }

        /// <summary>
        /// Get current resource state with comprehensive information
        /// </summary>
        /// <param name="instance">Game instance</param>
        /// <param name="methodName">Method being called</param>
        /// <returns>Current resource state object</returns>
        private static object GetCurrentResourceState(object instance, string methodName)
        {
            try
            {
                var resourceState = new Dictionary<string, object>();

                // Try to extract energy information for buildings
                if (methodName.Contains("Energy"))
                {
                    var energyInfo = ExtractEnergyInformation(instance);
                    if (energyInfo != null)
                    {
                        resourceState["Energy"] = energyInfo;
                    }
                }

                // Try to extract general resource information
                var generalResources = ExtractGeneralResourceInformation(instance);
                if (generalResources != null)
                {
                    resourceState["Resources"] = generalResources;
                }

                // Try to extract storage information
                var storageInfo = ExtractStorageInformation(instance);
                if (storageInfo != null)
                {
                    resourceState["Storage"] = storageInfo;
                }

                return resourceState.Count > 0 ? resourceState : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Extract energy information from instance
        /// </summary>
        /// <param name="instance">Game instance</param>
        /// <returns>Energy information or null</returns>
        private static object ExtractEnergyInformation(object instance)
        {
            try
            {
                var instanceType = instance.GetType();
                var energyInfo = new Dictionary<string, object>();

                // Try common energy property names
                var energyProperties = new[]
                {
                    "energyProduction", "EnergyProduction", "energy", "Energy",
                    "energyConsumption", "EnergyConsumption",
                    "energyOutput", "EnergyOutput", "energyInput", "EnergyInput"
                };
                
                foreach (var propName in energyProperties)
                {
                    var value = GetPropertyValue(instance, propName);
                    if (value != null)
                    {
                        energyInfo[propName] = value;
                    }
                }

                return energyInfo.Count > 0 ? energyInfo : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Extract general resource information from instance
        /// </summary>
        /// <param name="instance">Game instance</param>
        /// <returns>Resource information or null</returns>
        private static object ExtractGeneralResourceInformation(object instance)
        {
            try
            {
                var instanceType = instance.GetType();
                var resourceInfo = new Dictionary<string, object>();

                // Try common resource property names
                var resourceProperties = new[]
                {
                    "resources", "Resources", "resourceStock", "ResourceStock",
                    "production", "Production", "consumption", "Consumption",
                    "inventory", "Inventory", "storage", "Storage"
                };
                
                foreach (var propName in resourceProperties)
                {
                    var value = GetPropertyValue(instance, propName);
                    if (value != null)
                    {
                        resourceInfo[propName] = value;
                    }
                }

                return resourceInfo.Count > 0 ? resourceInfo : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Extract storage information from instance
        /// </summary>
        /// <param name="instance">Game instance</param>
        /// <returns>Storage information or null</returns>
        private static object ExtractStorageInformation(object instance)
        {
            try
            {
                var storageInfo = new Dictionary<string, object>();

                // Try common storage property names
                var storageProperties = new[]
                {
                    "storageCapacity", "StorageCapacity", "maxStorage", "MaxStorage",
                    "currentStorage", "CurrentStorage", "storageUsed", "StorageUsed",
                    "capacity", "Capacity", "maxCapacity", "MaxCapacity"
                };
                
                foreach (var propName in storageProperties)
                {
                    var value = GetPropertyValue(instance, propName);
                    if (value != null)
                    {
                        storageInfo[propName] = value;
                    }
                }

                return storageInfo.Count > 0 ? storageInfo : null;
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
        /// Extract resource type from method name
        /// </summary>
        /// <param name="methodName">Method name</param>
        /// <returns>Resource type identifier</returns>
        private static string ExtractResourceTypeFromMethodName(string methodName)
        {
            return methodName switch
            {
                var name when name.Contains("Energy") => "Energy",
                var name when name.Contains("Add") => "Add",
                var name when name.Contains("Remove") => "Remove",
                var name when name.Contains("Consume") => "Consume",
                var name when name.Contains("Produce") => "Produce",
                var name when name.Contains("Transfer") => "Transfer",
                var name when name.Contains("Extract") => "Extract",
                var name when name.Contains("Deposit") => "Deposit",
                var name when name.Contains("Update") => "Update",
                var name when name.Contains("Set") => "Set",
                var name when name.Contains("Change") => "Change",
                _ => "Generic"
            };
        }

        /// <summary>
        /// Extract resource-specific information from method arguments
        /// </summary>
        /// <param name="args">Method arguments</param>
        /// <param name="methodName">Method name for context</param>
        /// <returns>Resource information object</returns>
        private static object ExtractResourceInformation(object[] args, string methodName)
        {
            try
            {
                if (args == null || args.Length == 0)
                    return null;

                var resourceInfo = new Dictionary<string, object>();

                // For resource methods, first argument is often resource type or amount
                if (args.Length > 0)
                {
                    resourceInfo["PrimaryValue"] = args[0];
                }

                // Second argument might be amount or target
                if (args.Length > 1)
                {
                    resourceInfo["SecondaryValue"] = args[1];
                }

                // Look for specific patterns based on method name
                if (methodName.Contains("Transfer") && args.Length >= 2)
                {
                    resourceInfo["Source"] = args[0];
                    resourceInfo["Target"] = args[1];
                }

                return resourceInfo;
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
        /// Publish special resource events for critical game state changes
        /// </summary>
        /// <param name="resourceType">Type of resource event</param>
        /// <param name="eventData">Event data</param>
        private static void PublishSpecialResourceEvents(string resourceType, object eventData)
        {
            try
            {
                switch (resourceType)
                {
                    case "Energy":
                        ModEventBus.Publish("EnergySystemChanged", eventData);
                        break;

                    case "Produce":
                        ModEventBus.Publish("ResourceProduced", eventData);
                        break;

                    case "Consume":
                        ModEventBus.Publish("ResourceConsumed", eventData);
                        break;

                    case "Transfer":
                        ModEventBus.Publish("ResourceTransferred", eventData);
                        break;
                }
            }
            catch (Exception)
            {
                // Fail silently
            }
        }

        /// <summary>
        /// Get diagnostic information about resource event hooks
        /// </summary>
        /// <returns>Diagnostic information string</returns>
        public string GetDiagnosticInfo()
        {
            var info = new System.Text.StringBuilder();
            info.AppendLine("=== Resource Event Patching Service ===");
            info.AppendLine($"ResourceManager Type: {GetFriendlyTypeName(_resourceManagerType)}");
            info.AppendLine($"Building Type: {GetFriendlyTypeName(_buildingType)}");
            info.AppendLine($"Planet Type: {GetFriendlyTypeName(_planetType)}");
            info.AppendLine($"Hooked Methods: {_patchedMethods.Count}");
            info.AppendLine();

            var categoryGroups = new Dictionary<string, List<string>>();
            foreach (var patch in _patchedMethods)
            {
                var category = patch.Value.Contains("Energy") ? "Energy" : 
                              patch.Value.Contains("Building") ? "Building" : 
                              patch.Value.Contains("Global") ? "Global" : "General";
                
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