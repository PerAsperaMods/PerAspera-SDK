# 📚 PerAspera ModSDK Documentation Index

**Version 1.1.0** | **December 2024** | **Production Ready**

This is the complete documentation index for the PerAspera Modding SDK v1.1.0.

## 🚀 Getting Started

| Document | Purpose | For Who |
|----------|---------|---------|
| **[README.md](./README.md)** | Main SDK overview and quick start | Everyone |
| **[CHANGELOG.md](./CHANGELOG.md)** | Version history and migration guide | Developers |

## 📦 Package Documentation

| Package | Documentation | Description |
|---------|-------------|-------------|
| **PerAspera.ModSDK** | [README](./PerAspera.ModSDK/README.md) | Main SDK package - Start here |
| **PerAspera.Core** | Built-in XML docs | Low-level utilities |
| **PerAspera.Core.IL2CppExtensions** | [README](./PerAspera.Core.IL2CppExtensions/README.md) | IL2CPP helpers |
| **PerAspera.GameAPI** | Built-in XML docs | Native game access |
| **PerAspera.GameAPI.Wrappers** | [README](./PerAspera.GameAPI.Wrappers/README.md) | Type-safe game wrappers |
| **PerAspera.GameAPI.Events** | [README](./PerAspera.GameAPI.Events/README.md) | Event system |
| **PerAspera.GameAPI.Commands** | [README](./PerAspera.GameAPI.Commands/README.md) | Command integration |
| **PerAspera.GameAPI.Climate** | Built-in XML docs | Climate system helpers |
| **PerAspera.GameAPI.Overrides** | [README](./PerAspera.GameAPI.Overrides/README.md) | Behavior overrides |

## 🛠️ Development & Maintenance

| Document | Purpose | For Who |
|----------|---------|---------|
| **[VERSION-GUIDE.md](./VERSION-GUIDE.md)** | Complete versioning workflow | Maintainers |
| **[VERSIONING-QUICKREF.md](./VERSIONING-QUICKREF.md)** | Fast version commands | Maintainers |
| **[ARCHIVE-QUICKREF.md](./ARCHIVE-QUICKREF.md)** | Archive system usage | Maintainers |
| **[RELEASE-WORKFLOW.md](./RELEASE-WORKFLOW.md)** | Release process guide | Maintainers |

## 🎮 Game-Specific Guides

| Document | Purpose | Content |
|----------|---------|---------|
| **[GAME-EVENTS-REFERENCE.md](./GAME-EVENTS-REFERENCE.md)** | All available game events | Event names, parameters, usage |

## 📁 Project Structure

```
SDK/
├── PerAspera.ModSDK/           # 🎯 Main SDK package (start here)
├── PerAspera.Core/             # Low-level utilities  
├── PerAspera.Core.IL2CppExtensions/ # IL2CPP helpers
├── PerAspera.GameAPI/          # Native game access
├── PerAspera.GameAPI.Wrappers/ # Type-safe wrappers
├── PerAspera.GameAPI.Events/   # Event system
├── PerAspera.GameAPI.Commands/ # Command integration
├── PerAspera.GameAPI.Climate/  # Climate helpers
├── PerAspera.GameAPI.Overrides/ # Behavior overrides
├── _Archive/                   # Version archives
└── *.md                       # This documentation
```

## 🏗️ Architecture Overview

The SDK follows a layered architecture:

1. **Application Layer**: Your mods using `PerAspera.ModSDK`
2. **SDK Layer**: High-level APIs and event system
3. **Game API Layer**: Wrappers and native game access  
4. **Core Layer**: IL2CPP utilities and foundation
5. **Runtime Layer**: BepInEx.Unity.IL2CPP

## 🎯 Quick Start Workflow

1. **Install**: Add `PerAspera.ModSDK` NuGet package
2. **Inherit**: Extend `PerAsperaSDKPlugin` 
3. **Override**: Implement `OnSDKReady()` method
4. **Subscribe**: Use `SDK.Events.*` for game events
5. **Build**: Compile and deploy to BepInEx/plugins/

## 📝 Documentation Standards

- **README files**: Package-specific setup and usage
- **XML docs**: IntelliSense documentation in code
- **Changelog**: Version history and breaking changes  
- **Examples**: Working code samples in README files

## 🔗 External Resources

- **BepInEx Documentation**: https://docs.bepinex.dev/
- **Per Aspera Steam**: https://store.steampowered.com/app/944260/Per_Aspera/
- **C# IL2CPP Interop**: https://github.com/BepInEx/Il2CppInterop

---

**Last Updated**: December 17, 2024  
**SDK Version**: 1.1.0  
**Game Compatibility**: Per Aspera 1.5.x  
**Framework**: BepInEx 6.0.x