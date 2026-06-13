using System;
using System.Reflection;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

#pragma warning disable CS1591
namespace PerAspera.GameAPI.ResourceInit
{
    /// <summary>
    /// Auto-start plugin: fixes the native loader ordering bug that crashes custom
    /// Manufactured/Mined YAML resources used as building input/output.
    ///
    /// Root cause: YAMLLoader.CompleteLoading() calls BuildingType.PostInitialize()
    /// before ResourceType.PostInitialize() assigns indices to mod resources (index == -1).
    /// Building input/output structures are then built with invalid indices, and
    /// ResourceAllocation.RecreateIDs() (BaseGame.StartGameplay) throws IndexOutOfRangeException.
    ///
    /// Fix applied automatically when PerAspera.GameAPI.dll is deployed:
    ///  1. Block premature BuildingType.PostInitialize() (while _MaxIndex == 0).
    ///  2. After CompleteLoading(): run ResourceType then BuildingType PostInitialize.
    ///  3. Skip BuildPanelPresenter.FillDetails() for buildings with mod outputResource.
    ///  4. Finalizer on BuildPanelPresenter.Start() to swallow residual panel crashes.
    /// </summary>
    /// <example>
    /// // Aucune action requise par les mods : déployer PerAspera.GameAPI.dll suffit.
    /// </example>
    [BepInPlugin("PerAspera.GameAPI.ResourceInit", "PerAspera ResourceInit", "1.0.0")]
    public class ResourceInitAutoStart : BasePlugin
    {
        /// <summary>Highest vanilla+DLC resource index. Resources above this are mod-defined.</summary>
        internal const int VanillaMaxResourceIndex = 42;

        internal const string ModDevHelperGuid = "com.modperaspera.moddevhelper";

        public override void Load()
        {
            // ModDevHelper (debug tool) embeds the same fix + richer diagnostics.
            // Stay dormant to avoid double PostInitialize re-runs.
            if (IL2CPPChainloader.Instance.Plugins.ContainsKey(ModDevHelperGuid))
            {
                ResourceInitLog.Info("ModDevHelper détecté — fix délégué à ModDevHelper, SDK en veille.");
                return;
            }

            try
            {
                var harmony = new Harmony("PerAspera.GameAPI.ResourceInit");
                harmony.PatchAll(typeof(BuildingTypePostInitGate));
                harmony.PatchAll(typeof(CompleteLoadingFix));
                harmony.PatchAll(typeof(BuildPanelFillDetailsGuard));
                harmony.PatchAll(typeof(BuildPanelStartFinalizer));

                ResourceInitLog.Info("Actif — fix ordre PostInitialize + garde BuildPanel.");
            }
            catch (Exception ex)
            {
                ResourceInitLog.Error($"Initialisation échouée : {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Blocks premature BuildingType.PostInitialize() while ResourceType._MaxIndex == 0.
    /// CompleteLoadingFix replays it in the correct order.
    /// </summary>
    [HarmonyPatch(typeof(BuildingType), nameof(BuildingType.PostInitialize))]
    internal static class BuildingTypePostInitGate
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
            if (ResourceType._MaxIndex == 0)
            {
                ResourceInitLog.Warning("BuildingType.PostInitialize prématuré bloqué (MaxIndex=0) — re-run après assignation des index.");
                return false;
            }
            return true;
        }
    }

    /// <summary>
    /// Postfix on YAMLLoader.CompleteLoading (default priority — runs before
    /// YamlExtensionsAutoStart.Priority.Last): re-runs ResourceType then
    /// BuildingType PostInitialize so mod resources get valid indices.
    /// </summary>
    internal static class CompleteLoadingFix
    {
        static MethodBase? TargetMethod()
        {
            System.Type? loaderType = AccessTools.TypeByName("PerAspera.YAML.YAMLLoader");
            if (loaderType == null)
            {
                ResourceInitLog.Error("PerAspera.YAML.YAMLLoader introuvable — fix annulé.");
                return null;
            }
            MethodBase? method = AccessTools.Method(loaderType, "CompleteLoading");
            if (method == null)
                ResourceInitLog.Error("YAMLLoader.CompleteLoading introuvable — fix annulé.");
            return method;
        }

        [HarmonyPostfix]
        public static void Postfix()
        {
            try
            {
                int before = ResourceType._MaxIndex;
                ResourceType.PostInitialize();
                ResourceInitLog.Info($"ResourceType.PostInitialize re-run : MaxIndex {before}→{ResourceType._MaxIndex}.");
            }
            catch (Exception ex)
            {
                ResourceInitLog.Error($"ResourceType.PostInitialize re-run échoué : {ex.Message}");
                return;
            }

            try
            {
                BuildingType.PostInitialize();
                ResourceInitLog.Info("BuildingType.PostInitialize re-run OK.");
            }
            catch (Exception ex)
            {
                ResourceInitLog.Error($"BuildingType.PostInitialize re-run échoué : {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Skips BuildPanelPresenter.FillDetails() for buildings whose outputResource is a mod
    /// resource (index > vanilla max). The internal display array is vanilla-sized only.
    /// </summary>
    [HarmonyPatch(typeof(BuildPanelPresenter), "FillDetails",
        typeof(BuildButtonView), typeof(BuildingType))]
    internal static class BuildPanelFillDetailsGuard
    {
        [HarmonyPrefix]
        public static bool Prefix(BuildingType buildingType)
        {
            try
            {
                var outRes = buildingType?.outputResource;
                if (outRes != null && outRes.index > ResourceInitAutoStart.VanillaMaxResourceIndex)
                {
                    ResourceInitLog.Warning($"FillDetails ignoré pour '{buildingType!.key}' (output='{outRes.key}' idx={outRes.index} > vanilla max {ResourceInitAutoStart.VanillaMaxResourceIndex}).");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                ResourceInitLog.Error($"FillDetails guard : {ex.Message}");
                return true;
            }
        }
    }

    /// <summary>
    /// Finalizer on BuildPanelPresenter.Start() — swallows residual crashes so the
    /// build panel never blocks, logging the exception for diagnosis.
    /// </summary>
    internal static class BuildPanelStartFinalizer
    {
        static MethodBase? TargetMethod()
        {
            var method = AccessTools.Method(typeof(BuildPanelPresenter), "Start");
            if (method == null)
                ResourceInitLog.Error("BuildPanelPresenter.Start introuvable — finalizer annulé.");
            return method;
        }

        [HarmonyFinalizer]
        public static Exception? Finalizer(Exception? __exception)
        {
            if (__exception != null)
                ResourceInitLog.Error($"BuildPanel.Start crash avalé : {__exception.GetType().Name}: {__exception.Message}");
            return null;
        }
    }
}
