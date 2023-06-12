namespace OTools.Common;

public sealed class TextManager
{
    private Dictionary<string, LanguageFile> _languages;
    private LanguageFile _active;

    private string _lang;
    public string Language
    {
        get => _lang;

        set
        {
            if (_lang != value)
            {
                _active = _languages[value];
                _lang = value;
            }
        }
    }

    public (string singular, string plural) this[string val] => _active[val];

    public TextManager(IEnumerable<string> filePaths, string language)
    {
        _lang = language;

        _languages = new();

        foreach (string path in filePaths)
        {
            LanguageFile l = LanguageFile.LoadFromFile(path);
            _languages.Add(l.Language, l);
        }

        _active = _languages[_lang];
    }

    private class LanguageFile : Dictionary<string, (string singular, string plural)> 
    {
        public string Language { get; set; }

        public LanguageFile(string language) : base()
        {
            Language = language;
        }

        public static LanguageFile LoadFromFile(string path)
        {
            string[] lines = File.ReadAllLines(path);

            LanguageFile lF = new(lines[0]);

            foreach (string line in lines.Skip(1))
            {
                var kvp = line.Split(':');

                var spp = kvp[1].Split(',');

                var o = (spp[0].Trim(), spp[1].Trim());

                if (o.Item1 == "null")
                    o.Item1 = string.Empty;
                if (o.Item2 == "null")
                    o.Item2 = string.Empty;

                lF.Add(kvp[0], o);
            }

            return lF;
        }
    }
}