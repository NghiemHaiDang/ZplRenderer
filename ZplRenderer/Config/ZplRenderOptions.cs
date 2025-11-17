#if NET40
using System;

namespace ZplRenderer.Config
{
    public class ZplRenderOptions
    {
        public int Dpi { get; set; } = AppConstants.DefaultDpi;

        public int? LabelWidth { get; set; }

        public int? LabelHeight { get; set; }

        public ZplRenderOptions()
        {
        }

        public ZplRenderOptions(int dpi)
        {
            Dpi = dpi;
        }

        public ZplRenderOptions(int dpi, int labelWidth, int labelHeight)
        {
            Dpi = dpi;
            LabelWidth = labelWidth;
            LabelHeight = labelHeight;
        }
    }
}
#else
namespace ZplRenderer.Config
{
    public class ZplRenderOptions
    {
        public int Dpi { get; set; } = AppConstants.DefaultDpi;

        public int? LabelWidth { get; set; }

        public int? LabelHeight { get; set; }

        public ZplRenderOptions()
        {
        }

        public ZplRenderOptions(int dpi)
        {
            Dpi = dpi;
        }

        public ZplRenderOptions(int dpi, int labelWidth, int labelHeight)
        {
            Dpi = dpi;
            LabelWidth = labelWidth;
            LabelHeight = labelHeight;
        }
    }
}
#endif
