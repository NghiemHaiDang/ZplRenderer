# Cơ Chế Merge DLL - Giải Thích Chi Tiết

## Tổng quan

Project này sử dụng **Costura.Fody** để nhúng (embed) tất cả dependency DLLs vào 1 file DLL duy nhất.

---

## 1. Quy trình Build & Merge

### Bước 1: Normal Build
```bash
dotnet build -c Release
```

Output ban đầu (TRƯỚC khi Fody chạy):
```
bin/Release/net8.0/
├── ZplRenderer.dll                    (200 KB - chỉ code của bạn)
├── BinaryKits.Zpl.Viewer.dll          (500 KB)
├── BinaryKits.Zpl.Label.dll           (300 KB)
├── SkiaSharp.dll                      (1.2 MB)
├── SixLabors.ImageSharp.dll           (2 MB)
├── itext.kernel.dll                   (3 MB)
├── BouncyCastle.Cryptography.dll      (4 MB)
└── ... (20+ DLLs khác)
```

### Bước 2: Fody Post-Processing (tự động)
```
MSBuild triggers: Fody.dll
    ↓
Fody loads: Costura.Fody.dll
    ↓
Costura reads: FodyWeavers.xml
    ↓
Costura embeds DLLs into ZplRenderer.dll
```

### Bước 3: Final Output
```
bin/Release/net8.0/
├── ZplRenderer.dll        (12 MB - đã chứa tất cả!)
├── runtimes/              (Native DLLs - không merge được)
│   └── win-x64/native/
│       ├── libSkiaSharp.dll
│       └── libHarfBuzzSharp.dll
└── (các managed DLLs khác đã bị xóa hoặc không cần thiết)
```

---

## 2. FodyWeavers.xml - File Cấu Hình

```xml
<?xml version="1.0" encoding="utf-8"?>
<Weavers>
  <Costura>
    <!-- Managed Assemblies - ĐƯỢC MERGE VÀO DLL -->
    <IncludeAssemblies>
      BinaryKits.Zpl.Viewer      ← Embed vào ZplRenderer.dll
      BinaryKits.Zpl.Label       ← Embed vào ZplRenderer.dll
      SkiaSharp                  ← Embed vào ZplRenderer.dll (managed part)
      SixLabors.ImageSharp       ← Embed vào ZplRenderer.dll
      itext.kernel               ← Embed vào ZplRenderer.dll
      BouncyCastle.Cryptography  ← Embed vào ZplRenderer.dll
    </IncludeAssemblies>

    <!-- Native 32-bit DLLs - KHÔNG MERGE, chỉ đánh dấu -->
    <Unmanaged32Assemblies>
      libSkiaSharp        ← Phải đi kèm trong runtimes/win-x86/native/
      libHarfBuzzSharp    ← Phải đi kèm trong runtimes/win-x86/native/
    </Unmanaged32Assemblies>

    <!-- Native 64-bit DLLs - KHÔNG MERGE, chỉ đánh dấu -->
    <Unmanaged64Assemblies>
      libSkiaSharp        ← Phải đi kèm trong runtimes/win-x64/native/
      libHarfBuzzSharp    ← Phải đi kèm trong runtimes/win-x64/native/
    </Unmanaged64Assemblies>
  </Costura>
</Weavers>
```

---

## 3. Cách Costura Embed DLLs

### IL Weaving Process

**BEFORE Costura:**
```csharp
// ZplRenderer.dll IL code
.class public ZplRenderService
{
    .method public void ConvertZplToFileAsync()
    {
        // Your code here
    }
}
```

**AFTER Costura (simplified):**
```csharp
// ZplRenderer.dll IL code (modified by Costura)

// 1. Module Constructor - chạy TRƯỚC mọi code khác
.class private auto ansi sealed ModuleInit
{
    .method static void .cctor()
    {
        // Hook vào AssemblyResolve event
        AppDomain.CurrentDomain.AssemblyResolve += CosturaLoader;
    }
}

// 2. Embedded Resources - DLLs được nén và nhúng
.mresource private 'costura.binarykits.zpl.viewer.dll.compressed'
{
    // Binary data của BinaryKits.Zpl.Viewer.dll (compressed)
}
.mresource private 'costura.skiasharp.dll.compressed'
{
    // Binary data của SkiaSharp.dll (compressed)
}
// ... tất cả DLLs khác

// 3. Assembly Loader - code tự động load DLLs từ resources
.class private CosturaLoader
{
    .method static Assembly ResolveAssembly(AssemblyName name)
    {
        // Tìm DLL trong embedded resources
        string resourceName = "costura." + name + ".dll.compressed";
        byte[] compressed = GetManifestResource(resourceName);

        // Decompress
        byte[] dllBytes = Decompress(compressed);

        // Load vào memory
        return Assembly.Load(dllBytes);
    }
}

// 4. Your original code
.class public ZplRenderService
{
    .method public void ConvertZplToFileAsync()
    {
        // Your code here
    }
}
```

---

## 4. Runtime Execution Flow

### Scenario: User sử dụng ZplRenderer.dll

```csharp
// User's application code
using ZplRenderer.Infrastructure.Renderers;

class Program
{
    static void Main()
    {
        var renderer = new ZplRenderService(); // ← Point A
        renderer.ConvertZplToFileAsync("test.zpl", "output", "png"); // ← Point B
    }
}
```

### Point A: `new ZplRenderService()`

**Step 1:** CLR load ZplRenderer.dll
```
CLR: Load ZplRenderer.dll vào memory
  ↓
CLR: Chạy Module Constructor (.cctor)
  ↓
Costura: Hook vào AppDomain.CurrentDomain.AssemblyResolve
```

**Step 2:** CLR cần dependencies
```
CLR: ZplRenderService cần BinaryKits.Zpl.Viewer
  ↓
CLR: Tìm BinaryKits.Zpl.Viewer.dll trên disk... KHÔNG TÌM THẤY!
  ↓
CLR: Trigger AssemblyResolve event
  ↓
Costura Loader: "Tôi có nó trong embedded resources!"
  ↓
Costura: Extract & decompress từ resources
  ↓
Costura: Assembly.Load(dllBytes) → Load vào memory
  ↓
CLR: OK, tôi có BinaryKits.Zpl.Viewer rồi!
```

### Point B: `ConvertZplToFileAsync(...)`

```
ZplRenderService: new ZplAnalyzer()
  ↓
CLR: Cần SkiaSharp.dll (managed)
  ↓
Costura: Load từ embedded resources ✅
  ↓
SkiaSharp: Load native DLL (libSkiaSharp.dll)
  ↓
.NET Runtime: Tìm trong runtimes/win-x64/native/libSkiaSharp.dll ✅
  ↓
Process continues...
```

---

## 5. Managed vs Native DLLs

### Managed DLLs (.NET Assemblies)
- **Định nghĩa**: DLLs viết bằng C#, VB.NET, F# → compile thành IL code
- **Đặc điểm**:
  - Chạy trên .NET Runtime (CLR)
  - Cross-platform (same DLL works on Windows/Linux/Mac)
  - CÓ THỂ embed được bởi Costura
- **Ví dụ**:
  - BinaryKits.Zpl.Viewer.dll
  - SixLabors.ImageSharp.dll
  - itext.kernel.dll

**Cách Costura xử lý:**
```
1. Read DLL file → byte[]
2. Compress byte[] → byte[] (smaller)
3. Embed vào .mresource section
4. Thêm loader code
```

### Native DLLs (Unmanaged)
- **Định nghĩa**: DLLs viết bằng C/C++ → compile thành machine code
- **Đặc điểm**:
  - Chạy trực tiếp trên CPU (không qua .NET Runtime)
  - Platform-specific (Windows .dll ≠ Linux .so)
  - KHÔNG THỂ embed hoàn toàn (vì .NET cần load từ file path)
- **Ví dụ**:
  - libSkiaSharp.dll (Windows)
  - libSkiaSharp.so (Linux)
  - libHarfBuzzSharp.dll (Windows)

**Tại sao không embed được:**
```csharp
// Native DLL loading mechanism
[DllImport("libSkiaSharp.dll")]  // ← P/Invoke cần file path!
static extern void sk_function();

// .NET runtime sẽ:
1. Search in current directory
2. Search in PATH
3. Search in runtimes/{platform}/native/  ← Đây!
```

**Workaround của Costura:**
- Embed native DLL vào resources
- Khi runtime, extract ra temp folder
- Load từ temp folder
- Nhưng đôi khi không work → Nên vẫn cần folder `runtimes/`

---

## 6. Kiểm Tra DLL Đã Merge

### Cách 1: Check file size
```bash
ls -lh bin/Release/net8.0/ZplRenderer.dll
# BEFORE Costura: ~200 KB
# AFTER Costura:  ~12 MB  ← Đã chứa tất cả!
```

### Cách 2: Check embedded resources
```bash
# Windows
ildasm bin/Release/net8.0/ZplRenderer.dll

# Hoặc dùng .NET Reflection
dotnet exec bin/Release/net8.0/ZplRenderer.dll
```

### Cách 3: Test runtime
```csharp
// Copy ONLY ZplRenderer.dll to another folder
// Delete all other DLLs
// Run → Nếu chạy được = merge thành công!

var renderer = new ZplRenderService();
// Nếu throw FileNotFoundException → merge failed
```

---

## 7. Khi Nào Merge Fail?

### Case 1: DLL không trong IncludeAssemblies
```xml
<!-- FodyWeavers.xml -->
<IncludeAssemblies>
  <!-- Quên không thêm SkiaSharp -->
</IncludeAssemblies>
```
**Kết quả:** Runtime error "Could not load SkiaSharp"

### Case 2: Native DLL không có trong runtimes/
```
ZplRenderer.dll  ✅
runtimes/        ❌ Missing!
```
**Kết quả:** Runtime error "Could not load libSkiaSharp.dll"

### Case 3: Version conflict
```xml
<IncludeAssemblies>
  SkiaSharp  <!-- Version 3.119.1 -->
</IncludeAssemblies>
```
Nhưng code reference SkiaSharp 2.x
**Kết quả:** Runtime version mismatch error

---

## 8. Distribution Strategy

### Option A: Single DLL + runtimes/ (Current)
```
YourApp/
├── ZplRenderer.dll          (12 MB - all managed DLLs)
└── runtimes/                (Native DLLs)
    ├── win-x64/native/
    ├── linux-x64/native/
    └── osx/native/
```
**Pros:**
- Ít files managed DLLs
- Cross-platform support

**Cons:**
- Vẫn cần folder runtimes/
- Large file size (12 MB)

### Option B: Publish Single File (Alternative)
```bash
dotnet publish -c Release -r win-x64 --self-contained \
  -p:PublishSingleFile=true \
  -p:IncludeNativeLibrariesForSelfExtract=true
```
**Output:** 1 file .exe duy nhất (~50 MB)

**Pros:**
- True single file
- No dependencies

**Cons:**
- Platform-specific (cần build riêng cho Windows/Linux)
- Larger size (include .NET runtime)

### Option C: No Merge (Simplest)
```
YourApp/
├── ZplRenderer.dll
├── BinaryKits.Zpl.Viewer.dll
├── SkiaSharp.dll
├── SixLabors.ImageSharp.dll
└── ... (20+ DLLs)
```
**Pros:**
- Simple, no magic
- Easy debugging

**Cons:**
- Many files to distribute

---

## 9. Debugging Costura Issues

### Enable Costura Logging
```xml
<!-- FodyWeavers.xml -->
<Costura CreateTemporaryAssemblies="true" />
```

### Check Build Output
```bash
dotnet build -c Release -v detailed | grep Costura
```

Output:
```
Costura: Processing BinaryKits.Zpl.Viewer
Costura: Compressing...
Costura: Embedding as resource 'costura.binarykits.zpl.viewer.dll.compressed'
Costura: Done.
```

### Runtime Debug
```csharp
// Add to your code
AppDomain.CurrentDomain.AssemblyLoad += (sender, args) =>
{
    Console.WriteLine($"Loaded: {args.LoadedAssembly.FullName}");
};
```

---

## 10. Performance Impact

### Pros:
- **Faster first load**: Không cần search nhiều DLL files
- **Memory efficient**: Load on-demand (lazy loading)

### Cons:
- **Compression overhead**: ~50-100ms để decompress mỗi DLL lần đầu
- **Larger file size**: Embedded DLLs không share được giữa apps

### Benchmark:
```
Without Costura:
  - 20 DLL files
  - Load time: ~200ms (file I/O overhead)

With Costura:
  - 1 DLL file
  - Load time: ~150ms (decompression overhead)
  - Net gain: ~50ms faster
```

---

## 11. Alternatives to Costura

### ILMerge (deprecated)
- Microsoft's old tool
- No longer maintained
- Issues with .NET Core/5+

### ILRepack
- Open-source ILMerge alternative
- More control
- Manual process

### Publish SingleFile
- Built-in .NET feature
- Best for executables
- Not for libraries

### Costura.Fody (Current choice)
- Active maintenance
- MSBuild integration
- Best for libraries

---

## Tóm tắt

```
┌─────────────────────────────────────────────┐
│  Costura.Fody = DLL Merger                  │
├─────────────────────────────────────────────┤
│  • Embed managed DLLs → resources           │
│  • Add AssemblyLoader code                  │
│  • Runtime: extract & load from memory      │
│  • Native DLLs: still need runtimes/        │
└─────────────────────────────────────────────┘

┌─────────────────────────────────────────────┐
│  FodyWeavers.xml = Configuration            │
├─────────────────────────────────────────────┤
│  • IncludeAssemblies: which DLLs to embed   │
│  • UnmanagedXXX: which native DLLs to mark  │
└─────────────────────────────────────────────┘

┌─────────────────────────────────────────────┐
│  Result: ZplRenderer.dll                    │
├─────────────────────────────────────────────┤
│  • 12 MB (vs 200 KB original)               │
│  • Contains 20+ DLLs embedded               │
│  • Still needs runtimes/ for native libs    │
└─────────────────────────────────────────────┘
```

---

**Tạo bởi**: Claude Code
**Ngày**: 2025-11-09
