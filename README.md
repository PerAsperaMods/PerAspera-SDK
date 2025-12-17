# 🚀 PerAspera ModSDK

**The official Per Aspera modding SDK** - Transform complex IL2CPP modding into simple, event-driven development.

[![Version](https://img.shields.io/badge/version-1.0.0--beta-blue.svg)](https://github.com/PerAsperaMods/PerAspera-SDK/releases)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![BepInEx](https://img.shields.io/badge/BepInEx-6.0.x-orange.svg)](https://github.com/BepInEx/BepInEx)
[![Per Aspera](https://img.shields.io/badge/Per%20Aspera-1.5.x-red.svg)](https://store.steampowered.com/app/944260/Per_Aspera/)

## 🎯 What is PerAspera ModSDK?

A comprehensive, beginner-friendly SDK that makes Per Aspera modding **10x simpler**:

- 🏗️ **Single import**: `using PerAspera.ModSDK;`
- ⚡ **Event-driven**: No more Update() loops or performance issues  
- 🔒 **Type-safe**: Strong typing and IntelliSense support
- 📚 **Well-documented**: Clear examples and comprehensive guides
- 🛡️ **Error-handled**: Graceful error handling and informative logging

## 📦 SDK Components

### Core Packages

| Package | Description | Version |
|---------|-------------|---------|
| **PerAspera.Core** | Low-level utilities and IL2CPP extensions | 1.0.0-beta |
| **PerAspera.GameAPI** | IL2CPP game class wrappers and mirrors | 1.0.0-beta |
| **PerAspera.ModSDK** | High-level event-driven SDK (main package) | 1.0.0-beta |

### Architecture

```
PerAspera.ModSDK (Your Interface)
├── PerAspera.ModSDK.Events (Event Constants & Helpers)
│   └── PerAspera.GameAPI (Native Event System)
├── PerAspera.GameAPI.Wrappers (Public Game Object API)
│   ├── PerAspera.GameAPI (Native Wrappers)
│   └── PerAspera.Core (IL2CPP Utilities)
└── BepInEx.Unity.IL2CPP (Framework)
```

## 🔄 Before vs After

### ❌ **Before (Complex)**
```csharp
using Common;                               // ❓ Confusing names
using PerAspera.Core;                      // 🔧 Too low-level

public class MyMod : BasePlugin 
{
    private float lastCheck;
    
    public override void Load()
    {
        // Manual, error-prone initialization
        MainWrapper.EventBus.Subscribe("PlanetDaysPassed", handler);
        LogAspera.LogInfo("Loaded");
        
        // Polling nightmare
        Update = () => {
            if (Time.time - lastCheck > 1f) {
                CheckGameState(); // 🐌 Performance killer
                lastCheck = Time.time;
            }
        };
    }
}
```

### ✅ **After (Simple)**
```csharp
using PerAspera.ModSDK;

[BepInPlugin("com.example.mymod", "My Mod", "1.0.0")]
public class MyMod : PerAsperaSDKPlugin
{
    protected override void OnSDKReady()
    {
        // ⚡ Events are automatically available
        SDK.Events.PlanetDaysPassed += (days) => {
            Logger.LogInfo($"Planet day {days} passed!");
        };
        
        // 🔒 Type-safe game access
        SDK.Events.TemperatureChanged += (data) => {
            if (data.Temperature > 273.15f) {
                Logger.LogInfo("Above freezing!");
            }
        };
    }
}
```

## 🚀 Quick Start

### 1. Installation

Add to your mod's `.csproj`:
```xml
<PackageReference Include="PerAspera.ModSDK" Version="1.0.0-beta" />
```

Or install via NuGet:
```bash
dotnet add package PerAspera.ModSDK --version 1.0.0-beta
```

### 2. Create Your First Mod

```csharp
using PerAspera.ModSDK;

[BepInPlugin("com.yourname.terraformboost", "Terraform Boost", "1.0.0")]
public class TerraformBoostMod : PerAsperaSDKPlugin
{
    protected override void OnSDKReady()
    {
        Logger.LogInfo("🌱 Terraform Boost Mod loaded!");
        
        // Boost terraforming when day passes
        SDK.Events.PlanetDaysPassed += BoostTerraforming;
        
        // Monitor climate changes
        SDK.Events.TemperatureChanged += OnTemperatureChange;
    }
    
    private void BoostTerraforming(int dayNumber)
    {
        if (dayNumber % 10 == 0) // Every 10 days
        {
            Logger.LogInfo($"🚀 Terraforming boost on day {dayNumber}!");
            // Add your boost logic here
        }
    }
    
    private void OnTemperatureChange(ClimateEventData data)
    {
        if (data.CurrentValue > 273.15f) // Above 0°C
        {
            Logger.LogInfo("🌡️ Temperature is above freezing!");
        }
    }
}
```

### 3. Build and Deploy

```powershell
# Build your mod
dotnet build --configuration Release

# Copy to BepInEx
cp bin/Release/net6.0/YourMod.dll "F:\SteamLibrary\steamapps\common\Per Aspera\BepInEx\plugins\"
```

## 📋 Event System

### 🌍 Climate Events
```csharp
SDK.Events.TemperatureChanged += (data) => { /* Handle temperature changes */ };
SDK.Events.AtmosphereChanged += (data) => { /* Handle atmosphere changes */ };
SDK.Events.WaterStockChanged += (data) => { /* Handle water stock changes */ };
```

### 🏗️ Building Events
```csharp
SDK.Events.BuildingConstructed += (data) => { /* Handle new buildings */ };
SDK.Events.BuildingDestroyed += (data) => { /* Handle destroyed buildings */ };
SDK.Events.ProductionCompleted += (data) => { /* Handle production cycles */ };
```

### ⏰ Time Events
```csharp
SDK.Events.PlanetDaysPassed += (days) => { /* Daily events */ };
SDK.Events.MartianSeasonChanged += (season) => { /* Seasonal events */ };
```

### 💎 Resource Events
```csharp
SDK.Events.ResourceExtracted += (data) => { /* Handle resource mining */ };
SDK.Events.ResourceConsumed += (data) => { /* Handle resource usage */ };
SDK.Events.ResourceAdded += (data) => { /* Handle resource additions */ };
```

## 🔧 Development Tools

### Version Management Script

Use the included PowerShell script for professional version management:

```powershell
# Show current version info
.\Manage-Version.ps1 -Action show

# Bump versions
.\Manage-Version.ps1 -Action bump-patch    # 1.0.0 → 1.0.1
.\Manage-Version.ps1 -Action bump-minor    # 1.0.0 → 1.1.0
.\Manage-Version.ps1 -Action bump-major    # 1.0.0 → 2.0.0

# Create pre-releases
.\Manage-Version.ps1 -Action pre-release -PreReleaseType beta

# Build and package
.\Manage-Version.ps1 -Action build
.\Manage-Version.ps1 -Action package
```

### GitHub Actions Integration

Automatic CI/CD pipeline for releases:
- ✅ Automated builds on tag push
- ✅ NuGet package generation  
- ✅ GitHub Releases creation
- ✅ Documentation deployment

## 📚 Documentation

| Resource | Description |
|----------|-------------|
| [VERSION-GUIDE.md](VERSION-GUIDE.md) | Complete versioning and release management |
| [ARCHIVING-GUIDE.md](ARCHIVING-GUIDE.md) | SDK version archiving system |
| [ARCHIVE-QUICKREF.md](ARCHIVE-QUICKREF.md) | Quick reference for archiving |
| [CHANGELOG.md](CHANGELOG.md) | Version history and migration notes |
| [Examples/](Examples/) | Sample mods and code snippets |
| [GitHub Wiki](https://github.com/PerAsperaMods/PerAspera-SDK/wiki) | Complete documentation |

### 📦 Archived Versions

Previous SDK releases are preserved in `_Archive/` with full binaries and documentation.  
See [ARCHIVING-GUIDE.md](ARCHIVING-GUIDE.md) for version history and rollback instructions.

```powershell
# List all archived versions
Get-ChildItem SDK\_Archive -Directory | Select-Object Name, CreationTime
```

## 🔗 Compatibility

### Version Matrix
| SDK Version | Per Aspera | BepInEx | .NET |
|-------------|------------|---------|------|
| 1.0.x       | 1.5.x      | 6.0.x   | 6.0  |

### Migration Support
- **Automatic detection** of old modding patterns
- **Migration warnings** with suggested fixes
- **Compatibility layer** for existing mods
- **Step-by-step guides** for modernization

## 🤝 Community

### Get Involved
- **🐛 Report issues**: [GitHub Issues](https://github.com/PerAsperaMods/PerAspera-SDK/issues)
- **💬 Discuss features**: [GitHub Discussions](https://github.com/PerAsperaMods/PerAspera-SDK/discussions)  
- **📖 Improve docs**: [GitHub Wiki](https://github.com/PerAsperaMods/PerAspera-SDK/wiki)
- **🎮 Share mods**: [Steam Workshop](https://steamcommunity.com/app/944260/workshop/)

### Contributing
```bash
# Fork and clone
git clone https://github.com/YourName/PerAspera-SDK.git
cd PerAspera-SDK/SDK

# Create feature branch
git checkout -b feature/amazing-feature

# Make changes and test
.\Manage-Version.ps1 -Action build

# Submit PR
git push origin feature/amazing-feature
```

## 📄 License

MIT License - see [LICENSE](LICENSE) for details.

## 🙏 Acknowledgments

- **Per Aspera community** for feedback and testing
- **BepInEx team** for the modding framework
- **Unity community** for IL2CPP insights

---

**Ready to transform Mars? Start modding with PerAspera SDK today!** 🚀🔴
    }
    
    void Update()                          // ? Performance killer
    {
        if (Time.time - lastCheck > 0.25f) 
        {
            CheckGameState();              // ? Expensive scanning!
            lastCheck = Time.time;
        }
    }
}
```

### ? **After (Simple)**
```csharp
using PerAspera.ModSDK;                    // ? One clear import

public class MyMod : BasePlugin 
{
    public override void Load()
    {
        ModSDK.Initialize("MyMod", "1.0.0"); // ? Automatic setup
        ModSDK.Events.Subscribe(GameEvents.MartianDayPassed, OnMartianDay);
        ModSDK.Log.Info("Loaded");
    }
    
    private void OnMartianDay(object eventData) // ? Event-driven, efficient
    {
        var sol = ModSDK.Universe.GetCurrentSol();
        ModSDK.Log.Info($"New day: Sol {sol}");
    }
    
    public override bool Unload()
    {
        ModSDK.Shutdown();                 // ? Automatic cleanup
        return true;
    }
}
```

## ?? Installation

### 1. Add SDK Reference
```xml
<!-- In your mod's .csproj file -->
<ProjectReference Include="$(SolutionDir)SDK\PerAspera.ModSDK\PerAspera.ModSDK.csproj" />
```

### 2. Import and Initialize
```csharp
using PerAspera.ModSDK;

public override void Load()
{
    ModSDK.Initialize("YourModName", "1.0.0");
    // Your mod code here
}

public override bool Unload()
{
    ModSDK.Shutdown();
    return true;
}
```

## ?? Core APIs

### ?? Events - Event-Driven Development
```csharp
// Subscribe to game events (no more Update loops!)
ModSDK.Events.Subscribe(GameEvents.MartianDayPassed, OnMartianDay);
ModSDK.Events.Subscribe(GameEvents.TemperatureChanged, OnTempChanged);

// Publish custom mod events
ModSDK.Events.Publish("MyMod_CustomEvent", eventData);

// Unsubscribe when done
ModSDK.Events.Unsubscribe(GameEvents.MartianDayPassed, OnMartianDay);
```

### ?? Universe - Game State Access
```csharp
// Get current Martian day
int sol = ModSDK.Universe.GetCurrentSol();

// Get planet data
Planet planet = ModSDK.Universe.GetPlanet();

// Check if game is ready
if (ModSDK.Universe.IsGameReady()) 
{
    // Safe to access game systems
}
```

### ?? Logging - Proper Debug Output
```csharp
ModSDK.Log.Info("Information message");
ModSDK.Log.Warning("Warning message");  
ModSDK.Log.Error("Error message");
ModSDK.Log.Debug("Debug message");
```

## ?? Available Events

### ?? **Currently Available**
- `GameEvents.MartianDayPassed` - New Martian sol
- `GameEvents.TemperatureChanged` - Planet temperature changed  
- `GameEvents.AtmosphereChanged` - Atmosphere composition changed
- `GameEvents.WaterStockChanged` - Water reserves changed

### ?? **Coming Soon**
- `GameEvents.BuildingConstructed` - New building placed
- `GameEvents.ProductionCompleted` - Building finished production
- `GameEvents.ResourceExtracted` - Resource extracted from vein

## ?? Project Structure

```
SDK/
??? PerAspera.ModSDK/          # ?? Main SDK - your entry point
?   ??? ModSDK.cs              # Core SDK functionality
?   ??? README.md              # Usage documentation
?   ??? *.csproj
??? PerAspera.Core/            # ???  Core utilities (internal)
?   ??? Extensions/            # IL2CPP and type extensions
?   ??? Utilities/             # Helper functions
?   ??? *.csproj
??? PerAspera.GameAPI/         # ?? Game API layer (internal)
?   ??? Mirror/                # Game class wrappers
?   ??? Events/                # Event system implementation
?   ??? *.csproj
??? Examples/                  # ?? Working examples
?   ??? ExampleMod.cs          # Complete example mod
??? MIGRATION-GUIDE.md         # Migration from old structure
```

## ?? Migration from Legacy Code

If you have existing mods using the old `AsperaClass`/`Common` structure:

1. **Run the migration script**: `.\migrate-to-sdk.ps1`
2. **Update initialization**: Add `ModSDK.Initialize()` to your `Load()` method
3. **Add cleanup**: Add `ModSDK.Shutdown()` to your `Unload()` method
4. **Test everything**: Build and verify your mod still works

See [MIGRATION-GUIDE.md](MIGRATION-GUIDE.md) for detailed instructions.

## ?? Examples

### Simple Event Listener
```csharp
[BepInPlugin("com.yourname.simplemod", "Simple Mod", "1.0.0")]
public class SimpleMod : BasePlugin
{
    public override void Load()
    {
        ModSDK.Initialize("SimpleMod");
        ModSDK.Events.Subscribe(GameEvents.MartianDayPassed, OnNewDay);
        ModSDK.Log.Info("Simple mod loaded!");
    }

    private void OnNewDay(object eventData)
    {
        var sol = ModSDK.Universe.GetCurrentSol();
        if (sol % 10 == 0) 
        {
            ModSDK.Log.Info($"Milestone: Sol {sol}!");
        }
    }

    public override bool Unload()
    {
        ModSDK.Shutdown();
        return true;
    }
}
```

### Advanced Mod with Custom Events
```csharp
[BepInPlugin("com.yourname.advancedmod", "Advanced Mod", "1.0.0")]
public class AdvancedMod : BasePlugin
{
    public override void Load()
    {
        ModSDK.Initialize("AdvancedMod", "1.0.0");
        
        // Subscribe to multiple events
        ModSDK.Events.Subscribe(GameEvents.MartianDayPassed, OnMartianDay);
        ModSDK.Events.Subscribe(GameEvents.TemperatureChanged, OnTemperatureChanged);
        
        // Listen to other mods' events
        ModSDK.Events.Subscribe("OtherMod_ResourceFound", OnResourceFound);
        
        ModSDK.Log.Info("Advanced mod loaded!");
    }

    private void OnMartianDay(object eventData)
    {
        // Publish custom event for other mods
        ModSDK.Events.Publish("AdvancedMod_DailyReport", new 
        {
            Sol = ModSDK.Universe.GetCurrentSol(),
            Temperature = GetCurrentTemperature()
        });
    }
}
```

## ??? Building Your First Mod

1. **Create new BepInEx plugin project** targeting .NET 6.0
2. **Add SDK reference** to your .csproj
3. **Initialize SDK** in your Load() method
4. **Use events instead of Update()** for performance
5. **Add proper cleanup** in Unload() method

## ?? Getting Help

- ?? **Documentation**: Check individual README files in each project
- ?? **Examples**: Look at `/Examples/` and `/Individual-Mods/` 
- ?? **Issues**: Submit GitHub issues with `[SDK]` prefix
- ?? **Community**: Per Aspera modding Discord

## ?? System Requirements

- **.NET 6.0 SDK**
- **BepInEx 6.x IL2CPP** 
- **Per Aspera v1.4+**
- **Visual Studio 2022** or **VS Code** (recommended)

## ?? Benefits

### ?? **Performance**
- Event-driven architecture (no Update() loops)
- Optimized game API access
- Proper resource management

### ??? **Reliability** 
- Error handling and validation
- Graceful degradation
- Proper initialization/shutdown

### ?? **Productivity**
- IntelliSense-friendly APIs
- Clear documentation and examples
- Simplified debugging

### ?? **Community**
- Standardized mod development
- Inter-mod communication via events
- Shared learning resources

---

**?? Ready to start modding?** 

Check out [Examples/ExampleMod.cs](Examples/ExampleMod.cs) for a complete working example, then dive into the [SDK documentation](PerAspera.ModSDK/README.md)!

**?? Mars awaits your creativity!**#   G i t H u b   A c t i o n s   T e s t i n g 
 
 