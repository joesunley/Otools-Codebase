using OTools.Common;
using OTools.Events;

namespace OTools.StartTimeDistributor;

public class SimpleStartTimes
{
    private List<Entry> _entries;
    private DayStartTimeParameters _parameters;
    private int _day;

    public SimpleStartTimes(IEnumerable<Entry> entries, DayStartTimeParameters parameters, int day)
    {
        _entries = entries.ToList();
        _parameters = parameters;
        _day = day;
    }

    public Dictionary<Entry, DateTime> Create()
    {
        Dictionary<Entry, DateTime> startTimes = new();

        for (int group = 0; group < _parameters.Parameters.Length; group++)
        {
            if (_parameters.Parameters[group].AssociationType != "simple")
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

    private IEnumerable<(Entry, DateTime)> CreateForCourseAtEnd(byte course, GroupParameters parameters)
    {
        Dictionary<DateTime, Entry> startList = new();

        var entries = _entries.Shuffle()
                              .Where(x => x.Days[_day].Course == course)
                              .ToList();

        DateTime currentTime = parameters.LastStart;
        StartTimeBlock currentBlock = StartTimeBlock.Late;

        if (parameters.CourseSpacing == 0)
            parameters.CourseSpacing = parameters.StartInterval;
        if (parameters.ClassSpacing == 0)
            parameters.ClassSpacing = parameters.StartInterval;
        if (parameters.ClubSpacing == 0)
            parameters.ClubSpacing = parameters.StartInterval;

        while (entries.Count > 0 && currentTime > parameters.FirstStart)
        {
            // Check if at the start of excluded block
            foreach (var block in parameters.ExcludedBlocks)
            {
                if (block.BlockEnd == currentTime)
                    currentTime = block.BlockStart;
                break;
            }

            Entry? successful = null;
            var selectFrom = entries.Where(x => x.Days[_day].Preference == currentBlock || x.Days[_day].Preference == StartTimeBlock.Open).ToArray();

            if (selectFrom.Length == 0)
            {
                currentBlock -= 1;
                continue;
            }

            int curr = -1;
            while (curr < selectFrom.Length -1)
            {
                curr++;

                var selected = selectFrom[curr];
                bool isFailed = false;

                // Course Spacing
                DateTime checkTime = currentTime + TimeSpan.FromMinutes(parameters.CourseSpacing - parameters.StartInterval);
                while (checkTime > currentTime && !isFailed)
                {
                    if (startList.TryGetValue(checkTime, out Entry entry) && selected.Days[_day].Class == entry.Days[_day].Class)
                        isFailed = true;
                    checkTime -= TimeSpan.FromMinutes(parameters.StartInterval);
                }

                // Class Spacing
                checkTime = currentTime + TimeSpan.FromMinutes(parameters.ClassSpacing - parameters.StartInterval);
                while (checkTime > currentTime && !isFailed)
                {
                    if (startList.TryGetValue(checkTime, out Entry entry) && selected.Days[_day].Class == entry.Days[_day].Class)
                        isFailed = true;
                    checkTime -= TimeSpan.FromMinutes(parameters.StartInterval);
                }

                // Club Spacing
                checkTime = currentTime + TimeSpan.FromMinutes(parameters.ClubSpacing - parameters.StartInterval);
                while (checkTime > currentTime && !isFailed)
                {
                    if (startList.TryGetValue(checkTime, out Entry entry) && selected.Club == entry.Club)
                        isFailed = true;
                    checkTime -= TimeSpan.FromMinutes(parameters.StartInterval);
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

            currentTime -= TimeSpan.FromMinutes(parameters.StartInterval);
        }

        return startList.Select(x => (x.Value, x.Key));
    }

    private IEnumerable<(Entry, DateTime)> CreateForCourseAtStart(byte course, GroupParameters parameters)
    {
        Dictionary<DateTime, Entry> startList = new();

        var entries = _entries.Shuffle()
                              .Where(x => x.Days[_day].Course == course)
                              .ToList();

        DateTime currentTime = parameters.FirstStart;
        StartTimeBlock currentBlock = StartTimeBlock.Early;

        if (parameters.CourseSpacing == 0)
            parameters.CourseSpacing = parameters.StartInterval;
        if (parameters.ClassSpacing == 0)
            parameters.ClassSpacing = parameters.StartInterval;
        if (parameters.ClubSpacing == 0)
            parameters.ClubSpacing = parameters.StartInterval;

        while (entries.Count > 0 && currentTime < parameters.LastStart)
        {
            // Check if at the start of excluded block
            foreach (var block in parameters.ExcludedBlocks)
            {
                if (block.BlockStart == currentTime)
                    currentTime = block.BlockEnd; 
                break;
            }

            Entry? successful = null;
            var selectFrom = entries.Where(x => x.Days[_day].Preference == currentBlock || x.Days[_day].Preference == StartTimeBlock.Open).ToArray();

            if (selectFrom.Length == 0)
            {
                currentBlock += 1;
                continue;
            }

            int curr = -1;
            while (curr < selectFrom.Length - 1)
            {
                curr++;

                var selected = selectFrom[curr];
                bool isFailed = false;

                // Course Spacing
                DateTime checkTime = currentTime - TimeSpan.FromMinutes(parameters.CourseSpacing - parameters.StartInterval);
                while (checkTime < currentTime && !isFailed)
                {
                    if (startList.TryGetValue(checkTime, out Entry entry) && selected.Days[_day].Class == entry.Days[_day].Class)
                        isFailed = true;
                    checkTime += TimeSpan.FromMinutes(parameters.StartInterval);
                }

                // Class Spacing
                checkTime = currentTime + TimeSpan.FromMinutes(parameters.ClassSpacing - parameters.StartInterval);
                while (checkTime < currentTime && !isFailed)
                {
                    if (startList.TryGetValue(checkTime, out Entry entry) && selected.Days[_day].Class == entry.Days[_day].Class)
                        isFailed = true;
                    checkTime += TimeSpan.FromMinutes(parameters.StartInterval);
                }

                // Club Spacing
                checkTime = currentTime + TimeSpan.FromMinutes(parameters.ClubSpacing - parameters.StartInterval);
                while (checkTime  < currentTime && !isFailed)
                {
                    if (startList.TryGetValue(checkTime, out Entry entry) && selected.Club == entry.Club)
                        isFailed = true;
                    checkTime += TimeSpan.FromMinutes(parameters.StartInterval);
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

            currentTime += TimeSpan.FromMinutes(parameters.StartInterval);
        }

        return startList.Select(x => (x.Value, x.Key));
    }
}