using OneOf;
using OTools.Common;
using System.Diagnostics.CodeAnalysis;

namespace OTools.Courses;

public abstract class Course : IStorable
{
    public Guid Id { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }
    public string DisplayFormat { get; set; }

    public uint? Distance { get; set; }
    public uint? Climb { get; set; }

    protected Course(string name, string desc, string displayFormat, uint? distance, uint? climb)
    {
        Id = Guid.NewGuid();

        Name = name;
        Description = desc;
        DisplayFormat = displayFormat;
        
        Distance = distance;
        Climb = climb;
    }

    protected Course(Guid id, string name, string desc, string displayFormat, uint? distance, uint? climb)
    {
        Id = id;

        Name = name;
        Description = desc;
        DisplayFormat = displayFormat;

        Distance = distance;
        Climb = climb;
    }
}

/*
 * * Display Format:
 * *
 * * %n -> Control Number
 * * %c -> Control Code
 * * %s -> Control Score (score course only)
 * */

public sealed class LinearCourse : Course
{
    public ICoursePart Parts { get; set; }

    public LinearCourse(string name, string desc, string displayFormat, ICoursePart parts, uint? distance, uint? climb)
        : base(name, desc, displayFormat, distance, climb)
    {
        Parts = parts;
    }

    public LinearCourse(Guid id, string name, string desc, string displayFormat, ICoursePart parts, uint? distance, uint? climb)
        : base(id, name, desc, displayFormat, distance, climb)
    {
        Parts = parts;
    }
}

public sealed class ScoreCourse : Course
{
    public List<Control> Controls { get; set; }

    public ScoreCourse(string name, string desc, string displayFormat, IEnumerable<Control> controls, uint? distance, uint? climb)
        : base(name, desc, displayFormat, distance, climb)
    {
        Controls = new(controls);
    }

    public ScoreCourse(Guid id, string name, string desc, string displayFormat, IEnumerable<Control> controls, uint? distance, uint? climb)
        : base(id, name, desc, displayFormat, distance, climb)
    {
        Controls = new(controls);
    }
}

public sealed class Variation : OneOfBase<int, (int intVar, Variation subVar), IList<Variation>>
    , IParsable<Variation>
{
    public Variation(OneOf<int, (int intVar, Variation subVar), IList<Variation>> item) : base(item) { }

    public static implicit operator Variation(int _) => new(_);
    public static implicit operator Variation((int, Variation) _) => new(_);

    public static Variation Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out Variation? result))
            return result;
        else throw new FormatException();
    }
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Variation result)
    {
        throw new NotImplementedException();

        result = default;

        if (string.IsNullOrEmpty(s))
            return false;

        if (s.TryParse(out int num))
        {
            result = new(num);
            return true;
        }

        List<Variation> items = new();

        for (int i = 0; i < s.Length; i++ )
        {

        }

        result = new(items);
        return true;
    }

    public override string ToString()
    {
        return ":"; // TODO
    }
}

public class VariationException : ArgumentException { }


public interface ICoursePart
{
    IEnumerable<Control> GetVariation(Variation variation);
}

public sealed class CombinedCoursePart : List<ICoursePart>, ICoursePart
{
    public IEnumerable<Control> GetVariation(Variation variation)
    {

        return variation.Match(
            t0 => throw new VariationException(),
            t1 => throw new VariationException(),
            t2 =>
            {
                List<Control> controls = new();
                for (int i = 0; i < Count; i++)
                    controls.AddRange(this[i].GetVariation(t2[i]));
                return controls;
            }
            );
    }
}

public sealed class LinearCoursePart : List<Control>, ICoursePart
{
    public Control this[Guid id]
        => this.First(x => x.Id == id);

    public IEnumerable<Control> GetVariation(Variation variation)
    {
        if (variation.IsT1 || variation.IsT2 || variation.AsT0 != 0)
            throw new VariationException();

        return this;
    }
}

public sealed class VariationCoursePart : ICoursePart
{
    public List<ICoursePart> Parts { get; set; }

    public VariationCoursePart()
    {
        Parts = new();
    }

    public VariationCoursePart(IEnumerable<ICoursePart> parts)
    {
        Parts = new(parts);
    }

    public IEnumerable<Control> GetVariation(Variation variation)
    {
        return variation.Match(
            t0 => Parts[t0].GetVariation(0),
            t1 => Parts[t1.intVar].GetVariation(t1.subVar),
            t2 => throw new InvalidOperationException("Invalid Variation"));
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

    public IEnumerable<Control> GetVariation(Variation variation)
    {
        return variation.Match(
            t0 => throw new VariationException(),
            t1 =>
            {
                if (t1.intVar > MathUtils.Permutations(Loops.Count, Loops.Count))
                    throw new VariationException();

                var perms = Enumerable.Range(0, Loops.Count).GetPermutations();
                var perm = perms.ElementAt(t1.intVar).ToArray();

                List<Control> c = new() { Central };

                for (int i = 0; i < Loops.Count; i++)
                {
                    c.AddRange(Loops[perm[i]].GetVariation(t1.subVar.AsT2[i]));
                    c.Add(Central);
                }

                return c;
            },
            t2 => throw new VariationException()
            );
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

    public IEnumerable<Control> GetVariation(Variation variation)
    {
        return variation.Match(
            t0 => throw new VariationException(),
            t1 =>
            {
                if (t1.intVar > 2)
                    throw new VariationException();

                List<Control> c = new() { First };

                switch (t1.intVar)
                {
                    case 0:
                        c.AddRange(PartA.GetVariation(t1.subVar.AsT2[0]));
                        c.Add(Last);
                        c.AddRange(Back.GetVariation(t1.subVar.AsT2[1]));
                        c.Add(First);
                        c.AddRange(PartB.GetVariation(t1.subVar.AsT2[2]));
                        break;
                    case 1:
                        c.AddRange(PartB.GetVariation(t1.subVar.AsT2[0]));
                        c.Add(Last);
                        c.AddRange(Back.GetVariation(t1.subVar.AsT2[1]));
                        c.Add(First);
                        c.AddRange(PartA.GetVariation(t1.subVar.AsT2[2]));
                        break;
                }

                c.Add(Last);
                return c;
            },
            t2 => throw new VariationException()
            );
    }
}