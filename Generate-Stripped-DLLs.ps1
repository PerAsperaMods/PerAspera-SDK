# PerAspera SDK - Simple DLL Stripping Script
# Generates stripped DLLs for CI/CD

param(
    [string]$GameDirectory = ".\GameLibs",
    [string]$OutputPath = ".\GameLibs-Stripped",
    [switch]$Force
)

Write-Host "Per Aspera GameLibs Stripping Tool" -ForegroundColor Green
Write-Host "Creating reference-only DLLs for CI/CD..." -ForegroundColor Blue

# Check required tools
$GameLibsMakerUrl = "https://github.com/BepInEx/BepInEx.GameLibsMaker/releases/download/v1.0.0/BepInEx.GameLibsMaker.exe"
$GameLibsMakerPath = ".\BepInEx.GameLibsMaker.exe"

# Download GameLibsMaker if needed
if (-not (Test-Path $GameLibsMakerPath)) {
    Write-Host "Downloading BepInEx.GameLibsMaker..." -ForegroundColor Yellow
    try {
        Invoke-WebRequest -Uri $GameLibsMakerUrl -OutFile $GameLibsMakerPath
        Write-Host "Downloaded GameLibsMaker successfully" -ForegroundColor Green
    }
    catch {
        Write-Error "Failed to download GameLibsMaker: $_"
        exit 1
    }
}

# Verify directories
if (-not (Test-Path $GameDirectory)) {
    Write-Error "Game directory not found: $GameDirectory"
    exit 1
}

if ((Test-Path $OutputPath) -and $Force) {
    Remove-Item $OutputPath -Recurse -Force
    Write-Host "Cleaned existing output directory" -ForegroundColor Yellow
}

# Create configuration file for GameLibsMaker
$configFile = ".GameLibsMaker"
$configContent = @"
$GameDirectory
$OutputPath
"@
Set-Content -Path $configFile -Value $configContent -Encoding UTF8

Write-Host "Running GameLibsMaker (Publicize + Strip)..." -ForegroundColor Blue
Write-Host "   Source: $GameDirectory" -ForegroundColor Gray
Write-Host "   Output: $OutputPath" -ForegroundColor Gray

# Execute the tool
try {
    $process = Start-Process -FilePath $GameLibsMakerPath -Wait -PassThru -NoNewWindow
    
    if ($process.ExitCode -eq 0) {
        Write-Host "GameLibsMaker completed successfully!" -ForegroundColor Green
    } else {
        Write-Error "GameLibsMaker failed with exit code: $($process.ExitCode)"
        exit 1
    }
}
catch {
    Write-Error "Failed to run GameLibsMaker: $_"
    exit 1
}
finally {
    # Clean up config file
    if (Test-Path $configFile) {
        Remove-Item $configFile -Force
    }
}

# Verify results
if (Test-Path $OutputPath) {
    $strippedFiles = Get-ChildItem -Path $OutputPath -Filter "*.dll"
    Write-Host ""
    Write-Host "Results:" -ForegroundColor Cyan
    Write-Host "   Stripped DLLs generated: $($strippedFiles.Count)" -ForegroundColor Green
    
    # Show critical files
    $criticalFiles = @("Assembly-CSharp.dll", "ScriptsAssembly.dll", "UnityEngine.CoreModule.dll")
    foreach ($file in $criticalFiles) {
        $fullPath = Join-Path $OutputPath $file
        if (Test-Path $fullPath) {
            $size = Get-Item $fullPath | ForEach-Object { [math]::Round($_.Length / 1KB, 2) }
            Write-Host "     OK: $file ($size KB)" -ForegroundColor Green
        } else {
            Write-Host "     MISSING: $file" -ForegroundColor Red
        }
    }
    
    Write-Host ""
    Write-Host "Stripped GameLibs ready for CI/CD!" -ForegroundColor Green
    Write-Host "Location: $OutputPath" -ForegroundColor Blue
    Write-Host "Safe to commit to version control" -ForegroundColor Blue
} else {
    Write-Error "Output directory not created"
    exit 1
}