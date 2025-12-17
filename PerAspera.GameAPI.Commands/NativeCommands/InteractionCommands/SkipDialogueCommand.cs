using System;
using PerAspera.GameAPI.Commands.Core;
using PerAspera.GameAPI.Commands.Constants;

namespace PerAspera.GameAPI.Commands.NativeCommands.InteractionCommands
{
    /// <summary>
    /// Skip dialogue command for bypassing conversations
    /// Skips the current dialogue sequence for a faction
    /// </summary>
    public class SkipDialogueCommand : GameCommandBase
    {
        /// <summary>
        /// The faction skipping the dialogue
        /// </summary>
        public override object Faction { get; }
        
        public override string CommandType => NativeCommandTypes.SkipDialogue;
        
        /// <summary>
        /// The dialogue to skip
        /// </summary>
        public object Dialogue { get; }
        
        /// <summary>
        /// Create a new SkipDialogue command
        /// </summary>
        /// <param name="faction">Faction skipping the dialogue</param>
        /// <param name="dialogue">Dialogue to skip</param>
        public SkipDialogueCommand(object faction, object dialogue)
            : base(NativeCommandTypes.SkipDialogue)
        {
            Faction = faction ?? throw new ArgumentNullException(nameof(faction));
            Dialogue = dialogue ?? throw new ArgumentNullException(nameof(dialogue));
            
            Parameters[ParameterNames.Faction] = faction;
            Parameters[ParameterNames.Dialogue] = dialogue;
        }
        
        public override bool IsValid()
        {
            if (Faction == null)
                return false;
            
            if (Dialogue == null)
                return false;
            
            return true;
        }
        
        public override string GetDescription()
        {
            return $"Skip dialogue {Dialogue} for faction {Faction}";
        }
        
        public static SkipDialogueCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue(ParameterNames.Faction, out var faction))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Faction}");
                
            if (!parameters.TryGetValue(ParameterNames.Dialogue, out var dialogue))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Dialogue}");
            
            return new SkipDialogueCommand(faction, dialogue);
        }
    }
}
