using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.NativeCommands.BuildingManagementCommands
{
    /// <summary>
    /// Unlock building command for making a building type available to a faction
    /// Adds the building to the faction's available building list
    /// </summary>
    public class UnlockBuildingCommand : GameCommandBase
    {
        public override object Faction { get; }
        public override string CommandType => "UnlockBuilding";
        
        public object Building { get; set; }
        
        public UnlockBuildingCommand()
        {
            // Default faction assignment
        }
        
        /// <summary>
        /// Validates if the unlock building command is valid
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public override bool IsValid()
        {
            // Faction is required
            if (Faction == null)
                return false;
            
            // Building type is required
            if (Building == null)
                return false;
            
            return true;
        }
    }
}
