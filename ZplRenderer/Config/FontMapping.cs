using System.Drawing;

namespace ZplRenderer.Config
{
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
