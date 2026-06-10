using PerAspera.GameAPI.Commands.Helpers;
using PerAspera.GameAPI.Events.SDK;

namespace PerAspera.GameAPI.Commands.ModActions.BuiltinActions
{
    /// <summary>
    /// Clear all resources from faction stockpiles.
    /// Wraps the game's console cheat command ClearStockpiles.
    /// </summary>
    public class ClearStockpilesAction : IModTextAction
    {
        public string CommandName => "ClearStockpiles";

        /// <example>
        /// launchActions:
        ///   - command: ClearStockpiles
        /// </example>
        public bool Execute(string[] args, GameCommandsReadyEvent? ctx)
            => ResourceCommandHelper.ExecuteConsoleCommand("ClearStockpiles");
    }
}
