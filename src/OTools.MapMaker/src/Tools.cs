using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OTools.Maps;

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
					LineObject obj = new(lineInst.Segments, l.Colour, l.Colour);
					mapObjects.Add(obj);
				} break;
			}
		}

		throw new NotImplementedException();
	}
}