using System.Diagnostics;
using OTools.Common;
using SixLabors.ImageSharp.ColorSpaces;
using TerraFX.Interop.Windows;

namespace OTools.Maps;

[DebuggerDisplay("{Name}, {HexValue}")]
public abstract class Colour : IStorable
{
    public Guid Id { get; init; }

    public string Name { get; set; }

    public virtual uint HexValue { get; protected set; }

    public ushort Precedence { get; internal set; }

    protected Colour(string name)
    {
        Id = Guid.NewGuid();

        Name = name;
    }

    protected Colour(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public virtual (byte r, byte g, byte b) ToRGB()
    {
        byte b = (byte)(HexValue & 0x0000ff),
             g = (byte)((HexValue & 0x00ff00) >> 8),
             r = (byte)((HexValue & 0xff0000) >> 16);

        return (r, g, b);
    }

    public virtual (float c, float m, float y, float k) ToCMYK()
    {
        var rgb = ToRGB();

        float
            rd = (float)rgb.Item1 / 255,
            gd = (float)rgb.Item2 / 255,
            bd = (float)rgb.Item3 / 255;

        byte k = (byte)(1 - Math.Max(rd, Math.Max(gd, bd)));

        byte c = (byte)((1 - rd - k) / (1 - k)),
             m = (byte)((1 - gd - k) / (1 - k)),
             y = (byte)((1 - bd - k) / (1 - k));

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

    public static implicit operator uint(Colour colour)
        => colour.HexValue;

    private static readonly Guid s_transparentId = Guid.NewGuid();
    public static Colour Transparent
        => new RgbColour(s_transparentId, "Transparent", 0x0) { Precedence = 0 };
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
            (float c, float m, float y, float k) = ToCMYK();

            byte r = (byte)(255 * (1 - c) * (1 - k)),
                 g = (byte)(255 * (1 - m) * (1 - k)),
                 b = (byte)(255 * (1 - y) * (1 - k));

            return (uint)(b + (g << 8) + (r << 16));
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

    public RgbColour(string name, byte r, byte g, byte b)
        : base(name)
    {
        Red = r;
        Green = g;
        Blue = b;
    }
    
    public RgbColour(Guid id, string name, byte r, byte g, byte b)
        : base(id, name)
    {
        Red = r;
        Green = g;
        Blue = b;
    }
    public RgbColour(Guid id, string name, uint hexValue)
        : base(id, name)
    {
        Red = (byte)(hexValue & 0x0000ff);
        Green = (byte)((hexValue & 0x00ff00) >> 8);
        Blue = (byte)((hexValue & 0xff0000) >> 16);
    }

    public override uint HexValue => (uint)(Blue + (Green << 8) + (Red << 16));

    public override (byte r, byte g, byte b) ToRGB() 
        => (Red, Green, Blue);
}

public sealed class CmykColour : Colour
{
    public float Cyan { get; set; }
    public float Magenta { get; set; }
    public float Yellow { get; set; }
    public float Key { get; set; }

    public CmykColour(string name, float c, float m, float y, float k)
        : base(name)
    {
        Cyan = c;
        Magenta = m;
        Yellow = y;
        Key = k;
    }

    public CmykColour(Guid id, string name, float c, float m, float y, float k)
        : base(id, name)
    {
        Cyan = c;
        Magenta = m;
        Yellow = y;
        Key = k;
    }

    public override uint HexValue
    {
        get
        {
            byte r = (byte)(255 * (1 - Cyan) * (1 - Key)),
                 g = (byte)(255 * (1 - Magenta) * (1 - Key)),
                 b = (byte)(255 * (1 - Yellow) * (1 - Key));

            return (uint)(b + (g << 8) + (r << 16));
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

public enum ColourFormat { CMYK, RGB, Spot }