using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.NativeCommands.EnvironmentUtilityCommands
{
    /// <summary>
    /// Command to sabotage buildings or systems
    /// </summary>
    public class SabotageCommand : GameCommandBase
    {
        /// <summary>
        /// The faction executing the command
        /// </summary>
        public override object Faction { get; }
        
        /// <summary>
        /// The command type identifier
        /// </summary>
        public override string CommandType => "Sabotage";
        
        /// <summary>
        /// Target to sabotage
        /// </summary>
        public string Target { get; set; }
        
        /// <summary>
        /// Type of sabotage
        /// </summary>
        public string SabotageType { get; set; }
        
        /// <summary>
        /// Initialize a new SabotageCommand
        /// </summary>
        public SabotageCommand()
        {
        }
        
        /// <summary>
        /// Validates if the sabotage command is valid
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public override bool IsValid()
        {
            return Faction != null && !string.IsNullOrEmpty(Target) && !string.IsNullOrEmpty(SabotageType);
        }
    }
}