using Avalonia.Controls;
using OTools.AvaCommon;
using OTools.Maps;

namespace OTools.Routechoice;

public static class Manager
{

	public static PaintBox? PaintBox { get; set; }
	public static Course? Course { get; set; }
	public static Image? Image { get; set; }

	private static Tool _activeTool;

	public static Map Map { get; }

	public static Tool Tool
	{
		get => _activeTool;

		set
		{
			_activeTool = value;
			ActiveToolChanged?.Invoke(_activeTool);
		}
	}

	public static event Action<Tool>? ActiveToolChanged;

	static Manager()
	{
		Map = MapCreation.Create();
	}
}

public enum Tool { Course, Routechoice, Game }