using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.NativeCommands.SpecializedCommands
{
    /// <summary>
    /// Command to add or modify faction points/score
    /// </summary>
    public class AddPointsCommand : GameCommandBase
    {
        public override object Faction { get; }
        public override string CommandType => "AddPoints";

        public string TargetFaction { get; set; }
        public string PointType { get; set; }
        public int Amount { get; set; }
        public string Reason { get; set; }

        public AddPointsCommand()
        {
            Amount = 0;
        }

        /// <summary>
        /// Validates if the add points command is valid
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public override bool IsValid()
        {
            // Target faction must be specified
            if (string.IsNullOrEmpty(TargetFaction))
                return false;

            // Point type is required
            if (string.IsNullOrEmpty(PointType))
                return false;

            // Amount cannot be zero (use positive for add, negative for subtract)
            if (Amount == 0)
                return false;

            return true;
        }
    }
}