using System;

namespace PerAspera.GameAPI.Commands.Constants
{
    /// &lt;summary&gt;
    /// Types of native Per Aspera commands
    /// &lt;/summary&gt;
    public static class NativeCommandTypes
    {
        // Resource Commands
        public const string ImportResource = "ImportResource";
        public const string SpawnResourceVein = "SpawnResourceVein";
        public const string ExhaustResourceVein = "ExhaustResourceVein";
        
        // Building Commands  
        public const string UnlockBuilding = "UnlockBuilding";
        public const string AdditionalBuilding = "AdditionalBuilding";
        public const string BuildingRebuild = "BuildingRebuild";
        public const string FactionCreateBuilding = "FactionCreateBuilding";
        
        // Technology Commands
        public const string ResearchTechnology = "ResearchTechnology";
        public const string UnlockKnowledge = "UnlockKnowledge";
        public const string UnlockTech = "UnlockTech";
        
        // Dialogue Commands
        public const string StartDialogue = "StartDialogue";
        public const string NotifyDialogue = "NotifyDialogue";
        public const string ShowDialogue = "ShowDialogue";
        
        // Universe Commands
        public const string Sabotage = "Sabotage";
        public const string GameOver = "GameOver";
        public const string KillSwitchMode = "KillSwitchMode";
        public const string SetOverride = "SetOverride";
        public const string SPLoadPrefab = "SPLoadPrefab";
        
        // Special Commands
        public const string CustomCommand = "CustomCommand";
        public const string BatchCommand = "BatchCommand";
        
        /// &lt;summary&gt;
        /// Get all native command types
        /// &lt;/summary&gt;
        public static readonly string[] AllNativeTypes = {
            ImportResource, SpawnResourceVein, ExhaustResourceVein,
            UnlockBuilding, AdditionalBuilding, BuildingRebuild, FactionCreateBuilding,
            ResearchTechnology, UnlockKnowledge, UnlockTech,
            StartDialogue, NotifyDialogue, ShowDialogue,
            Sabotage, GameOver, KillSwitchMode, SetOverride, SPLoadPrefab,
            CustomCommand, BatchCommand
        };
        
        /// &lt;summary&gt;
        /// Check if command type is a native Per Aspera command
        /// &lt;/summary&gt;
        public static bool IsNativeCommandType(string commandType)
        {
            return Array.Exists(AllNativeTypes, t =&gt; t.Equals(commandType, StringComparison.OrdinalIgnoreCase));
        }
    }
    
    /// &lt;summary&gt;
    /// Parameter names used in commands
    /// &lt;/summary&gt;
    public static class ParameterNames
    {
        // Common parameters
        public const string Faction = "faction";
        public const string Quantity = "quantity";
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
    
    /// &lt;summary&gt;
    /// Error codes for command execution
    /// &lt;/summary&gt;
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
    
    /// &lt;summary&gt;
    /// Constants for command system configuration
    /// &lt;/summary&gt;
    public static class CommandConstants
    {
        /// &lt;summary&gt;
        /// Default timeout for command execution in milliseconds
        /// &lt;/summary&gt;
        public const int DefaultTimeoutMs = 5000;
        
        /// &lt;summary&gt;
        /// Maximum number of commands in a batch
        /// &lt;/summary&gt;
        public const int MaxBatchSize = 100;
        
        /// &lt;summary&gt;
        /// Maximum number of concurrent commands in parallel execution
        /// &lt;/summary&gt;
        public const int MaxConcurrentCommands = 8;
        
        /// &lt;summary&gt;
        /// Default retry count for failed commands
        /// &lt;/summary&gt;
        public const int DefaultRetryCount = 3;
        
        /// &lt;summary&gt;
        /// Version of the command system API
        /// &lt;/summary&gt;
        public const string ApiVersion = "1.0.0";
    }
}