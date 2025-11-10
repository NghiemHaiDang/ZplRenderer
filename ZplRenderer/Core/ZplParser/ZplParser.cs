using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ZplRenderer.Core.ZplParser
{
    public class ZplParser
    {
        private int currentX = 0;
        private int currentY = 0;
        private string currentFont = "A";
        private int currentFontHeight = 30;
        private int currentFontWidth = 0;
        private int currentOrientation = 0;

        public ZplLabel Parse(string zplCode)
        {
            var label = new ZplLabel();

            // Reset position
            currentX = 0;
            currentY = 0;

            // Split into commands
            var commands = ParseCommands(zplCode);

            foreach (var command in commands)
            {
                ProcessCommand(command, label);
            }

            return label;
        }

        private List<ZplCommand> ParseCommands(string zplCode)
        {
            var commands = new List<ZplCommand>();

            // Remove comments and clean up
            zplCode = Regex.Replace(zplCode, @"\^FX.*?(?=\^|\~)", "", RegexOptions.Singleline);

            // Split by ^ or ~ command markers
            var matches = Regex.Matches(zplCode, @"[\^~]([A-Z0-9]{1,3})([^~^]*)");

            foreach (Match match in matches)
            {
                var cmd = new ZplCommand
                {
                    CommandType = match.Groups[1].Value,
                    Data = match.Groups[2].Value.Trim()
                };

                // Parse parameters if data contains them
                if (!string.IsNullOrEmpty(cmd.Data))
                {
                    var parts = cmd.Data.Split(',');
                    cmd.Parameters.AddRange(parts);
                }

                commands.Add(cmd);
            }

            return commands;
        }

        private void ProcessCommand(ZplCommand command, ZplLabel label)
        {
            switch (command.CommandType)
            {
                case "PW": // Print Width
                    if (command.Parameters.Count > 0)
                    {
                        int width;
                        if (int.TryParse(command.Parameters[0], out width))
                        {
                            label.Width = width;
                        }
                    }
                    break;

                case "PL": // Page Length
                    if (command.Parameters.Count > 0)
                    {
                        int height;
                        if (int.TryParse(command.Parameters[0], out height))
                        {
                            label.Height = height;
                        }
                    }
                    break;

                case "FO": // Field Origin
                    if (command.Parameters.Count >= 2)
                    {
                        int.TryParse(command.Parameters[0], out currentX);
                        int.TryParse(command.Parameters[1], out currentY);
                    }
                    break;

                case "A": // Font selection (^A, ^A0, ^A@, etc)
                case "A0":
                case "AB":
                case "AC":
                case "AD":
                case "AE":
                case "AF":
                    if (command.Parameters.Count > 0)
                    {
                        currentFont = command.Parameters[0];
                        if (command.Parameters.Count > 1)
                        {
                            int.TryParse(command.Parameters[1], out currentOrientation);
                        }
                        if (command.Parameters.Count > 2)
                        {
                            int.TryParse(command.Parameters[2], out currentFontHeight);
                        }
                        if (command.Parameters.Count > 3)
                        {
                            int.TryParse(command.Parameters[3], out currentFontWidth);
                        }
                    }
                    break;

                case "FD": // Field Data
                    var textField = new ZplTextField
                    {
                        X = currentX,
                        Y = currentY,
                        Text = command.Data.Replace("^FD", "").Replace("^FS", "").Trim(),
                        FontName = currentFont,
                        FontHeight = currentFontHeight,
                        FontWidth = currentFontWidth > 0 ? currentFontWidth : currentFontHeight,
                        Orientation = currentOrientation
                    };
                    label.Elements.Add(textField);
                    break;

                case "GB": // Graphic Box
                    if (command.Parameters.Count >= 2)
                    {
                        var box = new ZplGraphicBox
                        {
                            X = currentX,
                            Y = currentY
                        };
                        int tempInt;
                        if (int.TryParse(command.Parameters[0], out tempInt))
                            box.Width = tempInt;
                        if (int.TryParse(command.Parameters[1], out tempInt))
                            box.Height = tempInt;
                        if (command.Parameters.Count > 2)
                        {
                            if (int.TryParse(command.Parameters[2], out tempInt))
                                box.Thickness = tempInt;
                        }
                        if (command.Parameters.Count > 3)
                        {
                            if (int.TryParse(command.Parameters[3], out tempInt))
                                box.Rounding = tempInt;
                        }
                        label.Elements.Add(box);
                    }
                    break;

                case "BC": // Code 128 Barcode
                case "B3": // Code 39 Barcode
                case "BE": // EAN-13 Barcode
                case "BQ": // QR Code
                    var barcode = new ZplBarcode
                    {
                        X = currentX,
                        Y = currentY,
                        BarcodeType = command.CommandType,
                        Orientation = currentOrientation
                    };

                    int barcodeTemp;
                    if (command.Parameters.Count > 0)
                    {
                        if (int.TryParse(command.Parameters[0], out barcodeTemp))
                            barcode.Orientation = barcodeTemp;
                    }
                    if (command.Parameters.Count > 1)
                    {
                        if (int.TryParse(command.Parameters[1], out barcodeTemp))
                            barcode.Height = barcodeTemp;
                    }
                    if (command.Parameters.Count > 2)
                    {
                        barcode.PrintInterpretationLine = command.Parameters[2].ToUpper() == "Y";
                    }
                    if (command.Parameters.Count > 3)
                    {
                        barcode.PrintInterpretationLineAboveCode = command.Parameters[3].ToUpper() == "Y";
                    }

                    label.Elements.Add(barcode);
                    break;

                case "LH": // Label Home
                    if (command.Parameters.Count >= 2)
                    {
                        int offsetX, offsetY;
                        int.TryParse(command.Parameters[0], out offsetX);
                        int.TryParse(command.Parameters[1], out offsetY);
                        // Apply offset to future coordinates
                    }
                    break;
            }
        }
    }
}
