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

        /// <summary>
        /// Create TriggerEventCommand from parameters array
        /// </summary>
        public static TriggerEventCommand FromParameters(object[] parameters)
        {
            var command = new TriggerEventCommand();
            
            if (parameters?.Length >= 1)
            {
                command.EventId = parameters[0]?.ToString();
                
                if (parameters.Length >= 2 && float.TryParse(parameters[1]?.ToString(), out var delay))
                {
                    command.DelaySeconds = delay;
                }
                
                if (parameters.Length > 2)
                {
                    var eventParams = new object[parameters.Length - 2];
                    System.Array.Copy(parameters, 2, eventParams, 0, eventParams.Length);
                    command.Parameters = eventParams;
                }
            }
            
            return command;
        }
    }
}
