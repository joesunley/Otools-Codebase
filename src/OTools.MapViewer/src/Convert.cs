using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using OTools.Maps;
using Sunley.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

using Ava = Avalonia.Controls.Shapes;
using OT = OTools.ObjectRenderer2D;

namespace OTools.MapViewer;

internal static class Convert
{
    public static IEnumerable<Shape> ConvertCollection(IEnumerable<OT.IShape> shapes)
	{
		return shapes.Select(el => (Shape)(el switch
		{
			OT.Rectangle r => ConvRectange(r),
			OT.Ellipse e => ConvEllipse(e),
			OT.Line l => ConvLine(l),
			OT.Area a => ConvArea(a),
			OT.Path p => ConvPath(p),
			_ => throw new NotImplementedException(),
		}));
	}

    private static Rectangle ConvRectange(OT.Rectangle rect)
    {
        Rectangle output = new()
        {
            Opacity = rect.Opacity,
            ZIndex = rect.ZIndex,

            Width = rect.Size.X,
            Height = rect.Size.Y,

            Fill = ColourToBrush(rect.Fill),

            Stroke = ColourToBrush(rect.BorderColour),
            StrokeThickness = rect.BorderWidth,
            StrokeDashArray = new(rect.DashArray),
        };

        output.SetTopLeft(rect.TopLeft);

        return output;
    }

    private static Ellipse ConvEllipse(OT.Ellipse ellipse)
    {
        Ellipse output = new()
        {
            Opacity = ellipse.Opacity,
            ZIndex = ellipse.ZIndex,

            Width = ellipse.Size.X,
            Height = ellipse.Size.Y,

            Fill = ColourToBrush(ellipse.Fill),

            Stroke = ColourToBrush(ellipse.BorderColour),
            StrokeThickness = ellipse.BorderWidth,
            StrokeDashArray = new(ellipse.DashArray),
        };
		
        output.SetTopLeft(ellipse.TopLeft);

		return output;
    }

    private static Polyline ConvLine(OT.Line line)
    {
        Polyline output = new()
        {
            Opacity = line.Opacity,
            ZIndex = line.ZIndex,

            Points = line.Points.Select(ToPoint).ToList(),

            Stroke = ColourToBrush(line.Colour),
            StrokeThickness = line.Width,
            StrokeDashArray = new(line.DashArray),
        };

        output.SetTopLeft(line.TopLeft);

        return output;
    }

    private static Polygon ConvArea(OT.Area area)
    {
        Polygon output = new()
        {
            Opacity = area.Opacity,
            ZIndex = area.ZIndex,

            Points = area.Points.Select(ToPoint).ToList(),

            Fill = ColourToBrush(area.Fill),

            Stroke = ColourToBrush(area.BorderColour),
            StrokeThickness = area.BorderWidth,
            StrokeDashArray = new(area.DashArray),
        };

        output.SetTopLeft(area.TopLeft);

        return output;
    }

    private static Path ConvPath(OT.Path path)
    {
        PathFigure pathFig = ConvPathFigure(path.Segments, path.IsClosed);

        PathFigures holeFigs = new();
        holeFigs.AddRange(path.Holes.Select(hole => ConvPathFigure(hole, true)));

        PathGeometry outlineGeom = new() { Figures = new() { pathFig } };
        PathGeometry holeGeom = new() { Figures = holeFigs, FillRule = FillRule.NonZero };

        CombinedGeometry geom = new(GeometryCombineMode.Exclude, outlineGeom, holeGeom);

        Path output = new()
        {
            Opacity = path.Opacity,
            ZIndex = path.ZIndex,

            Data = geom,


			Fill = path.Fill is not null ? ConvVisualFill(path.Fill.Value!) : null,

            Stroke = ColourToBrush(path.BorderColour),
            StrokeThickness = path.BorderWidth,
            StrokeDashArray = new(path.DashArray),
        };

        output.SetTopLeft(path.TopLeft);

        return output;
    }

    private static PathFigure ConvPathFigure(IList<OT.IPathSegment> segments, bool isClosed)
    {
        PathFigure fig = new() { IsClosed = isClosed };
        PathSegments segs = new();


        switch (segments[0])
        {
            case OT.PolyLineSegment sSeg:
            {
                PolyLineSegment startSeg = new();

                fig.StartPoint = sSeg.Points[0].ToPoint();
                //startSeg.Points = new(sSeg.Points.Skip(1).Select(ToPoint));

                startSeg.Points.AddRange(sSeg.Points.Skip(1).Select(ToPoint));

                segs.Add(startSeg);
            }
            break;
            case OT.PolyBezierSegment bSeg:
            {
                fig.StartPoint = bSeg.Points[0].EarlyControl.ToPoint();

                for (int i = 1; i < bSeg.Points.Count; i++)
                {
                    BezierPoint early = bSeg.Points[i - 1],
                        late = bSeg.Points[i];

                    BezierSegment seg = new()
                    {
                        Point1 = early.LateControl.ToPoint(),
                        Point2 = late.EarlyControl.ToPoint(),
                        Point3 = late.Anchor.ToPoint(),
                    };

                    segs.Add(seg);
                }
            }
            break;
        }

        if (segments.Count > 1)
        {
            for (int i = 1; i < segments.Count; i++)
            {
                switch (segments[i])
                {
                    case OT.PolyLineSegment poly:
                    {
                        PolyLineSegment seg = new(poly.Points.Select(ToPoint));

                        segs.Add(seg);
                    }
                    break;
                    case OT.PolyBezierSegment bez:
                    {
                        for (int j = 1; j < bez.Points.Count; j++)
                        {
                            BezierPoint early = bez.Points[j - 1],
                                late = bez.Points[j];

                            BezierSegment seg = new()
                            {
                                Point1 = early.LateControl.ToPoint(),
                                Point2 = late.EarlyControl.ToPoint(),
                                Point3 = late.Anchor.ToPoint(),
                            };

                            segs.Add(seg);
                        }
                    }
                    break;
                }
            }
        }

        fig.Segments = segs;

        return fig;
    }

    private static VisualBrush ConvVisualFill(OT.VisualFill fill)
    {
        if (fill.Shapes.Count == 0) return new();

        vec2 v2 = fill.Viewport.ZW;

        VisualBrush output = new()
        {
            TileMode = TileMode.None,

            DestinationRect = new(0, 0, v2.X, v2.Y, RelativeUnit.Absolute),
            SourceRect = new(0, 0, v2.X, v2.Y, RelativeUnit.Absolute),
        };	
        
        Canvas c = new() { Width = v2.X, Height = v2.Y};

        // Rectangle rectangle = new() { Height = 1000, Width = 1000, Fill = Brushes.Red };
        // c.Children.Add(rectangle);

        //List<OT.IShape> shapes = new();
        //foreach (var shape in fill.Shapes)
        //{
        //    if (shape is Rectangle)
        //    {
        //        shapes.Add(shape);
        //        continue;
        //    }

        //    var s = shape;
        //    s.TopLeft += fill.Viewport.XY;

        //    shapes.Add(s);
        //}

        //c.Children.AddRange(ConvertCollection(shapes));

        c.Children.AddRange(ConvertCollection(fill.Shapes));

        output.Visual = c;

        //output.Visual = new Rectangle { Width = v2.X, Height = v2.Y, Fill = Brushes.Red };

        return output;
    }

    private static IBrush ColourToBrush(uint colour)
    {
        var (r, g, b, a) = ((Colour)colour).ToRGBA();

        Color col = new(a, r, g, b);

        return new SolidColorBrush(col);
    }

    private static Point ToPoint(this vec2 v2) 
		=> new(v2.X, v2.Y);


	private static void SetTopLeft(this Shape shape, vec2 v2)
    {
        shape.SetValue(Canvas.LeftProperty, v2.X);
        shape.SetValue(Canvas.TopProperty, v2.Y);
    }
}