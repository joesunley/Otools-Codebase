using OTools.Common;
using OTools.Maps;
using Spectre.Console;
using System.IO;

/*
 * 0 -> MapFile
 * 1 -> icc file
 */

//if (args.Length != 2)
//    throw new ArgumentException("Invalid number of arguments.");

//string mapFile = args[0];
//ColourConverter.SetUri(args[1]);

string mapFile;

if (args.Length == 0)
{
    Console.Write("Filepath: ");
    mapFile = Console.ReadLine() ?? string.Empty;
}
else mapFile = args[0];

string iccLoc;

if (args.Length == 2)
    iccLoc = args[1];
else iccLoc = @"C:\Program Files (x86)\Purple Pen\USWebCoatedSWOP.icc";

ColourConverter.SetUri(iccLoc);

Map map = MapLoader.Load(mapFile);

map.MapInfo.ColourLUT = ColourLUT.Create(map.Colours);

Colour.Lut = map.MapInfo.ColourLUT;

Grid grid = new();

grid.AddColumn();
grid.AddColumn();

grid.AddRow(new string[] {"Calc", "Actual"});


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

    grid.AddRow(new Text[] { new("     ", s1), new("     ", s2)});
}

AnsiConsole.Write(grid);

MapLoader.Save(map, 2).Serialize(mapFile);