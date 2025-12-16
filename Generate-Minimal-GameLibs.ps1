# PerAspera SDK - Optimized DLL Sets for CI/CD
# Creates critical set + full ModSDK set for different use cases

param(
    [string]$GameDirectory = ".\GameLibs", 
    [string]$OutputCritical = ".\GameLibs-Critical",
    [string]$OutputModSDK = ".\GameLibs-ModSDK", 
    [string]$ArchiveCritical = ".\GameLibs-Critical.zip",
    [string]$ArchiveModSDK = ".\GameLibs-ModSDK.zip",
    [switch]$Force
)

Write-Host "Per Aspera Optimized GameLibs Tool" -ForegroundColor Green
Write-Host "Creating optimized DLL sets for different CI/CD scenarios..." -ForegroundColor Blue

# Liste des DLL critiques pour compilation de base
$CriticalDLLs = @(
    "Assembly-CSharp.dll",           # Classes du jeu Per Aspera
    "ScriptsAssembly.dll",           # Scripts Per Aspera (Blackboard, etc.)
    "Il2Cppmscorlib.dll",            # Types IL2CPP (Object, Type)
    "UnityEngine.CoreModule.dll",    # Unity de base
    "UnityEngine.IMGUIModule.dll",   # GUI Unity (GUIStyle)
    "Il2CppSystem.dll",              # System IL2CPP
    "UnityEngine.dll"                # UnityEngine principal
)

# Liste des DLL supplémentaires pour ModSDK (accès complet aux APIs)
$ModSDKExtraDLLs = @(
    # IL2CPP Core
    "Il2CppMicrosoft.CSharp.dll", "Il2CppMono.Security.dll", "Il2CppSystem.Configuration.dll", 
    "Il2CppSystem.Core.dll", "Il2CppSystem.Data.dll", "Il2CppSystem.Numerics.dll", 
    "Il2CppSystem.Runtime.Serialization.dll", "Il2CppSystem.Xml.dll", "Il2CppSystem.Xml.Linq.dll",
    
    # Unity Core Systems
    "Unity.Burst.dll", "Unity.Burst.Unsafe.dll", "Unity.Collections.dll", 
    "Unity.Collections.LowLevel.ILSupport.dll", "Unity.Jobs.dll", "Unity.Mathematics.dll",
    "Unity.Mathematics.Extensions.dll", "Unity.Entities.dll", "Unity.Entities.Hybrid.dll",
    "Unity.Transforms.dll", "Unity.Transforms.Hybrid.dll", "Unity.Scenes.dll",
    
    # Unity Engine Modules
    "UnityEngine.AudioModule.dll", "UnityEngine.PhysicsModule.dll", "UnityEngine.Physics2DModule.dll",
    "UnityEngine.AnimationModule.dll", "UnityEngine.UI.dll", "UnityEngine.UIElementsModule.dll",
    "UnityEngine.TextMeshPro.dll", "UnityEngine.ParticleSystemModule.dll", "UnityEngine.TerrainModule.dll",
    "UnityEngine.InputLegacyModule.dll", "UnityEngine.InputModule.dll", "UnityEngine.TextRenderingModule.dll",
    
    # Per Aspera Specific Libraries  
    "DOTween.dll", "DOTweenPro.dll", "Newtonsoft.Json.dll", "CsvHelper.dll", "YarnSpinner.dll",
    "FMODAssembly.dll", "Cinemachine.dll", "Unity.TextMeshPro.dll", "Unity.Timeline.dll",
    "ShapesRuntime.dll", "com.rlabrecque.steamworks.net.dll", "Protobuf.dll"
)

$GameLibsMakerPath = "F:\ModPeraspera\Tools\GameLibsMaker.exe"
$TempOutputPath = ".\GameLibs-Full-Temp"

# Vérifier les outils
if (-not (Test-Path $GameLibsMakerPath)) {
    Write-Error "GameLibsMaker not found at: $GameLibsMakerPath"
    exit 1
}

if (-not (Test-Path $GameDirectory)) {
    Write-Error "Game directory not found: $GameDirectory"
    exit 1
}

# Nettoyer les anciens fichiers
if ($Force) {
    Remove-Item $OutputCritical -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item $OutputModSDK -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item $TempOutputPath -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item $ArchiveCritical -Force -ErrorAction SilentlyContinue
    Remove-Item $ArchiveModSDK -Force -ErrorAction SilentlyContinue
}

# Créer le répertoire temporaire
New-Item -Path $TempOutputPath -ItemType Directory -Force | Out-Null

Write-Host "Step 1: Generating all stripped DLLs..." -ForegroundColor Yellow

# Créer le fichier de config pour GameLibsMaker
$configFile = ".GameLibsMaker"
$configContent = @"
$GameDirectory
$TempOutputPath
"@
Set-Content -Path $configFile -Value $configContent -Encoding UTF8

# Exécuter GameLibsMaker
try {
    $process = Start-Process -FilePath $GameLibsMakerPath -Wait -PassThru -NoNewWindow
    
    if ($process.ExitCode -ne 0) {
        Write-Error "GameLibsMaker failed with exit code: $($process.ExitCode)"
        exit 1
    }
}
finally {
    Remove-Item $configFile -Force -ErrorAction SilentlyContinue
}

Write-Host "Step 2: Creating Critical set (minimal compilation)..." -ForegroundColor Yellow

# Créer le répertoire de sortie critique
New-Item -Path $OutputCritical -ItemType Directory -Force | Out-Null

$criticalFound = @()
$criticalMissing = @()

foreach ($dll in $CriticalDLLs) {
    $sourcePath = Join-Path $TempOutputPath $dll
    $destPath = Join-Path $OutputCritical $dll
    
    if (Test-Path $sourcePath) {
        Copy-Item $sourcePath $destPath -Force
        $size = (Get-Item $destPath).Length
        $sizeKB = [math]::Round($size / 1KB, 2)
        Write-Host "   ✅ $dll ($sizeKB KB)" -ForegroundColor Green
        $criticalFound += $dll
    } else {
        Write-Host "   ❌ $dll" -ForegroundColor Red
        $criticalMissing += $dll
    }
}

# Copier le GameLibs.props pour Critical
$propsSource = Join-Path $TempOutputPath "GameLibs.props"
$propsCritical = Join-Path $OutputCritical "GameLibs.props"
if (Test-Path $propsSource) {
    Copy-Item $propsSource $propsCritical -Force
    Write-Host "   ✅ GameLibs.props" -ForegroundColor Green
}

Write-Host "Step 3: Creating ModSDK set (comprehensive APIs)..." -ForegroundColor Yellow

# Créer le répertoire de sortie ModSDK
New-Item -Path $OutputModSDK -ItemType Directory -Force | Out-Null

# Copier toutes les DLL critiques + extras pour ModSDK
$allModSDKDLLs = $CriticalDLLs + $ModSDKExtraDLLs
$modSDKFound = @()
$modSDKMissing = @()

foreach ($dll in $allModSDKDLLs) {
    $sourcePath = Join-Path $TempOutputPath $dll
    $destPath = Join-Path $OutputModSDK $dll
    
    if (Test-Path $sourcePath) {
        Copy-Item $sourcePath $destPath -Force
        $modSDKFound += $dll
    } else {
        $modSDKMissing += $dll
    }
}

# Copier le GameLibs.props pour ModSDK
$propsModSDK = Join-Path $OutputModSDK "GameLibs.props"
if (Test-Path $propsSource) {
    Copy-Item $propsSource $propsModSDK -Force
}

Write-Host "   📦 ModSDK set: $($modSDKFound.Count) DLL files copied" -ForegroundColor Green

Write-Host "Step 4: Creating archives..." -ForegroundColor Yellow

# Archive Critical (petit, rapide CI/CD)
if ($criticalFound.Count -gt 0) {
    Compress-Archive -Path "$OutputCritical\*" -DestinationPath $ArchiveCritical -Force
    $criticalSize = (Get-Item $ArchiveCritical).Length
    $criticalSizeMB = [math]::Round($criticalSize / 1MB, 2)
    Write-Host "   📦 Critical archive: $ArchiveCritical ($criticalSizeMB MB)" -ForegroundColor Green
}

# Archive ModSDK (complet, pour développement avancé)
if ($modSDKFound.Count -gt 0) {
    Compress-Archive -Path "$OutputModSDK\*" -DestinationPath $ArchiveModSDK -Force
    $modSDKSize = (Get-Item $ArchiveModSDK).Length
    $modSDKSizeMB = [math]::Round($modSDKSize / 1MB, 2)
    Write-Host "   📦 ModSDK archive: $ArchiveModSDK ($modSDKSizeMB MB)" -ForegroundColor Green
}

# Nettoyer le répertoire temporaire
Remove-Item $TempOutputPath -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "🎯 Results Summary:" -ForegroundColor Cyan
Write-Host ""

Write-Host "Critical Set (Basic Compilation):" -ForegroundColor Yellow
Write-Host "   ✅ DLL Count: $($criticalFound.Count)/$($CriticalDLLs.Count)" -ForegroundColor Green
Write-Host "   📁 Directory: $OutputCritical" -ForegroundColor Blue
Write-Host "   📦 Archive: $ArchiveCritical" -ForegroundColor Blue

Write-Host "ModSDK Set (Comprehensive APIs):" -ForegroundColor Yellow  
Write-Host "   ✅ DLL Count: $($modSDKFound.Count)/$($allModSDKDLLs.Count)" -ForegroundColor Green
Write-Host "   📁 Directory: $OutputModSDK" -ForegroundColor Blue
Write-Host "   📦 Archive: $ArchiveModSDK" -ForegroundColor Blue

if ($modSDKMissing.Count -gt 0) {
    Write-Host ""
    Write-Host "Missing ModSDK DLLs (may not be critical):" -ForegroundColor Yellow
    foreach ($dll in $modSDKMissing) {
        Write-Host "     - $dll" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "🚀 Deployment Strategy:" -ForegroundColor Magenta
Write-Host "   • Use Critical set for basic CI/CD (faster builds)" -ForegroundColor White
Write-Host "   • Use ModSDK set for comprehensive development" -ForegroundColor White
Write-Host "   • Consider committing just archives vs full directories" -ForegroundColor White

# Afficher les tailles comparées
$fullDLLCount = (Get-ChildItem -Path "GameLibs" -Filter "*.dll" | Measure-Object).Count
Write-Host ""
Write-Host "🎯 Optimization Impact:" -ForegroundColor Magenta
Write-Host "   Original: $fullDLLCount DLL files (full game)" -ForegroundColor Gray
Write-Host "   Critical: $($criticalFound.Count) DLL files (basic compilation)" -ForegroundColor Gray
Write-Host "   ModSDK: $($modSDKFound.Count) DLL files (comprehensive APIs)" -ForegroundColor Gray
$criticalReduction = [math]::Round((1 - $criticalFound.Count / $fullDLLCount) * 100, 1)
$modSDKReduction = [math]::Round((1 - $modSDKFound.Count / $fullDLLCount) * 100, 1)
Write-Host "   Critical reduction: $criticalReduction%" -ForegroundColor Green
Write-Host "   ModSDK reduction: $modSDKReduction%" -ForegroundColor Green