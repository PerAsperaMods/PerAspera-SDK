# SDK Per Aspera — Contexte développement

Sous-projet du workspace `F:\ModPeraspera\`. Voir [CLAUDE.md](../CLAUDE.md) à la racine pour le contexte global.

## Structure (consolidation 2026-06 — audit §1.1)

```
PerAspera.Core              (aucune dépendance projet)
  └─ IL2CppExtensions/      namespace PerAspera.Core.IL2CPP (absorbé)
PerAspera.GameAPI           ← Core + Database
  └─ Native/ Events/ Commands/ Wrappers/ Climate/ Overrides/ UI/  (absorbés, namespaces inchangés)
PerAspera.GameAPI.Database  (isolé — SQLite)
PerAspera.ModSDK            ← Core + GameAPI
```

4 projets, plus d'ordre de build à gérer — `dotnet build SDK.sln` suffit.

## Commandes de build

```powershell
# Build complet depuis F:\ModPeraspera\SDK\
dotnet build SDK.sln

# Build rapide (projet individuel)
dotnet build PerAspera.GameAPI\PerAspera.GameAPI.csproj

# Déploiement vers le jeu (depuis F:\ModPeraspera\) — build Release + copie 4 DLL :
.\scripts\deployment\deploy.ps1 -CSharp -SdkOnly
```

> ⚠️ Le script de déploiement build en **Release** et copie depuis `bin\Release\net6.0\`.
> Un build Debug manuel n'est pas pris en compte par le script — vérifier le mtime de la
> DLL déployée vs la source après tout déploiement.

## Import SDK dans les mods

```xml
<!-- Dans le .csproj de chaque mod -->
<Import Project="F:\ModPeraspera\SDK_DLL\sdkDLL.props" />
```

## Règles techniques critiques

### IL2CPP Type Safety
- `Type` nu = `System.Type` via alias global (`Directory.Build.props`) — plus de conflit CS0104 avec les assemblies du jeu
- Pour d'autres collisions de noms, ajouter un alias `<Using Include="..." Alias="..."/>` plutôt qu'une règle manuelle

### Accès au jeu — interop typé d'abord
> Audit complet : `F:\ModPeraspera\docs\SDK-CRITICAL-REVIEW.md`

Les proxies interop typés (`Planet`, `BaseGame`, `Faction`…) sont référencés par tous les projets.

**Ordre de préférence :**
1. **Membre typé du proxy interop** — compile-time safety, IntelliSense, erreur de build si le jeu change
2. **Wrapper SDK** — exposer le proxy typé (ex: `PlanetWrapper.NativePlanet`) et déléguer typé
3. **`SafeInvoke`/réflexion** — uniquement pour membres natifs inaccessibles (privés/strippés). Surveillé par **RS0030**

`SafeInvoke<float>("X")` échoue silencieusement (retourne `0`) — toujours vérifier dans InteropDump que le membre existe.

### SDK Access Pattern
```csharp
var baseGame = GameApi.wrapper.basegame;   // BaseGameWrapper (méthode préférée)
var planet   = GameApi.wrapper.planet;     // PlanetWrapper
float kelvin = planet.NativePlanet?.GetAverageTemperature() ?? 0f;  // proxy typé
var nativePlanet = Native.planet;          // natif IL2CPP (uniquement pour interop)
```

### Protocole SDK-First
Avant tout patch BepInEx/Harmony, vérifier d'abord :
1. `F:\ModPeraspera\docs\Capabilities-Matrix.md` — SDK couvre-t-il le besoin ?
2. `F:\ModPeraspera\docs\[Class]-Enhanced.md` — capacités SDK wrapper
3. Patcher uniquement pour les gaps confirmés.

### Autres règles
- `GameApi.wrapper.*` en priorité sur les instantiations directes
- `LogAspera` pour tout logging (pas `Log.LogInfo` directement)
- XML doc `<summary>` + `<example>` obligatoire sur toutes les méthodes publiques

## Référence sources jeu décompilées

Pour comprendre les classes natives à wrapper :
- `F:\ModPeraspera\Tools\InteropDump\ScriptsAssembly\` — **source de vérité** (ilspycmd, proxies typés) — vérifier ici EN PREMIER
- `F:\ModPeraspera\Tools\lispyExtract\` — fallback — dump plat par nom de classe
- `F:\ModPeraspera_Raw_Extrac\PerAsperaData\ScriptsAssembly\` — fallback — stubs par namespace (supersédé par InteropDump pour les lookups C#)
