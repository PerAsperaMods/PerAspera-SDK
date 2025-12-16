# PerAspera SDK Archiving System - Quick Reference

## 🎯 Purpose

Archive system for preserving SDK releases with full binaries, packages, and documentation.

## 📁 Structure

```
SDK/
├── _Archive/                    # All archived versions
│   ├── README.md               # Archive documentation
│   ├── v1.0.0-beta/           # Specific version archive
│   │   ├── bin/               # Compiled DLLs
│   │   ├── packages/          # NuGet packages
│   │   ├── docs/              # Documentation snapshot
│   │   └── VERSION-INFO.md    # Build metadata
│   └── PerAspera-SDK-*.zip    # Compressed archives
├── Archive-SDK.ps1            # Archiving script
└── ARCHIVING-GUIDE.md         # Full documentation
```

## 🚀 Quick Commands

### Create Archive

```powershell
# Auto-detect version from Version.props
.\SDK\Archive-SDK.ps1

# Specify version
.\SDK\Archive-SDK.ps1 -Version "1.1.0"

# Force overwrite existing
.\SDK\Archive-SDK.ps1 -Force
```

### Build + Archive

```powershell
# Build Release and archive
.\SDK\Manage-Version.ps1 -Action build -ArchiveAfterBuild

# Package and archive
.\SDK\Manage-Version.ps1 -Action package -ArchiveAfterBuild

# Archive only (standalone)
.\SDK\Manage-Version.ps1 -Action archive
```

### VS Code Tasks

Press `Ctrl+Shift+P` → "Run Task":
- **SDK: Archive Current Version** - Create archive now
- **SDK: Build and Archive** - Build Release + archive
- **SDK: Package and Archive** - Create NuGet + archive
- **SDK: List Archives** - Show all archived versions

## 📊 List Archives

```powershell
Get-ChildItem SDK\_Archive -Directory | 
    ForEach-Object {
        $size = (Get-ChildItem $_.FullName -Recurse -File | 
                 Measure-Object -Property Length -Sum).Sum
        [PSCustomObject]@{
            Version = $_.Name
            'Size (MB)' = [math]::Round($size / 1MB, 2)
            Created = $_.CreationTime
        }
    } | Format-Table -AutoSize
```

## 🔄 Restore Archive

```powershell
# Backup current SDK
Move-Item SDK\bin SDK\bin.backup -Force

# Restore from archive
Copy-Item SDK\_Archive\v1.0.0-beta\bin\* SDK\bin\ -Recurse -Force
```

## 📦 What Gets Archived

- ✅ **Binaries**: All DLLs from `bin/Release/`
- ✅ **Packages**: NuGet `.nupkg` files
- ✅ **Documentation**: CHANGELOG, README, guides
- ✅ **Metadata**: Build info, Git commit, file list
- ✅ **ZIP**: Compressed archive for distribution

## 🎯 When to Archive

✅ **Archive when:**
- Creating major/minor releases
- Before breaking changes
- CI/CD successful release
- Quarterly for LTS versions

❌ **Don't archive:**
- Debug builds
- Every commit
- Failed builds
- Experimental branches

## 📚 Documentation

- [ARCHIVING-GUIDE.md](ARCHIVING-GUIDE.md) - Complete guide
- [VERSION-GUIDE.md](SDK/VERSION-GUIDE.md) - Versioning strategy
- [RELEASE-WORKFLOW.md](SDK/RELEASE-WORKFLOW.md) - Release process

## 🆘 Troubleshooting

**No binaries to archive?**
```powershell
# Build Release first
.\SDK\Build-SDK.ps1 Release
# Then archive
.\SDK\Archive-SDK.ps1
```

**Archive too large?**
```powershell
# Binaries only
.\SDK\Archive-SDK.ps1 -IncludeBinaries -IncludePackages:$false -IncludeDocumentation:$false
```

---

**Last Updated**: 2024-12-16  
**Current SDK Version**: 1.1.0
