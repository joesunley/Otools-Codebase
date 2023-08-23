using OTools.Common;

namespace OTools.Events;

public sealed class Result : IStorable
{
    public Guid Id { get; set; }

    public Entry? Entry { get; set; }

    public List<Punch> Punches { get; set; } 

    public Result()
    {
        Id = Guid.NewGuid();
        Punches = new();
    }

    public Result(List<Punch> punches)
    {
        Id = Guid.NewGuid();
        Punches = punches;
    }

    public Result(Guid id, List<Punch> punches)
    {
        Id = id;
        Punches = punches;
    }
}

//public struct Punch
//{
//    public ushort Code { get; set; }
//    public DateTime TimeStamp { get; set; }
//}


public static class ResultFunctions
{
    public static TimeSpan GetResultTime(this Result result)
    {
        return result.Punches[^1].TimeStamp - result.Punches[0].TimeStamp;
    }

    public static IEnumerable<TimeSpan> GetSplitTimes(this Result result)
    {
        for (int i = 1; i < result.Punches.Count; i++)
            yield return result.Punches[i].TimeStamp - result.Punches[i - 1].TimeStamp;
    }

    public static IEnumerable<TimeSpan> GetElapsedTimes(this Result result)
    {
        for (int i = 0; i < result.Punches.Count; i++)
            yield return result.Punches[i].TimeStamp - result.Punches[0].TimeStamp;
    }

    public static IEnumerable<(TimeSpan, TimeSpan)> GetSplitElapsedTimes(this Result result)
    {
        IEnumerable<TimeSpan> splits = result.GetSplitTimes(),
            elapsed = result.GetElapsedTimes();

        Assert(splits.Count() == elapsed.Count());

        return splits.Zip(elapsed, (split, elapsed) => (split, elapsed));
    }
}