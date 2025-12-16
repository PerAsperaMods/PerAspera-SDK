# 📦 PerAspera SDK - Versioning & Release Guide

> **Automated version management and GitHub releases for the PerAspera Modding SDK**

## 🎯 Overview

This guide explains how to manage SDK versions and create releases on GitHub using our automated workflows and scripts.

### Quick Links
- [Version Strategy](#version-strategy)
- [Local Release Preparation](#local-release-preparation)
- [Automated GitHub Releases](#automated-github-releases)
- [Manual Release Process](#manual-release-process)
- [Troubleshooting](#troubleshooting)

---

## 📐 Version Strategy

The SDK follows **[Semantic Versioning (SemVer)](https://semver.org/)**:

```
MAJOR.MINOR.PATCH[-PRERELEASE.BUILD]
```

### Version Components

- **MAJOR**: Breaking changes (incompatible API changes)
- **MINOR**: New features (backward-compatible)
- **PATCH**: Bug fixes (backward-compatible)
- **PRERELEASE**: Optional pre-release identifier (`alpha`, `beta`, `rc`)
- **BUILD**: Auto-incremented build number based on Git commit count

### Examples

| Version | Type | Description |
|---------|------|-------------|
| `1.0.0` | Stable | First stable release |
| `1.1.0` | Minor | New features added |
| `1.1.1` | Patch | Bug fixes |
| `2.0.0` | Major | Breaking changes |
| `1.2.0-beta.123` | Pre-release | Beta version, build 123 |
| `2.0.0-rc.1` | Release Candidate | RC before stable 2.0.0 |

### When to Bump Which Version

**MAJOR (X.0.0)** - Breaking changes:
- Changed public API signatures
- Removed public APIs
- Changed behavior that breaks existing mods
- IL2CPP interop breaking changes

**MINOR (0.X.0)** - New features:
- New public APIs
- New SDK components
- Enhanced functionality (backward-compatible)
- New event types

**PATCH (0.0.X)** - Bug fixes:
- Bug fixes
- Documentation improvements
- Performance improvements
- Internal refactoring

---

## 🚀 Local Release Preparation

### Using the `Prepare-Release.ps1` Script

The recommended way to prepare a release locally:

```powershell
# Navigate to SDK directory
cd F:\ModPeraspera\SDK

# Prepare a patch release (1.1.0 → 1.1.1)
.\Prepare-Release.ps1 -VersionType patch

# Prepare a minor release (1.1.0 → 1.2.0)
.\Prepare-Release.ps1 -VersionType minor

# Prepare a major release (1.1.0 → 2.0.0)
.\Prepare-Release.ps1 -VersionType major

# Prepare a custom version
.\Prepare-Release.ps1 -VersionType custom -CustomVersion "2.0.0-beta.1"

# Prepare with pre-release suffix
.\Prepare-Release.ps1 -VersionType minor -PreRelease beta

# Prepare and push automatically
.\Prepare-Release.ps1 -VersionType patch -Push

# Dry run (preview changes without applying)
.\Prepare-Release.ps1 -VersionType patch -DryRun
```

### Script Parameters

| Parameter | Description | Values | Default |
|-----------|-------------|--------|---------|
| `-VersionType` | Type of version bump | `patch`, `minor`, `major`, `custom` | `patch` |
| `-CustomVersion` | Specific version (required with `-VersionType custom`) | e.g., `2.0.0-beta.1` | - |
| `-PreRelease` | Pre-release identifier | `none`, `alpha`, `beta`, `rc` | `none` |
| `-Push` | Automatically push to remote | - | `false` |
| `-DryRun` | Preview changes without applying | - | `false` |
| `-SkipBuild` | Skip build validation | - | `false` |
| `-CreateGitHubRelease` | Trigger GitHub Release workflow | - | `false` |

### What the Script Does

1. ✅ **Validates Git repository** (branch, uncommitted changes)
2. 📖 **Reads current version** from `Version.props`
3. 🔢 **Computes new version** based on parameters
4. 📝 **Updates `Version.props`** with new version
5. 📋 **Updates `CHANGELOG.md`** with release date
6. 🔨 **Builds and tests** the SDK (optional)
7. 📌 **Creates Git commit** with version bump
8. 🏷️ **Creates Git tag** (`v1.2.0`)
9. 🚀 **Pushes to remote** (if `-Push` specified)

### Example Workflow

```powershell
# 1. Prepare release locally
.\Prepare-Release.ps1 -VersionType minor -DryRun  # Preview changes

# 2. Review the changes
git diff Version.props CHANGELOG.md

# 3. Apply the changes
.\Prepare-Release.ps1 -VersionType minor

# 4. Review commit and tag
git log -1
git tag -l

# 5. Push to GitHub
git push origin main
git push origin v1.2.0
```

---

## 🤖 Automated GitHub Releases

### Workflow: `version-release.yml`

Automatically creates GitHub releases with compiled artifacts, NuGet packages, and changelog.

#### Triggering the Workflow

**Via GitHub Actions UI:**
1. Go to: https://github.com/PerAsperaMods/PerAspera-SDK/actions
2. Select **"SDK Version & Release"** workflow
3. Click **"Run workflow"**
4. Fill in parameters:
   - **Version type**: `patch`, `minor`, `major`, or `custom`
   - **Custom version**: Only if "custom" selected
   - **Pre-release**: `none`, `alpha`, `beta`, or `rc`
   - **Create release**: ✅ Checked
5. Click **"Run workflow"**

**Via GitHub CLI:**
```powershell
# Install GitHub CLI if not already installed
# https://cli.github.com/

# Trigger patch release
gh workflow run version-release.yml \
  --field version_type=patch \
  --field pre_release=none \
  --field create_release=true

# Trigger minor beta release
gh workflow run version-release.yml \
  --field version_type=minor \
  --field pre_release=beta \
  --field create_release=true

# Trigger custom version
gh workflow run version-release.yml \
  --field version_type=custom \
  --field custom_version="2.0.0-rc.1" \
  --field pre_release=rc \
  --field create_release=true
```

#### What the Workflow Does

1. 🔍 **Computes new version** based on inputs
2. 📝 **Updates `Version.props` and `CHANGELOG.md`**
3. 📌 **Commits and tags** the new version
4. 🔨 **Builds all SDK projects** (Debug + Release)
5. 🧪 **Runs tests** (if test projects exist)
6. 📦 **Creates NuGet packages** (`.nupkg` + `.snupkg`)
7. 📚 **Creates release archives**:
   - `PerAspera.SDK-1.2.0-dlls.zip` (DLLs only)
   - `PerAspera.SDK-1.2.0-full.zip` (DLLs + docs + examples)
8. 📋 **Extracts release notes** from CHANGELOG
9. 🚀 **Creates GitHub Release** with all artifacts

#### Release Artifacts

Each release includes:

| Artifact | Description |
|----------|-------------|
| `PerAspera.SDK-X.Y.Z-dlls.zip` | Compiled DLLs only |
| `PerAspera.SDK-X.Y.Z-full.zip` | Full SDK (DLLs + documentation) |
| `PerAspera.Core.X.Y.Z.nupkg` | NuGet package for Core |
| `PerAspera.GameAPI.X.Y.Z.nupkg` | NuGet package for GameAPI |
| `PerAspera.ModSDK.X.Y.Z.nupkg` | NuGet package for ModSDK |
| `*.snupkg` | Symbol packages for debugging |

---

## 🛠️ Manual Release Process

If you prefer manual control over the release process:

### Step 1: Update Version Manually

Edit [`Version.props`](Version.props):

```xml
<PropertyGroup>
  <SDKVersion>1.2.0</SDKVersion>
  <SDKVersionPrefix>1.2.0</SDKVersionPrefix>
  <SDKVersionSuffix></SDKVersionSuffix>
  <!-- ... -->
</PropertyGroup>
```

### Step 2: Update CHANGELOG

Edit [`CHANGELOG.md`](CHANGELOG.md):

```markdown
## [1.2.0] - 2025-01-15

### Added
- New feature XYZ

### Fixed
- Bug ABC

---

## [Unreleased]
...
```

### Step 3: Build and Test

```powershell
dotnet restore PerAspera.SDK.sln
dotnet build PerAspera.SDK.sln --configuration Release
dotnet test PerAspera.SDK.sln --configuration Release
```

### Step 4: Create Git Tag

```powershell
git add Version.props CHANGELOG.md
git commit -m "chore: bump version to 1.2.0"
git tag -a v1.2.0 -m "Release v1.2.0"
git push origin main
git push origin v1.2.0
```

### Step 5: Create GitHub Release

1. Go to: https://github.com/PerAsperaMods/PerAspera-SDK/releases/new
2. Choose tag: `v1.2.0`
3. Title: `PerAspera SDK 1.2.0`
4. Copy release notes from CHANGELOG
5. Upload artifacts:
   - Compile DLLs and zip them
   - Include NuGet packages
6. Publish release

---

## 🔧 Advanced Configuration

### Customizing Version.props

You can customize SDK metadata in `Version.props`:

```xml
<PropertyGroup>
  <!-- Version -->
  <SDKVersion>1.2.0</SDKVersion>
  
  <!-- Metadata -->
  <Authors>PerAspera Modding Community</Authors>
  <Company>PerAsperaMods</Company>
  <Copyright>Copyright © 2024-2025 PerAspera Modding Community</Copyright>
  
  <!-- Compatibility -->
  <GameVersion>1.5.0</GameVersion>
  <BepInExVersion>6.0.0</BepInExVersion>
  
  <!-- NuGet -->
  <PackageLicenseExpression>MIT</PackageLicenseExpression>
  <RepositoryUrl>https://github.com/PerAsperaMods/PerAspera-SDK</RepositoryUrl>
</PropertyGroup>
```

### CHANGELOG Format

Follow [Keep a Changelog](https://keepachangelog.com/) format:

```markdown
# Changelog

## [Unreleased]

### Added
- New features go here

### Changed
- Changes in existing functionality

### Deprecated
- Soon-to-be removed features

### Removed
- Removed features

### Fixed
- Bug fixes

### Security
- Security improvements

---

## [1.2.0] - 2025-01-15

### Added
- Event system improvements

### Fixed
- Memory leak in event dispatcher

---

## [1.1.0] - 2024-12-20
...
```

---

## 🐛 Troubleshooting

### "Version.props not found"
```powershell
# Ensure you're in the SDK directory
cd F:\ModPeraspera\SDK
```

### "Not a git repository"
```powershell
# Ensure .git exists
ls .git
```

### "Uncommitted changes detected"
```powershell
# Commit or stash changes first
git status
git add .
git commit -m "your message"
```

### "Build failed"
```powershell
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build --configuration Release
```

### "Push failed - remote rejected"
```powershell
# Pull latest changes first
git pull origin main --rebase
git push origin main
```

### GitHub Workflow Fails
1. Check workflow logs: https://github.com/PerAsperaMods/PerAspera-SDK/actions
2. Verify `GameLibs-Stripped` cache exists
3. Ensure GitHub token has permissions
4. Check for build errors in logs

---

## 📚 Additional Resources

- **[Semantic Versioning](https://semver.org/)** - Version numbering standard
- **[Keep a Changelog](https://keepachangelog.com/)** - Changelog format
- **[GitHub Releases](https://docs.github.com/en/repositories/releasing-projects-on-github)** - Official documentation
- **[GitHub CLI](https://cli.github.com/)** - Command-line GitHub tools

---

## 🤝 Contributing

When contributing version changes:

1. **Always update CHANGELOG.md** with your changes
2. **Follow SemVer** for version numbers
3. **Test builds locally** before releasing
4. **Use descriptive commit messages**: `chore: bump version to X.Y.Z`
5. **Create annotated tags**: `git tag -a vX.Y.Z -m "Release vX.Y.Z"`

---

## 📝 Quick Reference

### Common Commands

```powershell
# Patch release (bug fixes)
.\Prepare-Release.ps1 -VersionType patch -Push

# Minor release (new features)
.\Prepare-Release.ps1 -VersionType minor -Push

# Major release (breaking changes)
.\Prepare-Release.ps1 -VersionType major -Push

# Beta release
.\Prepare-Release.ps1 -VersionType minor -PreRelease beta -Push

# Preview changes without applying
.\Prepare-Release.ps1 -VersionType patch -DryRun

# GitHub automated release
gh workflow run version-release.yml \
  --field version_type=patch \
  --field pre_release=none \
  --field create_release=true
```

---

**🚀 Happy releasing!**

For questions or issues, please open a [GitHub Issue](https://github.com/PerAsperaMods/PerAspera-SDK/issues).
