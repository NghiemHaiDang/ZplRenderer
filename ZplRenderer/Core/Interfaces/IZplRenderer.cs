using System;

namespace ZplRenderer.Core.Interfaces
{
    public interface IZplRenderer
    {
        void ConvertZplToFile(string zplFilePath, string outputDirectory, string format);
    }
}
