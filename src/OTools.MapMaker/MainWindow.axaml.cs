using Avalonia.Controls;
using Avalonia.Interactivity;
using OTools.AvaCommon;
using OTools.Maps;
using OTools.ObjectRenderer2D;

namespace OTools.MapMaker
{
	public partial class MainWindow : Window
	{
		private MapMakerInstance _instance;

		public MainWindow()
		{
			InitializeComponent();

			Map map = MapLoader.Load(@"C:\Dev\OTools\test\Files\map2.xml");
			MapRenderer2D mapRenderer = new(map);

			_instance = new(paintBox, map, mapRenderer, default, default, default);

			Colour.Lut = map.MapInfo.ColourLUT;

			paintBox.ZoomChanged   += _      => StatusBarUpdate();
			paintBox.PointerMoved  += (_, _) => StatusBarUpdate();

			paintBox.KeyDown += (_, args) => WriteLine("Key Down: " + args.Key.ToString());

			btnColour.Click += (_, _) =>
			{
				Colour.Lut.IsLutEnabled = !Colour.Lut.IsLutEnabled;

				paintBox.Clear(map.Id);
				paintBox.Load(map, mapRenderer);
			};

			_instance.ActiveSymbol = map.Symbols[0];

			paintBox.Load(map, mapRenderer);
			symbolPane.Load(_instance);

			paintBox.PanTo(vec2.Zero);
		}

		private bool _isDebug = false;
		private void OnDebugClicked(object? sender, RoutedEventArgs e)
		{
			_isDebug = !_isDebug;

			if (_isDebug)
			{
				DebugTools.AllocConsole();
				btnDebug.Content = "Hide Debug";
			}
			else
			{
				DebugTools.FreeConsole();
				btnDebug.Content = "Show Debug";
			}
		}

		private void MenuItemClicked(object? sender, RoutedEventArgs e)
		{
			MenuItem s = sender as MenuItem
				?? throw new InvalidOperationException();

			_instance.MenuManager.Call(s);
		}

		void StatusBarUpdate()
		{
			statusBar.Text = $"Position: {paintBox.MousePosition.X:F2}, {paintBox.MousePosition.Y:F2}\tZoom: {paintBox.Zoom.X:F2}, {paintBox.Zoom.Y:F2}\tOffset: {paintBox.Offset.X:F2}, {paintBox.Offset.Y:F2}\tActive: {_instance.ActiveTool}, {_instance.ActiveSymbol?.Name ?? "None"}";
		}
	}
}

/*Mapper Menu
* 
*  File
*      New
*          Map
*          Event
*      Open
*      Save
*      Save as
*      Export As
*          Image
*          PDF
*          OCAD / OMAP
*      Print
*      Exit
*  Edit
*      Undo
*      Redo
*      Cut
*      Copy
*      Paste
*      Delete
*  Tools
*      Select
*      Place Point
*      Draw Path
*      Draw Ellipse
*      Draw Rectangle
*      Draw Free Hand
*      Fill Between Objects
*      Rotate Object
*      Scale Object
*      Stretch Object
*      Rotate Pattern
*      Convert to Bezier Path
*      Simplify Path
*  Map
*      Georeferencing
*      Layers
*          Add New Map Layer
*          Add New Template Layer
*  Symbols
*      Load Symbol Set
*      Edit Symbols
*  View
*      Zoom to Entire Map
*      Show Grid
*  Panes
*      Colours
*      Symbols
*      Templates
*  Help
*/
