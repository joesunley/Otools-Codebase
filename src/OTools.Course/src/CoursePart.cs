using System.Numerics;
using System.Text;
using OneOf;

namespace OTools.Courses;

public interface ICoursePart
{
	IEnumerable<Control> GetVariation(VariationItem var);
	VariationItem CreateVariation();
}

public sealed class Variation : List<VariationItem>, IParsable<Variation>
{
	public Variation() { }
	public Variation(IEnumerable<VariationItem> collection) : base(collection) { }

	public static Variation Parse(string s, IFormatProvider? provider)
	{
		List<VariationItem> items = new();

		for (int i = 0; i < s.Length; i++)
		{
			char c = s[i];

			if (c is not ('{' or '}'))
			{
				items.Add(new(int.Parse(c.ToString())));
				continue;
			}
			
			int start = i+1;
			int level = 1;

			while (level != 0)
			{
				i += 1;

				if (s[i] == '{')
					level++;
				else if (s[i] == '}')
					level--;
			}

			int item = items[^1].AsT0;
			items.Remove(items[^1]);
			items.Add(new(
				(item, Parse(s[start..i], provider))
			));
		}

		return new(items);
	}

	public static bool TryParse(string? s, IFormatProvider? provider, out Variation result) => throw new NotImplementedException();

	public override string ToString()
	{
		StringBuilder sb = new();

		foreach (var item in this)
		{
			if (item.IsT0)
				sb.Append(item.AsT0);
			else
			{
				sb.Append(item.AsT1.Item1);
				sb.Append('{');
				sb.Append(item.AsT1.Item2);
				sb.Append('}');
			}
		}

		return sb.ToString();
	}
}
public sealed class VariationItem : OneOfBase<int, (int, Variation)>
{
	public VariationItem(OneOf<int, (int, Variation)> item) : base(item) { }
}

public sealed class CombinedCoursePart : List<ICoursePart>, ICoursePart
{
    public CombinedCoursePart() { }
    public CombinedCoursePart(IEnumerable<ICoursePart> collection) : base(collection) {}
	public IEnumerable<Control> GetVariation(VariationItem var)
	{
		if (var.IsT0)
			return this[0].GetVariation(var);

		List<Control> controls = new();
		Variation v = var.AsT1.Item2;
			
		for (int i = 0; i < v.Count; i++)
			controls.AddRange(this[i].GetVariation(v[i]));
		
		return controls;	
	}

	public VariationItem CreateVariation()
	{
		Variation v = new();
		
		foreach (var part in this)
		{
			v.Add(part.CreateVariation());
		}

		return new((0, v));
	}
}

public sealed class LinearCoursePart : List<Control>, ICoursePart
{
	public LinearCoursePart() { }
	public LinearCoursePart(IEnumerable<Control> collection) : base(collection) { }
	
    public Control this[Guid id]
        => this.First(x => x.Id == id);

	public IEnumerable<Control> GetVariation(VariationItem var)
	{
		if (var.IsT1 || var.AsT0 != 0)
			throw new ArgumentException();

		return this;
	}
	public VariationItem CreateVariation() => throw new NotImplementedException();
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

	public IEnumerable<Control> GetVariation(VariationItem var)
	{
		if (var.IsT0)
			return Parts[var.AsT0].GetVariation(new(0));

		Variation v = var.AsT1.Item2;
		return Parts[var.AsT1.Item1].GetVariation(v[0]);

	}

	public VariationItem CreateVariation() => throw new NotImplementedException();
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

	public IEnumerable<Control> GetVariation(VariationItem var) => throw new NotImplementedException();
	public VariationItem CreateVariation() => throw new NotImplementedException();
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

	public IEnumerable<Control> GetVariation(VariationItem var) => throw new NotImplementedException();
	public VariationItem CreateVariation() => throw new NotImplementedException();
}