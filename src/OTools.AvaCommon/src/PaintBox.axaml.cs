using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using OTools.Maps;
using OTools.ObjectRenderer2D;
using OTools.Common;
using ownsmtp.logging;

namespace OTools.AvaCommon
{
    // Objects inside VisualBrush don't render


    public partial class PaintBox : UserControl
    {
        public PaintBox()
        {
            InitializeComponent();

            zoomBorder.ZoomChanged += (_, e) => ZoomChanged?.Invoke(e);

            canvas.PointerMoved += (_, args) =>
            {
                var point = args.GetCurrentPoint(canvas).Position;
                MousePosition = (point.X, point.Y);
                MouseMoved?.Invoke(MousePosition);
            };
        }

        public void ZoomTo(vec2 centre, float factor)
        {
            vec2 newCentre = (canvas.Width/ 2, canvas.Height /2) + centre;

            zoomBorder.Zoom(factor, 0, 0);
            zoomBorder.Pan(newCentre.X, newCentre.Y);
        }

        public void PanTo(vec2 centre)
        {
            vec2 newCentre = (canvas.Width / 2, canvas.Height / 2) + centre * -1;
            zoomBorder.Pan(newCentre.X, newCentre.Y);
        }
        public void PanTo(float x, float y) => PanTo((x, y));

		public void Rotate(vec2 centre, float angle)
		{
			canvas.RenderTransform = new RotateTransform(angle, centre.X, centre.Y);
		}

		public void Rotate() => Rotate(vec2.Zero, 0f);

        public vec2 Zoom => new(zoomBorder.ZoomX, zoomBorder.ZoomY);
        public vec2 Offset => new(zoomBorder.OffsetX, zoomBorder.OffsetY);
        public vec2 TopLeft => vec2.Zero;

        public event Action<ZoomChangedEventArgs>? ZoomChanged;

        public vec2 MousePosition { get; private set; }
        public event Action<vec2>? MouseMoved;

        public void Load(Map map)
        {
            canvas.Children.Clear();

            var render = new MapRenderer2D(map).Render();
            var els = render.Select(x => x.Item2).SelectMany(x => x);
            var conv = ObjConvert.ConvertCollection(els);

            canvas.Children.AddRange(conv);
        }


        #region ViewManager

        private List<Guid> _ids = new();
        
        public void Add(Guid id, IEnumerable<Control> objects)
        {
            Debugger.Info($"Added {id}");

            objects = objects.Select(x =>
            {
                x.Tag = id.ToString();
                return x;
            });
            
            canvas.Children.AddRange(objects);
            _ids.Add(id);
        }

        public void Update(Guid id, IEnumerable<Control> objects)
        {
            Debugger.Assert(_ids.Contains(id));
            Debugger.Info($"Updated {id}");
            
            objects = objects.Select(x =>
            {
                x.Tag = id.ToString();
                return x;
            });

            var els = canvas.Children.Select(x => (Control)x).Where(x => x.Tag is string s && s.Contains(id.ToString()));
            canvas.Children.RemoveAll(els);   
            
            canvas.Children.AddRange(objects);
        }

        public void AddOrUpdate(Guid id, IEnumerable<Control> objects)
        {
            if (_ids.Contains(id))
                Update(id, objects);
            else
                Add(id, objects);
        }

        public void Remove(Guid id)
        {
            Debugger.Assert(_ids.Contains(id));
            Debugger.Info($"Removed {id}");
            
            var els = canvas.Children.Select(x => (Control)x).Where(x => x.Tag is string s && s.Contains(id.ToString()));
            canvas.Children.RemoveAll(els);
            
            _ids.Remove(id);
        }
        
        public void Clear()
        {
            Debugger.Info("Cleared");

            foreach (Guid id in _ids)
            {
                var els = canvas.Children.Select(x => (Control)x).Where(x => x.Tag is string s && s.Contains(id.ToString()));
                canvas.Children.RemoveAll(els);

                _ids.Remove(id);
            }
            
            Debugger.Assert(!_ids.Any());
        }

        #endregion

    }
}
