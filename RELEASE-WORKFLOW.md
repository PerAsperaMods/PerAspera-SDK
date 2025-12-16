# 🏷️ SDK Release Workflow - Complete Example

This guide demonstrates a complete release workflow for the PerAspera SDK.

## 📋 Preparation Checklist

Before starting a release:

- [ ] All features implemented and tested
- [ ] Documentation updated
- [ ] CHANGELOG.md entries added
- [ ] All tests passing
- [ ] Git working directory clean

## 🔄 Release Process

### 1. Pre-Release (Beta Testing)

```powershell
# Switch to develop branch
git checkout develop
git pull origin develop

# Create a pre-release for community testing
.\Manage-Version.ps1 -Action pre-release -PreReleaseType beta
# Version: 1.0.0-beta or 1.0.0-beta.2

# Commit version changes
git add Version.props CHANGELOG.md
git commit -m "chore: bump to v1.0.0-beta"
git push origin develop

# Create and push tag (triggers GitHub Actions)
git tag v1.0.0-beta
git push origin v1.0.0-beta
```

**Result**: GitHub Actions creates:
- ✅ Automated build and tests
- ✅ NuGet packages (.nupkg + .snupkg)
- ✅ GitHub Release (marked as pre-release)
- ✅ Documentation deployment

### 2. Release Candidate

```powershell
# After beta testing, create RC
.\Manage-Version.ps1 -Action pre-release -PreReleaseType rc
# Version: 1.0.0-rc

git add Version.props CHANGELOG.md
git commit -m "chore: bump to v1.0.0-rc"
git tag v1.0.0-rc
git push origin develop --tags
```

### 3. Stable Release

```powershell
# Merge to main branch
git checkout main
git merge develop

# Promote to stable version
.\Manage-Version.ps1 -Action stable
# Version: 1.0.0

git add Version.props CHANGELOG.md
git commit -m "chore: release v1.0.0"
git tag v1.0.0
git push origin main --tags
```

## 🔧 Hotfix Process

For critical bugs in production:

```powershell
# Create hotfix branch from main
git checkout main
git checkout -b hotfix/critical-fix

# Make the fix
# ... code changes ...

# Test the fix
.\Manage-Version.ps1 -Action build
# ... manual testing ...

# Bump patch version
.\Manage-Version.ps1 -Action bump-patch
# Version: 1.0.0 → 1.0.1

git add .
git commit -m "fix: critical bug in resource calculation"
git checkout main
git merge hotfix/critical-fix
git tag v1.0.1
git push origin main --tags

# Merge back to develop
git checkout develop
git merge main
git push origin develop

# Delete hotfix branch
git branch -d hotfix/critical-fix
```

## 📦 Manual Package Creation

For local testing or manual deployment:

```powershell
# Build and create packages locally
.\Manage-Version.ps1 -Action package

# Packages are created in .\packages\
# PerAspera.Core.1.0.0-beta.nupkg
# PerAspera.GameAPI.1.0.0-beta.nupkg  
# PerAspera.ModSDK.1.0.0-beta.nupkg

# Test locally by copying to local NuGet feed
```

## 🚀 GitHub Actions Triggers

### Automatic Triggers

| Trigger | Action | Result |
|---------|---------|---------|
| `git push origin v*.*.*` | Tag push | Full release pipeline |
| Manual dispatch | GitHub UI | Custom version release |

### Manual GitHub Release

1. Go to **Actions** → **SDK Release Pipeline**
2. Click **Run workflow**
3. Enter version (e.g., `1.0.1`)
4. Select if pre-release
5. Click **Run workflow**

## 📊 Version Strategy

### Semantic Versioning Rules

| Change Type | Version Bump | Example |
|-------------|--------------|---------|
| Breaking API changes | Major | 1.0.0 → 2.0.0 |
| New features | Minor | 1.0.0 → 1.1.0 |
| Bug fixes | Patch | 1.0.0 → 1.0.1 |

### Pre-release Progression

```
1.0.0-alpha → 1.0.0-beta → 1.0.0-rc → 1.0.0
     ↓            ↓            ↓         ↓
 Early dev    Community    Final     Production
              testing     testing    ready
```

## 🔍 Quality Gates

### Automated Checks

- ✅ **Build Success**: All projects compile
- ✅ **Test Pass**: Unit tests execute successfully
- ✅ **Package Create**: NuGet packages generate correctly
- ✅ **Documentation**: XML docs and wiki build

### Manual Verification

Before each release:
- [ ] SDK examples work correctly
- [ ] Breaking changes documented
- [ ] Migration guide updated
- [ ] Performance regression tests
- [ ] Community feedback addressed

## 📈 Post-Release Tasks

### After Stable Release

1. **Announce Release**
   ```powershell
   # Discord notification (automatic via GitHub Actions)
   # Update documentation
   # Notify Steam Workshop community
   ```

2. **Update Dependencies**
   ```powershell
   # Update example projects to use new version
   # Update mod templates
   # Update documentation references
   ```

3. **Plan Next Release**
   ```powershell
   # Update roadmap
   # Create milestone for next version
   # Plan breaking changes (if any)
   ```

## 🐛 Rollback Procedure

If critical issues are discovered:

### 1. Quick Hotfix (Recommended)
```powershell
# Create immediate patch
.\Manage-Version.ps1 -Action bump-patch
# Fix and release v1.0.1
```

### 2. Version Rollback (Emergency)
```powershell
# Revert to previous stable tag
git checkout v0.9.0
git tag v1.0.1 # New patch over old stable
git push origin v1.0.1
```

### 3. Package Removal
```powershell
# Remove broken packages from GitHub Packages
# Notify community via Discord/GitHub
# Update documentation with rollback notes
```

## 📞 Support Channels

| Issue Type | Channel | Response Time |
|------------|---------|---------------|
| Bug reports | GitHub Issues | 24-48h |
| Feature requests | GitHub Discussions | Weekly review |
| Usage questions | Discord | Community driven |
| Critical security | Email maintainers | Immediate |

## 📝 Release Checklist Template

Copy this checklist for each release:

```markdown
## Release v1.0.0 Checklist

### Pre-Release
- [ ] All features implemented
- [ ] Tests passing
- [ ] Documentation updated
- [ ] CHANGELOG.md updated
- [ ] Version bumped
- [ ] Git working directory clean

### Release
- [ ] Tag created and pushed
- [ ] GitHub Actions completed successfully
- [ ] Packages available in GitHub Packages
- [ ] Release notes published
- [ ] Documentation deployed

### Post-Release  
- [ ] Community notified
- [ ] Examples updated
- [ ] Next milestone created
- [ ] Feedback collected
```

---

**This workflow ensures consistent, reliable releases of the PerAspera SDK.** 🚀