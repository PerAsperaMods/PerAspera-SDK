using System;
using PerAspera.GameAPI.Commands.Core;
using PerAspera.GameAPI.Commands.Constants;

namespace PerAspera.GameAPI.Commands.NativeCommands.InteractionCommands
{
    /// <summary>
    /// Disable keeper mode command for deactivating AI keeper assistance
    /// Disables the AI keeper mode for the specified faction, removing automated assistance
    /// </summary>
    public class DisableKeeperModeCommand : GameCommandBase
    {
        /// <summary>
        /// The faction to disable keeper mode for
        /// </summary>
        public override object Faction { get; }
        
        public override string CommandType => NativeCommandTypes.DisableKeeperMode;
        
        /// <summary>
        /// Create a new DisableKeeperMode command
        /// </summary>
        /// <param name="faction">Faction to disable keeper mode for</param>
        public DisableKeeperModeCommand(object faction)
            : base(NativeCommandTypes.DisableKeeperMode)
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
            return $"Disable keeper mode for faction {Faction}";
        }
        
        public static DisableKeeperModeCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue(ParameterNames.Faction, out var faction))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Faction}");
            
            return new DisableKeeperModeCommand(faction);
        }
    }
}