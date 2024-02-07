using OneOf;
using OneOf.Types;

namespace OTools.Common;

public class HtmlObject
{
    protected readonly string _identifier;

    public string Class { get; set; }
    public string Id { get; set; }
    public string Style { get; set; }

    public OneOf<string, HtmlObject, IEnumerable<HtmlObject>, None> InnerHtml { get; set; }

    public HtmlObject()
    {
        _identifier = string.Empty;

        Class = string.Empty;
        Id = string.Empty;
        Style = string.Empty;
        InnerHtml = new None();
    }

    public HtmlObject(string identifier) : this()
    {
        _identifier = identifier;
    }

    public virtual XMLNode ToXml()
    {
        XMLNode node = new(_identifier);

        if (Class != string.Empty)
            node.AddAttribute("class", Class);
        if (Id != string.Empty) 
            node.AddAttribute("id", Id);
        if (Style != string.Empty)
            node.AddAttribute("style", Style);

        InnerHtml.Match((t0) =>
        {
            node.InnerText = t0;
            return 1;
        }, (t1) =>
        {
            node.AddChild(t1.ToXml());
            return 1;
        }, (t2) =>
        {
            foreach (var c in t2)
                node.AddChild(c.ToXml());
            return 1;
        },
        (t3) =>
        {
            return 0;
        });

        return node;
    }
}

public sealed class HtmlLinkObject : HtmlObject
{
    public string Href { get; set; }

    public HtmlLinkObject() : base("a")
    {
        Href = string.Empty;
    }
    public HtmlLinkObject(string href) : this()
    {
        Href = href;
    }

    public override XMLNode ToXml()
    {
        XMLNode node = base.ToXml();

        if (Href != string.Empty)
            node.AddAttribute("href", Href);

        return node;
    }
}

public sealed class HtmlButtonObject : HtmlObject
{
    public string Src { get; set; }

    public HtmlButtonObject() : base("button")
    {
        Src = string.Empty;
    }

    public HtmlButtonObject(string src) : this()
    {
        Src = src;
    }

    public override XMLNode ToXml()
    {
        XMLNode node = base.ToXml();

        if (Src !=  string.Empty)
            node.AddAttribute("src", Src);

        return node;
    }
}

public sealed class HtmlHeadingObject : HtmlObject
{
    public byte Size { get; set; }

    public HtmlHeadingObject(byte size) : base($"h{size}")
    {
        Size = size;
    }
}

public sealed class HtmlDivObject : HtmlObject
{
    public HtmlDivObject() : base("div") { }
}

public sealed class HtmlImageObject : HtmlObject
{
    public string Src { get; set; }

    public HtmlImageObject() : base("img")
    {
        Src = string.Empty;
    }

    public HtmlImageObject(string src) : this()
    {
        Src = src;
    }

    public override XMLNode ToXml()
    {
        XMLNode node = base.ToXml();

        if (Src != string.Empty)
            node.AddAttribute("src", Src);

        return node;
    }
}

public sealed class HtmlDocument
{
    public string Title { get; set; }

    public List<string> StyleSheets { get; set; }
    public List<string> Scripts { get; set; }
    public List<string> DeferredScripts { get; set; }

    public HtmlObject Body { get; set; }

    public HtmlDocument()
    {
        Title = string.Empty;

        StyleSheets = new();
        Scripts = new();
        DeferredScripts = new();

        Body = new("body");
    }

    public HtmlDocument(string title) : this()
    {
        Title = title;
    }

    public XMLNode ToXml()
    {
        XMLNode node = new("html");

        node.AddAttribute("lang", "en");

        XMLNode head = new("head");

        XMLNode meta1 = new("meta");
        meta1.AddAttribute("charset", "UTF-8");

        XMLNode meta2 = new("meta");
        meta2.AddAttribute("name", "viewport");
        meta2.AddAttribute("content", "width=device-width, initial-scale=1.0");

        head.AddChild(meta1);
        head.AddChild(meta2);
        head.AddChild(new("title", Title));

        foreach (string s in StyleSheets)
        {
            XMLNode styleSheet = new("link");
            styleSheet.AddAttribute("href", s);
            styleSheet.AddAttribute("rel", "stylesheet");
            styleSheet.AddAttribute("type", "text/css");

            head.AddChild(styleSheet);
        }

        // Scripts

        node.AddChild(head);
        node.AddChild(Body.ToXml());

        return node;

    }
}


public class HtmlBuilder
{
    private HtmlDocument _doc;
    public static HtmlDocument Create(string title)
    {
        return new HtmlDocument(title);
    }

    public HtmlBuilder AddStyleSheet(string link)
    {
        _doc.StyleSheets.Add(link);
        return this;
    }

    public HtmlBuilder AddScript(string link)
    {
        _doc.Scripts.Add(link);
        return this;
    }

    public HtmlBuilder AddDeferredScript(string link)
    {
        _doc.DeferredScripts.Add(link);
        return this;
    }

    public HtmlBuilder CreateBody(HtmlObject obj)
    {
        if (_doc.Body.InnerHtml.IsT3)
            _doc.Body.InnerHtml = obj;
        else if (_doc.Body.InnerHtml.IsT2)
        {
            var temp = _doc.Body.InnerHtml.AsT2.ToList();
            temp.Add(obj);
            _doc.Body.InnerHtml = temp;
        }
        else
        {
            var temp = _doc.Body.InnerHtml.AsT1;
            _doc.Body.InnerHtml = new HtmlObject[] { temp, obj };
        }
        return this;
    }
    
    public class HtmlObjectBuilder
    {
        private HtmlObject _obj;

        public HtmlObjectBuilder CreateDiv()
        {
            _obj = new HtmlDivObject();
            return this;
        }
        public HtmlObjectBuilder CreateLink(string href)
        {
            _obj = new HtmlLinkObject(href);
            return this;
        }
        public HtmlObjectBuilder CreateImage(string src)
        {
            _obj = new HtmlImageObject(src);
            return this;
        }
        public HtmlObjectBuilder CreateHeading(byte size)
        {
            _obj = new HtmlHeadingObject(size);
            return this;
        }
        public HtmlObjectBuilder CreateButton(string src)
        {
            _obj = new HtmlButtonObject(src);
            return this;
        }

        public HtmlObjectBuilder SetStyle(string style)
        {
            _obj.Style = style;
            return this;
        }

        public HtmlObjectBuilder AddClass(string className)
        {
            if (_obj.Class != string.Empty)
                _obj.Class += $" {className}";
            else
                _obj.Class = className;
            return this;
        }
        public HtmlObjectBuilder SetId(string id)
        {
            _obj.Id = id;
            return this;
        }
        public HtmlObjectBuilder SetInner(string inner)
        {
            _obj.InnerHtml = inner;
            return this;
        }
        public HtmlObjectBuilder AddChild(HtmlObject child)
        {
            if (_obj.InnerHtml.IsT3)
                _obj.InnerHtml = child;
            else if (_obj.InnerHtml.IsT2)
            {
                var temp = _obj.InnerHtml.AsT2.ToList();
                temp.Add(child);
                _obj.InnerHtml = temp;
            }
            else
            {
                var temp = _obj.InnerHtml.AsT1;
                _obj.InnerHtml = new HtmlObject[] { temp, child };
            }
            return this;

        }

        public HtmlObject Build() => _obj;
    }
}
