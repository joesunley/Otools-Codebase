using Avalonia.Controls;
using OTools.Maps;

namespace OTools.MapViewer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            Map map = MapLoader.Load(@"C:\Users\Joe\Downloads\map2.xml");
            viewport.LoadAll(map);
        }
    }
}
