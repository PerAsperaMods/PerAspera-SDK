using System;
using System.Linq;

namespace PerAspera.SDK.TwitchIntegration.Commands
{
    /// <summary>
    /// Phase 1 command: Show available commands
    /// Always available, no game state required
    /// </summary>
    public class HelpCommand : ITwitchCommand
    {
        public string Name => "help";
        public string Description => "Show available commands";
        public CommandPhase RequiredPhase => CommandPhase.Early;
        
        public string Execute(string[] args, string username, CommandContext context)
        {
            if (context.IsEarlyPhase && !context.IsFullPhase)
            {
                return $"@{username} 🎮 Available commands: !help, !ping, !status. More commands available after game loads!";
            }
            else if (context.IsFullPhase)
            {
                return $"@{username} 🎮 Available commands: !help, !ping, !status, !game, !resources, !atmosphere, !time";
            }
            else
            {
                return $"@{username} 🎮 System initializing... Available soon: !help, !ping, !status";
            }
        }
    }
    
    /// <summary>
    /// Phase 1 command: Test connection
    /// Always available, no game state required
    /// </summary>
    public class PingCommand : ITwitchCommand
    {
        public string Name => "ping";
        public string Description => "Test Twitch connection";
        public CommandPhase RequiredPhase => CommandPhase.Early;
        
        public string Execute(string[] args, string username, CommandContext context)
        {
            return $"@{username} 🏓 Pong! Twitch integration is alive. Phase: {(context.IsFullPhase ? "Full" : "Early")}";
        }
    }
    
    /// <summary>
    /// Phase 1 command: Show system status
    /// Basic status available immediately, enhanced status when game loads
    /// </summary>
    public class StatusCommand : ITwitchCommand
    {
        public string Name => "status";
        public string Description => "Show system status";
        public CommandPhase RequiredPhase => CommandPhase.Early;
        
        public string Execute(string[] args, string username, CommandContext context)
        {
            if (context.IsFullPhase)
            {
                return $"@{username} 🎮 Status: Full integration active, all systems ready!";
            }
            else if (context.IsEarlyPhase)
            {
                return $"@{username} ⏳ Status: Early phase active, waiting for game to load...";
            }
            else
            {
                return $"@{username} 🔄 Status: System initializing...";
            }
        }
    }
    
    /// <summary>
    /// Phase 2 command: Show detailed game status
    /// Requires Universe wrapper for game state access
    /// </summary>
    public class GameStatusCommand : ITwitchCommand
    {
        public string Name => "game";
        public string Description => "Show current game status";
        public CommandPhase RequiredPhase => CommandPhase.Full;
        
        public string Execute(string[] args, string username, CommandContext context)
        {
            if (!context.HasUniverse)
            {
                return $"@{username} ⚠️ Game state not available yet";
            }
            
            try
            {
                var universe = context.Universe!;
                var sol = universe.CurrentSol;
                var speed = universe.GameSpeed;
                var paused = universe.IsPaused;
                
                var status = paused ? "PAUSED" : $"Running at {speed}x speed";
                return $"@{username} 🌍 Sol {sol} - {status}";
            }
            catch (Exception ex)
            {
                return $"@{username} ❌ Error reading game status: {ex.Message}";
            }
        }
    }
    
    /// <summary>
    /// Phase 2 command: Show resource levels
    /// Requires Planet wrapper for resource access
    /// </summary>
    public class ResourcesCommand : ITwitchCommand
    {
        public string Name => "resources";
        public string Description => "Show resource levels";
        public CommandPhase RequiredPhase => CommandPhase.Full;
        
        public string Execute(string[] args, string username, CommandContext context)
        {
            if (!context.HasPlanet)
            {
                return $"@{username} ⚠️ Planet data not available yet";
            }
            
            try
            {
                var planet = context.Planet!;
                var resources = new[]
                {
                    $"Water: {planet.WaterStock:F1}",
                    $"Silicon: {planet.SiliconStock:F1}",
                    $"Iron: {planet.IronStock:F1}"
                };
                
                return $"@{username} 📦 Resources: {string.Join(", ", resources)}";
            }
            catch (Exception ex)
            {
                return $"@{username} ❌ Error reading resources: {ex.Message}";
            }
        }
    }
    
    /// <summary>
    /// Phase 2 command: Show atmosphere status
    /// Requires Planet wrapper for atmospheric data access
    /// </summary>
    public class AtmosphereCommand : ITwitchCommand
    {
        public string Name => "atmosphere";
        public string Description => "Show atmospheric conditions";
        public CommandPhase RequiredPhase => CommandPhase.Full;
        
        public string Execute(string[] args, string username, CommandContext context)
        {
            if (!context.HasPlanet)
            {
                return $"@{username} ⚠️ Planet data not available yet";
            }
            
            try
            {
                var atmosphere = context.Planet!.Atmosphere;
                var temp = atmosphere.Temperature;
                var pressure = atmosphere.TotalPressure;
                
                return $"@{username} 🌡️ Atmosphere: {temp:F1}K ({temp - 273.15f:F1}°C), {pressure:F2} kPa";
            }
            catch (Exception ex)
            {
                return $"@{username} ❌ Error reading atmosphere: {ex.Message}";
            }
        }
    }
    
    /// <summary>
    /// Phase 2 command: Show time information
    /// Requires Universe wrapper for time data access
    /// </summary>
    public class TimeCommand : ITwitchCommand
    {
        public string Name => "time";
        public string Description => "Show time and game speed";
        public CommandPhase RequiredPhase => CommandPhase.Full;
        
        public string Execute(string[] args, string username, CommandContext context)
        {
            if (!context.HasUniverse)
            {
                return $"@{username} ⚠️ Universe data not available yet";
            }
            
            try
            {
                var universe = context.Universe!;
                var sol = universe.CurrentSol;
                var speed = universe.GameSpeed;
                var paused = universe.IsPaused;
                
                return $"@{username} ⏰ Sol {sol}, Speed: {speed}x, Paused: {paused}";
            }
            catch (Exception ex)
            {
                return $"@{username} ❌ Error reading time: {ex.Message}";
            }
        }
    }
}