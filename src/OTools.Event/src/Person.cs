using OneOf;
using OTools.Common;
using OTools.Courses;

namespace OTools.Events;

public class Person : IStorable
{
    public Guid Id { get; init; }

    public string CardNo { get; set; }
    public Name Name { get; set; }
    public DateOnly DateOfBirth { get; set; }
    public string Club { get; set; }
    public string Notes { get; set; }
    public Course? Course { get; set; }
    public OneOf<DateTime, StartTimeType> StartTime { get; set; }


    public Person(string cardNo, Name name, DateOnly dateOfBirth, string club, string notes, Course? course, OneOf<DateTime, StartTimeType> startTime)
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

    public Person(Guid id, string cardNo, Name name, DateOnly dateOfBirth, string club, string notes, Course? course, OneOf<DateTime, StartTimeType> startTime)
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

public class BofEntry : Person
{
    public string BofNumber { get; set; }

    public BofEntry(string cardNo, Name name, DateOnly dateOfBirth, string club, string notes, Course? course, OneOf<DateTime, StartTimeType> startTime, string bofNumber)
        : base(cardNo, name, dateOfBirth, club, notes, course, startTime)
    {
        BofNumber = bofNumber;
    }

    public BofEntry(Guid id, string cardNo, Name name, DateOnly dateOfBirth, string club, string notes, Course? course, OneOf<DateTime, StartTimeType> startTime, string bofNumber)
        : base(id, cardNo, name, dateOfBirth, club, notes, course, startTime)
    {
        BofNumber = bofNumber;
    }
}

[Flags]
public enum EntryFlag
{
    Disqualified   = 0b01,
    NonCompetitive = 0b10,
}

public enum StartTimeType
{
    Punching,
    MassStart,
}