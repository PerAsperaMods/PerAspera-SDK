using System;
using System.Collections.Generic;
using PerAspera.GameAPI.Commands.Core;
using PerAspera.GameAPI.Commands.Constants;
using PerAspera.GameAPI.Commands.NativeCommands;
using PerAspera.GameAPI.Commands.NativeCommands.InteractionCommands;
using PerAspera.GameAPI.Commands.NativeCommands.SpecializedCommands;
using PerAspera.GameAPI.Commands.NativeCommands.BuildingManagementCommands;
using PerAspera.GameAPI.Commands.NativeCommands.EnvironmentUtilityCommands;
using PerAspera.GameAPI.Commands.NativeCommands.ResourceManagementCommands;

namespace PerAspera.GameAPI.Commands.NativeCommands
{
    /// <summary>
    /// Factory for creating native command instances from command type and parameters
    /// Provides type-safe instantiation and parameter validation for all native commands
    /// </summary>
    public static class NativeCommandFactory
    {
        /// <summary>
        /// Create a native command instance from command type and parameters
        /// </summary>
        /// <param name="commandType">The native command type</param>
        /// <param name="parameters">Command parameters</param>
        /// <returns>Strongly-typed command instance</returns>
        /// <exception cref="ArgumentException">If command type is not supported or parameters are invalid</exception>
        public static IGameCommand CreateCommand(string commandType, Dictionary<string, object> parameters)
        {
            if (string.IsNullOrEmpty(commandType))
                throw new ArgumentException("Command type cannot be null or empty", nameof(commandType));
            
            if (parameters == null)
                parameters = new Dictionary<string, object>();
            
            return commandType switch
            {
                // Resource Management Commands
                NativeCommandTypes.ImportResource => ImportResourceCommand.FromParameters(parameters),
                NativeCommandTypes.ExportResource => ExportResourceCommand.FromParameters(parameters),
                NativeCommandTypes.SetResourceAmount => SetResourceAmountCommand.FromParameters(parameters),
                
                // Building Management Commands
                NativeCommandTypes.UnlockBuilding => UnlockBuildingCommand.FromParameters(parameters),
                NativeCommandTypes.LockBuilding => LockBuildingCommand.FromParameters(parameters),
                NativeCommandTypes.AddBuilding => AddBuildingCommand.FromParameters(parameters),
                NativeCommandTypes.RemoveBuilding => RemoveBuildingCommand.FromParameters(parameters),
                
                // Technology & Knowledge Commands
                NativeCommandTypes.ResearchTechnology => ResearchTechnologyCommand.FromParameters(parameters),
                NativeCommandTypes.UnlockKnowledge => UnlockKnowledgeCommand.FromParameters(parameters),
                NativeCommandTypes.LockKnowledge => LockKnowledgeCommand.FromParameters(parameters),
                
                // Interaction Commands
                NativeCommandTypes.StartDialogue => StartDialogueCommand.FromParameters(parameters),
                NativeCommandTypes.SkipDialogue => SkipDialogueCommand.FromParameters(parameters),
                NativeCommandTypes.EnableKeeperMode => EnableKeeperModeCommand.FromParameters(parameters),
                NativeCommandTypes.DisableKeeperMode => DisableKeeperModeCommand.FromParameters(parameters),
                
                // Environment & Utility Commands
                NativeCommandTypes.SpawnResourceVein => SpawnResourceVeinCommand.FromParameters(parameters),
                NativeCommandTypes.Sabotage => SabotageCommand.FromParameters(parameters),
                NativeCommandTypes.SetOverride => SetOverrideCommand.FromParameters(parameters),
                NativeCommandTypes.ShowMessage => ShowMessageCommand.FromParameters(parameters),
                NativeCommandTypes.ShowTutorialMessage => ShowTutorialMessageCommand.FromParameters(parameters),
                NativeCommandTypes.GameOver => GameOverCommand.FromParameters(parameters),
                
                // Game Control Commands
                NativeCommandTypes.WinGame => WinGameCommand.FromParameters(parameters),
                NativeCommandTypes.LoseGame => LoseGameCommand.FromParameters(parameters),
                NativeCommandTypes.PauseGame => PauseGameCommand.FromParameters(parameters),
                NativeCommandTypes.ResumeGame => ResumeGameCommand.FromParameters(parameters),
                NativeCommandTypes.SaveGame => SaveGameCommand.FromParameters(parameters),
                NativeCommandTypes.LoadGame => LoadGameCommand.FromParameters(parameters),
                NativeCommandTypes.RestartGame => RestartGameCommand.FromParameters(parameters),
                NativeCommandTypes.SetGameSpeed => SetGameSpeedCommand.FromParameters(parameters),
                
                // Specialized Commands
                NativeCommandTypes.SetClimate => SetClimateCommand.FromParameters(parameters),
                NativeCommandTypes.TriggerEvent => TriggerEventCommand.FromParameters(parameters),
                NativeCommandTypes.SpawnUnit => SpawnUnitCommand.FromParameters(parameters),
                NativeCommandTypes.DestroyUnit => DestroyUnitCommand.FromParameters(parameters),
                NativeCommandTypes.MoveUnit => MoveUnitCommand.FromParameters(parameters),
                NativeCommandTypes.SetFactionRelation => SetFactionRelationCommand.FromParameters(parameters),
                NativeCommandTypes.AddPoints => AddPointsCommand.FromParameters(parameters),
                NativeCommandTypes.SetAIAggression => SetAIAggressionCommand.FromParameters(parameters),
                
                _ => throw new ArgumentException($"Unsupported command type: {commandType}. " +
                    $"Supported types: {string.Join(", ", GetSupportedCommandTypes())}")
            };
        }
        
        /// <summary>
        /// Get all currently supported command types
        /// </summary>
        public static string[] GetSupportedCommandTypes()
        {
            return new[]
            {
                // Resource Management
                NativeCommandTypes.ImportResource,
                NativeCommandTypes.ExportResource,
                NativeCommandTypes.SetResourceAmount,
                
                // Building Management
                NativeCommandTypes.UnlockBuilding,
                NativeCommandTypes.LockBuilding,
                NativeCommandTypes.AddBuilding,
                NativeCommandTypes.RemoveBuilding,
                
                // Technology & Knowledge
                NativeCommandTypes.ResearchTechnology,
                NativeCommandTypes.UnlockKnowledge,
                NativeCommandTypes.LockKnowledge,
                
                // Interaction
                NativeCommandTypes.StartDialogue,
                NativeCommandTypes.SkipDialogue,
                NativeCommandTypes.EnableKeeperMode,
                NativeCommandTypes.DisableKeeperMode,
                
                // Environment & Utility
                NativeCommandTypes.SpawnResourceVein,
                NativeCommandTypes.Sabotage,
                NativeCommandTypes.SetOverride,
                NativeCommandTypes.ShowMessage,
                NativeCommandTypes.ShowTutorialMessage,
                NativeCommandTypes.GameOver,
                
                // Game Control
                NativeCommandTypes.WinGame,
                NativeCommandTypes.LoseGame,
                NativeCommandTypes.PauseGame,
                NativeCommandTypes.ResumeGame,
                NativeCommandTypes.SaveGame,
                NativeCommandTypes.LoadGame,
                NativeCommandTypes.RestartGame,
                NativeCommandTypes.SetGameSpeed,
                
                // Specialized
                NativeCommandTypes.SetClimate,
                NativeCommandTypes.TriggerEvent,
                NativeCommandTypes.SpawnUnit,
                NativeCommandTypes.DestroyUnit,
                NativeCommandTypes.MoveUnit,
                NativeCommandTypes.SetFactionRelation,
                NativeCommandTypes.AddPoints,
                NativeCommandTypes.SetAIAggression
            };
        }
        
        /// <summary>
        /// Check if a command type is supported by this factory
        /// </summary>
        public static bool IsCommandTypeSupported(string commandType)
        {
            if (string.IsNullOrEmpty(commandType))
                return false;
                
            var supportedTypes = GetSupportedCommandTypes();
            return Array.IndexOf(supportedTypes, commandType) >= 0;
        }
        
        /// <summary>
        /// Get the required parameters for a specific command type
        /// </summary>
        public static string[] GetRequiredParameters(string commandType)
        {
            return commandType switch
            {
                // Resource Management
                NativeCommandTypes.ImportResource => new[] { ParameterNames.Faction, ParameterNames.Resource, ParameterNames.Quantity },
                NativeCommandTypes.ExportResource => new[] { ParameterNames.Faction, ParameterNames.Resource, ParameterNames.Quantity },
                NativeCommandTypes.SetResourceAmount => new[] { ParameterNames.Faction, ParameterNames.Resource, ParameterNames.Amount },
                
                // Building Management
                NativeCommandTypes.UnlockBuilding => new[] { ParameterNames.Faction, ParameterNames.Building },
                NativeCommandTypes.LockBuilding => new[] { ParameterNames.Faction, ParameterNames.Building },
                NativeCommandTypes.AddBuilding => new[] { ParameterNames.Faction, ParameterNames.Building, ParameterNames.X, ParameterNames.Y, ParameterNames.Z },
                NativeCommandTypes.RemoveBuilding => new[] { ParameterNames.Faction, ParameterNames.Building },
                
                // Technology & Knowledge
                NativeCommandTypes.ResearchTechnology => new[] { ParameterNames.Faction, ParameterNames.Technology },
                NativeCommandTypes.UnlockKnowledge => new[] { ParameterNames.Faction, ParameterNames.Knowledge },
                NativeCommandTypes.LockKnowledge => new[] { ParameterNames.Faction, ParameterNames.Knowledge },
                
                // Interaction
                NativeCommandTypes.StartDialogue => new[] { ParameterNames.Faction, ParameterNames.Person, ParameterNames.Dialogue },
                NativeCommandTypes.SkipDialogue => new[] { ParameterNames.Faction, ParameterNames.Dialogue },
                NativeCommandTypes.EnableKeeperMode => new[] { ParameterNames.Faction },
                NativeCommandTypes.DisableKeeperMode => new[] { ParameterNames.Faction },
                
                // Environment & Utility
                NativeCommandTypes.SpawnResourceVein => new[] { ParameterNames.Faction, ParameterNames.Resource, ParameterNames.X, ParameterNames.Y, ParameterNames.Z },
                NativeCommandTypes.Sabotage => new[] { ParameterNames.Faction },
                NativeCommandTypes.SetOverride => new[] { ParameterNames.Key, ParameterNames.Value },
                NativeCommandTypes.ShowMessage => new[] { ParameterNames.Faction, ParameterNames.Message },
                NativeCommandTypes.ShowTutorialMessage => new[] { ParameterNames.Faction, ParameterNames.Message },
                NativeCommandTypes.GameOver => new string[0], // No parameters required
                
                _ => throw new ArgumentException($"Unknown command type: {commandType}")
            };
        }
        
        /// <summary>
        /// Validate that all required parameters are present for a command type
        /// </summary>
        public static bool ValidateParameters(string commandType, Dictionary<string, object> parameters, out string[] missingParameters)
        {
            var requiredParams = GetRequiredParameters(commandType);
            var missing = new List<string>();
            
            foreach (var param in requiredParams)
            {
                if (!parameters.ContainsKey(param) || parameters[param] == null)
                {
                    missing.Add(param);
                }
            }
            
            missingParameters = missing.ToArray();
            return missing.Count == 0;
        }
        
        /// <summary>
        /// Get command documentation and usage examples for a specific command type
        /// </summary>
        public static CommandDocumentation GetCommandDocumentation(string commandType)
        {
            return commandType switch
            {
                NativeCommandTypes.ImportResource => new CommandDocumentation(
                    "Import Resource",
                    "Adds specified quantity of resource to a faction's inventory",
                    GetRequiredParameters(commandType),
                    new[] { "Commands.ImportResource(faction, ResourceType.Water, 1000)" }
                ),
                
                NativeCommandTypes.ExportResource => new CommandDocumentation(
                    "Export Resource", 
                    "Removes specified quantity of resource from a faction's inventory",
                    GetRequiredParameters(commandType),
                    new[] { "Commands.ExportResource(faction, ResourceType.Iron, 500)" }
                ),
                
                NativeCommandTypes.SetResourceAmount => new CommandDocumentation(
                    "Set Resource Amount",
                    "Sets faction's resource amount to exact value (not additive)", 
                    GetRequiredParameters(commandType),
                    new[] { "Commands.SetResourceAmount(faction, ResourceType.Oxygen, 2500.5f)" }
                ),
                
                NativeCommandTypes.UnlockBuilding => new CommandDocumentation(
                    "Unlock Building",
                    "Makes a building type available to a faction",
                    GetRequiredParameters(commandType), 
                    new[] { "Commands.UnlockBuilding(faction, BuildingType.SolarPanel)" }
                ),
                
                NativeCommandTypes.AddBuilding => new CommandDocumentation(
                    "Add Building",
                    "Creates a building instance at specified coordinates",
                    GetRequiredParameters(commandType),
                    new[] { "Commands.ForFaction(faction).AddBuilding(BuildingType.SolarPanel, 100f, 0f, 50f)" }
                ),
                
                NativeCommandTypes.ResearchTechnology => new CommandDocumentation(
                    "Research Technology", 
                    "Unlocks a technology in the faction's tech tree",
                    GetRequiredParameters(commandType),
                    new[] { "Commands.ResearchTechnology(faction, TechnologyType.AdvancedEngineering)" }
                ),
                
                NativeCommandTypes.StartDialogue => new CommandDocumentation(
                    "Start Dialogue",
                    "Initiates a dialogue between faction and person",
                    GetRequiredParameters(commandType),
                    new[] { "Commands.StartDialogue(faction, scientist, DialogueType.Research)" }
                ),
                
                NativeCommandTypes.SpawnResourceVein => new CommandDocumentation(
                    "Spawn Resource Vein",
                    "Creates a resource deposit at specified coordinates", 
                    GetRequiredParameters(commandType),
                    new[] { "Commands.SpawnResourceVein(faction, ResourceType.Iron, 200f, 0f, 100f)" }
                ),
                
                NativeCommandTypes.ShowMessage => new CommandDocumentation(
                    "Show Message",
                    "Displays a message to the faction's user interface",
                    GetRequiredParameters(commandType), 
                    new[] { "Commands.ForFaction(faction).ShowMessage(\"Welcome to Mars!\")" }
                ),
                
                NativeCommandTypes.GameOver => new CommandDocumentation(
                    "Game Over",
                    "Immediately triggers the game over state", 
                    GetRequiredParameters(commandType),
                    new[] { "Commands.GameOver()" }
                ),
                
                _ => new CommandDocumentation(
                    commandType,
                    "Documentation not available",
                    GetRequiredParameters(commandType),
                    new[] { $"Commands.Create(\"{commandType}\")..." }
                )
            };
        }
    }
    
    /// <summary>
    /// Documentation information for a command type
    /// </summary>
    public class CommandDocumentation
    {
        public string Name { get; }
        public string Description { get; }
        public string[] RequiredParameters { get; }
        public string[] Examples { get; }
        
        public CommandDocumentation(string name, string description, string[] requiredParameters, string[] examples)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Description = description ?? throw new ArgumentNullException(nameof(description));
            RequiredParameters = requiredParameters ?? throw new ArgumentNullException(nameof(requiredParameters));
            Examples = examples ?? throw new ArgumentNullException(nameof(examples));
        }
        
        public override string ToString()
        {
            return $"{Name}: {Description} (Parameters: {string.Join(", ", RequiredParameters)})";
        }
    }
}