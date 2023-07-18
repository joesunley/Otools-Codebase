using Avalonia.Controls;
using Avalonia.Interactivity;
using OTools.AvaCommon;
using OTools.Common;
using OTools.Maps;

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

			paintBox.Load(map);

			var edit = MapEdit.Create();
			var draw = MapDraw.Create();

			edit.Start();

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
	}
}