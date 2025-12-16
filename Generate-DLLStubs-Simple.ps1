# PerAspera SDK - Simple DLL Stripping Script
# Génère des DLL stripped pour CI/CD

param(
    [string]$GameDirectory = ".\GameLibs",
    [string]$OutputPath = ".\GameLibs-Stripped",
    [switch]$Force
)

Write-Host "🎯 Per Aspera GameLibs Stripping Tool" -ForegroundColor Green
Write-Host "🔧 Creating reference-only DLLs for CI/CD..." -ForegroundColor Blue

# Vérifier les outils requis
$GameLibsMakerUrl = "https://github.com/BepInEx/BepInEx.GameLibsMaker/releases/download/v1.0.0/BepInEx.GameLibsMaker.exe"
$GameLibsMakerPath = ".\BepInEx.GameLibsMaker.exe"

# Télécharger GameLibsMaker si nécessaire
if (-not (Test-Path $GameLibsMakerPath)) {
    Write-Host "⬇️ Downloading BepInEx.GameLibsMaker..." -ForegroundColor Yellow
    try {
        Invoke-WebRequest -Uri $GameLibsMakerUrl -OutFile $GameLibsMakerPath
        Write-Host "✅ Downloaded GameLibsMaker" -ForegroundColor Green
    }
    catch {
        Write-Error "❌ Failed to download GameLibsMaker: $_"
        exit 1
    }
}

# Vérifier les répertoires
if (-not (Test-Path $GameDirectory)) {
    Write-Error "❌ Game directory not found: $GameDirectory"
    exit 1
}

if ((Test-Path $OutputPath) -and $Force) {
    Remove-Item $OutputPath -Recurse -Force
    Write-Host "🧹 Cleaned existing output directory" -ForegroundColor Yellow
}

# Créer le fichier de configuration pour GameLibsMaker
$configFile = ".GameLibsMaker"
$configContent = @"
$GameDirectory
$OutputPath
"@
Set-Content -Path $configFile -Value $configContent -Encoding UTF8

Write-Host "🔄 Running GameLibsMaker (Publicize + Strip)..." -ForegroundColor Blue
Write-Host "   🎮 Source: $GameDirectory" -ForegroundColor Gray
Write-Host "   📁 Output: $OutputPath" -ForegroundColor Gray

# Exécuter l'outil
try {
    $process = Start-Process -FilePath $GameLibsMakerPath -Wait -PassThru -NoNewWindow
    
    if ($process.ExitCode -eq 0) {
        Write-Host "✅ GameLibsMaker completed successfully!" -ForegroundColor Green
    } else {
        Write-Error "❌ GameLibsMaker failed with exit code: $($process.ExitCode)"
        exit 1
    }
}
catch {
    Write-Error "❌ Failed to run GameLibsMaker: $_"
    exit 1
}
finally {
    # Nettoyer le fichier de config
    if (Test-Path $configFile) {
        Remove-Item $configFile -Force
    }
}

# Vérifier les résultats
if (Test-Path $OutputPath) {
    $strippedFiles = Get-ChildItem -Path $OutputPath -Filter "*.dll"
    Write-Host "`n📊 Results:" -ForegroundColor Cyan
    Write-Host "   📦 Stripped DLLs generated: $($strippedFiles.Count)" -ForegroundColor Green
    
    # Afficher quelques fichiers générés
    $criticalFiles = @("Assembly-CSharp.dll", "ScriptsAssembly.dll", "UnityEngine.CoreModule.dll")
    foreach ($file in $criticalFiles) {
        $fullPath = Join-Path $OutputPath $file
        if (Test-Path $fullPath) {
            $size = Get-Item $fullPath | ForEach-Object { [math]::Round($_.Length / 1KB, 2) }
            Write-Host "     ✅ $file ($size KB)" -ForegroundColor Green
        } else {
            Write-Host "     ❌ $file (missing)" -ForegroundColor Red
        }
    }
    
    Write-Host "`n🎯 Stripped GameLibs ready for CI/CD!" -ForegroundColor Green
    Write-Host "📁 Location: $OutputPath" -ForegroundColor Blue
    Write-Host "🔗 Safe to commit to version control" -ForegroundColor Blue
} else {
    Write-Error "❌ Output directory not created"
    exit 1
}