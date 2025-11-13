using System;
using ZplRenderer.Config;
using ZplRenderer.Core.Interfaces;
using ZplRenderer.Infrastructure.Renderers;

class QuickTest
{
    static void Main()
    {
        Console.WriteLine("Quick DPI Test");
        Console.WriteLine("==============");

        try
        {
            var renderer = new ZplRenderService();

            // Create simple ZPL
            System.IO.File.WriteAllText("quick.zpl", "^XA^FO50,50^ADN,36,20^FDTEST^FS^XZ");

            Console.WriteLine("Test 1: Default (203 DPI)...");
            renderer.ConvertZplToFile("quick.zpl", "out1", "png");
            Console.WriteLine("  OK");

            Console.WriteLine("Test 2: With options (300 DPI)...");
            var options = new ZplRenderOptions(300);
            renderer.ConvertZplToFile("quick.zpl", "out2", "png", options);
            Console.WriteLine("  OK");

            Console.WriteLine();
            Console.WriteLine("SUCCESS!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.Message);
            Console.WriteLine(ex.StackTrace);
        }

        Console.ReadLine();
    }
}
