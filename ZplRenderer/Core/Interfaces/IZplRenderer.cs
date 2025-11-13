#if NET40
using System;
using System.Collections.Generic;
using System.Drawing;
using ZplRenderer.Config;

namespace ZplRenderer.Core.Interfaces
{
    public interface IZplRenderer
    {
        // Original method for backward compatibility
        void ConvertZplToFile(string zplFilePath, string outputDirectory, string format);

        // New method with options and Image output
        void ConvertZplToFile(string zplFilePath, string outputDirectory, string format, ZplRenderOptions options);

        // Method to get Image directly
        List<Image> ConvertZplToImages(string zplFilePath, ZplRenderOptions options);
    }
}
#else
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using ZplRenderer.Config;

namespace ZplRenderer.Core.Interfaces
{
    public interface IZplRenderer
    {
        // Original method for backward compatibility
        Task ConvertZplToFileAsync(string zplFilePath, string outputDirectory, string format);

        // New method with options and Image output
        Task ConvertZplToFileAsync(string zplFilePath, string outputDirectory, string format, ZplRenderOptions options);

        // Method to get Image directly
        Task<List<Image>> ConvertZplToImagesAsync(string zplFilePath, ZplRenderOptions options);
    }
}
#endif
