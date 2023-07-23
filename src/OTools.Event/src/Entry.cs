using OTools.Common;
using OTools.Courses;

namespace OTools.Events;

public class Entry : IStorable
{
    public Guid Id { get; init; }

    public string CardNo { get; set; }
    public Name Name { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string Club { get; set; }
    public string Notes { get; set; }
    public Course? Course { get; set; }
    public DateTime? StartTime { get; set; }

    public Entry()
    {
        Id = Guid.NewGuid();

        CardNo = string.Empty;
        Name = new(string.Empty, string.Empty);
        DateOfBirth = new(1900, 1, 1);
        Club = string.Empty;
        Notes = string.Empty;
        Course = null;
        StartTime = null;
    }

    public Entry(string cardNo, Name name, DateOnly dateOfBirth, string club, string notes, Course? course, DateTime? startTime)
    {
        Id = Guid.NewGuid();
        CardNo = cardNo;
        Name = name;
        DateOfBirth = dateOfBirth;
        Club = club;
        Notes = notes;
        Course = course;
        StartTime = startTime;
    }

    public Entry(Guid id, string cardNo, Name name, DateOnly dateOfBirth, string club, string notes, Course? course, DateTime? startTime)
    {
        Id = id;
        CardNo = cardNo;
        Name = name;
        DateOfBirth = dateOfBirth;
        Club = club;
        Notes = notes;
        Course = course;
        StartTime = startTime;
    }
}

public class BofEntry : Entry
{
    public string BofNumber { get; set; }

    public BofEntry()
    {
        BofNumber = string.Empty;
    }
    public BofEntry(string cardNo, Name name, DateOnly dateOfBirth, string club, string notes, Course? course, DateTime? startTime, string bofNumber)
        : base(cardNo, name, dateOfBirth, club, notes, course, startTime)
    {
        BofNumber = bofNumber;
    }

    public BofEntry(Guid id, string cardNo, Name name, DateOnly dateOfBirth, string club, string notes, Course? course, DateTime? startTime, string bofNumber)
        : base(id, cardNo, name, dateOfBirth, club, notes, course, startTime)
    {
        BofNumber = bofNumber;
    }
}

[Flags]
public enum EntryFlags
{
    Disqualified   = 0b01,
    NonCompetitive = 0b10,
}

