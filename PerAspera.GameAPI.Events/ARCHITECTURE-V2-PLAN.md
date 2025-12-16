# PerAspera.GameAPI.Events - Architecture Complète v2.0

**Date**: 2024-12-16  
**Status**: 🔄 **ARCHITECTURE REFACTORING**

---

## 🎯 Objectif

**Centraliser TOUS les événements dans le sous-projet `PerAspera.GameAPI.Events`**

### Problème Actuel
- ❌ Événements éparpillés entre GameAPI\Events\ et GameAPI.Events\
- ❌ Pas de séparation claire Native vs SDK
- ❌ Difficile de maintenir

### Solution Proposée
✅ **Un seul projet** : `PerAspera.GameAPI.Events`  
✅ **Deux namespaces** : `.Native` et `.SDK`  
✅ **Organisation claire** : Native/ et SDK/ folders

---

## 📦 Nouvelle Structure

```
PerAspera.GameAPI.Events/
│
├── Core/                               # Classes de base
│   ├── IGameEvent.cs                  # Interface commune
│   ├── GameEventBase.cs               # Base abstraite
│   ├── NativeGameEventBase.cs         # Base événements natifs
│   └── SDKEventBase.cs                # Base événements SDK
│
├── Data/                               # Classes de données
│   ├── ClimateEventData.cs            # Données climatiques
│   ├── MartianDayEventData.cs         # Données jour martien
│   └── BuildingEventData.cs           # Données buildings
│
├── Native/                             # Événements du JEU natif
│   ├── BuildingEvents.cs              # BuildingSpawnedNativeEvent, etc.
│   ├── ClimateEvents.cs               # TemperatureChangedNativeEvent, etc.
│   ├── DroneEvents.cs                 # DroneSpawnedNativeEvent, etc.
│   ├── ResourceEvents.cs              # ResourceAddedNativeEvent, etc.
│   └── UniverseEvents.cs              # Universe-level events
│
├── SDK/                                # Événements du SDK custom
│   ├── SystemEvents.cs                # BaseGameDetectedEvent, GameFullyLoadedEvent
│   ├── ModEvents.cs                   # ModSystemInitializedEvent, etc.
│   └── PlayerEvents.cs                # PlayerFactionDetectedEvent, etc.
│
├── Constants/
│   ├── NativeEventConstants.cs        # Constantes événements natifs
│   └── SDKEventConstants.cs           # Constantes événements SDK
│
├── Helpers/
│   ├── EventHelpers.cs                # Helpers généraux
│   ├── ClimateHelpers.cs              # Helpers climatiques
│   └── TimeHelpers.cs                 # Helpers temporels
│
├── GameEventConstants.cs               # DEPRECATED - migrer vers Constants/
├── EventHelpers.cs                     # DEPRECATED - migrer vers Helpers/
├── README.md
├── MODDER-GUIDE.md
└── PerAspera.GameAPI.Events.csproj
```

---

## 🔄 Migration Plan

### Phase 1: Créer la structure
- [ ] Créer dossiers Core/, Data/, Native/, SDK/, Constants/, Helpers/
- [ ] Créer NativeEventConstants.cs et SDKEventConstants.cs

### Phase 2: Migrer les bases
- [ ] Déplacer IGameEvent.cs → Core/
- [ ] Déplacer GameEventBase.cs → Core/
- [ ] Déplacer NativeGameEventBase.cs → Core/
- [ ] Créer SDKEventBase.cs → Core/

### Phase 3: Migrer les Data
- [ ] Déplacer ClimateEventData.cs → Data/
- [ ] Déplacer MartianDayEventData.cs → Data/
- [ ] Créer BuildingEventData.cs → Data/

### Phase 4: Migrer les Native Events
- [ ] Déplacer NativeGameEvents.cs content → Native/BuildingEvents.cs, ClimateEvents.cs, etc.
- [ ] Organiser par catégorie (Building, Climate, Drone, Resource, Universe)

### Phase 5: Migrer les SDK Events
- [ ] Déplacer GameEvents.cs content → SDK/SystemEvents.cs, ModEvents.cs, PlayerEvents.cs
- [ ] Séparer par catégorie (System, Mod, Player)

### Phase 6: Organiser les Helpers
- [ ] Déplacer EventHelpers.cs → Helpers/EventHelpers.cs
- [ ] Extraire climate helpers → Helpers/ClimateHelpers.cs
- [ ] Extraire time helpers → Helpers/TimeHelpers.cs

### Phase 7: Nettoyer GameAPI\Events\
- [ ] Supprimer GameAPI\Events\ folder (tout migré)
- [ ] Mettre à jour les références dans GameAPI

---

## 📋 Namespaces Proposés

```csharp
// Core
namespace PerAspera.GameAPI.Events.Core

// Data
namespace PerAspera.GameAPI.Events.Data

// Native events
namespace PerAspera.GameAPI.Events.Native

// SDK events
namespace PerAspera.GameAPI.Events.SDK

// Constants
namespace PerAspera.GameAPI.Events.Constants

// Helpers
namespace PerAspera.GameAPI.Events.Helpers
```

---

## 🎯 Usage Après Migration

### Moddeur utilise événements NATIFS
```csharp
using PerAspera.GameAPI.Events.Constants;
using PerAspera.GameAPI.Events.Native;
using PerAspera.GameAPI.Events.Helpers;

EventSystem.Subscribe(NativeEventConstants.BuildingSpawned, (data) =>
{
    if (EventHelpers.TryGetEventData<BuildingSpawnedNativeEvent>(data, out var evt))
    {
        Log.LogInfo($"Building: {evt.BuildingTypeKey}");
    }
});
```

### Moddeur utilise événements SDK
```csharp
using PerAspera.GameAPI.Events.Constants;
using PerAspera.GameAPI.Events.SDK;

EventSystem.Subscribe(SDKEventConstants.GameFullyLoaded, (data) =>
{
    if (EventHelpers.TryGetEventData<GameFullyLoadedEvent>(data, out var evt))
    {
        Log.LogInfo("Game ready!");
    }
});
```

### Moddeur créé événements custom
```csharp
using PerAspera.GameAPI.Events.Core;

public class MyCustomEvent : SDKEventBase
{
    public override string EventType => "MyMod.CustomEvent";
    public string Message { get; set; }
}
```

---

## ✅ Avantages

1. ✅ **Organisation claire** : Native vs SDK séparés
2. ✅ **Namespace propres** : `.Native`, `.SDK`, `.Helpers`
3. ✅ **Facilité de maintenance** : Chaque catégorie dans son fichier
4. ✅ **Découvrabilité** : IntelliSense guidé par dossiers
5. ✅ **Scalabilité** : Facile d'ajouter nouveaux événements
6. ✅ **Package unique** : Un seul NuGet à installer

---

## 🚀 Next Steps

1. Valider l'architecture proposée
2. Créer les dossiers et fichiers de base
3. Migrer progressivement (phase par phase)
4. Mettre à jour la documentation
5. Rebuild et tests

---

**🤔 Questions à valider :**
- ✅ Séparer Native/ et SDK/ ?
- ✅ Créer SDKEventBase en plus de NativeEventBase ?
- ✅ Séparer les constantes (NativeEventConstants vs SDKEventConstants) ?
- ✅ Organiser les helpers par domaine (Climate, Time, Building) ?
