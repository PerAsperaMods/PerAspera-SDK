using System;
using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using PerAspera.GameAPI.Commands;
using PerAspera.GameAPI.Commands.Constants;
using PerAspera.GameAPI.Commands.Builders;
using BepInEx.Unity.IL2CPP;

namespace PerAspera.GameAPI.Commands.Demo
{
    /// <summary>
    /// Demo plugin showing Commands SDK usage patterns
    /// Demonstrates the main features and usage patterns of the Commands SDK
    /// </summary>
    [BepInPlugin("PerAspera.GameAPI.Commands.Demo", "Commands SDK Demo", "1.0.0")]
    [BepInDependency("PerAspera.GameAPI.Commands", BepInDependency.DependencyFlags.HardDependency)]
    public class CommandsDemoPlugin : BasePlugin
    {
        internal new static ManualLogSource Logger { get; private set; }
        
        /// <summary>
        /// Required Load() method for BasePlugin
        /// </summary>
        public override void Load()
        {
            Logger = Log;
            Logger.LogInfo("Commands Demo Plugin Loaded");
        }
        
        /// <summary>
        /// Plugin initialization - register event handlers and schedule demo
        /// </summary>
        private void Awake()
        {
            Logger = base.Logger;
            Logger.LogInfo("Commands SDK Demo Plugin loading...");
            
            try
            {
                // Initialize Commands SDK
                Commands.Initialize();
                
                // Set up event handlers first
                SetupEventHandlers();
                
                // Schedule demo execution after game loads
                var runner = gameObject.AddComponent<DemoCommandRunner>();
                runner.Plugin = this;
                
                Logger.LogInfo("Commands SDK Demo Plugin loaded successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to initialize Commands SDK Demo: {ex.Message}");
                Logger.LogError(ex.StackTrace);
            }
        }
        
        /// <summary>
        /// Set up event handlers for command execution monitoring
        /// </summary>
        private void SetupEventHandlers()
        {
            Logger.LogInfo("Setting up Commands SDK event handlers...");
            
            // Subscribe to command execution events
            Commands.OnCommandExecuted(evt => {
                Logger.LogInfo($"✅ Command executed: {evt.CommandType} in {evt.Duration}ms");
                if (evt.Result?.Success == true)
                {
                    Logger.LogInfo($"   Success: {evt.Result.Value ?? "(no result)"}");
                }
            });
            
            // Subscribe to command failure events  
            Commands.OnCommandFailed(evt => {
                Logger.LogWarning($"❌ Command failed: {evt.CommandType} - {evt.Error?.Message ?? "Unknown error"}");
                if (evt.Error != null)
                {
                    Logger.LogWarning($"   Error: {evt.Error.Message}");
                }
            });
            
            // Subscribe to batch execution events
            Commands.OnBatchExecuted(evt => {
                Logger.LogInfo($"📦 Batch executed: {evt.SuccessCount}/{evt.TotalCommands} commands succeeded");
                Logger.LogInfo($"   Duration: {evt.TotalDuration}ms, Success Rate: {evt.SuccessRate:P1}");
            });
        }
        
        /// <summary>
        /// Main demo execution method
        /// Called by DemoCommandRunner after initialization delay
        /// </summary>
        public void RunDemoCommands()
        {
            Logger.LogInfo("=== Starting Commands SDK Demo ===");
            
            try
            {
                // Demo 1: Simple command creation and execution
                DemoSimpleCommands();
                
                // Demo 2: Faction-specific commands
                DemoFactionCommands();
                
                // Demo 3: Batch command execution
                DemoBatchCommands();
                
                // Demo 4: Parameter validation
                DemoParameterValidation();
                
                // Demo 5: Error handling patterns
                DemoErrorHandling();
                
                Logger.LogInfo("=== Commands SDK Demo Completed ===");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Demo execution failed: {ex.Message}");
                Logger.LogError(ex.StackTrace);
            }
        }
        
        private void DemoSimpleCommands()
        {
            Logger.LogInfo("--- Demo 1: Simple Commands ---");
            
            // Test GameOver command (no parameters)
            try
            {
                var gameOverResult = Commands.Create(NativeCommandTypes.GameOver)
                    .Execute();
                
                Logger.LogInfo($"GameOver command result: {gameOverResult.Success}");
            }
            catch (Exception ex)
            {
                Logger.LogInfo($"GameOver command failed (expected): {ex.Message}");
            }
            
            // Test SetOverride command
            try
            {
                var overrideResult = Commands.SetOverride("demo_key", "demo_value");
                Logger.LogInfo($"SetOverride result: {overrideResult.Success}");
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"SetOverride failed: {ex.Message}");
            }
        }
        
        private void DemoFactionCommands()
        {
            Logger.LogInfo("--- Demo 2: Faction Commands ---");
            
            // Create a mock faction object for testing
            var testFaction = new { Name = "TestFaction", Id = 1 };
            
            try
            {
                var factionResult = Commands.ForFaction(testFaction)
                    .ShowMessage("Hello from Commands SDK!")
                    .ShowTutorialMessage("This is a demo of the Commands API")
                    .Execute();
                
                Logger.LogInfo($"Faction commands result: {factionResult.Success}");
                Logger.LogInfo($"Executed {factionResult.Results.Count} commands");
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Faction commands failed: {ex.Message}");
            }
        }
        
        private void DemoBatchCommands()
        {
            Logger.LogInfo("--- Demo 3: Batch Commands ---");
            
            try
            {
                var batchResult = Commands.CreateBatch()
                    .AddCommand(Commands.Create(NativeCommandTypes.SetOverride)
                        .WithParameter(ParameterNames.Key, "batch_demo_1")
                        .WithParameter(ParameterNames.Value, "value1"))
                    .AddCommand(Commands.Create(NativeCommandTypes.SetOverride)
                        .WithParameter(ParameterNames.Key, "batch_demo_2")
                        .WithParameter(ParameterNames.Value, "value2"))
                    .AddCommand(Commands.Create(NativeCommandTypes.SetOverride)
                        .WithParameter(ParameterNames.Key, "batch_demo_3")
                        .WithParameter(ParameterNames.Value, "value3"))
                    .StopOnFailure(false)
                    .Execute();
                
                Logger.LogInfo($"Batch execution result: {batchResult.Success}");
                Logger.LogInfo($"Executed {batchResult.Results.Count}/3 commands");
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Batch commands failed: {ex.Message}");
            }
        }
        
        private void DemoParameterValidation()
        {
            Logger.LogInfo("--- Demo 4: Parameter Validation ---");
            
            // Test parameter builder validation
            try
            {
                var paramBuilder = new ParameterBuilder();
                
                // Test valid parameters
                paramBuilder
                    .Resource(new { Name = "Water", Type = "Liquid" })
                    .Quantity(1000)
                    .Position(10.5f, 0f, -5.2f)
                    .Message("Valid parameters test");
                
                var validParams = paramBuilder.Build();
                Logger.LogInfo($"Valid parameters created: {validParams.Count} parameters");
                
                // Test invalid parameters (should throw)
                try
                {
                    paramBuilder.Clear().Quantity(-100); // Should fail
                }
                catch (ArgumentException ex)
                {
                    Logger.LogInfo($"Parameter validation working: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Parameter validation demo failed: {ex.Message}");
            }
        }
        
        private void DemoErrorHandling()
        {
            Logger.LogInfo("--- Demo 5: Error Handling ---");
            
            // Test invalid command type
            try
            {
                var invalidResult = Commands.Create("InvalidCommandType").Execute();
                Logger.LogWarning("This should not succeed!");
            }
            catch (ArgumentException ex)
            {
                Logger.LogInfo($"Invalid command type handled correctly: {ex.Message}");
            }
            
            // Test missing required parameters
            try
            {
                var incompleteResult = Commands.Create(NativeCommandTypes.ImportResource)
                    .WithParameter(ParameterNames.Faction, new { Id = 1 })
                    // Missing resource and quantity
                    .Execute();
                Logger.LogWarning("This should not succeed!");
            }
            catch (Exception ex)
            {
                Logger.LogInfo($"Missing parameters handled correctly: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// MonoBehaviour component to handle delayed demo execution
    /// </summary>
    public class DemoCommandRunner : MonoBehaviour
    {
        public CommandsDemoPlugin Plugin { get; set; }
        private float delayTimer = 5.0f; // Wait 5 seconds after load
        private bool executed = false;
        
        private void Update()
        {
            if (!executed && delayTimer > 0)
            {
                delayTimer -= Time.deltaTime;
                
                if (delayTimer <= 0)
                {
                    executed = true;
                    Plugin?.RunDemoCommands();
                    
                    // Show statistics after execution
                    ShowCommandStatistics();
                }
            }
        }
        
        private void ShowCommandStatistics()
        {
            try
            {
                var stats = Commands.GetStatistics();
                
                Plugin.Logger.LogInfo("=== Commands SDK Statistics ===");
                Plugin.Logger.LogInfo($"Total commands executed: {stats.TotalExecuted}");
                Plugin.Logger.LogInfo($"Successful commands: {stats.TotalSuccessful}");
                Plugin.Logger.LogInfo($"Failed commands: {stats.TotalFailed}");
                Plugin.Logger.LogInfo($"Success rate: {stats.SuccessRate:P2}");
                Plugin.Logger.LogInfo($"Average execution time: {stats.AverageDuration:F2}ms");
                
                if (stats.TotalExecuted > 0)
                {
                    Plugin.Logger.LogInfo($"Fastest command: {stats.FastestCommand?.CommandType} ({stats.FastestDuration:F2}ms)");
                    Plugin.Logger.LogInfo($"Slowest command: {stats.SlowestCommand?.CommandType} ({stats.SlowestDuration:F2}ms)");
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"Failed to get statistics: {ex.Message}");
            }
        }
    }
}