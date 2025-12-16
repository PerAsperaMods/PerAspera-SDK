# PerAspera.GameAPI.Climate

**Advanced climate simulation and terraforming analysis for Per Aspera modding**

## 🎯 Purpose

Provides high-level climate modeling, simulation, and forecasting capabilities on top of the native Planet data. While `GameAPI.Helpers.ClimateHelper` gives direct access to Planet values, this module adds:

- 🌡️ **Climate Simulation** - Physics-based temperature/pressure modeling
- 📊 **Habitability Analysis** - Multi-factor terraforming progress tracking
- 🔮 **Forecasting** - Predict climate changes over time
- 🏗️ **Building Impact** - Calculate atmospheric effects of structures

## 📦 Architecture

```
PerAspera.GameAPI.Climate/
├── Simulation/              → Physics engines
│   ├── ClimateSimulator.cs
│   └── Models/
│       ├── GreenhouseModel.cs
│       ├── TemperatureModel.cs
│       └── PressureModel.cs
├── Analysis/                → Metrics & scoring
│   ├── HabitabilityAnalyzer.cs
│   ├── TerraformingAnalyzer.cs
│   └── BuildingImpactAnalyzer.cs
├── Forecasting/             → Predictions
│   ├── ClimateForecast.cs
│   └── TerraformingProjection.cs
└── Configuration/           → Parameters
    ├── ClimateConfig.cs
    └── TerraformingConstants.cs
```

## 🔗 Dependencies

```
PerAspera.Core                 → Logging, utilities
PerAspera.GameAPI              → Native access (ClimateHelper, Models)
PerAspera.GameAPI.Events       → Climate events subscription
PerAspera.GameAPI.Overrides    → Override integration (optional)
```

## 🚀 Quick Start

### Basic Climate Monitoring

```csharp
using PerAspera.GameAPI.Helpers;
using PerAspera.GameAPI.Climate.Analysis;

// Get current climate
var snapshot = ClimateHelper.GetSnapshot();
Log.Info($"Current: {snapshot.Temperature}K, {snapshot.TotalPressure}atm");

// Analyze habitability
var analyzer = new HabitabilityAnalyzer();
var habitability = analyzer.Calculate(snapshot);
Log.Info($"Habitability: {habitability}%");
```

### Advanced Simulation

```csharp
using PerAspera.GameAPI.Climate.Simulation;
using PerAspera.GameAPI.Climate.Configuration;

// Configure simulation
var config = new ClimateConfig
{
    GreenhouseEfficiency = 1.2f,
    SolarConstant = 590f,
    AlbedoFactor = 0.25f
};

var simulator = new ClimateSimulator(config);

// Simulate 50 sols ahead
var future = simulator.SimulateSols(50);
Log.Info($"Predicted temp in 50 sols: {future.Temperature}K");
```

### Forecasting

```csharp
using PerAspera.GameAPI.Climate.Forecasting;

var forecast = new ClimateForecast();

// Project temperature change
var projection = forecast.ProjectTemperature(
    currentSnapshot,
    co2Delta: 10f,      // +10 kPa CO2
    ghgDelta: 0.5f,     // +0.5 kPa GHG
    days: 100
);

Log.Info($"Expected warming: +{projection.TemperatureDelta}K");
```

## 📊 Example Use Cases

### 1. Terraforming Progress Dashboard

```csharp
using PerAspera.GameAPI.Climate.Analysis;

var analyzer = new TerraformingAnalyzer();
var status = analyzer.AnalyzeProgress();

Console.WriteLine(status.ToString());
// Output: "Terraforming: 45.2% habitable | Temp: Warming | Pressure: Building | O2: In Progress"

if (status.Habitability < 50f)
{
    Log.Warning($"Recommended: {status.RecommendedAction}");
}
```

### 2. Building Optimizer

```csharp
using PerAspera.GameAPI.Climate.Analysis;

var impact = new BuildingImpactAnalyzer();

// What building should I build next?
var recommendation = impact.GetOptimalBuilding("increase_temperature");
Log.Info($"Build: {recommendation.BuildingType} for +{recommendation.TemperatureImpact}K");
```

### 3. Long-Term Planning

```csharp
using PerAspera.GameAPI.Climate.Forecasting;

var projection = new TerraformingProjection();

// How long until habitable?
var timeline = projection.EstimateTimeToHabitable();
Log.Info($"Estimated {timeline.Sols} sols until breathable atmosphere");
Log.Info($"Milestones: {timeline.Milestones.Count} critical events");
```

## 🔧 Integration with Other Modules

### With Events

```csharp
using PerAspera.GameAPI.Events.Native;
using PerAspera.GameAPI.Climate.Analysis;

// React to climate changes
ModEventBus.Subscribe(ClimateEvents.TemperatureChanged, (data) =>
{
    var analyzer = new HabitabilityAnalyzer();
    var newHabitability = analyzer.Calculate();
    
    if (newHabitability > 80f)
    {
        Log.Info("🎉 Planet is now habitable!");
    }
});
```

### With Overrides

```csharp
using PerAspera.GameAPI.Overrides.Registry;
using PerAspera.GameAPI.Climate.Configuration;

// Override greenhouse effect for faster terraforming
var ghgOverride = new GetterOverride<float>(
    "Planet", "ghgPressure", "Boosted GHG",
    defaultValue: 1.5f
);
GetterOverrideRegistry.RegisterOverride(ghgOverride);

// Recalculate with new parameters
var simulator = new ClimateSimulator();
var result = simulator.Simulate();
```

## 📚 API Reference

### ClimateSimulator

Main simulation engine for physics-based climate modeling.

```csharp
public class ClimateSimulator
{
    public ClimateSimulator(ClimateConfig? config = null);
    
    // Simulate N sols into the future
    public ClimateSnapshot SimulateSols(int sols);
    
    // Simulate until condition met
    public ClimateSnapshot SimulateUntil(Func<ClimateSnapshot, bool> condition);
    
    // Calculate greenhouse warming
    public float CalculateGreenhouseEffect(ClimateSnapshot current);
}
```

### HabitabilityAnalyzer

Calculates multi-factor habitability scores.

```csharp
public class HabitabilityAnalyzer
{
    // Calculate overall habitability (0-100%)
    public float Calculate(ClimateSnapshot? snapshot = null);
    
    // Get detailed breakdown
    public HabitabilityBreakdown GetBreakdown(ClimateSnapshot snapshot);
    
    // Check specific requirement
    public bool IsBreathable(ClimateSnapshot snapshot);
}
```

### TerraformingAnalyzer

Track terraforming progress and phases.

```csharp
public class TerraformingAnalyzer
{
    // Get current status with phases
    public TerraformingStatus AnalyzeProgress();
    
    // Get next milestone
    public TerraformingMilestone GetNextMilestone();
    
    // Calculate completion percentage
    public float GetCompletionPercentage();
}
```

## 🎓 Advanced Topics

### Custom Climate Models

Extend the simulation with your own physics:

```csharp
public class CustomGreenhouseModel : IGreenhouseModel
{
    public float CalculateWarming(ClimateSnapshot snapshot)
    {
        // Your custom greenhouse calculation
        return snapshot.CO2Pressure * 2.5f + snapshot.GHGPressure * 100f;
    }
}

var simulator = new ClimateSimulator();
simulator.SetGreenhouseModel(new CustomGreenhouseModel());
```

### Performance Considerations

- `ClimateHelper.GetSnapshot()` - Fast, direct native access
- `ClimateSimulator.SimulateSols()` - Moderate, iterative calculation
- `TerraformingProjection.EstimateTimeToHabitable()` - Slow, can simulate 1000+ sols

Cache results when possible:

```csharp
// Cache snapshot for multiple analyses
var snapshot = ClimateHelper.GetSnapshot();

var habitability = new HabitabilityAnalyzer().Calculate(snapshot);
var status = new TerraformingAnalyzer().AnalyzeProgress(snapshot);
```

## 📖 Further Reading

- [GameAPI.Helpers.ClimateHelper](../PerAspera.GameAPI/Helpers/ClimateHelper.cs) - Direct Planet access
- [ClimateEventData](../PerAspera.GameAPI.Events/Data/ClimateEventData.cs) - Event payloads
- [Climate Guide](../../Organization-Wiki/Climate-System-Guide.md) - Game mechanics
- [REFACTORING-V2-ARCHITECTURE](../REFACTORING-V2-ARCHITECTURE.md) - SDK design

## 🤝 Contributing

This module is part of the SDK v2.0 restructuring. Follow these patterns:

1. **Simulation logic** goes in `Simulation/`
2. **Metrics/scoring** goes in `Analysis/`
3. **Predictions** go in `Forecasting/`
4. **Constants** go in `Configuration/`
5. **Direct access** stays in `GameAPI.Helpers.ClimateHelper`

---

**Status**: 🚧 In Development (SDK v2.0.0)
