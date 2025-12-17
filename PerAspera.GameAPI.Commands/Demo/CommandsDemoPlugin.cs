using BepInEx;
using BepInEx.Unity.IL2CPP;
using PerAspera.GameAPI.Commands;
using PerAspera.GameAPI.Commands.Constants;
using System;
using UnityEngine;

namespace PerAspera.Commands.Demo
{
    /// <summary>
    /// Demo plugin for PerAspera Commands SDK
    /// Demonstrates usage of the Commands API with Per Aspera game
    /// </summary>
    [BepInPlugin("com.perasperamods.commands.demo", "Commands SDK Demo", "1.0.0")]
    public class CommandsDemoPlugin : BasePlugin
    {
        public override void Load()
        {
            Log.LogInfo("=== PerAspera Commands SDK Demo Loading ===");
            
            try
            {
                // Initialize the SDK
                InitializeCommandsSDK();
                
                // Set up event handlers
                SetupEventHandlers();
                
                // Schedule demo commands
                ScheduleDemoCommands();
                
                Log.LogInfo("Commands SDK Demo loaded successfully!");
            }
            catch (Exception ex)
            {
                Log.LogError($"Failed to load Commands SDK Demo: {ex.Message}");
                Log.LogError(ex.StackTrace);
            }
        }
        
        private void InitializeCommandsSDK()
        {
            Log.LogInfo("Initializing Commands SDK...");
            
            // The SDK will auto-initialize when first used
            // But we can test if it's available
            var supportedCommands = Commands.GetSupportedCommandTypes();
            Log.LogInfo($"SDK supports {supportedCommands.Length} command types");
            
            foreach (var commandType in supportedCommands)
            {
                Log.LogDebug($"  - {commandType}");
            }
        }
        
        private void SetupEventHandlers()
        {
            Log.LogInfo("Setting up event handlers...");
            
            // Subscribe to command execution events
            Commands.OnCommandExecuted(evt => {
                Log.LogInfo($"✅ Command executed: {evt.CommandType} in {evt.Duration}ms");
                if (evt.Result?.Success == true)
                {
                    Log.LogInfo($"   Success: {evt.Result.Value ?? \"(no result)\"}");
                }
            });
            
            // Subscribe to command failure events  
            Commands.OnCommandFailed(evt => {
                Log.LogWarning($"❌ Command failed: {evt.CommandType}");
                Log.LogWarning($"   Error: {evt.Error}");
                if (evt.Exception != null)
                {
                    Log.LogError($"   Exception: {evt.Exception}");
                }
            });
        }
        
        private void ScheduleDemoCommands()
        {
            Log.LogInfo("Scheduling demo commands...");
            
            // Schedule commands to run after a delay to ensure game is loaded
            Application.targetFrameRate = 60; // Ensure consistent timing
            
            // Use Unity's invoke system for delayed execution
            var demoRunner = new GameObject("CommandsDemoRunner").AddComponent<DemoCommandRunner>();
            demoRunner.Plugin = this;
            
            // Don't destroy on load so it survives scene changes
            UnityEngine.Object.DontDestroyOnLoad(demoRunner.gameObject);
        }
        
        public void RunDemoCommands()
        {
            Log.LogInfo("=== Starting Commands Demo Execution ===");
            
            try
            {
                // Demo 1: Simple command execution
                DemoSimpleCommands();
                
                // Demo 2: Faction-based command chaining
                DemoFactionCommands();
                
                // Demo 3: Batch command execution
                DemoBatchCommands();
                
                // Demo 4: Parameter validation
                DemoParameterValidation();
                
                // Demo 5: Error handling
                DemoErrorHandling();
            }
            catch (Exception ex)
            {
                Log.LogError($"Demo execution failed: {ex.Message}");
                Log.LogError(ex.StackTrace);
            }
        }\n        \n        private void DemoSimpleCommands()\n        {\n            Log.LogInfo(\"--- Demo 1: Simple Commands ---\");\n            \n            // Test GameOver command (no parameters)\n            try\n            {\n                var gameOverResult = Commands.Create(NativeCommandTypes.GameOver)\n                    .Execute();\n                \n                Log.LogInfo($\"GameOver command result: {gameOverResult.Success}\");\n            }\n            catch (Exception ex)\n            {\n                Log.LogInfo($\"GameOver command failed (expected): {ex.Message}\");\n            }\n            \n            // Test SetOverride command\n            try\n            {\n                var overrideResult = Commands.SetOverride(\"demo_key\", \"demo_value\");\n                Log.LogInfo($\"SetOverride result: {overrideResult.Success}\");\n            }\n            catch (Exception ex)\n            {\n                Log.LogWarning($\"SetOverride failed: {ex.Message}\");\n            }\n        }\n        \n        private void DemoFactionCommands()\n        {\n            Log.LogInfo(\"--- Demo 2: Faction Commands ---\");\n            \n            // Create a mock faction object for testing\n            var testFaction = new { Name = \"TestFaction\", Id = 1 };\n            \n            try\n            {\n                var factionResult = Commands.ForFaction(testFaction)\n                    .ShowMessage(\"Hello from Commands SDK!\")\n                    .ShowTutorialMessage(\"This is a demo of the Commands API\")\n                    .Execute();\n                \n                Log.LogInfo($\"Faction commands result: {factionResult.Success}\");\n                Log.LogInfo($\"Executed {factionResult.Results.Count} commands\");\n            }\n            catch (Exception ex)\n            {\n                Log.LogWarning($\"Faction commands failed: {ex.Message}\");\n            }\n        }\n        \n        private void DemoBatchCommands()\n        {\n            Log.LogInfo(\"--- Demo 3: Batch Commands ---\");\n            \n            try\n            {\n                var batchResult = Commands.CreateBatch()\n                    .AddCommand(Commands.Create(NativeCommandTypes.SetOverride)\n                        .WithParameter(ParameterNames.Key, \"batch_demo_1\")\n                        .WithParameter(ParameterNames.Value, \"value1\"))\n                    .AddCommand(Commands.Create(NativeCommandTypes.SetOverride)\n                        .WithParameter(ParameterNames.Key, \"batch_demo_2\")\n                        .WithParameter(ParameterNames.Value, \"value2\"))\n                    .AddCommand(Commands.Create(NativeCommandTypes.SetOverride)\n                        .WithParameter(ParameterNames.Key, \"batch_demo_3\")\n                        .WithParameter(ParameterNames.Value, \"value3\"))\n                    .StopOnFailure(false)\n                    .Execute();\n                \n                Log.LogInfo($\"Batch execution result: {batchResult.Success}\");\n                Log.LogInfo($\"Executed {batchResult.Results.Count}/3 commands\");\n            }\n            catch (Exception ex)\n            {\n                Log.LogWarning($\"Batch commands failed: {ex.Message}\");\n            }\n        }\n        \n        private void DemoParameterValidation()\n        {\n            Log.LogInfo(\"--- Demo 4: Parameter Validation ---\");\n            \n            // Test parameter builder validation\n            try\n            {\n                var paramBuilder = new PerAspera.GameAPI.Commands.Builders.ParameterBuilder();\n                \n                // Test valid parameters\n                paramBuilder\n                    .Resource(new { Name = \"Water\", Type = \"Liquid\" })\n                    .Quantity(1000)\n                    .Position(10.5f, 0f, -5.2f)\n                    .Message(\"Valid parameters test\");\n                \n                var validParams = paramBuilder.Build();\n                Log.LogInfo($\"Valid parameters created: {validParams.Count} parameters\");\n                \n                // Test invalid parameters (should throw)\n                try\n                {\n                    paramBuilder.Clear().Quantity(-100); // Should fail\n                }\n                catch (ArgumentException ex)\n                {\n                    Log.LogInfo($\"Parameter validation working: {ex.Message}\");\n                }\n            }\n            catch (Exception ex)\n            {\n                Log.LogWarning($\"Parameter validation demo failed: {ex.Message}\");\n            }\n        }\n        \n        private void DemoErrorHandling()\n        {\n            Log.LogInfo(\"--- Demo 5: Error Handling ---\");\n            \n            // Test invalid command type\n            try\n            {\n                var invalidResult = Commands.Create(\"InvalidCommandType\").Execute();\n                Log.LogWarning(\"This should not succeed!\");\n            }\n            catch (ArgumentException ex)\n            {\n                Log.LogInfo($\"Invalid command type handled correctly: {ex.Message}\");\n            }\n            \n            // Test missing required parameters\n            try\n            {\n                var incompleteResult = Commands.Create(NativeCommandTypes.ImportResource)\n                    .WithParameter(ParameterNames.Faction, new { Id = 1 })\n                    // Missing resource and quantity\n                    .Execute();\n                Log.LogWarning(\"This should not succeed!\");\n            }\n            catch (Exception ex)\n            {\n                Log.LogInfo($\"Missing parameters handled correctly: {ex.Message}\");\n            }\n        }\n    }\n    \n    /// <summary>\n    /// MonoBehaviour component to handle delayed demo execution\n    /// </summary>\n    public class DemoCommandRunner : MonoBehaviour\n    {\n        public CommandsDemoPlugin Plugin { get; set; }\n        private float delayTimer = 5.0f; // Wait 5 seconds after load\n        private bool executed = false;\n        \n        private void Update()\n        {\n            if (!executed && delayTimer > 0)\n            {\n                delayTimer -= Time.deltaTime;\n                \n                if (delayTimer <= 0)\n                {\n                    executed = true;\n                    Plugin?.RunDemoCommands();\n                    \n                    // Show statistics after execution\n                    ShowCommandStatistics();\n                }\n            }\n        }\n        \n        private void ShowCommandStatistics()\n        {\n            try\n            {\n                var stats = Commands.GetStatistics();\n                \n                Plugin.Log.LogInfo(\"=== Commands SDK Statistics ===\");\n                Plugin.Log.LogInfo($\"Total commands executed: {stats.TotalExecuted}\");\n                Plugin.Log.LogInfo($\"Successful commands: {stats.TotalSuccessful}\");\n                Plugin.Log.LogInfo($\"Failed commands: {stats.TotalFailed}\");\n                Plugin.Log.LogInfo($\"Success rate: {stats.SuccessRate:P2}\");\n                Plugin.Log.LogInfo($\"Average execution time: {stats.AverageDuration:F2}ms\");\n                \n                if (stats.TotalExecuted > 0)\n                {\n                    Plugin.Log.LogInfo($\"Fastest command: {stats.FastestCommand?.CommandType} ({stats.FastestDuration:F2}ms)\");\n                    Plugin.Log.LogInfo($\"Slowest command: {stats.SlowestCommand?.CommandType} ({stats.SlowestDuration:F2}ms)\");\n                }\n            }\n            catch (Exception ex)\n            {\n                Plugin.Log.LogError($\"Failed to get statistics: {ex.Message}\");\n            }\n        }\n    }\n}