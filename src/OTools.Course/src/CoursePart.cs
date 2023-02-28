using System.Net.Http.Headers;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Text;

namespace OTools.Course;

public interface ICoursePart
{
    IEnumerable<Control> GetVariation(string varStr) { throw new NotImplementedException(); }
}

public sealed class CombinedCoursePart : List<ICoursePart>, ICoursePart
{
    public IEnumerable<Control> GetVariation(string varStr)
    {
        throw new NotImplementedException();
    }
}

public sealed class LinearCoursePart : List<Control>, ICoursePart
{
    public Control this[Guid id]
        => this.First(x => x.Id == id);

    public IEnumerable<Control> GetVariation(string varStr) => this;
}

public sealed class VariationCoursePart : ICoursePart
{
    public Control First { get; set; }
    public Control Last { get; set; }

    public List<ICoursePart> Parts { get; set; }

    public VariationCoursePart(Control first, Control last)
    {
        First = first;
        Last = last;

        Parts = new();
    }

    public VariationCoursePart(Control first, Control last, IEnumerable<ICoursePart> parts)
    {
        First = first;
        Last = last;

        Parts = new(parts);
    }

    public IEnumerable<Control> GetVariation(string varStr)
    {
        List<Control> controls = new() { First };

        int v = Convert.ToInt32(varStr[0]);

        if (varStr[1] == '{')
        {
            string newVarStr = _Utils.FilterVarStr(varStr);
            controls.AddRange(Parts[v].GetVariation(newVarStr));
        }
        else
        {
            // varStr not needed
            controls.AddRange(Parts[v].GetVariation(""));
        }

        controls.Add(Last);

        return controls;
    }
}

public sealed class ButterflyCoursePart : ICoursePart
{
    public Control Central { get; set; }

    public List<ICoursePart> Loops { get; set; }

    public ButterflyCoursePart(Control central)
    {
        Central = central;

        Loops = new();
    }

    public ButterflyCoursePart(Control central, IEnumerable<ICoursePart> loops)
    {
        Central = central;

        Loops = new(loops);
    }

    public IEnumerable<Control> GetVariation(string varStr)
    {
        List<Control> controls = new() { Central };

        int v = Convert.ToInt32(varStr[0]);
        string newVarStr = _Utils.FilterVarStr(varStr);

        if (v >= Loops.Count * 2) throw new ArgumentException();

        if (v == 1)
        {
            foreach (var loop in Loops)
            {
                controls.AddRange(loop.GetVariation(newVarStr));
                controls.Add(Central);
            }
        }
        else
        {
            for (int i = v-1; i < Loops.Count; i++)
            {
                controls.AddRange(Loops[i].GetVariation(newVarStr));
                controls.Add(Central);
            }
            for (int i = 0; i < v-1; i++)
            {
                controls.AddRange(Loops[i].GetVariation(newVarStr));
            }
        }

        throw new NotImplementedException();
    }
}

public sealed class PhiLoopCoursePart : ICoursePart
{
    public Control First { get; set; }
    public Control Last { get; set; }

    public ICoursePart PartA { get; set; }
    public ICoursePart PartB { get; set; }
    public ICoursePart Back { get; set; }

    public PhiLoopCoursePart(Control first, Control last, ICoursePart partA, ICoursePart partB, ICoursePart back)
    {
        First = first;
        Last = last;

        PartA = partA;
        PartB = partB;
        Back = back;
    }
}


file static class _Utils
{
    public static string FilterVarStr(string str)
    {
        bool finished = false;
        int index = 0;

        if (!str.Contains('{') || !str.Contains('}'))
            return str;

        int level = 0;
        bool started = false;

        (int, int) a = (0, 0);

        while (!finished)
        {
            char c = str[index];

            if (c == '{')
            {
                level++;
                started = true;
                a.Item1 = index;
            }
            if (c == '}')
            {
                level--;
                a.Item2 = index;
            }

            if (level == 0 && started)
                finished = true;
        }

        return str[a.Item1..a.Item2];
    }
}