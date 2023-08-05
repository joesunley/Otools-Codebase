using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using OTools.AvaCommon;
using OTools.Common;
using ownsmtp.logging;

namespace OTools.Routechoice
{
    public partial class MainWindow : Window
    {

        private Guid _imageId; 
        private readonly InputMonitor _inputMonitor;

        public MainWindow()
        {
            InitializeComponent();

            LoadMap(@"C:\Users\joe\Downloads\4.png");

            Manager.PaintBox = PaintBox;
            Manager.Tool = Tool.Course;

            PaintBox.MouseMoved += _ => SetStatusText();
            PaintBox.ZoomChanged += _ => SetStatusText();

            _inputMonitor = new(PaintBox);
			
			ODebugger.SetLevel(ServerLogLevel.Info);

			Draw _ = new();
		}
		
        public void LoadMap(string filePath)
        {
            if (!File.Exists(filePath)) 
                throw new FileNotFoundException("Map file not found", filePath);

            Image img = new()
			{
				Source = new Bitmap(filePath),
				RenderTransform = new ScaleTransform(0.25, 0.25),
			};

			img.SetTopLeft((1000, 1000));

            _imageId = Guid.NewGuid();
			img.Tag = _imageId;

			Manager.Image = img;

            PaintBox.Add(_imageId, img.Yield());

            PaintBox.PanTo(1000, 1000);
        }

        private void SetStatusText()
        {
            StatusBar.Text = $"Mouse: {PaintBox.MousePosition:F2}, Zoom: {PaintBox.Zoom:F2}, Offset: {PaintBox.Offset:F2}, TopLeft: {PaintBox.TopLeft:F2}";
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

        private void DebugAction(object? sender, RoutedEventArgs e)
        {

        }
    }
}