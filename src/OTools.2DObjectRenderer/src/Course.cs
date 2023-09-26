using OTools.Courses;
using OTools.Maps;
using ownsmtp.logging;

namespace OTools.ObjectRenderer2D;

public class CourseRenderer2D : IVisualRenderer
{
    private readonly Event _activeEvent;
    private readonly IMapRenderer2D _symbolRenderer;

    public CourseRenderer2D(Event activeEvent)
    {
        _activeEvent = activeEvent;
        _symbolRenderer = new MapRenderer2D(null);
    }

    public IEnumerable<(Guid, IEnumerable<IShape>)> Render()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IShape> RenderNormalControl(Control control)
    {
        var shapes = _symbolRenderer.RenderPointSymbol(_activeEvent.Symbols.Control);

        return shapes.Select(x => { x.TopLeft = control.Position; return x; });
    }

    
}