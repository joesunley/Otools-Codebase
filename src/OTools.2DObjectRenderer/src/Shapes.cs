using OTools.Maps;
using Sunley.Mathematics;
using OneOf;

namespace OTools.ObjectRenderer2D;

public interface IShape
{
    vec2 TopLeft { get; set; }
    float Opacity { get; set; }
    int ZIndex { get; set; }
}

public struct Circle : IShape
{
    public vec2 TopLeft { get; set; }

    public float Opacity { get; set; }

    public float Diameter { get; set; }

    public uint Fill { get; set; }

    public float BorderWidth { get; set; }
    public uint BorderColour { get; set; }

    public IEnumerable<double> DashArray { get; set; }

    public int ZIndex { get; set; }

    public Circle()
    {
        TopLeft = vec2.Zero;

        Opacity = 1f;

        Diameter = 0f;

        Fill = Colour.Transparent;

        BorderWidth = 0f;
        BorderColour = Colour.Transparent;

        DashArray = Enumerable.Empty<double>();

        ZIndex = 0;
    }
}

public struct Line : IShape
{
    public vec2 TopLeft { get; set; }

    public float Opacity { get; set; }

    public List<vec2> Points { get; set; }

    public float Width { get; set; }
    public uint Colour { get; set; }

    public IEnumerable<double> DashArray { get; set; }

    public int ZIndex { get; set; }

    public Line()
    {
		TopLeft = vec2.Zero;

        Opacity = 1f;
        Points = new();

        Width = 0f;
        Colour = Maps.Colour.Transparent;

        DashArray = Enumerable.Empty<double>();

        ZIndex = 0;
    }
}

public struct Area : IShape
{
    public vec2 TopLeft { get; set; }

    public float Opacity { get; set; }

    public List<vec2> Points { get; set; }

    public uint Fill { get; set; }

    public bool IsClosed { get; set; }
    public float BorderWidth { get; set; }
    public uint BorderColour { get; set; }

    public IEnumerable<double> DashArray { get; set; }

    public int ZIndex { get; set; }

    public Area()
    {
        TopLeft = vec2.Zero;

        Opacity = 1f;

        Points = new();

        Fill = Colour.Transparent;

        IsClosed = true;
        BorderWidth = 0f;
        BorderColour = Colour.Transparent;

        DashArray = Enumerable.Empty<double>();

        ZIndex = 0;
    }
}

public struct Path : IShape
{
    public vec2 TopLeft { get; set; }

    public float Opacity { get; set; }

    public List<IPathSegment> Segments { get; set; }
    public List<IPathSegment[]> Holes { get; set; }

    public OneOf<VisualFill, uint>? Fill { get; set; }

    public bool IsClosed { get; set; }
    public float BorderWidth { get; set; }
    public uint BorderColour { get; set; }

    public IEnumerable<double> DashArray { get; set; }

    public int ZIndex { get; set; }

    public Path()
    {
        TopLeft = vec2.Zero;

        Opacity = 1f;

        Segments = new();
        Holes = new();

        Fill = null;

        IsClosed = false;
        BorderWidth = 0f;
        BorderColour = Colour.Transparent;

        DashArray = Enumerable.Empty<double>();

        ZIndex = 0;
    }
}

public struct Rectangle : IShape
{
    public vec2 TopLeft { get; set; }

    public float Opacity { get; set; }

    public vec2 Size { get; set; }

    public uint Fill { get; set; }

    public float BorderWidth { get; set; }
    public uint BorderColour { get; set; }

    public IEnumerable<double> DashArray { get; set; }

    public int ZIndex { get; set; }

    public Rectangle()
    {
        TopLeft = vec2.Zero;

        Opacity = 1f;

        Size = vec2.Zero;

        Fill = Colour.Transparent;

        BorderWidth = 0f;
        BorderColour = Colour.Transparent;

        DashArray = Enumerable.Empty<double>();

        ZIndex = 0;
    }
}

public interface IPathSegment { }

public struct PolyBezierSegment : IPathSegment
{
    public List<BezierPoint> Points { get; set; }

    public PolyBezierSegment()
    {
        Points = new();
    }
}

public struct PolyLineSegment : IPathSegment
{
    public List<vec2> Points { get; set; }

    public PolyLineSegment()
    {
        Points = new();
    }
}

public struct Text : IShape
{
    public vec2 TopLeft { get; set; }

    public string Content { get; set; }

    public Font Font { get; set; }

    public HorizontalAlignment HorizontalAlignment { get; set; }

    public (Colour colour, float width) Border { get; set; }

    public (Colour colour, float width) Framing { get; set; }

    public Colour Fill { get; set; }

    public float Opacity { get; set; }

    public int ZIndex { get; set; }
}

public struct VisualFill
{
    public List<IShape> Shapes { get; set; }

    public vec4 Viewport { get; set; }
    public vec2 TopLeft { get; set; }

    public VisualFill()
    {
        Shapes = new();
        Viewport = vec4.Zero;
        TopLeft = vec2.Zero;
    }
}