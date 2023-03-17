using System.Collections;
using static System.Diagnostics.Debug;
using System.Runtime.CompilerServices;
using OTools.Maps;
using Sunley.Mathematics;

namespace OTools.OML;

#region Version 1

public interface ISymbolLoaderV1
{
	OMLNode SaveColours(IEnumerable<Colour> colours);
	OMLNode SaveColour(Colour colour);

	OMLNode SaveFill(IFill fill);
	OMLNode SaveSolidFill(SolidFill fill);
	OMLNode SaveRandomObjectFill(RandomObjectFill fill);
	OMLNode SaveSpacedObjectFill(SpacedObjectFill fill);
	OMLNode SavePatternFill(PatternFill fill);
	OMLNode SaveCombinedFill(CombinedFill fill);

	OMLNode SaveSymbols(IEnumerable<Symbol> symbols);
	OMLNode SaveSymbol(Symbol symbol);
	OMLNode SavePointSymbol(PointSymbol sym);
	OMLNode SaveLineSymbol(LineSymbol sym);
	OMLNode SaveAreaSymbol(AreaSymbol sym);
	OMLNode SaveTextSymbol(TextSymbol sym);
	OMLNode SaveDashStyle(DashStyle dash);
	OMLNode SaveMidStyle(MidStyle mid);

	OMLNode SaveMapObjects(IEnumerable<MapObject> mapObjects);
	OMLNode SaveMapObject(MapObject mapObject);
	OMLNode SavePointObject(PointObject obj);
	OMLNode SaveLineObject(LineObject obj);
	OMLNode SaveAreaObject(AreaObject obj);
	OMLNode SaveTextObject(TextObject obj);

	OMLNode SavePathCollection(PathCollection pathCollection);
	OMLNode SaveLinearPath(LinearPath line);
	OMLNode SaveBezierPath(BezierPath bez);
	OMLNode SaveHoles(IEnumerable<PathCollection> holes);

	IEnumerable<Colour> LoadColours(OMLNode node);
	Colour LoadColour(OMLNode node);

	IFill LoadFill(OMLNode node);
	SolidFill LoadSolidFill(OMLNode node);
	RandomObjectFill LoadRandomObjectFill(OMLNode node);
	SpacedObjectFill LoadSpacedObjectFill(OMLNode node);
	PatternFill LoadPatternFill(OMLNode node);
	CombinedFill LoadCombinedFill(OMLNode node);

	IEnumerable<Symbol> LoadSymbols(OMLNode node);
	Symbol LoadSymbol(OMLNode node);
	PointSymbol LoadPointSymbol(OMLNode node);
	LineSymbol LoadLineSymbol(OMLNode node);
	AreaSymbol LoadAreaSymbol(OMLNode node);
	TextSymbol LoadTextSymbol(OMLNode node);
	DashStyle LoadDashStyle(OMLNode node);
	MidStyle LoadMidStyle(OMLNode node);

	IEnumerable<MapObject> LoadMapObjects(OMLNode node);
	MapObject LoadMapObject(OMLNode node);
	PointObject LoadPointObject(OMLNode node);
	LineObject LoadLineObject(OMLNode node);
	AreaObject LoadAreaObject(OMLNode node);
	TextObject LoadTextObject(OMLNode node);

	PathCollection LoadPathCollection(OMLNode node);
	LinearPath LoadLinearPath(OMLNode node);
	BezierPath LoadBezierPath(OMLNode node);
	IEnumerable<PathCollection> LoadHoles(OMLNode node);
}

public class SymbolLoaderV1 : ISymbolLoaderV1
{
	#region Save
	
	public OMLNode SaveColours(IEnumerable<Colour> colours)
	{
		OMLNodeList l = new();
		foreach (Colour c in colours)
			l.Add(SaveColour(c));

		return new("Colours", l);
	}
	public OMLNode SaveColour(Colour colour) 
		=> new(colour.Name, $"#{colour.HexValue:X}");

	public OMLNode SaveFill(IFill fill)
	{
		return fill switch
		{
			SolidFill s => SaveSolidFill(s),
			RandomObjectFill r => SaveRandomObjectFill(r),
			SpacedObjectFill o => SaveSpacedObjectFill(o),
			PatternFill p => SavePatternFill(p),
			CombinedFill c => SaveCombinedFill(c),
			_ => throw new InvalidOperationException("Invalid fill type"),
		};
	}

	public OMLNode SaveSolidFill(SolidFill fill)
		=> new("SolidFill", fill.Colour.Name);

	public OMLNode SaveRandomObjectFill(RandomObjectFill fill)
	{
		return new("RandomObjectFill", new OMLNodeList
		{
			new("Spacing", fill.Spacing.ToString()),
			new("IsClipped", fill.IsClipped.ToString()),
			SaveMapObjects(fill.Objects),
		});
	}

	public OMLNode SaveSpacedObjectFill(SpacedObjectFill fill)
	{
		OMLNodeList l = new()
		{
			new("Spacing", fill.Spacing.ToString()),
			new("Offset", fill.Offset.ToString()),
			new("Rotation", (fill.Rotation * 180 / MathF.PI).ToString()),
			new("IsClipped", fill.IsClipped.ToString()),
			SaveMapObjects(fill.Objects),
		};

		return new("SpacedObjectFill", l);
	}

	public OMLNode SavePatternFill(PatternFill fill)
	{
		OMLNode fore = new("Foreground", new OMLNodeList
		{
			new("Colour", fill.ForeColour.Name),
			new("Width", fill.ForeSpacing.ToString()),
		});
		
		OMLNode back = new("Background", new OMLNodeList
		{
			new("Colour", fill.BackColour.Name),
			new("Width", fill.BackSpacing.ToString()),
		});

		return new("PatternFill", new OMLNodeList
		{
			new("Rotation", (fill.Rotation * 180 / MathF.PI).ToString()),
			fore, 
			back,
		});
	}

	public OMLNode SaveCombinedFill(CombinedFill fill)
	{
		OMLNodeList l = new();
		l.AddRange(fill.Fills.Select(SaveFill));

		return new("CombinedFill", l);
	}

	public OMLNode SaveSymbols(IEnumerable<Symbol> symbols)
	{
		OMLNodeList l = new();
		l.AddRange(symbols.Select(SaveSymbol));

		return new("Symbols", l);
	}

	public OMLNode SaveSymbol(Symbol symbol)
	{
		return symbol switch
		{
			PointSymbol p => SavePointSymbol(p),
			LineSymbol l => SaveLineSymbol(l),
			AreaSymbol a => SaveAreaSymbol(a),
			TextSymbol t => SaveTextSymbol(t),
			_ => throw new InvalidOperationException("Invalid symbol type."),
		};
	}

	private static OMLNodeList GetSymbolBase(Symbol sym)
	{
		OMLNodeList l = new()
		{
			new("Name", sym.Name),
			new("Description", sym.Description),
			new("Number", $"{sym.Number.First}-{sym.Number.Second}-{sym.Number.Third}"),
			new("IsUncrossable", sym.IsUncrossable.ToString()),
			new("IsHelper", sym.IsHelperSymbol.ToString()),
		};

		return l;
	}
	
	public OMLNode SavePointSymbol(PointSymbol sym)
	{
		OMLNodeList l = GetSymbolBase(sym);

		l.Add(new("IsRotatable", sym.IsRotatable.ToString()));
		l.Add(SaveMapObjects(sym.MapObjects));

		return new("PointSymbol", l);
	}

	public OMLNode SaveLineSymbol(LineSymbol sym)
	{
		OMLNodeList l = GetSymbolBase(sym);

		OMLNode border = new("Border", new OMLNodeList
		{
			new("Colour", sym.BorderColour.Name),
			new("Width", sym.BorderWidth.ToString()),
		});

		l.Add(border);
		l.Add(SaveDashStyle(sym.DashStyle));
		l.Add(SaveMidStyle(sym.MidStyle));
		l.Add(SaveLineStyle(sym.LineStyle));

        return new("LineSymbol", l);
	}

	public OMLNode SaveAreaSymbol(AreaSymbol sym)
	{
		OMLNodeList l = GetSymbolBase(sym);

		OMLNode border = new("Border", new OMLNodeList
		{
			new("Colour", sym.BorderColour.Name),
			new("Width", sym.BorderWidth.ToString()),
		});

		OMLNode fill = SaveFill(sym.Fill);

		l.Add(border);
		l.Add(fill);

		l.Add(SaveDashStyle(sym.DashStyle));
		l.Add(SaveMidStyle(sym.MidStyle));
		l.Add(SaveLineStyle(sym.LineStyle));
		l.Add(new("RotatablePattern", sym.RotatablePattern.ToString()));

		return new("AreaSymbol", l);

	}

	public OMLNode SaveTextSymbol(TextSymbol sym)
	{
		OMLNodeList l = GetSymbolBase(sym);

		OMLNode font = new("Font", new OMLNodeList
		{
			new("Family", sym.Font.FontFamily),
			new("Colour", sym.Font.Colour.Name),
			new("Size", sym.Font.Size.ToString()),
			new("LineSpacing", sym.Font.LineSpacing.ToString()),
			new("ParagraphSpacing", sym.Font.ParagraphSpacing.ToString()),
			new("CharacterSpacing", sym.Font.CharacterSpacing.ToString()),
			new("FontStyle", new OMLNodeList
			{
				new("Bold", sym.Font.FontStyle.Bold.ToString()),
				new("Underline", sym.Font.FontStyle.Underline.ToString()),
				new("Strikeout", sym.Font.FontStyle.Strikeout.ToString()),
				new("Italics", sym.Font.FontStyle.Italics.ToString()),
			}),
		});

		OMLNode border = new("Border", new OMLNodeList
		{
			new("Colour", sym.BorderColour.Name),
			new("Width", sym.BorderWidth.ToString()),
		});

		OMLNode framing = new("Framing", new OMLNodeList
		{
			new("Colour", sym.FramingColour.Name),
			new("Width", sym.FramingWidth.ToString()),
		});

		l.Add(font);
		l.Add(border);
		l.Add(framing);
		l.Add(new("IsRotatable", sym.IsRotatable.ToString()));

		return new("TextSymbol", l);
	}

	public OMLNode SaveDashStyle(DashStyle dash)
	{
		if (!dash.HasDash) return new("DashStyle", "None");

		return new("DashStyle", new OMLNodeList
		{
			new("DashLength", dash.DashLength.ToString()),
			new("GapLength", dash.GapLength.ToString()),
			new("GroupSize", dash.GroupSize.ToString()),
			new("GroupGapLength", dash.GroupGapLength.ToString()),
		});
	}

	public OMLNode SaveMidStyle(MidStyle mid)
	{
		if (!mid.HasMid) return new("MidStyle", "None");

		return new("MidStyle", new OMLNodeList
		{
			new("GapLength", mid.GapLength.ToString()),
			new("RequireMid", mid.RequireMid.ToString()),
			new("InitialOffset", mid.InitialOffset.ToString()),
			new("EndOffset", mid.EndOffset.ToString()),
			SaveMapObjects(mid.MapObjects),
		});
	}

	public OMLNode SaveLineStyle(LineStyle lin)
	{
		return new("LineStyle", new OMLNodeList
		{
			new("Join", lin.Join.ToString()),
			new("Cap", lin.Cap.ToString())
		});
	}

	public OMLNode SaveMapObjects(IEnumerable<MapObject> mapObjects)
	{
		OMLNodeList l = new();
		l.AddRange(mapObjects.Select(SaveMapObject));

		return new("MapObjects", l);
	}

	public OMLNode SaveMapObject(MapObject mapObject)
	{
		return mapObject switch
		{
			PointObject p => SavePointObject(p),
			LineObject l => SaveLineObject(l),
			AreaObject a => SaveAreaObject(a),
			TextObject t => SaveTextObject(t),
			_ => throw new InvalidOperationException("Invalid map object type."),
		};
	}

	public OMLNode SavePointObject(PointObject obj)
	{
		return new("PointObject", new OMLNodeList
		{
			new("Inner", new OMLNodeList
			{
				new("Colour", obj.InnerColour.Name),
				new("Width", obj.InnerRadius.ToString()),
			}),
			new("Outer", new OMLNodeList
			{
				new("Colour", obj.OuterColour.Name),
				new("Width", obj.OuterRadius.ToString()),
			}),
		});
	}

	public OMLNode SaveLineObject(LineObject obj)
	{
		return new("LineObject", new OMLNodeList
		{
			new("Border", new OMLNodeList
			{
				new("Colour", obj.Colour.Name),
				new("Width", obj.Width.ToString()),
			}),
			SavePathCollection(obj.Segments),
		});
	}

	public OMLNode SaveAreaObject(AreaObject obj)
	{
		return new("AreaObject", new OMLNodeList
		{
			new("Border", new OMLNodeList
			{
				new("Colour", obj.BorderColour.Name),
				new("Width", obj.BorderWidth.ToString()),
			}),
			SaveFill(obj.Fill),
			SavePathCollection(obj.Segments),
		});
	}

	public OMLNode SaveTextObject(TextObject obj)
	{
		return new("TextObject", new OMLNodeList
		{
			new("Font", new OMLNodeList
			{
				new("Family", obj.Font.FontFamily),
				new("Size", obj.Font.Size.ToString()),
				new("Colour", obj.Font.Colour.Name),
				new("Font Style", new OMLNodeList
				{
					new("Bold", obj.Font.FontStyle.Bold.ToString()),
					new("Underline", obj.Font.FontStyle.Underline.ToString()),
					new("Strikeout", obj.Font.FontStyle.Strikeout.ToString()),
					new("Italics", obj.Font.FontStyle.Italics.ToString()),
				}),
			}),
			new("Border", new OMLNodeList
			{
				new("Colour", obj.BorderColour.Name),
				new("Width", obj.BorderWidth.ToString()),
			}),
			new("Framing", new OMLNodeList
			{
				new("Colour", obj.FramingColour.Name),
				new("Width", obj.FramingWidth.ToString()),
			}),
			new("Rotation", (obj.Rotation * 180 / MathF.PI).ToString()),
			new("Text", obj.Text),
			new("TopLeft", obj.TopLeft.ToString()),
			new("Alignment", obj.HorizontalAlignment.ToString()),
		});
	}

	public OMLNode SavePathCollection(PathCollection pathCollection)
	{
		OMLNodeList l = new();
		List<int> gaps = new();

		foreach (IPathSegment seg in pathCollection)
		{
			l.Add(seg switch
			{
				LinearPath line => SaveLinearPath(line),
				BezierPath bez => SaveBezierPath(bez),
				_ => throw new InvalidOperationException("Invalid PathSegment type."),
			});

			if (seg.IsGap) gaps.Add(pathCollection.IndexOf(seg));
		}

		string gapStr = string.Concat(gaps.Select(x => $"{x}, "));
		l.Add(new("Gaps", gapStr));

		return new("PathCollection", l);
	}

	public OMLNode SaveLinearPath(LinearPath line)
	{
		return new("LinearPath", new OMLVec2s(line));
	}

	public OMLNode SaveBezierPath(BezierPath bez)
	{
		OMLNodeList l = new();

		foreach (var pt in bez)
			l.Add(new("BezierPoint", new OMLVec2s(
						  new[] { pt.EarlyControl, pt.Anchor, pt.LateControl })));

		return new("BezierPath", l);
	}

	public OMLNode SaveHoles(IEnumerable<PathCollection> holes)
	{
		OMLNodeList l = new();

		foreach (PathCollection hole in holes)
		{
			OMLNode h = SavePathCollection(hole);
			h.Title = "Hole";

			l.Add(h);
		}

		return new("Holes", l);
	}

	#endregion
	
	#region Load

	private ColourStore _colours;
	
	

	public IEnumerable<Colour> LoadColours(OMLNode node)
	{
		OMLNodeList l = (OMLNodeList)node.Value!;
		foreach (OMLNode c in l)
			_colours.Add(LoadColour(c));

		return _colours;
	}

	public Colour LoadColour(OMLNode node)
	{
		return new(node.Title, Convert.ToUInt32((OMLString)node.Value!, 16));
	}

	public IFill LoadFill(OMLNode node)
	{
		return node.Title switch
		{
			"SolidFill" => LoadSolidFill(node),
			"RandomObjectFill" => LoadRandomObjectFill(node),
			"SpacedObjectFill" => LoadSpacedObjectFill(node),
			"PatternFill" => LoadPatternFill(node),
			"CombinedFill" => LoadCombinedFill(node),
			_ => throw new InvalidOperationException("Invalid Fill type."),
		};
	}

	public SolidFill LoadSolidFill(OMLNode node)
	{
		return new(_colours[(OMLString)node.Value!]);
	}

	public RandomObjectFill LoadRandomObjectFill(OMLNode node)
	{
		OMLNodeList l = (OMLNodeList)node.Value!;

		return new(LoadMapObjects(l["MapObjects"]), 
				   bool.Parse((OMLString)l["IsClipped"].Value!),
				   float.Parse((OMLString)l["Spacing"].Value!));
	}

	public SpacedObjectFill LoadSpacedObjectFill(OMLNode node)
	{
		OMLNodeList l = (OMLNodeList)node.Value!;

		float rotationInDegrees = float.Parse((OMLString)l["Rotation"].Value!);
		float rotation = rotationInDegrees * (MathF.PI / 180);
		
		return new(LoadMapObjects(l["MapObjects"]),
			bool.Parse((OMLString)l["IsClipped"].Value!),
			vec2.Parse((OMLString)l["Spacing"].Value!),
			vec2.Parse((OMLString)l["Offset"].Value!),
			rotation);
	}

	public PatternFill LoadPatternFill(OMLNode node)
	{
		OMLNodeList l = (OMLNodeList)node.Value!;
		
		float rotationInDegrees = float.Parse((OMLString)l["Rotation"].Value!);
		float rotation = rotationInDegrees * (MathF.PI / 180);

		OMLNodeList fore = (OMLNodeList)l["Foreground"].Value!;
		OMLNodeList back = (OMLNodeList)l["Background"].Value!;
		
		return new(float.Parse((OMLString)fore["Width"].Value!), float.Parse((OMLString)fore["Width"].Value!), 
				   _colours[(OMLString)fore["Colour"].Value!], _colours[(OMLString)back["Colour"].Value!], 
				   rotation);
	}

	public CombinedFill LoadCombinedFill(OMLNode node)
	{
		OMLNodeList l = (OMLNodeList)node.Value!;
		
		List<IFill> fills = new();
		foreach (OMLNode f in l)
			fills.Add(LoadFill(f));
		
		return new(fills);
	}

	public IEnumerable<Symbol> LoadSymbols(OMLNode node, IEnumerable<Colour> colours)
	{
		_colours = new(colours);

		return LoadSymbols(node);
	}
	
	public IEnumerable<Symbol> LoadSymbols(OMLNode node)
	{
		OMLNodeList l = (OMLNodeList)node.Value!;
		foreach (OMLNode s in l)
			yield return LoadSymbol(s);
	}

	public Symbol LoadSymbol(OMLNode node)
	{
		return node.Title switch
		{
			"PointSymbol" => LoadPointSymbol(node),
			"LineSymbol" => LoadLineSymbol(node),
			"AreaSymbol" => LoadAreaSymbol(node),
			"TextSymbol" => LoadTextSymbol(node),
			_ => throw new InvalidOperationException("Invalid Symbol type."),
		};
	}

	private static (string name, string desc, SymbolNumber num, bool uncr, bool help) GetSymbolBase(OMLNodeList l)
	{
		string name = l["Name"];
		string desc = l["Description"];
		SymbolNumber num = new(l["Number"]);
		bool uncr = bool.Parse(l["IsUncrossable"]);
		bool help = bool.Parse(l["IsHelper"]);
		
		return (name, desc, num, uncr, help);
	}

	public PointSymbol LoadPointSymbol(OMLNode node)
	{
		OMLNodeList l = (OMLNodeList)node.Value!;
		var bas = GetSymbolBase(l);
		
		bool isR = bool.Parse(l["IsRotatable"]);
		
		return new(bas.name, bas.desc, bas.num, bas.uncr, bas.help, LoadMapObjects(l["MapObjects"]), isR);
	}

	public LineSymbol LoadLineSymbol(OMLNode node)
	{
		OMLNodeList l = (OMLNodeList)node.Value!;
		var bas = GetSymbolBase(l);

		OMLNodeList border = (OMLNodeList)l["Border"].Value!;
		Colour col = _colours[border["Colour"]];
		float wid = float.Parse(border["Width"]);

		DashStyle d = LoadDashStyle(l["DashStyle"]);
		MidStyle m = LoadMidStyle(l["MidStyle"]);
		LineStyle lS = LoadLineStyle(l["LineStyle"]);
		
		return new(bas.name, bas.desc, bas.num, bas.uncr, bas.help, col, wid, d, m, lS);
	}

	public AreaSymbol LoadAreaSymbol(OMLNode node)
	{
		OMLNodeList l = (OMLNodeList)node.Value!;
		var bas = GetSymbolBase(l);

		OMLNodeList border = (OMLNodeList)l["Border"].Value!;
		Colour col = _colours[border["Colour"]];
		float wid = float.Parse(border["Width"]);

		IFill fill = LoadFill(l.Find(x => x.Title.Contains("Fill"))!);
		
		DashStyle d = LoadDashStyle(l["DashStyle"]);
		MidStyle m = LoadMidStyle(l["MidStyle"]);
		LineStyle lS = LoadLineStyle(l["LineStyle"]);
		
		bool rotPattern = bool.Parse(l["RotatablePattern"]);

		return new(bas.name, bas.desc, bas.num, bas.uncr, bas.help, fill, col, wid, d, m, lS, rotPattern);
	}

	public TextSymbol LoadTextSymbol(OMLNode node)
	{
		OMLNodeList l = (OMLNodeList)node.Value!;
		var bas = GetSymbolBase(l);
		
		OMLNodeList font = (OMLNodeList)l["Font"].Value!;

		string family = font["Family"];
		Colour col = _colours[font["Colour"]];
		float size = float.Parse(font["Size"]),
			  lSpace = float.Parse(font["LineSpacing"]),
			  pSpace = float.Parse(font["ParagraphSpacing"]),
			  cSpace = float.Parse(font["CharacterSpacing"]);

		OMLNodeList fontStyle = (OMLNodeList)font["FontStyle"].Value!;
		
		bool bold = bool.Parse(fontStyle["Bold"]),
			 underline = bool.Parse(fontStyle["Underline"]),
			 strikeout = bool.Parse(fontStyle["Strikeout"]);
		ItalicsMode it = (string)fontStyle["Italics"] switch
		{
			"None" => ItalicsMode.None,
			"Italic" => ItalicsMode.Italic,
			"Oblique" => ItalicsMode.Oblique,
		};

		FontStyle fSt = new(bold, underline, strikeout, it);
		Font f = new(family, col, size, lSpace, pSpace, cSpace, fSt);
		
		OMLNodeList border = (OMLNodeList)l["Border"].Value!;

		Colour bCol = _colours[border["Colour"]];
		float bWid = float.Parse(border["Width"]);
		
		OMLNodeList framing = (OMLNodeList)l["Framing"].Value!;
		
		Colour fCol = _colours[framing["Colour"]];
		float fWid = float.Parse(framing["Width"]);
		
		bool isRot = bool.Parse(l["IsRotatable"]);

		return new(bas.name, bas.desc, bas.num, bas.uncr, bas.help, f, isRot, bCol, bWid, fCol, fWid);
	}

	public DashStyle LoadDashStyle(OMLNode node)
	{
		if (node.Value! is OMLString s && s == "None")
			return DashStyle.None;
		
		OMLNodeList l = (OMLNodeList)node.Value!;

		return new(float.Parse(l["DashLength"]), float.Parse(l["GapLength"]), 
				   int.Parse(l["GroupSize"]), float.Parse(l["GroupGapLength"]));
	}

	public MidStyle LoadMidStyle(OMLNode node)
	{
		if (node.Value is OMLString s && s == "None")
			return MidStyle.None;
		
		OMLNodeList l = (OMLNodeList)node.Value!;
		
		return new(LoadMapObjects(l["MapObjects"]), float.Parse(l["GapLength"]), bool.Parse(l["RequireMid"]), float.Parse(l["InitialOffset"]), float.Parse(l["EndOffset"]));
	}

	public LineStyle LoadLineStyle(OMLNode node)
	{
		OMLNodeList l = (OMLNodeList)node.Value!;

		return new(int.Parse(l["Join"]), int.Parse(l["Cap"]));
	}

	public IEnumerable<MapObject> LoadMapObjects(OMLNode node)
	{
		OMLNodeList l = (OMLNodeList)node.Value!;
		foreach (var n in l)
			yield return LoadMapObject(n);
	}

	public MapObject LoadMapObject(OMLNode node)
	{
		return node.Title switch
		{
			"PointObject" => LoadPointObject(node),
			"lineObject" => LoadLineObject(node),
			"AreaObject" => LoadAreaObject(node),
			"TextObject" => LoadTextObject(node),
		};
	}

	public PointObject LoadPointObject(OMLNode node)
	{
		OMLNodeList l = (OMLNodeList)node.Value!;
		OMLNodeList inner = (OMLNodeList)l["Inner"].Value!;
		OMLNodeList outer = (OMLNodeList)l["Outer"].Value!;
		
		Colour iCol = _colours[inner["Colour"]],
			   oCol = _colours[outer["Colour"]];
		float iWid = float.Parse(inner["Width"]),
			  oWid = float.Parse(outer["Width"]);
		
		return new(iCol, oCol, iWid, oWid);
	}

	public LineObject LoadLineObject(OMLNode node)
	{
		OMLNodeList l = (OMLNodeList)node.Value!;
		OMLNodeList border = (OMLNodeList)l["Border"].Value!;

		Colour col = _colours[border["Colour"]];
		float width = float.Parse(border["Width"]);

		PathCollection pC = LoadPathCollection(l["PathCollection"]);

		return new(pC, width, col);
	}

	public AreaObject LoadAreaObject(OMLNode node)
	{
		OMLNodeList l = (OMLNodeList)node.Value!;
		OMLNodeList border = (OMLNodeList)l["Border"].Value!;

		Colour col = _colours[border["Colour"]];
		float width = float.Parse(border["Width"]);
		IFill fill = LoadFill(l.Find(x => x.Title.Contains("Fill")));

		PathCollection pC = LoadPathCollection(l["PathCollection"]);

		return new(pC, width, col, fill);
	}

	public TextObject LoadTextObject(OMLNode node)
	{
		OMLNodeList l = (OMLNodeList)node.Value!;
		
		OMLNodeList font = (OMLNodeList)l["Font"].Value!;

		string family = font["Family"];
		Colour col = _colours[font["Colour"]];
		float size = float.Parse(font["Size"]),
			  lSpace = float.Parse(font["LineSpacing"]),
			  pSpace = float.Parse(font["ParagraphSpacing"]),
			  cSpace = float.Parse(font["CharacterSpacing"]);

		OMLNodeList fontStyle = (OMLNodeList)font["FontStyle"].Value!;
		
		bool bold = bool.Parse(fontStyle["Bold"]),
			 underline = bool.Parse(fontStyle["Underline"]),
			 strikeout = bool.Parse(fontStyle["Strikeout"]);
		ItalicsMode it = (string)fontStyle["Italics"] switch
		{
			"None" => ItalicsMode.None,
			"Italic" => ItalicsMode.Italic,
			"Oblique" => ItalicsMode.Oblique,
		};

		FontStyle fSt = new(bold, underline, strikeout, it);
		Font f = new(family, col, size, lSpace, pSpace, cSpace, fSt);
		
		OMLNodeList border = (OMLNodeList)l["Border"].Value!;

		Colour bCol = _colours[border["Colour"]];
		float bWid = float.Parse(border["Width"]);
		
		OMLNodeList framing = (OMLNodeList)l["Framing"].Value!;
		
		Colour fCol = _colours[framing["Colour"]];
		float fWid = float.Parse(framing["Width"]);

		float rotationInDegrees = float.Parse((OMLString)l["Rotation"].Value!);
		float rotation = rotationInDegrees * (MathF.PI / 180);

		string text = l["Text"];
		vec2 topLeft = vec2.Parse(l["TopLeft"]);
		HorizontalAlignment align = (string)l["Alignment"] switch
		{
			"Left" => HorizontalAlignment.Left,
			"Centre" => HorizontalAlignment.Centre,
			"Right" => HorizontalAlignment.Right,
			"Justify" => HorizontalAlignment.Justify,
		};

		return new(text, topLeft, rotation, f, bCol, bWid, fCol, fWid, align);
	}

	public PathCollection LoadPathCollection(OMLNode node)
	{
		OMLNodeList l = (OMLNodeList)node.Value!;

		PathCollection pC = new();

		foreach (OMLNode s in l)
		{
			pC.Add(s.Title switch
			{
				"LinearPath" => LoadLinearPath(s),
				"BezierPath" => LoadBezierPath(s),
				_ => throw new Exception("Invalid path segment type")
			});
		}

		string gapStr = l["Gaps"];
		
		for (int i = 0; i < pC.Count; i++)
		{
			if (gapStr.Contains(i.ToString()))
				pC[i].IsGap = true;
		}

		return pC;
	}

	public LinearPath LoadLinearPath(OMLNode node)
	{
		return new((OMLVec2s)node.Value!);
	}

	public BezierPath LoadBezierPath(OMLNode node)
	{
		OMLNodeList l = (OMLNodeList)node.Value!;

		List<BezierPoint> bezs = new();
		foreach (OMLNode p in l)
		{
			OMLVec2s v2s = (OMLVec2s)p.Value!;
			bezs.Add(new(v2s[1], v2s[0], v2s[2]));
		}

		return new(bezs);
	}

	public IEnumerable<PathCollection> LoadHoles(OMLNode node)
	{
		OMLNodeList l = (OMLNodeList)node.Value!;

		List<PathCollection> holes = new();
		foreach (OMLNode h in l)
			holes.Add(LoadPathCollection(h));

		return holes;
	}
	
	#endregion
}

#endregion