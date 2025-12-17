using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.NativeCommands.EnvironmentUtilityCommands
{
    /// <summary>
    /// Command to set override values for game systems
    /// </summary>
    public class SetOverrideCommand : GameCommandBase
    {
        /// <summary>
        /// The faction executing the command
        /// </summary>
        public override object Faction { get; }
        
        /// <summary>
        /// The command type identifier
        /// </summary>
        public override string CommandType => "SetOverride";
        
        /// <summary>
        /// System to override
        /// </summary>
        public string SystemName { get; set; }
        
        /// <summary>
        /// Override value
        /// </summary>
        public object OverrideValue { get; set; }
        
        /// <summary>
        /// Initialize a new SetOverrideCommand
        /// </summary>
        public SetOverrideCommand()
        {
        }
        
        /// <summary>
        /// Validates if the set override command is valid
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public override bool IsValid()
        {
            return Faction != null && !string.IsNullOrEmpty(SystemName);
        }
    }
}