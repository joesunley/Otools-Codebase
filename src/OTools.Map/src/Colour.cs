using OTools.Common;
using ownsmtp.logging;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.XPath;

namespace OTools.Maps;

[DebuggerDisplay("{Name}, {HexValue}")]
public abstract class Colour : IStorable
{
    public Guid Id { get; init; }

    public string Name { get; set; }

    public virtual uint HexValue { get; protected set; }

    public float Opacity { get; set; }

    public ushort Precedence { get; internal set; }

    protected Colour(string name)
    {
        Id = Guid.NewGuid();

        Name = name;

        Opacity = 1f;
    }

    protected Colour(Guid id, string name)
    {
        Id = id;
        Name = name;

        Opacity = 1f;
    }

    public virtual (byte r, byte g, byte b) ToRGB()
    {
        byte b = (byte)(HexValue & 0x0000ff),
             g = (byte)((HexValue & 0x00ff00) >> 8),
             r = (byte)((HexValue & 0xff0000) >> 16);

        return (r, g, b);
    }

    public virtual (byte a, byte r, byte g, byte b) ToARGB()
    {
        byte b = (byte)(HexValue & 0x0000ff),
             g = (byte)((HexValue & 0x00ff00) >> 8),
             r = (byte)((HexValue & 0xff0000) >> 16),
             a = (byte)((HexValue & 0xff000000) >> 24);

        return (a, r, g, b);

    }

    public virtual (float c, float m, float y, float k) ToCMYK()
    {
        var rgb = ToRGB();

        float
            rd = (float)rgb.r / 255,
            gd = (float)rgb.g / 255,
            bd = (float)rgb.b / 255;

        float k = (float)(1 - Math.Max(rd, Math.Max(gd, bd)));

        float c = (float)((1 - rd - k) / (1 - k)),
              m = (float)((1 - gd - k) / (1 - k)),
              y = (float)((1 - bd - k) / (1 - k));

        if (float.IsNaN(c)) c = 0;
        if (float.IsNaN(m)) m = 0;
        if (float.IsNaN(y)) y = 0;
        if (float.IsNaN(k)) k = 0;

        return (c, m, y, k);
    }

    public override bool Equals(object? obj) => Equals(obj as Colour);

    public bool Equals(Colour? other)
    {
        return other is not null &&
               Id.Equals(other.Id) &&
               Name == other.Name &&
               HexValue == other.HexValue;
    }

    public static bool operator ==(Colour lhs, Colour rhs)
        => lhs.Equals(rhs);
    public static bool operator !=(Colour lhs, Colour rhs)
        => !(lhs == rhs);

    public static ColourLUT Lut { get; set; } = new();

    public static implicit operator uint(Colour colour)
        => colour.HexValue;

    private static readonly Guid s_transparentId = Guid.NewGuid();
    public static Colour Transparent
        => new RgbColour(s_transparentId, "Transparent", 0, 0, 0, 0) { Precedence = 0 };
}

public sealed class SpotColour : Colour
{
    public Dictionary<SpotCol, float> SpotColours { get; set; }

    public SpotColour(string name, Dictionary<SpotCol, float> spotColours)
        : base(name)
    {
        SpotColours = spotColours;
    }

    public SpotColour(Guid id, string name, Dictionary<SpotCol, float> spotColours)
        : base(id, name)
    {
        SpotColours = spotColours;
    }

    public override uint HexValue
    {
        get
        {
            var (c, m, y, k) = ToCMYK();
            byte r, g, b;

            if (Lut.TryGetValue((c, m, y, k), out var result))
            {
                r = result.Item1;
                g = result.Item2;
                b = result.Item3;
            }
            else 
            {
                r = (byte)(255 * (1 - c) * (1 - k));
                g = (byte)(255 * (1 - m) * (1 - k));
                b = (byte)(255 * (1 - y) * (1 - k));
            }

            return (uint)(b + (g << 8) + (r << 16) + ((byte)(Opacity * 255) << 24));
        }
    }

    public override (float c, float m, float y, float k) ToCMYK()
    {
        float c = 0f, m = 0f, y = 0f, k = 0f;

        foreach (var kvp in SpotColours)
        {
            var cmyk = kvp.Key.Colour.ToCMYK();

            c += cmyk.c * kvp.Value * (1 - c);
            m += cmyk.m * kvp.Value * (1 - m);
            y += cmyk.y * kvp.Value * (1 - y);
            k += cmyk.k * kvp.Value * (1 - k);
        }

        return (c, m, y, k);
    }
}

public sealed class RgbColour : Colour
{
    public byte Red { get; set; }
    public byte Green { get; set; }
    public byte Blue { get; set; }

    public RgbColour(string name, byte r, byte g, byte b, float a = 1f)
        : base(name)
    {
        Red = r;
        Green = g;
        Blue = b;

        Opacity = a;
    }
    
    public RgbColour(Guid id, string name, byte r, byte g, byte b, float a = 1f)
        : base(id, name)
    {
        Red = r;
        Green = g;
        Blue = b;

        Opacity = a;
    }
    public RgbColour(Guid id, string name, uint hexValue)
        : base(id, name)
    {
        Red = (byte)((hexValue & 0xff0000) >> 16);
        Green = (byte)((hexValue & 0x00ff00) >> 8);
        Blue = (byte)(hexValue & 0x0000ff);

        Opacity = 1f;
    }

    public override uint HexValue => (uint)(Blue + (Green << 8) + (Red << 16) + ((byte)(Opacity * 255) << 24));

    public override (byte r, byte g, byte b) ToRGB() 
        => (Red, Green, Blue);
}

public sealed class CmykColour : Colour
{
    public float Cyan { get; set; }
    public float Magenta { get; set; }
    public float Yellow { get; set; }
    public float Key { get; set; }

    public CmykColour(string name, float c, float m, float y, float k, float a = 1f)
        : base(name)
    {
        Cyan = c;
        Magenta = m;
        Yellow = y;
        Key = k;

        Opacity = a;
    }

    public CmykColour(Guid id, string name, float c, float m, float y, float k, float a = 1f)
        : base(id, name)
    {
        Cyan = c;
        Magenta = m;
        Yellow = y;
        Key = k;

        Opacity = a;
    }

    public override uint HexValue
    {
        get
        {
            byte r, g, b;

            if (Lut.TryGetValue((Cyan, Magenta, Yellow, Key), out var result))
            {
                r = result.Item1;
                g = result.Item2;
                b = result.Item3;
            }
            else
            {
                r = (byte)(255 * (1 - Cyan) * (1 - Key));
                g = (byte)(255 * (1 - Magenta) * (1 - Key));
                b = (byte)(255 * (1 - Yellow) * (1 - Key));
            }

            return (uint)(b + (g << 8) + (r << 16) + ((byte)(Opacity * 255) << 24));
        }
    }

    override public (float c, float m, float y, float k) ToCMYK()
        => (Cyan, Magenta, Yellow, Key);
}

public sealed class SpotCol : IStorable
{
    public Guid Id { get; init; }
    public string Name { get; set; }
    public CmykColour Colour { get; set; }

    public SpotCol(string name, CmykColour colour)
    {
        Id = Guid.NewGuid();
        Name = name;
        Colour = colour;
    }

    public SpotCol(Guid id, string name, CmykColour colour)
    {
        Id = id;
        Name = name;
        Colour = colour;
    }
}

public enum ColourSpace { RGB, CMYK }

public sealed class ColourLUT : Dictionary<(float, float, float, float), (byte, byte, byte)>, IParsable<ColourLUT>
{
    public bool IsLutEnabled { get; set; } = true;

    public (byte r, byte g, byte b) this[float c, float m, float y, float k]
        => this[(c, m, y, k)];
    public new (byte r, byte g, byte b) this[(float, float, float, float) col]
        => this[col];

    public new bool TryGetValue((float, float, float, float) key, out (byte r, byte g, byte b) result)
    {
        if (IsLutEnabled)
            return base.TryGetValue(key, out result);
        result = default;
        return false;
    }

    public static ColourLUT Create(IEnumerable<Colour> colours)
    {
        ColourLUT lut = new();

        foreach (Colour colour in colours)
        {
            if (colour is RgbColour) continue;

            var cmyk = colour.ToCMYK();
            var rgb = ColourConverter.Convert(cmyk);

            lut.TryAdd(cmyk, rgb);
        }

        return lut;
    }

    public static ColourLUT Parse(string s, IFormatProvider? provider)
    {
        if (TryParse(s, provider, out ColourLUT? result))
            return result;
        throw new Exception();
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out ColourLUT result)
    {
        ColourLUT lut = new();

        result = null;
        if (string.IsNullOrEmpty(s)) return false;

        var lines = s.Split('\n');

        foreach (string line in lines)
        {
            var split = line.Split(':');

            if (split.Length != 2) return false;

            var c = split[0].Split(',');
            if (c.Length != 4) return false;


            (float, float, float, float) cmyk = (0, 0, 0, 0);

            if (!(
                c[0].TryParse(out cmyk.Item1) &&
                c[1].TryParse(out cmyk.Item2) &&
                c[2].TryParse(out cmyk.Item3) &&
                c[3].TryParse(out cmyk.Item4)
                ))
                return false;

            var r = split[1].Split(',');
            if (r.Length != 3) return false;

            (byte, byte, byte) rgb = (0, 0, 0);

            if (!(
                r[0].TryParse(out rgb.Item1) &&
                r[1].TryParse(out rgb.Item2) &&
                r[2].TryParse(out rgb.Item3)
                ))
                return false;

            lut.Add(cmyk, rgb);
        }

        result = lut;
        return true;
    }

    public override string ToString()
    {
        StringBuilder sb = new();

        foreach (var (key, value) in this)
            sb.AppendLine($"{key.Item1 * 100}, {key.Item2 * 100}, {key.Item3 * 100}, {key.Item4 * 100} : {value.Item1}, {value.Item2}, {value.Item3}");

        return sb.ToString();
    }
}