using System;
using System.IO;
using ZplRenderer.Infrastructure.Renderers;

class TestSingleLineFinal
{
    static void Main()
    {
        Console.WriteLine("=== TESTING SINGLE-LINE ZPL ===");
        Console.WriteLine("ZPL: ^XA^FT100,100^A0N,67,0^FDTestLabel^FS^XZ");
        Console.WriteLine();

        try
        {
            var renderer = new ZplRenderService();

            // Create single-line ZPL file
            string zplContent = "^XA^FT100,100^A0N,67,0^FDTestLabel^FS^XZ";
            File.WriteAllText("test_single_final.zpl", zplContent);

            Console.WriteLine("Converting to PNG...");
            renderer.ConvertZplToFile("test_single_final.zpl", "output_single_final", "png");

            // Check output
            string[] files = Directory.GetFiles("output_single_final", "*.png");
            Console.WriteLine("Files created: " + files.Length);

            if (files.Length > 0)
            {
                Console.WriteLine("✅ SUCCESS! Single-line ZPL works!");
                Console.WriteLine("File: " + files[0]);
                var fi = new FileInfo(files[0]);
                Console.WriteLine("Size: " + (fi.Length / 1024.0).ToString("F2") + " KB");
            }
            else
            {
                Console.WriteLine("❌ FAILED: No files created");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ ERROR: " + ex.Message);
            Console.WriteLine(ex.StackTrace);
        }
    }
}
