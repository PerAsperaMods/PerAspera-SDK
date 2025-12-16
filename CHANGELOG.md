# Changelog - PerAspera Modding SDK

All notable changes to the PerAspera Modding SDK will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial SDK structure with three core components
- Event-driven modding architecture
- Type-safe game API wrappers

### Changed
- Nothing yet

### Deprecated
- Nothing yet

### Removed
- Nothing yet

### Fixed
- Nothing yet

### Security
- Nothing yet

---

## [1.0.0-beta] - 2024-12-14

### Added
- **PerAspera.Core** - Low-level utilities and IL2CPP extensions
  - `LogAspera` - Enhanced logging system for mod development
  - `ReflectionHelpers` - Safe IL2CPP reflection utilities
  - `TypeExtensions` - Extension methods for IL2CPP types
  - `CargoQuantityHelper` - Resource management utilities
- **PerAspera.GameAPI** - IL2CPP game class wrappers and mirrors
  - Mirror wrappers for game classes (BaseGame, Universe, Planet)
  - Event system integration
  - Climate system helpers
  - Detection utilities for game state
- **PerAspera.ModSDK** - High-level modding SDK
  - Simple `using PerAspera.ModSDK;` import
  - Event-driven architecture
  - Automatic initialization and lifecycle management
  - Type-safe IntelliSense support

### Technical Details
- **Target Framework**: .NET 6.0
- **BepInEx Compatibility**: 6.0.x
- **Per Aspera Game Version**: 1.5.x
- **Architecture**: Event-driven with IL2CPP interop

### Breaking Changes
- This is the initial release, no breaking changes

---

## Version Scheme

We follow [Semantic Versioning](https://semver.org/):

- **MAJOR** (1.x.x): Breaking changes to public API
- **MINOR** (x.1.x): New features, backwards compatible
- **PATCH** (x.x.1): Bug fixes, backwards compatible

### Pre-release identifiers:
- **alpha**: Early development, unstable
- **beta**: Feature complete, testing phase
- **rc**: Release candidate, production ready

### Version Compatibility Matrix

| SDK Version | Per Aspera Game | BepInEx | .NET |
|-------------|----------------|---------|------|
| 1.0.x       | 1.5.x          | 6.0.x   | 6.0  |

---

## Migration Guide

### From Direct IL2CPP Development to SDK 1.0.0-beta

**Before** (Complex IL2CPP):
```csharp
using Common;
using PerAspera.Core;

public class MyMod : BasePlugin 
{
    public override void Load()
    {
        MainWrapper.EventBus.Subscribe("PlanetDaysPassed", handler);
        LogAspera.LogInfo("Manual setup required");
    }
}
```

**After** (Simple SDK):
```csharp
using PerAspera.ModSDK;

[BepInPlugin("com.example.mymod", "My Mod", "1.0.0")]
public class MyMod : PerAsperaSDKPlugin
{
    protected override void OnSDKReady()
    {
        // Events are automatically available
        SDK.Events.PlanetDaysPassed += (days) => {
            Logger.LogInfo($"Planet day {days} passed!");
        };
    }
}
```

## Development Status

- ✅ **Core**: Stable foundation
- ✅ **GameAPI**: Game wrappers implemented  
- 🔄 **ModSDK**: Active development
- 📝 **Documentation**: In progress
- 🧪 **Examples**: Being created

## Support

- **Repository**: https://github.com/PerAsperaMods/PerAspera-SDK
- **Issues**: https://github.com/PerAsperaMods/PerAspera-SDK/issues
- **Discussions**: https://github.com/PerAsperaMods/PerAspera-SDK/discussions
- **Wiki**: https://github.com/PerAsperaMods/PerAspera-SDK/wiki