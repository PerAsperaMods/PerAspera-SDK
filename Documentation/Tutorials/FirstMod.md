# Your First Per Aspera Mod

Complete step-by-step tutorial to create your first functional Per Aspera mod using the SDK.

## 🎯 What We'll Build

We'll create a **Mars Climate Monitor** mod that:
- 📊 Tracks Mars temperature changes in real-time
- 🌡️ Logs significant climate events  
- 📈 Displays atmospheric progress toward habitability
- 🎯 Shows terraforming milestones

**Expected Result**: A mod that enhances your Per Aspera experience with detailed climate insights.

## 🔧 Prerequisites

Before starting, ensure you have:
- ✅ [SDK installed and configured](../Installation.md)
- ✅ Per Aspera game with BepInEx working
- ✅ Visual Studio 2022 or VS Code
- ✅ Basic C# knowledge (beginner-friendly)

## 🚀 Step 1: Project Setup

### Create New Mod Project

```bash
# Create project directory
mkdir MarsClimateMonitor
cd MarsClimateMonitor

# Initialize .NET project
dotnet new console
```

### Configure Project File

Edit `MarsClimateMonitor.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <AssemblyTitle>Mars Climate Monitor</AssemblyTitle>
    <AssemblyDescription>Real-time Mars atmospheric monitoring and terraforming progress tracker</AssemblyDescription>
    <AssemblyCompany>Your Modding Name</AssemblyCompany>
    <AssemblyProduct>Per Aspera Climate Mods</AssemblyProduct>
    <Version>1.0.0</Version>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="PerAspera.ModSDK" Version="1.1.0" />
  </ItemGroup>

</Project>
```

### Verify Setup

```bash
# Restore packages
dotnet restore

# Should complete without errors
# If issues occur, check Installation Guide
```

## 📝 Step 2: Create the Plugin Foundation

Delete `Program.cs` and create `ClimateMonitorPlugin.cs`:

```csharp
using BepInEx;
using BepInEx.Unity.IL2CPP;
using PerAspera.ModSDK;
using System;

namespace MarsClimateMonitor
{
    /// <summary>
    /// Main plugin class for Mars Climate Monitor
    /// Tracks atmospheric changes and terraforming progress on Mars
    /// </summary>
    [BepInPlugin("com.yourname.marsclimate", "Mars Climate Monitor", "1.0.0")]
    public class ClimateMonitorPlugin : BasePlugin
    {
        // Plugin lifecycle: Load() is called when mod initializes
        public override void Load()
        {
            // Always log when your mod starts
            Log.LogInfo("🚀 Mars Climate Monitor v1.0.0 starting...");

            try
            {
                // Initialize the PerAspera SDK
                ModSDK.Initialize(this);
                Log.LogInfo("✅ ModSDK initialized successfully");

                // Setup our climate monitoring system
                SetupClimateMonitoring();

                Log.LogInfo("🌡️ Mars Climate Monitor ready! Monitoring atmospheric changes...");
            }
            catch (Exception ex)
            {
                Log.LogError($"❌ Failed to initialize Mars Climate Monitor: {ex.Message}");
                Log.LogError($"Stack trace: {ex.StackTrace}");
            }
        }

        private void SetupClimateMonitoring()
        {
            // We'll implement this next
            Log.LogInfo("📊 Setting up climate monitoring systems...");
        }
    }
}
```

### Test Basic Setup

```bash
# Build the project
dotnet build --configuration Debug

# Should complete successfully
# Check output: bin/Debug/net6.0/MarsClimateMonitor.dll exists
```

## 🌡️ Step 3: Add Climate Monitoring

Now let's add real climate monitoring functionality:

```csharp
using BepInEx;
using BepInEx.Unity.IL2CPP;
using PerAspera.ModSDK;
using PerAspera.GameAPI;
using PerAspera.GameAPI.Climate.Events;
using System;

namespace MarsClimateMonitor
{
    [BepInPlugin("com.yourname.marsclimate", "Mars Climate Monitor", "1.0.0")]
    public class ClimateMonitorPlugin : BasePlugin
    {
        // Track previous values for change detection
        private float _lastTemperature = float.MinValue;
        private float _lastPressure = float.MinValue;
        private bool _isFirstUpdate = true;

        public override void Load()
        {
            Log.LogInfo("🚀 Mars Climate Monitor v1.0.0 starting...");

            try
            {
                ModSDK.Initialize(this);
                Log.LogInfo("✅ ModSDK initialized successfully");

                SetupClimateMonitoring();
                SetupGameEventHandlers();

                Log.LogInfo("🌡️ Mars Climate Monitor ready! Monitoring atmospheric changes...");
            }
            catch (Exception ex)
            {
                Log.LogError($"❌ Failed to initialize Mars Climate Monitor: {ex.Message}");
            }
        }

        private void SetupClimateMonitoring()
        {
            Log.LogInfo("📊 Setting up climate monitoring systems...");

            // Check if game is ready for monitoring
            if (ModSDK.Universe.IsGameReady())
            {
                StartMonitoring();
            }
            else
            {
                Log.LogInfo("⏳ Game not ready yet, will start monitoring when available...");
            }
        }

        private void SetupGameEventHandlers()
        {
            // Subscribe to game state changes
            ModSDK.Events.Subscribe("game.loaded", OnGameLoaded);
            ModSDK.Events.Subscribe("planet.initialized", OnPlanetInitialized);
            
            // Subscribe to climate events  
            ModSDK.Events.Subscribe("climate.temperatureChanged", OnTemperatureChanged);
            ModSDK.Events.Subscribe("climate.atmosphereChanged", OnAtmosphereChanged);

            Log.LogInfo("📡 Event handlers registered");
        }

        private void OnGameLoaded(object eventData)
        {
            Log.LogInfo("🎮 Game loaded! Starting climate monitoring...");
            StartMonitoring();
        }

        private void OnPlanetInitialized(object eventData)
        {
            Log.LogInfo("🪐 Planet initialized! Climate systems active...");
            LogCurrentClimateStatus();
        }

        private void OnTemperatureChanged(object eventData)
        {
            // Get current climate data
            var climateData = ModSDK.Universe.GetClimateData();
            if (climateData != null)
            {
                float currentTemp = climateData.Temperature;
                
                if (_lastTemperature != float.MinValue)
                {
                    float tempChange = currentTemp - _lastTemperature;
                    
                    if (Math.Abs(tempChange) > 0.1f) // Only log significant changes
                    {
                        string trend = tempChange > 0 ? "🔥 warming" : "🧊 cooling";
                        Log.LogInfo($"🌡️ Temperature {trend}: {_lastTemperature:F1}K → {currentTemp:F1}K " +
                                  $"(Δ{tempChange:+0.0;-0.0}K)");
                        
                        CheckTemperatureMilestones(currentTemp);
                    }
                }
                
                _lastTemperature = currentTemp;
            }
        }

        private void OnAtmosphereChanged(object eventData)
        {
            var climateData = ModSDK.Universe.GetClimateData();
            if (climateData != null)
            {
                float currentPressure = climateData.TotalPressure;
                
                if (_lastPressure != float.MinValue)
                {
                    float pressureChange = currentPressure - _lastPressure;
                    
                    if (Math.Abs(pressureChange) > 0.001f) // Pressure changes are smaller
                    {
                        string trend = pressureChange > 0 ? "📈 increasing" : "📉 decreasing";
                        Log.LogInfo($"🌫️ Atmospheric pressure {trend}: {_lastPressure:F3} → {currentPressure:F3} atm " +
                                  $"(Δ{pressureChange:+0.000;-0.000})");
                        
                        CheckPressureMilestones(currentPressure);
                    }
                }
                
                _lastPressure = currentPressure;
            }
        }

        private void StartMonitoring()
        {
            Log.LogInfo("🔬 Starting atmospheric monitoring...");
            LogCurrentClimateStatus();
        }

        private void LogCurrentClimateStatus()
        {
            try
            {
                var climateData = ModSDK.Universe.GetClimateData();
                if (climateData == null)
                {
                    Log.LogWarning("⚠️ Climate data not available");
                    return;
                }

                // Get current Sol (Martian day)
                int currentSol = ModSDK.Universe.GetCurrentSol();

                Log.LogInfo("📊 === MARS CLIMATE REPORT ===");
                Log.LogInfo($"📅 Sol {currentSol}");
                Log.LogInfo($"🌡️ Temperature: {climateData.Temperature:F1} K ({KelvinToCelsius(climateData.Temperature):F1}°C)");
                Log.LogInfo($"🌫️ Atmospheric Pressure: {climateData.TotalPressure:F3} atm");
                Log.LogInfo($"💨 CO2: {climateData.CO2Pressure:F3} atm");
                Log.LogInfo($"🫁 O2: {climateData.O2Pressure:F3} atm");
                Log.LogInfo($"💧 Water Stock: {climateData.WaterStock:F1}");
                
                // Calculate habitability progress
                float habitability = CalculateHabitability(climateData);
                Log.LogInfo($"🏠 Habitability: {habitability:F1}% {GetHabitabilityEmoji(habitability)}");
                
                Log.LogInfo("================================");

                // Store initial values
                _lastTemperature = climateData.Temperature;
                _lastPressure = climateData.TotalPressure;
            }
            catch (Exception ex)
            {
                Log.LogError($"❌ Error reading climate data: {ex.Message}");
            }
        }

        // Helper methods for better user experience
        private float KelvinToCelsius(float kelvin)
        {
            return kelvin - 273.15f;
        }

        private float CalculateHabitability(PlanetClimateData climateData)
        {
            // Simple habitability calculation (you can make this more sophisticated)
            float tempScore = 0f;
            float pressureScore = 0f;
            float oxygenScore = 0f;

            // Temperature score (optimal around 273-300K)
            float celsius = KelvinToCelsius(climateData.Temperature);
            if (celsius >= -10f && celsius <= 30f)
                tempScore = 30f;
            else if (celsius >= -50f && celsius <= 50f)
                tempScore = 15f;

            // Pressure score (need at least 0.1 atm)
            if (climateData.TotalPressure >= 0.5f)
                pressureScore = 35f;
            else if (climateData.TotalPressure >= 0.1f)
                pressureScore = 20f;

            // Oxygen score (need significant O2)
            if (climateData.O2Pressure >= 0.15f)
                oxygenScore = 35f;
            else if (climateData.O2Pressure >= 0.05f)
                oxygenScore = 20f;

            return tempScore + pressureScore + oxygenScore;
        }

        private string GetHabitabilityEmoji(float habitability)
        {
            if (habitability >= 80f) return "🌍 Highly habitable!";
            if (habitability >= 60f) return "🟢 Good progress";
            if (habitability >= 40f) return "🟡 Making progress";
            if (habitability >= 20f) return "🟠 Early stages";
            return "🔴 Hostile environment";
        }

        private void CheckTemperatureMilestones(float temperature)
        {
            float celsius = KelvinToCelsius(temperature);
            
            if (celsius >= 0f && _lastTemperature < 273.15f)
                Log.LogInfo("🎉 MILESTONE: Water can exist as liquid! (0°C reached)");
            else if (celsius >= 15f && KelvinToCelsius(_lastTemperature) < 15f)
                Log.LogInfo("🎉 MILESTONE: Comfortable human temperature range! (15°C reached)");
        }

        private void CheckPressureMilestones(float pressure)
        {
            if (pressure >= 0.1f && _lastPressure < 0.1f)
                Log.LogInfo("🎉 MILESTONE: Minimum pressure for liquid water! (0.1 atm reached)");
            else if (pressure >= 0.5f && _lastPressure < 0.5f)
                Log.LogInfo("🎉 MILESTONE: Substantial atmosphere developing! (0.5 atm reached)");
        }
    }
}
```

## 🧪 Step 4: Test Your Mod

### Build and Deploy

```bash
# Build the mod
dotnet build --configuration Debug

# The DLL should be created
ls bin/Debug/net6.0/MarsClimateMonitor.dll
```

### Test in Game

1. **Copy DLL** to `Per Aspera/BepInEx/plugins/`
2. **Launch Per Aspera**
3. **Watch the console** for your mod's output

**Expected Console Output:**
```
[Info   : Mars Climate Monitor] 🚀 Mars Climate Monitor v1.0.0 starting...
[Info   : Mars Climate Monitor] ✅ ModSDK initialized successfully  
[Info   : Mars Climate Monitor] 📊 Setting up climate monitoring systems...
[Info   : Mars Climate Monitor] 📡 Event handlers registered
[Info   : Mars Climate Monitor] 🌡️ Mars Climate Monitor ready! Monitoring atmospheric changes...
[Info   : Mars Climate Monitor] 🎮 Game loaded! Starting climate monitoring...
[Info   : Mars Climate Monitor] 📊 === MARS CLIMATE REPORT ===
[Info   : Mars Climate Monitor] 📅 Sol 1
[Info   : Mars Climate Monitor] 🌡️ Temperature: 210.5 K (-62.7°C)
[Info   : Mars Climate Monitor] 🌫️ Atmospheric Pressure: 0.008 atm  
[Info   : Mars Climate Monitor] 💨 CO2: 0.007 atm
[Info   : Mars Climate Monitor] 🫁 O2: 0.000 atm
[Info   : Mars Climate Monitor] 💧 Water Stock: 0.0
[Info   : Mars Climate Monitor] 🏠 Habitability: 0.0% 🔴 Hostile environment
```

## 🎨 Step 5: Enhanced Features (Optional)

### Add Periodic Status Reports

```csharp
public class ClimateMonitorPlugin : BasePlugin
{
    private System.Timers.Timer _reportTimer;

    private void SetupClimateMonitoring()
    {
        // ... existing code ...

        // Setup periodic reports every 5 minutes of real time
        _reportTimer = new System.Timers.Timer(5 * 60 * 1000); // 5 minutes
        _reportTimer.Elapsed += (sender, e) => LogCurrentClimateStatus();
        _reportTimer.AutoReset = true;
        _reportTimer.Start();

        Log.LogInfo("⏰ Periodic climate reports enabled (every 5 minutes)");
    }

    // Don't forget to cleanup!
    public void OnDestroy()
    {
        _reportTimer?.Stop();
        _reportTimer?.Dispose();
    }
}
```

### Add Terraforming Goal Tracking

```csharp
private void CheckTerraformingGoals(PlanetClimateData climateData)
{
    bool tempGood = KelvinToCelsius(climateData.Temperature) > 0f;
    bool pressureGood = climateData.TotalPressure > 0.1f;
    bool oxygenGood = climateData.O2Pressure > 0.15f;

    int goalsComplete = (tempGood ? 1 : 0) + (pressureGood ? 1 : 0) + (oxygenGood ? 1 : 0);

    Log.LogInfo($"🎯 Terraforming Progress: {goalsComplete}/3 goals complete");
    Log.LogInfo($"   Temperature: {(tempGood ? "✅" : "❌")} {(tempGood ? "Good" : "Too cold")}");
    Log.LogInfo($"   Pressure: {(pressureGood ? "✅" : "❌")} {(pressureGood ? "Sufficient" : "Too thin")}");
    Log.LogInfo($"   Oxygen: {(oxygenGood ? "✅" : "❌")} {(oxygenGood ? "Breathable" : "Insufficient")}");

    if (goalsComplete == 3)
    {
        Log.LogInfo("🎉🎉🎉 MARS IS NOW HABITABLE! CONGRATULATIONS! 🎉🎉🎉");
    }
}
```

## 🐛 Troubleshooting

### Common Issues

#### Mod doesn't load
**Symptoms**: No log messages appear in console
**Solutions**:
1. Check BepInEx is properly installed
2. Verify DLL is in correct location
3. Check for compilation errors

#### "ModSDK not found" error
**Symptoms**: Exception on `ModSDK.Initialize()`
**Solutions**:
1. Verify NuGet package is installed: `dotnet list package`
2. Clean and rebuild: `dotnet clean && dotnet build`
3. Check .NET 6.0 is installed

#### "Climate data not available" warning
**Symptoms**: Mod loads but no climate data shown
**Solutions**:
1. Load an existing game save
2. Start a new game and let it initialize fully
3. Check game version compatibility

### Debug Tips

```csharp
// Add debug logging
Log.LogDebug($"Debug: Game ready = {ModSDK.Universe.IsGameReady()}");
Log.LogDebug($"Debug: Climate data available = {ModSDK.Universe.GetClimateData() != null}");

// Catch and log all exceptions
try 
{
    // Your code
}
catch (Exception ex)
{
    Log.LogError($"Detailed error: {ex}");
}
```

## 🚀 Next Steps

Congratulations! You've created your first functional Per Aspera mod. Here are some ideas to expand it:

### Beginner Enhancements
- Add support for multiple planets
- Track building construction events  
- Create alerts for critical climate changes
- Add configuration file for customization

### Intermediate Features
- Create a GUI overlay for real-time display
- Add charts and graphs for climate trends
- Implement climate prediction algorithms
- Add integration with other mods

### Advanced Projects
- Develop automated terraforming recommendations
- Create climate control systems
- Build multiplayer synchronization
- Implement machine learning for optimization

## 📚 Continue Learning

Ready to dive deeper? Check out these resources:

- **[Climate System Guide](../Guides/ClimateSystem.md)** - Advanced atmospheric simulation
- **[Event System Guide](../Guides/EventSystem.md)** - Comprehensive event handling
- **[API Reference](../API-Reference.md)** - Complete SDK documentation
- **[Code Examples](../Examples/)** - More sample projects

## 💡 Community

Share your mod and get help:

- **[Discord Community](https://discord.gg/peraspera-modding)** - Real-time help
- **[GitHub Discussions](https://github.com/PerAsperaMods/PerAspera-SDK/discussions)** - Share projects
- **[Steam Workshop](https://steamcommunity.com/workshop/browse/?appid=944290)** - Publish your mod

**Happy modding!** 🚀 You've taken the first step toward becoming a Mars terraforming engineer through code!