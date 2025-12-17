using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.NativeCommands.SpecializedCommands
{
    /// <summary>
    /// Command to set AI aggression levels
    /// </summary>
    public class SetAIAggressionCommand : GameCommandBase
    {
        public override object Faction { get; }
        public override string CommandType => "SetAIAggression";

        public string TargetFaction { get; set; }
        public float AggressionLevel { get; set; }
        public bool ApplyToAll { get; set; }
        public string AggressionType { get; set; }

        public SetAIAggressionCommand()
        {
            AggressionLevel = 0.5f;
            AggressionType = "General";
        }

        /// <summary>
        /// Validates if the AI aggression command is valid
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public override bool IsValid()
        {
            // If not applying to all, target faction must be specified
            if (!ApplyToAll && string.IsNullOrEmpty(TargetFaction))
                return false;

            // Aggression level should be between 0.0 and 1.0
            if (AggressionLevel < 0.0f || AggressionLevel > 1.0f)
                return false;

            // Aggression type is required
            if (string.IsNullOrEmpty(AggressionType))
                return false;

            return true;
        }
    }
}