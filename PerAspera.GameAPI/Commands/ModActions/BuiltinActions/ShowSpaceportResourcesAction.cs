using System;
using System.Collections;
using System.Reflection;
using PerAspera.Core;
using PerAspera.GameAPI.Events.SDK;

namespace PerAspera.GameAPI.Commands.ModActions.BuiltinActions
{
    /// <summary>
    /// Debug command: logs all spaceport resource states to BepInEx log.
    /// Shows developmentResources (gathered so far) and resourcesMet flag for each active spaceport.
    ///
    /// YAML usage:
    /// <code>
    /// - command: ShowSpaceportResources
    ///   arguments: []
    /// </code>
    /// </summary>
    public class ShowSpaceportResourcesAction : IModTextAction
    {
        private static readonly LogAspera _log = new LogAspera("SpaceportDebug");

        public string CommandName => "ShowSpaceportResources";

        public bool Execute(string[] args, GameCommandsReadyEvent? ctx)
        {
            if (!ActionContextHelper.TryGetFaction(ctx, out var faction, _log, CommandName))
                return false;

            try
            {
                var factionType = faction!.GetType();

                // faction.spacePorts → List<SpacePortComponent>
                // In IL2CPP proxies (BepInEx 6), native fields are exposed as C# properties.
                var spacePortsProp = factionType.GetProperty("spacePorts",
                    BindingFlags.Public | BindingFlags.Instance);
                var spacePorts = spacePortsProp?.GetValue(faction);

                if (spacePorts == null)
                {
                    _log.Warning("[SpaceportDebug] 'spacePorts' property not found or null on Faction");
                    return false;
                }

                var enumerable = spacePorts as IEnumerable;
                if (enumerable == null)
                {
                    _log.Warning($"[SpaceportDebug] spacePorts is not IEnumerable: {spacePorts.GetType().Name}");
                    return false;
                }

                int portIndex = 0;
                foreach (var spRaw in enumerable)
                {
                    if (spRaw == null) continue;
                    var spType = spRaw.GetType();

                    // portProject → PortProject
                    var portProjectProp = spType.GetProperty("portProject",
                        BindingFlags.Public | BindingFlags.Instance);
                    var portProject = portProjectProp?.GetValue(spRaw);

                    if (portProject == null)
                    {
                        _log.Info($"[SpaceportDebug] SpacePort[{portIndex}]: no portProject");
                        portIndex++;
                        continue;
                    }

                    var ppType = portProject.GetType();

                    // specialProject.key for the project name
                    var spProp = ppType.GetProperty("specialProject", BindingFlags.Public | BindingFlags.Instance);
                    var specialProject = spProp?.GetValue(portProject);
                    string projectName = GetStringField(specialProject, "key") ?? "?";

                    // resourcesMet
                    var resMetProp = ppType.GetProperty("resourcesMet", BindingFlags.Public | BindingFlags.Instance);
                    bool resourcesMet = (bool)(resMetProp?.GetValue(portProject) ?? false);

                    _log.Info($"[SpaceportDebug] SpacePort[{portIndex}] project='{projectName}' resourcesMet={resourcesMet}");

                    // developmentResources → Dictionary<ResourceType, CargoQuantity>
                    var devResProp = ppType.GetProperty("developmentResources",
                        BindingFlags.Public | BindingFlags.Instance);
                    var devResources = devResProp?.GetValue(portProject);

                    if (devResources == null)
                    {
                        _log.Info("  developmentResources=null");
                    }
                    else
                    {
                        LogDictionary(devResources);
                    }

                    portIndex++;
                }

                if (portIndex == 0)
                    _log.Info("[SpaceportDebug] No active spaceports found.");

                return true;
            }
            catch (Exception ex)
            {
                _log.Warning($"[SpaceportDebug] Exception: {ex.Message}");
                return false;
            }
        }

        private static void LogDictionary(object dict)
        {
            try
            {
                // Try IDictionary first
                if (dict is IDictionary idict)
                {
                    foreach (DictionaryEntry entry in idict)
                        _log.Info($"  {GetResourceName(entry.Key)}: {GetCargoFloat(entry.Value):F1}");
                    return;
                }

                // IL2CPP Dictionary — iterate via reflection
                var dictType = dict.GetType();
                var keysProperty = dictType.GetProperty("Keys", BindingFlags.Public | BindingFlags.Instance);
                var valuesProperty = dictType.GetProperty("Values", BindingFlags.Public | BindingFlags.Instance);
                var keys = keysProperty?.GetValue(dict) as IEnumerable;
                var values = valuesProperty?.GetValue(dict) as IEnumerable;

                if (keys == null || values == null)
                {
                    _log.Info($"  [cannot iterate dictionary of type {dictType.Name}]");
                    return;
                }

                var keyList = new System.Collections.Generic.List<object>();
                foreach (var k in keys) keyList.Add(k);
                var valueList = new System.Collections.Generic.List<object>();
                foreach (var v in values) valueList.Add(v);

                for (int i = 0; i < Math.Min(keyList.Count, valueList.Count); i++)
                    _log.Info($"  {GetResourceName(keyList[i])}: {GetCargoFloat(valueList[i]):F1}");
            }
            catch (Exception ex)
            {
                _log.Warning($"  [LogDictionary error: {ex.Message}]");
            }
        }

        private static string GetResourceName(object resourceType)
        {
            if (resourceType == null) return "null";
            try
            {
                var t = resourceType.GetType();
                var nameProp = t.GetProperty("name", BindingFlags.Public | BindingFlags.Instance)
                            ?? t.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance)
                            ?? t.GetProperty("key", BindingFlags.Public | BindingFlags.Instance);
                return nameProp?.GetValue(resourceType)?.ToString() ?? resourceType.GetType().Name;
            }
            catch { return "?"; }
        }

        private static float GetCargoFloat(object cargoQty)
        {
            if (cargoQty == null) return 0f;
            try
            {
                var toFloat = cargoQty.GetType().GetMethod("ToFloat",
                    BindingFlags.Public | BindingFlags.Instance);
                return (float)(toFloat?.Invoke(cargoQty, null) ?? 0f);
            }
            catch { return 0f; }
        }

        private static string? GetStringField(object? obj, string fieldName)
        {
            if (obj == null) return null;
            try
            {
                var t = obj.GetType();
                return (t.GetProperty(fieldName, BindingFlags.Public | BindingFlags.Instance)
                          ?.GetValue(obj)
                     ?? t.GetField(fieldName, BindingFlags.Public | BindingFlags.Instance)
                          ?.GetValue(obj))?.ToString();
            }
            catch { return null; }
        }
    }
}
