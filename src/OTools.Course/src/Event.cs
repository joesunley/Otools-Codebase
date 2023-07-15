using OTools.Common;
using OTools.CoursePlanner;
using OTools.Maps;

namespace OTools.Courses;

public sealed class Event
{
	public string Title { get; set; }
	
	public Metadata Metadata { get; set; }
	
	public CourseMap? Map { get; set; }
	public Map? SymbolMap { get; set; }

	public ControlStore Controls { get; set; }
	public CourseStore Courses { get; set; }
	public ItemStore Items { get; set; }
	
	public EventSettings Settings { get; set; }

	public Event(string title = "")
	{
		Title = title;

		Metadata = new();

		Controls = new();
		Courses = new();
		Items = new();
		
		Settings = EventSettings.Default;
	}

	public Event(string title, ControlStore controls, CourseStore courses, ItemStore items, EventSettings? settings = null)
	{
		Title = title;

		Metadata = new();

		Controls = controls;
		Courses = courses;
		Items = items;

		Settings = settings ?? EventSettings.Default;
	}
}