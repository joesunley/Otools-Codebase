﻿using OTools.Common;

namespace OTools.Maps;

public static class MapLoader
{
    private const ushort CURRENT_VERSION = 1;

    public static Map Load(string filePath)
    {
        XMLDocument doc = XMLDocument.Deserialize(File.ReadAllText(filePath));
        return Load(doc);
    }
    public static Map Load(XMLDocument doc)
    {

        if (!doc.Root.Attributes.Exists("version"))
            throw new IOException("Did not find map version.");

        return doc.Root.Attributes["version"] switch
        {
            "1" => new MapLoaderV1().LoadMap(doc.Root),
            _ => throw new IOException("Version not supported."),
        };
    }

    public static XMLDocument Save(Map map)
    {
        var mapNode = CURRENT_VERSION switch
        {
            1 => new MapLoaderV1().SaveMap(map),
            _ => throw new IOException("Version not supported."),
        };

        mapNode.AddAttribute("version", CURRENT_VERSION.ToString());

        return new(mapNode);
    }
}

#region Version 1

public interface IMapLoaderV1
{
    XMLNode SaveMap(Map map);
    Map LoadMap(XMLNode node);

    XMLNode SaveColours(IEnumerable<Colour> colours);
    XMLNode SaveColour(Colour colour);

    XMLNode SaveFill(IFill fill);
    XMLNode SaveSolidFill(SolidFill fill);
    XMLNode SaveRandomObjectFill(RandomObjectFill fill);
    XMLNode SaveSpacedObjectFill(SpacedObjectFill fill);
    XMLNode SavePatternFill(PatternFill fill);
    XMLNode SaveCombinedFill(CombinedFill fill);

    XMLNode SaveSymbols(IEnumerable<Symbol> symbols);
    XMLNode SaveSymbol(Symbol symbol);
    XMLNode SavePointSymbol(PointSymbol sym);
    XMLNode SaveLineSymbol(LineSymbol sym);
    XMLNode SaveAreaSymbol(AreaSymbol sym);
    XMLNode SaveTextSymbol(TextSymbol sym);
    XMLNode SaveDashStyle(DashStyle dash);
    XMLNode SaveMidStyle(MidStyle mid);

    XMLNode SaveMapObjects(IEnumerable<MapObject> mapObjects);
    XMLNode SaveMapObject(MapObject mapObject);
    XMLNode SavePointObject(PointObject obj);
    XMLNode SaveLineObject(LineObject obj);
    XMLNode SaveAreaObject(AreaObject obj);
    XMLNode SaveTextObject(TextObject obj);

    XMLNode SaveInstances(IEnumerable<Instance> instances);
    XMLNode SaveInstance(Instance instance);
    XMLNode SavePointInstance(PointInstance inst);
    XMLNode SavePathInstance(PathInstance inst);
    XMLNode SaveTextInstance(TextInstance inst);

    XMLNode SavePathCollection(PathCollection pathCollection);
    XMLNode SaveLinearPath(LinearPath line);
    XMLNode SaveBezierPath(BezierPath bez);
    XMLNode SaveHoles(IEnumerable<PathCollection> holes);


    IEnumerable<Colour> LoadColours(XMLNode node);
    Colour LoadColour(XMLNode node);

    IFill LoadFill(XMLNode node);
    SolidFill LoadSolidFill(XMLNode node);
    RandomObjectFill LoadRandomObjectFill(XMLNode node);
    SpacedObjectFill LoadSpacedObjectFill(XMLNode node);
    PatternFill LoadPatternFill(XMLNode node);
    CombinedFill LoadCombinedFill(XMLNode node);

    IEnumerable<Symbol> LoadSymbols(XMLNode node);
    Symbol LoadSymbol(XMLNode node);
    PointSymbol LoadPointSymbol(XMLNode node);
    LineSymbol LoadLineSymbol(XMLNode node);
    AreaSymbol LoadAreaSymbol(XMLNode node);
    TextSymbol LoadTextSymbol(XMLNode node);

    IEnumerable<MapObject> LoadMapObjects(XMLNode node);
    MapObject LoadMapObject(XMLNode node);
    PointObject LoadPointObject(XMLNode node);
    LineObject LoadLineObject(XMLNode node);
    AreaObject LoadAreaObject(XMLNode node);
    TextObject LoadTextObject(XMLNode node);

    IEnumerable<Instance> LoadInstances(XMLNode node);
    Instance LoadInstance(XMLNode node);
    PointInstance LoadPointInstance(XMLNode node);
    PathInstance LoadPathInstance(XMLNode node);
    TextInstance LoadTextInstance(XMLNode node);
}

public class MapLoaderV1 : IMapLoaderV1
{
    #region Save

    public XMLNode SaveMap(Map map)
    {
        XMLNode node = new("Map");

        node.AddAttribute("title", map.Title);

        node.AddChild(SaveColours(map.Colours));
        node.AddChild(SaveSymbols(map.Symbols));
        node.AddChild(SaveInstances(map.Instances));

        return node;
    }

    #region Colours

    public XMLNode SaveColours(IEnumerable<Colour> colours)
    {
        XMLNode node = new("Colours");
        foreach (Colour col in colours)
            node.Children.Add(SaveColour(col));
        return node;
    }

    public XMLNode SaveColour(Colour colour)
    {
        XMLNode node = new("Colour");

        node.AddAttribute("id", colour.Id.ToString());
        node.AddAttribute("name", colour.Name);
        node.AddAttribute("hex", colour.HexValue.ToString());

        return node;
    }

    public XMLNode SaveColourId(Colour colour)
    {
        if (colour.Name == "Transparent")
            return new("Colour") { InnerText = "Transparent" };

        return new("Colour") { InnerText = colour.Id.ToString() };
    }

    #endregion

    #region Fill

    public XMLNode SaveFill(IFill fill)
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

    public XMLNode SaveSolidFill(SolidFill fill)
    {
        XMLNode node = SaveColourId(fill.Colour);
        node.Name = "SolidFill";

        return node;
    }

    public XMLNode SaveRandomObjectFill(RandomObjectFill fill)
    {
        XMLNode node = new("RandomObjectFill");

        node.AddAttribute("spacing", fill.Spacing.ToString());
        node.AddAttribute("isClipped", fill.IsClipped.ToString());

        XMLNode mapObjects = SaveMapObjects(fill.Objects);

        node.AddChild(mapObjects);

        return node;
    }

    public XMLNode SaveSpacedObjectFill(SpacedObjectFill fill)
    {
        XMLNode node = new("SpacedObjectFill");

        XMLNode mapObjects = SaveMapObjects(fill.Objects);

        XMLNode spacing = SaveVec2(fill.Spacing);
        spacing.Name = "Spacing";

        XMLNode offset = SaveVec2(fill.Offset);
        offset.Name = "Offset";

        // Convert radians to degrees
        node.AddAttribute("rotation", (fill.Rotation * 180 / MathF.PI).ToString());
        node.AddAttribute("isClipped", fill.IsClipped.ToString());

        node.AddChild(mapObjects);
        node.AddChild(spacing);
        node.AddChild(offset);

        return node;
    }

    public XMLNode SavePatternFill(PatternFill fill)
    {
        XMLNode node = new("PatternFill");

        XMLNode fore = new("Foreground");

        XMLNode foreWidth = new("Width");
        foreWidth.InnerText = fill.ForeSpacing.ToString();

        fore.AddChild(SaveColourId(fill.ForeColour));
        fore.AddChild(foreWidth);

        XMLNode back = new("Background");

        XMLNode backWidth = new("Width")
        {
            InnerText = fill.BackSpacing.ToString(),
        };

        back.AddChild(SaveColourId(fill.BackColour));
        back.AddChild(backWidth);

        // Convert radians to degrees
        node.AddAttribute("rotation", (fill.Rotation * 180 / MathF.PI).ToString());

        node.AddChild(fore);
        node.AddChild(back);

        return node;
    }

    public XMLNode SaveCombinedFill(CombinedFill fill)
    {
        XMLNode node = new("CombinedFill");

        node.Children.AddRange(fill.Fills.Select(SaveFill));

        return node;
    }

    #endregion

    #region Symbols

    public XMLNode SaveSymbols(IEnumerable<Symbol> symbols)
    {
        XMLNode node = new("Symbols");
        foreach (Symbol sym in symbols)
            node.Children.Add(SaveSymbol(sym));
        return node;
    }

    public XMLNode SaveSymbol(Symbol sym)
    {
        return sym switch
        {
            PointSymbol p => SavePointSymbol(p),
            LineSymbol l => SaveLineSymbol(l),
            AreaSymbol a => SaveAreaSymbol(a),
            TextSymbol t => SaveTextSymbol(t),
            _ => throw new InvalidOperationException("Invalid symbol type"),
        };
    }

    public XMLNode SaveSymbolBase(Symbol sym)
    {
        XMLNode node = new XMLNode("Symbol");
        node.AddAttribute("id", sym.Id.ToString());
        node.AddAttribute("name", sym.Name);
        node.AddAttribute("description", sym.Description);
        node.AddAttribute("number", $"{sym.Number.First}-{sym.Number.Second}-{sym.Number.Third}");
        node.AddAttribute("isUncrossable", sym.IsUncrossable.ToString());
        node.AddAttribute("isHelper", sym.IsHelperSymbol.ToString());

        return node;
    }

    public XMLNode SavePointSymbol(PointSymbol sym)
    {
        XMLNode node = SaveSymbolBase(sym);
        node.Name = "PointSymbol";

        XMLNode objs = SaveMapObjects(sym.MapObjects);
        node.AddChild(objs);

        XMLNode style = new("Style");
        style.AddAttribute("isRotatable", sym.IsRotatable.ToString());

        node.AddChild(style);

        return node;
    }

    public XMLNode SaveLineSymbol(LineSymbol sym)
    {
        XMLNode node = SaveSymbolBase(sym);
        node.Name = "LineSymbol";

        XMLNode style = new("Style");
        XMLNode colour = SaveColourId(sym.BorderColour);
        XMLNode width = new("Width") { InnerText = sym.BorderWidth.ToString() };

        style.AddChild(colour);
        style.AddChild(width);

        if (sym.DashStyle.HasDash)
            style.AddChild(SaveDashStyle(sym.DashStyle));
        if (sym.MidStyle.HasMid)
            style.AddChild(SaveMidStyle(sym.MidStyle));

        node.AddChild(style);

        return node;
    }

    public XMLNode SaveAreaSymbol(AreaSymbol sym)
    {
        XMLNode node = SaveSymbolBase(sym);
        node.Name = "AreaSymbol";

        XMLNode style = new("Style");

        XMLNode border = new("Border");
        XMLNode borderCol = SaveColourId(sym.BorderColour);
        XMLNode width = new("Width");
        XMLNode fill = SaveFill(sym.Fill);

        width.InnerText = sym.BorderWidth.ToString();

        border.AddChild(borderCol);
        border.AddChild(width);

        style.AddChild(fill);
        style.AddChild(border);

        if (sym.DashStyle.HasDash)
            style.AddChild(SaveDashStyle(sym.DashStyle));
        if (sym.MidStyle.HasMid)
            style.AddChild(SaveMidStyle(sym.MidStyle));

        if (sym.RotatablePattern) style.AddAttribute("rotatablePattern", "True");

        node.AddChild(style);

        return node;
    }

    public XMLNode SaveTextSymbol(TextSymbol sym)
    {
        XMLNode node = SaveSymbolBase(sym);
        node.Name = "TextSymbol";

        XMLNode font = new("Font");

        font.AddAttribute("family", sym.Font.FontFamily);
        font.AddAttribute("size", sym.Font.Size.ToString());

        XMLNode colour = SaveColourId(sym.Font.Colour);

        XMLNode fontStyle = new("FontStyle");

        fontStyle.AddAttribute("bold", sym.Font.FontStyle.Bold.ToString());
        fontStyle.AddAttribute("underline", sym.Font.FontStyle.Underline.ToString());
        fontStyle.AddAttribute("strikeout", sym.Font.FontStyle.Strikeout.ToString());
        fontStyle.AddAttribute("italics", sym.Font.FontStyle.Italics.ToString());

        font.AddChild(colour);
        font.AddChild(fontStyle);

        XMLNode style = new("Style");

        XMLNode border = new("Border");

        if (sym.BorderWidth == 0 && sym.BorderColour == OTools.Maps.Colour.Transparent) border.InnerText = "None";
        else
        {
            XMLNode borderCol = SaveColourId(sym.BorderColour);
            XMLNode width = new("Width") { InnerText = sym.BorderWidth.ToString() };

            border.AddChild(borderCol);
            border.AddChild(width);
        }

        XMLNode framing = new("Framing");

        if (sym.FramingWidth == 0 && sym.BorderColour == OTools.Maps.Colour.Transparent) framing.InnerText = "None";
        else
        {
            XMLNode framingCol = SaveColourId(sym.FramingColour);
            XMLNode width = new("Width") { InnerText = sym.FramingWidth.ToString() };

            framing.AddChild(framingCol);
            framing.AddChild(width);
        }

        style.AddChild(border);
        style.AddChild(framing);

        if (sym.IsRotatable) style.AddAttribute("isRotatable", "true");

        node.AddChild(font);
        node.AddChild(style);

        return node;
    }

    public XMLNode SaveDashStyle(DashStyle dash)
    {
        XMLNode node = new("DashStyle");

        node.AddAttribute("dashLength", dash.DashLength.ToString());
        node.AddAttribute("gapLength", dash.GapLength.ToString());
        node.AddAttribute("groupSize", dash.GroupSize.ToString());
        node.AddAttribute("groupGapLength", dash.GroupGapLength.ToString());

        return node;
    }

    public XMLNode SaveMidStyle(MidStyle mid)
    {
        XMLNode node = new("MidStyle");

        node.AddAttribute("gapLength", mid.GapLength.ToString());
        node.AddAttribute("requireMid", mid.RequireMid.ToString());
        node.AddAttribute("initialOffset", mid.InitialOffset.ToString());
        node.AddAttribute("endOffset", mid.EndOffset.ToString());

        node.AddChild(SaveMapObjects(mid.MapObjects));

        return node;
    }

    #endregion

    #region Map Objects

    public XMLNode SaveMapObjects(IEnumerable<MapObject> objs)
    {
        XMLNode node = new("MapObjects");

        foreach (MapObject obj in objs)
            node.Children.Add(SaveMapObject(obj));

        return node;
    }

    public XMLNode SaveMapObject(MapObject obj)
    {
        return obj switch
        {
            PointObject p => SavePointObject(p),
            LineObject l => SaveLineObject(l),
            AreaObject a => SaveAreaObject(a),
            TextObject t => SaveTextObject(t),
            _ => throw new InvalidOperationException("Invalid map object type"),
        };
    }

    public XMLNode SavePointObject(PointObject obj)
    {
        XMLNode node = new("PointObject");
        node.AddAttribute("id", obj.Id.ToString());

        XMLNode style = new("Style");
        XMLNode inner = new("Inner");
        XMLNode outer = new("Outer");

        XMLNode iColour = SaveColourId(obj.InnerColour);
        XMLNode iWidth = new("Width") { InnerText = obj.InnerRadius.ToString() };

        inner.AddChild(iColour);
        inner.AddChild(iWidth);

        XMLNode oColour = SaveColourId(obj.OuterColour);
        XMLNode oWidth = new("Width") { InnerText = obj.OuterRadius.ToString() };


        outer.AddChild(oColour);
        outer.AddChild(oWidth);

        style.AddChild(inner);
        style.AddChild(outer);

        node.AddChild(style);

        return node;
    }

    public XMLNode SaveLineObject(LineObject obj)
    {
        XMLNode node = new("LineObject");
        node.AddAttribute("id", obj.Id.ToString());

        XMLNode style = new("Style");

        XMLNode colour = SaveColourId(obj.Colour);
        XMLNode width = new("Width") { InnerText = obj.Width.ToString() };

        style.AddChild(colour);
        style.AddChild(width);

        XMLNode segments = SavePathCollection(obj.Segments);

        node.AddChild(style);
        node.AddChild(segments);

        return node;
    }

    public XMLNode SaveAreaObject(AreaObject obj)
    {
        XMLNode node = new("AreaObject");
        node.AddAttribute("id", obj.Id.ToString());

        XMLNode style = new("Style");

        XMLNode fill = SaveFill(obj.Fill);

        XMLNode border = new("Border");

        XMLNode borderCol = SaveColourId(obj.BorderColour);
        XMLNode borderWidth = new("Width") { InnerText = obj.BorderWidth.ToString() };

        border.AddChild(borderCol);
        border.AddChild(borderWidth);

        style.AddChild(fill);
        style.AddChild(border);

        XMLNode segments = SavePathCollection(obj.Segments);

        node.AddChild(style);
        node.AddChild(segments);

        return node;
    }

    public XMLNode SaveTextObject(TextObject obj)
    {
        XMLNode node = new("TextObject");
        node.AddAttribute("id", obj.Id.ToString());

        XMLNode font = new("Font");

        font.AddAttribute("family", obj.Font.FontFamily);
        font.AddAttribute("size", obj.Font.Size.ToString());

        XMLNode colour = SaveColourId(obj.Font.Colour);

        XMLNode fontStyle = new("FontStyle");


        fontStyle.AddAttribute("bold", obj.Font.FontStyle.Bold.ToString());
        fontStyle.AddAttribute("underline", obj.Font.FontStyle.Underline.ToString());
        fontStyle.AddAttribute("strikeout", obj.Font.FontStyle.Strikeout.ToString());
        fontStyle.AddAttribute("italics", obj.Font.FontStyle.Italics.ToString());

        font.AddChild(colour);
        font.AddChild(fontStyle);

        XMLNode style = new("Style");

        XMLNode border = new("Border");

        if (obj.BorderWidth == 0 && obj.BorderColour == OTools.Maps.Colour.Transparent) border.InnerText = "None";
        else
        {
            XMLNode borderCol = SaveColourId(obj.BorderColour);
            XMLNode width = new("Width") { InnerText = obj.BorderWidth.ToString() };

            border.AddChild(borderCol);
            border.AddChild(width);
        }

        XMLNode framing = new("Border");

        if (obj.FramingWidth == 0 && obj.FramingColour == OTools.Maps.Colour.Transparent) framing.InnerText = "None";
        else
        {
            XMLNode framingCol = SaveColourId(obj.FramingColour);
            XMLNode width = new("Width") { InnerText = obj.FramingWidth.ToString() };

            framing.AddChild(framingCol);
            framing.AddChild(width);
        }

        style.AddChild(border);
        style.AddChild(framing);

        if (obj.Rotation != 0) style.AddAttribute("rotation", (obj.Rotation * 180 / MathF.PI).ToString());

        XMLNode text = new("Text") { InnerText = obj.Text };
        XMLNode topLeft = SaveVec2(obj.TopLeft);

        node.AddAttribute("alignment", obj.HorizontalAlignment.ToString());

        node.AddChild(font);
        node.AddChild(style);
        node.AddChild(text);
        node.AddChild(topLeft);

        return node;
    }

    #endregion

    #region Instances

    public XMLNode SaveInstances(IEnumerable<Instance> insts)
    {
        XMLNode node = new("Instances");

        foreach (Instance inst in insts)
            node.Children.Add(SaveInstance(inst));

        return node;
    }

    public XMLNode SaveInstance(Instance inst)
    {
        return inst switch
        {
            PointInstance p => SavePointInstance(p),
            LineInstance l => SavePathInstance(l),
            AreaInstance a => SavePathInstance(a),
            TextInstance t => SaveTextInstance(t),
            _ => throw new InvalidOperationException("Invalid instance type"),
        };
    }

    public XMLNode SaveInstanceBase(Instance inst)
    {
        XMLNode node = new("Instance");

        node.AddAttribute("id", inst.Id.ToString());
        node.AddAttribute("layer", inst.Layer.ToString());

        XMLNode symbol = new("Symbol")
        {
            InnerText = inst.Symbol.Id.ToString(),
        };

        node.AddChild(symbol);

        return node;
    }

    public XMLNode SavePointInstance(PointInstance inst)
    {
        XMLNode node = SaveInstanceBase(inst);
        node.Name = "PointInstance";

        XMLNode centre = SaveVec2(inst.Centre);
        centre.Name = "Centre";

        if (inst.Rotation != 0) node.AddAttribute("rotation", (inst.Rotation * 180 / MathF.PI).ToString());

        node.AddChild(centre);

        return node;
    }

    public XMLNode SavePathInstance(PathInstance inst)
    {
        XMLNode node = SaveInstanceBase(inst);

        node.Name = inst is LineInstance
            ? "LineInstance" : "AreaInstance";

        node.AddChild(SavePathCollection(inst.Segments));
        node.AddChild(SaveHoles(inst.Holes));

        if (inst.PatternRotation != 0)
            node.AddAttribute("patternRotation", (inst.PatternRotation * 180 / MathF.PI).ToString());

        return node;
    }

    public XMLNode SaveTextInstance(TextInstance inst)
    {
        XMLNode node = SaveInstanceBase(inst);
        node.Name = "TextInstance";

        if (inst.Rotation != 0) node.AddAttribute("rotation", (inst.Rotation * 180 / MathF.PI).ToString());

        node.AddAttribute("alignment", inst.HorizontalAlignment.ToString());

        XMLNode text = new("Text") { InnerText = inst.Text };

        XMLNode topLeft = SaveVec2(inst.TopLeft);
        topLeft.Name = "TopLeft";

        node.AddChild(text);
        node.AddChild(topLeft);

        return node;
    }

    #endregion

    #region Segments

    public XMLNode SavePathCollection(PathCollection coll)
    {
        XMLNode node = new("Segments");

        foreach (IPathSegment seg in coll)
        {
            XMLNode s = seg switch
            {
                LinearPath line => SaveLinearPath(line),
                BezierPath bez => SaveBezierPath(bez),
                _ => throw new InvalidOperationException(),
            };

            if (seg.IsGap) s.AddAttribute("isGap", "true");

            node.AddChild(s);
        }

        return node;
    }

    public XMLNode SaveLinearPath(LinearPath line)
    {
        XMLNode node = new("LinearPath");

        foreach (vec2 pt in line)
            node.AddChild(SaveVec2(pt));

        return node;
    }

    public XMLNode SaveBezierPath(BezierPath bez)
    {
        XMLNode node = new("BezierPath");

        foreach (BezierPoint pt in bez)
        {
            XMLNode p = new("BezierPoint");

            if (pt.EarlyControl != pt.Anchor)
            {
                XMLNode early = SaveVec2(pt.EarlyControl);
                early.Name = "EarlyControl";

                p.AddChild(early);
            }


            XMLNode anchor = SaveVec2(pt.Anchor);
            anchor.Name = "AnchorControl";

            p.AddChild(anchor);

            if (pt.LateControl != pt.Anchor)
            {
                XMLNode late = SaveVec2(pt.LateControl);
                late.Name = "LateControl";

                p.AddChild(late);
            }

            node.AddChild(p);
        }

        return node;
    }

    public XMLNode SaveHoles(IEnumerable<PathCollection> holes)
    {
        XMLNode node = new("Holes");

        foreach (PathCollection hole in holes)
        {
            XMLNode h = SavePathCollection(hole);
            h.Name = "Hole";

            node.AddChild(h);
        }

        return node;
    }

    #endregion

    private XMLNode SaveVec2(vec2 v2)
    {
        XMLNode node = new("Point");

        node.AddAttribute("x", v2.X.ToString());
        node.AddAttribute("y", v2.Y.ToString());

        return node;
    }

    #endregion

    #region Load

    private Map _map = new();
    
    public Map LoadMap(XMLNode node)
    {
        _map = new(node.Attributes["title"]);
        
        _map.Colours = new(LoadColours(node.Children["Colours"]));
        _map.Symbols = new(LoadSymbols(node.Children["Symbols"]));
        _map.Instances = new(LoadInstances(node.Children["Instances"]));

        return _map;        
    }

    #region Colours

    public IEnumerable<Colour> LoadColours(XMLNode node)
    {
        return node.Children.Select(LoadColour);
    }

    public Colour LoadColour(XMLNode node)
    {
        Guid id = Guid.Parse(node.Attributes["id"]);

        string name = node.Attributes["name"];

        uint colour = uint.Parse(node.Attributes["hex"]);

        return new(id, name, colour);
    }

    public Colour ColourId(XMLNode node)
    {
        return node.InnerText == "Transparent" ?
            OTools.Maps.Colour.Transparent : _map.Colours[Guid.Parse(node.InnerText)];
    }

    #endregion

    #region Fills

    public IFill LoadFill(XMLNode node)
    {
        if (!node.Name.Contains("Fill")) throw new ArgumentException("Node is not a Fill", nameof(node));

        return node.Name switch
        {
            "SolidFill" => LoadSolidFill(node),
            "RandomObjectFill" => LoadRandomObjectFill(node),
            "SpacedObjectFill" => LoadSpacedObjectFill(node),
            "PatternFill" => LoadPatternFill(node),
            "CombinedFill" => LoadCombinedFill(node),
            _ => throw new InvalidOperationException("Invalid Fill Type"),
        };
    }

    public SolidFill LoadSolidFill(XMLNode node)
    {
        return new(ColourId(node));
    }

    public RandomObjectFill LoadRandomObjectFill(XMLNode node)
    {
        var mapObjects = LoadMapObjects(node.Children["MapObjects"]);

        bool isClipped = bool.Parse(node.Attributes["isClipped"]);

        float spacing = float.Parse(node.Attributes["spacing"]);

        return new(mapObjects, isClipped, spacing);
    }

    public SpacedObjectFill LoadSpacedObjectFill(XMLNode node)
    {
        var mapObjects = LoadMapObjects(node.Children["MapObjects"]);

        bool isClipped = bool.Parse(node.Attributes["isClipped"]);

        vec2 spacing = new(
            float.Parse(node.Children["Spacing"].Attributes["x"]),
            float.Parse(node.Children["Spacing"].Attributes["y"])
            );

        vec2 offset = new(
            float.Parse(node.Children["Offset"].Attributes["x"]),
            float.Parse(node.Children["Offset"].Attributes["y"])
            );

        float rotationInDegrees = node.Attributes.Exists("rotation")
            ? float.Parse(node.Attributes["rotation"]) : 0f;

        float rotation = rotationInDegrees * (MathF.PI / 180);

        return new(mapObjects, isClipped, spacing, offset, rotation);
    }

    public PatternFill LoadPatternFill(XMLNode node)
    {
        XMLNode fore = node.Children["Foreground"];
        XMLNode back = node.Children["Background"];

        Colour forCol = ColourId(fore.Children["Colour"]);
        float forWidth = float.Parse(fore.Children["Width"].InnerText);

        Colour backCol = ColourId(back.Children["Colour"]);
        float backWidth = float.Parse(back.Children["Width"].InnerText);

        float rotationInDegrees = node.Attributes.Exists("rotation") ?
            float.Parse(node.Attributes["rotation"]) : 0f;

        float rotation = rotationInDegrees * (MathF.PI / 180);

        return new(forWidth, backWidth, forCol, backCol, rotation);
    }

    public CombinedFill LoadCombinedFill(XMLNode node)
    {
        IEnumerable<IFill> fills = node.Children.Select(LoadFill);

        return new(fills);
    }

    #endregion

    #region Symbols

    public IEnumerable<Symbol> LoadSymbols(XMLNode node)
    {
        return node.Children.Select(LoadSymbol);
    }

    public Symbol LoadSymbol(XMLNode node)
    {
        return node.Name switch
        {
            "PointSymbol" => LoadPointSymbol(node),
            "LineSymbol" => LoadLineSymbol(node),
            "AreaSymbol" => LoadAreaSymbol(node),
            "TextSymbol" => LoadTextSymbol(node),
            _ => throw new InvalidOperationException("Invalid symbol type"),
        };
    }

    public (Guid id, string name, string desc, SymbolNumber num, bool uncr, bool help) LoadSymbolBase(XMLNode node)
    {
        Guid id = Guid.Parse(node.Attributes["id"]);
        string name = node.Attributes["name"];
        string desc = node.Attributes["description"];
        SymbolNumber num = new(node.Attributes["number"]);

        bool isUncrossable = bool.Parse(node.Attributes["isUncrossable"]),
             isHelper = bool.Parse(node.Attributes["isHelper"]);

        return (id, name, desc, num, isUncrossable, isHelper);
    }

    public PointSymbol LoadPointSymbol(XMLNode node)
    {
        var bas = LoadSymbolBase(node);

        IEnumerable<MapObject> mapObjects = LoadMapObjects(node.Children["MapObjects"]);

        bool isRotatable = node.Children.Exists("Style") && node.Children["Style"].Attributes.Exists("rotation")
                            && bool.Parse(node.Children["Style"].Attributes["rotation"]);

        return new(bas.id, bas.name, bas.desc, bas.num, bas.uncr, bas.help, mapObjects, isRotatable);
    }

    public LineSymbol LoadLineSymbol(XMLNode node)
    {
        var bas = LoadSymbolBase(node);

        XMLNode style = node.Children["Style"];

        Colour col = ColourId(style.Children["Colour"]);
        float width = float.Parse(style.Children["Width"].InnerText);

        DashStyle dash = DashStyle.None;

        if (style.Children.Exists("DashStyle"))
        {
            XMLAttributeCollection d = style.Children["DashStyle"].Attributes;

            dash = new(
                float.Parse(d["dashLength"]),
                float.Parse(d["gapLength"]),
                int.Parse(d["groupSize"]),
                float.Parse(d["groupGapLength"])
                );
        }

        MidStyle mid = MidStyle.None;

        if (style.Children.Exists("MidStyle"))
        {
            XMLNode midStyle = style.Children["MidStyle"];

            IEnumerable<MapObject> objs = LoadMapObjects(midStyle);

            mid = new(
                objs,
                float.Parse(midStyle.Attributes["gapLength"]),
                bool.Parse(midStyle.Attributes["requireMid"]),
                float.Parse(midStyle.Attributes["initialOffset"]),
                float.Parse(midStyle.Attributes["endOffset"])
                );
        }

        return new LineSymbol(bas.id, bas.name, bas.desc, bas.num, bas.uncr, bas.help, col, width, dash, mid);
    }

    public AreaSymbol LoadAreaSymbol(XMLNode node)
    {
        var bas = LoadSymbolBase(node);

        XMLNode style = node.Children["Style"];

        IFill fill = LoadFill(style.Children[0]);

        XMLNode border = style.Children["Border"];

        Colour colour = ColourId(border.Children["Colour"]);
        float width = float.Parse(border.Children["Width"].InnerText);

        DashStyle dash = DashStyle.None;

        if (style.Children.Exists("DashStyle"))
        {
            XMLAttributeCollection d = style.Children["DashStyle"].Attributes;

            dash = new(
                float.Parse(d["dashLength"]),
                float.Parse(d["gapLength"]),
                int.Parse(d["groupSize"]),
                float.Parse(d["groupGapLength"])
                );
        }

        MidStyle mid = MidStyle.None;

        if (style.Children.Exists("MidStyle"))
        {
            XMLNode m = style.Children["MidStyle"];

            IEnumerable<MapObject> objs = LoadMapObjects(m);

            mid = new(
                objs,
                float.Parse(m.Attributes["gapLength"]),
                bool.Parse(m.Attributes["requireMid"]),
                float.Parse(m.Attributes["initialOffset"]),
                float.Parse(m.Attributes["endOffset"])
                );
        }

        bool isRotatable = style.Attributes.Exists("isRotatable") && bool.Parse(style.Attributes["isRotatable"]);

        return new AreaSymbol(bas.id, bas.name, bas.desc, bas.num, bas.uncr, bas.help, fill, colour, width, dash, mid, isRotatable);
    }

    public TextSymbol LoadTextSymbol(XMLNode node)
    {
        var bas = LoadSymbolBase(node);


        XMLNode font = node.Children["Font"];

        string family = font.Attributes["family"];
        float size = float.Parse(font.Attributes["size"]);

        Colour colour = ColourId(font.Children["Colour"]);

        XMLAttributeCollection spacing = font.Children["Spacing"].Attributes;

        float line = float.Parse(spacing["line"]),
              para = float.Parse(spacing["paragraph"]),
              chara = float.Parse(spacing["character"]);

        XMLAttributeCollection fontStyle = font.Children["FontStyle"].Attributes;

        bool bold = bool.Parse(fontStyle["bold"]),
             underline = bool.Parse(fontStyle["underline"]),
             strikeout = bool.Parse(fontStyle["strikeout"]);

        ItalicsMode italics = fontStyle["italics"] switch
        {
            "None" => ItalicsMode.None,
            "Italic" => ItalicsMode.Italic,
            "Oblique" => ItalicsMode.Oblique,
            _ => throw new Exception("Invalid italics mode"),
        };

        FontStyle fontStyle_ = new(bold, underline, strikeout, italics);

        Font font_ = new(family, colour, size, line, para, chara, fontStyle_);

        XMLNode style = node.Children["Style"];

        XMLNode border = style.Children["Border"];
        (Colour, float)? border_ = null;

        if (border.InnerText != "None")
        {
            Colour borderColour = ColourId(border.Children["Colour"]);
            float borderWidth = float.Parse(border.Children["Width"].InnerText);

            border_ = (borderColour, borderWidth);
        }

        XMLNode framing = style.Children["Framing"];
        (Colour, float)? framing_ = null;

        if (framing.InnerText != "None")
        {
            Colour framingColour = ColourId(framing.Children["Colour"]);
            float framingWidth = float.Parse(framing.Children["Width"].InnerText);

            framing_ = (framingColour, framingWidth);
        }

        bool isRotatable = style.Attributes.Exists("isRotatable") && bool.Parse(style.Attributes["isRotatable"]);

        return new(bas.id, bas.name, bas.desc, bas.num, bas.uncr, bas.help, font_, isRotatable, border_, framing_);
    }

    #endregion

    #region Map Objects

    public IEnumerable<MapObject> LoadMapObjects(XMLNode node)
    {
        return node.Children.Select(LoadMapObject);
    }

    public MapObject LoadMapObject(XMLNode node)
    {
        return node.Name switch
        {
            "PointObject" => LoadPointObject(node),
            "LineObject" => LoadLineObject(node),
            "AreaObject" => LoadAreaObject(node),
            "TextObject" => LoadTextObject(node),
            _ => throw new InvalidOperationException("Invalid map object type."),
        };
    }

    public PointObject LoadPointObject(XMLNode node)
    {
        Guid id = Guid.Parse(node.Attributes["id"]);

        XMLNode inner = node.Children["Style"].Children["Inner"];
        XMLNode outer = node.Children["Style"].Children["Outer"];

        Colour iCol = ColourId(inner.Children["Colour"]);
        Colour oCol = ColourId(outer.Children["Colour"]);

        float iWidth = float.Parse(inner.Children["Width"].InnerText);
        float oWidth = float.Parse(outer.Children["Width"].InnerText);

        return new PointObject(id, iCol, oCol, iWidth, oWidth);
    }
    public LineObject LoadLineObject(XMLNode node)
    {
        Guid id = Guid.Parse(node.Attributes["id"]);

        XMLNode style = node.Children["Style"];

        Colour col = ColourId(style.Children["Colour"]);
        float width = float.Parse(style.Children["Width"].InnerText);

        PathCollection pC = LoadPathCollection(node.Children["Segments"]);

        return new LineObject(id, pC, width, col);
    }
    public AreaObject LoadAreaObject(XMLNode node)
    {
        Guid id = Guid.Parse(node.Attributes["id"]);

        XMLNode style = node.Children["Style"];
        XMLNode fillNode = style.Children[0]; // Maybe make more robust?

        IFill fill = fillNode.Name switch
        {
            "SolidFill" => new SolidFill(_map.Colours[Guid.Parse(fillNode.InnerText)]),
            _ => throw new NotImplementedException(),
        };

        XMLNode border = style.Children["Border"];

        Colour col = ColourId(border.Children["Colour"]);
        float width = float.Parse(border.Children["Width"].InnerText);

        PathCollection pC = LoadPathCollection(node.Children["Segments"]);

        return new AreaObject(id, pC, width, col, fill);
    }
    public TextObject LoadTextObject(XMLNode node)
    {
        Guid id = Guid.Parse(node.Attributes["id"]);

        XMLNode font = node.Children["Font"];

        string family = font.Attributes["family"];
        float size = float.Parse(font.Attributes["size"]);

        Colour colour = ColourId(font.Children["Colour"]);

        XMLAttributeCollection spacing = font.Children["Spacing"].Attributes;

        float line = float.Parse(spacing["line"]),
              para = float.Parse(spacing["paragraph"]),
              chara = float.Parse(spacing["character"]);

        XMLAttributeCollection fontStyle = font.Children["FontStyle"].Attributes;

        bool bold = bool.Parse(fontStyle["bold"]),
             underline = bool.Parse(fontStyle["underline"]),
             strikeout = bool.Parse(fontStyle["strikeout"]);

        ItalicsMode italics = fontStyle["italics"] switch
        {
            "None" => ItalicsMode.None,
            "Italic" => ItalicsMode.Italic,
            "Oblique" => ItalicsMode.Oblique,
            _ => throw new Exception("Invalid italics mode"),
        };

        FontStyle fontStyle_ = new(bold, underline, strikeout, italics);

        Font font_ = new(family, colour, size, line, para, chara, fontStyle_);

        XMLNode style = node.Children["Style"];

        XMLNode border = style.Children["Border"];
        (Colour, float)? border_ = null;

        if (border.InnerText != "None")
        {
            Colour borderColour = ColourId(border.Children["Colour"]);
            float borderWidth = float.Parse(border.Children["Width"].InnerText);

            border_ = (borderColour, borderWidth);
        }

        XMLNode framing = style.Children["Framing"];
        (Colour, float)? framing_ = null;

        if (framing.InnerText != "None")
        {
            Colour framingColour = ColourId(framing.Children["Colour"]);
            float framingWidth = float.Parse(framing.Children["Width"].InnerText);

            framing_ = (framingColour, framingWidth);
        }

        string text = node.Children["Text"].InnerText;

        vec2 topLeft = new(
            float.Parse(node.Children["TopLeft"].Attributes["x"]),
            float.Parse(node.Children["TopLeft"].Attributes["y"]));

        float rotationInDegrees = style.Attributes.Exists("rotation") ? float.Parse(node.Attributes["rotation"]) : 0;
        float rotation = rotationInDegrees * (MathF.PI / 180);

        HorizontalAlignment alignment = node.Attributes["alignment"] switch
        {
            "Left" => HorizontalAlignment.Left,
            "Centre" => HorizontalAlignment.Centre,
            "Right" => HorizontalAlignment.Right,
            _ => throw new InvalidOperationException(),
        };

        return new TextObject(id, text, topLeft, rotation, font_, border_, framing_, alignment);

    }

    #endregion

    #region Instances

    public IEnumerable<Instance> LoadInstances(XMLNode node)
    {
        return node.Children.Select(LoadInstance);
    }

    public Instance LoadInstance(XMLNode node)
    {
        return node.Name switch
        {
            "PointInstance" => LoadPointInstance(node),
            "LineInstance" => LoadPathInstance(node),
            "AreaInstance" => LoadPathInstance(node),
            "TextInstance" => LoadTextInstance(node),
            _ => throw new InvalidOperationException("Invalid instance type")
        };
    }

    public (Guid id, int layer, Symbol sym) LoadInstanceBase(XMLNode node)
    {
        Guid id = Guid.Parse(node.Attributes["id"]);
        int layer = int.Parse(node.Attributes["layer"]);

        Guid symbolId = Guid.Parse(node.Children["Symbol"].InnerText);
        Symbol symbol = _map.Symbols[symbolId];

        return (id, layer, symbol);
    }

    public PointInstance LoadPointInstance(XMLNode node)
    {
        var bas = LoadInstanceBase(node);

        vec2 centre = new(
            float.Parse(node.Children["Centre"].Attributes["x"]),
            float.Parse(node.Children["Centre"].Attributes["y"]));

        float rotationInDegrees = 0;
        if (node.Attributes.Exists("rotation"))
            rotationInDegrees = float.Parse(node.Attributes["rotation"]);

        float rotation = rotationInDegrees * (MathF.PI / 180);

        return new(bas.id, bas.layer, (PointSymbol)bas.sym, centre, rotation);
    }

    public PathInstance LoadPathInstance(XMLNode node)
    {
        var bas = LoadInstanceBase(node);

        bool isClosed = node.Attributes.Exists("isClosed") && bool.Parse(node.Attributes["isClosed"]);
        PathCollection segments = LoadPathCollection(node.Children["Segments"]);

        List<PathCollection> holes = new();
        if (node.Children.Exists("Holes"))
        {
            holes.AddRange(node.Children["Holes"].Children
                               .Select(LoadPathCollection));
        }

        float patternRotationInDegrees = 0;
        if (node.Attributes.Exists("patternRotation"))
            patternRotationInDegrees = float.Parse(node.Attributes["patternRotation"]);

        float patternRotation = patternRotationInDegrees * (MathF.PI / 180);

        return node.Name switch
        {
            "LineInstance" => new LineInstance(bas.Item1, bas.Item2, (LineSymbol)bas.Item3, segments, isClosed, patternRotation, holes),
            "AreaInstance" => new AreaInstance(bas.Item1, bas.Item2, (AreaSymbol)bas.Item3, segments, isClosed, patternRotation, holes),
            _ => throw new InvalidOperationException()
        };
    }

    public PathCollection LoadPathCollection(XMLNode node)
    {
        PathCollection pC = new();

        node.Children.ForEach(seg =>
        {
            IPathSegment tSeg;

            switch (seg.Name)
            {
                case "LinearPath":
                {
                    List<vec2> pts = new();
                    seg.Children.ForEach(p => {
                        pts.Add(new(
                        float.Parse(p.Attributes["x"]),
                        float.Parse(p.Attributes["y"])
                        ));
                    });
                    tSeg = new LinearPath(pts);
                }
                break;
                case "BezierPath":
                {
                    List<BezierPoint> pts = new();
                    seg.Children.ForEach(p =>
                    {
                        vec2? early = null;
                        if (p.Children.Exists("EarlyControl"))
                        {
                            early = new(
                                float.Parse(p.Children["EarlyControl"].Attributes["x"]),
                                float.Parse(p.Children["EarlyControl"].Attributes["y"])
                                );
                        }

                        vec2? late = null;
                        if (p.Children.Exists("LateControl"))
                        {
                            late = new(
                                float.Parse(p.Children["LateControl"].Attributes["x"]),
                                float.Parse(p.Children["LateControl"].Attributes["y"])
                                );
                        }

                        vec2 anchor = new(
                            float.Parse(p.Children["Anchor"].Attributes["x"]),
                            float.Parse(p.Children["Anchor"].Attributes["y"])
                            );

                        pts.Add(new(anchor, early, late));
                    });
                    tSeg = new BezierPath(pts);
                }
                break;
                default: throw new InvalidOperationException();
            }

            tSeg.IsGap = seg.Attributes.Exists("isGap") && bool.Parse(seg.Attributes["isGap"]);
            pC.Add(tSeg);
        });

        return pC;
    }

    public TextInstance LoadTextInstance(XMLNode node)
    {
        var bas = LoadInstanceBase(node);

        vec2 topLeft = new(
            float.Parse(node.Children["TopLeft"].Attributes["x"]),
            float.Parse(node.Children["TopLeft"].Attributes["y"]));

        float rotationInDegrees = node.Attributes.Exists("rotation") ? float.Parse(node.Attributes["rotation"]) : 0;
        float rotation = rotationInDegrees * (MathF.PI / 180);

        HorizontalAlignment alignment = node.Attributes["alignment"] switch
        {
            "Left" => HorizontalAlignment.Left,
            "Centre" => HorizontalAlignment.Centre,
            "Right" => HorizontalAlignment.Right,
            _ => throw new InvalidOperationException(),
        };

        string text = node.Children["Text"].InnerText;

        return new(bas.id, bas.layer, (TextSymbol)bas.sym, text, topLeft, alignment, rotation);
    }

    #endregion

    #endregion
}

#endregion