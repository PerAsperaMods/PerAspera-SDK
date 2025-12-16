# SDK Archiving Guide

## 📦 Overview

The PerAspera SDK uses a comprehensive archiving system to preserve release versions for:
- **Rollback capability** - Revert to previous versions if needed
- **Version comparison** - Analyze changes between releases
- **Historical reference** - Document SDK evolution
- **Distribution** - Share specific stable versions with modders

## 🏗️ Archive Structure

```
SDK/
├── _Archive/                          # Archive root directory
│   ├── README.md                      # Archive documentation
│   ├── v1.0.0-beta/                  # Archived version
│   │   ├── bin/                       # Compiled binaries
│   │   │   ├── net6.0/
│   │   │   │   ├── PerAspera.Core.dll
│   │   │   │   ├── PerAspera.GameAPI.dll
│   │   │   │   └── PerAspera.ModSDK.dll
│   │   ├── packages/                  # NuGet packages
│   │   │   └── PerAspera.ModSDK.1.0.0-beta.nupkg
│   │   ├── docs/                      # Documentation snapshot
│   │   │   ├── CHANGELOG.md
│   │   │   ├── README.md
│   │   │   └── Documentation/
│   │   └── VERSION-INFO.md            # Build metadata
│   ├── v1.1.0/                       # Next version...
│   └── PerAspera-SDK-v1.0.0-beta.zip # Compressed archive
├── Archive-SDK.ps1                   # Archiving script
└── ...
```

## 🚀 Creating Archives

### Manual Archiving

Archive the current SDK version:

```powershell
# Basic archive (auto-detects version from Version.props)
.\Archive-SDK.ps1

# Archive specific version
.\Archive-SDK.ps1 -Version "1.1.0"

# Archive with options
.\Archive-SDK.ps1 -IncludeBinaries -IncludePackages -IncludeDocumentation

# Force overwrite existing archive
.\Archive-SDK.ps1 -Force
```

### Automatic Archiving

Archive automatically after builds:

```powershell
# Build and archive
.\Manage-Version.ps1 -Action build -ArchiveAfterBuild

# Package and archive
.\Manage-Version.ps1 -Action package -ArchiveAfterBuild

# Archive as standalone action
.\Manage-Version.ps1 -Action archive
```

### CI/CD Integration

Archives are created automatically during GitHub releases:

```yaml
- name: Archive SDK Release
  run: .\SDK\Archive-SDK.ps1 -Version ${{ env.VERSION }}
  
- name: Upload Archive Artifact
  uses: actions/upload-artifact@v3
  with:
    name: SDK-Archive-v${{ env.VERSION }}
    path: SDK/_Archive/v${{ env.VERSION }}
```

## 📋 What Gets Archived

### Binaries (`bin/`)
- `PerAspera.Core.dll` - Core utilities and IL2CPP extensions
- `PerAspera.GameAPI.dll` - Game class wrappers and mirrors
- `PerAspera.ModSDK.dll` - High-level modding SDK
- All dependencies and PDB files (Debug builds)

### Packages (`packages/`)
- `PerAspera.ModSDK.[version].nupkg` - Main SDK package
- `PerAspera.Core.[version].nupkg` - Core package (optional)
- `PerAspera.GameAPI.[version].nupkg` - GameAPI package (optional)

### Documentation (`docs/`)
- `CHANGELOG.md` - Version-specific changes
- `README.md` - SDK documentation
- `VERSION-GUIDE.md` - Versioning strategy
- `RELEASE-WORKFLOW.md` - Release process
- `GAME-EVENTS-REFERENCE.md` - Event system documentation
- `Documentation/` - Full documentation folder

### Metadata
- `VERSION-INFO.md` - Build information, Git commit, file list

## 🔄 Using Archived Versions

### Restore Archived SDK

```powershell
# Backup current SDK
Move-Item SDK\bin SDK\bin.backup -Force

# Restore from archive
Copy-Item SDK\_Archive\v1.0.0-beta\bin\* SDK\bin\ -Recurse -Force

# Update Version.props manually to match
```

### Reference Archived DLLs in Mods

```xml
<!-- YourMod.csproj -->
<ItemGroup>
  <Reference Include="PerAspera.ModSDK">
    <HintPath>..\..\SDK\_Archive\v1.0.0-beta\bin\net6.0\PerAspera.ModSDK.dll</HintPath>
  </Reference>
</ItemGroup>
```

### Install Archived NuGet Package

```powershell
# Add local package source
dotnet nuget add source F:\ModPeraspera\SDK\_Archive\v1.0.0-beta\packages

# Install specific version
dotnet add package PerAspera.ModSDK --version 1.0.0-beta
```

## 📊 Archive Management

### Retention Policy

- **Current major version**: Keep all minor/patch releases
- **Previous major version**: Keep last 3 minor releases
- **Older versions**: Keep major releases only

Example for SDK v2.x.x:
- Keep: v2.0.0, v2.1.0, v2.1.1, v2.2.0 (all v2.x)
- Keep: v1.9.0, v1.10.0, v1.11.0 (last 3 of v1.x)
- Keep: v1.0.0 (first v1.x release)
- Archive: Older pre-releases (move to external storage)

### Cleaning Old Archives

```powershell
# List all archives by size
Get-ChildItem SDK\_Archive -Directory | 
    ForEach-Object {
        $size = (Get-ChildItem $_.FullName -Recurse | 
                 Measure-Object -Property Length -Sum).Sum
        [PSCustomObject]@{
            Version = $_.Name
            SizeMB = [math]::Round($size / 1MB, 2)
            Date = $_.CreationTime
        }
    } | Sort-Object Date -Descending

# Remove specific archive
Remove-Item SDK\_Archive\v0.9.0-alpha -Recurse -Force
```

## 🎯 Best Practices

### When to Archive

✅ **DO Archive:**
- Before major/minor version bumps
- After successful CI/CD releases
- Before breaking changes
- Quarterly for LTS versions

❌ **DON'T Archive:**
- Every commit or debug build
- Failed builds
- Experimental branches
- Patch versions (unless critical)

### Archive Naming

- Use semantic versioning: `v1.2.3`, `v2.0.0-beta.1`
- Prefix with `v`: `v1.0.0` not `1.0.0`
- Match Git tags exactly

### Storage Optimization

```powershell
# Compress large archives
Compress-Archive -Path SDK\_Archive\v1.1.0\* `
                 -DestinationPath SDK\_Archive\PerAspera-SDK-v1.1.0.zip

# Remove uncompressed folder after verification
Remove-Item SDK\_Archive\v1.1.0 -Recurse -Force
```

## 🔍 Version Comparison

Compare two archived versions:

```powershell
# List DLL differences
$v1 = "v1.0.0-beta"
$v2 = "v1.1.0"

$files1 = Get-ChildItem "SDK\_Archive\$v1\bin" -Recurse -File
$files2 = Get-ChildItem "SDK\_Archive\$v2\bin" -Recurse -File

Compare-Object $files1.Name $files2.Name
```

## 📚 Additional Resources

- [Versioning Guide](VERSION-GUIDE.md) - Semantic versioning strategy
- [Release Workflow](RELEASE-WORKFLOW.md) - Complete release process
- [Changelog](CHANGELOG.md) - Version history

## 🆘 Troubleshooting

### Archive Creation Fails

```powershell
# Check if Release build exists
Test-Path SDK\bin\Release  # Should return True

# Rebuild SDK first
.\Build-SDK.ps1 Release

# Then archive
.\Archive-SDK.ps1
```

### Archive Too Large

```powershell
# Archive binaries only (no docs)
.\Archive-SDK.ps1 -IncludeBinaries -IncludePackages:$false -IncludeDocumentation:$false

# Or exclude PDB files
Get-ChildItem SDK\_Archive\v1.1.0 -Filter "*.pdb" -Recurse | Remove-Item
```

### Missing Version.props

```powershell
# Manually specify version
.\Archive-SDK.ps1 -Version "1.1.0"
```

---

**Last Updated**: 2024-12-16  
**SDK Version**: 1.1.0
