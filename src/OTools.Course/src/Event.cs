﻿using OTools.Common;
using OTools.CoursePlanner;
using OTools.Maps;

namespace OTools.Courses;

public sealed class Event
{
	public string Title { get; set; }
	
	public Metadata Metadata { get; set; }
	
	public CourseMap? Map { get; set; }
	public CourseSymbols Symbols { get; set; }

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

public sealed class CourseSymbols
{
    public PointSymbol Start { get; set; }
	public PointSymbol Finish { get; set; }
	public PointSymbol Control { get; set; }
	public PointSymbol CrossingPoint { get; set; }
	public PointSymbol Exchange { get; set; }

	public LineSymbol CourseLine { get; set; }
	public LineSymbol MarkedLine { get; set; }

	public SymbolStore Extras { get; set; }

    public CourseSymbols(PointSymbol start, PointSymbol finish, PointSymbol control, PointSymbol crossingPoint, PointSymbol exchange, LineSymbol courseLine, LineSymbol markedLine, SymbolStore? extras)
    {
        Start = start;
        Finish = finish;
        Control = control;
        CrossingPoint = crossingPoint;
        Exchange = exchange;

        CourseLine = courseLine;
        MarkedLine = markedLine;

        Extras = extras ?? new();
    }
}