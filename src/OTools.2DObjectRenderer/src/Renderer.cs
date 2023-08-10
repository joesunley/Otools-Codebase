namespace OTools.ObjectRenderer2D;

public interface IVisualRenderer
{
	public IEnumerable<(Guid, IEnumerable<IShape>)> Render();
}