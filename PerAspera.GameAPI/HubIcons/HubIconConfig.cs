using System.Collections.Generic;

namespace PerAspera.GameAPI.HubIcons
{
    /// <summary>
    /// DTO of one item of the <c>hubIcons</c> section in a mod's <c>sdk.yaml</c>: the worker-hub
    /// icon sprites a building type shows per active-drone count. Generic for any
    /// <c>droneCapacity</c> N — declare one entry per state you want to override (0..N). States
    /// you omit keep the game's native icon (empty / full).
    /// </summary>
    /// <example>
    /// # Mods/MonMod/sdk.yaml — chemins relatifs au dossier du mod
    /// extensions:
    ///   hubIcons:
    ///     building_drone_base_2:
    ///       icons:
    ///         1: Sprite/worker/2/Icon_WorkerRelay_2_1_2.png
    ///     building_drone_base_3:
    ///       icons:
    ///         1: Sprite/worker/3/Icon_WorkerRelay_3_1_3.png
    ///         2: Sprite/worker/3/Icon_WorkerRelay_3_2_3.png
    /// </example>
    public sealed class HubIconConfig
    {
        /// <summary>
        /// Map of active-drone count → sprite path (relative to the providing mod's folder).
        /// Key 0 = empty override, key N = full override, 1..N-1 = intermediate states.
        /// </summary>
        public Dictionary<int, string> Icons { get; set; } = new();
    }
}
