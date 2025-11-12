# ZplRenderer - Hướng Dẫn Sử Dụng

## 📋 Giới Thiệu

**ZplRenderer** là thư viện .NET cho phép chuyển đổi file ZPL (Zebra Programming Language) sang các định dạng PDF, PNG hoặc JPG.

### ✨ Tính Năng

- ✅ Chuyển đổi ZPL sang PDF, PNG, JPG
- ✅ Hỗ trợ .NET Framework 4.0+
- ✅ Chỉ cần 1 file DLL duy nhất
- ✅ Không cần cài đặt dependencies
- ✅ Tự động extract và chạy engine xử lý
- ✅ Xử lý nhiều labels trong một file ZPL

---

## 🎯 Yêu Cầu Hệ Thống

### Môi Trường Phát Triển (Developer)
- **.NET SDK:** 8.0+ (để build từ source)
- **.NET Framework:** 4.0 Developer Pack (để build target NET 4.0)
- **Visual Studio:** 2022+ (khuyến nghị) hoặc VS Code
- **Hệ điều hành:** Windows 7 trở lên

### Môi Trường Chạy (End User)

#### ✅ Cho ứng dụng .NET 4.0 - 4.8 (Legacy)
- **OS:** Windows 7/8/10/11 (x64)
- **.NET Framework:** 4.0 trở lên (có sẵn trong Windows)
- **Dung lượng:** ~52 MB (26MB DLL + 26MB temp khi chạy lần đầu)
- **Quyền:** Quyền ghi vào `%TEMP%` folder

**⚠️ QUAN TRỌNG:**
- **KHÔNG CẦN** cài .NET 8.0 trên máy đích
- **KHÔNG CẦN** cài thêm dependency nào khác
- DLL đã bao gồm .NET 8.0 runtime (self-contained)
- Chỉ cần .NET Framework 4.0 là đủ!

#### ✅ Cho ứng dụng .NET 8.0+
- **OS:** Windows/Linux/macOS
- **.NET Runtime:** 8.0+
- **Dung lượng:** ~15 MB (DLL + dependencies)

---

## 📦 Cài Đặt

### Bước 1: Build hoặc Download DLL

#### Option A: Build từ source
```bash
# Clone repository
git clone <repository-url>
cd ZplRenderer

# Build project
dotnet build -c Release
```

Output files sẽ nằm tại:
- **.NET 8.0+**: `bin/Release/net8.0/ZplRenderer.dll` (~200KB + dependencies)
- **.NET 4.0+**: `bin/Release/net40/ZplRenderer.dll` (~26MB, self-contained)

**Xem chi tiết:** [BUILD_GUIDE.md](BUILD_GUIDE.md)

#### Option B: Download pre-built DLL
Lấy file DLL từ Releases hoặc build output folder

### Bước 2: Chọn DLL phù hợp với project

| Ứng dụng của bạn | Chọn DLL | Kích thước | Đặc điểm |
|------------------|----------|------------|----------|
| .NET Framework 4.0 - 4.8 | `net40/ZplRenderer.dll` | 26MB | ✅ Self-contained<br>✅ Không cần .NET 8.0<br>✅ Chỉ cần 1 file DLL<br>⚠️ Lần đầu extract 26MB vào %TEMP% |
| .NET 8.0+ | `net8.0/ZplRenderer.dll` | ~200KB | ✅ Nhỏ gọn<br>⚠️ Cần copy dependencies<br>⚠️ Cần .NET 8.0 runtime |

**Khuyến nghị:**
- Môi trường legacy (chỉ có .NET 4.0): Dùng `net40/ZplRenderer.dll`
- Môi trường hiện đại (.NET 8.0+): Dùng `net8.0/ZplRenderer.dll`

### Bước 3: Thêm Reference vào Project

#### Cách 1: Qua Visual Studio
1. Right-click vào project → **Add** → **Reference**
2. Click **Browse** → Chọn file `ZplRenderer.dll` (net40 hoặc net8.0)
3. Click **OK**

#### Cách 2: Edit .csproj trực tiếp
```xml
<ItemGroup>
  <!-- For .NET 4.0 projects -->
  <Reference Include="ZplRenderer">
    <HintPath>path\to\net40\ZplRenderer.dll</HintPath>
  </Reference>

  <!-- For .NET 8.0+ projects -->
  <PackageReference Include="ZplRenderer">
    <HintPath>path\to\net8.0\ZplRenderer.dll</HintPath>
  </PackageReference>
</ItemGroup>
```

---

## 🚀 Cách Sử Dụng

### 1. Basic Usage - .NET Framework 4.0 - 4.8

```csharp
using System;
using ZplRenderer.Infrastructure.Renderers;

class Program
{
    static void Main(string[] args)
    {
        // Tạo service
        var service = new ZplRenderService();

        // Chuyển đổi ZPL sang PDF (SYNC method cho .NET 4.0)
        service.ConvertZplToFile(
            zplFilePath: @"C:\input\label.zpl",
            outputDirectory: @"C:\output",
            format: "pdf"
        );

        Console.WriteLine("Conversion completed!");
        // ✅ Chạy được trên môi trường CHỈ CÓ .NET Framework 4.0
        // ✅ KHÔNG CẦN cài .NET 8.0 trên máy
    }
}
```

### 2. Basic Usage - .NET 8.0+

```csharp
using ZplRenderer.Infrastructure.Renderers;

class Program
{
    static async Task Main(string[] args)
    {
        var service = new ZplRenderService();

        // ASYNC method cho .NET 8.0
        await service.ConvertZplToFileAsync(
            zplFilePath: @"C:\input\label.zpl",
            outputDirectory: @"C:\output",
            format: "pdf"
        );

        Console.WriteLine("Conversion completed!");
    }
}
```

### 3. Chuyển đổi sang PNG (.NET 4.0)

```csharp
var service = new ZplRenderService();

service.ConvertZplToFile(
    zplFilePath: @"C:\input\label.zpl",
    outputDirectory: @"C:\output",
    format: "png"
);
```

### 4. Chuyển đổi sang JPG (.NET 4.0)

```csharp
var service = new ZplRenderService();

service.ConvertZplToFile(
    zplFilePath: @"C:\input\label.zpl",
    outputDirectory: @"C:\output",
    format: "jpg"  // hoặc "jpeg"
);
```

---

## 📝 Ví Dụ Nâng Cao

### 1. Xử lý nhiều file với error handling

```csharp
using System;
using System.IO;
using ZplRenderer.Infrastructure.Renderers;

public class ZplConverter
{
    public void ProcessMultipleFiles(string inputFolder, string outputFolder)
    {
        var service = new ZplRenderService();
        string[] zplFiles = Directory.GetFiles(inputFolder, "*.zpl");

        int success = 0;
        int failed = 0;

        foreach (string zplFile in zplFiles)
        {
            try
            {
                Console.Write("Processing: " + Path.GetFileName(zplFile) + "...");

                service.ConvertZplToFile(zplFile, outputFolder, "pdf");

                Console.WriteLine(" OK");
                success++;
            }
            catch (Exception ex)
            {
                Console.WriteLine(" FAILED: " + ex.Message);
                failed++;
            }
        }

        Console.WriteLine();
        Console.WriteLine("Summary:");
        Console.WriteLine("  Succeeded: " + success);
        Console.WriteLine("  Failed: " + failed);
    }
}
```

### 2. Validation và error handling đầy đủ

```csharp
using System;
using System.IO;
using ZplRenderer.Infrastructure.Renderers;

public class SafeZplConverter
{
    private ZplRenderService service;

    public SafeZplConverter()
    {
        service = new ZplRenderService();
    }

    public bool ConvertZplSafely(string zplFile, string outputFolder, string format)
    {
        // Validate input file
        if (!File.Exists(zplFile))
        {
            Console.WriteLine("ERROR: File not found - " + zplFile);
            return false;
        }

        // Validate format
        if (!IsValidFormat(format))
        {
            Console.WriteLine("ERROR: Invalid format '" + format + "'. Use: png, jpg, or pdf");
            return false;
        }

        // Create output directory
        try
        {
            Directory.CreateDirectory(outputFolder);
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: Cannot create output folder - " + ex.Message);
            return false;
        }

        // Perform conversion
        try
        {
            Console.WriteLine("Converting " + Path.GetFileName(zplFile) + " to " + format.ToUpper() + "...");
            service.ConvertZplToFile(zplFile, outputFolder, format);
            Console.WriteLine("SUCCESS!");
            return true;
        }
        catch (FileNotFoundException ex)
        {
            Console.WriteLine("ERROR: File not found - " + ex.Message);
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine("ERROR: Access denied - " + ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.Message);
            return false;
        }
    }

    private bool IsValidFormat(string format)
    {
        string[] validFormats = { "png", "jpg", "jpeg", "pdf" };
        foreach (string validFormat in validFormats)
        {
            if (format.Equals(validFormat, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
```

### 3. Batch processor với progress reporting

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using ZplRenderer.Infrastructure.Renderers;

public class BatchZplProcessor
{
    public class ProcessResult
    {
        public int TotalFiles { get; set; }
        public int Succeeded { get; set; }
        public int Failed { get; set; }
        public List<string> Errors { get; set; }

        public ProcessResult()
        {
            Errors = new List<string>();
        }

        public void PrintSummary()
        {
            Console.WriteLine();
            Console.WriteLine("========== Summary ==========");
            Console.WriteLine("Total files: " + TotalFiles);
            Console.WriteLine("Succeeded: " + Succeeded);
            Console.WriteLine("Failed: " + Failed);

            if (Errors.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Errors:");
                foreach (string error in Errors)
                {
                    Console.WriteLine("  - " + error);
                }
            }
        }
    }

    public ProcessResult ProcessDirectory(string inputDir, string outputDir, string format)
    {
        var result = new ProcessResult();
        var service = new ZplRenderService();

        // Validate input directory
        if (!Directory.Exists(inputDir))
        {
            throw new DirectoryNotFoundException("Input directory not found: " + inputDir);
        }

        // Create output directory
        Directory.CreateDirectory(outputDir);

        // Get all ZPL files
        string[] files = Directory.GetFiles(inputDir, "*.zpl");
        result.TotalFiles = files.Length;

        Console.WriteLine("Found " + files.Length + " ZPL files");
        Console.WriteLine();

        // Process each file
        for (int i = 0; i < files.Length; i++)
        {
            string file = files[i];
            string fileName = Path.GetFileName(file);

            try
            {
                // Progress indicator
                Console.Write("[" + (i + 1) + "/" + files.Length + "] " + fileName + "...");

                service.ConvertZplToFile(file, outputDir, format);

                Console.WriteLine(" OK");
                result.Succeeded++;
            }
            catch (Exception ex)
            {
                Console.WriteLine(" FAILED");
                string errorMsg = fileName + ": " + ex.Message;
                result.Errors.Add(errorMsg);
                result.Failed++;
            }
        }

        return result;
    }
}

// Cách sử dụng:
// var processor = new BatchZplProcessor();
// var result = processor.ProcessDirectory(@"C:\input", @"C:\output", "pdf");
// result.PrintSummary();
```

---

## 🎨 Các Format Hỗ Trợ

| Format | Extension | Mô tả |
|--------|-----------|-------|
| PNG | `.png` | Ảnh chất lượng cao, trong suốt |
| JPEG | `.jpg`, `.jpeg` | Ảnh nén, kích thước nhỏ |
| PDF | `.pdf` | Document, nhiều trang |

### Output Files

**Đối với PDF:**
```
input.zpl → output/input.pdf (tất cả labels trong 1 file)
```

**Đối với PNG/JPG:**
```
input.zpl → output/label_0001.png
            output/label_0002.png
            output/label_0003.png
            ...
```

---

## ⚙️ Cơ Chế Hoạt Động

ZplRenderer có 2 triển khai khác nhau tùy theo framework:

### .NET 8.0 - Direct Rendering

```
ZplRenderService → BinaryKits.Zpl.Viewer → SkiaSharp → Output File
   (Trực tiếp)      (Parse ZPL)              (Render)     (PNG/JPG/PDF)
```

**Đặc điểm:**
- Xử lý trực tiếp trong cùng process
- Nhanh, hiệu năng cao
- Sử dụng các thư viện hiện đại

### .NET 4.0 - Wrapper Architecture

```
ZplRenderService (.NET 4.0)
    │
    ├─→ Extract Console.exe (lần đầu)
    │   Thời gian: ~0.7-1 giây (file 26MB)
    │   Vị trí: %TEMP%\ZplRenderer\
    │
    └─→ Execute Console.exe (.NET 8.0)
        │
        └─→ BinaryKits.Zpl.Viewer → SkiaSharp → Output File
```

**Lần chạy đầu tiên:**
1. Extract console app từ embedded resource → `%TEMP%\ZplRenderer\`
2. Execute console app để xử lý ZPL
3. Trả kết quả về

**Các lần chạy tiếp theo:**
1. Sử dụng lại console app đã extract (bỏ qua bước extract)
2. Execute console app để xử lý ZPL (nhanh hơn ~700ms)

**Chi tiết luồng code:** [CODE_FLOW.md](CODE_FLOW.md)

---

## 🐛 Xử Lý Lỗi

### Lỗi thường gặp

#### 1. "Embedded console app not found"

**Nguyên nhân:** DLL bị thiếu embedded resource

**Giải pháp:**
- Rebuild lại project
- Đảm bảo file console app đã được publish trước khi build DLL

#### 2. "Access denied" khi extract

**Nguyên nhân:** Không có quyền ghi vào temp folder

**Giải pháp:**
- Chạy ứng dụng với quyền Administrator
- Hoặc thay đổi temp directory:
```csharp
// Chỉnh sửa trong source code nếu cần
string tempDir = @"C:\MyApp\Temp"; // Thay vì GetTempPath()
```

#### 3. "ZPL conversion failed with exit code X"

**Nguyên nhân:** Console app gặp lỗi khi xử lý ZPL

**Giải pháp:**
- Kiểm tra file ZPL có hợp lệ không
- Kiểm tra output folder có tồn tại không
- Kiểm tra format có đúng không (png, jpg, pdf)

#### 4. Antivirus cảnh báo

**Nguyên nhân:** Hành vi extract file exe từ DLL

**Giải pháp:**
- Thêm exception cho `%TEMP%\ZplRenderer\*.exe`
- Hoặc whitelist toàn bộ ứng dụng

---

## 📊 Thông Tin Kỹ Thuật

### Kích thước file

| Component | Size |
|-----------|------|
| ZplRenderer.dll (net40) | 26 MB |
| Extracted console app | 26 MB |
| **Tổng disk usage** | **52 MB** |

### Hiệu năng

| Metric | Lần đầu | Lần sau |
|--------|---------|---------|
| Extract time | ~0.7-1s | 0s (skip) |
| Process time | Tùy ZPL | Tùy ZPL |
| **Total** | **~1-2s + process** | **~0.5s + process** |

### Memory Usage

- **.NET 4.0 wrapper:** ~20-30 MB
- **Console app process:** ~100-200 MB
- **Peak total:** ~250 MB

---

## 🔧 Tối Ưu Hiệu Suất

### 1. Pre-extract khi deploy (Khuyến nghị)

Thêm vào code khởi tạo ứng dụng:

```csharp
using System;
using System.IO;
using System.Reflection;

public static class ZplRendererSetup
{
    public static void PreExtract()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), "ZplRenderer");
        Directory.CreateDirectory(tempDir);
        string consolePath = Path.Combine(tempDir, "ZplRenderer.Console.exe");

        // Force extract
        if (File.Exists(consolePath))
            return; // Already extracted

        Assembly assembly = Assembly.LoadFrom("ZplRenderer.dll");
        using (Stream stream = assembly.GetManifestResourceStream("ZplRenderer.Console.exe"))
        {
            if (stream == null)
                throw new Exception("Embedded console app not found");

            using (FileStream fileStream = new FileStream(consolePath,
                FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 81920))
            {
                byte[] buffer = new byte[81920];
                int bytesRead;
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);
                }
            }
        }

        Console.WriteLine("ZplRenderer pre-extracted successfully");
    }
}

// Gọi khi khởi động ứng dụng:
// ZplRendererSetup.PreExtract();
```

### 2. Cleanup temp folder

```csharp
public static void CleanupTempFiles()
{
    string tempDir = Path.Combine(Path.GetTempPath(), "ZplRenderer");

    if (Directory.Exists(tempDir))
    {
        try
        {
            Directory.Delete(tempDir, recursive: true);
            Console.WriteLine("Temp files cleaned up");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Cannot cleanup: " + ex.Message);
        }
    }
}

// Gọi khi thoát ứng dụng hoặc uninstall
```

---

## 💡 Best Practices

### 1. Reuse Service Instance

✅ **Good:**
```csharp
var service = new ZplRenderService();
for (int i = 0; i < files.Length; i++)
{
    service.ConvertZplToFile(files[i], output, "pdf");
}
```

❌ **Bad:**
```csharp
for (int i = 0; i < files.Length; i++)
{
    var service = new ZplRenderService(); // Không cần tạo mới mỗi lần
    service.ConvertZplToFile(files[i], output, "pdf");
}
```

### 2. Always handle exceptions

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
    // Handle other errors
    Console.WriteLine("Error: " + ex.Message);
}
```

### 3. Validate input trước khi xử lý

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

service.ConvertZplToFile(zplFile, output, format);
```

---

## 📞 Support & Issues

Nếu gặp vấn đề:

1. **Kiểm tra:** File ZPL có hợp lệ không
2. **Kiểm tra:** Quyền ghi file/folder
3. **Kiểm tra:** .NET Framework version
4. **Kiểm tra:** Antivirus settings

---

## 📄 License

MIT License - Xem file LICENSE để biết thêm chi tiết.

---

## 🎯 Quick Start Checklist

- [ ] Build hoặc download `ZplRenderer.dll` ([BUILD_GUIDE.md](BUILD_GUIDE.md))
- [ ] Chọn DLL phù hợp (.NET 8.0 hoặc .NET 4.0)
- [ ] Add reference vào project
- [ ] Thêm `using ZplRenderer.Infrastructure.Renderers;`
- [ ] Tạo instance: `var service = new ZplRenderService();`
- [ ] Gọi: `service.ConvertZplToFile(input, output, format);` (.NET 4.0) hoặc `await service.ConvertZplToFileAsync(...)` (.NET 8.0)
- [ ] Test với file ZPL mẫu
- [ ] Deploy DLL cùng ứng dụng

---

## 📚 Tài Liệu Kỹ Thuật

### 🎯 Cho End Users
- **[DEPLOYMENT.md](DEPLOYMENT.md)** - Hướng dẫn deployment chi tiết cho .NET 4.0 và .NET 8.0 ⭐
- **[QUICK_START.md](QUICK_START.md)** - Hướng dẫn sử dụng nhanh

### 🔧 Cho Developers
- **[ARCHITECTURE_DECISION.md](ARCHITECTURE_DECISION.md)** - Tại sao chọn Wrapper Architecture? (Luồng tư duy từ đầu) ⭐⭐⭐
- **[BUILD_GUIDE.md](BUILD_GUIDE.md)** - Hướng dẫn build project từ source
- **[WHY_CONSOLE_APP.md](WHY_CONSOLE_APP.md)** - Giải thích tại sao có ZplRenderer.Console project
- **[MULTI_TARGET_EXPLAINED.md](MULTI_TARGET_EXPLAINED.md)** - Giải thích Multi-Target Framework chi tiết
- **[CODE_FLOW.md](CODE_FLOW.md)** - Mô tả chi tiết luồng hoạt động của code
- **[TECHNICAL_STACK.md](TECHNICAL_STACK.md)** - Giải thích các công nghệ sử dụng
- **[HOW_DLL_MERGING_WORKS.md](HOW_DLL_MERGING_WORKS.md)** - Cơ chế merge DLL (Costura.Fody)
- **[RELEASE_NOTES.md](RELEASE_NOTES.md)** - Lịch sử phát hành và thay đổi

---

**Built with ❤️ for multi-framework compatibility (.NET 8.0 & .NET Framework 4.0)**
