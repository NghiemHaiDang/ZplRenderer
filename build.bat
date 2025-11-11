@echo off
echo =====================================
echo Building ZplRenderer with Embedded Console App
echo =====================================

echo.
echo [Step 1] Cleaning previous builds...
dotnet clean ZplRenderer.sln --configuration Release
if errorlevel 1 goto :error

echo.
echo [Step 2] Restoring NuGet packages...
dotnet restore ZplRenderer.sln
if errorlevel 1 goto :error

echo.
echo [Step 3] Building console app (self-contained, single-file)...
dotnet publish ZplRenderer\ZplRenderer.Console\ZplRenderer.Console.csproj --configuration Release --runtime win-x64 --self-contained true --output ZplRenderer\ZplRenderer.Console\bin\Release\net8.0\win-x64\publish /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
if errorlevel 1 goto :error

echo.
echo [Step 4] Building ZplRenderer DLL (multi-target: net40 + net8.0)...
dotnet build ZplRenderer\ZplRenderer.csproj --configuration Release
if errorlevel 1 goto :error

echo.
echo =====================================
echo Build completed successfully!
echo =====================================
echo.
echo .NET 4.0 DLL: ZplRenderer\bin\Release\net40\ZplRenderer.dll
echo .NET 8.0 DLL: ZplRenderer\bin\Release\net8.0\ZplRenderer.dll
echo.
goto :end

:error
echo.
echo =====================================
echo Build failed!
echo =====================================
exit /b 1

:end
