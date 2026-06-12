using System;
using System.Collections.Generic;
using HarmonyLib;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;

namespace PerAspera.GameAPI.MultiOutput
{
    /// <summary>
    /// UI patch: shows the secondary outputs in the OUTPUT section of the in-world
    /// building card (<c>BuildingWorldPanel</c>).
    /// Injection is done at the <b>panel</b> level (its own <c>outputResource</c> list +
    /// a clone of <c>outputItemPrototype</c> in <c>outputContainer</c>) — never via
    /// <c>BuildingType.displayOutputs</c>, which duplicates the native output icon
    /// (tested in game, Wafhien 2026-06).
    /// The quantity label then refreshes automatically: the vanilla update loop
    /// (<c>UpdateInputOutput → UpdateDetails</c>) walks <c>outputResource</c> /
    /// <c>outputDetails</c> in lockstep and reads the stockpile via <c>GetStock</c> —
    /// exactly where <see cref="FactorySpawnOutputPatch"/> injects the cargo.
    /// </summary>
    [HarmonyPatch(typeof(BuildingWorldPanel), nameof(BuildingWorldPanel.SetBuilding))]
    internal static class BuildingWorldPanelMultiOutputPatch
    {
        /// <summary>Name tag of the ResourceDetail clones we create (used for cleanup).</summary>
        private const string CloneName = "SDKMultiOutputDetail";

        // 1 log Info par type de bâtiment (éviter le spam à chaque ouverture de panneau)
        private static readonly HashSet<string> _logged = new();

        /// <summary>
        /// Removes our clones from a previous SetBuilding pass before the vanilla
        /// rebuild — idempotent whatever the vanilla cleanup actually does.
        /// </summary>
        [HarmonyPrefix]
        public static void Prefix(BuildingWorldPanel __instance)
        {
            try
            {
                var container = __instance?.outputContainer;
                if (container == null) return;

                var root = container.transform;
                for (int i = root.childCount - 1; i >= 0; i--)
                {
                    var child = root.GetChild(i);
                    if (child == null || child.name != CloneName) continue;

                    var rect = child.TryCast<RectTransform>();
                    if (rect != null) container.RemoveChild(rect);
                    UnityEngine.Object.Destroy(child.gameObject);
                }
            }
            catch (Exception ex)
            {
                MultiOutputLog.Error($"Panel Prefix (cleanup) : {ex.Message}");
            }
        }

        /// <summary>
        /// After the vanilla panel setup: appends one ResourceDetail per secondary
        /// output to the OUTPUT container, and extends the panel's bookkeeping lists
        /// so the vanilla update loop refreshes our items too.
        /// </summary>
        [HarmonyPostfix]
        public static void Postfix(BuildingWorldPanel __instance, Building building)
        {
            try
            {
                string? key = building?.buildingType?.key;
                if (key == null) return;

                var extras = MultiOutput.GetResolvedOrNull(key);
                if (extras == null) return;

                var list = __instance.outputResource;
                // Count == 0 : panneau en mode construction/sans production — ne rien ajouter.
                if (list == null || list.Count == 0) return;

                var container = __instance.outputContainer;
                var proto = __instance.outputItemPrototype;
                if (container == null || proto == null) return;

                var added = new List<ResourceDetail>();
                foreach (var extra in extras)
                {
                    if (ContainsResource(list, extra.Resource)) continue;

                    var clone = UnityEngine.Object.Instantiate(
                        proto, container.transform, false);
                    clone.name = CloneName;
                    clone.gameObject.SetActive(true);

                    var sprite = __instance.GetOutputSprite(extra.Resource);
                    if (clone.imageIcon != null && sprite != null)
                        clone.imageIcon.sprite = sprite;
                    clone.SetResourceTooltip(extra.Resource);

                    var rect = clone.GetComponent<RectTransform>();
                    if (rect != null) container.AddChild(rect, -1, reparent: false);

                    list.Add(extra.Resource);
                    added.Add(clone);
                }
                if (added.Count == 0) return;

                // Étendre outputDetails pour que UpdateDetails rafraîchisse nos items.
                var old = __instance.outputDetails;
                int oldLen = old != null ? old.Length : 0;
                var arr = new Il2CppReferenceArray<ResourceDetail>(oldLen + added.Count);
                for (int i = 0; i < oldLen; i++) arr[i] = old![i];
                for (int j = 0; j < added.Count; j++) arr[oldLen + j] = added[j];
                __instance.outputDetails = arr;

                container.SetDirty();

                if (_logged.Add(key))
                    MultiOutputLog.Info($"Panneau OUTPUT : +{added.Count} sortie(s) secondaire(s) affichée(s) pour {key}.");
            }
            catch (Exception ex)
            {
                MultiOutputLog.Error($"Panel Postfix : {ex.Message}");
            }
        }

        private static bool ContainsResource(
            Il2CppSystem.Collections.Generic.List<ResourceType> list, ResourceType resource)
        {
            for (int i = 0; i < list.Count; i++)
            {
                var r = list[i];
                if (r != null && r.key == resource.key) return true;
            }
            return false;
        }
    }
}
