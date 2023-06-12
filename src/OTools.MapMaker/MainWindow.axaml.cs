using Avalonia.Controls;
using Avalonia.Interactivity;
using OTools.AvaCommon;
using OTools.Maps;

namespace OTools.MapMaker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Image img = Image.Load(new DecoderOptions(), @"C:\Users\Joe\Downloads\2-S-1.png");
            // Bitmap bmp;

            // using (MemoryStream ms = new())
            // {
            //     img.Save(ms, PngFormat.Instance);
            //     bmp = new Bitmap()
            // }

            // Bitmap bmp = new(@"C:\Users\Joe\Downloads\2-S-1.png");

            // Avalonia.Controls.Image dImg = new();
            // dImg.Source = bmp;

            // dImg.SetTopLeft((0, 0));

            // paintBox.canvas.Children.Add(dImg);

            paintBox.PanTo(vec2.Zero);

            Manager.PaintBox = paintBox;
            ViewManager.Set(paintBox, this);

            Map map = MapLoader.Load(@"C:\Dev\Phanes\tests\Files\map1.xml");
            map.Colours.UpdatePrecendences(0);
            paintBox.Load(map);

            MapDraw draw = new();

            
            Manager.Map = map;
            //Manager.Symbol = map.Symbols["Course Line"];


            symbolPane.Load(map);

            paintBox.ZoomChanged += e => statusBar.Text = $"Position: {paintBox.MousePosition.X:F2}, {paintBox.MousePosition.Y:F2}\tZoom: {e.ZoomX:F2}, {e.ZoomY:F2}\tOffset: {e.OffsetX:F2}, {e.OffsetY:F2}";
            paintBox.MouseMoved += args => statusBar.Text = $"Position: {args.X:F2}, {args.Y:F2}\tZoom: {paintBox.Zoom.X:F2}, {paintBox.Zoom.Y:F2}\tOffset: {paintBox.Offset.X:F2}, {paintBox.Offset.Y:F2}";

            KeyDown += (_, args) => WriteLine("Key Down: " + args.Key.ToString());
        }

        private bool _isDebug = false;
        private void OnDebugClicked(object? sender, RoutedEventArgs e)
        {
            _isDebug = !_isDebug;

            if (_isDebug)
            {
                DebugTools.AllocConsole();
                btnDebug.Content = "Hide Debug";
            }
            else
            {
                DebugTools.FreeConsole();
                btnDebug.Content = "Show Debug";
            }
        }
    }
}