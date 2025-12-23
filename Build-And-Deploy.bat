@echo off
REM ================================================
REM Build et deploiement automatique du SDK
REM ================================================

echo.
echo Build et deploiement du SDK Per Aspera...
echo.

cd /d "%~dp0"

set BUILD_CONFIG=Debug
set FORCE_DEPLOY=
set DEPLOY_ENABLED=1

REM Parse des arguments
:parse_args
if "%1"=="Release" set BUILD_CONFIG=Release
if "%1"=="Debug" set BUILD_CONFIG=Debug
if "%1"=="--force" set FORCE_DEPLOY=-Force
if "%1"=="--no-deploy" set DEPLOY_ENABLED=0
shift
if not "%1"=="" goto parse_args

echo 📋 Configuration: %BUILD_CONFIG%
if %DEPLOY_ENABLED%==1 (
    echo 🚀 Déploiement: Activé
    if defined FORCE_DEPLOY echo 💪 Mode: Force
) else (
    echo ⏭️  Déploiement: Désactivé
)
echo.

REM Build du SDK
if %DEPLOY_ENABLED%==1 (
    powershell.exe -NoProfile -ExecutionPolicy Bypass -File "Build-SDK.ps1" -Configuration %BUILD_CONFIG% -Deploy %FORCE_DEPLOY%
) else (
    powershell.exe -NoProfile -ExecutionPolicy Bypass -File "Build-SDK.ps1" -Configuration %BUILD_CONFIG%
)

if errorlevel 1 (
    echo.
    echo ❌ Échec du build/déploiement!
    pause
    exit /b 1
)

echo.
echo 🎉 Opération terminée avec succès!
if %DEPLOY_ENABLED%==1 echo 💡 Le SDK est maintenant déployé dans Per Aspera.
echo.
pause