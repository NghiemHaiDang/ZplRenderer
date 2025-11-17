using System;
using System.Diagnostics;
using System.IO;
using ZplRenderer.Core.Interfaces;
using ZplRenderer.Infrastructure.Renderers;
using ZplRenderer.Config;

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

                // Create sample ZPL files for testing
                string testZplWithSize = Path.Combine(currentDir, "test_with_size.zpl");
                string testZplNoSize = Path.Combine(currentDir, "test_no_size.zpl");

                if (!File.Exists(testZplWithSize))
                {
                    Console.WriteLine("[WARN] Test ZPL with size not found. Creating...");
                    CreateSampleZplWithSize(testZplWithSize);
                    Console.WriteLine("[INFO] Sample ZPL with size created: " + testZplWithSize);
                }

                if (!File.Exists(testZplNoSize))
                {
                    Console.WriteLine("[WARN] Test ZPL without size not found. Creating...");
                    CreateSampleZplNoSize(testZplNoSize);
                    Console.WriteLine("[INFO] Sample ZPL without size created: " + testZplNoSize);
                }
                Console.WriteLine();

                // ========================================
                // PRIORITY 1: ZPL CÓ ^PW/^LL (ƯU TIÊN CAO NHẤT)
                // ========================================
                // ZPL đã có kích thước ^PW812, ^LL1218 (4x6 inch @ 203 DPI)
                // Hệ thống SẼ DÙNG TRỰC TIẾP kích thước từ ZPL, KHÔNG inject gì thêm
                // Chỉ scale image theo DPI được chọn

                // Test 1: ZPL có size - Render @ 203 DPI (không scale)
                // Kết quả: 812x1218 pixels (giữ nguyên kích thước ZPL)
                Console.WriteLine("=== Test 1: Priority 1 - ZPL CÓ ^PW812/^LL1218 @ 203 DPI ===");
                TestConversion(renderer, testZplWithSize, Path.Combine(outputDir, "with_size_203dpi"), "png", 203);

                // Test 2: ZPL có size - Render @ 300 DPI (scale lên)
                // Logic: 812 * (300/203) = 1200, 1218 * (300/203) = 1800
                // Kết quả: 1200x1800 pixels (4x6 inch @ 300 DPI)
                Console.WriteLine();
                Console.WriteLine("=== Test 2: Priority 1 - ZPL CÓ ^PW812/^LL1218 @ 300 DPI ===");
                TestConversion(renderer, testZplWithSize, Path.Combine(outputDir, "with_size_300dpi"), "png", 300);

                // Test 3: ZPL có size - Render @ 600 DPI (scale lên gấp đôi 300 DPI)
                // Logic: 812 * (600/203) = 2400, 1218 * (600/203) = 3600
                // Kết quả: 2400x3600 pixels (4x6 inch @ 600 DPI)
                Console.WriteLine();
                Console.WriteLine("=== Test 3: Priority 1 - ZPL CÓ ^PW812/^LL1218 @ 600 DPI ===");
                TestConversion(renderer, testZplWithSize, Path.Combine(outputDir, "with_size_600dpi"), "png", 600);

                // ========================================
                // PRIORITY 3: SYSTEM DEFAULT (ƯU TIÊN THẤP NHẤT)
                // ========================================
                // ZPL KHÔNG có ^PW/^LL VÀ user KHÔNG truyền labelWidth/labelHeight
                // Hệ thống SẼ INJECT ^PW812, ^LL1218 (4x6 inch @ 203 DPI)
                // Sau đó scale theo DPI được chọn

                // Test 4: Không có size - Dùng default @ 203 DPI
                // Inject: ^PW812, ^LL1218
                // Kết quả: 812x1218 pixels (4x6 inch @ 203 DPI)
                Console.WriteLine();
                Console.WriteLine("=== Test 4: Priority 3 - SYSTEM DEFAULT @ 203 DPI ===");
                TestConversion(renderer, testZplNoSize, Path.Combine(outputDir, "no_size_203dpi"), "png", 203);

                // Test 5: Không có size - Dùng default @ 300 DPI
                // Inject: ^PW812, ^LL1218 → Render @ 203 DPI → Scale lên 300 DPI
                // Kết quả: 1200x1800 pixels (4x6 inch @ 300 DPI)
                Console.WriteLine();
                Console.WriteLine("=== Test 5: Priority 3 - SYSTEM DEFAULT @ 300 DPI ===");
                TestConversion(renderer, testZplNoSize, Path.Combine(outputDir, "no_size_300dpi"), "png", 300);

                // Test 6: Không có size - Dùng default @ 600 DPI
                // Inject: ^PW812, ^LL1218 → Render @ 203 DPI → Scale lên 600 DPI
                // Kết quả: 2400x3600 pixels (4x6 inch @ 600 DPI)
                Console.WriteLine();
                Console.WriteLine("=== Test 6: Priority 3 - SYSTEM DEFAULT @ 600 DPI ===");
                TestConversion(renderer, testZplNoSize, Path.Combine(outputDir, "no_size_600dpi"), "png", 600);

                // ========================================
                // PRIORITY 2: USER OVERRIDE WIDTH/HEIGHT
                // ========================================
                // ZPL KHÔNG có ^PW/^LL NHƯNG user TRUYỀN labelWidth/labelHeight
                // Hệ thống SẼ CONVERT size về 203 DPI rồi INJECT vào ZPL
                // Công thức: widthAt203Dpi = userWidth / dpi * 203

                // Test 7: User muốn 10cm x 15cm @ 203 DPI
                // 10cm = 3.94" * 203 = 800 dots, 15cm = 5.91" * 203 = 1200 dots
                // Convert: 800/203*203 = 800, 1200/203*203 = 1200
                // Inject: ^PW800, ^LL1200 → Render @ 203 DPI → Scale x1.0
                // Kết quả: 800x1200 pixels
                Console.WriteLine();
                Console.WriteLine("=== Test 7: Priority 2 - USER OVERRIDE 10x15cm @ 203 DPI ===");
                TestConversionWithSize(renderer, testZplNoSize, Path.Combine(outputDir, "user_override_203dpi"), "png", 203, 800, 1200);

                // Test 8: User muốn 10cm x 15cm @ 300 DPI
                // 10cm = 3.94" * 300 = 1182 dots, 15cm = 5.91" * 300 = 1773 dots
                // Convert: 1182/300*203 = 800, 1773/300*203 = 1200
                // Inject: ^PW800, ^LL1200 → Render @ 203 DPI (800x1200) → Scale lên 300 DPI
                // Scale: 800*(300/203) = 1182, 1200*(300/203) = 1773
                // Kết quả: 1182x1773 pixels ✓ Chính xác 10cm x 15cm @ 300 DPI
                Console.WriteLine();
                Console.WriteLine("=== Test 8: Priority 2 - USER OVERRIDE 10x15cm @ 300 DPI ===");
                TestConversionWithSize(renderer, testZplNoSize, Path.Combine(outputDir, "user_override_300dpi"), "png", 300, 1182, 1773);

                // Test 9: User muốn 10cm x 15cm @ 600 DPI
                // 10cm = 3.94" * 600 = 2364 dots, 15cm = 5.91" * 600 = 3546 dots
                // Convert: 2364/600*203 = 800, 3546/600*203 = 1200
                // Inject: ^PW800, ^LL1200 → Render @ 203 DPI (800x1200) → Scale lên 600 DPI
                // Scale: 800*(600/203) = 2364, 1200*(600/203) = 3546
                // Kết quả: 2364x3546 pixels ✓ Chính xác 10cm x 15cm @ 600 DPI
                Console.WriteLine();
                Console.WriteLine("=== Test 9: Priority 2 - USER OVERRIDE 10x15cm @ 600 DPI ===");
                TestConversionWithSize(renderer, testZplNoSize, Path.Combine(outputDir, "user_override_600dpi"), "png", 600, 2364, 3546);

                // ========================================
                // ĐỊNH DẠNG FILE KHÁC (PDF, JPG)
                // ========================================

                // Test 10: PDF format với system default size
                // ZPL không có size → Inject ^PW812, ^LL1218 (Priority 3)
                // Render @ 203 DPI → Scale lên 300 DPI → 1200x1800 pixels
                // Kết quả: File PDF với multi-page (nếu có nhiều label)
                Console.WriteLine();
                Console.WriteLine("=== Test 10: PDF FORMAT - System Default @ 300 DPI ===");
                TestConversion(renderer, testZplNoSize, Path.Combine(outputDir, "pdf_300dpi"), "pdf", 300);

                // Test 11: JPG format với user override 5x7 inch
                // User truyền: 5" * 203 = 1015 dots, 7" * 203 = 1421 dots (Priority 2)
                // Convert: 1015/203*203 = 1015, 1421/203*203 = 1421
                // Inject: ^PW1015, ^LL1421 → Render @ 203 DPI → Scale x1.0
                // Kết quả: 1015x1421 pixels, format JPG với white background
                Console.WriteLine();
                Console.WriteLine("=== Test 11: JPG FORMAT - User Override 5x7 inch @ 203 DPI ===");
                TestConversionWithSize(renderer, testZplNoSize, Path.Combine(outputDir, "jpg_5x7_203dpi"), "jpg", 203, 1015, 1421);

                // ========================================
                // XỬ LÝ LỖI (ERROR HANDLING)
                // ========================================

                // Test 12: Kiểm tra xử lý lỗi khi file không tồn tại
                // Mong đợi: Throw FileNotFoundException
                // Kết quả: Exception được catch đúng, hiển thị thông báo lỗi
                Console.WriteLine();
                Console.WriteLine("=== Test 12: XỬ LÝ LỖI - File không tồn tại ===");
                try
                {
                    renderer.ConvertZplToFile("nonexistent.zpl", outputDir, "png");
                    Console.WriteLine("[ERROR] Lỗi! Phải throw FileNotFoundException!");
                }
                catch (FileNotFoundException ex)
                {
                    Console.WriteLine("[OK] ✓ Đã catch FileNotFoundException đúng: " + ex.Message);
                }

                // Test 13: Kiểm tra xử lý lỗi khi format không hợp lệ
                // Format hỗ trợ: png, jpg, jpeg, pdf
                // Format không hỗ trợ: bmp, gif, tiff, etc.
                // Mong đợi: Throw ArgumentException
                // Kết quả: Exception được catch đúng, hiển thị thông báo lỗi
                Console.WriteLine();
                Console.WriteLine("=== Test 13: XỬ LÝ LỖI - Format không hợp lệ (bmp) ===");
                try
                {
                    renderer.ConvertZplToFile(testZplWithSize, outputDir, "bmp");
                    Console.WriteLine("[ERROR] Lỗi! Phải throw ArgumentException!");
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine("[OK] ✓ Đã catch ArgumentException đúng: " + ex.Message);
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

        /// <summary>
        /// Test conversion CHỈ với DPI (không truyền width/height)
        /// </summary>
        /// <remarks>
        /// Hàm này test 2 trường hợp:
        /// 1. Priority 1: Nếu ZPL có ^PW/^LL → Dùng trực tiếp
        /// 2. Priority 3: Nếu ZPL KHÔNG có ^PW/^LL → Inject system default (812x1218)
        /// </remarks>
        static void TestConversion(IZplRenderer renderer, string zplFile, string outputDir, string format, int dpi = 203)
        {
            try
            {
                Console.WriteLine("[START] Converting to " + format.ToUpper() + " at " + dpi + " DPI...");

                Stopwatch sw = Stopwatch.StartNew();

                // Tạo options CHỈ với DPI, KHÔNG truyền labelWidth/labelHeight
                // → Test Priority 1 (ZPL có size) hoặc Priority 3 (System default)
                var options = new ZplRenderOptions
                {
                    Dpi = dpi
                };

                // Gọi renderer để convert ZPL → Image
                renderer.ConvertZplToFile(zplFile, outputDir, format, options);

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

        /// <summary>
        /// Test conversion với DPI VÀ width/height do USER TRUYỀN VÀO
        /// </summary>
        /// <remarks>
        /// Hàm này test PRIORITY 2: User Override
        /// - User truyền labelWidth và labelHeight (kích thước ở DPI mục tiêu)
        /// - Hệ thống sẽ CONVERT về 203 DPI: widthAt203Dpi = width / dpi * 203
        /// - INJECT ^PW và ^LL vào ZPL
        /// - BinaryKits render @ 203 DPI → Scale lên DPI mục tiêu
        /// - Kết quả cuối cùng = chính xác kích thước user muốn
        /// </remarks>
        static void TestConversionWithSize(IZplRenderer renderer, string zplFile, string outputDir, string format, int dpi, int width, int height)
        {
            try
            {
                Console.WriteLine("[START] Converting to " + format.ToUpper() + " at " + dpi + " DPI...");
                Console.WriteLine("[INFO] User override size: " + width + "x" + height + " dots @ " + dpi + " DPI");

                // Tính kích thước vật lý (inches và cm) để hiển thị cho user
                double widthInches = width / (double)dpi;
                double heightInches = height / (double)dpi;
                double widthCm = widthInches * 2.54;
                double heightCm = heightInches * 2.54;

                Console.WriteLine("[INFO] Kích thước vật lý: " + widthInches.ToString("F2") + "\" x " + heightInches.ToString("F2") + "\" (" +
                                  widthCm.ToString("F1") + "cm x " + heightCm.ToString("F1") + "cm)");

                Stopwatch sw = Stopwatch.StartNew();

                // Tạo options với DPI VÀ size override (PRIORITY 2)
                // Hệ thống sẽ convert: widthAt203Dpi = width / dpi * 203
                var options = new ZplRenderOptions(dpi, width, height);

                // Gọi renderer - Logic PRIORITY 2 sẽ được thực thi
                renderer.ConvertZplToFile(zplFile, outputDir, format, options);

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

        /// <summary>
        /// Tạo file ZPL mẫu CÓ KÍCH THƯỚC (Priority 1 Test)
        /// </summary>
        /// <remarks>
        /// File này chứa ^PW812 và ^LL1218 (4x6 inch @ 203 DPI)
        /// Sử dụng cho Test 1, 2, 3 để test PRIORITY 1:
        /// - Hệ thống sẽ DÙNG TRỰC TIẾP kích thước từ ZPL
        /// - KHÔNG inject gì thêm
        /// - Chỉ scale image theo DPI được chọn
        /// </remarks>
        static void CreateSampleZplWithSize(string filePath)
        {
            // ZPL CÓ KÍCH THƯỚC RÕ RÀNG: Label 4x6 inch @ 203 DPI = 812x1218 dots
            // ^PW = Print Width (812 dots = 4 inches * 203 DPI)
            // ^LL = Label Length (1218 dots = 6 inches * 203 DPI)
            string zplContent = @"^XA

^FX Label size: 4 inches x 6 inches (812 x 1218 dots at 203 DPI)
^PW812
^LL1218

^FX Top section with logo, name and address
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

^FX Second section with recipient address
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

^FX Barcode section
^BY5,2,270
^FO100,550^BC^FD99998888^FS

^FX Bottom section
^FO50,900^GB700,250,3^FS
^FO400,900^GB3,250,3^FS
^CF0,40
^FO100,960^FDCtr. X34B-1^FS
^FO100,1010^FDREF1 F00B47^FS
^FO100,1060^FDREF2 BL4H8^FS
^CF0,190
^FO470,955^FDCA^FS

^XZ
";

            File.WriteAllText(filePath, zplContent);
        }

        /// <summary>
        /// Tạo file ZPL mẫu KHÔNG CÓ KÍCH THƯỚC (Priority 2 & 3 Test)
        /// </summary>
        /// <remarks>
        /// File này KHÔNG chứa ^PW và ^LL
        /// Sử dụng cho:
        /// - Test 4, 5, 6: Test PRIORITY 3 (System Default)
        ///   → Hệ thống inject ^PW812, ^LL1218
        /// - Test 7, 8, 9: Test PRIORITY 2 (User Override)
        ///   → Hệ thống inject size do user truyền vào (đã convert về 203 DPI)
        /// - Test 10, 11: Test PDF và JPG format
        /// </remarks>
        static void CreateSampleZplNoSize(string filePath)
        {
            // ZPL KHÔNG CÓ KÍCH THƯỚC
            // Hệ thống sẽ tự động inject ^PW/^LL theo Priority 2 hoặc 3
            string zplContent = @"^XA

^FX No ^PW or ^LL commands - size will be auto-detected or use defaults

^FX Simple label content
^CF0,60
^FO50,50^GB100,100,100^FS
^FO75,75^FR^GB100,100,100^FS
^FO93,93^GB40,40,40^FS
^FO220,50^FDTest Company^FS

^CF0,30
^FO50,200^FDProduct: ABC-123^FS
^FO50,250^FDQuantity: 100^FS
^FO50,300^FDDate: 2024-11-17^FS

^FX Barcode
^BY3,2,100
^FO50,400^BC^FD123456789^FS

^FX Footer
^CF0,25
^FO50,550^FDMade with ZplRenderer^FS

^XZ
";

            File.WriteAllText(filePath, zplContent);
        }
    }
}
