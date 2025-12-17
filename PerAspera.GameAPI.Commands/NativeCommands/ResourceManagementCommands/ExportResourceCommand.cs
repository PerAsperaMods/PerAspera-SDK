using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.NativeCommands.ResourceManagementCommands
{
    /// <summary>
    /// Command to export resources from faction inventory
    /// </summary>
    public class ExportResourceCommand : GameCommandBase
    {
        /// <summary>
        /// The faction executing the command
        /// </summary>
        public override object Faction { get; }
        
        /// <summary>
        /// The command type identifier
        /// </summary>
        public override string CommandType => "ExportResource";
        
        /// <summary>
        /// Resource type to export
        /// </summary>
        public object ResourceType { get; set; }
        
        /// <summary>
        /// Amount to export
        /// </summary>
        public int Amount { get; set; }
        
        /// <summary>
        /// Destination location
        /// </summary>
        public string Destination { get; set; }
        
        /// <summary>
        /// Initialize a new ExportResourceCommand
        /// </summary>
        public ExportResourceCommand()
        {
        }
        
        /// <summary>
        /// Validates if the export resource command is valid
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public override bool IsValid()
        {
            return Faction != null && ResourceType != null && Amount > 0;
        }
    }
}
