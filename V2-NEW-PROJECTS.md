# SDK v2.0 - Nouveaux Sous-Projets Documentation

**Date**: 16 décembre 2024  
**Version SDK**: 2.0.0 (en développement)  
**Changements**: Ajout de 2 nouveaux sous-projets modulaires

---

## 🆕 Nouveaux Sous-Projets

### 1. PerAspera.GameAPI.Wrappers

**Localisation**: `SDK/PerAspera.GameAPI.Wrappers/`

#### Objectif
Fournir des **wrappers type-safe** pour les classes natives du jeu avec noms identiques aux classes IL2CPP. Isolation complète de la complexité IL2CPP pour les modders.

#### Contenu
```
PerAspera.GameAPI.Wrappers/
├── WrapperBase.cs              # Classe abstraite avec SafeInvoke, logging, validation
├── Building.cs                 # Wrapper classe Building native
├── Planet.cs                   # Wrapper classe Planet native
├── Universe.cs                 # Wrapper classe Universe native
├── README.md                   # Documentation complète
└── PerAspera.GameAPI.Wrappers.csproj
```

#### Caractéristiques Clés
- ✅ **Noms identiques au jeu** : `Building`, `Planet`, `Universe` (pas `BuildingWrapper`)
- ✅ **Namespace distinct** : `PerAspera.GameAPI.Wrappers` (évite conflits)
- ✅ **Safe by default** : Toutes méthodes wrapped avec validation null
- ✅ **IntelliSense complet** : Documentation XML sur toutes propriétés/méthodes
- ✅ **Gestion d'erreurs** : Logging automatique des erreurs IL2CPP

#### API Publique
```csharp
using PerAspera.GameAPI.Wrappers;

// Accès singletons
var planet = Planet.GetCurrent();
var universe = Universe.GetCurrent();

// Wrapping objets natifs
var building = Building.FromNative(nativeObj);

// Propriétés type-safe
float temp = planet.Temperature;
planet.Temperature = 300f;
int sol = universe.CurrentSol;
```

#### Dépendances
- PerAspera.Core
- PerAspera.GameAPI (accès KeeperTypeRegistry)
- BepInEx.Unity.IL2CPP

---

### 2. PerAspera.ModSDK.Events

**Localisation**: `SDK/PerAspera.ModSDK.Events/`

#### Objectif
Fournir **constantes typées** et **helpers** pour le système d'événements. Remplace les "magic strings" par des constantes avec IntelliSense.

#### Contenu
```
PerAspera.ModSDK.Events/
├── GameEventConstants.cs       # Constantes pour tous événements natifs
├── EventHelpers.cs             # Utilities (filters, type checking, logging)
├── README.md                   # Documentation + exemples
└── PerAspera.ModSDK.Events.csproj
```

#### Caractéristiques Clés
- ✅ **Constantes typées** : `GameEventConstants.BuildingSpawned` au lieu de `"NativeBuildingSpawned"`
- ✅ **Helpers intelligents** : Filtres, conversions, calculs (années martiennes, etc.)
- ✅ **Type checking safe** : `TryGetEventData<T>()` pour éviter casts dangereux
- ✅ **Logging formaté** : `LogClimateEvent()`, `LogDayEvent()` avec formatage automatique
- ✅ **Filtres réutilisables** : `ClimateThresholdFilter()`, `BuildingTypeFilter()`, etc.

#### API Publique

**Constantes**:
```csharp
using PerAspera.ModSDK.Events;

// ✅ Type-safe, auto-completion
EventSystem.Subscribe(GameEventConstants.BuildingSpawned, handler);
EventSystem.Subscribe(GameEventConstants.TemperatureChanged, handler);
EventSystem.Subscribe(GameEventConstants.MartianDayChanged, handler);
```

**Helpers**:
```csharp
// Type checking safe
if (EventHelpers.TryGetEventData<ClimateEventData>(data, out var climate))
{
    // Analyse changement
    float delta = EventHelpers.GetClimateDelta(climate);
    bool significant = EventHelpers.IsSignificantChange(climate, 1.0f);
    
    // Logging formaté
    EventHelpers.LogClimateEvent(climate, "TEMP");
}

// Calculs temps martien
int year = EventHelpers.GetMartianYear(sol);
bool newYear = EventHelpers.IsNewMartianYear(sol);

// Filtres
var solarFilter = EventHelpers.BuildingTypeFilter("SolarPanel", "SolarPanelAdvanced");
if (solarFilter(eventData))
{
    // Traiter uniquement solar panels
}
```

#### Dépendances
- PerAspera.Core
- PerAspera.GameAPI (accès aux classes d'événements)
- BepInEx.Unity.IL2CPP

---

## 🏗️ Architecture Mise à Jour

### Ordre de Compilation
```
1. PerAspera.Core                (Utilitaires bas niveau)
2. PerAspera.GameAPI             (Système événements natifs + wrappers IL2CPP)
3. PerAspera.GameAPI.Wrappers    (API publique objets jeu) ← NOUVEAU
4. PerAspera.ModSDK.Events       (API publique événements) ← NOUVEAU
5. PerAspera.ModSDK              (SDK complet)
```

### Flux de Dépendances
```
ModSDK
├─→ ModSDK.Events ────→ GameAPI ─→ Core
└─→ GameAPI.Wrappers ─→ GameAPI ─→ Core
```

### Couches d'Abstraction
```
Layer 5: ModSDK              (API complète pour modders)
         │
Layer 4: ModSDK.Events       (Constantes + helpers événements)
         GameAPI.Wrappers    (Wrappers objets jeu)
         │
Layer 3: GameAPI             (Système natif + NativeEventPatcher)
         │
Layer 2: Core                (Utilitaires IL2CPP)
         │
Layer 1: BepInEx             (Framework)
```

---

## 📚 Documentation Associée

### Wrappers
- `SDK/PerAspera.GameAPI.Wrappers/README.md` - Guide complet wrappers
- `SDK/PerAspera.GameAPI.Wrappers/WrapperBase.cs` - Documentation XML inline

### Events
- `SDK/PerAspera.ModSDK.Events/README.md` - Guide complet événements + exemples
- `SDK/PerAspera.GameAPI/Events/NATIVE_EVENTS_DOCUMENTATION.md` - Doc technique événements natifs
- `DOC/EVENT-SDK-ARCHITECTURE.md` - Architecture complète système événements

---

## 🎯 Avantages pour Modders

### Avant (v1.x)
```csharp
// ❌ Magic strings
EventSystem.Subscribe("NativeBuildingSpawned", handler);

// ❌ Casts dangereux
var evt = (BuildingSpawnedNativeEvent)data; // Crash si mauvais type!

// ❌ Accès IL2CPP complexe
var planet = KeeperTypeRegistry.GetPlanet();
float temp = planet.InvokeMethod<float>("GetTemperature");
```

### Après (v2.0)
```csharp
// ✅ Constantes typées avec IntelliSense
EventSystem.Subscribe(GameEventConstants.BuildingSpawned, handler);

// ✅ Type checking safe
if (EventHelpers.TryGetEventData<BuildingSpawnedNativeEvent>(data, out var evt))
{
    // Utilisation safe
}

// ✅ API publique simple
var planet = Planet.GetCurrent();
float temp = planet.Temperature;
```

---

## 🔄 Migration v1.x → v2.0

### Wrappers
**Avant**:
```csharp
using PerAspera.GameAPI.Native;

var planet = KeeperTypeRegistry.GetPlanet();
float temp = planet.InvokeMethod<float>("GetTemperature");
planet.InvokeMethod("SetTemperature", 300f);
```

**Après**:
```csharp
using PerAspera.GameAPI.Wrappers;

var planet = Planet.GetCurrent();
float temp = planet.Temperature;
planet.Temperature = 300f;
```

### Events
**Avant**:
```csharp
EventSystem.Subscribe("NativeBuildingSpawned", (data) => {
    var evt = (BuildingSpawnedNativeEvent)data;
    Logger.LogInfo($"Building: {evt.BuildingTypeKey}");
});
```

**Après**:
```csharp
using PerAspera.ModSDK.Events;

EventSystem.Subscribe(GameEventConstants.BuildingSpawned, (data) => {
    if (EventHelpers.TryGetEventData<BuildingSpawnedNativeEvent>(data, out var evt))
    {
        Logger.LogInfo($"Building: {evt.BuildingTypeKey}");
    }
});
```

---

## 📊 Impact Analyse

| Aspect | v1.x | v2.0 | Impact |
|--------|------|------|--------|
| **Packages** | 3 projets | 5 projets | +2 (modularité) |
| **API publique** | GameAPI mixte | Wrappers séparés | ✅ Clarté |
| **Événements** | Magic strings | Constantes typées | ✅ Type-safety |
| **Courbe apprentissage** | Moyenne | Basse | ✅ Plus simple |
| **Performance** | Bonne | Identique | = |
| **Taille SDK** | ~500KB | ~600KB | +20% (docs) |

---

## ✅ Checklist Implémentation

- [x] Créer PerAspera.GameAPI.Wrappers projet
- [x] Implémenter WrapperBase.cs
- [x] Implémenter Building.cs, Planet.cs, Universe.cs
- [x] Documentation README Wrappers
- [x] Créer PerAspera.ModSDK.Events projet
- [x] Implémenter GameEventConstants.cs
- [x] Implémenter EventHelpers.cs
- [x] Documentation README Events
- [x] Ajouter projets à PerAspera.SDK.sln
- [ ] Mettre à jour SDK/README.md
- [ ] Mettre à jour SDK-ANALYSIS-REPORT.md
- [ ] Créer tests unitaires
- [ ] Mettre à jour exemples
- [ ] Mettre à jour CHANGELOG.md
- [ ] Build et validation complète

---

**Date création**: 16 décembre 2024  
**Auteur**: GitHub Copilot  
**Statut**: ✅ Projets créés, documentation en cours
