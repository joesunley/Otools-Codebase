using OTools.Common;
using OTools.Maps;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Diagnostics.CodeAnalysis;

var app = new CommandApp<WinMapCommand>();
return app.Run(args);

internal sealed class WinMapCommand : Command<WinMapCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "[filePath]")]
        public string? FilePath { get; init; }

        [CommandOption("-i|--iccpath")]
        public string? IccPath { get; init; }

        [CommandOption("-w|--write")]
        public bool WriteToFile { get; set; }
    }

    public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
    {
        string filePath;
        if (settings.FilePath is null)
        {
            Console.Write("Filepath: ");
            filePath = Console.ReadLine() ?? throw new InvalidOperationException();
            //if (File.Exists(@$"{Directory.GetCurrentDirectory()}\")
        }
        else filePath = settings.FilePath;

        string iccLoc = settings.IccPath ?? @"C:\Program Files (x86)\Purple Pen\USWebCoatedSWOP.icc";

        ColourConverter.SetUri(iccLoc);

        Map map = MapLoader.Load(filePath);

        map.MapInfo.ColourLUT = ColourLUT.Create(map.Colours);

        Colour.Lut = map.MapInfo.ColourLUT;

        Grid grid = new();

        grid.AddColumn();
        grid.AddColumn();

        grid.AddRow(new string[] { "Calc", "Actual" });


        foreach (var kvp in Colour.Lut)
        {
            var (c, m, y, k) = kvp.Key;

            byte r1 = (byte)(255 * (1 - c) * (1 - k)),
                 g1 = (byte)(255 * (1 - m) * (1 - k)),
                 b1 = (byte)(255 * (1 - y) * (1 - k));

            var (r, g, b) = kvp.Value;

            Color c1 = new(r1, g1, b1),
                   c2 = new(r, g, b);
            Style s1 = new(Color.White, c1),
                  s2 = new(Color.White, c2);

            grid.AddRow(new Text[] { new("     ", s1), new("     ", s2) });
        }

        AnsiConsole.Write(grid);

        if (settings.WriteToFile)
            MapLoader.Save(map, 2).Serialize(filePath);

        return 0;
    }
}