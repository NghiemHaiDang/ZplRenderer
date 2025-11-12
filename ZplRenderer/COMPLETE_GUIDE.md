# ZplRenderer - Tài Liệu Tổng Hợp Đầy Đủ

> **Tài liệu tổng hợp tất cả hướng dẫn và giải thích về ZplRenderer**
>
> **Version:** 1.0.0 | **Date:** 2025-11-13 | **Language:** Vietnamese

---

## 📑 Mục Lục

1. [Giới Thiệu & Quick Start](#1-giới-thiệu--quick-start)
2. [Kiến Trúc & Quyết Định Thiết Kế](#2-kiến-trúc--quyết-định-thiết-kế)
3. [Công Nghệ & Kỹ Thuật](#3-công-nghệ--kỹ-thuật)
4. [Hướng Dẫn Build](#4-hướng-dẫn-build)
5. [Hướng Dẫn Deployment](#5-hướng-dẫn-deployment)
6. [Luồng Code Chi Tiết](#6-luồng-code-chi-tiết)
7. [Multi-Target Framework](#7-multi-target-framework)
8. [Console App Architecture](#8-console-app-architecture)
9. [DLL Merging](#9-dll-merging)
10. [Release Notes & Tasks](#10-release-notes--tasks)
11. [FAQ & Troubleshooting](#11-faq--troubleshooting)

---

# 1. Giới Thiệu & Quick Start

## 1.1. Tổng Quan

**ZplRenderer** là thư viện .NET cho phép chuyển đổi file ZPL (Zebra Programming Language) sang các định dạng PDF, PNG hoặc JPG.

### Tính Năng Chính

- ✅ Chuyển đổi ZPL sang PDF, PNG, JPG
- ✅ Hỗ trợ .NET Framework 4.0+ (không cần upgrade)
- ✅ Chỉ cần 1 file DLL duy nhất
- ✅ Không cần cài đặt dependencies
- ✅ Tự động extract và chạy engine xử lý
- ✅ Xử lý nhiều labels trong một file ZPL

### Yêu Cầu Hệ Thống

**Môi Trường End User (.NET 4.0 - 4.8):**
- OS: Windows 7/8/10/11 (x64)
- .NET Framework: 4.0 trở lên (có sẵn)
- Dung lượng: ~52 MB (26MB DLL + 26MB temp khi chạy lần đầu)
- Quyền: Quyền ghi vào `%TEMP%` folder

**⚠️ QUAN TRỌNG:**
- **KHÔNG CẦN** cài .NET 8.0 trên máy đích
- **KHÔNG CẦN** cài thêm dependency nào khác
- DLL đã bao gồm .NET 8.0 runtime (self-contained)
- Chỉ cần .NET Framework 4.0 là đủ!

## 1.2. Quick Start - 5 Phút

### Bước 1: Lấy DLL (10 giây)
```
ZplRenderer/bin/Release/net40/ZplRenderer.dll
```

### Bước 2: Add Reference (20 giây)
**Visual Studio:**
1. Right-click project → **Add** → **Reference**
2. Click **Browse** → Chọn `ZplRenderer.dll`
3. Click **OK**

### Bước 3: Viết Code (2 phút)
```csharp
using ZplRenderer.Infrastructure.Renderers;

class Program
{
    static void Main(string[] args)
    {
        var service = new ZplRenderService();

        service.ConvertZplToFile(
            zplFilePath: @"C:\temp\label.zpl",
            outputDirectory: @"C:\temp\output",
            format: "pdf"
        );

        Console.WriteLine("Done!");
    }
}
```

### Bước 4: Các Format Hỗ Trợ

```csharp
// PDF - Tất cả labels trong 1 file
service.ConvertZplToFile(input, output, "pdf");

// PNG - Mỗi label 1 file PNG
service.ConvertZplToFile(input, output, "png");

// JPG - Mỗi label 1 file JPG
service.ConvertZplToFile(input, output, "jpg");
```

---

# 2. Kiến Trúc & Quyết Định Thiết Kế

## 2.1. Vấn Đề Ban Đầu

### Yêu Cầu
- ✅ Chuyển đổi ZPL → PNG/JPG/PDF
- ✅ Hỗ trợ .NET Framework 4.0+ (legacy applications)
- ✅ Sử dụng thư viện modern để có quality tốt nhất

### Xung Đột
```
Requirement A: Support .NET Framework 4.0
         +
Requirement B: Use modern libraries (need .NET 5+)
         =
    IMPOSSIBLE! 😱
```

**Modern libraries cần thiết:**
- BinaryKits.Zpl.Viewer 1.3.0 → Yêu cầu .NET 4.7.2+ hoặc .NET 6.0+
- SkiaSharp 3.119.1 → Yêu cầu .NET 6.0+
- iText7 9.0.0 → Yêu cầu .NET 4.6.2+ hoặc .NET Standard 2.0
- SixLabors.ImageSharp 3.1.12 → Yêu cầu .NET 6.0+

## 2.2. Các Giải Pháp Được Cân Nhắc

### ❌ Solution 1: Upgrade lên .NET 4.7.2
- Vấn đề: Vẫn không support SkiaSharp 3.x, ImageSharp 3.x
- Mất khách hàng legacy (Server 2008, embedded systems)

### ❌ Solution 2: Dùng Old Libraries
- BinaryKits không có version cũ support .NET 4.0
- Quality rendering kém
- Maintain 2 codebases hoàn toàn khác nhau

### ❌ Solution 3: Drop .NET 4.0 Support
- Khách hàng legacy không dùng được
- Mất market share

### ✅ Solution 5: Wrapper + Embedded Console App (CHỌN)

```
ZplRenderer.dll (.NET 4.0)
    ↓
Contains embedded: ZplRenderer.Console.exe (.NET 8.0)
    ↓
Console.exe is SELF-CONTAINED (.NET 8.0 runtime included)
    ↓
.NET 4.0 app → Extract exe → Execute → Get result
```

## 2.3. Wrapper Architecture

### Kiến Trúc Tổng Quan
```
┌────────────────────────────────────────────────┐
│         ZplRenderer.dll                        │
├────────────────────────────────────────────────┤
│  NET 8.0 Build          NET 4.0 Build          │
│  (Direct Rendering)     (Wrapper + Proxy)      │
└────────────────────────────────────────────────┘
```

### NET 4.0 Architecture
```
User's .NET 4.0 Application
    ↓
ZplRenderService.ConvertZplToFile()
    ↓
Extract ZplRenderer.Console.exe (embedded)
    ↓
Process.Start(Console.exe) ← New Process!
    ↓
Console.exe (NET 8.0 self-contained)
    ↓
BinaryKits → SkiaSharp → Output
```

### NET 8.0 Architecture
```
User's .NET 8.0 Application
    ↓
ZplRenderService.ConvertZplToFileAsync()
    ↓
BinaryKits.Zpl.Viewer (in-process)
    ↓
SkiaSharp (in-process)
    ↓
Output Files
```

## 2.4. Key Insights

> **Wrapper architecture không phải là "hack" hay "workaround".**
>
> Đây là **architectural pattern** hợp lý để bridge giữa legacy
> và modern platforms khi upgrade không khả thi.

**Tương tự như:**
- JNI (Java Native Interface) - Java gọi C code
- P/Invoke (.NET gọi native DLL)
- Electron (Web tech trong native shell)

---

# 3. Công Nghệ & Kỹ Thuật

## 3.1. Technology Stack

### .NET Frameworks
- **.NET Framework 4.0** - Legacy support
- **.NET 8.0** - Modern implementation
- **C# 12** - Ngôn ngữ lập trình

### Thư Viện ZPL
**BinaryKits.Zpl.Viewer (v1.3.0)**
- Mã nguồn mở (MIT License)
- Core engine để parse và render ZPL
- Output: PNG byte array

```csharp
var analyzer = new ZplAnalyzer(printerStorage);
var analyzeInfo = analyzer.Analyze(zplText);

var drawer = new ZplElementDrawer(printerStorage);
byte[] imageBytes = drawer.Draw(labelInfo.ZplElements);
```

### Graphics & Rendering
**SkiaSharp (v3.119.1)**
- Cross-platform 2D graphics library
- .NET wrapper cho Skia (Google)
- Native library: `libSkiaSharp.dll` (11.4 MB)

**HarfBuzzSharp (v8.3.1.2)**
- Text shaping engine
- Xử lý text rendering phức tạp
- Native library: `libHarfBuzzSharp.dll` (1.8 MB)

### Image Processing
**SixLabors.ImageSharp (v3.1.12)**
- Pure C# image processing
- Chuyển đổi PNG → JPG
- 100% managed code

```csharp
using (var image = Image.Load(imageBytes))
{
    image.Save($"{baseFileName}.jpg", new JpegEncoder());
}
```

### PDF Generation
**iText7 (v9.0.0)**
- Thư viện tạo PDF mạnh mẽ
- Ghép nhiều label images thành 1 PDF

```csharp
var pdfWriter = new PdfWriter(outputFile);
var pdfDoc = new PdfDocument(pdfWriter);
var document = new Document(pdfDoc);

var imageData = ImageDataFactory.Create(imageBytes);
var pdfImage = new Image(imageData);
document.Add(pdfImage);
```

### Cryptography
**BouncyCastle.Cryptography (v2.4.0)**
- Dependency của iText7
- PDF signing & encryption

### Build Tools
**Fody (v6.9.3)** + **Costura.Fody (v6.0.0)**
- IL weaving - embed dependencies vào DLL
- Nhúng tất cả managed DLLs vào 1 file

## 3.2. Workflow Xử Lý

```
┌─────────────┐
│  ZPL File   │
└──────┬──────┘
       │
       ▼
┌─────────────────────────────┐
│  BinaryKits.Zpl.Viewer      │ ← Parse ZPL text
│  (Analyzer)                 │
└──────┬──────────────────────┘
       │
       ▼
┌─────────────────────────────┐
│  ZplElementDrawer           │
│  (Uses SkiaSharp +          │ ← Render to PNG bytes
│   HarfBuzzSharp)            │
└──────┬──────────────────────┘
       │
       ├─────► PNG output (direct write)
       │
       ├─────► JPG output ──► SixLabors.ImageSharp
       │
       └─────► PDF output ──► iText7 + BouncyCastle
```

---

# 4. Hướng Dẫn Build

## 4.1. Yêu Cầu Môi Trường

| Phần mềm | Version | Mục đích |
|----------|---------|----------|
| .NET SDK | 8.0+ | Build .NET 8.0 targets |
| .NET Framework | 4.0+ SDK | Build .NET 4.0 target |
| Visual Studio | 2022+ | IDE (optional) |
| MSBuild | 17.0+ | Build tool |

## 4.2. Build Đơn Giản

```bash
# Di chuyển vào thư mục project
cd ZplRenderer

# Build toàn bộ solution
dotnet build

# Hoặc build Release
dotnet build -c Release
```

**MSBuild tự động:**
1. Kiểm tra `ZplRenderer.Console.exe` chưa tồn tại
2. Publish `ZplRenderer.Console` (nếu cần)
3. Build `ZplRenderer` cho cả 2 targets (net8.0 và net40)
4. Embed Console.exe vào .NET 4.0 DLL

## 4.3. Output Files

```
bin/Release/
├── net8.0/
│   ├── ZplRenderer.dll     (~200KB)
│   ├── *.dll               (Dependencies)
│   └── runtimes/           (Native libraries)
│
└── net40/
    └── ZplRenderer.dll     (~26MB) ⭐ SELF-CONTAINED
                            (Chứa embedded Console.exe + .NET 8.0 runtime)
```

## 4.4. Build Thủ Công (Advanced)

### Bước 1: Publish Console App
```bash
dotnet publish ZplRenderer.Console/ZplRenderer.Console.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true
```

**⚠️ Self-Contained:**
- Chứa toàn bộ .NET 8.0 runtime (~26MB)
- Không cần máy đích cài .NET 8.0
- Chạy standalone trên Windows 64-bit

### Bước 2: Build Main Library
```bash
dotnet build ZplRenderer.csproj -c Release
```

## 4.5. Xử Lý Lỗi Build

### Lỗi: "Console.exe not found"
```bash
# Xóa cache
dotnet clean

# Publish thủ công
dotnet publish ZplRenderer.Console/ZplRenderer.Console.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Build lại
dotnet build -c Release
```

### Lỗi: "Target framework not installed"
- Download .NET Framework 4.0 Developer Pack từ Microsoft

### Lỗi: NuGet restore failed
```bash
# Xóa NuGet cache
dotnet nuget locals all --clear

# Restore packages
dotnet restore

# Build lại
dotnet build
```

---

# 5. Hướng Dẫn Deployment

## 5.1. Chọn Phương Án Deployment

```
Môi trường đích có .NET 8.0 runtime không?
│
├─ CÓ → Dùng net8.0/ZplRenderer.dll
│        ├─ Kích thước: ~200KB + dependencies (~15MB total)
│        └─ Performance: Tốt nhất
│
└─ KHÔNG (chỉ có .NET Framework 4.0-4.8)
         └─ Dùng net40/ZplRenderer.dll
              ├─ Kích thước: 26MB (single file)
              ├─ Self-contained: Không cần .NET 8.0
              └─ Performance: Hơi chậm hơn
```

## 5.2. Deployment .NET Framework 4.0

### Deployment Files
```
YourApplication/
├── YourApp.exe              (Ứng dụng .NET 4.0)
└── ZplRenderer.dll          (26MB - CHỈ CẦN FILE NÀY!)
```

**KHÔNG CẦN:**
- ❌ Các DLL dependencies khác
- ❌ Folder runtimes/
- ❌ Cài .NET 8.0 trên máy đích

### Yêu Cầu Môi Trường Đích
- Windows 7+ (x64)
- .NET Framework 4.0+ (built-in)
- Quyền ghi `%TEMP%`
- ~52MB disk space

### Test Deployment
```csharp
using ZplRenderer.Infrastructure.Renderers;

var service = new ZplRenderService();

// Lần đầu: ~1 giây (extract)
service.ConvertZplToFile(@"C:\test\sample.zpl", @"C:\output", "pdf");

// Lần 2+: ~0ms overhead
```

## 5.3. Deployment .NET 8.0

### Deployment Files
```
YourApplication/
├── YourApp.exe
├── ZplRenderer.dll (~200KB)
├── BinaryKits.Zpl.Viewer.dll
├── SkiaSharp.dll
├── [... 20+ DLLs ...]
└── runtimes/
    └── win-x64/native/
        ├── libSkiaSharp.dll
        └── libHarfBuzzSharp.dll
```

### Yêu Cầu
- .NET 8.0 Runtime ⚠️ **BẮT BUỘC**
- ~30MB disk space

## 5.4. Deployment Scenarios

### Scenario 1: Intranet (100+ máy, không .NET 8.0)
**Giải pháp:** Dùng net40/ZplRenderer.dll

```powershell
# deploy.ps1
$sourceDLL = "\\server\share\ZplRenderer.dll"
$targetPath = "C:\Program Files\YourApp\"
Copy-Item $sourceDLL -Destination $targetPath -Force
```

### Scenario 2: Desktop App (Installer)

**WiX:**
```xml
<File Id="ZplRendererDLL"
      Source="bin\Release\net40\ZplRenderer.dll"
      KeyPath="yes" />
```

**Inno Setup:**
```ini
[Files]
Source: "bin\Release\net40\ZplRenderer.dll"; DestDir: "{app}"
```

### Scenario 3: Portable App (USB)
```
USB:/
├── YourPortableApp.exe
└── ZplRenderer.dll  (26MB - chạy mọi Windows có .NET 4.0)
```

---

# 6. Luồng Code Chi Tiết

## 6.1. Luồng .NET 8.0 (Direct)

```
User Code
    ↓
ConvertZplToFileAsync()
    ↓
Validate & Create Output Dir
    ↓
StreamReader → Read ZPL file
    ↓
LOOP: Parse từng dòng
    ↓
Buffer += line
    ↓
If line == "^XZ" (end of label)
    ↓
ProcessZplChunkAsync(buffer)
    ├─ ZplAnalyzer.Analyze(zplText)
    ├─ ZplElementDrawer.Draw()
    └─ Format Conversion:
        ├─ PNG: Direct write
        ├─ JPG: ImageSharp conversion
        └─ PDF: iText7 generation
    ↓
Clear buffer, GC.Collect()
    ↓
Continue loop
```

## 6.2. Luồng .NET 4.0 (Wrapper)

```
User Code (.NET 4.0)
    ↓
ConvertZplToFile()
    ↓
EnsureConsoleAppExtracted()
    ├─ Check: %TEMP%\ZplRenderer\Console.exe exists?
    ├─ NO → Extract from embedded resource (~700ms)
    └─ YES → Skip (0ms)
    ↓
Process.Start(Console.exe, args)
    ├─ FileName = %TEMP%\ZplRenderer\Console.exe
    ├─ Arguments = "input.zpl output pdf"
    └─ RedirectStandardOutput/Error = true
    ↓
NEW PROCESS: Console.exe (.NET 8.0)
    ├─ Load .NET 8.0 runtime (from embedded)
    ├─ Parse args
    ├─ Process ZPL (same as NET 8.0 flow)
    ├─ Write output files
    └─ Exit code = 0
    ↓
.NET 4.0 Process
    ├─ process.WaitForExit()
    ├─ Check ExitCode
    └─ Return to user
```

### Timeline NET 4.0

**Lần đầu:**
```
[0ms]     ConvertZplToFile()
[0-700ms] Extract Console.exe
[700ms]   Start process
[800ms]   Console loads runtime
[1000ms]  Start processing
[2000ms]  Done!
```

**Lần 2+:**
```
[0ms]     ConvertZplToFile()
[0ms]     Skip extraction ⚡
[100ms]   Processing
[500ms]   Done!
```

## 6.3. Embedded Resource Extraction

```csharp
private static void EnsureConsoleAppExtracted()
{
    string tempPath = Path.Combine(
        Path.GetTempPath(),
        "ZplRenderer",
        "ZplRenderer.Console.exe"
    );

    if (File.Exists(tempPath)) return;

    // Extract with 80KB buffer
    using (Stream stream = Assembly.GetExecutingAssembly()
        .GetManifestResourceStream("ZplRenderer.Console.exe"))
    {
        using (FileStream file = new FileStream(tempPath,
            FileMode.Create, FileAccess.Write, FileShare.None,
            bufferSize: 81920))
        {
            byte[] buffer = new byte[81920];
            int bytesRead;
            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                file.Write(buffer, 0, bytesRead);
            }
        }
    }
}
```

---

# 7. Multi-Target Framework

## 7.1. Multi-Target Là Gì?

```
Single Project → Build → Multiple Output DLLs
                         ├─ net8.0/ZplRenderer.dll
                         └─ net40/ZplRenderer.dll
```

## 7.2. Cấu Hình .csproj

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Library</OutputType>

    <!-- MULTI-TARGET -->
    <TargetFrameworks>net40;net8.0</TargetFrameworks>

    <LangVersion>latest</LangVersion>
  </PropertyGroup>
</Project>
```

## 7.3. Conditional Compilation

```csharp
public class ZplRenderService
{
#if NET40
    // CODE CHỈ CHO .NET 4.0
    public void ConvertZplToFile(...)
    {
        // Extract & execute Console.exe
    }
#else
    // CODE CHỈ CHO .NET 8.0
    public async Task ConvertZplToFileAsync(...)
    {
        // Direct rendering
    }
#endif
}
```

## 7.4. Conditional Package References

```xml
<!-- Packages CHỈ cho .NET 8.0 -->
<ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
  <PackageReference Include="BinaryKits.Zpl.Viewer" Version="1.3.0" />
  <PackageReference Include="SkiaSharp" Version="3.119.1" />
</ItemGroup>

<!-- Packages CHỈ cho .NET 4.0 -->
<ItemGroup Condition="'$(TargetFramework)' == 'net40'">
  <!-- Minimal packages -->
</ItemGroup>
```

## 7.5. Feature Comparison

| Feature | net40 | net8.0 |
|---------|-------|--------|
| API | Sync | Async |
| Implementation | Wrapper | Direct |
| Dependencies | Minimal | Full (20+ DLLs) |
| File Size | 26MB | ~200KB |
| First Run | ~700ms overhead | 0ms |
| Runtime | .NET FW 4.0 | .NET 8.0 |
| Cross-platform | ❌ Windows only | ✅ All |

---

# 8. Console App Architecture

## 8.1. Tại Sao Cần Console App?

**Vấn đề:** .NET 4.0 không support modern libraries
**Giải pháp:** Console app (.NET 8.0) → Embed → Execute khi cần

## 8.2. Console App Configuration

```xml
<!-- ZplRenderer.Console.csproj -->
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <TargetFramework>net8.0</TargetFramework>

  <!-- Self-contained -->
  <SelfContained>true</SelfContained>
  <RuntimeIdentifier>win-x64</RuntimeIdentifier>
  <PublishSingleFile>true</PublishSingleFile>
  <IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>

  <!-- Trimming -->
  <PublishTrimmed>true</PublishTrimmed>
  <TrimMode>partial</TrimMode>
</PropertyGroup>
```

## 8.3. CLI Interface

```csharp
static async Task<int> Main(string[] args)
{
    // args[0] = ZPL file path
    // args[1] = Output directory
    // args[2] = Format (png/jpg/pdf)

    await ConvertZplToFileAsync(args[0], args[1], args[2]);

    return 0; // Success
}
```

**Exit Codes:**
- 0: Success
- 1: Invalid arguments
- 2: File not found
- 3: Invalid format
- 99: Unhandled exception

## 8.4. File Layout After Deployment

```
CustomerServer (Windows Server 2012, .NET 4.0 only)
├── C:\MyApp\
│   ├── MyApp.exe
│   └── ZplRenderer.dll (26MB)
│       └── [Embedded: Console.exe]
│
└── %TEMP%\ZplRenderer\
    └── ZplRenderer.Console.exe (extracted on first run)
        └── [Contains .NET 8.0 runtime]
```

---

# 9. DLL Merging

## 9.1. Costura.Fody

**Purpose:** Embed tất cả dependency DLLs vào 1 file

### Before Costura:
```
bin/Release/net8.0/
├── ZplRenderer.dll (200 KB)
├── BinaryKits.Zpl.Viewer.dll (500 KB)
├── SkiaSharp.dll (1.2 MB)
└── ... (20+ DLLs)
```

### After Costura:
```
bin/Release/net8.0/
├── ZplRenderer.dll (12 MB - chứa tất cả!)
└── runtimes/ (Native DLLs - không merge được)
```

## 9.2. FodyWeavers.xml

```xml
<Weavers>
  <Costura>
    <IncludeAssemblies>
      BinaryKits.Zpl.Viewer
      SkiaSharp
      SixLabors.ImageSharp
      itext.kernel
    </IncludeAssemblies>

    <Unmanaged64Assemblies>
      libSkiaSharp
      libHarfBuzzSharp
    </Unmanaged64Assemblies>
  </Costura>
</Weavers>
```

## 9.3. Runtime Loading

```
User calls: new ZplRenderService()
    ↓
CLR loads ZplRenderer.dll
    ↓
Costura hooks AssemblyResolve event
    ↓
ZplRenderService needs BinaryKits.Zpl.Viewer
    ↓
CLR: Cannot find BinaryKits.Zpl.Viewer.dll on disk
    ↓
Costura: Extract from embedded resources
    ↓
Costura: Assembly.Load(bytes) → Load into memory
    ↓
CLR: OK, I have BinaryKits now!
```

## 9.4. Managed vs Native DLLs

**Managed DLLs (.NET Assemblies):**
- CÓ THỂ embed bởi Costura
- VD: BinaryKits.Zpl.Viewer.dll, iText7

**Native DLLs (Unmanaged):**
- KHÔNG THỂ embed hoàn toàn
- Phải đi kèm trong folder `runtimes/`
- VD: libSkiaSharp.dll, libHarfBuzzSharp.dll

---

# 10. Release Notes & Tasks

## 10.1. Version 1.0.0 (2025-11-11)

### Features
- ✅ Chuyển đổi ZPL sang PDF/PNG/JPG
- ✅ .NET Framework 4.0+ support
- ✅ Single DLL deployment
- ✅ Multi-label support
- ✅ Batch processing

### Performance
| Metric | Value |
|--------|-------|
| Console.exe size | 26 MB (after trimming) |
| Extract time | ~0.7-1s |
| Subsequent runs | ~0ms overhead |
| Memory usage | ~250 MB peak |

### Architecture
- Multi-target: net40 + net8.0
- Embedded Console app pattern
- Self-contained deployment

## 10.2. Project Tasks

### Task 1: Nghiên Cứu & Thiết Kế (30%)
- Phân tích compatibility
- Survey thư viện
- Thiết kế wrapper architecture

### Task 2: Build System (20%)
- Multi-target configuration
- Auto-build MSBuild target
- Conditional compilation

### Task 3: .NET 8.0 Implementation (25%)
- ZPL parsing với BinaryKits
- SkiaSharp rendering
- Format conversion
- Async patterns

### Task 4: .NET 4.0 Wrapper (15%)
- Resource extraction
- Process spawning
- IPC
- Error handling

### Task 5: Console App (5%)
- CLI interface
- Self-contained publish
- Exit codes

### Task 6: Documentation (5%)
- 10 markdown files
- ~250KB documentation
- ~3,500 lines

---

# 11. FAQ & Troubleshooting

## 11.1. Câu Hỏi Thường Gặp

### Q: Có cần cài .NET 8.0 không?
**A:** KHÔNG, nếu dùng `net40/ZplRenderer.dll`. DLL đã chứa .NET 8.0 runtime.

### Q: DLL lớn quá, có cách nào giảm không?
**A:** Dùng `net8.0/ZplRenderer.dll` (~200KB) nhưng cần .NET 8.0 runtime.

### Q: Có thể deploy trên Linux không?
**A:** CÓ, nhưng chỉ với net8.0 DLL + .NET 8.0 runtime.

### Q: Tại sao không dùng .NET Standard?
**A:** .NET Standard 2.0 vẫn không support các libraries modern:
- BinaryKits cần .NET 5+
- SkiaSharp 3.x cần .NET 6+

### Q: Process overhead có đáng lo không?
**A:** Không, vì:
- Overhead: ~100-200ms
- Processing: 1-10 seconds (typical)
- % overhead: 1-10%

## 11.2. Troubleshooting

### Issue: "FileNotFoundException"
**Nguyên nhân:** Thiếu dependencies
**Giải pháp:** Dùng net40/ZplRenderer.dll (self-contained)

### Issue: "Access denied" khi extract
**Nguyên nhân:** Không có quyền ghi %TEMP%
**Giải pháp:** Run as Administrator hoặc grant permissions

### Issue: Antivirus block
**Nguyên nhân:** AV chặn extract EXE từ DLL
**Giải pháp:**
- Whitelist `%TEMP%\ZplRenderer\*.exe`
- Code signing
- Submit false positive

### Issue: ".NET 8.0 not found" (net8.0 DLL)
**Nguyên nhân:** Thiếu .NET 8.0 runtime
**Giải pháp:**
- Cài .NET 8.0 Runtime
- Hoặc chuyển sang net40/ZplRenderer.dll

## 11.3. Best Practices

### ✅ DO: Reuse Service Instance
```csharp
var service = new ZplRenderService();
foreach (var file in files)
{
    service.ConvertZplToFile(file, output, "pdf");
}
```

### ❌ DON'T: Tạo mới mỗi lần
```csharp
foreach (var file in files)
{
    var service = new ZplRenderService(); // ❌ Lãng phí
    service.ConvertZplToFile(file, output, "pdf");
}
```

### ✅ DO: Always Handle Exceptions
```csharp
try
{
    service.ConvertZplToFile(input, output, format);
}
catch (FileNotFoundException)
{
    // Handle file not found
}
catch (UnauthorizedAccessException)
{
    // Handle permission denied
}
catch (Exception ex)
{
    Console.WriteLine("Error: " + ex.Message);
}
```

### ✅ DO: Validate Input
```csharp
if (!File.Exists(zplFile))
{
    Console.WriteLine("File not found");
    return;
}

if (!IsValidFormat(format))
{
    Console.WriteLine("Invalid format");
    return;
}
```

---

# 12. Tổng Kết

## 12.1. Key Achievements

### Technical Innovations
1. **Backward Compatibility** - Support .NET 4.0 (2010) với modern libraries (2024)
2. **Zero Installation** - Không cần cài .NET 8.0
3. **Single DLL** - Chỉ cần 1 file
4. **Multi-Target** - 1 codebase, 2 implementations

### Business Value
- ✅ Market coverage: Legacy + Modern
- ✅ Easy deployment
- ✅ No support tickets về .NET 8.0
- ✅ Customer satisfaction

## 12.2. Lessons Learned

1. **Multi-Target Frameworks** - Conditional compilation best practices
2. **Process Isolation** - IPC techniques
3. **.NET Compatibility** - Framework evolution understanding
4. **Documentation** - Write during development

## 12.3. Future Roadmap

### v1.1
- [ ] Async API cho .NET 4.5+
- [ ] Custom temp directory
- [ ] Progress reporting
- [ ] Improved error messages

### v2.0
- [ ] In-process rendering (no extract)
- [ ] Linux/macOS support
- [ ] Custom DPI settings
- [ ] NuGet package

---

## 📞 Support

**Tài Liệu:**
- README.md - Hướng dẫn cơ bản
- BUILD_GUIDE.md - Build instructions
- DEPLOYMENT.md - Deployment guide
- CODE_FLOW.md - Code flow details

**Issues:** GitHub Issues

**License:** MIT License

---

**Built with ❤️ for multi-framework compatibility**

**Version:** 1.0.0 | **Date:** 2025-11-13 | **Author:** ZplRenderer Team
