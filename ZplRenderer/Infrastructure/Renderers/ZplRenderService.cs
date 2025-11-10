using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ZplRenderer.Core.Interfaces;
using ZplRenderer.Core.ZplParser;
using ZplRenderer.Infrastructure.Utils;

namespace ZplRenderer.Infrastructure.Renderers
{
    public class ZplRenderService : IZplRenderer
    {
        private readonly ZplParser parser;
        private readonly ZplImageRenderer imageRenderer;

        public ZplRenderService()
        {
            parser = new ZplParser();
            imageRenderer = new ZplImageRenderer();
        }

        public void ConvertZplToFile(string zplFilePath, string outputDirectory, string format)
        {
            if (!File.Exists(zplFilePath))
                throw new FileNotFoundException("ZPL file not found: " + zplFilePath);
            format = format.ToLower();
            if (format != "png" && format != "jpg" && format != "jpeg")
            {
                throw new ArgumentException("Only PNG and JPG formats are supported. PDF support has been removed to reduce dependencies.");
            }

            Directory.CreateDirectory(outputDirectory);

            using (var reader = new StreamReader(zplFilePath))
            {
                string line;
                var buffer = new List<string>();
                int fileIndex = 1;

                while ((line = reader.ReadLine()) != null)
                {
                    buffer.Add(line);

                    if (line.Trim().Equals("^XZ", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            ProcessZplChunk(buffer, outputDirectory, format, fileIndex);
                            fileIndex++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Error processing label " + fileIndex + ": " + ex.Message);
                        }

                        buffer.Clear();
                        MemoryOptimizer.ForceCollect();
                    }
                }
            }
        }

        private void ProcessZplChunk(List<string> zplLines, string outputDirectory,
            string format, int fileIndex)
        {
            string zplText = string.Join(Environment.NewLine, zplLines);

            try
            {
                // Parse ZPL
                var label = parser.Parse(zplText);

                // Render to bitmap
                using (var bitmap = imageRenderer.RenderLabel(label))
                {
                    string baseFileName = Path.Combine(outputDirectory,
                        string.Format("label_{0:D4}", fileIndex));

                    switch (format)
                    {
                        case "png":
                            bitmap.Save(baseFileName + ".png", ImageFormat.Png);
                            break;

                        case "jpg":
                        case "jpeg":
                            bitmap.Save(baseFileName + ".jpg", ImageFormat.Jpeg);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error rendering label " + fileIndex + ": " + ex.Message);
                throw;
            }
        }
    }
}
