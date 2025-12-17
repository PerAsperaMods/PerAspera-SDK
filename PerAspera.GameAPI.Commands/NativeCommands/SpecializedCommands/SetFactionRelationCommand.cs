using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.NativeCommands.SpecializedCommands
{
    /// <summary>
    /// Command to set diplomatic relations between factions
    /// </summary>
    public class SetFactionRelationCommand : GameCommandBase
    {
        public override object Faction { get; }
        public override string CommandType => "SetFactionRelation";

        public string SourceFaction { get; set; }
        public string TargetFaction { get; set; }
        public float RelationValue { get; set; }
        public bool IsHostile { get; set; }

        public SetFactionRelationCommand()
        {
            RelationValue = 0.0f;
        }

        /// <summary>
        /// Validates if the faction relation command is valid
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public override bool IsValid()
        {
            // Both factions must be specified
            if (string.IsNullOrEmpty(SourceFaction) || string.IsNullOrEmpty(TargetFaction))
                return false;

            // Relation value should be within valid range (-1.0 to 1.0)
            if (RelationValue < -1.0f || RelationValue > 1.0f)
                return false;

            // Cannot set relation with self
            if (SourceFaction.Equals(TargetFaction))
                return false;

            return true;
        }
        /// <summary>
        /// Create SetFactionRelationCommand from parameters array
        /// </summary>
        public static SetFactionRelationCommand FromParameters(object[] parameters)
        {
            var command = new SetFactionRelationCommand();
            
            if (parameters?.Length >= 3)
            {
                command.Faction = parameters[0];
                command.TargetFaction = parameters[1];
                
                if (float.TryParse(parameters[2]?.ToString(), out var value))
                {
                    command.RelationValue = value;
                }
            }
            
            return command;
        }
    }
}
