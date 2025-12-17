using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.NativeCommands.SpecializedCommands
{
    /// <summary>
    /// Command to destroy specific units
    /// </summary>
    public class DestroyUnitCommand : GameCommandBase
    {
        public override object Faction { get; }
        public override string CommandType => "DestroyUnit";

        public string UnitId { get; set; }
        public string UnitType { get; set; }
        public int Count { get; set; }
        public bool DestroyAll { get; set; }

        public DestroyUnitCommand()
        {
            Count = 1;
        }

        /// <summary>
        /// Validates if the destroy unit command is valid
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public override bool IsValid()
        {
            // Either UnitId or UnitType must be specified
            if (string.IsNullOrEmpty(UnitId) && string.IsNullOrEmpty(UnitType))
                return false;

            // Count must be positive when not destroying all
            if (!DestroyAll && Count <= 0)
                return false;

            return true;
        }
        /// <summary>
        /// Create DestroyUnitCommand from parameters array
        /// </summary>
        public static DestroyUnitCommand FromParameters(object[] parameters)
        {
            var command = new DestroyUnitCommand();
            
            if (parameters?.Length >= 2)
            {
                command.Faction = parameters[0];
                command.UnitId = parameters[1];
            }
            
            return command;
        }
    }
}
