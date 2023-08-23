using System.Text;

namespace OTools.Common;

public sealed class Tag : List<object>
{
    public override string ToString()
    {
        StringBuilder sb = new();

        foreach (object obj in this)
        {
            sb.Append(obj.ToString());
            sb.Append(',');
        }

        return sb.ToString();
    }
}