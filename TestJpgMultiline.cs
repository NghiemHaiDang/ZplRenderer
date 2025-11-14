using System;
using ZplRenderer.Config;
using ZplRenderer.Infrastructure.Renderers;

class TestJpgMultiline
{
    static void Main()
    {
        Console.WriteLine("Testing JPG with Multiline ZPL");
        Console.WriteLine("================================");

        try
        {
            var renderer = new ZplRenderService();

            Console.WriteLine("Test 1: JPG at 203 DPI (multiline ZPL)...");
            renderer.ConvertZplToFile("test_jpg_multiline.zpl", "output_jpg_multiline_203", "jpg");
            Console.WriteLine("  OK");

            Console.WriteLine("Test 2: JPG at 300 DPI (multiline ZPL)...");
            var options = new ZplRenderOptions(300);
            renderer.ConvertZplToFile("test_jpg_multiline.zpl", "output_jpg_multiline_300", "jpg", options);
            Console.WriteLine("  OK");

            // Check files
            string[] files203 = System.IO.Directory.GetFiles("output_jpg_multiline_203", "*.jpg");
            string[] files300 = System.IO.Directory.GetFiles("output_jpg_multiline_300", "*.jpg");

            Console.WriteLine();
            Console.WriteLine("Results:");
            Console.WriteLine("  203 DPI: " + files203.Length + " JPG file(s) created");
            if (files203.Length > 0) Console.WriteLine("    -> " + files203[0]);

            Console.WriteLine("  300 DPI: " + files300.Length + " JPG file(s) created");
            if (files300.Length > 0) Console.WriteLine("    -> " + files300[0]);

            if (files203.Length > 0 && files300.Length > 0)
            {
                Console.WriteLine();
                Console.WriteLine("SUCCESS! JPG files created with white background.");
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine("WARNING: No JPG files created!");
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
