using System;
using PerAspera.GameAPI.Commands.Core;
using PerAspera.GameAPI.Commands.Constants;

namespace PerAspera.GameAPI.Commands.NativeCommands
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
        public object Faction { get; }
        
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
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
            
            if (Faction == null)
            {
                errorMessage = "Faction cannot be null";
                return false;
            }
            
            if (Person == null)
            {
                errorMessage = "Person cannot be null";
                return false;
            }
            
            if (Dialogue == null)
            {
                errorMessage = "Dialogue cannot be null";
                return false;
            }
            
            // Additional validation could check:
            // - If person is available for dialogue
            // - If dialogue prerequisites are met
            // - If dialogue is appropriate for the current game state
            
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
    
    /// <summary>
    /// Skip dialogue command for ending or bypassing dialogue sequences
    /// Immediately ends the current dialogue or skips to the next section
    /// </summary>
    /// <example>
    /// <code>
    /// // Skip current dialogue for player faction
    /// var result = new SkipDialogueCommand(playerFaction, currentDialogue).Execute();
    /// 
    /// // Using faction builder with automatic skip
    /// Commands.ForFaction(playerFaction)
    ///     .SkipDialogue(tutorialDialogue)
    ///     .Execute();
    /// </code>
    /// </example>
    public class SkipDialogueCommand : GameCommandBase
    {
        /// <summary>
        /// The faction skipping the dialogue
        /// </summary>
        public object Faction { get; }
        
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
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
            
            if (Faction == null)
            {
                errorMessage = "Faction cannot be null";
                return false;
            }
            
            if (Dialogue == null)
            {
                errorMessage = "Dialogue cannot be null";
                return false;
            }
            
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
    
    /// <summary>
    /// Enable keeper mode command for activating AI keeper assistance
    /// Enables the AI keeper mode for the specified faction, providing automated assistance
    /// </summary>
    /// <example>
    /// <code>
    /// // Enable keeper mode for player faction
    /// var result = new EnableKeeperModeCommand(playerFaction).Execute();
    /// 
    /// // Using convenience method
    /// Commands.ForFaction(playerFaction).EnableKeeperMode().Execute();
    /// </code>
    /// </example>
    public class EnableKeeperModeCommand : GameCommandBase
    {
        /// <summary>
        /// The faction to enable keeper mode for
        /// </summary>
        public object Faction { get; }
        
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
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
            
            if (Faction == null)
            {
                errorMessage = "Faction cannot be null";
                return false;
            }
            
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
    
    /// <summary>
    /// Disable keeper mode command for deactivating AI keeper assistance
    /// Disables the AI keeper mode for the specified faction, removing automated assistance
    /// </summary>
    /// <example>
    /// <code>
    /// // Disable keeper mode for player faction
    /// var result = new DisableKeeperModeCommand(playerFaction).Execute();
    /// 
    /// // Using convenience method
    /// Commands.ForFaction(playerFaction).DisableKeeperMode().Execute();
    /// </code>
    /// </example>
    public class DisableKeeperModeCommand : GameCommandBase
    {
        /// <summary>
        /// The faction to disable keeper mode for
        /// </summary>
        public object Faction { get; }
        
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
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
            
            if (Faction == null)
            {
                errorMessage = "Faction cannot be null";
                return false;
            }
            
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