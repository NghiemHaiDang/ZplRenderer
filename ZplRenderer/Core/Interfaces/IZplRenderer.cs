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
