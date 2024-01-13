using FuzzySharp;
using OTools.StartTimeDistributor;

namespace OTools.StartTimeGenerator;

public class StartTimeEditor
{
    private Dictionary<Entry, DateTime> _startTimes;

    public StartTimeEditor(Dictionary<Entry, DateTime> startTimes)
    {
        _startTimes = startTimes;
    }

    public (Entry, DateTime)[] FuzzySearch(string input)
    {
        var results = Process.ExtractSorted(input, _startTimes.Keys.Select(x => x.Name), cutoff: 10).ToDictionary(x => x.Value, x => x.Index);

        var a = _startTimes.Where(x => results.ContainsKey(x.Key.Name))
            .Select(x => (x, results[x.Key.Name]));

        return a.OrderBy(x => x.Item2).Select(x => (x.Item1.Key, x.Item1.Value)).ToArray();
    }

    public Dictionary<Entry, DateTime> Out() { return _startTimes; }
}