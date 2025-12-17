using System;
using PerAspera.GameAPI.Commands.Core;
using PerAspera.GameAPI.Commands.Constants;

namespace PerAspera.GameAPI.Commands.NativeCommands
{
    public class WinGameCommand : GameCommandBase
    {
        public override object Faction { get; }
        public override string CommandType => NativeCommandTypes.WinGame;
        
        public WinGameCommand() : base(NativeCommandTypes.WinGame) { }
        
        public override bool IsValid()
        {
            return true;
        }
        
        public override string GetDescription() => "Trigger game victory";
        
        public static WinGameCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            return new WinGameCommand();
        }
    }

    public class LoseGameCommand : GameCommandBase
    {
        public override object Faction { get; }
        public override string CommandType => NativeCommandTypes.LoseGame;
        
        public LoseGameCommand() : base(NativeCommandTypes.LoseGame) { }
        
        public override bool IsValid()
        {
            return true;
        }
        
        public override string GetDescription() => "Trigger game loss";
        
        public static LoseGameCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            return new LoseGameCommand();
        }
    }

    public class PauseGameCommand : GameCommandBase
    {
        public override object Faction { get; }
        public override string CommandType => NativeCommandTypes.PauseGame;
        
        public PauseGameCommand() : base(NativeCommandTypes.PauseGame) { }
        
        public override bool IsValid()
        {
            return true;
        }
        
        public override string GetDescription() => "Pause the game";
        
        public static PauseGameCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            return new PauseGameCommand();
        }
    }

    public class ResumeGameCommand : GameCommandBase
    {
        public override object Faction { get; }
        public override string CommandType => NativeCommandTypes.ResumeGame;
        
        public ResumeGameCommand() : base(NativeCommandTypes.ResumeGame) { }
        
        public override bool IsValid()
        {
            return true;
        }
        
        public override string GetDescription() => "Resume the game";
        
        public static ResumeGameCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            return new ResumeGameCommand();
        }
    }

    public class SaveGameCommand : GameCommandBase
    {
        public override object Faction { get; }
        public override string CommandType => NativeCommandTypes.SaveGame;
        
        public string SaveName { get; }
        
        public SaveGameCommand(string saveName) : base(NativeCommandTypes.SaveGame)
        {
            SaveName = saveName ?? "autosave";
            Parameters[ParameterNames.Key] = saveName;
        }
        
        public override bool IsValid()
        {
            return true;
        }
        
        public override string GetDescription() => $"Save game as '{SaveName}'";
        
        public static SaveGameCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            var saveName = parameters.TryGetValue(ParameterNames.Key, out var name) ? name?.ToString() : "autosave";
            return new SaveGameCommand(saveName);
        }
    }

    public class LoadGameCommand : GameCommandBase
    {
        public override object Faction { get; }
        public override string CommandType => NativeCommandTypes.LoadGame;
        
        public string SaveName { get; }
        
        public LoadGameCommand(string saveName) : base(NativeCommandTypes.LoadGame)
        {
            SaveName = saveName ?? throw new ArgumentNullException(nameof(saveName));
            Parameters[ParameterNames.Key] = saveName;
        }
        
        public override bool IsValid()
        {
            if (string.IsNullOrEmpty(SaveName)) 
                return false;
            return true;
        }
        
        public override string GetDescription() => $"Load game '{SaveName}'";
        
        public static LoadGameCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            return new LoadGameCommand(parameters[ParameterNames.Key]?.ToString());
        }
    }

    public class RestartGameCommand : GameCommandBase
    {
        public override object Faction { get; }
        public override string CommandType => NativeCommandTypes.RestartGame;
        
        public RestartGameCommand() : base(NativeCommandTypes.RestartGame) { }
        
        public override bool IsValid()
        {
            return true;
        }
        
        public override string GetDescription() => "Restart the game";
        
        public static RestartGameCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            return new RestartGameCommand();
        }
    }

    public class SetGameSpeedCommand : GameCommandBase
    {
        public override object Faction { get; }
        public override string CommandType => NativeCommandTypes.SetGameSpeed;
        
        public float Speed { get; }
        
        public SetGameSpeedCommand(float speed) : base(NativeCommandTypes.SetGameSpeed)
        {
            if (speed <= 0) throw new ArgumentException("Speed must be positive", nameof(speed));
            Speed = speed;
            Parameters[ParameterNames.Value] = speed;
        }
        
        public override bool IsValid()
        {
            if (Speed <= 0) 
                return false;
            return true;
        }
        
        public override string GetDescription() => $"Set game speed to {Speed}x";
        
        public static SetGameSpeedCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            return new SetGameSpeedCommand(Convert.ToSingle(parameters[ParameterNames.Value]));
        }
    }
}
