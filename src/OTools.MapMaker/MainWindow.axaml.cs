using Avalonia.Controls;
using Avalonia.Interactivity;
using OTools.AvaCommon;
using OTools.Common;
using OTools.Maps;
using OTools.ObjectRenderer2D;

namespace OTools.MapMaker
{
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();

			paintBox.PanTo(vec2.Zero);

			Manager.PaintBox = paintBox;
			ViewManager.Set(paintBox, this);

			Map map = MapLoader.Load(@"C:\Dev\Phanes\tests\Files\map1.xml");
			map.Colours.UpdatePrecendences(0);

			//Map map = DefaultISOM();

			Manager.MapRenderer = new MapRenderer2D(map);
			var render = Manager.MapRenderer.RenderMap();
			
			paintBox.Load(render.Select(x => (x.Item1.Id, x.Item2)));

			Manager.MapEdit = MapEdit.Create();
            Manager.MapDraw = MapDraw.Create();

			Manager.Map = map;
			Manager.Symbol = map.Symbols["Course Line"];

			symbolPane.Load(map);


			paintBox.ZoomChanged += e => statusBar.Text = $"Position: {paintBox.MousePosition.X:F2}, {paintBox.MousePosition.Y:F2}\tZoom: {e.ZoomX:F2}, {e.ZoomY:F2}\tOffset: {e.OffsetX:F2}, {e.OffsetY:F2}";
			paintBox.MouseMoved += args => statusBar.Text = $"Position: {args.Position.X:F2}, {args.Position.Y:F2}\tZoom: {paintBox.Zoom.X:F2}, {paintBox.Zoom.Y:F2}\tOffset: {paintBox.Offset.X:F2}, {paintBox.Offset.Y:F2}";

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

        private void CheckBoxClicked(object? sender, RoutedEventArgs e) { }
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
/*Planner Menu
 * 
 *  File
 *      New
 *          Map
 *          Event
 *      Open
 *      Save
 *      Save As
 *      Export As
 *          Image
 *          PDF
 *          OCAD / OMAP
 *          Routegadget File
 *          IOF XML
 *          Maprun File
 *      Print
 *          Courses
 *          Descriptions
 *      Exit
 *  Edit
 *      Undo
 *      Redo
 *      Cut
 *      Copy
 *      Paste
 *      Delete
 *  Tools
 *      Map Issue Point
 *      Start
 *      Control
 *      Finish
 *      Out-of-Bounds Boundary
 *      Out-of-Bounds Area
 *      Out-of-Bounds Border
 *      Crossing Point
 *      Crossing Section
 *      Temporary Construction or Closed Area
 *      Variations
 *          Forking
 *          Butterfly Loop
 *          Phi-Loop
 *  Map
 *      Change Map File
 *      Edit Map
 *  Event
 *      Courses
 *          Add Course
 *          Delete Course
 *          Properties
 *      Control Numbering
 *  View
 *      Zoom
 *          Entire Map
 *          Entire Course
 *      Show Grid
 *      Print Mode
 *  Help
 */