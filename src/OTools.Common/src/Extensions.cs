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

    private static Random s_rng = new();
    public static IList<T> Shuffle<T>(this IList<T> list)
    {
        T[] arr = new T[list.Count];
        list.CopyTo(arr, 0);
        int n = arr.Length;
        while (n > 1)
        {
            n--;
            int k = s_rng.Next(n + 1);
            T temp = arr[k];
            arr[k] = arr[n];
            arr[n] = temp;
        }
        return arr;
    }
}