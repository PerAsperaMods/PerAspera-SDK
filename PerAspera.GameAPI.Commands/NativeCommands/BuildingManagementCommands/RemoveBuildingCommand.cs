using PerAspera.GameAPI.Events;

namespace PerAspera.GameAPI.Commands.NativeCommands.BuildingManagementCommands
{
    /// <summary>
    /// Remove building command for destroying existing buildings
    /// Removes specified building instances from the planet
    /// </summary>
    public class RemoveBuildingCommand : GameCommandBase
    {
        public override object Faction { get; }
        public override string CommandType => "RemoveBuilding";
        
        public string BuildingId { get; set; }
        public object BuildingType { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float SearchRadius { get; set; }
        public bool RemoveAll { get; set; }
        
        public RemoveBuildingCommand()
        {
            SearchRadius = 10.0f;
        }
        
        /// <summary>
        /// Validates if the remove building command is valid
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public override bool IsValid()
        {
            // Faction is required
            if (Faction == null)
                return false;
            
            // Either specific building ID or type/position must be provided
            if (string.IsNullOrEmpty(BuildingId) && BuildingType == null)
                return false;
            
            // If using position-based removal, coordinates should be valid
            if (BuildingType != null && 
                (PositionX < -1000 || PositionX > 1000 || 
                 PositionY < -1000 || PositionY > 1000))
                return false;
            
            // Search radius should be positive
            if (SearchRadius <= 0)
                return false;
            
            return true;
        }
    }
}