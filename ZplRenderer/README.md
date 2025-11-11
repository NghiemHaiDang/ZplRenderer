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

### Môi Trường Phát Triển
- **.NET Framework:** 4.0 trở lên
- **Visual Studio:** 2010 hoặc mới hơn (khuyến nghị 2019+)
- **Hệ điều hành:** Windows 7 trở lên

### Môi Trường Chạy
- **OS:** Windows 7/8/10/11 (x64)
- **.NET Framework:** 4.0 trở lên (đã có sẵn trong Windows)
- **Dung lượng:** ~26 MB (DLL) + 26 MB (temp folder khi chạy lần đầu)
- **Quyền:** Quyền ghi vào temp folder (`%TEMP%`)

---

## 📦 Cài Đặt

### Bước 1: Download DLL

Lấy file DLL từ folder build output:
```
ZplRenderer/bin/Release/net40/ZplRenderer.dll
```

### Bước 2: Thêm Reference vào Project

#### Cách 1: Qua Visual Studio
1. Right-click vào project → **Add** → **Reference**
2. Click **Browse** → Chọn file `ZplRenderer.dll`
3. Click **OK**

#### Cách 2: Edit .csproj trực tiếp
```xml
<ItemGroup>
  <Reference Include="ZplRenderer">
    <HintPath>path\to\ZplRenderer.dll</HintPath>
  </Reference>
</ItemGroup>
```

---

## 🚀 Cách Sử Dụng

### 1. Basic Usage - Chuyển đổi đơn giản

```csharp
using ZplRenderer.Infrastructure.Renderers;

class Program
{
    static void Main(string[] args)
    {
        // Tạo service
        var service = new ZplRenderService();

        // Chuyển đổi ZPL sang PDF
        service.ConvertZplToFile(
            zplFilePath: @"C:\input\label.zpl",
            outputDirectory: @"C:\output",
            format: "pdf"
        );

        Console.WriteLine("Conversion completed!");
    }
}
```

### 2. Chuyển đổi sang PNG

```csharp
var service = new ZplRenderService();

service.ConvertZplToFile(
    zplFilePath: @"C:\input\label.zpl",
    outputDirectory: @"C:\output",
    format: "png"
);
```

### 3. Chuyển đổi sang JPG

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

### Lần chạy đầu tiên:

```
1. DLL extract console app → %TEMP%\ZplRenderer\
   Thời gian: ~0.7-1 giây (file 26MB)

2. Chạy console app để xử lý ZPL
   Thời gian: Tùy độ phức tạp của ZPL

3. Trả kết quả về
```

### Các lần chạy tiếp theo:

```
1. Sử dụng lại console app đã extract
   (Bỏ qua bước extract)

2. Chạy console app để xử lý ZPL
   Thời gian: Nhanh hơn lần đầu
```

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

- [ ] Download `ZplRenderer.dll` từ `bin/Release/net40/`
- [ ] Add reference vào project
- [ ] Thêm `using ZplRenderer.Infrastructure.Renderers;`
- [ ] Tạo instance: `var service = new ZplRenderService();`
- [ ] Gọi: `service.ConvertZplToFile(input, output, format);`
- [ ] Test với file ZPL mẫu
- [ ] Deploy DLL cùng ứng dụng

---

**Built with ❤️ for .NET Framework 4.0 compatibility**
