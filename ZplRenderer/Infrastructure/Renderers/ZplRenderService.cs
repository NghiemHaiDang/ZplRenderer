#if NET40
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using ZplRenderer.Config;
using ZplRenderer.Core.Interfaces;
#else
using BinaryKits.Zpl.Viewer;
using BinaryKits.Zpl.Viewer.Models;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZplRenderer.Config;
using ZplRenderer.Core.Interfaces;
using ZplRenderer.Infrastructure.Utils;
#endif

namespace ZplRenderer.Infrastructure.Renderers
{
    public class ZplRenderService : IZplRenderer
    {
#if NET40
        private static string consoleExePath;

        public void ConvertZplToFile(string zplFilePath, string outputDirectory, string format)
        {
            // Ensure console app is extracted
            EnsureConsoleAppExtracted();

            // Validate inputs
            if (!File.Exists(zplFilePath))
                throw new FileNotFoundException("ZPL file not found: " + zplFilePath);

            if (!IsValidFormat(format))
                throw new ArgumentException("Invalid format. Use: png, jpg, jpeg, or pdf");

            // Create output directory
            Directory.CreateDirectory(outputDirectory);

            // Execute console app
            var startInfo = new ProcessStartInfo
            {
                FileName = consoleExePath,
                Arguments = string.Format("\"{0}\" \"{1}\" \"{2}\"", zplFilePath, outputDirectory, format),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    throw new Exception(string.Format("ZPL conversion failed with exit code {0}. Error: {1}",
                        process.ExitCode, error));
                }
            }
        }

        private static void EnsureConsoleAppExtracted()
        {
            if (consoleExePath != null && File.Exists(consoleExePath))
                return;

            string tempDir = Path.Combine(Path.GetTempPath(), "ZplRenderer");
            Directory.CreateDirectory(tempDir);

            consoleExePath = Path.Combine(tempDir, "ZplRenderer.Console.exe");

            if (File.Exists(consoleExePath))
                return;

            // Extract embedded console app with optimized buffer
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "ZplRenderer.Console.exe";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new Exception("Embedded console app not found in DLL resources.");

                using (FileStream fileStream = new FileStream(consoleExePath,
                    FileMode.Create, FileAccess.Write, FileShare.None,
                    bufferSize: 81920)) // 80KB buffer for faster write
                {
                    byte[] buffer = new byte[81920];
                    int bytesRead;
                    while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }

        // New method with options
        public void ConvertZplToFile(string zplFilePath, string outputDirectory, string format, ZplRenderOptions options)
        {
            EnsureConsoleAppExtracted();

            if (!File.Exists(zplFilePath))
                throw new FileNotFoundException("ZPL file not found: " + zplFilePath);

            if (!IsValidFormat(format))
                throw new ArgumentException("Invalid format. Use: png, jpg, jpeg, or pdf");

            Directory.CreateDirectory(outputDirectory);

            // Build arguments with DPI and dimensions
            string arguments = string.Format("\"{0}\" \"{1}\" \"{2}\" {3}",
                zplFilePath, outputDirectory, format, options.Dpi);

            if (options.LabelWidth.HasValue && options.LabelHeight.HasValue)
            {
                arguments += string.Format(" {0} {1}", options.LabelWidth.Value, options.LabelHeight.Value);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = consoleExePath,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var process = Process.Start(startInfo))
            {
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    string debugInfo = string.Format("Command: {0} {1}\nOutput: {2}\nError: {3}",
                        consoleExePath, arguments, output, error);
                    throw new Exception(string.Format("ZPL conversion failed with exit code {0}. Debug: {1}",
                        process.ExitCode, debugInfo));
                }
            }
        }

        // New method to get Images directly
        public List<Image> ConvertZplToImages(string zplFilePath, ZplRenderOptions options)
        {
            EnsureConsoleAppExtracted();

            if (!File.Exists(zplFilePath))
                throw new FileNotFoundException("ZPL file not found: " + zplFilePath);

            var images = new List<Image>();

            // Create temp directory for output
            string tempOutputDir = Path.Combine(Path.GetTempPath(), "ZplRenderer_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempOutputDir);

            try
            {
                // Convert to PNG files first
                ConvertZplToFile(zplFilePath, tempOutputDir, "png", options);

                // Load PNG files as Images
                string[] pngFiles = Directory.GetFiles(tempOutputDir, "*.png");
                foreach (string pngFile in pngFiles)
                {
                    images.Add(Image.FromFile(pngFile));
                }

                return images;
            }
            finally
            {
                // Clean up temp files
                try
                {
                    if (Directory.Exists(tempOutputDir))
                    {
                        Directory.Delete(tempOutputDir, true);
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }

        private static bool IsValidFormat(string format)
        {
            return format.Equals("png", StringComparison.OrdinalIgnoreCase) ||
                   format.Equals("jpg", StringComparison.OrdinalIgnoreCase) ||
                   format.Equals("jpeg", StringComparison.OrdinalIgnoreCase) ||
                   format.Equals("pdf", StringComparison.OrdinalIgnoreCase);
        }
#else
        public async Task ConvertZplToFileAsync(string zplFilePath, string outputDirectory, string format)
        {
            if (!File.Exists(zplFilePath))
                throw new FileNotFoundException($"ZPL file not found: {zplFilePath}");

            Directory.CreateDirectory(outputDirectory);

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

                // Support both single-line and multiline ZPL formats
                if (line.IndexOf("^XZ", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    try
                    {
                        await ProcessZplChunkAsync(buffer, outputDirectory, format, fileIndex, document);
                        fileIndex++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"⚠️ Lỗi khi xử lý label {fileIndex}: {ex.Message}");
                    }

                    buffer.Clear();
                    MemoryOptimizer.ForceCollect();
                }
            }

            // Process remaining buffer if any (for files without proper line breaks)
            if (buffer.Count > 0)
            {
                try
                {
                    await ProcessZplChunkAsync(buffer, outputDirectory, format, fileIndex, document);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Lỗi khi xử lý label cuối cùng: {ex.Message}");
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

        private async Task ProcessZplChunkAsync(List<string> zplLines, string outputDirectory, string format, int fileIndex, Document? document)
        {
            string zplText = string.Join(Environment.NewLine, zplLines);

            await Task.Run(() =>
            {
                try
                {
                    // Use BinaryKits.Zpl.Viewer to render ZPL to image
                    IPrinterStorage printerStorage = new PrinterStorage();
                    var analyzer = new ZplAnalyzer(printerStorage);
                    var drawer = new ZplElementDrawer(printerStorage);

                    var analyzeInfo = analyzer.Analyze(zplText);

                    // Process each label in the ZPL
                    foreach (var labelInfo in analyzeInfo.LabelInfos)
                    {
                        byte[] imageBytes = drawer.Draw(labelInfo.ZplElements);

                        if (imageBytes == null || imageBytes.Length == 0)
                        {
                            Console.WriteLine($"⚠️ Không thể render label {fileIndex}");
                            continue;
                        }

                        string baseFileName = Path.Combine(outputDirectory, $"label_{fileIndex:D4}");

                        switch (format.ToLower())
                        {
                            case "png":
                                File.WriteAllBytes($"{baseFileName}.png", imageBytes);
                                break;
                            case "jpg":
                            case "jpeg":
                                using (var image = SixLabors.ImageSharp.Image.Load(imageBytes))
                                {
                                    image.Save($"{baseFileName}.jpg", new JpegEncoder());
                                }
                                break;
                            case "pdf":
                                if (document != null)
                                {
                                    var imageData = ImageDataFactory.Create(imageBytes);
                                    var pdfImage = new iText.Layout.Element.Image(imageData);

                                    // Scale image to fit page
                                    pdfImage.SetAutoScale(true);

                                    // Add image to document (new page for each label)
                                    document.Add(pdfImage);
                                    document.Add(new AreaBreak());
                                }
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ Lỗi khi render label {fileIndex}: {ex.Message}");
                }
            });
        }

        // New method with options (async version)
        public async Task ConvertZplToFileAsync(string zplFilePath, string outputDirectory, string format, ZplRenderOptions options)
        {
            // For .NET 8.0, we don't use console app, so just call the original method
            // In future, this could directly use BinaryKits with DPI support
            await ConvertZplToFileAsync(zplFilePath, outputDirectory, format);
        }

        // New method to get Images directly (async version)
        public async Task<List<System.Drawing.Image>> ConvertZplToImagesAsync(string zplFilePath, ZplRenderOptions options)
        {
            var images = new List<System.Drawing.Image>();

            // Create temp directory for output
            string tempOutputDir = Path.Combine(Path.GetTempPath(), "ZplRenderer_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempOutputDir);

            try
            {
                // Convert to PNG files first
                await ConvertZplToFileAsync(zplFilePath, tempOutputDir, "png", options);

                // Load PNG files as Images
                string[] pngFiles = Directory.GetFiles(tempOutputDir, "*.png");
                foreach (string pngFile in pngFiles)
                {
                    using (var stream = new FileStream(pngFile, FileMode.Open, FileAccess.Read))
                    {
                        images.Add(System.Drawing.Image.FromStream(stream));
                    }
                }

                return images;
            }
            finally
            {
                // Clean up temp files
                try
                {
                    if (Directory.Exists(tempOutputDir))
                    {
                        await Task.Run(() => Directory.Delete(tempOutputDir, true));
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
#endif
    }
}
