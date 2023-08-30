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

			Map map = MapLoader.Load(@"I:\OTools\test\Files\map4.xml");
			//Map map = OfficialSymbols.Create().CreateMap("ISOM");
			map.Colours.UpdatePrecendences(0);

			//var outp = MapLoader.Save(map, 2);
			//outp.Serialize(@"I:\OTools\test\Files\map4.xml");

			Manager.MapRenderer = new MapRenderer2D(map);

			paintBox.Load(Manager.MapRenderer.Render());

			Manager.MapEdit = MapEdit.Create();
			Manager.MapDraw = MapDraw.Create();

			Manager.Map = map;
			Manager.Symbol = map.Symbols["Contour"];

			string profileLocation = @"I:\OTools\lib\PSOcoated_v3.icc";
			ColourConverter.SetUri(profileLocation);

			if (map.MapInfo.ColourLUT.Count == 0)
				map.MapInfo.ColourLUT = ColourLUT.Create(map.Colours);

			Colour.Lut = map.MapInfo.ColourLUT;

			symbolPane.Load(map);

			btnColour.Click += (_, _) => Colour.Lut.IsLutEnabled = !Colour.Lut.IsLutEnabled;

			paintBox.MouseMoved += e =>
			{ // Dangerous
				AreaInstance area = (AreaInstance)map.Instances[Guid.Parse("a9cd3252-f3b5-4faf-8d22-8431cd66f24c")];

				List<vec2> p = new();
				foreach (var seg in area.Segments)
				{
					p.AddRange(seg switch
					{
						LinearPath l => l,
						BezierPath b => b.LinearApproximation(),
					});
				}

				List<IList<vec2>> hoels = new();

				foreach (var hole in area.Holes)
				{
					List<vec2> p2 = new();

					foreach (var seg in hole)
					{
						p2.AddRange(seg switch
						{
							LinearPath l => l,
							BezierPath b => b.LinearApproximation(),
						});
					}

					hoels.Add(p2);
				}

				bool res = PolygonTools.IsPointInPoly(p, hoels, e.Position);

				statusBar.Text = res.ToString();

			};

			//paintBox.ZoomChanged += e => statusBar.Text = $"Position: {paintBox.MousePosition.X:F2}, {paintBox.MousePosition.Y:F2}\tZoom: {e.ZoomX:F2}, {e.ZoomY:F2}\tOffset: {e.OffsetX:F2}, {e.OffsetY:F2}";
			//paintBox.MouseMoved += args => statusBar.Text = $"Position: {args.Position.X:F2}, {args.Position.Y:F2}\tZoom: {paintBox.Zoom.X:F2}, {paintBox.Zoom.Y:F2}\tOffset: {paintBox.Offset.X:F2}, {paintBox.Offset.Y:F2}";

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
