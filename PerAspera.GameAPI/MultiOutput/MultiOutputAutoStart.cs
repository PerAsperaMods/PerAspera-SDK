using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

#pragma warning disable CS1591
namespace PerAspera.GameAPI.MultiOutput
{
    /// <summary>
    /// Auto-start plugin for the MultiOutput system: registers the <c>multiOutput</c>
    /// extension schema before the datamodel loads, and applies the
    /// <c>Factory.SpawnOutput</c> Postfix that injects the extra outputs.
    /// </summary>
    /// <example>
    /// // Aucune action requise par les mods : déployer PerAspera.GameAPI.dll suffit.
    /// // Déclaration via sdk.yaml (section multiOutput) ou MultiOutput.RegisterExtraOutput().
    /// </example>
    [BepInPlugin("PerAspera.GameAPI.MultiOutput", "PerAspera MultiOutput", "1.0.0")]
    public class MultiOutputAutoStart : BasePlugin
    {
        public override void Load()
        {
            try
            {
                MultiOutput.Initialize();

                var harmony = new Harmony("PerAspera.GameAPI.MultiOutput");
                harmony.PatchAll(typeof(FactorySpawnOutputPatch));
                harmony.PatchAll(typeof(BuildingWorldPanelMultiOutputPatch));

                Log.LogInfo("[MultiOut] Actif — section 'multiOutput' enregistrée, patchs SpawnOutput + BuildingWorldPanel posés.");
            }
            catch (Exception ex)
            {
                Log.LogError($"[MultiOut] Initialisation échouée : {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Postfix on <c>Factory.SpawnOutput()</c> — runs only when a native production cycle
    /// actually completed (inputs consumed, productivity > 0), and injects the declared
    /// extra outputs as idle cargo in the factory's stockpile
    /// (<c>ICargoHolderOps_XAddIdleCargo</c>, badge null accepté — pas de [NotNull]).
    /// </summary>
    [HarmonyPatch(typeof(Factory), nameof(Factory.SpawnOutput))]
    internal static class FactorySpawnOutputPatch
    {
        [HarmonyPostfix]
        public static void Postfix(Factory __instance)
        {
            try
            {
                var buildingType = __instance?.buildingType;
                if (buildingType == null) return;

                string key = buildingType.key;
                var outputs = MultiOutput.GetResolvedOrNull(key);
                if (outputs == null) return;

                var stockpile = __instance!.stockpile;
                if (stockpile == null)
                {
                    MultiOutputLog.Warning($"{key} : stockpile null — sorties secondaires sautées.");
                    return;
                }

                float productivity = __instance.Productivity;

                foreach (var output in outputs)
                {
                    float qty = output.ScaleWithProductivity ? output.Quantity * productivity
                                                             : output.Quantity;
                    if (qty <= 0f) continue;

                    stockpile.ICargoHolderOps_XAddIdleCargo(
                        output.Resource, CargoQuantity.FromUnitFloat(qty), null);

                    MultiOutput.RaiseProduced(new SecondaryOutputProducedArgs
                    {
                        BuildingKey = key,
                        ResourceKey = output.Resource.key,
                        Quantity = qty
                    });
                }
            }
            catch (Exception ex)
            {
                MultiOutputLog.Error($"SpawnOutput Postfix : {ex.Message}");
            }
        }
    }
}
