using Avalonia.Controls;
using OTools.Maps;

namespace OTools.MapMaker
{
    public partial class SymbolPane : UserControl
    {
        private MapMakerInstance _instance;

        public SymbolPane()
        {
            InitializeComponent();
        }

        public void Load(MapMakerInstance instance)
        {
            _instance = instance;

            foreach (var symbol in _instance.Map.Symbols)
                stack.Children.Add(CreateButton(symbol));
        }

        public Button CreateButton(Symbol sym)
        {
            Button btn = new Button() { Content = sym.Name };

            btn.Click += (s, e) => _instance.ActiveSymbol = sym;

            return btn;
        }
    }
}
