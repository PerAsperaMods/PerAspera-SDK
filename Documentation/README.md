# PerAspera Modding SDK

**Professional modding framework for Per Aspera - The Mars terraforming strategy game**

[![Version](https://img.shields.io/badge/version-1.1.0-blue.svg)](https://github.com/PerAsperaMods/PerAspera-SDK/releases)
[![Framework](https://img.shields.io/badge/BepInEx-6.x%20IL2CPP-green.svg)](https://github.com/BepInEx/BepInEx)
[![Game Support](https://img.shields.io/badge/Per%20Aspera-Latest-orange.svg)](https://store.steampowered.com/app/944290/Per_Aspera/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

## 🌟 What is PerAspera SDK?

The PerAspera SDK is a comprehensive modding framework that provides developers with powerful tools to create sophisticated mods for Per Aspera. Built on BepInEx 6.x with IL2CPP support, it offers high-level APIs for game interaction, climate simulation, building systems, and resource management.

### ✨ Key Features

- **🎮 Game Integration**: Direct access to game systems (BaseGame, Universe, Planet)
- **🌡️ Climate System**: Advanced atmospheric and terraforming simulation
- **🏗️ Building API**: Monitor and modify building behavior and atmospheric impact
- **⚙️ Override System**: Dynamic runtime modification of game values
- **📊 Event System**: Comprehensive event handling for all game activities
- **🔧 Developer Tools**: Intelligent configuration, auto-detection, debugging utilities

## 🚀 Quick Start

### Prerequisites

- **Visual Studio 2022** or **VS Code** with C# extension
- **.NET 6.0 SDK** or later
- **Per Aspera** game installed
- **BepInEx 6.x IL2CPP** (auto-installed via NuGet)

### 1. Create Your First Mod

```bash
# Clone the SDK
git clone https://github.com/PerAsperaMods/PerAspera-SDK.git
cd PerAspera-SDK

# Create new mod project
dotnet new console -n MyFirstMod
cd MyFirstMod
```

### 2. Add SDK Reference

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="PerAspera.ModSDK" Version="1.1.0" />
  </ItemGroup>
</Project>
```

### 3. Create Basic Plugin

```csharp
using BepInEx;
using BepInEx.Unity.IL2CPP;
using PerAspera.ModSDK;

[BepInPlugin("com.yourname.myfirstmod", "My First Mod", "1.0.0")]
public class MyFirstModPlugin : BasePlugin
{
    public override void Load()
    {
        Log.LogInfo("Loading My First Mod...");

        // Initialize SDK
        ModSDK.Initialize(this);

        // Subscribe to game events
        ModSDK.Events.Subscribe("climate.temperatureChanged", OnTemperatureChanged);

        Log.LogInfo("My First Mod loaded successfully!");
    }

    private void OnTemperatureChanged(object eventData)
    {
        Log.LogInfo($"🌡️ Mars temperature changed: {eventData}");
    }
}
```

### 4. Build and Deploy

```bash
dotnet build --configuration Release
# DLL automatically copies to Per Aspera BepInEx/plugins/
```

## 📚 Documentation Structure

### Core Guides
- **[Installation & Setup](Installation.md)** - Complete environment setup
- **[Architecture Overview](Architecture.md)** - SDK design and component structure
- **[API Reference](API-Reference.md)** - Complete API documentation

### Development Guides
- **[Creating Your First Mod](Tutorials/FirstMod.md)** - Step-by-step beginner guide
- **[Climate System Guide](Guides/ClimateSystem.md)** - Atmospheric and terraforming APIs
- **[Building System Guide](Guides/BuildingSystem.md)** - Building monitoring and modification
- **[Event System Guide](Guides/EventSystem.md)** - Event handling and custom events
- **[Override System Guide](Guides/OverrideSystem.md)** - Dynamic value modification

### Advanced Topics
- **[Performance Optimization](Advanced/Performance.md)** - Efficient mod development
- **[Debugging & Troubleshooting](Advanced/Debugging.md)** - Common issues and solutions
- **[Custom Game Systems](Advanced/CustomSystems.md)** - Extending the SDK
- **[Multi-Mod Compatibility](Advanced/Compatibility.md)** - Working with other mods

### Examples & Samples
- **[Code Examples](Examples/)** - Ready-to-use code samples
- **[Complete Mod Projects](Samples/)** - Full mod implementations
- **[Best Practices](Best-Practices.md)** - Professional development patterns

## 🌍 Community & Support

### Getting Help
- **[FAQ](FAQ.md)** - Frequently asked questions
- **[Discord Community](https://discord.gg/peraspera-modding)** - Real-time help and discussion
- **[GitHub Issues](https://github.com/PerAsperaMods/PerAspera-SDK/issues)** - Bug reports and feature requests
- **[Wiki](https://github.com/PerAsperaMods/PerAspera-SDK/wiki)** - Community-driven documentation

### Contributing
- **[Contribution Guide](CONTRIBUTING.md)** - How to contribute to the SDK
- **[Code of Conduct](CODE_OF_CONDUCT.md)** - Community guidelines
- **[Development Setup](Development.md)** - SDK development environment

## 🔧 SDK Components

### Core Systems

| Component | Description | Use Cases |
|-----------|-------------|-----------|
| **GameAPI** | Direct game access | Access to BaseGame, Universe, Planet instances |
| **ClimateAPI** | Atmospheric simulation | Temperature, pressure, gas composition |
| **BuildingAPI** | Building system integration | Production monitoring, atmospheric impact |
| **EventSystem** | Unified event handling | Game state changes, user actions |
| **OverrideSystem** | Runtime value modification | Balance tweaks, feature toggles |

### Utility Libraries

| Library | Description | Use Cases |
|---------|-------------|-----------|
| **Mirror System** | IL2CPP wrapper generator | Safe access to game objects |
| **Command System** | Game command execution | Automated actions, cheats |
| **Detection System** | Game state monitoring | Auto-initialization, compatibility |
| **Logging System** | Advanced debugging | Structured logging, performance metrics |

## 📈 Version Information

**Current Version**: 1.1.0 (December 2024)

### Compatibility Matrix

| SDK Version | Per Aspera | BepInEx | .NET |
|-------------|------------|---------|------|
| 1.1.x | Latest | 6.0.0-be.752+ | 6.0+ |
| 1.0.x | 1.4.x | 6.0.0-be.700+ | 6.0+ |

## ⚖️ License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Per Aspera Development Team** - For creating an amazing terraforming game
- **BepInEx Team** - For the excellent modding framework
- **Community Contributors** - For testing, feedback, and contributions

---

**Ready to terraform Mars with code?** 🚀 Start with our [Installation Guide](Installation.md) and join the community of Per Aspera modders!