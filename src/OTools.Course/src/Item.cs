using OTools.Maps;

namespace OTools.Course;


public class Item: IStorable 
{
    public Guid Id => Object.Id;

    public Instance Object { get; set; }

    public Item(Instance obj)
    {
        Object = obj;
    }
}