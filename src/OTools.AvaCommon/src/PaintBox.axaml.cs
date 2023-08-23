using Avalonia.Controls;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Controls.Shapes;
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

			canvas.PointerMoved += (_, args) =>
			{
				var point = args.GetCurrentPoint(canvas);
				MousePosition = (point.Position.X, point.Position.Y);
				MouseMoved?.Invoke(new()
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
		public event Action<MouseMovedEventArgs>? MouseMoved;

		public void Load(Map map)
		{
			canvas.Children.Clear();

			var render = new MapRenderer2D(map).RenderMap();

			foreach (var (inst, els) in render)
			{
				var conv = ObjConvert.ConvertCollection(els);

				Add(inst.Id, conv);
			}
		}
		public void Load(IEnumerable<(Guid, IEnumerable<IShape>)> objects)
		{
			Clear();

			foreach (var (id, els) in objects)
			{
				Add(id, els.ConvertCollection());
			}
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

			var els = canvas.Children.Where(x => (x.ToString() ?? string.Empty).Contains(id.ToString()));
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
			ODebugger.Assert(_ids.Contains(id));
			ODebugger.Info($"Removed {id}");

            var els = canvas.Children.Where(x => (x.ToString() ?? string.Empty).Contains(id.ToString()));
            canvas.Children.RemoveAll(els);
			
			_ids.Remove(id);
		}
		
		public void Clear()
		{
			ODebugger.Info("Cleared");

			foreach (Guid id in _ids)
			{
				var els = canvas.Children.Where(x => x.Tag is string s && s.Contains(id.ToString()));
				canvas.Children.RemoveAll(els);

			}

			_ids.Clear();
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
