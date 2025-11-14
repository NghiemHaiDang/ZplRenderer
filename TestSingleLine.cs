using System;
using ZplRenderer.Config;
using ZplRenderer.Infrastructure.Renderers;

class TestSingleLine
{
    static void Main()
    {
        Console.WriteLine("Testing Single Line ZPL");
        Console.WriteLine("=======================");

        try
        {
            var renderer = new ZplRenderService();

            Console.WriteLine("ZPL: ^XA^FT100,100^A0N,67,0^FDTestLabel^FS^XZ");
            Console.WriteLine();

            Console.WriteLine("Test: Converting to PNG...");
            renderer.ConvertZplToFile("test_single_line.zpl", "output_single_line", "png");

            // Check output
            string[] files = System.IO.Directory.GetFiles("output_single_line", "*.png");
            Console.WriteLine("Files created: " + files.Length);

            if (files.Length > 0)
            {
                Console.WriteLine("SUCCESS! File: " + files[0]);
                var fi = new System.IO.FileInfo(files[0]);
                Console.WriteLine("Size: " + (fi.Length / 1024.0).ToString("F2") + " KB");
            }
            else
            {
                Console.WriteLine("ERROR: No files created!");
                Console.WriteLine();
                Console.WriteLine("This is because console app does not handle single-line ZPL yet.");
                Console.WriteLine("The fix was implemented but console app needs to be republished.");
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
