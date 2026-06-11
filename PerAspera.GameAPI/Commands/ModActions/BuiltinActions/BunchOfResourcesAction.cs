using PerAspera.GameAPI.Commands.Helpers;
using PerAspera.GameAPI.Events.SDK;


#pragma warning disable CS1591
namespace PerAspera.GameAPI.Commands.ModActions.BuiltinActions
{
    /// <summary>
    /// Give a large bunch of all basic resources to the faction.
    /// Wraps the game's console cheat command BunchOfResources.
    /// </summary>
    public class BunchOfResourcesAction : IModTextAction
    {
        public string CommandName => "BunchOfResources";

        /// <example>
        /// launchActions:
        ///   - command: BunchOfResources
        /// </example>
        public bool Execute(string[] args, GameCommandsReadyEvent? ctx)
            => ResourceCommandHelper.ExecuteConsoleCommand("BunchOfResources");
    }
}
#pragma warning restore CS1591
