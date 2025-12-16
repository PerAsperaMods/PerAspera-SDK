# PerAspera.GameAPI.Overrides

## 🎯 Generic Override System v2.0

Type-safe, extensible getter override system with strategy pattern, auto-discovery, and validation.

### ✨ Features

- **✅ Generic Type Support**: Override `float`, `int`, `bool`, `string`, `Vector3`, any type
- **✅ Strategy Pattern**: Replace, Multiply, Clamp, custom strategies
- **✅ Auto-Discovery**: Reflection-based patch discovery with attributes
- **✅ Thread-Safe**: Concurrent registry with event subscriptions
- **✅ Validation**: Runtime type checking and value validation
- **✅ Performance**: Optimized lookups, zero-allocation paths

### 📁 Architecture

```
PerAspera.GameAPI.Overrides/
├── Models/
│   ├── GetterOverride<T>.cs          # Generic override configuration
│   ├── IOverrideStrategy<T>.cs       # Strategy interface
│   └── OverrideStrategies/
│       ├── ReplaceStrategy<T>.cs     # Simple replacement
│       ├── MultiplyStrategy.cs       # Multiply numeric values
│       └── ClampStrategy.cs          # Clamp to min/max
├── Registry/
│   └── GetterOverrideRegistry.cs     # Thread-safe registry
├── Patching/
│   ├── AutoOverridePatchAttribute.cs # Auto-discovery attribute
│   ├── OverridePatchSystem.cs        # Reflection-based patching
│   └── OverridePatchHelpers.cs       # Boilerplate reduction
└── Validation/
    ├── IOverrideValidator<T>.cs      # Validation interface
    └── TypeCompatibilityChecker.cs   # Runtime type validation
```

### 🚀 Usage Example

#### 1. Register an Override

```csharp
using PerAspera.GameAPI.Overrides.Models;
using PerAspera.GameAPI.Overrides.Registry;

// Simple float override
var tempOverride = new GetterOverride<float>(
    "Planet", "GetAverageTemperature", "Temperature Override",
    defaultValue: -60f
);
tempOverride.SetValue(-30f);
tempOverride.SetEnabled(true);
GetterOverrideRegistry.RegisterOverride(tempOverride);

// Boolean override
var aliveOverride = new GetterOverride<bool>(
    "Building", "IsAlive", "Force Alive",
    defaultValue: false
);
GetterOverrideRegistry.RegisterOverride(aliveOverride);
```

#### 2. Create a Harmony Patch with Auto-Discovery

```csharp
using HarmonyLib;
using PerAspera.GameAPI.Overrides.Patching;

[AutoOverridePatch("Planet", "GetAverageTemperature", Category = "Climate")]
public static class PlanetTemperaturePatch
{
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Planet), "GetAverageTemperature")]
    public static void Postfix(ref float __result, Planet __instance)
    {
        // Option 1: Manual
        var overrideConfig = GetterOverrideRegistry.GetOverride<float>("Planet", "GetAverageTemperature");
        if (overrideConfig?.IsEnabled == true)
            __result = overrideConfig.ApplyStrategy(__result, __instance);

        // Option 2: Helper (cleaner)
        OverridePatchHelpers.ApplyOverride(ref __result, "Planet", "GetAverageTemperature", __instance);
    }
}
```

#### 3. Use Strategies

```csharp
using PerAspera.GameAPI.Overrides.Models.OverrideStrategies;

// Multiply energy production by 2x
var energyOverride = new GetterOverride<float>(
    "SolarPanel", "GetEnergyProduction", "Energy Multiplier",
    defaultValue: 2.0f,
    strategy: new MultiplyStrategy()
);

// Clamp temperature between -60 and +20
var clampOverride = new GetterOverride<float>(
    "Planet", "GetTemperature", "Temperature Clamp",
    defaultValue: 20f,
    strategy: new ClampStrategy(-60f, 20f)
);
```

#### 4. Initialize Auto-Discovery

```csharp
using PerAspera.GameAPI.Overrides.Patching;

// Initialize patch system
OverridePatchSystem.Initialize("MyMod.Overrides");

// Discover and apply all patches with [AutoOverridePatch]
OverridePatchSystem.DiscoverAndApplyPatches(Assembly.GetExecutingAssembly());

// Get statistics
var stats = OverridePatchSystem.GetStatistics();
Logger.LogInfo(stats.ToString()); // "Patches: 5 applied | Categories: [Climate=2, Energy=3]"
```

### 🔧 Migration from v1.x

**Old (v1.x):**
```csharp
var override = new GetterOverride("Planet", "GetTemperature", "Temp", 0f, -100f, 100f, "Climate");
override.SetValue(25f);
```

**New (v2.0):**
```csharp
var override = new GetterOverride<float>("Planet", "GetTemperature", "Temp", 0f)
{
    Category = "Climate",
    Validator = value => value >= -100f && value <= 100f
};
override.SetValue(25f);
```

### 📊 Benefits

| Feature | v1.x | v2.0 |
|---------|------|------|
| Type Support | `float` only | ✅ **Any type** |
| Type Safety | Runtime errors | ✅ **Compile-time** |
| Strategies | ❌ None | ✅ **Replace, Multiply, Clamp, Custom** |
| Auto-Discovery | ❌ Hardcoded | ✅ **Attribute-based** |
| Validation | ❌ Min/Max only | ✅ **Custom validators** |
| Performance | Dictionary lookup | ✅ **Optimized + caching** |

### 🎯 Best Practices

1. **Use specific types**: `GetterOverride<int>` not `GetterOverride<object>`
2. **Add validators**: Prevent invalid values at registration
3. **Use strategies**: For complex transformations (multiply, clamp)
4. **Subscribe to events**: React to value changes
5. **Use auto-discovery**: Cleaner code, less boilerplate

### 📦 Dependencies

- `PerAspera.Core` - Logging and utilities
- `PerAspera.Core.IL2CppExtensions` - IL2CPP interop
- `BepInEx.Unity.IL2CPP` - BepInEx framework
- `HarmonyX` - Runtime patching

---

**Version**: 2.0.0  
**Breaking Changes**: Yes (from v1.x)  
**Migration Guide**: See SDK/CHANGELOG.md
