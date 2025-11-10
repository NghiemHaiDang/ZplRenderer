using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using ZplRenderer.Core.ZplParser;

namespace ZplRenderer.Infrastructure.Renderers
{
    public class ZplImageRenderer
    {
        private const float DPI_SCALE = 203f / 96f;

        public Bitmap RenderLabel(ZplLabel label)
        {
            var bitmap = new Bitmap(label.Width, label.Height);
            using (var graphics = Graphics.FromImage(bitmap))
            {
                graphics.Clear(Color.White);
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                foreach (var element in label.Elements)
                {
                    if (element is ZplTextField textField)
                    {
                        RenderTextField(graphics, textField);
                    }
                    else if (element is ZplGraphicBox graphicBox)
                    {
                        RenderGraphicBox(graphics, graphicBox);
                    }
                    else if (element is ZplBarcode barcode)
                    {
                        RenderBarcode(graphics, barcode);
                    }
                }
            }

            return bitmap;
        }

        private void RenderTextField(Graphics graphics, ZplTextField textField)
        {
            if (string.IsNullOrEmpty(textField.Text))
                return;

            var fontFamily = GetFontFamily(textField.FontName);
            float fontSize = textField.FontHeight * 0.8f;

            using (var font = new Font(fontFamily, fontSize, FontStyle.Regular, GraphicsUnit.Pixel))
            using (var brush = new SolidBrush(Color.Black))
            {
                var state = graphics.Save();

                if (textField.Orientation != 0)
                {
                    graphics.TranslateTransform(textField.X, textField.Y);
                    graphics.RotateTransform(textField.Orientation * 90);
                    graphics.DrawString(textField.Text, font, brush, 0, 0);
                }
                else
                {
                    graphics.DrawString(textField.Text, font, brush, textField.X, textField.Y);
                }

                graphics.Restore(state);
            }
        }

        private void RenderGraphicBox(Graphics graphics, ZplGraphicBox box)
        {
            using (var pen = new Pen(Color.Black, box.Thickness))
            {
                if (box.Rounding > 0)
                {
                    var rect = new Rectangle(box.X, box.Y, box.Width, box.Height);
                    DrawRoundedRectangle(graphics, pen, rect, box.Rounding);
                }
                else
                {
                    graphics.DrawRectangle(pen, box.X, box.Y, box.Width, box.Height);
                }
            }
        }

        private void RenderBarcode(Graphics graphics, ZplBarcode barcode)
        {
            string data = barcode.Data;
            if (string.IsNullOrEmpty(data))
                return;

            int x = barcode.X;
            int y = barcode.Y;
            int barWidth = 2;
            int spaceWidth = 2;

            using (var brush = new SolidBrush(Color.Black))
            {
                foreach (char c in data)
                {
                    int charValue = (int)c % 2;
                    if (charValue == 1)
                    {
                        graphics.FillRectangle(brush, x, y, barWidth, barcode.Height);
                    }
                    x += barWidth + spaceWidth;
                }
                if (barcode.PrintInterpretationLine)
                {
                    using (var font = new Font("Arial", 10))
                    {
                        int textY = barcode.PrintInterpretationLineAboveCode ?
                            y - 15 : y + barcode.Height + 5;
                        graphics.DrawString(data, font, brush, barcode.X, textY);
                    }
                }
            }
        }

        private void DrawRoundedRectangle(Graphics graphics, Pen pen, Rectangle rect, int radius)
        {
            using (var path = new GraphicsPath())
            {
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
                path.CloseFigure();
                graphics.DrawPath(pen, path);
            }
        }

        private string GetFontFamily(string zplFont)
        {
            // Map ZPL fonts to Windows fonts
            switch (zplFont.ToUpper())
            {
                case "A":
                case "0":
                    return "Arial";
                case "B":
                    return "Courier New";
                case "C":
                case "D":
                case "E":
                    return "Courier New";
                case "F":
                    return "Times New Roman";
                case "G":
                    return "Arial";
                default:
                    return "Arial";
            }
        }

        public byte[] BitmapToBytes(Bitmap bitmap, ImageFormat format)
        {
            using (var stream = new MemoryStream())
            {
                bitmap.Save(stream, format);
                return stream.ToArray();
            }
        }
    }
}
