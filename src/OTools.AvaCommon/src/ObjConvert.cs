using Avalonia;
using Avalonia.Controls;
using AV = Avalonia.Controls.Shapes;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using OTools.Maps;
using ownsmtp.logging;
using OT = OTools.ObjectRenderer2D;
using System.Globalization;
using OTools.Common;
using Avalonia.Media.Imaging;

namespace OTools.AvaCommon;

public static class ObjConvert
{
	public static IEnumerable<Control> ConvertCollection(this IEnumerable<OT.IShape> shapes)
	{
		//return shapes.Select(el => (Shape)(el switch
		//{
		//	OT.Rectangle r => ConvRectange(r),
		//	OT.Circle e => ConvCircle(e),
		//	OT.Line l => ConvLine(l),
		//	OT.Area a => ConvArea(a),
		//	OT.Path p => ConvPath(p),
		//	OT.Text t => ConvText(t),
		//	_ => throw new NotImplementedException(),
		//}));

		List<Control> elements = new();

		foreach (var el in shapes)
		{
			switch (el)
			{
				case OT.Rectangle r: elements.Add(ConvRectangle(r)); break;
				case OT.Circle c: elements.Add(ConvCircle(c)); break;
				case OT.Line l: elements.Add(ConvLine(l)); break;
				case OT.Area a: elements.Add(ConvArea(a)); break;
				case OT.Path p: elements.Add(ConvPath(p)); break;
				case OT.Text t: elements.AddRange(ConvText(t)); break;
				case OT.BitmapImage b: elements.Add(ConvBitmap(b)); break;
			}
		}

		return elements;
	}

	private static IEnumerable<AV.Path> ConvText(OT.Text t)
	{
		vec2 centre = t.TopLeft;

		TextAlignment hAlign = t.HorizontalAlignment switch
		{
			HorizontalAlignment.Left => TextAlignment.Left,
			HorizontalAlignment.Centre => TextAlignment.Center,
			HorizontalAlignment.Right => TextAlignment.Right,
			HorizontalAlignment.Justify => TextAlignment.Justify,
			_ => throw new InvalidOperationException(),
		};

		FontWeight weight = t.Font.FontStyle.Bold ? FontWeight.Bold : FontWeight.Normal;

		Avalonia.Media.FontStyle style = t.Font.FontStyle.Italics switch
		{
			ItalicsMode.None => Avalonia.Media.FontStyle.Normal,
			ItalicsMode.Italic => Avalonia.Media.FontStyle.Italic,
			ItalicsMode.Oblique => Avalonia.Media.FontStyle.Oblique,
			_ => throw new InvalidOperationException(),
		};

		Typeface typeface = new(
			new FontFamily(t.Font.FontFamily),
			style, weight,
			FontStretch.Normal);

		FormattedText ft = new(t.Content, CultureInfo.CurrentCulture,
			FlowDirection.LeftToRight,
			typeface, t.Font.Size,
			_Utils.ColourToBrush(t.Font.Colour))
		{
			TextAlignment = hAlign,
		};

		Geometry geom = ft.BuildGeometry(centre.ToPoint())!;

		List<AV.Path> output = new();

		AV.Path main = new()
		{
			Data = geom,

			Fill = _Utils.ColourToBrush(t.Font.Colour),
			StrokeThickness = 0f, // No Border for main text

			ZIndex = t.Font.Colour.Precedence,
		};

		if (t.Framing.width != 0f)
		{
			AV.Path framing = new()
			{
				Data = geom,

				Stroke = _Utils.ColourToBrush(t.Framing.colour),
				StrokeThickness = t.Framing.width * 2f,

				ZIndex = t.Framing.colour.Precedence,
			};

			output.Add(framing);
		}

		if (t.Border.width != 0f)
		{
			AV.Path border = new()
			{
				Data = ft.BuildHighlightGeometry(centre.ToPoint())!,

				Stroke = _Utils.ColourToBrush(t.Border.colour),
				StrokeThickness = t.Border.width,
			};

			output.Add(border);
		}

		output.Add(main);

		return output;
	}

	private static Rectangle ConvRectangle(OT.Rectangle rect)
	{
		Rectangle output = new()
		{
			Opacity = rect.Opacity,
			ZIndex = rect.ZIndex,

			Width = rect.Size.X,
			Height = rect.Size.Y,

			Fill = _Utils.ColourToBrush(rect.Fill),

			Stroke = _Utils.ColourToBrush(rect.BorderColour),
			StrokeThickness = rect.BorderWidth,
			StrokeDashArray = new(rect.DashArray),
		};

		output.SetTopLeft(rect.TopLeft);

		return output;
	}

	private static AV.Path ConvCircle(OT.Circle circle)
	{
		OT.Path path = new()
		{
			Opacity = circle.Opacity,
			ZIndex = circle.ZIndex,

			Segments = new() { _Utils.CreateCircle(circle.Diameter / 2) },
			IsClosed = true,

			Fill = circle.Fill,

			BorderColour = circle.BorderColour,
			BorderWidth = circle.BorderWidth,
			DashArray = circle.DashArray,

			TopLeft = circle.TopLeft,
		};

		var output = ConvPath(path);

		return output;
	}

	private static Polyline ConvLine(OT.Line line)
	{
		Polyline output = new()
		{
			Opacity = line.Opacity,
			ZIndex = line.ZIndex,

			Points = line.Points.Select(_Utils.ToPoint).ToList(),

			Stroke = _Utils.ColourToBrush(line.Colour),
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

			Points = area.Points.Select(_Utils.ToPoint).ToList(),

			Fill = _Utils.ColourToBrush(area.Fill),

			Stroke = _Utils.ColourToBrush(area.BorderColour),
			StrokeThickness = area.BorderWidth,
			StrokeDashArray = new(area.DashArray),
		};

		output.SetTopLeft(area.TopLeft);

		return output;
	}

	private static AV.Path ConvPath(OT.Path path)
	{
		PathFigure pathFig = ConvPathFigure(path.Segments, path.IsClosed, path.TopLeft);

		PathFigures holeFigs = new();
		holeFigs.AddRange(path.Holes.Select(hole => ConvPathFigure(hole, true, path.TopLeft)));

		PathGeometry outlineGeom = new() { Figures = new() { pathFig } };
		PathGeometry holeGeom = new() { Figures = holeFigs, FillRule = FillRule.NonZero };

		/* CombinedGeometry causes weirdness on line only symbols
		 * 
		 * Caused when:
		 *  - 3 or more points
		 *  - Angle > 180°
		 *  - Forms a concave shape
		 *  - Once a 'full' shape has been made can't be unmade
		 */

		Geometry geom = !path.Holes.Any() ? outlineGeom :
			new CombinedGeometry(GeometryCombineMode.Exclude, outlineGeom, holeGeom);


		AV.Path output = new()
		{
			Opacity = path.Opacity,
			ZIndex = path.ZIndex,

			Data = geom,

			Stroke = _Utils.ColourToBrush(path.BorderColour),
			StrokeThickness = path.BorderWidth,
			StrokeDashArray = new(path.DashArray),

			StrokeJoin = PenLineJoin.Round,
		};

		if (!path.Fill.IsT2)
		{
			output.Fill = path.Fill.IsT0 ? 
				ConvVisualFill(path.Fill.AsT0) : 
				_Utils.ColourToBrush(path.Fill.AsT1);
		}

		//output.SetTopLeft(path.TopLeft);

		return output;
	}

	private static Image ConvBitmap(OT.BitmapImage img)
	{
		Image output = new()
		{
			Opacity = img.Opacity,
			ZIndex = img.ZIndex,

			Source = new Bitmap(img.Uri.AbsolutePath),

			RenderTransform = new TransformGroup()
			{
                Children = new()
				{
                    new RotateTransform(img.Rotation),
                    new ScaleTransform(img.Scaling.X, img.Scaling.Y),
                }
            },
		};

		output.SetTopLeft(img.TopLeft);

		return output;
	}

	private static Image ConvVector(OT.VectorImage img)
	{
		throw new NotImplementedException(); // May implement as convert to other shapes
	}

	private static PathFigure ConvPathFigure(IList<OT.IPathSegment> segments, bool isClosed, vec2 topLeft)
	{
		PathFigure fig = new() { IsClosed = isClosed };
		PathSegments segs = new();

		switch (segments[0])
		{
			case OT.PolyLineSegment sSeg:
			{
				PolyLineSegment startSeg = new();

				var points = sSeg.Points.Select(x => x + topLeft).ToArray();

				fig.StartPoint = points[0].ToPoint();

				for (int i = 1; i < points.Count(); i++)
					startSeg.Points.Add(points[i].ToPoint());

				segs.Add(startSeg);
			}
			break;
			case OT.PolyBezierSegment bSeg:
			{
				fig.StartPoint = (bSeg.Points[0].EarlyAnchor + topLeft).ToPoint();

				foreach (CubicBezier bez in bSeg.Points)
				{
					BezierSegment seg = new()
					{
						Point1 = (bez.EarlyControl + topLeft).ToPoint(),
						Point2 = (bez.LateControl + topLeft).ToPoint(),
						Point3 = (bez.LateAnchor + topLeft).ToPoint(),
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
						PolyLineSegment seg = new(poly.Points.Select(x => (x + topLeft).ToPoint()));

						segs.Add(seg);
					}
					break;
					case OT.PolyBezierSegment bez:
					{
						//for (int j = 1; j < bez.Points.Count; j++)
						//{
						//	BezierPoint early = bez.Points[j - 1],
						//		late = bez.Points[j];

						//	BezierSegment seg = new()
						//	{
						//		Point1 = (early.LateControl + topLeft).ToPoint(),
						//		Point2 = (late.EarlyControl + topLeft).ToPoint(),
						//		Point3 = (late.Anchor + topLeft).ToPoint(),
						//	};

						//	segs.Add(seg);
						//}

						foreach (CubicBezier b in bez.Points)
						{
							BezierSegment seg = new()
							{
								Point1 = (b.EarlyControl + topLeft).ToPoint(),
								Point2 = (b.LateControl + topLeft).ToPoint(),
								Point3 = (b.LateAnchor + topLeft).ToPoint(),
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

		//Rectangle rectangle = new() { Height = 1000, Width = 1000, Fill = Brushes.Red };
		//c.Children.Add(rectangle);

		List<OT.IShape> shapes = new();
		foreach (var shape in fill.Shapes)
		{
			var s = shape;
			//s.TopLeft += fill.Viewport.XY;

			ODebugger.Debug(s.TopLeft.ToString() ?? "");

			shapes.Add(s);
		}

		c.Children.AddRange(ConvertCollection(shapes));

		output.Visual = c;

		return output;
	}

	public static void SetTopLeft(this AvaloniaObject shape, vec2 v2)
	{
		//WriteLine(v2.ToString());

		shape.SetValue(Canvas.LeftProperty, v2.X);
		shape.SetValue(Canvas.TopProperty, v2.Y);
	}
}

internal static class _Utils
{
	public static IBrush ColourToBrush(uint colour)
	{
		byte b = (byte)(colour & 0x0000000ff),
			 g = (byte)((colour & 0x0000ff00) >> 8),
			 r = (byte)((colour & 0x00ff0000) >> 16),
			 a = (byte)((colour & 0xff000000) >> 24);

		Color col = new(a, r, g, b);

		return new SolidColorBrush(col);
	}

	public static Point ToPoint(this vec2 v2)
		=> new(v2.X, v2.Y);

	public static OT.PolyBezierSegment CreateCircle(float radius)
	{
		const float KAPPA = 0.5522847498f;
		float k = radius * KAPPA;


		BezierPoint top = new(
			anchor: (0, radius),
			earlyControl: (-k, radius),
			lateControl: (k, radius));

		BezierPoint right = new(
			anchor: (radius, 0),
			earlyControl: (radius, k),
			lateControl: (radius, -k));

		BezierPoint bottom = new(
			anchor: (0, -radius),
			earlyControl: (k, -radius),
			lateControl: (-k, -radius));

		BezierPoint left = new(
			anchor: (-radius, 0),
			earlyControl: (-radius, -k),
			lateControl: (-radius, k));

		BezierPath path = new() { top, right, bottom, left };

		return new() { Points = new(path.AsCubicBezier()) };
	}
}