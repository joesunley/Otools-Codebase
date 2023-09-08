using System.Text;

namespace OTools.Common;

public sealed class Tag : List<Guid>
{
    public override string ToString()
    {
        StringBuilder sb = new();

        foreach (Guid id in this)
        {
            sb.Append(id.ToString());
            sb.Append(',');
        }

        return sb.ToString();
    }
}