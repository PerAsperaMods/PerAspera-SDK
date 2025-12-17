# PerAspera.GameAPI.Commands

Command handling system for the PerAspera ModSDK. Provides a standardized way to interact with the game's command bus and execute game commands.

## Features

- **Command Bus Access**: Safe access to the game's native command bus
- **Type-Safe Commands**: Define and execute commands with proper type safety
- **Error Handling**: Comprehensive error handling and logging
- **Command Queue**: Queue commands for batch execution
- **BepInEx Integration**: Full compatibility with BepInEx modding framework

## Installation

This package is part of the PerAspera ModSDK and is automatically included when you reference the main SDK:

```xml
<PackageReference Include="PerAspera.ModSDK" Version="1.1.0" />
```

Or reference directly:

```xml
<ProjectReference Include="..\PerAspera.GameAPI.Commands\PerAspera.GameAPI.Commands.csproj" />
```

## Usage

### Basic Command Execution

```csharp
using PerAspera.GameAPI.Commands;

// Check if command bus is available
if (CommandBusAccessor.IsCommandBusAvailable())
{
    // Create and execute a command
    var myCommand = new MyCustomCommand();
    CommandBusAccessor.ExecuteCommand(myCommand);
}
```

### Creating Custom Commands

```csharp
using PerAspera.GameAPI.Commands;

public class MyCustomCommand : CommandBase
{
    public override string CommandName => "MyCommand";
    
    public override object? Execute()
    {
        Logger.Info("Executing my custom command!");
        
        // Your command logic here
        // Access game systems, modify state, etc.
        
        return null; // or return result
    }
    
    public override bool CanExecute()
    {
        // Check if command can be executed
        return CommandBusAccessor.IsCommandBusAvailable();
    }
}
```

### Using Command Executor

```csharp
using PerAspera.GameAPI.Commands;

var executor = new CommandExecutor();

// Execute command immediately
var command = new MyCustomCommand();
var result = executor.ExecuteCommand(command);

// Queue commands for later execution
executor.QueueCommand(new Command1());
executor.QueueCommand(new Command2());
executor.QueueCommand(new Command3());

// Execute all queued commands
int executed = executor.ExecuteQueuedCommands();
```

### Safe Command Execution

```csharp
// Try execute without throwing exceptions
if (CommandBusAccessor.TryExecuteCommand(myCommand))
{
    Logger.Info("Command executed successfully");
}
else
{
    Logger.Warning("Command execution failed");
}
```

## Architecture

### Core Components

1. **ICommand** - Base interface for all commands
   - `CommandName` - Unique identifier for the command
   - `Execute()` - Execute the command logic
   - `CanExecute()` - Check if command can be executed

2. **CommandBase** - Abstract base class for commands
   - Provides logging infrastructure
   - Implements basic validation
   - Handles error cases

3. **CommandBusAccessor** - Access to game's command bus
   - Discovers command bus type
   - Manages command bus instance
   - Routes commands to the game

4. **CommandExecutor** - Command execution manager
   - Executes commands with error handling
   - Manages command queue
   - Provides execution statistics

## Integration with Game

The command system integrates with the game through:

1. **Type Discovery**: Uses `GameTypeInitializer` to find the game's `CommandBus` type
2. **Reflection**: Accesses command bus instance via reflection
3. **Command Execution**: Routes commands through the game's command infrastructure

## BepInEx Integration

The command system is designed to work seamlessly with BepInEx:

- Uses BepInEx logging through `LogAspera` wrapper
- Compatible with IL2CPP games
- Initializes during plugin load
- Cleans up on plugin unload

### Example Plugin

```csharp
using BepInEx;
using BepInEx.Unity.IL2CPP;
using PerAspera.ModSDK;
using PerAspera.GameAPI.Commands;

[BepInPlugin("com.example.commandmod", "Command Example", "1.0.0")]
public class CommandExampleMod : PerAsperaSDKPlugin
{
    protected override void OnSDKReady()
    {
        Logger.LogInfo("Command mod loaded!");
        
        // Initialize command system
        CommandBusAccessor.Initialize();
        
        if (CommandBusAccessor.IsCommandBusAvailable())
        {
            Logger.LogInfo("Command bus is ready!");
            
            // Execute your commands
            var command = new MyCustomCommand();
            CommandBusAccessor.ExecuteCommand(command);
        }
    }
}
```

## API Reference

### CommandBusAccessor

```csharp
// Initialize the accessor
CommandBusAccessor.Initialize();

// Get command bus instance
object? commandBus = CommandBusAccessor.GetCommandBus();

// Check availability
bool available = CommandBusAccessor.IsCommandBusAvailable();

// Execute command
CommandBusAccessor.ExecuteCommand(command);

// Safe execute (no exceptions)
bool success = CommandBusAccessor.TryExecuteCommand(command);

// Get command bus type
Type? busType = CommandBusAccessor.GetCommandBusType();

// Reset (for testing)
CommandBusAccessor.Reset();
```

### CommandExecutor

```csharp
var executor = new CommandExecutor();

// Execute immediately
object? result = executor.ExecuteCommand(command);

// Safe execute
bool success = executor.TryExecuteCommand(command, out var result);

// Queue for later
executor.QueueCommand(command);

// Execute queue
int count = executor.ExecuteQueuedCommands();

// Queue management
int pending = executor.QueuedCommandCount;
executor.ClearQueue();
```

## Error Handling

The command system provides comprehensive error handling:

```csharp
try
{
    CommandBusAccessor.ExecuteCommand(command);
}
catch (ArgumentNullException)
{
    // Command was null
}
catch (InvalidOperationException)
{
    // Command bus not available
}
catch (CommandExecutionException ex)
{
    // Command execution failed
    Logger.Error($"Command failed: {ex.Message}");
}
```

## Best Practices

1. **Check Availability**: Always check if command bus is available before executing commands
2. **Use Safe Methods**: Use `TryExecuteCommand` for non-critical operations
3. **Queue Commands**: Use command queue for batch operations
4. **Implement CanExecute**: Always implement proper `CanExecute()` logic
5. **Handle Errors**: Wrap command execution in try-catch blocks
6. **Log Operations**: Use the provided logger for debugging

## Dependencies

- **PerAspera.Core** - Core utilities and logging
- **PerAspera.GameAPI** - Game type discovery
- **PerAspera.GameAPI.Events** - Event system integration
- **BepInEx.Unity.IL2CPP** - BepInEx framework

## Version History

- **1.1.0** (2025-12-17): Initial release
  - Basic command interface and base classes
  - Command bus accessor with type discovery
  - Command executor with queue support
  - Full BepInEx integration

## License

MIT License - See LICENSE file for details

## Contributing

Contributions are welcome! Please see the main SDK repository for contribution guidelines.

## Support

- **Documentation**: See `/doc/CommandSystem-Deliverable.md` for detailed documentation
- **Issues**: Report issues on the GitHub repository
- **Discord**: Join the PerAspera modding community
