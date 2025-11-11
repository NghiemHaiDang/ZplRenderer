# ZplRenderer - Release Notes

## Version 1.0.0 (2025-11-11)

### 🎉 Initial Release

Phiên bản đầu tiên của ZplRenderer - thư viện chuyển đổi ZPL sang PDF/PNG/JPG cho .NET Framework 4.0+

---

## ✨ Tính Năng

### Core Features
- ✅ **Chuyển đổi ZPL sang PDF**: Hỗ trợ nhiều labels trong một file PDF
- ✅ **Chuyển đổi ZPL sang PNG**: Output chất lượng cao với transparency
- ✅ **Chuyển đổi ZPL sang JPG/JPEG**: Output nén, kích thước nhỏ
- ✅ **Multi-label support**: Tự động tách và xử lý nhiều labels trong một file ZPL
- ✅ **Batch processing**: Xử lý nhiều file ZPL cùng lúc

### Compatibility
- ✅ **.NET Framework 4.0+**: Tương thích với tất cả version từ 4.0 trở lên
- ✅ **Single DLL deployment**: Chỉ cần 1 file DLL duy nhất
- ✅ **No dependencies**: Không cần cài đặt thư viện bổ sung
- ✅ **Windows 7+**: Hỗ trợ tất cả Windows version từ 7 trở lên

### Performance
- ✅ **Optimized size**: DLL chỉ 26 MB (sau trimming)
- ✅ **Fast extraction**: Extract console app trong ~0.7-1 giây
- ✅ **Efficient buffer**: Sử dụng 80KB buffer để tối ưu I/O
- ✅ **Smart caching**: Chỉ extract một lần, reuse cho các lần sau

---

## 🏗️ Kiến Trúc

### Architecture Pattern: Embedded Resource + Process Wrapper

```
.NET 4.0 App → ZplRenderer.dll → Extract Console App → Process ZPL → Output
```

### Components
1. **ZplRenderer.dll (26 MB)**
   - Wrapper code (.NET 4.0)
   - Embedded console app (.NET 8.0)
   - Public API interface

2. **ZplRenderer.Console.exe (Embedded)**
   - Self-contained .NET 8.0 application
   - ZPL processing engine (BinaryKits.Zpl.Viewer)
   - Image rendering (SkiaSharp)
   - PDF generation (iText7)

---

## 📦 Build Configuration

### Multi-Targeting
- **net40**: Wrapper với embedded resource
- **net8.0**: Full implementation với ZPL engine

### Optimization Features
- ✅ **PublishTrimmed**: Giảm size xuống 45%
- ✅ **TrimMode=partial**: An toàn với reflection
- ✅ **InvariantGlobalization**: Loại bỏ globalization data
- ✅ **PublishSingleFile**: Single executable
- ✅ **IncludeNativeLibrariesForSelfExtract**: Embed native DLLs

---

## 🚀 Performance Metrics

### File Size
| Component | Before | After | Reduction |
|-----------|--------|-------|-----------|
| Console.exe | 47 MB | 26 MB | **45%** |
| Total DLL | 47 MB | 26 MB | **45%** |

### Execution Time
| Operation | Time |
|-----------|------|
| First run (with extract) | ~1-2 seconds |
| Subsequent runs | ~0.5 seconds |
| Extract only | ~0.7-1 second |

### Memory Usage
| Process | Memory |
|---------|--------|
| .NET 4.0 wrapper | ~20-30 MB |
| Console app | ~100-200 MB |
| Peak total | ~250 MB |

---

## 📋 API Reference

### ZplRenderService Class

#### Constructor
```csharp
public ZplRenderService()
```

#### Methods
```csharp
public void ConvertZplToFile(
    string zplFilePath,      // Path to input ZPL file
    string outputDirectory,  // Output directory
    string format            // "pdf", "png", "jpg", or "jpeg"
)
```

**Parameters:**
- `zplFilePath`: Absolute path đến file ZPL input
- `outputDirectory`: Thư mục lưu output files
- `format`: Format output ("pdf", "png", "jpg", "jpeg")

**Throws:**
- `FileNotFoundException`: File ZPL không tồn tại
- `ArgumentException`: Format không hợp lệ
- `UnauthorizedAccessException`: Không có quyền ghi file
- `Exception`: Lỗi khác trong quá trình xử lý

---

## 🔧 Technical Details

### Dependencies (.NET 8.0 Console App)
- **BinaryKits.Zpl.Viewer** 1.3.0 - ZPL parsing & rendering
- **iText7** 9.0.0 - PDF generation
- **iText.bouncy-castle-adapter** 9.0.0 - PDF cryptography
- **SixLabors.ImageSharp** 3.1.12 - Image processing
- **SkiaSharp** 3.119.1 - Graphics rendering
- **HarfBuzzSharp** 8.3.1.2 - Text shaping

### Dependencies (.NET 4.0 Wrapper)
- **Microsoft.Bcl.Async** 1.0.168 - Async support for .NET 4.0

### Embedded Resources
- `ZplRenderer.Console.exe` (26 MB) - Embedded in net40 DLL

---

## 📊 Output Format Details

### PDF Output
- **Multi-page**: Tất cả labels trong một file PDF
- **Format**: `{filename}.pdf`
- **Page break**: Mỗi label một trang riêng
- **Quality**: Vector graphics khi có thể

### PNG Output
- **Separate files**: Mỗi label một file PNG
- **Format**: `label_0001.png`, `label_0002.png`, ...
- **Transparency**: Hỗ trợ alpha channel
- **Quality**: Lossless compression

### JPG Output
- **Separate files**: Mỗi label một file JPG
- **Format**: `label_0001.jpg`, `label_0002.jpg`, ...
- **Compression**: Lossy compression
- **Quality**: High quality JPEG

---

## 🔐 Security & Permissions

### Required Permissions
- ✅ Read access to input ZPL file
- ✅ Write access to output directory
- ✅ Write access to `%TEMP%\ZplRenderer\`
- ✅ Execute permission for extracted console app

### Security Considerations
- ✅ No network access required
- ✅ No registry modifications
- ✅ No system service installation
- ✅ Local file processing only
- ⚠️ Antivirus may flag self-extracting behavior

---

## 🐛 Known Issues & Limitations

### Current Limitations
1. **Windows only**: Chỉ hỗ trợ Windows (x64)
2. **Temp folder**: Yêu cầu quyền ghi vào temp folder
3. **First run delay**: Lần chạy đầu tiên mất ~1 giây để extract
4. **Process overhead**: Mỗi lần gọi spawn process mới
5. **File size**: DLL 26 MB có thể lớn cho một số trường hợp

### Known Issues
- ⚠️ Antivirus có thể cảnh báo về file exe được extract
- ⚠️ Một số ZPL command phức tạp có thể không render đúng
- ⚠️ Font rendering phụ thuộc vào font có sẵn trong system

---

## 🔄 Future Roadmap

### Planned Features (v1.1)
- [ ] Add async/await API cho .NET 4.5+
- [ ] Support custom temp directory
- [ ] Add progress reporting
- [ ] Improve error messages
- [ ] Add logging support

### Planned Features (v2.0)
- [ ] In-process rendering (không cần extract)
- [ ] Support .NET 6/7/8 native
- [ ] Linux/macOS support
- [ ] Custom DPI settings
- [ ] Batch processing API improvements

### Performance Improvements
- [ ] Reduce DLL size thêm 20-30%
- [ ] Lazy loading cho dependencies
- [ ] Shared process pool
- [ ] Memory optimization

---

## 📈 Changelog

### v1.0.0 (2025-11-11) - Initial Release

**Added:**
- Initial implementation với embedded resource pattern
- Support for PDF, PNG, JPG output
- Multi-targeting (net40 + net8.0)
- Optimized extraction với 80KB buffer
- Comprehensive error handling
- Documentation & examples

**Optimizations:**
- Reduced console app size từ 47MB → 26MB (45% reduction)
- Fast extraction (~0.7-1s) với large buffer
- Trimmed dependencies để giảm size

---

## 💾 Deployment Checklist

### For Developers
- [ ] Include `ZplRenderer.dll` từ `bin/Release/net40/`
- [ ] Test với sample ZPL files
- [ ] Verify temp folder permissions
- [ ] Test error handling
- [ ] Document usage trong application

### For End Users
- [ ] .NET Framework 4.0+ đã cài đặt
- [ ] Quyền ghi vào temp folder
- [ ] Antivirus exception (nếu cần)
- [ ] Đủ disk space (~52 MB)

---

## 📚 Documentation

### Available Documentation
- ✅ `README.md` - Hướng dẫn sử dụng chi tiết
- ✅ `EXAMPLES.cs` - 9 ví dụ code hoàn chỉnh
- ✅ `RELEASE_NOTES.md` - File này
- ✅ Inline XML documentation trong code

---

## 🤝 Credits

### Third-Party Libraries
- **BinaryKits.Zpl.Viewer** - ZPL rendering engine
- **iText7** - PDF generation
- **SkiaSharp** - Graphics rendering
- **SixLabors.ImageSharp** - Image processing

### Built With
- .NET SDK 8.0
- .NET Framework 4.0 Developer Pack
- Visual Studio / VS Code

---

## 📄 License

MIT License

Copyright (c) 2025 ZplRenderer

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

---

**Version 1.0.0 - Released on November 11, 2025**

**Built with ❤️ for .NET Framework 4.0 compatibility**
