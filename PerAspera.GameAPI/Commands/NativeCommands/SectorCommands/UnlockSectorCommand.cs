using System;
using System.Collections.Generic;
using PerAspera.GameAPI.Commands.Core;
using PerAspera.GameAPI.Commands.Constants;

#pragma warning disable CS1591
namespace PerAspera.GameAPI.Commands.NativeCommands.SectorCommands
{
    /// <summary>
    /// Unlocks a sector for a faction — wraps native CmdFactionUnlockSector
    /// Grants the faction access to build and operate in the specified sector
    /// </summary>
    /// <example>
    /// <code>
    /// // Unlock sector 3 for player faction
    /// Commands.ForFaction(playerFaction).UnlockSector(3).Execute();
    ///
    /// // YAML equivalent:
    /// // actions:
    /// //   - command: UnlockSector
    /// //     arguments:
    /// //       - PlayerFaction
    /// //       - 3
    /// </code>
    /// </example>
    public class UnlockSectorCommand : GameCommandBase
    {
        /// <summary>The faction that will gain access to the sector</summary>
        public override object Faction { get; }

        public override string CommandType => NativeCommandTypes.UnlockSector;

        /// <summary>Zero-based index of the sector to unlock</summary>
        public int SectorIndex { get; }

        /// <summary>
        /// Create a new UnlockSector command
        /// </summary>
        /// <param name="faction">Faction to unlock sector for (e.g. "PlayerFaction" or native Faction object)</param>
        /// <param name="sectorIndex">Index of the sector to unlock (0-based)</param>
        public UnlockSectorCommand(object faction, int sectorIndex)
            : base(NativeCommandTypes.UnlockSector)
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
            return $"Unlock sector {SectorIndex} for faction {Faction}";
        }

        public static UnlockSectorCommand FromParameters(Dictionary<string, object> parameters)
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

            return new UnlockSectorCommand(faction, sectorIndex);
        }
    }
}
#pragma warning restore CS1591
