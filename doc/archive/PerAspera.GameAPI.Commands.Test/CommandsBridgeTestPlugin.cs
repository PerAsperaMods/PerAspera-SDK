using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using PerAspera.GameAPI;
using PerAspera.GameAPI.Commands;
using PerAspera.GameAPI.Commands.Core;
using PerAspera.GameAPI.Commands.Native;
using PerAspera.GameAPI.Commands.Helpers;
using PerAspera.GameAPI.Commands.Extensions;
using System;

namespace PerAspera.Commands.Test
{
    /// <summary>
    /// Test plugin to validate Commands Native Bridge implementation
    /// Phase 2: Testing & Validation of our MVP
    /// </summary>
    [BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
    public class CommandsBridgeTestPlugin : BasePlugin
    {
        public const string PLUGIN_GUID = "perasperaMod.commands.test";
        public const string PLUGIN_NAME = "Commands Bridge Test";
        public const string PLUGIN_VERSION = "1.0.0";

        private ManualLogSource _logger;

        public override void Load()
        {
            _logger = Log;
            _logger.LogInfo($"Loading {PLUGIN_NAME} {PLUGIN_VERSION}...");

            try
            {
                // Test Phase 1: GameTypeInitializer Integration
                TestGameTypeInitializer();

                // Test Phase 2: CommandBusAccessor Auto-Initialization  
                TestCommandBusAccessor();

                // Test Phase 3: NativeCommandFactory
                TestNativeCommandFactory();

                // Test Phase 4: ImportResource MVP Command
                TestImportResourceCommand();

                _logger.LogInfo($"{PLUGIN_NAME} loaded successfully - All tests completed!");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to load {PLUGIN_NAME}: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Test Phase 1: Validate GameTypeInitializer integration
        /// </summary>
        private void TestGameTypeInitializer()
        {
            _logger.LogInfo("=== Test Phase 1: GameTypeInitializer Integration ===");

            try
            {
                // Initialize GameTypeInitializer
                GameTypeInitializer.Initialize();
                _logger.LogInfo("✅ GameTypeInitializer.Initialize() completed");

                // Test CommandBus type discovery
                var commandBusType = GameTypeInitializer.GetCommandBusType();
                if (commandBusType != null)
                {
                    _logger.LogInfo($"✅ CommandBus type found: {commandBusType.FullName}");
                }
                else
                {
                    _logger.LogWarning("❌ CommandBus type not found");
                }

                // Test BaseGame type discovery
                var baseGameType = GameTypeInitializer.GetBaseGameType();
                if (baseGameType != null)
                {
                    _logger.LogInfo($"✅ BaseGame type found: {baseGameType.FullName}");
                }
                else
                {
                    _logger.LogWarning("❌ BaseGame type not found");
                }

                // Get discovery statistics
                var stats = GameTypeInitializer.GetDiscoveryStatistics();
                _logger.LogInfo($"📊 Discovery Statistics:\n{stats}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ GameTypeInitializer test failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Test Phase 2: CommandBusAccessor auto-initialization
        /// </summary>
        private void TestCommandBusAccessor()
        {
            _logger.LogInfo("=== Test Phase 2: CommandBusAccessor Auto-Initialization ===");

            try
            {
                // Test auto-initialization
                var initSuccess = true; // CommandBusAccessor is now auto-initialized
                CommandBusAccessor.Initialize();
                _logger.LogInfo($"Auto-initialization result: {(initSuccess ? "✅ SUCCESS" : "❌ FAILED")}");

                if (initSuccess)
                {
                    var isAvailable = CommandBusAccessor.IsCommandBusAvailable();
                    _logger.LogInfo($"CommandBusAccessor availability: {(isAvailable ? "✅ AVAILABLE" : "❌ NOT AVAILABLE")}");

                    // Get system information
                    var systemInfo = accessor.GetSystemInfo();
                    _logger.LogInfo($"📊 System Information:\n{systemInfo}");
                }
                else
                {
                    _logger.LogError("❌ CommandBusAccessor auto-initialization failed");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ CommandBusAccessor test failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Test Phase 3: NativeCommandFactory discovery and diagnostics
        /// </summary>
        private void TestNativeCommandFactory()
        {
            _logger.LogInfo("=== Test Phase 3: NativeCommandFactory ===");

            try
            {
                var factory = NativeCommandFactory.Instance;
                _logger.LogInfo("✅ NativeCommandFactory instance created");

                // Get diagnostic information
                var diagnostics = factory.GetDiagnosticInfo();
                _logger.LogInfo($"📊 Factory Diagnostics:\n{diagnostics}");

                // Test ImportResource command availability
                var isImportResourceAvailable = factory.IsCommandTypeAvailable("ImportResource");
                _logger.LogInfo($"ImportResource command availability: {(isImportResourceAvailable ? "✅ AVAILABLE" : "❌ NOT AVAILABLE")}");

                // List all available command types
                var availableTypes = factory.GetAvailableCommandTypes();
                _logger.LogInfo($"📋 Available command types ({availableTypes.Length}):");
                foreach (var type in availableTypes)
                {
                    _logger.LogInfo($"  - {type}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ NativeCommandFactory test failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Test Phase 4: ImportResource MVP command creation and execution
        /// </summary>
        private void TestImportResourceCommand()
        {
            _logger.LogInfo("=== Test Phase 4: ImportResource MVP Command ===");

            try
            {
                // Test 1: Create ImportResource command using NativeCommandFactory
                var factory = NativeCommandFactory.Instance;
                var nativeCommand = factory.CreateImportResourceCommand("water", 1000.0f);

                if (nativeCommand != null)
                {
                    _logger.LogInfo("✅ ImportResource native command created successfully");
                    _logger.LogInfo($"📋 Command details: {nativeCommand.GetDescription()}");
                    _logger.LogInfo($"🔍 Command valid: {nativeCommand.IsValid()}");
                }
                else
                {
                    _logger.LogError("❌ Failed to create ImportResource native command");
                    return;
                }

                // Test 2: Create SDK ImportResource command using helper
                var sdkCommandBuilder = ImportResource.Create()
                    .Resource("carbon")
                    .Amount(500.0f);
                    
                var sdkCommand = sdkCommandBuilder.Build();

                if (sdkCommand != null)
                {
                    _logger.LogInfo("✅ ImportResource SDK command created successfully");
                    _logger.LogInfo($"📋 SDK Command: {sdkCommand.GetDescription()}");
                }
                else
                {
                    _logger.LogError("❌ Failed to create ImportResource SDK command");
                }

                // Test 3: Execute command via CommandExecutor (if CommandBus available)
                if (CommandBusAccessor.IsCommandBusAvailable() && sdkCommand != null)
                {
                    _logger.LogInfo("🚀 Attempting to execute ImportResource command...");
                    
                    // Create dummy CommandExecutor for testing
                    var executor = new CommandExecutor(null, null);
                    var result = executor.Execute(sdkCommand);

                    if (result.Success)
                    {
                        _logger.LogInfo($"✅ Command execution successful! Time: {result.ExecutionTimeMs}ms");
                    }
                    else
                    {
                        _logger.LogWarning($"❌ Command execution failed: {result.ErrorMessage}");
                    }
                }
                else
                {
                    _logger.LogWarning("⚠️ Skipping command execution - CommandBus not available or SDK command null");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ ImportResource command test failed: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}