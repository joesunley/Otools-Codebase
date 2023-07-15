using OTools.Common;
using OTools.Maps;

namespace OTools.FileFormatConverter;

public static class OmapLoader
{
	public static Map Load(string filePath, int version)
	{
		throw new NotImplementedException();
	}

	public static XMLDocument Save(Map map, int version)
	{
		throw new NotImplementedException();
	}
}

#region Version 9

#region Types

file struct v9_Color
{
	public string name;
	public float c, m, y, k;
	public float a;

	public v9_Color()
	{
		a = 1;

		name = string.Empty;
		c = 0;
		m = 0;
		y = 0;
		k = 0;
	}
}

file abstract class v9_Symbol
{
	[Flags]
	public enum Type
	{
		Point = 1,
		Line = 2,
		Area = 4,
		Text = 8,
		Combined = 16,
		NoSymbol = 0,
	}

	public string name, description;
	public Type type;
	public bool is_helper_symbol, is_hidden, is_protected, is_rotatable = false;
	public (int, int, int) number;
}

file class v9_PointSymbol : v9_Symbol
{
	public int inner_radius, outer_width;
	public v9_Color inner_color, outer_color;
	public List<Element> elements = new();

	public struct Element
	{
		public v9_Symbol symbol;
		public v9_Object objec;
	}
}

file class v9_LineSymbol : v9_Symbol
{
	public Border border, right_border;
	public v9_PointSymbol start_symbol, mid_symbol, end_symbol, dash_symbol;
	public v9_Color color;

	public int line_width,
			   minimum_length,
			   start_offset,
			   end_offset, // Base line
			   mid_symbols_per_spot,
			   mid_symbol_distance,
			   minimum_mid_symbol_count,
			   minimum_mid_symbol_count_when_closed, // Base line
			   segment_length,
			   end_length, // Not Dashed
			   dash_length,
			   break_length,
			   dashes_in_group,
			   in_group_break_length; // Dashed

	public CapStyle cap_style;
	public JoinStyle join_style;
	public MidSymbolPlacement mid_symbol_placement;

	public bool dashed,
				half_outer_dashes,
				show_at_least_one_symbol,
				suppress_dash_symbol_at_ends,
				scale_dash_symbol,
				have_border_lines;

	public enum CapStyle
	{
		FlatCap,
		RoundCap,
		SquareCap,
		PointedCap
	}

	public enum JoinStyle
	{
		BevelJoin,
		MiterJoin,
		RoundJoin
	}

	public enum MidSymbolPlacement
	{
		CenterOfDash,
		CenterOfDashGroup,
		CenterOfGap,
		NoMidSymbols = 99
	}

	public struct Border
	{
		public v9_Color color;
		public int width, shift, dash_length, break_length;
		public bool dashed;
	}
}

file class v9_AreaSymbol : v9_Symbol
{
	public List<FillPattern> patterns = new();
	public v9_Color color;
	public int minimum_area; // in mm^2 // FIXME: unit (factor) wrong

	public struct FillPattern
	{
		public enum Type
		{
			LinePattern = 1,
			PointPattern = 2
		}

		[Flags]
		public enum Option
		{
			Default = 0x00,
			NoClippingIfCompletelyInside = 0x01,
			NoClippingIfCenterInside = 0x02,
			NoClippingIfPartiallyInside = 0x03,
			AlternativeToClipping = 0x03, // Bitmask for NoClipping options
			Rotatable = 0x10,
		}

		public Type type;
		public Option flags;
		public float angle; // Rotation angle in radians
		public int line_spacing, line_offset;
		public string name;

		// For Type_t.LinePattern
		public v9_Color line_color;
		public int line_width;

		// For Type_t.Point
		public int offset_along_line, point_distance;
		public v9_PointSymbol point;
	}
}

file class v9_TextSymbol : v9_Symbol
{
	public string font_family, icon_text;
	public v9_Color color, framing_color, line_below_color;
	public List<int> custom_tabs;
	public double tab_interval;
	public float line_spacing, character_spacing;

	public int font_size,
			   paragraph_spacing,
			   framing_line_half_width,
			   framing_shadow_x_offset,
			   framing_shadow_y_offset,
			   line_below_width,
			   line_below_distance;

	public FramingMode framing_mode;
	public bool bold, italic, underline, kerning, framing, line_below;

	public enum FramingMode
	{
		NoFraming,
		LineFraming,
		ShadowFraming
	}
}

file class v9_CombinedSymbol : v9_Symbol
{
	public List<v9_Symbol?> parts = new();
	public List<bool> private_parts = new();
	public List<int> temp_part_indices = new();
}

file abstract class v9_Object
{
	public enum Type
	{
		Point,
		Path,
		Text = 4,
	}

	public Type type;
	public v9_Symbol symbol;
	public v9_Map map;

	public List<v9_MapCoord> coords;
	public double rotation;
}

file class v9_PointObject : v9_Object
{

}

file class v9_PathObject : v9_Object
{
	public vec2i pattern_origin;
}

file class v9_TextObject : v9_Object
{
	public string text;
	public HorizontalAlignment h_align;
	public VerticalAlignment v_align;
	public bool has_single_anchor = true;
	public v9_MapCoord size;

	public enum HorizontalAlignment
	{
		AlignLeft,
		AlignHCenter,
		AlignRight,
	}

	public enum VerticalAlignment
	{
		AlignBaseline,
		AlignTop,
		AlignVCenter,
		AlignBottom
	}
}

file class v9_Map
{
	public Dictionary<int, v9_Color> colors;
	public Dictionary<int, v9_Symbol> symbols;
	public Dictionary<int, v9_MapPart> parts;

	public string map_notes;

	public v9_Map()
	{
		colors = new();
		symbols = new();
		parts = new();
	}
}

file class v9_MapPart
{
	public string name;
	public v9_Map map;
	public List<v9_Object> objects;
}

file class v9_MapCoord
{
	public int xp, yp;
	public Flags fp;
	public BoundsOffset bounds_offset;

	[Flags]
	public enum Flags
	{
		CurveStart = 1 << 0, // 1
		ClosePoint = 1 << 1, // 2
		GapPoint = 1 << 2, // 4

		//Unused     1 << 3,
		HolePoint = 1 << 4, // 16
		DashPoint = 1 << 5, // 32

		MaskCopiedFlagsAtStart = GapPoint | DashPoint, // 36
		MaskCopiedFlagsAtEnd = GapPoint | DashPoint | HolePoint | ClosePoint, // 54
	}

	public struct BoundsOffset
	{
		public long x, y;
		public bool check_for_offset;
	}
}

#endregion

file static class v9_LocalLoad
{
	private static readonly List<(string name, int index, int symbol)> s_deferredCombinedSymbols = new();

	public static v9_Symbol LoadSymbol(XMLNode node, v9_Map map)
	{
		if (node.Name != "symbol")
			throw new ArgumentException("Expected symbol node");

		bool addable;

		XMLAttributeCollection attr = node.Attributes;

		v9_Symbol.Type symbol_type = (v9_Symbol.Type)attr["type"].Parse<int>();
		v9_Symbol s = symbol_type switch
		{
			v9_Symbol.Type.Point => new v9_PointSymbol(),
			v9_Symbol.Type.Line => new v9_LineSymbol(),
			v9_Symbol.Type.Area => new v9_AreaSymbol(),
			v9_Symbol.Type.Text => new v9_TextSymbol(),
			v9_Symbol.Type.Combined => new v9_CombinedSymbol(),
			_ => throw new ArgumentException("Unknown symbol type"),
		};

		string code = attr["code"];
		if (attr.Exists("id"))
		{
			string id = attr["id"];

			bool conversion_ok = int.TryParse(id, out int converted_id);
			if (id != string.Empty)
			{
				if (conversion_ok)
				{
					if (map.symbols.ContainsKey(converted_id))
						throw new Exception("Symbol ID is not unique");

					addable = true;
				}
			}

			if (code == string.Empty)
				code = id;
		}

		if (code != string.Empty)
		{
			string[] split = code.Split('.');
			s.number = split.Length switch
			{
				1 => (split[0].Parse<int>(), 0, 0),
				2 => (split[0].Parse<int>(), split[1].Parse<int>(), 0),
				3 => (split[0].Parse<int>(), split[1].Parse<int>(), split[2].Parse<int>()),
				_ => throw new ArgumentException(),
			};
		}

		if (attr.Exists("name"))
			s.name = attr["name"];

		s.is_helper_symbol = attr.IsTrue("is_helper_symbol");
		s.is_hidden = attr.IsTrue("is_hidden");
		s.is_protected = attr.IsTrue("is_protected");

		foreach (XMLNode child in node.Children)
		{
			if (child.Name == "description")
				s.description = child.InnerText;
			else if (child.Name == "icon")
				continue; // Not Implemented yet
			else
			{
				s = symbol_type switch
				{
					v9_Symbol.Type.Point => LoadPointSymbol(child, map, (v9_PointSymbol)s),
					v9_Symbol.Type.Line => LoadLineSymbol(child, map, (v9_LineSymbol)s),
					v9_Symbol.Type.Area => LoadAreaSymbol(child, map, (v9_AreaSymbol)s),
					v9_Symbol.Type.Text => LoadTextSymbol(child, map, (v9_TextSymbol)s),
					v9_Symbol.Type.Combined => LoadCombinedSymbol(child, map, (v9_CombinedSymbol)s),
					_ => s,
				};
			}
		}

		return s;
	}

	public static v9_PointSymbol LoadPointSymbol(XMLNode node, v9_Map map, v9_PointSymbol s)
	{
		Assert(node.Name == "point_symbol");

		XMLAttributeCollection attr = node.Attributes;

		s.is_rotatable = attr.IsTrue("rotatable");
		s.inner_radius = attr["inner_radius"].Parse<int>();
		int temp = attr["inner_color"].Parse<int>();
		s.inner_color = map.colors[temp];
		s.outer_width = attr["outer_width"].Parse<int>();
		temp = attr["outer_color"].Parse<int>();
		s.outer_color = map.colors[temp];

		foreach (XMLNode child in node.Children)
		{
			if (child.Name == "element")
			{
				v9_Symbol? el = null;

				foreach (XMLNode chi in child.Children)
				{
					if (chi.Name == "symbol" && el is null)
						el = LoadSymbol(chi, map);
					else if (chi.Name == "object" && el is not null)
					{
						v9_Object o = LoadObject(chi, map, el);
						s.elements.Add(new() { symbol = el, objec = o });
					}
				}
			}
		}

		return s;
	}

	public static v9_LineSymbol LoadLineSymbol(XMLNode node, v9_Map map, v9_LineSymbol s)
	{
		Assert(node.Name == "line_symbol");

		XMLAttributeCollection attr = node.Attributes;

		int temp = attr["color"].Parse<int>();
		s.color = map.colors[temp];
		s.line_width = attr["line_width"].Parse<int>();
		s.minimum_length = attr["minimum_length"].Parse<int>();
		s.join_style = (v9_LineSymbol.JoinStyle)attr["join_style"].Parse<int>();
		s.cap_style = (v9_LineSymbol.CapStyle)attr["cap_style"].Parse<int>();

		if (attr.Exists("start_offset") || attr.Exists("end_offset"))
		{
			s.start_offset = attr["start_offset"].Parse<int>();
			s.end_offset = attr["end_offset"].Parse<int>();
		}
		else if (s.cap_style == v9_LineSymbol.CapStyle.PointedCap)
		{
			s.start_offset = s.end_offset = attr["pointed_cap_length"].Parse<int>();
		}

		s.dashed = attr.IsTrue("dashed");
		s.segment_length = attr["segment_length"].Parse<int>();
		s.end_length = attr["end_length"].Parse<int>();
		s.show_at_least_one_symbol = attr.IsTrue("show_at_least_one_symbol");
		s.minimum_mid_symbol_count = attr["minimum_mid_symbol_count"].Parse<int>();
		s.minimum_mid_symbol_count_when_closed = attr["minimum_mid_symbol_count_when_closed"].Parse<int>();
		s.dash_length = attr["dash_length"].Parse<int>();
		s.break_length = attr["break_length"].Parse<int>();
		s.dashes_in_group = attr["dashes_in_group"].Parse<int>();
		s.in_group_break_length = attr["in_group_break_length"].Parse<int>();
		s.half_outer_dashes = attr.IsTrue("half_outer_dashes");
		s.mid_symbols_per_spot = attr["mid_symbols_per_spot"].Parse<int>();
		s.mid_symbol_distance = attr["mid_symbol_distance"].Parse<int>();
		s.mid_symbol_placement = (v9_LineSymbol.MidSymbolPlacement)(attr.Exists("mid_symbol_placement") ? attr["mid_symbol_placement"].Parse<int>() : 99);
		s.suppress_dash_symbol_at_ends = attr.IsTrue("suppress_dash_symbol_at_ends");
		s.scale_dash_symbol = attr.IsNotFalse("scale_dash_symbol");

		s.have_border_lines = false;
		foreach (XMLNode child in node.Children)
		{
			switch (child.Name)
			{
				case "start_symbol":
					s.start_symbol = (v9_PointSymbol)LoadSymbol(child.Children[0], map);
					break;
				case "mid_symbol":
					s.mid_symbol = (v9_PointSymbol)LoadSymbol(child.Children[0], map);
					break;
				case "end_symbol":
					s.end_symbol = (v9_PointSymbol)LoadSymbol(child.Children[0], map);
					break;
				case "dash_symbol":
					s.dash_symbol = (v9_PointSymbol)LoadSymbol(child.Children[0], map);
					break;
				default:
					{
						bool right_border_loaded = false;
						foreach (XMLNode chi in child.Children)
						{
							if (chi.Name == "border")
							{
								if (!s.have_border_lines)
								{
									s.border = LoadBorder(chi, map);
									s.have_border_lines = true;
								}
								else
								{
									s.right_border = LoadBorder(chi, map);
									right_border_loaded = true;
									break;
								}
							}
						}

						if (s.have_border_lines && !right_border_loaded)
							s.right_border = s.border;
					}
					break;
			}
		}

		return s;
	}

	public static v9_AreaSymbol LoadAreaSymbol(XMLNode node, v9_Map map, v9_AreaSymbol s)
	{
		Assert(node.Name == "area_symbol");

		XMLAttributeCollection attr = node.Attributes;

		int temp = attr["inner_color"].Parse<int>();
		s.color = map.colors[temp];
		s.minimum_area = attr["min_area"].Parse<int>();

		foreach (XMLNode child in node.Children)
		{
			if (child.Name == "pattern")
				s.patterns.Add(LoadFillPattern(child, map));
		}

		return s;
	}

	public static v9_TextSymbol LoadTextSymbol(XMLNode node, v9_Map map, v9_TextSymbol s)
	{
		Assert(node.Name == "text_symbol");

		XMLAttributeCollection attr = node.Attributes;

		s.icon_text = attr["icon_text"];

		s.is_rotatable = attr.IsTrue("rotatable");

		s.framing = false;
		s.line_below = false;
		s.custom_tabs = new();

		foreach (XMLNode child in node.Children)
		{
			if (child.Name == "font")
			{
				s.font_family = child.Attributes["family"];
				s.font_size = child.Attributes["size"].Parse<int>();
				s.bold = child.Attributes.IsTrue("bold");
				s.italic = child.Attributes.IsTrue("italic");
				s.underline = child.Attributes.IsTrue("underline");
			}
			else if (child.Name == "text")
			{
				int temp = child.Attributes["color"].Parse<int>();
				s.color = map.colors[temp];
				s.line_spacing = child.Attributes["line_spacing"].Parse<int>();
				s.paragraph_spacing = child.Attributes["paragraph_spacing"].Parse<int>();
				s.character_spacing = child.Attributes["character_spacing"].Parse<int>();
				s.kerning = child.Attributes.IsTrue("kerning");
			}
			else if (child.Name == "framing")
			{
				s.framing = true;
				int temp = child.Attributes["color"].Parse<int>();
				s.framing_color = map.colors[temp];
				s.framing_mode = (v9_TextSymbol.FramingMode)(child.Attributes["mode"].Parse<int>());
				s.framing_line_half_width = child.Attributes["line_half_width"].Parse<int>();
				s.framing_shadow_x_offset = child.Attributes["shadow_x_offset"].Parse<int>();
				s.framing_shadow_y_offset = child.Attributes["shadow_y_offset"].Parse<int>();
			}
			else if (child.Name == "line_below")
			{
				s.line_below = true;
				int temp = child.Attributes["color"].Parse<int>();
				s.line_below_color = map.colors[temp];
				s.line_below_width = child.Attributes["width"].Parse<int>();
				s.line_below_distance = child.Attributes["distance"].Parse<int>();
			}
			else if (child.Name == "tabs")
			{
				int num_custom_tabs = child.Attributes["count"].Parse<int>();

				s.custom_tabs.AddRange(child.Children.Where(c => c.Name == "tab").Select(c => c.InnerText.Parse<int>()));
			}
		}

		return s;
	}

	public static v9_CombinedSymbol LoadCombinedSymbol(XMLNode node, v9_Map map, v9_CombinedSymbol s)
	{
		Assert(node.Name == "combined_symbol");

		foreach (XMLNode child in node.Children)
		{
			if (child.Name == "part")
			{
				bool is_private = child.Attributes.IsTrue("private");
				s.private_parts.Add(is_private);
				if (is_private)
				{
					s.parts.Add(LoadSymbol(child.Children[0], map));
					s.temp_part_indices.Add(-1);
				}
				else
				{
					int temp = child.Attributes["symbol"].Parse<int>();
					s.temp_part_indices.Add(temp);

					if (temp < 0)
						continue;
					if (temp >= map.symbols.Count)
					{
						s.parts.Add(null);
						s_deferredCombinedSymbols.Add((s.name, node.Children.IndexOf(child), temp));
					}
					else
						s.parts.Add(map.symbols[temp]);
				}
			}
		}

		return s;
	}

	public static v9_LineSymbol.Border LoadBorder(XMLNode node, v9_Map map)
	{
		Assert(node.Name == "border");

		v9_LineSymbol.Border b = new();

		XMLAttributeCollection attr = node.Attributes;

		int temp = attr["color"].Parse<int>();
		b.color = map.colors[temp];
		b.width = attr["width"].Parse<int>();
		b.shift = attr["shift"].Parse<int>();
		b.dashed = attr.IsTrue("dashed");
		if (b.dashed)
		{
			b.dash_length = attr["dash_length"].Parse<int>();
			b.break_length = attr["break_length"].Parse<int>();
		}

		return b;
	}

	private static v9_AreaSymbol.FillPattern LoadFillPattern(XMLNode node, v9_Map map)
	{
		Assert(node.Name == "pattern");

		v9_AreaSymbol.FillPattern pattern = new();

		XMLAttributeCollection attr = node.Attributes;
			
		pattern.type = (v9_AreaSymbol.FillPattern.Type)attr["type"].Parse<int>();
		pattern.angle = attr["angle"].Parse<float>();
		if (attr.Exists("no_clipping"))
			pattern.flags = (v9_AreaSymbol.FillPattern.Option)(attr["no_clipping"].Parse<int>() &
															(int)v9_AreaSymbol.FillPattern.Option
																		   .AlternativeToClipping);
		else pattern.flags = 0;
		if (attr.Exists("rotatable"))
			pattern.flags |= v9_AreaSymbol.FillPattern.Option.Rotatable;
		pattern.line_spacing = attr["line_spacing"].Parse<int>();
		pattern.line_offset = attr["line_offset"].Parse<int>();
		pattern.offset_along_line = attr["offset_along_line"].Parse<int>();

		switch (pattern.type)
		{
			case v9_AreaSymbol.FillPattern.Type.LinePattern:
				int temp = attr["color"].Parse<int>();
				pattern.line_color = map.colors[temp];
				pattern.line_width = attr["line_width"].Parse<int>();
				break;
			case v9_AreaSymbol.FillPattern.Type.PointPattern:
				pattern.point_distance = attr["point_distance"].Parse<int>();
				foreach (XMLNode child in node.Children)
				{
					if (child.Name == "symbol")
						pattern.point = (v9_PointSymbol)LoadSymbol(child, map);
				}

				break;
		}

		return pattern;
	}


	public static v9_Object LoadObject(XMLNode node, v9_Map map, v9_Symbol? symbol)
	{
		Assert(node.Name == "object");

		XMLAttributeCollection attr = node.Attributes;

		v9_Object.Type object_type = (v9_Object.Type)attr["type"].Parse<int>();

		v9_Object obj = object_type switch
		{
			v9_Object.Type.Point => new v9_PointObject(),
			v9_Object.Type.Path => new v9_PathObject(),
			v9_Object.Type.Text => new v9_TextObject(),
		};

		obj.type = object_type;
		obj.map = map;

		if (symbol != null)
			obj.symbol = symbol;
		else
		{
			string symbol_id = attr["symbol"];
			bool conversion_ok = int.TryParse(symbol_id, out int id_converted);
			if (conversion_ok)
				obj.symbol = map.symbols[id_converted];
		}

		if (obj.symbol.is_rotatable && attr.Exists("rotation"))
			obj.rotation = attr["rotation"].Parse<double>();

		if (obj.type == v9_Object.Type.Text)
		{
			v9_TextObject text = (v9_TextObject)obj;

			text.h_align = (v9_TextObject.HorizontalAlignment)attr["h_align"].Parse<int>();
			text.v_align = (v9_TextObject.VerticalAlignment)attr["v_align"].Parse<int>();

			obj = text;
		}

		foreach (XMLNode child in node.Children)
		{
			if (child.Name == "coords")
			{
				if (object_type == v9_Object.Type.Text)
				{
					obj.coords = new(ReadCoordsForText(child));

					if (obj.coords.Count > 1)
					{
						((v9_TextObject)obj).size = obj.coords[1];
						((v9_TextObject)obj).has_single_anchor = false;
					}
				}
				else obj.coords = new(ReadCoords(child));
			}
			else if (child.Name == "pattern" && object_type == v9_Object.Type.Path)
			{
				v9_PathObject path = (v9_PathObject)obj;
				path.rotation = attr.Exists("rotation") ? attr["rotation"].Parse<double>() : 0;

				foreach (XMLNode chi in child.Children)
				{
					if (chi.Name == "coord")
						path.pattern_origin = new(chi.Attributes["x"].Parse<int>(), chi.Attributes["y"].Parse<int>());
				}
			}
			else if (child.Name == "text" && object_type == v9_Object.Type.Text)
			{
				v9_TextObject text = (v9_TextObject)obj;
				text.text = child.InnerText;
				text.text = text.text.Replace("\r", "");
			}
			else if (child.Name == "size" && object_type == v9_Object.Type.Text)
			{
				v9_TextObject text = (v9_TextObject)obj;
				int w = child.Attributes["width"].Parse<int>(),
					h = child.Attributes["height"].Parse<int>();

				text.size = new() { xp = w, yp = h };
				text.has_single_anchor = false;
			}
			else if (child.Name == "tags")
				continue; // Set Tags -> no plans to implement
		}

		if (object_type == v9_Object.Type.Path)
		{
			// Recalculate Path Parts
		}

		return obj;
	}

	public static IEnumerable<v9_MapCoord> ReadCoords(XMLNode node)
	{
		List<v9_MapCoord> coords = new();

		string[] c = node.InnerText.Split(';');
		if (c.Last() == "")
			c = c[Range.EndAt(c.Length - 1)];

		foreach (string s in c)
		{
			string[] vals = s.Split(' ');

			v9_MapCoord coord = new();

			coord.xp = vals[0].Parse<int>();
			coord.yp = vals[1].Parse<int>();

			if (vals.Length == 3)
				coord.fp = (v9_MapCoord.Flags)vals[2].Parse<int>();

			coords.Add(coord);
		}

		return coords;
	}

	public static IEnumerable<v9_MapCoord> ReadCoordsForText(XMLNode node)
	{
		// Is this right?
		return ReadCoords(node);
	}

	public static v9_MapPart LoadMapPart(XMLNode node, v9_Map map)
	{
		Assert(node.Name == "part");

		v9_MapPart part = new()
		{
			name = node.Attributes["name"],
			map = map,
			objects = new(),
		};

		foreach (XMLNode child in node.Children.Where(chi => chi.Name == "objects"))
			foreach (XMLNode chi in child.Children.Where(chi => chi.Name == "object"))
				part.objects.Add(LoadObject(chi, map, null));

		return part;
	}

	public static v9_Map LoadMap(XMLNode node)
	{
		v9_Map map = new();

		int version = node.Attributes["version"].Parse<int>();

		if (version < 1)
			throw new ArgumentException("Invalid File Format Version");
		if (version < 2) // Minimum version
			throw new ArgumentException("Unsupported old file format version");
		if (version > 9) // Make version
			Log("Unsupported new file format version");

		ImportElements(ref map, node);

		return map;
	}

	public static void ImportElements(ref v9_Map map, XMLNode node)
	{
		foreach (XMLNode child in node.Children)
		{
			string name = child.Name;

			switch (name)
			{
				case "colors": ImportColors(ref map, child); break;
				case "symbols": ImportSymbols(ref map, child); break;
				case "georeferencing": ImportGeoreferencing(ref map, child); break;
				case "view": ImportView(ref map); break;
				case "barrier": HandleBarrier(ref map, child); break;
				case "notes": ImportMapNotes(ref map, child); break;
				case "parts": ImportMapParts(ref map, child); break;
				case "templates": ImportTemplates(ref map); break;
				case "print": ImportPrint(ref map); break;
			}
		}

	}

	public static void ImportColors(ref v9_Map map, XMLNode node)
	{
		Dictionary<int, v9_Color> colors = new() {
			{ -1, new() { a = 0 } }, // Transparent
			{ -900, new() { k = 1, a = 1, }},  // Registration Black
		};

		foreach (XMLNode child in node.Children)
		{
			if (child.Name != "color") continue;

			v9_Color color = new()
			{
				name = child.Attributes["name"],

				c = child.Attributes["c"].Parse<float>(),
				m = child.Attributes["m"].Parse<float>(),
				y = child.Attributes["y"].Parse<float>(),
				k = child.Attributes["k"].Parse<float>(),
				a = child.Attributes["opacity"].Parse<float>(),
			};

			int id = child.Attributes["priority"].Parse<int>();



			colors.Add(id, color);
		}

		map.colors = colors;
	}

	public static void ImportSymbols(ref v9_Map map, XMLNode node)
	{
		Dictionary<int, v9_Symbol> symbols = new();

		foreach (XMLNode child in node.Children)
		{
			if (child.Name != "symbol") continue;

			v9_Symbol sym = LoadSymbol(child, map);
			int id = child.Attributes["id"].Parse<int>();

			symbols.Add(id, sym);

			Log($"Loaded: {sym.name} / {id}");
		}

		map.symbols = symbols;

		foreach (var item in s_deferredCombinedSymbols)
		{
			v9_Symbol sym = symbols.Values.First(x => x.name == item.name);
			int index = symbols.Values.ToList().IndexOf(sym);

			v9_CombinedSymbol combined = (v9_CombinedSymbol)sym;
			combined.parts[item.index] = map.symbols[item.symbol];

			map.symbols[index] = combined;
		}
	}

	public static void ImportGeoreferencing(ref v9_Map map, XMLNode node)
	{
		if (node.Name != "georeferencing")
			throw new ArgumentException();

		throw new NotImplementedException();
	}

	public static void ImportView(ref v9_Map map)
	{
		throw new NotImplementedException();
	}

	public static void HandleBarrier(ref v9_Map map, XMLNode node)
	{
		if (node.Attributes["version"].Parse<int>() <= 9)
			ImportElements(ref map, node);
		else throw new Exception();
	}

	public static void ImportMapNotes(ref v9_Map map, XMLNode node)
	{
		map.map_notes = node.InnerText;
	}

	public static void ImportMapParts(ref v9_Map map, XMLNode node)
	{
		Dictionary<int, v9_MapPart> parts = new();

		int i = 0;
		foreach (XMLNode child in node.Children)
		{
			v9_MapPart p = LoadMapPart(child, map);

			parts.Add(i, p);
			i++;
		}

		map.parts = parts;
	}

	public static void ImportTemplates(ref v9_Map map)
	{
		throw new NotImplementedException();
	}

	public static void ImportPrint(ref v9_Map map)
	{
		throw new NotImplementedException();
	}
}

file static class v9_LocalSave
{
	public static XMLNode SaveMap(v9_Map map)
	{
		throw new NotImplementedException();
	}
}

file static class v9_MarshallLoad
{
	private static Map s_map;

	public static Map ConvertMap(v9_Map map)
	{
		s_map = new();

		s_map.Colours = new(ConvertColours(map.colors));
		s_map.Symbols = new(ConvertSymbols(map.symbols));
		s_map.Instances = new(ConvertInstances(map.parts));

		return s_map;
	}

	public static IEnumerable<Colour> ConvertColours(Dictionary<int, v9_Color> colors)
	{
		IEnumerable<(int, v9_Color)> cols = colors.Select(x => (x.Key, x.Value));

		cols = cols.OrderBy(x => x.Item1);

		return cols.Select(x => ConvertColour(x.Item2));
	}

	private static readonly Dictionary<v9_Color, Colour> s_cachedColours = new();
	public static Colour ConvertColour(v9_Color c)
	{
		if (s_cachedColours.TryGetValue(c, out Colour? value)) return value!;

		Colour col = new CmykColour(c.name, c.c, c.m, c.y, c.k);

		s_cachedColours.Add(c, col);
		return col;
	}

	public static IEnumerable<Symbol> ConvertSymbols(Dictionary<int, v9_Symbol> symbols)
		=> symbols.Select(x => ConvertSymbol(x.Value));

	public static Symbol ConvertSymbol(v9_Symbol s)
	{
		return s switch
		{
			v9_PointSymbol p => ConvertPointSymbol(p),
			v9_LineSymbol l => ConvertLineSymbol(l),
			v9_AreaSymbol a => ConvertAreaSymbol(a),
			v9_CombinedSymbol c => ConvertCombinedSymbol(c),
			v9_TextSymbol t => ConvertTextSymbol(t),
		};
	}

	public static PointSymbol ConvertPointSymbol(v9_PointSymbol s)
	{
		PointSymbol op = new(s.name, s.description,
								 new((byte)s.number.Item1, (byte)s.number.Item1, (byte)s.number.Item1),
								 false, s.is_helper_symbol,
								 Enumerable.Empty<MapObject>(), s.is_rotatable);

		return op;
	}

	public static LineSymbol ConvertLineSymbol(v9_LineSymbol s)
	{
		LineSymbol op = new(s.name, s.description,
								new((byte)s.number.Item1, (byte)s.number.Item1, (byte)s.number.Item1),
								false, s.is_helper_symbol,
								ConvertColour(s.color), s.line_width / 1000,
								ConvertDashStyle(s.dashed, s.dash_length, s.break_length, s.dashes_in_group, s.in_group_break_length),
								ConvertMidStyle(s.start_offset, s.end_offset, s.mid_symbol_distance, s.mid_symbol), LineStyle.Default,
								ConvertBorderStyle(s.border));

		return op;
	}

	public static AreaSymbol ConvertAreaSymbol(v9_AreaSymbol s)
	{
		AreaSymbol op = new(s.name, s.description,
								new((byte)s.number.Item1, (byte)s.number.Item1, (byte)s.number.Item1),
								false, s.is_helper_symbol,
								Fill(s.patterns, s.color),
								Colour.Transparent, 0f,
								DashStyle.None, MidStyle.None, LineStyle.Default, BorderStyle.None,
								true); // Think all symbols are rotatable

		return op;
	}

	public static Symbol ConvertCombinedSymbol(v9_CombinedSymbol s)
	{
		if (s.parts.Count > 2)
			Log("More than 2 parts found. Discarding anything other that first line & area");

		v9_LineSymbol? line = null;
		if (s.parts.Any(p => p is v9_LineSymbol))
			line = (v9_LineSymbol)s.parts.First(p => p is v9_LineSymbol)!;

		v9_AreaSymbol? area = null;
		if (s.parts.Any(p => p is v9_AreaSymbol))
			area = (v9_AreaSymbol)s.parts.First(p => p is v9_AreaSymbol)!;

		if (line != null && area != null)
		{
			AreaSymbol op = new(s.name, s.description,
									new((byte)s.number.Item1, (byte)s.number.Item1, (byte)s.number.Item1),
									false, s.is_helper_symbol,
									Fill(area.patterns, area.color),
									ConvertColour(line.color), line.line_width / 1000,
									ConvertDashStyle(line.dashed, line.dash_length, line.break_length, line.dashes_in_group, line.in_group_break_length),
									ConvertMidStyle(line.start_offset, line.end_offset, line.mid_symbol_distance, line.mid_symbol), LineStyle.Default,
									ConvertBorderStyle(line.border),
									true);

			return op;
		}

		if (line != null)
		{
			line.name = s.name;
			line.description = s.description;
			line.number = s.number;
			line.is_rotatable = s.is_rotatable;
			line.is_helper_symbol = s.is_helper_symbol;
			line.is_hidden = s.is_hidden;
			line.is_protected = s.is_protected;

			return ConvertLineSymbol(line);
		}

		if (area != null)
		{
			area.name = s.name;
			area.description = s.description;
			area.number = s.number;
			area.is_rotatable = s.is_rotatable;
			area.is_helper_symbol = s.is_helper_symbol;
			area.is_hidden = s.is_hidden;
			area.is_protected = s.is_protected;

			return ConvertAreaSymbol(area);
		}

		throw new Exception("Must contain either line or area symbol");
	}

	public static TextSymbol ConvertTextSymbol(v9_TextSymbol s)
	{
		FontStyle fS = new(s.bold, s.underline, false,
							   s.italic ? ItalicsMode.Italic : ItalicsMode.None);

		Font f = new(s.font_family,
						 ConvertColour(s.color), s.font_size / 1000,
						 s.line_spacing, s.paragraph_spacing / 1000, s.character_spacing, fS);

		TextSymbol op = new(s.name, s.description,
								new((byte)s.number.Item1, (byte)s.number.Item1, (byte)s.number.Item1),
								false, s.is_helper_symbol,
								f, true,
								Colour.Transparent, 0f,
								s.framing ? ConvertColour(s.framing_color) : Colour.Transparent,
								s.framing ? s.framing_line_half_width / 500 : 0f);

		return op;
	}

	public static DashStyle ConvertDashStyle(bool dashed, int dashLength, int breakLength, int dashesInGroup, int inGroupBreakLength)
	{
		DashStyle d = new()
		{
			HasDash = dashed,

			DashLength = dashLength / 1000,
			GapLength = breakLength / 1000,
			GroupSize = dashesInGroup,
			GroupGapLength = inGroupBreakLength / 1000,
		};

		return d;
	}

	public static MidStyle ConvertMidStyle(int startOffset, int endOffset, int midSymbolDistance,
										v9_PointSymbol? midSymbol)
	{
		if (midSymbol is null || midSymbolDistance == 0)
			return MidStyle.None;

		MidStyle m = new()
		{
			HasMid = midSymbol is not null,

			MapObjects = new(ConvertPointSymbolToMapObjects(midSymbol)),

			GapLength = midSymbolDistance / 1000,

			InitialOffset = startOffset / 1000,
			EndOffset = endOffset / 1000,
		};

		return m;
	}

	public static BorderStyle ConvertBorderStyle(v9_LineSymbol.Border b)
	{
		BorderStyle border = new(ConvertColour(b.color), b.width / 1000, b.shift / 1000, DashStyle.None, MidStyle.None);

		if (b.dashed)
			border.DashStyle = new(b.dash_length / 1000, b.break_length / 1000);

		return border;
	}


	public static IFill Fill(List<v9_AreaSymbol.FillPattern> patterns, v9_Color color)
	{
		if (patterns.Count == 0 && color.a == 0)
			return new SolidFill(Colour.Transparent);

		if (patterns.Count == 1 && color.a == 0)
		{
			v9_AreaSymbol.FillPattern f = patterns.First();
			IFill fill = f.type switch
			{
				v9_AreaSymbol.FillPattern.Type.LinePattern => ConvertPatternFill(f),
				v9_AreaSymbol.FillPattern.Type.PointPattern => ConvertObjectFill(f),
			};

			return fill;
		}

		if (patterns.Count == 0 && color.a != 0)
			return new SolidFill(ConvertColour(color));

		List<IFill> combFills = new();

		if (color.a != 0)
			combFills.Add(new SolidFill(ConvertColour(color)));

		foreach (var f in patterns)
		{
			combFills.Add(f.type switch
			{
				v9_AreaSymbol.FillPattern.Type.LinePattern => ConvertPatternFill(f),
				v9_AreaSymbol.FillPattern.Type.PointPattern => ConvertObjectFill(f),
			});
		}

		Assert(combFills.Count != 0);

		return new CombinedFill(combFills);
	}

	public static PatternFill ConvertPatternFill(v9_AreaSymbol.FillPattern fill)
	{
		Assert(fill.type == v9_AreaSymbol.FillPattern.Type.LinePattern, "Fill must be Line Pattern");

		PatternFill op = new(fill.line_width / 1000, fill.line_spacing / 1000, ConvertColour(fill.line_color),
								 Colour.Transparent, MathF.PI - fill.angle);

		return op;
	}

	public static SpacedObjectFill ConvertObjectFill(v9_AreaSymbol.FillPattern fill)
	{
		Assert(fill.type == v9_AreaSymbol.FillPattern.Type.PointPattern);

		bool isClipped = fill.flags.HasFlag(v9_AreaSymbol.FillPattern.Option.Default |
											v9_AreaSymbol.FillPattern.Option.NoClippingIfCenterInside |
											v9_AreaSymbol.FillPattern.Option.NoClippingIfCompletelyInside);


		SpacedObjectFill op = new(ConvertPointSymbolToMapObjects(fill.point), isClipped,
									  (fill.point_distance / 1000, fill.line_spacing / 1000),
									  (fill.offset_along_line / 1000, fill.line_offset),
									  MathF.PI - fill.angle);

		return op;
	}

	public static IEnumerable<MapObject> ConvertPointSymbolToMapObjects(v9_PointSymbol s)
	{
		return Enumerable.Empty<MapObject>();
	}

	public static MapObject ConvertMapObject(v9_PointSymbol.Element el)
	{
		return el.symbol switch
		{
			v9_PointSymbol => ConvertPointObject(el),
			v9_LineSymbol => ConvertLineObject(el),
			v9_AreaSymbol => ConvertAreaObject(el),
			v9_CombinedSymbol => ConvertCombinedObject(el),
		};
	}

	public static PointObject ConvertPointObject(v9_PointSymbol.Element el)
	{
		Assert(el.symbol is v9_PointSymbol && el.objec is v9_PointObject);

		v9_PointSymbol sym = (v9_PointSymbol)el.symbol;
		v9_PointObject obj = (v9_PointObject)el.objec;

		PointObject pObj = new(ConvertColour(sym.inner_color), ConvertColour(sym.outer_color),
								   sym.inner_radius / 1000, sym.outer_width / 1000);

		return pObj;
	}

	public static LineObject ConvertLineObject(v9_PointSymbol.Element el)
	{
		Assert(el.symbol is v9_LineSymbol && el.objec is v9_PathObject);

		v9_LineSymbol sym = (v9_LineSymbol)el.symbol;
		v9_PathObject obj = (v9_PathObject)el.objec;

		PathCollection pC = ConvertPathWithoutHoles(obj.coords);

		LineObject lObj = new(pC, sym.line_width / 1000, ConvertColour(sym.color), false);

		return lObj;
	}

	public static AreaObject ConvertAreaObject(v9_PointSymbol.Element el)
	{
		Assert(el.symbol is v9_AreaSymbol && el.objec is v9_PathObject);

		v9_AreaSymbol sym = (v9_AreaSymbol)el.symbol;
		v9_PathObject obj = (v9_PathObject)el.objec;

		PathCollection pC = ConvertPathWithoutHoles(obj.coords);

		AreaObject aObj = new(pC, 0, Colour.Transparent, new SolidFill(ConvertColour(sym.color)));

		return aObj;
	}

	public static AreaObject ConvertCombinedObject(v9_PointSymbol.Element el)
	{
		Assert(el.symbol is v9_AreaSymbol && el.objec is v9_PathObject);

		v9_CombinedSymbol sym = (v9_CombinedSymbol)el.symbol;
		v9_PathObject obj = (v9_PathObject)el.objec;

		v9_LineSymbol border = (v9_LineSymbol)sym.parts.First(x => x is v9_LineSymbol)!;
		v9_AreaSymbol fill = (v9_AreaSymbol)sym.parts.First(x => x is v9_AreaSymbol)!;

		PathCollection pC = ConvertPathWithoutHoles(obj.coords);

		return new(pC, border.line_width / 1000, ConvertColour(border.color), new SolidFill(ConvertColour(fill.color)));
	}

	public static IEnumerable<Instance> ConvertInstances(Dictionary<int, v9_MapPart> parts)
	{
		foreach (KeyValuePair<int, v9_MapPart> p in parts)
			foreach (v9_Object i in p.Value.objects)
				yield return ConvertInstance(i, p.Key);
	}

	public static Instance ConvertInstance(v9_Object obj, int part)
	{
		return obj switch
		{
			v9_PointObject point => ConvertPointInstance(point, part),
			v9_PathObject path => ConvertPathInstance(path, part),
			v9_TextObject text => ConvertTextInstance(text, part),
		};
	}

	public static PointInstance ConvertPointInstance(v9_PointObject obj, int part)
	{
		// Instead of converting entire symbol uses the previously created one
		PointSymbol sym = (PointSymbol)s_map.Symbols[obj.symbol.name];

		vec2 pos = ConvertMapCoordSimple(obj.coords[0]);

		return new(part, sym, pos, (float)(MathF.PI - obj.rotation));
	}

	public static PathInstance ConvertPathInstance(v9_PathObject obj, int part)
	{
		var pC = ConvertPathCollection(obj.coords);

		return obj.symbol switch
		{
			v9_LineSymbol => new LineInstance(part, (LineSymbol)s_map.Symbols[obj.symbol.name], pC.path,
													 pC.isClosed),
			v9_AreaSymbol => new AreaInstance(part, (AreaSymbol)s_map.Symbols[obj.symbol.name], pC.path,
													 pC.isClosed, (float)(MathF.PI - obj.rotation), pC.holes),
			v9_CombinedSymbol => ConvertCombinedInstance(pC, obj, part),
			_ => throw new ArgumentException("Invalid Symbol Type"),
		};
	}

	private static TextInstance ConvertTextInstance(v9_TextObject text, int part)
	{
		HorizontalAlignment align = text.h_align switch
		{
			v9_TextObject.HorizontalAlignment.AlignLeft => HorizontalAlignment.Left,
			v9_TextObject.HorizontalAlignment.AlignHCenter => HorizontalAlignment.Centre,
			v9_TextObject.HorizontalAlignment.AlignRight => HorizontalAlignment.Right,
		};

		vec2 topLeft;
		topLeft = text.has_single_anchor ?
			ConvertMapCoordSimple(text.coords[0]) :
			ConvertMapCoordSimple(text.coords[0]); // Will change at somepoint


		TextInstance op = new(part, (TextSymbol)s_map.Symbols[text.symbol.name], text.text, topLeft, align,
								  (float)(MathF.PI - text.rotation));

		return op;
	}

	public static PathInstance ConvertCombinedInstance((PathCollection path, IEnumerable<PathCollection> holes, bool isClosed) inp, v9_PathObject obj, int part)
	{
		return s_map.Symbols[obj.symbol.name] switch
		{
			LineSymbol line => new LineInstance(part, line, inp.path, inp.isClosed),
			AreaSymbol area => new AreaInstance(part, area, inp.path, inp.isClosed,
														(float)(MathF.PI - obj.rotation), inp.holes),
			_ => throw new ArgumentException(),
		};
	}

	public static (PathCollection path, IEnumerable<PathCollection> holes, bool isClosed) ConvertPathCollection(IList<v9_MapCoord> coords)
	{
		/*
		 *  Number of holes is n-1 occurences of the 16 flag
		 *  Number of bezier points is n occurences of the 1 flag
		 */

		IEnumerable<PathCollection> convHoles = Enumerable.Empty<PathCollection>();
		PathCollection pC = new();
		bool isClosed;

		int holeCount = coords.Count(c => c.fp.HasFlag(v9_MapCoord.Flags.HolePoint));


		if (holeCount > 1)
		{
			int index = coords.IndexOf(coords.First(c => c.fp.HasFlag(v9_MapCoord.Flags.HolePoint)));
			List<v9_MapCoord> allHoles = coords.Skip(index + 1).ToList();

			List<IList<v9_MapCoord>> holes = new();

			for (int i = 0; i < allHoles.Count; i++)
			{
				List<v9_MapCoord> hole = new();

				for (; i < allHoles.Count; i++)
				{
					if (allHoles[i].fp.HasFlag(v9_MapCoord.Flags.HolePoint))
					{
						hole.Add(allHoles[i]);
						break;
					}

					hole.Add(allHoles[i]);
				}

				holes.Add(hole);
			}

			convHoles = holes.Select(ConvertPathWithoutHoles);

			pC = ConvertPathWithoutHoles(coords.Take(new Range(0, index + 1)).ToList());

			isClosed = coords[index].fp.HasFlag(v9_MapCoord.Flags.ClosePoint);
		}
		else
		{
			// No Holes

			pC = ConvertPathWithoutHoles(coords);

			isClosed = coords[^1].fp.HasFlag(v9_MapCoord.Flags.ClosePoint);
		}

		return (pC, convHoles, isClosed);
	}

	public static PathCollection ConvertPathWithoutHoles(IList<v9_MapCoord> coords)
	{
		PathCollection pC = new();

		bool currOnBezier = coords[0].fp.HasFlag(v9_MapCoord.Flags.CurveStart);

		List<BezierPoint> currBezier = new();
		List<vec2> currLinear = new();

		int i = 0;
		while (i < coords.Count)
		{
			if (currOnBezier)
			{
				if (i == 0)
				{
					currBezier.Add(new(
									   anchor: ConvertMapCoordSimple(coords[0]),
									   earlyControl: null,
									   lateControl: ConvertMapCoordSimple(coords[1])
									   ));

					if (coords[i + 3].fp.HasFlag(v9_MapCoord.Flags.CurveStart))
					{
						currOnBezier = true;
						i += 3;
					}
					else // Don't actually think this is possible
					{
						currOnBezier = false;
						pC.Add(new BezierPath(currBezier));
						currBezier.Clear();

						i += 2;
					}
				}
				else
				{
					// Last
					if (i == coords.Count - 1)
					{
						currBezier.Add(new(
										   anchor: ConvertMapCoordSimple(coords[i]),
										   earlyControl: ConvertMapCoordSimple(coords[i - 1]),
										   lateControl: null
										   ));

						continue; // Skips over rest
					}

					// Next bezier anchor
					if (coords[i + 3].fp.HasFlag(v9_MapCoord.Flags.CurveStart))
					{
						// First Bezier in sequence
						if (currBezier.Count == 0)
						{
							currBezier.Add(new(
											   anchor: ConvertMapCoordSimple(coords[i]),
											   earlyControl: null,
											   lateControl: ConvertMapCoordSimple(coords[i + 1])
											   ));
						}
						else
						{
							currBezier.Add(new(
											   anchor: ConvertMapCoordSimple(coords[i]),
											   earlyControl: ConvertMapCoordSimple(coords[i - 1]),
											   lateControl: ConvertMapCoordSimple(coords[i + 1])
											   ));
						}

						currOnBezier = true;
						i += 3;
					}
					else // Next is not a bezier
					{
						currBezier.Add(new(
										   anchor: ConvertMapCoordSimple(coords[i]),
										   earlyControl: ConvertMapCoordSimple(coords[i - 1]),
										   lateControl: ConvertMapCoordSimple(coords[i + 1])
										   ));

						currBezier.Add(new(
										   anchor: ConvertMapCoordSimple(coords[i + 3]),
										   earlyControl: ConvertMapCoordSimple(coords[i + 2]),
										   lateControl: null
										   ));

						// Next is at the end
						if (i + 3 != coords.Count - 1)
						{
							currOnBezier = false;
							pC.Add(new BezierPath(currBezier));
							currBezier.Clear();

						}

						i += 4;
					}
				}
			}
			else
			{
				currLinear.Add(ConvertMapCoordSimple(coords[i]));

				if (i != coords.Count - 1 && coords[i + 1].fp.HasFlag(v9_MapCoord.Flags.CurveStart))
				{
					currOnBezier = true;
					pC.Add(new LinearPath(currLinear));
					currLinear.Clear();

				}

				i++;
			}
		}

		if (currOnBezier)
			pC.Add(new BezierPath(currBezier));
		else
			pC.Add(new LinearPath(currLinear));

		return pC;
	}

	public static vec2 ConvertMapCoordSimple(v9_MapCoord coord)
	{
		return (new vec2(coord.xp, coord.yp)) / 1000;
	}
}

#endregion

file static class _Extensions
{
	public static bool IsTrue(this XMLAttributeCollection attr, string name)
	{
		return attr.Exists(name) && attr[name] == "true";
	}

	public static bool IsNotFalse(this XMLAttributeCollection attr, string name)
	{
		return attr.Exists(name) && attr[name] != "False";
	}
}
