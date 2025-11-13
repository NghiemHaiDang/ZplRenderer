using System;
using ZplRenderer.Config;
using ZplRenderer.Infrastructure.Renderers;

class TestJpg
{
    static void Main()
    {
        Console.WriteLine("Testing JPG Output");
        Console.WriteLine("==================");

        try
        {
            var renderer = new ZplRenderService();

            // Create test ZPL
            System.IO.File.WriteAllText("test_jpg.zpl", "^XA^FO50,50^ADN,36,20^FDJPG TEST^FS^XZ");

            Console.WriteLine("Test 1: JPG at 203 DPI...");
            renderer.ConvertZplToFile("test_jpg.zpl", "output_jpg_203", "jpg");
            Console.WriteLine("  OK - Check output_jpg_203 folder");

            Console.WriteLine("Test 2: JPG at 300 DPI...");
            var options = new ZplRenderOptions(300);
            renderer.ConvertZplToFile("test_jpg.zpl", "output_jpg_300", "jpg", options);
            Console.WriteLine("  OK - Check output_jpg_300 folder");

            Console.WriteLine();
            Console.WriteLine("SUCCESS! Open the JPG files to verify they have white background.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.Message);
            Console.WriteLine(ex.StackTrace);
        }

        Console.ReadLine();
    }
}
