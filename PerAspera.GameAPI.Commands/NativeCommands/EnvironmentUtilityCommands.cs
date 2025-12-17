using System;
using PerAspera.GameAPI.Commands.Core;
using PerAspera.GameAPI.Commands.Constants;

namespace PerAspera.GameAPI.Commands.NativeCommands
{
    /// <summary>
    /// Spawn resource vein command for creating resource deposits on the map
    /// Creates a new resource vein at specified coordinates for the faction to extract
    /// </summary>
    /// <example>
    /// <code>
    /// // Spawn iron vein at coordinates (200, 0, 100)
    /// var result = new SpawnResourceVeinCommand(playerFaction, ResourceType.Iron, 200f, 0f, 100f).Execute();
    /// 
    /// // Using convenience method
    /// var result = Commands.SpawnResourceVein(playerFaction, ResourceType.Iron, 200f, 0f, 100f);
    /// 
    /// // Spawn multiple veins in an area
    /// Commands.ForFaction(playerFaction)
    ///     .SpawnResourceVein(ResourceType.Iron, 200f, 0f, 100f)
    ///     .SpawnResourceVein(ResourceType.Copper, 210f, 0f, 105f)
    ///     .SpawnResourceVein(ResourceType.Aluminum, 190f, 0f, 95f)
    ///     .Execute();
    /// </code>
    /// </example>
    public class SpawnResourceVeinCommand : GameCommandBase
    {
        /// <summary>
        /// The faction that will have access to the resource vein
        /// </summary>
        public override object Faction { get; }
        
        /// <summary>
        /// The type of resource vein to spawn
        /// </summary>
        public object Resource { get; }
        
        /// <summary>
        /// X coordinate for vein placement
        /// </summary>
        public float X { get; }
        
        /// <summary>
        /// Y coordinate for vein placement
        /// </summary>
        public float Y { get; }
        
        /// <summary>
        /// Z coordinate for vein placement
        /// </summary>
        public float Z { get; }
        
        /// <summary>
        /// Create a new SpawnResourceVein command
        /// </summary>
        /// <param name="faction">Faction to spawn vein for</param>
        /// <param name="resource">Type of resource vein</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="z">Z coordinate</param>
        public SpawnResourceVeinCommand(object faction, object resource, float x, float y, float z)
            : base(NativeCommandTypes.SpawnResourceVein)
        {
            Faction = faction ?? throw new ArgumentNullException(nameof(faction));
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
            
            if (!float.IsFinite(x)) throw new ArgumentException("X coordinate must be finite", nameof(x));
            if (!float.IsFinite(y)) throw new ArgumentException("Y coordinate must be finite", nameof(y));
            if (!float.IsFinite(z)) throw new ArgumentException("Z coordinate must be finite", nameof(z));
            
            X = x;
            Y = y;
            Z = z;
            
            Parameters[ParameterNames.Faction] = faction;
            Parameters[ParameterNames.Resource] = resource;
            Parameters[ParameterNames.X] = x;
            Parameters[ParameterNames.Y] = y;
            Parameters[ParameterNames.Z] = z;
        }
        
        public override bool IsValid()
        {
            if (Faction == null) return false;
            if (Resource == null) return false;
            if (!float.IsFinite(X)) return false;
            if (!float.IsFinite(Y)) return false;
            if (!float.IsFinite(Z)) return false;
            
            return true;
        }
    }
    
    /// <summary>
    /// Sabotage command for disrupting faction operations
    /// Causes negative effects on the target faction's infrastructure or resources
    /// </summary>
    /// <example>
    /// <code>
    /// // Sabotage enemy faction
    /// var result = new SabotageCommand(enemyFaction).Execute();
    /// 
    /// // Using convenience method
    /// var result = Commands.Sabotage(enemyFaction);
    /// </code>
    /// </example>
    public class SabotageCommand : GameCommandBase
    {
        /// <summary>
        /// The faction to sabotage
        /// </summary>
        public object Faction { get; }
        
        /// <summary>
        /// Create a new Sabotage command
        /// </summary>
        /// <param name="faction">Faction to sabotage</param>
        public SabotageCommand(object faction)
            : base(NativeCommandTypes.Sabotage)
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
            return $"Sabotage faction {Faction}";
        }
        
        public static SabotageCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue(ParameterNames.Faction, out var faction))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Faction}");
            
            return new SabotageCommand(faction);
        }
    }
    
    /// <summary>
    /// Set override command for modifying game configuration values
    /// Allows runtime modification of game settings and parameters
    /// </summary>
    /// <example>
    /// <code>
    /// // Override building cost modifier
    /// var result = new SetOverrideCommand("building_cost_modifier", 0.5f).Execute();
    /// 
    /// // Override research speed
    /// var result = Commands.SetOverride("research_speed_multiplier", 2.0f);
    /// 
    /// // Set multiple overrides
    /// Commands.CreateBatch()
    ///     .AddCommand(Commands.Create("SetOverride").WithParameter("key", "resource_generation").WithParameter("value", 1.5f))
    ///     .AddCommand(Commands.Create("SetOverride").WithParameter("key", "building_speed").WithParameter("value", 2.0f))
    ///     .Execute();
    /// </code>
    /// </example>
    public class SetOverrideCommand : GameCommandBase
    {
        /// <summary>
        /// The configuration key to override
        /// </summary>
        public string Key { get; }
        
        /// <summary>
        /// The value to set
        /// </summary>
        public object Value { get; }
        
        /// <summary>
        /// Create a new SetOverride command
        /// </summary>
        /// <param name="key">Configuration key to override</param>
        /// <param name="value">Value to set</param>
        public SetOverrideCommand(string key, object value)
            : base(NativeCommandTypes.SetOverride)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            
            Key = key.Trim();
            Value = value ?? throw new ArgumentNullException(nameof(value));
            
            Parameters[ParameterNames.Key] = key;
            Parameters[ParameterNames.Value] = value;
        }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
            
            if (string.IsNullOrWhiteSpace(Key))
            {
                errorMessage = "Key cannot be null or empty";
                return false;
            }
            
            if (Value == null)
            {
                errorMessage = "Value cannot be null";
                return false;
            }
            
            return true;
        }
        
        public override string GetDescription()
        {
            return $"Set override {Key} = {Value}";
        }
        
        public static SetOverrideCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue(ParameterNames.Key, out var keyObj) || 
                !(keyObj is string key) || string.IsNullOrWhiteSpace(key))
                throw new ArgumentException($"Missing or invalid parameter: {ParameterNames.Key}");
                
            if (!parameters.TryGetValue(ParameterNames.Value, out var value))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Value}");
            
            return new SetOverrideCommand(key, value);
        }
    }
    
    /// <summary>
    /// Show message command for displaying information to players
    /// Displays a message to the specified faction's user interface
    /// </summary>
    /// <example>
    /// <code>
    /// // Show message to player faction
    /// var result = new ShowMessageCommand(playerFaction, "Resources have been imported successfully!").Execute();
    /// 
    /// // Using faction builder
    /// Commands.ForFaction(playerFaction)
    ///     .ShowMessage("Welcome to Mars terraforming!")
    ///     .Execute();
    /// </code>
    /// </example>
    public class ShowMessageCommand : GameCommandBase
    {
        /// <summary>
        /// The faction to show the message to
        /// </summary>
        public object Faction { get; }
        
        /// <summary>
        /// The message to display
        /// </summary>
        public string Message { get; }
        
        /// <summary>
        /// Create a new ShowMessage command
        /// </summary>
        /// <param name="faction">Faction to show message to</param>
        /// <param name="message">Message to display</param>
        public ShowMessageCommand(object faction, string message)
            : base(NativeCommandTypes.ShowMessage)
        {
            Faction = faction ?? throw new ArgumentNullException(nameof(faction));
            
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty", nameof(message));
            
            Message = message.Trim();
            
            Parameters[ParameterNames.Faction] = faction;
            Parameters[ParameterNames.Message] = message;
        }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
            
            if (Faction == null)
            {
                errorMessage = "Faction cannot be null";
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(Message))
            {
                errorMessage = "Message cannot be null or empty";
                return false;
            }
            
            return true;
        }
        
        public override string GetDescription()
        {
            return $"Show message to faction {Faction}: \"{Message}\"";
        }
        
        public static ShowMessageCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue(ParameterNames.Faction, out var faction))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Faction}");
                
            if (!parameters.TryGetValue(ParameterNames.Message, out var messageObj) ||
                !(messageObj is string message) || string.IsNullOrWhiteSpace(message))
                throw new ArgumentException($"Missing or invalid parameter: {ParameterNames.Message}");
            
            return new ShowMessageCommand(faction, message);
        }
    }
    
    /// <summary>
    /// Show tutorial message command for displaying tutorial/help information
    /// Displays a tutorial message with special formatting or behavior
    /// </summary>
    /// <example>
    /// <code>
    /// // Show tutorial message to player faction
    /// var result = new ShowTutorialMessageCommand(playerFaction, "Build your first solar panel to generate energy!").Execute();
    /// 
    /// // Chain tutorial messages
    /// Commands.ForFaction(newPlayerFaction)
    ///     .ShowTutorialMessage("Welcome to Per Aspera!")
    ///     .ShowTutorialMessage("Click on buildings to construct them.")
    ///     .ShowTutorialMessage("Monitor your resource levels carefully.")
    ///     .Execute();
    /// </code>
    /// </example>
    public class ShowTutorialMessageCommand : GameCommandBase
    {
        /// <summary>
        /// The faction to show the tutorial message to
        /// </summary>
        public object Faction { get; }
        
        /// <summary>
        /// The tutorial message to display
        /// </summary>
        public string Message { get; }
        
        /// <summary>
        /// Create a new ShowTutorialMessage command
        /// </summary>
        /// <param name="faction">Faction to show tutorial message to</param>
        /// <param name="message">Tutorial message to display</param>
        public ShowTutorialMessageCommand(object faction, string message)
            : base(NativeCommandTypes.ShowTutorialMessage)
        {
            Faction = faction ?? throw new ArgumentNullException(nameof(faction));
            
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty", nameof(message));
            
            Message = message.Trim();
            
            Parameters[ParameterNames.Faction] = faction;
            Parameters[ParameterNames.Message] = message;
        }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
            
            if (Faction == null)
            {
                errorMessage = "Faction cannot be null";
                return false;
            }
            
            if (string.IsNullOrWhiteSpace(Message))
            {
                errorMessage = "Message cannot be null or empty";
                return false;
            }
            
            return true;
        }
        
        public override string GetDescription()
        {
            return $"Show tutorial message to faction {Faction}: \"{Message}\"";
        }
        
        public static ShowTutorialMessageCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            if (!parameters.TryGetValue(ParameterNames.Faction, out var faction))
                throw new ArgumentException($"Missing required parameter: {ParameterNames.Faction}");
                
            if (!parameters.TryGetValue(ParameterNames.Message, out var messageObj) ||
                !(messageObj is string message) || string.IsNullOrWhiteSpace(message))
                throw new ArgumentException($"Missing or invalid parameter: {ParameterNames.Message}");
            
            return new ShowTutorialMessageCommand(faction, message);
        }
    }
    
    /// <summary>
    /// Game over command for ending the game
    /// Immediately triggers the game over state
    /// </summary>
    /// <example>
    /// <code>
    /// // Trigger game over
    /// var result = new GameOverCommand().Execute();
    /// 
    /// // Using convenience method
    /// var result = Commands.GameOver();
    /// </code>
    /// </example>
    public class GameOverCommand : GameCommandBase
    {
        /// <summary>
        /// Create a new GameOver command
        /// </summary>
        public GameOverCommand()
            : base(NativeCommandTypes.GameOver)
        {
            // GameOver command has no parameters
        }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
            return true; // GameOver command is always valid
        }
        
        public override string GetDescription()
        {
            return "Trigger game over";
        }
        
        public static GameOverCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            // GameOver command ignores all parameters
            return new GameOverCommand();
        }
    }
}