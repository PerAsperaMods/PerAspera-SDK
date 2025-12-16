# SDK Architecture v2.0 - Module Overview

## 📦 Current Sub-Projects Structure

### **Core Layer** (Foundations)
```
PerAspera.Core/
├── LogAspera.cs
├── ReflectionHelpers.cs
├── TypeExtensions.cs
└── CargoQuantityHelper.cs
→ Dependencies: None (pure utilities)

PerAspera.Core.IL2CppExtensions/
├── IL2CPP-specific extensions
└── Native interop helpers
→ Dependencies: PerAspera.Core
```

### **Abstractions Layer**
```
PerAspera.Abstractions/
└── Interfaces and base types
→ Dependencies: PerAspera.Core
```

### **GameAPI Layer** (Native Access)
```
PerAspera.GameAPI/
├── Native/
│   ├── KeeperTypeRegistry.cs          → Dynamic instance access
│   ├── NativeEventPatcher.cs          → Harmony patches
│   └── KeeperInstanceLibrary.cs
├── Helpers/                            ✅ NEW
│   └── ClimateHelper.cs               → Stateless climate API
├── Models/                             ✅ NEW
│   ├── ClimateSnapshot.cs             → DTOs
│   ├── AtmosphereData.cs
│   └── TerraformingStatus.cs
├── Mirror/
├── Detection/
└── Initialization/
→ Dependencies: Core, Core.IL2CppExtensions, GameAPI.Overrides
```

### **Feature Modules** (Domain-Specific)

#### **PerAspera.GameAPI.Events** ✅
```
├── Core/
│   ├── IGameEvent.cs
│   ├── GameEventBase.cs
│   └── NativeGameEventBase.cs
├── Data/
│   ├── ClimateEventData.cs
│   ├── MartianDayEventData.cs
│   └── BuildingEventData.cs
├── Native/
│   ├── ClimateEvents.cs               ✅ NEW
│   ├── BuildingEvents.cs
│   └── UniverseEvents.cs
├── Constants/
│   ├── SDKEventConstants.cs
│   └── NativeEventConstants.cs
└── Helpers/
    ├── ClimateHelpers.cs
    └── TimeHelpers.cs
→ Dependencies: Core, GameAPI
→ Purpose: Event system, constants, helpers
```

#### **PerAspera.GameAPI.Overrides** ✅
```
├── Models/
│   ├── GetterOverride<T>.cs
│   ├── IOverrideStrategy<T>.cs
│   └── OverrideStrategies/
├── Registry/
│   └── GetterOverrideRegistry.cs
├── Patching/
│   ├── AutoOverridePatchAttribute.cs
│   └── OverridePatchSystem.cs
└── Validation/
→ Dependencies: Core, Core.IL2CppExtensions
→ Purpose: Generic override system with strategies
```

#### **PerAspera.GameAPI.Climate** ✅ NEW
```
├── Simulation/
│   ├── ClimateSimulator.cs
│   └── Models/
│       ├── GreenhouseModel.cs
│       ├── TemperatureModel.cs
│       └── PressureModel.cs
├── Analysis/
│   ├── HabitabilityAnalyzer.cs
│   ├── TerraformingAnalyzer.cs
│   └── BuildingImpactAnalyzer.cs
├── Forecasting/
│   ├── ClimateForecast.cs
│   └── TerraformingProjection.cs
└── Configuration/
    ├── ClimateConfig.cs
    └── TerraformingConstants.cs
→ Dependencies: Core, GameAPI, GameAPI.Events, GameAPI.Overrides
→ Purpose: Climate simulation, analysis, forecasting
```

#### **PerAspera.GameAPI.Wrappers** ✅
```
├── WrapperBase.cs
├── Building.cs
├── Planet.cs
└── Universe.cs
→ Dependencies: Core, GameAPI
→ Purpose: Type-safe wrappers for native objects
```

### **SDK Layer** (Public API)
```
PerAspera.ModSDK/
├── ModSDK.cs
├── PerAsperaSDKPlugin.cs
└── Systems/
→ Dependencies: ALL sub-projects
→ Purpose: Unified public API for modders
```

---

## 🎯 Design Principles

### 1. **Separation of Concerns**
- **Core**: Pure utilities, no game logic
- **GameAPI**: Direct native access, thin wrappers
- **Feature Modules**: Domain-specific logic (Events, Overrides, Climate)
- **ModSDK**: Simplified public API

### 2. **Dependency Flow**
```
Core (no deps)
  ↓
GameAPI (uses Core)
  ↓
Feature Modules (use GameAPI + Core)
  ↓
ModSDK (orchestrates all modules)
```

### 3. **Feature Module Pattern**
Each feature gets its own project:
- ✅ Independent versioning
- ✅ Optional dependencies
- ✅ Clear boundaries
- ✅ Easier testing

### 4. **Stateless Helpers + Event-Driven**
- Helpers in GameAPI = stateless access (ClimateHelper)
- Events in GameAPI.Events = reactive patterns
- Simulation in Feature Modules = stateful logic

---

## 🔄 Integration: Climate System

### **Climate Components Distribution**

#### **GameAPI/Helpers/ClimateHelper.cs**
- `GetTemperature()`, `SetTemperature()`
- `GetCO2Pressure()`, `SetCO2Pressure()`
- `GetSnapshot()` → Returns ClimateSnapshot
- Stateless, direct Planet access

#### **GameAPI/Models/**
- `ClimateSnapshot.cs` → Immutable DTO
- `AtmosphereData.cs` → Gas composition
- `TerraformingStatus.cs` → Progress tracking

#### **GameAPI.Events/Data/ClimateEventData.cs**
- Event payload for climate changes
- Used by NativeEventPatcher

#### **GameAPI.Events/Native/ClimateEvents.cs** ✅ NEW
- Event constants and factory methods
- `TemperatureChanged`, `CO2PressureChanged`, etc.

#### **GameAPI.Climate/** ✅ NEW PROJECT
- **Simulation**: Advanced climate modeling
- **Analysis**: Habitability calculations
- **Forecasting**: Temperature/pressure predictions
- **Configuration**: Simulation parameters

---

## 📋 Future Feature Modules

### Potential Sub-Projects
```
PerAspera.GameAPI.Buildings/     → Building management
PerAspera.GameAPI.Resources/     → Resource tracking
PerAspera.GameAPI.Technology/    → Tech tree management
PerAspera.GameAPI.Factions/      → Faction operations
PerAspera.GameAPI.POI/           → Points of Interest
PerAspera.GameAPI.Weather/       → Weather systems
PerAspera.GameAPI.Geology/       → Terrain/geology
```

### When to Create a Sub-Project?
1. **Domain complexity** > 5-10 files
2. **Independent feature** with clear boundaries
3. **Optional functionality** (not core to SDK)
4. **Reusable across mods**

---

## ✅ Current Status

### Completed
- ✅ PerAspera.Core
- ✅ PerAspera.GameAPI (base)
- ✅ PerAspera.GameAPI.Events
- ✅ PerAspera.GameAPI.Overrides
- ✅ PerAspera.GameAPI.Wrappers
- ✅ PerAspera.GameAPI.Climate (structure created)

### In Progress
- 🔄 GameAPI/Helpers/ClimateHelper.cs
- 🔄 GameAPI/Models/ (Climate DTOs)
- 🔄 GameAPI.Events/Native/ClimateEvents.cs
- 🔄 GameAPI.Climate/Simulation/

### To Do
- ⏳ GameAPI.Climate implementation
- ⏳ Documentation updates
- ⏳ Example mods using new structure
