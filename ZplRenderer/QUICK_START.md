# ZplRenderer - Quick Start Guide

## 🚀 5 Phút Để Bắt Đầu

### Bước 1: Lấy DLL (10 giây)

Copy file DLL này vào project của bạn:
```
ZplRenderer/bin/Release/net40/ZplRenderer.dll
```

---

### Bước 2: Add Reference (20 giây)

**Visual Studio:**
1. Right-click project → **Add** → **Reference**
2. Click **Browse** → Chọn `ZplRenderer.dll`
3. Click **OK**

---

### Bước 3: Viết Code (2 phút)

```csharp
using ZplRenderer.Infrastructure.Renderers;

class Program
{
    static void Main(string[] args)
    {
        // Tạo service
        var service = new ZplRenderService();

        // Chuyển ZPL sang PDF
        service.ConvertZplToFile(
            zplFilePath: @"C:\temp\label.zpl",
            outputDirectory: @"C:\temp\output",
            format: "pdf"
        );

        Console.WriteLine("Done!");
    }
}
```

---

### Bước 4: Chạy Thử (1 phút)

1. Chuẩn bị file ZPL mẫu: `C:\temp\label.zpl`
2. Press **F5** để run
3. Kiểm tra output: `C:\temp\output\`

---

## 🎯 Các Format Hỗ Trợ

### PDF - Tất cả labels trong 1 file
```csharp
service.ConvertZplToFile(input, output, "pdf");
```

### PNG - Mỗi label 1 file PNG
```csharp
service.ConvertZplToFile(input, output, "png");
```

### JPG - Mỗi label 1 file JPG
```csharp
service.ConvertZplToFile(input, output, "jpg");
```

---

## 💡 Tips

### ✅ DO: Reuse service instance
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

---

## 🐛 Lỗi Thường Gặp

### "File not found"
→ Kiểm tra đường dẫn file ZPL

### "Access denied"
→ Chạy với quyền Administrator

### "Invalid format"
→ Chỉ dùng: `"pdf"`, `"png"`, `"jpg"`

---

## 📖 Tài Liệu Đầy Đủ

- **README.md** - Hướng dẫn chi tiết
- **EXAMPLES.cs** - 9 ví dụ code
- **RELEASE_NOTES.md** - Thông tin version

---

## ✅ Checklist

- [ ] Copy `ZplRenderer.dll` vào project
- [ ] Add reference
- [ ] Add `using ZplRenderer.Infrastructure.Renderers;`
- [ ] Tạo `new ZplRenderService()`
- [ ] Gọi `ConvertZplToFile()`
- [ ] Test với file ZPL mẫu
- [ ] Done! 🎉

---

**Thời gian:** ~5 phút từ download đến chạy được!

**Cần trợ giúp?** Xem `README.md` để biết thêm chi tiết.
