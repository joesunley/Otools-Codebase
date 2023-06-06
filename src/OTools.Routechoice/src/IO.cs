using OTools.Common;

namespace OTools.Routechoice;

public interface IRoutechoiceLoaderV1
{
    public XMLNode SaveCourse(Course course);
    public Course LoadCourse(XMLNode node);

    public XMLNode SaveRoutechoiceSet(RoutechoiceSet routechoiceSet);
    public RoutechoiceSet LoadRoutechoiceSet(XMLNode node);


    public XMLNode SaveRoutechoice(Routechoice routechoice);
    public Routechoice LoadRoutechoice(XMLNode node);
}

public class RoutechoiceLoaderV1 : IRoutechoiceLoaderV1
{
    public Course LoadCourse(XMLNode node)
    {
        Course c = new();

        foreach (XMLNode child in node.Children["Controls"].Children)
            c.Controls.Add(LoadVec2(child));

        foreach (XMLNode child in node.Children["RoutechoiceSets"].Children)
            c.Routechoices.Add(LoadRoutechoiceSet(child));

        return c;
    }

    public RoutechoiceSet LoadRoutechoiceSet(XMLNode node)
    {
        RoutechoiceSet set = new();

        foreach (XMLNode child in node.Children)
            set.Add(LoadRoutechoice(child));

        return set;
    }

    public Routechoice LoadRoutechoice(XMLNode node)
    {
        Routechoice rc = new(node.Attributes["label"]);

        foreach (XMLNode child in node.Children)
            rc.Points.Add(LoadVec2(child));

        return rc;
    }


    public XMLNode SaveCourse(Course course)
    {
        XMLNode node = new("Course");

        XMLNode controls = new("Controls");
        foreach (vec2 control in course.Controls)
            controls.AddChild(SaveVec2(control));

        XMLNode sets = new("RoutechoiceSets");
        foreach (RoutechoiceSet routechoiceSet in course.Routechoices)
            sets.AddChild(SaveRoutechoiceSet(routechoiceSet));

        node.AddChild(controls);
        node.AddChild(sets);

        return node;
    }

    public XMLNode SaveRoutechoiceSet(RoutechoiceSet routechoiceSet)
    {
        XMLNode node = new("RoutechoiceSet");

        foreach (Routechoice routechoice in routechoiceSet)
            node.AddChild(SaveRoutechoice(routechoice));

        return node;
    }

    public XMLNode SaveRoutechoice(Routechoice routechoice)
    {
        XMLNode node = new("Routechoice"); 

        node.AddAttribute("label", routechoice.Label);

        foreach (vec2 point in routechoice.Points)
            node.AddChild(SaveVec2(point));

        return node;
    }

    public vec2 LoadVec2(XMLNode node)
    {
        return new vec2(node.Attributes["x"].Parse<float>(), 
            node.Attributes["y"].Parse<float>());  
    }   

    public XMLNode SaveVec2(vec2 v2)
    {
        XMLNode node = new("V2");

        node.AddAttribute("x", v2.X.ToString());
        node.AddAttribute("y", v2.Y.ToString());

        return node;
    }
}