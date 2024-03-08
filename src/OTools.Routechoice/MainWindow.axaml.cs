using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using OTools.AvaCommon;
using OTools.Common;
using static OTools.Common.StaticLogger;

namespace OTools.Routechoice
{
    public partial class MainWindow : Window
    {

        private Guid _imageId; 
        private readonly InputMonitor _inputMonitor;

        public MainWindow()
        {
            InitializeComponent();

		}

    }
}