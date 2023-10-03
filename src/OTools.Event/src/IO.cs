using OTools.Common;
using OTools.Courses;

namespace OTools.Events;

public static class EventLoader
{
    private const ushort CURRENT_VERSION = 1;

    public static Event Load(string filePath)
    {

    }

    public static XMLDocument Save(Event ev, ushort version = 0)
    {
        ushort versionToUse = version == 0 ? CURRENT_VERSION : version;


    }
}

public interface IEventLoaderV1
{
    XMLNode SaveEvent(Event ev);
    Event LoadEvent(XMLNode node);

    XMLNode SaveEntries(IEnumerable<Entry> entries);
    XMLNode SaveEntry(Entry entry);

    XMLNode SavePunches(IEnumerable<Punch> punches);
    XMLNode SavePunch(Punch punch);

    XMLNode SaveResults(IEnumerable<Result> results);
    XMLNode SaveResult(Result result);
}

public class EventLoaderV1 
{


    
    public XMLNode SavePunch(Punch punch)
    {
        XMLNode node = new("Punch");
        
        node.AddAttribute("id", punch.Id.ToString());
        node.AddAttribute("card", punch.CardNumber);
        node.AddAttribute("code", punch.Code.ToString());
        node.AddAttribute("timestamp", punch.TimeStamp.Ticks.ToString());

        return node;
    }
}