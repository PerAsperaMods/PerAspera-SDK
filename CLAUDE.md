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
.\Build-SDK.ps1

# Build rapide (projet individuel)
dotnet build PerAspera.GameAPI.Wrappers\PerAspera.GameAPI.Wrappers.csproj

# Déploiement vers le jeu : Deploy-SDK-DLLs.ps1 → BepInEx\plugins\SDK\
```

## Import SDK dans les mods

```xml
<!-- Dans le .csproj de chaque mod -->
<Import Project="F:\ModPeraspera\SDK_DLL\sdkDLL.props" />
```

## Règles critiques

- `Type` nu = `System.Type` via alias global (`Directory.Build.props`) — plus de conflit CS0104 avec les assemblies du jeu
- `GameApi.wrapper.*` en priorité sur les instantiations directes
- `LogAspera` pour tout logging (pas `Log.LogInfo` directement)
- XML doc `<summary>` + `<example>` obligatoire sur toutes les méthodes publiques
- Vérifier `F:\ModPeraspera\docs\` avant tout ajout d'API

## Référence sources jeu décompilées

Pour comprendre les classes natives à wrapper :
- `F:\ModPeraspera_Raw_Extrac\PerAsperaData\ScriptsAssembly\` — C# par namespace
- `F:\ModPeraspera\Tools\lispyExtract\` — dump plat par classe
