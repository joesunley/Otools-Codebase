namespace OTools.Common;

public struct Distance : IEquatable<Distance>, IComparable<Distance>
{
    private long _value; // Distance in micrometres

    private Distance(long value) => _value = value;


    public decimal Millimetres => _value / 1_000m;
    public decimal Centimetres => _value / 10_000m;
    public decimal Metres => _value / 1_000_000m;
    public decimal Kilometres => _value / 1_000_000_000m;


    public static Distance FromMillimetres(decimal value)
    {
        return new((long)(value * 1_000));
    }

    public static Distance FromCentimetres(decimal value)
    {
        return new((long)(value * 10_000));
    }

    public static Distance FromMetres(decimal value)
    {
        return new((long)(value * 1_000_000));
    }

    public static Distance FromKilometres(decimal value)
    {
        return new((long)(value * 1_000_000_000));
    }

    public bool Equals(Distance other)
    {
        return _value == other._value;
    }
    public int CompareTo(Distance other)
    {
        return _value.CompareTo(other._value);
    }
}