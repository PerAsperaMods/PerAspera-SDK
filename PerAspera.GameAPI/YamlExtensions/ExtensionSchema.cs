using System;
using System.Collections.Generic;
using System.Linq;

namespace PerAspera.GameAPI.YamlExtensions
{
    /// <summary>
    /// Describes one extension section of the <c>sdk.yaml</c> sidecar file.
    /// A consumer system (MultiOutput, GasRegistry, a third-party mod…) registers its
    /// schema via <see cref="ExtensionSchemaRegistry.Register"/> so the loader knows how
    /// to deserialize, validate and report the section's items.
    /// </summary>
    /// <example>
    /// ExtensionSchemaRegistry.Register(new ExtensionSchema(
    ///     section: "multiOutput",
    ///     targetTable: "building",
    ///     dtoType: typeof(MultiOutputConfig),
    ///     validate: o => ((MultiOutputConfig)o).ExtraOutputs.Count > 0));
    /// </example>
    public sealed class ExtensionSchema
    {
        /// <summary>Top-level key under <c>extensions:</c> in sdk.yaml (e.g. "multiOutput").</summary>
        public string Section { get; }

        /// <summary>
        /// Native datamodel table the item keys refer to ("building", "resource"…).
        /// Metadata for tooling and cross-validation; the loader does not resolve natives itself.
        /// </summary>
        public string TargetTable { get; }

        /// <summary>DTO type each item of the section is deserialized to (YamlDotNet, camelCase).</summary>
        public Type DtoType { get; }

        /// <summary>
        /// Optional semantic validation, called per item after deserialization.
        /// Return false to reject the item (logged as ERROR, item skipped).
        /// </summary>
        public Func<object, bool>? Validate { get; }

        /// <summary>Creates an extension schema descriptor.</summary>
        /// <param name="section">Top-level key under <c>extensions:</c> (non-empty).</param>
        /// <param name="targetTable">Native table the item keys refer to.</param>
        /// <param name="dtoType">DTO type for item deserialization.</param>
        /// <param name="validate">Optional per-item semantic validation.</param>
        /// <example>new ExtensionSchema("gasEmissions", "building", typeof(List&lt;GasEmission&gt;))</example>
        public ExtensionSchema(string section, string targetTable, Type dtoType,
                               Func<object, bool>? validate = null)
        {
            if (string.IsNullOrWhiteSpace(section))
                throw new ArgumentException("section must be non-empty", nameof(section));
            Section = section;
            TargetTable = targetTable ?? "";
            DtoType = dtoType ?? throw new ArgumentNullException(nameof(dtoType));
            Validate = validate;
        }
    }

    /// <summary>
    /// Static registry of <see cref="ExtensionSchema"/> declarations. Consumer systems
    /// register their sections here BEFORE the game finishes loading YAML (typically from
    /// their plugin's Load()); the loader only accepts sections that are registered.
    /// Unknown sections found in a mod's sdk.yaml are logged as WARNING (the mod may
    /// target an optional SDK system that is not installed).
    /// </summary>
    /// <example>
    /// ExtensionSchemaRegistry.Register(new ExtensionSchema("multiOutput", "building", typeof(MultiOutputConfig)));
    /// bool known = ExtensionSchemaRegistry.TryGet("multiOutput", out var schema);
    /// </example>
    public static class ExtensionSchemaRegistry
    {
        private static readonly Dictionary<string, ExtensionSchema> _schemas = new();
        private static readonly object _lock = new();

        /// <summary>
        /// Registers a section schema. Re-registering the same section replaces the
        /// previous schema (last writer wins, logged).
        /// </summary>
        /// <param name="schema">The schema to register.</param>
        /// <example>ExtensionSchemaRegistry.Register(new ExtensionSchema("gases", "resource", typeof(GasDefinition)));</example>
        public static void Register(ExtensionSchema schema)
        {
            if (schema == null) throw new ArgumentNullException(nameof(schema));
            lock (_lock)
            {
                if (_schemas.ContainsKey(schema.Section))
                    YamlExtensionsLog.Warning($"Section '{schema.Section}' re-registered — previous schema replaced.");
                _schemas[schema.Section] = schema;
            }
        }

        /// <summary>Looks up the schema of a section.</summary>
        /// <param name="section">Section name.</param>
        /// <param name="schema">The schema if found.</param>
        /// <example>if (ExtensionSchemaRegistry.TryGet("multiOutput", out var s)) { … }</example>
        public static bool TryGet(string section, out ExtensionSchema schema)
        {
            lock (_lock)
            {
#pragma warning disable CS8601
                return _schemas.TryGetValue(section, out schema);
#pragma warning restore CS8601
            }
        }

        /// <summary>All registered schemas (snapshot).</summary>
        /// <example>foreach (var s in ExtensionSchemaRegistry.All) { … }</example>
        public static IReadOnlyList<ExtensionSchema> All
        {
            get { lock (_lock) { return _schemas.Values.ToList(); } }
        }
    }
}
