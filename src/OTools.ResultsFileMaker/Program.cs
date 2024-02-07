using Spectre.Console;
using OneOf;
using System.Text;
using Newtonsoft.Json;

string filePath;

if (args.Length > 0)
{
    filePath = args[0];
}
else
{
    Console.Write("FilePath: ");
    filePath = Console.ReadLine() ?? string.Empty;
}

var obj = JsonConvert.DeserializeObject<Rootobject>(filePath);

Console.ReadLine();

//ResultsFile file = new("Test", new(255, 0, 0), "", "");



class ResultsFile
{
    public string Title { get; set; }

    public string AccentColour { get; set; }

    public Rows Rows { get; set; }

    public string Css { get; set; }

    public ResultsFile(string title, string accentColour, Rows rows, string css)
    {
        Title = title;
        AccentColour = accentColour;
        Rows = rows;
        Css = css;
    }

    public override string ToString()
    {
        return $""" 
<!DOCTYPE html>
<html lang="en">
    <head>
        <meta charset="UTF-8" />
        <meta name="viewport" content="width=device-width, initial-scale=1.0" />

        <style>
        {Css}
        </style>

        <title>{Title}</title>
    </head>
    <body>
        {Rows}
    </body>
</html>
""";
    }
}

class Rows : List<OneOf<Row, Heading, Image, Button>>
{

    public override string ToString()
    {
        StringBuilder sb = new();

        sb.AppendLine("<div>");

        foreach (var r in this)
        {
            sb.AppendLine(r.ToString());
        }

        sb.AppendLine("</div>");

        return sb.ToString();
    }
}

class Row : List<OneOf<Column, Heading, Image, Button>>
{

}

class Column : List<OneOf<Heading, Image, Button>>
{

}

class Heading
{
    public Heading(string text, int size)
    {
        Text = text;
        Size = size;
    }

    public string Text { get; set; }
    public int Size { get; set; }

    public override string ToString()
    {
        if (Size == -1)
            return $"<p>{Text}</p>";
        else
            return $"<h{Size}>{Text}</h{Size}>";
    }
}

class Image
{
    public Image(string src, string href)
    {
        Src = src;
        Href = href;
    }

    public string Src { get; set; }
    public string Href { get; set; }

    public override string ToString()
    {
        if (Href == string.Empty)
            return $"""<img src="{Src}" />""";
        else
            return $"""<a href="{Href}"><img src="{Src}"></a>""";
    }
}

class Button
{
    public Button(string href, string text)
    {
        Href = href;
        Text = text;
    }

    public string Href { get; set; }
    public string Text { get; set; }

    public override string ToString()
    {
        return $"""<a href="{Href}"><button>{Text}</button></a>""";
    }
}





public class Rootobject
{
    public string Title { get; set; }
    public string Color { get; set; }
    public Body[] Body { get; set; }
}


public class Body
{
    public string Type { get; set; }
    public string Src { get; set; }
    public string Href { get; set; }
    public string Text { get; set; }
    public int Size { get; set; }
    public Body[] Content { get; set; }
}
