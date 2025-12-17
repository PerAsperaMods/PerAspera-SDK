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
    }
}
