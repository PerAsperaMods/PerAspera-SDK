# SDK v2.0.0 - Architecture Refactoring Plan

**Date**: 2024-12-16  
**Status**: 🔴 BREAKING CHANGES - Major Version  
**Migration**: v1.x → v2.0.0 (Non backward-compatible)

---

## 🎯 Vision v2.0

**Event-Driven Architecture + Helper APIs Sans État**

### Principes Directeurs

1. **Événements Natifs First** - Toute donnée vient des événements (NativeEventPatcher)
2. **Pas de Stockage d'Instances** - Accès dynamique via KeeperTypeRegistry
3. **Helpers = Proxies Intelligents** - Wrappers sans état pour simplifier l'usage
4. **Builders Fluent API** - Construction assistée d'objets complexes
5. **Type-Safe** - Fortement typé avec génériques et interfaces

---

## 🗑️ Ce qui est SUPPRIMÉ

### 1. Mirror Classes (Instance Storage)
```
❌ DELETE: Mirror/MirrorBaseGame.cs
❌ DELETE: Mirror/MirrorUniverse.cs
❌ DELETE: Mirror/MirrorPlanet.cs
❌ DELETE: Mirror/MirrorEventBus.cs
❌ DELETE: Mirror/SingletonMirror.cs
⚠️  KEEP (evaluate): Mirror/MirrorBlackboard.cs
⚠️  KEEP (evaluate): Mirror/MirrorKeeper.cs
```

**Raison** : Avec les événements natifs, plus besoin de stocker/wraper les instances du jeu.

### 2. Instance Detection System
```
❌ DELETE: Detection/GameInstanceDetector.cs
⚠️  SIMPLIFY: Initialization/GameTypeInitializer.cs
   - Garder: FindGameType(), GetTypeByName()
   - Supprimer: GetBaseGameInstance(), GetUniverseInstance(), _baseGameInstance
```

**Raison** : KeeperTypeRegistry + NativeEventPatcher fournissent l'accès nécessaire.

### 3. ModSDK Instance Storage
```csharp
// ModSDK.cs - À SUPPRIMER
❌ private static MirrorBaseGame baseGame;
❌ private static object? _baseGameInstance;
❌ private static object? _universeInstance;
❌ Subscribe to BaseGameDetected event
```

**Raison** : Accès via Helpers sans stocker d'état.

---

## ✅ Ce qui est CONSERVÉ

### 1. PerAspera.Core (Intact)
```
✅ LogAspera.cs
✅ ReflectionHelpers.cs
✅ TypeExtensions.cs
✅ CargoQuantityHelper.cs
✅ IL2CPP/
```

**Raison** : Utilitaires fondamentaux, indépendants de l'architecture.

### 2. Native Event System (Intact)
```
✅ Native/NativeEventPatcher.cs
✅ Native/ModEventBus.cs
✅ Native/KeeperTypeRegistry.cs
✅ Native/KeeperInstanceLibrary.cs
✅ Events/NativeGameEvents.cs
✅ Events/ClimateEventData.cs
✅ Events/MartianDayEventData.cs
```

**Raison** : Cœur du nouveau système, produit les événements.

### 3. GameEvents Constants
```csharp
✅ ModSDK/GameEvents.cs
   - Garder toutes les constantes d'événements
   - Nettoyer les alias legacy (ou marquer @Deprecated)
```

---

## 🆕 Ce qui est AJOUTÉ

### 1. Helpers API (Proxies Intelligents)

#### ClimateHelper.cs
```csharp
namespace PerAspera.GameAPI.Helpers
{
    /// <summary>
    /// Helper API for climate operations
    /// No state storage - dynamic access via KeeperTypeRegistry
    /// </summary>
    public static class ClimateHelper
    {
        private static readonly LogAspera _log = new LogAspera("API.Climate");
        
        /// <summary>Get current planet temperature in Kelvin</summary>
        public static float? GetTemperature()
        {
            var planet = KeeperTypeRegistry.GetPlanet();
            return planet?.InvokeMethod<float>("GetTemperature");
        }
        
        /// <summary>Set planet temperature</summary>
        public static bool SetTemperature(float kelvin)
        {
            var planet = KeeperTypeRegistry.GetPlanet();
            if (planet == null) return false;
            
            planet.InvokeMethod("SetTemperature", kelvin);
            return true;
        }
        
        /// <summary>Get all climate data snapshot</summary>
        public static ClimateSnapshot? GetSnapshot()
        {
            var planet = KeeperTypeRegistry.GetPlanet();
            if (planet == null) return null;
            
            return new ClimateSnapshot
            {
                Temperature = planet.InvokeMethod<float>("GetTemperature"),
                CO2Pressure = planet.InvokeMethod<float>("GetCO2Pressure"),
                O2Pressure = planet.InvokeMethod<float>("GetO2Pressure"),
                WaterStock = planet.InvokeMethod<float>("GetWaterStock"),
                // etc...
            };
        }
    }
    
    public class ClimateSnapshot
    {
        public float Temperature { get; set; }
        public float CO2Pressure { get; set; }
        public float O2Pressure { get; set; }
        public float WaterStock { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
```

#### UniverseHelper.cs
```csharp
namespace PerAspera.GameAPI.Helpers
{
    /// <summary>
    /// Helper API for universe/time operations
    /// </summary>
    public static class UniverseHelper
    {
        /// <summary>Get current Martian sol</summary>
        public static int GetCurrentSol()
        {
            var universe = KeeperTypeRegistry.GetUniverse();
            return universe?.InvokeMethod<int>("GetDaysPassed") ?? 0;
        }
        
        /// <summary>Get current game speed multiplier</summary>
        public static float GetGameSpeed()
        {
            var universe = KeeperTypeRegistry.GetUniverse();
            return universe?.InvokeMethod<float>("GetGameSpeed") ?? 1.0f;
        }
        
        /// <summary>Set game speed (1.0 = normal, 2.0 = 2x, etc.)</summary>
        public static bool SetGameSpeed(float speed)
        {
            var universe = KeeperTypeRegistry.GetUniverse();
            if (universe == null) return false;
            
            universe.InvokeMethod("SetGameSpeed", speed);
            return true;
        }
        
        /// <summary>Check if game is paused</summary>
        public static bool IsPaused()
        {
            var universe = KeeperTypeRegistry.GetUniverse();
            return universe?.InvokeMethod<bool>("IsPaused") ?? false;
        }
    }
}
```

#### BuildingHelper.cs
```csharp
namespace PerAspera.GameAPI.Helpers
{
    /// <summary>
    /// Helper API for building operations
    /// </summary>
    public static class BuildingHelper
    {
        /// <summary>Find all buildings of a specific type</summary>
        public static IEnumerable<object> FindByType(string buildingTypeKey)
        {
            var baseGame = KeeperTypeRegistry.GetBaseGame();
            if (baseGame == null) yield break;
            
            var buildings = baseGame.InvokeMethod<object>("GetAllBuildings");
            // Filter by type...
        }
        
        /// <summary>Spawn a new building at position</summary>
        public static object? SpawnBuilding(string typeKey, float x, float y)
        {
            var baseGame = KeeperTypeRegistry.GetBaseGame();
            if (baseGame == null) return null;
            
            return baseGame.InvokeMethod<object>("SpawnBuilding", typeKey, x, y);
        }
        
        /// <summary>Get building atmospheric impact</summary>
        public static AtmosphericImpact? GetAtmosphericImpact(object building)
        {
            if (building == null) return null;
            
            return new AtmosphericImpact
            {
                CO2Change = building.InvokeMethod<float>("GetCO2Impact"),
                O2Change = building.InvokeMethod<float>("GetO2Impact"),
                HeatChange = building.InvokeMethod<float>("GetHeatImpact")
            };
        }
    }
    
    public class AtmosphericImpact
    {
        public float CO2Change { get; set; }
        public float O2Change { get; set; }
        public float HeatChange { get; set; }
    }
}
```

#### ResourceHelper.cs
```csharp
namespace PerAspera.GameAPI.Helpers
{
    /// <summary>
    /// Helper API for resource operations
    /// </summary>
    public static class ResourceHelper
    {
        /// <summary>Get current stock of a resource</summary>
        public static float GetResourceStock(string resourceKey)
        {
            var planet = KeeperTypeRegistry.GetPlanet();
            if (planet == null) return 0f;
            
            return planet.InvokeMethod<float>("GetResourceStock", resourceKey);
        }
        
        /// <summary>Add resource to planet stock</summary>
        public static bool AddResource(string resourceKey, float amount)
        {
            var planet = KeeperTypeRegistry.GetPlanet();
            if (planet == null) return false;
            
            planet.InvokeMethod("AddResource", resourceKey, amount);
            return true;
        }
        
        /// <summary>Check if resource is available</summary>
        public static bool HasResource(string resourceKey, float amount)
        {
            return GetResourceStock(resourceKey) >= amount;
        }
    }
}
```

### 2. Builder Pattern (Fluent API)

#### BuildingBuilder.cs
```csharp
namespace PerAspera.GameAPI.Builders
{
    /// <summary>
    /// Fluent API builder for creating buildings
    /// </summary>
    public class BuildingBuilder
    {
        private string _typeKey = "";
        private float _x, _y;
        private object? _faction;
        private bool _operative = true;
        
        public BuildingBuilder WithType(string typeKey)
        {
            _typeKey = typeKey;
            return this;
        }
        
        public BuildingBuilder AtPosition(float x, float y)
        {
            _x = x;
            _y = y;
            return this;
        }
        
        public BuildingBuilder OwnedBy(object faction)
        {
            _faction = faction;
            return this;
        }
        
        public BuildingBuilder Operative(bool operative = true)
        {
            _operative = operative;
            return this;
        }
        
        public object? Build()
        {
            var baseGame = KeeperTypeRegistry.GetBaseGame();
            if (baseGame == null) return null;
            
            var building = baseGame.InvokeMethod<object>("SpawnBuilding", _typeKey, _x, _y);
            if (building != null)
            {
                if (_faction != null)
                    building.InvokeMethod("SetFaction", _faction);
                building.InvokeMethod("SetOperative", _operative);
            }
            
            return building;
        }
    }
    
    // Usage:
    // var mine = new BuildingBuilder()
    //     .WithType("WaterMine")
    //     .AtPosition(100f, 200f)
    //     .Operative(true)
    //     .Build();
}
```

---

## 📦 Nouvelle Structure Finale

```
PerAspera.Core/                    ✅ Unchanged
├── LogAspera.cs
├── ReflectionHelpers.cs
└── TypeExtensions.cs

PerAspera.GameAPI/
├── Native/                        ✅ Event System
│   ├── NativeEventPatcher.cs
│   ├── ModEventBus.cs
│   ├── KeeperTypeRegistry.cs      ← Accès dynamique instances
│   └── KeeperInstanceLibrary.cs
│
├── Events/                        ✅ Event Data
│   ├── NativeGameEvents.cs
│   ├── ClimateEventData.cs
│   ├── MartianDayEventData.cs
│   ├── GameEventBase.cs
│   └── IGameEvent.cs
│
├── Helpers/                       🆕 NEW - API de convenance
│   ├── ClimateHelper.cs
│   ├── UniverseHelper.cs
│   ├── BuildingHelper.cs
│   ├── ResourceHelper.cs
│   ├── TechnologyHelper.cs
│   └── FactionHelper.cs
│
├── Builders/                      🆕 NEW - Fluent APIs
│   ├── BuildingBuilder.cs
│   ├── ResourceBuilder.cs
│   └── EventBuilder.cs
│
├── Models/                        🆕 NEW - DTOs
│   ├── ClimateSnapshot.cs
│   ├── BuildingInfo.cs
│   ├── ResourceInfo.cs
│   └── AtmosphericImpact.cs
│
├── Initialization/
│   └── GameTypeInitializer.cs     ⚠️  SIMPLIFIED
│
└── Mirror/                        ⚠️  OPTIONAL - Types complexes seulement
    ├── MirrorBlackboard.cs?
    └── MirrorKeeper.cs?

PerAspera.ModSDK/
├── ModSDK.cs                      ♻️  REFACTORED - Plus de stockage instances
├── GameEvents.cs                  ✅ Constants
├── PerAsperaSDKPlugin.cs          ✅ Base plugin
└── Systems/
    ├── EventSystem.cs             ♻️  Branché sur ModEventBus
    ├── LoggingSystem.cs           ✅ Unchanged
    └── OverrideSystem.cs          ✅ Unchanged
```

---

## 🔄 Migration Path (v1.x → v2.0)

### Breaking Changes

#### 1. Suppression accès direct instances
```csharp
// ❌ v1.x - NE MARCHE PLUS
var baseGame = SDK.GetBaseGame();
var universe = baseGame.GetUniverse();
var sol = universe.GetCurrentSol();

// ✅ v2.0 - NOUVELLE API
var sol = UniverseHelper.GetCurrentSol();
```

#### 2. Événements via ModEventBus
```csharp
// ❌ v1.x - Ancienne souscription
SDK.Events.Subscribe("TemperatureChanged", handler);

// ✅ v2.0 - Événements natifs
ModEventBus.Subscribe("NativeTemperatureChanged", (data) => {
    if (data is ClimateEventData climate) {
        // ...
    }
});
```

#### 3. Utilisation Builders
```csharp
// ❌ v1.x - Création manuelle
var building = baseGame.SpawnBuilding("Mine", 100, 200);
building.SetFaction(faction);
building.SetOperative(true);

// ✅ v2.0 - Fluent API
var building = new BuildingBuilder()
    .WithType("Mine")
    .AtPosition(100, 200)
    .OwnedBy(faction)
    .Operative(true)
    .Build();
```

---

## 📋 Implementation Checklist

### Phase 1: Cleanup (Breaking)
- [ ] Supprimer Mirror classes inutiles
- [ ] Supprimer GameInstanceDetector
- [ ] Simplifier GameTypeInitializer
- [ ] Nettoyer ModSDK.cs (remove instance storage)

### Phase 2: Helpers
- [ ] Créer ClimateHelper.cs
- [ ] Créer UniverseHelper.cs
- [ ] Créer BuildingHelper.cs
- [ ] Créer ResourceHelper.cs
- [ ] Créer TechnologyHelper.cs

### Phase 3: Builders
- [ ] Créer BuildingBuilder.cs
- [ ] Créer ResourceBuilder.cs
- [ ] Créer EventBuilder.cs

### Phase 4: Models
- [ ] Créer ClimateSnapshot.cs
- [ ] Créer BuildingInfo.cs
- [ ] Créer AtmosphericImpact.cs

### Phase 5: Testing
- [ ] Tester tous les Helpers
- [ ] Tester tous les Builders
- [ ] Créer exemples de migration
- [ ] Tests d'intégration

### Phase 6: Documentation
- [ ] Mettre à jour README.md
- [ ] Créer MIGRATION-GUIDE.md
- [ ] Mettre à jour API-Reference.md
- [ ] Créer exemples v2.0

### Phase 7: Release
- [ ] Archiver v1.1.0
- [ ] Bump version → 2.0.0
- [ ] Tag Git v2.0.0
- [ ] Publier release notes

---

## 🎯 Avantages v2.0

1. **✨ Simplicité** - Plus besoin de gérer des instances
2. **⚡ Performance** - Pas de stockage, accès direct
3. **🛡️ Robustesse** - Événements natifs = pas de bugs de détection
4. **📚 Clarté** - APIs helpers explicites et documentées
5. **🔧 Extensibilité** - Facile d'ajouter nouveaux helpers
6. **🎨 Ergonomie** - Fluent APIs pour constructions complexes

---

**Next Step**: Commencer Phase 1 (Cleanup) ?
