using Sunley.Mathematics;
using System.Collections;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace OTools.OML;

public class OMLDocument
{
    public List<OMLNode> Nodes { get; set; }

    public OMLDocument()
    {
        Nodes = new();
    }

    public OMLDocument(IEnumerable<OMLNode> nodes)
    {
        Nodes = new(nodes);
    }

    public static OMLDocument Deserialize(string[] lines)
    {
        return new(_Parser.Parse(lines));
    }
    public static string[] Serialize(OMLDocument doc)
    {
        throw new NotImplementedException();
    }
}

public class OMLNode
{
    public string Title { get; set; }
    public IOMLValue? Value { get; set; }

    public OMLNode()
    {
        Title = string.Empty;
        Value = null;
    }
    public OMLNode(string title, IOMLValue value)
    {
        Title = title;
        Value = value;
    }

	public OMLNode(string title, string value)
	{
		Title = title;
		Value = (OMLString)value;
	}
	
	public static implicit operator string(OMLNode val)
	{
		if (val.Value is OMLString s) return s;
		throw new Exception();
	}
}

public interface IOMLValue
{
    OMLValueType Type();
}
public enum OMLValueType { String, Nest, Vec2s }

[DebuggerDisplay("{_value}")]
public class OMLString : IOMLValue
{
    private string _value;
    
    private OMLString(string val)
    {
        _value = val;
    }

    public static implicit operator string(OMLString value) => value._value;
    public static implicit operator OMLString(string value) => new OMLString(value);

    public OMLValueType Type() => OMLValueType.String;
}

public class OMLVec2s : IList<vec2>, IOMLValue
{
    private List<vec2> _values;

    public OMLVec2s(IEnumerable<vec2>? vals = null)
    {
        _values = (vals is null) ? new() : new(vals);
    }
	
    public static implicit operator List<vec2>(OMLVec2s value) => value._values;
    public static implicit operator OMLVec2s(List<vec2> value) => new OMLVec2s(value);

    public OMLValueType Type() => OMLValueType.Vec2s;


    public int Count => _values.Count;
    public bool IsReadOnly => false;
    public vec2 this[int index] { get => _values[index]; set => _values[index] = value; }
    public int IndexOf(vec2 item) => _values.IndexOf(item);
    public void Insert(int index, vec2 item) => _values.Insert(index, item);
    public void RemoveAt(int index) => _values.RemoveAt(index);
    public void Add(vec2 item) => _values.Add(item);
    public void Clear() => _values.Clear();
    public bool Contains(vec2 item) => _values.Contains(item);
    public void CopyTo(vec2[] array, int arrayIndex) => _values.CopyTo(array, arrayIndex);
    public bool Remove(vec2 item) => _values.Remove(item);
    public IEnumerator<vec2> GetEnumerator() => _values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class OMLNodeList : List<OMLNode>, IOMLValue
{
	public OMLNode this[string title] => Find(n => n.Title == title);
	
	
    public OMLValueType Type() => OMLValueType.Nest;
}