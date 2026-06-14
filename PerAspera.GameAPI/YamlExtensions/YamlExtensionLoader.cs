using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using PerAspera.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PerAspera.GameAPI.YamlExtensions
{
    /// <summary>Internal shared logger for the YamlExtensions module.</summary>
    internal static class YamlExtensionsLog
    {
        private static readonly LogAspera _log = new LogAspera("YamlExt");
        internal static void Info(string msg) => _log.Info(msg);
        internal static void Warning(string msg) => _log.Warning(msg);
        internal static void Error(string msg) => _log.Error(msg);
    }

    /// <summary>
    /// Scans <c>StreamingAssets/Mods/*/sdk.yaml</c> sidecar files, deserializes each
    /// registered section's items to their DTO type (YamlDotNet, camelCase), validates
    /// them, and exposes the merged result to <see cref="YamlExtensions"/>.
    ///
    /// Merge semantics: mods are processed in alphabetical directory order; on item-key
    /// conflict the LAST mod wins (mirrors the native <c>!replace</c> behaviour), with a
    /// WARNING logged. Parsed data is also persisted to ModDatabase (table
    /// <c>sdk_extensions</c>) for tooling/inspection.
    /// </summary>
    /// <example>
    /// // Triggered automatically post-CompleteLoading by YamlExtensionsAutoStart;
    /// // manual re-run (tests):
    /// YamlExtensionLoader.LoadAll();
    /// </example>
    public static class YamlExtensionLoader
    {
        /// <summary>Format version of sdk.yaml this loader understands.</summary>
        public const int SupportedVersion = 1;

        // section → (itemKey → typed DTO)
        private static readonly Dictionary<string, Dictionary<string, object>> _store = new();
        // section → (itemKey → providing modId), for conflict reporting
        private static readonly Dictionary<string, Dictionary<string, string>> _providers = new();
        private static readonly object _lock = new();

        /// <summary>True once a load pass has completed (even with 0 files found).</summary>
        public static bool IsLoaded { get; private set; }

        /// <summary>Raised after a load pass completes. Consumers resolve native refs here.</summary>
        public static event Action? OnLoaded;

        /// <summary>
        /// Root folder scanned for <c>*/sdk.yaml</c>. Defaults to the game's
        /// <c>StreamingAssets/Mods</c>; overridable for tests.
        /// </summary>
        public static string ModsRoot { get; set; } =
            Path.Combine(UnityEngine.Application.dataPath, "StreamingAssets", "Mods");

        /// <summary>
        /// Runs a full load pass: scan, parse, validate, merge, persist, then raises
        /// <see cref="OnLoaded"/>. Idempotent — a new pass replaces the previous store.
        /// </summary>
        /// <example>YamlExtensionLoader.LoadAll();</example>
        public static void LoadAll()
        {
            lock (_lock)
            {
                _store.Clear();
                _providers.Clear();

                try
                {
                    if (!Directory.Exists(ModsRoot))
                    {
                        YamlExtensionsLog.Info($"Dossier mods introuvable ({ModsRoot}) — aucune extension chargée.");
                    }
                    else
                    {
                        // Alphabetical order = deterministic merge; documented limitation
                        // (the native manifest order is not exposed here).
                        var files = Directory.GetDirectories(ModsRoot)
                            .OrderBy(d => d, StringComparer.OrdinalIgnoreCase)
                            .Select(d => Path.Combine(d, "sdk.yaml"))
                            .Where(File.Exists)
                            .ToList();

                        YamlExtensionsLog.Info($"{files.Count} fichier(s) sdk.yaml trouvé(s) dans {ModsRoot}.");
                        foreach (var file in files)
                            LoadFile(file);
                    }
                }
                catch (Exception ex)
                {
                    YamlExtensionsLog.Error($"Scan échoué : {ex.Message}");
                }

                IsLoaded = true;
            }

            try { OnLoaded?.Invoke(); }
            catch (Exception ex) { YamlExtensionsLog.Error($"Handler OnLoaded : {ex.Message}"); }
        }

        /// <summary>Typed lookup used by <see cref="YamlExtensions"/> (item or null).</summary>
        internal static object? Get(string section, string itemKey)
        {
            lock (_lock)
            {
                return _store.TryGetValue(section, out var items) &&
                       items.TryGetValue(itemKey, out var dto) ? dto : null;
            }
        }

        /// <summary>Snapshot of all items of a section (possibly empty).</summary>
        internal static Dictionary<string, object> GetSection(string section)
        {
            lock (_lock)
            {
                return _store.TryGetValue(section, out var items)
                    ? new Dictionary<string, object>(items)
                    : new Dictionary<string, object>();
            }
        }

        /// <summary>
        /// modId of the mod that provided an item (last writer on conflict), or null if the
        /// item is unknown. Lets consumers resolve file paths relative to the mod folder.
        /// </summary>
        internal static string? GetProvider(string section, string itemKey)
        {
            lock (_lock)
            {
                return _providers.TryGetValue(section, out var items) &&
                       items.TryGetValue(itemKey, out var modId) ? modId : null;
            }
        }

        /// <summary>True if at least one item was loaded for the section.</summary>
        internal static bool HasSection(string section)
        {
            lock (_lock) { return _store.ContainsKey(section) && _store[section].Count > 0; }
        }

        // ── Parsing ──────────────────────────────────────────────────────────

        private static void LoadFile(string path)
        {
            string modId = Path.GetFileName(Path.GetDirectoryName(path)) ?? "?";
            string text;
            try { text = File.ReadAllText(path); }
            catch (Exception ex)
            {
                YamlExtensionsLog.Error($"[{modId}] Lecture impossible : {ex.Message}");
                return;
            }

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            SdkYamlFile? root;
            try { root = deserializer.Deserialize<SdkYamlFile>(text); }
            catch (Exception ex)
            {
                YamlExtensionsLog.Error($"[{modId}] sdk.yaml invalide : {ex.Message}");
                return;
            }

            if (root?.Extensions == null || root.Extensions.Count == 0)
            {
                YamlExtensionsLog.Warning($"[{modId}] sdk.yaml sans section 'extensions:' — ignoré.");
                return;
            }
            if (root.SdkExtensionVersion > SupportedVersion)
                YamlExtensionsLog.Warning(
                    $"[{modId}] sdkExtensionVersion={root.SdkExtensionVersion} > supporté ({SupportedVersion}) — chargement best-effort.");

            string checksum = ComputeChecksum(text);
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            foreach (var sectionKvp in root.Extensions)
            {
                string section = sectionKvp.Key;
                if (!ExtensionSchemaRegistry.TryGet(section, out var schema))
                {
                    YamlExtensionsLog.Warning(
                        $"[{modId}] Section '{section}' inconnue (aucun système enregistré) — ignorée.");
                    continue;
                }
                if (sectionKvp.Value == null) continue;

                var jsonForDb = new Dictionary<string, string>();
                int ok = 0, ko = 0;

                foreach (var itemKvp in sectionKvp.Value)
                {
                    string itemKey = itemKvp.Key;
                    object? dto = DeserializeItem(serializer, deserializer, itemKvp.Value, schema.DtoType,
                                                  modId, section, itemKey);
                    if (dto == null) { ko++; continue; }

                    if (schema.Validate != null)
                    {
                        bool valid;
                        try { valid = schema.Validate(dto); }
                        catch (Exception ex)
                        {
                            YamlExtensionsLog.Error($"[{modId}] {section}/{itemKey} : validation a levé {ex.Message}");
                            valid = false;
                        }
                        if (!valid)
                        {
                            YamlExtensionsLog.Error($"[{modId}] {section}/{itemKey} : rejeté par la validation du schéma.");
                            ko++;
                            continue;
                        }
                    }

                    StoreItem(section, itemKey, dto, modId);
                    try { jsonForDb[itemKey] = System.Text.Json.JsonSerializer.Serialize(dto, schema.DtoType); }
                    catch { /* persistance DB best-effort */ }
                    ok++;
                }

                YamlExtensionsLog.Info($"[{modId}] Section '{section}' : {ok} item(s) chargé(s)" +
                                       (ko > 0 ? $", {ko} rejeté(s)" : "") + ".");
                PersistToDatabase(section, modId, jsonForDb, checksum);
            }
        }

        /// <summary>
        /// Re-serializes the generic YAML node then deserializes it to the schema's DTO
        /// type — robust round-trip without binding the whole file to a generic graph.
        /// </summary>
        private static object? DeserializeItem(ISerializer serializer, IDeserializer deserializer,
            object? rawNode, Type dtoType, string modId, string section, string itemKey)
        {
            if (rawNode == null)
            {
                YamlExtensionsLog.Error($"[{modId}] {section}/{itemKey} : item vide.");
                return null;
            }
            try
            {
                string yaml = serializer.Serialize(rawNode);
                return deserializer.Deserialize(yaml, dtoType);
            }
            catch (Exception ex)
            {
                YamlExtensionsLog.Error($"[{modId}] {section}/{itemKey} : désérialisation vers " +
                                        $"{dtoType.Name} échouée — {ex.Message}");
                return null;
            }
        }

        private static void StoreItem(string section, string itemKey, object dto, string modId)
        {
            if (!_store.TryGetValue(section, out var items))
            {
                items = new Dictionary<string, object>();
                _store[section] = items;
                _providers[section] = new Dictionary<string, string>();
            }
            if (items.ContainsKey(itemKey))
                YamlExtensionsLog.Warning(
                    $"Conflit {section}/{itemKey} : '{_providers[section][itemKey]}' écrasé par '{modId}' (dernier mod gagne).");
            items[itemKey] = dto;
            _providers[section][itemKey] = modId;
        }

        private static void PersistToDatabase(string section, string modId,
            Dictionary<string, string> itemsJson, string checksum)
        {
            if (itemsJson.Count == 0) return;
            try
            {
                Database.ModDatabase.Instance.StoreExtensionData(section, modId, itemsJson, checksum);
            }
            catch (Exception ex)
            {
                // DB = cache d'inspection uniquement ; le store mémoire fait foi.
                YamlExtensionsLog.Warning($"Persistance ModDatabase échouée ({section}) : {ex.Message}");
            }
        }

        private static string ComputeChecksum(string text)
        {
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(text)));
        }

        /// <summary>Root document shape of a sdk.yaml sidecar file.</summary>
        private sealed class SdkYamlFile
        {
            /// <summary>Format version (see <see cref="SupportedVersion"/>).</summary>
            public int SdkExtensionVersion { get; set; } = 1;

            /// <summary>section name → (item key → raw YAML node).</summary>
            public Dictionary<string, Dictionary<string, object>>? Extensions { get; set; }
        }
    }
}
