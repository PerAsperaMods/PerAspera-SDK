using PerAspera.GameAPI.Events.SDK;

namespace PerAspera.GameAPI.Commands.ModActions
{
    /// <summary>
    /// Interface for custom mod-defined TextActions executable from YAML.
    /// Implement this interface to create your own game commands that can be
    /// triggered from YAML action files, just like native Per Aspera commands.
    /// 
    /// Register your action with <see cref="ModTextActionRegistry.Register{T}"/> 
    /// before the game starts, typically in your plugin's Load() method.
    /// </summary>
    /// <example>
    /// <code>
    /// public class SpawnMyUnit : IModTextAction
    /// {
    ///     public string CommandName => "SpawnMyUnit";
    ///
    ///     public bool Execute(string[] args, GameCommandsReadyEvent ctx)
    ///     {
    ///         string unitId = args.Length > 0 ? args[0] : "default_unit";
    ///         // use ctx.NativeUniverse, ctx.NativePlayerFaction, etc.
    ///         Log.LogInfo($"Spawning unit {unitId}");
    ///         return true;
    ///     }
    /// }
    ///
    /// // In plugin Load():
    /// ModTextActionRegistry.Register&lt;SpawnMyUnit&gt;();
    ///
    /// // In YAML:
    /// actions:
    ///   - command: SpawnMyUnit
    ///     arguments:
    ///       - my_unit_id
    /// </code>
    /// </example>
    public interface IModTextAction
    {
        /// <summary>
        /// The command name as it appears in YAML (case-insensitive).
        /// Must be unique across all registered actions.
        /// </summary>
        string CommandName { get; }

        /// <summary>
        /// Execute the action.
        /// </summary>
        /// <param name="args">Arguments from the YAML definition (may be empty)</param>
        /// <param name="ctx">Game context — provides access to Universe, Planet, Faction, etc.
        /// Will be null if Initialize() was not called before execution.</param>
        /// <returns>True if the action succeeded, false if it failed</returns>
        bool Execute(string[] args, GameCommandsReadyEvent? ctx);
    }
}
