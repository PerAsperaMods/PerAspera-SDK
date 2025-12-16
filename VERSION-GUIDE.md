# 🏷️ PerAspera SDK Versioning Guide

Ce guide explique comment gérer les versions du SDK PerAspera de manière professionnelle et automatisée.

## 📋 Structure de Versioning

Le SDK utilise [Semantic Versioning](https://semver.org/) :

```
MAJOR.MINOR.PATCH[-PRERELEASE]
```

- **MAJOR** : Changements breaking de l'API
- **MINOR** : Nouvelles fonctionnalités compatibles
- **PATCH** : Corrections de bugs compatibles
- **PRERELEASE** : alpha, beta, rc

## 🛠️ Outils de Gestion

### Script PowerShell `Manage-Version.ps1`

Script principal pour toutes les opérations de versioning :

```powershell
# Afficher la version actuelle
.\Manage-Version.ps1 -Action show

# Bump versions
.\Manage-Version.ps1 -Action bump-major   # 1.0.0 → 2.0.0
.\Manage-Version.ps1 -Action bump-minor   # 1.0.0 → 1.1.0  
.\Manage-Version.ps1 -Action bump-patch   # 1.0.0 → 1.0.1

# Définir une version spécifique
.\Manage-Version.ps1 -Action set-version -Version "2.0.0-beta"

# Créer une pre-release
.\Manage-Version.ps1 -Action pre-release -PreReleaseType beta

# Promouvoir en stable
.\Manage-Version.ps1 -Action stable

# Build et package
.\Manage-Version.ps1 -Action build
.\Manage-Version.ps1 -Action package
```

### Fichiers de Configuration

#### `Version.props`
Fichier central contenant toutes les informations de version :

```xml
<SDKVersion>1.0.0-beta</SDKVersion>
<SDKVersionPrefix>1.0.0</SDKVersionPrefix>
<SDKVersionSuffix>beta</SDKVersionSuffix>
```

#### `CHANGELOG.md`
Historique détaillé des changements suivant [Keep a Changelog](https://keepachangelog.com/).

## 🚀 Workflow de Release

### 1. Développement
```powershell
# Développement en cours sur des branches features
git checkout -b feature/new-api
# ... développement ...
git commit -m "feat: add new API feature"
```

### 2. Pre-release
```powershell
# Créer une version beta pour tests
.\Manage-Version.ps1 -Action pre-release -PreReleaseType beta
# Version: 1.0.0-beta ou 1.0.0-beta.2

git add .
git commit -m "chore: bump to v1.0.0-beta"
git push origin main
```

### 3. Release Candidate
```powershell
# Version release candidate
.\Manage-Version.ps1 -Action pre-release -PreReleaseType rc
# Version: 1.0.0-rc

git add .
git commit -m "chore: bump to v1.0.0-rc"
git push origin main
```

### 4. Release Stable
```powershell
# Promouvoir en version stable
.\Manage-Version.ps1 -Action stable
# Version: 1.0.0

git add .
git commit -m "chore: release v1.0.0"
git tag v1.0.0
git push origin main --tags
```

## 🤖 Automatisation GitHub Actions

### Déclenchement Automatique

Le workflow GitHub Actions se déclenche sur :
- **Push de tags** : `v*.*.*` (ex: `v1.0.0`, `v1.0.0-beta`)
- **Déclenchement manuel** : via GitHub UI

### Processus Automatisé

1. **Validation** : Extraction et validation de la version
2. **Build** : Compilation Debug et Release
3. **Tests** : Exécution des tests unitaires
4. **Package** : Création des packages NuGet
5. **Documentation** : Génération avec DocFX
6. **Release** : Création du GitHub Release
7. **Publication** : GitHub Packages
8. **Notification** : Discord webhook

### Utilisation

#### Via Tag Git
```bash
# Créer et pousser un tag pour déclencher la release
git tag v1.0.0-beta
git push origin v1.0.0-beta
```

#### Via Interface GitHub
1. Aller dans **Actions** → **SDK Release Pipeline**
2. Cliquer **Run workflow**  
3. Saisir la version et les options
4. Cliquer **Run workflow**

## 🏗️ Architecture des Packages

### Packages NuGet Générés

```
PerAspera.Core.1.0.0-beta.nupkg          # Utilitaires de base
PerAspera.GameAPI.1.0.0-beta.nupkg       # Wrappers game classes  
PerAspera.ModSDK.1.0.0-beta.nupkg        # SDK principal
```

### Dépendances

```
PerAspera.ModSDK
├── PerAspera.GameAPI
│   └── PerAspera.Core
│       └── BepInEx.Unity.IL2CPP
└── BepInEx.PluginInfoProps
```

## 📊 Suivi des Versions

### Matrice de Compatibilité

| SDK Version | Per Aspera | BepInEx | .NET |
|-------------|------------|---------|------|
| 1.0.x       | 1.5.x      | 6.0.x   | 6.0  |
| 1.1.x       | 1.5.x      | 6.0.x   | 6.0  |

### Cycle de Release

- **Alpha** : Fonctionnalités experimentales
- **Beta** : Fonctionnalités complètes, tests communauté  
- **RC** : Candidate production, tests finaux
- **Stable** : Version production recommandée

## 🔧 Développement Local

### Setup Initial
```powershell
# Clone du repo
git clone https://github.com/PerAsperaMods/PerAspera-SDK.git
cd PerAspera-SDK/SDK

# Afficher la version actuelle
.\Manage-Version.ps1 -Action show

# Build du SDK
.\Manage-Version.ps1 -Action build
```

### Tests Locaux
```powershell
# Build et package pour tests
.\Manage-Version.ps1 -Action package

# Test en dry-run
.\Manage-Version.ps1 -Action bump-minor -DryRun
```

## 📚 Bonnes Pratiques

### Commits
- Utiliser [Conventional Commits](https://www.conventionalcommits.org/)
- `feat:` pour nouvelles fonctionnalités
- `fix:` pour corrections de bugs  
- `chore:` pour maintenance
- `docs:` pour documentation

### Branches
- `main` : Version stable
- `develop` : Intégration continue
- `feature/*` : Développement fonctionnalités
- `hotfix/*` : Corrections urgentes

### Tags
- Toujours prefixer avec `v` : `v1.0.0`
- Pousser tags après commit : `git push --tags`
- Annotés pour releases : `git tag -a v1.0.0 -m "Release 1.0.0"`

## 🚨 Troubleshooting

### Erreurs Communes

#### "Version.props not found"
```powershell
# Vérifier que vous êtes dans le bon dossier
cd F:\ModPeraspera\SDK
.\Manage-Version.ps1 -Action show
```

#### "Build failed"
```powershell
# Nettoyer et rebuild
dotnet clean PerAspera.SDK.sln
dotnet restore PerAspera.SDK.sln
dotnet build PerAspera.SDK.sln --configuration Release
```

#### "Git tag already exists"
```bash
# Supprimer le tag local et distant
git tag -d v1.0.0
git push origin :refs/tags/v1.0.0
```

### Support

- **Issues** : [GitHub Issues](https://github.com/PerAsperaMods/PerAspera-SDK/issues)
- **Discussions** : [GitHub Discussions](https://github.com/PerAsperaMods/PerAspera-SDK/discussions)
- **Wiki** : [Documentation complète](https://github.com/PerAsperaMods/PerAspera-SDK/wiki)