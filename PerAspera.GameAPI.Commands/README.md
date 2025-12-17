# PerAspera.GameAPI.Commands

🚀 **Type-safe command execution and custom command extension system for Per Aspera**

## Features

- ✅ **Type-safe API** for all 55 native Per Aspera commands
- ✅ **Fluent API** for chaining multiple commands 
- ✅ **Builder Pattern** for complex command construction
- ✅ **Event System** for command monitoring and debugging
- ✅ **Extensions** for natural SDK integration
- ✅ **Custom Commands** support for mod developers

## Quick Start

```csharp
using PerAspera.GameAPI.Commands;

// Simple command execution
Commands.ImportResource(faction, ResourceType.Water, 1000);
Commands.UnlockBuilding(faction, BuildingType.SolarPanel);

// Fluent API
var result = Commands.ForFaction(playerFaction)
    .ImportResource(ResourceType.Water, 1000)
    .UnlockBuilding(BuildingType.SolarPanel)
    .ResearchTechnology(TechnologyType.AdvancedSolar)
    .Execute();

// Builder pattern for complex commands
var result = Commands.Create("ImportResource")
    .WithFaction(faction)
    .WithParameter("resource", ResourceType.Water)
    .WithParameter("quantity", 1000)
    .ValidateParameters()
    .Execute();
```

## Architecture

This module implements a bridge between the Per Aspera SDK and the native CommandBus system:

```
SDK Commands API → CommandBus.Dispatch<T>() → Keeper.Register() → Game Engine
```

See the [Commands System Specification](../../../DOC/SDK/GameAPI/Commands-System-Specification.md) for detailed architecture information.

## Documentation

- **[API Reference](docs/api-reference.md)** - Complete API documentation
- **[Command Types](docs/command-types.md)** - List of all 55 native commands
- **[Examples](docs/examples.md)** - Usage examples and patterns
- **[Custom Commands](docs/custom-commands.md)** - Creating custom command types
