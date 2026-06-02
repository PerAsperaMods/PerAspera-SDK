# SDK Per Aspera — Contexte développement

Sous-projet du workspace `F:\ModPeraspera\`. Voir [CLAUDE.md](../CLAUDE.md) à la racine pour le contexte global.

## Ordre de build (dépendances)

```
1. PerAspera.Abstractions          (aucune dépendance)
2. PerAspera.Core
3. PerAspera.Core.IL2CppExtensions ← dépend de Core
4. PerAspera.GameAPI.Native
5. PerAspera.GameAPI               ← dépend de Core + Native
6. PerAspera.GameAPI.Events        ← dépend de GameAPI
7. PerAspera.GameAPI.Commands      ← dépend de GameAPI + Events
8. PerAspera.GameAPI.Climate       ← dépend de GameAPI + Events
9. PerAspera.GameAPI.Overrides     ← dépend de GameAPI
10. PerAspera.GameAPI.Wrappers     ← dépend de GameAPI
11. PerAspera.ModSDK               ← dépend de tout
```

## Commandes de build

```powershell
# Build complet depuis F:\ModPeraspera\SDK\
.\Build-SDK.ps1

# Build rapide (projet individuel)
dotnet build PerAspera.GameAPI.Wrappers\PerAspera.GameAPI.Wrappers.csproj

# DLL buildées → SDK-DLL\ (copier dans BepInEx/plugins/ pour tester)
```

## Import SDK dans les mods

```xml
<!-- Dans le .csproj de chaque mod -->
<Import Project="F:\ModPeraspera\SDK\SDK-DLL\sdkDLL.props" />
```

## Règles critiques

- `System.Type` JAMAIS `Type` nu (conflit IL2CPP entre assemblies)
- `GameApi.wrapper.*` en priorité sur les instantiations directes
- `LogAspera` pour tout logging (pas `Log.LogInfo` directement)
- XML doc `<summary>` + `<example>` obligatoire sur toutes les méthodes publiques
- Vérifier `F:\ModPeraspera\SDK-Enhanced-Classes\` avant tout ajout d'API

## Référence sources jeu décompilées

Pour comprendre les classes natives à wrapper :
- `F:\ModPeraspera_Raw_Extrac\PerAsperaData\ScriptsAssembly\` — C# par namespace
- `F:\ModPeraspera\Tools\lispyExtract\` — dump plat par classe
