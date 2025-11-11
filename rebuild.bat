@echo off
echo ================================================
echo ZplRenderer - Complete Rebuild Script
echo ================================================
echo.

echo [1/5] Closing Visual Studio processes...
taskkill /F /IM devenv.exe 2>nul
timeout /t 2 /nobreak >nul

echo [2/5] Cleaning cache folders...
rmdir /s /q .vs 2>nul
rmdir /s /q ZplRenderer\bin 2>nul
rmdir /s /q ZplRenderer\obj 2>nul
rmdir /s /q ZplRenderer.Console\bin 2>nul
rmdir /s /q ZplRenderer.Console\obj 2>nul

echo [3/5] Publishing console app...
dotnet publish ZplRenderer.Console\ZplRenderer.Console.csproj -c Release -r win-x64 --self-contained true
if errorlevel 1 goto error

echo [4/5] Restoring main project...
dotnet restore ZplRenderer\ZplRenderer.csproj --force
if errorlevel 1 goto error

echo [5/5] Building solution...
dotnet build ZplRenderer.sln --configuration Release
if errorlevel 1 goto error

echo.
echo ================================================
echo BUILD SUCCEEDED!
echo ================================================
echo.
echo Output files:
dir /b ZplRenderer\bin\Release\net40\ZplRenderer.dll 2>nul
dir /b ZplRenderer\bin\Release\net8.0\ZplRenderer.dll 2>nul
echo.
pause
exit /b 0

:error
echo.
echo ================================================
echo BUILD FAILED!
echo ================================================
echo.
pause
exit /b 1
