using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.NativeCommands.SpecializedCommands
{
    /// <summary>
    /// Command to spawn units on the planet
    /// </summary>
    public class SpawnUnitCommand : GameCommandBase
    {
        public override object Faction { get; }
        public override string CommandType => "SpawnUnit";

        public string UnitType { get; set; }
        public int Quantity { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public string TargetFaction { get; set; }

        public SpawnUnitCommand()
        {
            Quantity = 1;
        }

        /// <summary>
        /// Validates if the spawn unit command is valid
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public override bool IsValid()
        {
            // Unit type is required
            if (string.IsNullOrEmpty(UnitType))
                return false;

            // Quantity must be positive
            if (Quantity <= 0)
                return false;

            // Position coordinates should be valid
            // Assuming planet coordinates range validation
            if (PositionX < -1000 || PositionX > 1000 || 
                PositionY < -1000 || PositionY > 1000)
                return false;

            return true;
        }
    }
}
