using System;
using PerAspera.GameAPI.Commands.Core;
using PerAspera.GameAPI.Commands.Constants;

namespace PerAspera.GameAPI.Commands.NativeCommands.InteractionCommands
{
    /// <summary>
    /// Enable keeper mode command for activating AI keeper assistance
    /// Enables the AI keeper mode for the specified faction, providing automated assistance
    /// </summary>
    public class EnableKeeperModeCommand : GameCommandBase
    {
        /// <summary>
        /// The faction to enable keeper mode for
        /// </summary>
        public override object Faction { get; }
        
        public override string CommandType => NativeCommandTypes.EnableKeeperMode;
        
        /// <summary>
        /// Create a new EnableKeeperMode command
        /// </summary>
        /// <param name="faction">Faction to enable keeper mode for</param>
        public EnableKeeperModeCommand(object faction)
            : base(NativeCommandTypes.EnableKeeperMode)
        {
            Faction = faction ?? throw new ArgumentNullException(nameof(faction));
            
            Parameters[ParameterNames.Faction] = faction;
        }
        
        public override bool IsValid()
        {
            if (Faction == null)
                return false;
            
            return true;
        }
        
        public override string GetDescription()
        {
            return $"Enable keeper mode for faction {Faction}";
        }
        
        public static EnableKeeperModeCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue(ParameterNames.Faction, out var faction))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Faction}");
            
            return new EnableKeeperModeCommand(faction);
        }
    }
}
