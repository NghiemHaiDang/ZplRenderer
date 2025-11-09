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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZplRenderer.Core.Interfaces;
using ZplRenderer.Infrastructure.Utils;

namespace ZplRenderer.Infrastructure.Renderers
{
    public class ZplRenderService : IZplRenderer
    {
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

                if (line.Trim().Equals("^XZ", StringComparison.OrdinalIgnoreCase))
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
    }
}
