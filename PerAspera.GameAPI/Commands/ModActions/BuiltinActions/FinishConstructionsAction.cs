using PerAspera.GameAPI.Commands.Helpers;
using PerAspera.GameAPI.Events.SDK;


#pragma warning disable CS1591
namespace PerAspera.GameAPI.Commands.ModActions.BuiltinActions
{
    /// <summary>
    /// Instantly complete all pending building constructions.
    /// Wraps the game's console cheat command FinishConstructions.
    /// </summary>
    public class FinishConstructionsAction : IModTextAction
    {
        public string CommandName => "FinishConstructions";

        /// <example>
        /// launchActions:
        ///   - command: FinishConstructions
        /// </example>
        public bool Execute(string[] args, GameCommandsReadyEvent? ctx)
            => ResourceCommandHelper.ExecuteConsoleCommand("FinishConstructions");
    }
}
#pragma warning restore CS1591
