# PerAspera.GameAPI.Events

**Type-safe event constants and helpers for Per Aspera modding**

## 🎯 Purpose

This package provides **the public event API layer** for Per Aspera game integration:
- ✅ **Type-safe constants** - No more magic strings
- ✅ **Helper utilities** - Filtering, type checking, formatting
- ✅ **IntelliSense support** - Full IDE autocomplete

Part of the GameAPI architecture, it exposes native game events with strong typing.

## 📦 What's Included

### 1. GameEventConstants
**Type-safe constants for all game events** - eliminates typo-prone magic strings:

```csharp
using PerAspera.GameAPI.Events;

// ✅ CORRECT - Type-safe, autocomplete works
EventBus.Subscribe(GameEventConstants.BuildingSpawned, handler);
EventBus.Subscribe(GameEventConstants.TemperatureChanged, handler);

// ❌ WRONG - Magic string (no autocomplete, typo-prone)
EventBus.Subscribe("NativeBuildingSpawned", handler);
```

### 2. EventHelpers
**Utilities for common event operations**:
- Type-safe event data extraction (`TryGetEventData<T>`)
- Climate change detection (`IsSignificantChange`, `GetClimateDelta`)
- Martian time calculations (`GetMartianYear`, `IsNewMartianYear`)
- Formatted logging (`LogClimateEvent`, `LogDayEvent`)
- Filter factories (`ClimateThresholdFilter`, `MilestoneFilter`)

---

## 🚀 Quick Examples

### Monitor Temperature Changes

```csharp
using PerAspera.GameAPI.Events;

EventBus.Subscribe(GameEventConstants.TemperatureChanged, (data) =>
{
    if (EventHelpers.TryGetEventData<ClimateEventData>(data, out var climate))
    {
        // Only react to significant changes
        if (EventHelpers.IsSignificantChange(climate, threshold: 0.5f))
        {
            float delta = EventHelpers.GetClimateDelta(climate);
            Logger.LogInfo($"🌡️ Temp: {climate.CurrentValue:F2}K (Δ{delta:+0.0;-0.0})");
        }
    }
});
```

### Track Martian Days

```csharp
EventBus.Subscribe(GameEventConstants.MartianDayChanged, (data) =>
{
    if (EventHelpers.TryGetEventData<MartianDayEventData>(data, out var dayEvent))
    {
        int year = EventHelpers.GetMartianYear(dayEvent.CurrentSol);
        
        // New year celebration
        if (EventHelpers.IsNewMartianYear(dayEvent.CurrentSol))
        {
            Logger.LogInfo($"🎉 New Martian Year {year}!");
        }
        
        // Milestones every 100 sols
        if (EventHelpers.IsMilestone(dayEvent.CurrentSol, 100))
        {
            Logger.LogInfo($"⭐ Milestone: Sol {dayEvent.CurrentSol}");
        }
    }
});
```

### Filter Events

```csharp
// Only react to significant climate changes
var climateFilter = EventHelpers.ClimateThresholdFilter(threshold: 1.0f);

EventBus.Subscribe(GameEventConstants.TemperatureChanged, (data) =>
{
    if (climateFilter(data))
    {
        EventHelpers.LogClimateEvent(
            EventHelpers.AsEventData<ClimateEventData>(data), 
            "🌡️ SIGNIFICANT"
        );
    }
});
```

---

## 📚 Available Event Constants

### Buildings
- `BuildingSpawned` - Building created
- `BuildingDespawned` - Building destroyed
- `BuildingUpgraded` - Building upgraded
- `BuildingScrapped` - Building scrapped
- `BuildingStateChanged` - State changed

### Climate
- `ClimateChanged` - Any climate change
- `TemperatureChanged` - Temperature changed
- `CO2PressureChanged` - CO2 pressure changed
- `O2PressureChanged` - O2 pressure changed
- `N2PressureChanged` - N2 pressure changed
- `WaterStockChanged` - Water stock changed
- `TotalPressureChanged` - Total pressure changed
- `GHGPressureChanged` - GHG pressure changed
- `ArgonPressureChanged` - Argon pressure changed

### Time
- `MartianDayChanged` - Sol incremented
- `DayProgressed` - Time advanced

### Resources
- `ResourceAdded` - Resource added
- `ResourceConsumed` - Resource consumed
- `ResourceChanged` - Any resource change

### Game State
- `GameSpeedChanged` - Game speed changed
- `GamePauseChanged` - Game paused/unpaused
- `GameStateChanged` - Game state changed

### Other
- `FactionCreated` - Faction created
- `TechnologyResearched` - Tech researched
- `POIDiscovered` - POI discovered
- `DroneSpawned` - Drone spawned
- `DroneDespawned` - Drone destroyed

---

## 🔧 EventHelpers Reference

### Type Checking
```csharp
// Safe type checking with out parameter
bool TryGetEventData<T>(object eventData, out T? result)

// Direct casting (may return null)
T? AsEventData<T>(object eventData)
```

### Climate Helpers
```csharp
bool IsSignificantChange(ClimateEventData climate, float threshold = 0.1f)
float GetClimateDelta(ClimateEventData climate)
bool IsClimateIncrease(ClimateEventData climate)
bool IsClimateDecrease(ClimateEventData climate)
void LogClimateEvent(ClimateEventData climate, string prefix = "")
```

### Time Helpers
```csharp
int GetMartianYear(int sol)
int GetDayInYear(int sol)
bool IsNewMartianYear(int sol)
bool IsMilestone(int sol, int interval = 100)
void LogDayEvent(MartianDayEventData dayEvent, string prefix = "")
```

### Filters
```csharp
Func<object, bool> ClimateThresholdFilter(float threshold)
Func<object, bool> MilestoneFilter(int interval = 100)
```

---

## ⚠️ Best Practices

**✅ DO:** Use constants for event names  
**❌ DON'T:** Use magic strings

**✅ DO:** Check types with `TryGetEventData`  
**❌ DON'T:** Cast directly without checking

**✅ DO:** Use filters for performance  
**❌ DON'T:** Process every event unnecessarily

---

**Version**: 2.0.0  
**Game Compatibility**: Per Aspera 1.5.x  
**Status**: ✅ Production Ready
