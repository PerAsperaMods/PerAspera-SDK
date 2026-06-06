using PerAspera.Core;
using PerAspera.GameAPI.Commands.Helpers;
using PerAspera.GameAPI.Events.SDK;

namespace PerAspera.GameAPI.Commands.ModActions.BuiltinActions
{
    /// <summary>
    /// Built-in SDK action: imports any resource into the player faction's storage.
    /// Registered automatically by Commands.Initialize().
    ///
    /// This replaces the native ImportResource console command which only supports
    /// a limited set of gases and uses a different argument order.
    ///
    /// YAML usage:
    /// <code>
    /// launchActions:
    ///   - command: ImportResource
    ///     arguments:
    ///       - "WATER"   # resource key (required)
    ///       - "1000"    # amount (required, must be positive)
    ///     daysDelay: 0.0
    /// </code>
    ///
    /// Arguments:
    ///   [0]  resourceKey — the resource type key as defined in resource.yaml (e.g. WATER, ICE, CHG, NITROGEN, OXYGEN, IRON, SILICON…)
    ///   [1]  amount      — quantity to add (float, required, must be positive)
    ///
    /// Returns false (with a warning) if:
    ///   - resourceKey argument is missing or empty
    ///   - amount argument is missing, not a valid number, or not positive
    ///   - player faction is not available in context
    ///   - the game console is not accessible (game not ready)
    /// </summary>
    public class ImportResourceAction : IModTextAction
    {
        private static readonly LogAspera _log = new LogAspera("ImportResource");

        /// <summary>
        /// The name used in YAML under "command:". Case-insensitive at runtime.
        /// </summary>
        public string CommandName => "ImportResource";

        /// <summary>
        /// Executes the action: imports the specified resource into the player faction's storage.
        /// </summary>
        /// <param name="args">args[0] = resourceKey (string), args[1] = amount (float, positive)</param>
        /// <param name="ctx">Game context — provides faction name for the console command</param>
        /// <returns>true on success, false on any validation or execution failure</returns>
        public bool Execute(string[] args, GameCommandsReadyEvent? ctx)
        {
            // ── 1. Parse resource key ─────────────────────────────────────────
            if (!ActionContextHelper.TryGetString(args, 0, out string resourceKey, _log, CommandName))
                return false;

            // ── 2. Parse amount ───────────────────────────────────────────────
            if (!ActionContextHelper.TryGetPositiveFloat(args, 1, out float amount, _log, CommandName))
                return false;

            // ── 3. Get player faction name ────────────────────────────────────
            if (!ActionContextHelper.TryGetFaction(ctx, out var faction, _log, CommandName))
                return false;

            string factionName = faction!.name ?? "PlayerFaction";

            // ── 4. Execute via ResourceCommandHelper ──────────────────────────
            bool ok = ResourceCommandHelper.ExecuteResourceImportCommand(factionName, resourceKey, amount);

            if (ok)
                _log.Info($"[{CommandName}] Imported {amount}x '{resourceKey}' into '{factionName}'");
            else
                _log.Warning($"[{CommandName}] Failed to import '{resourceKey}' — see ResourceCommandHelper logs above");

            return ok;
        }
    }
}
