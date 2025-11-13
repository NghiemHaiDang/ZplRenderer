#if NET40
using System;
using System.Drawing;

namespace ZplRenderer.Config
{
    public class ZplRenderOptions
    {
        public int Dpi { get; set; } = 203;
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

    public class FontMapping
    {
        public string ZebraFontName { get; set; }
        public string FontAliasNumber { get; set; }
        public string WindowsFontName { get; set; }
        public FontStyle FontStyle { get; set; }

        public FontMapping(string zebraFontName, string fontAliasNumber, string windowsFontName, FontStyle fontStyle)
        {
            ZebraFontName = zebraFontName;
            FontAliasNumber = fontAliasNumber;
            WindowsFontName = windowsFontName;
            FontStyle = fontStyle;
        }
    }
}
#else
using System.Drawing;

namespace ZplRenderer.Config
{
    public class ZplRenderOptions
    {
        public int Dpi { get; set; } = 203;
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

    public class FontMapping
    {
        public string ZebraFontName { get; set; }
        public string FontAliasNumber { get; set; }
        public string WindowsFontName { get; set; }
        public FontStyle FontStyle { get; set; }

        public FontMapping(string zebraFontName, string fontAliasNumber, string windowsFontName, FontStyle fontStyle)
        {
            ZebraFontName = zebraFontName;
            FontAliasNumber = fontAliasNumber;
            WindowsFontName = windowsFontName;
            FontStyle = fontStyle;
        }
    }
}
#endif
