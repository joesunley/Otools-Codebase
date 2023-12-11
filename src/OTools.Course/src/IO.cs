using OTools.Common;
using OTools.Maps;
using Sunley.Mathematics;
using System.Diagnostics;
using System.Transactions;
using System.Xml;

namespace OTools.Courses;

public static class CourseLoader
{
	private const ushort CURRENT_VERSION = 1;
	
	public static Event Load(string filePath)
	{
		XMLDocument doc = XMLDocument.Deserialize(File.ReadAllText(filePath));
		
		if (!doc.Root.Attributes.Exists("version"))
			throw new Exception("Did not find course version.");

		return doc.Root.Attributes["version"] switch
		{
			"1" => new CourseLoaderV1().LoadEvent(doc.Root),
			_ => throw new IOException("Version not supported."),
		};
	}

	public static XMLDocument Save(Event ev, ushort version = 0)
	{
		ushort versionToUse = version == 0 ? CURRENT_VERSION : version;

		var eventNode = versionToUse switch
		{
			1 => new CourseLoaderV1().SaveEvent(ev),
            _ => throw new IOException("Version not supported."),
		};

		eventNode.AddAttribute("version", versionToUse.ToString());

		return new(eventNode);
	}
}

#region Version 1

public interface ICourseLoaderV1
{
    XMLNode SaveEvent(Event course);
    Event LoadEvent(XMLNode node);

    XMLNode SaveControls(IEnumerable<Control> controls);
    XMLNode SaveControl(Control control);
    XMLNode SaveDescription(Description desc);

    XMLNode SaveCourses(IEnumerable<Course> courses);   
    XMLNode SaveCourse(Course course);

    XMLNode SaveGaps(IEnumerable<(vec4, (vec2, sbyte))> gaps);
    XMLNode SaveBends(IEnumerable<(vec4, (IEnumerable<vec2>, sbyte))> bends);

    XMLNode SaveCourseParts(IEnumerable<ICoursePart> parts);
    XMLNode SaveCoursePart(ICoursePart part);
    XMLNode SaveCombinedCoursePart(CombinedCoursePart part);
    XMLNode SaveLinearCoursePart(LinearCoursePart part);
    XMLNode SaveVariationCoursePart(VariationCoursePart part);
    XMLNode SaveButterflyCoursePart(ButterflyCoursePart part);
    XMLNode SavePhiLoopCoursePart(PhiLoopCoursePart part);

    XMLNode SaveItems(IEnumerable<Item> items);

    XMLNode SaveMetadata(Metadata metadata);

    IEnumerable<Control> LoadControls(XMLNode node);
    Control LoadControl(XMLNode node);
    Description LoadDescription(XMLNode node);

    IEnumerable<Course> LoadCourses(XMLNode node);
    Course LoadCourse(XMLNode node);

    IEnumerable<(vec4, (vec2, sbyte))> LoadGaps(XMLNode node);
    IEnumerable<(vec4, (IEnumerable<vec2>, sbyte))> LoadBends(XMLNode node);

    IEnumerable<ICoursePart> LoadCourseParts(XMLNode node);
    ICoursePart LoadCoursePart(XMLNode node);
    CombinedCoursePart LoadCombinedCoursePart(XMLNode node);
    LinearCoursePart LoadLinearCoursePart(XMLNode node);
    VariationCoursePart LoadVariationCoursePart(XMLNode node);
    ButterflyCoursePart LoadButterflyCoursePart(XMLNode node);
    PhiLoopCoursePart LoadPhiLoopCoursePart(XMLNode node);

    IEnumerable<Item> LoadItems(XMLNode node);
    
    Metadata LoadMetadata(XMLNode node);
}

public class CourseLoaderV1 : ICourseLoaderV1
{

    public XMLNode SaveEvent(Event ev)
    {
        XMLNode node = new("Event");
        node.AddAttribute("title", ev.Title);
        
        node.AddChild(SaveMetadata(ev.Metadata));

        node.AddChild(SaveControls(ev.Controls));
        node.AddChild(SaveCourses(ev.Courses));
        node.AddChild(SaveItems(ev.Items));

        return node;    
    }

    public XMLNode SaveControls(IEnumerable<Control> controls)
    {
        XMLNode node = new("Controls");

        foreach (Control control in controls)
            node.AddChild(SaveControl(control));

        return node;
    }
    public XMLNode SaveControl(Control control)
    {
        XMLNode node = new("Control");

        node.AddAttribute("id", control.Id.ToString());
        node.AddAttribute("code", control.Code.ToString());
        node.AddAttribute("type", ((byte)control.Type).ToString());

        node.AddChild(SaveDescription(control.Description));

        XMLNode pos = SaveVec2(control.Position);
        pos.Name = "Position";
        node.AddChild(pos);

        return node;
    }
    public XMLNode SaveDescription(Description desc)
    {
        byte c = (byte)desc.ColumnC;
        byte d = (byte)desc.ColumnD;
        byte e = (byte)desc.ColumnE;
        byte f = (byte)desc.ColumnF;
        byte g = (byte)desc.ColumnG;
        byte h = (byte)desc.ColumnH;

        // BitConverter requires the padding to make to 8 bytes
        byte[] byteArr = { c, d, e, f, g, h, 0x0, 0x0 };
        ulong num = BitConverter.ToUInt64(byteArr, 0);

        string ret;
        if (desc.ColumnFDetail.HasValue)
        {
            var det = desc.ColumnFDetail.Value;
            ret = $"{num}-{det.Start}/{det.Operator}/{det.End}";
        }
        else ret = num.ToString();

        return new XMLNode("Description") { InnerText = ret };
    }

    public XMLNode SaveCourses(IEnumerable<Course> courses)
    {
        XMLNode node = new("Courses");

        foreach (Course course in courses)
            node.AddChild(SaveCourse(course));

        return node;
    }
    public XMLNode SaveCourse(Course course)
    {
        XMLNode node = new("Course");

        node.AddAttribute("id", course.Id.ToString());
        node.AddAttribute("name", course.Name);
        node.AddAttribute("description", course.Description);
        node.AddAttribute("displayFormat", course.DisplayFormat);
        node.AddAttribute("distance", (course.Distance ?? -1u).ToString());
        node.AddAttribute("climb", (course.Climb ?? -1u).ToString());

        if (course is ScoreCourse s)
        {
            node.Name = "ScoreCourse";

            XMLNode controls = new("Controls");

			foreach (Control c in s.Controls)
			{
				XMLNode control = new("Control");
				control.AddAttribute("id", c.Id.ToString());
				
				controls.AddChild(control);
			}

            node.AddChild(controls);
        }
        else if (course is LinearCourse l)
        {
            node.Name = "LinearCourse";
            node.AddChild(SaveCoursePart(l.Parts));
        }

        return node;
    }

    public XMLNode SaveGaps(IEnumerable<(vec4, (vec2, sbyte))> gaps)
    {
        XMLNode node = new("Gaps");

        foreach (var gap in gaps)
        {
            XMLNode child = new("Gap");

            child.AddAttribute("leg", gap.Item1.ToString());

            child.AddAttribute("range", gap.Item2.Item1.ToString()!);
            child.AddAttribute("course", gap.Item2.Item2.ToString());

            node.AddChild(child);
        }

        return node;
    }
    public XMLNode SaveBends(IEnumerable<(vec4, (IEnumerable<vec2>, sbyte))> bends)
    {
        XMLNode node = new("Gaps");

        foreach (var bend in bends)
        {
            XMLNode child = new("Gap");

            child.AddAttribute("leg", bend.Item1.ToString());
            child.AddAttribute("course", bend.Item2.Item2.ToString());

            XMLNode points = new("Points");

            foreach (vec2 v2 in bend.Item2.Item1)
                points.AddChild(SaveVec2(v2));

            child.AddChild(points);

            node.AddChild(child);
        }

        return node;
    }

    public XMLNode SaveCourseParts(IEnumerable<ICoursePart> parts)
    {
        XMLNode node = new("CourseParts");

        foreach (ICoursePart part in parts)
            node.AddChild(SaveCoursePart(part));

        return node;
    }
    public XMLNode SaveCoursePart(ICoursePart part)
    {
        return part switch
        {
            CombinedCoursePart ccp => SaveCombinedCoursePart(ccp),
            LinearCoursePart lcp => SaveLinearCoursePart(lcp),
            VariationCoursePart vcp => SaveVariationCoursePart(vcp),
            ButterflyCoursePart bcp => SaveButterflyCoursePart(bcp),
            PhiLoopCoursePart pcp => SavePhiLoopCoursePart(pcp),
            _ => throw new InvalidOperationException(), // Can't happen
        };
    }
    public XMLNode SaveCombinedCoursePart(CombinedCoursePart ccp)
    {
        XMLNode node = new("CombinedCoursePart");

        foreach (ICoursePart part in ccp)
            node.AddChild(SaveCoursePart(part));

        return node;
    }
    public XMLNode SaveLinearCoursePart(LinearCoursePart lcp)
    {
        XMLNode node = new("LinearCoursePart");

        foreach (Control c in lcp)
        {
            XMLNode child = new("Control");
            child.AddAttribute("id", c.Id.ToString());

            node.AddChild(child);
        }

        return node;
    }
    public XMLNode SaveVariationCoursePart(VariationCoursePart vcp)
    {
        XMLNode node = new("VariationCoursePart");

        foreach (ICoursePart part in vcp.Parts)
            node.AddChild(SaveCoursePart(part));

        return node;
    }
    public XMLNode SaveButterflyCoursePart(ButterflyCoursePart bcp)
    {
        XMLNode node = new("ButterflyCoursePart");

        node.AddAttribute("central", bcp.Central.Id.ToString());

        foreach (ICoursePart loop in bcp.Loops)
            node.AddChild(SaveCoursePart(loop));

        return node;
    }
    public XMLNode SavePhiLoopCoursePart(PhiLoopCoursePart pcp)
    {
        XMLNode node = new("PhiLoopCoursePart");

        node.AddAttribute("first", pcp.First.Id.ToString());
        node.AddAttribute("last", pcp.Last.Id.ToString());

        XMLNode partA = SaveCoursePart(pcp.PartA);
        partA.Name = "PartA";

        XMLNode partB = SaveCoursePart(pcp.PartB);
        partB.Name = "PartB";

        XMLNode back = SaveCoursePart(pcp.Back);
        back.Name = "Name";

        node.AddChild(partA);
        node.AddChild(partB);
        node.AddChild(back);

        return node;
    }

    public XMLNode SaveItems(IEnumerable<Item> items)
    {
        // This is shit

        ColourStore cols = new();
        SymbolStore syms = new();
        InstanceStore insts = new();

        foreach (Item item in items)
        {
            Symbol sym = item.Object.Symbol;

            if (!syms.Contains(sym.Id))
                syms.Add(sym);

            Instance inst = item.Object;
            
        }

        List<Colour> colours = new();
        foreach (Symbol sym in syms)
        {
            switch (sym)
            {
                case PointSymbol pSym:
                {
                    foreach (MapObject obj in pSym.MapObjects)
                        colours.AddRange(ObjCols(obj));
                } break;
                case LineSymbol lSym: { colours.Add(lSym.Colour); } break;
                case AreaSymbol aSym:
                {
                    colours.Add(aSym.Colour);
                    colours.AddRange(FillCols(aSym.Fill));
                } break;
                case TextSymbol tSym:
                {
                    colours.Add(tSym.Font.Colour);
                    colours.Add(tSym.BorderColour);
                    colours.Add(tSym.FramingColour);
                } break;
            }
        }

        foreach (Colour c in colours)
            if (!cols.Contains(c.Id))
                cols.Add(c);

        Map m = new(cols, new(), syms, insts);

        XMLNode node = new MapLoaderV1().SaveMap(m);
        node.Name = "Items";
        node.Attributes.RemoveAll(x => true);

        return node;
    }

    public XMLNode SaveMetadata(Metadata metadata)
    {
        XMLNode node = new("Metadata");
        
        foreach (KeyValuePair<string, string> kvp in metadata)
            node.AddAttribute(kvp.Key, kvp.Value);

        return node;
    }

    private IEnumerable<Colour> FillCols(IFill fill)
    {
        List<Colour> cols = new();

        switch (fill)
        {
            case SolidFill sFill: { cols.Add(sFill); } break;
            case ObjectFill oFill:
            {
                foreach (MapObject obj in oFill.Objects)
                    cols.AddRange(ObjCols(obj));
            } break;
            case PatternFill pFill:
            {
                cols.Add(pFill.ForeColour);
                cols.Add(pFill.BackColour);
            } break;
            case CombinedFill cFill:
            {
                foreach (IFill f in cFill.Fills)
                    cols.AddRange(FillCols(f));
            } break;
        }

        return cols;
    }
    private IEnumerable<Colour> ObjCols(MapObject obj)
    {
        List<Colour> cols = new();

        switch (obj)
        {
            case PointObject pObj:
            {
                cols.Add(pObj.InnerColour);
                cols.Add(pObj.OuterColour);
            } break;
            case LineObject lObj: { cols.Add(lObj.Colour); } break;
            case AreaObject aObj: { cols.AddRange(FillCols(aObj.Fill)); } break;
            case TextObject tObj:
            {
                cols.Add(tObj.Font.Colour);
                cols.Add(tObj.BorderColour);
                cols.Add(tObj.FramingColour);
            } break;
        }

        return cols;
    }

    private Event _event;

    public Event LoadEvent(XMLNode node)
    {
        //_event = new(node.Attributes["title"]);
        
        _event.Metadata = LoadMetadata(node.Children["Metadata"]);

        _event.Controls = new(LoadControls(node.Children["Controls"]));
        _event.Items = new(LoadItems(node.Children["Items"]));

        return _event;
    }

    public IEnumerable<Control> LoadControls(XMLNode node)
    {
        foreach (XMLNode child in node.Children)
            yield return LoadControl(child);
    }
    public Control LoadControl(XMLNode node)
    {
        Guid id = node.Attributes["id"].Parse<Guid>();
		ushort code = node.Attributes["code"].Parse<ushort>();
        vec2 pos = LoadVec2(node.Children["Position"]);
        ControlType type = (ControlType)node.Attributes["type"].Parse<byte>();
        Description desc = LoadDescription(node.Children["Description"]);

        return new(id, code, pos, type, desc, default);
    }
    public Description LoadDescription(XMLNode node)
    {
        string val = node.InnerText;

        if (val.Contains('-'))
        {
            var split1 = val.Split('-');

            ulong des = ushort.Parse(split1[0]);

            // Converts & filters out padding bytes
            var bytes = BitConverter.GetBytes(des)[..^2];

            var detail = split1[1].Split("/");

            if (detail.Length != 3)
                throw new Exception(); // TODO

            float start = float.Parse(detail[0]),
                end = float.Parse(detail[2]);
            char op = char.Parse(detail[1]);

            return new(bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5], new(start, end, op));
        }
        else
        {
            ulong num = ulong.Parse(val);
            var bytes = BitConverter.GetBytes(num)[..^2];

            return new(bytes[0], bytes[1], bytes[2], bytes[3], bytes[4], bytes[5], null);
        }
    }

    public IEnumerable<Course> LoadCourses(XMLNode node)
	{
		return node.Children.Select(LoadCourse);
	}
	
    public Course LoadCourse(XMLNode node)
    {
        Guid id = Guid.Parse(node.Attributes["id"]);
        string name = node.Attributes["name"],
            description = node.Attributes["description"],
            displayFormat = node.Attributes["displayFormat"];
        uint? distance = node.Attributes["distance"] == "-1" ? null : node.Attributes["distance"].Parse<uint>();
        uint? climb = node.Attributes["climb"] == "-1" ? null : node.Attributes["climb"].Parse<uint>();

        switch (node.Name)
        {
            case "ScoreCourse":
            {
                List<Control> controls = new();

                foreach (XMLNode child in node.Children["Controls"].Children)
                {
                    Guid cId = child.Attributes["id"].Parse<Guid>();
					controls.Add(_event.Controls[cId]);
				}

                return new ScoreCourse(id, name, description, displayFormat, controls, distance, climb);
            }
            case "LinearCourse":
            {
                ICoursePart parts = LoadCoursePart(node.Children[0]);

                return new LinearCourse(id, name, description, displayFormat, parts, distance, climb);
            }
            default: throw new XmlException($"Invalid Node Name {node.Name}"); // TODO
        }
    }

    public IEnumerable<(vec4, (vec2, sbyte))> LoadGaps(XMLNode node)
    {
        List<(vec4, (vec2, sbyte))> gaps = new();

        foreach (XMLNode child in node.Children)
        {
            vec4 v4 = child.Attributes["leg"].Parse<vec4>();
            vec2 v2 = child.Attributes["range"].Parse<vec2>();
            sbyte s = child.Attributes["course"].Parse<sbyte>();

            gaps.Add((v4, (v2, s)));
        }

        return gaps;
    }
    public IEnumerable<(vec4, (IEnumerable<vec2>, sbyte))> LoadBends(XMLNode node)
    {
        List<(vec4, (IEnumerable<vec2>, sbyte))> bends = new();

        foreach (XMLNode child in node.Children)
        {
            vec4 v4 = child.Attributes["leg"].Parse<vec4>();
            sbyte s = child.Attributes["course"].Parse<sbyte>();

            List<vec2> points = new();
            foreach (XMLNode chi in child.Children)
                points.Add(LoadVec2(chi));

            bends.Add((v4, (points, s)));
        }

        return bends;
    }

    public IEnumerable<ICoursePart> LoadCourseParts(XMLNode node)
    {
        foreach (XMLNode child in node.Children)
            yield return LoadCoursePart(child);
    }
    public ICoursePart LoadCoursePart(XMLNode node)
    {
        return node.Name switch
        {
            "CombinedCoursePart" => LoadCombinedCoursePart(node),
            "LinearCoursePart" => LoadLinearCoursePart(node),
            "VariationCoursePart" => LoadVariationCoursePart(node),
            "ButterflyCoursePart" => LoadButterflyCoursePart(node),
            "PhiLoopCoursePart" => LoadPhiLoopCoursePart(node),
            _ => throw new XmlException($"Invalid Node Name {node.Name}"),
        };
    }
    public CombinedCoursePart LoadCombinedCoursePart(XMLNode node)
    {
        CombinedCoursePart ccp = new();

        foreach (XMLNode child in node.Children)
            ccp.Add(LoadCoursePart(child));

        return ccp;
    }
    public LinearCoursePart LoadLinearCoursePart(XMLNode node)
    {
        LinearCoursePart lcp = new();

        foreach (XMLNode child in node.Children)
        {
            Guid id = Guid.Parse(child.InnerText);
            lcp.Add(_event.Controls[id]);
        }

        return lcp;
    }
    public VariationCoursePart LoadVariationCoursePart(XMLNode node)
    {
        List<ICoursePart> parts = new();
        foreach (XMLNode child in node.Children)
            parts.Add(LoadCoursePart(child));

        return new VariationCoursePart(parts);
    }
    public ButterflyCoursePart LoadButterflyCoursePart(XMLNode node)
    {
        Guid c = Guid.Parse(node.Attributes["central"]);

        List<ICoursePart> parts = new();
        foreach (XMLNode child in node.Children)
            parts.Add(LoadCoursePart(child));

        return new ButterflyCoursePart(_event.Controls[c], parts);
    }
    public PhiLoopCoursePart LoadPhiLoopCoursePart(XMLNode node)
    {
        Guid f = Guid.Parse(node.Attributes["first"]),
            l = Guid.Parse(node.Attributes["last"]);

        ICoursePart partA = LoadCoursePart(node.Children["PartA"]),
            partB = LoadCoursePart(node.Children["PartB"]),
            back = LoadCoursePart(node.Children["Back"]);

        return new PhiLoopCoursePart(_event.Controls[f], _event.Controls[l], partA, partB, back);
    }

    public IEnumerable<Item> LoadItems(XMLNode node)
    {
		return Enumerable.Empty<Item>();
		
        Map map = new MapLoaderV1().LoadMap(node);

        //return map.Instances.Select(i => new Item(i));
    }

    public Metadata LoadMetadata(XMLNode node)
    {
        Metadata metadata = new();
        
        foreach (XMLAttribute attr in node.Attributes)
            metadata.Add(attr.Name, attr.Value);

        return metadata;
    }

    private XMLNode SaveVec2(vec2 v2)
    {
        XMLNode node = new("Point");

        node.AddAttribute("x", v2.X.ToString());
        node.AddAttribute("y", v2.Y.ToString());

        return node;
    }
    private vec2 LoadVec2(XMLNode node)
    {
        float x = float.Parse(node.Attributes["x"]),
            y = float.Parse(node.Attributes["y"]);

        return new(x, y);

    }
}

#endregion