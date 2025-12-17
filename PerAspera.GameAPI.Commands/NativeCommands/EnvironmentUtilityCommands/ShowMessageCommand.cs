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
    }
}
