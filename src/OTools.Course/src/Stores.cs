using OTools.Maps;
using OTools.Common;

namespace OTools.Course;

public sealed class ControlStore : Store<Control>
{
    public ControlStore() { }

    public ControlStore(IEnumerable<Control> items)
        : base(items) { }
}

public sealed class CourseStore : Store<Course>
{
    public CourseStore() { }

    public CourseStore(IEnumerable<Course> items)
        : base(items) { }
}

public sealed class ItemStore : Store<Item>
{
    public ItemStore() { }

    public ItemStore(IEnumerable<Item> items)
        : base(items) { }
}