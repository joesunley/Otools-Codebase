using System.Diagnostics;
using System.Xml.Linq;

namespace OTools.Common;
using Sys = System.Xml;

public sealed class XMLDocument
{
    public XMLNode Root { get; set; }

    public XMLDocument()
    {
        Root = new();
    }

    public XMLDocument(XMLNode root)
    {
        Root = root;
    }

    public static XMLDocument Deserialize(string xml)
    {
        Sys.XmlDocument doc = new();
        doc.LoadXml(xml);

        return Marshall.DeserDoc(doc);
    }

    public static void Serialize(XMLDocument doc, string filePath)
        => Marshall.OutSerDoc(doc, filePath);

    public void Serialize(string filePath)
        => Marshall.OutSerDoc(this, filePath);
}

[DebuggerDisplay("{Name} : {Children.Count}, {Attributes.Count}")]
public sealed class XMLNode
{
    private XMLNodeCollection _children;
    private string? _innerText;

    public string Name { get; set; }

    public XMLNodeCollection Children
    {
        get => _children;

        set
        {
            if (_innerText != null)
                throw new InvalidOperationException("Cannot set Children when there is inner text");

            _children = value;
        }
    }

    public string InnerText
    {
        get => _innerText ?? String.Empty;
        set
        {
            if (_children.Count > 0)
                throw new InvalidOperationException("Cannot set InnerText when there are children");

            _innerText = value;
        }
    }

    public XMLAttributeCollection Attributes { get; set; }

    public XMLNode()
    {
        Name = "";
        _children = new();
        Attributes = new();
    }

    public XMLNode(string name)
        : this()
    {
        Name = name;
    }

    public void AddAttribute(string name, string value)
        => Attributes.Add(name, value);

    public void AddChild(XMLNode child)
    {
        if (_innerText != null)
            throw new InvalidOperationException("Cannot add child when there is inner text");

        Children.Add(child);
    }
}

public sealed class XMLNodeCollection : List<XMLNode>
{
    public XMLNode this[string name]
    {
        get => this.First(x => x.Name == name);

        set
        {
            int index = FindIndex(x => x.Name == name);
            this[index] = value;
        }
    }

    public void Add(string name)
        => Add(new XMLNode(name));

    public bool Exists(string name)
        => Exists(x => x.Name == name);
}

[DebuggerDisplay("{Name} : {Value}")]
public sealed class XMLAttribute
{
    public string Name { get; set; }
    public string Value { get; set; }

    public XMLAttribute()
    {
        Name = "";
        Value = "";
    }

    public XMLAttribute(string name, string value)
    {
        Name = name;
        Value = value;
    }
}

public sealed class XMLAttributeCollection : List<XMLAttribute>
{
    public string this[string name]
    {
        get => this.First(x => x.Name == name).Value;

        set
        {
            int index = FindIndex(x => x.Name == name);
            this[index].Value = value;
        }
    }

    public void Add(string name, string value)
        => Add(new XMLAttribute(name, value));

    public bool Exists(string name)
        => Exists(x => x.Name == name);
}

internal static class Marshall
{
    internal static XMLDocument DeserDoc(Sys.XmlDocument doc)
    {
        Sys.XmlNode root = doc.LastChild!;

        XMLNode rootNode = DeserNode(root);

        return new(rootNode);
    }

    internal static XMLNode DeserNode(Sys.XmlNode node)
    {
        XMLNode outNode = new(node.Name);

        if (node.Attributes != null)
            foreach (Sys.XmlAttribute attr in node.Attributes)
                outNode.AddAttribute(attr.Name, attr.Value);

        if (node.ChildNodes.Count != 0 && node.ChildNodes[0]!.Name == "#text")
        {
            outNode.InnerText = node.ChildNodes[0]!.Value!;
            return outNode;
        }

        foreach (Sys.XmlNode child in node.ChildNodes)
            outNode.AddChild(DeserNode(child));

        return outNode;
    }

    internal static void OutSerDoc(XMLDocument doc, string fileName)
    {
        XDocument xDoc = SerDoc(doc);
		
        xDoc.Save(fileName);
    }

    internal static XDocument SerDoc(XMLDocument doc)
    {
        XDocument xDoc = new();

        xDoc.Add(SerNode(doc.Root));

        return xDoc;
    }

    internal static XElement SerMeta(XMLAttributeCollection attrs)
    {
        XElement meta = new("m");

        foreach (XMLAttribute attr in attrs)
            meta.Add(new XAttribute(attr.Name, attr.Value));

        return meta;
    }
    internal static XElement SerNode(XMLNode node)
    {
        XElement outEl = new(node.Name);

        foreach (XMLAttribute attr in node.Attributes)
            outEl.Add(new XAttribute(attr.Name, attr.Value));

        foreach (XMLNode child in node.Children)
            outEl.Add(SerNode(child));

        if (node.Children.Count == 0)
            outEl.Value = node.InnerText;

        return outEl;
    }
}