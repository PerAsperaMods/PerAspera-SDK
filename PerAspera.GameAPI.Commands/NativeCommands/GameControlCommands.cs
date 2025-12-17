using System;
using PerAspera.GameAPI.Commands.Core;
using PerAspera.GameAPI.Commands.Constants;

namespace PerAspera.GameAPI.Commands.NativeCommands
{
    public class WinGameCommand : GameCommandBase
    {
        public WinGameCommand() : base(NativeCommandTypes.WinGame) { }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
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
        public LoseGameCommand() : base(NativeCommandTypes.LoseGame) { }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
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
        public PauseGameCommand() : base(NativeCommandTypes.PauseGame) { }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
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
        public ResumeGameCommand() : base(NativeCommandTypes.ResumeGame) { }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
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
        public string SaveName { get; }
        
        public SaveGameCommand(string saveName) : base(NativeCommandTypes.SaveGame)
        {
            SaveName = saveName ?? "autosave";
            Parameters[ParameterNames.Key] = saveName;
        }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
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
        public string SaveName { get; }
        
        public LoadGameCommand(string saveName) : base(NativeCommandTypes.LoadGame)
        {
            SaveName = saveName ?? throw new ArgumentNullException(nameof(saveName));
            Parameters[ParameterNames.Key] = saveName;
        }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
            if (string.IsNullOrEmpty(SaveName)) { errorMessage = "Save name cannot be null or empty"; return false; }
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
        public RestartGameCommand() : base(NativeCommandTypes.RestartGame) { }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
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
        public float Speed { get; }
        
        public SetGameSpeedCommand(float speed) : base(NativeCommandTypes.SetGameSpeed)
        {
            if (speed <= 0) throw new ArgumentException("Speed must be positive", nameof(speed));
            Speed = speed;
            Parameters[ParameterNames.Value] = speed;
        }
        
        protected override bool ValidateCommand(out string errorMessage)
        {
            errorMessage = null;
            if (Speed <= 0) { errorMessage = "Speed must be positive"; return false; }
            return true;
        }
        
        public override string GetDescription() => $"Set game speed to {Speed}x";
        
        public static SetGameSpeedCommand FromParameters(System.Collections.Generic.Dictionary<string, object> parameters)
        {
            return new SetGameSpeedCommand(Convert.ToSingle(parameters[ParameterNames.Value]));
        }
    }
}