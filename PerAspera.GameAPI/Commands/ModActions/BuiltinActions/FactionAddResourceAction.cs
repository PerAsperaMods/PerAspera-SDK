using System;
using System.Linq;
using System.Reflection;
using PerAspera.Core;
using PerAspera.GameAPI.Commands.ModActions.BuiltinActions;
using PerAspera.GameAPI.Events.SDK;
using PerAspera.GameAPI.Wrappers;


#pragma warning disable CS1591
namespace PerAspera.GameAPI.Commands.ModActions.BuiltinActions
{
    /// <summary>
    /// Adds a resource directly to the player faction via Faction.AddResource(ResourceType, int).
    /// Uses the proper game API — no console required.
    ///
    /// YAML usage:
    /// <code>
    /// - command: AddResource
    ///   arguments:
    ///     - "resource_iron"   # resource key (required)
    ///     - "500"             # amount (int, required, must be > 0)
    /// </code>
    /// </summary>
    public class FactionAddResourceAction : IModTextAction
    {
        private static readonly LogAspera _log = new LogAspera("FactionAddResource");

        // Cached once — Faction type and method never change after game loads.
        private static MethodInfo? _addResourceMethod;
        private static bool _methodSearched;

        public string CommandName => "AddResource";

        public bool Execute(string[] args, GameCommandsReadyEvent? ctx)
        {
            if (!ActionContextHelper.TryGetString(args, 0, out string resourceKey, _log, CommandName))
                return false;
            if (!ActionContextHelper.TryGetPositiveFloat(args, 1, out float amountF, _log, CommandName))
                return false;

            if (!ActionContextHelper.TryGetFaction(ctx, out var faction, _log, CommandName))
                return false;

            // Lookup the native ResourceType by key
            var nativeResourceType = KeeperTypeRegistry.GetResourceType(resourceKey);
            if (nativeResourceType == null)
            {
                _log.Warning($"[{CommandName}] ResourceType not found: '{resourceKey}'");
                return false;
            }

            // Use typed Faction.AddResource(ResourceType, int) — InteropDump (RS0030-clean)
            if (faction is Faction nativeFaction && nativeResourceType is ResourceType rt)
            {
                nativeFaction.AddResource(rt, (int)amountF);
                _log.Info($"[{CommandName}] Added {(int)amountF}x '{resourceKey}'");
                return true;
            }

            // Fallback via IL2CppExtensions.InvokeMethod — RS0030-exempt (Core)
            faction.InvokeMethod("AddResource", nativeResourceType, (int)amountF);
            _log.Info($"[{CommandName}] Added {(int)amountF}x '{resourceKey}'");
            return true;
        }
    }
}
#pragma warning restore CS1591
