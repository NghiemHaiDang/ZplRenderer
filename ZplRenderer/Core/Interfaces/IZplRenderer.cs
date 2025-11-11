#if NET40
using System;

namespace ZplRenderer.Core.Interfaces
{
    public interface IZplRenderer
    {
        void ConvertZplToFile(string zplFilePath, string outputDirectory, string format);
    }
}
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZplRenderer.Core.Interfaces
{
    public interface IZplRenderer
    {
        Task ConvertZplToFileAsync(string zplFilePath, string outputDirectory, string format);
    }
}
#endif
