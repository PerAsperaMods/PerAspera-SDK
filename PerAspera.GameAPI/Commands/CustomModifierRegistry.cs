using System;
using System.Collections.Generic;
using PerAspera.Core;

namespace PerAspera.GameAPI.Commands
{
    /// <summary>
    /// Handler delegate for custom YAML modifiers registered by mods.
    /// Called every time an enhancement containing this modifier key is unlocked (Enable called).
    /// </summary>
    /// <param name="modifierName">The modifier key from YAML (e.g. "drone_hop_capacity")</param>
    /// <param name="deltaValue">The float value from this enhancement's modifier entry (e.g. 2.0)</param>
    /// <example>
    /// <code>
    /// Commands.RegisterModifier("drone_hop_capacity", (name, delta) =>
    /// {
    ///     RoutingPatch.HopCapacity += (int)delta;
    ///     Log.Info($"drone_hop_capacity increased by {delta} → total {RoutingPatch.HopCapacity}");
    /// });
    /// </code>
    /// </example>
    public delegate void CustomModifierHandler(string modifierName, float deltaValue);

    /// <summary>
    /// Registry for custom YAML enhancement modifier handlers registered by mods.
    /// Checked by <see cref="Patches.EnhancementsEnablePatch"/> after native Enable() runs.
    ///
    /// Usage mirrors <see cref="CustomCommandRegistry"/>:
    ///  1. Register a handler in your plugin Load().
    ///  2. Declare the modifier key in your enhancements.yaml.
    ///  3. The handler is called each time an enhancement with that key is enabled.
    /// </summary>
    /// <example>
    /// <code>
    /// // enhancements.yaml
    /// enhancement_routing_optimization_1:
    ///   name: enhancement_routing_opt_name
    ///   description: enhancement_routing_opt_desc
    ///   iconName: Sprite/ICO_routing.png
    ///   modifiers:
    ///     - "drone_hop_capacity: 1"
    ///
    /// // Plugin.cs
    /// Commands.RegisterModifier("drone_hop_capacity", (name, delta) => {
    ///     RoutingPatch.ExtraHopCapacity += (int)delta;
    /// });
    /// </code>
    /// </example>
    public static class CustomModifierRegistry
    {
        private static readonly LogAspera _log = new LogAspera("CustomModifierRegistry");

        private static readonly Dictionary<string, CustomModifierHandler> _handlers =
            new Dictionary<string, CustomModifierHandler>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Register a C# handler for a custom YAML modifier key.
        /// If a handler already exists for this key, it is replaced.
        /// </summary>
        /// <param name="modifierKey">Modifier key as it appears in YAML (case-insensitive, e.g. "drone_hop_capacity")</param>
        /// <param name="handler">Handler called each time an enhancement with this key is enabled</param>
        public static void Register(string modifierKey, CustomModifierHandler handler)
        {
            if (string.IsNullOrWhiteSpace(modifierKey))
                throw new ArgumentNullException(nameof(modifierKey));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _handlers[modifierKey] = handler;
            _log.Info($"✅ Registered custom handler for modifier '{modifierKey}'");
        }

        /// <summary>
        /// Unregister a previously registered handler.
        /// </summary>
        public static void Unregister(string modifierKey)
        {
            if (_handlers.Remove(modifierKey))
                _log.Info($"Unregistered custom handler for modifier '{modifierKey}'");
        }

        /// <summary>
        /// Returns true if a handler is registered for this modifier key.
        /// </summary>
        public static bool IsRegistered(string modifierKey) =>
            !string.IsNullOrWhiteSpace(modifierKey) && _handlers.ContainsKey(modifierKey);

        /// <summary>
        /// All currently registered custom modifier keys.
        /// </summary>
        public static IReadOnlyCollection<string> RegisteredModifiers => _handlers.Keys;

        /// <summary>
        /// Try to invoke the registered handler for a given modifier key + delta value.
        /// Returns false if no handler is registered or if the handler throws.
        /// </summary>
        internal static bool TryInvoke(string modifierKey, float deltaValue)
        {
            if (!_handlers.TryGetValue(modifierKey, out var handler))
                return false;

            try
            {
                handler(modifierKey, deltaValue);
                return true;
            }
            catch (Exception ex)
            {
                _log.Warning($"[CustomModifierRegistry] Handler for '{modifierKey}' threw: {ex.Message}");
                return false;
            }
        }
    }
}
