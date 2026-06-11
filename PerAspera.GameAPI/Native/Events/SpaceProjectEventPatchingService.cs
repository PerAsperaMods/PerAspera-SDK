using System;
using System.Collections.Generic;
using HarmonyLib;
using PerAspera.Core;
using PerAspera.GameAPI.Events.Native;
using EnhancedEventBus = PerAspera.GameAPI.Events.Integration.EnhancedEventBus;

namespace PerAspera.GameAPI.Native.Events
{
    /// <summary>
    /// Service de patching pour les événements liés aux projets spatiaux (SpacePort).
    /// <para>Hooks Harmony Postfix sur :</para>
    /// <list type="bullet">
    ///   <item><description><c>PortProject.SetStage(SpaceportStage)</c> → <see cref="PortProjectStageChangedNativeEvent"/></description></item>
    ///   <item><description><c>SpecialProject.Complete()</c> → <see cref="SpecialProjectCompletedNativeEvent"/></description></item>
    ///   <item><description><c>SpecialProject.DispatchLaunchActions()</c> → <see cref="SpecialProjectLaunchedNativeEvent"/></description></item>
    ///   <item><description><c>SpecialProject.RecoverGatheredResources(SpacePortComponent)</c> → <see cref="PortProjectCancelledNativeEvent"/></description></item>
    /// </list>
    /// <para>Membres vérifiés dans <c>Tools\InteropDump\ScriptsAssembly\PortProject.cs</c>
    /// et <c>SpecialProject.cs</c>. Complete() est privée native mais exposée publique
    /// par le publicizer BepInEx.</para>
    /// <para>Risque SetStage : peut être inliné par il2cpp. Fallback documenté :
    /// patcher <c>PortProject.OnTick</c> avec détection delta <c>stage</c>.</para>
    /// </summary>
    public sealed class SpaceProjectEventPatchingService : BaseEventPatchingService
    {
        /// <summary>Initialise le service de patching pour les projets spatiaux.</summary>
        /// <param name="harmony">Instance Harmony partagée.</param>
        public SpaceProjectEventPatchingService(Harmony harmony)
            : base("SpaceProject", harmony) { }

        /// <inheritdoc />
        public override string GetEventType() => "SpaceProject";

        /// <inheritdoc />
        public override int InitializeEventHooks()
        {
            _log.Debug("🚀 Setting up space project event hooks...");
            int count = 0;

            count += PatchPortProjectSetStage()  ? 1 : 0;
            count += PatchSpecialProjectComplete()         ? 1 : 0;
            count += PatchSpecialProjectDispatchLaunch()   ? 1 : 0;
            count += PatchSpecialProjectRecover()          ? 1 : 0;

            _log.Info($"✅ SpaceProject hooks: {count}/4 patched");
            return count;
        }

        // ─────────────────────────── SetStage ───────────────────────────

        private bool PatchPortProjectSetStage()
        {
            try
            {
                var method = AccessTools.Method(typeof(PortProject), "SetStage",
                    new[] { typeof(SpaceportStage) });

                if (method == null)
                {
                    _log.Warning("PortProject.SetStage(SpaceportStage) introuvable — méthode peut-être inlinée.");
                    return false;
                }

                var postfix = AccessTools.Method(typeof(SpaceProjectEventPatchingService), nameof(SetStage_Postfix));

                _harmony.Patch(method, postfix: new HarmonyMethod(postfix));
                _patchedMethods["PortProject.SetStage"] = PortProjectStageChangedNativeEvent.Key;
                _log.Debug("✓ PortProject.SetStage patché");
                return true;
            }
            catch (Exception ex)
            {
                _log.Warning($"Échec patch SetStage : {ex.Message}");
                return false;
            }
        }

        [HarmonyPostfix]
        private static void SetStage_Postfix(PortProject __instance, SpaceportStage stage)
        {
            try
            {
                var prev = __instance.previousStage;
                var projectKey = __instance.specialProject?.type?.name ?? string.Empty;

                EnhancedEventBus.Publish(PortProjectStageChangedNativeEvent.Key,
                    new PortProjectStageChangedNativeEvent
                    {
                        NativePortProject   = __instance,
                        ProjectKey          = projectKey,
                        PreviousStageKey    = prev?.key ?? string.Empty,
                        NewStageKey         = stage?.key ?? string.Empty,
                    });
            }
            catch (Exception ex)
            {
                new LogAspera("SpaceProject.SetStage_Postfix").Warning($"Erreur postfix : {ex.Message}");
            }
        }

        // ─────────────────────────── Complete ───────────────────────────

        private bool PatchSpecialProjectComplete()
        {
            try
            {
                var method = AccessTools.Method(typeof(SpecialProject), "Complete");
                if (method == null)
                {
                    _log.Warning("SpecialProject.Complete() introuvable (privée + publicizer requis).");
                    return false;
                }
                var postfix = AccessTools.Method(typeof(SpaceProjectEventPatchingService), nameof(Complete_Postfix));
                _harmony.Patch(method, postfix: new HarmonyMethod(postfix));
                _patchedMethods["SpecialProject.Complete"] = SpecialProjectCompletedNativeEvent.Key;
                _log.Debug("✓ SpecialProject.Complete patché");
                return true;
            }
            catch (Exception ex)
            {
                _log.Warning($"Échec patch Complete : {ex.Message}");
                return false;
            }
        }

        [HarmonyPostfix]
        private static void Complete_Postfix(SpecialProject __instance)
        {
            try
            {
                var projectKey = __instance.type?.name ?? string.Empty;
                var gathered   = BuildGatheredSnapshot(__instance);

                EnhancedEventBus.Publish(SpecialProjectCompletedNativeEvent.Key,
                    new SpecialProjectCompletedNativeEvent
                    {
                        NativeSpecialProject = __instance,
                        ProjectKey           = projectKey,
                        GatheredResources    = gathered,
                    });
            }
            catch (Exception ex)
            {
                new LogAspera("SpaceProject.Complete_Postfix").Warning($"Erreur postfix : {ex.Message}");
            }
        }

        // ─────────────────────────── DispatchLaunchActions ───────────────────────────

        private bool PatchSpecialProjectDispatchLaunch()
        {
            try
            {
                var method = AccessTools.Method(typeof(SpecialProject), "DispatchLaunchActions");
                if (method == null)
                {
                    _log.Warning("SpecialProject.DispatchLaunchActions() introuvable.");
                    return false;
                }
                var postfix = AccessTools.Method(typeof(SpaceProjectEventPatchingService), nameof(DispatchLaunchActions_Postfix));
                _harmony.Patch(method, postfix: new HarmonyMethod(postfix));
                _patchedMethods["SpecialProject.DispatchLaunchActions"] = SpecialProjectLaunchedNativeEvent.Key;
                _log.Debug("✓ SpecialProject.DispatchLaunchActions patché");
                return true;
            }
            catch (Exception ex)
            {
                _log.Warning($"Échec patch DispatchLaunchActions : {ex.Message}");
                return false;
            }
        }

        [HarmonyPostfix]
        private static void DispatchLaunchActions_Postfix(SpecialProject __instance)
        {
            try
            {
                EnhancedEventBus.Publish(SpecialProjectLaunchedNativeEvent.Key,
                    new SpecialProjectLaunchedNativeEvent
                    {
                        NativeSpecialProject = __instance,
                        ProjectKey           = __instance.type?.name ?? string.Empty,
                        Launches             = __instance.launches,
                    });
            }
            catch (Exception ex)
            {
                new LogAspera("SpaceProject.DispatchLaunch_Postfix").Warning($"Erreur postfix : {ex.Message}");
            }
        }

        // ─────────────────────────── RecoverGatheredResources ───────────────────────────

        private bool PatchSpecialProjectRecover()
        {
            try
            {
                var method = AccessTools.Method(typeof(SpecialProject), "RecoverGatheredResources");
                if (method == null)
                {
                    _log.Warning("SpecialProject.RecoverGatheredResources() introuvable.");
                    return false;
                }
                var prefix = AccessTools.Method(typeof(SpaceProjectEventPatchingService), nameof(RecoverGatheredResources_Prefix));
                // Prefix pour capturer le snapshot AVANT que les ressources soient restituées
                _harmony.Patch(method, prefix: new HarmonyMethod(prefix));
                _patchedMethods["SpecialProject.RecoverGatheredResources"] = PortProjectCancelledNativeEvent.Key;
                _log.Debug("✓ SpecialProject.RecoverGatheredResources patché");
                return true;
            }
            catch (Exception ex)
            {
                _log.Warning($"Échec patch RecoverGatheredResources : {ex.Message}");
                return false;
            }
        }

        [HarmonyPrefix]
        private static void RecoverGatheredResources_Prefix(SpecialProject __instance)
        {
            try
            {
                EnhancedEventBus.Publish(PortProjectCancelledNativeEvent.Key,
                    new PortProjectCancelledNativeEvent
                    {
                        NativeSpecialProject = __instance,
                        ProjectKey           = __instance.type?.name ?? string.Empty,
                    });
            }
            catch (Exception ex)
            {
                new LogAspera("SpaceProject.RecoverPrefix").Warning($"Erreur prefix : {ex.Message}");
            }
        }

        // ─────────────────────────── Helpers ───────────────────────────

        /// <summary>
        /// Construit un snapshot des ressources collectées en parcourant les PortProjects actifs.
        /// Utilise <c>PortProject.developmentResources</c> (<c>Dictionary&lt;ResourceType, CargoQuantity&gt;</c>).
        /// </summary>
        private static IReadOnlyDictionary<string, float> BuildGatheredSnapshot(SpecialProject project)
        {
            var result = new Dictionary<string, float>();
            try
            {
                var ports = project.activePorts;
                if (ports == null) return result;

                foreach (var port in ports)
                {
                    var portProject = port?.portProject;
                    if (portProject == null) continue;

                    var devResources = portProject.developmentResources;
                    if (devResources == null) continue;

                    foreach (var kv in devResources)
                    {
                        var resourceName = kv.Key?.name;
                        if (string.IsNullOrEmpty(resourceName)) continue;
                        var qty = kv.Value.ToFloat();
                        if (result.ContainsKey(resourceName))
                            result[resourceName] += qty;
                        else
                            result[resourceName]  = qty;
                    }
                }
            }
            catch (Exception ex)
            {
                new LogAspera("SpaceProject.BuildSnapshot").Warning($"Snapshot partiel : {ex.Message}");
            }
            return result;
        }
    }
}
