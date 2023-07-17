using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics;
using OTools.AvaCommon;
using OTools.Maps;
using TerraFX.Interop.Windows;

namespace OTools.MapMaker;

public static class Tools
{
	public static PointSymbol ConvertSelectionToPointSymbol(IEnumerable<Instance> selection)
	{
		// Get symbols used in selection
		IEnumerable<Symbol> usedSymbols = selection.Select(x => x.Symbol).Distinct();
		
		List<MapObject> mapObjects = new();
		foreach (Instance inst in selection)
		{
			switch (inst.Symbol)
			{
				case PointSymbol p: {
					break;
				}
				case LineSymbol l: {
					LineInstance lineInst = (LineInstance)inst;
					LineObject obj = new(lineInst.Segments, l.Width, l.Colour, false);
					mapObjects.Add(obj);
				} break;
			}
		}

		throw new NotImplementedException();
	}

	public static (Instance nearest, float dist) NearestInstance(this IEnumerable<Instance> selection, vec2 pos)
	{
		if (selection.Count() == 0) 
			throw new ArgumentException("Collection cannot be empty", nameof(selection));	

		ReadOnlySpan<Instance> span = selection.ToArray();

		Instance closest = span[0]; // Annoying but necessary
		float dist = float.MaxValue;

		foreach (Instance inst in span)
		{
            switch (inst)
			{
				case PointInstance point: {
					vec4 bBox = BoundingBox.OfInstance(point);
					//vec2 radius = bBox.XY - point.Centre; // To improve

					float mag = vec2.Mag(point.Centre, pos);

					if (mag < dist)
					{
						dist = mag;
						closest = inst;
					}
				} break;
				case LineInstance lineInst: {
					var nearest = NearestContinuousPointOnPath(lineInst.Segments, pos);

					if (nearest.dist < dist)
					{
                        dist = nearest.dist;
                        closest = inst;
                    }
				} break;
			}
        }

		return (closest, dist);
	}

	public static (vec2 point, float dist) NearestContinuousPointOnPath(PathCollection path, vec2 pos)
	{
		vec2 point = vec2.Zero;
		float dist = float.MaxValue;

		foreach (IPath seg in path)
		{
            switch (seg)
			{
				case LinearPath line: {
					for (int i = 1; i < line.Count(); i++)
					{
						vec2 p = NearestPointOnLine((line[i - 1], line[i]), pos);
						float mag = vec2.Mag(p, pos);

						if (mag < dist)
						{
							point = p;
							dist = mag;
						}
					}
				} break;
				case BezierPath bezier: {
					throw new NotImplementedException();
				} break;
            }
        }

		return (point, dist);
	}

	public static vec2 NearestPointOnLine(vec4 line, vec2 pos)
	{
		vec2 xy = line.XY,
			zw = line.ZW;

		float gradient = (zw.Y - xy.Y) / (zw.X - xy.X);
		float perpGradient = -1 / gradient;

		if (gradient == 0)
			return (pos.X, xy.Y);
		if (float.IsInfinity(gradient))
			return (xy.X, pos.Y);

		float c = pos.Y - (perpGradient * pos.X);
		float abC = xy.Y - (gradient * xy.X);

		float x = (abC - c) / (perpGradient - gradient);
		float y = ((abC * perpGradient) - (c * gradient)) / (perpGradient - gradient);

		vec2 output = (x, y);

		if (vec2.Max(output, xy) > vec2.Max(zw, xy))
			return xy;

		return vec2.Mag(output, zw) > vec2.Mag(xy, zw) ? xy : output;
    }

	public static vec2 NearestPointOnCubicBezier((BezierPoint a, BezierPoint b) line, vec2 pos)
	{
		throw new NotImplementedException();
	}
}