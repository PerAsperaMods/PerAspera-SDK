using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.NativeCommands.BuildingManagementCommands
{
    /// <summary>
    /// Lock building command for removing a building type from a faction's available buildings
    /// Prevents the faction from constructing the specified building type
    /// </summary>
    public class LockBuildingCommand : GameCommandBase
    {
        public override object Faction { get; }
        public override string CommandType => "LockBuilding";
        
        public object Building { get; set; }
        public bool ForceRemove { get; set; }
        
        public LockBuildingCommand()
        {
            // Default faction assignment
        }
        
        /// <summary>
        /// Validates if the lock building command is valid
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
