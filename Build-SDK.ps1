# 🔧 Build Script pour PerAspera SDK

Write-Host "🔧 Building PerAspera SDK..." -ForegroundColor Cyan

# Configuration
$SolutionPath = "$PSScriptRoot\PerAspera.SDK.sln"
$BuildConfiguration = if ($args[0]) { $args[0] } else { "Debug" }

Write-Host "📁 Solution: $SolutionPath" -ForegroundColor Yellow
Write-Host "⚙️ Configuration: $BuildConfiguration" -ForegroundColor Yellow

try {
    # Build du SDK
    Write-Host "`n🔨 Building SDK solution..." -ForegroundColor Green
    dotnet build $SolutionPath --configuration $BuildConfiguration --verbosity minimal
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✅ SDK build successful!" -ForegroundColor Green
        
        # Afficher les DLLs générées
        Write-Host "`n📦 Generated assemblies:" -ForegroundColor Cyan
        Get-ChildItem "$PSScriptRoot\**\bin\$BuildConfiguration\**\*.dll" | ForEach-Object {
            Write-Host "   📄 $($_.Name) - $($_.Length) bytes" -ForegroundColor Gray
        }
        
        Write-Host "`n🎯 SDK ready for mod development!" -ForegroundColor Green
        Write-Host "   Mods can reference: PerAspera.ModSDK.dll" -ForegroundColor Gray
        Write-Host "   Which includes: PerAspera.Core + PerAspera.GameAPI" -ForegroundColor Gray
        
    } else {
        Write-Host "`n❌ SDK build failed!" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "`n💥 Build error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`n🏁 Done!" -ForegroundColor Cyan