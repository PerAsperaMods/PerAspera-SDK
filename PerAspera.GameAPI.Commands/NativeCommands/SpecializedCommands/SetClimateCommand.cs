using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.NativeCommands.SpecializedCommands
{
    /// <summary>
    /// Command to set climate values on the planet
    /// </summary>
    public class SetClimateCommand : GameCommandBase
    {
        public override object Faction { get; }
        public override string CommandType => "SetClimate";

        public float Temperature { get; set; }
        public float Pressure { get; set; }
        public float Oxygen { get; set; }
        public bool Relative { get; set; }

        public SetClimateCommand()
        {
            // Default faction assignment
        }

        /// <summary>
        /// Validates if the climate command parameters are valid
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public override bool IsValid()
        {
            // Temperature: -100 to 100 (relative) or absolute Kelvin values
            if (Relative && (Temperature < -100 || Temperature > 100))
                return false;

            // Pressure: 0 to max atmosphere pressure
            if (Pressure < 0)
                return false;

            // Oxygen: 0 to 100%
            if (Oxygen < 0 || Oxygen > 100)
                return false;

            return true;
        }
    }
}