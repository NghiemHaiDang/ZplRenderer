# ZplRenderer - Hỗ trợ .NET Framework 4.0

## 📦 Kết quả Build

Sau khi build, bạn sẽ có **1 FILE DLL DUY NHẤT** cho .NET 4.0:

```
ZplRenderer/bin/Release/net40/ZplRenderer.dll  (105 MB)
```

### ✅ File này chứa:
- Wrapper code cho .NET 4.0
- Console app .NET 8.0 (embedded)
- Tất cả dependencies cần thiết

### ❌ Các file dependency khác (không cần quan tâm):
```
- Microsoft.Threading.Tasks.dll
- System.IO.dll
- System.Runtime.dll
- System.Threading.Tasks.dll
```
(Các file này được copy tự động, nhưng bạn không cần deploy chúng nếu không muốn)

---

## 🚀 Cách sử dụng từ .NET 4.0

### 1. Thêm reference vào project

**Trong Visual Studio:**
- Right-click project → Add → Reference
- Browse → Chọn `ZplRenderer.dll` (từ folder `net40`)

**Hoặc trong .csproj:**
```xml
<ItemGroup>
  <Reference Include="ZplRenderer">
    <HintPath>path\to\net40\ZplRenderer.dll</HintPath>
  </Reference>
</ItemGroup>
```

### 2. Code sử dụng

```csharp
using ZplRenderer.Infrastructure.Renderers;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            var service = new ZplRenderService();

            // Convert ZPL to PDF
            service.ConvertZplToFile(
                zplFilePath: @"C:\input\labels.zpl",
                outputDirectory: @"C:\output",
                format: "pdf"
            );

            Console.WriteLine("Conversion completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
```

### Supported formats:
- `"png"` - PNG image
- `"jpg"` hoặc `"jpeg"` - JPEG image
- `"pdf"` - PDF document

---

## 🔧 Cách hoạt động

1. **Lần chạy đầu tiên:**
   - DLL tự động extract console app ra `C:\Users\YourName\AppData\Local\Temp\ZplRenderer\`
   - Mất khoảng 2-3 giây để extract (chỉ 1 lần duy nhất)

2. **Các lần chạy sau:**
   - Sử dụng lại file đã extract
   - Chạy nhanh hơn

3. **Process flow:**
   ```
   Your .NET 4.0 App
        ↓
   ZplRenderer.dll (wrapper)
        ↓
   Extract console app to temp
        ↓
   Run console app (.NET 8.0)
        ↓
   Process ZPL → Output file
        ↓
   Return result to .NET 4.0
   ```

---

## 📊 Thông tin kỹ thuật

| Đặc điểm | Giá trị |
|----------|---------|
| Kích thước DLL | ~105 MB |
| Target Framework | .NET Framework 4.0 |
| Dependencies | Microsoft.Bcl.Async (tự động) |
| Console app embedded | .NET 8.0 self-contained |
| Temp folder | `%TEMP%\ZplRenderer\` |
| Extract time | 2-3 giây (lần đầu) |

---

## ⚠️ Lưu ý

1. **File DLL lớn:** 105 MB là bình thường vì chứa toàn bộ .NET 8.0 runtime + dependencies
2. **Quyền ghi:** Cần quyền ghi vào temp folder
3. **Antivirus:** Một số antivirus có thể chặn việc extract file. Thêm exception nếu cần.
4. **First run:** Lần chạy đầu chậm hơn do phải extract file

---

## 🏗️ Build lại từ source

### Yêu cầu:
- .NET SDK 8.0
- .NET Framework 4.0 Developer Pack

### Bước 1: Build console app
```bash
dotnet publish ZplRenderer/ZplRenderer.Console/ZplRenderer.Console.csproj ^
  --configuration Release ^
  --runtime win-x64 ^
  --self-contained true ^
  -p:PublishSingleFile=true ^
  -p:IncludeNativeLibrariesForSelfExtract=true
```

### Bước 2: Build main DLL
```bash
dotnet build ZplRenderer/ZplRenderer.csproj --configuration Release -f net40
```

### Hoặc dùng build script:
```bash
# Windows
build.bat

# PowerShell
.\build.ps1
```

---

## 📝 Ví dụ nâng cao

### Xử lý nhiều file ZPL
```csharp
var service = new ZplRenderService();
string[] zplFiles = Directory.GetFiles(@"C:\input", "*.zpl");

foreach (string zplFile in zplFiles)
{
    try
    {
        service.ConvertZplToFile(zplFile, @"C:\output", "pdf");
        Console.WriteLine("Processed: " + Path.GetFileName(zplFile));
    }
    catch (Exception ex)
    {
        Console.WriteLine("Failed: " + Path.GetFileName(zplFile) + " - " + ex.Message);
    }
}
```

### Batch processing với error handling
```csharp
public class ZplBatchProcessor
{
    public void ProcessBatch(string inputFolder, string outputFolder, string format)
    {
        var service = new ZplRenderService();
        var files = Directory.GetFiles(inputFolder, "*.zpl");
        int success = 0;
        int failed = 0;

        foreach (var file in files)
        {
            try
            {
                service.ConvertZplToFile(file, outputFolder, format);
                success++;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error processing {0}: {1}",
                    Path.GetFileName(file), ex.Message);
                failed++;
            }
        }

        Console.WriteLine("Batch complete: {0} succeeded, {1} failed", success, failed);
    }
}
```

---

## 🐛 Troubleshooting

### Lỗi: "Embedded console app not found"
→ Console app chưa được embed. Chạy lại build script.

### Lỗi: "Access denied" khi extract
→ Không có quyền ghi vào temp folder. Chạy với admin hoặc thay đổi temp directory.

### Lỗi: "ZPL conversion failed with exit code X"
→ Console app gặp lỗi. Kiểm tra:
- File ZPL có hợp lệ không
- Output folder có tồn tại không
- Format có đúng không (png, jpg, pdf)

### File DLL quá lớn?
→ Đây là trade-off để có 1 file DLL duy nhất. Nếu muốn nhỏ hơn:
- Dùng wrapper riêng + console app riêng (2 files)
- Hoặc nâng cấp lên .NET 4.7.2+

---

## 📄 License

MIT License - Xem file LICENSE để biết thêm chi tiết.

---

## 💡 Tips

1. **Deployment:** Chỉ cần copy `ZplRenderer.dll` vào folder application
2. **Cleanup:** File trong temp folder có thể xóa, DLL sẽ tự extract lại
3. **Performance:** Sau lần đầu, performance tương đương native .NET 4.0
4. **Multi-threading:** An toàn khi gọi từ nhiều thread

---

**Built with ❤️ for .NET 4.0 compatibility**
