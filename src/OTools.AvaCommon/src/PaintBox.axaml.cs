using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Input;
using Avalonia.Media;
using OTools.Common;
using OTools.Maps;
using OTools.ObjectRenderer2D;
using ownsmtp.logging;
using System.Diagnostics;

namespace OTools.AvaCommon
{
    public partial class PaintBox : UserControl
	{
		public PaintBox()
		{
			InitializeComponent();

			zoomBorder.ZoomChanged += (_, e) => ZoomChanged?.Invoke(e);

			canvas.PointerMoved += (sender, args) =>
			{
				var point = args.GetCurrentPoint(canvas);
				MousePosition = (point.Position.X, point.Position.Y);
				PointerMoved?.Invoke(sender, new()
				{
					Position = MousePosition,
					Modifiers = args.KeyModifiers,
					Properties = point.Properties,
				});
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
		public new event EventHandler<MouseMovedEventArgs>? PointerMoved;

		public void Load(Map map, IMapRenderer2D? mapRenderer = null)
		{
			canvas.Children.Clear();

			var render = (mapRenderer ?? new MapRenderer2D(map)).RenderMap();

			foreach (var (inst, els) in render)
			{
				var conv = ObjConvert.ConvertCollection(els);

				Add(new Tag() { inst.Id, map.Id}, conv);
			}
		}
		public void Load(IEnumerable<(Guid, IEnumerable<IShape>)> objects, Guid? oId = null)
		{
			Clear();

			if (oId is null)
				foreach (var (id, els) in objects)
					Add(id, els.ConvertCollection());
			else
				foreach (var (id, els) in objects)
                    Add(new Tag() { id, oId.Value }, els.ConvertCollection());
			
		}


		#region ViewManager

		private readonly List<Guid> _ids = new();

        public IEnumerable<Control> this[Guid id] 
			=> canvas.Children.Select(x => x).Where(x => x.Tag is string s && s.Contains(id.ToString()));
		
		public void Add(Guid id, IEnumerable<Control> objects)
		{
			ODebugger.Assert(!_ids.Contains(id));
			ODebugger.Info($"Added {id}");

			objects = objects.Select(x =>
			{
				Tag tag = x.Tag as Tag ?? new();
				tag.Add(id);
				x.Tag = tag;

				return x;
			});
			
			canvas.Children.AddRange(objects);
			_ids.Add(id);
		}
		public void Add(Tag tag, IEnumerable<Control> objects)
		{
			ODebugger.Info($"Added {tag}");

			objects = objects.Select(x =>
			{
                x.Tag = tag;
                return x;
            });

			canvas.Children.AddRange(objects);
			_ids.AddRange(tag);
		}

		public void Update(Guid id, IEnumerable<Control> objects)
		{
			Debug.Assert(_ids.Contains(id));
			ODebugger.Info($"Updated {id}");
			
			objects = objects.Select(x =>
			{
                Tag tag = x.Tag as Tag ?? new();
                tag.Add(id);
                x.Tag = tag;

                return x;
            });

			var els = canvas.Children.Where(x => (x.Tag.ToString() ?? string.Empty).Contains(id.ToString()));
			canvas.Children.RemoveAll(els);   
			
			canvas.Children.AddRange(objects);
		}
		public void Update(Guid id, Tag tag, IEnumerable<Control> objects)
		{
			// Check only one id occurs
			ODebugger.Assert(_ids.Where(x => x == id).Count() == 1);
			ODebugger.Info($"Updated {tag}");

			objects = objects.Select(x =>
			{
                x.Tag = tag;
                return x;
            });

			var els = canvas.Children.Where(x => (x.Tag?.ToString() ?? string.Empty).Contains(id.ToString()));
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
			ODebugger.Assert(_ids.Contains(id));
			ODebugger.Info($"Removed {id}");

            var els = canvas.Children.Where(x => (x.Tag?.ToString() ?? string.Empty).Contains(id.ToString()));
            canvas.Children.RemoveAll(els);
			
			_ids.Remove(id);
		}
		
		public void Clear()
		{
			ODebugger.Info("Cleared");

			foreach (Guid id in _ids)
			{
                var els = canvas.Children.Where(x => (x.Tag?.ToString() ?? string.Empty).Contains(id.ToString()));
                canvas.Children.RemoveAll(els);

			}

			_ids.Clear();
		}
		public void Clear(Guid id)
		{
			ODebugger.Info($"Cleared {id}");

			var els = canvas.Children.Where(x => (x.Tag?.ToString() ?? string.Empty).Contains(id.ToString()));

			canvas.Children.RemoveAll(els);
			_ids.RemoveAll(x => x == id);
		}

		#endregion

		#region ViewManager2
		public Guid CurrentLayer { get; set; }

		private List<Layer> _layers = new();

		public void Add2(Guid id, IEnumerable<Control> objects)
		{
			ODebugger.Info($"Added {id} to {CurrentLayer}");

			objects = objects.Select(x =>
			{
				Tag tag = x.Tag as Tag ?? new();
				tag.Add(id);
				tag.Add(CurrentLayer);
				x.Tag = tag;

				return x;
			});

			canvas.Children.AddRange(objects);

			//_layers[0].ObjectIds.Add(id);
		}
		public void AddLayer(Guid layerId, IEnumerable<(Guid id, IEnumerable<Control> objects)> layer)
		{
			ODebugger.Info($"Added Layer {layerId}");

			foreach (var obj in layer)
			{
				var objs = obj.objects.Select(x =>
				{
					Tag tag = x.Tag as Tag ?? new();
					tag.Add(obj.id);
					tag.Add(layerId);
					x.Tag = tag;

					return x;
				});

				canvas.Children.AddRange(objs);
			}

			Layer l = new()
			{
				Id = layerId,
                ObjectIds = new(layer.Select(x => x.id)),
            };

			_layers.Add(l);
        }


		#endregion
	}
}

internal struct Layer : IStorable
{
	public Guid Id { get; set; }
	public List<Guid> ObjectIds { get; set; }

	public static implicit operator Guid(Layer layer) => layer.Id;
}

public struct MouseMovedEventArgs
{
	public vec2 Position { get; set; }
	public KeyModifiers Modifiers { get; set; }
	public PointerPointProperties Properties { get; set; }
}
