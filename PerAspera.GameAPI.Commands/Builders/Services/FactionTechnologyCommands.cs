using System;
using PerAspera.GameAPI.Commands.Constants;
using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.Builders.Services
{
    /// <summary>
    /// Service for technology and knowledge-related faction commands
    /// Handles technology research, knowledge unlocking, and research progress management
    /// </summary>
    public static class FactionTechnologyCommands
    {
        /// <summary>
        /// Create research technology command for faction
        /// </summary>
        public static CommandBuilder ResearchTechnology(object faction, object technology, TimeSpan? timeout = null)
        {
            var command = new CommandBuilder(NativeCommandTypes.ResearchTechnology)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Technology, technology);
                
            if (timeout.HasValue)
                command.WithTimeout(timeout.Value);
                
            return command;
        }
        
        /// <summary>
        /// Create unlock knowledge command for faction
        /// </summary>
        public static CommandBuilder UnlockKnowledge(object faction, object knowledge, TimeSpan? timeout = null)
        {
            var command = new CommandBuilder(NativeCommandTypes.UnlockKnowledge)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Knowledge, knowledge);
                
            if (timeout.HasValue)
                command.WithTimeout(timeout.Value);
                
            return command;
        }
        
        /// <summary>
        /// Create lock knowledge command for faction
        /// </summary>
        public static CommandBuilder LockKnowledge(object faction, object knowledge, TimeSpan? timeout = null)
        {
            var command = new CommandBuilder(NativeCommandTypes.LockKnowledge)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Knowledge, knowledge);
                
            if (timeout.HasValue)
                command.WithTimeout(timeout.Value);
                
            return command;
        }
    }
}