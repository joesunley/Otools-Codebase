using OTools.Maps;
using OTools.Symbols;

Console.WriteLine("Hello World!");

var set = OfficialSymbols.Create();

var map = set.CreateMap("ISOM");

MapLoader.Save(map, 2).Serialize(@"C:\Dev\OTools\test\Files\map2.xml");