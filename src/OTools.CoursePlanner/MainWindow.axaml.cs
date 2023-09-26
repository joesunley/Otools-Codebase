using Avalonia.Controls;
using OTools.AvaCommon;
using OTools.Common;
using OTools.Courses;
using OTools.ObjectRenderer2D;

namespace OTools.CoursePlanner;

public partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();

		//paintBox.PanTo(vec2.Zero);
		
		//Manager.PaintBox = paintBox;
		
		//Event ev = CourseLoader.Load(@"C:\Dev\Phanes\tests\Files\course1.xml");
		//ev.Symbols = CreateSymbolMap.DefaultISOM();
		//Manager.Event = ev;
        
		//Manager.CourseRenderer = new CourseRenderer2D(ev);

		//var render = Manager.CourseRenderer.RenderCourse(ev.Courses[1]);
		//paintBox.Load((ev.Courses[1].Id, n: render).Yield());
		
		//paintBox.ZoomChanged += args => statusBar.Text = $"Position: {paintBox.MousePosition:F2}\tZoom: {args.ZoomX:F2}, {args.ZoomY:F2}\tOffset: {args.OffsetX:F2}, {args.OffsetY:F2}";
		//paintBox.MouseMoved += args => statusBar.Text = $"Position: {args.Position:F2}\tZoom: {paintBox.Zoom:F2}\tOffset: {paintBox.Offset:F2}";
	}
}