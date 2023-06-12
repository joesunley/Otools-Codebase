using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace OTools.Common;

public static class Extensions
{
    public static IEnumerable<T> Yield<T>(this T item)
    {
        yield return item;
    }
}