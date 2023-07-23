using OTools.Common;
using System.Collections;

namespace OTools.Events;

public sealed class Result : IStorable
{
    public Guid Id { get; set; }

    public Entry Entry { get; set; }

    public List<Split> Legs { get; set; } 

    public Result()
    {
        Id = Guid.NewGuid();
        Legs = new();
    }

    public Result(List<Split> legs)
    {
        Id = Guid.NewGuid();
        Legs = legs;
    }

    public Result(Guid id, List<Split> legs)
    {
        Id = id;
        Legs = legs;
    }
}

public struct Split
{
    public ushort Code { get; set; }
    public DateTime RealTime { get; set; }
}


public static class ResultFunctions
{
    public static TimeSpan GetResultTime(this Result result)
    {
        return result.Legs[^1].RealTime - result.Legs[0].RealTime;
    }

    public static IEnumerable<TimeSpan> GetSplitTimes(this Result result)
    {
        for (int i = 1; i < result.Legs.Count; i++)
            yield return result.Legs[i].RealTime - result.Legs[i - 1].RealTime;
    }

    public static IEnumerable<TimeSpan> GetElapsedTimes(this Result result)
    {
        for (int i = 0; i < result.Legs.Count; i++)
            yield return result.Legs[i].RealTime - result.Legs[0].RealTime;
    }

    public static IEnumerable<(TimeSpan, TimeSpan)> GetSplitElapsedTimes(this Result result)
    {
        IEnumerable<TimeSpan> splits = result.GetSplitTimes(),
            elapsed = result.GetElapsedTimes();

        Assert(splits.Count() == elapsed.Count());

        return splits.Zip(elapsed, (split, elapsed) => (split, elapsed));
    }
}