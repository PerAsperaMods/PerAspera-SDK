using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.NativeCommands.EnvironmentUtilityCommands
{
    /// <summary>
    /// Command to show tutorial messages
    /// </summary>
    public class ShowTutorialMessageCommand : GameCommandBase
    {
        /// <summary>
        /// The faction executing the command
        /// </summary>
        public override object Faction { get; }
        
        /// <summary>
        /// The command type identifier
        /// </summary>
        public override string CommandType => "ShowTutorialMessage";
        
        /// <summary>
        /// Tutorial message ID
        /// </summary>
        public string TutorialId { get; set; }
        
        /// <summary>
        /// Tutorial step number
        /// </summary>
        public int Step { get; set; }
        
        /// <summary>
        /// Skip if already shown
        /// </summary>
        public bool SkipIfShown { get; set; }
        
        /// <summary>
        /// Initialize a new ShowTutorialMessageCommand
        /// </summary>
        public ShowTutorialMessageCommand()
        {
            SkipIfShown = true;
        }
        
        /// <summary>
        /// Validates if the show tutorial message command is valid
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public override bool IsValid()
        {
            return Faction != null && !string.IsNullOrEmpty(TutorialId) && Step >= 0;
        }
    }
}
