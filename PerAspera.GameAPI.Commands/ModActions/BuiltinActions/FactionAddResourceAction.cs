using System;
using System.Linq;
using System.Reflection;
using PerAspera.Core;
using PerAspera.GameAPI.Commands.ModActions.BuiltinActions;
using PerAspera.GameAPI.Events.SDK;
using PerAspera.GameAPI.Wrappers;

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

            // Cache Faction.AddResource(ResourceType, int) on first call
            if (!_methodSearched)
            {
                _methodSearched = true;
                var factionType = faction!.GetType();

                // Try exact signature first
                _addResourceMethod = factionType.GetMethod("AddResource",
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    null, new[] { nativeResourceType.GetType(), typeof(int) }, null);

                // Fallback: any 2-param overload named AddResource
                if (_addResourceMethod == null)
                {
                    _addResourceMethod = factionType
                        .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .FirstOrDefault(m => m.Name == "AddResource" && m.GetParameters().Length == 2);
                }

                if (_addResourceMethod == null)
                    _log.Warning($"[{CommandName}] Faction.AddResource(ResourceType, int) not found on {factionType.Name}");
                else
                    _log.Info($"[{CommandName}] Faction.AddResource ready (found on {factionType.Name})");
            }

            if (_addResourceMethod == null)
                return false;

            _addResourceMethod.Invoke(faction, new object[] { nativeResourceType, (int)amountF });
            _log.Info($"[{CommandName}] Added {(int)amountF}x '{resourceKey}'");
            return true;
        }
    }
}
