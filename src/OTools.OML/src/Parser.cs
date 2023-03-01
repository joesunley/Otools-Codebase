using Sunley.Mathematics;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

[assembly:InternalsVisibleTo("Console")]

namespace OTools.OML;


internal static class _Parser
{
    public static IEnumerable<OMLNode> Parse(string[] lines)
    {
        var tree = _IndentedTextToTreeParser.Parse(lines);

        // Console.WriteLine(_IndentedTextToTreeParser.Dump(tree));

        foreach (var node in tree)
            yield return _NodeParser.ParseNode(node);
    }

    public static string Serialize(IEnumerable<OMLNode> nodes)
    {
        StringBuilder sb = new();

        foreach (OMLNode node in nodes)
            _NodeParser.SerializeNode(node, 0, sb);

        return sb.ToString();
    }
}

// Need to be in seperate classes for some reason
internal static class _NodeParser
{ 
    public static OMLNode ParseNode(_IndentTreeNode node)
    {
        if (node.Children.Count > 0)
        {
            OMLNodeList list = new();
            foreach (var child in node.Children)
                list.Add(ParseNode(child));

            string titl = node.Text.Split(':')[0].Trim();

            return new(titl, list);
        }

        string line = node.Text;

        line = line.Trim();

        var kv = line.Split(':');

        string title = kv[0].Trim();
        string value = kv[1].Trim();

        if (value.Substring(0, 2) == "v2")
        {
            // removes v2(...)
            string vec2s = value.Substring(3, value.Length - 4).Trim();
            string[] vecs = vec2s.Split(';');

            OMLVec2s v2s = new();
            foreach (var vec in vecs)
            {
                if (vec == "")
                    continue;

                string[] components = vec.Split(",");

                float x = float.Parse(components[0]),
                    y = float.Parse(components[1]);

                v2s.Add((x, y));
            }

            return new(title, v2s);
        }

		// value = value.Replace("\\n", "\n");
		
        return new(title, value);
    }

    public static void SerializeNode(OMLNode node, int level, StringBuilder sb)
    {
        sb.AppendLine();
        for (int i = 0; i < level; i++)
            sb.Append('\t');
        sb.Append($"{node.Title}: ");

        switch (node.Value!.Type())
        {
            case OMLValueType.String:
				string str = (OMLString)node.Value;
				// str = str.Replace("\n", "\\n");
				sb.Append(str);
                return;
            case OMLValueType.Vec2s:
                sb.Append("v2(");
                foreach (vec2 v2 in (OMLVec2s)node.Value)
                    sb.Append($"{v2.X},{v2.Y};");
                sb.Append(')');
                return;
            case OMLValueType.Nest:
                foreach (OMLNode child in (OMLNodeList)node.Value!)
                    SerializeNode(child, level + 1, sb);
                return;
        }
    }
}

internal static class _IndentedTextToTreeParser
{
    // https://stackoverflow.com/questions/21735468/parse-indented-text-tree-in-java

    public static List<_IndentTreeNode> Parse(IEnumerable<string> lines, int rootDepth = 0, char indentChar = '\t')
    {
        var roots = new List<_IndentTreeNode>();

        // --

        _IndentTreeNode prev = null;

        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line.Trim(indentChar)))
                throw new Exception(@"Empty lines are not allowed.");

            var currentDepth = countWhiteSpacesAtBeginningOfLine(line, indentChar);

            if (currentDepth == rootDepth)
            {
                var root = new _IndentTreeNode(line, rootDepth);
                prev = root;

                roots.Add(root);
            }
            else
            {
                if (prev == null)
                    throw new Exception(@"Unexpected indention.");
                if (currentDepth > prev.Depth + 1)
                    throw new Exception(@"Unexpected indention (children were skipped).");

                if (currentDepth > prev.Depth)
                {
                    var node = new _IndentTreeNode(line.Trim(), currentDepth, prev);
                    prev.AddChild(node);

                    prev = node;
                }
                else if (currentDepth == prev.Depth)
                {
                    var node = new _IndentTreeNode(line.Trim(), currentDepth, prev.Parent);
                    prev.Parent.AddChild(node);

                    prev = node;
                }
                else
                {
                    while (currentDepth < prev.Depth) prev = prev.Parent;

                    // at this point, (currentDepth == prev.Depth) = true
                    var node = new _IndentTreeNode(line.Trim(indentChar), currentDepth, prev.Parent);
                    prev.Parent.AddChild(node);
                }
            }
        }

        // --

        return roots;
    }

    public static string Dump(IEnumerable<_IndentTreeNode> roots)
    {
        var sb = new StringBuilder();

        foreach (var root in roots)
        {
            doDump(root, sb, @"");
        }

        return sb.ToString();
    }

    private static int countWhiteSpacesAtBeginningOfLine(string line, char indentChar)
    {
        var lengthBefore = line.Length;
        var lengthAfter = line.TrimStart(indentChar).Length;
        return lengthBefore - lengthAfter;
    }

    private static void doDump(_IndentTreeNode treeNode, StringBuilder sb, string indent)
    {
        sb.AppendLine(indent + treeNode.Text);
        foreach (var child in treeNode.Children)
        {
            doDump(child, sb, indent + @"    ");
        }
    }
}

internal static class _TreeToIndentedText
{

}

[DebuggerDisplay(@"{Depth}: {Text} ({Children.Count} children)")]
internal class _IndentTreeNode
{
    public _IndentTreeNode(string text, int depth = 0, _IndentTreeNode parent = null)
    {
        Text = text;
        Depth = depth;
        Parent = parent;
    }

    public string Text { get; }
    public int Depth { get; }
    public _IndentTreeNode Parent { get; }
    public List<_IndentTreeNode> Children { get; } = new List<_IndentTreeNode>();

    public void AddChild(_IndentTreeNode child)
    {
        if (child != null) Children.Add(child);
    }
}
