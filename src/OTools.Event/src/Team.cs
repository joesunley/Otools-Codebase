using OTools.Common;

namespace OTools.Events;

public class Team : IStorable
{
    public Guid Id { get; private set; }

    public List<Entry> Members { get; set; }

    public bool AllowReruns { get; set; } // Allow the same runner to run 2 legs

    public Team(IEnumerable<Entry> members, bool allowReruns)
    {
        Id = Guid.NewGuid();

        Members = new(members);
        AllowReruns = allowReruns;
    }

    public Team(Guid id, IEnumerable<Entry> members, bool allowReruns)
    {
        Id = id;
        Members = new(members);
        AllowReruns = allowReruns;
    }
}