# Hướng Dẫn Deployment - ZplRenderer

## Tổng Quan

ZplRenderer hỗ trợ deployment cho cả môi trường legacy (.NET Framework 4.0) và hiện đại (.NET 8.0+).

---

## 🎯 Chọn Phương Án Deployment

### Decision Tree

```
Môi trường đích của bạn có .NET 8.0 runtime không?
│
├─ CÓ → Dùng net8.0/ZplRenderer.dll
│        ├─ Kích thước: ~200KB + dependencies (~15MB total)
│        └─ Performance: Tốt nhất
│
└─ KHÔNG (chỉ có .NET Framework 4.0-4.8)
         └─ Dùng net40/ZplRenderer.dll
              ├─ Kích thước: 26MB (single file)
              ├─ Self-contained: Không cần .NET 8.0
              └─ Performance: Hơi chậm hơn (overhead từ process execution)
```

---

## 📦 Deployment Option 1: .NET Framework 4.0 - 4.8 (Legacy)

### ✅ Ưu điểm
- Chỉ cần 1 file DLL duy nhất
- Không cần cài .NET 8.0 trên máy đích
- Tương thích với mọi ứng dụng .NET Framework 4.0+
- Self-contained (đã bao gồm .NET 8.0 runtime)

### ⚠️ Nhược điểm
- File DLL lớn (26MB)
- Lần đầu chạy sẽ extract 26MB vào `%TEMP%` (~700ms overhead)
- Cần quyền ghi vào temp folder

### 📋 Checklist Deployment

#### Bước 1: Build DLL

```bash
dotnet build -c Release -f net40
```

Output: `bin/Release/net40/ZplRenderer.dll` (~26MB)

#### Bước 2: Verify DLL

```bash
# Kiểm tra kích thước (phải ~26MB)
dir bin\Release\net40\ZplRenderer.dll

# Kiểm tra embedded resource
# DLL phải chứa ZplRenderer.Console.exe bên trong
```

#### Bước 3: Deployment Files

```
YourApplication/
├── YourApp.exe              (Ứng dụng .NET 4.0 của bạn)
└── ZplRenderer.dll          (26MB - CHỈ CẦN FILE NÀY!)
```

**KHÔNG CẦN:**
- ❌ Các DLL dependencies khác
- ❌ Folder runtimes/
- ❌ Config files
- ❌ Cài .NET 8.0 trên máy đích

#### Bước 4: Yêu Cầu Môi Trường Đích

| Yêu cầu | Có sẵn? |
|---------|---------|
| Windows 7+ (x64) | ✅ Thường có |
| .NET Framework 4.0+ | ✅ Built-in Windows |
| Quyền ghi `%TEMP%` | ✅ User bình thường có |
| ~52MB disk space | ✅ Minimal |

#### Bước 5: Test Deployment

```csharp
// Test code trên máy đích
using System;
using ZplRenderer.Infrastructure.Renderers;

class Program
{
    static void Main()
    {
        try
        {
            var service = new ZplRenderService();

            // Lần đầu tiên sẽ mất ~1 giây (extract)
            service.ConvertZplToFile(
                @"C:\test\sample.zpl",
                @"C:\output",
                "pdf"
            );

            Console.WriteLine("SUCCESS! DLL hoạt động tốt!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.Message);
        }
    }
}
```

**Expected behavior lần đầu:**
```
1. Program starts
2. Extract ZplRenderer.Console.exe to %TEMP%\ZplRenderer\ (~700ms)
3. Execute console app to process ZPL
4. Output file created
5. "SUCCESS! DLL hoạt động tốt!"
```

**Lần chạy thứ 2 trở đi:**
```
1. Program starts
2. Skip extraction (file đã có)
3. Execute console app (~0ms overhead)
4. Output file created
```

---

## 📦 Deployment Option 2: .NET 8.0+ (Modern)

### ✅ Ưu điểm
- DLL nhỏ gọn (~200KB)
- Performance tốt (no process overhead)
- Direct rendering, không qua intermediate steps

### ⚠️ Nhược điểm
- Cần .NET 8.0 runtime trên máy đích
- Phải copy nhiều dependencies
- Phức tạp hơn trong deployment

### 📋 Checklist Deployment

#### Bước 1: Build DLL

```bash
dotnet build -c Release -f net8.0
```

#### Bước 2: Collect Dependencies

```bash
# Publish để lấy tất cả dependencies
dotnet publish -c Release -f net8.0 -o publish/
```

#### Bước 3: Deployment Files

```
YourApplication/
├── YourApp.exe                      (Ứng dụng .NET 8.0)
├── ZplRenderer.dll                  (~200KB)
├── BinaryKits.Zpl.Viewer.dll
├── BinaryKits.Zpl.Label.dll
├── SkiaSharp.dll
├── SixLabors.ImageSharp.dll
├── itext.kernel.dll
├── BouncyCastle.Cryptography.dll
├── HarfBuzzSharp.dll
├── [... other dependencies ...]
└── runtimes/                        (Native libraries)
    ├── win-x64/native/
    │   ├── libSkiaSharp.dll
    │   └── libHarfBuzzSharp.dll
    ├── linux-x64/native/
    └── osx/native/
```

#### Bước 4: Yêu Cầu Môi Trường Đích

| Yêu cầu | Cần cài đặt |
|---------|-------------|
| Windows/Linux/macOS | ✅ |
| **.NET 8.0 Runtime** | ⚠️ **BẮT BUỘC** |
| ~30MB disk space | ✅ |

Cài .NET 8.0 Runtime:
```bash
# Download từ: https://dotnet.microsoft.com/download/dotnet/8.0
# Hoặc check nếu đã có:
dotnet --version
```

#### Bước 5: Test Deployment

```csharp
// Test code (async)
using ZplRenderer.Infrastructure.Renderers;

class Program
{
    static async Task Main()
    {
        var service = new ZplRenderService();

        await service.ConvertZplToFileAsync(
            @"C:\test\sample.zpl",
            @"C:\output",
            "pdf"
        );

        Console.WriteLine("SUCCESS!");
    }
}
```

---

## 🔧 Advanced Deployment Scenarios

### Scenario 1: Intranet Deployment (Môi trường nội bộ)

**Tình huống:** Deploy lên 100+ máy trong công ty, không có .NET 8.0

**Giải pháp:**
```
1. Dùng net40/ZplRenderer.dll
2. Copy via Group Policy hoặc deployment tool
3. Không cần cài thêm gì

Deploy script (PowerShell):
```powershell
# deploy.ps1
$sourceDLL = "\\server\share\ZplRenderer.dll"
$targetPath = "C:\Program Files\YourApp\"

Copy-Item $sourceDLL -Destination $targetPath -Force
Write-Host "Deployment completed!"
```

### Scenario 2: Desktop Application (ClickOnce/Installer)

**Tình huống:** Desktop app với installer

**Giải pháp:**

#### WiX Installer
```xml
<!-- Product.wxs -->
<File Id="ZplRendererDLL"
      Source="bin\Release\net40\ZplRenderer.dll"
      KeyPath="yes" />
```

#### Inno Setup
```ini
[Files]
Source: "bin\Release\net40\ZplRenderer.dll"; DestDir: "{app}"; Flags: ignoreversion
```

### Scenario 3: Cloud Deployment (Azure/AWS)

**Tình huống:** ASP.NET application trên cloud

**Giải pháp:**

#### Azure App Service
```yaml
# azure-pipelines.yml
- task: CopyFiles@2
  inputs:
    SourceFolder: '$(Build.SourcesDirectory)/bin/Release/net8.0'
    Contents: |
      ZplRenderer.dll
      **/*.dll
      runtimes/**
    TargetFolder: '$(Build.ArtifactStagingDirectory)'
```

#### Docker
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY bin/Release/net8.0/publish/ .
ENTRYPOINT ["dotnet", "YourApp.dll"]

# ZplRenderer.dll sẽ được copy cùng dependencies
```

### Scenario 4: Portable Application (USB/No Install)

**Tình huống:** App chạy từ USB, không cài đặt

**Giải pháp:**

Dùng .NET 4.0 DLL (tốt nhất):
```
USB:/
├── YourPortableApp.exe
└── ZplRenderer.dll        (26MB)

Advantages:
✅ Chạy trên mọi Windows có .NET 4.0 (hầu hết)
✅ Không cần install
✅ True portable
```

---

## 🐛 Troubleshooting Deployment Issues

### Issue 1: "FileNotFoundException" khi chạy

**Nguyên nhân:** Thiếu dependencies

**Giải pháp:**
```bash
# Option A: Kiểm tra có đủ DLLs
dir YourApp\

# Option B: Dùng .NET 4.0 DLL (self-contained)
# Copy net40/ZplRenderer.dll thay vì net8.0/
```

### Issue 2: "Access denied" khi extract

**Nguyên nhân:** Không có quyền ghi `%TEMP%`

**Giải pháp:**
```csharp
// Không thể fix qua code (DLL không cho phép custom path)
// Workaround: Run as Administrator hoặc grant TEMP access
```

### Issue 3: Antivirus block extraction

**Nguyên nhân:** AV chặn hành vi extract EXE từ DLL

**Giải pháp:**
```
1. Whitelist %TEMP%\ZplRenderer\*.exe
2. Hoặc whitelist toàn bộ ứng dụng
3. Submit false positive report tới AV vendor
```

### Issue 4: ".NET 8.0 runtime not found" (net8.0 DLL)

**Nguyên nhân:** Máy đích thiếu .NET 8.0

**Giải pháp:**
```bash
# Option A: Cài .NET 8.0 Runtime
dotnet --info

# Option B: Chuyển sang dùng net40/ZplRenderer.dll
```

---

## 📊 Deployment Comparison

| Feature | NET 4.0 DLL | NET 8.0 DLL |
|---------|-------------|-------------|
| **File count** | 1 file | ~20+ files |
| **Total size** | 26MB | ~15MB |
| **Runtime requirement** | .NET FW 4.0 (built-in) | .NET 8.0 (cần cài) |
| **First run overhead** | ~700ms (extract) | 0ms |
| **Subsequent runs** | 0ms | 0ms |
| **Deployment complexity** | ⭐ Very Easy | ⭐⭐⭐ Complex |
| **Best for** | Legacy environments | Modern apps |
| **Cross-platform** | ❌ Windows only | ✅ Win/Linux/Mac |

---

## 🎯 Deployment Checklist

### Pre-Deployment

- [ ] Build DLL ở chế độ Release
- [ ] Kiểm tra kích thước file
- [ ] Test trên máy dev
- [ ] Test trên VM/container sạch
- [ ] Kiểm tra permissions required
- [ ] Document dependencies

### Deployment

- [ ] Copy đúng DLL version (net40 hoặc net8.0)
- [ ] Copy tất cả dependencies (nếu net8.0)
- [ ] Verify file integrity (checksum)
- [ ] Test ứng dụng sau khi deploy
- [ ] Check logs/errors

### Post-Deployment

- [ ] Monitor first run (extract time)
- [ ] Check %TEMP% usage
- [ ] Verify output files
- [ ] Monitor performance
- [ ] Document any issues

---

## 📞 Support & Resources

### Nếu gặp vấn đề:

1. **Kiểm tra:** [README.md](README.md) - Hướng dẫn cơ bản
2. **Build issues:** [BUILD_GUIDE.md](BUILD_GUIDE.md)
3. **Code issues:** [CODE_FLOW.md](CODE_FLOW.md)
4. **Tech details:** [TECHNICAL_STACK.md](TECHNICAL_STACK.md)

### Common Questions

**Q: Tôi nên dùng DLL nào?**
A: Nếu môi trường chỉ có .NET Framework 4.0 → Dùng net40/ZplRenderer.dll

**Q: Có cần cài .NET 8.0 không?**
A: KHÔNG, nếu dùng net40/ZplRenderer.dll. CÓ, nếu dùng net8.0/ZplRenderer.dll

**Q: DLL lớn quá, có cách nào giảm không?**
A: Dùng net8.0 DLL (~200KB) nhưng cần .NET 8.0 runtime trên máy đích

**Q: Có thể deploy trên Linux không?**
A: CÓ, nhưng chỉ với net8.0 DLL + .NET 8.0 runtime

---

## 🚀 Quick Deployment Commands

### Windows (PowerShell)

```powershell
# Deploy to local app folder
Copy-Item "bin\Release\net40\ZplRenderer.dll" -Destination "C:\YourApp\"

# Deploy to network share
Copy-Item "bin\Release\net40\ZplRenderer.dll" -Destination "\\server\apps\YourApp\"

# Deploy with verification
$source = "bin\Release\net40\ZplRenderer.dll"
$dest = "C:\YourApp\ZplRenderer.dll"
Copy-Item $source -Destination $dest -Force
if (Test-Path $dest) { Write-Host "Deployment OK" } else { Write-Host "FAILED" }
```

### Linux/Mac (Bash) - NET 8.0 only

```bash
# Deploy .NET 8.0 version
cp -r bin/Release/net8.0/* /opt/yourapp/
cp -r bin/Release/net8.0/runtimes/ /opt/yourapp/

# Verify
ls -lh /opt/yourapp/ZplRenderer.dll
```

---

**Tác giả:** Claude Code
**Ngày:** 2025-11-12
**Version:** 1.0
