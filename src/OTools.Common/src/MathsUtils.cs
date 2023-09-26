using Sunley.Mathematics;

namespace OTools.Common;

public static class MathUtils
{
    public static IEnumerable<vec2> CreateEquilateralTriangle(float size, vec2? centreParam = null)
    {
        vec2 centre = centreParam ?? vec2.Zero;
        float sizeSquared = (float)Math.Pow(size, 2);

        float y = sizeSquared / (2 * size);
        float x = (float)Math.Sqrt(sizeSquared - Math.Pow(y, 2));

        vec2 a = (0, 0), b = (size, 0), c = (y, -x);

        vec2 off = (y, -x / 2.5);



        return new[] { a, b, c }.Select(x => x - off + centre);
    }

    public static int Permutations(int n, int r)
    {
        int result = 1;

        for (int i = n; i > n - r; i--)
            result *= i;

        return result;
    }

    // https://stackoverflow.com/questions/756055/listing-all-permutations-of-a-string-integer/13022090#13022090

    public static IEnumerable<IEnumerable<T>> GetPermutations<T>(this IEnumerable<T> list)
    {
        var array = list as T[] ?? list.ToArray();

        var factorials = Enumerable.Range(0, array.Length + 1)
            .Select(Factorial)
            .ToArray();

        for (var i = 0L; i < factorials[array.Length]; i++)
        {
            var seq = GenerateSequence(i, array.Length - 1, factorials);

            yield return GeneratePermutation(array, seq);
        }
    }

    private static IEnumerable<T> GeneratePermutation<T>(T[] array, IReadOnlyList<int> seq)
    {
        var clone = (T[])array.Clone();

        for (int i = 0; i < clone.Length - 1; i++)
        {
            Swap(ref clone[i], ref clone[i + seq[i]]);
        }

        return clone;
    }

    private static int[] GenerateSequence(long number, int size, IReadOnlyList<long> factorials)
    {
        var seq = new int[size];

        for (var i = 0; i < size; i++)
        {
            var factorial = factorials[size - i];

            seq[i] = (int)(number / factorial);
            number %= factorial;
        }

        return seq;
    }

    static void Swap<T>(ref T a, ref T b)
    {
        T temp = a;
        a = b;
        b = temp;
    }

    private static long Factorial(int n)
    {
        long result = n;

        for (int i = 1; i < n; i++)
            result *= i;

        return result;
    }

}