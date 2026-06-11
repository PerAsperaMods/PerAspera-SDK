# SDK Architecture — Per Aspera Modding

> **Lecture prioritaire pour les agents.** Ce fichier est la source d'autorité sur la structure du SDK.  
> Avant d'écrire du code, identifier le bon projet dans ce document.

---

## ⚠️ CONSOLIDATION 2026-06 — 4 assemblies

> Les « couches » ci-dessous décrivent désormais des **namespaces dans 2 assemblies**,
> plus des projets séparés (audit SDK-CRITICAL-REVIEW §1.1, exécuté 2026-06-10) :
>
> | Assembly | Absorbe |
> |----------|---------|
> | `PerAspera.Core` | Core + Core.IL2CppExtensions (dossier `IL2CppExtensions/`) |
> | `PerAspera.GameAPI` | GameAPI + Native + Events + Commands + Wrappers + Climate + Overrides + UI (sous-dossiers) |
> | `PerAspera.GameAPI.Database` | inchangé (isolé, SQLite) |
> | `PerAspera.ModSDK` | inchangé (façade) |
>
> Tous les namespaces sont inchangés — les sections par domaine ci-dessous restent
> valides comme documentation des APIs ; seul le découpage en projets a disparu.
> `PerAspera.Abstractions` (placeholder vide) a été supprimé.

## Graphe de dépendances (couches → namespaces)

```
┌─────────────────────────────────────────────────────────────┐
│  Layer 7 — PUBLIC FACADE                                    │
│  PerAspera.ModSDK          → point d'entrée unique pour les │
│                              mods (ModSDK.Initialize)       │
└─────────────┬───────────────────────────────────────────────┘
              │ dépend de tout
┌─────────────▼───────────────────────────────────────────────┐
│  Layer 6 — DOMAIN SYSTEMS                                   │
│  PerAspera.GameAPI.Climate ← Core, Commands, Wrappers       │
│  PerAspera.GameAPI.UI      ← Core, GameAPI, Wrappers        │
└─────────────┬───────────────────────────────────────────────┘
┌─────────────▼───────────────────────────────────────────────┐
│  Layer 5 — WRAPPERS                                         │
│  PerAspera.GameAPI.Wrappers ← Core, IL2CppExt, GameAPI,    │
│                               Native, Database              │
└─────────────┬───────────────────────────────────────────────┘
┌─────────────▼───────────────────────────────────────────────┐
│  Layer 4 — SERVICES                                         │
│  PerAspera.GameAPI.Commands ← Core, IL2CppExt, GameAPI,    │
│                               Events                        │
│  PerAspera.GameAPI.Database ← (NuGet seul — isolé)         │
└─────────────┬───────────────────────────────────────────────┘
┌─────────────▼───────────────────────────────────────────────┐
│  Layer 3 — GAME ACCESS                                      │
│  PerAspera.GameAPI          ← Core, IL2CppExt, Native,     │
│                               Overrides                     │
│  PerAspera.GameAPI.Events   ← Core, GameAPI, Native        │
└─────────────┬───────────────────────────────────────────────┘
┌─────────────▼───────────────────────────────────────────────┐
│  Layer 2 — NATIVE BRIDGE                                    │
│  PerAspera.GameAPI.Native   ← Core                         │
│  PerAspera.GameAPI.Overrides ← Core, IL2CppExt             │
└─────────────┬───────────────────────────────────────────────┘
┌─────────────▼───────────────────────────────────────────────┐
│  Layer 1 — UTILITIES                                        │
│  PerAspera.Core             (aucune dépendance projet)      │
└─────────────┬───────────────────────────────────────────────┘
┌─────────────▼───────────────────────────────────────────────┐
│  Layer 0 — IL2CPP PRIMITIVES                                │
│  PerAspera.Core.IL2CppExtensions  (aucune dépendance projet)│
└─────────────────────────────────────────────────────────────┘
```

> `Core` et `IL2CppExtensions` sont indépendants l'un de l'autre (aucune dépendance projet entre eux).

---

## Projets — référence rapide

### `PerAspera.Core.IL2CppExtensions` — Layer 0
**Rôle :** interop IL2CPP bas-niveau, sans dépendance projet.  
**Classes clés :**
- `IL2CppExtensions` — `GetMemberValue<T>`, `SetMemberValue`, `InvokeMethod`, `ConvertIl2CppList<T>`, `ConvertIl2CppArray<T>`
- `ReflectionHelpers` — `FindType`, `FindTypes`, `GetDerivedTypes`, `GetSingletonInstance<T>`
- `TypeExtensions`, `PerAsperaExtensions`, `ConsoleCommandExecutor`

**Utiliser quand :** accéder à un champ/méthode IL2CPP par réflexion, convertir des collections IL2CPP en managed.

---

### `PerAspera.Core` — Layer 1
**Rôle :** logging et utilitaires de réflexion partagés par tout le SDK.  
**Classes clés :**
- `LogAspera` — logger BepInEx + fichier par composant. Méthodes statiques `LogAspera.LogInfo/LogError…`
- `Utilities` — `GetMemberValue`, `SetFieldOrProp`, `GetFloatFieldOrProp`, `FindTypeStatic`, `ToFloat/ToInt`
- `CargoQuantityHelper` — utilitaire cargo

**Utiliser quand :** logger dans un plugin, accéder à un membre par reflection sans IL2CPP interop.  
**Ne pas utiliser pour :** accès aux objets du jeu — utiliser Wrappers.

---

### `PerAspera.GameAPI.Native` — Layer 2
**Rôle :** registry d'instances natives du jeu (singletons IL2CPP live).  
**Classes clés :**
- `InstanceManager` — `Initialize()`, `RegisterBaseGame/Universe/Planet`, `GetBaseGame/Universe/CurrentPlanet`, `GetInstance<T>(key)`
- `NativeTypes` — définitions des types natifs

**Utiliser quand :** accéder à `BaseGame`, `Universe`, `Planet` à partir d'un contexte sans Harmony.  
**Ne pas utiliser en premier** — préférer `GameApi.wrapper.basegame` (Wrappers layer).

---

### `PerAspera.GameAPI.Overrides` — Layer 2
**Rôle :** système de surcharge de getters (GetterOverride, OverridePatchSystem).  
**Classes clés :**
- `GetterOverrideRegistry` — enregistrement/activation des overrides
- `GetterOverride`, `IOverrideStrategy` + stratégies `Replace`/`Multiply`/`Clamp`
- `OverridePatchSystem`, `AutoOverridePatchAttribute` — pose des patches Harmony
- `TypeCompatibilityChecker` — validation de type avant override

**Utiliser quand :** surcharger une propriété calculée du jeu sans Harmony transpiler.

---

### `PerAspera.GameAPI` — Layer 3
**Rôle :** point d'accès central aux objets du jeu (BaseGame, Universe, Planet).  
**Classes clés :**
- `GameTypeInitializer` — découverte des types/instances du jeu (`GetBaseGameInstance()`)
- `EventPatchingService` variants — `SpaceProjectEventPatchingService` (alive), autres supprimés 2026-06
- `TypeDiscoveryCache`, `ResourceTypeDiscovery` — cache de découverte de types
- `Models` — `AtmosphereData`, `ClimateSnapshot`, `TerraformingStatus`
- `Patches` — `PlanetPatches`, `EnergyPatches`

**Utiliser quand :** base pour les projets Layer 4+.

---

### `PerAspera.GameAPI.Events` — Layer 3
**Rôle :** détection du démarrage du jeu et bus d'événements SDK.  
**Classes clés :**
- `EventsAutoStartPlugin` — plugin BepInEx qui démarre automatiquement la détection
- `NativeEventHub` — Postfix unique sur `GameEventBus.DispatchInternal`, 121 events natifs
- `NativeEventExtensions` — `ResolveBuilding(keeper)`, `ResolveFaction(universe)`, `ResolveDroneFromFaction(faction)`, `ResolveWayFromFaction(faction)`
- `BaseGameUpdatePatches` — poll lifecycle → `GameCommandsReadyEvent` + `GameUIReadyEvent`
- `EnhancedEventBus` — `Publish/Subscribe` + helpers `SubscribeTo*(handler)`
- Lifecycle events : `GameSessionStartedEvent` (ancrage natif GevUniverse*), `GameCommandsReadyEvent`, `GameUIReadyEvent`

**Utiliser quand :** s'abonner à l'initialisation du jeu, écouter des événements cross-mod.

---

### `PerAspera.GameAPI.Commands` — Layer 4
**Rôle :** exécution de commandes jeu (construction, ressources, factions).  
**Classes clés :**
- `CommandExecutor` — `Execute(command)`, `ExecuteAsync(command)`
- `CommandBusAccessor` — accès au bus natif de commandes du jeu
- `CommandBase` — base class pour commandes custom
- `CustomCommandRegistry` — enregistrement de nouvelles commandes
- `ICommand` — interface à implémenter

**Utiliser quand :** déclencher une action jeu (placer un bâtiment, importer une ressource).  
**Skill associée :** `/per-aspera-commands-sdk`

---

### `PerAspera.GameAPI.Database` — Layer 4
**Rôle :** persistance SQLite pour données YAML des mods.  
**Classes clés :**
- `ModDatabase` — `Instance`, `StoreYAMLData`, `RetrieveYAMLData`, `GetAtmosphericResources`, `ValidateDataType`, `NeedsUpdate`

**Utiliser quand :** stocker/requêter des données YAML custom entre sessions.  
**Note :** projet autonome — aucune dépendance sur les autres projets SDK (NuGet seul).  
**Skill associée :** `/per-aspera-database-modding`

---

### `PerAspera.GameAPI.Wrappers` — Layer 5
**Rôle :** wrappers typés et sûrs autour des objets IL2CPP natifs du jeu.  
**Classes clés (20+) :**
- `BaseGameWrapper` — `GetCurrent()`, accès à `Universe`, `Planet`, `CommandBus`
- `PlanetWrapper` — `GetCurrent()`, `GetAtmosphere()`, `GetResources()`
- `ResourceTypeWrapper`, `BuildingTypeWrapper`, `BuildingWrapper`
- `WrapperBase` — base class pour tous les wrappers (hérite de `NativeWrapper` / IL2CppExtensions)
- `GameDataManager`, `GameDataRegistries` — registres de données
- `SceneManager`, `SceneWrapper` — gestion des scènes Unity

**Point d'entrée préféré :**
```csharp
var baseGame = GameApi.wrapper.basegame;   // BaseGameWrapper
var planet   = GameApi.wrapper.planet;     // PlanetWrapper
var resource = GameApi.wrapper.resourcetype;
```

**Skill associée :** `/per-aspera-wrappers-sdk`

---

### `PerAspera.GameAPI.Climate` — Layer 6
**Rôle :** contrôle de l'atmosphère et de la terraformation.  
**Classes clés :**
- `ClimateController` — `SetTemperature`, `SetGasPressure`, `AddTerraformingEffect`
- `Atmosphere` — lecture/écriture des pressions de gaz
- `TerraformingEffectsController` — effets de terraformation actifs
- `ResourceBasedClimate` — mode atmosphère piloté par ressources

**Skill associée :** `/per-aspera-climate-sdk`

---

### `PerAspera.GameAPI.UI` — Layer 6
**Rôle :** panneaux HUD, overlays, fenêtres Unity IMGUI.  
**Classes clés :**
- `Components` — `UIPanel`, `UIGamePanel`, `UISimplePanel`, `UIResourceRow`, `UISimpleRow`, `UIOverlaySystem`, `UISceneHelper`
- `Core` — `UIColors`, `UIStyles`

**Skill associée :** agent `per-aspera-sdk-ui`

---

### `PerAspera.ModSDK` — Layer 7
**Rôle :** façade publique unique — point d'entrée pour tous les mods.  
**Classes clés :**
- `ModSDK.Initialize(modName, version)` — initialise tous les sous-systèmes (OverrideSystem, etc.)
- `PerAsperaSDKPlugin` — plugin BepInEx maître du SDK
- `GameEvents` — événements prédéfinis du SDK

**Utiliser quand :** bootstrap d'un mod. Un seul appel `ModSDK.Initialize` dans `Awake()`.

---

## Règles techniques obligatoires

### ⭐ Interop typé d'abord (vision 2026-06)

> Audit : `F:\ModPeraspera\docs\SDK-CRITICAL-REVIEW.md` · Migration pilote : `PlanetWrapper`

Les proxies interop générés par Il2CppInterop (`BepInEx\interop\` : `Planet`, `BaseGame`,
`Faction`, `Universe`…) sont référencés par tous les projets et donnent un **accès typé
compile-time** aux classes du jeu. **Ordre de préférence pour accéder au jeu :**

1. **Membre typé du proxy interop** — IntelliSense, erreur de build si le jeu change.
   Dans un wrapper : exposer le proxy (`public Planet? NativePlanet => GetNativeObject() as Planet;`)
   et déléguer typé (`NativePlanet?.GetAverageTemperature() ?? 0f`).
2. **Helpers Safe\*/Try\* de WrapperBase** — uniquement pour les membres natifs
   réellement inaccessibles (privés/strippés), **après vérification dans le dump décompilé**.
3. **Réflexion directe** — IL2CppExtensions seulement (règle ci-dessous, RS0030).

**Pourquoi :** un binding string-based (`SafeInvoke<float>("X")`) vers un membre absent ne
produit aucune erreur — juste `default(T)` silencieux. La migration PlanetWrapper a révélé
plusieurs API fantômes (`GetResourceStock`, `AddResource`, `buildings`) qui n'ont jamais
existé sur Planet et retournaient 0/vide depuis toujours. L'appel typé transforme cette
classe de bugs invisibles en erreurs de compilation.

### ⛔ Reflection — isolation dans IL2CppExtensions

**Toute utilisation de reflection est interdite hors de `PerAspera.Core.IL2CppExtensions`.**

| Contexte | Reflection autorisée ? |
|----------|----------------------|
| `PerAspera.Core.IL2CppExtensions` | ✅ Oui — c'est son rôle |
| Tout autre module SDK (`GameAPI.*`, `Core`, `ModSDK`…) | ❌ Non |
| Mods (`Individual-Mods\*`) | ❌ Non |

**Ce qui est interdit hors IL2CppExtensions :**
```csharp
// ❌ Interdit dans les wrappers, les mods, les autres modules SDK
typeof(X).GetMethod("Foo");
typeof(X).GetField("bar");
methodInfo.Invoke(obj, args);
MethodBase.GetMethodFromHandle(...);
```

**Ce qu'on fait à la place :**
```csharp
// ✅ D'ABORD : membre public → délégation typée au proxy interop (cf. règle ⭐ ci-dessus)
public string Name => NativePlanet?.name ?? "Unknown";

// ✅ FALLBACK : membre privé/strippé vérifié dans le dump → helpers WrapperBase
public bool IsAlive => SafeGetField<bool>("_alive");

// ✅ Si un nouveau helper de reflection est nécessaire :
//    1. L'ajouter dans IL2CppExtensions (ex: un helper GetFieldValue générique)
//    2. L'exposer via une méthode publique nommée clairement
//    3. L'appeler depuis le wrapper/module qui en a besoin
```

**Pourquoi :** la reflection IL2CPP est fragile (conflicts de types entre PluginsAssembly et ScriptsAssembly, marshaling implicite, erreurs silencieuses). Centraliser dans un seul projet permet de corriger un bug de reflection une seule fois au lieu de le chasser dans tout le codebase.

**Enforcement :** depuis 2026-06-10 la règle est outillée — `Microsoft.CodeAnalysis.BannedApiAnalyzers`
(diagnostic **RS0030**, liste dans `SDK\BannedSymbols.txt`, branché via `SDK\Directory.Build.props`)
émet un warning sur tout appel `GetMethod`/`GetField`/`GetProperty`/`Invoke` hors de
`PerAspera.Core.IL2CppExtensions`. Mode warning le temps de résorber l'existant (**596 violations
mesurées au rebuild du 2026-06-10**, voir `F:\ModPeraspera\docs\SDK-CRITICAL-REVIEW.md`) ;
passera en error ensuite.

---

## Table de décision — "Je veux faire X"

| Besoin | Projet à utiliser |
|--------|------------------|
| Logger dans mon plugin | `Core` → `new LogAspera("MonPlugin")` |
| Lire un champ IL2CPP par réflexion | `Core.IL2CppExtensions` → `GetMemberValue<T>` |
| Convertir une liste IL2CPP | `Core.IL2CppExtensions` → `ConvertIl2CppList<T>` |
| Accéder à BaseGame/Planet | `GameAPI.Wrappers` → `GameApi.wrapper.basegame` |
| Exécuter une commande jeu | `GameAPI.Commands` → `CommandExecutor.Execute` |
| S'abonner au démarrage | `GameAPI.Events` → `EnhancedEventBus.Subscribe(SDKEventConstants.GameFullyLoaded, …)` |
| Contrôler l'atmosphère | `GameAPI.Climate` → `ClimateController` |
| Persister des données YAML | `GameAPI.Database` → `ModDatabase.Instance.StoreYAMLData` |
| Bootstrapper un mod | `ModSDK` → `ModSDK.Initialize("MonMod")` |
| Surcharger une propriété jeu | `GameAPI.Overrides` → `OverridePatchSystem` |

---

## État réel des projets

> Mise à jour 2026-06-10 — l'ancienne section « stubs à compléter » était obsolète :
> `GameAPI`, `Overrides` et `UI` sont implémentés (voir leurs sections ci-dessus).

| Projet | État |
|--------|------|
| `PerAspera.Abstractions` | **Placeholder vide** — aucun fichier `.cs`. Ne pas supprimer (réservé aux interfaces partagées futures). |
| Tous les autres | Implémentés — le code source fait foi (Routage documentaire priorité 3). |


---

## Projets additionnels (hors SDK principal)

| Projet | Usage |
|--------|-------|
| `PerAspera.GameAPI.Commands.Test` | Tests unitaires Commands |
| `PerAspera.GameAPI.TwitchIntegration` | Intégration Twitch (API + PubSub) |
| `PerAspera.SDK.TwitchIntegration` | Façade SDK pour Twitch |
