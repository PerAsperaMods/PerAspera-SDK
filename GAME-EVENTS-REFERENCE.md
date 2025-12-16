# ?? Game Events Reference - PerAspera ModSDK

Complete reference of all available game events in the PerAspera ModSDK.

## ?? Usage Pattern

```csharp
using PerAspera.ModSDK;

// Subscribe to events
ModSDK.Events.Subscribe(GameEvents.MartianDayPassed, OnMartianDay);

// Event handler
private void OnMartianDay(object eventData)
{
    var sol = ModSDK.Universe.GetCurrentSol();
    ModSDK.Log.Info($"New Martian day: Sol {sol}");
}
```

## ?? Available Events

### ?? **Martian Time Events**

#### `GameEvents.MartianDayPassed`
- **Description**: Fired when a new Martian sol (day) begins
- **Event Data**: `MartianDayEventData`
- **Frequency**: Once per Martian day
- **Use Case**: Daily calculations, progress tracking

```csharp
public class MartianDayEventData
{
    public int DaysPassed { get; set; }     // Current sol number
    public Planet Planet { get; set; }      // Planet instance
    public DateTime Timestamp { get; set; } // When event fired
}
```

#### `GameEvents.MartianSeasonChanged`
- **Description**: Fired when Martian season changes
- **Event Data**: `MartianSeasonEventData`
- **Frequency**: 4 times per Martian year
- **Use Case**: Seasonal modifications, weather patterns

### ??? **Climate Events**

#### `GameEvents.TemperatureChanged`
- **Description**: Fired when planetary temperature changes significantly
- **Event Data**: `ClimateEventData`
- **Frequency**: Variable (temperature fluctuations)
- **Use Case**: Climate monitoring, building efficiency

```csharp
public class ClimateEventData
{
    public string EventType { get; set; }     // Event type name
    public float CurrentValue { get; set; }   // Current value
    public float? PreviousValue { get; set; } // Previous value (if available)
    public float? Delta { get; set; }         // Change amount
    public int MartianSol { get; set; }       // Current sol
    public DateTime Timestamp { get; set; }   // When event occurred
    public object? RawGameEvent { get; set; } // Raw game event data
}
```

#### `GameEvents.AtmosphereChanged`
- **Description**: Fired when atmospheric composition changes
- **Event Data**: `ClimateEventData`
- **Frequency**: Variable (terraforming progress)
- **Use Case**: Atmosphere monitoring, progress tracking

#### `GameEvents.WaterStockChanged`
- **Description**: Fired when planetary water reserves change
- **Event Data**: `ClimateEventData`
- **Frequency**: Variable (water production/consumption)
- **Use Case**: Water management, resource planning

### ?? **Building Events (Planned - Coming Soon)**

#### `GameEvents.BuildingConstructed`
- **Description**: Fired when a new building is placed
- **Event Data**: `BuildingEventData`
- **Frequency**: Per building construction
- **Use Case**: Building tracking, economy analysis

#### `GameEvents.BuildingDestroyed`
- **Description**: Fired when a building is destroyed
- **Event Data**: `BuildingEventData`
- **Frequency**: Per building destruction
- **Use Case**: Loss tracking, rebuilding logic

#### `GameEvents.ProductionCompleted`
- **Description**: Fired when a building completes production
- **Event Data**: `ProductionEventData`
- **Frequency**: Per production cycle
- **Use Case**: Production monitoring, efficiency tracking

### ?? **Resource Events (Planned - Coming Soon)**

#### `GameEvents.ResourceExtracted`
- **Description**: Fired when resources are extracted from veins
- **Event Data**: `ResourceEventData`
- **Frequency**: Per extraction operation
- **Use Case**: Resource tracking, depletion monitoring

#### `GameEvents.ResourceConsumed`
- **Description**: Fired when resources are consumed by buildings
- **Event Data**: `ResourceEventData`
- **Frequency**: Per consumption operation
- **Use Case**: Consumption analysis, efficiency tracking

#### `GameEvents.ResourceStockChanged`
- **Description**: Fired when resource stockpiles change significantly
- **Event Data**: `ResourceEventData`
- **Frequency**: Variable (stock changes)
- **Use Case**: Inventory management, supply chain monitoring

## ?? Event Data Types

### Base Event Data
All events include these basic properties:

```csharp
public class BaseEventData
{
    public string EventType { get; set; }
    public int MartianSol { get; set; }
    public DateTime Timestamp { get; set; }
    public Planet Planet { get; set; }
}
```

### Specialized Event Data

#### ClimateEventData
```csharp
public class ClimateEventData : BaseEventData
{
    public float CurrentValue { get; set; }
    public float? PreviousValue { get; set; }
    public float Delta => CurrentValue - (PreviousValue ?? 0);
    public string Units { get; set; }          // Temperature: °C, Pressure: kPa, etc.
    public object RawGameEvent { get; set; }   // Original game event
}
```

#### MartianDayEventData
```csharp
public class MartianDayEventData : BaseEventData
{
    public int DaysPassed { get; set; }
    public int Season { get; set; }            // Current Martian season
    public float DayLength { get; set; }       // Hours in this sol
}
```

#### BuildingEventData (Planned)
```csharp
public class BuildingEventData : BaseEventData
{
    public string BuildingType { get; set; }
    public Vector3 Position { get; set; }
    public string BuildingId { get; set; }
    public object BuildingInstance { get; set; }
}
```

## ?? Event Frequency Guide

### High Frequency Events (Handle efficiently)
- `ProductionCompleted` - Multiple times per sol
- `ResourceConsumed` - Continuous during production
- `ResourceExtracted` - Per extraction operation

### Medium Frequency Events
- `TemperatureChanged` - Several times per sol
- `AtmosphereChanged` - Changes during terraforming
- `BuildingConstructed` - Player dependent

### Low Frequency Events (Safe for heavy operations)
- `MartianDayPassed` - Once per sol (24.6 hours)
- `MartianSeasonChanged` - 4 times per Martian year
- `WaterStockChanged` - Occasional large changes

## ??? Best Practices

### Performance
```csharp
// ? Good: Light processing for high-frequency events
private void OnProductionCompleted(object eventData)
{
    _productionCount++; // Simple counter
    // Heavy analysis scheduled for daily update
}

// ? Good: Heavy processing for low-frequency events
private void OnMartianDay(object eventData)
{
    AnalyzeCompleteEconomy(); // Complex analysis once per day
}
```

### Error Handling
```csharp
private void OnEvent(object eventData)
{
    try
    {
        if (eventData is ExpectedEventType data)
        {
            ProcessEvent(data);
        }
    }
    catch (Exception ex)
    {
        ModSDK.Log.Error($"Event handler error: {ex.Message}");
        // Continue gracefully, don't break the event system
    }
}
```

### Cleanup
```csharp
public override bool Unload()
{
    // Unsubscribe from all events before shutdown
    ModSDK.Events.Unsubscribe(GameEvents.MartianDayPassed, OnMartianDay);
    ModSDK.Shutdown();
    return true;
}
```

## ?? Custom Mod Events

You can also publish custom events for inter-mod communication:

```csharp
// Publishing mod event
ModSDK.Events.Publish("MyMod_CustomEvent", new { Data = "value" });

// Other mods can subscribe to your events
ModSDK.Events.Subscribe("MyMod_CustomEvent", OnCustomEvent);
```

### Naming Convention
- Use your mod name as prefix: `"ModName_EventName"`
- Use PascalCase: `"ClimatAspera_AnalysisComplete"`
- Be descriptive: `"PowerMod_GridOverloadDetected"`

## ?? Event Statistics

Get real-time statistics about event subscriptions:

```csharp
var stats = ModSDK.Events.GetStats();
foreach (var kvp in stats)
{
    ModSDK.Log.Info($"Event '{kvp.Key}': {kvp.Value} subscribers");
}
```

---

**?? Ready to build event-driven mods?** Use these events to create responsive, efficient Per Aspera modifications!

**?? For more examples, see:** `/SDK/Examples/ExampleMod.cs`