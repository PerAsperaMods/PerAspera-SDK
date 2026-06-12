using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace PerAspera.GameAPI.MultiOutput
{
    /// <summary>
    /// Shared clone pool for the secondary-output UI patches.
    ///
    /// Design rules (learned the hard way, 2026-06-12):
    /// <list type="bullet">
    /// <item><b>Never Destroy a clone</b> — Destroy is deferred in Unity and the vanilla
    /// update loop may still hold the reference within the same frame → hard crash.
    /// Clones are pooled and toggled with SetActive instead.</item>
    /// <item><b>Never mutate the panel's own bookkeeping</b> (<c>outputResource</c> /
    /// <c>outputDetails</c>) — vanilla rebuilds them on its own schedule and the lists
    /// desynchronise from our clones. We own our clones end-to-end and refresh their
    /// quantity label ourselves in an <c>UpdateInputOutput</c> Postfix.</item>
    /// <item><b>No .NET reflection</b> — every member used here is exposed by the typed
    /// interop proxies (verified in Tools/InteropDump/ScriptsAssembly/, 2026-06-12).</item>
    /// </list>
    /// </summary>
    internal static class MultiOutputPanelPool
    {
        /// <summary>Name tag of the ResourceDetail clones we create (debug/identification).</summary>
        internal const string CloneName = "SDKMultiOutputDetail";

        internal sealed class Entry
        {
            public ResourceDetail Detail = null!;
            public ResourceType Resource = null!;
        }

        // Keyed by the native pointer of the panel instance (stable: Boehm GC is non-moving,
        // and the panels are persistent presenters — at most a couple of instances per session).
        private static readonly Dictionary<IntPtr, List<Entry>> _pools = new();

        /// <summary>
        /// Brings the clone set of <paramref name="panelPtr"/> in sync with
        /// <paramref name="extras"/>: reuses pooled clones, instantiates missing ones,
        /// deactivates the surplus. Pass <c>extras = null</c> to just hide everything.
        /// Returns the number of active clones.
        /// </summary>
        internal static int Sync(
            IntPtr panelPtr,
            LayoutContainer? container,
            ResourceDetail? prototype,
            IReadOnlyList<ResolvedExtraOutput>? extras,
            Func<ResourceType, Sprite?> spriteSelector)
        {
            if (!_pools.TryGetValue(panelPtr, out var pool))
            {
                pool = new List<Entry>();
                _pools[panelPtr] = pool;
            }

            // Drop entries whose GameObject got destroyed behind our back (scene unload…)
            pool.RemoveAll(e => !IsAlive(e.Detail));

            if (extras == null || extras.Count == 0 || container == null || prototype == null)
            {
                foreach (var e in pool) Hide(e.Detail, container);
                return 0;
            }

            int active = 0;
            for (int i = 0; i < extras.Count; i++)
            {
                var extra = extras[i];
                Entry entry;
                if (i < pool.Count)
                {
                    entry = pool[i];
                }
                else
                {
                    var clone = UnityEngine.Object.Instantiate(prototype, container.transform, false);
                    clone.name = CloneName;
                    entry = new Entry { Detail = clone };
                    pool.Add(entry);
                }

                entry.Resource = extra.Resource;

                var detail = entry.Detail;
                var sprite = spriteSelector(extra.Resource);
                if (detail.imageIcon != null && sprite != null)
                    detail.imageIcon.sprite = sprite;
                detail.SetResourceTooltip(extra.Resource);

                if (!detail.gameObject.activeSelf)
                {
                    detail.gameObject.SetActive(true);
                    var rect = detail.GetComponent<RectTransform>();
                    if (rect != null) container.AddChild(rect, -1, reparent: false);
                }
                active++;
            }

            // Surplus clones from a previous building with more extras
            for (int i = extras.Count; i < pool.Count; i++)
                Hide(pool[i].Detail, container);

            container.SetDirty();
            return active;
        }

        /// <summary>Active clones of a panel (empty list when none).</summary>
        internal static List<Entry> GetEntries(IntPtr panelPtr)
            => _pools.TryGetValue(panelPtr, out var pool) ? pool : _empty;

        private static readonly List<Entry> _empty = new();

        private static void Hide(ResourceDetail detail, LayoutContainer? container)
        {
            if (!IsAlive(detail) || !detail.gameObject.activeSelf) return;
            var rect = detail.GetComponent<RectTransform>();
            if (rect != null) container?.RemoveChild(rect);
            detail.gameObject.SetActive(false);
        }

        private static bool IsAlive(ResourceDetail? detail)
        {
            try
            {
                // Unity's overloaded == reports destroyed natives as null through the proxy
                return detail != null && detail.gameObject != null;
            }
            catch { return false; }
        }
    }

    /// <summary>
    /// In-world building card (<c>BuildingWorldPanel</c>, the hover/click card):
    /// adds one pooled ResourceDetail per secondary output to the OUTPUT container.
    /// </summary>
    [HarmonyPatch(typeof(BuildingWorldPanel), nameof(BuildingWorldPanel.SetBuilding))]
    internal static class BuildingWorldPanelMultiOutputPatch
    {
        private static readonly HashSet<string> _logged = new();

        [HarmonyPostfix]
        public static void Postfix(BuildingWorldPanel __instance, Building building)
        {
            try
            {
                string? key = building?.buildingType?.key;
                var extras = key != null ? MultiOutput.GetResolvedOrNull(key) : null;

                // Construction mode: vanilla hides the OUTPUT section — hide our clones too.
                var list = __instance.outputResource;
                if (list == null || list.Count == 0) extras = null;

                int shown = MultiOutputPanelPool.Sync(
                    __instance.Pointer,
                    __instance.outputContainer,
                    __instance.outputItemPrototype,
                    extras,
                    r => __instance.GetOutputSprite(r));

                if (shown > 0)
                {
                    RefreshTexts(__instance);   // avoid one frame of prefab placeholder text
                    if (key != null && _logged.Add(key))
                        MultiOutputLog.Info($"Panneau OUTPUT (carte) : +{shown} sortie(s) secondaire(s) pour {key}.");
                }
            }
            catch (Exception ex)
            {
                MultiOutputLog.Error($"WorldPanel SetBuilding Postfix : {ex.Message}");
            }
        }

        /// <summary>Quantity refresh — same formatter as vanilla (<c>GetOutputString</c>).</summary>
        internal static void RefreshTexts(BuildingWorldPanel panel)
        {
            foreach (var entry in MultiOutputPanelPool.GetEntries(panel.Pointer))
            {
                var detail = entry.Detail;
                if (detail == null || !detail.gameObject.activeSelf || detail.textQuantity == null)
                    continue;
                detail.textQuantity.text = panel.GetOutputString(
                    panel.GetStock(entry.Resource),
                    panel.GetMaxStockOutput(entry.Resource));
            }
        }
    }

    /// <summary>
    /// Keeps the quantity label of the world-card clones in sync with the stockpile,
    /// on the same cadence as the vanilla refresh.
    /// </summary>
    [HarmonyPatch(typeof(BuildingWorldPanel), nameof(BuildingWorldPanel.UpdateInputOutput))]
    internal static class BuildingWorldPanelMultiOutputUpdatePatch
    {
        [HarmonyPostfix]
        public static void Postfix(BuildingWorldPanel __instance)
        {
            try { BuildingWorldPanelMultiOutputPatch.RefreshTexts(__instance); }
            catch (Exception ex) { MultiOutputLog.Error($"WorldPanel UpdateInputOutput Postfix : {ex.Message}"); }
        }
    }

    /// <summary>
    /// Left-side building screen (<c>BuildingScreenPanel</c>): same clone pooling on its
    /// own OUTPUT container. No <c>GetOutputSprite</c>/<c>GetStock</c> here — the sprite
    /// comes from <c>ResourceType.iconName</c> and the stock from
    /// <c>Stockpile.GetTotalStock</c> (not GetOwnStockForShow — idle cargo added via
    /// ICargoHolderOps_XAddIdleCargo has no registered slot, so GetOwnStockForShow returns 0).
    /// </summary>
    [HarmonyPatch(typeof(BuildingScreenPanel), nameof(BuildingScreenPanel.SetBuilding))]
    internal static class BuildingScreenPanelMultiOutputPatch
    {
        private static readonly HashSet<string> _logged = new();

        [HarmonyPostfix]
        public static void Postfix(BuildingScreenPanel __instance, Building building)
        {
            try
            {
                string? key = building?.buildingType?.key;
                var extras = key != null ? MultiOutput.GetResolvedOrNull(key) : null;

                int shown = MultiOutputPanelPool.Sync(
                    __instance.Pointer,
                    __instance.outputContainer,
                    __instance.outputItemPrototype,
                    extras,
                    r => r.iconName);

                if (shown > 0)
                {
                    RefreshTexts(__instance);
                    if (key != null && _logged.Add(key))
                        MultiOutputLog.Info($"Panneau OUTPUT (écran gauche) : +{shown} sortie(s) secondaire(s) pour {key}.");
                }
            }
            catch (Exception ex)
            {
                MultiOutputLog.Error($"ScreenPanel SetBuilding Postfix : {ex.Message}");
            }
        }

        internal static void RefreshTexts(BuildingScreenPanel panel)
        {
            var stockpile = panel.building?.stockpile;
            if (stockpile == null) return;

            foreach (var entry in MultiOutputPanelPool.GetEntries(panel.Pointer))
            {
                var detail = entry.Detail;
                if (detail == null || !detail.gameObject.activeSelf || detail.textQuantity == null)
                    continue;
                float stock = stockpile.GetTotalStock(entry.Resource).ToFloat();
                detail.textQuantity.text = Mathf.FloorToInt(stock).ToString();
            }
        }
    }

    /// <summary>Quantity refresh for the left-side screen, vanilla cadence.</summary>
    [HarmonyPatch(typeof(BuildingScreenPanel), nameof(BuildingScreenPanel.UpdateInputOutput))]
    internal static class BuildingScreenPanelMultiOutputUpdatePatch
    {
        [HarmonyPostfix]
        public static void Postfix(BuildingScreenPanel __instance)
        {
            try { BuildingScreenPanelMultiOutputPatch.RefreshTexts(__instance); }
            catch (Exception ex) { MultiOutputLog.Error($"ScreenPanel UpdateInputOutput Postfix : {ex.Message}"); }
        }
    }
}
