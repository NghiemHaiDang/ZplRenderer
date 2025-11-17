#if NET40
using System;
using System.Collections.Generic;
using System.Drawing;
using ZplRenderer.Config;

namespace ZplRenderer.Core.Interfaces
{
    public interface IZplRenderer
    {
        /// <summary>
        /// HÀM DUY NHẤT - Convert ZPL to file với tất cả tham số optional
        /// </summary>
        /// <param name="zplFilePath">Đường dẫn file ZPL (BẮT BUỘC)</param>
        /// <param name="outputDirectory">Thư mục output (null = Desktop/ZplRenderer_Output)</param>
        /// <param name="format">Format file (null = "png")</param>
        /// <param name="options">Options (null = default: DPI 203, no size override)</param>
        void ConvertZplToFile(string zplFilePath, string outputDirectory = null, string format = null, ZplRenderOptions options = null);

        /// <summary>
        /// Convert ZPL to Image objects trong memory
        /// </summary>
        /// <param name="zplFilePath">Đường dẫn file ZPL (BẮT BUỘC)</param>
        /// <param name="options">Options (null = default: DPI 203, no size override)</param>
        /// <param name="format">Format (null = "jpg", có thể "png")</param>
        List<Image> ConvertZplToImages(string zplFilePath, ZplRenderOptions options = null, string format = null);
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
        /// <summary>
        /// HÀM DUY NHẤT - Convert ZPL to file async với tất cả tham số optional
        /// </summary>
        /// <param name="zplFilePath">Đường dẫn file ZPL (BẮT BUỘC)</param>
        /// <param name="outputDirectory">Thư mục output (null = Desktop/ZplRenderer_Output)</param>
        /// <param name="format">Format file (null = "png")</param>
        /// <param name="options">Options (null = default: DPI 203, no size override)</param>
        Task ConvertZplToFileAsync(string zplFilePath, string? outputDirectory = null, string? format = null, ZplRenderOptions? options = null);

        /// <summary>
        /// Convert ZPL to Image objects trong memory (async)
        /// </summary>
        /// <param name="zplFilePath">Đường dẫn file ZPL (BẮT BUỘC)</param>
        /// <param name="options">Options (null = default: DPI 203, no size override)</param>
        /// <param name="format">Format (null = "jpg", có thể "png")</param>
        Task<List<Image>> ConvertZplToImagesAsync(string zplFilePath, ZplRenderOptions? options = null, string? format = null);
    }
}
#endif
