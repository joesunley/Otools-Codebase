using OneOf;
using OneOf.Types;
using OTools.Common;

namespace OTools.Events;

public sealed class Result : IStorable
{
    public Guid Id { get; set; }

    public OneOf<Entry, Unknown> Entry { get; set; }

    public List<Punch> Punches { get; set; } 

    public Result(OneOf<Entry, Unknown> entry, List<Punch>? punches)
    {
        Id = Guid.NewGuid();
        Entry = entry;
        Punches = punches ?? new();
    }

    public Result(Guid id, OneOf<Entry, Unknown> entry, List<Punch> punches)
    {
        Id = id;
        Entry = entry;
        Punches = punches;
    }
}