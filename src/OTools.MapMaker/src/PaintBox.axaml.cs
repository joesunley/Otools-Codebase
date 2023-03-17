using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using OTools.Maps;
using OTools.ObjectRenderer2D;
using System;
using System.Linq;

namespace OTools.MapMaker
{
    // Objects inside VisualBrush don't render


    public partial class PaintBox : UserControl
    {
        public PaintBox()
        {
            InitializeComponent();

            zoomBorder.ZoomChanged += (_, e) => ZoomChanged?.Invoke(e);
        }

        public void ZoomTo(vec2 centre, float factor)
        {
            vec2 newCentre = (canvas.Width/ 2, canvas.Height /2) + centre;

            zoomBorder.Zoom(factor, 0, 0);
            zoomBorder.Pan(newCentre.X, newCentre.Y);
        }

        public void PanTo(vec2 centre)
        {
            vec2 newCentre = (canvas.Width / 2, canvas.Height / 2) + centre;
            zoomBorder.Pan(newCentre.X, newCentre.Y);
        }

        public vec2 Zoom => new(zoomBorder.ZoomX, zoomBorder.ZoomY);
        public vec2 Offset => new(zoomBorder.OffsetX, zoomBorder.OffsetY);

        public event Action<ZoomChangedEventArgs>? ZoomChanged;

        public void Load(Map map)
        {
            canvas.Children.Clear();

            var render = new MapRender(map).Render();
            var els = render.Select(x => x.Item2).SelectMany(x => x);
            var conv = Convert.ConvertCollection(els);

            canvas.Children.AddRange(conv);
        }
    }
}
