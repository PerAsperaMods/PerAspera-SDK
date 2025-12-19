using System;
using System.Collections.Generic;

namespace PerAspera.SDK.TwitchIntegration
{
    /// <summary>
    /// Context object containing game state for command execution
    /// Follows SDK wrapper pattern for safe game access
    /// </summary>
    public class CommandContext
    {
        /// <summary>BaseGame wrapper instance (available from Phase 1)</summary>
        public PerAspera.GameAPI.Wrappers.BaseGame? BaseGame { get; set; }
        
        /// <summary>Universe wrapper instance (available from Phase 2)</summary>
        public PerAspera.GameAPI.Wrappers.Universe? Universe { get; set; }
        
        /// <summary>Planet wrapper instance (available from Phase 2)</summary>
        public PerAspera.GameAPI.Wrappers.Planet? Planet { get; set; }
        
        /// <summary>Whether we're in early phase (limited functionality)</summary>
        public bool IsEarlyPhase { get; set; }
        
        /// <summary>Whether we're in full phase (all functionality available)</summary>
        public bool IsFullPhase { get; set; }
        
        /// <summary>Check if Universe data is available</summary>
        public bool HasUniverse => Universe != null;
        
        /// <summary>Check if Planet data is available</summary>
        public bool HasPlanet => Planet != null;
        
        /// <summary>Check if BaseGame data is available</summary>
        public bool HasBaseGame => BaseGame != null;
    }
    
    /// <summary>
    /// Base interface for all Twitch commands
    /// Follows SDK pattern for consistent command architecture
    /// </summary>
    public interface ITwitchCommand
    {
        /// <summary>Command name (without ! prefix)</summary>
        string Name { get; }
        
        /// <summary>Brief description for help text</summary>
        string Description { get; }
        
        /// <summary>Required phase (early or full)</summary>
        CommandPhase RequiredPhase { get; }
        
        /// <summary>Execute the command with given arguments and context</summary>
        string Execute(string[] args, string username, CommandContext context);
    }
    
    /// <summary>
    /// Phase requirements for commands
    /// </summary>
    public enum CommandPhase
    {
        /// <summary>Available in early phase (no game state required)</summary>
        Early,
        /// <summary>Requires full phase (game state required)</summary>
        Full
    }
    
    /// <summary>
    /// Registry for Twitch commands with phase-aware execution
    /// Thread-safe and follows SDK architecture patterns
    /// </summary>
    public class CommandRegistry
    {
        private readonly Dictionary<string, ITwitchCommand> _commands = new();
        private readonly object _lock = new();
        private readonly PerAspera.Core.IL2CPP.LogAspera _log = PerAspera.Core.IL2CPP.LogAspera.Create("CommandRegistry");
        
        /// <summary>
        /// Register a new command
        /// </summary>
        public void RegisterCommand(string name, ITwitchCommand command)
        {
            lock (_lock)
            {
                _commands[name.ToLowerInvariant()] = command;
                _log.Info($"Command registered: {name} (Phase: {command.RequiredPhase})");
            }
        }
        
        /// <summary>
        /// Check if command exists
        /// </summary>
        public bool HasCommand(string name)
        {
            lock (_lock)
            {
                return _commands.ContainsKey(name.ToLowerInvariant());
            }
        }
        
        /// <summary>
        /// Execute a command with context validation
        /// </summary>
        public string ExecuteCommand(string name, string[] args, string username, CommandContext context)
        {
            lock (_lock)
            {
                var commandName = name.ToLowerInvariant();
                
                if (!_commands.TryGetValue(commandName, out var command))
                {
                    return $"@{username} Unknown command '{name}'. Type !help for available commands.";
                }
                
                // Check phase requirements
                if (command.RequiredPhase == CommandPhase.Full && !context.IsFullPhase)
                {
                    return $"@{username} Command '{name}' requires full game initialization. Please wait...";
                }
                
                try
                {
                    return command.Execute(args, username, context);
                }
                catch (Exception ex)
                {
                    _log.Error($"Command '{name}' execution failed: {ex.Message}");
                    return $"@{username} Error executing command '{name}': {ex.Message}";
                }
            }
        }
        
        /// <summary>
        /// Get all available commands for current phase
        /// </summary>
        public List<ITwitchCommand> GetAvailableCommands(CommandPhase maxPhase)
        {
            lock (_lock)
            {
                var available = new List<ITwitchCommand>();
                foreach (var command in _commands.Values)
                {
                    if (command.RequiredPhase <= maxPhase)
                    {
                        available.Add(command);
                    }
                }
                return available;
            }
        }
        
        /// <summary>
        /// Get total command count
        /// </summary>
        public int GetCommandCount()
        {
            lock (_lock)
            {
                return _commands.Count;
            }
        }
        
        /// <summary>
        /// Clear all commands
        /// </summary>
        public void Clear()
        {
            lock (_lock)
            {
                _commands.Clear();
                _log.Info("All commands cleared");
            }
        }
    }
}