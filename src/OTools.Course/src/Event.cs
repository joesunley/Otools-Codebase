namespace OTools.Course;

public sealed class Event
{
    public string Title { get; set; }

    public ControlStore Controls { get; set; }
    public CourseStore Courses { get; set; }
    public ItemStore Items { get; set; }
	
	public EventSettings Settings { get; set; }

    public Event(string title = "")
    {
        Title = title;

        Controls = new();
        Courses = new();
        Items = new();
		
		Settings = EventSettings.Default;
    }

    public Event(string title, ControlStore controls, CourseStore courses, ItemStore items, EventSettings? settings = null)
    {
        Title = title;

        Controls = controls;
        Courses = courses;
        Items = items;
		
		Settings = settings is null ? settings : EventSettings.Default;
	}
}