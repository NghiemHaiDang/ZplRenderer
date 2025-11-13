# ZPL Renderer - Implementation Guide for New Requirements

## Yêu cầu mới

### 1. DPI Configuration (203, 300, 600 dpi)
### 2. Label Size Detection
###  3. Font Mapping System
### 4. Return System.Drawing.Image

---

## 1. DPI Configuration

### BinaryKits.Zpl.Viewer Support
BinaryKits.Zpl.Viewer đã hỗ trợ DPI thông qua `PrinterStorage`:

```csharp
var printerStorage = new PrinterStorage();
printerStorage.SetPrinterDpi(300); // 203, 300, or 600
```

### Implementation trong Console App

**Program.cs - Update to accept DPI parameter:**
```csharp
static async Task<int> Main(string[] args)
{
    // Old: args[0]=zplFile, args[1]=outputDir, args[2]=format
    // New: args[0]=zplFile, args[1]=outputDir, args[2]=format, args[3]=dpi (optional)

    int dpi = 203; // default
    if (args.Length >= 4 && int.TryParse(args[3], out int customDpi))
    {
        dpi = customDpi;
    }

    _logger.LogInformation("Using DPI: {Dpi}", dpi);

    await ConvertZplToFileAsync(zplFilePath, outputDirectory, format, dpi);
}
```

**ProcessZplChunkAsync - Use DPI:**
```csharp
static async Task ProcessZplChunkAsync(List<string> zplLines, string outputDirectory,
    string format, int fileIndex, Document? document, int dpi)
{
    string zplText = string.Join(Environment.NewLine, zplLines);

    await Task.Run(() =>
    {
        // Setup printer with DPI
        IPrinterStorage printerStorage = new PrinterStorage();
        printerStorage.SetPrinterDpi(dpi); // <--- KEY CHANGE

        var analyzer = new ZplAnalyzer(printerStorage);
        var drawer = new ZplElementDrawer(printerStorage);

        var analyzeInfo = analyzer.Analyze(zplText);

        foreach (var labelInfo in analyzeInfo.LabelInfos)
        {
            byte[] imageBytes = drawer.Draw(labelInfo.ZplElements);
            // ... rest of code
        }
    });
}
```

---

## 2. Label Size Detection

### Automatic from ZPL
BinaryKits.Zpl.Viewer **TỰ ĐỘNG** detect label size từ ZPL commands:
- `^PW` (Print Width)
- `^LL` (Label Length)

**Không cần code thêm** - Library tự xử lý!

### Manual Override (if needed)
Nếu cần override:

```csharp
var printerStorage = new PrinterStorage();
printerStorage.SetLabelWidth(812);  // width in dots
printerStorage.SetLabelHeight(1218); // height in dots
```

**Conversion từ inches sang dots:**
```
dots = inches * dpi
Example: 4 inches x 6 inches at 203 dpi
  width = 4 * 203 = 812 dots
  height = 6 * 203 = 1218 dots
```

---

## 3. Font Mapping System

### Current Issue
BinaryKits.Zpl.Viewer sử dụng SkiaSharp font system, không trực tiếp map với Windows fonts.

### Solution Approach

#### Option A: Custom Font Mapping (Complex)
Requires modifying BinaryKits source or creating custom drawer.

#### Option B: Font Configuration File (Recommended)
Tạo file config để map fonts:

**fonts.json:**
```json
{
  "fontMappings": [
    {
      "zebraFontName": "ARIALNB1",
      "fontAliasNumber": "0",
      "windowsFontName": "Arial Narrow",
      "fontStyle": "Bold"
    },
    {
      "zebraFontName": "TIMES1",
      "fontAliasNumber": "1",
      "windowsFontName": "Times New Roman",
      "fontStyle": "Regular"
    }
  ]
}
```

**FontMappingService.cs:**
```csharp
public class FontMappingService
{
    private Dictionary<string, FontMapping> _mappings;

    public FontMappingService(string configPath)
    {
        LoadMappings(configPath);
    }

    public FontMapping GetMapping(string zebraFontOrAlias)
    {
        // Lookup by Zebra font name or alias number
        return _mappings.GetValueOrDefault(zebraFontOrAlias);
    }
}
```

#### Option C: Use BinaryKits Default (Easiest)
BinaryKits đã có built-in font mapping. Chỉ cần đảm bảo fonts được install trên system.

**Recommended fonts to install:**
- Arial / Arial Narrow
- Times New Roman
- Courier New
- Verdana

---

## 4. Return System.Drawing.Image

### Update Interface

```csharp
public interface IZplRenderer
{
    // New method
    List<Image> ConvertZplToImages(string zplFilePath, ZplRenderOptions options);
}
```

### Implementation

```csharp
public List<Image> ConvertZplToImages(string zplFilePath, ZplRenderOptions options)
{
    var images = new List<Image>();

    // Read ZPL file
    var zplLines = File.ReadAllLines(zplFilePath);
    var zplText = string.Join(Environment.NewLine, zplLines);

    // Setup printer
    IPrinterStorage printerStorage = new PrinterStorage();
    printerStorage.SetPrinterDpi(options.Dpi);

    if (options.LabelWidth.HasValue)
        printerStorage.SetLabelWidth(options.LabelWidth.Value);
    if (options.LabelHeight.HasValue)
        printerStorage.SetLabelHeight(options.LabelHeight.Value);

    // Render
    var analyzer = new ZplAnalyzer(printerStorage);
    var drawer = new ZplElementDrawer(printerStorage);
    var analyzeInfo = analyzer.Analyze(zplText);

    foreach (var labelInfo in analyzeInfo.LabelInfos)
    {
        byte[] imageBytes = drawer.Draw(labelInfo.ZplElements);

        // Convert byte[] to System.Drawing.Image
        using (var ms = new MemoryStream(imageBytes))
        {
            Image image = Image.FromStream(ms);
            images.Add(new Bitmap(image)); // Create copy
        }
    }

    return images;
}
```

### Important Notes

1. **Memory Management**: Caller phải dispose Images sau khi dùng:
```csharp
var images = renderer.ConvertZplToImages(zplFile, options);
try
{
    foreach (var image in images)
    {
        // Use image
    }
}
finally
{
    foreach (var image in images)
    {
        image?.Dispose();
    }
}
```

2. **Cross-platform**: System.Drawing.Common chỉ hoạt động tốt trên Windows. Linux/Mac cần cấu hình thêm.

---

## Complete Example Usage

```csharp
// Create options
var options = new ZplRenderOptions
{
    Dpi = 300,
    LabelWidth = 1218,  // Optional: 4 inches * 300 dpi
    LabelHeight = 1827  // Optional: 6 inches * 300 dpi
};

// Option 1: Get Images directly
var renderer = new ZplRenderService();
var images = renderer.ConvertZplToImages("label.zpl", options);

try
{
    // Use images
    images[0].Save("output.png", ImageFormat.Png);
}
finally
{
    foreach (var img in images)
        img?.Dispose();
}

// Option 2: Save to file with options
renderer.ConvertZplToFile("label.zpl", "output", "png", options);
```

---

## Migration Steps

### Phase 1: Add DPI Support ✅
1. Update Console app to accept DPI parameter
2. Pass DPI to PrinterStorage
3. Test with 203, 300, 600 dpi

### Phase 2: Add Options Class ✅
1. Create ZplRenderOptions class
2. Update interface với overload methods
3. Maintain backward compatibility

### Phase 3: Add Image Return
1. Implement ConvertZplToImages method
2. Handle memory management
3. Document disposal requirements

### Phase 4: Font Mapping (Optional)
1. Create FontMapping class
2. Load from config file
3. Apply to BinaryKits (if possible)

---

## Testing Checklist

- [ ] Test 203 DPI output
- [ ] Test 300 DPI output
- [ ] Test 600 DPI output
- [ ] Test auto label size detection
- [ ] Test manual label size override
- [ ] Test Image return and disposal
- [ ] Test backward compatibility
- [ ] Memory leak testing
- [ ] Font rendering quality

---

## Known Limitations

1. **Font Mapping**: BinaryKits uses SkiaSharp fonts, may not perfectly match Zebra printer fonts
2. **System.Drawing.Common**: Windows-centric, limited cross-platform support
3. **Memory**: Image objects must be manually disposed
4. **DPI**: Higher DPI = larger images = more memory

---

## Next Steps

Quyết định implementation approach:

**Option A - Full Implementation**: Implement tất cả features (3-4 hours)
**Option B - Incremental**: Implement từng phase (recommended)
**Option C - Minimal**: Chỉ DPI và Image return (1-2 hours)

Bạn muốn tôi implement option nào?
