using OTools.ObjectRenderer2D;

namespace OTools.Pathfinder;

public static class PathSmoothing
{
    public static IList<vec2> Smooth(IList<vec2> path, IEnumerable<vec4> barriers)
    {
        List<vec2> newPath = new() { path[0], path[1] };

        for (int i = 0; i < path.Count - 1; i++)
        {
            vec2? latest = null;

            for (int j = i+1; j < path.Count; j++)
            {
                vec4 line = (path[i], path[j]);

                if(!barriers.Any(b => PolygonTools.DoLinesIntersect(line, b)))
                    path[^1] = path[j];
            }
        }

        // Theoretical
        return newPath;
    }
}