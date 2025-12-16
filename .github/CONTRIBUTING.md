## 🚀 Guide de Contribution - Mods Per Aspera

Merci de votre intérêt pour contribuer aux mods Per Aspera ! Ce guide vous aidera à contribuer efficacement au projet.

### 🎯 Types de contributions

- 🐛 **Correction de bugs** - Corrections dans les mods existants
- ✨ **Nouvelles fonctionnalités** - Ajout de nouvelles capacités
- 📚 **Documentation** - Amélioration des guides et exemples
- 🔧 **Outils de développement** - Automatisation et CI/CD
- 🎨 **Assets** - Modifications YAML, textures, données de jeu

### 🛠️ Prérequis

#### Environnement de développement
- **Visual Studio 2022** ou **VS Code** avec extension C#
- **.NET 6.0 SDK** ou **.NET Framework 4.7.2**
- **Per Aspera** installé avec **BepInEx 6.x IL2CPP**
- **Git** pour le versioning

#### Connaissances recommandées
- **C#** - Langage principal des mods
- **Unity** - Moteur de jeu Per Aspera
- **HarmonyX** - Framework de patching
- **YAML** - Format des données du jeu
- **IL2CPP** - Compilation Unity vers C++

### 📁 Structure du projet

```
PerAsperaMod/
├── .github/                    # GitHub Actions et templates
├── Documentation/              # Documentation technique
├── Common/                     # Utilitaires partagés
├── AsperaClass/               # Classes de base du jeu
├── [ModName]/                 # Projets de mods individuels
│   ├── bin/Release/           # Assemblies compilés
│   └── [ModName].csproj       # Configuration projet
└── working/                   # Mod principal de développement
```

### 🔄 Processus de contribution

#### 1. Préparation
```bash
# Fork et clone le repository
git clone https://github.com/VOTRE_USERNAME/PerAsperaMod.git
cd PerAsperaMod

# Créer une branche pour votre contribution
git checkout -b feature/nom-de-votre-fonctionnalite
```

#### 2. Configuration de l'environnement
```bash
# Restaurer les dépendances
dotnet restore PerAsperaMod.sln

# Configurer le chemin du jeu (modifier dans les .csproj)
# <GameDir>F:\ModPeraspera\Per Aspera</GameDir>
```

#### 3. Développement

##### Pour un nouveau mod :
1. **Créer le projet** :
   ```bash
   # Copier un projet existant comme template
   cp -r working/ MonNouveauMod/
   # Renommer les fichiers et classes appropriés
   ```

2. **Configurer le projet** :
   - Modifier `MonNouveauMod.csproj`
   - Ajouter le projet à `PerAsperaMod.sln`
   - Implémenter `BasePlugin` de BepInEx

3. **Structure de code recommandée** :
   ```csharp
   [BepInPlugin(GUID, PluginName, PluginVersion)]
   public class MonNouveauModPlugin : BasePlugin
   {
       public const string GUID = "com.votreusername.monnouveaumod";
       public const string PluginName = "Mon Nouveau Mod";
       public const string PluginVersion = "1.0.0";
       
       public override void Load()
       {
           // Initialisation du mod
           LogSource.LogInfo($"{PluginName} chargé !");
       }
   }
   ```

##### Pour modifier un mod existant :
1. **Analyser le code existant**
2. **Créer des patches Harmony** :
   ```csharp
   [HarmonyPatch(typeof(ClasseACibler), "MethodeACibler")]
   public static class PatchMethode
   {
       static void Postfix(ClasseACibler __instance, ref TypeRetour __result)
       {
           // Votre modification
       }
   }
   ```

#### 4. Tests
```bash
# Compiler le projet
dotnet build MonNouveauMod/ --configuration Release

# Le mod sera automatiquement copié vers BepInEx/plugins/
# Tester dans le jeu et vérifier les logs BepInEx/LogOutput.log
```

#### 5. Documentation
- **Mettre à jour README** si nécessaire
- **Documenter les nouvelles APIs** 
- **Ajouter exemples d'utilisation**
- **Expliquer les modifications de gameplay**

### 📝 Standards de code

#### Conventions de nommage
```csharp
// Classes : PascalCase
public class MonModPlugin : BasePlugin { }

// Méthodes : PascalCase  
public void InitialiserMod() { }

// Variables : camelCase
private ManualLogSource logSource;

// Constantes : UPPER_CASE
public const string MOD_GUID = "com.example.mod";
```

#### Structure des patches Harmony
```csharp
[BepInDependency("com.aspera.common")] // Dépendances
[BepInPlugin(GUID, PluginName, PluginVersion)]
public class MonModPlugin : BasePlugin
{
    internal static ManualLogSource Logger;
    
    public override void Load()
    {
        Logger = LogSource;
        
        // Appliquer les patches
        Harmony.CreateAndPatchAll(typeof(MonModPlugin));
        
        Logger.LogInfo("Mod chargé avec succès !");
    }
}

[HarmonyPatch(typeof(ClasseCible))]
public static class PatchesClasseCible
{
    [HarmonyPatch("MethodeCiblee")]
    [HarmonyPostfix]
    static void MethodeCiblee_Postfix(/* paramètres */)
    {
        MonModPlugin.Logger.LogDebug("Patch appliqué");
    }
}
```

### 🧪 Tests et validation

#### Tests locaux
1. **Compilation sans erreurs**
2. **Chargement du mod dans BepInEx**
3. **Fonctionnalité testée en jeu** 
4. **Pas de régression sur autres mods**
5. **Performance acceptable**

#### CI/CD automatique
Les GitHub Actions valideront automatiquement :
- ✅ **Compilation** de tous les projets
- ✅ **Validation YAML** des fichiers de données
- ✅ **Analyse de sécurité** du code
- ✅ **Tests automatiques** (si présents)

### 📤 Soumettre une Pull Request

#### Checklist avant soumission
- [ ] **Code testé** localement
- [ ] **Documentation** mise à jour
- [ ] **Commit messages** clairs
- [ ] **Pas de fichiers** de build/cache committs
- [ ] **Respect des conventions** de nommage

#### Template de Pull Request
```markdown
## Description
Brief description de vos changements

## Type de changement
- [ ] Bug fix
- [ ] Nouvelle fonctionnalité
- [ ] Breaking change
- [ ] Documentation

## Mod concerné
- [ ] AtmosphereRelease
- [ ] MasterGui
- [ ] [Autre mod]

## Tests effectués
- [ ] Compilation locale
- [ ] Test en jeu
- [ ] Compatibilité avec autres mods

## Screenshots/Logs
Si applicable, ajoutez des captures ou logs

## Checklist
- [ ] Code suit les conventions
- [ ] Documentation mise à jour
- [ ] Tests passent localement
```

### 🤝 Support et aide

#### Canaux de support
1. **[Issues GitHub](../../issues)** - Pour bugs et questions
2. **[Discussions](../../discussions)** - Pour idées et aide générale
3. **Assistant automatique** - Analyse automatique des issues

#### Obtenir de l'aide
- 🏷️ **Labellez vos issues** avec `mod-help`
- 🤖 **Mentionnez `@mod-assistant`** pour aide automatique
- 📋 **Fournissez les logs** BepInEx complets
- 🔍 **Décrivez votre environnement** (OS, versions)

### 📚 Ressources utiles

- **[Documentation technique](../Documentation/)** - Guides détaillés
- **[Exemples de code](../working/)** - Templates et références
- **[BepInEx Documentation](https://docs.bepinex.dev/)** - Framework de modding
- **[Harmony Documentation](https://harmony.pardeike.net/)** - Patching avancé
- **[Unity Scripting API](https://docs.unity3d.com/ScriptReference/)** - APIs Unity

---

**Merci de contribuer au modding Per Aspera ! 🚀**

*Ce projet suit le [Code of Conduct](CODE_OF_CONDUCT.md). En contribuant, vous acceptez de respecter ces règles.*