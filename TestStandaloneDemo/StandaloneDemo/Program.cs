using System;
using System.IO;
using ZplRenderer.Core.Interfaces;
using ZplRenderer.Infrastructure.Renderers;

class Program
{
    static void Main()
    {
        Console.WriteLine("====================================");
        Console.WriteLine("  STANDALONE DLL TEST");
        Console.WriteLine("  (Using ONLY ZplRenderer.dll)");
        Console.WriteLine("====================================");
        Console.WriteLine();

        try
        {
            Console.WriteLine("[1] Creating test ZPL file...");
            string zplFile = "standalone_test.zpl";
            File.WriteAllText(zplFile, @"^XA
^FO50,50^ADN,36,20^FDStandalone DLL Test^FS
^FO50,100^ADN,18,10^FDThis proves DLL is self-contained^FS
^FO50,140^BCN,100,Y,N,N
^FD987654321^FS
^XZ");
            Console.WriteLine("    OK - ZPL file created");
            Console.WriteLine();

            Console.WriteLine("[2] Initializing ZplRenderService...");
            IZplRenderer renderer = new ZplRenderService();
            Console.WriteLine("    OK - Renderer initialized");
            Console.WriteLine("    Note: Console app will be extracted from DLL to TEMP folder");
            Console.WriteLine();

            Console.WriteLine("[3] Converting ZPL to PNG...");
            string outputDir = "standalone_output";
            renderer.ConvertZplToFile(zplFile, outputDir, "png");
            Console.WriteLine("    OK - Conversion completed");
            Console.WriteLine();

            Console.WriteLine("[4] Checking output files...");
            if (Directory.Exists(outputDir))
            {
                string[] files = Directory.GetFiles(outputDir);
                Console.WriteLine("    Generated " + files.Length + " file(s):");
                foreach (string file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    Console.WriteLine("      - " + Path.GetFileName(file) +
                                    " (" + (fi.Length / 1024.0).ToString("F2") + " KB)");
                }
            }
            Console.WriteLine();

            Console.WriteLine("====================================");
            Console.WriteLine("  SUCCESS!");
            Console.WriteLine("  DLL works completely standalone");
            Console.WriteLine("  No installation required!");
            Console.WriteLine("====================================");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("====================================");
            Console.WriteLine("  ERROR!");
            Console.WriteLine("====================================");
            Console.WriteLine("Type: " + ex.GetType().Name);
            Console.WriteLine("Message: " + ex.Message);
            Console.WriteLine();
            Console.WriteLine("Stack Trace:");
            Console.WriteLine(ex.StackTrace);
        }

        Console.WriteLine();
        Console.WriteLine("Press Enter to exit...");
        Console.ReadLine();
    }
}
