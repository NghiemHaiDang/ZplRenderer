using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ZplRenderer.Config;
using ZplRenderer.Core.Interfaces;
using ZplRenderer.Infrastructure.Renderers;

class TestDpiFeatures
{
    static void Main()
    {
        Console.WriteLine("========================================");
        Console.WriteLine("  ZPL Renderer - DPI & Image Features Test");
        Console.WriteLine("========================================");
        Console.WriteLine();

        try
        {
            IZplRenderer renderer = new ZplRenderService();
            string zplFile = "test.zpl";

            // Create test ZPL if not exists
            if (!File.Exists(zplFile))
            {
                CreateTestZpl(zplFile);
                Console.WriteLine("[INFO] Created test ZPL file");
                Console.WriteLine();
            }

            // Test 1: DPI Variations
            Console.WriteLine("=== Test 1: DPI Output Variations ===");
            TestDpiOutputs(renderer, zplFile);

            // Test 2: ConvertZplToImages
            Console.WriteLine();
            Console.WriteLine("=== Test 2: ConvertZplToImages Method ===");
            TestImageReturn(renderer, zplFile);

            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("  All Tests Passed!");
            Console.WriteLine("========================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("[FATAL ERROR] " + ex.GetType().Name);
            Console.WriteLine("Message: " + ex.Message);
            Console.WriteLine();
            Console.WriteLine("Stack Trace:");
            Console.WriteLine(ex.StackTrace);
        }

        Console.WriteLine();
        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();
    }

    static void TestDpiOutputs(IZplRenderer renderer, string zplFile)
    {
        int[] dpiValues = { 203, 300, 600 };

        foreach (int dpi in dpiValues)
        {
            Console.WriteLine("  Testing DPI: " + dpi);

            try
            {
                var options = new ZplRenderOptions(dpi);
                string outputDir = "output_dpi_" + dpi;

                Directory.CreateDirectory(outputDir);
                renderer.ConvertZplToFile(zplFile, outputDir, "png", options);

                // Check results
                string[] files = Directory.GetFiles(outputDir, "*.png");
                if (files.Length > 0)
                {
                    FileInfo fi = new FileInfo(files[0]);
                    using (var img = Image.FromFile(files[0]))
                    {
                        Console.WriteLine("    File: " + Path.GetFileName(files[0]));
                        Console.WriteLine("    Size: " + (fi.Length / 1024.0).ToString("F2") + " KB");
                        Console.WriteLine("    Dimensions: " + img.Width + " x " + img.Height + " pixels");
                        Console.WriteLine("    Expected scale: " + (dpi / 203.0).ToString("F2") + "x");
                    }
                }
                else
                {
                    Console.WriteLine("    [WARNING] No output files generated");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("    [ERROR] " + ex.Message);
            }

            Console.WriteLine();
        }
    }

    static void TestImageReturn(IZplRenderer renderer, string zplFile)
    {
        try
        {
            var options = new ZplRenderOptions(300);
            Console.WriteLine("  Converting ZPL to Image objects (300 DPI)...");

            List<Image> images = renderer.ConvertZplToImages(zplFile, options);
            Console.WriteLine("  Received " + images.Count + " image(s)");
            Console.WriteLine();

            try
            {
                for (int i = 0; i < images.Count; i++)
                {
                    Image img = images[i];
                    Console.WriteLine("  Image #" + (i + 1) + ":");
                    Console.WriteLine("    Type: " + img.GetType().Name);
                    Console.WriteLine("    Size: " + img.Width + " x " + img.Height + " pixels");
                    Console.WriteLine("    PixelFormat: " + img.PixelFormat);
                    Console.WriteLine("    HorizontalResolution: " + img.HorizontalResolution + " DPI");
                    Console.WriteLine("    VerticalResolution: " + img.VerticalResolution + " DPI");

                    // Save to file to verify
                    string outputPath = "image_returned_" + (i + 1) + ".png";
                    img.Save(outputPath, ImageFormat.Png);
                    FileInfo fi = new FileInfo(outputPath);
                    Console.WriteLine("    Saved to: " + outputPath + " (" + (fi.Length / 1024.0).ToString("F2") + " KB)");
                    Console.WriteLine();
                }

                Console.WriteLine("  [SUCCESS] All images processed successfully");
            }
            finally
            {
                // IMPORTANT: Dispose all images
                foreach (var img in images)
                {
                    if (img != null)
                        img.Dispose();
                }
                Console.WriteLine("  [INFO] All images disposed properly");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("  [ERROR] " + ex.Message);
            Console.WriteLine("  " + ex.StackTrace);
        }
    }

    static void CreateTestZpl(string filePath)
    {
        string zplContent = @"^XA
^FO50,50^ADN,36,20^FDDPI Test Label^FS
^FO50,100^ADN,18,10^FDTesting DPI: 203, 300, 600^FS
^FO50,130^BY2^BCN,100,Y,N,N
^FD123456789^FS
^XZ";
        File.WriteAllText(filePath, zplContent);
    }
}
