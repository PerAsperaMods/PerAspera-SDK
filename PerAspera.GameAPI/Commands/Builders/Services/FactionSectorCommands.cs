using System;
using PerAspera.GameAPI.Commands.Constants;
using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.Builders.Services
{
    /// <summary>
    /// Service for sector-related faction commands
    /// Handles sector unlocking and locking (satellite loss, expansion, etc.)
    /// </summary>
    public static class FactionSectorCommands
    {
        /// <summary>
        /// Create an UnlockSector command — wraps native CmdFactionUnlockSector
        /// </summary>
        /// <param name="faction">Faction gaining access to the sector</param>
        /// <param name="sectorIndex">Sector index to unlock (0-based)</param>
        /// <param name="timeout">Optional execution timeout</param>
        /// <example>
        /// <code>
        /// // Unlock sectors 1 and 2 for the player
        /// Commands.ForFaction(playerFaction)
        ///     .UnlockSector(1)
        ///     .UnlockSector(2)
        ///     .Execute();
        /// </code>
        /// </example>
        public static CommandBuilder UnlockSector(object faction, int sectorIndex, TimeSpan? timeout = null)
        {
            var command = new CommandBuilder(NativeCommandTypes.UnlockSector)
                .WithFaction(faction)
                .WithParameter(ParameterNames.SectorIndex, sectorIndex);

            if (timeout.HasValue)
                command.WithTimeout(timeout.Value);

            return command;
        }

        /// <summary>
        /// Create a LockSector command — custom SDK command, no native equivalent
        /// Sets Sector.enabled = false so IsInsideEnabledSectors() blocks construction
        /// </summary>
        /// <param name="faction">Faction losing access to the sector</param>
        /// <param name="sectorIndex">Sector index to lock (0-based)</param>
        /// <param name="timeout">Optional execution timeout</param>
        /// <example>
        /// <code>
        /// // Lock sector 3 when a satellite is compromised
        /// Commands.ForFaction(playerFaction).LockSector(3).Execute();
        ///
        /// // Restore access after recovery
        /// Commands.ForFaction(playerFaction).UnlockSector(3).Execute();
        /// </code>
        /// </example>
        public static CommandBuilder LockSector(object faction, int sectorIndex, TimeSpan? timeout = null)
        {
            var command = new CommandBuilder(NativeCommandTypes.LockSector)
                .WithFaction(faction)
                .WithParameter(ParameterNames.SectorIndex, sectorIndex);

            if (timeout.HasValue)
                command.WithTimeout(timeout.Value);

            return command;
        }
    }
}
