using BinaryKits.Zpl.Viewer;
using BinaryKits.Zpl.Viewer.Models;
using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace ZplRenderer.Console
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                if (args.Length != 3)
                {
                    System.Console.WriteLine("Usage: ZplRenderer.Console.exe <zplFilePath> <outputDirectory> <format>");
                    System.Console.WriteLine("Format: png, jpg, jpeg, or pdf");
                    return 1;
                }

                string zplFilePath = args[0];
                string outputDirectory = args[1];
                string format = args[2];

                // Validate input
                if (!File.Exists(zplFilePath))
                {
                    System.Console.WriteLine($"Error: ZPL file not found: {zplFilePath}");
                    return 2;
                }

                if (!IsValidFormat(format))
                {
                    System.Console.WriteLine($"Error: Invalid format '{format}'. Use: png, jpg, jpeg, or pdf");
                    return 3;
                }

                // Create output directory
                Directory.CreateDirectory(outputDirectory);

                // Process ZPL file
                await ConvertZplToFileAsync(zplFilePath, outputDirectory, format);

                System.Console.WriteLine("✓ Conversion completed successfully");
                return 0;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"Error: {ex.Message}");
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

        static async Task ConvertZplToFileAsync(string zplFilePath, string outputDirectory, string format)
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

                if (line.Trim().Equals("^XZ", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        await ProcessZplChunkAsync(buffer, outputDirectory, format, fileIndex, document);
                        fileIndex++;
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine($"⚠ Warning: Error processing label {fileIndex}: {ex.Message}");
                    }

                    buffer.Clear();
                    GC.Collect();
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

        static async Task ProcessZplChunkAsync(List<string> zplLines, string outputDirectory, string format, int fileIndex, Document? document)
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
                            System.Console.WriteLine($"⚠ Warning: Unable to render label {fileIndex}");
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
                    System.Console.WriteLine($"⚠ Warning: Error rendering label {fileIndex}: {ex.Message}");
                }
            });
        }
    }
}
