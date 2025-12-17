using System;
using System.Collections.Generic;
using PerAspera.GameAPI.Commands.Constants;
using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.Builders.Services
{
    /// <summary>
    /// Service for resource-related faction commands
    /// Handles resource import, export, allocation, and vein spawning operations
    /// </summary>
    public static class FactionResourceCommands
    {
        /// <summary>
        /// Create import resource command for faction
        /// </summary>
        public static CommandBuilder ImportResource(object faction, object resource, int quantity, TimeSpan? timeout = null)
        {
            var command = new CommandBuilder(NativeCommandTypes.ImportResource)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Resource, resource)
                .WithParameter(ParameterNames.Quantity, quantity);
                
            if (timeout.HasValue)
                command.WithTimeout(timeout.Value);
                
            return command;
        }
        
        /// <summary>
        /// Create export resource command for faction
        /// </summary>
        public static CommandBuilder ExportResource(object faction, object resource, int quantity, TimeSpan? timeout = null)
        {
            var command = new CommandBuilder(NativeCommandTypes.ExportResource)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Resource, resource)
                .WithParameter(ParameterNames.Quantity, quantity);
                
            if (timeout.HasValue)
                command.WithTimeout(timeout.Value);
                
            return command;
        }
        
        /// <summary>
        /// Create set resource amount command for faction
        /// </summary>
        public static CommandBuilder SetResourceAmount(object faction, object resource, float amount, TimeSpan? timeout = null)
        {
            var command = new CommandBuilder(NativeCommandTypes.SetResourceAmount)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Resource, resource)
                .WithParameter(ParameterNames.Amount, amount);
                
            if (timeout.HasValue)
                command.WithTimeout(timeout.Value);
                
            return command;
        }
        
        /// <summary>
        /// Create spawn resource vein command for faction
        /// </summary>
        public static CommandBuilder SpawnResourceVein(object faction, object resource, float x, float y, float z, TimeSpan? timeout = null)
        {
            var command = new CommandBuilder(NativeCommandTypes.SpawnResourceVein)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Resource, resource)
                .WithParameter(ParameterNames.X, x)
                .WithParameter(ParameterNames.Y, y)
                .WithParameter(ParameterNames.Z, z);
                
            if (timeout.HasValue)
                command.WithTimeout(timeout.Value);
                
            return command;
        }
    }
}