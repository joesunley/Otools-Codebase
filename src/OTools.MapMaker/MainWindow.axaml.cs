using Avalonia.Controls;
using OTools.Maps;

namespace OTools.MapMaker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ViewManager.Set(paintBox);

            Map map = MapLoader.Load(@"C:\Dev\Phanes\tests\Files\map1.xml");
            //paintBox.Load(map);

            MapDraw draw = new();

            Manager.ActiveMap = map;
            Manager.ActiveSymbol = map.Symbols["Course Line"];

            paintBox.PanTo(vec2.Zero);

            symbolPane.Load(map);

            paintBox.ZoomChanged += e => statusBar.Text = $"Position: {ViewManager.MousePosition.X:F2}, {ViewManager.MousePosition.Y:F2}\tZoom: {e.ZoomX:F2}, {e.ZoomY:F2}\tOffset: {e.OffsetX:F2}, {e.OffsetY:F2}";
            ViewManager.MouseMove += args => statusBar.Text = $"Position: {args.X:F2}, {args.Y:F2}\tZoom: {ViewManager.Zoom.X:F2}, {ViewManager.Zoom.Y:F2}\tOffset: {ViewManager.Offset.X:F2}, {ViewManager.Offset.Y:F2}";
        }
    }
}