# PerAspera SDK - Release Preparation Script
# Automate version bump, CHANGELOG update, and release preparation

param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("patch", "minor", "major", "custom")]
    [string]$VersionType = "patch",
    
    [Parameter(Mandatory = $false)]
    [string]$CustomVersion,
    
    [Parameter(Mandatory = $false)]
    [ValidateSet("none", "alpha", "beta", "rc")]
    [string]$PreRelease = "none",
    
    [Parameter(Mandatory = $false)]
    [switch]$Push = $false,
    
    [Parameter(Mandatory = $false)]
    [switch]$DryRun = $false,
    
    [Parameter(Mandatory = $false)]
    [switch]$SkipBuild = $false,
    
    [Parameter(Mandatory = $false)]
    [switch]$CreateGitHubRelease = $false
)

$ErrorActionPreference = "Stop"
$SDKRoot = $PSScriptRoot
$VersionFile = Join-Path $SDKRoot "Version.props"
$ChangelogFile = Join-Path $SDKRoot "CHANGELOG.md"

# Colors for output
function Write-Header { param($Text) Write-Host "`n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan; Write-Host "  $Text" -ForegroundColor Cyan; Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`n" -ForegroundColor Cyan }
function Write-Step { param($Text) Write-Host "▶ $Text" -ForegroundColor Green }
function Write-Info { param($Text) Write-Host "  ℹ $Text" -ForegroundColor Blue }
function Write-Warning { param($Text) Write-Host "  ⚠ $Text" -ForegroundColor Yellow }
function Write-Error { param($Text) Write-Host "  ✖ $Text" -ForegroundColor Red }
function Write-Success { param($Text) Write-Host "  ✔ $Text" -ForegroundColor Green }

Write-Header "PerAspera SDK - Release Preparation"

# ================================
# 1. Validate Git Repository
# ================================
Write-Step "Validating Git repository..."

if (-not (Test-Path ".git")) {
    Write-Error "Not a git repository!"
    exit 1
}

# Check for uncommitted changes
$gitStatus = git status --porcelain
if ($gitStatus -and -not $DryRun) {
    Write-Warning "Uncommitted changes detected:"
    Write-Host $gitStatus
    $continue = Read-Host "Continue anyway? (y/N)"
    if ($continue -ne "y") {
        Write-Info "Aborted by user"
        exit 0
    }
}

# Check current branch
$currentBranch = git rev-parse --abbrev-ref HEAD
Write-Info "Current branch: $currentBranch"

if ($currentBranch -ne "main" -and -not $DryRun) {
    Write-Warning "Not on 'main' branch!"
    $continue = Read-Host "Continue anyway? (y/N)"
    if ($continue -ne "y") {
        Write-Info "Aborted by user"
        exit 0
    }
}

Write-Success "Git repository validated"

# ================================
# 2. Read Current Version
# ================================
Write-Step "Reading current version..."

if (-not (Test-Path $VersionFile)) {
    Write-Error "Version.props not found at: $VersionFile"
    exit 1
}

$versionContent = Get-Content $VersionFile -Raw
$versionMatch = [regex]::Match($versionContent, '<SDKVersion>([^<]+)</SDKVersion>')

if (-not $versionMatch.Success) {
    Write-Error "Could not find SDKVersion in Version.props"
    exit 1
}

$currentVersion = $versionMatch.Groups[1].Value
Write-Info "Current version: $currentVersion"

# Parse current version
if ($currentVersion -match '^(\d+)\.(\d+)\.(\d+)(?:-(.+))?$') {
    $major = [int]$Matches[1]
    $minor = [int]$Matches[2]
    $patch = [int]$Matches[3]
    $suffix = $Matches[4]
} else {
    Write-Error "Invalid current version format: $currentVersion"
    exit 1
}

Write-Success "Current version parsed: $major.$minor.$patch$(if($suffix){"-$suffix"})"

# ================================
# 3. Compute New Version
# ================================
Write-Step "Computing new version..."

if ($VersionType -eq "custom") {
    if (-not $CustomVersion) {
        Write-Error "CustomVersion parameter required when VersionType is 'custom'"
        exit 1
    }
    $newVersion = $CustomVersion
} else {
    switch ($VersionType) {
        "major" { $major++; $minor = 0; $patch = 0 }
        "minor" { $minor++; $patch = 0 }
        "patch" { $patch++ }
    }
    $newVersion = "$major.$minor.$patch"
}

# Add pre-release suffix
if ($PreRelease -ne "none") {
    $buildNumber = git rev-list --count HEAD
    $newVersion = "$newVersion-$PreRelease.$buildNumber"
}

Write-Info "New version: $newVersion"
Write-Success "Version computed successfully"

# ================================
# 4. Update Version.props
# ================================
Write-Step "Updating Version.props..."

if ($newVersion -match '^(\d+\.\d+\.\d+)(?:-(.+))?$') {
    $versionPrefix = $Matches[1]
    $versionSuffix = $Matches[2]
}

$newContent = $versionContent -replace '<SDKVersion>[^<]+</SDKVersion>', "<SDKVersion>$newVersion</SDKVersion>"
$newContent = $newContent -replace '<SDKVersionPrefix>[^<]+</SDKVersionPrefix>', "<SDKVersionPrefix>$versionPrefix</SDKVersionPrefix>"

if ($versionSuffix) {
    $newContent = $newContent -replace '<SDKVersionSuffix>[^<]*</SDKVersionSuffix>', "<SDKVersionSuffix>$versionSuffix</SDKVersionSuffix>"
} else {
    $newContent = $newContent -replace '<SDKVersionSuffix>[^<]*</SDKVersionSuffix>', "<SDKVersionSuffix></SDKVersionSuffix>"
}

if (-not $DryRun) {
    Set-Content $VersionFile $newContent -Encoding UTF8
    Write-Success "Version.props updated"
} else {
    Write-Info "DRY RUN: Would update Version.props"
}

# ================================
# 5. Update CHANGELOG.md
# ================================
Write-Step "Updating CHANGELOG.md..."

$changelogContent = Get-Content $ChangelogFile -Raw
$date = Get-Date -Format "yyyy-MM-dd"

# Replace [Unreleased] with new version
$updatedChangelog = $changelogContent -replace '\[Unreleased\]', "[$newVersion] - $date"

# Add new [Unreleased] section
$unreleasedTemplate = @"

## [Unreleased]

### Added
- 

### Changed
- 

### Fixed
- 

---

"@

$updatedChangelog = $updatedChangelog -replace '(# Changelog[^\n]*\n[^\n]*\n[^\n]*\n[^\n]*\n)', "`$1$unreleasedTemplate"

if (-not $DryRun) {
    Set-Content $ChangelogFile $updatedChangelog -Encoding UTF8
    Write-Success "CHANGELOG.md updated"
} else {
    Write-Info "DRY RUN: Would update CHANGELOG.md"
}

# ================================
# 6. Build and Test (Optional)
# ================================
if (-not $SkipBuild) {
    Write-Step "Building SDK projects..."
    
    try {
        dotnet restore PerAspera.SDK.sln
        dotnet build PerAspera.SDK.sln --configuration Release --no-restore
        Write-Success "Build succeeded"
        
        # Run tests if they exist
        $testProjects = Get-ChildItem -Path . -Recurse -Filter "*.Tests.csproj"
        if ($testProjects) {
            Write-Step "Running tests..."
            dotnet test PerAspera.SDK.sln --configuration Release --no-build --verbosity minimal
            Write-Success "Tests passed"
        }
    } catch {
        Write-Error "Build or tests failed: $_"
        exit 1
    }
} else {
    Write-Warning "Build skipped (--SkipBuild flag)"
}

# ================================
# 7. Git Commit and Tag
# ================================
if (-not $DryRun) {
    Write-Step "Creating Git commit and tag..."
    
    git add Version.props CHANGELOG.md
    git commit -m "chore: bump version to $newVersion"
    
    $tagName = "v$newVersion"
    git tag -a $tagName -m "Release $tagName"
    
    Write-Success "Commit and tag created: $tagName"
    
    if ($Push) {
        Write-Step "Pushing to remote..."
        git push origin $currentBranch
        git push origin $tagName
        Write-Success "Pushed to remote"
    } else {
        Write-Warning "Changes not pushed (use -Push to push automatically)"
        Write-Info "To push manually, run:"
        Write-Host "  git push origin $currentBranch" -ForegroundColor Gray
        Write-Host "  git push origin $tagName" -ForegroundColor Gray
    }
} else {
    Write-Info "DRY RUN: Would create commit and tag v$newVersion"
}

# ================================
# 8. GitHub Release (Optional)
# ================================
if ($CreateGitHubRelease -and -not $DryRun) {
    Write-Step "Triggering GitHub Release workflow..."
    
    $ghInstalled = Get-Command gh -ErrorAction SilentlyContinue
    if ($ghInstalled) {
        try {
            gh workflow run version-release.yml `
                --field version_type=$VersionType `
                --field custom_version=$newVersion `
                --field pre_release=$PreRelease `
                --field create_release=true
            
            Write-Success "GitHub Release workflow triggered"
            Write-Info "Check: https://github.com/PerAsperaMods/PerAspera-SDK/actions"
        } catch {
            Write-Warning "Failed to trigger GitHub workflow: $_"
            Write-Info "You can manually create the release on GitHub"
        }
    } else {
        Write-Warning "GitHub CLI (gh) not installed"
        Write-Info "Install: https://cli.github.com/"
        Write-Info "Or manually create release on GitHub"
    }
}

# ================================
# 9. Summary
# ================================
Write-Header "Release Preparation Complete"

Write-Host ""
Write-Host "📦 Version:    " -NoNewline -ForegroundColor Cyan
Write-Host "$currentVersion → $newVersion" -ForegroundColor White

Write-Host "🏷️  Tag:       " -NoNewline -ForegroundColor Cyan
Write-Host "v$newVersion" -ForegroundColor White

Write-Host "📝 CHANGELOG:  " -NoNewline -ForegroundColor Cyan
Write-Host "Updated" -ForegroundColor Green

if (-not $DryRun) {
    Write-Host "✓ Git Status: " -NoNewline -ForegroundColor Cyan
    if ($Push) {
        Write-Host "Committed and Pushed" -ForegroundColor Green
    } else {
        Write-Host "Committed (not pushed)" -ForegroundColor Yellow
    }
} else {
    Write-Host "⚠ Mode:       " -NoNewline -ForegroundColor Cyan
    Write-Host "DRY RUN (no changes made)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
if (-not $Push -and -not $DryRun) {
    Write-Host "  1. Review changes: git log -1" -ForegroundColor Gray
    Write-Host "  2. Push changes:   git push origin $currentBranch && git push origin v$newVersion" -ForegroundColor Gray
}
if (-not $CreateGitHubRelease) {
    Write-Host "  3. Create GitHub Release manually or use -CreateGitHubRelease" -ForegroundColor Gray
}
Write-Host ""
