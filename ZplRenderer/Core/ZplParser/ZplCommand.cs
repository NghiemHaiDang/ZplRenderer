using System;
using System.Collections.Generic;

namespace ZplRenderer.Core.ZplParser
{
    public class ZplCommand
    {
        public string CommandType { get; set; }
        public List<string> Parameters { get; set; }
        public string Data { get; set; }

        public ZplCommand()
        {
            Parameters = new List<string>();
            Data = string.Empty;
        }
    }

    public class ZplLabel
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Dpi { get; set; }
        public List<ZplElement> Elements { get; set; }

        public ZplLabel()
        {
            Width = 812;  // Default 4 inches at 203 DPI
            Height = 1218; // Default 6 inches at 203 DPI
            Dpi = 203;
            Elements = new List<ZplElement>();
        }
    }

    public abstract class ZplElement
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class ZplTextField : ZplElement
    {
        public string Text { get; set; }
        public string FontName { get; set; }
        public int FontHeight { get; set; }
        public int FontWidth { get; set; }
        public int Orientation { get; set; }

        public ZplTextField()
        {
            Text = string.Empty;
            FontName = "A";
            FontHeight = 30;
            FontWidth = 0;
            Orientation = 0;
        }
    }

    public class ZplGraphicBox : ZplElement
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Thickness { get; set; }
        public int Rounding { get; set; }

        public ZplGraphicBox()
        {
            Thickness = 1;
            Rounding = 0;
        }
    }

    public class ZplBarcode : ZplElement
    {
        public string BarcodeType { get; set; }
        public string Data { get; set; }
        public int Height { get; set; }
        public int Orientation { get; set; }
        public bool PrintInterpretationLine { get; set; }
        public bool PrintInterpretationLineAboveCode { get; set; }

        public ZplBarcode()
        {
            BarcodeType = "3"; // Code 39
            Data = string.Empty;
            Height = 100;
            Orientation = 0;
            PrintInterpretationLine = true;
            PrintInterpretationLineAboveCode = false;
        }
    }

    public class ZplGraphicField : ZplElement
    {
        public byte[] ImageData { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public ZplGraphicField()
        {
            ImageData = new byte[0];
        }
    }
}
