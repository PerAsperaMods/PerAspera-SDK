using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PerAspera.Core;
using PerAspera.GameAPI.YamlExtensions;
using UnityEngine;

namespace PerAspera.GameAPI.HubIcons
{
    /// <summary>Internal shared logger for the HubIcons module.</summary>
    internal static class HubIconsLog
    {
        private static readonly LogAspera _log = new LogAspera("HubIcons");
        internal static void Info(string msg) => _log.Info(msg);
        internal static void Warning(string msg) => _log.Warning(msg);
        internal static void Error(string msg) => _log.Error(msg);
    }

    /// <summary>
    /// Worker-hub icon system: lets a building type show a custom icon per active-drone count.
    /// The game natively renders only two states (empty = <c>BuildingType.emptyHubIconName</c>,
    /// full = <c>BuildingType.iconName</c>); this system fills the intermediate states (1..N-1)
    /// and may also override empty/full. Generic for any <c>droneCapacity</c> N.
    ///
    /// Declarative via the <c>hubIcons</c> section of a mod's <c>sdk.yaml</c> (see
    /// <see cref="HubIconConfig"/>) or programmatic via <see cref="RegisterHubIcon"/>. Sprites
    /// are loaded from PNG files relative to the providing mod's folder. The runtime swap is
    /// applied by <c>HubIconAutoStart</c> (Postfix on the lens' <c>SetBuildingHubIcon</c>).
    /// </summary>
    /// <example>
    /// // Mod C# (optionnel — la voie normale est sdk.yaml) :
    /// var sprite = ...; // chargé par le mod
    /// HubIcons.RegisterHubIcon("building_drone_base_2", 1, sprite);
    /// </example>
    public static class HubIcons
    {
        /// <summary>sdk.yaml section name handled by this system.</summary>
        public const string Section = "hubIcons";

        // buildingKey → (active drone count → sprite), consulted in the SetBuildingHubIcon hot path
        private static readonly Dictionary<string, Dictionary<int, Sprite>> _resolved = new();
        private static readonly object _lock = new();

        /// <summary>
        /// Declares the schema and subscribes resolution to YamlExtensions.DataLoaded.
        /// Called once by <c>HubIconAutoStart</c>; idempotent.
        /// </summary>
        /// <example>HubIcons.Initialize(); // fait automatiquement par l'auto-start</example>
        public static void Initialize()
        {
            ExtensionSchemaRegistry.Register(new ExtensionSchema(
                section: Section,
                targetTable: "building",
                dtoType: typeof(HubIconConfig),
                validate: o => ((HubIconConfig)o).Icons is { Count: > 0 } icons
                               && icons.All(kv => kv.Key >= 0 && !string.IsNullOrWhiteSpace(kv.Value))));

            YamlExtensions.YamlExtensions.DataLoaded += Resolve;
        }

        /// <summary>
        /// Registers a hub icon by code (same effect as a sdk.yaml entry, but the caller
        /// supplies the already-loaded sprite). Safe to call after the datamodel is ready.
        /// </summary>
        /// <param name="buildingTypeKey">Building type key (e.g. "building_drone_base_2").</param>
        /// <param name="activeCount">Active-drone count this icon applies to (0 = empty, N = full).</param>
        /// <param name="sprite">Sprite to display for that state.</param>
        /// <example>HubIcons.RegisterHubIcon("building_drone_base_2", 1, mySprite);</example>
        public static void RegisterHubIcon(string buildingTypeKey, int activeCount, Sprite sprite)
        {
            if (sprite == null || string.IsNullOrWhiteSpace(buildingTypeKey) || activeCount < 0) return;
            lock (_lock)
            {
                if (!_resolved.TryGetValue(buildingTypeKey, out var states))
                    _resolved[buildingTypeKey] = states = new Dictionary<int, Sprite>();
                states[activeCount] = sprite;
            }
        }

        /// <summary>True if at least one hub icon state is configured for the building type.</summary>
        /// <param name="buildingTypeKey">Building type key.</param>
        /// <example>if (HubIcons.HasConfig("building_drone_base_2")) { … }</example>
        public static bool HasConfig(string buildingTypeKey)
        {
            lock (_lock) { return _resolved.ContainsKey(buildingTypeKey); }
        }

        /// <summary>
        /// Sprite to display for a building type at a given active-drone count, or null if no
        /// custom icon is configured for that state (caller keeps the native icon).
        /// </summary>
        /// <param name="buildingTypeKey">Building type key.</param>
        /// <param name="activeCount">Active-drone count.</param>
        /// <example>var s = HubIcons.GetSprite("building_drone_base_3", 2);</example>
        public static Sprite? GetSprite(string buildingTypeKey, int activeCount)
        {
            lock (_lock)
            {
                return _resolved.TryGetValue(buildingTypeKey, out var states) &&
                       states.TryGetValue(activeCount, out var sprite) ? sprite : null;
            }
        }

        // ── Résolution (post-DataLoaded : les tables natives sont peuplées, Unity prêt) ──

        private static void Resolve()
        {
            lock (_lock) { _resolved.Clear(); }

            int sprites = 0, buildings = 0;
            foreach (var kvp in YamlExtensions.YamlExtensions.GetAll<HubIconConfig>(Section))
            {
                string buildingKey = kvp.Key;
                if (BuildingType.table == null || !BuildingType.table.ContainsKey(buildingKey))
                {
                    HubIconsLog.Error($"'{buildingKey}' absent de la table building — hub icons ignorés.");
                    continue;
                }

                string? modId = YamlExtensions.YamlExtensions.GetProviderId(Section, buildingKey);
                if (modId == null)
                {
                    HubIconsLog.Error($"'{buildingKey}' : modId fournisseur introuvable — ignoré.");
                    continue;
                }
                string modFolder = Path.Combine(YamlExtensions.YamlExtensions.ModsRoot, modId);

                int perBuilding = 0;
                foreach (var (count, relPath) in kvp.Value.Icons)
                {
                    Sprite? sprite = LoadSprite(modFolder, relPath, buildingKey, count);
                    if (sprite == null) continue;
                    RegisterHubIcon(buildingKey, count, sprite);
                    perBuilding++;
                    sprites++;
                }
                if (perBuilding > 0) buildings++;
            }

            HubIconsLog.Info($"{sprites} sprite(s) de hub sur {buildings} type(s) de bâtiment prêt(s).");
        }

        private static Sprite? LoadSprite(string modFolder, string relPath, string buildingKey, int count)
        {
            try
            {
                string file = Path.Combine(modFolder, relPath);
                if (!File.Exists(file))
                {
                    HubIconsLog.Error($"{buildingKey}/{count} : fichier introuvable ({file}).");
                    return null;
                }

                byte[] data = File.ReadAllBytes(file);
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Bilinear,
                    name = $"{buildingKey}_{count}"
                };
                ImageConversion.LoadImage(tex, data);
                var sprite = Sprite.Create(tex,
                    new Rect(0f, 0f, tex.width, tex.height),
                    new Vector2(0.5f, 0.5f), 100f);
                sprite.name = $"{buildingKey}_{count}";

                HubIconsLog.Info($"  {buildingKey}/{count} chargé ({tex.width}x{tex.height}).");
                return sprite;
            }
            catch (Exception ex)
            {
                HubIconsLog.Error($"{buildingKey}/{count} : chargement échoué — {ex.Message}");
                return null;
            }
        }
    }
}
