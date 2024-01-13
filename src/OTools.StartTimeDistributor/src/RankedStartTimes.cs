using OTools.Common;

namespace OTools.StartTimeDistributor;

public class RankedStartTimes
{
    private List<Entry> _entries;
    private List<(string, float)> _rankings;
    private GroupParameters _parameters;
    private int[] _courses;

    public RankedStartTimes(IEnumerable<Entry> entries, IEnumerable<(string, float)> rankings, GroupParameters parameters, int[] courses)
    {
        _entries = entries.ToList();
        _rankings = rankings.ToList();
        _parameters = parameters;
        _courses = courses;
    }

    public Dictionary<Entry, DateTime> Create()
    {
        FilterRankings(ref _rankings, _entries);
        OrderRankings(ref _rankings);

        Dictionary<Entry, DateTime> startTimes = new();

        foreach (byte course in _courses)
        {
            var tCourse = CreateForCourse(course);

            foreach (var e in tCourse)
                startTimes.Add(e.Item1, e.Item2);
        }

        return startTimes;
    }


    private IEnumerable<(Entry, DateTime)> CreateForCourse(byte course)
    {
        Dictionary<string, Entry> lookup = new(
            _entries.Where(x => x.Course == course && x.RankingKey != "")
                    .Where(x => _rankings.Select(x => x.Item1).Contains(x.RankingKey))
                    .Select(x => new KeyValuePair<string, Entry>(x.RankingKey, x))
            );

        List<(Entry, DateTime)> unshuffledStartTimes = new();
        List<(Entry, DateTime)> nonRankedTimes = new();
        var nonRankedSlice = _entries.Where(x => x.Course == course && !lookup.ContainsKey(x.RankingKey)).ToArray();

        DateTime currentTime = _parameters.FirstStart;

        foreach (var entry in nonRankedSlice.Shuffle())
        {
            nonRankedTimes.Add((entry, currentTime));
            currentTime += TimeSpan.FromMinutes(_parameters.StartInterval);
        }

        foreach (string ranking in _rankings.Select(x => x.Item1))
        {
            if (!lookup.Keys.Contains(ranking))
                continue;

            unshuffledStartTimes.Add((lookup[ranking], currentTime));

            currentTime += TimeSpan.FromMinutes(_parameters.StartInterval);

            if (currentTime > _parameters.LastStart)
                throw new Exception("Too many entries");
        }

        if (_parameters.Grouping <= 0)
            return Join(nonRankedTimes, unshuffledStartTimes);

        (Entry, DateTime)[] startTimes = unshuffledStartTimes.ToArray();

        for (int i = 0; i < startTimes.Length; i += _parameters.Grouping)
        {
            if (i + 4 >= startTimes.Length)
                continue;

            var range = startTimes[i..(i + 4)];
            range = range.Shuffle().ToArray();

            for (int j = i; j < i + 4; j++)
                startTimes[j] = range[j - i];
        }

        return Join(nonRankedTimes, startTimes);
    }

    private static void FilterRankings(ref List<(string, float)> rankings, IEnumerable<Entry> entries) 
        => rankings = rankings.Where(x => entries.Select(y => y.RankingKey).Contains(x.Item1)).ToList();
    private static void OrderRankings(ref List<(string, float)> rankings) 
        => rankings = rankings.OrderBy(x => x.Item2).ToList();

    private static IEnumerable<T> Join<T>(IEnumerable<T> a, IEnumerable<T> b)
        => a.Concat(b);
}

public static class WorldRanking
{
    private const bool COMPENSATE_FOR_LESS_THAN_6_EVENTS = false;

    public static Dictionary<string, float> FromCSV(string[] filePaths)
    {
        Dictionary<string, float> kvps = new();

        List<string> lines = new();

        foreach (string path in filePaths)
            lines.AddRange(File.ReadAllLines(path).Skip(1));


        foreach (string line in lines)
        {
            string[] cells = line.Split(';');

            string iofID = cells[0];
            float wrsPoints = float.Parse(cells[5]);
            float avgWrsPoints = float.Parse(cells[7]);

            if (kvps.TryGetValue(iofID, out float existing))
                kvps[iofID] = float.Max(existing, COMPENSATE_FOR_LESS_THAN_6_EVENTS ? avgWrsPoints : wrsPoints);
            else
                kvps.Add(iofID, COMPENSATE_FOR_LESS_THAN_6_EVENTS ? avgWrsPoints : wrsPoints);
        }

        return kvps;
    }
}