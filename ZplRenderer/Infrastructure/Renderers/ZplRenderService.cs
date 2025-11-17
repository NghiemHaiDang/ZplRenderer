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
using SixLabors.ImageSharp.Processing;
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

        private static void EnsureConsoleAppExtracted()
        {
            if (consoleExePath != null && File.Exists(consoleExePath))
                return;

            string tempDir = Path.Combine(Path.GetTempPath(), "ZplRenderer");
            Directory.CreateDirectory(tempDir);

            // Detect platform (32-bit or 64-bit)
            bool is64Bit = IntPtr.Size == 8;
            string platform = is64Bit ? "x64" : "x86";

            consoleExePath = Path.Combine(tempDir, "ZplRenderer.Console." + platform + ".exe");

            if (File.Exists(consoleExePath))
                return;

            // Extract embedded console app with optimized buffer
            Assembly assembly = Assembly.GetExecutingAssembly();
            string resourceName = "ZplRenderer.Console." + platform + ".exe";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new Exception(string.Format("Embedded console app not found in DLL resources. Looking for: {0}. Platform: {1}-bit",
                        resourceName, is64Bit ? "64" : "32"));

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

        /// <summary>
        /// ConvertZplToFile
        /// </summary>
        /// <param name="zplFilePath">Path to ZPL file (required)</param>
        /// <param name="outputDirectory">Output Folder (null = Desktop/ZplRenderer_Output)</param>
        /// <param name="format">Format file (null = "png")</param>
        /// <param name="options">Options (null = default: DPI 203, no size override)</param>
        public void ConvertZplToFile(string zplFilePath, string outputDirectory = null, string format = null, ZplRenderOptions options = null)
        {
            EnsureConsoleAppExtracted();

            if (!File.Exists(zplFilePath))
                throw new FileNotFoundException("ZPL file not found: " + zplFilePath);

            if (string.IsNullOrEmpty(format))
            {
                format = "png";
            }

            if (!IsValidFormat(format))
                throw new ArgumentException("Invalid format. Use: png, jpg, jpeg, or pdf");

            if (string.IsNullOrEmpty(outputDirectory))
            {
                outputDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ZplRenderer_Output");
            }

            Directory.CreateDirectory(outputDirectory);

            if (options == null)
            {
                options = new ZplRenderOptions();
            }

            // Build arguments
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

        /// <summary>
        /// ConvertZplToImages
        /// </summary>
        /// <param name="zplFilePath">Path to ZPL file (required)</param>
        /// <param name="options">Options (null = default: DPI 203, no size override)</param>
        /// <param name="format">Format (null = "jpg", có thể "png")</param>
        public List<Image> ConvertZplToImages(string zplFilePath, ZplRenderOptions options = null, string format = null)
        {
            EnsureConsoleAppExtracted();

            if (!File.Exists(zplFilePath))
                throw new FileNotFoundException("ZPL file not found: " + zplFilePath);

            // Default format = "jpg" nếu null
            if (string.IsNullOrEmpty(format))
            {
                format = "jpg";
            }

            if (!format.Equals("png", StringComparison.OrdinalIgnoreCase) &&
                !format.Equals("jpg", StringComparison.OrdinalIgnoreCase) &&
                !format.Equals("jpeg", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Format must be 'png' or 'jpg' for ConvertZplToImagesAsync. PDF is not supported.");
            }

            if (options == null)
            {
                options = new ZplRenderOptions();
            }

            var images = new List<Image>();
            string tempOutputDir = Path.Combine(Path.GetTempPath(), "ZplRenderer_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempOutputDir);

            try
            {
                ConvertZplToFile(zplFilePath, tempOutputDir, format, options);
                string searchPattern = format.Equals("png", StringComparison.OrdinalIgnoreCase) ? "*.png" : "*.jpg";
                string[] imageFiles = Directory.GetFiles(tempOutputDir, searchPattern);
                foreach (string imageFile in imageFiles)
                {
                    using (var stream = new FileStream(imageFile, FileMode.Open, FileAccess.Read))
                    {
                        images.Add(Image.FromStream(stream));
                    }
                }

                return images;
            }
            finally
            {
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
        /// <summary>
        /// ConvertZplToFileAsync
        /// </summary>
        /// <param name="zplFilePath">Path to ZPL file (required)</param>
        /// <param name="outputDirectory">Output Folder(null = Desktop/ZplRenderer_Output)</param>
        /// <param name="format">Format file (null = "png")</param>
        /// <param name="options">Options (null = default: DPI 203, no size override)</param>
        public async Task ConvertZplToFileAsync(string zplFilePath, string? outputDirectory = null, string? format = null, ZplRenderOptions? options = null)
        {
            if (!File.Exists(zplFilePath))
                throw new FileNotFoundException($"ZPL file not found: {zplFilePath}");

            if (string.IsNullOrEmpty(format))
            {
                format = "png";
            }

            if (string.IsNullOrEmpty(outputDirectory))
            {
                outputDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ZplRenderer_Output");
            }

            Directory.CreateDirectory(outputDirectory);

            if (options == null)
            {
                options = new ZplRenderOptions();
            }

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
                        await ProcessZplChunkAsync(buffer, outputDirectory, format, fileIndex, document, options);
                        fileIndex++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing label {fileIndex}: {ex.Message}");
                    }

                    buffer.Clear();
                    MemoryOptimizer.ForceCollect();
                }
            }

            if (buffer.Count > 0)
            {
                try
                {
                    await ProcessZplChunkAsync(buffer, outputDirectory, format, fileIndex, document, options);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing last label: {ex.Message}");
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

        private async Task ProcessZplChunkAsync(List<string> zplLines, string outputDirectory, string format, int fileIndex, Document? document, ZplRenderOptions options)
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

                    // Apply DPI scaling: BinaryKits renders at 203 DPI by default
                    double dpiScale = options.Dpi / 203.0;

                    var analyzeInfo = analyzer.Analyze(zplText);

                    foreach (var labelInfo in analyzeInfo.LabelInfos)
                    {
                        byte[] imageBytes = drawer.Draw(labelInfo.ZplElements);

                        if (imageBytes == null || imageBytes.Length == 0)
                        {
                            Console.WriteLine($"Unable to render label {fileIndex}");
                            continue;
                        }
                        if (Math.Abs(dpiScale - 1.0) > 0.001)
                        {
                            using (var originalImage = SixLabors.ImageSharp.Image.Load(imageBytes))
                            {
                                int newWidth = (int)(originalImage.Width * dpiScale);
                                int newHeight = (int)(originalImage.Height * dpiScale);

                                using (var scaledImage = originalImage.Clone(ctx => ctx.Resize(newWidth, newHeight)))
                                {
                                    using (var ms = new MemoryStream())
                                    {
                                        scaledImage.Save(ms, new PngEncoder());
                                        imageBytes = ms.ToArray();
                                    }
                                }
                            }
                        }

                        string baseFileName = Path.Combine(outputDirectory, $"label_{fileIndex:D4}");

                        switch (format.ToLower())
                        {
                            case "png":
                                File.WriteAllBytes($"{baseFileName}.png", imageBytes);
                                break;
                            case "jpg":
                            case "jpeg":
                                using (var sourceImage = SixLabors.ImageSharp.Image.Load(imageBytes))
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
                    Console.WriteLine($"Unable to render label {fileIndex}: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// ConvertZplToImagesAsync
        /// </summary>
        /// <param name="zplFilePath">Path to ZPL file (required)</param>
        /// <param name="options">Options (null = default: DPI 203, no size override)</param>
        /// <param name="format">Format (null = "jpg", có thể "png")</param>
        public async Task<List<System.Drawing.Image>> ConvertZplToImagesAsync(string zplFilePath, ZplRenderOptions? options = null, string? format = null)
        {
            var images = new List<System.Drawing.Image>();

            string tempOutputDir = Path.Combine(Path.GetTempPath(), "ZplRenderer_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempOutputDir);
            if (string.IsNullOrEmpty(format))
            {
                format = "jpg";
            }

            if (!format.Equals("png", StringComparison.OrdinalIgnoreCase) &&
                !format.Equals("jpg", StringComparison.OrdinalIgnoreCase) &&
                !format.Equals("jpeg", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Format must be 'png' or 'jpg' for ConvertZplToImagesAsync. PDF is not supported.");
            }

            if (options == null)
            {
                options = new ZplRenderOptions();
            }

            try
            {
                await ConvertZplToFileAsync(zplFilePath, tempOutputDir, format, options);
                string searchPattern = format.Equals("png", StringComparison.OrdinalIgnoreCase) ? "*.png" : "*.jpg";
                string[] imageFiles = Directory.GetFiles(tempOutputDir, searchPattern);
                foreach (string imageFile in imageFiles)
                {
                    using (var stream = new FileStream(imageFile, FileMode.Open, FileAccess.Read))
                    {
                        images.Add(System.Drawing.Image.FromStream(stream));
                    }
                }

                return images;
            }
            finally
            {
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
