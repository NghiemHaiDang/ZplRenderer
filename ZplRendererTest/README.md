# ZplRendererTest - Test Console Application

Project console để test và debug ZplRenderer DLL.

## Cấu trúc Project

- **ZplRendererTest.csproj**: Project file, target .NET Framework 4.0
- **Program.cs**: Code test chính với các test cases
- **.vscode/**: VS Code debug configuration

## Cách sử dụng

### 1. Build Project

```bash
cd ZplRendererTest
dotnet build
```

### 2. Chạy từ Command Line

```bash
cd ZplRendererTest/bin/Debug/net40
ZplRendererTest.exe
```

### 3. Debug trong Visual Studio

- Mở solution `ZplRenderer.sln`
- Set `ZplRendererTest` làm Startup Project (right-click > Set as Startup Project)
- Nhấn F5 để chạy với debugger
- Đặt breakpoint tại các dòng cần debug:
  - Line 19: Khởi tạo renderer
  - Line 114: Trước khi gọi ConvertZplToFile
  - Line 36: Tạo file ZPL mẫu

### 4. Debug trong VS Code

- Mở thư mục `ZplRendererTest` trong VS Code
- Nhấn F5 hoặc chọn "Run > Start Debugging"
- Chọn ".NET Framework Launch" configuration

## Test Cases

Application này bao gồm 5 test cases:

1. **Test 1: Convert to PNG** - Chuyển đổi ZPL sang PNG
2. **Test 2: Convert to JPG** - Chuyển đổi ZPL sang JPG
3. **Test 3: Convert to PDF** - Chuyển đổi ZPL sang PDF
4. **Test 4: Error Handling (Non-existent File)** - Test xử lý lỗi file không tồn tại
5. **Test 5: Error Handling (Invalid Format)** - Test xử lý lỗi format không hợp lệ

## Output

Kết quả chuyển đổi sẽ được lưu trong thư mục:
```
ZplRendererTest/bin/Debug/net40/output/
├── png/
├── jpg/
└── pdf/
```

## Debugging Tips

### Breakpoints quan trọng

1. **Program.cs:19** - Khởi tạo ZplRenderService
   - Kiểm tra DLL được load đúng không

2. **Program.cs:114** - Gọi renderer.ConvertZplToFile()
   - Step Into (F11) để debug vào DLL
   - Xem flow xử lý bên trong ZplRenderService

3. **Program.cs:36** - CreateSampleZplFile()
   - Kiểm tra nội dung ZPL được tạo

### Debug vào trong DLL

1. Build ZplRenderer project ở mode Debug:
   ```bash
   cd ZplRenderer
   dotnet build -c Debug
   ```

2. Đảm bảo file .pdb được copy cùng với DLL:
   ```
   ZplRendererTest/bin/Debug/net40/
   ├── ZplRenderer.dll
   └── ZplRenderer.pdb  ← Cần có file này
   ```

3. Trong Visual Studio:
   - Tools > Options > Debugging
   - Uncheck "Enable Just My Code"
   - Đặt breakpoint trong code của ZplRenderer
   - Step Into (F11) từ Program.cs sẽ nhảy vào code DLL

### Watch Variables

Khi debug, theo dõi các biến sau:

- `renderer` - Instance của ZplRenderService
- `testZplFile` - Đường dẫn file ZPL
- `outputDir` - Thư mục output
- `format` - Format output (png/jpg/pdf)
- `sw.ElapsedMilliseconds` - Thời gian xử lý

## Lưu ý

- Application tự động tạo file test.zpl nếu chưa có
- Sau khi chạy xong, nhấn phím bất kỳ để mở thư mục output
- Các warning về Microsoft.Bcl.Async có thể ignore (không ảnh hưởng đến test)
