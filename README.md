# PerAspera ModSDK

**A community-made modding SDK for Per Aspera** — makes BepInEx 6 IL2CPP modding easier with typed wrappers, an event system, and a commands API.

> This is an **unofficial community project**, not affiliated with or endorsed by the Per Aspera developers.

[![Version](https://img.shields.io/badge/version-1.1.0-blue.svg)](https://github.com/PerAsperaMods/PerAspera-SDK/releases)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![BepInEx](https://img.shields.io/badge/BepInEx-6.0.0--be.752-orange.svg)](https://github.com/BepInEx/BepInEx)
[![.NET](https://img.shields.io/badge/.NET-6.0-purple.svg)](https://dotnet.microsoft.com/)
[![Per Aspera](https://img.shields.io/badge/Per%20Aspera-1.8.x-red.svg)](https://store.steampowered.com/app/944260/Per_Aspera/)

---

## What is this?

A set of C# libraries that wrap the Per Aspera IL2CPP game assemblies and expose a cleaner API for mod development:

- **Typed event subscriptions** — subscribe to game events without string-based bus calls
- **Safe wrappers** — access game objects (Planet, Universe, Buildings) with null protection
- **Commands API** — create custom YAML actions usable in Special Projects and tech trees
- **Climate helpers** — read and react to atmosphere/temperature data
- **GetterOverride system** — patch game property getters at runtime without Harmony transpilers

This SDK is used internally for developing mods in the Individual-Mods/ folder of this repo.

---

## Prerequisites

- **.NET 6 SDK**
- **Per Aspera** installed via Steam
- **BepInEx 6.0.0-be.752** installed — launch the game once to generate interop assemblies in BepInEx/interop/
- **Git** (two repos to clone — see below)

---

## Getting started (clone and build)

The SDK lives in its own git repository nested inside the main repo. Clone both:

```powershell
git clone https://github.com/PerAsperaMods/ModPeraspera  C:\MyMods
git clone https://github.com/PerAsperaMods/PerAspera-SDK C:\MyMods\SDK
```

### 1. Generate local game library references

Game DLLs are never committed. Run the setup wizard once:

```powershell
cd C:\MyMods
.\Generate-GameLibs.ps1
```

The wizard auto-detects your Steam install, publicizes the three main assemblies
(Assembly-CSharp, ScriptsAssembly, PluginsAssembly) and copies all interop DLLs
into gamelibs/. This folder is gitignored — never committed.

Re-run with -Force after a game update.

### 2. Build

```powershell
cd C:\MyMods\SDK
dotnet build SDK.sln -c Release
```

Expected result: **0 errors**.

---

## SDK packages

| Package | Role |
|---------|------|
| PerAspera.Core | Base utilities, LogAspera logging, IL2CPP reflection helpers |
| PerAspera.Core.IL2CppExtensions | GetMemberValue, SetMemberValue, InvokeMethod extension methods |
| PerAspera.GameAPI | Native game type access, GameApi entry point, UnityGuiHelper |
| PerAspera.GameAPI.Native | Thin typed wrappers over raw IL2CPP types (NativeTypes.cs) |
| PerAspera.GameAPI.Wrappers | High-level wrappers: PlanetWrapper, BaseGameWrapper, UniverseWrapper |
| PerAspera.GameAPI.Events | ModEventBus, EnhancedEventBus, GameEvents constants |
| PerAspera.GameAPI.Commands | CommandExecutor, builder pattern, IGameCommand, OnCommandExecuted |
| PerAspera.GameAPI.Climate | AtmosphereSimulator, TemperatureCalculator, WaterCycleSimulator |
| PerAspera.GameAPI.Overrides | GetterOverride, OverridePatchSystem — runtime property patching |
| PerAspera.ModSDK | Aggregates all of the above; main package for mod authors |

---

## Writing a mod

### Minimal plugin

```csharp
using BepInEx;
using BepInEx.Unity.IL2CPP;
using PerAspera.Core;
using PerAspera.GameAPI.Events;

[BepInPlugin("com.yourname.mymod", "My Mod", "1.0.0")]
public class MyMod : BasePlugin
{
    public override void Load()
    {
        LogAspera.Initialize(Log);

        ModEventBus.Subscribe(GameEvents.PlanetDaysPassed, (_, args) =>
        {
            LogAspera.LogInfo($"Sol {args} passed!");
        });
    }
}
```

### Custom YAML action (Special Projects / tech trees)

```csharp
using PerAspera.GameAPI.Commands;
using SdkCommands = PerAspera.GameAPI.Commands.Commands;

[BepInPlugin("com.yourname.myaction", "My Action", "1.0.0")]
public class MyActionPlugin : BasePlugin
{
    public override void Load()
    {
        LogAspera.Initialize(Log);
        SdkCommands.RegisterAction(new GiveScienceAction());
    }
}

public class GiveScienceAction : IModTextAction
{
    public string CommandName => "GiveScience";

    public bool Execute(string[] args, GameCommandsReadyEvent? ctx)
    {
        if (!ActionContextHelper.TryGetPositiveFloat(args, 0, out float amount, null, CommandName))
            return false;
        if (!ActionContextHelper.TryGetFaction(ctx, out var faction, null, CommandName))
            return false;

        faction!.GetCurrentlyResearchedTechnology()?.AddResearchPoints(amount);
        return true;
    }
}
```

In YAML:

```yaml
launchActions:
  - command: GiveScience
    arguments:
      - "500"
    daysDelay: 0.0
```

See Individual-Mods/Example-AddResearchPoints/ for a complete working project.

### GetterOverride (patch a property at runtime)

```csharp
var solarOverride = GetterOverride.Create("solar_efficiency_boost");
solarOverride.Register();
solarOverride.SetValue(1.5f);   // +50% solar panel output
solarOverride.Enable();
```

---

## Build modes

| Mode | When | Game DLLs source |
|------|------|-----------------|
| **Local** | dotnet build on dev machine | gamelibs/ generated by Generate-GameLibs.ps1 |
| **CI** | GITHUB_ACTIONS=true env var | BepInEx.AssemblyPublicizer.MSBuild + PERASPERAGAMEPATH env var |

Handled automatically in SDK/Directory.Build.props — no manual switch needed.

---

## Deploying to BepInEx

After building, push DLLs to the game plugins folder:

```powershell
.\Deploy-SDK-Quick.ps1           # smart copy (skips up-to-date files)
.\Deploy-SDK-Quick.ps1 -Force    # force copy all
```

Target: GameDir\BepInEx\plugins\SDK\

---

## Documentation

| File | Content |
|------|---------|
| CHANGELOG.md | Version history |
| GAME-EVENTS-REFERENCE.md | All available GameEvents constants |
| SDK-ARCHITECTURE-V2.md | Internal architecture notes |
| VERSION-GUIDE.md | Versioning and release workflow |
| [Organization Wiki](https://github.com/PerAsperaMods/.github/tree/main/Organization-Wiki) | User-facing guides and tutorials |
| [Copilot Agents & Skills](https://github.com/PerAsperaMods/.github/tree/main) | VS Code agents (`@per-aspera-*`) and slash-command skills (`/per-aspera-*`) |

---

## License

MIT — see LICENSE.