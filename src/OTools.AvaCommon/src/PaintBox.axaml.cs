using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using OTools.Maps;
using OTools.ObjectRenderer2D;
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls.Shapes;
using Ava = Avalonia.Controls.Shapes;

namespace OTools.AvaCommon
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
            var conv = ObjConvert.ConvertCollection(els);

            canvas.Children.AddRange(conv);
        }
        
        #region ViewManager

        private List<Guid> _ids = new();
        
        public void Add(Guid id, IEnumerable<Shape> objects)
        {
            WriteLine($"Added {id}");

            objects = objects.Select((x =>
            {
                x.Tag = id.ToString();
                return x;
            }));
            
            canvas.Children.AddRange(objects);
            _ids.Add(id);
        }

        public void Update(Guid id, IEnumerable<Shape> objects)
        {
            Assert(_ids.Contains(id));
            WriteLine($"Updated {id}");
            
            objects = objects.Select((x =>
            {
                x.Tag = id.ToString();
                return x;
            }));

            var els = canvas.Children.Select(x => (Shape)x).Where(x => x.Tag is string s && s.Contains(id.ToString()));
            canvas.Children.RemoveAll(els);   
            
            canvas.Children.AddRange(objects);
        }

        public void AddOrUpdate(Guid id, IEnumerable<Shape> objects)
        {
            if (_ids.Contains(id))
                Update(id, objects);
            else
                Add(id, objects);
        }

        public void Remove(Guid id)
        {
            Assert(_ids.Contains(id));
            WriteLine($"Removed {id}");
            
            var els = canvas.Children.Select(x => (Shape)x).Where(x => x.Tag is string s && s.Contains(id.ToString()));
            canvas.Children.RemoveAll(els);
            
            _ids.Remove(id);
        }
        
        public void Clear()
        {
            WriteLine("Cleared");

            foreach (Guid id in _ids)
            {
                var els = canvas.Children.Select(x => (Shape)x).Where(x => x.Tag is string s && s.Contains(id.ToString()));
                canvas.Children.RemoveAll(els);

                _ids.Remove(id);
            }
            
            Assert(!_ids.Any());
        }
        
        #endregion
        
    }
}
