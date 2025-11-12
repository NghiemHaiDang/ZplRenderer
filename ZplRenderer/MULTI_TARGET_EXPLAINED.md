# Multi-Target Framework - Giải Thích Chi Tiết

## Tổng Quan

**Multi-Target** (hay Multi-Targeting) là kỹ thuật cho phép một project .NET build ra nhiều phiên bản DLL khác nhau, mỗi phiên bản tối ưu cho một .NET framework cụ thể.

---

## 1. Multi-Target Là Gì?

### Định nghĩa

```
Single Project → Build → Multiple Output DLLs
                         ├─ net8.0/ZplRenderer.dll    (cho .NET 8.0+)
                         ├─ net40/ZplRenderer.dll     (cho .NET 4.0+)
                         └─ netstandard2.0/...        (nếu có)
```

**Thay vì:**
- Tạo 2 projects riêng: `ZplRenderer.Net8` và `ZplRenderer.Net40`
- Maintain 2 codebases khác nhau
- Quản lý 2 solutions

**Bạn có:**
- 1 project duy nhất: `ZplRenderer.csproj`
- 1 codebase với conditional compilation
- Build 1 lần → Được nhiều DLLs

---

## 2. Tại Sao Cần Multi-Target?

### Vấn đề: .NET Framework Fragmentation

```
Khách hàng A              Khách hàng B              Khách hàng C
├─ Windows Server 2012    ├─ Windows 10             ├─ Linux
├─ .NET Framework 4.0     ├─ .NET 8.0               ├─ .NET 8.0
└─ Legacy apps            └─ Modern apps            └─ Cross-platform
```

**Nếu chỉ build cho .NET 8.0:**
- ❌ Khách hàng A không dùng được (không có .NET 8.0)
- ✅ Khách hàng B, C OK

**Nếu chỉ build cho .NET 4.0:**
- ✅ Khách hàng A OK
- ⚠️ Khách hàng B, C dùng được nhưng không tối ưu (thiếu features mới)

**Với Multi-Target:**
- ✅ Khách hàng A: Dùng net40/ZplRenderer.dll
- ✅ Khách hàng B: Dùng net8.0/ZplRenderer.dll
- ✅ Khách hàng C: Dùng net8.0/ZplRenderer.dll

---

## 3. Cấu Hình Multi-Target

### 3.1 File .csproj

```xml
<!-- ZplRenderer.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Library</OutputType>

    <!-- ⭐ MULTI-TARGET CONFIGURATION -->
    <TargetFrameworks>net40;net8.0</TargetFrameworks>
    <!--              ^^^^  ^^^^^^                   -->
    <!--              |     |                        -->
    <!--              |     └─ .NET 8.0             -->
    <!--              └─ .NET Framework 4.0          -->

    <LangVersion>latest</LangVersion>
  </PropertyGroup>
</Project>
```

**Lưu ý:**
- Số nhiều: `<TargetFrameworks>` (có chữ "s")
- Cách nhau bằng dấu `;`
- Thứ tự không quan trọng

### 3.2 Target Framework Monikers (TFM)

| TFM | Framework | Ý nghĩa |
|-----|-----------|---------|
| `net40` | .NET Framework 4.0 | Windows only, legacy |
| `net45` | .NET Framework 4.5 | Windows only |
| `net48` | .NET Framework 4.8 | Windows only, latest .NET FW |
| `netstandard2.0` | .NET Standard 2.0 | Cross-platform, compatible |
| `net6.0` | .NET 6 | Cross-platform, LTS |
| `net7.0` | .NET 7 | Cross-platform, STS |
| `net8.0` | .NET 8 | Cross-platform, LTS, latest |

**Tại sao chọn net40 và net8.0?**
- `net40`: Compatibility rộng nhất cho legacy (2010+)
- `net8.0`: Hiệu năng cao nhất, modern features

---

## 4. Build Process Chi Tiết

### 4.1 Khi chạy `dotnet build`

```
┌─────────────────────────────────────────────────┐
│ 1. MSBuild đọc .csproj                          │
│    → Phát hiện: TargetFrameworks = "net40;net8.0"│
└──────────────┬──────────────────────────────────┘
               │
               ├─→ Build Target 1: net8.0
               │   ├─ Compile code với C# latest
               │   ├─ Reference .NET 8.0 BCL
               │   ├─ Restore NuGet packages (net8.0 compatible)
               │   └─ Output: bin/Debug/net8.0/ZplRenderer.dll
               │
               └─→ Build Target 2: net40
                   ├─ Compile code với C# compatible với .NET 4.0
                   ├─ Reference .NET Framework 4.0 BCL
                   ├─ Restore NuGet packages (net40 compatible)
                   └─ Output: bin/Debug/net40/ZplRenderer.dll
```

### 4.2 Output Structure

```
bin/
└── Debug/  (hoặc Release/)
    ├── net8.0/                    ← Build lần 1
    │   ├── ZplRenderer.dll        (~200KB)
    │   ├── ZplRenderer.pdb
    │   ├── BinaryKits.Zpl.Viewer.dll
    │   ├── SkiaSharp.dll
    │   └── ... (nhiều dependencies)
    │
    └── net40/                     ← Build lần 2
        ├── ZplRenderer.dll        (~26MB, embedded Console.exe)
        ├── ZplRenderer.pdb
        └── System.*.dll           (minimal dependencies)
```

**Lưu ý:**
- Cùng 1 project, 2 DLLs khác nhau hoàn toàn
- Khác về kích thước, dependencies, capabilities

---

## 5. Conditional Compilation

### 5.1 Vấn đề: Code Khác Nhau Cho Mỗi Target

.NET 4.0 không hỗ trợ:
- Async/await
- Modern libraries (SkiaSharp 3.x, iText7 9.x)
- C# 12 features

.NET 8.0 có tất cả:
- Async/await, Task
- Modern NuGet packages
- Latest C# features

**Giải pháp:** Conditional Compilation

### 5.2 Preprocessor Directives

```csharp
// ZplRenderService.cs
namespace ZplRenderer.Infrastructure.Renderers
{
    public class ZplRenderService : IZplRenderer
    {
#if NET40
        // ========================================
        // CODE CHỈ COMPILE KHI BUILD CHO NET 4.0
        // ========================================

        private static string consoleExePath;

        // SYNC method cho .NET 4.0
        public void ConvertZplToFile(string zplFilePath,
                                     string outputDirectory,
                                     string format)
        {
            // Extract embedded Console.exe
            EnsureConsoleAppExtracted();

            // Execute process
            var process = Process.Start(consoleExePath, args);
            process.WaitForExit();
        }

#else
        // ========================================
        // CODE CHỈ COMPILE KHI BUILD CHO NET 8.0
        // ========================================

        // ASYNC method cho .NET 8.0
        public async Task ConvertZplToFileAsync(string zplFilePath,
                                                 string outputDirectory,
                                                 string format)
        {
            // Direct rendering với BinaryKits.Zpl.Viewer
            using var analyzer = new ZplAnalyzer(printerStorage);
            var analyzeInfo = await analyzer.AnalyzeAsync(zplText);
            // ...
        }

#endif
    }
}
```

### 5.3 Các Preprocessor Symbols

| Symbol | Build Target | Ý nghĩa |
|--------|--------------|---------|
| `NET40` | net40 | .NET Framework 4.0 |
| `NET45` | net45 | .NET Framework 4.5 |
| `NET48` | net48 | .NET Framework 4.8 |
| `NET8_0` | net8.0 | .NET 8.0 |
| `NETSTANDARD2_0` | netstandard2.0 | .NET Standard 2.0 |

**Sử dụng:**

```csharp
#if NET40
    // Code for .NET Framework 4.0
#elif NET8_0
    // Code for .NET 8.0
#else
    // Code for other targets
#endif
```

### 5.4 Usings Conditional

```csharp
// ============================================
// USINGS CHO .NET 4.0
// ============================================
#if NET40
using System;
using System.Diagnostics;      // Cho Process.Start
using System.IO;
using System.Reflection;       // Cho Assembly.GetManifestResourceStream

// ============================================
// USINGS CHO .NET 8.0
// ============================================
#else
using BinaryKits.Zpl.Viewer;
using BinaryKits.Zpl.Viewer.Models;
using iText.IO.Image;
using iText.Kernel.Pdf;
using SixLabors.ImageSharp;
using System.Threading.Tasks;
#endif
```

**Tại sao cần?**
- .NET 4.0 không có BinaryKits.Zpl.Viewer (cần .NET 5+)
- Nếu không dùng `#if`, sẽ bị compile error cho net40

---

## 6. NuGet Packages Multi-Target

### 6.1 Conditional Package References

```xml
<!-- ZplRenderer.csproj -->

<!-- ================================================ -->
<!-- PACKAGES CHỈ CHO .NET 8.0                        -->
<!-- ================================================ -->
<ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
  <PackageReference Include="BinaryKits.Zpl.Viewer" Version="1.3.0" />
  <PackageReference Include="itext7" Version="9.0.0" />
  <PackageReference Include="SixLabors.ImageSharp" Version="3.1.12" />
  <PackageReference Include="SkiaSharp" Version="3.119.1" />
</ItemGroup>

<!-- ================================================ -->
<!-- PACKAGES CHỈ CHO .NET 4.0                        -->
<!-- ================================================ -->
<ItemGroup Condition="'$(TargetFramework)' == 'net40'">
  <PackageReference Include="Microsoft.Bcl.Async" Version="1.0.168" />
  <!-- Không có BinaryKits, SkiaSharp, ... vì không compatible -->
</ItemGroup>
```

**Tại sao?**
- BinaryKits.Zpl.Viewer yêu cầu .NET 5+
- iText7 9.x yêu cầu .NET 5+
- SkiaSharp 3.x yêu cầu .NET 6+

Nếu cố reference cho net40 → Build error!

### 6.2 Embedded Resource Conditional

```xml
<!-- Embed Console.exe CHỈ cho .NET 4.0 -->
<ItemGroup Condition="'$(TargetFramework)' == 'net40'">
  <EmbeddedResource Include="ZplRenderer.Console\bin\Release\net8.0\win-x64\publish\ZplRenderer.Console.exe">
    <LogicalName>ZplRenderer.Console.exe</LogicalName>
  </EmbeddedResource>
</ItemGroup>
```

**Tại sao chỉ net40?**
- net8.0 không cần embedded exe (direct rendering)
- net40 cần exe để proxy sang .NET 8.0

---

## 7. So Sánh 2 Targets

### 7.1 Feature Comparison

| Feature | net40 Build | net8.0 Build |
|---------|-------------|--------------|
| **API** | `ConvertZplToFile(...)` (sync) | `ConvertZplToFileAsync(...)` (async) |
| **Implementation** | Wrapper + Process execution | Direct rendering |
| **Dependencies** | Minimal | Full (20+ DLLs) |
| **Rendering Engine** | External Console.exe | Built-in libraries |
| **File Size** | 26MB (self-contained) | ~200KB + deps |
| **First Run Overhead** | ~700ms (extract) | 0ms |
| **Runtime Requirement** | .NET FW 4.0 only | .NET 8.0 runtime |
| **Cross-platform** | ❌ Windows only | ✅ Win/Linux/Mac |

### 7.2 Architecture Comparison

```
┌──────────────────────────────────────────────────┐
│ NET 4.0 BUILD                                    │
├──────────────────────────────────────────────────┤
│                                                  │
│  User Code (.NET 4.0)                            │
│       ↓                                          │
│  ZplRenderService.ConvertZplToFile()             │
│       ↓                                          │
│  Extract Console.exe (embedded)                  │
│       ↓                                          │
│  Process.Start(Console.exe)  ← New Process!      │
│       ↓                                          │
│  Console.exe (NET 8.0 self-contained)            │
│       ↓                                          │
│  BinaryKits → SkiaSharp → Output                 │
│                                                  │
└──────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────┐
│ NET 8.0 BUILD                                    │
├──────────────────────────────────────────────────┤
│                                                  │
│  User Code (.NET 8.0)                            │
│       ↓                                          │
│  ZplRenderService.ConvertZplToFileAsync()        │
│       ↓                                          │
│  BinaryKits.Zpl.Viewer (in-process)              │
│       ↓                                          │
│  SkiaSharp (in-process)                          │
│       ↓                                          │
│  Output Files                                    │
│                                                  │
└──────────────────────────────────────────────────┘
```

---

## 8. Best Practices Multi-Targeting

### 8.1 Code Organization

```csharp
// ✅ GOOD: Shared interface
namespace ZplRenderer.Core.Interfaces
{
    public interface IZplRenderer
    {
        // NET 4.0: sync implementation
        // NET 8.0: async implementation
    }
}

// ✅ GOOD: Platform-specific implementation
namespace ZplRenderer.Infrastructure.Renderers
{
    public class ZplRenderService : IZplRenderer
    {
#if NET40
        // NET 4.0 specific code
#else
        // NET 8.0 specific code
#endif
    }
}
```

### 8.2 Naming Conventions

**❌ BAD:**
```
ZplRenderer.Net40.csproj
ZplRenderer.Net80.csproj
```

**✅ GOOD:**
```
ZplRenderer.csproj  (multi-target)
```

### 8.3 Testing Multi-Target

```bash
# Test NET 8.0 build
dotnet test -f net8.0

# Test NET 4.0 build
dotnet test -f net40

# Test all targets
dotnet test
```

---

## 9. Advantages vs Disadvantages

### ✅ Advantages

1. **Single Codebase**
   - Maintain 1 project thay vì nhiều
   - Bug fix 1 lần, apply cho tất cả targets

2. **Flexibility**
   - User chọn DLL phù hợp với môi trường
   - Tối ưu cho từng platform

3. **Modern SDK Features**
   - Dùng được SDK-style project
   - Modern tooling support

4. **Package Compatibility**
   - Publish 1 NuGet package support nhiều frameworks

### ⚠️ Disadvantages

1. **Build Time**
   - Build nhiều targets → chậm hơn
   - Mỗi target = 1 lần compile

2. **Complexity**
   - Cần hiểu preprocessor directives
   - Conditional packages/references

3. **Testing Overhead**
   - Phải test tất cả targets
   - Mỗi target có thể có bugs riêng

4. **Debugging Confusion**
   - IDE có thể nhầm lẫn active target
   - Breakpoints có thể không hit

---

## 10. Real-World Examples

### 10.1 Popular Libraries Using Multi-Targeting

```
Newtonsoft.Json:
  ├─ net20
  ├─ net35
  ├─ net40
  ├─ net45
  ├─ netstandard1.0
  ├─ netstandard2.0
  └─ net6.0

AutoMapper:
  ├─ netstandard2.0
  ├─ netstandard2.1
  └─ net6.0

Serilog:
  ├─ net45
  ├─ net461
  ├─ netstandard1.3
  ├─ netstandard2.0
  └─ net5.0
```

### 10.2 ZplRenderer Strategy

```
ZplRenderer:
  ├─ net40        ← Legacy compatibility (wrapper architecture)
  └─ net8.0       ← Modern performance (direct architecture)

Rationale:
- net40: Maximum compatibility (Windows Server 2008+)
- net8.0: Maximum performance (latest features)
- Không support net45-net48: net40 đã cover được
- Không support net6.0-net7.0: net8.0 là LTS, no need STS versions
```

---

## 11. How to Choose Targets

### Decision Matrix

```
Cần support Windows Server 2008-2012?
│
├─ YES → Include net40
│
└─ NO → Start from net6.0+

Cần support .NET Core 3.1? (out of support)
│
├─ YES → Include netcoreapp3.1 (not recommended)
│
└─ NO → Use net6.0+ only

Cần cross-platform?
│
├─ YES → Use netstandard2.0 or net6.0+
│
└─ NO → .NET Framework targets OK
```

### Recommended Combinations

| Scenario | Targets | Reasoning |
|----------|---------|-----------|
| **Maximum compatibility** | `net40;net8.0` | Legacy + Modern |
| **Modern only** | `net8.0` | Latest LTS |
| **Library for others** | `netstandard2.0;net8.0` | Broad compat + Modern |
| **Enterprise** | `net48;net8.0` | Latest .NET FW + Modern |

---

## 12. Debugging Multi-Target Projects

### 12.1 Select Active Target (Visual Studio)

```
Solution Explorer
  → Right-click project
    → Properties
      → Application
        → Target Framework dropdown
          → Select: net40 hoặc net8.0
```

### 12.2 Command Line Debug

```bash
# Debug net8.0 build
dotnet build -f net8.0
dotnet run --project YourApp.csproj --framework net8.0

# Debug net40 build
dotnet build -f net40
# (Run exe trực tiếp)
```

### 12.3 Conditional Breakpoints

```csharp
#if NET40
    // Breakpoint here chỉ hit khi debug net40
    Debugger.Break();
#else
    // Breakpoint here chỉ hit khi debug net8.0
    Debugger.Break();
#endif
```

---

## 13. Migration Path

### From Single Target to Multi-Target

```bash
# BEFORE: Single target
<TargetFramework>net8.0</TargetFramework>

# AFTER: Multi-target
<TargetFrameworks>net40;net8.0</TargetFrameworks>
```

**Steps:**
1. Change `TargetFramework` → `TargetFrameworks` (add 's')
2. Add multiple targets with `;` separator
3. Add conditional package references
4. Add `#if` directives for incompatible code
5. Build and fix errors for each target
6. Test all targets

---

## 14. Common Pitfalls

### ❌ Pitfall 1: Wrong Symbol

```csharp
// WRONG
#if NET8.0  // ❌ Underscore missing!
    ...
#endif

// CORRECT
#if NET8_0  // ✅ Underscore for version
    ...
#endif
```

### ❌ Pitfall 2: Package Not Compatible

```xml
<!-- WRONG: BinaryKits không support net40 -->
<ItemGroup>
  <PackageReference Include="BinaryKits.Zpl.Viewer" Version="1.3.0" />
</ItemGroup>

<!-- CORRECT: Conditional -->
<ItemGroup Condition="'$(TargetFramework)' == 'net8.0'">
  <PackageReference Include="BinaryKits.Zpl.Viewer" Version="1.3.0" />
</ItemGroup>
```

### ❌ Pitfall 3: Using Unavailable APIs

```csharp
#if NET40
    // ❌ WRONG: async/await không có trong .NET 4.0
    public async Task DoSomething()
    {
        await Task.Delay(100);
    }
#endif

// ✅ CORRECT: Sync method cho .NET 4.0
#if NET40
    public void DoSomething()
    {
        System.Threading.Thread.Sleep(100);
    }
#else
    public async Task DoSomethingAsync()
    {
        await Task.Delay(100);
    }
#endif
```

---

## Tóm Tắt

```
┌────────────────────────────────────────────────────┐
│ MULTI-TARGET FRAMEWORK                             │
├────────────────────────────────────────────────────┤
│                                                    │
│  1 Project                                         │
│    ↓                                               │
│  <TargetFrameworks>net40;net8.0</TargetFrameworks> │
│    ↓                                               │
│  MSBuild compiles 2 times                          │
│    ├─→ net40 → bin/Debug/net40/ZplRenderer.dll    │
│    └─→ net8.0 → bin/Debug/net8.0/ZplRenderer.dll  │
│                                                    │
│  Benefits:                                         │
│  ✅ Single codebase                                │
│  ✅ Maximum compatibility                          │
│  ✅ Platform-specific optimizations                │
│                                                    │
│  Requirements:                                     │
│  ⚠️ Conditional compilation (#if NET40)            │
│  ⚠️ Conditional package references                 │
│  ⚠️ Test all targets                               │
│                                                    │
└────────────────────────────────────────────────────┘
```

---

**Tác giả:** Claude Code
**Ngày:** 2025-11-12
**Version:** 1.0
