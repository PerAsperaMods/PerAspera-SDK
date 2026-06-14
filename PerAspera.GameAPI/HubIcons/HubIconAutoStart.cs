using System;
using System.Runtime.InteropServices;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;

#pragma warning disable CS1591
namespace PerAspera.GameAPI.HubIcons
{
    /// <summary>
    /// Auto-start plugin for the HubIcons system: registers the <c>hubIcons</c> extension schema
    /// before the datamodel loads, and applies the Postfix on the lenses' <c>SetBuildingHubIcon</c>
    /// that swaps the worker-hub icon according to the active-drone count.
    /// </summary>
    /// <example>
    /// // Aucune action requise par les mods : déployer PerAspera.GameAPI.dll suffit.
    /// // Déclaration via sdk.yaml (section hubIcons) ou HubIcons.RegisterHubIcon().
    /// </example>
    [BepInPlugin("PerAspera.GameAPI.HubIcons", "PerAspera HubIcons", "1.0.0")]
    public class HubIconAutoStart : BasePlugin
    {
        public override void Load()
        {
            try
            {
                HubIcons.Initialize();

                var harmony = new Harmony("PerAspera.GameAPI.HubIcons");
                harmony.PatchAll(typeof(NormalLensHubIconPatch));
                harmony.PatchAll(typeof(TrafficLensHubIconPatch));

                Log.LogInfo("[HubIcons] Actif — section 'hubIcons' enregistrée, " +
                            "patchs SetBuildingHubIcon (NormalLens + TrafficLens) posés.");
            }
            catch (Exception ex)
            {
                Log.LogError($"[HubIcons] Initialisation échouée : {ex.Message}");
            }
        }
    }

    /// <summary>Postfix on <c>NormalLens.SetBuildingHubIcon</c> — applies the custom hub icon.</summary>
    [HarmonyPatch(typeof(NormalLens), "SetBuildingHubIcon")]
    internal static class NormalLensHubIconPatch
    {
        [HarmonyPostfix]
        public static void Postfix(BuildingPresenter presenter)
            => HubIconRuntime.Apply(presenter);
    }

    /// <summary>Postfix on <c>TrafficLens.SetBuildingHubIcon</c> — applies the custom hub icon.</summary>
    [HarmonyPatch(typeof(TrafficLens), "SetBuildingHubIcon")]
    internal static class TrafficLensHubIconPatch
    {
        [HarmonyPostfix]
        public static void Postfix(BuildingPresenter presenter)
            => HubIconRuntime.Apply(presenter);
    }

    /// <summary>
    /// Shared runtime that reads the building's active-drone count and overrides the hub icon via
    /// the game's own <c>BuildingView.OverrideIcon</c>. The icon is drawn by an instanced batch
    /// (<c>LensSystem.iconBatchIndices : Dictionary&lt;Material,int&gt;</c>): the batch renders the
    /// texture of the supplied <b>Material</b> at the sprite's Rect — NOT the sprite's own texture,
    /// and a null material throws. So to show an external PNG we clone the building's
    /// <c>iconMaterial</c> (correct icon shader, already a batch key) and swap its main texture to
    /// our sprite's texture; the batch then registers/renders that material. Runs as a Postfix
    /// after the native icon is set, so our override wins for the frame.
    /// </summary>
    internal static class HubIconRuntime
    {
        // dronesPassingOrDocked: DroneAccounting at Building+0x98, int value at +0x20 (offsets validés)
        private const int OFFSET_DRONE_ACCOUNTING = 0x98;
        private const int OFFSET_DRONES_PASSING_OR_DOCKED = 0x20;

        // Buildings we currently hold an override on → restore native when leaving a custom state.
        private static readonly System.Collections.Generic.HashSet<IntPtr> _overridden = new();
        // One batch-material per sprite (clone of an icon material + our texture). ≤ #declared icons.
        private static readonly System.Collections.Generic.Dictionary<Sprite, Material> _spriteMats = new();
        // Captured icon-shader template (any building's iconMaterial) — reused if a view's is null.
        private static Material? _iconMatTemplate;

        // Bounded startup diagnostic (first applications only, to keep logs clean).
        private static int _diag;
        private const int DIAG_MAX = 12;

        public static void Apply(BuildingPresenter presenter)
        {
            if (presenter == null || presenter.Pointer == IntPtr.Zero) return;
            try
            {
                Building building = presenter.building;
                if (building == null || building.Pointer == IntPtr.Zero) return;

                BuildingType bt = building.buildingType;
                if (bt == null) return;
                string key = bt.key;
                if (!HubIcons.HasConfig(key)) return; // fast path: building has no hub icons

                IntPtr acct = Marshal.ReadIntPtr(building.Pointer + OFFSET_DRONE_ACCOUNTING);
                if (acct == IntPtr.Zero) return;
                int active = Marshal.ReadInt32(acct + OFFSET_DRONES_PASSING_OR_DOCKED);

                BuildingView view = presenter.buildingView;
                if (view == null || view.Pointer == IntPtr.Zero) return;

                Sprite? sprite = HubIcons.GetSprite(key, active);
                if (sprite != null)
                {
                    Material? mat = GetBatchMaterial(view, sprite);
                    if (mat == null)
                    {
                        if (_diag < DIAG_MAX)
                        { _diag++; HubIconsLog.Warning($"[apply] {key} active={active} : iconMaterial indisponible — saut."); }
                        return;
                    }

                    view.OverrideIcon(sprite, mat);
                    _overridden.Add(building.Pointer);

                    if (_diag < DIAG_MAX)
                    {
                        _diag++;
                        HubIconsLog.Info($"[apply] {key} active={active} → override (mat={mat.name}).");
                    }
                }
                else if (_overridden.Remove(building.Pointer))
                {
                    // Left a custom state → restore the game's native icon.
                    view.ResetIconToDefault();
                }
            }
            catch (Exception ex)
            {
                HubIconsLog.Error($"Apply: {ex.Message}");
            }
        }

        /// <summary>
        /// Returns (cached) a Material that draws <paramref name="sprite"/>'s texture with the icon
        /// shader, suitable as a batch key for <c>OverrideIcon</c>. Cloned from the building's
        /// <c>iconMaterial</c> (or the previously captured template) with the main texture swapped.
        /// </summary>
        private static Material? GetBatchMaterial(BuildingView view, Sprite sprite)
        {
            if (_spriteMats.TryGetValue(sprite, out var cached) && cached != null) return cached;

            Material? template = view.iconMaterial != null ? view.iconMaterial : _iconMatTemplate;
            if (template == null) return null;
            _iconMatTemplate = template;

            var mat = new Material(template) { mainTexture = sprite.texture };
            mat.name = $"HubIcon_{sprite.name}";
            _spriteMats[sprite] = mat;
            return mat;
        }
    }
}
