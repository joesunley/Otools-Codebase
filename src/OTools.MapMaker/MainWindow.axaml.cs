using System.IO;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using OTools.Maps;
using SixLabors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using Image = SixLabors.ImageSharp.Image;

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
            // paintBox.PanTo(vec2.Zero);

            ViewManager.Set(paintBox, this);

            Map map = MapLoader.Load(@"C:\Dev\Phanes\tests\Files\map1.xml");
            //paintBox.Load(map);

            MapDraw draw = new();

            Manager.Map = map;
            Manager.Symbol = map.Symbols["Course Line"];


            symbolPane.Load(map);

            paintBox.ZoomChanged += e => statusBar.Text = $"Position: {ViewManager.MousePosition.X:F2}, {ViewManager.MousePosition.Y:F2}\tZoom: {e.ZoomX:F2}, {e.ZoomY:F2}\tOffset: {e.OffsetX:F2}, {e.OffsetY:F2}";
            ViewManager.MouseMove += args => statusBar.Text = $"Position: {args.X:F2}, {args.Y:F2}\tZoom: {ViewManager.Zoom.X:F2}, {ViewManager.Zoom.Y:F2}\tOffset: {ViewManager.Offset.X:F2}, {ViewManager.Offset.Y:F2}";

            KeyDown += (_, args) => WriteLine("Key Down: " + args.Key.ToString());
        }
    }
}