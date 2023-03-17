using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Media;
using OTools.Maps;
using System.Data;

namespace OTools.MapViewer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //Map map = MapLoader.Load(@"C:\Users\Joe\Downloads\map2.xml");
            //viewport.LoadAll(map);

            //viewport.zoomBorder.ZoomChanged += (s, e) =>
            //{
            //    stack.Text = $"X: {e.ZoomX:F2}, Y: {e.ZoomY:F2}\nX: {e.OffsetX:F2}, Y: {e.OffsetY:F2}";
            //};
        }
    }
}