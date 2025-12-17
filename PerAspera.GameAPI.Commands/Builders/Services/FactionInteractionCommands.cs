using System;
using PerAspera.GameAPI.Commands.Constants;
using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.Builders.Services
{
    /// <summary>
    /// Service for interaction and messaging faction commands
    /// Handles dialogues, messages, keeper mode, and environmental sabotage operations
    /// </summary>
    public static class FactionInteractionCommands
    {
        /// <summary>
        /// Create start dialogue command for faction
        /// </summary>
        public static CommandBuilder StartDialogue(object faction, object person, object dialogue, TimeSpan? timeout = null)
        {
            var command = new CommandBuilder(NativeCommandTypes.StartDialogue)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Person, person)
                .WithParameter(ParameterNames.Dialogue, dialogue);
                
            if (timeout.HasValue)
                command.WithTimeout(timeout.Value);
                
            return command;
        }
        
        /// <summary>
        /// Create skip dialogue command for faction
        /// </summary>
        public static CommandBuilder SkipDialogue(object faction, object dialogue, TimeSpan? timeout = null)
        {
            var command = new CommandBuilder(NativeCommandTypes.SkipDialogue)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Dialogue, dialogue);
                
            if (timeout.HasValue)
                command.WithTimeout(timeout.Value);
                
            return command;
        }
        
        /// <summary>
        /// Create enable keeper mode command for faction
        /// </summary>
        public static CommandBuilder EnableKeeperMode(object faction, TimeSpan? timeout = null)
        {
            var command = new CommandBuilder(NativeCommandTypes.EnableKeeperMode)
                .WithFaction(faction);
                
            if (timeout.HasValue)
                command.WithTimeout(timeout.Value);
                
            return command;
        }
        
        /// <summary>
        /// Create disable keeper mode command for faction
        /// </summary>
        public static CommandBuilder DisableKeeperMode(object faction, TimeSpan? timeout = null)
        {
            var command = new CommandBuilder(NativeCommandTypes.DisableKeeperMode)
                .WithFaction(faction);
                
            if (timeout.HasValue)
                command.WithTimeout(timeout.Value);
                
            return command;
        }
        
        /// <summary>
        /// Create sabotage command for faction
        /// </summary>
        public static CommandBuilder Sabotage(object faction, TimeSpan? timeout = null)
        {
            var command = new CommandBuilder(NativeCommandTypes.Sabotage)
                .WithFaction(faction);
                
            if (timeout.HasValue)
                command.WithTimeout(timeout.Value);
                
            return command;
        }
        
        /// <summary>
        /// Create show message command for faction
        /// </summary>
        public static CommandBuilder ShowMessage(object faction, string message, TimeSpan? timeout = null)
        {
            var command = new CommandBuilder(NativeCommandTypes.ShowMessage)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Message, message);
                
            if (timeout.HasValue)
                command.WithTimeout(timeout.Value);
                
            return command;
        }
        
        /// <summary>
        /// Create show tutorial message command for faction
        /// </summary>
        public static CommandBuilder ShowTutorialMessage(object faction, string message, TimeSpan? timeout = null)
        {
            var command = new CommandBuilder(NativeCommandTypes.ShowTutorialMessage)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Message, message);
                
            if (timeout.HasValue)
                command.WithTimeout(timeout.Value);
                
            return command;
        }
    }
}