using OTools.Common;

namespace OTools.StartTimeDistributor;

public class SimpleStartTimes
{
    private List<Entry> _entries;
    private DayStartTimeParameters _parameters;

    public SimpleStartTimes(IEnumerable<Entry> entries, DayStartTimeParameters parameters)
    {
        _entries = entries.ToList();
        _parameters = parameters;
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
                var tCourse = CreateForCourse(course, _parameters.Parameters[group]);

                foreach (var e in tCourse)
                    startTimes.Add(e.Item1, e.Item2);
            }
        }

        return startTimes;
    }

    private IEnumerable<(Entry, DateTime)> CreateForCourse(byte course, GroupParameters parameters)
    {
        throw new NotImplementedException();
    }
}