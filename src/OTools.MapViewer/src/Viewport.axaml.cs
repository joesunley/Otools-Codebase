using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Microsoft.Win32.SafeHandles;
using OTools.Maps;
using OTools.ObjectRenderer2D;
using System.Linq;

namespace OTools.MapViewer
{
    public partial class Viewport : UserControl
    {
        public Viewport()
        {
            InitializeComponent();

            zoomBorder.Pan(-500000, -500000);
        }

        public void LoadAll(Map map)
        {

            canvas.Children.Clear();

            var render = new MapRenderer2D(map).RenderMap();
            var els = render.Select(x => x.Item2).SelectMany(x => x);
            var conv = Convert.ConvertCollection(els);

            canvas.Children.AddRange(conv);
        }
    }
}
