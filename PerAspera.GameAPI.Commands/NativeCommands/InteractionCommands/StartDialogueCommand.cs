using System;
using PerAspera.GameAPI.Commands.Core;
using PerAspera.GameAPI.Commands.Constants;

namespace PerAspera.GameAPI.Commands.NativeCommands.InteractionCommands
{
    /// <summary>
    /// Start dialogue command for initiating conversations between characters
    /// Begins a dialogue sequence between a faction and a specific person
    /// </summary>
    /// <example>
    /// <code>
    /// // Start dialogue between player faction and scientist
    /// var result = new StartDialogueCommand(playerFaction, scientist, DialogueType.Research).Execute();
    /// 
    /// // Using convenience method
    /// var result = Commands.StartDialogue(playerFaction, scientist, DialogueType.Research);
    /// 
    /// // Chain dialogues in a faction builder
    /// Commands.ForFaction(playerFaction)
    ///     .StartDialogue(scientist, DialogueType.Introduction)
    ///     .StartDialogue(engineer, DialogueType.ProjectUpdate)
    ///     .Execute();
    /// </code>
    /// </example>
    public class StartDialogueCommand : GameCommandBase
    {
        /// <summary>
        /// The faction participating in the dialogue
        /// </summary>
        public override object Faction { get; }
        
        public override string CommandType => NativeCommandTypes.StartDialogue;
        
        /// <summary>
        /// The person/character to dialogue with
        /// </summary>
        public object Person { get; }
        
        /// <summary>
        /// The dialogue to start
        /// </summary>
        public object Dialogue { get; }
        
        /// <summary>
        /// Create a new StartDialogue command
        /// </summary>
        /// <param name="faction">Faction participating in dialogue</param>
        /// <param name="person">Person to dialogue with</param>
        /// <param name="dialogue">Dialogue to start</param>
        public StartDialogueCommand(object faction, object person, object dialogue)
            : base(NativeCommandTypes.StartDialogue)
        {
            Faction = faction ?? throw new ArgumentNullException(nameof(faction));
            Person = person ?? throw new ArgumentNullException(nameof(person));
            Dialogue = dialogue ?? throw new ArgumentNullException(nameof(dialogue));
            
            Parameters[ParameterNames.Faction] = faction;
            Parameters[ParameterNames.Person] = person;
            Parameters[ParameterNames.Dialogue] = dialogue;
        }
        
        public override bool IsValid()
        {
            if (Faction == null)
                return false;
            
            if (Person == null)
                return false;
            
            if (Dialogue == null)
                return false;
            
            return true;
        }
        
        public override string GetDescription()
        {
            return $"Start dialogue {Dialogue} between faction {Faction} and person {Person}";
        }
        
        public static StartDialogueCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue(ParameterNames.Faction, out var faction))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Faction}");
                
            if (!parameters.TryGetValue(ParameterNames.Person, out var person))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Person}");
                
            if (!parameters.TryGetValue(ParameterNames.Dialogue, out var dialogue))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Dialogue}");
            
            return new StartDialogueCommand(faction, person, dialogue);
        }
    }
}