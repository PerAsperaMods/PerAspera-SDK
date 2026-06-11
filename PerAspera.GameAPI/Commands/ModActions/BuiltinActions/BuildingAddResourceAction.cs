using PerAspera.Core;
using PerAspera.GameAPI.Commands.Helpers;
using PerAspera.GameAPI.Events.SDK;


#pragma warning disable CS1591
namespace PerAspera.GameAPI.Commands.ModActions.BuiltinActions
{
    /// <summary>
    /// Add a resource directly to a specific building's storage.
    /// Wraps the game's console cheat command BuildingAddResource.
    /// </summary>
    public class BuildingAddResourceAction : IModTextAction
    {
        private static readonly LogAspera _log = new LogAspera("BuildingAddResource");

        public string CommandName => "BuildingAddResource";

        /// <example>
        /// launchActions:
        ///   - command: BuildingAddResource
        ///     arguments: ["building-12345", "water", "250"]
        /// </example>
        public bool Execute(string[] args, GameCommandsReadyEvent? ctx)
        {
            if (!ActionContextHelper.TryGetString(args, 0, out var buildingId, _log, CommandName))
                return false;

            if (!ActionContextHelper.TryGetString(args, 1, out var resourceKey, _log, CommandName))
                return false;

            if (!ActionContextHelper.TryGetPositiveFloat(args, 2, out var amount, _log, CommandName))
                return false;

            return ResourceCommandHelper.ExecuteBuildingAddResourceCommand(buildingId, resourceKey, amount);
        }
    }
}
#pragma warning restore CS1591
