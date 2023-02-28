using OTools.Maps;
using OTools.Common;

using _ = OTools.Maps;
using Sunley.Mathematics;

namespace OTools.FileFormatConverter;

public static class OmapLoader
{
    public static Map Load(XMLDocument doc)
    {
        _Map map = _RawLoad.LoadMap(doc.Root);

        Map convMap = _MarshallLoad.ConvertMap(map);

        return convMap;
    }
}

#region Objects

file struct _Color
{
    public string name;
    public float c, m, y, k;
    public float a;

    public _Color()
    {
        a = 1;

        name = string.Empty;
        c = 0;
        m = 0;
        y = 0;
        k = 0;
    }
}

#region Symbols 

file abstract class _Symbol
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

file class _PointSymbol : _Symbol
{
    public int inner_radius, outer_width;
    public _Color inner_color, outer_color;
    public List<Element> elements = new();

    public struct Element
    {
        public _Symbol symbol;
        public _Object objec;
    }
}

file class _LineSymbol : _Symbol
{
    public Border border, right_border;
    public _PointSymbol start_symbol, mid_symbol, end_symbol, dash_symbol;
    public _Color color;

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
        public _Color color;
        public int width, shift, dash_length, break_length;
        public bool dashed;
    }
}

file class _AreaSymbol : _Symbol
{
    public List<FillPattern> patterns = new();
    public _Color color;
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
        public _Color line_color;
        public int line_width;

        // For Type_t.Point
        public int offset_along_line, point_distance;
        public _PointSymbol point;
    }
}

file class _TextSymbol : _Symbol
{
    public string font_family, icon_text;
    public _Color color, framing_color, line_below_color;
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

file class _CombinedSymbol : _Symbol
{
    public List<_Symbol?> parts = new();
    public List<bool> private_parts = new();
    public List<int> temp_part_indices = new();
}

#endregion

#region Objects

file abstract class _Object
{
    public enum Type
    {
        Point,
        Path,
        Text = 4,
    }

    public Type type;
    public _Symbol symbol;
    public _Map map;

    public List<_MapCoord> coords;
    public double rotation;
}

file class _PointObject : _Object
{

}

file class _PathObject : _Object
{
    public vec2i pattern_origin;
}

file class _TextObject : _Object
{
    public string text;
    public HorizontalAlignment h_align;
    public VerticalAlignment v_align;
    public bool has_single_anchor = true;
    public _MapCoord size;

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

#endregion

file class _Map
{
    public Dictionary<int, _Color> colors;
    public Dictionary<int, _Symbol> symbols;
    public Dictionary<int, _MapPart> parts;

    public string map_notes;

    public _Map()
    {
        colors = new();
        symbols = new();
        parts = new();
    }
}

file class _MapPart
{
    public string name;
    public _Map map;
    public List<_Object> objects;
}

file class _MapCoord
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

file static class _RawLoad
{
    private static readonly List<(string name, int index, int symbol)> s_deferredCombinedSymbols = new();

    public static _Symbol LoadSymbol(XMLNode node, _Map map)
    {
        if (node.Name != "symbol")
            throw new ArgumentException("Expected symbol node");

        bool addable;

        XMLAttributeCollection attr = node.Attributes;

        _Symbol.Type symbol_type = (_Symbol.Type)int.Parse(attr["type"]);
        _Symbol s = symbol_type switch
        {
            _Symbol.Type.Point => new _PointSymbol(),
            _Symbol.Type.Line => new _LineSymbol(),
            _Symbol.Type.Area => new _AreaSymbol(),
            _Symbol.Type.Text => new _TextSymbol(),
            _Symbol.Type.Combined => new _CombinedSymbol(),
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
                1 => (int.Parse(split[0]), 0, 0),
                2 => (int.Parse(split[0]), int.Parse(split[1]), 0),
                3 => (int.Parse(split[0]), int.Parse(split[1]), int.Parse(split[2])),
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
                    _Symbol.Type.Point => LoadPointSymbol(child, map, (_PointSymbol)s),
                    _Symbol.Type.Line => LoadLineSymbol(child, map, (_LineSymbol)s),
                    _Symbol.Type.Area => LoadAreaSymbol(child, map, (_AreaSymbol)s),
                    _Symbol.Type.Text => LoadTextSymbol(child, map, (_TextSymbol)s),
                    _Symbol.Type.Combined => LoadCombinedSymbol(child, map, (_CombinedSymbol)s),
                    _ => s,
                };
            }
        }

        return s;
    }

    public static _PointSymbol LoadPointSymbol(XMLNode node, _Map map, _PointSymbol s)
    {
        Assert(node.Name == "point_symbol");

        XMLAttributeCollection attr = node.Attributes;

        s.is_rotatable = attr.IsTrue("rotatable");
        s.inner_radius = int.Parse(attr["inner_radius"]);
        int temp = int.Parse(attr["inner_color"]);
        s.inner_color = map.colors[temp];
        s.outer_width = int.Parse(attr["outer_width"]);
        temp = int.Parse(attr["outer_color"]);
        s.outer_color = map.colors[temp];

        foreach (XMLNode child in node.Children)
        {
            if (child.Name == "element")
            {
                _Symbol? el = null;

                foreach (XMLNode chi in child.Children)
                {
                    if (chi.Name == "symbol" && el is null)
                        el = LoadSymbol(chi, map);
                    else if (chi.Name == "object" && el is not null)
                    {
                        _Object o = LoadObject(chi, map, el);
                        s.elements.Add(new() { symbol = el, objec = o });
                    }
                }
            }
        }

        return s;
    }

    public static _LineSymbol LoadLineSymbol(XMLNode node, _Map map, _LineSymbol s)
    {
        Assert(node.Name == "line_symbol");

        XMLAttributeCollection attr = node.Attributes;

        int temp = int.Parse(attr["color"]);
        s.color = map.colors[temp];
        s.line_width = int.Parse(attr["line_width"]);
        s.minimum_length = int.Parse(attr["minimum_length"]);
        s.join_style = (_LineSymbol.JoinStyle)int.Parse(attr["join_style"]);
        s.cap_style = (_LineSymbol.CapStyle)int.Parse(attr["cap_style"]);

        if (attr.Exists("start_offset") || attr.Exists("end_offset"))
        {
            s.start_offset = int.Parse(attr["start_offset"]);
            s.end_offset = int.Parse(attr["end_offset"]);
        }
        else if (s.cap_style == _LineSymbol.CapStyle.PointedCap)
        {
            s.start_offset = s.end_offset = int.Parse(attr["pointed_cap_length"]);
        }

        s.dashed = attr.IsTrue("dashed");
        s.segment_length = int.Parse(attr["segment_length"]);
        s.end_length = int.Parse(attr["end_length"]);
        s.show_at_least_one_symbol = attr.IsTrue("show_at_least_one_symbol");
        s.minimum_mid_symbol_count = int.Parse(attr["minimum_mid_symbol_count"]);
        s.minimum_mid_symbol_count_when_closed = int.Parse(attr["minimum_mid_symbol_count_when_closed"]);
        s.dash_length = int.Parse(attr["dash_length"]);
        s.break_length = int.Parse(attr["break_length"]);
        s.dashes_in_group = int.Parse(attr["dashes_in_group"]);
        s.in_group_break_length = int.Parse(attr["in_group_break_length"]);
        s.half_outer_dashes = attr.IsTrue("half_outer_dashes");
        s.mid_symbols_per_spot = int.Parse(attr["mid_symbols_per_spot"]);
        s.mid_symbol_distance = int.Parse(attr["mid_symbol_distance"]);
        s.mid_symbol_placement = (_LineSymbol.MidSymbolPlacement)(attr.Exists("mid_symbol_placement") ? int.Parse(attr["mid_symbol_placement"]) : 99);
        s.suppress_dash_symbol_at_ends = attr.IsTrue("suppress_dash_symbol_at_ends");
        s.scale_dash_symbol = attr.IsNotFalse("scale_dash_symbol");

        s.have_border_lines = false;
        foreach (XMLNode child in node.Children)
        {
            switch (child.Name)
            {
                case "start_symbol":
                    s.start_symbol = (_PointSymbol)LoadSymbol(child.Children[0], map);
                    break;
                case "mid_symbol":
                    s.mid_symbol = (_PointSymbol)LoadSymbol(child.Children[0], map);
                    break;
                case "end_symbol":
                    s.end_symbol = (_PointSymbol)LoadSymbol(child.Children[0], map);
                    break;
                case "dash_symbol":
                    s.dash_symbol = (_PointSymbol)LoadSymbol(child.Children[0], map);
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

    public static _AreaSymbol LoadAreaSymbol(XMLNode node, _Map map, _AreaSymbol s)
    {
        Assert(node.Name == "area_symbol");

        XMLAttributeCollection attr = node.Attributes;

        int temp = int.Parse(attr["inner_color"]);
        s.color = map.colors[temp];
        s.minimum_area = int.Parse(attr["min_area"]);

        foreach (XMLNode child in node.Children)
        {
            if (child.Name == "pattern")
                s.patterns.Add(LoadFillPattern(child, map));
        }

        return s;
    }

    public static _TextSymbol LoadTextSymbol(XMLNode node, _Map map, _TextSymbol s)
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
                s.font_size = int.Parse(child.Attributes["size"]);
                s.bold = child.Attributes.IsTrue("bold");
                s.italic = child.Attributes.IsTrue("italic");
                s.underline = child.Attributes.IsTrue("underline");
            }
            else if (child.Name == "text")
            {
                int temp = int.Parse(child.Attributes["color"]);
                s.color = map.colors[temp];
                s.line_spacing = int.Parse(child.Attributes["line_spacing"]);
                s.paragraph_spacing = int.Parse(child.Attributes["paragraph_spacing"]);
                s.character_spacing = float.Parse(child.Attributes["character_spacing"]);
                s.kerning = child.Attributes.IsTrue("kerning");
            }
            else if (child.Name == "framing")
            {
                s.framing = true;
                int temp = int.Parse(child.Attributes["color"]);
                s.framing_color = map.colors[temp];
                s.framing_mode = (_TextSymbol.FramingMode)(int.Parse(child.Attributes["mode"]));
                s.framing_line_half_width = int.Parse(child.Attributes["line_half_width"]);
                s.framing_shadow_x_offset = int.Parse(child.Attributes["shadow_x_offset"]);
                s.framing_shadow_y_offset = int.Parse(child.Attributes["shadow_y_offset"]);
            }
            else if (child.Name == "line_below")
            {
                s.line_below = true;
                int temp = int.Parse(child.Attributes["color"]);
                s.line_below_color = map.colors[temp];
                s.line_below_width = int.Parse(child.Attributes["width"]);
                s.line_below_distance = int.Parse(child.Attributes["distance"]);
            }
            else if (child.Name == "tabs")
            {
                int num_custom_tabs = int.Parse(child.Attributes["count"]);

                s.custom_tabs.AddRange(child.Children.Where(c => c.Name == "tab").Select(c => int.Parse(c.InnerText)));
            }
        }

        return s;
    }

    public static _CombinedSymbol LoadCombinedSymbol(XMLNode node, _Map map, _CombinedSymbol s)
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
                    int temp = int.Parse(child.Attributes["symbol"]);
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

    public static _LineSymbol.Border LoadBorder(XMLNode node, _Map map)
    {
        Assert(node.Name == "border");

        _LineSymbol.Border b = new();

        XMLAttributeCollection attr = node.Attributes;

        int temp = int.Parse(attr["color"]);
        b.color = map.colors[temp];
        b.width = int.Parse(attr["width"]);
        b.shift = int.Parse(attr["shift"]);
        b.dashed = attr.IsTrue("dashed");
        if (b.dashed)
        {
            b.dash_length = int.Parse(attr["dash_length"]);
            b.break_length = int.Parse(attr["break_length"]);
        }

        return b;
    }

    private static _AreaSymbol.FillPattern LoadFillPattern(XMLNode node, _Map map)
    {
        Assert(node.Name == "pattern");

        _AreaSymbol.FillPattern pattern = new();

        XMLAttributeCollection attr = node.Attributes;

        pattern.type = (_AreaSymbol.FillPattern.Type)int.Parse(attr["type"]);
        pattern.angle = float.Parse(attr["angle"]);
        if (attr.Exists("no_clipping"))
            pattern.flags = (_AreaSymbol.FillPattern.Option)(int.Parse(attr["no_clipping"]) &
                                                            (int)_AreaSymbol.FillPattern.Option
                                                                           .AlternativeToClipping);
        else pattern.flags = 0;
        if (attr.Exists("rotatable"))
            pattern.flags |= _AreaSymbol.FillPattern.Option.Rotatable;
        pattern.line_spacing = int.Parse(attr["line_spacing"]);
        pattern.line_offset = int.Parse(attr["line_offset"]);
        pattern.offset_along_line = int.Parse(attr["offset_along_line"]);

        switch (pattern.type)
        {
            case _AreaSymbol.FillPattern.Type.LinePattern:
                int temp = int.Parse(attr["color"]);
                pattern.line_color = map.colors[temp];
                pattern.line_width = int.Parse(attr["line_width"]);
                break;
            case _AreaSymbol.FillPattern.Type.PointPattern:
                pattern.point_distance = int.Parse(attr["point_distance"]);
                foreach (XMLNode child in node.Children)
                {
                    if (child.Name == "symbol")
                        pattern.point = (_PointSymbol)LoadSymbol(child, map);
                }

                break;
        }

        return pattern;
    }


    public static _Object LoadObject(XMLNode node, _Map map, _Symbol? symbol)
    {
        Assert(node.Name == "object");

        XMLAttributeCollection attr = node.Attributes;

        _Object.Type object_type = (_Object.Type)int.Parse(attr["type"]);

        _Object obj = object_type switch
        {
            _Object.Type.Point => new _PointObject(),
            _Object.Type.Path => new _PathObject(),
            _Object.Type.Text => new _TextObject(),
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
            obj.rotation = double.Parse(attr["rotation"]);

        if (obj.type == _Object.Type.Text)
        {
            _TextObject text = (_TextObject)obj;

            text.h_align = (_TextObject.HorizontalAlignment)(int.Parse(attr["h_align"]));
            text.v_align = (_TextObject.VerticalAlignment)(int.Parse(attr["v_align"]));

            obj = text;
        }

        foreach (XMLNode child in node.Children)
        {
            if (child.Name == "coords")
            {
                if (object_type == _Object.Type.Text)
                {
                    obj.coords = new(ReadCoordsForText(child));

                    if (obj.coords.Count > 1)
                    {
                        ((_TextObject)obj).size = obj.coords[1];
                        ((_TextObject)obj).has_single_anchor = false;
                    }
                }
                else obj.coords = new(ReadCoords(child));
            }
            else if (child.Name == "pattern" && object_type == _Object.Type.Path)
            {
                _PathObject path = (_PathObject)obj;
                path.rotation = attr.Exists("rotation") ? double.Parse(attr["rotation"]) : 0;

                foreach (XMLNode chi in child.Children)
                {
                    if (chi.Name == "coord")
                        path.pattern_origin = new(int.Parse(chi.Attributes["x"]), int.Parse(chi.Attributes["y"]));
                }
            }
            else if (child.Name == "text" && object_type == _Object.Type.Text)
            {
                _TextObject text = (_TextObject)obj;
                text.text = child.InnerText;
                text.text = text.text.Replace("\r", "");
            }
            else if (child.Name == "size" && object_type == _Object.Type.Text)
            {
                _TextObject text = (_TextObject)obj;
                int w = int.Parse(child.Attributes["width"]),
                    h = int.Parse(child.Attributes["height"]);

                text.size = new() { xp = w, yp = h };
                text.has_single_anchor = false;
            }
            else if (child.Name == "tags")
                continue; // Set Tags -> no plans to implement
        }

        if (object_type == _Object.Type.Path)
        {
            // Recalculate Path Parts
        }

        return obj;
    }

    public static IEnumerable<_MapCoord> ReadCoords(XMLNode node)
    {
        List<_MapCoord> coords = new();

        string[] c = node.InnerText.Split(';');
        if (c.Last() == "")
            c = c[Range.EndAt(c.Length - 1)];

        foreach (string s in c)
        {
            string[] vals = s.Split(' ');

            _MapCoord coord = new();

            coord.xp = int.Parse(vals[0]);
            coord.yp = int.Parse(vals[1]);

            if (vals.Length == 3)
                coord.fp = (_MapCoord.Flags)int.Parse(vals[2]);

            coords.Add(coord);
        }

        return coords;
    }

    public static IEnumerable<_MapCoord> ReadCoordsForText(XMLNode node)
    {
        // Is this right?
        return ReadCoords(node);
    }

    public static _MapPart LoadMapPart(XMLNode node, _Map map)
    {
        Assert(node.Name == "part");

        _MapPart part = new()
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

    public static _Map LoadMap(XMLNode node)
    {
        _Map map = new();

        int version = int.Parse(node.Attributes["version"]);

        if (version < 1)
            throw new ArgumentException("Invalid File Format Version");
        if (version < 2) // Minimum version
            throw new ArgumentException("Unsupported old file format version");
        if (version > 9) // Make version
            Log("Unsupported new file format version");

        ImportElements(ref map, node);

        return map;
    }

    #region Import

    public static void ImportElements(ref _Map map, XMLNode node)
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

    public static void ImportColors(ref _Map map, XMLNode node)
    {
        Dictionary<int, _Color> colors = new() {
            { -1, new() { a = 0 } }, // Transparent
			{ -900, new() { k = 1, a = 1, }},  // Registration Black
		};

        foreach (XMLNode child in node.Children)
        {
            if (child.Name != "color") continue;

            _Color color = new()
            {
                name = child.Attributes["name"],

                c = float.Parse(child.Attributes["c"]),
                m = float.Parse(child.Attributes["m"]),
                y = float.Parse(child.Attributes["y"]),
                k = float.Parse(child.Attributes["k"]),

                a = float.Parse(child.Attributes["opacity"]),
            };

            int id = int.Parse(child.Attributes["priority"]);



            colors.Add(id, color);
        }

        map.colors = colors;
    }

    public static void ImportSymbols(ref _Map map, XMLNode node)
    {
        Dictionary<int, _Symbol> symbols = new();

        foreach (XMLNode child in node.Children)
        {
            if (child.Name != "symbol") continue;

            _Symbol sym = LoadSymbol(child, map);
            int id = int.Parse(child.Attributes["id"]);

            symbols.Add(id, sym);

            Log($"Loaded: {sym.name} / {id}");
        }

        map.symbols = symbols;

        foreach (var item in s_deferredCombinedSymbols)
        {
            _Symbol sym = symbols.Values.First(x => x.name == item.name);
            int index = symbols.Values.ToList().IndexOf(sym);

            _CombinedSymbol combined = (_CombinedSymbol)sym;
            combined.parts[item.index] = map.symbols[item.symbol];

            map.symbols[index] = combined;
        }
    }

    public static void ImportGeoreferencing(ref _Map map, XMLNode node)
    {
        if (node.Name != "georeferencing")
            throw new ArgumentException();

        // Not Implemented Yet
    }

    public static void ImportView(ref _Map map)
    {
        // Not Implemented Yet
    }

    public static void HandleBarrier(ref _Map map, XMLNode node)
    {
        if (int.Parse(node.Attributes["version"]) <= 9)
            ImportElements(ref map, node);
        else throw new Exception();
    }

    public static void ImportMapNotes(ref _Map map, XMLNode node)
    {
        map.map_notes = node.InnerText;
    }

    public static void ImportMapParts(ref _Map map, XMLNode node)
    {
        Dictionary<int, _MapPart> parts = new();

        int i = 0;
        foreach (XMLNode child in node.Children)
        {
            _MapPart p = LoadMapPart(child, map);

            parts.Add(i, p);
            i++;
        }

        map.parts = parts;
    }

    public static void ImportTemplates(ref _Map map)
    {
        // Not Implemented Yet
    }

    public static void ImportPrint(ref _Map map)
    {
        // Not Implemented Yet
    }

    #endregion
}

file static class _RawSave
{
    public static XMLNode SaveMap(_Map map)
    {
        throw new NotImplementedException();
    }
}

file static class _MarshallLoad
{
    private static Map s_map;

    public static Map ConvertMap(_Map map)
    {
        s_map = new();

        s_map.Colours = new(ConvertColours(map.colors));
        s_map.Symbols = new(ConvertSymbols(map.symbols));
        s_map.Instances = new(ConvertInstances(map.parts));

        return s_map;
    }

    public static IEnumerable<Colour> ConvertColours(Dictionary<int, _Color> colors)
    {
        IEnumerable<(int, _Color)> cols = colors.Select(x => (x.Key, x.Value));

        cols = cols.OrderBy(x => x.Item1);

        return cols.Select(x => Colour(x.Item2));
    }

    private static readonly Dictionary<_Color, Colour> s_cachedColours = new();
    public static Colour Colour(_Color c)
    {
        if (s_cachedColours.TryGetValue(c, out Colour value)) return value;

        Colour col = new(c.name, ((byte)(c.c * 100),
                                      (byte)(c.m * 100),
                                      (byte)(c.y * 100),
                                      (byte)(c.k * 100)),
                             (byte)(c.a * 255));

        s_cachedColours.Add(c, col);
        return col;
    }

    #region Symbols

    public static IEnumerable<Symbol> ConvertSymbols(Dictionary<int, _Symbol> symbols)
        => symbols.Select(x => ConvertSymbol(x.Value));

    public static Symbol ConvertSymbol(_Symbol s)
    {
        return s switch
        {
            _PointSymbol p => ConvertPointSymbol(p),
            _LineSymbol l => ConvertLineSymbol(l),
            _AreaSymbol a => ConvertAreaSymbol(a),
            _CombinedSymbol c => ConvertCombinedSymbol(c),
            _TextSymbol t => ConvertTextSymbol(t),
        };
    }

    public static PointSymbol ConvertPointSymbol(_PointSymbol s)
    {
        PointSymbol op = new(s.name, s.description,
                                 new((byte)s.number.Item1, (byte)s.number.Item1, (byte)s.number.Item1),
                                 false, s.is_helper_symbol,
                                 Enumerable.Empty<MapObject>(), s.is_rotatable);

        return op;
    }

    public static LineSymbol ConvertLineSymbol(_LineSymbol s)
    {
        LineSymbol op = new(s.name, s.description,
                                new((byte)s.number.Item1, (byte)s.number.Item1, (byte)s.number.Item1),
                                false, s.is_helper_symbol,
                                Colour(s.color), s.line_width / 1000,
                                DashStyle(s.dashed, s.dash_length, s.break_length, s.dashes_in_group, s.in_group_break_length),
                                MidStyle(s.start_offset, s.end_offset, s.mid_symbol_distance, s.mid_symbol));

        return op;
    }

    public static AreaSymbol ConvertAreaSymbol(_AreaSymbol s)
    {
        AreaSymbol op = new(s.name, s.description,
                                new((byte)s.number.Item1, (byte)s.number.Item1, (byte)s.number.Item1),
                                false, s.is_helper_symbol,
                                Fill(s.patterns, s.color),
                                _.Colour.Transparent, 0f,
                                _.DashStyle.None, _.MidStyle.None,
                                true); // Think all symbols are rotatable

        return op;
    }

    public static Symbol ConvertCombinedSymbol(_CombinedSymbol s)
    {
        if (s.parts.Count > 2)
            Log("More than 2 parts found. Discarding anything other that first line & area");

        _LineSymbol? line = null;
        if (s.parts.Any(p => p is _LineSymbol))
            line = (_LineSymbol)s.parts.First(p => p is _LineSymbol)!;

        _AreaSymbol? area = null;
        if (s.parts.Any(p => p is _AreaSymbol))
            area = (_AreaSymbol)s.parts.First(p => p is _AreaSymbol)!;

        if (line != null && area != null)
        {
            AreaSymbol op = new(s.name, s.description,
                                    new((byte)s.number.Item1, (byte)s.number.Item1, (byte)s.number.Item1),
                                    false, s.is_helper_symbol,
                                    Fill(area.patterns, area.color),
                                    Colour(line.color), line.line_width / 1000,
                                    DashStyle(line.dashed, line.dash_length, line.break_length, line.dashes_in_group, line.in_group_break_length),
                                    MidStyle(line.start_offset, line.end_offset, line.mid_symbol_distance, line.mid_symbol),
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

    public static TextSymbol ConvertTextSymbol(_TextSymbol s)
    {
        FontStyle fS = new(s.bold, s.underline, false,
                               s.italic ? ItalicsMode.Italic : ItalicsMode.None);

        Font f = new(s.font_family,
                         Colour(s.color), s.font_size / 1000,
                         s.line_spacing, s.paragraph_spacing / 1000, s.character_spacing, fS);

        TextSymbol op = new(s.name, s.description,
                                new((byte)s.number.Item1, (byte)s.number.Item1, (byte)s.number.Item1),
                                false, s.is_helper_symbol,
                                f, true,
                                _.Colour.Transparent, 0f,
                                s.framing ? Colour(s.framing_color) : _.Colour.Transparent,
                                s.framing ? s.framing_line_half_width / 500 : 0f);

        return op;
    }

    public static DashStyle DashStyle(bool dashed, int dashLength, int breakLength, int dashesInGroup, int inGroupBreakLength)
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

    public static MidStyle MidStyle(int startOffset, int endOffset, int midSymbolDistance,
                                        _PointSymbol? midSymbol)
    {
        if (midSymbol is null || midSymbolDistance == 0)
            return _.MidStyle.None;

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

    public static IFill Fill(List<_AreaSymbol.FillPattern> patterns, _Color color)
    {
        if (patterns.Count == 0 && color.a == 0)
            return new SolidFill(_.Colour.Transparent);

        if (patterns.Count == 1 && color.a == 0)
        {
            _AreaSymbol.FillPattern f = patterns.First();
            IFill fill = f.type switch
            {
                _AreaSymbol.FillPattern.Type.LinePattern => ConvertPatternFill(f),
                _AreaSymbol.FillPattern.Type.PointPattern => ConvertObjectFill(f),
            };

            return fill;
        }

        if (patterns.Count == 0 && color.a != 0)
            return new SolidFill(Colour(color));

        List<IFill> combFills = new();

        if (color.a != 0)
            combFills.Add(new SolidFill(Colour(color)));

        foreach (var f in patterns)
        {
            combFills.Add(f.type switch
            {
                _AreaSymbol.FillPattern.Type.LinePattern => ConvertPatternFill(f),
                _AreaSymbol.FillPattern.Type.PointPattern => ConvertObjectFill(f),
            });
        }

        Assert(combFills.Count != 0);

        return new CombinedFill(combFills);
    }

    public static PatternFill ConvertPatternFill(_AreaSymbol.FillPattern fill)
    {
        Assert(fill.type == _AreaSymbol.FillPattern.Type.LinePattern, "Fill must be Line Pattern");

        PatternFill op = new(fill.line_width / 1000, fill.line_spacing / 1000, Colour(fill.line_color),
                                 _.Colour.Transparent, MathF.PI - fill.angle);

        return op;
    }

    public static SpacedObjectFill ConvertObjectFill(_AreaSymbol.FillPattern fill)
    {
        Assert(fill.type == _AreaSymbol.FillPattern.Type.PointPattern);

        bool isClipped = fill.flags.HasFlag(_AreaSymbol.FillPattern.Option.Default |
                                            _AreaSymbol.FillPattern.Option.NoClippingIfCenterInside |
                                            _AreaSymbol.FillPattern.Option.NoClippingIfCompletelyInside);


        SpacedObjectFill op = new(ConvertPointSymbolToMapObjects(fill.point), isClipped,
                                      (fill.point_distance / 1000, fill.line_spacing / 1000),
                                      (fill.offset_along_line / 1000, fill.line_offset),
                                      MathF.PI - fill.angle);

        return op;
    }

    public static IEnumerable<MapObject> ConvertPointSymbolToMapObjects(_PointSymbol s)
    {
        return Enumerable.Empty<MapObject>();
    }

    #endregion

    #region Map Objects

    public static MapObject ConvertMapObject(_PointSymbol.Element el)
    {
        return el.symbol switch
        {
            _PointSymbol => ConvertPointObject(el),
            _LineSymbol => ConvertLineObject(el),
            _AreaSymbol => ConvertAreaObject(el),
            _CombinedSymbol => ConvertCombinedObject(el),
        };
    }

    public static PointObject ConvertPointObject(_PointSymbol.Element el)
    {
        Assert(el.symbol is _PointSymbol && el.objec is _PointObject);

        _PointSymbol sym = (_PointSymbol)el.symbol;
        _PointObject obj = (_PointObject)el.objec;

        PointObject pObj = new(Colour(sym.inner_color), Colour(sym.outer_color),
                                   sym.inner_radius / 1000, sym.outer_width / 1000);

        return pObj;
    }

    public static LineObject ConvertLineObject(_PointSymbol.Element el)
    {
        Assert(el.symbol is _LineSymbol && el.objec is _PathObject);

        _LineSymbol sym = (_LineSymbol)el.symbol;
        _PathObject obj = (_PathObject)el.objec;

        PathCollection pC = ConvertPathWithoutHoles(obj.coords);

        LineObject lObj = new(pC, sym.line_width / 1000, Colour(sym.color));

        return lObj;
    }

    public static AreaObject ConvertAreaObject(_PointSymbol.Element el)
    {
        Assert(el.symbol is _AreaSymbol && el.objec is _PathObject);

        _AreaSymbol sym = (_AreaSymbol)el.symbol;
        _PathObject obj = (_PathObject)el.objec;

        PathCollection pC = ConvertPathWithoutHoles(obj.coords);

        AreaObject aObj = new(pC, 0, _.Colour.Transparent, new SolidFill(Colour(sym.color)));

        return aObj;
    }

    public static AreaObject ConvertCombinedObject(_PointSymbol.Element el)
    {
        Assert(el.symbol is _AreaSymbol && el.objec is _PathObject);

        _CombinedSymbol sym = (_CombinedSymbol)el.symbol;
        _PathObject obj = (_PathObject)el.objec;

        _LineSymbol border = (_LineSymbol)sym.parts.First(x => x is _LineSymbol)!;
        _AreaSymbol fill = (_AreaSymbol)sym.parts.First(x => x is _AreaSymbol)!;

        PathCollection pC = ConvertPathWithoutHoles(obj.coords);

        return new(pC, border.line_width / 1000, Colour(border.color), new SolidFill(Colour(fill.color)));
    }

    #endregion

    #region Instances

    public static IEnumerable<Instance> ConvertInstances(Dictionary<int, _MapPart> parts)
    {
        foreach (KeyValuePair<int, _MapPart> p in parts)
            foreach (_Object i in p.Value.objects)
                yield return ConvertInstance(i, p.Key);
    }

    public static Instance ConvertInstance(_Object obj, int part)
    {
        return obj switch
        {
            _PointObject point => ConvertPointInstance(point, part),
            _PathObject path => ConvertPathInstance(path, part),
            _TextObject text => ConvertTextInstance(text, part),
        };
    }

    public static PointInstance ConvertPointInstance(_PointObject obj, int part)
    {
        // Instead of converting entire symbol uses the previously created one
        PointSymbol sym = (PointSymbol)s_map.Symbols[obj.symbol.name];

        vec2 pos = ConvertMapCoordSimple(obj.coords[0]);

        return new(part, sym, pos, (float)(MathF.PI - obj.rotation));
    }

    public static PathInstance ConvertPathInstance(_PathObject obj, int part)
    {
        var pC = ConvertPathCollection(obj.coords);

        return obj.symbol switch
        {
            _LineSymbol => new LineInstance(part, (LineSymbol)s_map.Symbols[obj.symbol.name], pC.path,
                                                     pC.isClosed, (float)(MathF.PI - obj.rotation), pC.holes),
            _AreaSymbol => new AreaInstance(part, (AreaSymbol)s_map.Symbols[obj.symbol.name], pC.path,
                                                     pC.isClosed, (float)(MathF.PI - obj.rotation), pC.holes),
            _CombinedSymbol => ConvertCombinedInstance(pC, obj, part),
            _ => throw new ArgumentException("Invalid Symbol Type"),
        };
    }

    private static TextInstance ConvertTextInstance(_TextObject text, int part)
    {
        HorizontalAlignment align = text.h_align switch
        {
            _TextObject.HorizontalAlignment.AlignLeft => HorizontalAlignment.Left,
            _TextObject.HorizontalAlignment.AlignHCenter => HorizontalAlignment.Centre,
            _TextObject.HorizontalAlignment.AlignRight => HorizontalAlignment.Right,
        };

        vec2 topLeft;
        topLeft = text.has_single_anchor ?
            ConvertMapCoordSimple(text.coords[0]) :
            ConvertMapCoordSimple(text.coords[0]); // Will change at somepoint


        TextInstance op = new(part, (TextSymbol)s_map.Symbols[text.symbol.name], text.text, topLeft, align,
                                  (float)(MathF.PI - text.rotation));

        return op;
    }

    public static PathInstance ConvertCombinedInstance((PathCollection path, IEnumerable<PathCollection> holes, bool isClosed) inp, _PathObject obj, int part)
    {
        return s_map.Symbols[obj.symbol.name] switch
        {
            LineSymbol line => new LineInstance(part, line, inp.path, inp.isClosed,
                                                        (float)(MathF.PI - obj.rotation)),
            AreaSymbol area => new AreaInstance(part, area, inp.path, inp.isClosed,
                                                        (float)(MathF.PI - obj.rotation), inp.holes),
            _ => throw new ArgumentException(),
        };
    }

    public static (PathCollection path, IEnumerable<PathCollection> holes, bool isClosed) ConvertPathCollection(IList<_MapCoord> coords)
    {
        /*
		 *  Number of holes is n-1 occurences of the 16 flag
		 *  Number of bezier points is n occurences of the 1 flag
		 */

        IEnumerable<PathCollection> convHoles = Enumerable.Empty<PathCollection>();
        PathCollection pC = new();
        bool isClosed;

        int holeCount = coords.Count(c => c.fp.HasFlag(_MapCoord.Flags.HolePoint));


        if (holeCount > 1)
        {
            int index = coords.IndexOf(coords.First(c => c.fp.HasFlag(_MapCoord.Flags.HolePoint)));
            List<_MapCoord> allHoles = coords.Skip(index + 1).ToList();

            List<IList<_MapCoord>> holes = new();

            for (int i = 0; i < allHoles.Count; i++)
            {
                List<_MapCoord> hole = new();

                for (; i < allHoles.Count; i++)
                {
                    if (allHoles[i].fp.HasFlag(_MapCoord.Flags.HolePoint))
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

            isClosed = coords[index].fp.HasFlag(_MapCoord.Flags.ClosePoint);
        }
        else
        {
            // No Holes

            pC = ConvertPathWithoutHoles(coords);

            isClosed = coords[^1].fp.HasFlag(_MapCoord.Flags.ClosePoint);
        }

        return (pC, convHoles, isClosed);
    }

    public static PathCollection ConvertPathWithoutHoles(IList<_MapCoord> coords)
    {
        PathCollection pC = new();

        bool currOnBezier = coords[0].fp.HasFlag(_MapCoord.Flags.CurveStart);

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

                    if (coords[i + 3].fp.HasFlag(_MapCoord.Flags.CurveStart))
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
                    if (coords[i + 3].fp.HasFlag(_MapCoord.Flags.CurveStart))
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

                if (i != coords.Count - 1 && coords[i + 1].fp.HasFlag(_MapCoord.Flags.CurveStart))
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

    #endregion

    public static vec2 ConvertMapCoordSimple(_MapCoord coord)
    {
        return (new vec2(coord.xp, coord.yp)) / 1000;
    }
}

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
