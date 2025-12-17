using System;
using PerAspera.GameAPI.Commands.Core;
using PerAspera.GameAPI.Commands.Constants;

namespace PerAspera.GameAPI.Commands.NativeCommands
{
    /// <summary>
    /// Research technology command for unlocking technologies in the tech tree
    /// Advances the faction's technological progress by researching specific technologies
    /// </summary>
    /// <example>
    /// <code>
    /// // Research advanced engineering for player faction
    /// var result = new ResearchTechnologyCommand(playerFaction, TechnologyType.AdvancedEngineering).Execute();
    /// 
    /// // Using convenience method
    /// var result = Commands.ResearchTechnology(playerFaction, TechnologyType.AdvancedEngineering);
    /// 
    /// // Chain multiple technology research
    /// Commands.ForFaction(playerFaction)
    ///     .ResearchTechnology(TechnologyType.BasicEngineering)
    ///     .ResearchTechnology(TechnologyType.AdvancedEngineering)
    ///     .ResearchTechnology(TechnologyType.ExpertEngineering)
    ///     .Execute();
    /// </code>
    /// </example>
    public class ResearchTechnologyCommand : GameCommandBase
    {
        /// <summary>
        /// The faction that will research the technology
        /// </summary>
        public override object Faction { get; }
        
        public override string CommandType => NativeCommandTypes.ResearchTechnology;
        
        /// <summary>
        /// The technology to research
        /// </summary>
        public object Technology { get; }
        
        /// <summary>
        /// Create a new ResearchTechnology command
        /// </summary>
        /// <param name="faction">Faction to research technology for</param>
        /// <param name="technology">Technology to research</param>
        /// <exception cref="ArgumentNullException">If faction or technology is null</exception>
        public ResearchTechnologyCommand(object faction, object technology)
            : base(NativeCommandTypes.ResearchTechnology)
        {
            Faction = faction ?? throw new ArgumentNullException(nameof(faction));
            Technology = technology ?? throw new ArgumentNullException(nameof(technology));
            
            Parameters[ParameterNames.Faction] = faction;
            Parameters[ParameterNames.Technology] = technology;
        }
        
        public override bool IsValid()
        {
            if (Faction == null)
                return false;
            
            if (Technology == null)
                return false;
            
            // Note: Additional validation could check:
            // - If technology prerequisites are met
            // - If technology is already researched
            // - If technology is valid for the faction
            // This would require access to game's technology tree data
            
            return true;
        }
        
        public override string GetDescription()
        {
            return $"Research technology {Technology} for faction {Faction}";
        }
        
        public static ResearchTechnologyCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue(ParameterNames.Faction, out var faction))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Faction}");
                
            if (!parameters.TryGetValue(ParameterNames.Technology, out var technology))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Technology}");
            
            return new ResearchTechnologyCommand(faction, technology);
        }
    }
    
    /// <summary>
    /// Unlock knowledge command for making knowledge available to a faction
    /// Grants access to specific knowledge items in the knowledge tree
    /// </summary>
    /// <example>
    /// <code>
    /// // Unlock basic chemistry knowledge for player faction
    /// var result = new UnlockKnowledgeCommand(playerFaction, KnowledgeType.BasicChemistry).Execute();
    /// 
    /// // Using convenience method
    /// var result = Commands.UnlockKnowledge(playerFaction, KnowledgeType.BasicChemistry);
    /// 
    /// // Unlock multiple knowledge items in sequence
    /// Commands.ForFaction(playerFaction)
    ///     .UnlockKnowledge(KnowledgeType.BasicPhysics)
    ///     .UnlockKnowledge(KnowledgeType.BasicChemistry)
    ///     .UnlockKnowledge(KnowledgeType.BasicBiology)
    ///     .Execute();
    /// </code>
    /// </example>
    public class UnlockKnowledgeCommand : GameCommandBase
    {
        /// <summary>
        /// The faction that will gain the knowledge
        /// </summary>
        public override object Faction { get; }
        
        public override string CommandType => NativeCommandTypes.UnlockKnowledge;
        
        /// <summary>
        /// The knowledge to unlock
        /// </summary>
        public object Knowledge { get; }
        
        /// <summary>
        /// Create a new UnlockKnowledge command
        /// </summary>
        /// <param name="faction">Faction to unlock knowledge for</param>
        /// <param name="knowledge">Knowledge to unlock</param>
        public UnlockKnowledgeCommand(object faction, object knowledge)
            : base(NativeCommandTypes.UnlockKnowledge)
        {
            Faction = faction ?? throw new ArgumentNullException(nameof(faction));
            Knowledge = knowledge ?? throw new ArgumentNullException(nameof(knowledge));
            
            Parameters[ParameterNames.Faction] = faction;
            Parameters[ParameterNames.Knowledge] = knowledge;
        }
        
        public override bool IsValid()
        {
            if (Faction == null)
                return false;
            
            if (Knowledge == null)
                return false;
            
            // Additional validation could check:
            // - If knowledge prerequisites are met
            // - If knowledge is already unlocked
            // - If knowledge is valid for the faction type
            
            return true;
        }
        
        public override string GetDescription()
        {
            return $"Unlock knowledge {Knowledge} for faction {Faction}";
        }
        
        public static UnlockKnowledgeCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue(ParameterNames.Faction, out var faction))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Faction}");
                
            if (!parameters.TryGetValue(ParameterNames.Knowledge, out var knowledge))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Knowledge}");
            
            return new UnlockKnowledgeCommand(faction, knowledge);
        }
    }
    
    /// <summary>
    /// Lock knowledge command for removing knowledge from a faction's available knowledge
    /// Removes access to specific knowledge items (useful for scenarios or balance)
    /// </summary>
    /// <example>
    /// <code>
    /// // Lock advanced physics knowledge from AI faction
    /// var result = new LockKnowledgeCommand(aiFaction, KnowledgeType.AdvancedPhysics).Execute();
    /// 
    /// // Lock multiple knowledge items for a faction
    /// Commands.ForFaction(restrictedFaction)
    ///     .LockKnowledge(KnowledgeType.NuclearPhysics)
    ///     .LockKnowledge(KnowledgeType.QuantumMechanics)
    ///     .Execute();
    /// </code>
    /// </example>
    public class LockKnowledgeCommand : GameCommandBase
    {
        /// <summary>
        /// The faction that will lose the knowledge
        /// </summary>
        public override object Faction { get; }
        
        public override string CommandType => NativeCommandTypes.LockKnowledge;
        
        /// <summary>
        /// The knowledge to lock
        /// </summary>
        public object Knowledge { get; }
        
        /// <summary>
        /// Create a new LockKnowledge command
        /// </summary>
        /// <param name="faction">Faction to lock knowledge for</param>
        /// <param name="knowledge">Knowledge to lock</param>
        public LockKnowledgeCommand(object faction, object knowledge)
            : base(NativeCommandTypes.LockKnowledge)
        {
            Faction = faction ?? throw new ArgumentNullException(nameof(faction));
            Knowledge = knowledge ?? throw new ArgumentNullException(nameof(knowledge));
            
            Parameters[ParameterNames.Faction] = faction;
            Parameters[ParameterNames.Knowledge] = knowledge;
        }
        
        public override bool IsValid()
        {
            if (Faction == null)
                return false;
            
            if (Knowledge == null)
                return false;
            
            return true;
        }
        
        public override string GetDescription()
        {
            return $"Lock knowledge {Knowledge} for faction {Faction}";
        }
        
        public static LockKnowledgeCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue(ParameterNames.Faction, out var faction))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Faction}");
                
            if (!parameters.TryGetValue(ParameterNames.Knowledge, out var knowledge))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Knowledge}");
            
            return new LockKnowledgeCommand(faction, knowledge);
        }
    }
}