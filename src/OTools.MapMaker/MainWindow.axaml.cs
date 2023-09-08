using Avalonia.Controls;
using Avalonia.Interactivity;
using OTools.AvaCommon;
using OTools.Common;
using OTools.Maps;
using OTools.ObjectRenderer2D;
using OTools.Symbols;

namespace OTools.MapMaker
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			paintBox.PanTo(vec2.Zero);

			Manager.PaintBox = paintBox;

			Map map = MapLoader.Load(@"C:\Dev\OTools\test\Files\map2.xml");
			//Map map = OfficialSymbols.Create().CreateMap("ISOM");
			map.Colours.UpdatePrecendences(0);

			Manager.MapRenderer = new MapRenderer2D(map);

			Manager.MapEdit = MapEdit.Create();
			Manager.MapDraw = MapDraw.Create();

            Manager.Map = map;
			Manager.Symbol = map.Symbols["Contour"];

			paintBox.Load(Manager.Map, Manager.MapRenderer);

			Colour.Lut = map.MapInfo.ColourLUT;

			symbolPane.Load(map);

			btnColour.Click += (_, _) =>
			{
				Colour.Lut.IsLutEnabled = !Colour.Lut.IsLutEnabled;

				paintBox.Clear(Manager.Map.Id);
				paintBox.Load(Manager.Map, Manager.MapRenderer);
			};

			paintBox.ZoomChanged += _ => StatusBarUpdate();
			paintBox.MouseMoved += _ => StatusBarUpdate();

			KeyDown += (_, args) => WriteLine("Key Down: " + args.Key.ToString());
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

			// Invoke Handler
			Manager.MenuClickHandle(s);
		}

		void StatusBarUpdate()
		{
			statusBar.Text = $"Position: {paintBox.MousePosition.X:F2}, {paintBox.MousePosition.Y:F2}\tZoom: {paintBox.Zoom.X:F2}, {paintBox.Zoom.Y:F2}\tOffset: {paintBox.Offset.X:F2}, {paintBox.Offset.Y:F2}\tActive: {Manager.Tool}, {Manager.Symbol?.Name ?? "None"}";
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
