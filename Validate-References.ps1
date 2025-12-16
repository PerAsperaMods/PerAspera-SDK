# Valider les références DLL selon le mode de compilation

param(
    [ValidateSet("Full", "Stub")]
    [string]$Mode = "Full",
    [switch]$Verbose = $false
)

Write-Host "🔍 Validating DLL references in mode: $Mode" -ForegroundColor Cyan

$errors = @()
$warnings = @()
$success = @()

if ($Mode -eq "Stub") {
    Write-Host "📋 Checking CI/CD stub references..." -ForegroundColor Blue
    
    # Vérifier que le répertoire stubs existe
    $stubsPath = "F:\ModPeraspera\SDK\stubs"
    if (-not (Test-Path $stubsPath)) {
        $errors += "❌ Stubs directory not found: $stubsPath"
        Write-Error "Stubs directory missing. Run .\Generate-DLLStubs.ps1 first."
        return $false
    }
    
    # DLL stubs requis pour la compilation
    $requiredStubs = @(
        @{ Name = "Assembly-CSharp.dll"; Critical = $true },
        @{ Name = "UnityEngine.CoreModule.dll"; Critical = $true },
        @{ Name = "UnityEngine.dll"; Critical = $true },
        @{ Name = "Il2CppSystem.dll"; Critical = $false },
        @{ Name = "Il2Cppmscorlib.dll"; Critical = $false }
    )
    
    foreach ($stub in $requiredStubs) {
        $stubPath = Join-Path $stubsPath $stub.Name
        if (Test-Path $stubPath) {
            $fileInfo = Get-Item $stubPath
            $sizeKB = [math]::Round($fileInfo.Length / 1KB, 2)
            $success += "✅ $($stub.Name) ($sizeKB KB)"
            
            if ($Verbose) {
                Write-Host "  ✅ Found: $($stub.Name) ($sizeKB KB)" -ForegroundColor Green
            }
        } else {
            if ($stub.Critical) {
                $errors += "❌ Missing critical stub: $($stub.Name)"
                Write-Error "Critical stub missing: $($stub.Name)"
            } else {
                $warnings += "⚠️ Missing optional stub: $($stub.Name)"
                if ($Verbose) {
                    Write-Warning "Optional stub missing: $($stub.Name)"
                }
            }
        }
    }
    
    # Vérifier les variables d'environnement CI/CD
    if ($env:GITHUB_ACTIONS -eq "true") {
        $success += "✅ GitHub Actions environment detected"
        Write-Host "🔗 GitHub Actions environment detected - using stubs" -ForegroundColor Blue
    } else {
        $warnings += "⚠️ Not in CI/CD environment, but using stub mode"
        Write-Warning "Not in CI/CD environment but using stubs"
    }
    
} else {
    Write-Host "📋 Checking local development references..." -ForegroundColor Blue
    
    # Vérifier que le répertoire GameLibs existe
    $gameLibsPath = "F:\ModPeraspera\SDK\GameLibs"
    if (-not (Test-Path $gameLibsPath)) {
        $errors += "❌ GameLibs directory not found: $gameLibsPath"
        Write-Error "GameLibs directory missing. Run .\fix-gamelibs-references.ps1 first."
        return $false
    }
    
    # DLL principales requises pour le développement
    $requiredGameLibs = @(
        @{ Name = "Assembly-CSharp.dll"; Critical = $true },
        @{ Name = "UnityEngine.CoreModule.dll"; Critical = $true },
        @{ Name = "UnityEngine.dll"; Critical = $true },
        @{ Name = "Il2CppSystem.dll"; Critical = $true },
        @{ Name = "Il2Cppmscorlib.dll"; Critical = $true },
        @{ Name = "DOTween.dll"; Critical = $false },
        @{ Name = "Newtonsoft.Json.dll"; Critical = $false }
    )
    
    foreach ($lib in $requiredGameLibs) {
        $libPath = Join-Path $gameLibsPath $lib.Name
        if (Test-Path $libPath) {
            $fileInfo = Get-Item $libPath
            $sizeMB = [math]::Round($fileInfo.Length / 1MB, 2)
            $success += "✅ $($lib.Name) ($sizeMB MB)"
            
            if ($Verbose) {
                Write-Host "  ✅ Found: $($lib.Name) ($sizeMB MB)" -ForegroundColor Green
            }
        } else {
            if ($lib.Critical) {
                $errors += "❌ Missing critical GameLib: $($lib.Name)"
                Write-Error "Critical GameLib missing: $($lib.Name)"
            } else {
                $warnings += "⚠️ Missing optional GameLib: $($lib.Name)"
                if ($Verbose) {
                    Write-Warning "Optional GameLib missing: $($lib.Name)"
                }
            }
        }
    }
    
    # Vérifier que nous ne sommes pas en environnement CI/CD
    if ($env:GITHUB_ACTIONS -eq "true") {
        $warnings += "⚠️ In CI/CD environment but using full mode"
        Write-Warning "In GitHub Actions but using full GameLibs mode"
    } else {
        $success += "✅ Local development environment detected"
        Write-Host "🔧 Local development environment - using full GameLibs" -ForegroundColor Blue
    }
}

# Résumé final
Write-Host "`n📊 Validation Summary:" -ForegroundColor Cyan
Write-Host "   Mode: $Mode" -ForegroundColor Gray

if ($success.Count -gt 0) {
    Write-Host "   ✅ Success ($($success.Count)):" -ForegroundColor Green
    foreach ($item in $success) {
        Write-Host "      $item" -ForegroundColor Green
    }
}

if ($warnings.Count -gt 0) {
    Write-Host "   ⚠️ Warnings ($($warnings.Count)):" -ForegroundColor Yellow
    foreach ($item in $warnings) {
        Write-Host "      $item" -ForegroundColor Yellow
    }
}

if ($errors.Count -gt 0) {
    Write-Host "   ❌ Errors ($($errors.Count)):" -ForegroundColor Red
    foreach ($item in $errors) {
        Write-Host "      $item" -ForegroundColor Red
    }
    Write-Host "`n💥 Validation failed! Fix errors before proceeding." -ForegroundColor Red
    return $false
} else {
    Write-Host "`n🎯 Validation successful! References are properly configured." -ForegroundColor Green
    return $true
}