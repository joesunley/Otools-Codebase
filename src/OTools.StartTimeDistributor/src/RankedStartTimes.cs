using OTools.Common;

namespace OTools.StartTimeDistributor;

public class RankedStartTimes
{
    private List<Entry> _entries;
    private DayStartTimeParameters _parameters;
    private IList<(string, float)> _rankings;
    private int _day;

    public RankedStartTimes(IEnumerable<Entry> entries, IEnumerable<(string, float)> rankings, DayStartTimeParameters parameters, int day)
    {
        _entries = entries.ToList();
        _rankings = rankings.ToList();
        _parameters = parameters;
        _day = day;
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
                var tCourse = (_parameters.Parameters[group].Allocation == "start") ?
                        CreateForCourseAtStart(course, _parameters.Parameters[group]) :
                        CreateForCourseAtEnd(course, _parameters.Parameters[group]);

                foreach (var e in tCourse)
                    startTimes.Add(e.Item1, e.Item2);
            }
        }

        return startTimes;
    }
    private IEnumerable<(Entry, DateTime)> CreateForCourseAtStart(byte course, GroupParameters parameters)
    {
        Dictionary<string, Entry> lookup = new(
            _entries.Where(x => x.Days[_day].Course == course && x.RankingKey != "")
                    .Where(x => _rankings.Select(x => x.Item1).Contains(x.RankingKey))
                    .Select(x => new KeyValuePair<string, Entry>(x.RankingKey, x))
            );

        List<(Entry, DateTime)> unshuffledStartTimes = new();
        List<(Entry, DateTime)> nonRankedTimes = new();
        var nonRankedSlice = _entries.Where(x => x.Days[_day].Course == course && !lookup.ContainsKey(x.RankingKey)).ToArray();

        DateTime currentTime = parameters.FirstStart;

        if (parameters.CourseSpacing == 0)
            parameters.CourseSpacing = parameters.StartInterval;
        if (parameters.ClassSpacing == 0)
            parameters.ClassSpacing = parameters.StartInterval;
        if (parameters.ClubSpacing == 0)
            parameters.ClubSpacing = parameters.StartInterval;

        foreach (var entry in nonRankedSlice.Shuffle())
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

            if (currentTime > parameters.LastStart)
                throw new Exception("Too many entries");
        }

        if (parameters.Grouping <= 0)
            return Join(nonRankedTimes, unshuffledStartTimes);

        (Entry, DateTime)[] startTimes = unshuffledStartTimes.ToArray();

        for (int i = 0; i < startTimes.Length; i += parameters.Grouping)
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

    private IEnumerable<(Entry, DateTime)> CreateForCourseAtEnd(byte course, GroupParameters parameters)
    {
        Dictionary<string, Entry> lookup = new(
            _entries.Where(x => x.Days[_day].Course == course && x.RankingKey != "")
                    .Where(x => _rankings.Select(x => x.Item1).Contains(x.RankingKey))
                    .Select(x => new KeyValuePair<string, Entry>(x.RankingKey, x))     
            );

        List<(Entry, DateTime)> unshuffledStartTimes = new();
        List<(Entry, DateTime)> nonRankedTimes = new();
        var nonRankedSlice = _entries.Where(x => x.Days[_day].Course == course && !lookup.ContainsKey(x.RankingKey)).ToArray();

        DateTime currentTime = parameters.LastStart;

        if (parameters.CourseSpacing == 0)
            parameters.CourseSpacing = parameters.StartInterval;
        if (parameters.ClassSpacing == 0)
            parameters.ClassSpacing = parameters.StartInterval;
        if (parameters.ClubSpacing == 0)
            parameters.ClubSpacing = parameters.StartInterval;

        foreach (var entry in nonRankedSlice.Shuffle())
        {
            nonRankedTimes.Add((entry, currentTime));
            currentTime += TimeSpan.FromMinutes(parameters.StartInterval);
        }

        foreach (string ranking in _rankings.Select(x => x.Item1))
        {
            if (!lookup.Keys.Contains(ranking))
                continue;

            unshuffledStartTimes.Add((lookup[ranking], currentTime));

            currentTime -= TimeSpan.FromMinutes(parameters.StartInterval);

            if (currentTime > parameters.LastStart)
                throw new Exception("Too many entries");
        }

        if (parameters.Grouping <= 0)
            return Join(nonRankedTimes, unshuffledStartTimes);

        (Entry, DateTime)[] startTimes = unshuffledStartTimes.ToArray();

        for (int i = 0; i < startTimes.Length; i += parameters.Grouping)
        {
            if (i+4 >= startTimes.Length)
                continue;

            var range = startTimes[i..(i + 4)];
            range = range.Shuffle().ToArray();

            for (int j = i; j < i + 4; j++)
                startTimes[j] = range[j - i];
        }

        return Join(nonRankedTimes, startTimes);
    }

    private static void FilterRankings(ref IList<(string, float)> rankings, IEnumerable<Entry> entries)
    {
        rankings = rankings.Where(x => entries.Select(y => y.RankingKey).Contains(x.Item1)).ToList();
    }
    private static void OrderRankings(ref IList<(string, float)> rankings)
    {
        rankings = rankings.OrderBy(x => x.Item2).ToList();
    }

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