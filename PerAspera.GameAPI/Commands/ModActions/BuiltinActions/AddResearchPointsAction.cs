#pragma warning disable CS1591
using PerAspera.Core;
using PerAspera.GameAPI.Events.SDK;
using PerAspera.GameAPI.Wrappers;

namespace PerAspera.GameAPI.Commands.ModActions.BuiltinActions
{
    /// <summary>
    /// Adds research points directly to the player faction (not tied to current technology).
    /// <example>
    /// launchActions:
    ///   - command: AddResearchPoints
    ///     arguments: ["500"]
    /// </example>
    /// </summary>
    public class AddResearchPointsAction : IModTextAction
    {
        private static readonly LogAspera _log = new LogAspera("AddResearchPoints");
        public string CommandName => "AddResearchPoints";

        public bool Execute(string[] args, GameCommandsReadyEvent? ctx)
        {
            if (!ActionContextHelper.TryGetPositiveFloat(args, 0, out float amount, _log, CommandName))
                return false;
            if (!ActionContextHelper.TryGetFaction(ctx, out var nativeFaction, _log, CommandName))
                return false;

            var faction = FactionWrapper.FromNative(nativeFaction);
            bool ok = faction?.AddResearchPoints(amount) ?? false;
            if (ok) _log.Info($"[{CommandName}] +{amount} pts → '{nativeFaction!.name}'");
            return ok;
        }
    }
}
#pragma warning restore CS1591
