# 📦 Quick Versioning Reference

## 🚀 Fast Track - Most Common Use Cases

### Release a Patch Version (Bug Fixes)
```powershell
cd F:\ModPeraspera\SDK
.\Prepare-Release.ps1 -VersionType patch -Push
```

### Release a Minor Version (New Features)
```powershell
cd F:\ModPeraspera\SDK
.\Prepare-Release.ps1 -VersionType minor -Push
```

### Release a Beta Version
```powershell
.\Prepare-Release.ps1 -VersionType minor -PreRelease beta -Push
```

### Preview Changes Before Applying
```powershell
.\Prepare-Release.ps1 -VersionType patch -DryRun
```

---

## 🤖 GitHub Actions Automated Release

### Via GitHub UI
1. Go to: https://github.com/PerAsperaMods/PerAspera-SDK/actions
2. Click **"SDK Version & Release"**
3. Click **"Run workflow"**
4. Select options and run

### Via GitHub CLI
```powershell
# Patch release
gh workflow run version-release.yml --field version_type=patch --field create_release=true

# Minor beta release
gh workflow run version-release.yml --field version_type=minor --field pre_release=beta --field create_release=true
```

---

## 📋 Version Types

| Command | Current → New | Use When |
|---------|---------------|----------|
| `patch` | 1.2.3 → 1.2.4 | Bug fixes |
| `minor` | 1.2.3 → 1.3.0 | New features |
| `major` | 1.2.3 → 2.0.0 | Breaking changes |
| `custom` | Any → Custom | Specific version needed |

---

## 🏷️ Pre-Release Suffixes

| Suffix | Example | Use When |
|--------|---------|----------|
| `none` | 1.2.0 | Stable release |
| `alpha` | 1.2.0-alpha.123 | Early development |
| `beta` | 1.2.0-beta.45 | Feature complete, testing |
| `rc` | 1.2.0-rc.1 | Release candidate |

---

## 🛠️ Prepare-Release.ps1 Parameters

```powershell
.\Prepare-Release.ps1 `
  -VersionType <patch|minor|major|custom> `
  [-CustomVersion "X.Y.Z"] `
  [-PreRelease <none|alpha|beta|rc>] `
  [-Push] `
  [-DryRun] `
  [-SkipBuild] `
  [-CreateGitHubRelease]
```

| Parameter | Description |
|-----------|-------------|
| `-VersionType` | Type of version bump |
| `-CustomVersion` | Specific version (with `-VersionType custom`) |
| `-PreRelease` | Add pre-release suffix |
| `-Push` | Auto-push to GitHub |
| `-DryRun` | Preview without applying |
| `-SkipBuild` | Skip build validation |
| `-CreateGitHubRelease` | Trigger GitHub Release workflow |

---

## ✅ What Gets Updated

1. ✏️ **Version.props** - `SDKVersion`, `SDKVersionPrefix`, `SDKVersionSuffix`
2. 📝 **CHANGELOG.md** - Replaces `[Unreleased]` with version and date
3. 📌 **Git Tag** - Creates annotated tag `vX.Y.Z`
4. 🔨 **Build** - Validates SDK compiles (unless `-SkipBuild`)

---

## 🔍 Troubleshooting One-Liners

```powershell
# Check current version
Select-String -Path Version.props -Pattern '<SDKVersion>'

# View recent releases
git tag -l | Select-Object -Last 5

# View uncommitted changes
git status

# Force clean state
git reset --hard HEAD
git clean -fd

# Revert last commit (keep changes)
git reset HEAD~1
```

---

## 📚 Full Documentation

For complete details, see [VERSIONING-GUIDE.md](VERSIONING-GUIDE.md)

---

**🚀 Ready? Just run the command and go!**
