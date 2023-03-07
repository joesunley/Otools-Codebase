using System.Diagnostics;
using OTools.Common;


namespace OTools.Maps;

[DebuggerDisplay("{Name}, {HexValue}")]
public sealed class Colour : IStorable, IEquatable<Colour?>
{
    public Guid Id { get; init; }

    public string Name { get; set; }
    public uint HexValue { get; set; }

    public Colour(string name, uint hexValue)
    {
        Id = Guid.NewGuid();

        Name = name;
        HexValue = hexValue;
    }

    public Colour(Guid id, string name, uint hexValue)
    {
        Id = id;
        Name = name;
        HexValue = hexValue;
    }

    public Colour(string name, (byte r, byte g, byte b) rgb)
    {
        Id = Guid.NewGuid();

        Name = name;
        HexValue = (uint)(rgb.b + (rgb.g << 8) + (rgb.r << 16));
    }

    public Colour(string name, (byte c, byte m, byte y, byte k) cmyk, float alpha = 1f)
    {
        float c = cmyk.c / 100f,
              m = cmyk.m / 100f,
              y = cmyk.y / 100f,
              k = cmyk.k / 100f;

        byte r = (byte)(255 * (1 - c) * (1 - k)),
             g = (byte)(255 * (1 - m) * (1 - k)),
             b = (byte)(255 * (1 - y) * (1 - k)),
             a = (byte)(255 * alpha);

        Id = Guid.NewGuid();

        Name = name;
        HexValue = (uint)(b + (g << 8) + (r << 16) + (a << 24));
    }

    public Colour(uint hexValue)
    {
        Id = Guid.NewGuid();

        Name = string.Empty;
        HexValue = hexValue;
    }
    public (byte r, byte g, byte b) ToRGB()
    {
        byte b = (byte)(HexValue & 0x0000ff),
             g = (byte)((HexValue & 0x00ff00) >> 8),
             r = (byte)((HexValue & 0xff0000) >> 16);

        return (r, g, b);
    }

    public (byte r, byte g, byte b, byte a) ToRGBA()
    {
        byte b = (byte)(HexValue & 0x0000000ff),
             g = (byte)((HexValue & 0x0000ff00) >> 8),
             r = (byte)((HexValue & 0x00ff0000) >> 16),
             a = (byte)((HexValue & 0xff000000) >> 24);

        return (r, g, b, a);

    }

    public (byte c, byte m, byte y, byte k) ToCMYK()
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

    public override bool Equals(object? obj)
    {
        return Equals(obj as Colour);
    }

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


    public static implicit operator Colour(uint hexValue)
        => new(hexValue);

    public static implicit operator uint(Colour colour)
        => colour.HexValue;

    private static readonly Guid s_id = Guid.NewGuid();
    public static Colour Transparent
        => new(s_id, "Transparent", 0x0);


}