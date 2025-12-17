using System;
using PerAspera.GameAPI.Commands.Builders;
using PerAspera.GameAPI.Commands.Constants;
using PerAspera.GameAPI.Commands.Core;

namespace PerAspera.GameAPI.Commands
{
    /// <summary>
    /// Main static entry point for Commands API with fluent builder pattern
    /// Provides convenient methods for creating and executing Per Aspera commands
    /// </summary>
    public static class Commands
    {
        /// <summary>
        /// Create a new command builder for the specified command type
        /// </summary>
        /// <param name="commandType">Type of command to create (e.g., "ImportResource")</param>
        /// <returns>CommandBuilder for fluent configuration</returns>
        public static CommandBuilder Create(string commandType)
        {
            if (string.IsNullOrEmpty(commandType))
                throw new ArgumentException("Command type cannot be null or empty", nameof(commandType));
                
            return new CommandBuilder(commandType);
        }
        
        /// <summary>
        /// Create a faction-specific command builder for chaining faction commands
        /// </summary>
        /// <param name="faction">Faction to execute commands for</param>
        /// <returns>FactionCommandBuilder for faction-specific fluent API</returns>
        public static FactionCommandBuilder ForFaction(object faction)
        {
            if (faction == null)
                throw new ArgumentNullException(nameof(faction));
                
            return new FactionCommandBuilder(faction);
        }
        
        /// <summary>
        /// Create a batch command builder for executing multiple commands
        /// </summary>
        /// <returns>BatchCommandBuilder for batch execution</returns>
        public static BatchCommandBuilder CreateBatch()
        {
            return new BatchCommandBuilder();
        }
        
        // Static convenience methods for common commands
        
        /// <summary>
        /// Import resource for faction (convenience method)
        /// </summary>
        public static CommandResult ImportResource(object faction, object resource, int quantity)
        {
            return Create(NativeCommandTypes.ImportResource)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Resource, resource)
                .WithParameter(ParameterNames.Quantity, quantity)
                .Execute();
        }
        
        /// <summary>
        /// Unlock building for faction (convenience method)
        /// </summary>
        public static CommandResult UnlockBuilding(object faction, object building)
        {
            return Create(NativeCommandTypes.UnlockBuilding)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Building, building)
                .Execute();
        }
        
        /// <summary>
        /// Research technology for faction (convenience method)
        /// </summary>
        public static CommandResult ResearchTechnology(object faction, object technology)
        {
            return Create(NativeCommandTypes.ResearchTechnology)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Technology, technology)
                .Execute();
        }
        
        /// <summary>
        /// Unlock knowledge for faction (convenience method)
        /// </summary>
        public static CommandResult UnlockKnowledge(object faction, object knowledge)
        {
            return Create(NativeCommandTypes.UnlockKnowledge)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Knowledge, knowledge)
                .Execute();
        }
        
        /// <summary>
        /// Start dialogue (convenience method)
        /// </summary>
        public static CommandResult StartDialogue(object faction, object person, object dialogue)
        {
            return Create(NativeCommandTypes.StartDialogue)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Person, person)
                .WithParameter(ParameterNames.Dialogue, dialogue)
                .Execute();
        }
        
        /// <summary>
        /// Set override value (convenience method)
        /// </summary>
        public static CommandResult SetOverride(string key, object value)
        {
            return Create(NativeCommandTypes.SetOverride)
                .WithParameter(ParameterNames.Key, key)
                .WithParameter(ParameterNames.Value, value)
                .Execute();
        }
        
        /// <summary>
        /// Sabotage faction (convenience method)
        /// </summary>
        public static CommandResult Sabotage(object targetFaction)
        {
            return Create(NativeCommandTypes.Sabotage)
                .WithFaction(targetFaction)
                .Execute();
        }
        
        /// <summary>
        /// Spawn resource vein (convenience method)
        /// </summary>
        public static CommandResult SpawnResourceVein(object faction, object resource, float x, float y, float z)
        {
            return Create(NativeCommandTypes.SpawnResourceVein)
                .WithFaction(faction)
                .WithParameter(ParameterNames.Resource, resource)
                .WithParameter(ParameterNames.X, x)
                .WithParameter(ParameterNames.Y, y)
                .WithParameter(ParameterNames.Z, z)
                .Execute();
        }
        
        /// <summary>
        /// Game over command (convenience method)
        /// </summary>
        public static CommandResult GameOver()
        {
            return Create(NativeCommandTypes.GameOver)
                .Execute();
        }
        
        /// <summary>
        /// Check if command type is supported
        /// </summary>
        public static bool IsCommandTypeSupported(string commandType)
        {
            return NativeCommandTypes.IsNativeCommandType(commandType);
        }
        
        /// <summary>
        /// Get all supported command types
        /// </summary>
        public static string[] GetSupportedCommandTypes()
        {
            return NativeCommandTypes.AllNativeTypes;
        }
        
        /// <summary>
        /// Subscribe to command execution events
        /// </summary>
        public static void OnCommandExecuted(Action<Events.CommandExecutedEvent> handler)
        {
            CommandDispatcher.Instance.SubscribeToExecutedEvents(handler);
        }
        
        /// <summary>
        /// Subscribe to command failure events
        /// </summary>
        public static void OnCommandFailed(Action<Events.CommandFailedEvent> handler)
        {
            CommandDispatcher.Instance.SubscribeToFailedEvents(handler);
        }
        
        /// <summary>
        /// Unsubscribe from command execution events
        /// </summary>
        public static void OffCommandExecuted(Action<Events.CommandExecutedEvent> handler)
        {
            CommandDispatcher.Instance.UnsubscribeFromExecutedEvents(handler);
        }
        
        /// <summary>
        /// Unsubscribe from command failure events
        /// </summary>
        public static void OffCommandFailed(Action<Events.CommandFailedEvent> handler)
        {
            CommandDispatcher.Instance.UnsubscribeFromFailedEvents(handler);
        }
        
        /// <summary>
        /// Get command execution statistics
        /// </summary>
        public static Events.CommandStatistics GetStatistics()
        {
            return CommandDispatcher.Instance.GetStatistics();
        }
    }
}