using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;
using PerAspera.Core;
using PerAspera.GameAPI.UI.Toolkit;
using UnityEngine;
using UnityEngine.UI;

namespace PerAspera.GameAPI.UI.ResourceBar
{
    /// <summary>
    /// Public configuration + game-fix entry point for the HUD resource bar overflow.
    /// The bar overflows once mods add many custom resources; this paginates the two native
    /// containers (mined / manufactured) using the generic <see cref="UIPager"/> toolkit — native
    /// rendering untouched, just windowed into pages.
    ///
    /// <para><b>Adaptable:</b> set <see cref="ResourceFilter"/> to restrict which resources appear
    /// (the future sub-category filter driven by a custom YAML attribute on <c>resourceType</c>
    /// plugs in here). Set <see cref="PageSize"/> / <see cref="Enabled"/> before the game scene loads.</para>
    /// </summary>
    public static class ResourceBarFix
    {
        /// <summary>Master switch (default on — it is a strict overflow fix).</summary>
        public static bool Enabled = true;

        /// <summary>Items shown per page in each bar.</summary>
        public static int PageSize = 6;

        /// <summary>
        /// Optional resource filter. Return false to hide a resource from the bar entirely.
        /// Hook point for the upcoming YAML sub-category filtering. Null = show all.
        /// </summary>
        public static Func<ResourceType, bool>? ResourceFilter;

        /// <summary>Bridges <see cref="ResourceFilter"/> to the generic <see cref="UIPager"/> (Transform-based).</summary>
        internal static bool TransformFilter(Transform child)
        {
            if (ResourceFilter == null) return true;
            try
            {
                var item = child.GetComponent<ResourceItem>();
                return item == null || item.resourceType == null || ResourceFilter(item.resourceType);
            }
            catch { return true; }
        }
    }

    /// <summary>
    /// Auto-start plugin (loads with the SDK assembly): registers <see cref="UIPagerView"/> and
    /// applies the <c>ResourcesPanel.Initialize</c> Postfix that attaches a pager to each bar.
    /// </summary>
    [BepInPlugin("PerAspera.GameAPI.ResourceBar", "PerAspera ResourceBar Fix", "1.0.0")]
    public class ResourceBarAutoStart : BasePlugin
    {
        public override void Load()
        {
            try
            {
                if (!ClassInjector.IsTypeRegisteredInIl2Cpp<UIPagerView>())
                    ClassInjector.RegisterTypeInIl2Cpp<UIPagerView>();

                var harmony = new Harmony("PerAspera.GameAPI.ResourceBar");
                harmony.PatchAll(typeof(ResourceBarInitPatch));

                Log.LogInfo("[ResBar] Actif — pagination des barres mined/manufactured (overflow fix).");
            }
            catch (Exception ex)
            {
                Log.LogError($"[ResBar] Initialisation échouée : {ex.Message}");
            }
        }
    }

    /// <summary>Attaches a <see cref="UIPagerView"/> to each native resource container.</summary>
    [HarmonyPatch(typeof(ResourcesPanel), nameof(ResourcesPanel.Initialize))]
    internal static class ResourceBarInitPatch
    {
        private static readonly LogAspera _log = new LogAspera("ResBar.Init");

        [HarmonyPostfix]
        public static void Postfix(ResourcesPanel __instance)
        {
            if (!ResourceBarFix.Enabled) return;
            try
            {
                UISprites.Refresh();
                var prototype = __instance.resourceItemPrefab;
                // Mined bar (left) -> control on its LEFT edge ; manufactured bar (right) -> RIGHT edge.
                Attach("ResBarPager_Mined", __instance.containerPanelMined, leftEdge: true, prototype);
                Attach("ResBarPager_Manufactured", __instance.containerPanelManufactured, leftEdge: false, prototype);
            }
            catch (Exception ex)
            {
                _log.Warning($"attach failed: {ex.Message}");
            }
        }

        private static void Attach(string name, Transform? container, bool leftEdge, ResourceItem? prototype)
        {
            if (container == null) return;
            if (container.Find(name) != null) return; // already attached

            // Graft the control INTO the bar container as an ignore-layout child: uGUI keeps it glued
            // to the chosen edge automatically (responsive), and UIPager skips ignore-layout children
            // so it is never paginated.
            var go = new GameObject(name);
            var rt = go.AddComponent<RectTransform>();
            rt.SetParent(container, false);

            var le = go.AddComponent<LayoutElement>();
            le.ignoreLayout = true;

            var view = go.AddComponent<UIPagerView>();
            view.Target = container;
            view.Pager = new UIPager { PageSize = ResourceBarFix.PageSize };
            view.Filter = ResourceBarFix.TransformFilter;
            // Always keep the bar PageSize-wide: pad short pages with blank slots.
            view.PadToPageSize = true;
            var proto = prototype;
            view.PlaceholderFactory = (parent, i) => CreateBlankSlot(proto, parent);
            if (leftEdge)
            {
                view.EdgeAnchor = new Vector2(0f, 0.5f);   // left edge of the bar (the "-1" slot)
                view.EdgePivot = new Vector2(1f, 0.5f);    // control's right edge meets the bar
                view.EdgeOffset = new Vector2(16f, 0f);    // small overlap so it reads as an inline slot
            }
            else
            {
                view.EdgeAnchor = new Vector2(1f, 0.5f);   // right edge of the bar (the "7th" slot)
                view.EdgePivot = new Vector2(0f, 0.5f);    // control's left edge meets the bar
                view.EdgeOffset = new Vector2(-16f, 0f);   // small overlap so it reads as an inline slot
            }

            _log.Info($"pager grafted into {container.name} ({(leftEdge ? "left" : "right")} edge, children={container.childCount})");
        }

        // Blank slot = a clone of the resource-item prefab with its script disabled and EVERY child
        // hidden, keeping only the root slot background. Hiding all children removes the icon, the
        // quantity/name text, the warning, the colored overlay AND the green stock graph in one shot.
        // Same size/style as a real item, so a padded page reads as PageSize uniform slots.
        private static GameObject? CreateBlankSlot(ResourceItem? prototype, Transform container)
        {
            if (prototype == null || container == null) return null;
            try
            {
                var clone = UnityEngine.Object.Instantiate(prototype, container, false);
                clone.name = "__pad";
                clone.enabled = false;                 // stop ResourceItem Update/Refresh (no NRE)
                if (clone.button != null) clone.button.interactable = false;

                var t = clone.transform;
                for (int i = 0; i < t.childCount; i++)
                {
                    var child = t.GetChild(i);
                    if (child != null) child.gameObject.SetActive(false);
                }
                return clone.gameObject;
            }
            catch (Exception ex)
            {
                _log.Warning($"blank slot failed: {ex.Message}");
                return null;
            }
        }
    }
}
