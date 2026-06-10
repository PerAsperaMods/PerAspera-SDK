using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.NativeCommands.BuildingManagementCommands
{
    /// <summary>
    /// Add building command for spawning a new building at a specific location
    /// Creates a new building instance on the planet
    /// </summary>
    public class AddBuildingCommand : GameCommandBase
    {
        public override object Faction { get; }
        public override string CommandType => "AddBuilding";
        
        public object BuildingType { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public int Rotation { get; set; }
        public bool AutoConnect { get; set; }
        
        public AddBuildingCommand()
        {
            AutoConnect = true;
            Rotation = 0;
        }
        
        /// <summary>
        /// Validates if the add building command is valid
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public override bool IsValid()
        {
            // Faction is required
            if (Faction == null)
                return false;
            
            // Building type is required
            if (BuildingType == null)
                return false;
            
            // Position coordinates should be valid
            if (PositionX < -1000 || PositionX > 1000 || 
                PositionY < -1000 || PositionY > 1000)
                return false;
            
            // Rotation should be valid (0-360 degrees)
            if (Rotation < 0 || Rotation >= 360)
                return false;
            
            return true;
        }
        /// <summary>
        /// Create AddBuildingCommand from parameters dictionary
        /// </summary>
        public static AddBuildingCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            var command = new AddBuildingCommand();
            
            if (parameters.TryGetValue("BuildingType", out var buildingType))
            {
                command.BuildingType = buildingType;
            }
            
            return command;
        }
    }
}
