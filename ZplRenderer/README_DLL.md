# ZplRenderer - DLL Đã Merge

## Giới thiệu

ZplRenderer.dll là thư viện .NET 8.0 chuyển đổi file ZPL (Zebra Programming Language) sang các định dạng ảnh.

## Cấu trúc Package

Package bao gồm:
- **ZplRenderer.dll** (6.4MB) - DLL chính đã merge tất cả managed dependencies:
  - BinaryKits.Zpl.Viewer
  - BinaryKits.Zpl.Label
  - iText7 (PDF generation)
  - SixLabors.ImageSharp
  - BouncyCastle.Cryptography
  - Và tất cả dependencies khác

## Cách sử dụng

### 1. Thêm reference vào project

```xml
<ItemGroup>
  <Reference Include="ZplRenderer">
    <HintPath>path\to\ZplRenderer.dll</HintPath>
  </Reference>
</ItemGroup>
```

### 2. Code example

```csharp
using ZplRenderer.Core.Interfaces;
using ZplRenderer.Infrastructure.Renderers;

// Tạo renderer
IZplRenderer renderer = new ZplRenderService();

// Chuyển đổi ZPL sang PNG
await renderer.ConvertZplToFileAsync("input.zpl", "output", "png");

// Chuyển đổi sang JPG
await renderer.ConvertZplToFileAsync("input.zpl", "output", "jpg");

// Chuyển đổi sang PDF (tất cả labels trong 1 file)
await renderer.ConvertZplToFileAsync("input.zpl", "output", "pdf");
```

### 3. Formats hỗ trợ

- **PNG** - Định dạng mặc định, chất lượng cao
- **JPG/JPEG** - Định dạng nén, file nhỏ hơn
- **PDF** - Tất cả labels được ghép vào 1 file PDF

## Lưu ý quan trọng

⚠️ **Native Dependencies**: 

ZplRenderer sử dụng SkiaSharp cho rendering, đây là native library. Nếu gặp lỗi "Could not load SkiaSharp", bạn cần:

1. Deploy thêm native libraries từ package SkiaSharp
2. Hoặc đảm bảo runtime có SkiaSharp natives

## Build từ source

```bash
cd ZplRenderer/ZplRenderer
dotnet build -c Release
```

Output: `bin/Release/net8.0/ZplRenderer.dll`

## Dependencies đã merge

Tất cả managed assemblies sau đã được merge vào DLL:
- BinaryKits.Zpl.Viewer 1.3.0
- BinaryKits.Zpl.Label 3.3.0
- itext.kernel, itext.layout, itext.io, itext.commons
- itext.bouncy-castle-adapter
- BouncyCastle.Cryptography
- SixLabors.ImageSharp 3.1.12

## License

Project sử dụng:
- BinaryKits.Zpl.Viewer (MIT License)
- iText7 (AGPL License)
- SixLabors.ImageSharp (Apache License 2.0)

Vui lòng tuân thủ license của các thư viện này khi sử dụng.
