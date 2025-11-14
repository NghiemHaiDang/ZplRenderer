using System;
using System.IO;
using ZplRenderer.Infrastructure.Renderers;

class TestSingleLineNet8
{
    static async Task Main()
    {
        Console.WriteLine("Testing Single Line ZPL with .NET 8.0");
        Console.WriteLine("======================================");

        try
        {
            var renderer = new ZplRenderService();

            Console.WriteLine("ZPL: ^XA^FT100,100^A0N,67,0^FDTestLabel^FS^XZ");
            Console.WriteLine();

            Console.WriteLine("Test: Converting to PNG...");
            await renderer.ConvertZplToFileAsync("test_single_line.zpl", "output_single_line_net8", "png");

            // Check output
            string[] files = Directory.GetFiles("output_single_line_net8", "*.png");
            Console.WriteLine("Files created: " + files.Length);

            if (files.Length > 0)
            {
                Console.WriteLine("SUCCESS! File: " + files[0]);
                var fi = new FileInfo(files[0]);
                Console.WriteLine("Size: " + (fi.Length / 1024.0).ToString("F2") + " KB");
            }
            else
            {
                Console.WriteLine("ERROR: No files created!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.Message);
            Console.WriteLine(ex.StackTrace);
        }

        Console.ReadLine();
    }
}
