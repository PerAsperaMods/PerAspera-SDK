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

## Documentation

📚 **Complete documentation has been moved to:**

**[DOC/SDK/Commands/](../../DOC/SDK/Commands/README.md)**

### Quick Links
- **[Getting Started](../../DOC/SDK/Commands/Quick-Start.md)** - Setup and basic usage
- **[API Reference](../../DOC/SDK/Commands/API-Reference.md)** - Complete API documentation  
- **[Examples](../../DOC/SDK/Commands/Examples.md)** - Usage patterns and examples
- **[Integration Guide](../../DOC/SDK/Commands/Integration-Guide.md)** - BepInX IL2CPP setup

### Architecture Overview

This module implements a bridge between the Per Aspera SDK and the native CommandBus system:

```
SDK Commands API → CommandBus.Dispatch<T>() → Keeper.Register() → Game Engine
```

For detailed architecture information, see **[Architecture Guide](../../DOC/SDK/Commands/Architecture.md)**.
