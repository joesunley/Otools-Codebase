using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using OTools.AvaCommon;
using OTools.Common;
using OTools.Maps;
using ownsmtp.logging;
using r = OTools.ObjectRenderer2D;

namespace OTools.Routechoice;

public class RcGame
{
	private Image _image;
	private Guid _imageId, _legId;

	private PaintBox _paintBox;
	private Course _course;
	
	private r.IMapRenderer2D _renderer;

	private int _currentLeg;

	public RcGame()
	{
		_image = Manager.Image!;
		_imageId = Guid.Parse((string)_image.Tag!);

		_legId = Guid.NewGuid();
		
		_paintBox = Manager.PaintBox!;
		_course = Manager.Course!;
		
		_renderer = new r.MapRenderer2D(Manager.SymbolMap);

		_currentLeg = 0;
	}

	public void Start()
	{
		_paintBox.Clear();
		
		_currentLeg = -1;
		NextLeg();
	}

	public void NextLeg()
	{
		_currentLeg++;

		vec4 leg = (_course.Controls[_currentLeg], _course.Controls[_currentLeg + 1]);

		var rotation = Rotate.Do(leg, MathF.PI / 2f);
		vec2[] points =  { rotation.leg.XY, rotation.leg.ZW };

		_image.RenderTransform = new RotateTransform(rotation.rotVals.Z, rotation.rotVals.X, rotation.rotVals.Y);
		
		LineSymbol symLine = (LineSymbol)Manager.SymbolMap.Symbols["Line"];
		LineInstance l = new(0, symLine, new(points), false);

		_paintBox.AddOrUpdate(_legId, _renderer.RenderPathInstance(l).ConvertCollection());
		_paintBox.AddOrUpdate(_imageId, _image.Yield());
		
		_paintBox.PanTo(rotation.rotVals.X, rotation.rotVals.Y);

		ODebugger.Debug($"{rotation.leg}, {rotation.rotVals}");
	}
}