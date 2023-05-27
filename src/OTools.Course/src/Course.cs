using OTools.Common;
using OTools.Maps;

namespace OTools.Courses;

public abstract class Course : IStorable
{
    protected readonly Event _parent;
    public Guid Id { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
    public string DisplayFormat { get; set; }

    protected Course(Event parent, string name, string description, string displayFormat)
    {
        _parent = parent;
        Id = Guid.NewGuid();

        Name = name;
        Description = description;
        DisplayFormat = displayFormat;
    }

    protected Course(Event parent, Guid id, string name, string description, string displayFormat)
    {
        _parent = parent;
        Id = id;

        Name = name;
        Description = description;
        DisplayFormat = displayFormat;
    }
}

/*
 * Display Format:
 *
 * %n -> Control Number
 * %c -> Control Code
 * %s -> Control Score (score course only)
 */

public sealed class LinearCourse : Course
{
    public float? Distance { get; set; }
    public uint? Climb { get; set; }

    public List<ICoursePart> Parts { get; set; } // Maybe change to CombinedCoursePart?

    public LinearCourse(Event parent, string name, string description, string displayFormat, IEnumerable<ICoursePart> parts, float? distance = null, uint? climb = null)
        : base(parent, name, description, displayFormat)
    {
        Distance = distance;
        Climb = climb;
        Parts = new(parts);
    }
    public LinearCourse(Event parent, Guid id, string name, string description, string displayFormat, IEnumerable<ICoursePart> parts, float? distance = null, uint? climb = null)
        : base(parent, id, name, description, displayFormat)
    {
        Distance = distance;
        Climb = climb;
        Parts = new(parts);
    }
}

public sealed class ScoreCourse : Course
{
    public List<Control> Controls { get; set; }
    public List<float> Scores { get; set; }

    public ScoreCourse(Event parent, string name, string description, string displayFormat, IEnumerable<Control> controls, IEnumerable<float> scores)
        : base(parent, name, description, displayFormat)
    {
        Controls = new(controls);
        Scores = new(scores);
    }

    public ScoreCourse(Event parent, Guid id, string name, string description, string displayFormat, IEnumerable<Control> controls, IEnumerable<float> scores)
        : base(parent, id, name, description, displayFormat)
    {
        Controls = new(controls);
        Scores = new(scores);
    }

}