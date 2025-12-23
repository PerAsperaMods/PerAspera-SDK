@echo off
REM ================================================
REM Script de deploiement rapide des DLLs SDK
REM ================================================

echo.
echo Deploiement des DLLs SDK Per Aspera...
echo.

cd /d "%~dp0"

if "%1"=="--force" (
    powershell.exe -NoProfile -ExecutionPolicy Bypass -File "Deploy-SDK-Quick.ps1" -Force
) else (
    powershell.exe -NoProfile -ExecutionPolicy Bypass -File "Deploy-SDK-Quick.ps1"
)

if errorlevel 1 (
    echo.
    echo Echec du deploiement!
    pause
    exit /b 1
)

echo.
echo Deploiement termine!
echo.
echo Vous pouvez maintenant lancer Per Aspera pour tester vos mods.
echo.
pause