using OTools.Common;
using OTools.Maps;

namespace OTools.Courses;


public class Item: IStorable 
{
    public Guid Id => Object.Id;

    public Instance Object { get; set; }

    public Item(Instance obj)
    {
        Object = obj;
    }
}