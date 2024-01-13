using OTools.Common;
using System.Runtime.InteropServices;

namespace OTools.StartTimeDistributor;

public class SimpleStartTimes
{
    private List<Entry> _entries;
    private GroupParameters _parameters;
    private int[] _courses;

    public SimpleStartTimes(IEnumerable<Entry> entries, GroupParameters parameters, int[] courses)
    {
        _entries = entries.ToList();
        _parameters = parameters;
        _courses = courses;
    }

    public Dictionary<Entry, DateTime> Create()
    {
        Dictionary<Entry, DateTime> startTimes = new();

        if (_parameters.AssociationType != "simple")
            throw new ArgumentException();

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
        Dictionary<DateTime, Entry> startList = new();

        var entries = _entries.Shuffle()
                              .Where(x => x.Course == course)
                              .ToList();

        DateTime currentTime = _parameters.LastStart;
        StartTimeBlock currentBlock = StartTimeBlock.Late;

        while (entries.Count > 0 && currentTime > _parameters.FirstStart)
        {
            Entry? successful = null;
            var selectFrom = entries.Where(x => x.Preference == currentBlock || x.Preference == StartTimeBlock.Open).ToArray();

            if (selectFrom.Length == 0)
            {
                currentBlock -= 1;
                continue;
            }

            int curr = -1;
            while (curr < selectFrom.Length - 1)
            {
                curr++;

                var selected = selectFrom[curr];
                bool isFailed = false;

                // Course Spacing
                DateTime checkTime = currentTime + TimeSpan.FromMinutes(_parameters.CourseSpacing - _parameters.StartInterval);
                while (checkTime > currentTime && !isFailed)
                {
                    if (startList.TryGetValue(checkTime, out Entry entry) && selected.Class == entry.Class)
                        isFailed = true;
                    checkTime -= TimeSpan.FromMinutes(_parameters.StartInterval);
                }

                // Class Spacing
                checkTime = currentTime + TimeSpan.FromMinutes(_parameters.ClassSpacing - _parameters.StartInterval);
                while (checkTime > currentTime && !isFailed)
                {
                    if (startList.TryGetValue(checkTime, out Entry entry) && selected.Class == entry.Class)
                        isFailed = true;
                    checkTime -= TimeSpan.FromMinutes(_parameters.StartInterval);
                }

                // Club Spacing
                checkTime = currentTime + TimeSpan.FromMinutes(_parameters.ClubSpacing - _parameters.StartInterval);
                while (checkTime > currentTime && !isFailed)
                {
                    if (startList.TryGetValue(checkTime, out Entry entry) && selected.Club == entry.Club)
                        isFailed = true;
                    checkTime -= TimeSpan.FromMinutes(_parameters.StartInterval);
                }

                if (!isFailed)
                {
                    successful = selected;
                    break;
                }
            }

            if (successful.HasValue)
            {
                startList.Add(currentTime, successful.Value);
                entries.Remove(successful.Value);
            }

            currentTime -= TimeSpan.FromMinutes(_parameters.StartInterval);
        }

        return startList.Select(x => (x.Value, x.Key));
    }
}