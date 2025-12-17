using System;

namespace PerAspera.GameAPI.Commands.Constants
{
    /// <summary>
    /// Types of native Per Aspera commands
    /// </summary>
    public static class NativeCommandTypes
    {
        // Resource Commands
        public const string ImportResource = "ImportResource";
        public const string ExportResource = "ExportResource";
        public const string SetResourceAmount = "SetResourceAmount";
        public const string SpawnResourceVein = "SpawnResourceVein";
        public const string ExhaustResourceVein = "ExhaustResourceVein";
        
        // Building Commands  
        public const string UnlockBuilding = "UnlockBuilding";
        public const string LockBuilding = "LockBuilding";
        public const string AddBuilding = "AddBuilding";
        public const string RemoveBuilding = "RemoveBuilding";
        public const string AdditionalBuilding = "AdditionalBuilding";
        public const string BuildingRebuild = "BuildingRebuild";
        public const string FactionCreateBuilding = "FactionCreateBuilding";
        
        // Technology & Knowledge Commands
        public const string ResearchTechnology = "ResearchTechnology";
        public const string UnlockKnowledge = "UnlockKnowledge";
        public const string LockKnowledge = "LockKnowledge";
        public const string UnlockTech = "UnlockTech";
        
        // Interaction Commands
        public const string StartDialogue = "StartDialogue";
        public const string SkipDialogue = "SkipDialogue";
        public const string EnableKeeperMode = "EnableKeeperMode";
        public const string DisableKeeperMode = "DisableKeeperMode";
        public const string NotifyDialogue = "NotifyDialogue";
        public const string ShowDialogue = "ShowDialogue";
        
        // Environment & Utility Commands
        public const string Sabotage = "Sabotage";
        public const string SetOverride = "SetOverride";
        public const string ShowMessage = "ShowMessage";
        public const string ShowTutorialMessage = "ShowTutorialMessage";
        public const string SPLoadPrefab = "SPLoadPrefab";
        
        // Game Control Commands
        public const string GameOver = "GameOver";
        public const string WinGame = "WinGame";
        public const string LoseGame = "LoseGame";
        public const string PauseGame = "PauseGame";
        public const string ResumeGame = "ResumeGame";
        public const string SaveGame = "SaveGame";
        public const string LoadGame = "LoadGame";
        public const string RestartGame = "RestartGame";
        public const string SetGameSpeed = "SetGameSpeed";
        public const string KillSwitchMode = "KillSwitchMode";
        
        // Specialized Commands
        public const string SetClimate = "SetClimate";
        public const string TriggerEvent = "TriggerEvent";
        public const string SpawnUnit = "SpawnUnit";
        public const string DestroyUnit = "DestroyUnit";
        public const string MoveUnit = "MoveUnit";
        public const string SetFactionRelation = "SetFactionRelation";
        public const string AddPoints = "AddPoints";
        public const string SetAIAggression = "SetAIAggression";
        
        // Special Commands
        public const string CustomCommand = "CustomCommand";
        public const string BatchCommand = "BatchCommand";
        
        /// <summary>
        /// Get all native command types
        /// </summary>
        public static readonly string[] AllNativeTypes = {
            ImportResource, SpawnResourceVein, ExhaustResourceVein,
            UnlockBuilding, AdditionalBuilding, BuildingRebuild, FactionCreateBuilding,
            ResearchTechnology, UnlockKnowledge, UnlockTech,
            StartDialogue, NotifyDialogue, ShowDialogue,
            Sabotage, GameOver, KillSwitchMode, SetOverride, SPLoadPrefab,
            CustomCommand, BatchCommand
        };
        
        /// <summary>
        /// Check if command type is a native Per Aspera command
        /// </summary>
        public static bool IsNativeCommandType(string commandType)
        {
            return Array.Exists(AllNativeTypes, t => t.Equals(commandType, StringComparison.OrdinalIgnoreCase));
        }
    }
    
    /// <summary>
    /// Parameter names used in commands
    /// </summary>
    public static class ParameterNames
    {
        // Common parameters
        public const string Faction = "faction";
        public const string Quantity = "quantity";
        public const string Amount = "amount";
        public const string Duration = "duration";
        public const string Message = "message";
        public const string Resource = "resource";
        public const string Building = "building";
        public const string Technology = "technology";
        public const string Knowledge = "knowledge";
        
        // Position parameters
        public const string Position = "position";
        public const string X = "x";
        public const string Y = "y";
        public const string Z = "z";
        
        // Dialogue parameters
        public const string Person = "person";
        public const string Dialogue = "dialogue";
        public const string DialogueId = "dialogueId";
        
        // Override parameters
        public const string Key = "key";
        public const string Value = "value";
        public const string OverrideKey = "overrideKey";
        public const string OverrideValue = "overrideValue";
        
        // Building parameters
        public const string BuildingType = "buildingType";
        public const string BuildingHandle = "buildingHandle";
        public const string BuildingCategory = "buildingCategory";
        
        // Special parameters
        public const string Force = "force";
        public const string Silent = "silent";
        public const string Validate = "validate";
    }
    
    /// <summary>
    /// Error codes for command execution
    /// </summary>
    public static class ErrorCodes
    {
        public const string ValidationFailed = "VALIDATION_FAILED";
        public const string NativeExecutionFailed = "NATIVE_EXECUTION_FAILED";
        public const string InvalidParameters = "INVALID_PARAMETERS";
        public const string FactionNotFound = "FACTION_NOT_FOUND";
        public const string ResourceNotFound = "RESOURCE_NOT_FOUND";
        public const string BuildingNotFound = "BUILDING_NOT_FOUND";
        public const string TechnologyNotFound = "TECHNOLOGY_NOT_FOUND";
        public const string CommandBusNotAvailable = "COMMAND_BUS_NOT_AVAILABLE";
        public const string KeeperNotAvailable = "KEEPER_NOT_AVAILABLE";
        public const string UnknownCommandType = "UNKNOWN_COMMAND_TYPE";
        public const string ExecutionTimeout = "EXECUTION_TIMEOUT";
        public const string InternalError = "INTERNAL_ERROR";
    }
    
    /// <summary>
    /// Constants for command system configuration
    /// </summary>
    public static class CommandConstants
    {
        /// <summary>
        /// Default timeout for command execution in milliseconds
        /// </summary>
        public const int DefaultTimeoutMs = 5000;
        
        /// <summary>
        /// Maximum number of commands in a batch
        /// </summary>
        public const int MaxBatchSize = 100;
        
        /// <summary>
        /// Maximum number of concurrent commands in parallel execution
        /// </summary>
        public const int MaxConcurrentCommands = 8;
        
        /// <summary>
        /// Default retry count for failed commands
        /// </summary>
        public const int DefaultRetryCount = 3;
        
        /// <summary>
        /// Version of the command system API
        /// </summary>
        public const string ApiVersion = "1.0.0";
    }
}
