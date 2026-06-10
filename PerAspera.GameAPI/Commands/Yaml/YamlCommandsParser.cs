using System;
using System.Collections.Generic;
using System.IO;
using PerAspera.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PerAspera.GameAPI.Commands.Yaml
{
    /// <summary>
    /// Parses YAML action definitions from strings or files.
    /// Supports two formats:
    ///   - Inline list: a YAML string/file whose root is a list of action objects
    ///   - Wrapped file: a YAML file with a root "actions:" key (YamlActionsFile format)
    /// </summary>
    /// <example>
    /// <code>
    /// // Parse from raw YAML string
    /// var actions = YamlCommandsParser.ParseString(@"
    ///   - command: FinishConstructions
    ///     arguments: []
    ///     daysDelay: 0.0
    ///     showInFrontend: false
    /// ");
    ///
    /// // Parse from a dedicated actions file
    /// var actions = YamlCommandsParser.ParseFile(@"C:\mods\my-mod\startup-actions.yaml");
    /// </code>
    /// </example>
    public static class YamlCommandsParser
    {
        private static readonly LogAspera _log = new LogAspera("YamlCommandsParser");

        private static IDeserializer BuildDeserializer()
        {
            return new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();
        }

        /// <summary>
        /// Parse a YAML string containing an "actions:" block (YamlActionsFile format).
        /// The root key must be "actions:".
        /// </summary>
        /// <param name="yamlContent">YAML string with root "actions:" key</param>
        /// <returns>List of action definitions, or empty list on failure</returns>
        public static List<YamlActionDefinition> ParseString(string yamlContent)
        {
            if (string.IsNullOrWhiteSpace(yamlContent))
            {
                _log.Warning("ParseString: empty YAML content");
                return new List<YamlActionDefinition>();
            }

            try
            {
                var deserializer = BuildDeserializer();
                var file = deserializer.Deserialize<YamlActionsFile>(yamlContent);
                if (file?.Actions == null || file.Actions.Count == 0)
                {
                    _log.Warning("ParseString: no actions found in YAML content");
                    return new List<YamlActionDefinition>();
                }

                _log.Info($"✅ Parsed {file.Actions.Count} action(s) from YAML string");
                return file.Actions;
            }
            catch (Exception ex)
            {
                _log.Error($"❌ ParseString failed: {ex.Message}");
                return new List<YamlActionDefinition>();
            }
        }

        /// <summary>
        /// Parse a raw YAML list of actions (no root key wrapper).
        /// The YAML root is directly a sequence of action objects.
        /// </summary>
        /// <param name="yamlContent">YAML string where root is a list</param>
        /// <returns>List of action definitions, or empty list on failure</returns>
        public static List<YamlActionDefinition> ParseList(string yamlContent)
        {
            if (string.IsNullOrWhiteSpace(yamlContent))
            {
                _log.Warning("ParseList: empty YAML content");
                return new List<YamlActionDefinition>();
            }

            try
            {
                var deserializer = BuildDeserializer();
                var actions = deserializer.Deserialize<List<YamlActionDefinition>>(yamlContent);
                if (actions == null || actions.Count == 0)
                {
                    _log.Warning("ParseList: no actions found");
                    return new List<YamlActionDefinition>();
                }

                _log.Info($"✅ Parsed {actions.Count} action(s) from YAML list");
                return actions;
            }
            catch (Exception ex)
            {
                _log.Error($"❌ ParseList failed: {ex.Message}");
                return new List<YamlActionDefinition>();
            }
        }

        /// <summary>
        /// Parse a YAML file containing an "actions:" block.
        /// Automatically tries YamlActionsFile format first, then raw list as fallback.
        /// </summary>
        /// <param name="filePath">Absolute path to the YAML file</param>
        /// <returns>List of action definitions, or empty list if file not found or parse fails</returns>
        public static List<YamlActionDefinition> ParseFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                _log.Warning("ParseFile: filePath is null or empty");
                return new List<YamlActionDefinition>();
            }

            if (!File.Exists(filePath))
            {
                _log.Warning($"ParseFile: file not found: {filePath}");
                return new List<YamlActionDefinition>();
            }

            try
            {
                var content = File.ReadAllText(filePath);
                _log.Debug($"ParseFile: reading {filePath}");

                // Try wrapped format first (root "actions:" key)
                var result = ParseString(content);
                if (result.Count > 0)
                    return result;

                // Fallback: try raw list format
                result = ParseList(content);
                if (result.Count > 0)
                    return result;

                _log.Warning($"ParseFile: no actions found in {Path.GetFileName(filePath)}");
                return new List<YamlActionDefinition>();
            }
            catch (IOException ex)
            {
                _log.Error($"❌ ParseFile IO error reading '{filePath}': {ex.Message}");
                return new List<YamlActionDefinition>();
            }
        }
    }
}
