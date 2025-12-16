# ?? PerAspera ModSDK

**The official Per Aspera modding SDK** - Simplify mod development with clean, event-driven APIs.

## ?? Quick Start

### 1. Create Your Mod
```csharp
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class MyAmazingMod : BasePlugin
{
    public override void Load()
    {
        // Initialize SDK
        ModSDK.Initialize("MyAmazingMod", "1.0.0");
        
        // Subscribe to events
        ModSDK.Events.Subscribe(GameEvents.MartianDayPassed, OnMartianDay);
        
        ModSDK.Log.Info("My amazing mod loaded!");
    }

    public override bool Unload()
    {
        ModSDK.Shutdown();
        return true;
    }

    private void OnMartianDay(object eventData)
    {
        var sol = ModSDK.Universe.GetCurrentSol();
        ModSDK.Log.Info($"New Martian day! Sol: {sol}");
    }
}
```

### 2. Install SDK in Your Project
```xml
<ProjectReference Include="..\..\SDK\PerAspera.ModSDK\PerAspera.ModSDK.csproj" />
```

## ?? Available APIs

### ?? Events - Event-driven Development
```csharp
// Subscribe to game events
ModSDK.Events.Subscribe(GameEvents.MartianDayPassed, OnMartianDay);
ModSDK.Events.Subscribe(GameEvents.TemperatureChanged, OnTemperatureChanged);

// Publish custom mod events
ModSDK.Events.Publish("MyMod_ResourceDiscovered", resourceData);

// Get event statistics
var stats = ModSDK.Events.GetStats();
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
    // Safe to access game data
}
```

### ?? Logging - Proper Debug Output
```csharp
ModSDK.Log.Info("Information message");
ModSDK.Log.Warning("Warning message");
ModSDK.Log.Error("Error message");
ModSDK.Log.Debug("Debug message");
```

## ?? Why Use ModSDK?

### ? **Before (Complex)**
```csharp
// Manual Mirror class usage
MainWrapper.EventBus.Subscribe("PlanetDaysPassed", handler);

// Continuous scanning (inefficient)
private void Update()
{
    if (Time.time - lastCheck > 0.25f)
    {
        ScanAllBuildings(); // Expensive!
        lastCheck = Time.time;
    }
}
```

### ? **After (Simple)**
```csharp
// Clean SDK usage
ModSDK.Events.Subscribe(GameEvents.MartianDayPassed, OnMartianDay);

// Event-driven (efficient)
private void OnMartianDay(object eventData)
{
    // Runs once per sol - perfect timing!
    UpdateModLogic();
}
```

## ?? Available Events

### ?? **Currently Available**
- `GameEvents.MartianDayPassed` - New Martian day
- `GameEvents.TemperatureChanged` - Planet temperature changed
- `GameEvents.AtmosphereChanged` - Atmosphere composition changed
- `GameEvents.WaterStockChanged` - Water stock changed

### ?? **Planned (Coming Soon)**
- `GameEvents.BuildingConstructed` - New building built
- `GameEvents.ProductionCompleted` - Building finished production
- `GameEvents.ResourceExtracted` - Resource extracted from vein

## ?? Project Structure

```
SDK/
??? PerAspera.ModSDK/          # ?? Main SDK (your entry point)
??? PerAspera.Core/            # ???  Core utilities and extensions
??? PerAspera.GameAPI/         # ?? Game API wrappers and mirrors
```

## ?? Example Mods

See `/Individual-Mods/` for complete working examples:
- **ClimatAspera** - Climate monitoring and analysis
- **NativeEvents** - Event system showcase
- **AsperaBaseGameProvider** - Game state monitoring

## ?? Dependencies

- **.NET 6.0**
- **BepInEx 6.x IL2CPP**
- **Per Aspera v1.4+**

## ??? Building Your Mod

1. Create new BepInEx plugin project
2. Add reference to `PerAspera.ModSDK`
3. Initialize SDK in your `Load()` method
4. Use event-driven architecture
5. Call `ModSDK.Shutdown()` in `Unload()`

---

**Happy modding! ?? Mars awaits your creativity!**