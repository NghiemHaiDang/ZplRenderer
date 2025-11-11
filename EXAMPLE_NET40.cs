// ======================================================
// EXAMPLE: How to use ZplRenderer from .NET 4.0
// ======================================================

using System;
using System.IO;
using ZplRenderer.Infrastructure.Renderers;

namespace YourNamespace
{
    class Program
    {
        static void Main(string[] args)
        {
            // Example 1: Simple conversion
            SimpleConversion();

            // Example 2: Batch processing
            // BatchProcessing();

            // Example 3: With error handling
            // ConversionWithErrorHandling();

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Example 1: Simple ZPL to PDF conversion
        /// </summary>
        static void SimpleConversion()
        {
            Console.WriteLine("=== Example 1: Simple Conversion ===");

            var service = new ZplRenderService();

            service.ConvertZplToFile(
                zplFilePath: @"C:\temp\input.zpl",
                outputDirectory: @"C:\temp\output",
                format: "pdf"
            );

            Console.WriteLine("Conversion completed!");
        }

        /// <summary>
        /// Example 2: Batch process multiple ZPL files
        /// </summary>
        static void BatchProcessing()
        {
            Console.WriteLine("\n=== Example 2: Batch Processing ===");

            var service = new ZplRenderService();
            string inputFolder = @"C:\temp\zpl_files";
            string outputFolder = @"C:\temp\output";

            if (!Directory.Exists(inputFolder))
            {
                Console.WriteLine("Input folder not found: " + inputFolder);
                return;
            }

            string[] zplFiles = Directory.GetFiles(inputFolder, "*.zpl");
            Console.WriteLine("Found {0} ZPL files", zplFiles.Length);

            foreach (string zplFile in zplFiles)
            {
                try
                {
                    Console.Write("Processing: " + Path.GetFileName(zplFile) + "...");

                    service.ConvertZplToFile(zplFile, outputFolder, "pdf");

                    Console.WriteLine(" OK");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(" FAILED: " + ex.Message);
                }
            }

            Console.WriteLine("Batch processing completed!");
        }

        /// <summary>
        /// Example 3: Conversion with comprehensive error handling
        /// </summary>
        static void ConversionWithErrorHandling()
        {
            Console.WriteLine("\n=== Example 3: With Error Handling ===");

            string zplFile = @"C:\temp\label.zpl";
            string outputFolder = @"C:\temp\output";
            string format = "pdf";

            // Validate input
            if (!File.Exists(zplFile))
            {
                Console.WriteLine("ERROR: ZPL file not found: " + zplFile);
                return;
            }

            if (!IsValidFormat(format))
            {
                Console.WriteLine("ERROR: Invalid format. Use: png, jpg, or pdf");
                return;
            }

            // Create output directory
            try
            {
                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                    Console.WriteLine("Created output directory: " + outputFolder);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: Cannot create output directory: " + ex.Message);
                return;
            }

            // Perform conversion
            try
            {
                Console.WriteLine("Converting ZPL to {0}...", format.ToUpper());

                var service = new ZplRenderService();
                service.ConvertZplToFile(zplFile, outputFolder, format);

                Console.WriteLine("SUCCESS!");
                Console.WriteLine("Output: " + outputFolder);
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine("ERROR: File not found - " + ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine("ERROR: Permission denied - " + ex.Message);
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

        /// <summary>
        /// Helper: Validate output format
        /// </summary>
        static bool IsValidFormat(string format)
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

    /// <summary>
    /// Example 4: Reusable batch processor class
    /// </summary>
    public class ZplBatchProcessor
    {
        private ZplRenderService _service;

        public ZplBatchProcessor()
        {
            _service = new ZplRenderService();
        }

        public BatchResult ProcessDirectory(string inputDirectory, string outputDirectory, string format)
        {
            var result = new BatchResult();

            if (!Directory.Exists(inputDirectory))
            {
                throw new DirectoryNotFoundException("Input directory not found: " + inputDirectory);
            }

            // Ensure output directory exists
            Directory.CreateDirectory(outputDirectory);

            // Get all ZPL files
            string[] files = Directory.GetFiles(inputDirectory, "*.zpl");
            result.TotalFiles = files.Length;

            Console.WriteLine("Processing {0} files...", files.Length);

            foreach (string file in files)
            {
                try
                {
                    _service.ConvertZplToFile(file, outputDirectory, format);
                    result.SuccessCount++;
                    Console.WriteLine("[OK] " + Path.GetFileName(file));
                }
                catch (Exception ex)
                {
                    result.FailedCount++;
                    result.Errors.Add(Path.GetFileName(file) + ": " + ex.Message);
                    Console.WriteLine("[FAILED] " + Path.GetFileName(file) + ": " + ex.Message);
                }
            }

            return result;
        }

        public class BatchResult
        {
            public int TotalFiles { get; set; }
            public int SuccessCount { get; set; }
            public int FailedCount { get; set; }
            public System.Collections.Generic.List<string> Errors { get; set; }

            public BatchResult()
            {
                Errors = new System.Collections.Generic.List<string>();
            }

            public void PrintSummary()
            {
                Console.WriteLine("\n========== Summary ==========");
                Console.WriteLine("Total files: {0}", TotalFiles);
                Console.WriteLine("Succeeded: {0}", SuccessCount);
                Console.WriteLine("Failed: {0}", FailedCount);

                if (Errors.Count > 0)
                {
                    Console.WriteLine("\nErrors:");
                    foreach (string error in Errors)
                    {
                        Console.WriteLine("  - " + error);
                    }
                }
            }
        }
    }
}

// ======================================================
// Usage of BatchProcessor:
// ======================================================
/*
var processor = new ZplBatchProcessor();

try
{
    var result = processor.ProcessDirectory(
        inputDirectory: @"C:\temp\zpl_files",
        outputDirectory: @"C:\temp\output",
        format: "pdf"
    );

    result.PrintSummary();
}
catch (Exception ex)
{
    Console.WriteLine("Batch processing failed: " + ex.Message);
}
*/
