using System;
using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.NativeCommands.ResourceManagementCommands
{
    /// <summary>
    /// Command to import resources to faction inventory
    /// </summary>
    public class ImportResourceCommand : GameCommandBase
    {
        /// <summary>
        /// The faction executing the command
        /// </summary>
        public override object Faction { get; }
        
        /// <summary>
        /// The command type identifier
        /// </summary>
        public override string CommandType => "ImportResource";
        
        /// <summary>
        /// Resource type to import
        /// </summary>
        public object ResourceType { get; set; }
        
        /// <summary>
        /// Amount to import
        /// </summary>
        public int Amount { get; set; }
        
        /// <summary>
        /// Source location
        /// </summary>
        public string Source { get; set; }
        
        /// <summary>
        /// Initialize a new ImportResourceCommand
        /// </summary>
        public ImportResourceCommand()
        {
        }
        
        /// <summary>
        /// Validates if the import resource command is valid
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public override bool IsValid()
        {
            return Faction != null && ResourceType != null && Amount > 0;
        }

        /// <summary>
        /// Create ImportResourceCommand from parameters dictionary
        /// </summary>
        public static ImportResourceCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            var command = new ImportResourceCommand();
            
            if (parameters.TryGetValue("Faction", out var faction))
            {
                // Faction property is readonly, handled in constructor
            }
            
            if (parameters.TryGetValue("ResourceType", out var resourceType))
            {
                command.ResourceType = resourceType;
            }
            
            if (parameters.TryGetValue("Amount", out var amount) && int.TryParse(amount?.ToString(), out var amountValue))
            {
                command.Amount = amountValue;
            }
            
            if (parameters.TryGetValue("Source", out var source))
            {
                command.Source = source?.ToString();
            }
            
            return command;
        }
    }
}
