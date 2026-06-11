# 🚀 Scripts de Déploiement SDK

Ce dossier contient les scripts pour déployer automatiquement les DLLs du SDK Per Aspera vers le dossier BepInX plugins.

## 📂 Structure des DLLs

Les DLLs du SDK sont générées et copiées dans `../SDK_DLL/` puis déployées dans :
```
BepInX/plugins/SDK/
├── PerAspera.Core.dll
├── PerAspera.Core.IL2CppExtensions.dll  
├── PerAspera.GameAPI.dll
├── PerAspera.GameAPI.Climate.dll
├── PerAspera.GameAPI.Commands.dll
├── PerAspera.GameAPI.Events.dll
├── PerAspera.GameAPI.Overrides.dll
├── PerAspera.GameAPI.Wrappers.dll
└── PerAspera.ModSDK.dll
```

## ⚡ Tâches VS Code (Recommandé)

Utilisez **Ctrl+Shift+P** → **Tasks: Run Task** pour accéder aux tâches :

### **SDK: Deploy DLLs to BepInX**
Déploiement rapide avec comparaison de dates. Ignore les DLLs déjà à jour.
- 🏃‍♂️ Rapide et efficace
- 📅 Compare les dates de modification
- 🗂️ Crée automatiquement le dossier `SDK/`

### **SDK: Deploy DLLs to BepInX (Force)**
Force le déploiement de toutes les DLLs même si déjà à jour.
- 💪 Déploiement forcé
- ✅ Garantit la synchronisation complète
- 🔄 Utile après changements de configuration

### **SDK: Deploy DLLs (Complete)**
Déploiement complet avec backup et validation détaillée.
- 📦 Backup automatique des anciennes DLLs
- 🔍 Validation post-déploiement
- 📊 Rapport détaillé avec statistiques
- 🎨 Interface colorée et informative

### **SDK: Build and Deploy**
Workflow complet : build + déploiement automatique.
- 🔧 Build du SDK en premier
- 🚀 Déploiement automatique si build réussi
- 📈 Workflow de développement optimal

## 🛠️ Scripts Disponibles

### Scripts PowerShell

#### `Deploy-SDK-DLLs.ps1` (Complet)
Script principal avec toutes les fonctionnalités :
```powershell
# Déploiement standard
.\Deploy-SDK-DLLs.ps1

# Force le déploiement même si les DLLs sont à jour
.\Deploy-SDK-DLLs.ps1 -Force

# Mode verbose pour plus de détails
.\Deploy-SDK-DLLs.ps1 -Verbose

# Sans backup automatique
.\Deploy-SDK-DLLs.ps1 -BackupOld:$false
```

**Fonctionnalités :**
- ✅ Validation des chemins et de la configuration
- 📦 Backup automatique des anciennes DLLs
- 🔍 Comparaison des dates de modification
- 📊 Résumé détaillé avec statistiques
- 🛡️ Validation post-déploiement
- 🎨 Interface colorée et informative

#### `Deploy-SDK-Quick.ps1` (Simplifié)
Version rapide et légère :
```powershell
# Déploiement rapide
.\Deploy-SDK-Quick.ps1

# Force le déploiement
.\Deploy-SDK-Quick.ps1 -Force
```

**Fonctionnalités :**
- ⚡ Déploiement rapide sans backup
- 🎯 Validation minimale
- 📋 Sortie concise

#### `Build-SDK.ps1` (Modifié)
Script de build avec déploiement intégré :
```powershell
# Build simple
.\Build-SDK.ps1

# Build Release
.\Build-SDK.ps1 Release

# Build et déploiement automatique
.\Build-SDK.ps1 -Deploy

# Build Release avec déploiement forcé
.\Build-SDK.ps1 Release -Deploy -Force
```

### Scripts Batch

#### `Deploy-SDK.bat` (Déploiement seul)
```batch
REM Déploiement normal
Deploy-SDK.bat

REM Déploiement forcé
Deploy-SDK.bat --force
```

#### `Build-And-Deploy.bat` (Combiné)
```batch
REM Build Debug + déploiement
Build-And-Deploy.bat

REM Build Release + déploiement  
Build-And-Deploy.bat Release

REM Build avec déploiement forcé
Build-And-Deploy.bat --force

REM Build sans déploiement
Build-And-Deploy.bat --no-deploy
```

## 🔧 Configuration

Les scripts utilisent la configuration dans `../modding-config.json` :

```json
{
  "paths": {
    "modDevelopmentRoot": "F:\\ModPeraspera",
    "bepInExPluginsDirectory": "F:\\SteamLibrary\\steamapps\\common\\Per Aspera\\BepInEx\\plugins"
  }
}
```

## 📋 Workflow Recommandé

### Développement Actif (Tâches VS Code) 
1. **Build + Déploiement** : `Ctrl+Shift+P` → **SDK: Build and Deploy**
2. **Lancer Per Aspera** pour tester
3. **Répéter** le cycle

### Déploiement Rapide (Tâches VS Code)
1. **Déploiement standard** : `Ctrl+Shift+P` → **SDK: Deploy DLLs to BepInX**  
2. **Déploiement forcé** : `Ctrl+Shift+P` → **SDK: Deploy DLLs to BepInX (Force)**

### Alternative Scripts Manuels
1. **Déploiement rapide** : `Deploy-SDK.bat` (double-clic)
2. **Ou via PowerShell** : `.\Deploy-SDK-Quick.ps1`

### Déploiement Sécurisé (Avec backups)
1. **Déploiement complet** : `Ctrl+Shift+P` → **SDK: Deploy DLLs (Complete)**
2. **Vérification** des backups créés
3. **Test** en jeu

## 🚨 Dépannage

### "Dossier SDK_DLL non trouvé"
- Vérifiez que les DLLs ont été générées : buildez d'abord le SDK
- Vérifiez le chemin dans `modding-config.json`

### "Dossier BepInX plugins non trouvé"  
- Vérifiez l'installation de BepInX dans Per Aspera
- Vérifiez le chemin `bepInExPluginsDirectory` dans la configuration

### "Accès refusé"
- Fermez Per Aspera avant le déploiement
- Lancez en tant qu'administrateur si nécessaire
- Vérifiez les permissions sur le dossier BepInX

## 📊 Statut des Backups

Le script complet crée automatiquement des backups dans :
```
F:\SteamLibrary\steamapps\common\Per Aspera\BepInEx\plugins\_sdk_backup_YYYYMMDD_HHMMSS\
```

Pour restaurer une version précédente, copiez manuellement les DLLs depuis un backup.

## 💡 Tips

- **Double-clic** sur les fichiers `.bat` pour une utilisation rapide
- Utilisez **PowerShell ISE** ou **VS Code** pour modifier les scripts
- Activez le **mode verbose** pour diagnostiquer les problèmes
- Les **dates de modification** sont comparées pour éviter les copies inutiles