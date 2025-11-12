# Hướng Dẫn Build Project - ZplRenderer

## Tổng Quan

ZplRenderer là project multi-target framework:
- **.NET 8.0** - Sử dụng trực tiếp các thư viện hiện đại
- **.NET 4.0** - Wrapper cho tương thích với legacy applications

---

## 1. Yêu Cầu Môi Trường

### Phần Mềm Cần Thiết

| Phần mềm | Version | Mục đích |
|----------|---------|----------|
| **.NET SDK** | 8.0+ | Build .NET 8.0 targets |
| **.NET Framework** | 4.0+ SDK | Build .NET 4.0 target |
| **Visual Studio** | 2022+ (khuyến nghị) | IDE (optional) |
| **MSBuild** | 17.0+ | Build tool |
| **Git** | Latest | Version control (optional) |

### Kiểm Tra Môi Trường

```bash
# Kiểm tra .NET SDK
dotnet --version
# Output: 8.0.x hoặc cao hơn

# Kiểm tra .NET Framework
dir "C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.0"
# Folder này phải tồn tại

# Kiểm tra MSBuild
dotnet msbuild -version
# Output: 17.x.x
```

---

## 2. Cấu Trúc Project

```
ZplRenderer/
├── ZplRenderer.sln                      # Solution file
├── ZplRenderer.csproj                   # Main library project (multi-target)
│   ├── Target: net8.0                   # .NET 8.0 build
│   └── Target: net40                    # .NET 4.0 build
├── ZplRenderer.Console/
│   └── ZplRenderer.Console.csproj       # Console app (.NET 8.0 only)
├── Core/                                # Business logic
├── Infrastructure/                      # Implementation
├── Config/                              # Configuration
└── *.md                                 # Documentation
```

---

## 3. Quy Trình Build Tự Động

### 3.1 Build Đơn Giản (Khuyến Nghị)

```bash
# Di chuyển vào thư mục project
cd C:\DangNH30\LGCNS\ZplRenderer\ZplRenderer

# Build toàn bộ solution
dotnet build

# Hoặc build Release
dotnet build -c Release
```

**MSBuild sẽ tự động:**
1. Kiểm tra nếu `ZplRenderer.Console.exe` chưa tồn tại
2. Publish `ZplRenderer.Console` project (nếu cần)
3. Build `ZplRenderer` cho cả 2 targets (net8.0 và net40)
4. Embed Console.exe vào .NET 4.0 DLL

### 3.2 Output Files

```
bin/
├── Debug/  (hoặc Release/)
    ├── net8.0/
    │   ├── ZplRenderer.dll              # .NET 8.0 DLL (~200KB)
    │   ├── ZplRenderer.pdb              # Debug symbols
    │   ├── *.dll                        # Dependencies
    │   └── runtimes/                    # Native libraries
    │       └── win-x64/native/
    │           ├── libSkiaSharp.dll
    │           └── libHarfBuzzSharp.dll
    └── net40/
        ├── ZplRenderer.dll              # .NET 4.0 DLL (~26MB) ⭐ SELF-CONTAINED
        │                                # Chứa embedded Console.exe với .NET 8.0 runtime
        │                                # KHÔNG CẦN .NET 8.0 trên máy đích!
        ├── ZplRenderer.pdb              # Debug symbols
        └── *.dll                        # Minimal dependencies (chỉ cho .NET 4.0)
```

**Giải thích:**
- **net8.0/ZplRenderer.dll**: Cho ứng dụng .NET 8.0+, cần .NET 8.0 runtime
- **net40/ZplRenderer.dll**: Cho ứng dụng .NET 4.0+, tự chứa .NET 8.0 runtime, không cần cài thêm

---

## 4. Quy Trình Build Thủ Công (Advanced)

### 4.1 Build Từng Bước

#### Bước 1: Publish Console App

```bash
# Publish Console app thành single-file executable
dotnet publish ZplRenderer.Console/ZplRenderer.Console.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true

# Output: ZplRenderer.Console/bin/Release/net8.0/win-x64/publish/ZplRenderer.Console.exe
```

**Tham số giải thích:**
- `-c Release` - Build ở chế độ Release (optimized)
- `-r win-x64` - Target Windows 64-bit
- `--self-contained true` - Bao gồm .NET 8.0 runtime (~26MB)
- `-p:PublishSingleFile=true` - Đóng gói thành 1 file .exe

**⚠️ QUAN TRỌNG - Self-Contained Executable:**

File `ZplRenderer.Console.exe` là **self-contained**, có nghĩa:
- ✅ Chứa toàn bộ .NET 8.0 runtime bên trong
- ✅ Không cần máy đích cài .NET 8.0
- ✅ Chạy standalone trên Windows 64-bit
- ✅ File này sẽ được nhúng vào `net40/ZplRenderer.dll`

Khi ứng dụng .NET 4.0 gọi DLL:
1. DLL extract Console.exe ra `%TEMP%\ZplRenderer\`
2. Execute Console.exe (tự chứa .NET 8.0 runtime)
3. Console.exe xử lý ZPL và trả kết quả
4. **KHÔNG CẦN** .NET 8.0 trên máy đích!

#### Bước 2: Build Main Library

```bash
# Build cả 2 targets
dotnet build ZplRenderer.csproj -c Release

# Hoặc build riêng từng target
dotnet build ZplRenderer.csproj -c Release -f net8.0
dotnet build ZplRenderer.csproj -c Release -f net40
```

### 4.2 Kiểm Tra Embedded Resource

```bash
# Sử dụng ildasm hoặc dotnet tool
dotnet list ZplRenderer.csproj resources

# Hoặc kiểm tra file size
dir bin\Release\net40\ZplRenderer.dll
# Kích thước phải ~26MB (chứa embedded 26MB exe)
```

---

## 5. Build Configurations

### 5.1 Debug vs Release

| Configuration | Tối ưu hóa | Debug symbols | Kích thước | Khi nào dùng |
|---------------|------------|---------------|------------|--------------|
| **Debug** | Không | Có | Lớn hơn | Development, debugging |
| **Release** | Có | Tùy chọn | Nhỏ hơn | Production, distribution |

```bash
# Debug build (default)
dotnet build

# Release build
dotnet build -c Release
```

### 5.2 Clean Build

```bash
# Xóa toàn bộ build artifacts
dotnet clean

# Xóa và build lại
dotnet clean && dotnet build -c Release
```

---

## 6. Xử Lý Lỗi Build

### 6.1 Lỗi: "Console.exe not found"

**Nguyên nhân:**
- Console app chưa được publish
- Đường dẫn trong .csproj không đúng

**Giải pháp:**
```bash
# 1. Xóa cache
dotnet clean

# 2. Publish thủ công
dotnet publish ZplRenderer.Console/ZplRenderer.Console.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# 3. Build lại
dotnet build -c Release
```

### 6.2 Lỗi: "Target framework not installed"

**Nguyên nhân:**
- Thiếu .NET Framework 4.0 Developer Pack

**Giải pháp:**
```bash
# Download và cài .NET Framework 4.0 Developer Pack từ:
# https://dotnet.microsoft.com/download/dotnet-framework/net40
```

### 6.3 Lỗi: "Access denied" khi build

**Nguyên nhân:**
- File DLL đang được sử dụng
- Antivirus đang lock file

**Giải pháp:**
```bash
# 1. Đóng tất cả process đang dùng DLL
taskkill /F /IM ZplRenderer.Console.exe

# 2. Clean và build lại
dotnet clean
dotnet build
```

### 6.4 Lỗi: NuGet restore failed

**Nguyên nhân:**
- Không có internet
- NuGet cache corrupted

**Giải pháp:**
```bash
# Xóa NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore

# Build lại
dotnet build
```

---

## 7. Build Scripts

### 7.1 Windows PowerShell Script

```powershell
# build.ps1
param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

Write-Host "Building ZplRenderer - Configuration: $Configuration" -ForegroundColor Green

# Step 1: Clean
Write-Host "`n[1/4] Cleaning..." -ForegroundColor Yellow
dotnet clean -c $Configuration

# Step 2: Restore packages
Write-Host "`n[2/4] Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore

# Step 3: Publish Console app
Write-Host "`n[3/4] Publishing Console app..." -ForegroundColor Yellow
dotnet publish ZplRenderer.Console/ZplRenderer.Console.csproj `
    -c $Configuration `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true

# Step 4: Build main library
Write-Host "`n[4/4] Building main library..." -ForegroundColor Yellow
dotnet build -c $Configuration

Write-Host "`nBuild completed successfully!" -ForegroundColor Green
Write-Host "`nOutput files:"
Write-Host "  - .NET 8.0: bin\$Configuration\net8.0\ZplRenderer.dll"
Write-Host "  - .NET 4.0: bin\$Configuration\net40\ZplRenderer.dll"
```

**Sử dụng:**
```powershell
# Debug build
.\build.ps1

# Release build
.\build.ps1 -Configuration Release
```

### 7.2 Windows Batch Script

```batch
@echo off
REM build.bat

echo ========================================
echo Building ZplRenderer
echo ========================================

REM Clean
echo.
echo [1/4] Cleaning...
dotnet clean -c Release

REM Restore
echo.
echo [2/4] Restoring packages...
dotnet restore

REM Publish Console
echo.
echo [3/4] Publishing Console app...
dotnet publish ZplRenderer.Console\ZplRenderer.Console.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

REM Build
echo.
echo [4/4] Building main library...
dotnet build -c Release

echo.
echo ========================================
echo Build completed!
echo ========================================
echo.
echo Output files:
echo   - .NET 8.0: bin\Release\net8.0\ZplRenderer.dll
echo   - .NET 4.0: bin\Release\net40\ZplRenderer.dll
echo.
pause
```

**Sử dụng:**
```batch
build.bat
```

---

## 8. CI/CD Integration

### 8.1 GitHub Actions Example

```yaml
# .github/workflows/build.yml
name: Build ZplRenderer

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: windows-latest

    steps:
    - uses: actions/checkout@v3

    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 8.0.x

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build -c Release --no-restore

    - name: Test
      run: dotnet test -c Release --no-build --verbosity normal

    - name: Upload artifacts
      uses: actions/upload-artifact@v3
      with:
        name: ZplRenderer-DLLs
        path: |
          bin/Release/net8.0/ZplRenderer.dll
          bin/Release/net40/ZplRenderer.dll
```

### 8.2 Azure DevOps Pipeline

```yaml
# azure-pipelines.yml
trigger:
- main
- develop

pool:
  vmImage: 'windows-latest'

steps:
- task: UseDotNet@2
  inputs:
    version: '8.0.x'

- task: DotNetCoreCLI@2
  displayName: 'Restore packages'
  inputs:
    command: 'restore'

- task: DotNetCoreCLI@2
  displayName: 'Build'
  inputs:
    command: 'build'
    arguments: '-c Release'

- task: CopyFiles@2
  displayName: 'Copy DLLs to artifact staging'
  inputs:
    Contents: |
      bin/Release/net8.0/ZplRenderer.dll
      bin/Release/net40/ZplRenderer.dll
    TargetFolder: '$(Build.ArtifactStagingDirectory)'

- task: PublishBuildArtifacts@1
  displayName: 'Publish artifacts'
  inputs:
    PathtoPublish: '$(Build.ArtifactStagingDirectory)'
    ArtifactName: 'ZplRenderer'
```

---

## 9. Troubleshooting Guide

### Checklist Khi Build Fail

- [ ] Đã cài .NET SDK 8.0+?
- [ ] Đã cài .NET Framework 4.0 Developer Pack?
- [ ] Đã chạy `dotnet restore`?
- [ ] Đã chạy `dotnet clean` trước khi build lại?
- [ ] Console.exe đã được publish chưa?
- [ ] Có file bị lock bởi process khác không?
- [ ] Antivirus có đang block không?
- [ ] Có kết nối internet để download NuGet packages?

### Kiểm Tra Build Output

```bash
# Build với verbose logging
dotnet build -v detailed > build.log 2>&1

# Xem log file
notepad build.log
```

---

## 10. Best Practices

### 10.1 Development Workflow

1. **Pull latest code**
   ```bash
   git pull origin main
   ```

2. **Clean previous build**
   ```bash
   dotnet clean
   ```

3. **Restore packages**
   ```bash
   dotnet restore
   ```

4. **Build Debug** (để test)
   ```bash
   dotnet build
   ```

5. **Test changes**
   ```bash
   dotnet test
   ```

6. **Build Release** (khi ready)
   ```bash
   dotnet build -c Release
   ```

### 10.2 Before Commit

```bash
# Đảm bảo code build thành công
dotnet build -c Release

# Format code (nếu có tool)
dotnet format

# Commit
git add .
git commit -m "Your message"
git push
```

---

## 11. Quick Reference

### Common Commands

```bash
# Build tất cả
dotnet build

# Build Release
dotnet build -c Release

# Build riêng .NET 8.0
dotnet build -f net8.0

# Build riêng .NET 4.0
dotnet build -f net40

# Clean
dotnet clean

# Restore packages
dotnet restore

# Publish Console app
dotnet publish ZplRenderer.Console/ZplRenderer.Console.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true

# Build verbose
dotnet build -v detailed

# List targets
dotnet msbuild -targets
```

---

## Tóm Tắt

**Build đơn giản (1 lệnh):**
```bash
dotnet build -c Release
```

**MSBuild tự động:**
1. Publish Console app (nếu cần)
2. Build .NET 8.0 DLL
3. Build .NET 4.0 DLL (embed Console.exe)
4. Hoàn tất!

**Output:**
- `bin/Release/net8.0/ZplRenderer.dll` - Cho .NET 8.0+ apps
- `bin/Release/net40/ZplRenderer.dll` - Cho .NET 4.0+ apps (26MB, self-contained)

---

**Tác giả:** Claude Code
**Ngày:** 2025-11-12
**Version:** 1.0
