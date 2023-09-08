using Avalonia.Controls;
using OTools.Maps;

namespace OTools.MapMaker
{
    public partial class SymbolPane : UserControl
    {
        public SymbolPane()
        {
            InitializeComponent();
        }

        public void Load(Map map)
        {
            foreach (var symbol in map.Symbols)
                stack.Children.Add(CreateButton(symbol));
        }

        public Button CreateButton(Symbol sym)
        {
            Button btn = new Button() { Content = sym.Name };

            btn.Click += (s, e) => Manager.Symbol = sym;

            return btn;
        }
    }
}
