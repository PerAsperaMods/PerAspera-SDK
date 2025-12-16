# Installation & Setup Guide

Complete guide to setting up the PerAspera SDK development environment.

## 🎯 Prerequisites

### Required Software

#### 1. Development Environment
- **Visual Studio 2022** (Community, Professional, or Enterprise) OR
- **Visual Studio Code** with C# Dev Kit extension
- **.NET 6.0 SDK** or later ([Download](https://dotnet.microsoft.com/download/dotnet/6.0))

#### 2. Game Installation
- **Per Aspera** (Steam or GOG version)
- **BepInEx 6.x IL2CPP** (automatically installed via NuGet)

#### 3. Version Control (Recommended)
- **Git** for version control
- **GitHub Desktop** (optional, for beginners)

### System Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| **OS** | Windows 10 | Windows 10/11 |
| **RAM** | 8 GB | 16 GB |
| **Storage** | 2 GB free | 5 GB free |
| **.NET** | 6.0 | 6.0 or 8.0 |

## 🚀 Step-by-Step Installation

### Method 1: Using NuGet Packages (Recommended)

This is the easiest method for most developers.

#### 1. Create New Project

```bash
# Create a new mod project
dotnet new console -n MyPerAsperaMod
cd MyPerAsperaMod

# Add the SDK package
dotnet add package PerAspera.ModSDK --version 1.1.0
```

#### 2. Configure Project File

Edit your `.csproj` file:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <AssemblyTitle>My Per Aspera Mod</AssemblyTitle>
    <AssemblyDescription>Description of what your mod does</AssemblyDescription>
    <AssemblyCompany>Your Name</AssemblyCompany>
    <AssemblyProduct>Per Aspera Mods</AssemblyProduct>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="PerAspera.ModSDK" Version="1.1.0" />
  </ItemGroup>

</Project>
```

#### 3. Create Plugin Class

Create `Plugin.cs`:

```csharp
using BepInEx;
using BepInEx.Unity.IL2CPP;
using PerAspera.ModSDK;

namespace MyPerAsperaMod
{
    [BepInPlugin("com.yourname.myperasperapmod", "My Per Aspera Mod", "1.0.0")]
    public class Plugin : BasePlugin
    {
        public override void Load()
        {
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} is loading!");

            // Initialize the ModSDK
            ModSDK.Initialize(this);

            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} is loaded!");
        }
    }
}
```

### Method 2: Clone SDK Repository (For Advanced Users)

Use this method if you want to modify the SDK itself or work with the latest development version.

#### 1. Clone Repository

```bash
git clone https://github.com/PerAsperaMods/PerAspera-SDK.git
cd PerAspera-SDK
```

#### 2. Build SDK

```bash
dotnet restore
dotnet build --configuration Release
```

#### 3. Reference Local Build

In your mod project:

```xml
<ItemGroup>
  <ProjectReference Include="..\..\PerAspera-SDK\PerAspera.ModSDK\PerAspera.ModSDK.csproj" />
</ItemGroup>
```

## 🎮 Game Setup

### 1. Install BepInEx IL2CPP

The SDK will automatically install BepInEx via NuGet, but you need to set it up in your game directory.

#### Automatic Setup (Recommended)

1. Build your mod once:
   ```bash
   dotnet build
   ```

2. The SDK will automatically detect your game installation and guide you through BepInEx setup.

#### Manual Setup

If automatic detection fails:

1. Download [BepInEx 6.x IL2CPP](https://github.com/BepInEx/BepInEx/releases)
2. Extract to your Per Aspera game directory
3. Run the game once to initialize BepInEx
4. Copy your compiled mod DLL to `BepInEx/plugins/`

### 2. Verify Installation

Launch Per Aspera and check:

1. **BepInEx Console** appears
2. Your mod loads successfully
3. No error messages in the console

Example successful loading:
```
[Info   :   BepInEx] Loading [My Per Aspera Mod 1.0.0]
[Info   :My Per Aspera Mod] Plugin My Per Aspera Mod is loading!
[Info   :     ModSDK] PerAspera ModSDK v1.1.0 initializing...
[Info   :     ModSDK] Game instance detected: Per Aspera v1.4.0
[Info   :My Per Aspera Mod] Plugin My Per Aspera Mod is loaded!
```

## 🛠️ Development Environment Setup

### Visual Studio 2022 Setup

#### 1. Install Required Extensions

- **C# Dev Kit** (for enhanced C# support)
- **Unity Tools** (for better Unity game development)
- **GitHub Extension** (for version control)

#### 2. Configure Project Settings

1. Right-click project → Properties
2. Set Target Framework to `.NET 6.0`
3. Set Output Path to BepInEx plugins directory (optional)

#### 3. Debugging Configuration

Add to `launchSettings.json`:

```json
{
  "profiles": {
    "Per Aspera Debug": {
      "commandName": "Executable",
      "executablePath": "C:\\SteamLibrary\\steamapps\\common\\Per Aspera\\Per Aspera.exe",
      "workingDirectory": "C:\\SteamLibrary\\steamapps\\common\\Per Aspera\\"
    }
  }
}
```

### VS Code Setup

#### 1. Install Extensions

```bash
code --install-extension ms-dotnettools.csharp
code --install-extension ms-dotnettools.csdevkit
code --install-extension GitHub.vscode-github-actions
```

#### 2. Configure Workspace

Create `.vscode/settings.json`:

```json
{
  "dotnet.defaultSolution": "./MyPerAsperaMod.sln",
  "omnisharp.enableRoslynAnalyzers": true,
  "csharp.semanticHighlighting.enabled": true,
  "files.exclude": {
    "**/bin": true,
    "**/obj": true
  }
}
```

#### 3. Build Tasks

Create `.vscode/tasks.json`:

```json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "build",
      "command": "dotnet",
      "type": "process",
      "args": ["build", "--configuration", "Debug"],
      "group": "build",
      "presentation": {
        "echo": true,
        "reveal": "silent",
        "focus": false,
        "panel": "shared"
      }
    },
    {
      "label": "build-release",
      "command": "dotnet",
      "type": "process",
      "args": ["build", "--configuration", "Release"],
      "group": "build"
    }
  ]
}
```

## 🔧 Configuration

### SDK Configuration

The SDK uses intelligent configuration that auto-detects your Per Aspera installation:

```csharp
// SDK will automatically find:
// - Per Aspera game directory
// - BepInEx installation
// - Game assemblies for reference

ModSDK.Initialize(this);  // Auto-configuration happens here
```

### Manual Configuration (If Needed)

If auto-detection fails, you can manually configure paths:

```xml
<!-- In your .csproj file -->
<PropertyGroup>
  <PerAsperaGamePath>C:\SteamLibrary\steamapps\common\Per Aspera</PerAsperaGamePath>
  <BepInExPath>$(PerAsperaGamePath)\BepInEx</BepInExPath>
</PropertyGroup>
```

### Logging Configuration

Configure logging levels in `BepInEx.cfg`:

```ini
[Logging.Console]
Enabled = true
LogLevels = Fatal, Error, Warning, Message, Info

[Logging.Disk]
Enabled = true
LogLevels = Fatal, Error, Warning, Message, Info, Debug
```

## ✅ Verification Checklist

After installation, verify everything works:

### Build Verification

```bash
# Should build without errors
dotnet build --configuration Debug

# Should create DLL in output directory
ls bin/Debug/net6.0/*.dll
```

### Runtime Verification

1. ✅ Game launches with BepInEx
2. ✅ Mod loads without errors
3. ✅ SDK initializes successfully
4. ✅ Log messages appear correctly

### Common Success Indicators

```
[Info   :     ModSDK] ✅ Per Aspera game detected v1.4.0
[Info   :     ModSDK] ✅ GameAPI initialized successfully
[Info   :     ModSDK] ✅ Climate system ready
[Info   :     ModSDK] ✅ Event system active
[Info   :     ModSDK] ✅ ModSDK ready for use
```

## 🐛 Troubleshooting

### Common Issues

#### "Game not detected"
- Verify Per Aspera is installed via Steam/GOG
- Check game directory permissions
- Manually set `PerAsperaGamePath` in project file

#### "BepInEx not found"
- Install BepInEx 6.x IL2CPP manually
- Run game once to initialize BepInEx
- Check BepInEx version compatibility

#### "Assembly load errors"
- Clear `bin/` and `obj/` directories
- Run `dotnet clean` then `dotnet build`
- Verify .NET 6.0 SDK is installed

### Getting Help

If you encounter issues:

1. Check the [Troubleshooting Guide](Advanced/Debugging.md)
2. Search [GitHub Issues](https://github.com/PerAsperaMods/PerAspera-SDK/issues)
3. Ask on [Discord Community](https://discord.gg/peraspera-modding)
4. Create a new GitHub issue with logs and system info

## 🚀 Next Steps

Now that you have the SDK installed:

1. **[Create Your First Mod](Tutorials/FirstMod.md)** - Build a simple climate monitoring mod
2. **[Explore Examples](Examples/)** - See practical code samples
3. **[Read API Reference](API-Reference.md)** - Understand available features
4. **[Join Community](https://discord.gg/peraspera-modding)** - Connect with other modders

Ready to start modding? Let's build something amazing for Mars! 🚀