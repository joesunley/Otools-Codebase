using System.Runtime.InteropServices;

namespace OTools.Common;

public enum ColourSpace { RGB, CMYK }

public static class ColourConverter
{
    private static Uri? s_activeProfile;

    public static void SetUri(string filePath)
    {
        s_activeProfile = new(filePath);
    }

    public static (byte, byte, byte) Convert((float c, float m, float y, float k) col)
    {
        float[] colourValues = { col.c, col.m, col.y, col.k };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var color = System.Windows.Media.Color.FromValues(colourValues, s_activeProfile);

            return (color.R, color.G, color.B);
        }
        else // Implement other
        {
            byte r = (byte)(255 * (1 - col.c) * (1 - col.k)),
                g = (byte)(255 * (1 - col.m) * (1 - col.k)),
                b = (byte)(255 * (1 - col.y) * (1 - col.k));

            return (r, g, b);
        }
    }
}