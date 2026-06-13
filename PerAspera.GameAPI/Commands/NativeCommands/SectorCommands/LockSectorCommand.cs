using System;
using System.Collections.Generic;
using PerAspera.GameAPI.Commands.Core;
using PerAspera.GameAPI.Commands.Constants;

#pragma warning disable CS1591
namespace PerAspera.GameAPI.Commands.NativeCommands.SectorCommands
{
    /// <summary>
    /// Locks a sector, preventing any construction or operation within it
    /// Custom SDK command — no native equivalent in Per Aspera.
    /// Implemented via direct IL2CPP manipulation of Sector.enabled = false.
    /// The game's IsInsideEnabledSectors() check will block all placement in locked sectors.
    /// </summary>
    /// <example>
    /// <code>
    /// // Lock sector 3 (e.g. satellite compromised)
    /// Commands.ForFaction(playerFaction).LockSector(3).Execute();
    ///
    /// // YAML (requires SDK plugin active):
    /// // actions:
    /// //   - command: LockSector
    /// //     arguments:
    /// //       - PlayerFaction
    /// //       - 3
    ///
    /// // Re-unlock after recovery:
    /// // actions:
    /// //   - command: UnlockSector
    /// //     arguments:
    /// //       - PlayerFaction
    /// //       - 3
    /// </code>
    /// </example>
    public class LockSectorCommand : GameCommandBase
    {
        /// <summary>The faction whose sector will be locked</summary>
        public override object Faction { get; }

        public override string CommandType => NativeCommandTypes.LockSector;

        /// <summary>Zero-based index of the sector to lock</summary>
        public int SectorIndex { get; }

        /// <summary>
        /// Create a new LockSector command
        /// </summary>
        /// <param name="faction">Faction to lock sector for</param>
        /// <param name="sectorIndex">Index of the sector to lock (0-based, must match a valid unlocked sector)</param>
        public LockSectorCommand(object faction, int sectorIndex)
            : base(NativeCommandTypes.LockSector)
        {
            Faction = faction ?? throw new ArgumentNullException(nameof(faction));
            SectorIndex = sectorIndex;

            Parameters[ParameterNames.Faction] = faction;
            Parameters[ParameterNames.SectorIndex] = sectorIndex;
        }

        public override bool IsValid()
        {
            return Faction != null && SectorIndex >= 0;
        }

        public override string GetDescription()
        {
            return $"Lock sector {SectorIndex} for faction {Faction}";
        }

        public static LockSectorCommand FromParameters(Dictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue(ParameterNames.Faction, out var faction))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Faction}");

            if (!parameters.TryGetValue(ParameterNames.SectorIndex, out var sectorIndexRaw))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.SectorIndex}");

            int sectorIndex = sectorIndexRaw switch
            {
                int i => i,
                string s when int.TryParse(s, out int parsed) => parsed,
                _ => throw new ArgumentException($"Invalid sector index: {sectorIndexRaw}")
            };

            return new LockSectorCommand(faction, sectorIndex);
        }
    }
}
#pragma warning restore CS1591
