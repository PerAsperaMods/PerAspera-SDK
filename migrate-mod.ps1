# PerAspera Mod Migration Script
# Transforms legacy AsperaClass usage to new ModSDK

param(
    [Parameter(Mandatory=$true)]
    [string]$ModPath,
    
    [switch]$DryRun,
    
    [switch]$BackupFirst
)

Write-Host "?? PerAspera Mod Migration Tool" -ForegroundColor Cyan
Write-Host "Converting AsperaClass ? ModSDK in: $ModPath" -ForegroundColor Green

if (-not (Test-Path $ModPath)) {
    Write-Error "? Mod path not found: $ModPath"
    exit 1
}

# Create backup if requested
if ($BackupFirst) {
    $backupPath = "$ModPath.backup-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
    Write-Host "?? Creating backup: $backupPath" -ForegroundColor Yellow
    if (-not $DryRun) {
        Copy-Item -Path $ModPath -Destination $backupPath -Recurse
    }
}

# Migration patterns
$migrations = @{
    # Using statements
    'using Common;' = 'using PerAspera.ModSDK;'
    'using PerAspera\.Core;' = '// using PerAspera.Core; // ? Now included in ModSDK'
    'using AsperaClass;' = 'using PerAspera.ModSDK;'
    
    # MainWrapper ? ModSDK
    'MainWrapper\.Universe\.GetPlanet\(\)' = 'ModSDK.Universe.GetPlanetMirror()'
    'MainWrapper\.Universe\.GetPlayerFaction\(\)' = 'ModSDK.Universe.GetPlayerFaction()'
    'MainWrapper\.IsCommandReady' = 'ModSDK.Universe.IsGameReady()'
    
    # EventBus ? Events
    'EventBus\.Instance\.Subscribe<([^>]+)>\(([^)]+)\)' = 'ModSDK.Events.Subscribe<$1>($2)'
    'EventBus\.Instance\.Publish<([^>]+)>\(([^)]+)\)' = 'ModSDK.Events.Publish<$1>($2)'
    'EventBus\.Instance\.Unsubscribe<([^>]+)>\(([^)]+)\)' = 'ModSDK.Events.Unsubscribe<$1>($2)'
    
    # MirrorEventBus ? Events  
    'MirrorEventBus\.Subscribe\(([^,]+),\s*([^)]+)\)' = 'ModSDK.Events.Subscribe($1, $2)'
    'MirrorEventBus\.Publish\(([^,]+),\s*([^)]+)\)' = 'ModSDK.Events.Publish($1, $2)'
    
    # Direct class usage
    'new MirrorUniverse\(\)' = 'ModSDK.Universe.GetPlanetMirror() // ? Or use MirrorUniverse.Shared'
    'new MirrorPlanet\(' = 'new PerAspera.GameAPI.MirrorPlanet('
    'new MirrorResourceType\(' = 'new PerAspera.GameAPI.MirrorResourceType('
    
    # Command helpers
    'CommandHelper\.GiveResourceToPlayer' = 'PerAspera.GameAPI.CommandHelper.GiveResourceToPlayer'
    'ResourceHelper\.GetResourceType' = 'PerAspera.GameAPI.ResourceHelper.GetResourceType'
    
    # Specific ClimatAspera fixes
    'ModSDK\.Events\.OnClimateAnalysis\(([^,]+),\s*([^)]+)\)' = 'ModSDK.Events.Publish($1, $2)'
}

# Find C# files to process
$csFiles = Get-ChildItem -Path $ModPath -Filter "*.cs" -Recurse | Where-Object {
    $_.FullName -notmatch '\\obj\\' -and $_.FullName -notmatch '\\bin\\'
}

Write-Host "?? Found $($csFiles.Count) C# files to process" -ForegroundColor Green

$totalChanges = 0

foreach ($file in $csFiles) {
    Write-Host "?? Processing: $($file.Name)" -ForegroundColor Cyan
    
    $content = Get-Content -Path $file.FullName -Raw
    $originalContent = $content
    $fileChanges = 0
    
    foreach ($pattern in $migrations.Keys) {
        $replacement = $migrations[$pattern]
        
        # Use regex replacement
        $newContent = $content -replace $pattern, $replacement
        
        if ($newContent -ne $content) {
            $matches = [regex]::Matches($content, $pattern)
            $fileChanges += $matches.Count
            $content = $newContent
            
            Write-Host "  ? Applied pattern: $pattern ($($matches.Count) times)" -ForegroundColor Green
        }
    }
    
    # Additional project reference updates for .csproj files
    if ($file.Extension -eq '.csproj') {
        $content = $content -replace '<ProjectReference Include=".*AsperaClass.*"', '<!-- ? Migrated to ModSDK: <ProjectReference Include="..\..\SDK\PerAspera.ModSDK\PerAspera.ModSDK.csproj" -->'
    }
    
    if ($fileChanges -gt 0) {
        Write-Host "  ?? Total changes in file: $fileChanges" -ForegroundColor Yellow
        $totalChanges += $fileChanges
        
        if (-not $DryRun) {
            Set-Content -Path $file.FullName -Value $content -NoNewline
        }
    } else {
        Write-Host "  ? No changes needed" -ForegroundColor Gray
    }
}

# Summary
Write-Host "`n?? Migration Summary:" -ForegroundColor Cyan
Write-Host "?? Files processed: $($csFiles.Count)" -ForegroundColor White  
Write-Host "?? Total changes: $totalChanges" -ForegroundColor White

if ($DryRun) {
    Write-Host "?? DRY RUN - No files were actually modified" -ForegroundColor Yellow
    Write-Host "Run without -DryRun to apply changes" -ForegroundColor Yellow
} else {
    Write-Host "? Migration completed successfully!" -ForegroundColor Green
}

Write-Host "`n?? Manual steps still required:" -ForegroundColor Yellow
Write-Host "1. Update project references to PerAspera.ModSDK" -ForegroundColor White
Write-Host "2. Test compilation and functionality" -ForegroundColor White  
Write-Host "3. Remove old AsperaClass references" -ForegroundColor White
Write-Host "4. Update using statements if needed" -ForegroundColor White

Write-Host "`n?? Migration complete! Your mod now uses the modern PerAspera SDK." -ForegroundColor Green