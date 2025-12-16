# Initialize SDK as Independent Git Repository
# Connects to https://github.com/PerAsperaMods/PerAspera-SDK

param(
    [string]$GitHubRepo = "PerAsperaMods/PerAspera-SDK",
    [switch]$DryRun = $false
)

$ErrorActionPreference = "Stop"
$SDKRoot = $PSScriptRoot

Write-Host "================================" -ForegroundColor Cyan
Write-Host "SDK Git Repository Init" -ForegroundColor Cyan
Write-Host "================================`n" -ForegroundColor Cyan

# Check Git
Write-Host "Checking Git..." -ForegroundColor Green
$gitVersion = git --version
Write-Host "OK: $gitVersion`n" -ForegroundColor Green

# Check .git exists
Write-Host "Checking for existing .git..." -ForegroundColor Green
$gitDir = Join-Path $SDKRoot ".git"
if (Test-Path $gitDir) {
    Write-Host "WARNING: .git already exists" -ForegroundColor Yellow
    $continue = Read-Host "Delete and reinitialize? (type 'yes' to confirm)"
    if ($continue -ne "yes") {
        exit 0
    }
    Remove-Item $gitDir -Recurse -Force
}
Write-Host "OK`n" -ForegroundColor Green

# Create .gitignore
Write-Host "Creating .gitignore..." -ForegroundColor Green
$gitignore = @"
bin/
obj/
packages/
*.nupkg
*.snupkg
.vs/
.vscode/
*.user
.idea/
_ReSharper*/
TestResults/
GameLibs-Stripped/
_Archive/
test-game-manual/
*.log
*.tmp
.DS_Store
Thumbs.db
"@

Set-Content (Join-Path $SDKRoot ".gitignore") $gitignore -Encoding UTF8
Write-Host "OK`n" -ForegroundColor Green

# Init Git
Write-Host "Initializing Git repository..." -ForegroundColor Green
Push-Location $SDKRoot

git init
git branch -M main
Write-Host "OK`n" -ForegroundColor Green

# Configure user
Write-Host "Configuring Git user..." -ForegroundColor Green
$globalName = git config --global user.name 2>$null
$globalEmail = git config --global user.email 2>$null

if ($globalName -and $globalEmail) {
    Write-Host "Using global config: $globalName <$globalEmail>`n" -ForegroundColor Green
} else {
    Write-Host "WARNING: No global Git config found" -ForegroundColor Yellow
    $name = Read-Host "Enter your Git name"
    $email = Read-Host "Enter your Git email"
    git config user.name "$name"
    git config user.email "$email"
    Write-Host ""
}

# Initial commit
Write-Host "Creating initial commit..." -ForegroundColor Green
git add .
git commit -m "Initial SDK repository setup"
Write-Host "OK`n" -ForegroundColor Green

# Add remote
Write-Host "Adding GitHub remote..." -ForegroundColor Green
$remoteUrl = "https://github.com/$GitHubRepo.git"
git remote add origin $remoteUrl
Write-Host "Remote: $remoteUrl`n" -ForegroundColor Green

# Summary
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Setup Complete!" -ForegroundColor Cyan
Write-Host "================================`n" -ForegroundColor Cyan

Write-Host "Repository: $SDKRoot" -ForegroundColor White
Write-Host "GitHub: https://github.com/$GitHubRepo" -ForegroundColor White
Write-Host "Branch: main`n" -ForegroundColor White

Write-Host "Next step:" -ForegroundColor Cyan
Write-Host "  git push -u origin main" -ForegroundColor Gray
Write-Host ""

Pop-Location
