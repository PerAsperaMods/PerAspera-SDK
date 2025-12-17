using PerAspera.GameAPI.Commands.Core;
using PerAspera.GameAPI.Events;

namespace PerAspera.GameAPI.Commands.NativeCommands.SpecializedCommands
{
    /// <summary>
    /// Command to move units to specific locations
    /// </summary>
    public class MoveUnitCommand : GameCommandBase
    {
        public override object Faction { get; }
        public override string CommandType => "MoveUnit";

        public string UnitId { get; set; }
        public float TargetX { get; set; }
        public float TargetY { get; set; }
        public bool Force { get; set; }
        public float Speed { get; set; }

        public MoveUnitCommand()
        {
            Speed = 1.0f;
        }

        /// <summary>
        /// Validates if the move unit command is valid
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public override bool IsValid()
        {
            // Unit ID is required
            if (string.IsNullOrEmpty(UnitId))
                return false;

            // Target coordinates should be valid
            if (TargetX < -1000 || TargetX > 1000 || 
                TargetY < -1000 || TargetY > 1000)
                return false;

            // Speed must be positive
            if (Speed <= 0)
                return false;

            return true;
        }
    }
}