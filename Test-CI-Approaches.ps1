# Test des performances : GameLibsMaker vs AssemblyPublicizer
# Compare les deux approches pour CI/CD

param(
    [switch]$TestAssemblyPublicizer,
    [switch]$TestGameLibsMaker,
    [switch]$Both
)

Write-Host "=== Per Aspera CI/CD Approach Comparison ===" -ForegroundColor Green

if ($Both -or $TestAssemblyPublicizer) {
    Write-Host ""
    Write-Host "🚀 Testing AssemblyPublicizer Approach..." -ForegroundColor Yellow
    
    # Backup original Directory.Build.props
    Copy-Item "Directory.Build.props" "Directory.Build.props.backup" -Force
    
    # Create AssemblyPublicizer configuration
    $assemblypublicizerConfig = @'
<Project>
  <!-- 🎮 Per Aspera SDK - AssemblyPublicizer Modern Approach -->
  
  <PropertyGroup>
    <SdkRoot>$(MSBuildThisFileDirectory)</SdkRoot>
    <IsCIBuild Condition="'$(GITHUB_ACTIONS)' == 'true'">true</IsCIBuild>
    <IsCIBuild Condition="'$(IsCIBuild)' == ''">false</IsCIBuild>
    
    <RestoreAdditionalProjectSources>https://nuget.bepinex.dev/v3/index.json</RestoreAdditionalProjectSources>
    
    <!-- Per Aspera Game Detection -->
    <PerAsperaGamePath Condition="'$(PerAsperaGamePath)' == '' AND Exists('F:\SteamLibrary\steamapps\common\Per Aspera')">F:\SteamLibrary\steamapps\common\Per Aspera</PerAsperaGamePath>
  </PropertyGroup>
  
  <!-- Modern AssemblyPublicizer for CI/CD -->
  <ItemGroup Condition="'$(IsCIBuild)' == 'true'">
    <PackageReference Include="BepInEx.AssemblyPublicizer.MSBuild" Version="0.4.2" PrivateAssets="all" />
    
    <!-- Publicize game assemblies automatically -->
    <Publicize Include="Assembly-CSharp" />
    <Publicize Include="ScriptsAssembly" />
    <Publicize Include="UnityEngine.CoreModule" />
    <Publicize Include="UnityEngine.IMGUIModule" />
    <Publicize Include="UnityEngine" />
    <Publicize Include="Il2Cppmscorlib" />
    <Publicize Include="Il2CppSystem" />
  </ItemGroup>
  
  <!-- Reference game assemblies directly -->
  <ItemGroup Condition="'$(IsCIBuild)' == 'true'">
    <Reference Include="$(PerAsperaGamePath)\Assembly-CSharp.dll" Publicize="true" />
    <Reference Include="$(PerAsperaGamePath)\ScriptsAssembly.dll" Publicize="true" />
    <Reference Include="$(PerAsperaGamePath)\BepInEx\interop\UnityEngine.CoreModule.dll" Publicize="true" />
    <Reference Include="$(PerAsperaGamePath)\BepInEx\interop\UnityEngine.IMGUIModule.dll" Publicize="true" />
    <Reference Include="$(PerAsperaGamePath)\BepInEx\interop\UnityEngine.dll" Publicize="true" />
    <Reference Include="$(PerAsperaGamePath)\BepInEx\interop\Il2Cppmscorlib.dll" Publicize="true" />
    <Reference Include="$(PerAsperaGamePath)\BepInEx\interop\Il2CppSystem.dll" Publicize="true" />
  </ItemGroup>
  
  <!-- Local development: use existing GameLibs -->
  <PropertyGroup Condition="'$(IsCIBuild)' != 'true'">
    <PerAsperaGameLibs>$(SdkRoot)GameLibs</PerAsperaGameLibs>
  </PropertyGroup>
  
  <!-- Local development import -->
  <Import Project="$(SdkRoot)GameLibs\GameLibs.props" Condition="'$(IsCIBuild)' != 'true' AND Exists('$(SdkRoot)GameLibs\GameLibs.props')" />
</Project>
'@
    
    Set-Content -Path "Directory.Build.props" -Value $assemblypublicizerConfig -Encoding UTF8
    
    Write-Host "   📝 AssemblyPublicizer configuration applied" -ForegroundColor Green
    
    # Test build
    Write-Host "   🔨 Testing build with AssemblyPublicizer..." -ForegroundColor Cyan
    $env:GITHUB_ACTIONS = "true"
    $startTime = Get-Date
    
    try {
        $buildResult = dotnet build --configuration Release --verbosity minimal 2>&1
        $endTime = Get-Date
        $duration = ($endTime - $startTime).TotalSeconds
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   ✅ AssemblyPublicizer build: SUCCESS ($([math]::Round($duration, 2))s)" -ForegroundColor Green
        } else {
            Write-Host "   ❌ AssemblyPublicizer build: FAILED ($([math]::Round($duration, 2))s)" -ForegroundColor Red
            Write-Host "   Error details:" -ForegroundColor Yellow
            Write-Host $buildResult
        }
    }
    finally {
        $env:GITHUB_ACTIONS = $null
    }
    
    # Restore original config
    Move-Item "Directory.Build.props.backup" "Directory.Build.props" -Force
    Write-Host "   📄 Original configuration restored" -ForegroundColor Blue
}

if ($Both -or $TestGameLibsMaker) {
    Write-Host ""
    Write-Host "🔧 Testing GameLibsMaker Approach (current)..." -ForegroundColor Yellow
    
    # Test current approach
    Write-Host "   🔨 Testing build with stripped DLLs..." -ForegroundColor Cyan
    $env:GITHUB_ACTIONS = "true"
    $startTime = Get-Date
    
    try {
        $buildResult = dotnet build --configuration Release --verbosity minimal 2>&1
        $endTime = Get-Date
        $duration = ($endTime - $startTime).TotalSeconds
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "   ✅ GameLibsMaker build: SUCCESS ($([math]::Round($duration, 2))s)" -ForegroundColor Green
        } else {
            Write-Host "   ❌ GameLibsMaker build: FAILED ($([math]::Round($duration, 2))s)" -ForegroundColor Red
        }
    }
    finally {
        $env:GITHUB_ACTIONS = $null
    }
}

Write-Host ""
Write-Host "=== Comparison Summary ===" -ForegroundColor Cyan
Write-Host ""

Write-Host "🚀 AssemblyPublicizer Benefits:" -ForegroundColor Green
Write-Host "   • Zero files to commit (automatic publicization)" -ForegroundColor White
Write-Host "   • NuGet package integration (modern MSBuild)" -ForegroundColor White  
Write-Host "   • Automatic CI/CD (no manual script execution)" -ForegroundColor White
Write-Host "   • Cleaner Git history (no binary DLL commits)" -ForegroundColor White

Write-Host ""
Write-Host "🔧 GameLibsMaker Benefits:" -ForegroundColor Yellow
Write-Host "   • Pre-stripped DLLs (potentially faster builds)" -ForegroundColor White
Write-Host "   • Full control over which assemblies to include" -ForegroundColor White
Write-Host "   • Known working configuration" -ForegroundColor White
Write-Host "   • Offline-capable (no NuGet download needed)" -ForegroundColor White

Write-Host ""
Write-Host "💡 Recommendation:" -ForegroundColor Magenta
Write-Host "   Try AssemblyPublicizer for modern CI/CD workflow" -ForegroundColor White
Write-Host "   Keep GameLibsMaker as fallback for local development" -ForegroundColor White