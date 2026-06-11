using System;
using PerAspera.Core;
using PerAspera.GameAPI.Economy.Wrappers;
using PerAspera.GameAPI.Events.SDK;


#pragma warning disable CS1591
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
    ///
    /// MIGRATION 2026-06-10 — réécrit en accès typé via PortProjectWrapper.
    /// Élimine ~48 RS0030 (GetProperty/GetValue sur Faction/SpacePortComponent/PortProject/ResourceType/CargoQuantity).
    /// Source de vérité : Tools\InteropDump\ScriptsAssembly\Faction.cs (spacePorts),
    /// SpacePortComponent.cs (portProject), PortProject.cs (stage/done/developmentResources).
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
                // Faction.spacePorts : List<SpacePortComponent> — typé, confirmé dump ligne 13142
                var spacePorts = faction!.spacePorts;

                if (spacePorts == null || spacePorts.Count == 0)
                {
                    _log.Info("[SpaceportDebug] No active spaceports found.");
                    return true;
                }

                int portIndex = 0;
                foreach (var port in spacePorts)
                {
                    if (port == null) continue;

                    // SpacePortComponent.portProject : PortProject — typé, confirmé dump ligne 162
                    var portProject = port.portProject;
                    if (portProject == null)
                    {
                        _log.Info($"[SpaceportDebug] SpacePort[{portIndex}]: no portProject");
                        portIndex++;
                        continue;
                    }

                    // PortProjectWrapper : accès typé à stageKey, resourcesMet, developmentResources
                    var wrapper = new PortProjectWrapper(portProject);
                    _log.Info($"[SpaceportDebug] SpacePort[{portIndex}] project='{wrapper.ProjectKey}'" +
                              $" stage='{wrapper.StageKey}' resourcesMet={wrapper.ResourcesMet}" +
                              $" done={wrapper.Done}");

                    var devRes = wrapper.GetDevelopmentResources();
                    if (devRes.Count == 0)
                    {
                        _log.Info("  developmentResources=(vide)");
                    }
                    else
                    {
                        foreach (var (resKey, qty) in devRes)
                            _log.Info($"  {resKey}: {qty:F1}");
                    }

                    portIndex++;
                }

                return true;
            }
            catch (Exception ex)
            {
                _log.Warning($"[SpaceportDebug] Exception: {ex.Message}");
                return false;
            }
        }
    }
}
#pragma warning restore CS1591
