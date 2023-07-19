namespace OTools.Common;

public class LanguageManager : Dictionary<string, LanguageItem>
{
    public LanguageManager() { }

    public static LanguageManager Deserialize(string filePath)
    {
        XMLDocument doc = XMLDocument.Deserialize(File.ReadAllText(filePath));
        XMLNode root = doc.Root;

        LanguageManager lM = new();

        foreach (XMLNode child in doc.Root.Children)
        {
            string key = child.Attributes["key"];

            LanguageItem lI = new();

            foreach (XMLNode grandChild in child.Children)
            {
                string lang = grandChild.Name;
                string text = grandChild.InnerText;

                lI.Add(lang, text);
            }

            lM.Add(key, lI);
        }

        return lM;
    }

    public static XMLDocument Serialize(LanguageManager lM)
    {
        XMLNode root = new("Languages");

        foreach (var l in lM)
        {
            XMLNode node = new("Language");
            node.Attributes.Add("key", l.Key);

            foreach (var t in l.Value)
            {
                XMLNode child = new(t.Key);
                child.InnerText = t.Value;

                node.Children.Add(child);
            }

            root.Children.Add(node);
        }

        return new XMLDocument(root);
    }
}

// Key: Language Code, Value: Text
public class LanguageItem : Dictionary<string, string> { }