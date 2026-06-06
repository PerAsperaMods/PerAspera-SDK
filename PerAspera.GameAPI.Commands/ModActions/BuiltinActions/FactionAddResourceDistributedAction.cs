using PerAspera.Core;
using PerAspera.GameAPI.Commands.Helpers;
using PerAspera.GameAPI.Events.SDK;

namespace PerAspera.GameAPI.Commands.ModActions.BuiltinActions
{
    /// <summary>
    /// Distribute a resource amount across all faction stockpiles.
    /// Wraps the game's console cheat command FactionAddResourceDistributed.
    /// </summary>
    public class FactionAddResourceDistributedAction : IModTextAction
    {
        private static readonly LogAspera _log = new LogAspera("FactionAddResourceDistributed");

        public string CommandName => "FactionAddResourceDistributed";

        /// <example>
        /// launchActions:
        ///   - command: FactionAddResourceDistributed
        ///     arguments: ["water", "500"]
        /// </example>
        public bool Execute(string[] args, GameCommandsReadyEvent? ctx)
        {
            if (!ActionContextHelper.TryGetString(args, 0, out var resourceKey, _log, CommandName))
                return false;

            if (!ActionContextHelper.TryGetPositiveFloat(args, 1, out var amount, _log, CommandName))
                return false;

            return ResourceCommandHelper.ExecuteResourceDistributedCommand(resourceKey, amount);
        }
    }
}
