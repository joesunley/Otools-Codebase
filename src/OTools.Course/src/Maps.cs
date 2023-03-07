using Avalonia.Media.Imaging;
using OTools.Maps;

public interface IEventMap {}

public sealed class OMap : IEventMap
{
	public Map Map { get; set; }
}

public sealed class VectorMap : IEventMap
{
	
}

public sealed class BitmapMap : IEventMap
{
	public Bitmap Bitmap { get; set; }

	void Load()
	{
	}
}