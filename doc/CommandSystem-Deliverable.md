# Command System Implementation - Deliverable Documentation

## Overview

This document describes the implementation of the Command Handling System for the PerAspera ModSDK, which provides a standardized way for mods to interact with the game's command bus and execute game commands.

## Purpose

The Command System enables:
- **Command Execution**: Execute game commands through a unified interface
- **Command Bus Integration**: Access and interact with the game's native command bus
- **Type-Safe Commands**: Define and execute commands with proper type safety
- **BepInEx Integration**: Full compatibility with BepInEx modding framework
- **Event-Driven Architecture**: Integrate with the existing event system

## Architecture

### Components

1. **PerAspera.GameAPI.Commands** - Core command handling library
   - `CommandBusAccessor` - Provides access to the game's command bus
   - `ICommand` - Base interface for all commands
   - `CommandBase` - Abstract base class for command implementation
   - `CommandExecutor` - Handles command execution and validation

2. **Integration Points**
   - Integrates with `PerAspera.GameAPI` for game type discovery
   - Uses `PerAspera.Core` for logging and utilities
   - Publishes events through `PerAspera.GameAPI.Events`

### Design Principles

- **Separation of Concerns**: Command logic is separated from execution
- **Dependency Injection**: Command bus is injected, not hardcoded
- **Error Handling**: Comprehensive error handling and logging
- **Extensibility**: Easy to add new command types
- **Performance**: Minimal overhead and efficient execution

## Implementation Details

### CommandBusAccessor

Provides safe access to the game's native command bus:

```csharp
public static class CommandBusAccessor
{
    // Gets the command bus instance from the game
    public static object? GetCommandBus();
    
    // Checks if command bus is available
    public static bool IsCommandBusAvailable();
    
    // Executes a command on the bus
    public static void ExecuteCommand(ICommand command);
}
```

### ICommand Interface

All commands must implement this interface:

```csharp
public interface ICommand
{
    string CommandName { get; }
    object? Execute();
    bool CanExecute();
}
```

### Command Base Class

Provides common functionality for commands:

```csharp
public abstract class CommandBase : ICommand
{
    protected readonly LogAspera Logger;
    
    public abstract string CommandName { get; }
    public abstract object? Execute();
    public virtual bool CanExecute() => true;
}
```

## BepInEx Integration

The Command System integrates with BepInEx in the following ways:

1. **Logging**: Uses BepInEx's logging system through LogAspera wrapper
2. **IL2CPP Support**: Works with IL2CPP-based games through proper type handling
3. **Plugin Lifecycle**: Initializes during mod loading, cleanup on unload
4. **Configuration**: Can be configured through BepInEx configuration system

### References

- BepInEx Documentation: https://docs.bepinex.dev/
- BepInEx IL2CPP: https://docs.bepinex.dev/articles/dev_guide/plugin_tutorial/index.html
- BepInEx API: https://docs.bepinex.dev/api/

## Usage Example

```csharp
using PerAspera.ModSDK;
using PerAspera.GameAPI.Commands;

[BepInPlugin("com.example.commandmod", "Command Example Mod", "1.0.0")]
public class CommandExampleMod : PerAsperaSDKPlugin
{
    protected override void OnSDKReady()
    {
        // Check if command bus is available
        if (CommandBusAccessor.IsCommandBusAvailable())
        {
            Logger.LogInfo("Command bus is available!");
            
            // Execute a custom command
            var myCommand = new MyCustomCommand();
            if (myCommand.CanExecute())
            {
                CommandBusAccessor.ExecuteCommand(myCommand);
            }
        }
    }
}

// Custom command implementation
public class MyCustomCommand : CommandBase
{
    public override string CommandName => "MyCommand";
    
    public override object? Execute()
    {
        Logger.Info("Executing my custom command!");
        // Command logic here
        return null;
    }
}
```

## Testing Strategy

1. **Unit Tests**: Test command execution and validation
2. **Integration Tests**: Test command bus integration
3. **Runtime Tests**: Test in actual game environment with BepInEx

## Dependencies

- PerAspera.Core (>= 1.1.0)
- PerAspera.GameAPI (>= 1.1.0)
- PerAspera.GameAPI.Events (>= 1.1.0)
- BepInEx.Unity.IL2CPP (>= 6.0.0)

## Future Enhancements

1. **Command Queue**: Queue commands for execution
2. **Command History**: Track executed commands
3. **Command Validation**: Enhanced validation and error handling
4. **Remote Commands**: Support for remote command execution
5. **Command Batching**: Execute multiple commands as a batch

## Version History

- **1.0.0** (2025-12-17): Initial implementation
  - Basic command bus accessor
  - Command interface and base classes
  - BepInEx integration

## Authors

PerAspera Modding Community

## License

MIT License - See LICENSE file for details
