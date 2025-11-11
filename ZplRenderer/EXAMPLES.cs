// ================================================================
// ZplRenderer - Complete Examples
// ================================================================

using System;
using System.Collections.Generic;
using System.IO;
using ZplRenderer.Infrastructure.Renderers;

namespace ZplRenderer.Examples
{
    // ============================================================
    // EXAMPLE 1: Basic Usage
    // ============================================================
    public class Example01_BasicUsage
    {
        public static void Run()
        {
            Console.WriteLine("=== Example 1: Basic Usage ===");

            var service = new ZplRenderService();

            // Chuyển đổi ZPL sang PDF
            service.ConvertZplToFile(
                zplFilePath: @"C:\temp\label.zpl",
                outputDirectory: @"C:\temp\output",
                format: "pdf"
            );

            Console.WriteLine("Conversion completed!");
        }
    }

    // ============================================================
    // EXAMPLE 2: Convert to Different Formats
    // ============================================================
    public class Example02_DifferentFormats
    {
        public static void Run()
        {
            Console.WriteLine("=== Example 2: Different Formats ===");

            var service = new ZplRenderService();
            string inputFile = @"C:\temp\label.zpl";
            string outputDir = @"C:\temp\output";

            // Convert to PNG
            Console.WriteLine("Converting to PNG...");
            service.ConvertZplToFile(inputFile, outputDir, "png");

            // Convert to JPG
            Console.WriteLine("Converting to JPG...");
            service.ConvertZplToFile(inputFile, outputDir, "jpg");

            // Convert to PDF
            Console.WriteLine("Converting to PDF...");
            service.ConvertZplToFile(inputFile, outputDir, "pdf");

            Console.WriteLine("All conversions completed!");
        }
    }

    // ============================================================
    // EXAMPLE 3: Batch Processing
    // ============================================================
    public class Example03_BatchProcessing
    {
        public static void Run()
        {
            Console.WriteLine("=== Example 3: Batch Processing ===");

            var service = new ZplRenderService();
            string inputFolder = @"C:\temp\zpl_files";
            string outputFolder = @"C:\temp\output";

            if (!Directory.Exists(inputFolder))
            {
                Console.WriteLine("Input folder not found: " + inputFolder);
                return;
            }

            string[] zplFiles = Directory.GetFiles(inputFolder, "*.zpl");
            Console.WriteLine("Found " + zplFiles.Length + " ZPL files");
            Console.WriteLine();

            int success = 0;
            int failed = 0;

            foreach (string zplFile in zplFiles)
            {
                try
                {
                    string fileName = Path.GetFileName(zplFile);
                    Console.Write("Processing: " + fileName + "...");

                    service.ConvertZplToFile(zplFile, outputFolder, "pdf");

                    Console.WriteLine(" OK");
                    success++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" FAILED: " + ex.Message);
                    failed++;
                }
            }

            Console.WriteLine();
            Console.WriteLine("Summary:");
            Console.WriteLine("  Succeeded: " + success);
            Console.WriteLine("  Failed: " + failed);
        }
    }

    // ============================================================
    // EXAMPLE 4: With Full Error Handling
    // ============================================================
    public class Example04_ErrorHandling
    {
        public static void Run()
        {
            Console.WriteLine("=== Example 4: Full Error Handling ===");

            string zplFile = @"C:\temp\label.zpl";
            string outputFolder = @"C:\temp\output";
            string format = "pdf";

            // Validate input file
            if (!File.Exists(zplFile))
            {
                Console.WriteLine("ERROR: ZPL file not found - " + zplFile);
                return;
            }

            // Validate format
            if (!IsValidFormat(format))
            {
                Console.WriteLine("ERROR: Invalid format '" + format + "'");
                Console.WriteLine("Valid formats: png, jpg, jpeg, pdf");
                return;
            }

            // Create output directory
            try
            {
                Directory.CreateDirectory(outputFolder);
                Console.WriteLine("Output directory ready: " + outputFolder);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Cannot create output directory");
                Console.WriteLine("  " + ex.Message);
                return;
            }

            // Perform conversion
            try
            {
                Console.WriteLine("Converting ZPL to " + format.ToUpper() + "...");

                var service = new ZplRenderService();
                service.ConvertZplToFile(zplFile, outputFolder, format);

                Console.WriteLine("SUCCESS!");
                Console.WriteLine("Output: " + outputFolder);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("ERROR: File not found");
                Console.WriteLine("  " + ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("ERROR: Access denied");
                Console.WriteLine("  " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner exception: " + ex.InnerException.Message);
                }
            }
        }

        private static bool IsValidFormat(string format)
        {
            string[] validFormats = { "png", "jpg", "jpeg", "pdf" };
            foreach (string validFormat in validFormats)
            {
                if (format.Equals(validFormat, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }

    // ============================================================
    // EXAMPLE 5: Batch Processor Class
    // ============================================================
    public class BatchProcessor
    {
        private ZplRenderService service;

        public BatchProcessor()
        {
            service = new ZplRenderService();
        }

        public ProcessResult ProcessDirectory(string inputDir, string outputDir, string format)
        {
            var result = new ProcessResult();

            // Validate
            if (!Directory.Exists(inputDir))
            {
                throw new DirectoryNotFoundException("Input directory not found: " + inputDir);
            }

            // Create output directory
            Directory.CreateDirectory(outputDir);

            // Get files
            string[] files = Directory.GetFiles(inputDir, "*.zpl");
            result.TotalFiles = files.Length;

            Console.WriteLine("Processing " + files.Length + " files...");
            Console.WriteLine();

            // Process each file
            for (int i = 0; i < files.Length; i++)
            {
                string file = files[i];
                string fileName = Path.GetFileName(file);

                try
                {
                    Console.Write("[" + (i + 1) + "/" + files.Length + "] " + fileName + "...");
                    service.ConvertZplToFile(file, outputDir, format);
                    Console.WriteLine(" OK");
                    result.SuccessCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" FAILED");
                    result.FailedCount++;
                    result.Errors.Add(fileName + ": " + ex.Message);
                }
            }

            return result;
        }

        public class ProcessResult
        {
            public int TotalFiles { get; set; }
            public int SuccessCount { get; set; }
            public int FailedCount { get; set; }
            public List<string> Errors { get; set; }

            public ProcessResult()
            {
                Errors = new List<string>();
            }

            public void PrintSummary()
            {
                Console.WriteLine();
                Console.WriteLine("========== Summary ==========");
                Console.WriteLine("Total files: " + TotalFiles);
                Console.WriteLine("Succeeded: " + SuccessCount);
                Console.WriteLine("Failed: " + FailedCount);

                if (Errors.Count > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("Errors:");
                    foreach (string error in Errors)
                    {
                        Console.WriteLine("  - " + error);
                    }
                }
            }
        }
    }

    // ============================================================
    // EXAMPLE 6: Using Batch Processor
    // ============================================================
    public class Example06_UsingBatchProcessor
    {
        public static void Run()
        {
            Console.WriteLine("=== Example 6: Using Batch Processor ===");

            var processor = new BatchProcessor();

            try
            {
                var result = processor.ProcessDirectory(
                    inputDir: @"C:\temp\zpl_files",
                    outputDir: @"C:\temp\output",
                    format: "pdf"
                );

                result.PrintSummary();
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Batch processing failed");
                Console.WriteLine("  " + ex.Message);
            }
        }
    }

    // ============================================================
    // EXAMPLE 7: Pre-Extract Console App (Performance Optimization)
    // ============================================================
    public class Example07_PreExtract
    {
        public static void PreExtractConsoleApp()
        {
            Console.WriteLine("=== Example 7: Pre-Extract Console App ===");
            Console.WriteLine("This will extract the console app to temp folder");
            Console.WriteLine("Call this once during application setup/installation");
            Console.WriteLine();

            try
            {
                string tempDir = Path.Combine(Path.GetTempPath(), "ZplRenderer");
                Directory.CreateDirectory(tempDir);
                string consolePath = Path.Combine(tempDir, "ZplRenderer.Console.exe");

                if (File.Exists(consolePath))
                {
                    Console.WriteLine("Console app already extracted");
                    Console.WriteLine("Location: " + consolePath);
                    return;
                }

                Console.WriteLine("Extracting console app...");

                // Extract logic would go here
                // (Simplified for example - actual implementation in ZplRenderService)

                Console.WriteLine("Console app extracted successfully!");
                Console.WriteLine("Location: " + consolePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Failed to pre-extract console app");
                Console.WriteLine("  " + ex.Message);
            }
        }
    }

    // ============================================================
    // EXAMPLE 8: Cleanup Temp Files
    // ============================================================
    public class Example08_Cleanup
    {
        public static void CleanupTempFiles()
        {
            Console.WriteLine("=== Example 8: Cleanup Temp Files ===");
            Console.WriteLine("This will remove extracted console app from temp folder");
            Console.WriteLine();

            try
            {
                string tempDir = Path.Combine(Path.GetTempPath(), "ZplRenderer");

                if (!Directory.Exists(tempDir))
                {
                    Console.WriteLine("Temp directory does not exist - nothing to clean");
                    return;
                }

                Console.WriteLine("Removing: " + tempDir);
                Directory.Delete(tempDir, recursive: true);
                Console.WriteLine("Cleanup completed!");
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("ERROR: Access denied - cannot delete temp files");
            }
            catch (IOException ex)
            {
                Console.WriteLine("ERROR: Cannot delete temp files");
                Console.WriteLine("  " + ex.Message);
                Console.WriteLine("  Files may be in use");
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Cleanup failed");
                Console.WriteLine("  " + ex.Message);
            }
        }
    }

    // ============================================================
    // EXAMPLE 9: Format Converter Utility
    // ============================================================
    public class FormatConverter
    {
        private ZplRenderService service;

        public FormatConverter()
        {
            service = new ZplRenderService();
        }

        public void ConvertToAllFormats(string zplFile, string outputDir)
        {
            Console.WriteLine("Converting to all formats...");
            Console.WriteLine("Input: " + Path.GetFileName(zplFile));
            Console.WriteLine();

            string[] formats = { "png", "jpg", "pdf" };

            foreach (string format in formats)
            {
                try
                {
                    Console.Write("  " + format.ToUpper() + "...");
                    service.ConvertZplToFile(zplFile, outputDir, format);
                    Console.WriteLine(" OK");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" FAILED: " + ex.Message);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Conversion completed!");
        }
    }

    // ============================================================
    // MAIN PROGRAM - Run All Examples
    // ============================================================
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("================================================================");
            Console.WriteLine("ZplRenderer - Complete Examples");
            Console.WriteLine("================================================================");
            Console.WriteLine();

            // Uncomment the example you want to run:

            // Example01_BasicUsage.Run();
            // Example02_DifferentFormats.Run();
            // Example03_BatchProcessing.Run();
            // Example04_ErrorHandling.Run();
            // Example06_UsingBatchProcessor.Run();
            // Example07_PreExtract.PreExtractConsoleApp();
            // Example08_Cleanup.CleanupTempFiles();

            // Example với FormatConverter
            // var converter = new FormatConverter();
            // converter.ConvertToAllFormats(@"C:\temp\label.zpl", @"C:\temp\output");

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
