# Archive-SDK.ps1
# Creates a versioned archive of the current SDK build

[CmdletBinding()]
param(
    [Parameter(Mandatory=$false)]
    [string]$Version,
    
    [Parameter(Mandatory=$false)]
    [string]$ArchiveDir = "$PSScriptRoot\_Archive",
    
    [Parameter(Mandatory=$false)]
    [switch]$IncludeBinaries = $true,
    
    [Parameter(Mandatory=$false)]
    [switch]$IncludePackages = $true,
    
    [Parameter(Mandatory=$false)]
    [switch]$IncludeDocumentation = $true,
    
    [Parameter(Mandatory=$false)]
    [switch]$Force
)

$ErrorActionPreference = "Stop"

# Colors for output
function Write-Success { param($msg) Write-Host "✓ $msg" -ForegroundColor Green }
function Write-Info { param($msg) Write-Host "→ $msg" -ForegroundColor Cyan }
function Write-Warning { param($msg) Write-Host "⚠ $msg" -ForegroundColor Yellow }
function Write-Error { param($msg) Write-Host "✗ $msg" -ForegroundColor Red }

Write-Host "`n═══════════════════════════════════════" -ForegroundColor Magenta
Write-Host "   PerAspera SDK Archive Creator" -ForegroundColor Magenta
Write-Host "═══════════════════════════════════════`n" -ForegroundColor Magenta

# Get version from Version.props if not provided
if (-not $Version) {
    Write-Info "Reading version from Version.props..."
    $versionPropsPath = Join-Path $PSScriptRoot "Version.props"
    
    if (-not (Test-Path $versionPropsPath)) {
        Write-Error "Version.props not found at: $versionPropsPath"
        exit 1
    }
    
    [xml]$versionProps = Get-Content $versionPropsPath
    $Version = $versionProps.Project.PropertyGroup.SDKVersion
    
    if (-not $Version) {
        Write-Error "Could not read SDKVersion from Version.props"
        exit 1
    }
}

Write-Success "SDK Version: $Version"

# Create archive directory structure
$archivePath = Join-Path $ArchiveDir "v$Version"

if (Test-Path $archivePath) {
    if ($Force) {
        Write-Warning "Archive already exists. Removing due to -Force flag..."
        Remove-Item $archivePath -Recurse -Force
    } else {
        Write-Error "Archive for version $Version already exists at: $archivePath"
        Write-Info "Use -Force to overwrite existing archive"
        exit 1
    }
}

Write-Info "Creating archive directory: $archivePath"
New-Item -ItemType Directory -Path $archivePath -Force | Out-Null

# Create subdirectories
$binPath = Join-Path $archivePath "bin"
$packagesPath = Join-Path $archivePath "packages"
$docsPath = Join-Path $archivePath "docs"

New-Item -ItemType Directory -Path $binPath -Force | Out-Null
New-Item -ItemType Directory -Path $packagesPath -Force | Out-Null
New-Item -ItemType Directory -Path $docsPath -Force | Out-Null

# Archive binaries
if ($IncludeBinaries) {
    Write-Info "Archiving binaries..."
    
    $sdkBinPath = Join-Path $PSScriptRoot "bin\Release"
    if (Test-Path $sdkBinPath) {
        $dllFiles = Get-ChildItem -Path $sdkBinPath -Filter "*.dll" -Recurse
        
        foreach ($dll in $dllFiles) {
            $relativePath = $dll.FullName.Substring($sdkBinPath.Length + 1)
            $targetPath = Join-Path $binPath $relativePath
            $targetDir = Split-Path $targetPath -Parent
            
            if (-not (Test-Path $targetDir)) {
                New-Item -ItemType Directory -Path $targetDir -Force | Out-Null
            }
            
            Copy-Item $dll.FullName -Destination $targetPath -Force
            Write-Success "  Archived: $relativePath"
        }
    } else {
        Write-Warning "No Release binaries found. Run Build-SDK.ps1 first."
    }
}

# Archive NuGet packages
if ($IncludePackages) {
    Write-Info "Archiving NuGet packages..."
    
    $nupkgFiles = Get-ChildItem -Path $PSScriptRoot -Filter "*.nupkg" -File
    
    foreach ($pkg in $nupkgFiles) {
        if ($pkg.Name -like "*$Version*") {
            Copy-Item $pkg.FullName -Destination $packagesPath -Force
            Write-Success "  Archived: $($pkg.Name)"
        }
    }
    
    if ($nupkgFiles.Count -eq 0) {
        Write-Warning "No NuGet packages found for version $Version"
    }
}

# Archive documentation
if ($IncludeDocumentation) {
    Write-Info "Archiving documentation..."
    
    $docFiles = @(
        "CHANGELOG.md",
        "README.md",
        "VERSION-GUIDE.md",
        "RELEASE-WORKFLOW.md",
        "GAME-EVENTS-REFERENCE.md"
    )
    
    foreach ($docFile in $docFiles) {
        $sourcePath = Join-Path $PSScriptRoot $docFile
        if (Test-Path $sourcePath) {
            Copy-Item $sourcePath -Destination $docsPath -Force
            Write-Success "  Archived: $docFile"
        }
    }
    
    # Archive Documentation folder
    $docsFolder = Join-Path $PSScriptRoot "Documentation"
    if (Test-Path $docsFolder) {
        $targetDocsFolder = Join-Path $docsPath "Documentation"
        Copy-Item $docsFolder -Destination $targetDocsFolder -Recurse -Force
        Write-Success "  Archived: Documentation folder"
    }
}

# Create VERSION-INFO.md with build metadata
Write-Info "Creating VERSION-INFO.md..."

$versionInfo = @"
# SDK Version $Version - Build Information

## Build Metadata

- **Version**: $Version
- **Archive Date**: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
- **Archive Created By**: $env:USERNAME
- **Computer**: $env:COMPUTERNAME

## Git Information

"@

# Try to get Git information
try {
    $gitCommit = git rev-parse HEAD 2>$null
    $gitBranch = git rev-parse --abbrev-ref HEAD 2>$null
    $gitTag = git describe --tags --exact-match 2>$null
    
    if ($gitCommit) {
        $versionInfo += @"

- **Git Commit**: ``$gitCommit``
- **Git Branch**: ``$gitBranch``
- **Git Tag**: ``$gitTag``

"@
    }
} catch {
    $versionInfo += "`n- Git information not available`n"
}

$versionInfo += @"

## Archive Contents

"@

# List all archived files
$allFiles = Get-ChildItem -Path $archivePath -Recurse -File
$versionInfo += "`n### Files ($($allFiles.Count) total)`n`n"

$allFiles | ForEach-Object {
    $relativePath = $_.FullName.Substring($archivePath.Length + 1)
    $size = if ($_.Length -gt 1MB) { "{0:N2} MB" -f ($_.Length / 1MB) } 
            elseif ($_.Length -gt 1KB) { "{0:N2} KB" -f ($_.Length / 1KB) }
            else { "$($_.Length) bytes" }
    $versionInfo += "- ``$relativePath`` ($size)`n"
}

$versionInfo += @"

## Usage

To use this archived version in your mod:

1. Reference the DLLs from the ``bin/`` directory
2. Or install the NuGet package from ``packages/``
3. Review ``docs/CHANGELOG.md`` for version-specific changes

## Restoration

To restore this version as the active SDK:

``````powershell
# Backup current SDK
Move-Item SDK\bin SDK\bin.backup

# Restore archived binaries
Copy-Item _Archive\v$Version\bin\* SDK\bin\ -Recurse -Force

# Update Version.props
# (Manual edit required)
``````

---

*This archive is read-only. Do not modify archived files.*
"@

$versionInfoPath = Join-Path $archivePath "VERSION-INFO.md"
$versionInfo | Out-File -FilePath $versionInfoPath -Encoding UTF8
Write-Success "Created VERSION-INFO.md"

# Create a compressed archive (optional)
Write-Info "Creating compressed archive..."
$zipPath = Join-Path $ArchiveDir "PerAspera-SDK-v$Version.zip"

try {
    Compress-Archive -Path $archivePath\* -DestinationPath $zipPath -Force
    $zipSize = (Get-Item $zipPath).Length / 1MB
    Write-Success "Created ZIP archive: PerAspera-SDK-v$Version.zip ({0:N2} MB)" -f $zipSize
} catch {
    Write-Warning "Could not create ZIP archive: $_"
}

# Summary
Write-Host "`n═══════════════════════════════════════" -ForegroundColor Green
Write-Host "   Archive Created Successfully!" -ForegroundColor Green
Write-Host "═══════════════════════════════════════`n" -ForegroundColor Green

Write-Info "Archive Location: $archivePath"
Write-Info "ZIP Archive: $zipPath"
Write-Info "Version: $Version"
Write-Info "Files Archived: $($allFiles.Count)"

Write-Host "`nArchive is ready for distribution and backup.`n"
