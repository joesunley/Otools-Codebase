using OTools.Common;

HtmlDocument doc = new("EUOC Big Weekend");

doc.StyleSheets.Add("https://results.euoc.co.uk/res/bw24style.css");


HtmlDivObject divTitle = new();
divTitle.Class = "title";


HtmlLinkObject link = new("https://www.euoc.co.uk/big-weekend");
HtmlImageObject img = new("https://results.euoc.co.uk/res/logo.jpg");
img.Class = "logo";

link.InnerHtml = img;
divTitle.InnerHtml = link;




doc.Body.InnerHtml = new HtmlObject[]
{
    new HtmlDivObject()
    {
        Class = "title",
        InnerHtml = new HtmlLinkObject("https://www.euoc.co.uk/big-weekend")
        {
            InnerHtml = new HtmlImageObject("https://results.euoc.co.uk/res/logo.jpg")
            {
                Class = "logo",
            }
        }
    },
    new HtmlDivObject()
    {
        Class = "row",
        InnerHtml = new HtmlDivObject()
        {
            Class = "column",
            InnerHtml = new HtmlObject[]
            {
                new HtmlHeadingObject(2) { InnerHtml = "Friday 26th" },
                new HtmlHeadingObject(3) { InnerHtml = "2x2 Mixed Sprint Relay" },

                new HtmlDivObject()
                {
                    Class = "buttons",
                    InnerHtml = new HtmlLinkObject("")
                    {
                        InnerHtml = new HtmlButtonObject()
                        {
                            Class = "btn active",
                            InnerHtml = "Results"
                        }
                    }
                }
            }
        }
    }

};

new HtmlBuilder()
    .AddStyleSheet("")
    .CreateBody()
        


var xml = doc.ToXml();

XMLDocument xmlDoc = new(xml);

xmlDoc.Serialize("C:\\Users\\joe\\Downloads\\test.html");