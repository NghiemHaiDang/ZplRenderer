# Build script for ZplRenderer with .NET 4.0 support
# This script builds the console app first, then embeds it into the .NET 4.0 DLL

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Building ZplRenderer with Embedded Console App" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan

# Step 1: Clean previous builds
Write-Host "`n[Step 1] Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean ZplRenderer.sln --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "Clean failed!" -ForegroundColor Red
    exit 1
}

# Step 2: Restore packages
Write-Host "`n[Step 2] Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore ZplRenderer.sln
if ($LASTEXITCODE -ne 0) {
    Write-Host "Restore failed!" -ForegroundColor Red
    exit 1
}

# Step 3: Build and publish console app as self-contained single file
Write-Host "`n[Step 3] Building console app (self-contained, single-file)..." -ForegroundColor Yellow
dotnet publish ZplRenderer\ZplRenderer.Console\ZplRenderer.Console.csproj `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output ZplRenderer\ZplRenderer.Console\bin\Release\net8.0\win-x64\publish `
    /p:PublishSingleFile=true `
    /p:IncludeNativeLibrariesForSelfExtract=true

if ($LASTEXITCODE -ne 0) {
    Write-Host "Console app build failed!" -ForegroundColor Red
    exit 1
}

$consoleExePath = "ZplRenderer\ZplRenderer.Console\bin\Release\net8.0\win-x64\publish\ZplRenderer.Console.exe"
if (Test-Path $consoleExePath) {
    $fileSize = (Get-Item $consoleExePath).Length / 1MB
    Write-Host "  ✓ Console app built successfully: $([math]::Round($fileSize, 2)) MB" -ForegroundColor Green
} else {
    Write-Host "  ✗ Console app executable not found!" -ForegroundColor Red
    exit 1
}

# Step 4: Build main DLL (both .NET 4.0 and .NET 8.0 versions)
Write-Host "`n[Step 4] Building ZplRenderer DLL (multi-target: net40 + net8.0)..." -ForegroundColor Yellow
dotnet build ZplRenderer\ZplRenderer.csproj --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Host "DLL build failed!" -ForegroundColor Red
    exit 1
}

# Step 5: Verify outputs
Write-Host "`n[Step 5] Verifying outputs..." -ForegroundColor Yellow

$net40Dll = "ZplRenderer\bin\Release\net40\ZplRenderer.dll"
$net80Dll = "ZplRenderer\bin\Release\net8.0\ZplRenderer.dll"

if (Test-Path $net40Dll) {
    $net40Size = (Get-Item $net40Dll).Length / 1MB
    Write-Host "  ✓ .NET 4.0 DLL: $([math]::Round($net40Size, 2)) MB" -ForegroundColor Green
} else {
    Write-Host "  ✗ .NET 4.0 DLL not found!" -ForegroundColor Red
}

if (Test-Path $net80Dll) {
    $net80Size = (Get-Item $net80Dll).Length / 1MB
    Write-Host "  ✓ .NET 8.0 DLL: $([math]::Round($net80Size, 2)) MB" -ForegroundColor Green
} else {
    Write-Host "  ✗ .NET 8.0 DLL not found!" -ForegroundColor Red
}

# Step 6: Summary
Write-Host "`n=====================================" -ForegroundColor Cyan
Write-Host "Build Summary" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "For .NET 4.0 projects, use: " -NoNewline
Write-Host "$net40Dll" -ForegroundColor Green
Write-Host "`nThis DLL contains:" -ForegroundColor White
Write-Host "  • Wrapper code that extracts and runs console app" -ForegroundColor Gray
Write-Host "  • Embedded console app (self-contained .NET 8.0)" -ForegroundColor Gray
Write-Host "  • No external dependencies required" -ForegroundColor Gray
Write-Host "`nUsage from .NET 4.0:" -ForegroundColor White
Write-Host '  var service = new ZplRenderService();' -ForegroundColor Gray
Write-Host '  service.ConvertZplToFile("input.zpl", "output", "pdf");' -ForegroundColor Gray
Write-Host "`n=====================================" -ForegroundColor Cyan
