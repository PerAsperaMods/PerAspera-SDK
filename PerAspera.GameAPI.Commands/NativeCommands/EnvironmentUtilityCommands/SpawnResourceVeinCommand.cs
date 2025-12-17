using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.NativeCommands.EnvironmentUtilityCommands
{
    /// <summary>
    /// Command to spawn resource veins on the planet
    /// </summary>
    public class SpawnResourceVeinCommand : GameCommandBase
    {
        /// <summary>
        /// The faction executing the command
        /// </summary>
        public override object Faction { get; }
        
        /// <summary>
        /// The command type identifier
        /// </summary>
        public override string CommandType => "SpawnResourceVein";
        
        /// <summary>
        /// Resource type to spawn
        /// </summary>
        public object ResourceType { get; set; }
        
        /// <summary>
        /// X position coordinate
        /// </summary>
        public float PositionX { get; set; }
        
        /// <summary>
        /// Y position coordinate
        /// </summary>
        public float PositionY { get; set; }
        
        /// <summary>
        /// Amount of resource in the vein
        /// </summary>
        public int Amount { get; set; }
        
        /// <summary>
        /// Initialize a new SpawnResourceVeinCommand
        /// </summary>
        public SpawnResourceVeinCommand()
        {
            Amount = 1000;
        }
        
        /// <summary>
        /// Validates if the spawn resource vein command is valid
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public override bool IsValid()
        {
            return Faction != null && ResourceType != null && Amount > 0;
        }
        
        /// <summary>
        /// Create SpawnResourceVeinCommand from parameters dictionary
        /// </summary>
        public static SpawnResourceVeinCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            var command = new SpawnResourceVeinCommand();
            
            if (parameters.TryGetValue("ResourceType", out var resourceType))
            {
                command.ResourceType = resourceType;
            }
            
            if (parameters.TryGetValue("PositionX", out var posX) && float.TryParse(posX?.ToString(), out var posXValue))
            {
                command.PositionX = posXValue;
            }
            
            if (parameters.TryGetValue("PositionZ", out var posZ) && float.TryParse(posZ?.ToString(), out var posZValue))
            {
                command.PositionZ = posZValue;
            }
            
            if (parameters.TryGetValue("ResourceAmount", out var amount) && int.TryParse(amount?.ToString(), out var amountValue))
            {
                command.ResourceAmount = amountValue;
            }
            
            return command;
        }
    }
}
