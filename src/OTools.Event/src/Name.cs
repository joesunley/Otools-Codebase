namespace OTools.Events;

public struct Name : IEquatable<Name>
{
    public string Fore { get; set; }
    public string Last { get; set; }

    public Name()
    {
        Fore = string.Empty;
        Last = string.Empty;
    }

    public Name(string name)
    {
        this = Create(name);
    }

    public Name(string fore, string last)
    {
        Fore = fore;
        Last = last;
    }

    public static implicit operator Name((string, string) name) => new(name.Item1, name.Item2);
    public static implicit operator (string, string)(Name name) => (name.Fore, name.Last);

    public static Name Create(string name)
    {
        var parts = name.Split(' ');

        if (parts.Length == 0)
            return new();
        else if (parts.Length == 1)
            return new(parts[0], string.Empty);
        else if (parts.Length == 2)
            return new(parts[0], parts[1]);
        else
            return new(parts[0], string.Concat(parts[1..]));
    }

    public bool Equals(Name other) => Fore.Equals(other.Fore) && Last.Equals(other.Last);
    public override bool Equals(object? obj) => obj is Name other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(Fore, Last);
    public override string ToString() => $"{Fore} {Last}";
    public static bool operator ==(Name left, Name right) => left.Equals(right);
    public static bool operator !=(Name left, Name right) => !(left == right);

}