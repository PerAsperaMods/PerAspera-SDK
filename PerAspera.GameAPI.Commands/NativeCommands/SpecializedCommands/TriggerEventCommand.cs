using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands.NativeCommands.SpecializedCommands
{
    /// <summary>
    /// Command to trigger predefined game events
    /// </summary>
    public class TriggerEventCommand : GameCommandBase
    {
        public override object Faction { get; }
        public override string CommandType => "TriggerEvent";

        public string EventId { get; set; }
        public object[] Parameters { get; set; }
        public float DelaySeconds { get; set; }

        public TriggerEventCommand()
        {
            Parameters = new object[0];
        }

        /// <summary>
        /// Validates if the event trigger command is valid
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        public override bool IsValid()
        {
            // Event ID is required
            if (string.IsNullOrEmpty(EventId))
                return false;

            // Delay cannot be negative
            if (DelaySeconds < 0)
                return false;

            // Parameters array should not be null
            if (Parameters == null)
                return false;

            return true;
        }
    }
}
