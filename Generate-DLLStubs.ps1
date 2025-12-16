# Générer les DLL stubs avec BepInEx.GameLibsMaker

param(
    [string]$GameDirectory = "F:\SteamLibrary\steamapps\common\Per Aspera",
    [string]$OutputPath = "F:\ModPeraspera\SDK\GameLibs-Stripped",
    [string]$GameLibsMakerPath = "",
    [switch]$Force = $false,
    [switch]$DownloadTool = $false
)

Write-Host "🔧 Generating stripped GameLibs with BepInEx.GameLibsMaker..." -ForegroundColor Cyan

# Télécharger GameLibsMaker si nécessaire
if ($DownloadTool -or -not $GameLibsMakerPath -or -not (Test-Path $GameLibsMakerPath)) {
    Write-Host "📥 Downloading BepInEx.GameLibsMaker..." -ForegroundColor Blue
    
    $downloadUrl = "https://github.com/EnoPM/BepInEx.GameLibsMaker/releases/latest/download/GameLibsMaker.exe"
    $toolsPath = "F:\ModPeraspera\SDK\tools"
    $GameLibsMakerPath = Join-Path $toolsPath "GameLibsMaker.exe"
    
    if (-not (Test-Path $toolsPath)) {
        New-Item -Path $toolsPath -ItemType Directory -Force | Out-Null
    }
    
    try {
        Invoke-WebRequest -Uri $downloadUrl -OutFile $GameLibsMakerPath
        Write-Host "✅ Downloaded GameLibsMaker to: $GameLibsMakerPath" -ForegroundColor Green
    }
    catch {
        Write-Error "❌ Failed to download GameLibsMaker: $_"
        Write-Host "📋 Manual download: https://github.com/EnoPM/BepInEx.GameLibsMaker/releases" -ForegroundColor Yellow
        return $false
    }
}

# Vérifier que le jeu existe
if (-not (Test-Path $GameDirectory)) {
    Write-Error "❌ Per Aspera game directory not found: $GameDirectory"
    Write-Host "💡 Please install Per Aspera or update the path" -ForegroundColor Yellow
    return $false
}

# Vérifier les DLL critiques du jeu
$criticalGameFiles = @(
    "Per Aspera_Data\Managed\Assembly-CSharp.dll",
    "Per Aspera_Data\Managed\UnityEngine.CoreModule.dll"
)

foreach ($file in $criticalGameFiles) {
    $fullPath = Join-Path $GameDirectory $file
    if (-not (Test-Path $fullPath)) {
        Write-Error "❌ Critical game file missing: $file"
        return $false
    }
}

# Nettoyer le répertoire de sortie si demandé
if (Test-Path $OutputPath) {
    if ($Force) {
        Remove-Item $OutputPath -Recurse -Force
        Write-Host "🧹 Cleaned existing output directory" -ForegroundColor Yellow
    } else {
        Write-Host "⚠️ Output directory exists. Use -Force to overwrite" -ForegroundColor Yellow
        Write-Host "📁 Existing: $OutputPath" -ForegroundColor Gray
    }
}

Write-Host "🎯 Starting GameLibsMaker processing..." -ForegroundColor Green
Write-Host "   🎮 Game Directory: $GameDirectory" -ForegroundColor Gray
Write-Host "   📁 Output Directory: $OutputPath" -ForegroundColor Gray
Write-Host "   🛠️ Tool: $GameLibsMakerPath" -ForegroundColor Gray

# Exécuter GameLibsMaker
try {
    Write-Host "`n🔄 Running GameLibsMaker (Publicize + Strip)..." -ForegroundColor Blue
    
    # Configuration file pour GameLibsMaker
    $configFile = ".GameLibsMaker"
    $configContent = @"
$GameDirectory
$OutputPath
"@
    Set-Content -Path $configFile -Value $configContent -Encoding UTF8
    
    # Exécuter l'outil
    $process = Start-Process -FilePath $GameLibsMakerPath -Wait -PassThru -NoNewWindow
    
    if ($process.ExitCode -eq 0) {
        Write-Host "✅ GameLibsMaker completed successfully!" -ForegroundColor Green
    } else {
        Write-Error "❌ GameLibsMaker failed with exit code: $($process.ExitCode)"
        return $false
    }
    
    # Nettoyer le fichier de config temporaire
    if (Test-Path $configFile) {
        Remove-Item $configFile -Force
    }
}
catch {
    Write-Error "❌ Failed to run GameLibsMaker: $_"
    return $false
}

# Vérifier les résultats
if (Test-Path $OutputPath) {
    $strippedFiles = Get-ChildItem -Path $OutputPath -Filter "*.dll" | Measure-Object
    $gameLibsProps = Join-Path $OutputPath "GameLibs.props"
    
    Write-Host "`n📊 Results:" -ForegroundColor Cyan
    Write-Host "   📦 Stripped DLLs: $($strippedFiles.Count)" -ForegroundColor Green
    
    if (Test-Path $gameLibsProps) {
        Write-Host "   ✅ GameLibs.props generated" -ForegroundColor Green
    } else {
        Write-Warning "   ⚠️ GameLibs.props not found"
    }
    
    # Afficher quelques fichiers générés
    $sampleFiles = Get-ChildItem -Path $OutputPath -Filter "*.dll" | Select-Object -First 5
    foreach ($file in $sampleFiles) {
        $sizeKB = [math]::Round($file.Length / 1KB, 2)
        Write-Host "     - $($file.Name) ($sizeKB KB)" -ForegroundColor Gray
    }
    
    if ($strippedFiles.Count -gt 5) {
        Write-Host "     ... and $($strippedFiles.Count - 5) more files" -ForegroundColor Gray
    }
}

Write-Host "`n🎯 Stripped GameLibs ready for CI/CD!" -ForegroundColor Green
Write-Host "📁 Location: $OutputPath" -ForegroundColor Blue
Write-Host "🔗 These can be safely committed to version control" -ForegroundColor Blue

return $true