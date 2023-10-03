using OTools.Common;
using OTools.Maps;

namespace OTools.Courses;


public class Item: IStorable
{
    public Guid Id => Object.Id;

    public Instance Object { get; set; }

    public List<Guid> VisibleCourses { get; set; }
    public bool ShowOnAllControls { get; set; }

    public Item(Instance obj, IEnumerable<Guid> visibleCourses, bool showOnAllCourses)
    {
        Object = obj;
        VisibleCourses = new(visibleCourses);
        ShowOnAllControls = showOnAllCourses;
    }
}