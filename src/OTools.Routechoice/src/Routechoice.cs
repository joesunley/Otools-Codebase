using System.Collections.Generic;

namespace OTools.Routechoice;

public class Course
{
    public List<vec2> Controls { get; set; }

    public List<RoutechoiceSet> Routechoices { get; set; }

    public Course()
    {
        Controls = new();
        Routechoices = new();
    }

    public Course(IList<vec2> controls, IList<RoutechoiceSet> routechoices) 
    {
        Controls = new(controls);
        Routechoices = new(routechoices);
    }

    public (vec4, RoutechoiceSet) GetLeg(int legNo)
    {
        Assert(legNo > 0 && legNo <= Controls.Count - 1);

        vec4 cs = new(Controls[legNo - 1], Controls[legNo]);

        return (cs, Routechoices[legNo - 1]);
    }

    // Zero-based leg number
    public (vec4, RoutechoiceSet) GetLegZero(int legNo)
    {
        Assert(legNo >= 0 && legNo < Controls.Count - 1);

        vec4 cs = new(Controls[legNo], Controls[legNo + 1]);

        return (cs, Routechoices[legNo]);
    }
}


public class RoutechoiceSet : List<Routechoice> { }

public class Routechoice
{
    public List<vec2> Points { get; set; }
    public string Label { get; set; }

    public Routechoice()
    {
        Points = new();
        Label = string.Empty;
    }

    public Routechoice(string label)
    {
        Points = new();
        Label = label;
    }

    public Routechoice(IList<vec2> points, string label)
    {
        Points = new(points);
        Label = label;
    }
}

public static class RoutechoiceCalcs
{
    public static float Length(this IList<vec2> points)
    {
        float length = 0;

        for (int i = 1; i < points.Count; i++)
            length += vec2.Mag(points[i], points[i - 1]);

        return length;
    }
}