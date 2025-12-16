# PerAspera SDK Version Management Script
# Gère les versions du SDK, les releases et les tags Git

param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("show", "bump-major", "bump-minor", "bump-patch", "set-version", "pre-release", "stable", "build", "package", "publish", "archive")]
    [string]$Action = "show",
    
    [Parameter(Mandatory = $false)]
    [string]$Version,
    
    [Parameter(Mandatory = $false)]
    [ValidateSet("alpha", "beta", "rc")]
    [string]$PreReleaseType = "beta",
    
    [Parameter(Mandatory = $false)]
    [switch]$Push = $false,
    
    [Parameter(Mandatory = $false)]
    [switch]$DryRun = $false,
    
    [Parameter(Mandatory = $false)]
    [switch]$ArchiveAfterBuild = $false
)

$ErrorActionPreference = "Stop"
$SDKRoot = $PSScriptRoot
$VersionFile = Join-Path $SDKRoot "Version.props"

# Couleurs pour l'affichage
function Write-Header { param($Text) Write-Host "[SDK] $Text" -ForegroundColor Green }
function Write-Info { param($Text) Write-Host "[INFO] $Text" -ForegroundColor Cyan }
function Write-Warning { param($Text) Write-Host "[WARN] $Text" -ForegroundColor Yellow }
function Write-Error { param($Text) Write-Host "[ERROR] $Text" -ForegroundColor Red }
function Write-Success { param($Text) Write-Host "[SUCCESS] $Text" -ForegroundColor Green }

# Lecture de la version actuelle depuis Version.props
function Get-CurrentVersion {
    if (-not (Test-Path $VersionFile)) {
        Write-Error "Version.props not found at: $VersionFile"
        exit 1
    }
    
    $content = Get-Content $VersionFile -Raw
    $versionMatch = [regex]::Match($content, '<SDKVersion>([^<]+)</SDKVersion>')
    
    if (-not $versionMatch.Success) {
        Write-Error "Could not find SDKVersion in Version.props"
        exit 1
    }
    
    return $versionMatch.Groups[1].Value
}

# Mise à jour de Version.props
function Set-Version {
    param([string]$NewVersion)
    
    if (Test-Path $VersionFile) {
        $content = Get-Content $VersionFile -Raw
        
        # Parse version
        if ($NewVersion -match '^(\d+)\.(\d+)\.(\d+)(?:-(.+))?$') {
            $major = $Matches[1]
            $minor = $Matches[2]
            $patch = $Matches[3]
            $suffix = $Matches[4]
            
            $versionPrefix = "$major.$minor.$patch"
            
            # Mise à jour des propriétés XML
            $content = $content -replace '<SDKVersion>[^<]+</SDKVersion>', "<SDKVersion>$NewVersion</SDKVersion>"
            $content = $content -replace '<SDKVersionPrefix>[^<]+</SDKVersionPrefix>', "<SDKVersionPrefix>$versionPrefix</SDKVersionPrefix>"
            
            if ($suffix) {
                $content = $content -replace '<SDKVersionSuffix>[^<]*</SDKVersionSuffix>', "<SDKVersionSuffix>$suffix</SDKVersionSuffix>"
            } else {
                $content = $content -replace '<SDKVersionSuffix>[^<]*</SDKVersionSuffix>', "<SDKVersionSuffix></SDKVersionSuffix>"
            }
            
            if (-not $DryRun) {
                Set-Content $VersionFile $content -Encoding UTF8
                Write-Success "Version updated to $NewVersion in Version.props"
            } else {
                Write-Info "DRY RUN: Would update version to $NewVersion"
            }
        } else {
            Write-Error "Invalid version format: $NewVersion (expected: X.Y.Z or X.Y.Z-suffix)"
            exit 1
        }
    }
}

# Parse version string
function Parse-Version {
    param([string]$VersionString)
    
    if ($VersionString -match '^(\d+)\.(\d+)\.(\d+)(?:-(.+))?$') {
        return @{
            Major = [int]$Matches[1]
            Minor = [int]$Matches[2] 
            Patch = [int]$Matches[3]
            PreRelease = $Matches[4]
            Full = $VersionString
        }
    }
    
    Write-Error "Invalid version format: $VersionString"
    exit 1
}

# Bump version
function Bump-Version {
    param(
        [string]$BumpType,
        [string]$CurrentVersion
    )
    
    $version = Parse-Version $CurrentVersion
    
    switch ($BumpType) {
        "major" {
            $newVersion = "$($version.Major + 1).0.0"
        }
        "minor" {
            $newVersion = "$($version.Major).$($version.Minor + 1).0"
        }
        "patch" {
            $newVersion = "$($version.Major).$($version.Minor).$($version.Patch + 1)"
        }
    }
    
    # Si c'était une pre-release et qu'on bump, on supprime le suffix
    if ($version.PreRelease -and $BumpType -ne "patch") {
        # Le patch peut garder le suffix pre-release
        return $newVersion
    } elseif ($version.PreRelease -and $BumpType -eq "patch") {
        return "$newVersion-$($version.PreRelease)"
    }
    
    return $newVersion
}

# Création du tag Git
function Create-GitTag {
    param([string]$Version)
    
    $tagName = "v$Version"
    
    try {
        # Vérifier si le tag existe
        $existingTag = git tag -l $tagName 2>$null
        if ($existingTag) {
            Write-Warning "Tag $tagName already exists"
            return $false
        }
        
        if (-not $DryRun) {
            git tag -a $tagName -m "Release version $Version"
            Write-Success "Created Git tag: $tagName"
            
            if ($Push) {
                git push origin $tagName
                Write-Success "Pushed tag to origin"
            }
        } else {
            Write-Info "DRY RUN: Would create Git tag: $tagName"
        }
        
        return $true
    } catch {
        Write-Error "Failed to create Git tag: $_"
        return $false
    }
}

# Build du SDK
function Build-SDK {
    param([string]$Configuration = "Release")
    
    Write-Header "Building PerAspera SDK ($Configuration)"
    
    $solutionFile = Join-Path $SDKRoot "PerAspera.SDK.sln"
    
    if (-not $DryRun) {
        dotnet clean $solutionFile --configuration $Configuration
        dotnet build $solutionFile --configuration $Configuration --no-restore
        
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Build completed successfully"
            return $true
        } else {
            Write-Error "Build failed"
            return $false
        }
    } else {
        Write-Info "DRY RUN: Would build $solutionFile in $Configuration mode"
        return $true
    }
}

# Package NuGet
function Create-Packages {
    Write-Header "Creating NuGet packages"
    
    $projects = @(
        "PerAspera.Core\PerAspera.Core.csproj",
        "PerAspera.GameAPI\PerAspera.GameAPI.csproj", 
        "PerAspera.ModSDK\PerAspera.ModSDK.csproj"
    )
    
    foreach ($project in $projects) {
        $projectPath = Join-Path $SDKRoot $project
        
        if (-not $DryRun) {
            dotnet pack $projectPath --configuration Release --no-build --output "packages"
            if ($LASTEXITCODE -eq 0) {
                Write-Success "Packaged $project"
            } else {
                Write-Error "Failed to package $project"
                return $false
            }
        } else {
            Write-Info "DRY RUN: Would package $project"
        }
    }
    
    return $true
}

# Affichage des informations
function Show-VersionInfo {
    $currentVersion = Get-CurrentVersion
    $version = Parse-Version $currentVersion
    
    Write-Header "PerAspera SDK Version Information"
    Write-Host ""
    Write-Host "Current Version: " -NoNewline
    Write-Host $currentVersion -ForegroundColor Green
    Write-Host "  Major: $($version.Major)"
    Write-Host "  Minor: $($version.Minor)" 
    Write-Host "  Patch: $($version.Patch)"
    
    if ($version.PreRelease) {
        Write-Host "  Pre-release: $($version.PreRelease)" -ForegroundColor Yellow
    } else {
        Write-Host "  Pre-release: None" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "Repository: https://github.com/PerAsperaMods/PerAspera-SDK" -ForegroundColor Cyan
    Write-Host "Location: $SDKRoot" -ForegroundColor Gray
    
    # Git info
    try {
        $gitBranch = git branch --show-current 2>$null
        $gitCommit = git rev-parse --short HEAD 2>$null
        $gitStatus = git status --porcelain 2>$null
        
        Write-Host ""
        Write-Host "Git Information:"
        Write-Host "  Branch: $gitBranch" -ForegroundColor Cyan
        Write-Host "  Commit: $gitCommit" -ForegroundColor Gray
        
        if ($gitStatus) {
            Write-Host "  Status: " -NoNewline
            Write-Host "Uncommitted changes" -ForegroundColor Yellow
        } else {
            Write-Host "  Status: " -NoNewline  
            Write-Host "Clean" -ForegroundColor Green
        }
    } catch {
        Write-Host "Git information not available" -ForegroundColor Gray
    }
}

# Script principal
try {
    switch ($Action) {
        "show" {
            Show-VersionInfo
        }
        
        "bump-major" {
            $current = Get-CurrentVersion
            $newVersion = Bump-Version "major" $current
            Write-Info "Bumping version: $current → $newVersion"
            Set-Version $newVersion
            Create-GitTag $newVersion
        }
        
        "bump-minor" {
            $current = Get-CurrentVersion
            $newVersion = Bump-Version "minor" $current
            Write-Info "Bumping version: $current → $newVersion"
            Set-Version $newVersion
            Create-GitTag $newVersion
        }
        
        "bump-patch" {
            $current = Get-CurrentVersion
            $newVersion = Bump-Version "patch" $current
            Write-Info "Bumping version: $current → $newVersion"
            Set-Version $newVersion
            Create-GitTag $newVersion
        }
        
        "set-version" {
            if (-not $Version) {
                Write-Error "Version parameter is required for set-version action"
                exit 1
            }
            $current = Get-CurrentVersion
            Write-Info "Setting version: $current -> $Version"
            Set-Version $Version
            Create-GitTag $Version
        }
        
        "pre-release" {
            $current = Get-CurrentVersion
            $version = Parse-Version $current
            
            if ($version.PreRelease) {
                # Increment pre-release
                if ($version.PreRelease -match "^(\w+)(\d*)$") {
                    $type = $Matches[1]
                    $number = if ($Matches[2]) { [int]$Matches[2] + 1 } else { 1 }
                    $newVersion = "$($version.Major).$($version.Minor).$($version.Patch)-$type$number"
                } else {
                    $newVersion = "$($version.Major).$($version.Minor).$($version.Patch)-$PreReleaseType.1"
                }
            } else {
                $newVersion = "$($version.Major).$($version.Minor).$($version.Patch)-$PreReleaseType"
            }
            
            Write-Info "Creating pre-release: $current -> $newVersion"
            Set-Version $newVersion
            Create-GitTag $newVersion
        }
        
        "stable" {
            $current = Get-CurrentVersion
            $version = Parse-Version $current
            
            if ($version.PreRelease) {
                $newVersion = "$($version.Major).$($version.Minor).$($version.Patch)"
                Write-Info "Promoting to stable: $current -> $newVersion"
                Set-Version $newVersion
                Create-GitTag $newVersion
            } else {
                Write-Warning "Version $current is already stable"
            }
        }
        
        "build" {
            Build-SDK
        }
        
        "package" {
            if (Build-SDK) {
                Create-Packages
            }
        }
        
        "publish" {
            Write-Info "Publishing packages to GitHub Packages"
            # TODO: Implement GitHub Packages publishing
            Write-Warning "GitHub Packages publishing not yet implemented"
        }
        
        "archive" {
            Write-Info "Creating SDK archive for current version"
            $archiveScript = Join-Path $SDKRoot "Archive-SDK.ps1"
            if (Test-Path $archiveScript) {
                & $archiveScript -IncludeBinaries -IncludePackages -IncludeDocumentation
            } else {
                Write-Error "Archive-SDK.ps1 not found at: $archiveScript"
            }
        }
    }
    
    # Auto-archive after successful builds if requested
    if ($ArchiveAfterBuild -and ($Action -eq "build" -or $Action -eq "package")) {
        Write-Info "Auto-archiving build..."
        $archiveScript = Join-Path $SDKRoot "Archive-SDK.ps1"
        if (Test-Path $archiveScript) {
            & $archiveScript -IncludeBinaries -IncludePackages -IncludeDocumentation
        }
    }
    
    Write-Host ""
    Write-Success "Operation completed successfully!"
    
} catch {
    Write-Error "Script failed: $_"
    exit 1
}