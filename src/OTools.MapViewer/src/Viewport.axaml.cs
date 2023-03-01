using Avalonia.Controls;
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

            scrollViewer.ScrollChanged += (s, e) => e.Handled = true;
        }

        public void LoadAll(Map map)
		{
			canvas.Children.Clear();
			
			var render = new MapRender(map).Render();
			var els = render.Select(x => x.Item2).SelectMany(x => x);
			var conv = Convert.ConvertCollection(els);
			
			canvas.Children.AddRange(conv);
		}
    }
}
