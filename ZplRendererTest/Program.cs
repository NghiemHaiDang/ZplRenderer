using System;
using System.Diagnostics;
using System.IO;
using ZplRenderer.Core.Interfaces;
using ZplRenderer.Infrastructure.Renderers;

namespace ZplRendererTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== ZPL Renderer Test Console ===");
            Console.WriteLine();

            try
            {
                // Initialize the renderer
                IZplRenderer renderer = new ZplRenderService();
                Console.WriteLine("[INFO] ZplRenderService initialized successfully");

                // Setup test paths
                string currentDir = Directory.GetCurrentDirectory();
                string testZplFile = Path.Combine(currentDir, "test.zpl");
                string outputDir = Path.Combine(currentDir, "output");

                Console.WriteLine("[INFO] Current Directory: " + currentDir);
                Console.WriteLine("[INFO] Test ZPL File: " + testZplFile);
                Console.WriteLine("[INFO] Output Directory: " + outputDir);
                Console.WriteLine();

                // Create a sample ZPL file for testing if it doesn't exist
                if (!File.Exists(testZplFile))
                {
                    Console.WriteLine("[WARN] Test ZPL file not found. Creating sample ZPL file...");
                    CreateSampleZplFile(testZplFile);
                    Console.WriteLine("[INFO] Sample ZPL file created: " + testZplFile);
                    Console.WriteLine();
                }

                // Test 1: Convert to PNG
                Console.WriteLine("=== Test 1: Converting ZPL to PNG ===");
                TestConversion(renderer, testZplFile, Path.Combine(outputDir, "png"), "png");

                // Test 2: Convert to JPG
                Console.WriteLine();
                Console.WriteLine("=== Test 2: Converting ZPL to JPG ===");
                TestConversion(renderer, testZplFile, Path.Combine(outputDir, "jpg"), "jpg");

                // Test 3: Convert to PDF
                Console.WriteLine();
                Console.WriteLine("=== Test 3: Converting ZPL to PDF ===");
                TestConversion(renderer, testZplFile, Path.Combine(outputDir, "pdf"), "pdf");

                // Test 4: Error handling - non-existent file
                Console.WriteLine();
                Console.WriteLine("=== Test 4: Error Handling (Non-existent File) ===");
                try
                {
                    renderer.ConvertZplToFile("nonexistent.zpl", outputDir, "png");
                    Console.WriteLine("[ERROR] Should have thrown FileNotFoundException!");
                }
                catch (FileNotFoundException ex)
                {
                    Console.WriteLine("[OK] Correctly caught FileNotFoundException: " + ex.Message);
                }

                // Test 5: Error handling - invalid format
                Console.WriteLine();
                Console.WriteLine("=== Test 5: Error Handling (Invalid Format) ===");
                try
                {
                    renderer.ConvertZplToFile(testZplFile, outputDir, "bmp");
                    Console.WriteLine("[ERROR] Should have thrown ArgumentException!");
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine("[OK] Correctly caught ArgumentException: " + ex.Message);
                }

                Console.WriteLine();
                Console.WriteLine("=== All Tests Completed ===");
                Console.WriteLine("Press any key to open output folder...");
                Console.ReadKey();

                // Open output folder
                if (Directory.Exists(outputDir))
                {
                    Process.Start("explorer.exe", outputDir);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("[FATAL ERROR] " + ex.GetType().Name + ": " + ex.Message);
                Console.WriteLine("Stack Trace:");
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void TestConversion(IZplRenderer renderer, string zplFile, string outputDir, string format)
        {
            try
            {
                Console.WriteLine("[START] Converting to " + format.ToUpper() + "...");

                Stopwatch sw = Stopwatch.StartNew();

                // Set breakpoint here to debug the conversion process
                renderer.ConvertZplToFile(zplFile, outputDir, format);

                sw.Stop();

                Console.WriteLine("[SUCCESS] Conversion completed in " + sw.ElapsedMilliseconds + "ms");
                Console.WriteLine("[INFO] Output saved to: " + outputDir);

                // List generated files
                if (Directory.Exists(outputDir))
                {
                    string[] files = Directory.GetFiles(outputDir);
                    Console.WriteLine("[INFO] Generated " + files.Length + " file(s):");
                    foreach (string file in files)
                    {
                        FileInfo fi = new FileInfo(file);
                        Console.WriteLine("  - " + Path.GetFileName(file) + " (" + (fi.Length / 1024.0).ToString("F2") + " KB)");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ERROR] " + ex.GetType().Name + ": " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("[INNER ERROR] " + ex.InnerException.Message);
                }
            }
        }

        static void CreateSampleZplFile(string filePath)
        {
            // Sample ZPL code for a simple label
            string zplContent = @"^XA

^FX Top section with logo, name and address.
^CF0,60
^FO50,50^GB100,100,100^FS
^FO75,75^FR^GB100,100,100^FS
^FO93,93^GB40,40,40^FS
^FO220,50^FDIntershipping, Inc.^FS
^CF0,30
^FO220,115^FD1000 Shipping Lane^FS
^FO220,155^FDShelbyville TN 38102^FS
^FO220,195^FDUnited States (USA)^FS
^FO50,250^GB700,3,3^FS

^FX Second section with recipient address and permit information.
^CFA,30
^FO50,300^FDJohn Doe^FS
^FO50,340^FD100 Main Street^FS
^FO50,380^FDSpringfield TN 39021^FS
^FO50,420^FDUnited States (USA)^FS
^CFA,15
^FO600,300^GB150,150,3^FS
^FO638,340^FDPermit^FS
^FO638,390^FD123456^FS
^FO50,500^GB700,3,3^FS

^FX Third section with bar code.
^BY5,2,270
^FO100,550^BC^FD99998888^FS

^FX Fourth section (the two boxes on the bottom).
^FO50,900^GB700,250,3^FS
^FO400,900^GB3,250,3^FS
^CF0,40
^FO100,960^FDCtr. X34B-1^FS
^FO100,1010^FDREF1 F00B47^FS
^FO100,1060^FDREF2 BL4H8^FS
^CF0,190
^FO470,955^FDCA^FS

^XZ";

            File.WriteAllText(filePath, zplContent);
        }
    }
}
