using System;
using System.Reflection;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

#pragma warning disable CS1591
namespace PerAspera.GameAPI.YamlExtensions
{
    /// <summary>
    /// Auto-start plugin: hooks <c>YAMLLoader.CompleteLoading</c> so the extension
    /// sidecar files (<c>sdk.yaml</c>) are loaded right after the game's datamodel is
    /// fully merged. Same auto-start pattern as <c>EventsAutoStartPlugin</c>.
    /// </summary>
    /// <example>
    /// // Aucune action requise par les mods : déployer PerAspera.GameAPI.dll suffit.
    /// // Les consommateurs s'abonnent à YamlExtensions.DataLoaded.
    /// </example>
    [BepInPlugin("PerAspera.GameAPI.YamlExtensions", "PerAspera YamlExtensions", "1.0.0")]
    public class YamlExtensionsAutoStart : BasePlugin
    {
        public override void Load()
        {
            try
            {
                var harmony = new Harmony("PerAspera.GameAPI.YamlExtensions");
                harmony.PatchAll(typeof(CompleteLoadingHook));
                Log.LogInfo("[YamlExt] Hook CompleteLoading posé — sdk.yaml chargés post-datamodel.");
            }
            catch (Exception ex)
            {
                Log.LogError($"[YamlExt] Initialisation échouée : {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Postfix on <c>YAMLLoader.CompleteLoading</c> — triggers the sdk.yaml load pass
    /// once the native tables are populated (so consumers can resolve string refs).
    /// </summary>
    internal static class CompleteLoadingHook
    {
        static MethodBase? TargetMethod()
        {
            System.Type? loaderType = AccessTools.TypeByName("PerAspera.YAML.YAMLLoader");
            if (loaderType == null)
            {
                YamlExtensionsLog.Error("PerAspera.YAML.YAMLLoader introuvable — sdk.yaml non chargés.");
                return null;
            }
            MethodBase? method = AccessTools.Method(loaderType, "CompleteLoading");
            if (method == null)
                YamlExtensionsLog.Error("YAMLLoader.CompleteLoading introuvable — sdk.yaml non chargés.");
            return method;
        }

        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)] // après ResourceInitAutoStart (default priority) qui re-run PostInitialize
        public static void Postfix()
        {
            try { YamlExtensionLoader.LoadAll(); }
            catch (Exception ex) { YamlExtensionsLog.Error($"LoadAll : {ex.Message}"); }
        }
    }
}
