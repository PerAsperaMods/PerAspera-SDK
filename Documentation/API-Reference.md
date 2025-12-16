# API Reference

Complete reference documentation for all PerAspera SDK APIs, classes, and methods.

## 📚 Table of Contents

- [ModSDK API](#modsdk-api) - Main entry point
- [GameAPI](#gameapi) - Direct game access
- [Climate System API](#climate-system-api) - Atmospheric simulation
- [Building System API](#building-system-api) - Infrastructure management
- [Event System API](#event-system-api) - Event handling
- [Override System API](#override-system-api) - Value modification
- [Utility APIs](#utility-apis) - Helper functions

---

## ModSDK API

The main entry point for all SDK functionality. Always initialize this first.

### Static Methods

#### `Initialize(BasePlugin plugin)`
Initializes the entire SDK system with your plugin instance.

**Parameters:**
- `plugin` *(BasePlugin)*: Your BepInEx plugin instance

**Throws:**
- `ModSDKException`: If initialization fails
- `InvalidOperationException`: If already initialized

**Example:**
```csharp
public override void Load()
{
    ModSDK.Initialize(this);  // Pass your plugin instance
}
```

---

### ModSDK.Universe

Access to universe-wide game data and time systems.

#### `GetCurrentSol() -> int`
Gets the current Martian sol (day) number.

**Returns:** Current sol as integer (starts from 1)

**Example:**
```csharp
int currentSol = ModSDK.Universe.GetCurrentSol();
Log.LogInfo($"Today is Sol {currentSol}");
```

#### `GetClimateData() -> PlanetClimateData?`
Gets comprehensive climate data for the current planet.

**Returns:** Climate data object or null if not available

**Example:**
```csharp
var climate = ModSDK.Universe.GetClimateData();
if (climate != null)
{
    Log.LogInfo($"Temperature: {climate.Temperature}K");
    Log.LogInfo($"Pressure: {climate.TotalPressure} atm");
}
```

#### `IsGameReady() -> bool`
Checks if the game is fully loaded and systems are available.

**Returns:** True if game ready, false otherwise

**Example:**
```csharp
if (ModSDK.Universe.IsGameReady())
{
    StartMonitoring();
}
else
{
    Log.LogInfo("Waiting for game to load...");
}
```

#### `GetPlanet() -> object?`
Gets the raw planet instance (advanced usage).

**Returns:** Planet object or null if not available

**Example:**
```csharp
var planet = ModSDK.Universe.GetPlanet();
// Use with caution - direct IL2CPP access
```

#### `GetPlanetMirror() -> MirrorPlanet?`
Gets the planet wrapped in a safe Mirror interface.

**Returns:** Mirror-wrapped planet or null

**Example:**
```csharp
var planet = ModSDK.Universe.GetPlanetMirror();
if (planet != null)
{
    var temperature = planet.GetTemperature();
}
```

---

### ModSDK.Events

Unified event system for subscribing to game events.

#### `Subscribe(string eventName, Action<object> handler)`
Subscribe to a named event with a handler function.

**Parameters:**
- `eventName` *(string)*: Name of event to subscribe to
- `handler` *(Action<object>)*: Function to call when event occurs

**Example:**
```csharp
ModSDK.Events.Subscribe("climate.temperatureChanged", OnTemperatureChanged);

private void OnTemperatureChanged(object eventData)
{
    Log.LogInfo($"Temperature changed: {eventData}");
}
```

#### `Unsubscribe(string eventName, Action<object> handler)`
Remove a specific event handler.

**Parameters:**
- `eventName` *(string)*: Name of event to unsubscribe from  
- `handler` *(Action<object>)*: Handler function to remove

#### `UnsubscribeAll()`
Remove all event subscriptions for this mod.

**Example:**
```csharp
public void OnDestroy()
{
    ModSDK.Events.UnsubscribeAll();  // Cleanup
}
```

#### Common Event Names

| Event Name | Description | Data Type |
|------------|-------------|-----------|
| `"game.loaded"` | Game finished loading | `null` |
| `"planet.initialized"` | Planet systems ready | `PlanetData` |
| `"climate.temperatureChanged"` | Temperature changed | `ClimateEventData` |
| `"climate.atmosphereChanged"` | Atmospheric composition changed | `ClimateEventData` |
| `"building.constructed"` | Building completed | `BuildingEventData` |
| `"building.destroyed"` | Building removed | `BuildingEventData` |

---

### ModSDK.Log

Enhanced logging system with structured output.

#### `Info(string message)`
Log informational message.

#### `Warning(string message)`  
Log warning message.

#### `Error(string message)`
Log error message.

#### `Debug(string message)`
Log debug message (only in debug builds).

**Example:**
```csharp
ModSDK.Log.Info("🚀 Mod started successfully");
ModSDK.Log.Warning("⚠️ Climate data unavailable");
ModSDK.Log.Error("❌ Failed to initialize system");
ModSDK.Log.Debug("🔍 Debug info for developers");
```

---

## GameAPI

Low-level access to Per Aspera game systems.

### GameAPI.BaseGame

Access to the main game singleton and core systems.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsGameLoaded` | `bool` | True if game is fully loaded |
| `GameVersion` | `string` | Current game version |
| `IsInGame` | `bool` | True if actively playing (not in menus) |

#### Methods

#### `GetGameState() -> GameState`
Gets current game state (loading, playing, paused, etc.).

#### `GetBlackboard() -> object?`
Gets the game's data blackboard for advanced access.

**Example:**
```csharp
if (GameAPI.BaseGame.IsGameLoaded && GameAPI.BaseGame.IsInGame)
{
    var blackboard = GameAPI.BaseGame.GetBlackboard();
    // Advanced game data access
}
```

---

### GameAPI.Universe  

Access to universe, planet, and faction management.

#### `GetMartianSol() -> int`
Gets current Martian sol number.

#### `GetRawPlanet() -> object?`
Gets raw planet instance (use with caution).

#### `GetAllPlanets() -> object[]`
Gets array of all planets in universe.

#### `GetPlayerFaction() -> object?`
Gets the player's faction instance.

**Example:**
```csharp
int sol = GameAPI.Universe.GetMartianSol();
var planets = GameAPI.Universe.GetAllPlanets();
Log.LogInfo($"Sol {sol}: Managing {planets.Length} planets");
```

---

### GameAPI.Planet

Direct planet instance access and manipulation.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Temperature` | `float` | Current temperature in Kelvin |
| `AtmosphericPressure` | `float` | Total atmospheric pressure |
| `WaterStock` | `float` | Available water resources |

#### Methods

#### `GetClimateData() -> PlanetClimateData`
Gets comprehensive climate information.

#### `GetBuildingCount(string buildingType) -> int`
Gets count of specific building type.

#### `GetResourceStock(string resourceName) -> float`
Gets current stock of specified resource.

**Example:**
```csharp
var planet = GameAPI.Planet;
var temp = planet.Temperature;
var pressure = planet.AtmosphericPressure;

int domes = planet.GetBuildingCount("ResidentialDome");
float water = planet.GetResourceStock("Water");

Log.LogInfo($"Planet: {temp}K, {pressure}atm, {domes} domes, {water} water");
```

---

## Climate System API

Advanced atmospheric simulation and climate modeling.

### ClimateSimulation

Main climate simulation system.

#### Static Properties

| Property | Type | Description |
|----------|------|-------------|
| `IsInitialized` | `bool` | True if climate system ready |
| `Atmosphere` | `AtmosphericComposition` | Current atmospheric state |

#### Static Methods

#### `GetCurrentStatus() -> ClimateStatus`
Gets comprehensive current climate status.

```csharp
var status = ClimateSimulation.GetCurrentStatus();
Log.LogInfo($"Climate: {status.Temperature}K, {status.Breathability}% breathable");
```

#### `PredictClimate(int solsInFuture) -> ClimateStatus`
Predicts climate state after specified sols.

```csharp
var futureClimate = ClimateSimulation.PredictClimate(100);
Log.LogInfo($"In 100 sols: {futureClimate.Temperature}K predicted");
```

#### Events

#### `ClimateUpdated` *(event Action<ClimateSimulation>)*
Fired when climate system updates.

#### `SignificantClimateChange` *(event Action<ClimateChangeEvent>)*
Fired when major climate changes occur.

**Example:**
```csharp
ClimateSimulation.ClimateUpdated += OnClimateUpdate;
ClimateSimulation.SignificantClimateChange += OnMajorChange;

private void OnClimateUpdate(ClimateSimulation sim)
{
    // Regular climate updates
}

private void OnMajorChange(ClimateChangeEvent change)
{
    Log.LogInfo($"Major climate change: {change.Description}");
}
```

---

### AtmosphericComposition

Detailed atmospheric gas management.

#### Methods

#### `RegisterGas(string symbol, string name, float concentration, bool isGreenhouseGas)`
Register a new atmospheric gas.

**Parameters:**
- `symbol` *(string)*: Chemical symbol (e.g., "CH4")
- `name` *(string)*: Full name (e.g., "Methane")  
- `concentration` *(float)*: Initial concentration (0.0-1.0)
- `isGreenhouseGas` *(bool)*: Whether gas contributes to greenhouse effect

#### `SetGasConcentration(string symbol, float concentration)`
Set concentration of existing gas.

#### `GetGasConcentration(string symbol) -> float`
Get current concentration of gas.

#### `GetTotalPressure() -> float`
Calculate total atmospheric pressure.

#### `CalculateGreenhouseEffect() -> float`
Calculate current greenhouse warming effect.

#### `CalculateBreathabilityIndex() -> float`
Calculate how breathable atmosphere is (0-100%).

**Example:**
```csharp
var atmosphere = ClimateSimulation.Atmosphere;

// Add methane to atmosphere
atmosphere.RegisterGas("CH4", "Methane", 0.001f, true);

// Increase oxygen for breathability
atmosphere.SetGasConcentration("O2", 0.18f);

// Check results
float pressure = atmosphere.GetTotalPressure();
float greenhouse = atmosphere.CalculateGreenhouseEffect();
float breathability = atmosphere.CalculateBreathabilityIndex();

Log.LogInfo($"Atmosphere: {pressure:F3}atm, {greenhouse:F2} greenhouse, {breathability:F1}% breathable");
```

#### Events

#### `CompositionChanged` *(event Action<AtmosphericComposition>)*
Fired when gas concentrations change.

---

### ClimateStatus (Data Class)

Comprehensive climate information structure.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Temperature` | `float` | Temperature in Kelvin |
| `AtmosphericPressure` | `float` | Total pressure in atmospheres |
| `GreenhouseEffect` | `float` | Greenhouse warming effect |
| `BreathabilityIndex` | `float` | Breathability percentage (0-100) |
| `CO2Pressure` | `float` | CO2 partial pressure |
| `O2Pressure` | `float` | O2 partial pressure |
| `N2Pressure` | `float` | N2 partial pressure |
| `WaterVapor` | `float` | Water vapor concentration |
| `IsHabitable` | `bool` | Whether suitable for human life |

---

### PlanetClimateData (Data Class)

Raw climate data from game systems.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Temperature` | `float` | Surface temperature (K) |
| `CO2Pressure` | `float` | CO2 partial pressure |
| `O2Pressure` | `float` | O2 partial pressure |
| `N2Pressure` | `float` | N2 partial pressure |
| `GHGPressure` | `float` | Greenhouse gas pressure |
| `WaterStock` | `float` | Available water |
| `TotalPressure` | `float` | Total atmospheric pressure |
| `IsHabitable` | `bool` | Game's habitability flag |
| `PlanetName` | `string` | Name of planet |
| `Timestamp` | `DateTime` | When data was captured |

---

## Building System API

Infrastructure monitoring and interaction.

### BuildingSystem

Main building management system.

#### Static Methods

#### `GetAllBuildings() -> BuildingInfo[]`
Gets information about all buildings on planet.

#### `GetBuildingsByType(string buildingType) -> BuildingInfo[]`
Gets all buildings of specific type.

#### `GetBuildingProduction(object building) -> ProductionData`
Gets production information for a building.

#### `GetAtmosphericImpact(object building) -> AtmosphereImpactData`
Gets how building affects planetary atmosphere.

**Example:**
```csharp
var allBuildings = BuildingSystem.GetAllBuildings();
var domes = BuildingSystem.GetBuildingsByType("ResidentialDome");

foreach (var dome in domes)
{
    var production = BuildingSystem.GetBuildingProduction(dome.RawBuilding);
    var impact = BuildingSystem.GetAtmosphericImpact(dome.RawBuilding);
    
    Log.LogInfo($"Dome {dome.Id}: producing {production.OutputRate}, atmosphere impact {impact.CO2Change}");
}
```

#### Events

#### `BuildingConstructed` *(event Action<BuildingEventData>)*
Fired when building construction completes.

#### `BuildingDestroyed` *(event Action<BuildingEventData>)*
Fired when building is destroyed.

#### `ProductionCompleted` *(event Action<ProductionEventData>)*
Fired when building completes production cycle.

---

### BuildingInfo (Data Class)

Information about a single building.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Id` | `string` | Unique building identifier |
| `Type` | `string` | Building type name |
| `Position` | `Vector3` | 3D position on planet |
| `IsOperational` | `bool` | Whether building is active |
| `PowerConsumption` | `float` | Current power usage |
| `Efficiency` | `float` | Operating efficiency (0-1) |
| `RawBuilding` | `object` | Direct game building object |

---

## Event System API

Comprehensive event handling system for game activities.

### EventSystem

Central event management system.

#### Static Methods

#### `Subscribe<T>(string eventName, Action<T> handler)`
Subscribe to typed event with specific data type.

```csharp
EventSystem.Subscribe<ClimateEventData>("climate.changed", OnClimateChanged);

private void OnClimateChanged(ClimateEventData data)
{
    Log.LogInfo($"Climate changed: {data.NewValue}");
}
```

#### `Fire(string eventName, object data)`
Fire a custom event (for advanced mod integration).

```csharp
EventSystem.Fire("mymod.customEvent", new { message = "Hello from my mod!" });
```

#### `RegisterEventType<T>(string eventPattern)`
Register a new event type for your mod.

```csharp
EventSystem.RegisterEventType<MyCustomEventData>("mymod.*");
```

---

### Event Data Classes

#### ClimateEventData

Climate change event information.

| Property | Type | Description |
|----------|------|-------------|
| `EventType` | `string` | Type of climate change |
| `CurrentValue` | `float` | New value |
| `PreviousValue` | `float` | Previous value |
| `Delta` | `float` | Change amount |
| `MartianSol` | `int` | Sol when change occurred |
| `RawGameEvent` | `object` | Original game event |

#### BuildingEventData

Building-related event information.

| Property | Type | Description |
|----------|------|-------------|
| `BuildingId` | `string` | Building identifier |
| `BuildingType` | `string` | Type of building |
| `Position` | `Vector3` | Building position |
| `EventType` | `string` | Event type (constructed/destroyed) |
| `Timestamp` | `DateTime` | When event occurred |

---

## Override System API

Dynamic runtime modification of game values.

### OverrideSystem

Value override management system.

#### Static Methods

#### `RegisterOverride(string key, float value, string description)`
Register a new value override.

**Parameters:**
- `key` *(string)*: Unique identifier (e.g., "buildings.dome.efficiency")
- `value` *(float)*: Override value
- `description` *(string)*: Human-readable description

```csharp
OverrideSystem.RegisterOverride("buildings.dome.powerEfficiency", 1.5f, "Increase dome power efficiency by 50%");
```

#### `UpdateOverride(string key, float newValue)`
Update existing override value.

#### `RemoveOverride(string key)`
Remove an override (restore original value).

#### `GetOverride(string key) -> GetterOverride?`
Get information about an existing override.

#### `GetAllOverrides() -> GetterOverride[]`
Get all active overrides.

**Example:**
```csharp
// Register override
OverrideSystem.RegisterOverride("climate.greenhouse.effect", 1.2f, "Boost greenhouse warming");

// Later update it
OverrideSystem.UpdateOverride("climate.greenhouse.effect", 0.8f);

// Check current value
var override = OverrideSystem.GetOverride("climate.greenhouse.effect");
if (override != null)
{
    Log.LogInfo($"Greenhouse override: {override.CurrentValue} (was {override.DefaultValue})");
}

// Remove when done
OverrideSystem.RemoveOverride("climate.greenhouse.effect");
```

---

### GetterOverride (Data Class)

Information about a value override.

#### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Key` | `string` | Unique override identifier |
| `DisplayName` | `string` | Human-readable name |
| `Description` | `string` | Override description |
| `DefaultValue` | `float` | Original game value |
| `CurrentValue` | `float` | Current override value |
| `MinValue` | `float` | Minimum allowed value |
| `MaxValue` | `float` | Maximum allowed value |
| `IsEnabled` | `bool` | Whether override is active |
| `Category` | `string` | Override category |
| `Units` | `string` | Value units (e.g., "K", "atm") |

---

## Utility APIs

Helper functions and utilities.

### MirrorHelper

Safe access to IL2CPP objects.

#### Static Methods

#### `SafeAccess<T>(object target, Func<object, T> accessor, T defaultValue)`
Safely access IL2CPP object properties with fallback.

```csharp
var temperature = MirrorHelper.SafeAccess(
    planet,
    p => ((PlanetInstance)p).Temperature,
    210.0f  // Default if access fails
);
```

#### `IsValidObject(object target) -> bool`
Check if IL2CPP object is valid and accessible.

#### `GetTypeName(object target) -> string`
Get the actual type name of an IL2CPP object.

---

### GameInstanceDetector

Automatic game installation detection.

#### Static Methods

#### `FindPerAsperaInstallation() -> string?`
Automatically locate Per Aspera installation directory.

#### `FindBepInExInstallation() -> string?`
Locate BepInEx installation within game directory.

#### `ValidateInstallation(string gamePath) -> bool`
Verify game installation is valid and complete.

**Example:**
```csharp
var gamePath = GameInstanceDetector.FindPerAsperaInstallation();
if (gamePath != null && GameInstanceDetector.ValidateInstallation(gamePath))
{
    Log.LogInfo($"Found valid Per Aspera installation: {gamePath}");
}
```

---

## Exception Classes

### ModSDKException

Base exception for all SDK-related errors.

#### Properties
- `Message` *(string)*: Error description
- `InnerException` *(Exception?)*: Original cause

#### Constructors
- `ModSDKException(string message)`
- `ModSDKException(string message, Exception innerException)`

### GameAPIException

Specific to GameAPI access errors.

### ClimateSystemException

Specific to climate system errors.

---

## Constants and Enumerations

### GameEvents (Static Class)

Pre-defined event name constants.

```csharp
public static class GameEvents
{
    // Climate events
    public const string TemperatureChanged = "climate.temperatureChanged";
    public const string AtmosphereChanged = "climate.atmosphereChanged";
    public const string WaterStockChanged = "climate.waterStockChanged";
    
    // Building events
    public const string BuildingConstructed = "building.constructed";
    public const string BuildingDestroyed = "building.destroyed";
    public const string ProductionCompleted = "building.productionCompleted";
    
    // Game state events
    public const string GameLoaded = "game.loaded";
    public const string PlanetInitialized = "planet.initialized";
    public const string DayPassed = "game.dayPassed";
}
```

### Common Building Types

| Constant | Description |
|----------|-------------|
| `"ResidentialDome"` | Living quarters for colonists |
| `"SolarArray"` | Solar power generation |
| `"AtmosphereProcessor"` | Atmospheric modification |
| `"WaterExtractor"` | Water resource extraction |
| `"OxygenGenerator"` | Oxygen production facility |

### Common Resource Types

| Constant | Description |
|----------|-------------|
| `"Water"` | Water resources |
| `"Power"` | Electrical energy |
| `"Oxygen"` | Breathable oxygen |
| `"Food"` | Food supplies |
| `"Minerals"` | Raw minerals |

---

## Usage Examples

### Complete Climate Monitoring System

```csharp
using PerAspera.ModSDK;
using PerAspera.GameAPI.Climate.Events;

public class AdvancedClimateMonitor : BasePlugin
{
    public override void Load()
    {
        ModSDK.Initialize(this);
        
        // Subscribe to all climate events
        ClimateSimulation.ClimateUpdated += OnClimateUpdate;
        ClimateSimulation.SignificantClimateChange += OnSignificantChange;
        
        // Subscribe to building events that affect climate
        EventSystem.Subscribe<BuildingEventData>(GameEvents.BuildingConstructed, OnBuildingBuilt);
        
        // Register climate enhancement overrides
        OverrideSystem.RegisterOverride("climate.greenhouse.multiplier", 1.3f, "Enhanced greenhouse effect");
        
        Log.LogInfo("Advanced Climate Monitor initialized");
    }
    
    private void OnClimateUpdate(ClimateSimulation simulation)
    {
        var status = ClimateSimulation.GetCurrentStatus();
        
        // Log detailed climate data
        ModSDK.Log.Debug($"Climate Update: {status.Temperature:F1}K, {status.BreathabilityIndex:F1}% breathable");
        
        // Check for approaching milestones
        if (status.BreathabilityIndex > 45f && status.BreathabilityIndex < 55f)
        {
            ModSDK.Log.Info("🎯 Approaching 50% breathability milestone!");
        }
    }
    
    private void OnSignificantChange(ClimateChangeEvent changeEvent)
    {
        ModSDK.Log.Info($"🌡️ Major climate change: {changeEvent.Description}");
        ModSDK.Log.Info($"   Temperature: {changeEvent.Temperature:F1}K");
        ModSDK.Log.Info($"   Breathability: {changeEvent.BreathabilityIndex:F1}%");
        
        // Predict future climate
        var prediction = ClimateSimulation.PredictClimate(50);
        ModSDK.Log.Info($"📈 Prediction (50 sols): {prediction.Temperature:F1}K, {prediction.BreathabilityIndex:F1}% breathable");
    }
    
    private void OnBuildingBuilt(BuildingEventData building)
    {
        if (building.BuildingType == "AtmosphereProcessor")
        {
            var impact = BuildingSystem.GetAtmosphericImpact(building.RawBuilding);
            ModSDK.Log.Info($"🏭 New atmosphere processor: {impact.CO2Change:+0.000;-0.000} CO2 change");
        }
    }
}
```

This API reference provides complete documentation for developing sophisticated Per Aspera mods using the SDK. For more examples and tutorials, see the [Examples](Examples/) directory.