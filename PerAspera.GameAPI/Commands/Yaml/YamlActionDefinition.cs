using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace PerAspera.GameAPI.Commands.Yaml
{
    /// <summary>
    /// Represents a single action entry in a YAML actions block.
    /// Matches the native TextAction format used by technology.yaml and rule.yaml.
    /// </summary>
    /// <example>
    /// <code>
    /// # In technology.yaml or a custom actions.yaml:
    /// actions:
    ///   - command: UnlockBuilding
    ///     arguments:
    ///       - PlayerFaction
    ///       - building_water_mine
    ///     daysDelay: 0.0
    ///     showInFrontend: false
    ///   - command: FinishConstructions
    ///     arguments: []
    ///     daysDelay: 0.0
    ///     showInFrontend: false
    /// </code>
    /// </example>
    public class YamlActionDefinition
    {
        /// <summary>
        /// Console command name (e.g. "UnlockBuilding", "FinishConstructions", "SetEngineTimescale").
        /// Case-insensitive match with native game console commands.
        /// </summary>
        [YamlMember(Alias = "command")]
        public string Command { get; set; } = string.Empty;

        /// <summary>
        /// Ordered list of arguments passed to the command.
        /// Matches the native TextAction.arguments field.
        /// </summary>
        [YamlMember(Alias = "arguments")]
        public List<string> Arguments { get; set; } = new List<string>();

        /// <summary>
        /// Delay in in-game days before this action executes.
        /// 0.0 means execute immediately.
        /// </summary>
        [YamlMember(Alias = "daysDelay")]
        public float DaysDelay { get; set; } = 0f;

        /// <summary>
        /// Whether this action should be visible in the game frontend UI.
        /// </summary>
        [YamlMember(Alias = "showInFrontend")]
        public bool ShowInFrontend { get; set; } = false;

        /// <summary>
        /// Optional human-readable label for debugging and logging.
        /// Not used by the game engine.
        /// </summary>
        [YamlMember(Alias = "label")]
        public string? Label { get; set; }

        /// <summary>
        /// Returns a short string representation for logging purposes.
        /// </summary>
        public override string ToString()
        {
            var args = Arguments != null && Arguments.Count > 0
                ? string.Join(", ", Arguments)
                : "(no args)";
            return $"{Command}({args}) [delay={DaysDelay}]";
        }
    }

    /// <summary>
    /// Represents a standalone YAML file containing a list of actions to execute.
    /// Use when you want to define actions in a dedicated file (e.g. startup-actions.yaml).
    /// </summary>
    /// <example>
    /// <code>
    /// # startup-actions.yaml
    /// actions:
    ///   - command: FinishConstructions
    ///     arguments: []
    ///     daysDelay: 0.0
    ///     showInFrontend: false
    ///   - command: ImportResource
    ///     arguments:
    ///       - PlayerFaction
    ///       - Water
    ///       - "1000"
    ///     daysDelay: 0.0
    ///     showInFrontend: false
    /// </code>
    /// </example>
    public class YamlActionsFile
    {
        /// <summary>
        /// List of actions to execute.
        /// </summary>
        [YamlMember(Alias = "actions")]
        public List<YamlActionDefinition> Actions { get; set; } = new List<YamlActionDefinition>();
    }
}
