using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.NativeCommands.ResourceManagementCommands
{
    /// <summary>
    /// Command to set specific resource amounts for factions
    /// </summary>
    public class SetResourceAmountCommand : GameCommandBase
    {
        /// <summary>
        /// The faction executing the command
        /// </summary>
        public override object Faction { get; }
        
        /// <summary>
        /// The command type identifier
        /// </summary>
        public override string CommandType => "SetResourceAmount";
        
        /// <summary>
        /// Resource type to set
        /// </summary>
        public object ResourceType { get; set; }
        
        /// <summary>
        /// New amount to set
        /// </summary>
        public int Amount { get; set; }
        
        /// <summary>
        /// Whether to add to existing amount or replace
        /// </summary>
        public bool AddToExisting { get; set; }
        
        /// <summary>
        /// Initialize a new SetResourceAmountCommand
        /// </summary>
        public SetResourceAmountCommand()
        {
        }
        
        /// <summary>
        /// Validates if the set resource amount command is valid
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public override bool IsValid()
        {
            return Faction != null && ResourceType != null && Amount >= 0;
        }
    }
}
