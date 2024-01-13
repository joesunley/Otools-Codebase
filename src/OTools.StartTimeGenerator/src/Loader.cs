using OTools.StartTimeDistributor;
using Sunley.Mathematics;

namespace OTools.StartTimeGenerator;

public static class Loader
{
    public static IEnumerable<Entry> LoadEntries(string path, string filter)
    {
        if (!File.Exists(path) || filter == string.Empty)
            return Enumerable.Empty<Entry>();

        filter = filter.ToLower();

        Entry_Filter ef = new()
        {
            id = filter.IndexOf('i'),
            name = filter.IndexOf('n'),
            club = filter.IndexOf('c'),
            rankingId = filter.IndexOf('r'),
            course = filter.IndexOf('x'),
            clas = filter.IndexOf('y'),
            preference = filter.IndexOf('p'),
        };

        List<Entry> entries = new List<Entry>();
        string[] lines = File.ReadAllLines(path).Skip(1).ToArray();

        foreach (string line in lines)
        {
            string[] values = line.Split(',').Select(x => x.Replace("\"", "")).ToArray();

            byte course = values[ef.clas] switch
            {
                "Long" => 1,
                "Medium" => 2,
                "Short" => 3,
                "Junior/Novice" => 4,
                "Not Entering Friday" => 5,
            };

            entries.Add(new()
            {
                Id = values[ef.id].Parse<int>(),

                Name = values[ef.name],
                Club = values[ef.club],
                RankingKey = values[ef.rankingId],

                //Course = values[ef.course].Parse<byte>(),
                Course = course,
                Class = values[ef.clas],
                Preference = Enum.Parse<StartTimeBlock>(values[ef.preference]),
            });

            
        }

        return entries;
    }

    public static Dictionary<string, float> LoadRankings(string path, string filters)
    {
        if (!File.Exists(path) || filters == string.Empty)
            return new();

        int key = filters.IndexOf('k'),
            value = filters.IndexOf('v');

        Dictionary<string, float> rankings = new();
        string[] lines = File.ReadAllLines(path).Skip(1).ToArray();

        foreach (string line in lines)
        {
            string[] values = line.Split(',').Select(x => x.Replace("\"", "")).ToArray();

            rankings.Add(
                values[key],
                values[value].Parse<float>());
        }

        return rankings;
    }
}

file struct Entry_Filter
{
    public int id;
    public int name;
    public int club;
    public int rankingId;
    public int course;
    public int clas;
    public int preference;
}