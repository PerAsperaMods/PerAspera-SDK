using System;
using PerAspera.GameAPI.Commands.Constants;
using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.Builders.Services
{
    /// <summary>
    /// Service for building-related faction commands
    /// Handles building unlocking, locking, placement, and removal operations
    /// </summary>
    public static class FactionBuildingCommands
    {
        /// <summary>
        /// Create unlock building command for faction
        /// </summary>
        public static CommandBuilder UnlockBuilding(object faction, object building, TimeSpan? timeout = null)
        {
            var command = new CommandBuilder(NativeCommandTypes.UnlockBuilding)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Building, building);
                
            if (timeout.HasValue)
                command.WithTimeout(timeout.Value);
                
            return command;
        }
        
        /// <summary>
        /// Create lock building command for faction
        /// </summary>
        public static CommandBuilder LockBuilding(object faction, object building, TimeSpan? timeout = null)
        {
            var command = new CommandBuilder(NativeCommandTypes.LockBuilding)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Building, building);
                
            if (timeout.HasValue)
                command.WithTimeout(timeout.Value);
                
            return command;
        }
        
        /// <summary>
        /// Create add building command for faction with position
        /// </summary>
        public static CommandBuilder AddBuilding(object faction, object building, float x, float y, float z, TimeSpan? timeout = null)
        {
            var command = new CommandBuilder(NativeCommandTypes.AddBuilding)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Building, building)
                .WithParameter(ParameterNames.X, x)
                .WithParameter(ParameterNames.Y, y)
                .WithParameter(ParameterNames.Z, z);
                
            if (timeout.HasValue)
                command.WithTimeout(timeout.Value);
                
            return command;
        }
        
        /// <summary>
        /// Create remove building command for faction
        /// </summary>
        public static CommandBuilder RemoveBuilding(object faction, object building, TimeSpan? timeout = null)
        {
            var command = new CommandBuilder(NativeCommandTypes.RemoveBuilding)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Building, building);
                
            if (timeout.HasValue)
                command.WithTimeout(timeout.Value);
                
            return command;
        }
    }
}