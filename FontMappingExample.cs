using System;
using System.Drawing;
using ZplRenderer.Config;

class FontMappingExample
{
    static void Main()
    {
        Console.WriteLine("=== Font Mapping Configuration Example ===\n");

        // Example 1: Get default font mappings
        Console.WriteLine("Default Font Mappings:");
        foreach (var mapping in FontMappingConfig.FontMappings)
        {
            Console.WriteLine($"  {mapping.ZebraFontName} (Alias: {mapping.FontAliasNumber}) -> {mapping.WindowsFontName} [{mapping.FontStyle}]");
        }

        Console.WriteLine("\n=== Example 2: Add Custom Font Mapping ===");
        // Add custom font mapping
        FontMappingConfig.AddFontMapping("TIMESNR", "1", "Times New Roman", FontStyle.Regular);
        FontMappingConfig.AddFontMapping("TIMESNRB", "1", "Times New Roman", FontStyle.Bold);

        Console.WriteLine("After adding custom mappings:");
        foreach (var mapping in FontMappingConfig.FontMappings)
        {
            Console.WriteLine($"  {mapping.ZebraFontName} (Alias: {mapping.FontAliasNumber}) -> {mapping.WindowsFontName} [{mapping.FontStyle}]");
        }

        Console.WriteLine("\n=== Example 3: Lookup Font by Name ===");
        var arialMapping = FontMappingConfig.GetFontMapping("ARIALNB1");
        if (arialMapping != null)
        {
            Console.WriteLine($"Found: {arialMapping.ZebraFontName} -> {arialMapping.WindowsFontName} [{arialMapping.FontStyle}]");
        }

        Console.WriteLine("\n=== Example 4: Lookup Font by Alias ===");
        var aliasMappings = FontMappingConfig.GetFontMappingByAlias("0");
        if (aliasMappings != null)
        {
            Console.WriteLine($"Found alias '0': {aliasMappings.ZebraFontName} -> {aliasMappings.WindowsFontName}");
        }

        Console.WriteLine("\n=== Example 5: Custom Usage ===");
        Console.WriteLine("Usage in your code:");
        Console.WriteLine(@"
var mapping = new FontMapping(""ARIALNB1"", ""0"", ""Arial Narrow"", FontStyle.Bold);
// or
FontMappingConfig.AddFontMapping(""ARIALNB1"", ""0"", ""Arial Narrow"", FontStyle.Bold);

// Retrieve
var font = FontMappingConfig.GetFontMapping(""ARIALNB1"");
Console.WriteLine($""{font.WindowsFontName}, {font.FontStyle}"");
");
    }
}
