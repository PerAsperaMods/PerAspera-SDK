using PerAspera.Core;
using PerAspera.GameAPI.Events.SDK;

namespace PerAspera.GameAPI.Commands.ModActions.BuiltinActions
{
    /// <summary>
    /// Built-in SDK action: gives research points directly to the player faction.
    /// Registered automatically by Commands.Initialize().
    ///
    /// YAML usage:
    /// <code>
    /// launchActions:
    ///   - command: GiveSciencePoints
    ///     arguments: ["500"]
    ///     daysDelay: 0.0
    /// </code>
    ///
    /// Arguments:
    ///   [0] amount — number of research points to add (required, must be positive)
    /// </summary>
    public class GiveSciencePointsAction : IModTextAction
    {
        private static readonly LogAspera _log = new LogAspera("GiveSciencePoints");

        /// <summary>Gets the YAML command name for this action.</summary>
        public string CommandName => "GiveSciencePoints";

        /// <summary>
        /// Adds the specified amount of research points to the player faction.
        /// </summary>
        /// <param name="args">args[0] = amount (float, required, positive)</param>
        /// <param name="ctx">Game context — NativePlayerFaction must be non-null</param>
        /// <returns>True on success, false if arguments are invalid or faction unavailable</returns>
        public bool Execute(string[] args, GameCommandsReadyEvent? ctx)
        {
            if (!ActionContextHelper.TryGetPositiveFloat(args, 0, out float amount, _log, CommandName))
                return false;

            if (!ActionContextHelper.TryGetFaction(ctx, out var faction, _log, CommandName))
                return false;

            Technology? tech = faction!.GetCurrentlyResearchedTechnology()
                               ?? faction.currentTechnologyType;

            if (tech == null)
            {
                _log.Warning($"[GiveSciencePoints] No technology currently being researched by '{faction.name}'. Select a technology to research first.");
                return false;
            }

            tech.AddResearchPoints(amount);
            _log.Info($"[GiveSciencePoints] Added {amount} pts to '{tech.TechnologyType?.key ?? "?"}' on '{faction.name}'");
            return true;
        }
    }
}
