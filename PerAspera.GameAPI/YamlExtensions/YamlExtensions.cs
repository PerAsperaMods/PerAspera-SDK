using System;
using System.Collections.Generic;
using System.Linq;

namespace PerAspera.GameAPI.YamlExtensions
{
    /// <summary>
    /// Public read API over the merged <c>sdk.yaml</c> extension data.
    /// Data is available once the game's YAML datamodel has finished loading
    /// (the loader runs automatically after <c>YAMLLoader.CompleteLoading</c>);
    /// subscribe to <see cref="DataLoaded"/> to resolve native references safely.
    /// </summary>
    /// <example>
    /// // Déclaration (au Load du plugin consommateur) :
    /// ExtensionSchemaRegistry.Register(new ExtensionSchema("multiOutput", "building", typeof(MultiOutputConfig)));
    ///
    /// // Consommation (après DataLoaded) :
    /// YamlExtensions.DataLoaded += () => {
    ///     var cfg = YamlExtensions.GetFor&lt;MultiOutputConfig&gt;("multiOutput", "building_ma_raffinerie");
    ///     foreach (var (key, c) in YamlExtensions.GetAll&lt;MultiOutputConfig&gt;("multiOutput")) { … }
    /// };
    /// </example>
    public static class YamlExtensions
    {
        /// <summary>True once the load pass has completed.</summary>
        /// <example>if (YamlExtensions.IsLoaded) { … }</example>
        public static bool IsLoaded => YamlExtensionLoader.IsLoaded;

        /// <summary>
        /// Raised after extension data is (re)loaded. If data is already loaded when a
        /// handler subscribes, the handler is invoked immediately (no race at startup).
        /// </summary>
        /// <example>YamlExtensions.DataLoaded += () => Resolve();</example>
        public static event Action DataLoaded
        {
            add
            {
                YamlExtensionLoader.OnLoaded += value;
                if (YamlExtensionLoader.IsLoaded) value();
            }
            remove => YamlExtensionLoader.OnLoaded -= value;
        }

        /// <summary>
        /// Returns the extension config of one item, or null if the item has none.
        /// </summary>
        /// <typeparam name="T">DTO type registered for the section.</typeparam>
        /// <param name="section">Section name (e.g. "multiOutput").</param>
        /// <param name="itemKey">Datamodel item key (e.g. "building_ma_raffinerie").</param>
        /// <example>var cfg = YamlExtensions.GetFor&lt;MultiOutputConfig&gt;("multiOutput", "building_x");</example>
        public static T? GetFor<T>(string section, string itemKey) where T : class
        {
            var dto = YamlExtensionLoader.Get(section, itemKey);
            if (dto == null) return null;
            if (dto is T typed) return typed;
            YamlExtensionsLog.Error(
                $"GetFor<{typeof(T).Name}>('{section}') : le schéma enregistré utilise {dto.GetType().Name}.");
            return null;
        }

        /// <summary>
        /// Returns all items of a section, keyed by datamodel item key. Empty if the
        /// section is unknown or no mod provides it.
        /// </summary>
        /// <typeparam name="T">DTO type registered for the section.</typeparam>
        /// <param name="section">Section name.</param>
        /// <example>foreach (var (key, cfg) in YamlExtensions.GetAll&lt;MultiOutputConfig&gt;("multiOutput")) { … }</example>
        public static IReadOnlyDictionary<string, T> GetAll<T>(string section) where T : class
        {
            var result = new Dictionary<string, T>();
            foreach (var kvp in YamlExtensionLoader.GetSection(section))
            {
                if (kvp.Value is T typed) result[kvp.Key] = typed;
                else YamlExtensionsLog.Error(
                    $"GetAll<{typeof(T).Name}>('{section}') : item '{kvp.Key}' de type {kvp.Value.GetType().Name} ignoré.");
            }
            return result;
        }

        /// <summary>True if at least one loaded mod provides data for the section.</summary>
        /// <param name="section">Section name.</param>
        /// <example>if (YamlExtensions.HasSection("gasEmissions")) { … }</example>
        public static bool HasSection(string section) => YamlExtensionLoader.HasSection(section);

        /// <summary>
        /// modId of the mod that provided an item (the one whose sdk.yaml declared it; last
        /// writer on conflict), or null if unknown. Use it to resolve asset paths relative to
        /// the mod folder: <c>Path.Combine(YamlExtensions.ModsRoot, providerId, relativePath)</c>.
        /// </summary>
        /// <param name="section">Section name (e.g. "hubIcons").</param>
        /// <param name="itemKey">Datamodel item key (e.g. "building_drone_base_2").</param>
        /// <example>string? mod = YamlExtensions.GetProviderId("hubIcons", "building_drone_base_2");</example>
        public static string? GetProviderId(string section, string itemKey)
            => YamlExtensionLoader.GetProvider(section, itemKey);

        /// <summary>
        /// Root folder scanned for <c>*/sdk.yaml</c> (the game's <c>StreamingAssets/Mods</c>).
        /// Combine with <see cref="GetProviderId"/> to locate a mod's asset files.
        /// </summary>
        /// <example>var dir = Path.Combine(YamlExtensions.ModsRoot, modId);</example>
        public static string ModsRoot => YamlExtensionLoader.ModsRoot;

        /// <summary>
        /// Convenience passthrough to <see cref="ExtensionSchemaRegistry.Register"/> —
        /// declare your section before the game finishes loading YAML.
        /// </summary>
        /// <param name="schema">The section schema.</param>
        /// <example>YamlExtensions.RegisterSchema(new ExtensionSchema("gases", "resource", typeof(GasDefinition)));</example>
        public static void RegisterSchema(ExtensionSchema schema) => ExtensionSchemaRegistry.Register(schema);

        /// <summary>
        /// Exports all registered schemas as JSON (section, targetTable, dtoType) for
        /// external tooling (validate_yaml_mods.py).
        /// </summary>
        /// <example>File.WriteAllText("sdk-extension-schemas.json", YamlExtensions.ExportSchemasJson());</example>
        public static string ExportSchemasJson()
        {
            var export = ExtensionSchemaRegistry.All
                .Select(s => new { section = s.Section, targetTable = s.TargetTable, dtoType = s.DtoType.FullName })
                .ToList();
            return System.Text.Json.JsonSerializer.Serialize(export,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
        }
    }
}
