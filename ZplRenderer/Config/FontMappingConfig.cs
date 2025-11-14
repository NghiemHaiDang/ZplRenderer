using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ZplRenderer.Config
{
    /// <summary>
    /// Configuration for ZPL to Windows font mappings
    /// </summary>
    public static class FontMappingConfig
    {
        private static List<FontMapping> _fontMappings;

        /// <summary>
        /// Get all configured font mappings
        /// </summary>
        public static List<FontMapping> FontMappings
        {
            get
            {
                if (_fontMappings == null)
                {
                    InitializeDefaultMappings();
                }
                return _fontMappings;
            }
        }

        /// <summary>
        /// Initialize default font mappings
        /// </summary>
        private static void InitializeDefaultMappings()
        {
            _fontMappings = new List<FontMapping>
            {
                // Example: ARIALNB1 -> Arial Narrow Bold
                new FontMapping("ARIALNB1", "0", "Arial Narrow", FontStyle.Bold),

                // Add more mappings as needed
                new FontMapping("ARIAL", "0", "Arial", FontStyle.Regular),
                new FontMapping("ARIALB", "0", "Arial", FontStyle.Bold),
                new FontMapping("ARIALI", "0", "Arial", FontStyle.Italic),
                new FontMapping("ARIALBI", "0", "Arial", FontStyle.Bold | FontStyle.Italic),
            };
        }

        /// <summary>
        /// Add custom font mapping
        /// </summary>
        public static void AddFontMapping(string zebraFontName, string fontAliasNumber, string windowsFontName, FontStyle fontStyle)
        {
            if (_fontMappings == null)
            {
                InitializeDefaultMappings();
            }

            _fontMappings.Add(new FontMapping(zebraFontName, fontAliasNumber, windowsFontName, fontStyle));
        }

        /// <summary>
        /// Get Windows font info by Zebra font name
        /// </summary>
        public static FontMapping GetFontMapping(string zebraFontName)
        {
            return FontMappings.FirstOrDefault(f => f.ZebraFontName == zebraFontName);
        }

        /// <summary>
        /// Get Windows font info by font alias number
        /// </summary>
        public static FontMapping GetFontMappingByAlias(string fontAliasNumber)
        {
            return FontMappings.FirstOrDefault(f => f.FontAliasNumber == fontAliasNumber);
        }

        /// <summary>
        /// Clear all mappings and reset to defaults
        /// </summary>
        public static void ResetToDefaults()
        {
            _fontMappings = null;
            InitializeDefaultMappings();
        }

        /// <summary>
        /// Set custom font mappings (replaces all existing mappings)
        /// </summary>
        public static void SetFontMappings(List<FontMapping> mappings)
        {
            _fontMappings = mappings;
        }
    }
}
