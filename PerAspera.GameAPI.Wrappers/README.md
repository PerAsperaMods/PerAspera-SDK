# PerAspera.GameAPI.Wrappers

**Public API wrappers for Per Aspera native game classes**

## 🎯 Purpose

This package provides **type-safe, documented wrappers** around native IL2CPP game classes. Modders should **ONLY** use these wrappers, never access native game objects directly.

## ✅ What Are Wrappers?

Wrappers are **safe abstractions** that:
- ✅ Hide IL2CPP complexity
- ✅ Provide IntelliSense documentation
- ✅ Handle errors gracefully
- ✅ Validate inputs
- ✅ Log warnings when operations fail

## 📦 Available Wrappers

### Core Game Objects

| Wrapper | Native Class | Purpose |
|---------|-------------|---------|
| `Building` | `Building` | Buildings, factories, infrastructure |
| `Planet` | `Planet` | Climate, resources, planetary data |
| `Universe` | `Universe` | Time, game state, speed control |

*More wrappers coming soon: Drone, Faction, Technology, etc.*

## 🚀 Usage

### Get Current Game State

```csharp
using PerAspera.GameAPI.Wrappers;

// Get current planet
var planet = Planet.GetCurrent();
if (planet != null)
{
    Logger.LogInfo($"Temperature: {planet.Temperature}K");
    Logger.LogInfo($"Pressure: {planet.TotalPressure} kPa");
}

// Get current universe (time/game state)
var universe = Universe.GetCurrent();
if (universe != null)
{
    Logger.LogInfo($"Current Sol: {universe.CurrentSol}");
    Logger.LogInfo($"Game Speed: {universe.GameSpeed}x");
}
```

### Modify Climate

```csharp
var planet = Planet.GetCurrent();
if (planet != null)
{
    // Increase temperature
    planet.Temperature += 10f;
    
    // Add oxygen
    planet.O2Pressure += 5f;
    
    // Add water
    planet.WaterStock += 1000f;
}
```

### Work with Buildings

```csharp
// Wrap a native building object from an event
ModEventBus.Subscribe("NativeBuildingSpawned", (data) => {
    if (data is BuildingSpawnedNativeEvent evt)
    {
        var building = Building.FromNative(evt.Building);
        if (building != null)
        {
            Logger.LogInfo($"Building spawned: {building.TypeKey} at {building.Position}");
            Logger.LogInfo($"Health: {building.Health}, Powered: {building.IsPowered}");
        }
    }
});
```

### Control Game Speed

```csharp
var universe = Universe.GetCurrent();
if (universe != null)
{
    // Speed up game
    universe.GameSpeed = 3.0f;
    
    // Pause game
    universe.Pause();
    
    // Wait for condition then unpause
    if (planet.Temperature > 273.15f)
        universe.Unpause();
}
```

## ⚠️ Important Rules

### ✅ DO Use Wrappers

```csharp
// ✅ CORRECT - Use wrapper
var planet = Planet.GetCurrent();
float temp = planet.Temperature;
```

### ❌ DON'T Access Native Objects

```csharp
// ❌ WRONG - Direct access to native
var planet = KeeperTypeRegistry.GetPlanet(); // Internal API!
float temp = planet.InvokeMethod<float>("GetTemperature"); // Unsafe!
```

### ✅ DO Check for Null

```csharp
var planet = Planet.GetCurrent();
if (planet != null && planet.IsValid)
{
    // Safe to use
    planet.Temperature = 300f;
}
```

### ❌ DON'T Assume Objects Exist

```csharp
// ❌ WRONG - Might crash if planet not loaded
var planet = Planet.GetCurrent();
planet.Temperature = 300f; // NullReferenceException!
```

## 🏗️ Architecture

```
Modder Code
    ↓ (uses)
PerAspera.GameAPI.Wrappers  ← Public API (this package)
    ↓ (accesses via)
PerAspera.GameAPI.Native    ← Internal (KeeperTypeRegistry, NativeEventPatcher)
    ↓ (wraps)
Per Aspera Game (IL2CPP)    ← Native game code
```

**Key Point**: Modders interact ONLY with Wrappers, never with Native or IL2CPP layers.

## 🔧 Extending Wrappers

All wrappers inherit from `WrapperBase` which provides:

- ✅ Null checking
- ✅ Error handling
- ✅ Logging
- ✅ Safe method invocation

Example of creating a new wrapper:

```csharp
public class Drone : WrapperBase
{
    public Drone(object nativeDrone) : base(nativeDrone) { }
    
    public static Drone? FromNative(object? nativeDrone)
    {
        return nativeDrone != null ? new Drone(nativeDrone) : null;
    }
    
    // Properties using SafeInvoke
    public string DroneType => SafeInvoke<string>("GetDroneType") ?? "Unknown";
    
    public (float x, float y) Position
    {
        get
        {
            var pos = SafeInvoke<object>("get_position");
            if (pos == null) return (0, 0);
            return (pos.GetFieldValue<float>("x"), pos.GetFieldValue<float>("y"));
        }
    }
    
    // Actions using SafeInvokeVoid
    public void GoTo(float x, float y)
    {
        SafeInvokeVoid("GoToPosition", x, y);
    }
}
```

## 📚 See Also

- [PerAspera.Core](../PerAspera.Core/README.md) - Low-level utilities
- [PerAspera.GameAPI](../PerAspera.GameAPI/README.md) - Native events and internals
- [PerAspera.ModSDK](../PerAspera.ModSDK/README.md) - Complete SDK

## 🆘 Support

If a wrapper is missing or has incorrect data:
1. Check the game version compatibility
2. Verify the wrapper is up to date
3. Report issues with SDK version and game version

---

**Version**: 2.0.0  
**Game Compatibility**: Per Aspera 1.5.x  
**Status**: ✅ Public API - Safe for modders
