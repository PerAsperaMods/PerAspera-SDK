using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.NativeCommands.EnvironmentUtilityCommands
{
    /// <summary>
    /// Command to show messages to players
    /// </summary>
    public class ShowMessageCommand : GameCommandBase
    {
        /// <summary>
        /// The faction executing the command
        /// </summary>
        public override object Faction { get; }
        
        /// <summary>
        /// The command type identifier
        /// </summary>
        public override string CommandType => "ShowMessage";
        
        /// <summary>
        /// Message text to display
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// Message title
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Duration to show the message
        /// </summary>
        public float Duration { get; set; }
        
        /// <summary>
        /// Initialize a new ShowMessageCommand
        /// </summary>
        public ShowMessageCommand()
        {
            Duration = 5.0f;
        }
        
        /// <summary>
        /// Validates if the show message command is valid
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public override bool IsValid()
        {
            return Faction != null && !string.IsNullOrEmpty(Message) && Duration > 0;
        }
        /// <summary>
        /// Create ShowMessageCommand from parameters array
        /// </summary>
        public static ShowMessageCommand FromParameters(object[] parameters)
        {
            var command = new ShowMessageCommand();
            
            if (parameters?.Length >= 2)
            {
                command.Faction = parameters[0];
                command.Message = parameters[1]?.ToString();
                
                if (parameters.Length > 2 && float.TryParse(parameters[2]?.ToString(), out var duration))
                {
                    command.Duration = duration;
                }
                
                if (parameters.Length > 3)
                {
                    command.MessageType = parameters[3]?.ToString();
                }
            }
            
            return command;
        }
    }
}
