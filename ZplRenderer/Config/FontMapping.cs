using System.Drawing;

namespace ZplRenderer.Config
{
    /// <summary>
    /// Font mapping configuration for ZPL fonts to Windows fonts
    /// </summary>
    public class FontMapping
    {
        /// <summary>
        /// Font name in Zebra printer (e.g., "ARIALNB1")
        /// </summary>
        public string ZebraFontName { get; set; }

        /// <summary>
        /// Font alias number (e.g., "0")
        /// </summary>
        public string FontAliasNumber { get; set; }

        /// <summary>
        /// Windows font name (e.g., "Arial Narrow")
        /// </summary>
        public string WindowsFontName { get; set; }

        /// <summary>
        /// Font style (e.g., FontStyle.Bold)
        /// </summary>
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
