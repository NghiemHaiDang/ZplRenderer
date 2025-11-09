# Giải Thích Các Công Nghệ Sử Dụng

## Tổng quan

ZplRenderer là một thư viện .NET 8.0 chuyển đổi file ZPL (Zebra Programming Language) sang các định dạng ảnh (PNG, JPG, PDF). Project sử dụng nhiều công nghệ và thư viện khác nhau để đạt được mục tiêu này.

---

## 1. Nền tảng & Runtime

### .NET 8.0
- **Mô tả**: Phiên bản mới nhất của nền tảng .NET (LTS - Long Term Support)
- **Vai trò**: Nền tảng runtime để chạy ứng dụng
- **Lý do chọn**:
  - Hiệu năng cao
  - Hỗ trợ cross-platform (Windows, Linux, macOS)
  - API hiện đại với async/await
  - LTS support đến 2026

### C# 12
- **Mô tả**: Ngôn ngữ lập trình chính
- **Tính năng sử dụng**:
  - Async/await cho xử lý bất đồng bộ
  - LINQ cho xử lý collection
  - Pattern matching
  - Nullable reference types

---

## 2. Thư viện Xử lý ZPL

### BinaryKits.Zpl.Viewer (v1.3.0)
- **Mô tả**: Thư viện mã nguồn mở để parse và render ZPL
- **Vai trò**: Core engine để chuyển đổi ZPL thành hình ảnh
- **Cách hoạt động**:
  ```csharp
  var analyzer = new ZplAnalyzer(printerStorage);
  var analyzeInfo = analyzer.Analyze(zplText);  // Parse ZPL

  var drawer = new ZplElementDrawer(printerStorage);
  byte[] imageBytes = drawer.Draw(labelInfo.ZplElements);  // Render to image
  ```
- **Output**: Byte array của PNG image
- **License**: MIT License
- **GitHub**: https://github.com/BinaryKits/BinaryKits.Zpl

### BinaryKits.Zpl.Label (v3.3.0)
- **Mô tả**: Thư viện hỗ trợ cho Zpl.Viewer, chứa model definitions
- **Vai trò**: Định nghĩa cấu trúc dữ liệu cho ZPL elements
- **License**: MIT License

---

## 3. Thư viện Graphics & Rendering

### SkiaSharp (v3.119.1)
- **Mô tả**: Cross-platform 2D graphics library, .NET wrapper cho Skia (Google)
- **Vai trò**: Engine render graphics bên dưới BinaryKits.Zpl.Viewer
- **Đặc điểm**:
  - **Native library**: Viết bằng C/C++, cần native DLL
  - **Cross-platform**: Hỗ trợ Windows, Linux, macOS, Android, iOS
  - **High performance**: Được Google sử dụng trong Chrome & Android
- **Native dependencies**:
  - Windows: `libSkiaSharp.dll` (11.4 MB)
  - Linux: `libSkiaSharp.so`
  - macOS: `libSkiaSharp.dylib`
- **Tại sao cần native DLL**:
  - SkiaSharp là wrapper cho C++ library
  - Các thuật toán graphics được optimize ở native code
  - Không thể embed hoàn toàn vào managed assembly

### HarfBuzzSharp (v8.3.1.2)
- **Mô tả**: Text shaping engine
- **Vai trò**: Xử lý text rendering phức tạp (fonts, Unicode, ligatures)
- **Đặc điểm**:
  - **Native library**: Viết bằng C/C++
  - Hỗ trợ nhiều ngôn ngữ & scripts phức tạp
- **Native dependencies**:
  - Windows: `libHarfBuzzSharp.dll` (1.8 MB)
  - Linux/macOS: có file tương ứng
- **Sử dụng bởi**: SkiaSharp để render text

---

## 4. Thư viện Xử lý Ảnh

### SixLabors.ImageSharp (v3.1.12)
- **Mô tả**: Pure C# image processing library
- **Vai trò**: Chuyển đổi format ảnh (PNG → JPG)
- **Cách sử dụng**:
  ```csharp
  using (var image = Image.Load(imageBytes))
  {
      image.Save($"{baseFileName}.jpg", new JpegEncoder());
  }
  ```
- **Ưu điểm**:
  - 100% managed code (không cần native DLL)
  - High performance
  - Nhiều format hỗ trợ: PNG, JPG, GIF, BMP, TIFF, WebP
- **License**: Apache License 2.0
- **Website**: https://sixlabors.com/

---

## 5. Thư viện Tạo PDF

### iText7 (v9.0.0)
- **Mô tả**: Thư viện tạo và xử lý PDF mạnh mẽ
- **Vai trò**: Ghép nhiều label images thành 1 file PDF
- **Packages sử dụng**:
  - `itext.kernel`: Core PDF engine
  - `itext.layout`: Layout & formatting
  - `itext.io`: I/O operations
  - `itext.commons`: Common utilities
- **Cách hoạt động**:
  ```csharp
  var pdfWriter = new PdfWriter(outputFile);
  var pdfDoc = new PdfDocument(pdfWriter);
  var document = new Document(pdfDoc);

  // Add images
  var imageData = ImageDataFactory.Create(imageBytes);
  var pdfImage = new Image(imageData);
  document.Add(pdfImage);
  ```
- **License**: AGPL License (cần chú ý khi commercial)
- **Website**: https://itextpdf.com/

---

## 6. Thư viện Mã hóa

### BouncyCastle.Cryptography (v2.4.0)
- **Mô tả**: Cryptography library
- **Vai trò**: Dependency của iText7 cho PDF signing & encryption
- **Tính năng**:
  - RSA, AES, SHA encryption
  - Digital signatures
  - Certificate handling
- **Được dùng bởi**: iText7 khi cần sign/encrypt PDF

### itext.bouncy-castle-adapter (v9.0.0)
- **Mô tả**: Adapter để iText7 sử dụng BouncyCastle
- **Vai trò**: Bridge giữa iText7 và BouncyCastle

---

## 7. Build Tools & Assembly Merging

### Fody (v6.9.3)
- **Mô tả**: Extensible tool để can thiệp vào quá trình compile
- **Vai trò**: Framework cho Costura.Fody
- **Cơ chế**: IL weaving - chỉnh sửa Intermediate Language sau compile
- **Website**: https://github.com/Fody/Fody

### Costura.Fody (v6.0.0)
- **Mô tả**: Plugin của Fody để embed dependencies vào DLL
- **Vai trò**: Nhúng tất cả managed DLLs vào 1 file DLL duy nhất
- **Cách hoạt động**:
  1. Build project → tạo ZplRenderer.dll + nhiều dependency DLLs
  2. Fody chạy sau build
  3. Costura embed các DLL dependencies vào ZplRenderer.dll
  4. Khi runtime, Costura tự động extract và load từ embedded resources
- **Cấu hình**: File `FodyWeavers.xml`
  ```xml
  <Costura>
    <IncludeAssemblies>
      SkiaSharp
      SixLabors.ImageSharp
      ...
    </IncludeAssemblies>
  </Costura>
  ```
- **Lưu ý**:
  - Chỉ embed được **managed assemblies** (.NET DLLs)
  - **Không embed được native DLLs** (libSkiaSharp.dll)
  - Native DLLs phải đi kèm trong folder `runtimes/`

---

## 8. Kiến trúc Project

### Clean Architecture Pattern
```
ZplRenderer/
├── Core/                    # Business logic layer
│   ├── Interfaces/          # IZplRenderer interface
│   └── Enums/              # OutputFormat enum
├── Infrastructure/          # Implementation layer
│   ├── Renderers/          # ZplRenderService implementation
│   └── Utils/              # MemoryOptimizer, helpers
└── Config/                 # Configuration & constants
    └── AppConstants.cs
```

### Dependency Injection Ready
- Sử dụng Interface (`IZplRenderer`)
- Dễ dàng mock cho unit testing
- Tuân theo SOLID principles

---

## 9. Workflow Xử lý

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
       ├─────► JPG output ──┐
       │                    ▼
       │              ┌─────────────────┐
       │              │ SixLabors.      │ ← Convert PNG→JPG
       │              │ ImageSharp      │
       │              └─────────────────┘
       │
       └─────► PDF output ──┐
                            ▼
                      ┌─────────────────┐
                      │  iText7 +       │ ← Create PDF
                      │  BouncyCastle   │   Add images
                      └─────────────────┘
```

---

## 10. Native Dependencies Distribution

### Vấn đề
- SkiaSharp & HarfBuzzSharp là native libraries
- Mỗi platform cần native DLL riêng

### Giải pháp
- Folder `runtimes/` chứa native DLLs cho mọi platforms:
  ```
  runtimes/
  ├── win-x64/native/
  │   ├── libSkiaSharp.dll
  │   └── libHarfBuzzSharp.dll
  ├── win-x86/native/
  ├── linux-x64/native/
  │   ├── libSkiaSharp.so
  │   └── libHarfBuzzSharp.so
  └── osx/native/
      ├── libSkiaSharp.dylib
      └── libHarfBuzzSharp.dylib
  ```

### Runtime Loading
- .NET runtime tự động detect platform
- Load native DLL từ `runtimes/{platform}/native/`
- Không cần code thêm

---

## 11. Performance Optimizations

### Memory Management
- **MemoryOptimizer.ForceCollect()**: Gọi GC sau mỗi label
- **Streaming**: Đọc file ZPL theo từng dòng, không load toàn bộ vào RAM
- **Buffer**: Sử dụng `List<string>` buffer cho từng label

### Async/Await
- File I/O sử dụng async để không block thread
- Parallel processing có thể scale trong tương lai

---

## 12. Licenses Compliance

| Library | License | Commercial Use |
|---------|---------|----------------|
| .NET 8.0 | MIT | ✅ Free |
| BinaryKits.Zpl.Viewer | MIT | ✅ Free |
| SkiaSharp | MIT | ✅ Free |
| SixLabors.ImageSharp | Apache 2.0 | ✅ Free (với điều kiện) |
| iText7 | AGPL 3.0 | ⚠️ Cần commercial license hoặc open source |
| BouncyCastle | MIT | ✅ Free |
| Fody/Costura | MIT | ✅ Free |

**Lưu ý quan trọng về iText7**:
- AGPL license yêu cầu mở source code nếu dùng trong web services
- Nếu dùng thương mại, cần mua commercial license
- Xem thêm: https://itextpdf.com/how-buy

---

## 13. Tài liệu tham khảo

- **ZPL Language**: [Zebra Programming Guide](https://www.zebra.com/us/en/support-downloads/knowledge-articles/zpl-programming-guide.html)
- **SkiaSharp Docs**: https://learn.microsoft.com/en-us/dotnet/api/skiasharp
- **iText7 Docs**: https://api.itextpdf.com/
- **ImageSharp Docs**: https://docs.sixlabors.com/

---

## Tóm tắt Stack

```
Application Layer:    ZplRenderService (C# 12, .NET 8.0)
                            │
ZPL Processing:       BinaryKits.Zpl.Viewer
                            │
Graphics Engine:      SkiaSharp + HarfBuzzSharp (Native C++)
                            │
Image Processing:     SixLabors.ImageSharp (Pure C#)
                            │
PDF Generation:       iText7 + BouncyCastle
                            │
Assembly Merging:     Costura.Fody + Fody
```

---

**Tạo bởi**: Claude Code
**Ngày cập nhật**: 2025-11-09
