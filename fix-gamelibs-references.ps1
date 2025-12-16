# Fix GameLibs.props references to prevent local copy
$filePath = "F:\ModPeraspera\SDK\GameLibs\GameLibs.props"
$content = Get-Content $filePath -Raw

# Pattern pour remplacer chaque référence 
$pattern = '(<Reference Include="[^"]+">[\r\n\s]*<HintPath>[^<]+</HintPath>)([\r\n\s]*</Reference>)'
$replacement = '$1${2}            <Private>false</Private>$2'

# Appliquer le remplacement
$newContent = $content -replace $pattern, $replacement

# Sauvegarder
$newContent | Set-Content $filePath -NoNewline

Write-Host "✅ Fixed GameLibs.props references to prevent DLL copying" -ForegroundColor Green
Write-Host "🔧 All game DLL references now have <Private>false</Private>" -ForegroundColor Yellow