using System;
using System.Collections.Generic;
using System.Linq;
using PerAspera.Core;
using PerAspera.GameAPI.YamlExtensions;

namespace PerAspera.GameAPI.MultiOutput
{
    /// <summary>Internal shared logger for the MultiOutput module.</summary>
    internal static class MultiOutputLog
    {
        private static readonly LogAspera _log = new LogAspera("MultiOut");
        internal static void Info(string msg) => _log.Info(msg);
        internal static void Warning(string msg) => _log.Warning(msg);
        internal static void Error(string msg) => _log.Error(msg);
    }

    /// <summary>An extra output resolved against the native datamodel (ready for injection).</summary>
    /// <example>foreach (var o in MultiOutput.GetExtraOutputs("building_water_mine")) { … }</example>
    public sealed class ResolvedExtraOutput
    {
        /// <summary>Native resource produced.</summary>
        public ResourceType Resource { get; }
        /// <summary>Units per completed cycle (before productivity scaling).</summary>
        public float Quantity { get; }
        /// <summary>Whether the quantity scales with the factory's productivity.</summary>
        public bool ScaleWithProductivity { get; }

        internal ResolvedExtraOutput(ResourceType resource, float quantity, bool scale)
        {
            Resource = resource;
            Quantity = quantity;
            ScaleWithProductivity = scale;
        }
    }

    /// <summary>Event payload raised after a secondary output was injected.</summary>
    public sealed class SecondaryOutputProducedArgs
    {
        /// <summary>Datamodel key of the producing building type.</summary>
        public string BuildingKey { get; internal set; } = "";
        /// <summary>Datamodel key of the produced resource.</summary>
        public string ResourceKey { get; internal set; } = "";
        /// <summary>Units actually injected (after scaling).</summary>
        public float Quantity { get; internal set; }
    }

    /// <summary>
    /// Multi-output system: lets a building type produce several resources per cycle.
    /// Declarative via the <c>multiOutput</c> section of <c>sdk.yaml</c> (see
    /// <see cref="MultiOutputConfig"/>) or programmatic via <see cref="RegisterExtraOutput"/>.
    /// The native single output is untouched; extra outputs are injected as idle cargo in
    /// the factory's stockpile right after each native production cycle
    /// (Postfix on <c>Factory.SpawnOutput</c>).
    /// </summary>
    /// <example>
    /// // Mod C# :
    /// MultiOutput.RegisterExtraOutput("building_water_mine", "resource_carbon", 1f);
    /// MultiOutput.OnSecondaryOutputProduced += a => LogAspera.Info($"{a.ResourceKey} +{a.Quantity}");
    /// </example>
    public static class MultiOutput
    {
        /// <summary>sdk.yaml section name handled by this system.</summary>
        public const string Section = "multiOutput";

        // buildingKey → resolved outputs (consulted in the SpawnOutput hot path)
        private static readonly Dictionary<string, List<ResolvedExtraOutput>> _resolved = new();
        // code-registered raw entries (merged at resolution; survive re-resolution)
        private static readonly List<(string buildingKey, ExtraOutputDef def)> _codeEntries = new();
        private static readonly object _lock = new();

        /// <summary>Raised after a secondary output was injected into a stockpile.</summary>
        /// <example>MultiOutput.OnSecondaryOutputProduced += a => { … };</example>
        public static event Action<SecondaryOutputProducedArgs>? OnSecondaryOutputProduced;

        /// <summary>
        /// Declares the schema and subscribes resolution to YamlExtensions.DataLoaded.
        /// Called once by <see cref="MultiOutputAutoStart"/>; idempotent.
        /// </summary>
        /// <example>MultiOutput.Initialize(); // fait automatiquement par l'auto-start</example>
        public static void Initialize()
        {
            ExtensionSchemaRegistry.Register(new ExtensionSchema(
                section: Section,
                targetTable: "building",
                dtoType: typeof(MultiOutputConfig),
                validate: o => ((MultiOutputConfig)o).ExtraOutputs is { Count: > 0 } outs
                               && outs.All(e => e.Quantity > 0f && !string.IsNullOrWhiteSpace(e.Resource))));

            YamlExtensions.YamlExtensions.DataLoaded += Resolve;
        }

        /// <summary>
        /// Registers an extra output by code (same semantics as a sdk.yaml entry).
        /// Safe to call from a plugin's Load(); resolution happens when the datamodel is ready.
        /// </summary>
        /// <param name="buildingTypeKey">Building type key (e.g. "building_water_mine").</param>
        /// <param name="resourceKey">Resource key (e.g. "resource_carbon").</param>
        /// <param name="quantity">Units per completed cycle.</param>
        /// <param name="scaleWithProductivity">Scale by factory productivity (default true).</param>
        /// <example>MultiOutput.RegisterExtraOutput("building_water_mine", "resource_carbon", 1f);</example>
        public static void RegisterExtraOutput(string buildingTypeKey, string resourceKey,
                                               float quantity, bool scaleWithProductivity = true)
        {
            var def = new ExtraOutputDef
            {
                Resource = resourceKey,
                Quantity = quantity,
                ScaleWithProductivity = scaleWithProductivity
            };
            lock (_lock) { _codeEntries.Add((buildingTypeKey, def)); }

            if (YamlExtensions.YamlExtensions.IsLoaded)
                ResolveEntry(buildingTypeKey, def, "code");
        }

        /// <summary>Effective extra outputs of a building type (empty list if none).</summary>
        /// <param name="buildingTypeKey">Building type key.</param>
        /// <example>var outs = MultiOutput.GetExtraOutputs("building_water_mine");</example>
        public static IReadOnlyList<ResolvedExtraOutput> GetExtraOutputs(string buildingTypeKey)
        {
            lock (_lock)
            {
                return _resolved.TryGetValue(buildingTypeKey, out var list)
                    ? list.ToList()
                    : (IReadOnlyList<ResolvedExtraOutput>)Array.Empty<ResolvedExtraOutput>();
            }
        }

        /// <summary>Hot-path lookup used by the SpawnOutput patch (no allocation when absent).</summary>
        internal static List<ResolvedExtraOutput>? GetResolvedOrNull(string buildingTypeKey)
        {
            lock (_lock)
            {
                return _resolved.TryGetValue(buildingTypeKey, out var list) ? list : null;
            }
        }

        internal static void RaiseProduced(SecondaryOutputProducedArgs args)
        {
            try { OnSecondaryOutputProduced?.Invoke(args); }
            catch (Exception ex) { MultiOutputLog.Error($"Handler OnSecondaryOutputProduced : {ex.Message}"); }
        }

        // ── Résolution (post-DataLoaded : les tables natives sont peuplées) ──

        private static void Resolve()
        {
            lock (_lock) { _resolved.Clear(); }

            int items = 0;
            foreach (var kvp in YamlExtensions.YamlExtensions.GetAll<MultiOutputConfig>(Section))
            {
                foreach (var def in kvp.Value.ExtraOutputs)
                    if (ResolveEntry(kvp.Key, def, "sdk.yaml")) items++;
            }

            List<(string, ExtraOutputDef)> codeSnapshot;
            lock (_lock) { codeSnapshot = _codeEntries.ToList(); }
            foreach (var (key, def) in codeSnapshot)
                if (ResolveEntry(key, def, "code")) items++;

            MultiOutputLog.Info($"{items} sortie(s) secondaire(s) actives sur " +
                                $"{_resolved.Count} type(s) de bâtiment.");
        }

        private static bool ResolveEntry(string buildingKey, ExtraOutputDef def, string origin)
        {
            try
            {
                if (BuildingType.table == null || !BuildingType.table.ContainsKey(buildingKey))
                {
                    MultiOutputLog.Error($"[{origin}] '{buildingKey}' absent de la table building — entrée ignorée.");
                    return false;
                }
                if (ResourceType.table == null || !ResourceType.table.ContainsKey(def.Resource))
                {
                    MultiOutputLog.Error($"[{origin}] '{def.Resource}' absent de la table resource " +
                                         $"(pour '{buildingKey}') — entrée ignorée.");
                    return false;
                }

                var resource = ResourceType.table[def.Resource];
                var resolved = new ResolvedExtraOutput(resource, def.Quantity, def.ScaleWithProductivity);
                lock (_lock)
                {
                    if (!_resolved.TryGetValue(buildingKey, out var list))
                        _resolved[buildingKey] = list = new List<ResolvedExtraOutput>();
                    list.Add(resolved);
                }
                MultiOutputLog.Info($"[{origin}] {buildingKey} → +{def.Quantity} {def.Resource}" +
                                    (def.ScaleWithProductivity ? " (×productivité)" : "") + ".");
                return true;
            }
            catch (Exception ex)
            {
                MultiOutputLog.Error($"[{origin}] Résolution {buildingKey}/{def.Resource} : {ex.Message}");
                return false;
            }
        }

        // NOTE affichage : ne PAS pousser la sortie secondaire dans
        // BuildingType.displayOutputs — testé en jeu (Wafhien, juin 2026) : le panneau
        // OUTPUT vanilla superpose les icônes au même emplacement (layout prévu pour une
        // seule sortie). L'affichage propre nécessite un patch UI du BuildingWorldPanel
        // (chantier séparé, domaine per-aspera-sdk-ui). En attendant, les sorties
        // secondaires restent visibles dans le STOCKPILE du bâtiment.
    }
}
