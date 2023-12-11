using OTools.Common;
using OTools.Maps;
using Sunley.Mathematics;
using System.Xml.Schema;

namespace OTools.ObjectRenderer2D;

public sealed class GridRenderer2D : IVisualRenderer
{
    public vec2 Spacing { get; set; }
    public vec2 Offset { get; set; }
    public vec4 Extents { get; set; }

    public LineSymbol GridSymbol { get; set; }

    private IMapRenderer2D _mapRenderer;

    public GridRenderer2D(vec2 spacing, vec2 offset, vec4 extents, LineSymbol gridSymbol, IMapRenderer2D? renderer = null)
    {
        Spacing = spacing;
        Offset = offset;
        Extents = extents;
        GridSymbol = gridSymbol;

        _mapRenderer = renderer ?? new MapRenderer2D(null);
    }

    public IEnumerable<(Guid, IEnumerable<IShape>)> Render()
    {
        List<IShape> shapes = new();

        // Vertical

        float x = Offset.X;
        while (x < Extents.Z)
        {
            PathCollection pC = new(new vec2[] { (x, Extents.Y), (x, Extents.W) });
            LineInstance line = new(0, GridSymbol, pC, false);

            shapes.AddRange(_mapRenderer.RenderPathInstance(line));

            x += Spacing.X;
        }

        x = Offset.X;
        while (x > Extents.X)
        {
            PathCollection pC = new(new vec2[] { (x, Extents.Y), (x, Extents.W) });
            LineInstance line = new(0, GridSymbol, pC, false);
            
            shapes.AddRange(_mapRenderer.RenderPathInstance(line));

            x -= Spacing.X;
        }

        // Horizontal

        float y = Offset.Y;
        while (y < Extents.W)
        {
            PathCollection pC = new(new vec2[] { (Extents.X, y), (Extents.Z, y) });
            LineInstance line = new(0, GridSymbol, pC, false);

            shapes.AddRange(_mapRenderer.RenderPathInstance(line));

            y += Spacing.Y;
        }

        y = Offset.Y;
        while (y > Extents.Y)
        {
            PathCollection pC = new(new vec2[] { (Extents.X, y), (Extents.Z, y) });
            LineInstance line = new(0, GridSymbol, pC, false);

            shapes.AddRange(_mapRenderer.RenderPathInstance(line));

            y -= Spacing.Y;
        }

        return (Guid.NewGuid(), (IEnumerable<IShape>)shapes).Yield();
    }


}