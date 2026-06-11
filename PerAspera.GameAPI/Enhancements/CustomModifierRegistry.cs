using System.Collections.Generic;
using PerAspera.Core;

namespace PerAspera.GameAPI.Enhancements
{
    /// <summary>
    /// Registry for custom enhancement modifier handlers.
    /// Mods register handlers for YAML modifier keys unknown to the native game.
    /// When <see cref="EnhancementsEnablePatch"/> fires on <c>Enhancements.Enable()</c>,
    /// it iterates the <c>EnhancementType.modifiers</c> strings, and for each key found here
    /// invokes the registered delegate with the parsed float value.
    /// </summary>
    /// <example>
    /// // In your plugin Load():
    /// CustomModifierRegistry.Register("drone_hop_bonus", delta =>
    /// {
    ///     RoutingPatch.ExtraHopCapacity += (int)delta;
    ///     LogAspera.Info($"[MkAspera] drone_hop_bonus applied: {delta}");
    /// });
    /// </example>
    public static class CustomModifierRegistry
    {
        private static readonly LogAspera _log = new LogAspera("GameAPI.Enhancements.CustomModifierRegistry");
        private static readonly object _lock = new();
        private static readonly Dictionary<string, Action<float>> _handlers = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>All currently registered modifier keys (case-insensitive).</summary>
        public static IReadOnlyCollection<string> RegisteredKeys
        {
            get { lock (_lock) { return new List<string>(_handlers.Keys); } }
        }

        /// <summary>
        /// Register a handler for a custom modifier key.
        /// Replaces any existing handler for the same key.
        /// </summary>
        /// <param name="key">Modifier key as declared in YAML (case-insensitive).</param>
        /// <param name="handler">Delegate called with the parsed float value when an enhancement bearing this key is enabled.</param>
        /// <example>
        /// CustomModifierRegistry.Register("ami_routing_modifier", delta =>
        ///     RoutingPatch.ExtraHopCapacity += (int)delta);
        /// </example>
        public static void Register(string key, Action<float> handler)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            if (handler == null) return;
            lock (_lock) { _handlers[key] = handler; }
            _log.Info($"Registered custom modifier handler: '{key}'");
        }

        /// <summary>Removes the handler for <paramref name="key"/>, if any.</summary>
        public static void Unregister(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return;
            lock (_lock) { _handlers.Remove(key); }
        }

        /// <summary>Returns true if a handler is registered for <paramref name="key"/>.</summary>
        public static bool IsRegistered(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return false;
            lock (_lock) { return _handlers.ContainsKey(key); }
        }

        /// <summary>
        /// Attempt to invoke the handler registered for <paramref name="key"/>.
        /// </summary>
        /// <returns>True if a handler was found and invoked, false if key is unknown.</returns>
        /// <example>
        /// // Used internally by EnhancementsEnablePatch — you normally don't call this directly.
        /// bool handled = CustomModifierRegistry.TryInvoke("drone_hop_bonus", 2f);
        /// </example>
        public static bool TryInvoke(string key, float value)
        {
            Action<float>? handler;
            lock (_lock) { _handlers.TryGetValue(key, out handler); }

            if (handler == null) return false;

            try
            {
                handler(value);
                _log.Debug($"Custom modifier '{key}' = {value} → handler invoked");
                return true;
            }
            catch (Exception ex)
            {
                _log.Error($"Custom modifier handler '{key}' threw: {ex.Message}");
                return false;
            }
        }

        /// <summary>Removes all registered handlers. Intended for tests or plugin teardown.</summary>
        public static void Clear()
        {
            lock (_lock) { _handlers.Clear(); }
        }
    }
}
