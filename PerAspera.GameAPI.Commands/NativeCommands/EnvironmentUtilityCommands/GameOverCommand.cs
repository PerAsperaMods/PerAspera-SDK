using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.NativeCommands.EnvironmentUtilityCommands
{
    /// <summary>
    /// Command to trigger game over condition
    /// </summary>
    public class GameOverCommand : GameCommandBase
    {
        /// <summary>
        /// The faction executing the command
        /// </summary>
        public override object Faction { get; }
        
        /// <summary>
        /// The command type identifier
        /// </summary>
        public override string CommandType => "GameOver";
        
        /// <summary>
        /// Reason for game over
        /// </summary>
        public string Reason { get; set; }
        
        /// <summary>
        /// Victory or defeat
        /// </summary>
        public bool IsVictory { get; set; }
        
        /// <summary>
        /// Initialize a new GameOverCommand
        /// </summary>
        public GameOverCommand()
        {
        }
        
        /// <summary>
        /// Validates if the game over command is valid
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public override bool IsValid()
        {
            return Faction != null && !string.IsNullOrEmpty(Reason);
        }
    }
}
