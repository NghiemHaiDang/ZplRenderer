using BinaryKits.Zpl.Viewer;
using BinaryKits.Zpl.Viewer.Models;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Processing;
using Microsoft.Extensions.Logging;

namespace ZplRenderer.Console
{
    class Program
    {
        private static ILogger? _logger;

        static async Task<int> Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
            _logger = loggerFactory.CreateLogger<Program>();

            try
            {
                if (args.Length < 3)
                {
                    _logger.LogError("Invalid arguments. Usage: ZplRenderer.Console.exe <zplFilePath> <outputDirectory> <format> [dpi] [width] [height]");
                    _logger.LogError("Format: png, jpg, jpeg, or pdf");
                    _logger.LogError("DPI (optional): 203, 300, or 600 (default: 203)");
                    _logger.LogError("Width/Height (optional): label dimensions in dots");
                    return 1;
                }

                string zplFilePath = args[0];
                string outputDirectory = args[1];
                string format = args[2];

                // Parse optional DPI parameter (default 203)
                int dpi = 203;
                if (args.Length >= 4 && int.TryParse(args[3], out int customDpi))
                {
                    if (customDpi == 203 || customDpi == 300 || customDpi == 600)
                    {
                        dpi = customDpi;
                    }
                    else
                    {
                        _logger.LogWarning("Invalid DPI {Dpi}. Using default 203. Valid values: 203, 300, 600", customDpi);
                    }
                }

                // Parse optional width/height parameters
                int? labelWidth = null;
                int? labelHeight = null;
                if (args.Length >= 6)
                {
                    if (int.TryParse(args[4], out int width))
                        labelWidth = width;
                    if (int.TryParse(args[5], out int height))
                        labelHeight = height;
                }

                _logger.LogInformation("Starting ZPL conversion - File: {ZplFile}, Output: {OutputDir}, Format: {Format}, DPI: {Dpi}",
                    zplFilePath, outputDirectory, format, dpi);

                if (labelWidth.HasValue && labelHeight.HasValue)
                {
                    _logger.LogInformation("Label size override - Width: {Width}, Height: {Height}", labelWidth, labelHeight);
                }

                if (!File.Exists(zplFilePath))
                {
                    _logger.LogError("ZPL file not found: {ZplFile}", zplFilePath);
                    return 2;
                }

                if (!IsValidFormat(format))
                {
                    _logger.LogError("Invalid format '{Format}'. Supported formats: png, jpg, jpeg, pdf", format);
                    return 3;
                }

                // Output directory
                Directory.CreateDirectory(outputDirectory);
                _logger.LogDebug("Output directory created/verified: {OutputDir}", outputDirectory);

                // Process ZPL file
                await ConvertZplToFileAsync(zplFilePath, outputDirectory, format, dpi, labelWidth, labelHeight);

                _logger.LogInformation("Conversion completed successfully");
                return 0;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Fatal error during conversion");
                return 99;
            }
        }

        static bool IsValidFormat(string format)
        {
            return format.Equals("png", StringComparison.OrdinalIgnoreCase) ||
                   format.Equals("jpg", StringComparison.OrdinalIgnoreCase) ||
                   format.Equals("jpeg", StringComparison.OrdinalIgnoreCase) ||
                   format.Equals("pdf", StringComparison.OrdinalIgnoreCase);
        }

        static async Task ConvertZplToFileAsync(string zplFilePath, string outputDirectory, string format,
            int dpi = 203, int? labelWidth = null, int? labelHeight = null)
        {
            using var reader = new StreamReader(zplFilePath);
            string? line;
            var buffer = new List<string>();
            int fileIndex = 1;

            PdfWriter? pdfWriter = null;
            PdfDocument? pdfDoc = null;
            Document? document = null;

            if (format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
            {
                string outputFile = Path.Combine(outputDirectory, $"{Path.GetFileNameWithoutExtension(zplFilePath)}.pdf");
                pdfWriter = new PdfWriter(outputFile);
                pdfDoc = new PdfDocument(pdfWriter);
                document = new Document(pdfDoc);
            }

            while ((line = await reader.ReadLineAsync()) != null)
            {
                buffer.Add(line);

                if (line.IndexOf("^XZ", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    try
                    {
                        await ProcessZplChunkAsync(buffer, outputDirectory, format, fileIndex, document, dpi, labelWidth, labelHeight);
                        _logger?.LogDebug("Successfully processed label {LabelIndex}", fileIndex);
                        fileIndex++;
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogWarning(ex, "Error processing label {LabelIndex}", fileIndex);
                    }

                    buffer.Clear();
                    GC.Collect();
                }
            }

            if (buffer.Count > 0)
            {
                try
                {
                    await ProcessZplChunkAsync(buffer, outputDirectory, format, fileIndex, document, dpi, labelWidth, labelHeight);
                    _logger?.LogDebug("Successfully processed label {LabelIndex}", fileIndex);
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Error processing final label");
                }
            }

            if (document != null)
            {
                document.Close();
            }

            if (pdfDoc != null)
            {
                pdfDoc.Close();
            }

            if (pdfWriter != null)
            {
                pdfWriter.Close();
            }
        }

        static async Task ProcessZplChunkAsync(List<string> zplLines, string outputDirectory, string format, int fileIndex, Document? document,
            int dpi = 203, int? labelWidth = null, int? labelHeight = null)
        {
            string zplText = string.Join(Environment.NewLine, zplLines);

            await Task.Run(() =>
            {
                try
                {
                    // Priority logic for label size:
                    // 1. ZPL has ^PW/^LL → Use from ZPL (highest priority)
                    // 2. User provides labelWidth/labelHeight → Convert to 203 DPI and inject into ZPL
                    // 3. No size from ZPL and user → Use system default (812x1218 at 203 DPI)
                    bool hasWidth = zplText.Contains("^PW", StringComparison.OrdinalIgnoreCase);
                    bool hasHeight = zplText.Contains("^LL", StringComparison.OrdinalIgnoreCase);

                    // If ZPL doesn't have size and user provides override, inject into ZPL
                    if (!hasWidth && labelWidth.HasValue)
                    {
                        int widthAt203Dpi = (int)(labelWidth.Value / (double)dpi * 203.0);

                        // Insert ^PW after ^XA
                        zplText = System.Text.RegularExpressions.Regex.Replace(
                            zplText,
                            @"\^XA",
                            $"^XA{Environment.NewLine}^PW{widthAt203Dpi}",
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase
                        );
                        _logger?.LogDebug("Injected ^PW{Width} into ZPL (user {UserWidth} @ {Dpi} DPI → {Width} @ 203 DPI)",
                            widthAt203Dpi, labelWidth.Value, dpi, widthAt203Dpi);
                    }

                    if (!hasHeight && labelHeight.HasValue)
                    {
                        // User provides size at target DPI, convert to 203 DPI for BinaryKits
                        int heightAt203Dpi = (int)(labelHeight.Value / (double)dpi * 203.0);

                        // Insert ^LL after ^XA (or after ^PW if we just added it)
                        string pattern = hasWidth || labelWidth.HasValue ? @"\^PW\d+" : @"\^XA";
                        zplText = System.Text.RegularExpressions.Regex.Replace(
                            zplText,
                            pattern,
                            $"$0{Environment.NewLine}^LL{heightAt203Dpi}",
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase
                        );
                        _logger?.LogDebug("Injected ^LL{Height} into ZPL (user {UserHeight} @ {Dpi} DPI → {Height} @ 203 DPI)",
                            heightAt203Dpi, labelHeight.Value, dpi, heightAt203Dpi);
                    }

                    // If no size in ZPL and user didn't provide, use system defaults (4x6 inches at 203 DPI)
                    if (!hasWidth && !labelWidth.HasValue)
                    {
                        // Always inject 812 (4 inches @ 203 DPI), will be scaled later
                        const int defaultWidth = 812; // AppConstants.DefaultLabelWidth
                        zplText = System.Text.RegularExpressions.Regex.Replace(
                            zplText,
                            @"\^XA",
                            $"^XA{Environment.NewLine}^PW{defaultWidth}",
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase
                        );
                        _logger?.LogDebug("Using system default width: {Width} dots (4 inches @ 203 DPI, will scale to {Dpi} DPI)",
                            defaultWidth, dpi);
                    }

                    if (!hasHeight && !labelHeight.HasValue)
                    {
                        // Always inject 1218 (6 inches @ 203 DPI), will be scaled later
                        const int defaultHeight = 1218; // AppConstants.DefaultLabelHeight
                        string pattern = hasWidth || labelWidth.HasValue || true ? @"\^PW\d+" : @"\^XA";
                        zplText = System.Text.RegularExpressions.Regex.Replace(
                            zplText,
                            pattern,
                            $"$0{Environment.NewLine}^LL{defaultHeight}",
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase
                        );
                        _logger?.LogDebug("Using system default height: {Height} dots (6 inches @ 203 DPI, will scale to {Dpi} DPI)",
                            defaultHeight, dpi);
                    }

                    IPrinterStorage printerStorage = new PrinterStorage();

                    var analyzer = new ZplAnalyzer(printerStorage);
                    var drawer = new ZplElementDrawer(printerStorage);

                    // Apply DPI scaling: BinaryKits renders at 203 DPI by default
                    double dpiScale = dpi / 203.0;
                    _logger?.LogDebug("DPI: {Dpi}, Scale factor: {Scale}", dpi, dpiScale);

                    var analyzeInfo = analyzer.Analyze(zplText);

                    // Process each label in the ZPL
                    foreach (var labelInfo in analyzeInfo.LabelInfos)
                    {
                        byte[] imageBytes = drawer.Draw(labelInfo.ZplElements);

                        if (imageBytes == null || imageBytes.Length == 0)
                        {
                            _logger?.LogWarning("Unable to render label {LabelIndex}", fileIndex);
                            continue;
                        }

                        string baseFileName = Path.Combine(outputDirectory, $"label_{fileIndex:D4}");

                        // Apply DPI scaling if needed
                        if (Math.Abs(dpiScale - 1.0) > 0.001)
                        {
                            using (var originalImage = SixLabors.ImageSharp.Image.Load(imageBytes))
                            {
                                int newWidth = (int)(originalImage.Width * dpiScale);
                                int newHeight = (int)(originalImage.Height * dpiScale);

                                using (var scaledImage = originalImage.Clone(ctx => ctx.Resize(newWidth, newHeight)))
                                {
                                    // Convert scaled image back to bytes for further processing
                                    using (var ms = new MemoryStream())
                                    {
                                        scaledImage.Save(ms, new PngEncoder());
                                        imageBytes = ms.ToArray();
                                    }
                                }
                            }
                            _logger?.LogDebug("Image scaled from 203 DPI to {Dpi} DPI", dpi);
                        }

                        switch (format.ToLower())
                        {
                            case "png":
                                File.WriteAllBytes($"{baseFileName}.png", imageBytes);
                                break;
                            case "jpg":
                            case "jpeg":
                                using 
                                (var sourceImage = SixLabors.ImageSharp.Image.Load(imageBytes))
                                {
                                    using (var rgbImage = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgb24>(sourceImage.Width, sourceImage.Height, SixLabors.ImageSharp.Color.White))
                                    {
                                        rgbImage.Mutate(ctx => ctx.DrawImage(sourceImage, 1.0f));
                                        rgbImage.Save($"{baseFileName}.jpg", new JpegEncoder { Quality = 95 });
                                    }
                                }
                                break;
                            case "pdf":
                                if (document != null)
                                {
                                    var imageData = ImageDataFactory.Create(imageBytes);
                                    var pdfImage = new iText.Layout.Element.Image(imageData);
                                    pdfImage.SetAutoScale(true);
                                    document.Add(pdfImage);
                                    document.Add(new AreaBreak());

                                }
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Error rendering label {LabelIndex}", fileIndex);
                }
            });
        }
    }
}
