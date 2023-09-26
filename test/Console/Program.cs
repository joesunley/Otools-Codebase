//using OTools.Maps;
//using OTools.Symbols;

//Console.WriteLine("Hello World!");

//var set = OfficialSymbols.Create();

//var map = set.CreateMap("ISOM");

//MapLoader.Save(map, 2).Serialize(@"C:\Dev\OTools\test\Files\map2.xml");

using OTools.Common;

int[] ints = { 1, 2, 3 };

var res = ints.GetPermutations();

for (int i = 0; i < 100; i++)
{
    Console.WriteLine(string.Join(",", ints.GetPermutations().ElementAt(2)));
}

Console.ReadLine();

foreach (var item in res)
{
    Console.WriteLine(string.Join(", ", item));
}