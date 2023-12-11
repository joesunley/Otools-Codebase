using OTools.Common;

namespace OTools.StartTimeDistributor;

public class RankedStartTimes
{
    private List<Entry> _entries;
    private DayStartTimeParameters _parameters;
    private IList<(string, ushort)> _rankings;

    public RankedStartTimes(IEnumerable<Entry> entries, IEnumerable<(string, ushort)> rankings, DayStartTimeParameters parameters)
    {
        _entries = entries.ToList();
        _rankings = rankings.ToList();
        _parameters = parameters;
    }

    public Dictionary<Entry, DateTime> Create()
    {
        FilterRankings(ref _rankings, _entries);
        OrderRankings(ref _rankings);

        Dictionary<Entry, DateTime> startTimes = new();

        for (int group = 0; group < _parameters.Parameters.Length; group++)
        {
            if (_parameters.Parameters[group].AssociationType != "ranked")
                continue;

            foreach (byte course in _parameters.CourseGroupings[group])
            {
                var tCourse = CreateForCourse(course, _parameters.Parameters[group]);

                foreach (var e in tCourse)
                    startTimes.Add(e.Item1, e.Item2);
            }
        }

        return startTimes;
    }

    private IEnumerable<(Entry, DateTime)> CreateForCourse(byte course, GroupParameters parameters)
    {
        Dictionary<string, Entry> lookup = new(
            _entries.Where(x => x.Course == course)
                    .Select(x => new KeyValuePair<string, Entry>(x.RankingKey, x))     
            );

        List<(Entry, DateTime)> unshuffledStartTimes = new();
        List<(Entry, DateTime)> nonRankedTimes = new();
        var nonRankedSlice = _entries.Where(x => x.RankingKey == "");

        DateTime currentTime = ParseDateTime(parameters.FirstStart);

        foreach (var entry in nonRankedSlice)
        {
            nonRankedTimes.Add((entry, currentTime));
            currentTime += TimeSpan.FromMinutes(parameters.StartInterval);
        }

        foreach (string ranking in _rankings.Select(x => x.Item1))
        {
            if (!lookup.Keys.Contains(ranking))
                continue;

            unshuffledStartTimes.Add((lookup[ranking], currentTime));

            currentTime += TimeSpan.FromMinutes(parameters.StartInterval);

            if (currentTime > ParseDateTime(parameters.LastStart))
                throw new Exception("Too many entries");
        }

        if (parameters.Grouping == -1)
            return Join(nonRankedTimes, unshuffledStartTimes);

        (Entry, DateTime)[] startTimes = unshuffledStartTimes.ToArray();

        for (int i = 0; i < unshuffledStartTimes.Count; i += parameters.Grouping)
        {
            if (i+4 > unshuffledStartTimes.Count)
                continue;

            var range = startTimes[i..(i + 4)];
            range = range.Shuffle().ToArray();

            for (int j = i; j <= i + 4; j++)
                startTimes[j] = range[j - i];
        }

        return Join(nonRankedTimes, startTimes);
    }

    private static void FilterRankings(ref IList<(string, ushort)> rankings, IEnumerable<Entry> entries)
    {
        rankings = rankings.Where(x => entries.Select(y => y.RankingKey).Contains(x.Item1)).ToList();
    }
    private static void OrderRankings(ref IList<(string, ushort)> rankings)
    {
        rankings = rankings.OrderByDescending(x => x.Item2).ToList();
    }

    private static DateTime ParseDateTime(string input)
    {
        return DateTime.Parse(input, null, System.Globalization.DateTimeStyles.RoundtripKind);
    }

    private static IEnumerable<T> Join<T>(IEnumerable<T> a, IEnumerable<T> b)
        => a.Concat(b);
}


public static class WorldRanking
{
private const bool COMPENSATE_FOR_LESS_THAN_6_EVENTS = false;

public static Dictionary<string, ushort> FromCSV(string filePath)
{
    Dictionary<string, ushort> kvps = new();

    string[] lines = File.ReadAllLines(filePath);

    foreach (string line in lines.Skip(1))
    {
        string[] cells = line.Split(';');

        string iofID = cells[0];
        ushort wrsPoints = ushort.Parse(cells[5]);
        ushort avgWrsPoints = ushort.Parse(cells[7]);

        kvps.Add(iofID, COMPENSATE_FOR_LESS_THAN_6_EVENTS 
            ? avgWrsPoints : wrsPoints);
    }

    return kvps;
}
}