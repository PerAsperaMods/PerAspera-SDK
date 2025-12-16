# Guide Moddeur - Système d'Événements Per Aspera

**Version SDK**: 2.0.0  
**Date**: 2024-12-16

---

## 🎯 Vue d'Ensemble

Le système d'événements Per Aspera est organisé en **3 couches** :

```
┌─────────────────────────────────────────────────────┐
│  PerAspera.ModSDK (API haut niveau)                 │
│  ├─ ModSDK.Initialize() - Point d'entrée            │
│  └─ EventSystem.Subscribe/Publish - Interface simple│
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│  PerAspera.GameAPI.Events (Constants & Helpers)     │
│  ├─ GameEventConstants.* - Noms type-safe           │
│  └─ EventHelpers.* - Utilitaires                    │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│  PerAspera.GameAPI (Event Data & Native System)     │
│  ├─ ClimateEventData, MartianDayEventData, etc.     │
│  ├─ NativeEventPatcher - Patches Harmony            │
│  └─ ModEventBus - Publication événements natifs     │
└─────────────────────────────────────────────────────┘
```

---

## 📚 **1. S'abonner aux Événements Natifs**

### **Méthode Simple** (Recommandée pour débutants)

```csharp
using BepInEx;
using BepInEx.Unity.IL2CPP;
using PerAspera.ModSDK;
using PerAspera.GameAPI.Events;

[BepInPlugin("com.yourname.mymod", "My Mod", "1.0.0")]
public class MyModPlugin : BasePlugin
{
    public override void Load()
    {
        // Initialiser le SDK
        ModSDK.Initialize(this);
        
        // S'abonner avec des constantes type-safe
        EventSystem.Subscribe(GameEventConstants.TemperatureChanged, OnTemperatureChange);
        EventSystem.Subscribe(GameEventConstants.BuildingSpawned, OnBuildingSpawned);
        EventSystem.Subscribe(GameEventConstants.MartianDayChanged, OnDayChanged);
        
        Log.LogInfo("Mod loaded!");
    }
    
    private void OnTemperatureChange(object eventData)
    {
        // Extraction type-safe avec helper
        if (EventHelpers.TryGetEventData<ClimateEventData>(eventData, out var climate))
        {
            // Filtrer les changements insignifiants
            if (EventHelpers.IsSignificantChange(climate, threshold: 0.5f))
            {
                float delta = EventHelpers.GetClimateDelta(climate);
                Log.LogInfo($"🌡️ Temp: {climate.CurrentValue:F2}K (Δ{delta:+0.0;-0.0})");
            }
        }
    }
    
    private void OnBuildingSpawned(object eventData)
    {
        Log.LogInfo("🏗️ Building spawned!");
    }
    
    private void OnDayChanged(object eventData)
    {
        if (EventHelpers.TryGetEventData<MartianDayEventData>(eventData, out var day))
        {
            // Calculer l'année martienne
            int year = EventHelpers.GetMartianYear(day.CurrentSol);
            
            if (EventHelpers.IsNewMartianYear(day.CurrentSol))
            {
                Log.LogInfo($"🎉 New Martian Year {year}!");
            }
        }
    }
}
```

---

## 🔧 **2. Utiliser les Helpers et Filtres**

### **Filtres Prédéfinis**

```csharp
using PerAspera.GameAPI.Events;

// Filtrer par seuil climatique
var climateFilter = EventHelpers.ClimateThresholdFilter(threshold: 1.0f);

EventSystem.Subscribe(GameEventConstants.TemperatureChanged, (data) =>
{
    if (climateFilter(data))
    {
        // Ne traiter que les changements > 1.0K
        var climate = EventHelpers.AsEventData<ClimateEventData>(data);
        EventHelpers.LogClimateEvent(climate, "🌡️ SIGNIFICANT");
    }
});

// Filtrer les milestones
var milestoneFilter = EventHelpers.MilestoneFilter(interval: 100);

EventSystem.Subscribe(GameEventConstants.MartianDayChanged, (data) =>
{
    if (milestoneFilter(data))
    {
        Log.LogInfo("⭐ 100 sols milestone!");
    }
});
```

### **Helpers Climatiques**

```csharp
// Analyser les changements climatiques
if (EventHelpers.TryGetEventData<ClimateEventData>(data, out var climate))
{
    // Direction du changement
    bool increasing = EventHelpers.IsClimateIncrease(climate);
    bool decreasing = EventHelpers.IsClimateDecrease(climate);
    
    // Delta précis
    float delta = EventHelpers.GetClimateDelta(climate);
    
    // Logging formaté automatique
    EventHelpers.LogClimateEvent(climate, "CLIMATE");
}
```

### **Helpers Temporels**

```csharp
if (EventHelpers.TryGetEventData<MartianDayEventData>(data, out var dayEvent))
{
    // Conversions temporelles
    int year = EventHelpers.GetMartianYear(dayEvent.CurrentSol);
    int dayInYear = EventHelpers.GetDayInYear(dayEvent.CurrentSol);
    
    // Détection d'événements
    bool newYear = EventHelpers.IsNewMartianYear(dayEvent.CurrentSol);
    bool milestone = EventHelpers.IsMilestone(dayEvent.CurrentSol, 50);
    
    // Logging formaté
    EventHelpers.LogDayEvent(dayEvent, "SOL");
}
```

---

## 🎨 **3. Créer des Événements Custom**

### **Événement Simple**

```csharp
using PerAspera.GameAPI.Events;

// Définir votre data class
public class MyCustomEventData
{
    public string Message { get; set; }
    public int Value { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

// Publier l'événement
public void TriggerMyEvent()
{
    var eventData = new MyCustomEventData 
    { 
        Message = "Something happened",
        Value = 42
    };
    
    // Préfixer avec le nom de votre mod
    EventSystem.Publish("MyMod.CustomEvent", eventData);
}

// S'abonner ailleurs
EventSystem.Subscribe("MyMod.CustomEvent", (data) =>
{
    if (data is MyCustomEventData customData)
    {
        Log.LogInfo($"Received: {customData.Message} - {customData.Value}");
    }
});
```

### **Événement Complexe avec GameEventBase**

```csharp
using PerAspera.GameAPI.Events;

// Hériter de GameEventBase pour intégration complète
public class ProductionBonusEvent : GameEventBase
{
    public override string EventType => "MyMod.ProductionBonus";
    
    public string BuildingType { get; set; }
    public float BonusMultiplier { get; set; }
    public int DurationSols { get; set; }
    
    public ProductionBonusEvent(string buildingType, float bonus, int duration)
    {
        BuildingType = buildingType;
        BonusMultiplier = bonus;
        DurationSols = duration;
        Timestamp = DateTime.Now;
    }
}

// Publier
var bonus = new ProductionBonusEvent("SolarPanel", 1.5f, 10);
EventSystem.Publish(bonus.EventType, bonus);

// Consommer
EventSystem.Subscribe("MyMod.ProductionBonus", (data) =>
{
    if (EventHelpers.TryGetEventData<ProductionBonusEvent>(data, out var bonus))
    {
        ApplyBonus(bonus.BuildingType, bonus.BonusMultiplier, bonus.DurationSols);
    }
});
```

---

## 📋 **4. Événements Natifs Disponibles**

### **Buildings**
```csharp
GameEventConstants.BuildingSpawned
GameEventConstants.BuildingDespawned
GameEventConstants.BuildingUpgraded
GameEventConstants.BuildingScrapped
GameEventConstants.BuildingStateChanged
```

### **Climate**
```csharp
GameEventConstants.TemperatureChanged
GameEventConstants.CO2PressureChanged
GameEventConstants.O2PressureChanged
GameEventConstants.N2PressureChanged
GameEventConstants.WaterStockChanged
GameEventConstants.TotalPressureChanged
GameEventConstants.GHGPressureChanged
GameEventConstants.ArgonPressureChanged
```

### **Time**
```csharp
GameEventConstants.MartianDayChanged
GameEventConstants.DayProgressed
```

### **Resources**
```csharp
GameEventConstants.ResourceAdded
GameEventConstants.ResourceConsumed
GameEventConstants.ResourceChanged
```

### **Game State**
```csharp
GameEventConstants.GameSpeedChanged
GameEventConstants.GamePauseChanged
GameEventConstants.GameStateChanged
```

### **Other**
```csharp
GameEventConstants.FactionCreated
GameEventConstants.TechnologyResearched
GameEventConstants.POIDiscovered
GameEventConstants.DroneSpawned
```

**Total**: 30+ événements disponibles  
📚 **Liste complète**: [GameEventConstants.cs](../PerAspera.GameAPI.Events/GameEventConstants.cs)

---

## 🔥 **5. Exemples Pratiques**

### **Monitorer la Terraformation**

```csharp
using PerAspera.GameAPI.Events;

public class TerraformMonitor
{
    private float _initialTemp;
    private float _initialPressure;
    private int _startSol;
    
    public void Initialize()
    {
        EventSystem.Subscribe(GameEventConstants.TemperatureChanged, OnTempChange);
        EventSystem.Subscribe(GameEventConstants.TotalPressureChanged, OnPressureChange);
        
        // Track milestones
        EventSystem.Subscribe(GameEventConstants.MartianDayChanged, OnDayPassed);
    }
    
    private void OnTempChange(object data)
    {
        if (EventHelpers.TryGetEventData<ClimateEventData>(data, out var climate))
        {
            if (_initialTemp == 0) _initialTemp = climate.CurrentValue.Value;
            
            float progress = climate.CurrentValue.Value - _initialTemp;
            Log.LogInfo($"🌡️ Terraform Progress: +{progress:F1}K since start");
        }
    }
    
    private void OnPressureChange(object data)
    {
        if (EventHelpers.TryGetEventData<ClimateEventData>(data, out var climate))
        {
            if (_initialPressure == 0) _initialPressure = climate.CurrentValue.Value;
            
            float progress = (climate.CurrentValue.Value / _initialPressure - 1) * 100;
            Log.LogInfo($"🌫️ Atmosphere: {progress:+0.0;-0.0}% change");
        }
    }
    
    private void OnDayPassed(object data)
    {
        if (EventHelpers.TryGetEventData<MartianDayEventData>(data, out var day))
        {
            if (_startSol == 0) _startSol = day.CurrentSol;
            
            int elapsed = day.CurrentSol - _startSol;
            
            if (EventHelpers.IsMilestone(elapsed, 100))
            {
                Log.LogInfo($"📊 {elapsed} sols of terraforming completed!");
            }
        }
    }
}
```

### **Récompenser les Buildings Efficaces**

```csharp
using PerAspera.GameAPI.Events;

public class EfficiencyRewards
{
    private Dictionary<string, int> _buildingCounts = new();
    
    public void Initialize()
    {
        EventSystem.Subscribe(GameEventConstants.BuildingSpawned, OnBuildingSpawned);
        EventSystem.Subscribe(GameEventConstants.MartianDayChanged, CheckMilestones);
    }
    
    private void OnBuildingSpawned(object data)
    {
        // Track buildings (vous auriez besoin des event data propres aux buildings)
        _buildingCounts["Total"] = _buildingCounts.GetValueOrDefault("Total", 0) + 1;
        
        if (_buildingCounts["Total"] % 10 == 0)
        {
            Log.LogInfo($"🏗️ Milestone: {_buildingCounts["Total"]} buildings constructed!");
        }
    }
    
    private void CheckMilestones(object data)
    {
        if (EventHelpers.TryGetEventData<MartianDayEventData>(data, out var day))
        {
            // Publish custom reward event
            if (EventHelpers.IsMilestone(day.CurrentSol, 50))
            {
                var reward = new EfficiencyRewardEvent
                {
                    Buildings = _buildingCounts["Total"],
                    Sol = day.CurrentSol,
                    BonusMultiplier = 1.25f
                };
                
                EventSystem.Publish("MyMod.EfficiencyReward", reward);
            }
        }
    }
}

public class EfficiencyRewardEvent
{
    public int Buildings { get; set; }
    public int Sol { get; set; }
    public float BonusMultiplier { get; set; }
}
```

### **Système d'Alertes Climatiques**

```csharp
using PerAspera.GameAPI.Events;

public class ClimateAlerts
{
    private const float FREEZE_THRESHOLD = 273.15f; // 0°C
    private const float SAFE_PRESSURE = 0.61f; // kPa minimum
    
    public void Initialize()
    {
        EventSystem.Subscribe(GameEventConstants.TemperatureChanged, CheckTemperature);
        EventSystem.Subscribe(GameEventConstants.TotalPressureChanged, CheckPressure);
    }
    
    private void CheckTemperature(object data)
    {
        if (EventHelpers.TryGetEventData<ClimateEventData>(data, out var climate))
        {
            float temp = climate.CurrentValue.Value;
            
            if (temp < FREEZE_THRESHOLD && EventHelpers.IsClimateDecrease(climate))
            {
                PublishAlert("❄️ WARNING: Temperature below freezing and decreasing!");
            }
            else if (temp >= FREEZE_THRESHOLD && climate.PreviousValue < FREEZE_THRESHOLD)
            {
                PublishAlert("☀️ SUCCESS: Temperature above freezing!");
            }
        }
    }
    
    private void CheckPressure(object data)
    {
        if (EventHelpers.TryGetEventData<ClimateEventData>(data, out var climate))
        {
            float pressure = climate.CurrentValue.Value;
            
            if (pressure >= SAFE_PRESSURE && climate.PreviousValue < SAFE_PRESSURE)
            {
                PublishAlert("🌫️ MILESTONE: Breathable pressure achieved!");
            }
        }
    }
    
    private void PublishAlert(string message)
    {
        Log.LogWarning(message);
        
        var alert = new ClimateAlertEvent { Message = message };
        EventSystem.Publish("MyMod.ClimateAlert", alert);
    }
}

public class ClimateAlertEvent
{
    public string Message { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
```

---

## ⚠️ **Best Practices**

### ✅ **À FAIRE**

1. **Utiliser les constantes type-safe**
   ```csharp
   // ✅ CORRECT
   EventSystem.Subscribe(GameEventConstants.BuildingSpawned, handler);
   ```

2. **Vérifier les types avec helpers**
   ```csharp
   // ✅ CORRECT
   if (EventHelpers.TryGetEventData<ClimateEventData>(data, out var climate))
   {
       // Safe to use climate
   }
   ```

3. **Préfixer vos événements custom**
   ```csharp
   // ✅ CORRECT
   EventSystem.Publish("MyMod.CustomEvent", data);
   ```

4. **Utiliser les filtres pour performance**
   ```csharp
   // ✅ CORRECT - Filtrer avant traitement
   if (EventHelpers.IsSignificantChange(climate, 1.0f))
   {
       ProcessChange(climate);
   }
   ```

5. **Gérer les erreurs**
   ```csharp
   // ✅ CORRECT
   try
   {
       EventSystem.Subscribe(eventName, handler);
   }
   catch (ModSDKException ex)
   {
       Log.LogError($"Failed to subscribe: {ex.Message}");
   }
   ```

### ❌ **À ÉVITER**

1. **Magic strings**
   ```csharp
   // ❌ WRONG - Typo-prone, no autocomplete
   EventSystem.Subscribe("NativeBuildingSpawned", handler);
   ```

2. **Cast direct sans vérification**
   ```csharp
   // ❌ WRONG - Peut crasher
   var climate = (ClimateEventData)data;
   ```

3. **Événements sans préfixe**
   ```csharp
   // ❌ WRONG - Risque de collision
   EventSystem.Publish("CustomEvent", data);
   ```

4. **Traiter tous les événements**
   ```csharp
   // ❌ WRONG - Performance
   EventSystem.Subscribe(GameEventConstants.TemperatureChanged, (data) =>
   {
       ProcessEveryChange(data); // Called too often!
   });
   ```

5. **Oublier d'initialiser**
   ```csharp
   // ❌ WRONG - Crashera
   public override void Load()
   {
       EventSystem.Subscribe(...); // ModSDK not initialized!
   }
   ```

---

## 🔗 **Ressources**

| Document | Description |
|----------|-------------|
| [GameEventConstants.cs](../PerAspera.GameAPI.Events/GameEventConstants.cs) | Liste complète des événements |
| [EventHelpers.cs](../PerAspera.GameAPI.Events/EventHelpers.cs) | Helpers et filtres |
| [PerAspera.GameAPI.Events README](../PerAspera.GameAPI.Events/README.md) | Doc API Events |
| [EventSystem.cs](../PerAspera.ModSDK/Systems/EventSystem.cs) | Système d'abonnement |
| [Examples/](../Examples/) | Mods d'exemple complets |

---

## 🚀 **Template Mod Complet**

```csharp
using BepInEx;
using BepInEx.Unity.IL2CPP;
using PerAspera.ModSDK;
using PerAspera.GameAPI.Events;

namespace MyAwesomeMod
{
    [BepInPlugin("com.myname.awesomemod", "Awesome Mod", "1.0.0")]
    public class AwesomeModPlugin : BasePlugin
    {
        public override void Load()
        {
            Log.LogInfo("Loading Awesome Mod...");
            
            // 1. Initialize SDK
            ModSDK.Initialize(this);
            
            // 2. Subscribe to native events
            SubscribeToNativeEvents();
            
            // 3. Subscribe to custom events (from other mods)
            SubscribeToCustomEvents();
            
            Log.LogInfo("✅ Awesome Mod loaded!");
        }
        
        private void SubscribeToNativeEvents()
        {
            // Climate monitoring
            EventSystem.Subscribe(GameEventConstants.TemperatureChanged, OnTemperatureChange);
            EventSystem.Subscribe(GameEventConstants.CO2PressureChanged, OnCO2Change);
            
            // Time tracking
            EventSystem.Subscribe(GameEventConstants.MartianDayChanged, OnDayPassed);
            
            // Building management
            EventSystem.Subscribe(GameEventConstants.BuildingSpawned, OnBuildingSpawned);
        }
        
        private void SubscribeToCustomEvents()
        {
            // Listen to events from other mods
            EventSystem.Subscribe("OtherMod.CustomEvent", OnOtherModEvent);
        }
        
        private void OnTemperatureChange(object data)
        {
            if (EventHelpers.TryGetEventData<ClimateEventData>(data, out var climate))
            {
                if (EventHelpers.IsSignificantChange(climate, 1.0f))
                {
                    ProcessTemperatureChange(climate);
                }
            }
        }
        
        private void OnCO2Change(object data)
        {
            if (EventHelpers.TryGetEventData<ClimateEventData>(data, out var climate))
            {
                Log.LogInfo($"CO2: {climate.CurrentValue:F2} kPa");
            }
        }
        
        private void OnDayPassed(object data)
        {
            if (EventHelpers.TryGetEventData<MartianDayEventData>(data, out var day))
            {
                int year = EventHelpers.GetMartianYear(day.CurrentSol);
                Log.LogInfo($"Sol {day.CurrentSol} - Year {year}");
                
                // Publish custom event
                if (EventHelpers.IsMilestone(day.CurrentSol, 100))
                {
                    PublishMilestone(day.CurrentSol);
                }
            }
        }
        
        private void OnBuildingSpawned(object data)
        {
            Log.LogInfo("New building!");
        }
        
        private void OnOtherModEvent(object data)
        {
            Log.LogInfo("Received event from another mod!");
        }
        
        private void ProcessTemperatureChange(ClimateEventData climate)
        {
            float delta = EventHelpers.GetClimateDelta(climate);
            Log.LogInfo($"🌡️ Significant temp change: Δ{delta:+0.0;-0.0}K");
        }
        
        private void PublishMilestone(int sol)
        {
            var milestone = new MilestoneEvent { Sol = sol };
            EventSystem.Publish("AwesomeMod.Milestone", milestone);
        }
    }
    
    public class MilestoneEvent
    {
        public int Sol { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
```

---

**🎉 Vous êtes prêt à modder Per Aspera avec un système d'événements type-safe et puissant !**
