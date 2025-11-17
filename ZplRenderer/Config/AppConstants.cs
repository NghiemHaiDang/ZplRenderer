namespace ZplRenderer.Config
{
    public static class AppConstants
    {
        // Default label width in dots at 203 DPI
        // 4 inches = 812 dots at 203 DPI
        public const int DefaultLabelWidth = 812;

        // Default label height in dots at 203 DPI
        // 6 inches = 1218 dots at 203 DPI
        public const int DefaultLabelHeight = 1218;

        // Default DPI for rendering
        public const int DefaultDpi = 203;

        public static readonly int[] SupportedDpi = { 203, 300, 600 };
    }
}
